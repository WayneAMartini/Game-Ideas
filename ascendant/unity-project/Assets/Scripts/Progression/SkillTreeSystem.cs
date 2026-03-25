using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Progression
{
    public class SkillTreeSystem : MonoBehaviour
    {
        public static SkillTreeSystem Instance { get; private set; }

        [Header("Skill Tree Definitions")]
        [SerializeField] SkillTreeData[] _skillTrees;

        // Per-hero: heroSlot -> set of purchased node IDs
        readonly Dictionary<int, HashSet<string>> _purchasedNodes = new();

        // Per-hero: heroSlot -> available skill points
        readonly Dictionary<int, int> _availablePoints = new();

        // Respec cost: base * (1 + respecCount)^2
        const float RespecGoldBase = 500f;
        readonly Dictionary<int, int> _respecCounts = new();

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
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
        }

        void OnLevelUp(LevelUpEvent evt)
        {
            // Award 1 skill point per level
            if (!_availablePoints.ContainsKey(evt.HeroSlot))
                _availablePoints[evt.HeroSlot] = 0;
            _availablePoints[evt.HeroSlot]++;

            EventBus.Publish(new SkillPointsChangedEvent
            {
                HeroSlot = evt.HeroSlot,
                Available = _availablePoints[evt.HeroSlot],
                Spent = GetSpentPoints(evt.HeroSlot)
            });
        }

        public SkillTreeData GetSkillTree(string classId)
        {
            if (_skillTrees == null) return null;
            for (int i = 0; i < _skillTrees.Length; i++)
                if (_skillTrees[i] != null && _skillTrees[i].classId == classId)
                    return _skillTrees[i];
            return null;
        }

        public int GetAvailablePoints(int heroSlot)
        {
            return _availablePoints.TryGetValue(heroSlot, out int pts) ? pts : 0;
        }

        public int GetSpentPoints(int heroSlot)
        {
            if (!_purchasedNodes.TryGetValue(heroSlot, out var nodes)) return 0;

            int spent = 0;
            foreach (var nodeId in nodes)
            {
                var def = FindNodeDefinition(heroSlot, nodeId);
                if (def != null) spent += def.cost;
            }
            return spent;
        }

        public bool IsNodePurchased(int heroSlot, string nodeId)
        {
            return _purchasedNodes.TryGetValue(heroSlot, out var nodes) && nodes.Contains(nodeId);
        }

        public SkillNodeState GetNodeState(int heroSlot, string nodeId, string classId)
        {
            if (IsNodePurchased(heroSlot, nodeId)) return SkillNodeState.Purchased;
            if (CanInvest(heroSlot, nodeId, classId)) return SkillNodeState.Available;
            return SkillNodeState.Locked;
        }

        public bool CanInvest(int heroSlot, string nodeId, string classId)
        {
            var tree = GetSkillTree(classId);
            if (tree == null) return false;

            var node = FindNodeInTree(tree, nodeId);
            if (node == null) return false;

            // Already purchased?
            if (IsNodePurchased(heroSlot, nodeId)) return false;

            // Enough points?
            if (GetAvailablePoints(heroSlot) < node.cost) return false;

            // Level requirement
            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero != null && hero.Level < node.unlockLevel) return false;

            // Prerequisites met?
            if (node.prerequisiteNodeIds != null)
            {
                for (int i = 0; i < node.prerequisiteNodeIds.Length; i++)
                {
                    if (!IsNodePurchased(heroSlot, node.prerequisiteNodeIds[i]))
                        return false;
                }
            }

            return true;
        }

        public bool InvestInNode(int heroSlot, string nodeId, string classId)
        {
            if (!CanInvest(heroSlot, nodeId, classId)) return false;

            var tree = GetSkillTree(classId);
            var node = FindNodeInTree(tree, nodeId);

            if (!_purchasedNodes.ContainsKey(heroSlot))
                _purchasedNodes[heroSlot] = new HashSet<string>();

            _purchasedNodes[heroSlot].Add(nodeId);
            _availablePoints[heroSlot] -= node.cost;

            // Recalculate hero stats
            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            hero?.RecalculateStats();

            EventBus.Publish(new SkillTreeChangedEvent
            {
                HeroSlot = heroSlot,
                ClassId = classId,
                BranchId = FindBranchForNode(tree, nodeId)
            });

            EventBus.Publish(new SkillPointsChangedEvent
            {
                HeroSlot = heroSlot,
                Available = _availablePoints[heroSlot],
                Spent = GetSpentPoints(heroSlot)
            });

            return true;
        }

        // Get total stat bonus from all purchased skill nodes for a hero
        public float GetSkillTreeStatBonus(int heroSlot, StatType stat, string classId)
        {
            if (!_purchasedNodes.TryGetValue(heroSlot, out var nodes)) return 0f;

            var tree = GetSkillTree(classId);
            if (tree == null) return 0f;

            float total = 0f;
            foreach (var nodeId in nodes)
            {
                var def = FindNodeInTree(tree, nodeId);
                if (def?.statBonuses != null)
                {
                    for (int i = 0; i < def.statBonuses.Length; i++)
                        if (def.statBonuses[i].stat == stat)
                            total += def.statBonuses[i].value;
                }
            }
            return total;
        }

        // Respec: reset all nodes, refund points
        public bool Respec(int heroSlot, bool freeOnAscension = false)
        {
            if (!_purchasedNodes.ContainsKey(heroSlot)) return false;

            if (!freeOnAscension)
            {
                int respecCount = _respecCounts.TryGetValue(heroSlot, out int c) ? c : 0;
                float cost = RespecGoldBase * Mathf.Pow(1 + respecCount, 2f);
                var currency = Economy.CurrencyManager.Instance;
                if (currency == null || !currency.SpendGold(cost)) return false;
                _respecCounts[heroSlot] = respecCount + 1;
            }

            int refundedPoints = GetSpentPoints(heroSlot);
            _purchasedNodes[heroSlot].Clear();

            if (!_availablePoints.ContainsKey(heroSlot))
                _availablePoints[heroSlot] = 0;
            _availablePoints[heroSlot] += refundedPoints;

            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            hero?.RecalculateStats();

            string classId = hero?.Data?.classId ?? "";

            EventBus.Publish(new SkillTreeChangedEvent
            {
                HeroSlot = heroSlot,
                ClassId = classId,
                BranchId = ""
            });

            EventBus.Publish(new SkillPointsChangedEvent
            {
                HeroSlot = heroSlot,
                Available = _availablePoints[heroSlot],
                Spent = 0
            });

            return true;
        }

        // Reset on Ascension: clear all nodes and points
        public void ResetForAscension(int heroSlot)
        {
            _purchasedNodes.Remove(heroSlot);
            _availablePoints.Remove(heroSlot);
            _respecCounts.Remove(heroSlot);
        }

        SkillNodeDefinition FindNodeDefinition(int heroSlot, string nodeId)
        {
            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data == null) return null;
            var tree = GetSkillTree(hero.Data.classId);
            return FindNodeInTree(tree, nodeId);
        }

        static SkillNodeDefinition FindNodeInTree(SkillTreeData tree, string nodeId)
        {
            if (tree?.branches == null) return null;
            for (int b = 0; b < tree.branches.Length; b++)
            {
                var branch = tree.branches[b];
                if (branch?.nodes == null) continue;
                for (int n = 0; n < branch.nodes.Length; n++)
                    if (branch.nodes[n].nodeId == nodeId)
                        return branch.nodes[n];
            }
            return null;
        }

        static string FindBranchForNode(SkillTreeData tree, string nodeId)
        {
            if (tree?.branches == null) return "";
            for (int b = 0; b < tree.branches.Length; b++)
            {
                var branch = tree.branches[b];
                if (branch?.nodes == null) continue;
                for (int n = 0; n < branch.nodes.Length; n++)
                    if (branch.nodes[n].nodeId == nodeId)
                        return branch.branchId;
            }
            return "";
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
