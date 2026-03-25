using UnityEngine;

namespace Ascendant.Progression
{
    public enum MasteryTier
    {
        Novice,      // 0 stages
        Apprentice,  // 100 stages
        Journeyman,  // 500 stages
        Expert,      // 2000 stages
        Master,      // 5000 stages
        Grandmaster  // 10000 stages
    }

    [System.Serializable]
    public class MasteryTierReward
    {
        public MasteryTier tier;
        public int stageThreshold;
        public float statBonusPercent; // permanent stat bonus
        public bool unlocksAlternateAbility;
        public bool unlocksCosmeticSkin;
        public bool unlocksPassiveAbility;
        [TextArea] public string rewardDescription;
    }

    [CreateAssetMenu(fileName = "NewClassMastery", menuName = "Ascendant/Class Mastery Data")]
    public class ClassMasteryData : ScriptableObject
    {
        [Header("Identity")]
        public string classId;
        public string className;

        [Header("Tier Rewards")]
        public MasteryTierReward[] tierRewards = new MasteryTierReward[]
        {
            new() { tier = MasteryTier.Novice, stageThreshold = 0, statBonusPercent = 0f, rewardDescription = "Class unlocked" },
            new() { tier = MasteryTier.Apprentice, stageThreshold = 100, statBonusPercent = 5f, rewardDescription = "+5% class-specific stat" },
            new() { tier = MasteryTier.Journeyman, stageThreshold = 500, statBonusPercent = 8f, unlocksAlternateAbility = true, rewardDescription = "Alternate ability variant unlock" },
            new() { tier = MasteryTier.Expert, stageThreshold = 2000, statBonusPercent = 10f, unlocksCosmeticSkin = true, rewardDescription = "Class skin unlock + 10% class stat" },
            new() { tier = MasteryTier.Master, stageThreshold = 5000, statBonusPercent = 15f, unlocksPassiveAbility = true, rewardDescription = "Mastery passive ability" },
            new() { tier = MasteryTier.Grandmaster, stageThreshold = 10000, statBonusPercent = 20f, unlocksCosmeticSkin = true, rewardDescription = "Ultimate class cosmetic + title" }
        };

        public MasteryTier GetTierForStages(int stagesCleared)
        {
            MasteryTier result = MasteryTier.Novice;
            for (int i = 0; i < tierRewards.Length; i++)
            {
                if (stagesCleared >= tierRewards[i].stageThreshold)
                    result = tierRewards[i].tier;
            }
            return result;
        }

        public MasteryTierReward GetReward(MasteryTier tier)
        {
            for (int i = 0; i < tierRewards.Length; i++)
                if (tierRewards[i].tier == tier)
                    return tierRewards[i];
            return null;
        }

        public int GetNextTierThreshold(int stagesCleared)
        {
            for (int i = 0; i < tierRewards.Length; i++)
                if (stagesCleared < tierRewards[i].stageThreshold)
                    return tierRewards[i].stageThreshold;
            return tierRewards[tierRewards.Length - 1].stageThreshold;
        }

        public float GetTotalStatBonus(int stagesCleared)
        {
            float total = 0f;
            for (int i = 0; i < tierRewards.Length; i++)
                if (stagesCleared >= tierRewards[i].stageThreshold)
                    total += tierRewards[i].statBonusPercent;
            return total;
        }
    }
}
