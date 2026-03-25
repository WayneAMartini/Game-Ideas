using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Thief Tier 2 tap mechanic.
    /// Pickpocket: each tap has a 20% chance to steal bonus gold (publishes GoldStolenEvent).
    /// Passive - Lucky Break: +30% gold from all sources, +10% equipment drop rate.
    /// </summary>
    public class ThiefTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Pickpocket Config")]
        [SerializeField] float _stealChance = 0.20f;
        [SerializeField] float _baseGoldStealAmount = 5f;
        [SerializeField] int _heroSlot = 0;

        [Header("Lucky Break Config")]
        [SerializeField] float _goldBonusMultiplierValue = 1.3f;
        [SerializeField] float _dropRateBonusValue = 0.1f;

        // Public properties for other systems
        /// <summary>Multiplier applied to all gold pickups for this hero (Lucky Break passive).</summary>
        public float GoldBonusMultiplier => _goldBonusMultiplierValue;

        /// <summary>Flat bonus to equipment drop rate from Lucky Break passive.</summary>
        public float DropRateBonus => _dropRateBonusValue;

        /// <summary>The hero party slot this Thief occupies.</summary>
        public int HeroSlot => _heroSlot;

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var target = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

            // Normal attack damage
            target.TakeDamage(damage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });

            // Pickpocket: roll for bonus gold steal
            if (Random.value <= _stealChance)
            {
                float stolenGold = _baseGoldStealAmount * _goldBonusMultiplierValue;

                EventBus.Publish(new GoldStolenEvent
                {
                    HeroSlot = _heroSlot,
                    Amount = stolenGold
                });
            }
        }

        public void Reset()
        {
            // No persistent per-tap state to clear for Thief
        }
    }
}
