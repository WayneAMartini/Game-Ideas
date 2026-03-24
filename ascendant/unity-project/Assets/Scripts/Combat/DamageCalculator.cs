using UnityEngine;

namespace Ascendant.Combat
{
    public static class DamageCalculator
    {
        // Tap damage = (BaseTapPower + HeroTapBonus) * MomentumMultiplier * AffinityBonus
        public static float CalculateTapDamage(float baseTapPower, float heroTapBonus,
            float momentumMultiplier, float affinityBonus = 1f)
        {
            return (baseTapPower + heroTapBonus) * momentumMultiplier * affinityBonus;
        }

        // Auto-attack damage = Hero ATK (separate from tap damage)
        public static float CalculateAutoAttackDamage(float heroAtk, float targetDef)
        {
            float defense = 1f - (targetDef / (targetDef + 100f));
            return Mathf.Max(1f, heroAtk * defense);
        }

        // Ability damage = multiplier * heroAtk
        public static float CalculateAbilityDamage(float heroAtk, float abilityMultiplier,
            float targetDef, float affinityBonus = 1f)
        {
            float defense = 1f - (targetDef / (targetDef + 100f));
            return Mathf.Max(1f, heroAtk * abilityMultiplier * defense * affinityBonus);
        }
    }
}
