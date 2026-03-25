using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    [Serializable]
    public class GuildSaveData
    {
        public string GuildId;
        public string GuildName;
        public int DailyContribution;
        public long LastContributionDayUnix;
    }

    public class GuildManager : MonoBehaviour
    {
        public static GuildManager Instance { get; private set; }

        // Guild Tech Tree definitions
        public static readonly string[] TechIds = { "atk", "def", "hp", "gold_rate", "xp_rate" };
        public static readonly string[] TechNames = { "Attack", "Defense", "Health", "Gold Rate", "XP Rate" };
        public const int MaxTechLevel = 10;
        public const int ContributionPerLevel = 1000; // Guild coins per level

        GuildInfo _currentGuild;
        string _guildId;
        List<GuildChatMessage> _chatCache = new();
        float _chatRefreshTimer;
        const float ChatRefreshInterval = 15f;

        public GuildInfo CurrentGuild => _currentGuild;
        public bool IsInGuild => !string.IsNullOrEmpty(_guildId);
        public string GuildId => _guildId;
        public IReadOnlyList<GuildChatMessage> ChatMessages => _chatCache;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            if (!IsInGuild) return;

            _chatRefreshTimer += Time.deltaTime;
            if (_chatRefreshTimer >= ChatRefreshInterval)
            {
                _chatRefreshTimer = 0f;
                RefreshChat();
            }
        }

        public void CreateGuild(string name, string description, Action<bool> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(false);
                return;
            }

            service.CreateGuild(name, description, (success, guildId) =>
            {
                if (success)
                {
                    _guildId = guildId;
                    RefreshGuildInfo();

                    EventBus.Publish(new GuildJoinedEvent
                    {
                        GuildId = guildId,
                        GuildName = name
                    });
                }
                callback?.Invoke(success);
            });
        }

        public void JoinGuild(string guildId, Action<bool> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(false);
                return;
            }

            service.JoinGuild(guildId, success =>
            {
                if (success)
                {
                    _guildId = guildId;
                    RefreshGuildInfo();
                }
                callback?.Invoke(success);
            });
        }

        public void LeaveGuild(Action<bool> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(false);
                return;
            }

            service.LeaveGuild(success =>
            {
                if (success)
                {
                    _guildId = null;
                    _currentGuild = null;
                    _chatCache.Clear();
                    EventBus.Publish(new GuildLeftEvent());
                }
                callback?.Invoke(success);
            });
        }

        public void RefreshGuildInfo()
        {
            if (string.IsNullOrEmpty(_guildId)) return;

            var service = FirebaseManager.Instance?.Service;
            service?.GetGuildInfo(_guildId, info =>
            {
                _currentGuild = info;

                if (info != null)
                {
                    EventBus.Publish(new GuildJoinedEvent
                    {
                        GuildId = info.GuildId,
                        GuildName = info.GuildName
                    });
                }
            });
        }

        public void SearchGuilds(string query, Action<List<GuildInfo>> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(new List<GuildInfo>());
                return;
            }

            service.SearchGuilds(query, callback);
        }

        public void ContributeToTech(string techId, int guildCoinsAmount, Action<bool> callback)
        {
            if (!IsInGuild || _currentGuild == null)
            {
                callback?.Invoke(false);
                return;
            }

            // Check if player can afford it
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null || !currency.CanAfford(CurrencyType.GuildCoins, guildCoinsAmount))
            {
                callback?.Invoke(false);
                return;
            }

            // Check tech level cap
            int currentLevel = GetTechLevel(techId);
            if (currentLevel >= MaxTechLevel)
            {
                callback?.Invoke(false);
                return;
            }

            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                // Queue offline
                OfflineQueue.Instance?.Enqueue(new OfflineAction
                {
                    Type = OfflineActionType.GuildContribution,
                    Payload = $"{techId}|{guildCoinsAmount}"
                });

                currency.SpendCurrency(CurrencyType.GuildCoins, guildCoinsAmount);
                callback?.Invoke(true);
                return;
            }

            currency.SpendCurrency(CurrencyType.GuildCoins, guildCoinsAmount);

            service.ContributeToTechTree(techId, guildCoinsAmount, success =>
            {
                if (success)
                {
                    RefreshGuildInfo();
                    EventBus.Publish(new GuildTechUpgradedEvent
                    {
                        TechId = techId,
                        NewLevel = GetTechLevel(techId)
                    });
                }
                callback?.Invoke(success);
            });
        }

        public int GetTechLevel(string techId)
        {
            if (_currentGuild?.TechTreeLevels == null) return 0;
            if (_currentGuild.TechTreeLevels.TryGetValue(techId, out int points))
                return Mathf.Min(points / ContributionPerLevel, MaxTechLevel);
            return 0;
        }

        public float GetTechBonusPercent(string techId)
        {
            // Each level = 5% bonus
            return GetTechLevel(techId) * 5f;
        }

        public float GetAtkBonus() => GetTechBonusPercent("atk");
        public float GetDefBonus() => GetTechBonusPercent("def");
        public float GetHpBonus() => GetTechBonusPercent("hp");
        public float GetGoldRateBonus() => GetTechBonusPercent("gold_rate");
        public float GetXpRateBonus() => GetTechBonusPercent("xp_rate");

        // Chat
        public void SendChat(string message, Action<bool> callback)
        {
            if (!IsInGuild || string.IsNullOrEmpty(message))
            {
                callback?.Invoke(false);
                return;
            }

            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                OfflineQueue.Instance?.Enqueue(new OfflineAction
                {
                    Type = OfflineActionType.GuildChat,
                    Payload = message
                });
                callback?.Invoke(true);
                return;
            }

            service.SendGuildChat(message, success =>
            {
                if (success) RefreshChat();
                callback?.Invoke(success);
            });
        }

        public void RefreshChat()
        {
            if (!IsInGuild) return;

            var service = FirebaseManager.Instance?.Service;
            service?.GetGuildChat(50, messages =>
            {
                if (messages != null)
                    _chatCache = messages;
            });
        }

        // Save/Load
        public GuildSaveData GatherSaveData()
        {
            return new GuildSaveData
            {
                GuildId = _guildId ?? "",
                GuildName = _currentGuild?.GuildName ?? ""
            };
        }

        public void LoadSaveData(GuildSaveData data)
        {
            if (data == null || string.IsNullOrEmpty(data.GuildId)) return;
            _guildId = data.GuildId;
            RefreshGuildInfo();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
