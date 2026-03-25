using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Defender Tier 2 tap mechanic.
    /// Shield Bash: deals 60% of normal damage and applies Weakened debuff (-3% defense per stack, max 15).
    /// Passive - Bulwark: absorbs 20% of damage dealt to adjacent party members.
    /// </summary>
    public class DefenderTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Shield Bash Config")]
        [SerializeField] float _damageMultiplierValue = 0.6f;
        [SerializeField] float _weakenedDefenseReductionPerStack = 0.03f;
        [SerializeField] int _weakenedMaxStacks = 15;

        [Header("Bulwark Config")]
        [SerializeField] float _bulwarkAbsorptionPercent = 0.20f;

        // Weakened stacks per enemy ID
        readonly Dictionary<int, int> _weakenedStacks = new Dictionary<int, int>();

        // Public interface
        public float DamageMultiplier => _damageMultiplierValue;
        public float BulwarkAbsorptionPercent => _bulwarkAbsorptionPercent;

        /// <summary>Returns the current Weakened debuff stacks on the given enemy.</summary>
        public int GetWeakenedStacks(int enemyId)
        {
            return _weakenedStacks.TryGetValue(enemyId, out int stacks) ? stacks : 0;
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var target = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

            // Apply Weakened stack
            if (!_weakenedStacks.ContainsKey(target.Id))
                _weakenedStacks[target.Id] = 0;

            _weakenedStacks[target.Id] = Mathf.Min(
                _weakenedStacks[target.Id] + 1,
                _weakenedMaxStacks
            );

            // Damage is reduced by Shield Bash multiplier but benefits from Weakened debuff on target
            int currentStacks = _weakenedStacks[target.Id];
            float defenseReduction = currentStacks * _weakenedDefenseReductionPerStack; // e.g. 0.45 at max
            float effectiveDamage = damage * _damageMultiplierValue * (1f + defenseReduction);

            target.TakeDamage(effectiveDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = effectiveDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });

            if (target.IsDead)
                _weakenedStacks.Remove(target.Id);
        }

        public void Reset()
        {
            _weakenedStacks.Clear();
        }
    }
}
