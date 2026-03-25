using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class StarSaveData
    {
        public List<HeroStarEntry> HeroStars = new();
    }

    [Serializable]
    public class HeroStarEntry
    {
        public string ClassId;
        public int StarRating;
    }

    public class StarSystem : MonoBehaviour
    {
        public static StarSystem Instance { get; private set; }

        public const int MinStars = 1;
        public const int MaxStars = 7;
        public const float StatBonusPerStar = 0.10f; // +10% per star

        // Star-Up costs (Star Fragments required)
        static readonly int[] FragmentCosts = { 0, 0, 0, 20, 50, 100, 200 };
        // Additional material costs per star tier
        static readonly int[] MaterialGoldCosts = { 0, 0, 0, 10000, 50000, 200000, 500000 };
        // 7-Star requires Aether Crystals
        const int SevenStarAetherCost = 50;

        readonly Dictionary<string, int> _heroStars = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int GetStarRating(string classId)
        {
            return _heroStars.TryGetValue(classId, out var stars) ? stars : MinStars;
        }

        public void SetStarRating(string classId, int stars)
        {
            _heroStars[classId] = Mathf.Clamp(stars, MinStars, MaxStars);
        }

        public float GetStatMultiplier(string classId)
        {
            int stars = GetStarRating(classId);
            return 1f + (stars - 1) * StatBonusPerStar;
        }

        public bool CanStarUp(string classId)
        {
            int current = GetStarRating(classId);
            if (current >= MaxStars) return false;

            int targetStar = current + 1;
            if (targetStar < 3) return true; // 1->2, 2->3 are free/automatic from summon rarity

            int fragCost = GetFragmentCost(targetStar);
            int goldCost = GetGoldCost(targetStar);

            var cm = CurrencyManager.Instance;
            if (cm == null) return false;

            if (!cm.CanAfford(CurrencyType.StarFragments, fragCost)) return false;
            if (!cm.CanAfford(CurrencyType.Gold, goldCost)) return false;

            // 7-Star needs Aether Crystals
            if (targetStar == 7 && !cm.CanAfford(CurrencyType.AetherCrystals, SevenStarAetherCost))
                return false;

            return true;
        }

        public bool TryStarUp(string classId)
        {
            if (!CanStarUp(classId)) return false;

            int current = GetStarRating(classId);
            int targetStar = current + 1;

            var cm = CurrencyManager.Instance;

            if (targetStar >= 3)
            {
                int fragCost = GetFragmentCost(targetStar);
                int goldCost = GetGoldCost(targetStar);

                cm.SpendCurrency(CurrencyType.StarFragments, fragCost);
                cm.SpendCurrency(CurrencyType.Gold, goldCost);

                if (targetStar == 7)
                    cm.SpendCurrency(CurrencyType.AetherCrystals, SevenStarAetherCost);
            }

            _heroStars[classId] = targetStar;

            EventBus.Publish(new StarUpEvent
            {
                HeroClassId = classId,
                NewStarRating = targetStar
            });

            return true;
        }

        public int GetFragmentCost(int targetStar)
        {
            if (targetStar < 0 || targetStar >= FragmentCosts.Length) return 0;
            return FragmentCosts[targetStar];
        }

        public int GetGoldCost(int targetStar)
        {
            if (targetStar < 0 || targetStar >= MaterialGoldCosts.Length) return 0;
            return MaterialGoldCosts[targetStar];
        }

        // Save/Load
        public StarSaveData GatherSaveData()
        {
            var data = new StarSaveData();
            foreach (var kvp in _heroStars)
            {
                data.HeroStars.Add(new HeroStarEntry
                {
                    ClassId = kvp.Key,
                    StarRating = kvp.Value
                });
            }
            return data;
        }

        public void LoadSaveData(StarSaveData data)
        {
            if (data == null) return;
            _heroStars.Clear();
            foreach (var entry in data.HeroStars)
                _heroStars[entry.ClassId] = entry.StarRating;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
