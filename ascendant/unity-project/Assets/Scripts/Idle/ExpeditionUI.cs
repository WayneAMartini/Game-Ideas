using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;

namespace Ascendant.Idle
{
    public class ExpeditionUI : MonoBehaviour
    {
        [Header("Expedition List")]
        [SerializeField] GameObject _expeditionPanel;
        [SerializeField] Transform _availableExpeditionsContainer;
        [SerializeField] Transform _activeExpeditionsContainer;

        [Header("Slot Prefabs (assign in inspector)")]
        [SerializeField] GameObject _expeditionSlotPrefab;
        [SerializeField] GameObject _activeSlotPrefab;
        [SerializeField] GameObject _lockedSlotPrefab;

        [Header("Deployment")]
        [SerializeField] GameObject _deploymentPanel;
        [SerializeField] TMPro.TextMeshProUGUI _expeditionNameText;
        [SerializeField] TMPro.TextMeshProUGUI _expeditionDescText;
        [SerializeField] TMPro.TextMeshProUGUI _durationText;
        [SerializeField] TMPro.TextMeshProUGUI _rewardsText;
        [SerializeField] TMPro.TextMeshProUGUI _recommendedLevelText;
        [SerializeField] Button _deployButton;
        [SerializeField] Button _closeButton;

        [Header("Active Expedition Display")]
        [SerializeField] TMPro.TextMeshProUGUI[] _activeTimerTexts; // one per slot
        [SerializeField] Slider[] _activeProgressBars;              // one per slot
        [SerializeField] Button[] _collectButtons;                  // one per slot

        ExpeditionData _selectedExpedition;

        void OnEnable()
        {
            EventBus.Subscribe<ExpeditionCompletedEvent>(OnExpeditionCompleted);
            EventBus.Subscribe<ExpeditionCollectedEvent>(OnExpeditionCollected);

            if (_deployButton != null)
                _deployButton.onClick.AddListener(OnDeployPressed);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnClosePressed);

            for (int i = 0; i < _collectButtons?.Length; i++)
            {
                int slotIndex = i;
                if (_collectButtons[i] != null)
                    _collectButtons[i].onClick.AddListener(() => OnCollectPressed(slotIndex));
            }
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ExpeditionCompletedEvent>(OnExpeditionCompleted);
            EventBus.Unsubscribe<ExpeditionCollectedEvent>(OnExpeditionCollected);

