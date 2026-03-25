using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Marksman Tier 2 tap mechanic.
    /// Aimed Shot: slow taps charge for higher damage (100%-400%, cap at 2s gap).
    /// Fully charged shots (>1.8s gap) are guaranteed crits (2x damage).
    /// Passive - Steady Hand: consecutive hits on the same target gain +5% per hit, max 10 stacks.
    /// </summary>
    public class MarkmanTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Aimed Shot Config")]
        [SerializeField] float _quickTapThreshold = 0.5f;
        [SerializeField] float _maxChargeTime = 2.0f;
        [SerializeField] float _critChargeThreshold = 1.8f;
        [SerializeField] float _maxDamageMultiplier = 4.0f;

        [Header("Steady Hand Config")]
        [SerializeField] float _steadyHandBonusPerStack = 0.05f;
        [SerializeField] int _steadyHandMaxStacks = 10;

        // Aimed Shot state
        float _lastTapTime = -999f;

        // Steady Hand state
        int _lastTargetId = -1;
        int _steadyHandStacks = 0;

        // Public properties for UI
        public float ChargePercent
        {
            get
            {
                float gap = Time.time - _lastTapTime;
                if (gap <= _quickTapThreshold) return 0f;
                return Mathf.Clamp01((gap - _quickTapThreshold) / (_maxChargeTime - _quickTapThreshold));
            }
        }

        public int SteadyHandStacks => _steadyHandStacks;

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            float now = Time.time;
            float gapSinceLastTap = now - _lastTapTime;
            _lastTapTime = now;

            // Resolve target for Steady Hand tracking
            var target = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

            // Steady Hand: track consecutive hits on same target
            if (target.Id == _lastTargetId)
            {
                _steadyHandStacks = Mathf.Min(_steadyHandStacks + 1, _steadyHandMaxStacks);
            }
            else
            {
                _steadyHandStacks = 0;
                _lastTargetId = target.Id;
            }

            float steadyHandBonus = 1f + (_steadyHandStacks * _steadyHandBonusPerStack);

            // Aimed Shot damage calculation
            float damageMultiplier;
            bool isCrit = false;

            if (gapSinceLastTap < _quickTapThreshold)
            {
                // Quick tap: base damage
                damageMultiplier = 1f;
            }
            else
            {
                // Charged shot: scale from 1x to maxDamageMultiplier based on gap
                float chargeRatio = Mathf.Clamp01(
                    (gapSinceLastTap - _quickTapThreshold) / (_maxChargeTime - _quickTapThreshold)
                );
                damageMultiplier = Mathf.Lerp(1f, _maxDamageMultiplier, chargeRatio);

                // Fully charged = guaranteed crit (2x on top of charge multiplier)
                if (gapSinceLastTap >= _critChargeThreshold)
                {
                    isCrit = true;
                    damageMultiplier *= 2f;
                }
            }

            float finalDamage = damage * damageMultiplier * steadyHandBonus;
            target.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = finalDamage,
                IsCritical = isCrit,
                IsAoE = false,
                WorldPosition = target.transform.position
            });

            if (target.IsDead)
            {
                _steadyHandStacks = 0;
                _lastTargetId = -1;
            }
        }

        public void Reset()
        {
            _lastTapTime = -999f;
            _lastTargetId = -1;
            _steadyHandStacks = 0;
        }
    }
}
