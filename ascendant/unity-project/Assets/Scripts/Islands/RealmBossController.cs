using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Islands
{
    public class RealmBossController : MonoBehaviour
    {
        public static RealmBossController Instance { get; private set; }

        [SerializeField] EnemySpawner _spawner;

        Enemy _boss;
        int _currentPhaseIndex;
        List<BossMechanic> _activeMechanics = new();
        bool _isBossFight;
        float _bossMaxHp;
        float _phaseTransitionCooldown;

        // Phase 1: Individual Tests
        int _testingHeroIndex;
        float _testTimer;
        bool _inIndividualTest;

        // Phase 4: Mercy Choice
        bool _mercyChoicePresented;

        public bool IsBossFight => _isBossFight;
        public bool IsIndividualTest => _inIndividualTest;
        public int TestingHeroIndex => _testingHeroIndex;

        // The Radiant Guardian - 4 Phase Boss
        static readonly BossPhaseData[] RadiantGuardianPhases = new[]
        {
            new BossPhaseData
            {
                phaseName = "Judgment",
                hpThreshold = 1f,
                description = "Tests each hero individually for 15 seconds.",
                atkMultiplier = 1f,
                attackSpeedMultiplier = 1f,
                mechanics = new[] { BossMechanicType.IndividualTest }
            },
            new BossPhaseData
            {
                phaseName = "Wrath",
                hpThreshold = 0.75f,
                description = "Full party fight. Attacks with all six affinities in rotation.",
                atkMultiplier = 1.5f,
                attackSpeedMultiplier = 1.2f,
                mechanics = new[] { BossMechanicType.AffinityRotation, BossMechanicType.GroundSlam }
            },
            new BossPhaseData
            {
                phaseName = "Transcendence",
                hpThreshold = 0.4f,
                description = "Environment becomes the boss. Columns of light and energy beams.",
                atkMultiplier = 2f,
                attackSpeedMultiplier = 1.5f,
                mechanics = new[] { BossMechanicType.EnvironmentBoss, BossMechanicType.AddSpawning }
            },
            new BossPhaseData
            {
                phaseName = "Ascension",
                hpThreshold = 0.1f,
                description = "At 10% HP, offers mercy or destruction choice.",
                atkMultiplier = 3f,
                attackSpeedMultiplier = 2f,
                mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.MercyChoice }
            }
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
            EventBus.Subscribe<RealmBossUnlockedEvent>(OnRealmBossUnlocked);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<RealmBossUnlockedEvent>(OnRealmBossUnlocked);
        }

        void OnRealmBossUnlocked(RealmBossUnlockedEvent evt)
        {
            Debug.Log($"[RealmBossController] Realm {evt.RealmNumber} boss unlocked: The Radiant Guardian!");
        }

        void Update()
        {
            if (!_isBossFight || _boss == null || _boss.IsDead) return;

            if (_phaseTransitionCooldown > 0f)
            {
                _phaseTransitionCooldown -= Time.deltaTime;
                return;
            }

            // Handle Phase 1 individual tests
            if (_inIndividualTest)
            {
                UpdateIndividualTest();
                return;
            }

            // Update mechanics
            foreach (var mechanic in _activeMechanics)
            {
                mechanic.Update(Time.deltaTime);
            }

            // Check phase transitions
            CheckPhaseTransition();
        }

        public void StartRealmBoss(Transform parent)
        {
            var prefab = GetEnemyPrefab();
            if (prefab == null)
            {
                Debug.LogError("[RealmBossController] No enemy prefab available");
                return;
            }

            Vector3 pos = new Vector3(2f, 0.5f, 0f);
            _boss = Instantiate(prefab, pos, Quaternion.identity, parent);

            // Use first available enemy data, scaled massively
            var island = IslandManager.Instance?.CurrentIsland;
            var enemies = island?.enemyTypes;
            EnemyData bossData = enemies != null && enemies.Count > 0 ? enemies[0] : null;
            if (bossData == null)
            {
                Debug.LogError("[RealmBossController] No enemy data for realm boss");
                Destroy(_boss.gameObject);
                return;
            }

            _boss.Initialize(bossData, 100);
            _boss.transform.localScale = Vector3.one * 3f; // Realm Boss is massive

            _bossMaxHp = _boss.MaxHp * 20f; // Realm Boss has 20x HP
            EnemyManager.Instance?.Register(_boss);

            _isBossFight = true;
            _currentPhaseIndex = 0;
            _mercyChoicePresented = false;

            ActivatePhase(0);

            EventBus.Publish(new IslandBossSpawnedEvent
            {
                IslandIndex = 11,
                BossName = "The Radiant Guardian"
            });

            Debug.Log("[RealmBossController] The Radiant Guardian appears!");
        }

        void ActivatePhase(int phaseIndex)
        {
            foreach (var mechanic in _activeMechanics)
                mechanic.Deactivate();
            _activeMechanics.Clear();

            _currentPhaseIndex = phaseIndex;
            if (phaseIndex >= RadiantGuardianPhases.Length) return;

            var phase = RadiantGuardianPhases[phaseIndex];

            // Phase 1: Start individual hero tests
            if (phaseIndex == 0)
            {
                StartIndividualTests();
            }
            else
            {
                // Create mechanics
                foreach (var mechType in phase.mechanics)
                {
                    if (mechType == BossMechanicType.IndividualTest) continue;
                    if (mechType == BossMechanicType.AffinityRotation) continue; // Handled separately
                    if (mechType == BossMechanicType.EnvironmentBoss) continue;  // Handled separately
                    if (mechType == BossMechanicType.MercyChoice) continue;      // Handled at phase trigger

                    var mechanic = CreateMechanic(mechType);
                    mechanic.Initialize(_boss);
                    mechanic.Activate();
                    _activeMechanics.Add(mechanic);
                }
            }

            EventBus.Publish(new BossPhaseChangedEvent
            {
                PhaseIndex = phaseIndex,
                PhaseName = phase.phaseName,
                HpThreshold = phase.hpThreshold
            });

            _phaseTransitionCooldown = 2f;

            Debug.Log($"[RealmBossController] Phase {phaseIndex + 1}: {phase.phaseName}");
        }

        void StartIndividualTests()
        {
            _inIndividualTest = true;
            _testingHeroIndex = 0;
            _testTimer = 15f;
            Debug.Log("[RealmBossController] Phase 1 — Judgment: Testing heroes individually");
        }

        void UpdateIndividualTest()
        {
            _testTimer -= Time.deltaTime;
            if (_testTimer <= 0f)
            {
                _testingHeroIndex++;
                if (_testingHeroIndex >= 4)
                {
                    _inIndividualTest = false;
                    // All tests passed, move to Phase 2
                    ActivatePhase(1);
                }
                else
                {
                    _testTimer = 15f;
                    Debug.Log($"[RealmBossController] Testing hero slot {_testingHeroIndex}");
                }
            }
        }

        void CheckPhaseTransition()
        {
            float hpPercent = _boss.CurrentHp / _bossMaxHp;

            int nextPhase = _currentPhaseIndex + 1;
            if (nextPhase < RadiantGuardianPhases.Length)
            {
                if (hpPercent <= RadiantGuardianPhases[nextPhase].hpThreshold)
                {
                    // Phase 4 mercy choice
                    if (nextPhase == 3 && !_mercyChoicePresented)
                    {
                        _mercyChoicePresented = true;
                        Debug.Log("[RealmBossController] The Guardian offers a choice: Mercy or Destruction?");
                    }

                    ActivatePhase(nextPhase);
                }
            }
        }

        BossMechanic CreateMechanic(BossMechanicType type)
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
                EndRealmBoss();
            }
        }

        void EndRealmBoss()
        {
            foreach (var mechanic in _activeMechanics)
                mechanic.Deactivate();
            _activeMechanics.Clear();

            _isBossFight = false;
            _inIndividualTest = false;

            EventBus.Publish(new BossDefeatedEvent
            {
                BossName = "The Radiant Guardian",
                IsIslandBoss = false,
                IsRealmBoss = true
            });

            EventBus.Publish(new RealmBossDefeatedEvent { RealmNumber = 1 });

            _boss = null;
            Debug.Log("[RealmBossController] The Radiant Guardian has been defeated! Ascension unlocked!");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
