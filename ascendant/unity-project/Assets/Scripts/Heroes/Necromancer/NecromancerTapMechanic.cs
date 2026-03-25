using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class NecromancerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 2;
        [SerializeField] float _raisedMinionChance = 0.40f;
        [SerializeField] float _minionAtkPercent = 0.30f;
        [SerializeField] float _minionHpPercent = 0.25f;
        [SerializeField] float _minionAttackInterval = 2.0f;
        [SerializeField] float _minionDecayDuration = 60f;
        [SerializeField] float _damagePerMinion = 0.03f;
        [SerializeField] float _damageReductionPerMinion = 0.02f;

        [Header("State")]
        [SerializeField] int _minionCount = 0;

        // Per-minion state arrays (max 5)
        float[] _minionHp;
        float[] _minionMaxHp;
        float[] _minionAttackTimer;
        float[] _minionDecayTimer;
        bool[] _minionAlive;

        // Expose properties
        public int MinionCount => _minionCount;
        public int MaxMinions => 5;
        public float ArmyDamageBonus => _minionCount * _damagePerMinion;
        public float ArmyDamageReduction => _minionCount * _damageReductionPerMinion;

        void Awake()
        {
            _minionHp = new float[MaxMinions];
            _minionMaxHp = new float[MaxMinions];
            _minionAttackTimer = new float[MaxMinions];
            _minionDecayTimer = new float[MaxMinions];
            _minionAlive = new bool[MaxMinions];
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
            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null || !hero.IsAlive) return;

            float minionAtk = hero.CurrentAtk * _minionAtkPercent;

            for (int i = 0; i < MaxMinions; i++)
            {
                if (!_minionAlive[i]) continue;

                // Decay timer
                _minionDecayTimer[i] += Time.deltaTime;
                if (_minionDecayTimer[i] >= _minionDecayDuration)
                {
                    DismissMinion(i);
                    continue;
                }

                // Auto-attack
                _minionAttackTimer[i] += Time.deltaTime;
                if (_minionAttackTimer[i] >= _minionAttackInterval)
                {
                    _minionAttackTimer[i] = 0f;
                    MinionAttack(i, minionAtk);
                }
            }
        }

        void MinionAttack(int index, float minionAtk)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(transform.position);
            if (enemy == null || enemy.IsDead) return;

            float dmg = minionAtk * (1f + ArmyDamageBonus);
            enemy.TakeDamage(dmg, DamageType.Magical);

            // Reset decay on combat participation
            _minionDecayTimer[index] = 0f;

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = dmg,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });
        }

        void OnEnemyKilled(EnemyKilledEvent e)
        {
            TryRaiseMinion(e.WorldPosition);
        }

        void TryRaiseMinion(Vector3 spawnPosition)
        {
            if (_minionCount >= MaxMinions) return;

            if (Random.value > _raisedMinionChance) return;

            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null || !hero.IsAlive) return;

            // Find a free slot
            int slot = -1;
            for (int i = 0; i < MaxMinions; i++)
            {
                if (!_minionAlive[i])
                {
                    slot = i;
                    break;
                }
            }
            if (slot < 0) return;

            float maxHp = hero.MaxHp * _minionHpPercent;
            _minionHp[slot] = maxHp;
            _minionMaxHp[slot] = maxHp;
            _minionAttackTimer[slot] = 0f;
            _minionDecayTimer[slot] = 0f;
            _minionAlive[slot] = true;
            _minionCount++;

            EventBus.Publish(new MinionRaisedEvent
            {
                HeroSlot = _heroSlot,
                MinionCount = _minionCount,
                MaxMinions = MaxMinions
            });
        }

        void DismissMinion(int index)
        {
            if (!_minionAlive[index]) return;
            _minionAlive[index] = false;
            _minionCount = Mathf.Max(0, _minionCount - 1);

            EventBus.Publish(new MinionDiedEvent
            {
                HeroSlot = _heroSlot,
                MinionCount = _minionCount
            });
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float outDamage = damage * (1f + ArmyDamageBonus);
            enemy.TakeDamage(outDamage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = outDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });
        }

        public void Reset()
        {
            for (int i = 0; i < MaxMinions; i++)
            {
                _minionAlive[i] = false;
                _minionAttackTimer[i] = 0f;
                _minionDecayTimer[i] = 0f;
            }
            _minionCount = 0;
        }
    }
}
