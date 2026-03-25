using System;
using System.Collections.Generic;

namespace Ascendant.Backend
{
    public enum AuthState
    {
        SignedOut,
        Anonymous,
        SignedIn
    }

    public enum ConnectionState
    {
        Offline,
        Connecting,
        Online
    }

    [Serializable]
    public class PlayerProfile
    {
        public string UserId;
        public string DisplayName;
        public AuthState AuthState;
        public long LastLoginUnix;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string UserId;
        public string DisplayName;
        public long Score;
        public int Rank;
    }

    [Serializable]
    public class GuildInfo
    {
        public string GuildId;
        public string GuildName;
        public string Description;
        public int Level;
        public string LeaderId;
        public List<GuildMemberInfo> Members = new();
        public Dictionary<string, int> TechTreeLevels = new();
    }

    [Serializable]
    public class GuildMemberInfo
    {
        public string UserId;
        public string DisplayName;
        public GuildRole Role;
        public long LastActiveUnix;
        public int ContributionPoints;
    }

    public enum GuildRole
    {
        Member,
        Officer,
        Leader
    }

    [Serializable]
    public class GuildChatMessage
    {
        public string SenderId;
        public string SenderName;
        public string Message;
        public long TimestampUnix;
    }

    [Serializable]
    public class ArenaDefenseTeam
    {
        public string UserId;
        public string DisplayName;
        public List<ArenaHeroSnapshot> Heroes = new();
        public int EloRating;
        public ArenaRank Rank;
    }

    [Serializable]
    public class ArenaHeroSnapshot
    {
        public string ClassId;
        public int Level;
        public int StarRating;
        public float Atk;
        public float Def;
        public float Hp;
        public float Spd;
    }

    public enum ArenaRank
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Legend
    }

    [Serializable]
    public class WorldBossState
    {
        public string BossId;
        public string BossName;
        public double MaxHp;
        public double CurrentHp;
        public long EventStartUnix;
        public long EventEndUnix;
        public bool IsActive;
    }

    public interface IBackendService
    {
        // Connection
        ConnectionState Connection { get; }
        event Action<ConnectionState> OnConnectionChanged;

        // Auth
        void SignInAnonymous(Action<bool, string> callback);
        void SignInWithGameCenter(Action<bool, string> callback);
        void LinkAnonymousToGameCenter(Action<bool> callback);
        PlayerProfile GetCurrentPlayer();

        // Cloud Save
        void SavePlayerData(string json, long timestampUnix, Action<bool> callback);
        void LoadPlayerData(Action<bool, string, long> callback);

        // Leaderboards
        void SubmitScore(string leaderboardId, long score, Action<bool> callback);
        void GetLeaderboard(string leaderboardId, bool weekly, int count, Action<List<LeaderboardEntry>> callback);
        void GetPlayerRank(string leaderboardId, bool weekly, Action<int, float> callback);

        // Guild
        void CreateGuild(string name, string description, Action<bool, string> callback);
        void JoinGuild(string guildId, Action<bool> callback);
        void LeaveGuild(Action<bool> callback);
        void GetGuildInfo(string guildId, Action<GuildInfo> callback);
        void SearchGuilds(string query, Action<List<GuildInfo>> callback);
        void ContributeToTechTree(string techId, int amount, Action<bool> callback);
        void SendGuildChat(string message, Action<bool> callback);
        void GetGuildChat(int limit, Action<List<GuildChatMessage>> callback);

        // Guild Expedition
        void GetExpeditionGrid(Action<GuildExpeditionState> callback);
        void ClearExpeditionNode(int x, int y, Action<bool> callback);

        // Arena
        void SetDefenseTeam(ArenaDefenseTeam team, Action<bool> callback);
        void GetArenaOpponents(int count, Action<List<ArenaDefenseTeam>> callback);
        void ReportArenaResult(string opponentId, bool won, Action<bool, int> callback);
        void GetArenaSeasonInfo(Action<ArenaSeasonInfo> callback);

        // World Boss
        void GetWorldBossState(Action<WorldBossState> callback);
        void ReportWorldBossDamage(double damage, Action<bool> callback);
        void GetWorldBossDamageLeaderboard(int count, Action<List<LeaderboardEntry>> callback);
    }

    [Serializable]
    public class GuildExpeditionState
    {
        public int GridWidth = 5;
        public int GridHeight = 5;
        public List<ExpeditionNode> Nodes = new();
        public bool BossUnlocked;
        public bool BossDefeated;
        public long WeekStartUnix;
    }

    [Serializable]
    public class ExpeditionNode
    {
        public int X;
        public int Y;
        public ExpeditionNodeType Type;
        public bool Cleared;
        public string ClearedByUserId;
        public string ClearedByName;
    }

    public enum ExpeditionNodeType
    {
        Empty,
        Enemy,
        Treasure,
        Elite,
        Boss
    }

    [Serializable]
    public class ArenaSeasonInfo
    {
        public int SeasonNumber;
        public long SeasonStartUnix;
        public long SeasonEndUnix;
        public int PlayerElo;
        public ArenaRank PlayerRank;
        public int Wins;
        public int Losses;
        public int AttemptsToday;
        public int MaxAttemptsPerDay = 5;
    }
}
