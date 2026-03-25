using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ascendant.Backend
{
    public class LocalBackendService : IBackendService
    {
        ConnectionState _connection = ConnectionState.Online;
        PlayerProfile _player;
        string _guildId;

        // Local stores
        readonly Dictionary<string, List<LeaderboardEntry>> _leaderboards = new();
        readonly Dictionary<string, GuildInfo> _guilds = new();
        readonly Dictionary<string, List<GuildChatMessage>> _guildChats = new();
        readonly Dictionary<string, GuildExpeditionState> _guildExpeditions = new();
        readonly List<ArenaDefenseTeam> _arenaPool = new();
        ArenaDefenseTeam _playerDefenseTeam;
        ArenaSeasonInfo _seasonInfo;
        WorldBossState _worldBoss;
        string _cloudSaveJson;
        long _cloudSaveTimestamp;

        public ConnectionState Connection => _connection;
        public event Action<ConnectionState> OnConnectionChanged;

        public LocalBackendService()
        {
            _player = new PlayerProfile
            {
                UserId = SystemInfo.deviceUniqueIdentifier,
                DisplayName = "Player",
                AuthState = AuthState.Anonymous,
                LastLoginUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _seasonInfo = new ArenaSeasonInfo
            {
                SeasonNumber = 1,
                SeasonStartUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                SeasonEndUnix = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
                PlayerElo = 1000,
                PlayerRank = ArenaRank.Bronze,
                Wins = 0,
                Losses = 0,
                AttemptsToday = 0,
                MaxAttemptsPerDay = 5
            };

            GenerateArenaPool();
        }

        // Auth
        public void SignInAnonymous(Action<bool, string> callback)
        {
            _player.AuthState = AuthState.Anonymous;
            _player.UserId = SystemInfo.deviceUniqueIdentifier;
            callback?.Invoke(true, _player.UserId);
        }

        public void SignInWithGameCenter(Action<bool, string> callback)
        {
            // Game Center not available offline — fall back to anonymous
            _player.AuthState = AuthState.Anonymous;
            callback?.Invoke(true, _player.UserId);
        }

        public void LinkAnonymousToGameCenter(Action<bool> callback)
        {
            callback?.Invoke(false); // Not available offline
        }

        public PlayerProfile GetCurrentPlayer() => _player;

        // Cloud Save
        public void SavePlayerData(string json, long timestampUnix, Action<bool> callback)
        {
            _cloudSaveJson = json;
            _cloudSaveTimestamp = timestampUnix;

            // Also persist to disk as backup
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "cloud_save_local.json");
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LocalBackend] Failed to write local cloud backup: {e.Message}");
            }

            callback?.Invoke(true);
        }

        public void LoadPlayerData(Action<bool, string, long> callback)
        {
            if (!string.IsNullOrEmpty(_cloudSaveJson))
            {
                callback?.Invoke(true, _cloudSaveJson, _cloudSaveTimestamp);
                return;
            }

            // Try loading from disk
            string path = Path.Combine(Application.persistentDataPath, "cloud_save_local.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    callback?.Invoke(true, json, 0);
                    return;
                }
                catch { }
            }

            callback?.Invoke(false, null, 0);
        }

        // Leaderboards
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            if (!_leaderboards.ContainsKey(leaderboardId))
                _leaderboards[leaderboardId] = new List<LeaderboardEntry>();

            var entries = _leaderboards[leaderboardId];
            var existing = entries.Find(e => e.UserId == _player.UserId);
            if (existing != null)
            {
                if (score > existing.Score)
                    existing.Score = score;
            }
            else
            {
                entries.Add(new LeaderboardEntry
                {
                    UserId = _player.UserId,
                    DisplayName = _player.DisplayName,
                    Score = score,
                    Rank = 1
                });
            }

            // Re-sort and assign ranks
            entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            callback?.Invoke(true);
        }

        public void GetLeaderboard(string leaderboardId, bool weekly, int count, Action<List<LeaderboardEntry>> callback)
        {
            if (_leaderboards.TryGetValue(leaderboardId, out var entries))
            {
                int take = Math.Min(count, entries.Count);
                callback?.Invoke(entries.GetRange(0, take));
            }
            else
            {
                callback?.Invoke(new List<LeaderboardEntry>());
            }
        }

        public void GetPlayerRank(string leaderboardId, bool weekly, Action<int, float> callback)
        {
            if (_leaderboards.TryGetValue(leaderboardId, out var entries))
            {
                var entry = entries.Find(e => e.UserId == _player.UserId);
                if (entry != null)
                {
                    float percentile = entries.Count > 0 ? (1f - (float)entry.Rank / entries.Count) * 100f : 0f;
                    callback?.Invoke(entry.Rank, percentile);
                    return;
                }
            }
            callback?.Invoke(-1, 0f);
        }

        // Guild
        public void CreateGuild(string name, string description, Action<bool, string> callback)
        {
            string guildId = Guid.NewGuid().ToString("N")[..8];
            var guild = new GuildInfo
            {
                GuildId = guildId,
                GuildName = name,
                Description = description,
                Level = 1,
                LeaderId = _player.UserId,
                Members = new List<GuildMemberInfo>
                {
                    new()
                    {
                        UserId = _player.UserId,
                        DisplayName = _player.DisplayName,
                        Role = GuildRole.Leader,
                        LastActiveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        ContributionPoints = 0
                    }
                }
            };

            _guilds[guildId] = guild;
            _guildId = guildId;
            callback?.Invoke(true, guildId);
        }

        public void JoinGuild(string guildId, Action<bool> callback)
        {
            if (!_guilds.TryGetValue(guildId, out var guild))
            {
                callback?.Invoke(false);
                return;
            }

            if (guild.Members.Count >= 30)
            {
                callback?.Invoke(false);
                return;
            }

            guild.Members.Add(new GuildMemberInfo
            {
                UserId = _player.UserId,
                DisplayName = _player.DisplayName,
                Role = GuildRole.Member,
                LastActiveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            _guildId = guildId;
            callback?.Invoke(true);
        }

        public void LeaveGuild(Action<bool> callback)
        {
            if (string.IsNullOrEmpty(_guildId) || !_guilds.TryGetValue(_guildId, out var guild))
            {
                callback?.Invoke(false);
                return;
            }

            guild.Members.RemoveAll(m => m.UserId == _player.UserId);
            _guildId = null;
            callback?.Invoke(true);
        }

        public void GetGuildInfo(string guildId, Action<GuildInfo> callback)
        {
            _guilds.TryGetValue(guildId, out var guild);
            callback?.Invoke(guild);
        }

        public void SearchGuilds(string query, Action<List<GuildInfo>> callback)
        {
            var results = new List<GuildInfo>();
            string lower = query.ToLowerInvariant();
            foreach (var guild in _guilds.Values)
            {
                if (guild.GuildName.ToLowerInvariant().Contains(lower))
                    results.Add(guild);
            }
            callback?.Invoke(results);
        }

        public void ContributeToTechTree(string techId, int amount, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(_guildId) || !_guilds.TryGetValue(_guildId, out var guild))
            {
                callback?.Invoke(false);
                return;
            }

            if (!guild.TechTreeLevels.ContainsKey(techId))
                guild.TechTreeLevels[techId] = 0;

            guild.TechTreeLevels[techId] += amount;
            callback?.Invoke(true);
        }

        public void SendGuildChat(string message, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(_guildId))
            {
                callback?.Invoke(false);
                return;
            }

            if (!_guildChats.ContainsKey(_guildId))
                _guildChats[_guildId] = new List<GuildChatMessage>();

            _guildChats[_guildId].Add(new GuildChatMessage
            {
                SenderId = _player.UserId,
                SenderName = _player.DisplayName,
                Message = message,
                TimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            callback?.Invoke(true);
        }

        public void GetGuildChat(int limit, Action<List<GuildChatMessage>> callback)
        {
            if (string.IsNullOrEmpty(_guildId) || !_guildChats.TryGetValue(_guildId, out var messages))
            {
                callback?.Invoke(new List<GuildChatMessage>());
                return;
            }

            int start = Math.Max(0, messages.Count - limit);
            callback?.Invoke(messages.GetRange(start, messages.Count - start));
        }

        // Guild Expedition
        public void GetExpeditionGrid(Action<GuildExpeditionState> callback)
        {
            if (string.IsNullOrEmpty(_guildId))
            {
                callback?.Invoke(null);
                return;
            }

            if (!_guildExpeditions.TryGetValue(_guildId, out var state))
            {
                state = GenerateExpeditionGrid();
                _guildExpeditions[_guildId] = state;
            }

            callback?.Invoke(state);
        }

        public void ClearExpeditionNode(int x, int y, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(_guildId) || !_guildExpeditions.TryGetValue(_guildId, out var state))
            {
                callback?.Invoke(false);
                return;
            }

            var node = state.Nodes.Find(n => n.X == x && n.Y == y);
            if (node == null || node.Cleared)
            {
                callback?.Invoke(false);
                return;
            }

            node.Cleared = true;
            node.ClearedByUserId = _player.UserId;
            node.ClearedByName = _player.DisplayName;

            // Check if all non-boss nodes cleared
            bool allCleared = true;
            foreach (var n in state.Nodes)
            {
                if (n.Type != ExpeditionNodeType.Boss && !n.Cleared)
                {
                    allCleared = false;
                    break;
                }
            }
            state.BossUnlocked = allCleared;

            callback?.Invoke(true);
        }

        GuildExpeditionState GenerateExpeditionGrid()
        {
            var state = new GuildExpeditionState
            {
                GridWidth = 5,
                GridHeight = 5,
                WeekStartUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var rng = new System.Random();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var type = ExpeditionNodeType.Enemy;
                    if (x == 2 && y == 2) type = ExpeditionNodeType.Boss;
                    else if (rng.NextDouble() < 0.3) type = ExpeditionNodeType.Treasure;
                    else if (rng.NextDouble() < 0.15) type = ExpeditionNodeType.Elite;

                    state.Nodes.Add(new ExpeditionNode
                    {
                        X = x,
                        Y = y,
                        Type = type,
                        Cleared = false
                    });
                }
            }

            return state;
        }

        // Arena
        public void SetDefenseTeam(ArenaDefenseTeam team, Action<bool> callback)
        {
            _playerDefenseTeam = team;
            callback?.Invoke(true);
        }

        public void GetArenaOpponents(int count, Action<List<ArenaDefenseTeam>> callback)
        {
            var opponents = new List<ArenaDefenseTeam>();
            int playerElo = _seasonInfo.PlayerElo;

            // Sort pool by distance from player ELO
            var sorted = new List<ArenaDefenseTeam>(_arenaPool);
            sorted.Sort((a, b) =>
                Math.Abs(a.EloRating - playerElo).CompareTo(Math.Abs(b.EloRating - playerElo)));

            int take = Math.Min(count, sorted.Count);
            for (int i = 0; i < take; i++)
                opponents.Add(sorted[i]);

            callback?.Invoke(opponents);
        }

        public void ReportArenaResult(string opponentId, bool won, Action<bool, int> callback)
        {
            int eloChange = won ? 25 : -20;
            _seasonInfo.PlayerElo = Math.Max(0, _seasonInfo.PlayerElo + eloChange);
            _seasonInfo.AttemptsToday++;

            if (won) _seasonInfo.Wins++;
            else _seasonInfo.Losses++;

            _seasonInfo.PlayerRank = EloToRank(_seasonInfo.PlayerElo);
            callback?.Invoke(true, _seasonInfo.PlayerElo);
        }

        public void GetArenaSeasonInfo(Action<ArenaSeasonInfo> callback)
        {
            callback?.Invoke(_seasonInfo);
        }

        static ArenaRank EloToRank(int elo)
        {
            if (elo >= 2400) return ArenaRank.Legend;
            if (elo >= 2000) return ArenaRank.Diamond;
            if (elo >= 1600) return ArenaRank.Platinum;
            if (elo >= 1200) return ArenaRank.Gold;
            if (elo >= 800) return ArenaRank.Silver;
            return ArenaRank.Bronze;
        }

        void GenerateArenaPool()
        {
            var rng = new System.Random(42);
            string[] names = { "SkyBlade", "NovaStar", "IronHeart", "ShadowFang", "ThunderKin",
                              "FrostBite", "BlazeMage", "MoonPriest", "StormRider", "DarkHunter",
                              "FlameWard", "IceReaper", "LightMonk", "VoidBard", "EarthDruid" };
            string[] classes = { "warrior", "mage", "priest", "rogue", "ranger",
                                "berserker", "paladin", "necromancer", "monk", "bard" };

            for (int i = 0; i < 15; i++)
            {
                int elo = 600 + rng.Next(1800);
                var team = new ArenaDefenseTeam
                {
                    UserId = $"bot_{i}",
                    DisplayName = names[i],
                    EloRating = elo,
                    Rank = EloToRank(elo),
                    Heroes = new List<ArenaHeroSnapshot>()
                };

                for (int h = 0; h < 4; h++)
                {
                    int level = 20 + rng.Next(80);
                    team.Heroes.Add(new ArenaHeroSnapshot
                    {
                        ClassId = classes[rng.Next(classes.Length)],
                        Level = level,
                        StarRating = 1 + rng.Next(5),
                        Atk = 50 + level * 5 + rng.Next(100),
                        Def = 30 + level * 3 + rng.Next(60),
                        Hp = 200 + level * 20 + rng.Next(500),
                        Spd = 10 + level * 0.5f + rng.Next(5)
                    });
                }

                _arenaPool.Add(team);
            }
        }

        // World Boss
        public void GetWorldBossState(Action<WorldBossState> callback)
        {
            if (_worldBoss == null)
            {
                var now = DateTimeOffset.UtcNow;
                _worldBoss = new WorldBossState
                {
                    BossId = "inferno_titan",
                    BossName = "Inferno Titan",
                    MaxHp = 100_000_000_000.0,
                    CurrentHp = 100_000_000_000.0,
                    EventStartUnix = now.ToUnixTimeSeconds(),
                    EventEndUnix = now.AddHours(48).ToUnixTimeSeconds(),
                    IsActive = true
                };
            }

            callback?.Invoke(_worldBoss);
        }

        public void ReportWorldBossDamage(double damage, Action<bool> callback)
        {
            if (_worldBoss != null && _worldBoss.IsActive)
            {
                _worldBoss.CurrentHp = Math.Max(0, _worldBoss.CurrentHp - damage);
                if (_worldBoss.CurrentHp <= 0)
                    _worldBoss.IsActive = false;

                // Also submit to leaderboard
                SubmitScore("world_boss_damage", (long)damage, null);
            }

            callback?.Invoke(true);
        }

        public void GetWorldBossDamageLeaderboard(int count, Action<List<LeaderboardEntry>> callback)
        {
            GetLeaderboard("world_boss_damage", false, count, callback);
        }
    }
}
