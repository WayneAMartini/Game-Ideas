using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    public class BannerManager : MonoBehaviour
    {
        public static BannerManager Instance { get; private set; }

        [SerializeField] List<BannerData> _allBanners = new();

        int _currentBannerIndex;
        readonly List<BannerData> _activeBanners = new();

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
            RefreshActiveBanners();
        }

        public void RefreshActiveBanners()
        {
            _activeBanners.Clear();
            foreach (var banner in _allBanners)
            {
                if (banner != null && banner.IsActive())
                    _activeBanners.Add(banner);
            }

            if (_currentBannerIndex >= _activeBanners.Count)
                _currentBannerIndex = 0;
        }

        public BannerData GetActiveBanner()
        {
            if (_activeBanners.Count == 0) return null;
            return _activeBanners[_currentBannerIndex];
        }

        public List<BannerData> GetAllActiveBanners()
        {
            return new List<BannerData>(_activeBanners);
        }

        public void NextBanner()
        {
            if (_activeBanners.Count <= 1) return;
            _currentBannerIndex = (_currentBannerIndex + 1) % _activeBanners.Count;
            PublishBannerChanged();
        }

        public void PreviousBanner()
        {
            if (_activeBanners.Count <= 1) return;
            _currentBannerIndex = (_currentBannerIndex - 1 + _activeBanners.Count) % _activeBanners.Count;
            PublishBannerChanged();
        }

        public void SelectBanner(int index)
        {
            if (index < 0 || index >= _activeBanners.Count) return;
            _currentBannerIndex = index;
            PublishBannerChanged();
        }

        void PublishBannerChanged()
        {
            var banner = GetActiveBanner();
            if (banner != null)
            {
                EventBus.Publish(new BannerChangedEvent
                {
                    BannerId = banner.bannerId,
                    BannerName = banner.bannerName
                });
            }
        }

        public void AddBanner(BannerData banner)
        {
            if (banner != null && !_allBanners.Contains(banner))
                _allBanners.Add(banner);
            RefreshActiveBanners();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
