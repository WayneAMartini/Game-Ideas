using System.Collections.Generic;
using UnityEngine;
using Ascendant.Progression;

namespace Ascendant.Islands
{
    [CreateAssetMenu(fileName = "NewBossLoot", menuName = "Ascendant/Boss Loot Table")]
    public class BossLootTable : ScriptableObject
    {
        [Header("Guaranteed Drops")]
        public float goldReward = 1000f;
        public float xpReward = 500f;
        public EquipmentRarity guaranteedMinRarity = EquipmentRarity.Rare;

        [Header("Bonus Drop Chances")]
        [Range(0f, 1f)] public float epicDropChance = 0.2f;
        [Range(0f, 1f)] public float legendaryDropChance = 0.05f;

        [Header("Boss-Specific Drops")]
        public List<EquipmentData> possibleDrops;

        public EquipmentRarity RollRarity()
        {
            float roll = Random.value;
            if (roll < legendaryDropChance)
                return EquipmentRarity.Legendary;
            if (roll < legendaryDropChance + epicDropChance)
                return EquipmentRarity.Epic;
            return guaranteedMinRarity;
        }
    }
}
