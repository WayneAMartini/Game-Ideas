using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Islands;

namespace Ascendant.Progression
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] ProgressionConfig _config;
        [SerializeField] EnemySpawner _spawner;

        int _currentIsland = 1;
        int _currentStage = 1;
        float _transitionTimer;
        bool _isTransitioning;

        public int CurrentIsland => _currentIsland;
        public int CurrentStage => _currentStage;
        public int StagesPerIsland => _config != null ? _config.stagesPerIsland : 100;
        public bool IsTransitioning => _isTransitioning;

        public string CurrentIslandName
        {
            get
            {
                var island = IslandManager.Instance?.CurrentIsland;
                return island != null ? island.islandName : $"Island {_currentIsland}";
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
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        void Start()
        {
            // Sync with IslandManager if available
            if (IslandManager.Instance != null)
            {
                _currentIsland = IslandManager.Instance.CurrentIslandNumber;
                // Apply biome for starting island
                var island = IslandManager.Instance.CurrentIsland;
                if (island != null)
                {
                    BiomeEffectSystem.Instance?.SetBiome(island.biomeData);
                }
            }

            StartStage();
        }

        void Update()
        {
            if (_isTransitioning)
            {
                _transitionTimer -= Time.deltaTime;
                if (_transitionTimer <= 0f)
                {
                    _isTransitioning = false;
                    GameManager.Instance?.SetState(GameState.Combat);
                    StartStage();
                }
            }
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            // Don't check wave clear during boss fights — boss controllers handle that
            var state = GameManager.Instance?.CurrentState;
            if (state == GameState.IslandBoss || state == GameState.RealmBoss)
                return;

            // Check if all enemies are dead
            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnWaveCleared();
            }
        }

        void OnIslandChanged(IslandChangedEvent evt)
        {
            _currentIsland = evt.IslandData != null ? evt.IslandData.islandNumber : 1;
            _currentStage = 1;

            // Apply new biome
            BiomeEffectSystem.Instance?.SetBiome(evt.IslandData?.biomeData);

            StartStage();
        }

        void OnBossDefeated(BossDefeatedEvent evt)
        {
            if (evt.IsIslandBoss)
            {
                // Island boss defeated — trigger island transition
                _isTransitioning = true;
                _transitionTimer = 2f; // Longer delay for boss celebration
                GameManager.Instance?.SetState(GameState.StageTransition);

                // Advance to next island
                _currentStage = 1;
                IslandManager.Instance?.AdvanceToNextIsland();
                _currentIsland = IslandManager.Instance?.CurrentIslandNumber ?? _currentIsland + 1;
            }
        }

        void OnWaveCleared()
        {
            EventBus.Publish(new WaveClearedEvent { StageNumber = _currentStage });
            AdvanceStage();
        }

        void AdvanceStage()
        {
            _currentStage++;

            int stagesPerIsland = _config != null ? _config.stagesPerIsland : 100;

            if (_currentStage > stagesPerIsland)
            {
                // Island complete — handled by IslandBossController when boss is defeated
                // This shouldn't happen during normal flow since stage 100 is a boss
                _currentStage = 1;
                _currentIsland++;
            }

            _isTransitioning = true;
            _transitionTimer = _config != null ? _config.stageTransitionDelay : 1f;

            GameManager.Instance?.SetState(GameState.StageTransition);

            EventBus.Publish(new StageAdvancedEvent
            {
                NewStage = _currentStage,
                Island = _currentIsland
            });
        }

        void StartStage()
        {
            Debug.Log($"[StageManager] Starting {CurrentIslandName} Stage {_currentStage}. Spawner={_spawner != null}, EnemyManager={EnemyManager.Instance != null}");
            EnemyManager.Instance?.ClearAll();

            int stagesPerIsland = _config != null ? _config.stagesPerIsland : 100;

            // Check for island boss (stage 100)
            if (_currentStage >= stagesPerIsland)
            {
                StartIslandBoss();
                return;
            }

            // Check for mini-boss (every 10th stage)
            if (MiniBossController.IsMiniBossStage(_currentStage))
            {
                StartMiniBoss();
                return;
            }

            // Normal wave — use island-specific enemies if available
            SpawnIslandWave();
        }

        void SpawnIslandWave()
        {
            var islandManager = IslandManager.Instance;
            if (islandManager != null && _spawner != null)
            {
                var enemies = islandManager.GetCurrentEnemyTypes();
                if (enemies != null && enemies.Count > 0)
                {
                    _spawner.SpawnWaveWithEnemies(_currentStage, enemies);
                    return;
                }
            }

            // Fallback to default spawning
            _spawner?.SpawnWave(_currentStage);
        }

        void StartMiniBoss()
        {
            GameManager.Instance?.SetState(GameState.MiniBoss);

            var miniBoss = MiniBossController.Instance;
            if (miniBoss != null)
            {
                var islandManager = IslandManager.Instance;
                var enemies = islandManager?.GetCurrentEnemyTypes();
                EnemyData bossData = enemies != null && enemies.Count > 0
                    ? enemies[Random.Range(0, enemies.Count)]
                    : null;

                if (bossData != null)
                {
                    var parent = _spawner != null ? _spawner.transform : transform;
                    miniBoss.SpawnMiniBoss(bossData, _currentStage, parent);
                    return;
                }
            }

            // Fallback — just spawn a tough wave
            _spawner?.SpawnWave(_currentStage);
        }

        void StartIslandBoss()
        {
            GameManager.Instance?.SetState(GameState.IslandBoss);

            var islandManager = IslandManager.Instance;
            var island = islandManager?.CurrentIsland;

            // Check if this is the final island (Realm Boss)
            if (islandManager != null && islandManager.AllIslandsCleared)
            {
                GameManager.Instance?.SetState(GameState.RealmBoss);
                var realmBoss = RealmBossController.Instance;
                if (realmBoss != null)
                {
                    var parent = _spawner != null ? _spawner.transform : transform;
                    realmBoss.StartRealmBoss(parent);
                    return;
                }
            }

            var bossFight = IslandBossController.Instance;
            if (bossFight != null && island != null)
            {
                var parent = _spawner != null ? _spawner.transform : transform;
                bossFight.StartBossFight(island, parent);
                return;
            }

            // Fallback
            _spawner?.SpawnWave(_currentStage);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
