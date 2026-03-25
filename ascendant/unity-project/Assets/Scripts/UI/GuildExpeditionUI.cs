using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class GuildExpeditionUI : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] Transform _gridContainer;
        [SerializeField] GameObject _nodePrefab;
        [SerializeField] int _gridSize = 5;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI _clearsRemainingText;
        [SerializeField] TextMeshProUGUI _statusText;
        [SerializeField] TextMeshProUGUI _bossStatusText;

        [Header("Colors")]
        [SerializeField] Color _enemyColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField] Color _treasureColor = new(1f, 0.84f, 0f);
        [SerializeField] Color _eliteColor = new(0.6f, 0.2f, 0.8f);
        [SerializeField] Color _bossColor = new(1f, 0f, 0f);
        [SerializeField] Color _clearedColor = new(0.3f, 0.7f, 0.3f);
        [SerializeField] Color _lockedColor = new(0.3f, 0.3f, 0.3f);

        GameObject[,] _nodeObjects;

        void OnEnable()
        {
            EventBus.Subscribe<GuildExpeditionNodeClearedEvent>(OnNodeCleared);
            EventBus.Subscribe<GuildExpeditionBossUnlockedEvent>(OnBossUnlocked);
            RefreshGrid();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GuildExpeditionNodeClearedEvent>(OnNodeCleared);
            EventBus.Unsubscribe<GuildExpeditionBossUnlockedEvent>(OnBossUnlocked);
        }

        void OnNodeCleared(GuildExpeditionNodeClearedEvent evt) => RefreshGrid();
        void OnBossUnlocked(GuildExpeditionBossUnlockedEvent evt) => RefreshGrid();

        void RefreshGrid()
        {
            var expedition = GuildExpedition.Instance;
            if (expedition == null) return;

            expedition.RefreshGrid(() =>
            {
                BuildGrid(expedition.CurrentGrid);
                UpdateInfo();
            });
        }

        void BuildGrid(GuildExpeditionState grid)
        {
            ClearGrid();

            if (grid == null || _gridContainer == null || _nodePrefab == null) return;

            _nodeObjects = new GameObject[grid.GridWidth, grid.GridHeight];

            foreach (var node in grid.Nodes)
            {
                var go = Instantiate(_nodePrefab, _gridContainer);
                go.SetActive(true);
                _nodeObjects[node.X, node.Y] = go;

                var img = go.GetComponent<Image>();
                var text = go.GetComponentInChildren<TextMeshProUGUI>();

                if (node.Cleared)
                {
                    if (img != null) img.color = _clearedColor;
                    if (text != null) text.text = node.ClearedByName ?? "Cleared";
                }
                else if (node.Type == ExpeditionNodeType.Boss && !grid.BossUnlocked)
                {
                    if (img != null) img.color = _lockedColor;
                    if (text != null) text.text = "BOSS\n(Locked)";
                }
                else
                {
                    if (img != null) img.color = GetNodeColor(node.Type);
                    if (text != null) text.text = GetNodeLabel(node.Type);

                    // Add click handler
                    var btn = go.GetComponent<Button>();
                    if (btn == null) btn = go.AddComponent<Button>();

                    int x = node.X, y = node.Y;
                    btn.onClick.AddListener(() => OnNodeClicked(x, y));
                }
            }
        }

        void OnNodeClicked(int x, int y)
        {
            var expedition = GuildExpedition.Instance;
            if (expedition == null || !expedition.CanClearNode) return;

            expedition.ClearNode(x, y, success =>
            {
                if (success)
                    UpdateInfo();
            });
        }

        void UpdateInfo()
        {
            var expedition = GuildExpedition.Instance;
            if (expedition == null) return;

            if (_clearsRemainingText != null)
                _clearsRemainingText.text = $"Clears: {expedition.RemainingClears}/{expedition.NodesPerDay}";

            if (_statusText != null)
            {
                var grid = expedition.CurrentGrid;
                if (grid != null)
                {
                    int cleared = 0, total = 0;
                    foreach (var node in grid.Nodes)
                    {
                        if (node.Type != ExpeditionNodeType.Boss)
                        {
                            total++;
                            if (node.Cleared) cleared++;
                        }
                    }
                    _statusText.text = $"Progress: {cleared}/{total} nodes";
                }
            }

            if (_bossStatusText != null)
            {
                var grid = expedition.CurrentGrid;
                if (grid == null)
                    _bossStatusText.text = "";
                else if (grid.BossDefeated)
                    _bossStatusText.text = "Boss Defeated!";
                else if (grid.BossUnlocked)
                    _bossStatusText.text = "Boss Unlocked — Attack!";
                else
                    _bossStatusText.text = "Clear all nodes to unlock boss";
            }
        }

        Color GetNodeColor(ExpeditionNodeType type)
        {
            return type switch
            {
                ExpeditionNodeType.Enemy => _enemyColor,
                ExpeditionNodeType.Treasure => _treasureColor,
                ExpeditionNodeType.Elite => _eliteColor,
                ExpeditionNodeType.Boss => _bossColor,
                _ => _enemyColor
            };
        }

        static string GetNodeLabel(ExpeditionNodeType type)
        {
            return type switch
            {
                ExpeditionNodeType.Enemy => "Enemy",
                ExpeditionNodeType.Treasure => "Treasure",
                ExpeditionNodeType.Elite => "Elite",
                ExpeditionNodeType.Boss => "BOSS",
                _ => "?"
            };
        }

        void ClearGrid()
        {
            if (_gridContainer == null) return;
            for (int i = _gridContainer.childCount - 1; i >= 0; i--)
                Destroy(_gridContainer.GetChild(i).gameObject);
            _nodeObjects = null;
        }

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
