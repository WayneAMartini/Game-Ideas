using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class ReaperTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Harvest Config")]
        [SerializeField] float _executeThreshold = 0.3f;
        [SerializeField] float _executeMultiplier = 3f;

        [Header("Soul Collection Passive")]
        [SerializeField] int _maxSouls = 20;
        [SerializeField] float _soulDamageBonusPerSoul = 0.02f;
        [SerializeField] float _soulAbilityBonusPerSoul = 0.05f;

        [Header("Hero Slot")]
        [SerializeField] int _heroSlot = 0;

        // Runtime state
        int _souls = 0;

        // ---- Public API ----

        public int Souls => _souls;
        public int MaxSouls => _maxSouls;
        public float ExecuteThreshold => _executeThreshold;

        /// <summary>+2% all damage per soul (0 to +40%).</summary>
        public float SoulDamageBonus => _souls * _soulDamageBonusPerSoul;

        /// <summary>+5% ability damage per soul (0 to +100%).</summary>
        public float SoulAbilityBonus => _souls * _soulAbilityBonusPerSoul;

        /// <summary>
        /// Attempt to spend souls. Returns true and deducts if enough are available.
        /// </summary>
        public bool TrySpendSouls(int amount)
        {
            if (_souls < amount) return false;
            _souls -= amount;

            EventBus.Publish(new SoulCollectedEvent
            {
                HeroSlot = _heroSlot,
                SoulCount = _souls,
                MaxSouls = _maxSouls
            });

            return true;
        }

        // ---- Unity lifecycle ----

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float finalDamage = damage;
            bool isExecute = false;

            // Apply soul damage bonus
            finalDamage *= (1f + SoulDamageBonus);

            // Execute: triple damage vs enemies below 30% HP
            float hpPercent = enemy.MaxHp > 0f ? (enemy.CurrentHp / enemy.MaxHp) : 0f;
            if (hpPercent < _executeThreshold)
            {
                finalDamage *= _executeMultiplier;
                isExecute = true;
            }

            enemy.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = isExecute,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            if (isExecute)
            {
                EventBus.Publish(new ExecuteTriggeredEvent
                {
                    HeroSlot = _heroSlot,
                    EnemyId = enemy.Id,
                    Damage = finalDamage
                });
            }
        }

        public void Reset()
        {
            _souls = 0;
        }

        // ---- Event Handlers ----

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (_souls >= _maxSouls) return;

            _souls++;

            EventBus.Publish(new SoulCollectedEvent
            {
                HeroSlot = _heroSlot,
                SoulCount = _souls,
                MaxSouls = _maxSouls
            });
        }
    }
}
