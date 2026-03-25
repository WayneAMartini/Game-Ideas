using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Economy;

namespace Ascendant.UI
{
    public class StoreUI : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] Button _stardustTabButton;
        [SerializeField] Button _subscriptionTabButton;
        [SerializeField] Button _packsTabButton;

        [Header("Content")]
        [SerializeField] Transform _productContainer;
        [SerializeField] GameObject _productCardPrefab;

        [Header("Restore")]
        [SerializeField] Button _restoreButton;

        IAPProductType _currentTab = IAPProductType.StardustPackage;

        void Start()
        {
            if (_stardustTabButton != null)
                _stardustTabButton.onClick.AddListener(() => ShowTab(IAPProductType.StardustPackage));
            if (_subscriptionTabButton != null)
                _subscriptionTabButton.onClick.AddListener(() => ShowTab(IAPProductType.Subscription));
            if (_packsTabButton != null)
                _packsTabButton.onClick.AddListener(() => ShowTab(IAPProductType.StarterPack));
            if (_restoreButton != null)
                _restoreButton.onClick.AddListener(OnRestore);

            ShowTab(IAPProductType.StardustPackage);
        }

        void ShowTab(IAPProductType type)
        {
            _currentTab = type;
            RefreshProductList();
        }

        void RefreshProductList()
        {
            if (_productContainer == null) return;

            // Clear existing cards
            foreach (Transform child in _productContainer)
                Destroy(child.gameObject);

            var iap = IAPManager.Instance;
            if (iap == null) return;

            var products = iap.GetProductsByType(_currentTab);
            foreach (var product in products)
            {
                if (_productCardPrefab == null) continue;

                var card = Instantiate(_productCardPrefab, _productContainer);
                var cardUI = card.GetComponent<StoreProductCard>();
                if (cardUI != null)
                    cardUI.Setup(product, OnPurchase);
            }
        }

        void OnPurchase(IAPProduct product)
        {
            var iap = IAPManager.Instance;
            if (iap == null) return;

            iap.Purchase(product.productId);
            RefreshProductList();
        }

        void OnRestore()
        {
            IAPManager.Instance?.RestorePurchases();
        }
    }

    public class StoreProductCard : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] TextMeshProUGUI _priceText;
        [SerializeField] TextMeshProUGUI _contentsText;
        [SerializeField] Button _buyButton;

        IAPProduct _product;
        System.Action<IAPProduct> _onPurchase;

        public void Setup(IAPProduct product, System.Action<IAPProduct> onPurchase)
        {
            _product = product;
            _onPurchase = onPurchase;

            if (_nameText != null)
                _nameText.text = product.displayName;
            if (_priceText != null)
                _priceText.text = product.priceString;
            if (_contentsText != null && product.stardustAmount > 0)
                _contentsText.text = $"{product.stardustAmount} Stardust";
            if (_buyButton != null)
            {
                _buyButton.onClick.AddListener(OnBuyClicked);
                _buyButton.interactable = !(product.isOneTimePurchase && product.purchased);
            }
        }

        void OnBuyClicked()
        {
            _onPurchase?.Invoke(_product);
        }
    }
}
