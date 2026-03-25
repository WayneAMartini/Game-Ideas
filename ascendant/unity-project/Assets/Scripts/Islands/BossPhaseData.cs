using UnityEngine;

namespace Ascendant.Islands
{
    [System.Serializable]
    public class BossPhaseData
    {
        public string phaseName;
        [Tooltip("HP threshold to trigger this phase (1.0 = 100%, 0.6 = 60%)")]
        public float hpThreshold = 1f;
        [TextArea(2, 4)]
        public string description;
        public float atkMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public BossMechanicType[] mechanics;
    }

    public enum BossMechanicType
    {
        Enrage,
        ShieldPhase,
        AddSpawning,
        GroundSlam,
        LifeSteal,
        Split,
        Reflect,
        FireBreath,
        BurningGround,
        FireballDeflect,
        AffinityRotation,
        EnvironmentBoss,
        MercyChoice,
        IndividualTest
    }
}
