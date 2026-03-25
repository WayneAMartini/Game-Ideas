using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject _skillTreePanel;
        [SerializeField] RectTransform _treeContainer;
        [SerializeField] GameObject _nodeButtonPrefab;
        [SerializeField] GameObject _connectionLinePrefab;

        [Header("Branch Tabs")]
        [SerializeField] Button[] _branchTabs;
        [SerializeField] TextMeshProUGUI[] _branchTabLabels;

        [Header("Node Detail")]
        [SerializeField] GameObject _nodeDetailPanel;
        [SerializeField] TextMeshProUGUI _nodeNameText;
        [SerializeField] TextMeshProUGUI _nodeDescriptionText;
        [SerializeField] TextMeshProUGUI _nodeCostText;
        [SerializeField] TextMeshProUGUI _nodeStatsText;
        [SerializeField] Button _investButton;
        [SerializeField] TextMeshProUGUI _investButtonText;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI _pointsText;
        [SerializeField] Button _respecButton;
        [SerializeField] TextMeshProUGUI _respecCostText;

        [Header("Pinch Zoom")]
        [SerializeField] float _minZoom = 0.5f;
        [SerializeField] float _maxZoom = 2f;

        int _selectedHeroSlot;
        string _selectedClassId;
        int _currentBranch;
        string _selectedNodeId;
        float _currentZoom = 1f;

        static readonly Color LockedColor = new(0.4f, 0.4f, 0.4f);
        static readonly Color AvailableColor = new(0.3f, 0.5f, 1f);
        static readonly Color PurchasedColor = new(1f, 0.85f, 0f);
        static readonly Color CapstoneColor = new(1f, 0.5f, 0f);

        void OnEnable()
        {
            EventBus.Subscribe<SkillTreeChangedEvent>(OnSkillTreeChanged);
            EventBus.Subscribe<SkillPointsChangedEvent>(OnPointsChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<SkillTreeChangedEvent>(OnSkillTreeChanged);
            EventBus.Unsubscribe<SkillPointsChangedEvent>(OnPointsChanged);
        }

        void OnSkillTreeChanged(SkillTreeChangedEvent evt)
        {
            if (evt.HeroSlot == _selectedHeroSlot)
                RefreshTree();
        }

        void OnPointsChanged(SkillPointsChangedEvent evt)
        {
            if (evt.HeroSlot == _selectedHeroSlot)
                UpdatePointsDisplay();
        }

        public void Open(int heroSlot, string classId)
        {
            _selectedHeroSlot = heroSlot;
            _selectedClassId = classId;
            _currentBranch = 0;

            if (_skillTreePanel != null) _skillTreePanel.SetActive(true);

            SetupBranchTabs();
            RefreshTree();
            UpdatePointsDisplay();
        }

        public void Close()
        {
            if (_skillTreePanel != null) _skillTreePanel.SetActive(false);
            if (_nodeDetailPanel != null) _nodeDetailPanel.SetActive(false);
        }

        void SetupBranchTabs()
        {
            var system = SkillTreeSystem.Instance;
            if (system == null) return;

            var tree = system.GetSkillTree(_selectedClassId);
            if (tree?.branches == null) return;

            for (int i = 0; i < _branchTabs.Length && i < tree.branches.Length; i++)
            {
                int branchIdx = i;
                if (_branchTabs[i] != null)
                {
                    _branchTabs[i].gameObject.SetActive(true);
                    _branchTabs[i].onClick.RemoveAllListeners();
                    _branchTabs[i].onClick.AddListener(() => SelectBranch(branchIdx));
                }
                if (_branchTabLabels != null && i < _branchTabLabels.Length && _branchTabLabels[i] != null)
                    _branchTabLabels[i].text = tree.branches[i].branchName;
            }
        }

        void SelectBranch(int branchIndex)
        {
            _currentBranch = branchIndex;
            RefreshTree();
        }

        void RefreshTree()
        {
            if (_treeContainer == null) return;

            // Clear existing nodes
            for (int i = _treeContainer.childCount - 1; i >= 0; i--)
                Destroy(_treeContainer.GetChild(i).gameObject);

            var system = SkillTreeSystem.Instance;
            if (system == null) return;

            var tree = system.GetSkillTree(_selectedClassId);
            if (tree?.branches == null || _currentBranch >= tree.branches.Length) return;

            var branch = tree.branches[_currentBranch];
            if (branch?.nodes == null) return;

            // Create node buttons
            for (int n = 0; n < branch.nodes.Length; n++)
            {
                var node = branch.nodes[n];
                CreateNodeButton(node, system);
            }

            // Draw connection lines
            for (int n = 0; n < branch.nodes.Length; n++)
            {
                var node = branch.nodes[n];
                if (node.prerequisiteNodeIds == null) continue;
                for (int p = 0; p < node.prerequisiteNodeIds.Length; p++)
                    DrawConnection(node.prerequisiteNodeIds[p], node.nodeId, branch);
            }
        }

        void CreateNodeButton(SkillNodeDefinition node, SkillTreeSystem system)
        {
            if (_nodeButtonPrefab == null) return;

            var go = Instantiate(_nodeButtonPrefab, _treeContainer);
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = node.treePosition * 100f; // scale positions

            var state = system.GetNodeState(_selectedHeroSlot, node.nodeId, _selectedClassId);
            var color = state switch
            {
                SkillNodeState.Purchased => node.isCapstone ? CapstoneColor : PurchasedColor,
                SkillNodeState.Available => AvailableColor,
                _ => LockedColor
            };

            var img = go.GetComponentInChildren<Image>();
            if (img != null) img.color = color;

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = node.nodeName;

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                string captured = node.nodeId;
                btn.onClick.AddListener(() => SelectNode(captured));
            }
        }

        void DrawConnection(string fromId, string toId, SkillBranch branch)
        {
            if (_connectionLinePrefab == null || _treeContainer == null) return;

            Vector2 fromPos = Vector2.zero, toPos = Vector2.zero;
            bool foundFrom = false, foundTo = false;

            for (int i = 0; i < branch.nodes.Length; i++)
            {
                if (branch.nodes[i].nodeId == fromId) { fromPos = branch.nodes[i].treePosition * 100f; foundFrom = true; }
                if (branch.nodes[i].nodeId == toId) { toPos = branch.nodes[i].treePosition * 100f; foundTo = true; }
            }

            if (!foundFrom || !foundTo) return;

            var lineGO = Instantiate(_connectionLinePrefab, _treeContainer);
            lineGO.transform.SetAsFirstSibling(); // lines behind nodes
            var lineRect = lineGO.GetComponent<RectTransform>();
            if (lineRect != null)
            {
                var mid = (fromPos + toPos) / 2f;
                lineRect.anchoredPosition = mid;
                var diff = toPos - fromPos;
                lineRect.sizeDelta = new Vector2(diff.magnitude, 4f);
                float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                lineRect.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        void SelectNode(string nodeId)
        {
            _selectedNodeId = nodeId;
            ShowNodeDetail(nodeId);
        }

        void ShowNodeDetail(string nodeId)
        {
            var system = SkillTreeSystem.Instance;
            if (system == null) return;

            var tree = system.GetSkillTree(_selectedClassId);
            if (tree == null) return;

            SkillNodeDefinition node = null;
            for (int b = 0; b < tree.branches.Length; b++)
            {
                if (tree.branches[b]?.nodes == null) continue;
                for (int n = 0; n < tree.branches[b].nodes.Length; n++)
                {
                    if (tree.branches[b].nodes[n].nodeId == nodeId)
                    {
                        node = tree.branches[b].nodes[n];
                        break;
                    }
                }
                if (node != null) break;
            }

            if (node == null) return;

            if (_nodeDetailPanel != null) _nodeDetailPanel.SetActive(true);
            if (_nodeNameText != null) _nodeNameText.text = node.nodeName;
            if (_nodeDescriptionText != null) _nodeDescriptionText.text = node.description;
            if (_nodeCostText != null) _nodeCostText.text = $"Cost: {node.cost} SP";

            if (_nodeStatsText != null && node.statBonuses != null)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < node.statBonuses.Length; i++)
                    sb.AppendLine($"+{node.statBonuses[i].value:F1} {node.statBonuses[i].stat}");
                _nodeStatsText.text = sb.ToString();
            }

            var state = system.GetNodeState(_selectedHeroSlot, nodeId, _selectedClassId);
            if (_investButton != null)
                _investButton.interactable = state == SkillNodeState.Available;
            if (_investButtonText != null)
            {
                _investButtonText.text = state switch
                {
                    SkillNodeState.Purchased => "Purchased",
                    SkillNodeState.Available => "Invest",
                    _ => "Locked"
                };
            }
        }

        public void OnInvestClicked()
        {
            if (string.IsNullOrEmpty(_selectedNodeId)) return;
            SkillTreeSystem.Instance?.InvestInNode(_selectedHeroSlot, _selectedNodeId, _selectedClassId);
            ShowNodeDetail(_selectedNodeId);
            RefreshTree();
        }

        public void OnRespecClicked()
        {
            SkillTreeSystem.Instance?.Respec(_selectedHeroSlot);
            RefreshTree();
            UpdatePointsDisplay();
        }

        void UpdatePointsDisplay()
        {
            var system = SkillTreeSystem.Instance;
            if (system == null) return;

            int available = system.GetAvailablePoints(_selectedHeroSlot);
            int spent = system.GetSpentPoints(_selectedHeroSlot);

            if (_pointsText != null)
                _pointsText.text = $"SP: {available} available / {spent} spent";
        }

        void Update()
        {
            HandlePinchZoom();
        }

        void HandlePinchZoom()
        {
            if (Input.touchCount != 2 || _treeContainer == null) return;

            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);

            if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
                float curDist = (t0.position - t1.position).magnitude;

                if (prevDist > 0f)
                {
                    float delta = curDist / prevDist;
                    _currentZoom = Mathf.Clamp(_currentZoom * delta, _minZoom, _maxZoom);
                    _treeContainer.localScale = Vector3.one * _currentZoom;
                }
            }
        }
    }
}
