using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Economy;

namespace Ascendant.UI
{
    public class CurrencyUI : MonoBehaviour
    {
        [Header("Top Bar (Always Visible)")]
        [SerializeField] TextMeshProUGUI _goldText;
        [SerializeField] TextMeshProUGUI _stardustText;

        [Header("Expandable Drawer")]
        [SerializeField] GameObject _drawerPanel;
        [SerializeField] TextMeshProUGUI _ascensionShardsText;
        [SerializeField] TextMeshProUGUI _aetherCrystalsText;
        [SerializeField] TextMeshProUGUI _classTokensText;
        [SerializeField] TextMeshProUGUI _guildCoinsText;
        [SerializeField] TextMeshProUGUI _starFragmentsText;

        [Header("Drawer Toggle")]
        [SerializeField] Button _toggleButton;

        bool _drawerOpen;

        void Start()
        {
            if (_drawerPanel != null)
                _drawerPanel.SetActive(false);

            if (_toggleButton != null)
                _toggleButton.onClick.AddListener(ToggleDrawer);

            RefreshAll();
        }

        void OnEnable()
        {
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            RefreshCurrency(evt.Type, evt.Amount);
        }

        void RefreshAll()
        {
            var cm = CurrencyManager.Instance;
            if (cm == null) return;

            RefreshCurrency(CurrencyType.Gold, cm.Gold);
            RefreshCurrency(CurrencyType.Stardust, cm.Stardust);
            RefreshCurrency(CurrencyType.AscensionShards, cm.AscensionShards);
            RefreshCurrency(CurrencyType.AetherCrystals, cm.AetherCrystals);
            RefreshCurrency(CurrencyType.ClassTokens, cm.ClassTokens);
            RefreshCurrency(CurrencyType.GuildCoins, cm.GuildCoins);
            RefreshCurrency(CurrencyType.StarFragments, cm.StarFragments);
        }

        void RefreshCurrency(CurrencyType type, double amount)
        {
            string formatted = FormatNumber(amount);

            switch (type)
            {
                case CurrencyType.Gold:
                    SetText(_goldText, formatted);
                    break;
                case CurrencyType.Stardust:
                    SetText(_stardustText, formatted);
                    break;
                case CurrencyType.AscensionShards:
                    SetText(_ascensionShardsText, formatted);
                    break;
                case CurrencyType.AetherCrystals:
                    SetText(_aetherCrystalsText, formatted);
                    break;
                case CurrencyType.ClassTokens:
                    SetText(_classTokensText, formatted);
                    break;
                case CurrencyType.GuildCoins:
                    SetText(_guildCoinsText, formatted);
                    break;
                case CurrencyType.StarFragments:
                    SetText(_starFragmentsText, formatted);
                    break;
            }
        }

        void ToggleDrawer()
        {
            _drawerOpen = !_drawerOpen;
            if (_drawerPanel != null)
                _drawerPanel.SetActive(_drawerOpen);
        }

        static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }

        public static string FormatNumber(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:F1}M";
            if (value >= 10_000) return $"{value / 1_000:F1}K";
            return $"{value:N0}";
        }
    }
}
