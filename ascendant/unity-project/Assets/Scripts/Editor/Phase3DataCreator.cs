#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Ascendant.Idle;

namespace Ascendant.Editor
{
    public static class Phase3DataCreator
    {
        [MenuItem("Ascendant/Create Phase 3 Data")]
        public static void CreateAll()
        {
            CreateExpeditionData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Phase 3 ScriptableObject data created successfully!");
        }

        [MenuItem("Ascendant/Create Phase 3 Expedition Data")]
        static void CreateExpeditionData()
        {
            // --- Resource Gathering (2h) ---
            var resource = ScriptableObject.CreateInstance<ExpeditionData>();
            resource.expeditionId = "resource_gathering";
            resource.displayName = "Resource Gathering";
            resource.description = "Send a hero to gather gold and crafting materials from the surrounding islands.";
            resource.expeditionType = ExpeditionType.ResourceGathering;
            resource.durationHours = 2f;
            resource.recommendedHeroLevel = 1;
            resource.baseGoldReward = 5000f;
            resource.baseXpReward = 1000f;
            resource.baseMaterialReward = 10;
            resource.stardustReward = 0;
            resource.classTokenReward = 0;
            resource.ascensionShardReward = 0;
            resource.equipmentDropChance = 0.05f;
            resource.maxEquipmentRarity = 0; // Common
            resource.levelScalingFactor = 0.02f;
            CreateAsset(resource, "Assets/Data/Expeditions/ResourceGathering.asset");

            // --- Scouting Mission (4h) ---
            var scouting = ScriptableObject.CreateInstance<ExpeditionData>();
            scouting.expeditionId = "scouting_mission";
            scouting.displayName = "Scouting Mission";
            scouting.description = "Scout ahead to gather intel on undiscovered islands and collect Stardust.";
            scouting.expeditionType = ExpeditionType.ScoutingMission;
            scouting.durationHours = 4f;
            scouting.recommendedHeroLevel = 10;
            scouting.baseGoldReward = 2000f;
            scouting.baseXpReward = 2000f;
            scouting.baseMaterialReward = 5;
            scouting.stardustReward = 50;
            scouting.classTokenReward = 0;
            scouting.ascensionShardReward = 0;
            scouting.equipmentDropChance = 0.10f;
            scouting.maxEquipmentRarity = 1; // Uncommon
            scouting.levelScalingFactor = 0.025f;
            CreateAsset(scouting, "Assets/Data/Expeditions/ScoutingMission.asset");

            // --- Dungeon Delve (8h) ---
            var dungeon = ScriptableObject.CreateInstance<ExpeditionData>();
            dungeon.expeditionId = "dungeon_delve";
            dungeon.displayName = "Dungeon Delve";
            dungeon.description = "Explore a dangerous dungeon for equipment and Class Tokens.";
            dungeon.expeditionType = ExpeditionType.DungeonDelve;
            dungeon.durationHours = 8f;
            dungeon.recommendedHeroLevel = 25;
            dungeon.baseGoldReward = 8000f;
            dungeon.baseXpReward = 5000f;
            dungeon.baseMaterialReward = 15;
            dungeon.stardustReward = 0;
            dungeon.classTokenReward = 5;
            dungeon.ascensionShardReward = 0;
            dungeon.equipmentDropChance = 0.30f;
            dungeon.maxEquipmentRarity = 2; // Rare
            dungeon.levelScalingFactor = 0.03f;
            CreateAsset(dungeon, "Assets/Data/Expeditions/DungeonDelve.asset");

            // --- Rare Expedition (12h) ---
            var rare = ScriptableObject.CreateInstance<ExpeditionData>();
            rare.expeditionId = "rare_expedition";
            rare.displayName = "Rare Expedition";
            rare.description = "A perilous journey to the edge of the sky islands. Legendary materials and Ascension Shards await.";
            rare.expeditionType = ExpeditionType.RareExpedition;
            rare.durationHours = 12f;
            rare.recommendedHeroLevel = 50;
            rare.baseGoldReward = 15000f;
            rare.baseXpReward = 10000f;
            rare.baseMaterialReward = 25;
            rare.stardustReward = 100;
            rare.classTokenReward = 3;
            rare.ascensionShardReward = 10;
            rare.equipmentDropChance = 0.50f;
            rare.maxEquipmentRarity = 4; // Legendary
            rare.levelScalingFactor = 0.04f;
            CreateAsset(rare, "Assets/Data/Expeditions/RareExpedition.asset");

            Debug.Log("Expedition data created: Resource Gathering, Scouting Mission, Dungeon Delve, Rare Expedition");
        }

        static void CreateAsset(Object obj, string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string[] parts = dir.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    currentPath = nextPath;
                }
            }

            AssetDatabase.CreateAsset(obj, path);
        }
    }
}
#endif
