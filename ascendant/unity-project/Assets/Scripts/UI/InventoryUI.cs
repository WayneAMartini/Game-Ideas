using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject _inventoryPanel;
        [SerializeField] Transform _gridContent;
        [SerializeField] GameObject _itemSlotPrefab;

        [Header("Detail Panel")]
        [SerializeField] GameObject _detailPanel;
        [SerializeField] TextMeshProUGUI _itemNameText;
        [SerializeField] TextMeshProUGUI _itemRarityText;
        [SerializeField] TextMeshProUGUI _itemStatsText;
        [SerializeField] TextMeshProUGUI _itemSetText;
        [SerializeField] TextMeshProUGUI _enhanceLevelText;
        [SerializeField] Button _equipButton;
        [SerializeField] Button _enhanceButton;
        [SerializeField] TextMeshProUGUI _enhanceCostText;

        [Header("Compare Panel")]
        [SerializeField] GameObject _comparePanel;
        [SerializeField] TextMeshProUGUI _compareCurrentText;
        [SerializeField] TextMeshProUGUI _compareNewText;

        [Header("Filters")]
        [SerializeField] TMP_Dropdown _slotFilter;
        [SerializeField] TMP_Dropdown _rarityFilter;
        [SerializeField] TMP_Dropdown _sortDropdown;

        [Header("Hero Selection")]
        [SerializeField] int _selectedHeroSlot;

        Equipment _selectedItem;
        EquipmentSlot? _filterSlot;
        EquipmentRarity? _filterRarity;

        void OnEnable()
        {
            EventBus.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);
            EventBus.Subscribe<EquipmentDropEvent>(OnEquipmentDrop);
            RefreshGrid();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);
            EventBus.Unsubscribe<EquipmentDropEvent>(OnEquipmentDrop);
        }

        void OnEquipmentChanged(EquipmentChangedEvent evt) => RefreshGrid();
        void OnEquipmentDrop(EquipmentDropEvent evt) => RefreshGrid();

        public void SetSelectedHero(int heroSlot)
        {
            _selectedHeroSlot = heroSlot;
            RefreshGrid();
        }

        public void RefreshGrid()
        {
            var system = EquipmentSystem.Instance;
            if (system == null || _gridContent == null) return;

            // Clear existing slots
            for (int i = _gridContent.childCount - 1; i >= 0; i--)
                Destroy(_gridContent.GetChild(i).gameObject);

            // Populate
            var inventory = system.Inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                if (!PassesFilter(item)) continue;

                if (_itemSlotPrefab != null)
                {
                    var slotGO = Instantiate(_itemSlotPrefab, _gridContent);
                    SetupSlotVisual(slotGO, item);
                }
            }
        }

        bool PassesFilter(Equipment item)
        {
            if (_filterSlot.HasValue && item.slot != _filterSlot.Value) return false;
            if (_filterRarity.HasValue && item.rarity != _filterRarity.Value) return false;
            return true;
        }

        void SetupSlotVisual(GameObject slotGO, Equipment item)
        {
            var nameText = slotGO.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = $"{item.equipmentName} +{item.enhanceLevel}";

            var img = slotGO.GetComponentInChildren<Image>();
            if (img != null)
                img.color = GetRarityColor(item.rarity);

            var button = slotGO.GetComponent<Button>();
            if (button != null)
            {
                var captured = item;
                button.onClick.AddListener(() => SelectItem(captured));
            }
        }

        public void SelectItem(Equipment item)
        {
            _selectedItem = item;
            ShowDetail(item);
            ShowComparison(item);
        }

        void ShowDetail(Equipment item)
        {
            if (_detailPanel != null) _detailPanel.SetActive(true);
            if (_itemNameText != null) _itemNameText.text = item.equipmentName;
            if (_itemRarityText != null)
            {
                _itemRarityText.text = item.rarity.ToString();
                _itemRarityText.color = GetRarityColor(item.rarity);
            }

            if (_itemStatsText != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"{item.primaryStat.stat}: +{item.primaryStat.value:F1}");
                if (item.secondaryStats != null)
                    foreach (var s in item.secondaryStats)
                        sb.AppendLine($"{s.stat}: +{s.value:F1}");
                _itemStatsText.text = sb.ToString();
            }

            if (_itemSetText != null)
                _itemSetText.text = string.IsNullOrEmpty(item.setName) ? "" : $"Set: {item.setName}";

            if (_enhanceLevelText != null)
                _enhanceLevelText.text = $"+{item.enhanceLevel}";

            if (_enhanceCostText != null && EquipmentSystem.Instance != null)
                _enhanceCostText.text = $"Enhance: {EquipmentSystem.Instance.GetEnhanceCost(item.enhanceLevel):F0} Gold";
        }

        void ShowComparison(Equipment newItem)
        {
            if (_comparePanel == null) return;

            var system = EquipmentSystem.Instance;
            if (system == null) return;

            var current = system.GetEquipped(_selectedHeroSlot, newItem.slot);
            if (current == null)
            {
                _comparePanel.SetActive(false);
                return;
            }

            _comparePanel.SetActive(true);

            if (_compareCurrentText != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"[Equipped] {current.equipmentName} +{current.enhanceLevel}");
                sb.AppendLine($"{current.primaryStat.stat}: +{current.GetTotalStat(current.primaryStat.stat):F1}");
                _compareCurrentText.text = sb.ToString();
            }

            if (_compareNewText != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"[New] {newItem.equipmentName} +{newItem.enhanceLevel}");
                sb.AppendLine($"{newItem.primaryStat.stat}: +{newItem.GetTotalStat(newItem.primaryStat.stat):F1}");
                float diff = newItem.GetPowerScore() - current.GetPowerScore();
                sb.AppendLine(diff >= 0 ? $"<color=green>+{diff:F0} Power</color>" : $"<color=red>{diff:F0} Power</color>");
                _compareNewText.text = sb.ToString();
            }
        }

        public void OnEquipButtonClicked()
        {
            if (_selectedItem == null) return;
            EquipmentSystem.Instance?.Equip(_selectedHeroSlot, _selectedItem);
            _selectedItem = null;
            if (_detailPanel != null) _detailPanel.SetActive(false);
            RefreshGrid();
        }

        public void OnEnhanceButtonClicked()
        {
            if (_selectedItem == null) return;
            if (EquipmentSystem.Instance != null && EquipmentSystem.Instance.TryEnhance(_selectedItem))
                ShowDetail(_selectedItem);
        }

        public void OnSlotFilterChanged(int index)
        {
            _filterSlot = index <= 0 ? null : (EquipmentSlot)(index - 1);
            RefreshGrid();
        }

        public void OnRarityFilterChanged(int index)
        {
            _filterRarity = index <= 0 ? null : (EquipmentRarity)(index - 1);
            RefreshGrid();
        }

        public void OnSortChanged(int index)
        {
            var system = EquipmentSystem.Instance;
            if (system == null) return;

            if (index == 0) system.SortByPower();
            else if (index == 1) system.SortByRarity();
            RefreshGrid();
        }

        public static Color GetRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Common => Color.white,
                EquipmentRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                EquipmentRarity.Rare => new Color(0.3f, 0.5f, 1f),
                EquipmentRarity.Epic => new Color(0.7f, 0.3f, 0.9f),
                EquipmentRarity.Legendary => new Color(1f, 0.6f, 0f),
                EquipmentRarity.Mythic => new Color(1f, 0.15f, 0.15f),
                _ => Color.white
            };
        }
    }
}
