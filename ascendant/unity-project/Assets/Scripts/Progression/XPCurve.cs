using UnityEngine;

namespace Ascendant.Progression
{
    [CreateAssetMenu(fileName = "XPCurve", menuName = "Ascendant/XP Curve")]
    public class XPCurve : ScriptableObject
    {
        [Header("XP Formula: BaseXP * level^Exponent")]
        public float baseXP = 100f;
        public float exponent = 1.5f;
        public int levelCap = 100;

        [Header("Milestone Levels")]
        public int[] milestoneLevels = { 10, 25, 50, 75, 100 };

        public float GetXpForLevel(int level)
        {
            if (level <= 1) return 0f;
            return baseXP * Mathf.Pow(level, exponent);
        }

        public bool IsMilestoneLevel(int level)
        {
            for (int i = 0; i < milestoneLevels.Length; i++)
                if (milestoneLevels[i] == level) return true;
            return false;
        }
    }
}
