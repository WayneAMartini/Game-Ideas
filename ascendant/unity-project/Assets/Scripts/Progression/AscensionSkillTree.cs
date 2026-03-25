using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Progression
{
    public class AscensionSkillTree : MonoBehaviour
    {
        public static AscensionSkillTree Instance { get; private set; }

        [Header("Node Definitions (4 branches x 10 nodes)")]
        [SerializeField] AscensionNode[] _allNodes;

        readonly HashSet<string> _purchasedNodeIds = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public AscensionNode[] AllNodes => _allNodes;

        public bool IsNodePurchased(string nodeId)
        {
            return _purchasedNodeIds.Contains(nodeId);
        }

        public AscensionNodeState GetNodeState(string nodeId)
        {
            if (IsNodePurchased(nodeId)) return AscensionNodeState.Purchased;
            if (CanPurchase(nodeId)) return AscensionNodeState.Available;
            return AscensionNodeState.Locked;
        }

        public bool CanPurchase(string nodeId)
        {
            var node = FindNode(nodeId);
            if (node == null) return false;

            if (IsNodePurchased(nodeId)) return false;

            // Check shard cost
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null || !currency.CanAfford(CurrencyType.AscensionShards, node.shardCost))
                return false;

            // Check prerequisites
            if (node.prerequisiteNodeIds != null)
            {
                for (int i = 0; i < node.prerequisiteNodeIds.Length; i++)
                {
                    if (!IsNodePurchased(node.prerequisiteNodeIds[i]))
                        return false;
                }
            }

            return true;
        }

        public bool PurchaseNode(string nodeId)
        {
            if (!CanPurchase(nodeId)) return false;

            var node = FindNode(nodeId);
            var currency = Economy.CurrencyManager.Instance;

            if (!currency.SpendCurrency(CurrencyType.AscensionShards, node.shardCost))
                return false;

            _purchasedNodeIds.Add(nodeId);

            // Recalculate all hero stats since ascension tree is global
            RecalculateAllHeroStats();

            EventBus.Publish(new AscensionSkillNodePurchasedEvent
            {
                NodeId = nodeId,
                BranchId = node.branch.ToString(),
                ShardCost = node.shardCost
            });

            return true;
        }

        public float GetTotalStatBonusPercent(StatType stat)
        {
            float total = 0f;
            if (_allNodes == null) return total;

            foreach (var node in _allNodes)
            {
                if (node == null || !_purchasedNodeIds.Contains(node.nodeId)) continue;
                if (node.statBonuses == null) continue;

                for (int i = 0; i < node.statBonuses.Length; i++)
                {
                    if (node.statBonuses[i].stat == stat)
                        total += node.statBonuses[i].percentValue;
                }
            }
            return total;
        }

        public float GetTotalGoldBonus()
        {
            float total = 0f;
            if (_allNodes == null) return total;
            foreach (var node in _allNodes)
            {
                if (node != null && _purchasedNodeIds.Contains(node.nodeId))
                    total += node.goldBonusPercent;
            }
            return total;
        }

        public float GetTotalXpBonus()
        {
            float total = 0f;
            if (_allNodes == null) return total;
            foreach (var node in _allNodes)
            {
                if (node != null && _purchasedNodeIds.Contains(node.nodeId))
                    total += node.xpBonusPercent;
            }
            return total;
        }

        public float GetTotalDropRateBonus()
        {
            float total = 0f;
            if (_allNodes == null) return total;
            foreach (var node in _allNodes)
            {
                if (node != null && _purchasedNodeIds.Contains(node.nodeId))
                    total += node.dropRateBonusPercent;
            }
            return total;
        }

        public float GetTotalCooldownReduction()
        {
            float total = 0f;
            if (_allNodes == null) return total;
            foreach (var node in _allNodes)
            {
                if (node != null && _purchasedNodeIds.Contains(node.nodeId))
                    total += node.cooldownReductionPercent;
            }
            return total;
        }

        public int GetPurchasedCount()
        {
            return _purchasedNodeIds.Count;
        }

        public int GetPurchasedCountInBranch(AscensionBranch branch)
        {
            int count = 0;
            if (_allNodes == null) return count;
            foreach (var node in _allNodes)
            {
                if (node != null && node.branch == branch && _purchasedNodeIds.Contains(node.nodeId))
                    count++;
            }
            return count;
        }

        public double GetTotalShardsSpent()
        {
            double total = 0;
            if (_allNodes == null) return total;
            foreach (var node in _allNodes)
            {
                if (node != null && _purchasedNodeIds.Contains(node.nodeId))
                    total += node.shardCost;
            }
            return total;
        }

        public AscensionNode[] GetNodesInBranch(AscensionBranch branch)
        {
            if (_allNodes == null) return new AscensionNode[0];

            var list = new List<AscensionNode>();
            foreach (var node in _allNodes)
            {
                if (node != null && node.branch == branch)
                    list.Add(node);
            }
            list.Sort((a, b) => a.tier.CompareTo(b.tier));
            return list.ToArray();
        }

        public HashSet<string> GetPurchasedNodeIds()
        {
            return new HashSet<string>(_purchasedNodeIds);
        }

        public void LoadPurchasedNodes(IEnumerable<string> nodeIds)
        {
            _purchasedNodeIds.Clear();
            foreach (var id in nodeIds)
                _purchasedNodeIds.Add(id);
        }

        AscensionNode FindNode(string nodeId)
        {
            if (_allNodes == null) return null;
            for (int i = 0; i < _allNodes.Length; i++)
                if (_allNodes[i] != null && _allNodes[i].nodeId == nodeId)
                    return _allNodes[i];
            return null;
        }

        void RecalculateAllHeroStats()
        {
            var party = Party.PartyManager.Instance;
            if (party == null) return;

            var heroes = party.GetAllHeroes();
            for (int i = 0; i < heroes.Length; i++)
                heroes[i]?.RecalculateStats();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public enum AscensionNodeState
    {
        Locked,
        Available,
        Purchased
    }
}
