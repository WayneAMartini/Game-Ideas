using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class AscensionUI : MonoBehaviour
    {
        public static AscensionUI Instance { get; private set; }

        [Header("Confirmation Panel")]
        [SerializeField] GameObject _confirmationPanel;
        [SerializeField] TextMeshProUGUI _heroNameText;
        [SerializeField] TextMeshProUGUI _currentTierText;
        [SerializeField] TextMeshProUGUI _newTierText;
        [SerializeField] TextMeshProUGUI _shardsPreviewText;
        [SerializeField] TextMeshProUGUI _highestIslandText;
        [SerializeField] TextMeshProUGUI _ascensionCountText;

        [Header("Reset/Keep Lists")]
        [SerializeField] TextMeshProUGUI _resetsText;
        [SerializeField] TextMeshProUGUI _keepsText;

        [Header("Buttons")]
        [SerializeField] Button _ascendButton;
        [SerializeField] Button _cancelButton;

        [Header("Summary Panel")]
        [SerializeField] GameObject _summaryPanel;
        [SerializeField] TextMeshProUGUI _summaryShardsText;
        [SerializeField] TextMeshProUGUI _summaryTierText;
        [SerializeField] TextMeshProUGUI _summaryPromotionText;
        [SerializeField] Button _summaryCloseButton;

        [Header("Ascend Button (Hero Detail Screen)")]
        [SerializeField] GameObject _ascendButtonContainer;
        [SerializeField] Button _heroDetailAscendButton;

        int _pendingHeroSlot = -1;
        AscensionPreview _currentPreview;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (_ascendButton != null)
                _ascendButton.onClick.AddListener(OnConfirmAscension);
            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelAscension);
            if (_summaryCloseButton != null)
                _summaryCloseButton.onClick.AddListener(CloseSummary);
            if (_heroDetailAscendButton != null)
                _heroDetailAscendButton.onClick.AddListener(OnHeroDetailAscendClicked);

            HideAll();
        }

        void OnEnable()
        {
            EventBus.Subscribe<AscensionEvent>(OnAscensionCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<AscensionEvent>(OnAscensionCompleted);
        }

        public void ShowAscendButton(int heroSlot, bool canAscend)
        {
            if (_ascendButtonContainer != null)
                _ascendButtonContainer.SetActive(canAscend);

            _pendingHeroSlot = heroSlot;
        }

        void OnHeroDetailAscendClicked()
        {
            if (_pendingHeroSlot < 0) return;
            ShowConfirmation(_pendingHeroSlot);
        }

        public void ShowConfirmation(int heroSlot)
        {
            var ascSystem = AscensionSystem.Instance;
            if (ascSystem == null || !ascSystem.CanAscend(heroSlot)) return;

            _currentPreview = ascSystem.GetAscensionPreview(heroSlot);
            _pendingHeroSlot = heroSlot;

            if (_heroNameText != null)
                _heroNameText.text = _currentPreview.HeroName;
            if (_currentTierText != null)
                _currentTierText.text = $"Current: {AscensionTierData.GetTierInfo(_currentPreview.CurrentTier).DisplayName}";
            if (_newTierText != null)
            {
                if (_currentPreview.WillPromote)
                    _newTierText.text = $"NEW TIER: {AscensionTierData.GetTierInfo(_currentPreview.NewTier).DisplayName}!";
                else
                    _newTierText.text = $"Next: {AscensionTierData.GetTierInfo(_currentPreview.NewTier).DisplayName}";
            }
            if (_shardsPreviewText != null)
                _shardsPreviewText.text = $"+{_currentPreview.ShardsToEarn:N0} Ascension Shards";
            if (_highestIslandText != null)
                _highestIslandText.text = $"Highest Island: {_currentPreview.HighestIsland}";
            if (_ascensionCountText != null)
                _ascensionCountText.text = $"Ascension #{_currentPreview.CurrentAscensions + 1}";

            if (_resetsText != null)
                _resetsText.text = "RESETS:\n- Hero Level (back to 1)\n- Island Progress (back to Island 1)\n- Skill Tree (free respec)";
            if (_keepsText != null)
                _keepsText.text = "KEEPS:\n- All Equipment\n- Class Mastery Progress\n- Pantheon Progress\n- Ascension Skill Tree";

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(true);
            if (_summaryPanel != null)
                _summaryPanel.SetActive(false);
        }

        void OnConfirmAscension()
        {
            if (_pendingHeroSlot < 0) return;

            // Start cinematic, then perform ascension
            var cinematic = AscensionCinematic.Instance;
            if (cinematic != null)
            {
                if (_confirmationPanel != null)
                    _confirmationPanel.SetActive(false);
                cinematic.PlayAscensionSequence(_pendingHeroSlot, OnCinematicComplete);
            }
            else
            {
                // No cinematic — just ascend
                PerformAscension();
            }
        }

        void OnCinematicComplete()
        {
            PerformAscension();
        }

        void PerformAscension()
        {
            AscensionSystem.Instance?.PerformAscension(_pendingHeroSlot);
        }

        void OnAscensionCompleted(AscensionEvent evt)
        {
            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);

            ShowSummary(evt);
        }

        void ShowSummary(AscensionEvent evt)
        {
            if (_summaryShardsText != null)
                _summaryShardsText.text = $"+{evt.ShardsEarned:N0} Ascension Shards";

            var tier = AscensionTierData.GetTierForAscensions(evt.AscensionCount);
            var tierInfo = AscensionTierData.GetTierInfo(tier);

            if (_summaryTierText != null)
                _summaryTierText.text = $"Tier: {tierInfo.DisplayName}";

            if (_summaryPromotionText != null)
            {
                var prevTier = AscensionTierData.GetTierForAscensions(evt.AscensionCount - 1);
                if (tier != prevTier)
                    _summaryPromotionText.text = $"PROMOTED to {tierInfo.DisplayName}!\n{tierInfo.Description}";
                else
                    _summaryPromotionText.text = "";
            }

            if (_summaryPanel != null)
                _summaryPanel.SetActive(true);
        }

        void OnCancelAscension()
        {
            HideAll();
            _pendingHeroSlot = -1;
            GameManager.Instance?.SetState(GameState.Combat);
        }

        void CloseSummary()
        {
            HideAll();
            _pendingHeroSlot = -1;
            GameManager.Instance?.SetState(GameState.Combat);
        }

        void HideAll()
        {
            if (_confirmationPanel != null) _confirmationPanel.SetActive(false);
            if (_summaryPanel != null) _summaryPanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
