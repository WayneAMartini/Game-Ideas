using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Party;

namespace Ascendant.Backend
{
    [Serializable]
    public class ArenaSaveData
    {
        public int Elo = 1000;
        public int Wins;
        public int Losses;
        public int AttemptsToday;
        public long LastAttemptDayUnix;
        public List<ArenaHeroSnapshot> DefenseTeamHeroes = new();
    }

    public class ArenaManager : MonoBehaviour
    {
        public static ArenaManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _attemptsPerDay = 5;
        [SerializeField] int _refreshCostAetherCrystals = 50;

        int _elo = 1000;
        int _wins;
        int _losses;
        int _attemptsToday;
        long _lastAttemptDayUnix;
        ArenaDefenseTeam _playerDefense;
        List<ArenaDefenseTeam> _currentOpponents = new();

        public int Elo => _elo;
        public int Wins => _wins;
        public int Losses => _losses;
        public int AttemptsToday => _attemptsToday;
        public int AttemptsPerDay => _attemptsPerDay;
        public int RemainingAttempts => Mathf.Max(0, _attemptsPerDay - _attemptsToday);
        public bool CanAttack => RemainingAttempts > 0;
        public ArenaRank CurrentRank => EloToRank(_elo);
        public IReadOnlyList<ArenaDefenseTeam> CurrentOpponents => _currentOpponents;
        public ArenaDefenseTeam PlayerDefense => _playerDefense;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            var config = FirebaseManager.Instance?.Config;
            if (config != null)
                _attemptsPerDay = config.arenaAttemptsPerDay;
        }

        void Start()
        {
            CheckDayReset();

            // Auto-set defense team from current party if none exists
            if (_playerDefense == null)
                AutoSetDefenseTeam();
        }

        void CheckDayReset()
        {
            long today = DateTimeOffset.UtcNow.Date.ToUnixTimeSeconds();
            if (_lastAttemptDayUnix < today)
            {
                _attemptsToday = 0;
                _lastAttemptDayUnix = today;
            }
        }

