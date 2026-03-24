using UnityEngine;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;
using Ascendant.Economy;

namespace Ascendant.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Stage Display")]
        [SerializeField] TextMeshProUGUI _stageText;

        [Header("Currency Display")]
        [SerializeField] TextMeshProUGUI _goldText;
        [SerializeField] TextMeshProUGUI _xpText;

        [Header("Momentum Display")]
        [SerializeField] UnityEngine.UI.Slider _momentumBar;
        [SerializeField] TextMeshProUGUI _momentumText;

        void OnEnable()
        {
            EventBus.Subscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<MomentumChangedEvent>(OnMomentumChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<MomentumChangedEvent>(OnMomentumChanged);
        }

        void Start()
        {
            UpdateStageDisplay();
            UpdateCurrencyDisplay();
        }

        void OnStageAdvanced(StageAdvancedEvent evt)
        {
            UpdateStageDisplay();
        }

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            UpdateCurrencyDisplay();
        }

        void OnMomentumChanged(MomentumChangedEvent evt)
        {
            if (_momentumBar != null)
                _momentumBar.value = evt.Stacks / 100f;

            if (_momentumText != null)
                _momentumText.text = $"x{evt.Multiplier:F2}";
        }

        void UpdateStageDisplay()
        {
            if (_stageText == null) return;

            var sm = StageManager.Instance;
            if (sm != null)
                _stageText.text = $"Island {sm.CurrentIsland} \u2014 Stage {sm.CurrentStage}/{sm.StagesPerIsland}";
        }

        void UpdateCurrencyDisplay()
        {
            var cm = CurrencyManager.Instance;
            if (cm == null) return;

            if (_goldText != null)
                _goldText.text = FormatNumber(cm.Gold);

            if (_xpText != null)
                _xpText.text = FormatNumber(cm.Xp);
        }

        static string FormatNumber(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:F1}M";
            if (value >= 1_000) return $"{value / 1_000:F1}K";
            return Mathf.RoundToInt((float)value).ToString();
        }
    }
}
