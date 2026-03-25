using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Events;

namespace Ascendant.UI
{
    public class VoidRiftUI : MonoBehaviour
    {
        [Header("Entry Screen")]
        [SerializeField] GameObject _entryPanel;
        [SerializeField] TextMeshProUGUI _riftNameText;
        [SerializeField] TextMeshProUGUI _riftDescriptionText;
        [SerializeField] TextMeshProUGUI _riftTimerText;
        [SerializeField] TextMeshProUGUI _attemptsText;
        [SerializeField] TextMeshProUGUI _difficultyText;
        [SerializeField] TextMeshProUGUI _rewardsPreviewText;

        [Header("Stage Select")]
        [SerializeField] Button[] _stageButtons;
        [SerializeField] TextMeshProUGUI[] _stageLabels;
        [SerializeField] Image[] _stageStatusIcons;

        [Header("Combat Screen")]
        [SerializeField] GameObject _combatPanel;
        [SerializeField] TextMeshProUGUI _currentStageText;
        [SerializeField] TextMeshProUGUI _modifierText;

        [Header("Results Screen")]
        [SerializeField] GameObject _resultsPanel;
        [SerializeField] TextMeshProUGUI _resultTitleText;
        [SerializeField] TextMeshProUGUI _resultRewardsText;
        [SerializeField] TextMeshProUGUI _resultAttemptsText;
        [SerializeField] Button _resultsCloseButton;

        [Header("Schedule")]
        [SerializeField] GameObject _schedulePanel;
        [SerializeField] TextMeshProUGUI _nextRiftText;

        void OnEnable()
        {
            EventBus.Subscribe<VoidRiftStartedEvent>(OnRiftStarted);
            EventBus.Subscribe<VoidRiftStageStartedEvent>(OnStageStarted);
            EventBus.Subscribe<VoidRiftStageClearedEvent>(OnStageCleared);
            EventBus.Subscribe<VoidRiftStageFailedEvent>(OnStageFailed);
            EventBus.Subscribe<VoidRiftCompletedEvent>(OnRiftCompleted);

            if (_resultsCloseButton) _resultsCloseButton.onClick.AddListener(OnResultsClose);

            for (int i = 0; i < _stageButtons.Length; i++)
            {
                int stage = i;
                if (_stageButtons[i]) _stageButtons[i].onClick.AddListener(() => OnStageClicked(stage));
            }
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<VoidRiftStartedEvent>(OnRiftStarted);
            EventBus.Unsubscribe<VoidRiftStageStartedEvent>(OnStageStarted);
            EventBus.Unsubscribe<VoidRiftStageClearedEvent>(OnStageCleared);
            EventBus.Unsubscribe<VoidRiftStageFailedEvent>(OnStageFailed);
            EventBus.Unsubscribe<VoidRiftCompletedEvent>(OnRiftCompleted);
        }

        public void ShowRiftScreen()
        {
            HideAll();
            if (_entryPanel) _entryPanel.SetActive(true);
            RefreshRiftInfo();
        }

        void RefreshRiftInfo()
        {
            var rift = VoidRiftManager.Instance;
            if (rift == null) return;

            var config = rift.CurrentRift;
            if (config == null) return;

            if (_riftNameText) _riftNameText.text = config.riftName;
            if (_riftDescriptionText) _riftDescriptionText.text = config.description;
            if (_attemptsText) _attemptsText.text = $"Attempts: {rift.AttemptsRemaining}/{config.maxAttempts}";
            if (_difficultyText) _difficultyText.text = "Endgame Difficulty";
            if (_rewardsPreviewText) _rewardsPreviewText.text = $"Aether Crystals: {config.aetherCrystalsPerStage}/stage\nMythic Chance: {config.mythicEquipmentChance * 100:F0}%";

            RefreshStageButtons();
        }

        void RefreshStageButtons()
        {
            var rift = VoidRiftManager.Instance;
            if (rift == null) return;

            int stageCount = rift.CurrentRift?.stageCount ?? 5;
            for (int i = 0; i < _stageButtons.Length; i++)
            {
                if (i >= stageCount)
                {
                    if (_stageButtons[i]) _stageButtons[i].gameObject.SetActive(false);
                    continue;
                }

                if (_stageButtons[i]) _stageButtons[i].gameObject.SetActive(true);
                if (_stageLabels != null && i < _stageLabels.Length && _stageLabels[i])
                    _stageLabels[i].text = $"Stage {i + 1}";

                bool completed = rift.CompletedStages.Contains(i);
                bool canAttempt = rift.CanAttemptStage(i);

                if (_stageButtons[i]) _stageButtons[i].interactable = canAttempt && !completed;
            }
        }

        void Update()
        {
            if (_entryPanel != null && _entryPanel.activeSelf)
            {
                var rift = VoidRiftManager.Instance;
                if (rift != null && _riftTimerText)
                {
                    long seconds = rift.GetTimeRemaining();
                    int hours = (int)(seconds / 3600);
                    int mins = (int)((seconds % 3600) / 60);
                    _riftTimerText.text = $"Time Left: {hours}h {mins}m";
                }
            }
        }

        void OnStageClicked(int stage)
        {
            VoidRiftManager.Instance?.StartStage(stage);
        }

        void OnResultsClose()
        {
            HideAll();
            ShowRiftScreen();
        }

        void OnRiftStarted(VoidRiftStartedEvent evt)
        {
            RefreshRiftInfo();
        }

        void OnStageStarted(VoidRiftStageStartedEvent evt)
        {
            HideAll();
            if (_combatPanel) _combatPanel.SetActive(true);
            if (_currentStageText) _currentStageText.text = $"Stage {evt.StageNumber + 1}";

            var modifier = VoidRiftManager.Instance?.GetStageModifier(evt.StageNumber);
            if (_modifierText) _modifierText.text = modifier != null ? modifier.modifierName : "None";
        }

        void OnStageCleared(VoidRiftStageClearedEvent evt)
        {
            HideAll();
            if (_resultsPanel) _resultsPanel.SetActive(true);
            if (_resultTitleText) _resultTitleText.text = evt.AllStagesCleared ? "Rift Complete!" : "Stage Cleared!";
            if (_resultRewardsText) _resultRewardsText.text = $"Aether Crystals: +{evt.AetherCrystalsEarned}";
            if (_resultAttemptsText) _resultAttemptsText.text = $"Attempts: {VoidRiftManager.Instance?.AttemptsRemaining ?? 0}";
        }

        void OnStageFailed(VoidRiftStageFailedEvent evt)
        {
            HideAll();
            if (_resultsPanel) _resultsPanel.SetActive(true);
            if (_resultTitleText) _resultTitleText.text = "Stage Failed";
            if (_resultRewardsText) _resultRewardsText.text = "No rewards";
            if (_resultAttemptsText) _resultAttemptsText.text = $"Attempts remaining: {evt.AttemptsRemaining}";
        }

        void OnRiftCompleted(VoidRiftCompletedEvent evt)
        {
            HideAll();
            if (_resultsPanel) _resultsPanel.SetActive(true);
            if (_resultTitleText) _resultTitleText.text = "Void Rift Conquered!";
            if (_resultRewardsText) _resultRewardsText.text = $"Total Aether Crystals: {evt.TotalAetherCrystals}";
        }

        void HideAll()
        {
            if (_entryPanel) _entryPanel.SetActive(false);
            if (_combatPanel) _combatPanel.SetActive(false);
            if (_resultsPanel) _resultsPanel.SetActive(false);
            if (_schedulePanel) _schedulePanel.SetActive(false);
        }
    }
}
