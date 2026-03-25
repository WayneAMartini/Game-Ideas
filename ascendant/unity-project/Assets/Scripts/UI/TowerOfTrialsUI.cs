using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Events;

namespace Ascendant.UI
{
    public class TowerOfTrialsUI : MonoBehaviour
    {
        [Header("Entry Screen")]
        [SerializeField] GameObject _entryPanel;
        [SerializeField] TextMeshProUGUI _personalBestText;
        [SerializeField] TextMeshProUGUI _weeklyBestText;
        [SerializeField] TextMeshProUGUI _weeklyResetTimerText;
        [SerializeField] Button _enterButton;

        [Header("Floor Screen")]
        [SerializeField] GameObject _floorPanel;
        [SerializeField] TextMeshProUGUI _currentFloorText;
        [SerializeField] TextMeshProUGUI _modifierNameText;
        [SerializeField] TextMeshProUGUI _modifierDescText;
        [SerializeField] TextMeshProUGUI _enemyLevelText;
        [SerializeField] Button _leaveButton;

        [Header("Buff Selection")]
        [SerializeField] GameObject _buffPanel;
        [SerializeField] Button[] _buffButtons;
        [SerializeField] TextMeshProUGUI[] _buffNameTexts;
        [SerializeField] TextMeshProUGUI[] _buffDescTexts;

        [Header("Results Screen")]
        [SerializeField] GameObject _resultsPanel;
        [SerializeField] TextMeshProUGUI _resultTitleText;
        [SerializeField] TextMeshProUGUI _floorsReachedText;
        [SerializeField] TextMeshProUGUI _rewardsText;
        [SerializeField] Button _resultsCloseButton;

        TowerBuff[] _pendingBuffChoices;

        void OnEnable()
        {
            EventBus.Subscribe<TowerEnteredEvent>(OnTowerEntered);
            EventBus.Subscribe<TowerFloorStartedEvent>(OnFloorStarted);
            EventBus.Subscribe<TowerFloorClearedEvent>(OnFloorCleared);
            EventBus.Subscribe<TowerBuffChoiceEvent>(OnBuffChoice);
            EventBus.Subscribe<TowerCompletedEvent>(OnTowerCompleted);
            EventBus.Subscribe<TowerFailedEvent>(OnTowerFailed);

            if (_enterButton) _enterButton.onClick.AddListener(OnEnterClicked);
            if (_leaveButton) _leaveButton.onClick.AddListener(OnLeaveClicked);
            if (_resultsCloseButton) _resultsCloseButton.onClick.AddListener(OnResultsCloseClicked);

            for (int i = 0; i < _buffButtons.Length; i++)
            {
                int idx = i;
                if (_buffButtons[i]) _buffButtons[i].onClick.AddListener(() => OnBuffSelected(idx));
            }
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<TowerEnteredEvent>(OnTowerEntered);
            EventBus.Unsubscribe<TowerFloorStartedEvent>(OnFloorStarted);
            EventBus.Unsubscribe<TowerFloorClearedEvent>(OnFloorCleared);
            EventBus.Unsubscribe<TowerBuffChoiceEvent>(OnBuffChoice);
            EventBus.Unsubscribe<TowerCompletedEvent>(OnTowerCompleted);
            EventBus.Unsubscribe<TowerFailedEvent>(OnTowerFailed);
        }

        public void ShowEntryScreen()
        {
            HideAll();
            if (_entryPanel) _entryPanel.SetActive(true);
            RefreshEntryScreen();
        }

        void RefreshEntryScreen()
        {
            var tower = TowerOfTrials.Instance;
            if (tower == null) return;

            if (_personalBestText) _personalBestText.text = $"Personal Best: Floor {tower.PersonalBest}";
            if (_weeklyBestText) _weeklyBestText.text = $"Weekly Best: Floor {tower.WeeklyBest}";
        }

