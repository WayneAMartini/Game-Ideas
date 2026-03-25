using UnityEngine;
using Ascendant.Heroes;

namespace Ascendant.Progression
{
    [CreateAssetMenu(fileName = "NewClassGrowthRates", menuName = "Ascendant/Class Growth Rates")]
    public class ClassGrowthRates : ScriptableObject
    {
        public string classId;

        [Header("Growth Tiers: Low=1.5, Medium=2.5, High=4.0")]
        public StatGrowth atkGrowth = StatGrowth.Medium;
        public StatGrowth defGrowth = StatGrowth.Medium;
        public StatGrowth hpGrowth = StatGrowth.Medium;
        public StatGrowth spdGrowth = StatGrowth.Low;

        public float AtkPerLevel => HeroData.GrowthValue(atkGrowth);
        public float DefPerLevel => HeroData.GrowthValue(defGrowth);
        public float HpPerLevel => HeroData.GrowthValue(hpGrowth) * 5f;
        public float SpdPerLevel => HeroData.GrowthValue(spdGrowth) * 0.005f;
    }
}
