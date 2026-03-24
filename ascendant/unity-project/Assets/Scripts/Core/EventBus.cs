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
}
