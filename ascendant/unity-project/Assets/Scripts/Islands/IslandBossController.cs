using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Islands
{
    public class IslandBossController : MonoBehaviour
    {
        public static IslandBossController Instance { get; private set; }

        [SerializeField] EnemySpawner _spawner;

        Enemy _boss;
        IslandData _islandData;
        int _currentPhaseIndex;
        List<BossMechanic> _activePhaseMechanics = new();
        bool _isBossFight;
        float _bossMaxHp;
        float _phaseTransitionCooldown;

        public bool IsBossFight => _isBossFight;
        public Enemy Boss => _boss;
        public int CurrentPhaseIndex => _currentPhaseIndex;
        public float BossHpPercent => _boss != null ? _boss.CurrentHp / _bossMaxHp : 0f;

        public BossPhaseData CurrentPhase
        {
            get
            {
                if (_islandData?.bossPhases == null || _currentPhaseIndex >= _islandData.bossPhases.Count)
                    return null;
                return _islandData.bossPhases[_currentPhaseIndex];
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
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void Update()
        {
            if (!_isBossFight || _boss == null || _boss.IsDead) return;

            if (_phaseTransitionCooldown > 0f)
            {
                _phaseTransitionCooldown -= Time.deltaTime;
                return;
            }

            // Update active mechanics
            foreach (var mechanic in _activePhaseMechanics)
            {
                mechanic.Update(Time.deltaTime);

                // Handle add spawning
                if (mechanic is AddSpawningMechanic addMech && addMech.ShouldSpawnAdds())
                {
                    SpawnBossAdds();
                }
            }

            // Check for phase transitions
            CheckPhaseTransition();
        }

        public void StartBossFight(IslandData island, Transform parent)
        {
            _islandData = island;
            _currentPhaseIndex = 0;

            var prefab = GetEnemyPrefab();
            if (prefab == null || island.enemyTypes == null || island.enemyTypes.Count == 0)
            {
                Debug.LogError("[IslandBossController] Cannot start boss fight - missing prefab or enemy data");
                return;
            }

            Vector3 pos = new Vector3(2f, 0.5f, 0f);
            _boss = Instantiate(prefab, pos, Quaternion.identity, parent);
            _boss.Initialize(island.enemyTypes[0], 100);
            _boss.transform.localScale = Vector3.one * 2f; // Boss is larger

            _bossMaxHp = _boss.MaxHp * island.islandBossHpMultiplier;
            // Note: The boss HP is scaled via the stage 100 stats + multiplier
            // The actual HP modification would need to be applied through Enemy initialization

            EnemyManager.Instance?.Register(_boss);
            _isBossFight = true;

            // Activate first phase mechanics
            ActivatePhase(0);

            EventBus.Publish(new IslandBossSpawnedEvent
            {
                IslandIndex = island.islandNumber - 1,
                BossName = island.islandBossName
            });

            Debug.Log($"[IslandBossController] Boss fight started: {island.islandBossName}");
        }

        void ActivatePhase(int phaseIndex)
        {
            // Deactivate previous mechanics
            foreach (var mechanic in _activePhaseMechanics)
                mechanic.Deactivate();
            _activePhaseMechanics.Clear();

            _currentPhaseIndex = phaseIndex;
            var phase = CurrentPhase;
            if (phase == null) return;

            // Create mechanics for this phase
            if (phase.mechanics != null)
            {
                foreach (var mechType in phase.mechanics)
                {
                    var mechanic = CreateMechanic(mechType);
                    mechanic.Initialize(_boss);
                    mechanic.Activate();
                    _activePhaseMechanics.Add(mechanic);
                }
            }

            EventBus.Publish(new BossPhaseChangedEvent
            {
                PhaseIndex = phaseIndex,
                PhaseName = phase.phaseName,
                HpThreshold = phase.hpThreshold
            });

            // Brief transition cooldown
            _phaseTransitionCooldown = 1f;

            Debug.Log($"[IslandBossController] Phase {phaseIndex + 1}: {phase.phaseName}");
        }

        void CheckPhaseTransition()
        {
            if (_islandData?.bossPhases == null) return;

            float hpPercent = _boss.CurrentHp / _bossMaxHp;

            // Check if we should transition to next phase
            int nextPhase = _currentPhaseIndex + 1;
            if (nextPhase < _islandData.bossPhases.Count)
            {
                var next = _islandData.bossPhases[nextPhase];
                if (hpPercent <= next.hpThreshold)
                {
                    ActivatePhase(nextPhase);
                }
            }
        }

        protected BossMechanic CreateMechanic(BossMechanicType type)
        {
            return type switch
            {
                BossMechanicType.Enrage => new EnrageMechanic(),
                BossMechanicType.ShieldPhase => new ShieldPhaseMechanic(),
                BossMechanicType.AddSpawning => new AddSpawningMechanic(),
                BossMechanicType.GroundSlam => new GroundSlamMechanic(),
                BossMechanicType.LifeSteal => new LifeStealMechanic(),
                BossMechanicType.Split => new SplitMechanic(),
                BossMechanicType.Reflect => new ReflectMechanic(),
                _ => new EnrageMechanic()
            };
        }

        void SpawnBossAdds()
        {
            if (_spawner != null)
            {
                _spawner.SpawnWave(Mathf.Max(1, 50)); // Spawn at mid-level difficulty
            }
            Debug.Log("[IslandBossController] Boss summoned adds!");
        }

        Enemy GetEnemyPrefab()
        {
            if (_spawner == null) return null;
            var field = typeof(EnemySpawner).GetField("_enemyPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_spawner) as Enemy;
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_isBossFight || _boss == null) return;
            if (evt.EnemyId == _boss.Id)
            {
                EndBossFight();
            }
        }

        void EndBossFight()
        {
            foreach (var mechanic in _activePhaseMechanics)
                mechanic.Deactivate();
            _activePhaseMechanics.Clear();

            _isBossFight = false;

            EventBus.Publish(new BossDefeatedEvent
            {
                BossName = _islandData?.islandBossName ?? "Unknown",
                IsIslandBoss = true,
                IsRealmBoss = false
            });

            // Trigger island completion
            IslandManager.Instance?.CompleteCurrentIsland();

            _boss = null;
            _islandData = null;

            Debug.Log("[IslandBossController] Island boss defeated!");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
