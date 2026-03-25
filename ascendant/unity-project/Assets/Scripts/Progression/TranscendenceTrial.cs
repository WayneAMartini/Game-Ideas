using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Progression
{
    public class TranscendenceTrial : MonoBehaviour
    {
        public static TranscendenceTrial Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _totalWaves = 5;
        [SerializeField] float _timeLimitSeconds = 300f; // 5 minutes
        [SerializeField] float _difficultyScalePerWave = 1.5f;
        [SerializeField] EnemyData[] _trialEnemies;

        int _currentWave;
        float _timer;
        bool _isActive;
        int _trialHeroSlot = -1;
        string _trialClassId;

        public int CurrentWave => _currentWave;
        public int TotalWaves => _totalWaves;
        public float TimeRemaining => _timer;
        public float TimeLimitSeconds => _timeLimitSeconds;
        public bool IsActive => _isActive;
        public int TrialHeroSlot => _trialHeroSlot;

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

        void Update()
        {
            if (!_isActive) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                // Time's up — trial failed
                EndTrial(false);
            }
        }

        public bool StartTrial(int heroSlot)
        {
            var demigodSystem = DemigodSystem.Instance;
            if (demigodSystem == null || !demigodSystem.CanAttemptTranscendence(heroSlot))
                return false;

            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data == null) return false;

            _trialHeroSlot = heroSlot;
            _trialClassId = hero.Data.classId;
            _currentWave = 0;
            _timer = _timeLimitSeconds;
            _isActive = true;

            GameManager.Instance?.SetState(GameState.TranscendenceTrial);

            EventBus.Publish(new TranscendenceTrialStartedEvent
            {
                HeroSlot = heroSlot,
                ClassId = _trialClassId
            });

            SpawnNextWave();
            return true;
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_isActive) return;

            // Check if wave is cleared
            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnWaveCleared();
            }
        }

        void OnWaveCleared()
        {
            _currentWave++;

            if (_currentWave >= _totalWaves)
            {
                // All waves cleared — trial passed!
                EndTrial(true);
                return;
            }

            SpawnNextWave();
        }

        void SpawnNextWave()
        {
            EnemyManager.Instance?.ClearAll();

            // Calculate wave difficulty
            int baseStage = 100; // Island 12 boss-tier difficulty
            float waveMultiplier = Mathf.Pow(_difficultyScalePerWave, _currentWave);
            int effectiveStage = Mathf.RoundToInt(baseStage * waveMultiplier);

            // Spawn enemies based on available trial enemies
            var spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null && _trialEnemies != null && _trialEnemies.Length > 0)
            {
                int enemyCount = 3 + _currentWave; // 3,4,5,6,7 enemies per wave
                for (int i = 0; i < enemyCount; i++)
                {
                    var enemyData = _trialEnemies[Random.Range(0, _trialEnemies.Length)];
                    spawner.SpawnSingleEnemy(enemyData, effectiveStage);
                }
            }
            else
            {
                // Fallback: use default spawner
                spawner?.SpawnWave(effectiveStage);
            }
        }

        void EndTrial(bool success)
        {
            _isActive = false;

            EventBus.Publish(new TranscendenceTrialCompletedEvent
            {
                HeroSlot = _trialHeroSlot,
                ClassId = _trialClassId,
                Success = success
            });

            if (success)
            {
                // Retire the hero as a Demigod
                DemigodSystem.Instance?.RetireDemigod(_trialClassId);
            }

            // Return to combat state
            GameManager.Instance?.SetState(GameState.Combat);

            _trialHeroSlot = -1;
            _trialClassId = null;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
