using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class GachaSaveData
    {
        public int EpicPityCounter;
        public int LegendaryPityCounter;
        public int TotalPullCount;
        public int SparkCounter;
        public List<string> Wishlist = new();
        public List<string> OwnedHeroes = new();
    }

    [Serializable]
    public class GachaPullResult
    {
        public string HeroClassId;
        public HeroRarity Rarity;
        public bool IsDuplicate;
        public int StarFragmentsAwarded;
    }

    public class GachaSystem : MonoBehaviour
    {
        public static GachaSystem Instance { get; private set; }

        public const int SinglePullCost = 300;
        public const int TenPullCost = 2700;
        public const int EpicPityThreshold = 30;
        public const int LegendaryPityThreshold = 90;
        public const int SparkThreshold = 200;
        public const int MaxWishlistSize = 5;

        // Duplicate -> Star Fragment conversion
        const int DuplicateUncommonFragments = 5;
        const int DuplicateRareFragments = 15;
        const int DuplicateEpicFragments = 40;
        const int DuplicateLegendaryFragments = 100;

        int _epicPityCounter;
        int _legendaryPityCounter;
        int _totalPullCount;
        int _sparkCounter;
        readonly List<string> _wishlist = new();
        readonly HashSet<string> _ownedHeroes = new();

        public int EpicPityCounter => _epicPityCounter;
        public int LegendaryPityCounter => _legendaryPityCounter;
        public int TotalPullCount => _totalPullCount;
        public int SparkCounter => _sparkCounter;
        public IReadOnlyList<string> Wishlist => _wishlist;
        public int PullsUntilEpicPity => Mathf.Max(0, EpicPityThreshold - _epicPityCounter);
        public int PullsUntilLegendaryPity => Mathf.Max(0, LegendaryPityThreshold - _legendaryPityCounter);
        public int PullsUntilSpark => Mathf.Max(0, SparkThreshold - _sparkCounter);
        public bool IsSparkReady => _sparkCounter >= SparkThreshold;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool CanAffordSinglePull()
        {
            return CurrencyManager.Instance != null &&
                   CurrencyManager.Instance.CanAfford(CurrencyType.Stardust, SinglePullCost);
        }

        public bool CanAffordTenPull()
        {
            return CurrencyManager.Instance != null &&
                   CurrencyManager.Instance.CanAfford(CurrencyType.Stardust, TenPullCost);
        }

        public GachaPullResult SinglePull(BannerData banner = null)
        {
            if (!CanAffordSinglePull()) return null;

            CurrencyManager.Instance.SpendCurrency(CurrencyType.Stardust, SinglePullCost);
            var result = ExecutePull(banner);
            PublishPullResult(result);
            return result;
        }

        public List<GachaPullResult> TenPull(BannerData banner = null)
        {
            if (!CanAffordTenPull()) return null;

            CurrencyManager.Instance.SpendCurrency(CurrencyType.Stardust, TenPullCost);

            var results = new List<GachaPullResult>(10);
            bool hasRareOrAbove = false;

            for (int i = 0; i < 10; i++)
            {
                var result = ExecutePull(banner);
                results.Add(result);
                if (result.Rarity >= HeroRarity.Rare)
                    hasRareOrAbove = true;
            }

            // Guarantee at least 1 Rare+ in 10-pull
            if (!hasRareOrAbove)
            {
                // Upgrade the last pull to Rare
                var last = results[9];
                last.Rarity = HeroRarity.Rare;
                last.HeroClassId = GetRandomHeroForRarity(HeroRarity.Rare, banner);
                last.IsDuplicate = _ownedHeroes.Contains(last.HeroClassId);
                if (last.IsDuplicate)
                    last.StarFragmentsAwarded = DuplicateRareFragments;
            }

            foreach (var r in results)
                PublishPullResult(r);

            EventBus.Publish(new GachaMultiPullEvent { PullCount = 10 });
            return results;
        }

        GachaPullResult ExecutePull(BannerData banner)
        {
            _epicPityCounter++;
            _legendaryPityCounter++;
            _totalPullCount++;
            _sparkCounter++;

            var rarity = RollRarity(banner);
            string heroClassId = GetRandomHeroForRarity(rarity, banner);
            bool isDuplicate = _ownedHeroes.Contains(heroClassId);
            int fragments = 0;

            if (isDuplicate)
            {
                fragments = GetFragmentsForRarity(rarity);
                CurrencyManager.Instance?.AddCurrency(CurrencyType.StarFragments, fragments);
            }
            else
            {
                _ownedHeroes.Add(heroClassId);
            }

            // Reset pity counters on hit
            if (rarity >= HeroRarity.Epic)
                _epicPityCounter = 0;
            if (rarity >= HeroRarity.Legendary)
                _legendaryPityCounter = 0;

            // Publish pity counter changes
            EventBus.Publish(new PityCounterChangedEvent
            {
                EpicPity = _epicPityCounter,
                LegendaryPity = _legendaryPityCounter
            });

            // Check spark
            if (_sparkCounter >= SparkThreshold)
            {
                EventBus.Publish(new SparkReadyEvent { TotalPulls = _totalPullCount });
            }

            return new GachaPullResult
            {
                HeroClassId = heroClassId,
                Rarity = rarity,
                IsDuplicate = isDuplicate,
                StarFragmentsAwarded = fragments
            };
        }

        HeroRarity RollRarity(BannerData banner)
        {
            // Pity overrides
            if (_legendaryPityCounter >= LegendaryPityThreshold)
                return HeroRarity.Legendary;
            if (_epicPityCounter >= EpicPityThreshold)
                return HeroRarity.Epic;

            // Base rates: Uncommon 60%, Rare 30%, Epic 8%, Legendary 2%
            float legendaryRate = 0.02f;
            float epicRate = 0.08f;
            float rareRate = 0.30f;

            // Banner rate boosts
            if (banner != null)
            {
                legendaryRate *= banner.legendaryRateMultiplier;
                epicRate *= banner.epicRateMultiplier;
            }

            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            cumulative += legendaryRate;
            if (roll < cumulative) return HeroRarity.Legendary;

            cumulative += epicRate;
            if (roll < cumulative) return HeroRarity.Epic;

            cumulative += rareRate;
            if (roll < cumulative) return HeroRarity.Rare;

            return HeroRarity.Uncommon;
        }

        string GetRandomHeroForRarity(HeroRarity rarity, BannerData banner)
        {
            // Check banner featured heroes first
            if (banner != null && banner.featuredHeroes != null && banner.featuredHeroes.Count > 0)
            {
                // 50% chance to get a featured hero when pulling at featured rarity
                if (rarity == banner.featuredRarity && UnityEngine.Random.value < 0.5f)
                {
                    return banner.featuredHeroes[UnityEngine.Random.Range(0, banner.featuredHeroes.Count)];
                }
            }

            // Check wishlist (2x boost = select from wishlist ~66% of the time)
            if (_wishlist.Count > 0 && UnityEngine.Random.value < 0.66f)
            {
                return _wishlist[UnityEngine.Random.Range(0, _wishlist.Count)];
            }

            // Fallback: pick from all available heroes for this rarity
            // Use the full hero class pool
            return GetRandomHeroClassId();
        }

        static readonly string[] AllHeroClasses =
        {
            "warrior", "mage", "priest", "rogue",
            "ranger", "spellblade", "warlock", "necromancer",
            "dragonhunter", "monk", "paladin", "bard",
            "chronomancer", "gunslinger", "reaper",
            "defender", "berserker", "druid", "thief",
            "shaman", "alchemist", "warden", "summoner", "marksman"
        };

        static string GetRandomHeroClassId()
        {
            return AllHeroClasses[UnityEngine.Random.Range(0, AllHeroClasses.Length)];
        }

        static int GetFragmentsForRarity(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Uncommon => DuplicateUncommonFragments,
                HeroRarity.Rare => DuplicateRareFragments,
                HeroRarity.Epic => DuplicateEpicFragments,
                HeroRarity.Legendary => DuplicateLegendaryFragments,
                _ => DuplicateUncommonFragments
            };
        }

        void PublishPullResult(GachaPullResult result)
        {
            EventBus.Publish(new GachaPullEvent
            {
                HeroClassId = result.HeroClassId,
                Rarity = result.Rarity,
                IsDuplicate = result.IsDuplicate,
                StarFragmentsAwarded = result.StarFragmentsAwarded
            });
        }

        // Spark: choose any hero for free after 200 pulls
        public bool SparkSelect(string heroClassId)
        {
            if (_sparkCounter < SparkThreshold) return false;

            _sparkCounter = 0;
            if (!_ownedHeroes.Contains(heroClassId))
                _ownedHeroes.Add(heroClassId);

            EventBus.Publish(new GachaPullEvent
            {
                HeroClassId = heroClassId,
                Rarity = HeroRarity.Legendary,
                IsDuplicate = false,
                StarFragmentsAwarded = 0
            });

            return true;
        }

        // Wishlist management
        public bool AddToWishlist(string classId)
        {
            if (_wishlist.Count >= MaxWishlistSize) return false;
            if (_wishlist.Contains(classId)) return false;
            _wishlist.Add(classId);
            return true;
        }

        public bool RemoveFromWishlist(string classId)
        {
            return _wishlist.Remove(classId);
        }

        public bool IsOnWishlist(string classId)
        {
            return _wishlist.Contains(classId);
        }

        public bool OwnsHero(string classId)
        {
            return _ownedHeroes.Contains(classId);
        }

        public void RegisterOwnedHero(string classId)
        {
            _ownedHeroes.Add(classId);
        }

        // Save/Load
        public GachaSaveData GatherSaveData()
        {
            return new GachaSaveData
            {
                EpicPityCounter = _epicPityCounter,
                LegendaryPityCounter = _legendaryPityCounter,
                TotalPullCount = _totalPullCount,
                SparkCounter = _sparkCounter,
                Wishlist = new List<string>(_wishlist),
                OwnedHeroes = new List<string>(_ownedHeroes)
            };
        }

        public void LoadSaveData(GachaSaveData data)
        {
            if (data == null) return;
            _epicPityCounter = data.EpicPityCounter;
            _legendaryPityCounter = data.LegendaryPityCounter;
            _totalPullCount = data.TotalPullCount;
            _sparkCounter = data.SparkCounter;
            _wishlist.Clear();
            _wishlist.AddRange(data.Wishlist);
            _ownedHeroes.Clear();
            foreach (var h in data.OwnedHeroes)
                _ownedHeroes.Add(h);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