        void Update()
        {
            if (_entryPanel != null && _entryPanel.activeSelf)
            {
                var tower = TowerOfTrials.Instance;
                if (tower != null && _weeklyResetTimerText)
                {
                    long seconds = tower.GetTimeUntilReset();
                    int hours = (int)(seconds / 3600);
                    int mins = (int)((seconds % 3600) / 60);
                    _weeklyResetTimerText.text = $"Resets in: {hours}h {mins}m";
                }
            }
        }

        void OnEnterClicked()
        {
            TowerOfTrials.Instance?.EnterTower();
        }

        void OnLeaveClicked()
        {
            TowerOfTrials.Instance?.LeaveTower();
        }

        void OnResultsCloseClicked()
        {
            HideAll();
        }

        void OnTowerEntered(TowerEnteredEvent evt)
        {
            HideAll();
            if (_floorPanel) _floorPanel.SetActive(true);
        }

        void OnFloorStarted(TowerFloorStartedEvent evt)
        {
            HideAll();
            if (_floorPanel) _floorPanel.SetActive(true);

            if (_currentFloorText) _currentFloorText.text = $"Floor {evt.FloorNumber}";
            if (_modifierNameText) _modifierNameText.text = evt.ModifierName;
            if (_enemyLevelText) _enemyLevelText.text = $"Enemy Level: {evt.EnemyLevel}";

            var tower = TowerOfTrials.Instance;
            if (tower?.CurrentModifier != null && _modifierDescText)
                _modifierDescText.text = tower.CurrentModifier.description;
        }

        void OnFloorCleared(TowerFloorClearedEvent evt)
        {
            if (evt.IsMilestone)
            {
                Debug.Log($"[TowerUI] Milestone reached: Floor {evt.FloorNumber}!");
            }
        }

        void OnBuffChoice(TowerBuffChoiceEvent evt)
        {
            _pendingBuffChoices = evt.Choices;
            HideAll();
            if (_buffPanel) _buffPanel.SetActive(true);

            for (int i = 0; i < evt.Choices.Length && i < _buffButtons.Length; i++)
            {
                if (_buffNameTexts != null && i < _buffNameTexts.Length && _buffNameTexts[i])
                    _buffNameTexts[i].text = evt.Choices[i].buffName;
                if (_buffDescTexts != null && i < _buffDescTexts.Length && _buffDescTexts[i])
                    _buffDescTexts[i].text = evt.Choices[i].description;
            }
        }

        void OnBuffSelected(int index)
        {
            if (_pendingBuffChoices == null || index >= _pendingBuffChoices.Length) return;
            TowerOfTrials.Instance?.SelectBuff(_pendingBuffChoices[index]);
            _pendingBuffChoices = null;
        }

        void OnTowerCompleted(TowerCompletedEvent evt)
        {
            ShowResults("Tower Complete!", evt.FloorsCleared, evt.PersonalBest);
        }

        void OnTowerFailed(TowerFailedEvent evt)
        {
            ShowResults("Defeated!", evt.FloorsCleared, evt.PersonalBest);
        }

        void ShowResults(string title, int floors, int best)
        {
            HideAll();
            if (_resultsPanel) _resultsPanel.SetActive(true);
            if (_resultTitleText) _resultTitleText.text = title;
            if (_floorsReachedText) _floorsReachedText.text = $"Floors Cleared: {floors} / {TowerOfTrials.Instance?.MaxFloors ?? 50}";
            if (_rewardsText) _rewardsText.text = $"Personal Best: Floor {best}";
        }

        void HideAll()
        {
            if (_entryPanel) _entryPanel.SetActive(false);
            if (_floorPanel) _floorPanel.SetActive(false);
            if (_buffPanel) _buffPanel.SetActive(false);
            if (_resultsPanel) _resultsPanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (_enterButton) _enterButton.onClick.RemoveAllListeners();
            if (_leaveButton) _leaveButton.onClick.RemoveAllListeners();
            if (_resultsCloseButton) _resultsCloseButton.onClick.RemoveAllListeners();
        }
    }
}
