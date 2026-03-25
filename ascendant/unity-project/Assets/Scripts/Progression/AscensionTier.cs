using System.Collections.Generic;

namespace Ascendant.Progression
{
    public enum AscensionTierLevel
    {
        Mortal = 0,     // 0 ascensions
        Awakened = 1,   // 1st ascension
        Exalted = 2,    // 3rd ascension
        Mythic = 3,     // 5th ascension
        Demigod = 4     // 10th ascension
    }

    public static class AscensionTierData
    {
        public struct TierInfo
        {
            public AscensionTierLevel Tier;
            public int AscensionsRequired;
            public float StatBonusPercent;
            public string DisplayName;
            public string Description;
        }

        static readonly TierInfo[] Tiers =
        {
            new TierInfo
            {
                Tier = AscensionTierLevel.Mortal,
                AscensionsRequired = 0,
                StatBonusPercent = 0f,
                DisplayName = "Mortal",
                Description = "Starting tier"
            },
            new TierInfo
            {
                Tier = AscensionTierLevel.Awakened,
                AscensionsRequired = 1,
                StatBonusPercent = 15f,
                DisplayName = "Awakened",
                Description = "Advanced skill branch access"
            },
            new TierInfo
            {
                Tier = AscensionTierLevel.Exalted,
                AscensionsRequired = 3,
                StatBonusPercent = 35f,
                DisplayName = "Exalted",
                Description = "Mythic gear evolution eligibility"
            },
            new TierInfo
            {
                Tier = AscensionTierLevel.Mythic,
                AscensionsRequired = 5,
                StatBonusPercent = 60f,
                DisplayName = "Mythic",
                Description = "Ultimate ability enhancement"
            },
            new TierInfo
            {
                Tier = AscensionTierLevel.Demigod,
                AscensionsRequired = 10,
                StatBonusPercent = 100f,
                DisplayName = "Demigod",
                Description = "Transcendence trial access"
            }
        };

        public static TierInfo GetTierInfo(AscensionTierLevel tier)
        {
            return Tiers[(int)tier];
        }

        public static AscensionTierLevel GetTierForAscensions(int ascensionCount)
        {
            if (ascensionCount >= 10) return AscensionTierLevel.Demigod;
            if (ascensionCount >= 5) return AscensionTierLevel.Mythic;
            if (ascensionCount >= 3) return AscensionTierLevel.Exalted;
            if (ascensionCount >= 1) return AscensionTierLevel.Awakened;
            return AscensionTierLevel.Mortal;
        }

        public static float GetStatBonusPercent(int ascensionCount)
        {
            var tier = GetTierForAscensions(ascensionCount);
            return Tiers[(int)tier].StatBonusPercent;
        }

        public static int GetAscensionsForNextTier(AscensionTierLevel currentTier)
        {
            int next = (int)currentTier + 1;
            if (next >= Tiers.Length) return -1; // already max
            return Tiers[next].AscensionsRequired;
        }
    }
}
