using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Economy;

namespace Ascendant.UI
{
    public class SummonUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] Button _singlePullButton;
        [SerializeField] Button _tenPullButton;

        [Header("Cost Labels")]
        [SerializeField] TextMeshProUGUI _singleCostText;
        [SerializeField] TextMeshProUGUI _tenCostText;

        [Header("Pity Display")]
        [SerializeField] TextMeshProUGUI _epicPityText;
        [SerializeField] TextMeshProUGUI _legendaryPityText;
        [SerializeField] TextMeshProUGUI _sparkText;

        [Header("Banner")]
        [SerializeField] Image _bannerImage;
        [SerializeField] TextMeshProUGUI _bannerNameText;
        [SerializeField] Button _nextBannerButton;
        [SerializeField] Button _prevBannerButton;

        [Header("Results Panel")]
        [SerializeField] GameObject _resultsPanel;
        [SerializeField] Transform _resultsContainer;
        [SerializeField] GameObject _resultCardPrefab;
        [SerializeField] Button _closeResultsButton;

        [Header("Animation")]
        [SerializeField] GameObject _portalEffect;
        [SerializeField] float _pullAnimDuration = 1.5f;

        [Header("Wishlist")]
        [SerializeField] Button _wishlistButton;
        [SerializeField] GameObject _wishlistPanel;

        void Start()
        {
            if (_singlePullButton != null)
                _singlePullButton.onClick.AddListener(OnSinglePull);
            if (_tenPullButton != null)
                _tenPullButton.onClick.AddListener(OnTenPull);
            if (_closeResultsButton != null)
                _closeResultsButton.onClick.AddListener(CloseResults);
            if (_nextBannerButton != null)
                _nextBannerButton.onClick.AddListener(OnNextBanner);
            if (_prevBannerButton != null)
                _prevBannerButton.onClick.AddListener(OnPrevBanner);
            if (_wishlistButton != null)
                _wishlistButton.onClick.AddListener(ToggleWishlist);

            if (_resultsPanel != null) _resultsPanel.SetActive(false);
            if (_wishlistPanel != null) _wishlistPanel.SetActive(false);

            UpdateCostLabels();
            UpdatePityDisplay();
            UpdateBannerDisplay();
        }

        void OnEnable()
        {
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<PityCounterChangedEvent>(OnPityChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<PityCounterChangedEvent>(OnPityChanged);
        }

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == CurrencyType.Stardust)
                UpdateCostLabels();
        }

        void OnPityChanged(PityCounterChangedEvent evt)
        {
            UpdatePityDisplay();
        }

        void OnSinglePull()
        {
            var gacha = GachaSystem.Instance;
            if (gacha == null || !gacha.CanAffordSinglePull()) return;

            var banner = BannerManager.Instance?.GetActiveBanner();
            var result = gacha.SinglePull(banner);
            if (result != null)
                StartCoroutine(ShowPullAnimation(new List<GachaPullResult> { result }));
        }

        void OnTenPull()
        {
            var gacha = GachaSystem.Instance;
            if (gacha == null || !gacha.CanAffordTenPull()) return;

            var banner = BannerManager.Instance?.GetActiveBanner();
            var results = gacha.TenPull(banner);
            if (results != null)
                StartCoroutine(ShowPullAnimation(results));
        }

        IEnumerator ShowPullAnimation(List<GachaPullResult> results)
        {
            // Disable buttons during animation
            SetButtonsInteractable(false);

            // Play portal animation
            if (_portalEffect != null)
                _portalEffect.SetActive(true);

            yield return new WaitForSeconds(_pullAnimDuration);

            if (_portalEffect != null)
                _portalEffect.SetActive(false);

            // Show results
            ShowResults(results);
            SetButtonsInteractable(true);
        }

        void ShowResults(List<GachaPullResult> results)
        {
            if (_resultsPanel == null || _resultsContainer == null) return;

            // Clear previous results
            foreach (Transform child in _resultsContainer)
                Destroy(child.gameObject);

            foreach (var result in results)
            {
                if (_resultCardPrefab != null)
                {
                    var card = Instantiate(_resultCardPrefab, _resultsContainer);
                    var cardUI = card.GetComponent<SummonResultCard>();
                    if (cardUI != null)
                        cardUI.Setup(result);
                }
            }

            _resultsPanel.SetActive(true);
        }

        void CloseResults()
        {
            if (_resultsPanel != null)
                _resultsPanel.SetActive(false);
        }

        void UpdateCostLabels()
        {
            if (_singleCostText != null)
                _singleCostText.text = $"{GachaSystem.SinglePullCost}";
            if (_tenCostText != null)
                _tenCostText.text = $"{GachaSystem.TenPullCost}";

            var gacha = GachaSystem.Instance;
            if (_singlePullButton != null)
                _singlePullButton.interactable = gacha != null && gacha.CanAffordSinglePull();
            if (_tenPullButton != null)
                _tenPullButton.interactable = gacha != null && gacha.CanAffordTenPull();
        }

        void UpdatePityDisplay()
        {
            var gacha = GachaSystem.Instance;
            if (gacha == null) return;

            if (_epicPityText != null)
                _epicPityText.text = $"Epic in {gacha.PullsUntilEpicPity} pulls";
            if (_legendaryPityText != null)
                _legendaryPityText.text = $"Legendary in {gacha.PullsUntilLegendaryPity} pulls";
            if (_sparkText != null)
                _sparkText.text = gacha.IsSparkReady
                    ? "SPARK READY! Choose any hero!"
                    : $"Spark: {gacha.SparkCounter}/{GachaSystem.SparkThreshold}";
        }

        void UpdateBannerDisplay()
        {
            var bannerMgr = BannerManager.Instance;
            if (bannerMgr == null) return;

            var banner = bannerMgr.GetActiveBanner();
            if (banner == null) return;

            if (_bannerNameText != null)
                _bannerNameText.text = banner.bannerName;
            if (_bannerImage != null && banner.bannerArt != null)
                _bannerImage.sprite = banner.bannerArt;
        }

        void OnNextBanner()
        {
            BannerManager.Instance?.NextBanner();
            UpdateBannerDisplay();
        }

        void OnPrevBanner()
        {
            BannerManager.Instance?.PreviousBanner();
            UpdateBannerDisplay();
        }

        void ToggleWishlist()
        {
            if (_wishlistPanel != null)
                _wishlistPanel.SetActive(!_wishlistPanel.activeSelf);
        }

        void SetButtonsInteractable(bool interactable)
        {
            if (_singlePullButton != null) _singlePullButton.interactable = interactable;
            if (_tenPullButton != null) _tenPullButton.interactable = interactable;
        }
    }

    public class SummonResultCard : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _heroNameText;
        [SerializeField] TextMeshProUGUI _rarityText;
        [SerializeField] Image _rarityBorder;
        [SerializeField] TextMeshProUGUI _duplicateText;

        public void Setup(GachaPullResult result)
        {
            if (_heroNameText != null)
                _heroNameText.text = result.HeroClassId;

            if (_rarityText != null)
                _rarityText.text = result.Rarity.ToString();

            if (_rarityBorder != null)
                _rarityBorder.color = GetRarityColor(result.Rarity);

            if (_duplicateText != null)
            {
                _duplicateText.gameObject.SetActive(result.IsDuplicate);
                if (result.IsDuplicate)
                    _duplicateText.text = $"+{result.StarFragmentsAwarded} Star Fragments";
            }
        }

        static Color GetRarityColor(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => Color.white,
                HeroRarity.Uncommon => Color.green,
                HeroRarity.Rare => new Color(0.2f, 0.5f, 1f),
                HeroRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                HeroRarity.Legendary => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };
        }
    }
}
