using UnityEngine;

namespace Ascendant.Combat
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Ascendant/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName;
        public Sprite sprite;
        public Affinity affinity;
        public EnemyCategory category;
        public EnemyAttackType attackType = EnemyAttackType.Melee;

        [Header("Base Stats (Stage 1)")]
        public float baseHp = 50f;
        public float baseAtk = 5f;
        public float baseDef = 2f;

        [Header("Rewards (Stage 1)")]
        public float baseGoldDrop = 10f;
        public float baseXpDrop = 5f;

        [Header("Scaling")]
        [Tooltip("HP multiplier per stage")]
        public float hpScalePerStage = 0.08f;
        [Tooltip("ATK multiplier per stage")]
        public float atkScalePerStage = 0.05f;
        [Tooltip("DEF multiplier per stage")]
        public float defScalePerStage = 0.03f;
        [Tooltip("Gold multiplier per stage")]
        public float goldScalePerStage = 0.06f;
        [Tooltip("XP multiplier per stage")]
        public float xpScalePerStage = 0.04f;

        public float GetHp(int stage)
        {
            return baseHp * (1f + hpScalePerStage * (stage - 1));
        }

        public float GetAtk(int stage)
        {
            return baseAtk * (1f + atkScalePerStage * (stage - 1));
        }

        public float GetDef(int stage)
        {
            return baseDef * (1f + defScalePerStage * (stage - 1));
        }

        public float GetGoldDrop(int stage)
        {
            return baseGoldDrop * (1f + goldScalePerStage * (stage - 1));
        }

        public float GetXpDrop(int stage)
        {
            return baseXpDrop * (1f + xpScalePerStage * (stage - 1));
        }
    }
}
