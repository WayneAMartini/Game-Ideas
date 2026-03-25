using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Economy;

namespace Ascendant.UI
{
    public class BattlePassUI : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] TextMeshProUGUI _tierText;
        [SerializeField] Slider _xpProgressBar;
        [SerializeField] TextMeshProUGUI _xpText;
        [SerializeField] TextMeshProUGUI _seasonTimerText;

        [Header("Tier List")]
        [SerializeField] ScrollRect _tierScrollRect;
        [SerializeField] Transform _tierContainer;
        [SerializeField] GameObject _tierCardPrefab;

        [Header("Premium")]
        [SerializeField] Button _upgradePremiumButton;
        [SerializeField] GameObject _premiumBadge;

        [Header("Quests Panel")]
        [SerializeField] GameObject _questsPanel;
        [SerializeField] Button _questsToggleButton;

        void Start()
        {
            if (_upgradePremiumButton != null)
                _upgradePremiumButton.onClick.AddListener(OnUpgradePremium);
            if (_questsToggleButton != null)
                _questsToggleButton.onClick.AddListener(ToggleQuests);

            if (_questsPanel != null)
                _questsPanel.SetActive(false);

            RefreshAll();
        }

        void OnEnable()
        {
            EventBus.Subscribe<BattlePassXPGainedEvent>(OnXPGained);
            EventBus.Subscribe<BattlePassTierClaimedEvent>(OnTierClaimed);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<BattlePassXPGainedEvent>(OnXPGained);
            EventBus.Unsubscribe<BattlePassTierClaimedEvent>(OnTierClaimed);
        }

        void OnXPGained(BattlePassXPGainedEvent evt)
        {
            RefreshProgress();
        }

        void OnTierClaimed(BattlePassTierClaimedEvent evt)
        {
            RefreshTierList();
        }

        void RefreshAll()
        {
            RefreshProgress();
            RefreshTierList();
            RefreshPremiumState();
            RefreshSeasonTimer();
        }

        void RefreshProgress()
        {
            var bp = BattlePassSystem.Instance;
            if (bp == null) return;

            if (_tierText != null)
                _tierText.text = $"Tier {bp.CurrentTier}/{bp.MaxTiers}";

            if (_xpProgressBar != null)
            {
                int needed = bp.XPToNextTier;
                _xpProgressBar.value = needed > 0 ? (float)bp.CurrentXP / needed : 1f;
            }

            if (_xpText != null)
                _xpText.text = $"{bp.CurrentXP}/{bp.XPToNextTier} XP";
        }

        void RefreshTierList()
        {
            if (_tierContainer == null || _tierCardPrefab == null) return;

            // Clear existing tier cards
            foreach (Transform child in _tierContainer)
                Destroy(child.gameObject);

            var bp = BattlePassSystem.Instance;
            if (bp == null) return;

            int totalTiers = bp.MaxTiers;
            for (int i = 0; i < totalTiers; i++)
            {
                var card = Instantiate(_tierCardPrefab, _tierContainer);
                var cardUI = card.GetComponent<BattlePassTierCard>();
                if (cardUI != null)
                    cardUI.Setup(i, bp);
            }
        }

        void RefreshPremiumState()
        {
            var bp = BattlePassSystem.Instance;
            if (bp == null) return;

            if (_premiumBadge != null)
                _premiumBadge.SetActive(bp.IsPremium);
            if (_upgradePremiumButton != null)
                _upgradePremiumButton.gameObject.SetActive(!bp.IsPremium);
        }

        void RefreshSeasonTimer()
        {
            var bp = BattlePassSystem.Instance;
            if (bp?.CurrentPassData == null) return;

            var remaining = bp.CurrentPassData.GetTimeRemaining();
            if (_seasonTimerText != null)
            {
                if (remaining.TotalDays >= 1)
                    _seasonTimerText.text = $"{(int)remaining.TotalDays}d {remaining.Hours}h remaining";
                else
                    _seasonTimerText.text = $"{remaining.Hours}h {remaining.Minutes}m remaining";
            }
        }

        void OnUpgradePremium()
        {
            IAPManager.Instance?.Purchase("battle_pass_premium");
            RefreshPremiumState();
        }

        void ToggleQuests()
        {
            if (_questsPanel != null)
                _questsPanel.SetActive(!_questsPanel.activeSelf);
        }

        void Update()
        {
            // Update season timer every second
            RefreshSeasonTimer();
        }
    }

    public class BattlePassTierCard : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _tierNumberText;
        [SerializeField] TextMeshProUGUI _freeRewardText;
        [SerializeField] TextMeshProUGUI _premiumRewardText;
        [SerializeField] Button _claimFreeButton;
        [SerializeField] Button _claimPremiumButton;
        [SerializeField] Image _background;
        [SerializeField] GameObject _lockedOverlay;

        int _tier;

        public void Setup(int tier, BattlePassSystem bp)
        {
            _tier = tier;

            if (_tierNumberText != null)
                _tierNumberText.text = $"{tier + 1}";

            var passData = bp.CurrentPassData;
            if (passData != null)
            {
                var freeReward = passData.GetFreeReward(tier);
                if (_freeRewardText != null)
                    _freeRewardText.text = freeReward?.description ?? "";

                var premiumReward = passData.GetPremiumReward(tier);
                if (_premiumRewardText != null)
                    _premiumRewardText.text = premiumReward?.description ?? "";
            }

            // Highlight current tier
            bool isReached = tier <= bp.CurrentTier;
            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(!isReached);

            // Claim buttons
            if (_claimFreeButton != null)
            {
                _claimFreeButton.gameObject.SetActive(bp.CanClaimFreeReward(tier));
                _claimFreeButton.onClick.AddListener(() => bp.ClaimFreeReward(_tier));
            }

            if (_claimPremiumButton != null)
            {
                _claimPremiumButton.gameObject.SetActive(bp.CanClaimPremiumReward(tier));
                _claimPremiumButton.onClick.AddListener(() => bp.ClaimPremiumReward(_tier));
            }

            if (_background != null)
            {
                if (tier == bp.CurrentTier)
                    _background.color = new Color(1f, 0.84f, 0f, 0.3f);
                else if (isReached)
                    _background.color = new Color(1f, 1f, 1f, 0.1f);
            }
        }
    }
}
