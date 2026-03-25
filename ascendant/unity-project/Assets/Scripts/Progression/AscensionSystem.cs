using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Islands;
using Ascendant.Economy;
using Ascendant.Party;

namespace Ascendant.Progression
{
    [System.Serializable]
    public class AscensionSaveData
    {
        public List<HeroAscensionData> HeroAscensions = new();
        public List<AscensionSkillTreeSaveData> SkillTreeNodes = new();
        public List<DemigodSaveData> Demigods = new();
    }

    [System.Serializable]
    public class HeroAscensionData
    {
        public int HeroSlot;
        public string ClassId;
        public int AscensionCount;
        public int HighestIslandReached;
    }

    [System.Serializable]
    public class AscensionSkillTreeSaveData
    {
        public string NodeId;
        public string BranchId;
    }

    [System.Serializable]
    public class DemigodSaveData
    {
        public string ClassId;
        public string BuffDescription;
    }

    public class AscensionSystem : MonoBehaviour
    {
        public static AscensionSystem Instance { get; private set; }

        [Header("Config")]
        [SerializeField] float _baseShardsPerIsland = 10f;
        [SerializeField] int _minIslandToAscend = 3;

        // Per-hero ascension tracking: heroSlot -> highest island
        readonly Dictionary<int, int> _highestIslandReached = new();

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
            EventBus.Subscribe<IslandCompletedEvent>(OnIslandCompleted);
            EventBus.Subscribe<StageAdvancedEvent>(OnStageAdvanced);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<IslandCompletedEvent>(OnIslandCompleted);
            EventBus.Unsubscribe<StageAdvancedEvent>(OnStageAdvanced);
        }

        void OnIslandCompleted(IslandCompletedEvent evt)
        {
            UpdateHighestIsland(evt.IslandIndex + 1); // IslandIndex is 0-based
        }

        void OnStageAdvanced(StageAdvancedEvent evt)
        {
            UpdateHighestIsland(evt.Island);
        }

        void UpdateHighestIsland(int island)
        {
            // Track highest island for all active heroes
            var party = PartyManager.Instance;
            if (party == null) return;

            var heroes = party.GetAllHeroes();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] == null) continue;
                int current = _highestIslandReached.TryGetValue(i, out int val) ? val : 1;
                if (island > current)
                    _highestIslandReached[i] = island;
            }
        }

        public bool CanAscend(int heroSlot)
        {
            var hero = PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data == null) return false;

            // Check if hero has reached minimum island
            int highest = GetHighestIsland(heroSlot);
            if (highest < _minIslandToAscend) return false;

            // Already a Demigod with 10+ ascensions = must do Transcendence Trial instead
            var tierSystem = TierBonusSystem.Instance;
            if (tierSystem != null && tierSystem.GetAscensionCount(heroSlot) >= 10)
                return false;

            return true;
        }

        public int GetHighestIsland(int heroSlot)
        {
            return _highestIslandReached.TryGetValue(heroSlot, out int val) ? val : 1;
        }

        public double CalculateShards(int heroSlot)
        {
            int island = GetHighestIsland(heroSlot);
            var tierSystem = TierBonusSystem.Instance;
            float tierMult = 1f;
            if (tierSystem != null)
            {
                int ascCount = tierSystem.GetAscensionCount(heroSlot);
                tierMult = 1f + ascCount * 0.1f; // +10% per prior ascension
            }
            return _baseShardsPerIsland * island * tierMult;
        }

        public AscensionPreview GetAscensionPreview(int heroSlot)
        {
            var hero = PartyManager.Instance?.GetHero(heroSlot);
            var tierSystem = TierBonusSystem.Instance;
            int currentAscensions = tierSystem?.GetAscensionCount(heroSlot) ?? 0;
            var currentTier = AscensionTierData.GetTierForAscensions(currentAscensions);
            var nextTier = AscensionTierData.GetTierForAscensions(currentAscensions + 1);

            return new AscensionPreview
            {
                HeroSlot = heroSlot,
                ClassId = hero?.Data?.classId ?? "",
                HeroName = hero?.Data?.heroName ?? "",
                CurrentAscensions = currentAscensions,
                ShardsToEarn = CalculateShards(heroSlot),
                HighestIsland = GetHighestIsland(heroSlot),
                CurrentTier = currentTier,
                NewTier = nextTier,
                WillPromote = nextTier != currentTier
            };
        }

        public bool PerformAscension(int heroSlot)
        {
            if (!CanAscend(heroSlot)) return false;

            var hero = PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data == null) return false;

            string classId = hero.Data.classId;
            int highestIsland = GetHighestIsland(heroSlot);
            double shards = CalculateShards(heroSlot);

            GameManager.Instance?.SetState(GameState.Ascending);

            // 1. Award Ascension Shards
            CurrencyManager.Instance?.AddCurrency(CurrencyType.AscensionShards, shards);

            // 2. Increment ascension count
            TierBonusSystem.Instance?.IncrementAscension(heroSlot);
            int newAscCount = TierBonusSystem.Instance?.GetAscensionCount(heroSlot) ?? 1;

            // 3. Record ascension in class mastery (permanent)
            ClassMasterySystem.Instance?.RecordAscension(classId);

            // 4. Reset skill tree (free respec)
            SkillTreeSystem.Instance?.ResetForAscension(heroSlot);

            // 5. Reset hero level to 1
            hero.ResetLevel();

            // 6. Reset island progress to Island 1
            ResetIslandProgress(heroSlot);

            // 7. Recalculate stats with tier bonuses
            hero.RecalculateStats();

            // Publish ascension event
            EventBus.Publish(new AscensionEvent
            {
                HeroSlot = heroSlot,
                ClassId = classId,
                AscensionCount = newAscCount,
                ShardsEarned = shards,
                HighestIslandReached = highestIsland
            });

            return true;
        }

        void ResetIslandProgress(int heroSlot)
        {
            _highestIslandReached[heroSlot] = 1;

            // Reset the global island/stage if this is the active hero context
            var islandManager = IslandManager.Instance;
            if (islandManager != null)
                islandManager.ResetForAscension();

            var stageManager = StageManager.Instance;
            if (stageManager != null)
                stageManager.ResetForAscension();
        }

        public void SetHighestIsland(int heroSlot, int island)
        {
            _highestIslandReached[heroSlot] = island;
        }

        public Dictionary<int, int> GetAllHighestIslands()
        {
            return new Dictionary<int, int>(_highestIslandReached);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public struct AscensionPreview
    {
        public int HeroSlot;
        public string ClassId;
        public string HeroName;
        public int CurrentAscensions;
        public double ShardsToEarn;
        public int HighestIsland;
        public AscensionTierLevel CurrentTier;
        public AscensionTierLevel NewTier;
        public bool WillPromote;
    }
}
