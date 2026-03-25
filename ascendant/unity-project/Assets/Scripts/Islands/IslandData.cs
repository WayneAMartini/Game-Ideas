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
        [Tooltip("Secondary affinity for dual-affinity islands (Realm 2+)")]
        public Affinity secondaryAffinity = Affinity.None;
        public BiomeData biomeData;

        public bool IsDualAffinity => secondaryAffinity != Affinity.None && secondaryAffinity != affinity;

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
        [Tooltip("Number of simultaneous mechanics on mini-bosses (1 for Realm 1, 2 for Realm 2, 3 for Realm 3)")]
        public int miniBossMechanicCount = 1;

        [Header("Unlock")]
        public IslandData previousIsland;

        [Header("Rewards")]
        public float goldMultiplier = 1f;
        public float xpMultiplier = 1f;
    }
}
