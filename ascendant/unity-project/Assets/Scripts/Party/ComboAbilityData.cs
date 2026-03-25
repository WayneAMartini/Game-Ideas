using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Party
{
    public enum ComboTargetType
    {
        AllEnemies,
        SingleEnemy,
        PartyBuff,
        Hybrid
    }

    [CreateAssetMenu(fileName = "NewComboAbility", menuName = "Ascendant/Combo Ability")]
    public class ComboAbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string comboId;
        public string comboName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Hero Pair")]
        public string classIdA;
        public string classIdB;

        [Header("Mechanics")]
        public ComboTargetType targetType;
        public float damageMultiplier = 3f;
        public DamageType damageType = DamageType.Physical;
        public float healPercent;
        public float buffMultiplier;
        public float buffDuration;
        public float goldMultiplier;

        [Header("Timing")]
        public float windowDuration = 3f; // abilities must be used within this window
    }
}
