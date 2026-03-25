using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    [Serializable]
    public class WorldBossSaveData
    {
        public int AttemptsToday;
        public long LastAttemptDayUnix;
        public double PersonalBestDamage;
        public double TotalDamageDealt;
        public string LastBossId;
    }

    public class WorldBossManager : MonoBehaviour
    {
        public static WorldBossManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _attemptsPerDay = 3;
        [SerializeField] float _fightDurationSeconds = 60f;

        WorldBossState _currentBoss;
        int _attemptsToday;
        long _lastAttemptDayUnix;
        double _personalBestDamage;
        double _totalDamageDealt;
        bool _isFighting;
        float _fightTimer;
        double _currentFightDamage;

        public WorldBossState CurrentBoss => _currentBoss;
        public bool IsEventActive => _currentBoss != null && _currentBoss.IsActive;
        public int AttemptsToday => _attemptsToday;
        public int AttemptsPerDay => _attemptsPerDay;
        public int RemainingAttempts => Mathf.Max(0, _attemptsPerDay - _attemptsToday);
        public bool CanAttempt => IsEventActive && RemainingAttempts > 0 && !_isFighting;
        public bool IsFighting => _isFighting;
        public float FightTimeRemaining => _fightTimer;
        public float FightDuration => _fightDurationSeconds;
        public double CurrentFightDamage => _currentFightDamage;
        public double PersonalBestDamage => _personalBestDamage;
        public double TotalDamageDealt => _totalDamageDealt;

        public float BossHpPercent
        {
            get
            {
                if (_currentBoss == null || _currentBoss.MaxHp <= 0) return 1f;
                return (float)(_currentBoss.CurrentHp / _currentBoss.MaxHp);
            }
        }

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
            {
                _attemptsPerDay = config.worldBossAttemptsPerDay;
                _fightDurationSeconds = config.worldBossFightDurationSeconds;
            }
        }

        void Start()
        {
            CheckDayReset();
            RefreshBossState();
        }

        void Update()
        {
            if (!_isFighting) return;

            _fightTimer -= Time.deltaTime;
            if (_fightTimer <= 0f)
            {
                EndFight();
            }
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

        public void RefreshBossState(Action callback = null)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke();
                return;
            }

            service.GetWorldBossState(state =>
            {
                var oldBoss = _currentBoss;
                _currentBoss = state;

                if (state != null && state.IsActive && (oldBoss == null || !oldBoss.IsActive))
                {
                    EventBus.Publish(new WorldBossEventStartedEvent
                    {
                        BossId = state.BossId,
                        BossName = state.BossName
                    });
                }

                callback?.Invoke();
            });
        }

        public bool StartFight()
        {
            CheckDayReset();

            if (!CanAttempt) return false;

            _attemptsToday++;
            _isFighting = true;
            _fightTimer = _fightDurationSeconds;
            _currentFightDamage = 0;

            return true;
        }

        public void DealDamage(double damage)
        {
            if (!_isFighting) return;

            _currentFightDamage += damage;
            _totalDamageDealt += damage;

            if (_currentBoss != null)
            {
                _currentBoss.CurrentHp = Math.Max(0, _currentBoss.CurrentHp - damage);

                EventBus.Publish(new WorldBossDamageDealtEvent
                {
                    Damage = damage,
                    RemainingHp = _currentBoss.CurrentHp,
                    MaxHp = _currentBoss.MaxHp
                });

                if (_currentBoss.CurrentHp <= 0)
                {
                    _currentBoss.IsActive = false;
                    EventBus.Publish(new WorldBossDefeatedEvent { BossId = _currentBoss.BossId });
                }
            }
        }

        void EndFight()
        {
            _isFighting = false;

            // Update personal best
            if (_currentFightDamage > _personalBestDamage)
                _personalBestDamage = _currentFightDamage;

            // Report damage to backend
            var service = FirebaseManager.Instance?.Service;
            if (service != null)
            {
                service.ReportWorldBossDamage(_currentFightDamage, success =>
                {
                    if (!success)
                    {
                        OfflineQueue.Instance?.Enqueue(new OfflineAction
                        {
                            Type = OfflineActionType.WorldBossDamage,
                            Payload = _currentFightDamage.ToString("F0")
                        });
                    }
                });
            }
            else
            {
                OfflineQueue.Instance?.Enqueue(new OfflineAction
                {
                    Type = OfflineActionType.WorldBossDamage,
                    Payload = _currentFightDamage.ToString("F0")
                });
            }

            // Submit to leaderboard
            LeaderboardManager.Instance?.SubmitBossDamage((long)_currentFightDamage);

            Debug.Log($"[WorldBoss] Fight ended. Damage dealt: {FormatDamage(_currentFightDamage)}");
        }

        public void GetDamageLeaderboard(Action<List<LeaderboardEntry>> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            service.GetWorldBossDamageLeaderboard(100, callback);
        }

        public WorldBossRewardTier GetRewardTier(float percentile)
        {
            if (percentile >= 99f) return WorldBossRewardTier.Diamond;
            if (percentile >= 90f) return WorldBossRewardTier.Gold;
            if (percentile >= 50f) return WorldBossRewardTier.Silver;
            return WorldBossRewardTier.Bronze;
        }

        public static string FormatDamage(double damage)
        {
            if (damage >= 1_000_000_000) return $"{damage / 1_000_000_000:F1}B";
            if (damage >= 1_000_000) return $"{damage / 1_000_000:F1}M";
            if (damage >= 1_000) return $"{damage / 1_000:F1}K";
            return damage.ToString("F0");
        }

        // Save/Load
        public WorldBossSaveData GatherSaveData()
        {
            return new WorldBossSaveData
            {
                AttemptsToday = _attemptsToday,
                LastAttemptDayUnix = _lastAttemptDayUnix,
                PersonalBestDamage = _personalBestDamage,
                TotalDamageDealt = _totalDamageDealt,
                LastBossId = _currentBoss?.BossId ?? ""
            };
        }

        public void LoadSaveData(WorldBossSaveData data)
        {
            if (data == null) return;
            _attemptsToday = data.AttemptsToday;
            _lastAttemptDayUnix = data.LastAttemptDayUnix;
            _personalBestDamage = data.PersonalBestDamage;
            _totalDamageDealt = data.TotalDamageDealt;
            CheckDayReset();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public enum WorldBossRewardTier
    {
        Bronze,  // Any damage
        Silver,  // Top 50%
        Gold,    // Top 10%
        Diamond  // Top 1%
    }
}
