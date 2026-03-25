using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Events;

namespace Ascendant.UI
{
    public class EventShopUI : MonoBehaviour
    {
        [Header("Shop Panel")]
        [SerializeField] GameObject _shopPanel;
        [SerializeField] TextMeshProUGUI _shopTitleText;
        [SerializeField] TextMeshProUGUI _currencyBalanceText;
        [SerializeField] Transform _itemListContent;
        [SerializeField] GameObject _shopItemPrefab;
        [SerializeField] Button _closeButton;

        [Header("Item Detail")]
        [SerializeField] GameObject _detailPanel;
        [SerializeField] TextMeshProUGUI _itemNameText;
        [SerializeField] TextMeshProUGUI _itemDescText;
        [SerializeField] TextMeshProUGUI _itemCostText;
        [SerializeField] Button _purchaseButton;
        [SerializeField] Button _detailCloseButton;

        EventShopItem _selectedItem;
        readonly List<GameObject> _spawnedItems = new();

        void OnEnable()
        {
            EventBus.Subscribe<EventShopPurchaseEvent>(OnPurchase);
            if (_closeButton) _closeButton.onClick.AddListener(OnClose);
            if (_purchaseButton) _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (_detailCloseButton) _detailCloseButton.onClick.AddListener(OnDetailClose);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EventShopPurchaseEvent>(OnPurchase);
        }

        public void ShowShop()
        {
            if (_shopPanel) _shopPanel.SetActive(true);
            if (_detailPanel) _detailPanel.SetActive(false);
            RefreshShop();
        }

        void RefreshShop()
        {
            var mgr = SeasonalEventManager.Instance;
            if (mgr == null || !mgr.IsEventActive) return;

            var config = mgr.ActiveEvent;
            if (_shopTitleText) _shopTitleText.text = $"{config?.eventName ?? "Event"} Shop";
            if (_currencyBalanceText) _currencyBalanceText.text = $"{config?.eventCurrencyName ?? "Tokens"}: {mgr.EventCurrency}";

            // Clear existing items
            foreach (var go in _spawnedItems)
                if (go) Destroy(go);
            _spawnedItems.Clear();

            // Spawn shop items
            if (config?.shopItems == null || _shopItemPrefab == null || _itemListContent == null) return;

            foreach (var item in config.shopItems)
            {
                var go = Instantiate(_shopItemPrefab, _itemListContent);
                _spawnedItems.Add(go);

                var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText) nameText.text = $"{item.itemName} - {item.eventCurrencyCost}";

                var button = go.GetComponentInChildren<Button>();
                if (button)
                {
                    var captured = item;
                    button.onClick.AddListener(() => ShowItemDetail(captured));

                    bool purchased = mgr.IsItemPurchased(item.itemId);
                    bool canAfford = mgr.CanPurchaseItem(item);
                    button.interactable = canAfford && !purchased;
                }
            }
        }

        void ShowItemDetail(EventShopItem item)
        {
            _selectedItem = item;
            if (_detailPanel) _detailPanel.SetActive(true);
            if (_itemNameText) _itemNameText.text = item.itemName;
            if (_itemDescText) _itemDescText.text = item.description;
            if (_itemCostText) _itemCostText.text = $"Cost: {item.eventCurrencyCost}";

            var mgr = SeasonalEventManager.Instance;
            if (_purchaseButton) _purchaseButton.interactable = mgr != null && mgr.CanPurchaseItem(item);
        }

        void OnPurchaseClicked()
        {
            if (_selectedItem == null) return;
            SeasonalEventManager.Instance?.PurchaseItem(_selectedItem);
        }

        void OnPurchase(EventShopPurchaseEvent evt)
        {
            RefreshShop();
            if (_detailPanel) _detailPanel.SetActive(false);
        }

        void OnClose()
        {
            if (_shopPanel) _shopPanel.SetActive(false);
        }

        void OnDetailClose()
        {
            if (_detailPanel) _detailPanel.SetActive(false);
        }
    }
}