        public void AutoSetDefenseTeam()
        {
            var party = PartyManager.Instance;
            if (party == null) return;

            _playerDefense = new ArenaDefenseTeam
            {
                UserId = AuthManager.Instance?.UserId ?? "local",
                DisplayName = AuthManager.Instance?.DisplayName ?? "Player",
                EloRating = _elo,
                Rank = CurrentRank,
                Heroes = new List<ArenaHeroSnapshot>()
            };

            var heroes = party.GetAllHeroes();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] == null) continue;
                _playerDefense.Heroes.Add(CreateHeroSnapshot(heroes[i]));
            }

            // Upload to backend
            var service = FirebaseManager.Instance?.Service;
            service?.SetDefenseTeam(_playerDefense, null);
        }

        public void SetDefenseTeam(Hero[] heroes, Action<bool> callback)
        {
            _playerDefense = new ArenaDefenseTeam
            {
                UserId = AuthManager.Instance?.UserId ?? "local",
                DisplayName = AuthManager.Instance?.DisplayName ?? "Player",
                EloRating = _elo,
                Rank = CurrentRank,
                Heroes = new List<ArenaHeroSnapshot>()
            };

            foreach (var hero in heroes)
            {
                if (hero == null) continue;
                _playerDefense.Heroes.Add(CreateHeroSnapshot(hero));
            }

            var service = FirebaseManager.Instance?.Service;
            if (service != null)
            {
                service.SetDefenseTeam(_playerDefense, callback);
            }
            else
            {
                callback?.Invoke(true);
            }
        }

        ArenaHeroSnapshot CreateHeroSnapshot(Hero hero)
        {
            return new ArenaHeroSnapshot
            {
                ClassId = hero.Data?.classId ?? "",
                Level = hero.Level,
                StarRating = Economy.StarSystem.Instance?.GetStarRating(hero.Data?.classId ?? "") ?? 1,
                Atk = hero.CurrentAtk,
                Def = hero.CurrentDef,
                Hp = hero.MaxHp,
                Spd = hero.CurrentSpd
            };
        }

        public void FindOpponents(Action<List<ArenaDefenseTeam>> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(new List<ArenaDefenseTeam>());
                return;
            }

            service.GetArenaOpponents(3, opponents =>
            {
                _currentOpponents = opponents ?? new List<ArenaDefenseTeam>();
                callback?.Invoke(_currentOpponents);
            });
        }

        public void Attack(ArenaDefenseTeam opponent, Action<bool, ArenaBattleResult> callback)
        {
            CheckDayReset();

            if (!CanAttack)
            {
                callback?.Invoke(false, null);
                return;
            }

            _attemptsToday++;

            // Simulate battle
            var result = ArenaBattleSimulator.SimulateBattle(_playerDefense, opponent);

            // Report to backend
            var service = FirebaseManager.Instance?.Service;
            if (service != null)
            {
                service.ReportArenaResult(opponent.UserId, result.PlayerWon, (success, newElo) =>
                {
                    if (success) _elo = newElo;
                    FinishMatch(result);
                    callback?.Invoke(true, result);
                });
            }
            else
            {
                // Offline: calculate ELO locally
                int eloChange = result.PlayerWon ? 25 : -20;
                _elo = Mathf.Max(0, _elo + eloChange);

                // Queue for sync
                OfflineQueue.Instance?.Enqueue(new OfflineAction
                {
                    Type = OfflineActionType.ArenaResult,
                    Payload = $"{opponent.UserId}|{(result.PlayerWon ? "1" : "0")}"
                });

                FinishMatch(result);
                callback?.Invoke(true, result);
            }
        }

        void FinishMatch(ArenaBattleResult result)
        {
            if (result.PlayerWon) _wins++;
            else _losses++;

            EventBus.Publish(new ArenaMatchResultEvent
            {
                Won = result.PlayerWon,
                NewElo = _elo,
                NewRank = CurrentRank
            });
        }

        public bool RefreshAttempts()
        {
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null) return false;
            if (!currency.SpendCurrency(CurrencyType.AetherCrystals, _refreshCostAetherCrystals))
                return false;

            _attemptsToday = 0;
            return true;
        }

        public static ArenaRank EloToRank(int elo)
        {
            if (elo >= 2400) return ArenaRank.Legend;
            if (elo >= 2000) return ArenaRank.Diamond;
            if (elo >= 1600) return ArenaRank.Platinum;
            if (elo >= 1200) return ArenaRank.Gold;
            if (elo >= 800) return ArenaRank.Silver;
            return ArenaRank.Bronze;
        }

        // Save/Load
        public ArenaSaveData GatherSaveData()
        {
            return new ArenaSaveData
            {
                Elo = _elo,
                Wins = _wins,
                Losses = _losses,
                AttemptsToday = _attemptsToday,
                LastAttemptDayUnix = _lastAttemptDayUnix,
                DefenseTeamHeroes = _playerDefense?.Heroes ?? new List<ArenaHeroSnapshot>()
            };
        }

        public void LoadSaveData(ArenaSaveData data)
        {
            if (data == null) return;
            _elo = data.Elo;
            _wins = data.Wins;
            _losses = data.Losses;
            _attemptsToday = data.AttemptsToday;
            _lastAttemptDayUnix = data.LastAttemptDayUnix;

            if (data.DefenseTeamHeroes != null && data.DefenseTeamHeroes.Count > 0)
            {
                _playerDefense = new ArenaDefenseTeam
                {
                    UserId = AuthManager.Instance?.UserId ?? "local",
                    DisplayName = AuthManager.Instance?.DisplayName ?? "Player",
                    EloRating = _elo,
                    Rank = CurrentRank,
                    Heroes = data.DefenseTeamHeroes
                };
            }

            CheckDayReset();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
