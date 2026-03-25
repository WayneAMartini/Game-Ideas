using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        readonly Dictionary<CurrencyType, double> _currencies = new();

        // Overflow caps per currency (0 = unlimited)
        static readonly Dictionary<CurrencyType, double> OverflowCaps = new()
        {
            { CurrencyType.Gold, 0 },
            { CurrencyType.XP, 0 },
            { CurrencyType.Stardust, 999_999 },
            { CurrencyType.AscensionShards, 999_999 },
            { CurrencyType.AetherCrystals, 99_999 },
            { CurrencyType.ClassTokens, 99_999 },
            { CurrencyType.GuildCoins, 999_999 },
            { CurrencyType.StarFragments, 999_999 }
        };

        public double Gold => GetCurrency(CurrencyType.Gold);
        public double Xp => GetCurrency(CurrencyType.XP);
        public double Stardust => GetCurrency(CurrencyType.Stardust);
        public double AscensionShards => GetCurrency(CurrencyType.AscensionShards);
        public double AetherCrystals => GetCurrency(CurrencyType.AetherCrystals);
        public double ClassTokens => GetCurrency(CurrencyType.ClassTokens);
        public double GuildCoins => GetCurrency(CurrencyType.GuildCoins);
        public double StarFragments => GetCurrency(CurrencyType.StarFragments);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeCurrencies();
        }

        void InitializeCurrencies()
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                if (!_currencies.ContainsKey(type))
                    _currencies[type] = 0;
            }
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            AddGold(evt.GoldReward);
            AddXp(evt.XpReward);
        }

        public double GetCurrency(CurrencyType type)
        {
            return _currencies.TryGetValue(type, out var val) ? val : 0;
        }

        public bool CanAfford(CurrencyType type, double amount)
        {
            return GetCurrency(type) >= amount;
        }

        public void AddCurrency(CurrencyType type, double amount)
        {
            if (amount <= 0) return;

            double current = GetCurrency(type);
            double newVal = current + amount;

            // Apply overflow cap
            if (OverflowCaps.TryGetValue(type, out var cap) && cap > 0)
                newVal = System.Math.Min(newVal, cap);

            _currencies[type] = newVal;

            EventBus.Publish(new CurrencyChangedEvent
            {
                Type = type,
                Amount = newVal,
                Delta = amount
            });
        }

        public bool SpendCurrency(CurrencyType type, double amount)
        {
            if (amount <= 0) return false;
            if (!CanAfford(type, amount)) return false;

            _currencies[type] -= amount;

            EventBus.Publish(new CurrencyChangedEvent
            {
                Type = type,
                Amount = _currencies[type],
                Delta = -amount
            });

            return true;
        }

        // Legacy convenience methods
        public void AddGold(double amount) => AddCurrency(CurrencyType.Gold, amount);
        public bool SpendGold(double amount) => SpendCurrency(CurrencyType.Gold, amount);

        public void AddXp(double amount)
        {
            if (amount <= 0) return;

            AddCurrency(CurrencyType.XP, amount);

            // Distribute XP to all party heroes
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null)
            {
                var heroes = partyManager.GetAllAliveHeroes();
                if (heroes.Length > 0)
                {
                    float perHeroXp = (float)amount / heroes.Length;
                    foreach (var hero in heroes)
                        hero.AddXp(perHeroXp);
                }
            }
            else
            {
                var hero = Heroes.HeroManager.Instance?.GetPrimaryHero();
                hero?.AddXp((float)amount);
            }
        }

        public void SetCurrency(CurrencyType type, double value)
        {
            _currencies[type] = System.Math.Max(0, value);
        }

        public Dictionary<CurrencyType, double> GetAllCurrencies()
        {
            return new Dictionary<CurrencyType, double>(_currencies);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
