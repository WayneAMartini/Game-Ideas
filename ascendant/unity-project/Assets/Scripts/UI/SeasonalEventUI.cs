using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Events;

namespace Ascendant.UI
{
    public class SeasonalEventUI : MonoBehaviour
    {
        [Header("Event Hub")]
        [SerializeField] GameObject _hubPanel;
        [SerializeField] TextMeshProUGUI _eventNameText;
        [SerializeField] TextMeshProUGUI _eventDescText;
        [SerializeField] TextMeshProUGUI _eventTimerText;
        [SerializeField] TextMeshProUGUI _eventCurrencyText;
        [SerializeField] TextMeshProUGUI _eventProgressText;
        [SerializeField] Button _eventIslandButton;
        [SerializeField] Button _eventShopButton;
        [SerializeField] Button _closeButton;

        [Header("Event Banner (Main Screen)")]
        [SerializeField] GameObject _mainScreenBanner;
        [SerializeField] TextMeshProUGUI _bannerText;
        [SerializeField] Button _bannerButton;

        [Header("Quest List")]
        [SerializeField] GameObject _questPanel;
        [SerializeField] Transform _questListContent;
        [SerializeField] GameObject _questItemPrefab;

        [Header("Post-Event Summary")]
        [SerializeField] GameObject _summaryPanel;
        [SerializeField] TextMeshProUGUI _summaryTitleText;
        [SerializeField] TextMeshProUGUI _summaryRewardsText;
        [SerializeField] Button _summaryCloseButton;

        void OnEnable()
        {
            EventBus.Subscribe<SeasonalEventStartedEvent>(OnEventStarted);
            EventBus.Subscribe<SeasonalEventEndedEvent>(OnEventEnded);
            EventBus.Subscribe<SeasonalEventStageCompletedEvent>(OnStageCompleted);
            EventBus.Subscribe<EventQuestCompletedEvent>(OnQuestCompleted);

            if (_eventIslandButton) _eventIslandButton.onClick.AddListener(OnIslandClicked);
            if (_eventShopButton) _eventShopButton.onClick.AddListener(OnShopClicked);
            if (_closeButton) _closeButton.onClick.AddListener(OnCloseClicked);
            if (_bannerButton) _bannerButton.onClick.AddListener(OnBannerClicked);
            if (_summaryCloseButton) _summaryCloseButton.onClick.AddListener(OnSummaryClose);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<SeasonalEventStartedEvent>(OnEventStarted);
            EventBus.Unsubscribe<SeasonalEventEndedEvent>(OnEventEnded);
            EventBus.Unsubscribe<SeasonalEventStageCompletedEvent>(OnStageCompleted);
            EventBus.Unsubscribe<EventQuestCompletedEvent>(OnQuestCompleted);
        }

        void Start()
        {
            UpdateBanner();
        }

        void Update()
        {
            if (_hubPanel != null && _hubPanel.activeSelf)
                RefreshHub();

            if (_eventTimerText && SeasonalEventManager.Instance != null)
            {
                long seconds = SeasonalEventManager.Instance.GetTimeRemaining();
                if (seconds > 0)
                {
                    int days = (int)(seconds / 86400);
                    int hours = (int)((seconds % 86400) / 3600);
                    _eventTimerText.text = days > 0 ? $"{days}d {hours}h" : $"{hours}h";
                }
            }
        }

        public void ShowEventHub()
        {
            HideAll();
            if (_hubPanel) _hubPanel.SetActive(true);
            RefreshHub();
        }

        void RefreshHub()
        {
            var mgr = SeasonalEventManager.Instance;
            if (mgr == null || !mgr.IsEventActive) return;

            var config = mgr.ActiveEvent;
            if (config == null) return;

            if (_eventNameText) _eventNameText.text = config.eventName;
            if (_eventDescText) _eventDescText.text = config.description;
            if (_eventCurrencyText) _eventCurrencyText.text = $"{config.eventCurrencyName}: {mgr.EventCurrency}";
            if (_eventProgressText)
                _eventProgressText.text = $"Stage {mgr.EventStageProgress}/{config.eventStageCount}";
        }

        void UpdateBanner()
        {
            var mgr = SeasonalEventManager.Instance;
            bool active = mgr != null && mgr.IsEventActive;

            if (_mainScreenBanner) _mainScreenBanner.SetActive(active);
            if (active && _bannerText)
                _bannerText.text = mgr.ActiveEvent?.eventName ?? "Event Active!";
        }

        void OnIslandClicked()
        {
            SeasonalEventManager.Instance?.EnterEventIsland();
        }

        void OnShopClicked()
        {
            // EventShopUI handles its own display
            var shopUI = FindFirstObjectByType<EventShopUI>();
            if (shopUI != null) shopUI.ShowShop();
        }

        void OnCloseClicked()
        {
            HideAll();
        }

        void OnBannerClicked()
        {
            ShowEventHub();
        }

        void OnSummaryClose()
        {
            if (_summaryPanel) _summaryPanel.SetActive(false);
        }

        void OnEventStarted(SeasonalEventStartedEvent evt)
        {
            UpdateBanner();
        }

        void OnEventEnded(SeasonalEventEndedEvent evt)
        {
            UpdateBanner();
            HideAll();
            if (_summaryPanel) _summaryPanel.SetActive(true);
            if (_summaryTitleText) _summaryTitleText.text = $"{evt.EventName} Complete!";
            if (_summaryRewardsText)
                _summaryRewardsText.text = $"Total Currency: {evt.TotalCurrencyEarned}\nStages: {evt.StagesCompleted}";
        }

        void OnStageCompleted(SeasonalEventStageCompletedEvent evt)
        {
            RefreshHub();
        }

        void OnQuestCompleted(EventQuestCompletedEvent evt)
        {
            Debug.Log($"[SeasonalEventUI] Quest completed: {evt.QuestName} (+{evt.CurrencyReward} event currency)");
        }

        void HideAll()
        {
            if (_hubPanel) _hubPanel.SetActive(false);
            if (_questPanel) _questPanel.SetActive(false);
            if (_summaryPanel) _summaryPanel.SetActive(false);
        }
    }
}
