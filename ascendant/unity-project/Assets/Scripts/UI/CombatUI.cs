using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;
using Ascendant.Economy;
using Ascendant.Party;

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
        [SerializeField] Slider _momentumBar;
        [SerializeField] TextMeshProUGUI _momentumText;

        [Header("Hero Portraits (4 slots)")]
        [SerializeField] HeroPortrait[] _heroPortraits = new HeroPortrait[4];

        [Header("Cooldown Overlays (per hero, 3 per hero)")]
        [SerializeField] CooldownUI[] _cooldownUIs;

        [Header("Ultimate Charge Bars (per hero)")]
        [SerializeField] UltimateChargeBar[] _ultimateChargeBars;

        [Header("Combo Point Display (Rogue)")]
        [SerializeField] ComboPointUI _comboPointUI;

        [Header("Formation Display")]
        [SerializeField] GameObject _formationPanel;
        [SerializeField] Image[] _formationSlotImages;

        void OnEnable()
        {
            EventBus.Subscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Subscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void Start()
        {
            UpdateStageDisplay();
            UpdateCurrencyDisplay();
            RefreshPartyDisplay();
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

        void OnPartyChanged(PartyChangedEvent evt)
        {
            RefreshPartyDisplay();
        }

        void RefreshPartyDisplay()
        {
            var partyManager = PartyManager.Instance;
            if (partyManager == null) return;

            for (int i = 0; i < 4; i++)
            {
                var hero = partyManager.GetHero(i);

                if (i < _heroPortraits.Length && _heroPortraits[i] != null)
                {
                    if (hero != null)
                    {
                        _heroPortraits[i].gameObject.SetActive(true);
                        _heroPortraits[i].Initialize(hero);
                    }
                    else
                    {
                        _heroPortraits[i].gameObject.SetActive(false);
                    }
                }

                // Update formation slot images
                if (_formationSlotImages != null && i < _formationSlotImages.Length &&
                    _formationSlotImages[i] != null)
                {
                    _formationSlotImages[i].gameObject.SetActive(hero != null);
                    if (hero != null && hero.Data != null && hero.Data.portrait != null)
                        _formationSlotImages[i].sprite = hero.Data.portrait;
                }
            }
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
