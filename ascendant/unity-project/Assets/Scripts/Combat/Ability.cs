using UnityEngine;

namespace Ascendant.Combat
{
    public enum AbilityTargetType
    {
        SingleEnemy,
        AllEnemies,
        FrontlineEnemies,
        PartyBuff,
        Self
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "Ascendant/Ability")]
    public class Ability : ScriptableObject
    {
        [Header("Identity")]
        public string abilityName;
        public string description;
        public Sprite icon;
        public int slotIndex; // 0 = Ability 1, 1 = Ability 2, 2 = Ultimate

        [Header("Mechanics")]
        public float damageMultiplier = 2f; // multiplier of hero ATK
        public float cooldown = 8f;
        public AbilityTargetType targetType;
        public bool isUltimate;

        [Header("Buff/Debuff")]
        public float buffMultiplier; // e.g., 0.15 for +15% ATK
        public float buffDuration;
        public float stunDuration;

        [Header("Ultimate")]
        public float chargeRequired = 100f; // how much charge needed
    }
}
