using UnityEngine;

namespace Ascendant.Progression
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Ascendant/Progression Config")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Stage Scaling")]
        [Tooltip("Enemy HP multiplier per stage (compound)")]
        public float enemyHpScalePerStage = 0.08f;
        [Tooltip("Enemy ATK multiplier per stage")]
        public float enemyAtkScalePerStage = 0.05f;
        [Tooltip("Gold reward multiplier per stage")]
        public float goldScalePerStage = 0.06f;
        [Tooltip("XP reward multiplier per stage")]
        public float xpScalePerStage = 0.04f;

        [Header("Wave Config")]
        public int minEnemiesPerWave = 3;
        public int maxEnemiesPerWave = 5;

        [Header("Island Config")]
        public int stagesPerIsland = 100;
        public int miniBossEveryNStages = 10;

        [Header("Transition")]
        public float stageTransitionDelay = 1f;

        public float GetEnemyHpMultiplier(int stage)
        {
            return 1f + enemyHpScalePerStage * (stage - 1);
        }

        public float GetEnemyAtkMultiplier(int stage)
        {
            return 1f + enemyAtkScalePerStage * (stage - 1);
        }

        public float GetGoldMultiplier(int stage)
        {
            return 1f + goldScalePerStage * (stage - 1);
        }

        public float GetXpMultiplier(int stage)
        {
            return 1f + xpScalePerStage * (stage - 1);
        }
    }
}
