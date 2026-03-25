using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Events
{
    [Serializable]
    public class TowerSaveData
    {
        public int CurrentFloor;
        public int PersonalBestFloor;
        public int WeeklyBestFloor;
        public long WeekResetTimestampUnix;
        public List<string> SelectedBuffIds = new();
        public float[] PartyHpPercents = new float[4];
        public bool InProgress;
    }

    public class TowerOfTrials : MonoBehaviour
    {
        public static TowerOfTrials Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _maxFloors = 50;
        [SerializeField] List<TowerModifier> _modifierPool;
        [SerializeField] EnemySpawner _spawner;

        int _currentFloor;
        int _personalBest;
        int _weeklyBest;
        long _weekResetTimestamp;
        bool _inProgress;
        TowerModifier _currentModifier;
        List<TowerBuff> _selectedBuffs = new();
        float[] _partyHpPercents = new float[4] { 1f, 1f, 1f, 1f };

        public int CurrentFloor => _currentFloor;
        public int PersonalBest => _personalBest;
        public int WeeklyBest => _weeklyBest;
        public int MaxFloors => _maxFloors;
        public bool InProgress => _inProgress;
        public TowerModifier CurrentModifier => _currentModifier;
        public IReadOnlyList<TowerBuff> SelectedBuffs => _selectedBuffs;

        // Floor milestone rewards
        static readonly TowerReward[] MilestoneRewards = new[]
        {
            new TowerReward { floorMilestone = 10, goldReward = 5000, materialReward = 100, aetherCrystals = 0, guaranteedEquipment = false },
            new TowerReward { floorMilestone = 20, goldReward = 15000, materialReward = 300, aetherCrystals = 0, guaranteedEquipment = true },
            new TowerReward { floorMilestone = 30, goldReward = 30000, materialReward = 500, aetherCrystals = 5, guaranteedEquipment = false },
            new TowerReward { floorMilestone = 40, goldReward = 50000, materialReward = 800, aetherCrystals = 10, guaranteedEquipment = true },
            new TowerReward { floorMilestone = 50, goldReward = 100000, materialReward = 1500, aetherCrystals = 25, guaranteedEquipment = true },
        };

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
            CheckWeeklyReset();
        }

        void CheckWeeklyReset()
        {
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (_weekResetTimestamp == 0 || now >= _weekResetTimestamp)
            {
                _weeklyBest = 0;
                _inProgress = false;
                _currentFloor = 0;
                // Next Monday 00:00 UTC
                var nextMonday = DateTime.UtcNow.Date;
                while (nextMonday.DayOfWeek != DayOfWeek.Monday)
                    nextMonday = nextMonday.AddDays(1);
                if (nextMonday <= DateTime.UtcNow.Date)
                    nextMonday = nextMonday.AddDays(7);
                _weekResetTimestamp = new DateTimeOffset(nextMonday).ToUnixTimeSeconds();
            }
        }

        public long GetTimeUntilReset()
        {
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            return Mathf.Max(0, (int)(_weekResetTimestamp - now));
        }

        public bool CanEnterTower()
        {
            CheckWeeklyReset();
            return true; // Tower can always be attempted, just resets weekly
        }

        public void EnterTower()
        {
            if (_inProgress) return;

            _inProgress = true;
            _currentFloor = 0;
            _selectedBuffs.Clear();
            _partyHpPercents = new float[4] { 1f, 1f, 1f, 1f };

            GameManager.Instance?.SetState(GameState.TowerOfTrials);

            EventBus.Publish(new TowerEnteredEvent());
            AdvanceFloor();
        }

        public void AdvanceFloor()
        {
            _currentFloor++;

            if (_currentFloor > _maxFloors)
            {
                CompleteTower();
                return;
            }

            // Assign random modifier for this floor
            _currentModifier = GetRandomModifier(_currentFloor);

            // Check if buff selection is available (every 5 floors)
            if (_currentFloor > 1 && (_currentFloor - 1) % 5 == 0)
            {
                var choices = TowerBuff.GenerateBuffChoices(_currentFloor * 31 + _weeklyBest);
                EventBus.Publish(new TowerBuffChoiceEvent
                {
                    Choices = choices,
                    FloorNumber = _currentFloor
                });
                // Wait for buff selection before spawning
                return;
            }

            SpawnFloorEnemies();
        }

        public void SelectBuff(TowerBuff buff)
        {
            _selectedBuffs.Add(buff);
            SpawnFloorEnemies();
        }

        void SpawnFloorEnemies()
        {
            EnemyManager.Instance?.ClearAll();

            int enemyLevel = _currentFloor * 2;
            int enemyCount = 3 + _currentFloor / 10;

            // Use spawner to create enemies at the floor's level
            if (_spawner != null)
            {
                _spawner.SpawnWave(enemyLevel);
            }

            EventBus.Publish(new TowerFloorStartedEvent
            {
                FloorNumber = _currentFloor,
                ModifierName = _currentModifier?.modifierName ?? "None",
                EnemyLevel = enemyLevel
            });

            Debug.Log($"[TowerOfTrials] Floor {_currentFloor} - Modifier: {_currentModifier?.modifierName ?? "None"}, Level: {enemyLevel}");
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_inProgress) return;
            if (GameManager.Instance?.CurrentState != GameState.TowerOfTrials) return;

            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnFloorCleared();
            }
        }

        void OnFloorCleared()
        {
            // Save party HP percentages (HP persists between floors)
            SavePartyHp();

            // Check milestone rewards
            var reward = GetMilestoneReward(_currentFloor);
            if (reward != null)
            {
                AwardMilestoneReward(reward);
            }

            // Update bests
            if (_currentFloor > _personalBest) _personalBest = _currentFloor;
            if (_currentFloor > _weeklyBest) _weeklyBest = _currentFloor;

            EventBus.Publish(new TowerFloorClearedEvent
            {
                FloorNumber = _currentFloor,
                IsMilestone = reward != null,
                PersonalBest = _personalBest
            });

            // Advance to next floor
            AdvanceFloor();
        }

        void SavePartyHp()
        {
            var party = Party.PartyManager.Instance;
            if (party == null) return;

            for (int i = 0; i < 4; i++)
            {
                var hero = party.GetHero(i);
                if (hero != null && hero.MaxHp > 0)
                    _partyHpPercents[i] = hero.CurrentHp / hero.MaxHp;
                else
                    _partyHpPercents[i] = 0f;
            }
        }

        void CompleteTower()
        {
            _inProgress = false;
            _personalBest = _maxFloors;
            _weeklyBest = _maxFloors;

            EventBus.Publish(new TowerCompletedEvent
            {
                FloorsCleared = _maxFloors,
                PersonalBest = _personalBest
            });

            // Submit leaderboard score
            Backend.LeaderboardManager.Instance?.SubmitBossDamage(_maxFloors); // Reuse for tower

            GameManager.Instance?.SetState(GameState.Combat);
            Debug.Log("[TowerOfTrials] Tower completed!");
        }

        public void FailTower()
        {
            SavePartyHp();
            _inProgress = false;

            if (_currentFloor > _personalBest) _personalBest = _currentFloor;
            if (_currentFloor > _weeklyBest) _weeklyBest = _currentFloor;

            EventBus.Publish(new TowerFailedEvent
            {
                FloorsCleared = _currentFloor,
                PersonalBest = _personalBest
            });

            GameManager.Instance?.SetState(GameState.Combat);
            Debug.Log($"[TowerOfTrials] Failed at floor {_currentFloor}");
        }

        public void LeaveTower()
        {
            if (_inProgress)
            {
                FailTower();
            }
        }

        TowerModifier GetRandomModifier(int floor)
        {
            if (_modifierPool == null || _modifierPool.Count == 0) return null;
            int seed = floor * 17 + (int)(_weekResetTimestamp % 1000);
            var rng = new System.Random(seed);
            return _modifierPool[rng.Next(_modifierPool.Count)];
        }

        TowerReward GetMilestoneReward(int floor)
        {
            foreach (var reward in MilestoneRewards)
            {
                if (reward.floorMilestone == floor) return reward;
            }
            return null;
        }

        void AwardMilestoneReward(TowerReward reward)
        {
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null) return;

            if (reward.goldReward > 0)
                currency.AddCurrency(CurrencyType.Gold, reward.goldReward);
            if (reward.aetherCrystals > 0)
                currency.AddCurrency(CurrencyType.AetherCrystals, reward.aetherCrystals);

            Debug.Log($"[TowerOfTrials] Milestone reward at floor {reward.floorMilestone}: {reward.goldReward} gold, {reward.aetherCrystals} Aether Crystals");
        }

        // Buff stat aggregation
        public float GetTotalAtkMultiplier()
        {
            float mult = _currentModifier != null ? _currentModifier.atkMultiplier : 1f;
            foreach (var buff in _selectedBuffs) mult += buff.atkBonus;
            return mult;
        }

        public float GetTotalDefMultiplier()
        {
            float mult = _currentModifier != null ? _currentModifier.defMultiplier : 1f;
            foreach (var buff in _selectedBuffs) mult += buff.defBonus;
            return mult;
        }

        public float GetTotalHpMultiplier()
        {
            float mult = _currentModifier != null ? _currentModifier.hpMultiplier : 1f;
            foreach (var buff in _selectedBuffs) mult += buff.hpBonus;
            return mult;
        }

        // Save/Load
        public TowerSaveData GatherSaveData()
        {
            return new TowerSaveData
            {
                CurrentFloor = _currentFloor,
                PersonalBestFloor = _personalBest,
                WeeklyBestFloor = _weeklyBest,
                WeekResetTimestampUnix = _weekResetTimestamp,
                InProgress = _inProgress,
                PartyHpPercents = (float[])_partyHpPercents.Clone()
            };
        }

        public void LoadSaveData(TowerSaveData data)
        {
            if (data == null) return;
            _currentFloor = data.CurrentFloor;
            _personalBest = data.PersonalBestFloor;
            _weeklyBest = data.WeeklyBestFloor;
            _weekResetTimestamp = data.WeekResetTimestampUnix;
            _inProgress = data.InProgress;
            if (data.PartyHpPercents != null && data.PartyHpPercents.Length == 4)
                _partyHpPercents = (float[])data.PartyHpPercents.Clone();

            CheckWeeklyReset();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
