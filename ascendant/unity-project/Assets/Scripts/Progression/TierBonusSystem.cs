using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Progression
{
    public class TierBonusSystem : MonoBehaviour
    {
        public static TierBonusSystem Instance { get; private set; }

        // heroSlot -> ascension count
        readonly Dictionary<int, int> _ascensionCounts = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetAscensionCount(int heroSlot, int count)
        {
            _ascensionCounts[heroSlot] = count;
        }

        public int GetAscensionCount(int heroSlot)
        {
            return _ascensionCounts.TryGetValue(heroSlot, out int c) ? c : 0;
        }

        public AscensionTierLevel GetTier(int heroSlot)
        {
            return AscensionTierData.GetTierForAscensions(GetAscensionCount(heroSlot));
        }

        public float GetTierStatMultiplier(int heroSlot)
        {
            float bonusPercent = AscensionTierData.GetStatBonusPercent(GetAscensionCount(heroSlot));
            return 1f + bonusPercent / 100f;
        }

        public void IncrementAscension(int heroSlot)
        {
            int prev = GetAscensionCount(heroSlot);
            _ascensionCounts[heroSlot] = prev + 1;

            var oldTier = AscensionTierData.GetTierForAscensions(prev);
            var newTier = AscensionTierData.GetTierForAscensions(prev + 1);

            if (newTier != oldTier)
            {
                EventBus.Publish(new AscensionTierChangedEvent
                {
                    HeroSlot = heroSlot,
                    OldTier = oldTier,
                    NewTier = newTier,
                    AscensionCount = prev + 1
                });
            }
        }

        public Dictionary<int, int> GetAllAscensionCounts()
        {
            return new Dictionary<int, int>(_ascensionCounts);
        }

        public void LoadAscensionCounts(Dictionary<int, int> counts)
        {
            _ascensionCounts.Clear();
            foreach (var kvp in counts)
                _ascensionCounts[kvp.Key] = kvp.Value;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
