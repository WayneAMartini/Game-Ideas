using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Events
{
    [Serializable]
    public class VoidRiftSaveData
    {
        public int CurrentRiftIndex;
        public int CurrentStage;
        public int AttemptsRemaining;
        public long RiftStartTimestampUnix;
        public long RiftEndTimestampUnix;
        public bool RiftActive;
        public List<int> CompletedStages = new();
    }

    [Serializable]
    public class VoidRiftConfig
    {
        public string riftName;
        public RiftTheme theme;
        public string description;
        public int stageCount = 5;
        public int maxAttempts = 3;
        public int durationDays = 3;
        public List<RiftModifier> stageModifiers;

        // Rewards per stage
        public int aetherCrystalsPerStage = 5;
        public float mythicEquipmentChance = 0.1f;
        public double goldPerStage = 10000;
    }

    public class VoidRiftManager : MonoBehaviour
    {
        public static VoidRiftManager Instance { get; private set; }

        [Header("Rift Configurations")]
        [SerializeField] List<VoidRiftConfig> _riftRotation;
        [SerializeField] EnemySpawner _spawner;

        [Header("Schedule")]
        [SerializeField] int _cycleDays = 14; // Bi-weekly

        int _currentRiftIndex;
        int _currentStage;
        int _attemptsRemaining;
        long _riftStartTimestamp;
        long _riftEndTimestamp;
        bool _riftActive;
        bool _inRiftCombat;
        HashSet<int> _completedStages = new();

        public VoidRiftConfig CurrentRift =>
            _riftRotation != null && _currentRiftIndex < _riftRotation.Count
                ? _riftRotation[_currentRiftIndex]
                : null;

        public int CurrentStage => _currentStage;
        public int AttemptsRemaining => _attemptsRemaining;
        public bool IsRiftActive => _riftActive;
        public bool InRiftCombat => _inRiftCombat;
        public IReadOnlyCollection<int> CompletedStages => _completedStages;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void Start()
        {
            CheckRiftSchedule();
        }

        void CheckRiftSchedule()
        {
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            if (_riftActive && now >= _riftEndTimestamp)
            {
                // Rift expired
                _riftActive = false;
                _inRiftCombat = false;
                _completedStages.Clear();
                AdvanceToNextRift();
            }

            if (!_riftActive)
            {
                StartNewRift();
            }
        }

        void StartNewRift()
        {
            if (_riftRotation == null || _riftRotation.Count == 0) return;

            _riftActive = true;
            var rift = CurrentRift;
            if (rift == null) return;

            _attemptsRemaining = rift.maxAttempts;
            _completedStages.Clear();
            _currentStage = 0;

            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            _riftStartTimestamp = now;
            _riftEndTimestamp = now + rift.durationDays * 86400L;

            EventBus.Publish(new VoidRiftStartedEvent
            {
                RiftName = rift.riftName,
                Theme = rift.theme
            });

            Debug.Log($"[VoidRift] New rift: {rift.riftName} ({rift.theme})");
        }

        void AdvanceToNextRift()
        {
            if (_riftRotation == null || _riftRotation.Count == 0) return;
            _currentRiftIndex = (_currentRiftIndex + 1) % _riftRotation.Count;
        }

        public long GetTimeRemaining()
        {
            if (!_riftActive) return 0;
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            return Mathf.Max(0, (int)(_riftEndTimestamp - now));
        }

        public bool CanAttemptStage(int stage)
        {
            if (!_riftActive) return false;
            if (_attemptsRemaining <= 0) return false;
            if (stage < 0 || stage >= (CurrentRift?.stageCount ?? 5)) return false;
            // Must complete stages in order
            if (stage > 0 && !_completedStages.Contains(stage - 1)) return false;
            return true;
        }

        public void StartStage(int stage)
        {
            if (!CanAttemptStage(stage)) return;

            _currentStage = stage;
            _attemptsRemaining--;
            _inRiftCombat = true;

            GameManager.Instance?.SetState(GameState.VoidRift);

            // Spawn enemies at endgame difficulty
            int enemyLevel = 100 + (stage + 1) * 20;
            _spawner?.SpawnWave(enemyLevel);

            EventBus.Publish(new VoidRiftStageStartedEvent
            {
                StageNumber = stage,
                RiftName = CurrentRift?.riftName ?? "",
                EnemyLevel = enemyLevel
            });

            Debug.Log($"[VoidRift] Stage {stage + 1} started, enemy level {enemyLevel}, attempts remaining: {_attemptsRemaining}");
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_inRiftCombat) return;
            if (GameManager.Instance?.CurrentState != GameState.VoidRift) return;

            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnStageClear();
            }
        }

        void OnStageClear()
        {
            _inRiftCombat = false;
            _completedStages.Add(_currentStage);

            // Award rewards
            var rift = CurrentRift;
            if (rift != null)
            {
                var currency = Economy.CurrencyManager.Instance;
                if (currency != null)
                {
                    currency.AddCurrency(CurrencyType.AetherCrystals, rift.aetherCrystalsPerStage);
                    currency.AddCurrency(CurrencyType.Gold, rift.goldPerStage);
                }
            }

            EventBus.Publish(new VoidRiftStageClearedEvent
            {
                StageNumber = _currentStage,
                AetherCrystalsEarned = rift?.aetherCrystalsPerStage ?? 0,
                AllStagesCleared = _completedStages.Count >= (rift?.stageCount ?? 5)
            });

            if (_completedStages.Count >= (rift?.stageCount ?? 5))
            {
                OnRiftCompleted();
            }
            else
            {
                GameManager.Instance?.SetState(GameState.Combat);
            }
        }

        void OnRiftCompleted()
        {
            // Bonus completion reward
            var currency = Economy.CurrencyManager.Instance;
            currency?.AddCurrency(CurrencyType.AetherCrystals, 20);

            EventBus.Publish(new VoidRiftCompletedEvent
            {
                RiftName = CurrentRift?.riftName ?? "",
                TotalAetherCrystals = (CurrentRift?.aetherCrystalsPerStage ?? 5) * (CurrentRift?.stageCount ?? 5) + 20
            });

            GameManager.Instance?.SetState(GameState.Combat);
            Debug.Log("[VoidRift] Rift completed!");
        }

        public void FailStage()
        {
            _inRiftCombat = false;

            EventBus.Publish(new VoidRiftStageFailedEvent
            {
                StageNumber = _currentStage,
                AttemptsRemaining = _attemptsRemaining
            });

            GameManager.Instance?.SetState(GameState.Combat);
        }

        public RiftModifier GetStageModifier(int stage)
        {
            var rift = CurrentRift;
            if (rift?.stageModifiers == null || stage >= rift.stageModifiers.Count) return null;
            return rift.stageModifiers[stage];
        }

        // Save/Load
        public VoidRiftSaveData GatherSaveData()
        {
            return new VoidRiftSaveData
            {
                CurrentRiftIndex = _currentRiftIndex,
                CurrentStage = _currentStage,
                AttemptsRemaining = _attemptsRemaining,
                RiftStartTimestampUnix = _riftStartTimestamp,
                RiftEndTimestampUnix = _riftEndTimestamp,
                RiftActive = _riftActive,
                CompletedStages = new List<int>(_completedStages)
            };
        }

        public void LoadSaveData(VoidRiftSaveData data)
        {
            if (data == null) return;
            _currentRiftIndex = data.CurrentRiftIndex;
            _currentStage = data.CurrentStage;
            _attemptsRemaining = data.AttemptsRemaining;
            _riftStartTimestamp = data.RiftStartTimestampUnix;
            _riftEndTimestamp = data.RiftEndTimestampUnix;
            _riftActive = data.RiftActive;
            _completedStages.Clear();
            if (data.CompletedStages != null)
                foreach (var s in data.CompletedStages)
                    _completedStages.Add(s);

            CheckRiftSchedule();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
