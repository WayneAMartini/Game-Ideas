using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class BattlePassSaveData
    {
        public string SeasonId;
        public int CurrentTier;
        public int CurrentXP;
        public bool IsPremium;
        public List<int> ClaimedFreeTiers = new();
        public List<int> ClaimedPremiumTiers = new();
    }

    public class BattlePassSystem : MonoBehaviour
    {
        public static BattlePassSystem Instance { get; private set; }

        [SerializeField] BattlePassData _currentPassData;

        int _currentTier;
        int _currentXP;
        bool _isPremium;
        readonly HashSet<int> _claimedFreeTiers = new();
        readonly HashSet<int> _claimedPremiumTiers = new();

        public BattlePassData CurrentPassData => _currentPassData;
        public int CurrentTier => _currentTier;
        public int CurrentXP => _currentXP;
        public bool IsPremium => _isPremium;
        public int MaxTiers => _currentPassData != null ? _currentPassData.totalTiers : 50;

        public int XPToNextTier => _currentPassData != null
            ? _currentPassData.GetXPForTier(_currentTier + 1)
            : 1000;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
        }

        void OnQuestCompleted(QuestCompletedEvent evt)
        {
            AddXP(evt.BattlePassXP);
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            if (_currentTier >= MaxTiers) return;

            _currentXP += amount;

            // Level up through tiers
            while (_currentXP >= XPToNextTier && _currentTier < MaxTiers)
            {
                _currentXP -= XPToNextTier;
                _currentTier++;
            }

            EventBus.Publish(new BattlePassXPGainedEvent
            {
                Amount = amount,
                CurrentXP = _currentXP,
                CurrentTier = _currentTier
            });
        }

        public void ActivatePremium()
        {
            _isPremium = true;
        }

        public bool CanClaimFreeReward(int tier)
        {
            if (tier > _currentTier) return false;
            return !_claimedFreeTiers.Contains(tier);
        }

        public bool CanClaimPremiumReward(int tier)
        {
            if (!_isPremium) return false;
            if (tier > _currentTier) return false;
            return !_claimedPremiumTiers.Contains(tier);
        }

        public bool ClaimFreeReward(int tier)
        {
            if (!CanClaimFreeReward(tier)) return false;

            var reward = _currentPassData?.GetFreeReward(tier);
            if (reward != null)
                AwardReward(reward);

            _claimedFreeTiers.Add(tier);

            EventBus.Publish(new BattlePassTierClaimedEvent
            {
                Tier = tier,
                IsPremium = false
            });

            return true;
        }

        public bool ClaimPremiumReward(int tier)
        {
            if (!CanClaimPremiumReward(tier)) return false;

            var reward = _currentPassData?.GetPremiumReward(tier);
            if (reward != null)
                AwardReward(reward);

            _claimedPremiumTiers.Add(tier);

            EventBus.Publish(new BattlePassTierClaimedEvent
            {
                Tier = tier,
                IsPremium = true
            });

            return true;
        }

        void AwardReward(BattlePassTierReward reward)
        {
            if (reward.amount > 0)
            {
                CurrencyManager.Instance?.AddCurrency(reward.currencyType, reward.amount);
            }
        }

        public bool IsFreeTierClaimed(int tier) => _claimedFreeTiers.Contains(tier);
        public bool IsPremiumTierClaimed(int tier) => _claimedPremiumTiers.Contains(tier);

        // Save/Load
        public BattlePassSaveData GatherSaveData()
        {
            return new BattlePassSaveData
            {
                SeasonId = _currentPassData != null ? _currentPassData.seasonId : "",
                CurrentTier = _currentTier,
                CurrentXP = _currentXP,
                IsPremium = _isPremium,
                ClaimedFreeTiers = new List<int>(_claimedFreeTiers),
                ClaimedPremiumTiers = new List<int>(_claimedPremiumTiers)
            };
        }

        public void LoadSaveData(BattlePassSaveData data)
        {
            if (data == null) return;

            // Check if save is for current season
            string currentSeason = _currentPassData != null ? _currentPassData.seasonId : "";
            if (!string.IsNullOrEmpty(data.SeasonId) && data.SeasonId != currentSeason)
            {
                Debug.Log("[BattlePass] Save is from different season, starting fresh.");
                return;
            }

            _currentTier = data.CurrentTier;
            _currentXP = data.CurrentXP;
            _isPremium = data.IsPremium;
            _claimedFreeTiers.Clear();
            foreach (var t in data.ClaimedFreeTiers)
                _claimedFreeTiers.Add(t);
            _claimedPremiumTiers.Clear();
            foreach (var t in data.ClaimedPremiumTiers)
                _claimedPremiumTiers.Add(t);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
