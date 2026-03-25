using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;
using Ascendant.Economy;
using Ascendant.Party;
using Ascendant.Islands;

namespace Ascendant.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Stage Display")]
        [SerializeField] TextMeshProUGUI _stageText;

        [Header("Island/Biome Display")]
        [SerializeField] TextMeshProUGUI _islandNameText;
        [SerializeField] Image _biomeIndicator;

        [Header("Boss HP Bar")]
        [SerializeField] GameObject _bossHpPanel;
        [SerializeField] Slider _bossHpBar;
        [SerializeField] TextMeshProUGUI _bossNameText;
        [SerializeField] TextMeshProUGUI _bossPhaseText;

        [Header("Boss Mechanic Warning")]
        [SerializeField] GameObject _mechanicWarningPanel;
        [SerializeField] TextMeshProUGUI _mechanicWarningText;

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

        float _mechanicWarningTimer;

        void OnEnable()
        {
            EventBus.Subscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Subscribe<PartyChangedEvent>(OnPartyChanged);
            EventBus.Subscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Subscribe<IslandBossSpawnedEvent>(OnIslandBossSpawned);
            EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<BossMechanicActivatedEvent>(OnBossMechanicActivated);
            EventBus.Subscribe<MiniBossSpawnedEvent>(OnMiniBossSpawned);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<MomentumChangedEvent>(OnMomentumChanged);
            EventBus.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
            EventBus.Unsubscribe<IslandChangedEvent>(OnIslandChanged);
            EventBus.Unsubscribe<IslandBossSpawnedEvent>(OnIslandBossSpawned);
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<BossMechanicActivatedEvent>(OnBossMechanicActivated);
            EventBus.Unsubscribe<MiniBossSpawnedEvent>(OnMiniBossSpawned);
        }

        void Start()
        {
            UpdateStageDisplay();
            UpdateIslandDisplay();
            UpdateCurrencyDisplay();
            RefreshPartyDisplay();
            HideBossUI();
        }

        void Update()
        {
            // Boss HP bar update
            UpdateBossHpBar();

            // Mechanic warning auto-hide
            if (_mechanicWarningTimer > 0f)
            {
                _mechanicWarningTimer -= Time.deltaTime;
                if (_mechanicWarningTimer <= 0f)
                    HideMechanicWarning();
            }
        }

        void OnStageAdvanced(StageAdvancedEvent evt)
        {
            UpdateStageDisplay();
            HideBossUI();
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

        void OnIslandChanged(IslandChangedEvent evt)
        {
            UpdateIslandDisplay();
            UpdateStageDisplay();
        }

        void OnIslandBossSpawned(IslandBossSpawnedEvent evt)
        {
            ShowBossUI(evt.BossName);
        }

        void OnMiniBossSpawned(MiniBossSpawnedEvent evt)
        {
            ShowBossUI($"Mini-Boss (Stage {evt.StageNumber})");
        }

        void OnBossPhaseChanged(BossPhaseChangedEvent evt)
        {
            if (_bossPhaseText != null)
                _bossPhaseText.text = evt.PhaseName;
        }

        void OnBossDefeated(BossDefeatedEvent evt)
        {
            HideBossUI();
        }

        void OnBossMechanicActivated(BossMechanicActivatedEvent evt)
        {
            ShowMechanicWarning(evt.WarningText);
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
                _stageText.text = $"{sm.CurrentIslandName} \u2014 Stage {sm.CurrentStage}/{sm.StagesPerIsland}";
        }

        void UpdateIslandDisplay()
        {
            var island = IslandManager.Instance?.CurrentIsland;
            if (island == null) return;

            if (_islandNameText != null)
                _islandNameText.text = island.islandName;

            if (_biomeIndicator != null)
            {
                _biomeIndicator.color = island.biomeData != null
                    ? island.biomeData.ambientColor
                    : Color.white;
            }
        }

        void ShowBossUI(string bossName)
        {
            if (_bossHpPanel != null) _bossHpPanel.SetActive(true);
            if (_bossNameText != null) _bossNameText.text = bossName;
            if (_bossHpBar != null) _bossHpBar.value = 1f;
            if (_bossPhaseText != null) _bossPhaseText.text = "";
        }

        void HideBossUI()
        {
            if (_bossHpPanel != null) _bossHpPanel.SetActive(false);
            HideMechanicWarning();
        }

        void UpdateBossHpBar()
        {
            if (_bossHpBar == null) return;

            var islandBoss = IslandBossController.Instance;
            if (islandBoss != null && islandBoss.IsBossFight)
            {
                _bossHpBar.value = islandBoss.BossHpPercent;
                return;
            }

            var realmBoss = RealmBossController.Instance;
            if (realmBoss != null && realmBoss.IsBossFight)
            {
                // Realm boss HP tracked similarly
                return;
            }

            var miniBoss = MiniBossController.Instance;
            if (miniBoss != null && miniBoss.IsMiniBossFight && miniBoss.CurrentMiniBoss != null)
            {
                var mb = miniBoss.CurrentMiniBoss;
                _bossHpBar.value = mb.CurrentHp / mb.MaxHp;
            }
        }

        void ShowMechanicWarning(string text)
        {
            if (_mechanicWarningPanel != null) _mechanicWarningPanel.SetActive(true);
            if (_mechanicWarningText != null) _mechanicWarningText.text = text;
            _mechanicWarningTimer = 3f;
        }

        void HideMechanicWarning()
        {
            if (_mechanicWarningPanel != null) _mechanicWarningPanel.SetActive(false);
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
