using UnityEngine;

namespace Ascendant.Progression
{
    public enum AscensionBranch
    {
        Power,      // damage bonuses
        Fortitude,  // HP/DEF bonuses
        Prosperity, // gold/XP/drop rate bonuses
        Swiftness   // SPD/cooldown bonuses
    }

    [System.Serializable]
    public class AscensionStatBonus
    {
        public StatType stat;
        public float percentValue; // e.g. 5 = +5%
    }

    [CreateAssetMenu(fileName = "NewAscensionNode", menuName = "Ascendant/Ascension Node")]
    public class AscensionNode : ScriptableObject
    {
        [Header("Identity")]
        public string nodeId;
        public string nodeName;
        [TextArea] public string description;
        public AscensionBranch branch;
        public int tier; // 0-9 within branch (10 nodes per branch)

        [Header("Cost")]
        public double shardCost = 50;

        [Header("Effects")]
        public AscensionStatBonus[] statBonuses;

        [Header("Special Effects")]
        public float goldBonusPercent;
        public float xpBonusPercent;
        public float dropRateBonusPercent;
        public float cooldownReductionPercent;
        public float stageClearSpeedBonusPercent;

        [Header("Prerequisites")]
        public string[] prerequisiteNodeIds;
    }
}
