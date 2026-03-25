#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Ascendant.Backend;

namespace Ascendant.Editor
{
    public static class Phase9DataCreator
    {
        [MenuItem("Ascendant/Phase 9/Create Firebase Config")]
        public static void CreateFirebaseConfig()
        {
            var config = ScriptableObject.CreateInstance<FirebaseConfig>();
            config.projectId = "";
            config.apiKey = "";
            config.appId = "";
            config.useCloudSave = true;
            config.useLeaderboards = true;
            config.useGuilds = true;
            config.useArena = true;
            config.useWorldBoss = true;

            string dir = "Assets/Data/Backend";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "Backend");
            }

            AssetDatabase.CreateAsset(config, $"{dir}/FirebaseConfig.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            Debug.Log("[Phase9] Created FirebaseConfig asset");
        }

        [MenuItem("Ascendant/Phase 9/Create World Boss Data Assets")]
        public static void CreateWorldBossDataAssets()
        {
            string dir = "Assets/Data/WorldBosses";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "WorldBosses");
            }

            CreateWorldBoss(dir, "InfernoTitan", "inferno_titan", "Inferno Titan",
                "A colossal titan wreathed in eternal flames.",
                Ascendant.Combat.Affinity.Flame, 100_000_000_000, 5000f, 2000f, new Color(1f, 0.3f, 0f));

            CreateWorldBoss(dir, "FrostLeviathan", "frost_leviathan", "Frost Leviathan",
                "An ancient serpent of living ice that rises from the frozen depths.",
                Ascendant.Combat.Affinity.Frost, 120_000_000_000, 4500f, 2500f, new Color(0.3f, 0.7f, 1f));

            CreateWorldBoss(dir, "StormColossus", "storm_colossus", "Storm Colossus",
                "A towering golem forged from thunderclouds and lightning.",
                Ascendant.Combat.Affinity.Storm, 110_000_000_000, 5500f, 1800f, new Color(0.8f, 0.8f, 0.2f));

            CreateWorldBoss(dir, "ShadowBehemoth", "shadow_behemoth", "Shadow Behemoth",
                "A nightmarish creature born from the void between worlds.",
                Ascendant.Combat.Affinity.Shadow, 90_000_000_000, 6000f, 1500f, new Color(0.4f, 0f, 0.6f));

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase9] Created 4 World Boss data assets");
        }

        static void CreateWorldBoss(string dir, string assetName, string bossId, string bossName,
            string desc, Ascendant.Combat.Affinity affinity, double hp, float atk, float def, Color color)
        {
            var data = ScriptableObject.CreateInstance<WorldBossData>();
            data.bossId = bossId;
            data.bossName = bossName;
            data.description = desc;
            data.affinity = affinity;
            data.globalHpPool = hp;
            data.baseAtk = atk;
            data.baseDef = def;
            data.themeColor = color;

            data.bronzeRewards = new WorldBossRewardTable { gold = 5000, stardust = 50, aetherCrystals = 5, guildCoins = 100 };
            data.silverRewards = new WorldBossRewardTable { gold = 15000, stardust = 150, aetherCrystals = 15, guildCoins = 300, epicEquipmentChance = 0.1f };
            data.goldRewards = new WorldBossRewardTable { gold = 50000, stardust = 500, aetherCrystals = 50, guildCoins = 1000, epicEquipmentChance = 0.3f, legendaryEquipmentChance = 0.05f };
            data.diamondRewards = new WorldBossRewardTable { gold = 200000, stardust = 2000, aetherCrystals = 200, guildCoins = 5000, epicEquipmentChance = 0.5f, legendaryEquipmentChance = 0.15f };

            AssetDatabase.CreateAsset(data, $"{dir}/{assetName}WorldBossData.asset");
        }
    }
}
#endif
