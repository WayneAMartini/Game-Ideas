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
        XP
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
}
