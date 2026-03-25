using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Islands
{
    public enum BiomeEffectType
    {
        None,
        DamageOverTime,     // Fire damage, poison, etc.
        HealingModifier,    // Increase/decrease healing
        SpeedModifier,      // Increase/decrease attack speed
        AccuracyDebuff,     // Reduce accuracy for non-matching heroes
        DamageReflect,      // Reflect portion of damage
        PeriodicRooting,    // Randomly root heroes
        RandomAoE,          // Random lightning/AoE damage
        HpRegen,            // HP regeneration bonus
        ShieldGrant,        // Grant shields to matching heroes
        EnemyFireShield,    // Enemies gain fire shields
        DamageBuff          // Flat damage bonus for matching heroes
    }

    [CreateAssetMenu(fileName = "NewBiome", menuName = "Ascendant/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        [Header("Identity")]
        public string biomeName;
        public Affinity biomeAffinity;
        [TextArea(2, 4)]
        public string effectDescription;

        [Header("Effect")]
        public BiomeEffectType effectType;

        [Header("Effect Values")]
        [Tooltip("Damage/heal amount per tick (% of max HP for DoT, flat for others)")]
        public float effectValue = 0.01f;
        [Tooltip("How often the effect triggers (seconds)")]
        public float tickInterval = 5f;
        [Tooltip("Modifier value (speed %, accuracy %, etc.)")]
        public float modifierValue = 0f;

        [Header("Immunity")]
        [Tooltip("Heroes with this affinity are immune to the negative effect")]
        public Affinity immuneAffinity;

        [Header("Visuals")]
        public Color ambientColor = Color.white;
        public Color particleColor = Color.white;
    }
}
