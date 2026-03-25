using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class BattlePassTierReward
    {
        public CurrencyType currencyType;
        public int amount;
        public string itemId; // for non-currency rewards like skins, heroes
        public string description;
    }

    [CreateAssetMenu(fileName = "NewBattlePass", menuName = "Ascendant/Battle Pass Data")]
    public class BattlePassData : ScriptableObject
    {
        [Header("Season Info")]
        public string seasonId;
        public string seasonName;
        public int totalTiers = 50;
        public int xpPerTier = 1000;

        [Header("Schedule")]
        public string startDate;
        public string endDate;

        [Header("Free Track Rewards")]
        public List<BattlePassTierReward> freeRewards = new();

        [Header("Premium Track Rewards")]
        public List<BattlePassTierReward> premiumRewards = new();

        public int GetXPForTier(int tier)
        {
            // Slightly increasing XP per tier
            return xpPerTier + (tier * 50);
        }

        public BattlePassTierReward GetFreeReward(int tier)
        {
            if (tier < 0 || tier >= freeRewards.Count) return null;
            return freeRewards[tier];
        }

        public BattlePassTierReward GetPremiumReward(int tier)
        {
            if (tier < 0 || tier >= premiumRewards.Count) return null;
            return premiumRewards[tier];
        }

        public TimeSpan GetTimeRemaining()
        {
            if (DateTime.TryParse(endDate, out var end))
                return end - DateTime.UtcNow;
            return TimeSpan.Zero;
        }

        public bool IsActive()
        {
            var now = DateTime.UtcNow;
            if (DateTime.TryParse(startDate, out var start) && DateTime.TryParse(endDate, out var end))
                return now >= start && now <= end;
            return true; // Default to active if no dates set
        }
    }
}
