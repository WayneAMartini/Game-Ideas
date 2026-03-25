using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Events
{
    [CreateAssetMenu(fileName = "NewRiftModifier", menuName = "Ascendant/Rift Modifier")]
    public class RiftModifier : ScriptableObject
    {
        public string modifierName;
        [TextArea(2, 4)]
        public string description;

        [Header("Combat Modifiers")]
        public bool abilitiesDisabled;
        public float cooldownMultiplier = 1f;
        public float enemySpeedMultiplier = 1f;
        public bool reducedVisibility;
        public bool enemiesInvisibleUntilAttacked;
        public float playerDamageMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float enemyHpMultiplier = 1f;

        [Header("Visual")]
        public Color overlayColor = new Color(0, 0, 0, 0);
        public string visualEffectId;
    }

    public enum RiftTheme
    {
        VoidOfSilence,
        TemporalRift,
        ShadowRealm,
        ChaosVortex,
        AbyssalDepths
    }
}
