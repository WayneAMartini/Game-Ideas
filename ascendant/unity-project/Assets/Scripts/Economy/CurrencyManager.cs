using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        double _gold;
        double _xp;

        public double Gold => _gold;
        public double Xp => _xp;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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

        public void AddGold(double amount)
        {
            _gold += amount;
            EventBus.Publish(new CurrencyChangedEvent
            {
                Type = CurrencyType.Gold,
                Amount = _gold,
                Delta = amount
            });
        }

        public void AddXp(double amount)
        {
            _xp += amount;

            // Also give XP to the active hero
            var hero = Heroes.HeroManager.Instance?.GetPrimaryHero();
            hero?.AddXp((float)amount);

            EventBus.Publish(new CurrencyChangedEvent
            {
                Type = CurrencyType.XP,
                Amount = _xp,
                Delta = amount
            });
        }

        public bool SpendGold(double amount)
        {
            if (_gold < amount) return false;
            _gold -= amount;
            EventBus.Publish(new CurrencyChangedEvent
            {
                Type = CurrencyType.Gold,
                Amount = _gold,
                Delta = -amount
            });
            return true;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
