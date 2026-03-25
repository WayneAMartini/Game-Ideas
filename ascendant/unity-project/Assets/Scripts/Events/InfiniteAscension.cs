using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Events
{
    [Serializable]
    public class InfiniteAscensionSaveData
    {
        public bool Unlocked;
        public int HighestIslandReached;
        public int CurrentIslandNumber;
        public int CurrentStage;
        public int Seed;
    }

    public class InfiniteAscension : MonoBehaviour
    {
        public static InfiniteAscension Instance { get; private set; }

        [SerializeField] EnemySpawner _spawner;

        bool _unlocked;
        int _highestIsland;
        int _currentIsland;
        int _currentStage;
        int _seed;
        ProceduralIsland _currentProceduralIsland;
        bool _inInfiniteCombat;

        public bool Unlocked => _unlocked;
        public int HighestIsland => _highestIsland;
        public int CurrentIsland => _currentIsland;
        public int CurrentStage => _currentStage;
        public ProceduralIsland CurrentProceduralIsland => _currentProceduralIsland;
        public bool InCombat => _inInfiniteCombat;

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
            EventBus.Subscribe<RealmBossDefeatedEvent>(OnRealmBossDefeated);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<RealmBossDefeatedEvent>(OnRealmBossDefeated);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnRealmBossDefeated(RealmBossDefeatedEvent evt)
        {
            if (evt.RealmNumber >= 3)
            {
                Unlock();
            }
        }

        public void Unlock()
        {
            if (_unlocked) return;
            _unlocked = true;
            _seed = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % int.MaxValue);

            EventBus.Publish(new InfiniteAscensionUnlockedEvent());
            Debug.Log("[InfiniteAscension] Unlocked! Procedural islands now available.");
        }

        public void EnterInfiniteMode()
        {
            if (!_unlocked) return;

            _currentIsland = 37; // First island after Realm 3
            _currentStage = 1;
            _inInfiniteCombat = true;
            _currentProceduralIsland = ProceduralIslandGenerator.Generate(_currentIsland, _seed);

            GameManager.Instance?.SetState(GameState.InfiniteAscension);

            EventBus.Publish(new InfiniteAscensionIslandStartedEvent
            {
                IslandNumber = _currentIsland,
                IslandName = _currentProceduralIsland.IslandName,
                DifficultyMultiplier = _currentProceduralIsland.DifficultyMultiplier
            });

            SpawnStage();
            Debug.Log($"[InfiniteAscension] Entered: {_currentProceduralIsland.IslandName} (Island {_currentIsland})");
        }

        public void ContinueFromHighest()
        {
            if (!_unlocked) return;
            _currentIsland = Mathf.Max(37, _highestIsland);
            _currentStage = 1;
            _inInfiniteCombat = true;
            _currentProceduralIsland = ProceduralIslandGenerator.Generate(_currentIsland, _seed);

            GameManager.Instance?.SetState(GameState.InfiniteAscension);
            SpawnStage();
        }

        void SpawnStage()
        {
            EnemyManager.Instance?.ClearAll();

            if (_currentProceduralIsland == null) return;

            int stageCount = _currentProceduralIsland.StageCount;

            // Island boss at final stage
            if (_currentStage >= stageCount)
            {
                // Boss stage
                int bossLevel = (int)(100 * _currentProceduralIsland.DifficultyMultiplier);
                _spawner?.SpawnWave(bossLevel);
                return;
            }

            // Mini-boss every 10 stages
            bool isMiniBoss = _currentStage > 0 && _currentStage % 10 == 0 && _currentStage < stageCount;
            int enemyLevel = (int)(_currentStage * _currentProceduralIsland.DifficultyMultiplier);

            _spawner?.SpawnWave(Mathf.Max(1, enemyLevel));
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_inInfiniteCombat) return;
            if (GameManager.Instance?.CurrentState != GameState.InfiniteAscension) return;

            // Award scaled rewards
            var currency = Economy.CurrencyManager.Instance;
            if (currency != null && _currentProceduralIsland != null)
            {
                currency.AddCurrency(CurrencyType.Gold, evt.GoldReward * _currentProceduralIsland.GoldMultiplier);
            }

            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnStageClear();
            }
        }

        void OnStageClear()
        {
            if (_currentProceduralIsland == null) return;

            _currentStage++;

            if (_currentStage > _currentProceduralIsland.StageCount)
            {
                // Island complete, advance to next procedural island
                OnIslandComplete();
                return;
            }

            SpawnStage();
        }

        void OnIslandComplete()
        {
            if (_currentIsland > _highestIsland) _highestIsland = _currentIsland;

            // Award completion bonus
            var currency = Economy.CurrencyManager.Instance;
            if (currency != null && _currentProceduralIsland != null)
            {
                double bonus = 10000 * _currentProceduralIsland.DifficultyMultiplier;
                currency.AddCurrency(CurrencyType.Gold, bonus);
                currency.AddCurrency(CurrencyType.AetherCrystals, 5);
            }

            // Milestone rewards
            int islandsBeyondRealm3 = _currentIsland - 36;
            if (islandsBeyondRealm3 % 50 == 0)
            {
                currency?.AddCurrency(CurrencyType.AetherCrystals, 50);
            }

            EventBus.Publish(new InfiniteAscensionIslandCompletedEvent
            {
                IslandNumber = _currentIsland,
                IslandName = _currentProceduralIsland.IslandName,
                HighestIsland = _highestIsland
            });

            // Submit leaderboard
            Backend.LeaderboardManager.Instance?.SubmitBossDamage(_highestIsland);

            // Generate next island
            _currentIsland++;
            _currentStage = 1;
            _currentProceduralIsland = ProceduralIslandGenerator.Generate(_currentIsland, _seed);

            EventBus.Publish(new InfiniteAscensionIslandStartedEvent
            {
                IslandNumber = _currentIsland,
                IslandName = _currentProceduralIsland.IslandName,
                DifficultyMultiplier = _currentProceduralIsland.DifficultyMultiplier
            });

            SpawnStage();
        }

        public void LeaveInfiniteMode()
        {
            _inInfiniteCombat = false;
            GameManager.Instance?.SetState(GameState.Combat);
        }

        // Save/Load
        public InfiniteAscensionSaveData GatherSaveData()
        {
            return new InfiniteAscensionSaveData
            {
                Unlocked = _unlocked,
                HighestIslandReached = _highestIsland,
                CurrentIslandNumber = _currentIsland,
                CurrentStage = _currentStage,
                Seed = _seed
            };
        }

        public void LoadSaveData(InfiniteAscensionSaveData data)
        {
            if (data == null) return;
            _unlocked = data.Unlocked;
            _highestIsland = data.HighestIslandReached;
            _currentIsland = data.CurrentIslandNumber;
            _currentStage = data.CurrentStage;
            _seed = data.Seed;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
