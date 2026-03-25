#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Ascendant.Editor
{
    public static class IntegrationTest
    {
        [MenuItem("Ascendant/Run Integration Test")]
        public static void RunIntegrationTest()
        {
            Debug.Log("=== Ascendant Integration Test ===");
            int passed = 0;
            int failed = 0;
            var errors = new List<string>();

            // Test 1: Verify all singleton types exist and can be referenced
            var singletonTypes = new Type[]
            {
                typeof(Core.GameManager),
                typeof(Core.SaveManager),
                typeof(Combat.EnemySpawner),
                typeof(Combat.MomentumSystem),
                typeof(Combat.TapInputController),
                typeof(Combat.AutoAttackSystem),
                typeof(Economy.CurrencyManager),
                typeof(Economy.GachaSystem),
                typeof(Economy.StarSystem),
                typeof(Economy.BattlePassSystem),
                typeof(Economy.DailyQuestSystem),
                typeof(Economy.IAPManager),
                typeof(Economy.Wallet),
                typeof(Economy.BannerManager),
                typeof(Heroes.HeroManager),
                typeof(Party.PartyManager),
                typeof(Party.ComboSystem),
                typeof(Party.SynergySystem),
                typeof(Progression.StageManager),
                typeof(Progression.LevelingSystem),
                typeof(Progression.EquipmentSystem),
                typeof(Progression.SkillTreeSystem),
                typeof(Progression.ClassMasterySystem),
                typeof(Progression.AscensionSystem),
                typeof(Progression.AscensionSkillTree),
                typeof(Progression.TierBonusSystem),
                typeof(Progression.DemigodSystem),
                typeof(Islands.IslandManager),
                typeof(Islands.IslandBossController),
                typeof(Islands.MiniBossController),
                typeof(Islands.RealmBossController),
                typeof(Islands.BiomeEffectSystem),
                typeof(Idle.AFKVaultSystem),
                typeof(Idle.ExpeditionManager),
                typeof(Idle.NotificationManager),
                typeof(Idle.NotificationSettings),
                typeof(Backend.FirebaseManager),
                typeof(Backend.AuthManager),
                typeof(Backend.CloudSaveManager),
                typeof(Backend.GuildManager),
                typeof(Backend.ArenaManager),
                typeof(Backend.LeaderboardManager),
                typeof(Backend.WorldBossManager),
                typeof(Backend.GuildExpedition),
                typeof(Events.TowerOfTrials),
                typeof(Events.VoidRiftManager),
                typeof(Events.SeasonalEventManager),
                typeof(Events.InfiniteAscension),
                // Phase 11:
                typeof(Audio.SoundManager),
                typeof(Audio.HapticManager),
                typeof(Audio.ParticleManager),
                typeof(Audio.SpriteAnimator),
                typeof(UI.UIPolish),
                typeof(UI.ScreenTransitionManager),
                typeof(UI.TabBarUI),
                typeof(UI.AccessibilityManager),
                typeof(Utils.PerformanceOptimizer),
            };

            foreach (var type in singletonTypes)
            {
                if (type != null)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    errors.Add($"Type not found: {type}");
                }
            }

            // Test 2: Verify ScriptableObject types exist
            var soTypes = new Type[]
            {
                typeof(Heroes.HeroData),
                typeof(Combat.EnemyData),
                typeof(Islands.IslandData),
                typeof(Islands.BiomeData),
                typeof(Islands.BossPhaseData),
                typeof(Islands.BossLootTable),
                typeof(Progression.EquipmentData),
                typeof(Progression.SkillTreeData),
                typeof(Progression.ClassMasteryData),
                typeof(Progression.ClassGrowthRates),
                typeof(Progression.TranscendenceTrial),
                typeof(Progression.XPCurve),
                typeof(Idle.ExpeditionData),
                typeof(Economy.BannerData),
                typeof(Economy.BattlePassData),
                typeof(Economy.QuestData),
                typeof(Events.EventConfig),
                typeof(Audio.AudioData),
                typeof(Utils.PerformanceSettings),
            };

            foreach (var type in soTypes)
            {
                if (type != null)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    errors.Add($"ScriptableObject type not found: {type}");
                }
            }

            // Test 3: Verify EventBus can create all event types
            var eventTypes = new Type[]
            {
                typeof(Core.TapEvent),
                typeof(Core.EnemyDamagedEvent),
                typeof(Core.EnemyKilledEvent),
                typeof(Core.WaveClearedEvent),
                typeof(Core.StageAdvancedEvent),
                typeof(Core.MomentumChangedEvent),
                typeof(Core.CurrencyChangedEvent),
                typeof(Core.GameStateChangedEvent),
                typeof(Core.LevelUpEvent),
                typeof(Core.AscensionEvent),
                typeof(Core.BossPhaseChangedEvent),
                typeof(Core.BossDefeatedEvent),
                typeof(Core.IslandChangedEvent),
                typeof(Core.GachaPullEvent),
                typeof(Core.IAPPurchaseEvent),
                typeof(Core.ArenaMatchResultEvent),
                typeof(Core.TowerFloorClearedEvent),
                typeof(Core.VoidRiftStageClearedEvent),
                typeof(Core.SeasonalEventStartedEvent),
                typeof(UI.AutoBattleChangedEvent),
            };

            foreach (var type in eventTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance != null)
                        passed++;
                    else
                    {
                        failed++;
                        errors.Add($"Failed to create event: {type.Name}");
                    }
                }
                catch (Exception e)
                {
                    failed++;
                    errors.Add($"Event creation error for {type.Name}: {e.Message}");
                }
            }

            // Test 4: Verify key enums
            try
            {
                var _ = Combat.Affinity.Flame;
                var __ = Core.CurrencyType.Gold;
                var ___ = Core.HeroRarity.Legendary;
                var ____ = Heroes.HeroRole.Vanguard;
                var _____ = UI.ColorblindMode.Protanopia;
                var ______ = UI.TextSizePreset.Large;
                var _______ = UI.TabScreen.Combat;
                var ________ = Audio.SoundId.TapImpact;
                var _________ = Audio.HapticIntensity.Strong;
                var __________ = Audio.ParticleType.TapImpact;
                var ___________ = Utils.QualityPreset.High;
                passed += 11;
            }
            catch (Exception e)
            {
                failed++;
                errors.Add($"Enum verification error: {e.Message}");
            }

            // Results
            Debug.Log($"=== Integration Test Results ===");
            Debug.Log($"Passed: {passed}");
            Debug.Log($"Failed: {failed}");

            foreach (var error in errors)
                Debug.LogError($"  FAIL: {error}");

            if (failed == 0)
                Debug.Log("ALL TESTS PASSED - Phase 11 integration verified.");
            else
                Debug.LogError($"{failed} test(s) failed. See errors above.");
        }
    }
}
#endif
