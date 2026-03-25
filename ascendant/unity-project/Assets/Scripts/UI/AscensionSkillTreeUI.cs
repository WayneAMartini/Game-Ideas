using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class AscensionSkillTreeUI : MonoBehaviour
    {
        public static AscensionSkillTreeUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] GameObject _treePanel;

        [Header("Branch Tabs")]
        [SerializeField] Button _powerTab;
        [SerializeField] Button _fortitudeTab;
        [SerializeField] Button _prosperityTab;
        [SerializeField] Button _swiftnessTab;

        [Header("Node Display")]
        [SerializeField] Transform _nodeContainer;
        [SerializeField] GameObject _nodeButtonPrefab;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI _branchTitleText;
        [SerializeField] TextMeshProUGUI _shardsAvailableText;
        [SerializeField] TextMeshProUGUI _totalSpentText;
        [SerializeField] TextMeshProUGUI _nodesUnlockedText;

        [Header("Node Detail")]
        [SerializeField] GameObject _nodeDetailPanel;
        [SerializeField] TextMeshProUGUI _nodeNameText;
        [SerializeField] TextMeshProUGUI _nodeDescText;
        [SerializeField] TextMeshProUGUI _nodeCostText;
        [SerializeField] TextMeshProUGUI _nodeEffectText;
        [SerializeField] Button _purchaseButton;
        [SerializeField] Button _closeButton;

        [Header("Colors")]
        [SerializeField] Color _lockedColor = Color.gray;
        [SerializeField] Color _availableColor = new Color(0.3f, 0.5f, 1f);
        [SerializeField] Color _purchasedColor = new Color(1f, 0.85f, 0.2f);

        AscensionBranch _currentBranch = AscensionBranch.Power;
        string _selectedNodeId;

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
            if (_powerTab != null) _powerTab.onClick.AddListener(() => SelectBranch(AscensionBranch.Power));
            if (_fortitudeTab != null) _fortitudeTab.onClick.AddListener(() => SelectBranch(AscensionBranch.Fortitude));
            if (_prosperityTab != null) _prosperityTab.onClick.AddListener(() => SelectBranch(AscensionBranch.Prosperity));
            if (_swiftnessTab != null) _swiftnessTab.onClick.AddListener(() => SelectBranch(AscensionBranch.Swiftness));
            if (_purchaseButton != null) _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);

            if (_nodeDetailPanel != null) _nodeDetailPanel.SetActive(false);
            if (_treePanel != null) _treePanel.SetActive(false);
        }

        void OnEnable()
        {
            EventBus.Subscribe<AscensionSkillNodePurchasedEvent>(OnNodePurchased);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<AscensionSkillNodePurchasedEvent>(OnNodePurchased);
        }

        public void Open()
        {
            if (_treePanel != null) _treePanel.SetActive(true);
            SelectBranch(_currentBranch);
        }

        public void Close()
        {
            if (_treePanel != null) _treePanel.SetActive(false);
            if (_nodeDetailPanel != null) _nodeDetailPanel.SetActive(false);
        }

        void SelectBranch(AscensionBranch branch)
        {
            _currentBranch = branch;
            if (_branchTitleText != null)
                _branchTitleText.text = branch.ToString();

            RefreshDisplay();
        }

        void RefreshDisplay()
        {
            var tree = AscensionSkillTree.Instance;
            if (tree == null) return;

            var currency = Economy.CurrencyManager.Instance;
            double shards = currency?.AscensionShards ?? 0;

            if (_shardsAvailableText != null)
                _shardsAvailableText.text = $"Shards: {shards:N0}";
            if (_totalSpentText != null)
                _totalSpentText.text = $"Total Spent: {tree.GetTotalShardsSpent():N0}";
            if (_nodesUnlockedText != null)
                _nodesUnlockedText.text = $"Nodes: {tree.GetPurchasedCount()} / {(tree.AllNodes?.Length ?? 0)}";

            RefreshNodes();
        }

        void RefreshNodes()
        {
            var tree = AscensionSkillTree.Instance;
            if (tree == null || _nodeContainer == null) return;

            // Clear existing node buttons
            for (int i = _nodeContainer.childCount - 1; i >= 0; i--)
                Destroy(_nodeContainer.GetChild(i).gameObject);

            var nodes = tree.GetNodesInBranch(_currentBranch);
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                CreateNodeButton(node, tree.GetNodeState(node.nodeId));
            }
        }

        void CreateNodeButton(AscensionNode node, AscensionNodeState state)
        {
            if (_nodeButtonPrefab == null || _nodeContainer == null) return;

            var go = Instantiate(_nodeButtonPrefab, _nodeContainer);
            var btn = go.GetComponent<Button>();
            var img = go.GetComponent<Image>();
            var label = go.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = node.nodeName;

            if (img != null)
            {
                img.color = state switch
                {
                    AscensionNodeState.Locked => _lockedColor,
                    AscensionNodeState.Available => _availableColor,
                    AscensionNodeState.Purchased => _purchasedColor,
                    _ => _lockedColor
                };
            }

            string nodeId = node.nodeId;
            if (btn != null)
                btn.onClick.AddListener(() => ShowNodeDetail(nodeId));
        }

        void ShowNodeDetail(string nodeId)
        {
            var tree = AscensionSkillTree.Instance;
            if (tree?.AllNodes == null) return;

            AscensionNode node = null;
            foreach (var n in tree.AllNodes)
            {
                if (n != null && n.nodeId == nodeId)
                {
                    node = n;
                    break;
                }
            }
            if (node == null) return;

            _selectedNodeId = nodeId;

            if (_nodeNameText != null) _nodeNameText.text = node.nodeName;
            if (_nodeDescText != null) _nodeDescText.text = node.description;
            if (_nodeCostText != null) _nodeCostText.text = $"Cost: {node.shardCost:N0} Shards";

            // Build effect text
            string effect = "";
            if (node.statBonuses != null)
            {
                for (int i = 0; i < node.statBonuses.Length; i++)
                    effect += $"+{node.statBonuses[i].percentValue}% {node.statBonuses[i].stat}\n";
            }
            if (node.goldBonusPercent > 0) effect += $"+{node.goldBonusPercent}% Gold\n";
            if (node.xpBonusPercent > 0) effect += $"+{node.xpBonusPercent}% XP\n";
            if (node.dropRateBonusPercent > 0) effect += $"+{node.dropRateBonusPercent}% Drop Rate\n";
            if (node.cooldownReductionPercent > 0) effect += $"+{node.cooldownReductionPercent}% CDR\n";
            if (node.stageClearSpeedBonusPercent > 0) effect += $"+{node.stageClearSpeedBonusPercent}% Stage Clear Speed\n";
            if (_nodeEffectText != null) _nodeEffectText.text = effect.TrimEnd('\n');

            var state = tree.GetNodeState(nodeId);
            if (_purchaseButton != null)
            {
                _purchaseButton.gameObject.SetActive(state == AscensionNodeState.Available);
                _purchaseButton.interactable = state == AscensionNodeState.Available;
            }

            if (_nodeDetailPanel != null) _nodeDetailPanel.SetActive(true);
        }

        void OnPurchaseClicked()
        {
            if (string.IsNullOrEmpty(_selectedNodeId)) return;
            AscensionSkillTree.Instance?.PurchaseNode(_selectedNodeId);
        }

        void OnNodePurchased(AscensionSkillNodePurchasedEvent evt)
        {
            RefreshDisplay();
            if (!string.IsNullOrEmpty(_selectedNodeId))
                ShowNodeDetail(_selectedNodeId);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
