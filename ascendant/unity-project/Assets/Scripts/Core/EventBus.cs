using System;
using System.Collections.Generic;

namespace Ascendant.Core
{
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = result;
            }
        }

        public static void Publish<T>(T evt)
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler)?.Invoke(evt);
        }

        public static void Clear()
        {
            _handlers.Clear();
        }
    }

    // --- Game Events ---

    public struct EnemyDamagedEvent
    {
        public int EnemyId;
        public float Damage;
        public bool IsCritical;
        public bool IsAoE;
        public UnityEngine.Vector3 WorldPosition;
    }

    public struct EnemyKilledEvent
    {
        public int EnemyId;
        public float GoldReward;
        public float XpReward;
        public UnityEngine.Vector3 WorldPosition;
    }

    public struct WaveClearedEvent
    {
        public int StageNumber;
    }

    public struct StageAdvancedEvent
    {
        public int NewStage;
        public int Island;
    }

    public struct TapEvent
    {
        public UnityEngine.Vector3 WorldPosition;
        public float Damage;
        public int TapCount;
    }

    public struct MomentumChangedEvent
    {
        public int Stacks;
        public float Multiplier;
    }

    public struct CurrencyChangedEvent
    {
        public CurrencyType Type;
        public double Amount;
        public double Delta;
    }

    public enum CurrencyType
    {
        Gold,
        XP,
        Stardust,
        AscensionShards,
        AetherCrystals,
        ClassTokens,
        GuildCoins,
        StarFragments
    }

    public struct HeroDamagedEvent
    {
        public int HeroSlot;
        public float Damage;
        public float CurrentHp;
        public float MaxHp;
    }

    public struct HeroHealedEvent
    {
        public int HeroSlot;
        public float Amount;
        public float CurrentHp;
        public float MaxHp;
    }

    public struct AbilityUsedEvent
    {
        public int HeroSlot;
        public int AbilitySlot;
        public string AbilityName;
    }

    public struct UltimateChargeChangedEvent
    {
        public int HeroSlot;
        public float ChargePercent;
    }

    public struct GameStateChangedEvent
    {
        public GameState OldState;
        public GameState NewState;
    }

    public struct ShockwaveTriggeredEvent
    {
        public float Damage;
        public UnityEngine.Vector3 Origin;
    }

    public struct AutoAttackEvent
    {
        public int HeroSlot;
        public int EnemyId;
        public float Damage;
    }

    public struct PartyChangedEvent { }

    public struct ComboPointsChangedEvent
    {
        public int ComboPoints;
        public int MaxComboPoints;
    }

    public struct FinishingBlowEvent
    {
        public float Damage;
        public UnityEngine.Vector3 WorldPosition;
    }

    public struct HeroDeathEvent
    {
        public int HeroSlot;
    }

    public struct HeroRevivedEvent
    {
        public int HeroSlot;
        public float CurrentHp;
        public float MaxHp;
    }

    // --- AFK / Idle Events ---

    public struct AFKVaultReadyEvent
    {
        public Idle.AFKRewards Rewards;
    }

    public struct AFKVaultCollectedEvent
    {
        public Idle.AFKRewards Rewards;
    }

    // --- Expedition Events ---

    public struct ExpeditionStartedEvent
    {
        public int SlotIndex;
        public string ExpeditionId;
    }

    public struct ExpeditionCompletedEvent
    {
        public int SlotIndex;
        public string ExpeditionId;
    }

    public struct ExpeditionCollectedEvent
    {
        public int SlotIndex;
    }

    // --- Phase 4: Progression Events ---

    public struct LevelUpEvent
    {
        public int HeroSlot;
        public int NewLevel;
        public string ClassId;
        public bool IsMilestone;
    }

    public struct EquipmentChangedEvent
    {
        public int HeroSlot;
        public Progression.EquipmentSlot Slot;
    }

    public struct EquipmentEnhancedEvent
    {
        public string EquipmentId;
        public int NewEnhanceLevel;
    }

    public struct EquipmentDropEvent
    {
        public Progression.EquipmentRarity Rarity;
        public string EquipmentName;
    }

    public struct SkillTreeChangedEvent
    {
        public int HeroSlot;
        public string ClassId;
        public string BranchId;
    }

    public struct ClassMasteryTierUpEvent
    {
        public string ClassId;
        public Progression.MasteryTier NewTier;
    }

    public struct SkillPointsChangedEvent
    {
        public int HeroSlot;
        public int Available;
        public int Spent;
    }

    // --- Phase 7: Resource System Events ---

    public struct ResourceChangedEvent
    {
        public int HeroSlot;
        public string ResourceName;
        public float Current;
        public float Max;
    }

    // --- Phase 7: Pet/Companion Events ---

    public struct PetDamagedEvent
    {
        public int OwnerHeroSlot;
        public float Damage;
        public float CurrentHp;
        public float MaxHp;
    }

    public struct PetDiedEvent
    {
        public int OwnerHeroSlot;
    }

    public struct PetRevivedEvent
    {
        public int OwnerHeroSlot;
    }

    public struct PetAttackEvent
    {
        public int OwnerHeroSlot;
        public int EnemyId;
        public float Damage;
    }

    // --- Phase 7: Totem Events ---

    public struct TotemPlacedEvent
    {
        public int HeroSlot;
        public string TotemType;
        public UnityEngine.Vector3 Position;
    }

    public struct TotemExpiredEvent
    {
        public int HeroSlot;
        public string TotemType;
    }

    // --- Phase 7: Minion Events ---

    public struct MinionRaisedEvent
    {
        public int HeroSlot;
        public int MinionCount;
        public int MaxMinions;
    }

    public struct MinionDiedEvent
    {
        public int HeroSlot;
        public int MinionCount;
    }

    // --- Phase 7: Song/Aura Events ---

    public struct SongChangedEvent
    {
        public int HeroSlot;
        public string SongName;
    }

    public struct CrescendoTriggeredEvent
    {
        public int HeroSlot;
        public string SongName;
    }

    // --- Phase 7: Familiar Events ---

    public struct FamiliarSwappedEvent
    {
        public int HeroSlot;
        public string FamiliarName;
    }

    // --- Phase 7: Potion Events ---

    public struct PotionThrownEvent
    {
        public int HeroSlot;
        public string PotionType;
    }

    // --- Phase 7: Ammo Events ---

    public struct AmmoChangedEvent
    {
        public int HeroSlot;
        public int CurrentAmmo;
        public int MaxAmmo;
        public bool IsReloading;
    }

    // --- Phase 7: Synergy Events ---

    public struct SynergyActivatedEvent
    {
        public string SynergyName;
        public string Description;
    }

    public struct ComboAbilityTriggeredEvent
    {
        public string ComboName;
        public int HeroSlotA;
        public int HeroSlotB;
        public float Damage;
    }

    public struct ComboDiscoveredEvent
    {
        public string ComboId;
        public string ComboName;
    }

    // --- Phase 7: Class-Specific Events ---

    public struct ShapeshiftEvent
    {
        public int HeroSlot;
        public string FormName;
    }

    public struct GoldStolenEvent
    {
        public int HeroSlot;
        public float Amount;
    }

    public struct SoulCollectedEvent
    {
        public int HeroSlot;
        public int SoulCount;
        public int MaxSouls;
    }

    public struct ExecuteTriggeredEvent
    {
        public int HeroSlot;
        public int EnemyId;
        public float Damage;
    }

    public struct TimeFreezeTriggerEvent
    {
        public int EnemyId;
        public float Duration;
    }

    public struct RootAppliedEvent
    {
        public int EnemyId;
        public float Duration;
    }

    // --- Phase 8: Economy, Gacha & Monetization Events ---

    public struct GachaPullEvent
    {
        public string HeroClassId;
        public HeroRarity Rarity;
        public bool IsDuplicate;
        public int StarFragmentsAwarded;
    }

    public struct GachaMultiPullEvent
    {
        public int PullCount;
    }

    public struct PityCounterChangedEvent
    {
        public int EpicPity;
        public int LegendaryPity;
    }

    public struct SparkReadyEvent
    {
        public int TotalPulls;
    }

    public struct StarUpEvent
    {
        public string HeroClassId;
        public int NewStarRating;
    }

    public struct BannerChangedEvent
    {
        public string BannerId;
        public string BannerName;
    }

    public struct IAPPurchaseEvent
    {
        public string ProductId;
        public bool Success;
    }

    public struct BattlePassTierClaimedEvent
    {
        public int Tier;
        public bool IsPremium;
    }

    public struct BattlePassXPGainedEvent
    {
        public int Amount;
        public int CurrentXP;
        public int CurrentTier;
    }

    public struct QuestCompletedEvent
    {
        public string QuestId;
        public bool IsWeekly;
        public int BattlePassXP;
    }

    public struct QuestProgressEvent
    {
        public string QuestId;
        public int CurrentProgress;
        public int RequiredProgress;
    }

    public struct DailyQuestsRefreshedEvent { }
    public struct WeeklyQuestsRefreshedEvent { }

    public enum HeroRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // --- Phase 9: Backend & Social Events ---

    public struct BackendConnectionChangedEvent
    {
        public Backend.ConnectionState OldState;
        public Backend.ConnectionState NewState;
    }

    public struct AuthStateChangedEvent
    {
        public Backend.AuthState State;
        public string UserId;
    }

    public struct CloudSaveSyncedEvent
    {
        public bool Success;
        public long TimestampUnix;
    }

    public struct CloudSaveConflictEvent
    {
        public long LocalTimestamp;
        public long ServerTimestamp;
    }

    public struct LeaderboardScoreSubmittedEvent
    {
        public string LeaderboardId;
        public long Score;
    }

    public struct GuildJoinedEvent
    {
        public string GuildId;
        public string GuildName;
    }

    public struct GuildLeftEvent { }

    public struct GuildTechUpgradedEvent
    {
        public string TechId;
        public int NewLevel;
    }

    public struct ArenaMatchResultEvent
    {
        public bool Won;
        public int NewElo;
        public Backend.ArenaRank NewRank;
    }

    public struct ArenaSeasonEndedEvent
    {
        public int SeasonNumber;
        public Backend.ArenaRank FinalRank;
        public int FinalElo;
    }

    public struct WorldBossEventStartedEvent
    {
        public string BossId;
        public string BossName;
    }

    public struct WorldBossDamageDealtEvent
    {
        public double Damage;
        public double RemainingHp;
        public double MaxHp;
    }

    public struct WorldBossDefeatedEvent
    {
        public string BossId;
    }

    public struct GuildExpeditionNodeClearedEvent
    {
        public int X;
        public int Y;
        public string ClearedByName;
    }

    public struct GuildExpeditionBossUnlockedEvent { }

    // --- Phase 5: Island & Boss Events ---

    public struct IslandChangedEvent
    {
        public int IslandIndex;
        public Islands.IslandData IslandData;
    }

    public struct IslandCompletedEvent
    {
        public int IslandIndex;
        public Islands.IslandData IslandData;
    }

    public struct IslandUnlockedEvent
    {
        public int IslandIndex;
    }

    public struct RealmBossUnlockedEvent
    {
        public int RealmNumber;
    }

    public struct MiniBossSpawnedEvent
    {
        public int StageNumber;
        public Islands.BossMechanicType Mechanic;
    }

    public struct IslandBossSpawnedEvent
    {
        public int IslandIndex;
        public string BossName;
    }

    public struct BossPhaseChangedEvent
    {
        public int PhaseIndex;
        public string PhaseName;
        public float HpThreshold;
    }

    public struct BossDefeatedEvent
    {
        public string BossName;
        public bool IsIslandBoss;
        public bool IsRealmBoss;
    }

    public struct BiomeEffectAppliedEvent
    {
        public Islands.BiomeEffectType EffectType;
        public float Value;
    }

    public struct BossMechanicActivatedEvent
    {
        public Islands.BossMechanicType MechanicType;
        public string WarningText;
    }

    public struct DodgePromptEvent
    {
        public UnityEngine.Vector3 TargetPosition;
        public float TimeWindow;
    }

    public struct RealmBossDefeatedEvent
    {
        public int RealmNumber;
    }

    // --- Phase 6: Ascension Prestige Events ---

    public struct AscensionEvent
    {
        public int HeroSlot;
        public string ClassId;
        public int AscensionCount;
        public double ShardsEarned;
        public int HighestIslandReached;
    }

    public struct AscensionTierChangedEvent
    {
        public int HeroSlot;
        public Progression.AscensionTierLevel OldTier;
        public Progression.AscensionTierLevel NewTier;
        public int AscensionCount;
    }

    public struct AscensionSkillNodePurchasedEvent
    {
        public string NodeId;
        public string BranchId;
        public double ShardCost;
    }

    public struct DemigodRetiredEvent
    {
        public string ClassId;
        public string DemigodBuffDescription;
        public int PantheonSlotsFilled;
    }

    public struct TranscendenceTrialStartedEvent
    {
        public int HeroSlot;
        public string ClassId;
    }

    public struct TranscendenceTrialCompletedEvent
    {
        public int HeroSlot;
        public string ClassId;
        public bool Success;
    }

    public struct PantheonMilestoneEvent
    {
        public int SlotsFilled;
        public string MilestoneName;
        public string Description;
    }

    // --- Phase 10: Events & Endgame ---

    // Tower of Trials
    public struct TowerEnteredEvent { }

    public struct TowerFloorStartedEvent
    {
        public int FloorNumber;
        public string ModifierName;
        public int EnemyLevel;
    }

    public struct TowerFloorClearedEvent
    {
        public int FloorNumber;
        public bool IsMilestone;
        public int PersonalBest;
    }

    public struct TowerBuffChoiceEvent
    {
        public Events.TowerBuff[] Choices;
        public int FloorNumber;
    }

    public struct TowerCompletedEvent
    {
        public int FloorsCleared;
        public int PersonalBest;
    }

    public struct TowerFailedEvent
    {
        public int FloorsCleared;
        public int PersonalBest;
    }

    // Void Rifts
    public struct VoidRiftStartedEvent
    {
        public string RiftName;
        public Events.RiftTheme Theme;
    }

    public struct VoidRiftStageStartedEvent
    {
        public int StageNumber;
        public string RiftName;
        public int EnemyLevel;
    }

    public struct VoidRiftStageClearedEvent
    {
        public int StageNumber;
        public int AetherCrystalsEarned;
        public bool AllStagesCleared;
    }

    public struct VoidRiftStageFailedEvent
    {
        public int StageNumber;
        public int AttemptsRemaining;
    }

    public struct VoidRiftCompletedEvent
    {
        public string RiftName;
        public int TotalAetherCrystals;
    }

    // Seasonal Events
    public struct SeasonalEventStartedEvent
    {
        public string EventId;
        public string EventName;
        public Events.SeasonalEventTheme Theme;
    }

    public struct SeasonalEventEndedEvent
    {
        public string EventId;
        public string EventName;
        public int TotalCurrencyEarned;
        public int StagesCompleted;
    }

    public struct SeasonalEventStageCompletedEvent
    {
        public int StageNumber;
        public int TotalStages;
        public int EventCurrencyEarned;
    }

    public struct EventShopPurchaseEvent
    {
        public string ItemId;
        public string ItemName;
        public int Cost;
    }

    public struct EventQuestCompletedEvent
    {
        public string QuestId;
        public string QuestName;
        public int CurrencyReward;
    }

    // Infinite Ascension
    public struct InfiniteAscensionUnlockedEvent { }

    public struct InfiniteAscensionIslandStartedEvent
    {
        public int IslandNumber;
        public string IslandName;
        public float DifficultyMultiplier;
    }

    public struct InfiniteAscensionIslandCompletedEvent
    {
        public int IslandNumber;
        public string IslandName;
        public int HighestIsland;
    }
}
