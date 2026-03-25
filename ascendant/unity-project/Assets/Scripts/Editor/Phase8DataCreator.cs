#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ascendant.Economy;
using Ascendant.Core;
using System.Collections.Generic;

namespace Ascendant.Editor
{
    public static class Phase8DataCreator
    {
        [MenuItem("Ascendant/Create Phase 8 Data")]
        public static void CreatePhase8Data()
        {
            CreateBanners();
            CreateBattlePassData();
            CreateQuestData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase8DataCreator] All Phase 8 ScriptableObjects created.");
        }

        static void CreateBanners()
        {
            string path = "Assets/Data/Banners";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "Banners");
            }

            // Standard Banner (always available)
            var standard = ScriptableObject.CreateInstance<BannerData>();
            standard.bannerId = "standard";
            standard.bannerName = "Standard Summon";
            standard.description = "The standard summoning portal. All heroes available.";
            standard.isDefault = true;
            standard.legendaryRateMultiplier = 1f;
            standard.epicRateMultiplier = 1f;
            AssetDatabase.CreateAsset(standard, $"{path}/StandardBanner.asset");

            // Starter Spotlight Banner
            var starter = ScriptableObject.CreateInstance<BannerData>();
            starter.bannerId = "starter_spotlight";
            starter.bannerName = "Starter Spotlight";
            starter.description = "Boosted rates for starter classes!";
            starter.isDefault = true;
            starter.featuredHeroes = new List<string> { "warrior", "mage", "priest", "rogue" };
            starter.featuredRarity = HeroRarity.Rare;
            starter.epicRateMultiplier = 1.5f;
            AssetDatabase.CreateAsset(starter, $"{path}/StarterSpotlightBanner.asset");

            // Tier 2 Featured Banner
            var featured = ScriptableObject.CreateInstance<BannerData>();
            featured.bannerId = "tier2_featured";
            featured.bannerName = "Shadow & Blade";
            featured.description = "Featured: Necromancer & SpellBlade with boosted rates!";
            featured.isDefault = true;
            featured.featuredHeroes = new List<string> { "necromancer", "spellblade" };
            featured.featuredRarity = HeroRarity.Epic;
            featured.legendaryRateMultiplier = 1.5f;
            featured.epicRateMultiplier = 2f;
            AssetDatabase.CreateAsset(featured, $"{path}/ShadowBladeBanner.asset");
        }

        static void CreateBattlePassData()
        {
            string path = "Assets/Data/BattlePass";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "BattlePass");
            }

            var pass = ScriptableObject.CreateInstance<BattlePassData>();
            pass.seasonId = "season_1";
            pass.seasonName = "Season 1: Dawn of Ascendant";
            pass.totalTiers = 50;
            pass.xpPerTier = 1000;

            // Generate 50 tiers of rewards
            pass.freeRewards = new List<BattlePassTierReward>();
            pass.premiumRewards = new List<BattlePassTierReward>();

            for (int i = 0; i < 50; i++)
            {
                // Free track: mostly Gold and materials
                var freeReward = new BattlePassTierReward();
                if (i % 10 == 9) // Every 10th tier
                {
                    freeReward.currencyType = CurrencyType.Stardust;
                    freeReward.amount = 50;
                    freeReward.description = "50 Stardust";
                }
                else if (i % 5 == 4) // Every 5th tier
                {
                    freeReward.currencyType = CurrencyType.ClassTokens;
                    freeReward.amount = 10;
                    freeReward.description = "10 Class Tokens";
                }
                else
                {
                    freeReward.currencyType = CurrencyType.Gold;
                    freeReward.amount = 5000 * (i + 1);
                    freeReward.description = $"{5000 * (i + 1)} Gold";
                }
                pass.freeRewards.Add(freeReward);

                // Premium track: Stardust, skins, hero at tier 50
                var premiumReward = new BattlePassTierReward();
                if (i == 49) // Tier 50
                {
                    premiumReward.itemId = "guaranteed_hero_summon";
                    premiumReward.description = "Guaranteed Hero Summon";
                }
                else if (i % 10 == 9)
                {
                    premiumReward.currencyType = CurrencyType.Stardust;
                    premiumReward.amount = 200;
                    premiumReward.description = "200 Stardust";
                }
                else if (i % 5 == 4)
                {
                    premiumReward.currencyType = CurrencyType.StarFragments;
                    premiumReward.amount = 20;
                    premiumReward.description = "20 Star Fragments";
                }
                else
                {
                    premiumReward.currencyType = CurrencyType.Stardust;
                    premiumReward.amount = 50 + (i * 5);
                    premiumReward.description = $"{50 + (i * 5)} Stardust";
                }
                pass.premiumRewards.Add(premiumReward);
            }

            AssetDatabase.CreateAsset(pass, $"{path}/Season1Pass.asset");
        }

        static void CreateQuestData()
        {
            string path = "Assets/Data/Quests";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "Quests");
            }

            // Daily quests
            CreateQuest(path, "daily_kill_50", "Slay 50 Enemies", QuestType.KillEnemies, 50, false, 50, 1000);
            CreateQuest(path, "daily_kill_100", "Slay 100 Enemies", QuestType.KillEnemies, 100, false, 50, 2000);
            CreateQuest(path, "daily_stages_10", "Clear 10 Stages", QuestType.ClearStages, 10, false, 50, 2000);
            CreateQuest(path, "daily_stages_20", "Clear 20 Stages", QuestType.ClearStages, 20, false, 50, 4000);
            CreateQuest(path, "daily_abilities_20", "Use 20 Abilities", QuestType.UseAbilities, 20, false, 50, 1500);
            CreateQuest(path, "daily_abilities_50", "Use 50 Abilities", QuestType.UseAbilities, 50, false, 50, 3000);
            CreateQuest(path, "daily_gold_10k", "Collect 10,000 Gold", QuestType.CollectGold, 10000, false, 50, 0);
            CreateQuest(path, "daily_gold_50k", "Collect 50,000 Gold", QuestType.CollectGold, 50000, false, 50, 0);
            CreateQuest(path, "daily_expedition_1", "Complete 1 Expedition", QuestType.CompleteExpeditions, 1, false, 50, 3000);

            // Weekly quests
            CreateQuest(path, "weekly_kill_500", "Slay 500 Enemies", QuestType.KillEnemies, 500, true, 200, 10000);
            CreateQuest(path, "weekly_stages_50", "Clear 50 Stages", QuestType.ClearStages, 50, true, 200, 20000);
            CreateQuest(path, "weekly_abilities_100", "Use 100 Abilities", QuestType.UseAbilities, 100, true, 200, 15000);
            CreateQuest(path, "weekly_gold_500k", "Collect 500,000 Gold", QuestType.CollectGold, 500000, true, 200, 0);
            CreateQuest(path, "weekly_expedition_5", "Complete 5 Expeditions", QuestType.CompleteExpeditions, 5, true, 200, 25000);
        }

        static void CreateQuest(string folder, string id, string name, QuestType type, int amount, bool weekly, int bpXP, int gold)
        {
            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.questId = id;
            quest.questName = name;
            quest.description = name;
            quest.questType = type;
            quest.requiredAmount = amount;
            quest.isWeekly = weekly;
            quest.battlePassXP = bpXP;
            quest.goldReward = gold;
            AssetDatabase.CreateAsset(quest, $"{folder}/{id}.asset");
        }
    }
}
#endif
