using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

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
            // Check if all enemies are dead
            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnWaveCleared();
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

            if (_currentStage > (_config != null ? _config.stagesPerIsland : 100))
            {
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
            EnemyManager.Instance?.ClearAll();
            _spawner?.SpawnWave(_currentStage);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
