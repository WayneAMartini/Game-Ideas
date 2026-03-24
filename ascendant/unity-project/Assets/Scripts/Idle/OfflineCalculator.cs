using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Idle
{
    [Serializable]
    public struct AFKRewards
    {
        public double Gold;
        public double Xp;
        public int MaterialDrops;
        public List<EquipmentDrop> EquipmentDrops;
        public double OfflineHours;
        public bool ReturnBonus;
    }

    [Serializable]
    public struct EquipmentDrop
    {
        public string Rarity; // "Common" or "Uncommon"
        public int ItemSeed;  // deterministic seed for generating the item later
    }

    public static class OfflineCalculator
    {
        public const float MaxOfflineHours = 10f;
        public const float IdleEfficiency = 0.5f; // 50% combat efficiency
        public const float EquipmentChancePerHour = 0.05f; // 5% per hour
        public const float ReturnBonusWindowHours = 6f;
        public const float ReturnBonusMultiplier = 1.25f;

        // Base rates per stage (gold per hour at 100% efficiency)
        // Scales with stage: baseGoldRate * stageMultiplier
        const float BaseGoldPerHour = 100f;
        const float BaseXpPerHour = 50f;
        const float BaseMaterialsPerHour = 2f;

        public static AFKRewards Calculate(
            long lastTimestampUnix,
            long currentTimestampUnix,
            int stageLevel,
            float partyPower,
            long lastCollectionTimestampUnix = 0)
        {
            var rewards = new AFKRewards
            {
                EquipmentDrops = new List<EquipmentDrop>()
            };

            double elapsedSeconds = currentTimestampUnix - lastTimestampUnix;
            if (elapsedSeconds <= 0) return rewards;

            double elapsedHours = elapsedSeconds / 3600.0;
            double cappedHours = Math.Min(elapsedHours, MaxOfflineHours);
            rewards.OfflineHours = cappedHours;

            // Stage multiplier: scales rewards with progression
            float stageMultiplier = 1f + 0.06f * (stageLevel - 1); // matches ProgressionConfig.goldScalePerStage

            // Party power provides a minor bonus (normalized around 100 base power)
            float powerMultiplier = Mathf.Max(1f, partyPower / 100f);

            // Check return-within-6-hours bonus
            bool returnBonus = false;
            if (lastCollectionTimestampUnix > 0)
            {
                double hoursSinceLastCollection = (currentTimestampUnix - lastCollectionTimestampUnix) / 3600.0;
                returnBonus = hoursSinceLastCollection <= ReturnBonusWindowHours;
            }
            rewards.ReturnBonus = returnBonus;
            float bonusMult = returnBonus ? ReturnBonusMultiplier : 1f;

            // Gold: StageGoldRate * 0.5 (idle efficiency) * hours * bonuses
            rewards.Gold = BaseGoldPerHour * stageMultiplier * IdleEfficiency * cappedHours * powerMultiplier * bonusMult;

            // XP: similar formula, distributed to active party on collection
            rewards.Xp = BaseXpPerHour * stageMultiplier * IdleEfficiency * cappedHours * powerMultiplier * bonusMult;

            // Materials: scale with time and stage
            rewards.MaterialDrops = Mathf.FloorToInt((float)(BaseMaterialsPerHour * cappedHours * stageMultiplier * IdleEfficiency));

            // Equipment drops: 5% chance per hour, common/uncommon only
            // Use deterministic seeding so results are reproducible if needed
            int seed = (int)(lastTimestampUnix % int.MaxValue);
            var rng = new System.Random(seed);
            int fullHours = Mathf.FloorToInt((float)cappedHours);
            for (int i = 0; i < fullHours; i++)
            {
                double roll = rng.NextDouble();
                if (roll < EquipmentChancePerHour)
                {
                    rewards.EquipmentDrops.Add(new EquipmentDrop
                    {
                        Rarity = rng.NextDouble() < 0.7 ? "Common" : "Uncommon",
                        ItemSeed = rng.Next()
                    });
                }
            }

            return rewards;
        }
    }
}
