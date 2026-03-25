using System;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    [Serializable]
    public class GuildExpeditionSaveData
    {
        public int NodesClearedToday;
        public long LastClearDayUnix;
    }

    public class GuildExpedition : MonoBehaviour
    {
        public static GuildExpedition Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _nodesPerDay = 3;
        [SerializeField] int _gridSize = 5;

        GuildExpeditionState _currentGrid;
        int _nodesClearedToday;
        long _lastClearDayUnix;

        public GuildExpeditionState CurrentGrid => _currentGrid;
        public int NodesClearedToday => _nodesClearedToday;
        public int NodesPerDay => _nodesPerDay;
        public int RemainingClears => Mathf.Max(0, _nodesPerDay - _nodesClearedToday);
        public bool CanClearNode => RemainingClears > 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            var config = FirebaseManager.Instance?.Config;
            if (config != null)
            {
                _nodesPerDay = config.guildExpeditionNodesPerDay;
                _gridSize = config.guildExpeditionGridSize;
            }
        }

        void Start()
        {
            CheckDayReset();
        }

        void CheckDayReset()
        {
            long today = DateTimeOffset.UtcNow.Date.ToUnixTimeSeconds();
            if (_lastClearDayUnix < today)
            {
                _nodesClearedToday = 0;
                _lastClearDayUnix = today;
            }
        }

        public void RefreshGrid(Action callback = null)
        {
            var guildManager = GuildManager.Instance;
            if (guildManager == null || !guildManager.IsInGuild)
            {
                _currentGrid = null;
                callback?.Invoke();
                return;
            }

            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke();
                return;
            }

            service.GetExpeditionGrid(grid =>
            {
                _currentGrid = grid;
                callback?.Invoke();
            });
        }

        public void ClearNode(int x, int y, Action<bool> callback)
        {
            CheckDayReset();

            if (!CanClearNode)
            {
                Debug.Log("[GuildExpedition] No more clears remaining today");
                callback?.Invoke(false);
                return;
            }

            if (_currentGrid == null)
            {
                callback?.Invoke(false);
                return;
            }

            // Validate node exists and isn't cleared
            var node = _currentGrid.Nodes.Find(n => n.X == x && n.Y == y);
            if (node == null || node.Cleared)
            {
                callback?.Invoke(false);
                return;
            }

            // Don't allow clearing boss node unless it's unlocked
            if (node.Type == ExpeditionNodeType.Boss && !_currentGrid.BossUnlocked)
            {
                Debug.Log("[GuildExpedition] Boss node is locked — clear all other nodes first");
                callback?.Invoke(false);
                return;
            }

            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(false);
                return;
            }

            service.ClearExpeditionNode(x, y, success =>
            {
                if (success)
                {
                    _nodesClearedToday++;
                    _lastClearDayUnix = DateTimeOffset.UtcNow.Date.ToUnixTimeSeconds();

                    // Award rewards based on node type
                    AwardNodeReward(node.Type);

                    EventBus.Publish(new GuildExpeditionNodeClearedEvent
                    {
                        X = x,
                        Y = y,
                        ClearedByName = AuthManager.Instance?.DisplayName ?? "Player"
                    });

                    // Refresh grid to get updated state
                    RefreshGrid();

                    // Check if boss is now unlocked
                    if (_currentGrid != null && _currentGrid.BossUnlocked)
                    {
                        EventBus.Publish(new GuildExpeditionBossUnlockedEvent());
                    }
                }

                callback?.Invoke(success);
            });
        }

        void AwardNodeReward(ExpeditionNodeType type)
        {
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null) return;

            switch (type)
            {
                case ExpeditionNodeType.Enemy:
                    currency.AddCurrency(CurrencyType.GuildCoins, 50);
                    currency.AddGold(500);
                    break;
                case ExpeditionNodeType.Treasure:
                    currency.AddCurrency(CurrencyType.GuildCoins, 100);
                    currency.AddGold(2000);
                    currency.AddCurrency(CurrencyType.Stardust, 30);
                    break;
                case ExpeditionNodeType.Elite:
                    currency.AddCurrency(CurrencyType.GuildCoins, 150);
                    currency.AddGold(3000);
                    break;
                case ExpeditionNodeType.Boss:
                    currency.AddCurrency(CurrencyType.GuildCoins, 500);
                    currency.AddGold(10000);
                    currency.AddCurrency(CurrencyType.Stardust, 100);
                    currency.AddCurrency(CurrencyType.AetherCrystals, 20);
                    break;
            }
        }

        // Save/Load
        public GuildExpeditionSaveData GatherSaveData()
        {
            return new GuildExpeditionSaveData
            {
                NodesClearedToday = _nodesClearedToday,
                LastClearDayUnix = _lastClearDayUnix
            };
        }

        public void LoadSaveData(GuildExpeditionSaveData data)
        {
            if (data == null) return;
            _nodesClearedToday = data.NodesClearedToday;
            _lastClearDayUnix = data.LastClearDayUnix;
            CheckDayReset();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
