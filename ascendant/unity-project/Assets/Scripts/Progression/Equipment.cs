using System;
using UnityEngine;

namespace Ascendant.Progression
{
    [Serializable]
    public class Equipment
    {
        public string id;
        public string equipmentName;
        public EquipmentSlot slot;
        public EquipmentRarity rarity;
        public int levelRequirement;
        public string classRestriction;
        public string setId;
        public string setName;

        public StatRoll primaryStat;
        public StatRoll[] secondaryStats;

        public int enhanceLevel; // +0 to +15
        public bool persistent = true; // survives Ascension

        // Assigned hero slot (-1 = unequipped, in inventory)
        public int equippedHeroSlot = -1;

        public static Equipment Generate(int stage, EquipmentRarity rarity)
        {
            var eq = new Equipment
            {
                id = Guid.NewGuid().ToString("N")[..12],
                rarity = rarity,
                enhanceLevel = 0,
                persistent = true,
                equippedHeroSlot = -1
            };

            // Random slot
            var slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
            eq.slot = slots[UnityEngine.Random.Range(0, slots.Length)];

            // Primary stat based on slot
            eq.primaryStat = RollPrimaryStat(eq.slot, stage, rarity);

            // Secondary stats based on rarity
            int numSecondary = EquipmentData.MaxSecondaryStats(rarity);
            eq.secondaryStats = new StatRoll[numSecondary];
            for (int i = 0; i < numSecondary; i++)
                eq.secondaryStats[i] = RollSecondaryStat(stage, rarity);

            eq.equipmentName = GenerateName(eq.slot, rarity);
            eq.levelRequirement = Mathf.Max(1, stage / 2);

            return eq;
        }

        static StatRoll RollPrimaryStat(EquipmentSlot slot, int stage, EquipmentRarity rarity)
        {
            float mult = EquipmentData.RarityStatMultiplier(rarity);
            float baseValue = 5f + stage * 0.5f;

            StatType type = slot switch
            {
                EquipmentSlot.Weapon => StatType.ATK,
                EquipmentSlot.Armor => StatType.DEF,
                EquipmentSlot.Helm => StatType.HP,
                EquipmentSlot.Accessory => StatType.CritRate,
                EquipmentSlot.Relic => StatType.ATK,
                EquipmentSlot.Mount => StatType.SPD,
                _ => StatType.ATK
            };

            float value = type switch
            {
                StatType.HP => baseValue * 10f * mult,
                StatType.CritRate => Mathf.Min(0.5f, (0.02f + stage * 0.001f) * mult),
                StatType.SPD => (0.01f + stage * 0.0005f) * mult,
                _ => baseValue * mult
            };

            return new StatRoll { stat = type, value = value };
        }

        static StatRoll RollSecondaryStat(int stage, EquipmentRarity rarity)
        {
            float mult = EquipmentData.RarityStatMultiplier(rarity) * 0.5f;
            float baseValue = 2f + stage * 0.3f;

            var allStats = (StatType[])Enum.GetValues(typeof(StatType));
            var type = allStats[UnityEngine.Random.Range(0, allStats.Length)];

            float value = type switch
            {
                StatType.HP => baseValue * 8f * mult,
                StatType.CritRate => Mathf.Min(0.3f, (0.01f + stage * 0.0005f) * mult),
                StatType.CritDamage => (0.05f + stage * 0.002f) * mult,
                StatType.DodgeChance => Mathf.Min(0.2f, (0.01f + stage * 0.0003f) * mult),
                StatType.LifeSteal => Mathf.Min(0.15f, (0.005f + stage * 0.0002f) * mult),
                StatType.SPD => (0.005f + stage * 0.0003f) * mult,
                _ => baseValue * mult
            };

            return new StatRoll { stat = type, value = value };
        }

        static string GenerateName(EquipmentSlot slot, EquipmentRarity rarity)
        {
            string prefix = rarity switch
            {
                EquipmentRarity.Common => "Worn",
                EquipmentRarity.Uncommon => "Sturdy",
                EquipmentRarity.Rare => "Fine",
                EquipmentRarity.Epic => "Heroic",
                EquipmentRarity.Legendary => "Legendary",
                EquipmentRarity.Mythic => "Mythic",
                _ => ""
            };

            string typeName = slot switch
            {
                EquipmentSlot.Weapon => "Blade",
                EquipmentSlot.Armor => "Plate",
                EquipmentSlot.Helm => "Helm",
                EquipmentSlot.Accessory => "Ring",
                EquipmentSlot.Relic => "Relic",
                EquipmentSlot.Mount => "Steed",
                _ => "Item"
            };

            return $"{prefix} {typeName}";
        }

        public float GetTotalStat(StatType stat)
        {
            float total = 0f;
            if (primaryStat.stat == stat)
                total += primaryStat.value;

            if (secondaryStats != null)
            {
                for (int i = 0; i < secondaryStats.Length; i++)
                    if (secondaryStats[i].stat == stat)
                        total += secondaryStats[i].value;
            }

            // Enhancement bonus: +5% per enhance level
            total *= (1f + enhanceLevel * 0.05f);

            return total;
        }

        public float GetPowerScore()
        {
            float power = 0f;
            power += primaryStat.value;
            if (secondaryStats != null)
                for (int i = 0; i < secondaryStats.Length; i++)
                    power += secondaryStats[i].value;
            power *= (1f + enhanceLevel * 0.05f);
            power *= EquipmentData.RarityStatMultiplier(rarity);
            return power;
        }
    }
}
