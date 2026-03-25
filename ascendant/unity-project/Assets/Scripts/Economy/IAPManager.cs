using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public string displayName;
        public string priceString;
        public float priceUSD;
        public IAPProductType type;
        public int stardustAmount;
        public bool isOneTimePurchase;
        public bool purchased; // for one-time packs
    }

    public enum IAPProductType
    {
        StardustPackage,
        Subscription,
        StarterPack,
        BattlePass
    }

    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        readonly List<IAPProduct> _products = new();
        bool _patronBlessingActive;
        DateTime _patronBlessingExpiry;

        public bool IsPatronBlessingActive => _patronBlessingActive && DateTime.UtcNow < _patronBlessingExpiry;
        public IReadOnlyList<IAPProduct> Products => _products;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeProducts();
        }

        void InitializeProducts()
        {
            _products.Clear();

            // Stardust packages
            AddProduct("stardust_100", "Handful of Stars", "$0.99", 0.99f, IAPProductType.StardustPackage, 100);
            AddProduct("stardust_600", "Star Pouch", "$4.99", 4.99f, IAPProductType.StardustPackage, 600);
            AddProduct("stardust_1300", "Star Chest", "$9.99", 9.99f, IAPProductType.StardustPackage, 1300);
            AddProduct("stardust_3500", "Constellation", "$24.99", 24.99f, IAPProductType.StardustPackage, 3500);
            AddProduct("stardust_7500", "Galaxy", "$49.99", 49.99f, IAPProductType.StardustPackage, 7500);
            AddProduct("stardust_16000", "Supernova", "$99.99", 99.99f, IAPProductType.StardustPackage, 16000);

            // Patron's Blessing subscription
            AddProduct("patron_blessing", "Patron's Blessing", "$4.99/mo", 4.99f, IAPProductType.Subscription, 0);

            // Starter packs (one-time)
            AddProduct("starter_warrior", "Adventurer's Kit", "$2.99", 2.99f, IAPProductType.StarterPack, 300, true);
            AddProduct("starter_bundle", "Hero's Arsenal", "$9.99", 9.99f, IAPProductType.StarterPack, 1000, true);
            AddProduct("starter_champion", "Champion's Legacy", "$19.99", 19.99f, IAPProductType.StarterPack, 3000, true);
            AddProduct("starter_ascendant", "Ascendant's Birthright", "$49.99", 49.99f, IAPProductType.StarterPack, 8000, true);

            // Battle Pass premium
            AddProduct("battle_pass_premium", "Ascendant Pass", "$9.99", 9.99f, IAPProductType.BattlePass, 0);
        }

        void AddProduct(string id, string name, string price, float usd, IAPProductType type, int stardust, bool oneTime = false)
        {
            _products.Add(new IAPProduct
            {
                productId = id,
                displayName = name,
                priceString = price,
                priceUSD = usd,
                type = type,
                stardustAmount = stardust,
                isOneTimePurchase = oneTime
            });
        }

        public bool Purchase(string productId)
        {
            var product = _products.Find(p => p.productId == productId);
            if (product == null) return false;

            // One-time packs can't be re-purchased
            if (product.isOneTimePurchase && product.purchased)
            {
                Debug.Log($"[IAPManager] Pack '{productId}' already purchased.");
                return false;
            }

            // In a real implementation, this would go through Unity IAP
            // For now, simulate the purchase and award immediately
            return ProcessPurchase(product);
        }

        bool ProcessPurchase(IAPProduct product)
        {
            // Receipt validation stub — will connect to Firebase later
            if (!ValidateReceipt(product.productId))
            {
                Debug.LogWarning($"[IAPManager] Receipt validation failed for {product.productId}");
                return false;
            }

            var cm = CurrencyManager.Instance;

            switch (product.type)
            {
                case IAPProductType.StardustPackage:
                    cm?.AddCurrency(CurrencyType.Stardust, product.stardustAmount);
                    break;

                case IAPProductType.Subscription:
                    ActivatePatronBlessing();
                    break;

                case IAPProductType.StarterPack:
                    cm?.AddCurrency(CurrencyType.Stardust, product.stardustAmount);
                    AwardStarterPackBonus(product.productId);
                    product.purchased = true;
                    break;

                case IAPProductType.BattlePass:
                    BattlePassSystem.Instance?.ActivatePremium();
                    break;
            }

            EventBus.Publish(new IAPPurchaseEvent
            {
                ProductId = product.productId,
                Success = true
            });

            Debug.Log($"[IAPManager] Purchase complete: {product.displayName}");
            return true;
        }

        void ActivatePatronBlessing()
        {
            _patronBlessingActive = true;
            _patronBlessingExpiry = DateTime.UtcNow.AddDays(30);
            Debug.Log("[IAPManager] Patron's Blessing activated for 30 days.");
        }

        void AwardStarterPackBonus(string productId)
        {
            var cm = CurrencyManager.Instance;
            if (cm == null) return;

            switch (productId)
            {
                case "starter_warrior":
                    cm.AddCurrency(CurrencyType.Gold, 500_000);
                    break;
                case "starter_bundle":
                    cm.AddCurrency(CurrencyType.Gold, 1_000_000);
                    break;
                case "starter_champion":
                    cm.AddCurrency(CurrencyType.Gold, 5_000_000);
                    cm.AddCurrency(CurrencyType.ClassTokens, 100);
                    break;
                case "starter_ascendant":
                    cm.AddCurrency(CurrencyType.Gold, 10_000_000);
                    ActivatePatronBlessing();
                    break;
            }
        }

        // Stub for server-side validation (Firebase Cloud Functions)
        bool ValidateReceipt(string productId)
        {
            // TODO: Connect to Firebase Cloud Functions for real receipt validation
            return true;
        }

        public void RestorePurchases()
        {
            // TODO: Implement restore purchases via Unity IAP
            Debug.Log("[IAPManager] Restore purchases requested.");
        }

        public IAPProduct GetProduct(string productId)
        {
            return _products.Find(p => p.productId == productId);
        }

        public List<IAPProduct> GetProductsByType(IAPProductType type)
        {
            return _products.FindAll(p => p.type == type);
        }

        // Save/Load for patron blessing state
        public bool GetPatronActive() => _patronBlessingActive;
        public long GetPatronExpiryUnix() => new DateTimeOffset(_patronBlessingExpiry).ToUnixTimeSeconds();

        public void LoadPatronState(bool active, long expiryUnix)
        {
            _patronBlessingActive = active;
            _patronBlessingExpiry = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
