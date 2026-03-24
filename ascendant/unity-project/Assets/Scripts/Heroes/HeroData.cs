using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public enum HeroRole
    {
        Vanguard,
        Striker,
        Caster,
        Ranger,
        Support,
        Specialist
    }

    public enum HeroPosition
    {
        Frontline,
        Backline
    }

    public enum StatGrowth
    {
        Low,    // +
        Medium, // ++
        High    // +++
    }

    [CreateAssetMenu(fileName = "NewHero", menuName = "Ascendant/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName;
        public string className;
        public string classId;
        public Sprite portrait;
        public HeroRole role;
        public HeroPosition position;
        public Affinity affinity;

        [Header("Base Stats (Level 1)")]
        public float baseAtk = 15f;
        public float baseDef = 10f;
        public float baseHp = 100f;
        public float baseSpd = 1f; // attacks per second

        [Header("Growth Rates")]
        public StatGrowth atkGrowth = StatGrowth.Medium;
        public StatGrowth defGrowth = StatGrowth.Medium;
        public StatGrowth hpGrowth = StatGrowth.Medium;
        public StatGrowth spdGrowth = StatGrowth.Low;

        [Header("Tap Bonus")]
        public float baseTapBonus = 5f;

        // Growth rate values: Low = 1.5, Medium = 2.5, High = 4.0
        public static float GrowthValue(StatGrowth growth)
        {
            return growth switch
            {
                StatGrowth.Low => 1.5f,
                StatGrowth.Medium => 2.5f,
                StatGrowth.High => 4.0f,
                _ => 2.0f
            };
        }

        public float GetAtk(int level) => baseAtk + GrowthValue(atkGrowth) * (level - 1);
        public float GetDef(int level) => baseDef + GrowthValue(defGrowth) * (level - 1);
        public float GetHp(int level) => baseHp + GrowthValue(hpGrowth) * 5f * (level - 1);
        public float GetSpd(int level) => baseSpd + GrowthValue(spdGrowth) * 0.005f * (level - 1);
    }
}