            if (_deployButton != null)
                _deployButton.onClick.RemoveListener(OnDeployPressed);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnClosePressed);
        }

        void Update()
        {
            UpdateActiveExpeditionTimers();
        }

        public void Show()
        {
            if (_expeditionPanel != null)
                _expeditionPanel.SetActive(true);
            if (_deploymentPanel != null)
                _deploymentPanel.SetActive(false);
            RefreshDisplay();
        }

        public void Hide()
        {
            if (_expeditionPanel != null)
                _expeditionPanel.SetActive(false);
        }

        public void SelectExpedition(ExpeditionData data)
        {
            _selectedExpedition = data;
            ShowDeploymentPanel(data);
        }

        void ShowDeploymentPanel(ExpeditionData data)
        {
            if (_deploymentPanel != null)
                _deploymentPanel.SetActive(true);

            if (_expeditionNameText != null)
                _expeditionNameText.text = data.displayName;

            if (_expeditionDescText != null)
                _expeditionDescText.text = data.description;

            if (_durationText != null)
            {
                int hours = Mathf.FloorToInt(data.durationHours);
                _durationText.text = hours + "h";
            }

            if (_rewardsText != null)
            {
                string rewards = "";
                if (data.baseGoldReward > 0) rewards += FormatNumber(data.baseGoldReward) + " Gold";
                if (data.baseMaterialReward > 0) rewards += (rewards.Length > 0 ? " + " : "") + data.baseMaterialReward + " Materials";
                if (data.stardustReward > 0) rewards += (rewards.Length > 0 ? " + " : "") + data.stardustReward + " Stardust";
                if (data.classTokenReward > 0) rewards += (rewards.Length > 0 ? " + " : "") + data.classTokenReward + " Class Tokens";
                if (data.ascensionShardReward > 0) rewards += (rewards.Length > 0 ? " + " : "") + data.ascensionShardReward + " Ascension Shards";
                if (data.equipmentDropChance > 0) rewards += (rewards.Length > 0 ? " + " : "") + "Equipment Drop";
                _rewardsText.text = rewards;
            }

            if (_recommendedLevelText != null)
                _recommendedLevelText.text = "Recommended Lv. " + data.recommendedHeroLevel;

            if (_deployButton != null)
                _deployButton.interactable = ExpeditionManager.Instance != null && ExpeditionManager.Instance.CanStartExpedition();
        }

        void OnDeployPressed()
        {
            if (_selectedExpedition == null) return;
            if (ExpeditionManager.Instance == null) return;

            // Use a default hero level (in a full implementation, the player would select a specific hero)
            int heroLevel = GetBenchHeroLevel();

            if (ExpeditionManager.Instance.StartExpedition(_selectedExpedition, heroLevel))
            {
                if (_deploymentPanel != null)
                    _deploymentPanel.SetActive(false);
                _selectedExpedition = null;
                RefreshDisplay();
            }
        }

        void OnClosePressed()
        {
            if (_deploymentPanel != null)
                _deploymentPanel.SetActive(false);
            _selectedExpedition = null;
        }

        void OnCollectPressed(int slotIndex)
        {
            ExpeditionManager.Instance?.CollectExpedition(slotIndex);
        }

        void OnExpeditionCompleted(ExpeditionCompletedEvent evt)
        {
            RefreshDisplay();
        }

        void OnExpeditionCollected(ExpeditionCollectedEvent evt)
        {
            RefreshDisplay();
        }

        void RefreshDisplay()
        {
            var manager = ExpeditionManager.Instance;
            if (manager == null) return;

            // Update active expedition slots
            for (int i = 0; i < manager.MaxSlots; i++)
            {
                bool isUnlocked = i < manager.UnlockedSlots;
                bool hasExpedition = i < manager.ActiveExpeditions.Count;

                if (_collectButtons != null && i < _collectButtons.Length && _collectButtons[i] != null)
                {
                    bool completed = hasExpedition && manager.ActiveExpeditions[i].Completed;
                    _collectButtons[i].gameObject.SetActive(isUnlocked && hasExpedition && completed);
                }

                if (_activeProgressBars != null && i < _activeProgressBars.Length && _activeProgressBars[i] != null)
                {
                    _activeProgressBars[i].gameObject.SetActive(isUnlocked && hasExpedition);
                    if (hasExpedition)
                        _activeProgressBars[i].value = manager.GetExpeditionProgress(i);
                }

                if (_activeTimerTexts != null && i < _activeTimerTexts.Length && _activeTimerTexts[i] != null)
                {
                    _activeTimerTexts[i].gameObject.SetActive(isUnlocked && hasExpedition);
                }
            }
        }

        void UpdateActiveExpeditionTimers()
        {
            var manager = ExpeditionManager.Instance;
            if (manager == null) return;

            for (int i = 0; i < manager.ActiveExpeditions.Count && i < manager.MaxSlots; i++)
            {
                var exp = manager.ActiveExpeditions[i];

                if (_activeProgressBars != null && i < _activeProgressBars.Length && _activeProgressBars[i] != null)
                    _activeProgressBars[i].value = manager.GetExpeditionProgress(i);

                if (_activeTimerTexts != null && i < _activeTimerTexts.Length && _activeTimerTexts[i] != null)
                {
                    if (exp.Completed)
                    {
                        _activeTimerTexts[i].text = "Complete!";
                    }
                    else
                    {
                        float remaining = manager.GetRemainingSeconds(i);
                        int hours = Mathf.FloorToInt(remaining / 3600f);
                        int minutes = Mathf.FloorToInt((remaining % 3600f) / 60f);
                        int seconds = Mathf.FloorToInt(remaining % 60f);
                        _activeTimerTexts[i].text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                    }
                }
            }
        }

        int GetBenchHeroLevel()
        {
            // Average level of party heroes as default (bench heroes would be similar level)
            var partyManager = Party.PartyManager.Instance;
            if (partyManager == null) return 1;

            var heroes = partyManager.GetAllAliveHeroes();
            if (heroes.Length == 0) return 1;

            int totalLevel = 0;
            foreach (var hero in heroes)
                totalLevel += hero.Level;
            return totalLevel / heroes.Length;
        }

        static string FormatNumber(double value)
        {
            if (value >= 1_000_000) return (value / 1_000_000).ToString("F1") + "M";
            if (value >= 1_000) return (value / 1_000).ToString("F1") + "K";
            return value.ToString("F0");
        }
    }
}
