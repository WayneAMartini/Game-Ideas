using UnityEngine;

namespace Ascendant.Idle
{
    public enum ExpeditionType
    {
        ResourceGathering,
        ScoutingMission,
        DungeonDelve,
        RareExpedition
    }

    [CreateAssetMenu(fileName = "NewExpedition", menuName = "Ascendant/Expedition Data")]
    public class ExpeditionData : ScriptableObject
    {
        [Header("Identity")]
        public string expeditionId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public ExpeditionType expeditionType;

        [Header("Duration")]
        [Tooltip("Duration in hours")]
        public float durationHours;

        [Header("Requirements")]
        [Tooltip("Recommended hero level for this expedition")]
        public int recommendedHeroLevel = 1;

        [Header("Rewards")]
        public float baseGoldReward;
        public float baseXpReward;
        public int baseMaterialReward;
        [Tooltip("Stardust reward (scouting missions)")]
        public int stardustReward;
        [Tooltip("Class Tokens reward (dungeon delve)")]
        public int classTokenReward;
        [Tooltip("Ascension Shards reward (rare expeditions)")]
        public int ascensionShardReward;

        [Header("Equipment Drops")]
        [Tooltip("Chance (0-1) of getting an equipment drop")]
        [Range(0f, 1f)]
        public float equipmentDropChance;
        [Tooltip("Max rarity tier for equipment drops (0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary)")]
        [Range(0, 4)]
        public int maxEquipmentRarity = 1;

        [Header("Scaling")]
        [Tooltip("Reward multiplier per hero level above recommended")]
        public float levelScalingFactor = 0.02f;

        public float GetDurationSeconds() => durationHours * 3600f;

        public float GetRewardMultiplier(int heroLevel)
        {
            int levelDiff = Mathf.Max(0, heroLevel - recommendedHeroLevel);
            return 1f + levelDiff * levelScalingFactor;
        }
    }
}
