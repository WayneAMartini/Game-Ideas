using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Islands;

namespace Ascendant.UI
{
    public class IslandMapUI : MonoBehaviour
    {
        [Header("Map Container")]
        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] RectTransform _mapContent;

        [Header("Island Node Prefab")]
        [SerializeField] GameObject _islandNodePrefab;

        [Header("Info Panel")]
        [SerializeField] GameObject _infoPanel;
        [SerializeField] TextMeshProUGUI _infoNameText;
        [SerializeField] TextMeshProUGUI _infoDescText;
        [SerializeField] TextMeshProUGUI _infoAffinityText;
        [SerializeField] TextMeshProUGUI _infoBossText;
        [SerializeField] TextMeshProUGUI _infoEffectText;
        [SerializeField] Button _infoCTAButton;
        [SerializeField] TextMeshProUGUI _infoCTAText;

        [Header("Visual")]
        [SerializeField] float _islandSpacing = 200f;
        [SerializeField] Image _currentBeacon;

        IslandNode[] _islandNodes;
        int _selectedIslandIndex = -1;

        void OnEnable()
        {
            EventBus.Subscribe<IslandUnlockedEvent>(OnIslandUnlocked);
            EventBus.Subscribe<IslandCompletedEvent>(OnIslandCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<IslandUnlockedEvent>(OnIslandUnlocked);
            EventBus.Unsubscribe<IslandCompletedEvent>(OnIslandCompleted);
        }

        void Start()
        {
            BuildMap();
            HideInfoPanel();
        }

        void BuildMap()
        {
            var manager = IslandManager.Instance;
            if (manager == null) return;

            var islands = manager.AllIslands;
            if (islands == null || islands.Count == 0) return;

            _islandNodes = new IslandNode[islands.Count];

            for (int i = 0; i < islands.Count; i++)
            {
                var island = islands[i];
                if (_islandNodePrefab == null) continue;

                var nodeObj = Instantiate(_islandNodePrefab, _mapContent);
                var rt = nodeObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Vertical layout - higher islands go higher
                    float x = (i % 2 == 0) ? -80f : 80f; // Zigzag pattern
                    float y = i * _islandSpacing;
                    rt.anchoredPosition = new Vector2(x, y);
                }

                var node = nodeObj.GetComponent<IslandNode>();
                if (node == null)
                    node = nodeObj.AddComponent<IslandNode>();

                node.Initialize(island, i, OnIslandNodeTapped);
                node.SetState(
                    manager.IsIslandCompleted(i),
                    manager.IsIslandUnlocked(i),
                    i == manager.CurrentIslandNumber - 1
                );

                _islandNodes[i] = node;
            }

            // Resize content to fit all nodes
            if (_mapContent != null)
            {
                float height = islands.Count * _islandSpacing + 200f;
                _mapContent.sizeDelta = new Vector2(_mapContent.sizeDelta.x, height);
            }

            // Scroll to current island
            ScrollToIsland(manager.CurrentIslandNumber - 1);
        }

        void OnIslandNodeTapped(int index)
        {
            _selectedIslandIndex = index;
            ShowInfoPanel(index);
        }

        void ShowInfoPanel(int index)
        {
            var manager = IslandManager.Instance;
            if (manager == null) return;

            var island = manager.GetIsland(index);
            if (island == null) return;

            if (_infoPanel != null) _infoPanel.SetActive(true);
            if (_infoNameText != null) _infoNameText.text = island.islandName;
            if (_infoDescText != null) _infoDescText.text = island.description;
            if (_infoAffinityText != null) _infoAffinityText.text = $"Affinity: {island.affinity}";
            if (_infoBossText != null) _infoBossText.text = $"Boss: {island.islandBossName}";
            if (_infoEffectText != null && island.biomeData != null)
                _infoEffectText.text = island.biomeData.effectDescription;

            bool isUnlocked = manager.IsIslandUnlocked(index);
            bool isCurrent = index == manager.CurrentIslandNumber - 1;

            if (_infoCTAButton != null)
            {
                _infoCTAButton.interactable = isUnlocked;
                if (_infoCTAText != null)
                {
                    if (!isUnlocked) _infoCTAText.text = "Locked";
                    else if (isCurrent) _infoCTAText.text = "Current Island";
                    else if (manager.IsIslandCompleted(index)) _infoCTAText.text = "Revisit";
                    else _infoCTAText.text = "Travel";
                }
            }
        }

        void HideInfoPanel()
        {
            if (_infoPanel != null)
                _infoPanel.SetActive(false);
        }

        public void OnTravelButtonClicked()
        {
            if (_selectedIslandIndex < 0) return;
            var manager = IslandManager.Instance;
            if (manager == null || !manager.IsIslandUnlocked(_selectedIslandIndex)) return;

            manager.SetCurrentIsland(_selectedIslandIndex);
            RefreshNodeStates();
            HideInfoPanel();
        }

        void ScrollToIsland(int index)
        {
            if (_scrollRect == null || _mapContent == null) return;
            if (_islandNodes == null || index < 0 || index >= _islandNodes.Length) return;

            float normalizedY = (float)index / Mathf.Max(1, _islandNodes.Length - 1);
            _scrollRect.verticalNormalizedPosition = normalizedY;
        }

        void RefreshNodeStates()
        {
            var manager = IslandManager.Instance;
            if (manager == null || _islandNodes == null) return;

            for (int i = 0; i < _islandNodes.Length; i++)
            {
                if (_islandNodes[i] == null) continue;
                _islandNodes[i].SetState(
                    manager.IsIslandCompleted(i),
                    manager.IsIslandUnlocked(i),
                    i == manager.CurrentIslandNumber - 1
                );
            }
        }

        void OnIslandUnlocked(IslandUnlockedEvent evt)
        {
            RefreshNodeStates();
        }

        void OnIslandCompleted(IslandCompletedEvent evt)
        {
            RefreshNodeStates();
        }
    }
}
