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

        // Full Phase 2 damage pipeline:
        // RawDamage * TypeMultiplier * AffinityBonus * (1 - DefenseReduction)
        public static float CalculateFullDamage(float rawDamage, DamageType damageType,
            EnemyCategory enemyCategory, Affinity attackerAffinity, Affinity defenderAffinity,
            float targetDef)
        {
            float typeMultiplier = DamageTypeHelper.GetTypeMultiplier(damageType, enemyCategory);
            float affinityBonus = AffinityHelper.GetMultiplier(attackerAffinity, defenderAffinity);
            float defenseReduction = 1f - (targetDef / (targetDef + 100f));

            return Mathf.Max(1f, rawDamage * typeMultiplier * affinityBonus * defenseReduction);
        }

        // Auto-attack with full pipeline
        public static float CalculateAutoAttackFull(float heroAtk, DamageType damageType,
            Affinity attackerAffinity, Enemy target, float positionBonus = 0f)
        {
            float rawDamage = heroAtk * (1f + positionBonus);
            return CalculateFullDamage(rawDamage, damageType, target.Category,
                attackerAffinity, target.Affinity, target.Def);
        }

        // Ability with full pipeline
        public static float CalculateAbilityFull(float heroAtk, float abilityMultiplier,
            DamageType damageType, Affinity attackerAffinity, Enemy target,
            float positionBonus = 0f)
        {
            float rawDamage = heroAtk * abilityMultiplier * (1f + positionBonus);
            return CalculateFullDamage(rawDamage, damageType, target.Category,
                attackerAffinity, target.Affinity, target.Def);
        }
    }
}
