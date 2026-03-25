using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Islands
{
    public class MiniBossController : MonoBehaviour
    {
        public static MiniBossController Instance { get; private set; }

        [SerializeField] EnemySpawner _spawner;

        Enemy _currentMiniBoss;
        BossMechanic _activeMechanic;
        bool _isMiniBossFight;

        public bool IsMiniBossFight => _isMiniBossFight;
        public Enemy CurrentMiniBoss => _currentMiniBoss;
        public BossMechanic ActiveMechanic => _activeMechanic;

        static readonly BossMechanicType[] MechanicPool =
        {
            BossMechanicType.Enrage,
            BossMechanicType.ShieldPhase,
            BossMechanicType.AddSpawning,
            BossMechanicType.GroundSlam,
            BossMechanicType.LifeSteal,
            BossMechanicType.Split,
            BossMechanicType.Reflect
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

        void Update()
        {
            if (!_isMiniBossFight || _activeMechanic == null) return;
            _activeMechanic.Update(Time.deltaTime);

            // Check for add spawning trigger
            if (_activeMechanic is AddSpawningMechanic addMech && addMech.ShouldSpawnAdds())
            {
                SpawnMinions(2);
            }

            // Check for split trigger
            if (_activeMechanic is SplitMechanic splitMech && splitMech.ShouldSplit())
            {
                SpawnSplitCopy();
            }
        }

        public static bool IsMiniBossStage(int stage)
        {
            return stage > 0 && stage % 10 == 0 && stage < 100;
        }

        public void SpawnMiniBoss(EnemyData baseData, int stage, Transform parent)
        {
            var island = IslandManager.Instance?.CurrentIsland;
            float hpMult = island != null ? island.miniBossHpMultiplier : 3f;
            float atkMult = island != null ? island.miniBossAtkMultiplier : 1.5f;

            // Create a runtime mini-boss using the base enemy data with multipliers
            Vector3 pos = new Vector3(2f, 0.5f, 0f);
            var prefab = _spawner != null ? GetEnemyPrefab() : null;
            if (prefab == null)
            {
                Debug.LogWarning("[MiniBossController] No enemy prefab available");
                return;
            }

            var boss = Instantiate(prefab, pos, Quaternion.identity, parent);
            boss.Initialize(baseData, stage);
            // Scale up HP and ATK for mini-boss (applied via damage calculations)
            boss.transform.localScale = Vector3.one * 1.5f; // Visual indication

            EnemyManager.Instance?.Register(boss);
            _currentMiniBoss = boss;
            _isMiniBossFight = true;

            // Assign random mechanic
            var mechanicType = MechanicPool[Random.Range(0, MechanicPool.Length)];
            _activeMechanic = CreateMechanic(mechanicType);
            _activeMechanic.Initialize(boss);
            _activeMechanic.Activate();

            EventBus.Publish(new MiniBossSpawnedEvent
            {
                StageNumber = stage,
                Mechanic = mechanicType
            });

            Debug.Log($"[MiniBossController] Spawned mini-boss with {mechanicType} mechanic at stage {stage}");
        }

        Enemy GetEnemyPrefab()
        {
            // Use reflection or direct reference to get the prefab from EnemySpawner
            // For now, we access via a serialized field
            if (_spawner == null) return null;
            var field = typeof(EnemySpawner).GetField("_enemyPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_spawner) as Enemy;
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

        void SpawnMinions(int count)
        {
            if (_spawner == null) return;
            // Spawn weaker versions of regular enemies as adds
            _spawner.SpawnWave(Mathf.Max(1, _currentMiniBoss != null ? 1 : 1));
            Debug.Log($"[MiniBossController] Spawned {count} minion adds");
        }

        void SpawnSplitCopy()
        {
            if (_currentMiniBoss == null) return;
            var prefab = GetEnemyPrefab();
            if (prefab == null) return;

            // Spawn a weaker copy near the boss
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), 0f);
            var copy = Instantiate(prefab, _currentMiniBoss.transform.position + offset,
                Quaternion.identity, _currentMiniBoss.transform.parent);

            // The split copy would use the same data but is already weakened
            var island = IslandManager.Instance?.CurrentIsland;
            var enemies = island?.enemyTypes;
            if (enemies != null && enemies.Count > 0)
            {
                copy.Initialize(enemies[0], 1);
            }
            copy.transform.localScale = Vector3.one * 1.2f;
            EnemyManager.Instance?.Register(copy);

            Debug.Log("[MiniBossController] Boss split into a copy!");
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_isMiniBossFight || _currentMiniBoss == null) return;
            if (evt.EnemyId == _currentMiniBoss.Id)
            {
                EndMiniBossFight();
            }
        }

        void EndMiniBossFight()
        {
            _isMiniBossFight = false;
            _activeMechanic?.Deactivate();
            _activeMechanic = null;
            _currentMiniBoss = null;
        }

        public float GetMiniBossHpMultiplier()
        {
            var island = IslandManager.Instance?.CurrentIsland;
            return island != null ? island.miniBossHpMultiplier : 3f;
        }

        public float GetMiniBossAtkMultiplier()
        {
            var island = IslandManager.Instance?.CurrentIsland;
            return island != null ? island.miniBossAtkMultiplier : 1.5f;
        }

        public float GetMiniBossGoldMultiplier()
        {
            var island = IslandManager.Instance?.CurrentIsland;
            return island != null ? island.miniBossGoldMultiplier : 2f;
        }

        public float GetMiniBossXpMultiplier()
        {
            var island = IslandManager.Instance?.CurrentIsland;
            return island != null ? island.miniBossXpMultiplier : 2f;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
