using UnityEngine;

namespace Ascendant.Progression
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Helm,
        Accessory,
        Relic,
        Mount
    }

    public enum EquipmentRarity
    {
        Common,     // 60%
        Uncommon,   // 25%
        Rare,       // 10%
        Epic,       // 4%
        Legendary,  // 0.9%
        Mythic      // 0.1%
    }

    public enum StatType
    {
        ATK,
        DEF,
        HP,
        SPD,
        CritRate,
        CritDamage,
        DodgeChance,
        LifeSteal
    }

    [System.Serializable]
    public struct StatRoll
    {
        public StatType stat;
        public float value;
    }

    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Ascendant/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Identity")]
        public string equipmentName;
        public string description;
        public Sprite icon;
        public EquipmentSlot slot;
        public EquipmentRarity rarity;

        [Header("Requirements")]
        public int levelRequirement;
        public string classRestriction; // empty = any class

        [Header("Stats")]
        public StatRoll primaryStat;
        public StatRoll[] secondaryStats;

        [Header("Set")]
        public string setId; // empty = no set
        public string setName;

        public static float RarityStatMultiplier(EquipmentRarity r)
        {
            return r switch
            {
                EquipmentRarity.Common => 1.0f,
                EquipmentRarity.Uncommon => 1.3f,
                EquipmentRarity.Rare => 1.7f,
                EquipmentRarity.Epic => 2.5f,
                EquipmentRarity.Legendary => 4.0f,
                EquipmentRarity.Mythic => 6.0f,
                _ => 1.0f
            };
        }

        public static int MaxSecondaryStats(EquipmentRarity r)
        {
            return r switch
            {
                EquipmentRarity.Common => 1,
                EquipmentRarity.Uncommon => 2,
                EquipmentRarity.Rare => 2,
                EquipmentRarity.Epic => 3,
                EquipmentRarity.Legendary => 4,
                EquipmentRarity.Mythic => 4,
                _ => 1
            };
        }

        public static float DropChance(EquipmentRarity r)
        {
            return r switch
            {
                EquipmentRarity.Common => 0.60f,
                EquipmentRarity.Uncommon => 0.25f,
                EquipmentRarity.Rare => 0.10f,
                EquipmentRarity.Epic => 0.04f,
                EquipmentRarity.Legendary => 0.009f,
                EquipmentRarity.Mythic => 0.001f,
                _ => 0.60f
            };
        }
    }
}
