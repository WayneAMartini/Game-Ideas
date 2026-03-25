using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [CreateAssetMenu(fileName = "NewBanner", menuName = "Ascendant/Banner Data")]
    public class BannerData : ScriptableObject
    {
        [Header("Identity")]
        public string bannerId;
        public string bannerName;
        public string description;
        public Sprite bannerArt;

        [Header("Schedule")]
        public string startDate; // ISO 8601
        public string endDate;
        public bool isDefault; // Standard banner, always available

        [Header("Featured Heroes")]
        public List<string> featuredHeroes = new();
        public HeroRarity featuredRarity = HeroRarity.Epic;

        [Header("Rate Modifications")]
        [Range(1f, 3f)] public float legendaryRateMultiplier = 1f;
        [Range(1f, 3f)] public float epicRateMultiplier = 1f;

        public bool IsActive()
        {
            if (isDefault) return true;

            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                return false;

            var now = DateTime.UtcNow;
            if (DateTime.TryParse(startDate, out var start) && DateTime.TryParse(endDate, out var end))
                return now >= start && now <= end;

            return false;
        }

        public TimeSpan GetTimeRemaining()
        {
            if (isDefault) return TimeSpan.MaxValue;
            if (DateTime.TryParse(endDate, out var end))
                return end - DateTime.UtcNow;
            return TimeSpan.Zero;
        }
    }
}
