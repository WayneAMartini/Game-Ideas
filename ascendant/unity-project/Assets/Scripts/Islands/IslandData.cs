using System.Collections.Generic;
using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Islands
{
    [CreateAssetMenu(fileName = "NewIsland", menuName = "Ascendant/Island Data")]
    public class IslandData : ScriptableObject
    {
        [Header("Identity")]
        public string islandName;
        public int islandNumber;
        public int realmNumber = 1;
        [TextArea(2, 4)]
        public string description;

        [Header("Biome")]
        public Affinity affinity;
        public BiomeData biomeData;

        [Header("Stages")]
        public int stageCount = 100;

        [Header("Enemies")]
        public List<EnemyData> enemyTypes;

        [Header("Boss")]
        public string islandBossName;
        [TextArea(2, 4)]
        public string islandBossDescription;
        public float islandBossHpMultiplier = 10f;
        public float islandBossAtkMultiplier = 3f;
        public List<BossPhaseData> bossPhases;

        [Header("Mini-Boss")]
        public float miniBossHpMultiplier = 3f;
        public float miniBossAtkMultiplier = 1.5f;
        public float miniBossGoldMultiplier = 2f;
        public float miniBossXpMultiplier = 2f;

        [Header("Unlock")]
        public IslandData previousIsland;

        [Header("Rewards")]
        public float goldMultiplier = 1f;
        public float xpMultiplier = 1f;
    }
}
