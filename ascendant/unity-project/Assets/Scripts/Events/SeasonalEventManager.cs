using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Islands;

namespace Ascendant.Events
{
    [Serializable]
    public class SeasonalEventSaveData
    {
        public string ActiveEventId;
        public long EventStartTimestampUnix;
        public long EventEndTimestampUnix;
        public int EventCurrency;
        public int EventStageProgress;
        public List<string> PurchasedShopItems = new();
        public List<EventQuestProgress> QuestProgress = new();
        public bool EventBossDefeated;
    }

    [Serializable]
    public class EventQuestProgress
    {
        public string QuestId;
        public int CurrentProgress;
        public bool Completed;
    }

    public class SeasonalEventManager : MonoBehaviour
    {
        public static SeasonalEventManager Instance { get; private set; }

        [Header("Event Rotation")]
        [SerializeField] List<EventConfig> _eventConfigs;

        [Header("References")]
        [SerializeField] EnemySpawner _spawner;

        EventConfig _activeEvent;
        long _eventStartTimestamp;
        long _eventEndTimestamp;
        int _eventCurrency;
        int _eventStageProgress;
        HashSet<string> _purchasedItems = new();
        Dictionary<string, EventQuestProgress> _questProgress = new();
        bool _eventBossDefeated;
        bool _inEventCombat;

        public EventConfig ActiveEvent => _activeEvent;
        public bool IsEventActive => _activeEvent != null && DateTimeOffset.UtcNow.ToUnixTimeSeconds() < _eventEndTimestamp;
        public int EventCurrency => _eventCurrency;
        public int EventStageProgress => _eventStageProgress;
        public bool EventBossDefeated => _eventBossDefeated;
        public bool InEventCombat => _inEventCombat;

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
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void Start()
        {
            CheckEventSchedule();
        }

        void CheckEventSchedule()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (_activeEvent != null && now >= _eventEndTimestamp)
            {
                EndEvent();
            }
        }

        public void StartEvent(EventConfig config)
        {
            if (config == null) return;
            if (_activeEvent != null) EndEvent();

            _activeEvent = config;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _eventStartTimestamp = now;
            _eventEndTimestamp = now + config.durationDays * 86400L;
            _eventCurrency = 0;
            _eventStageProgress = 0;
            _purchasedItems.Clear();
            _eventBossDefeated = false;
            _inEventCombat = false;

            // Initialize quests
            _questProgress.Clear();
            if (config.eventQuests != null)
            {
                foreach (var quest in config.eventQuests)
                {
                    _questProgress[quest.questId] = new EventQuestProgress
                    {
                        QuestId = quest.questId,
                        CurrentProgress = 0,
                        Completed = false
                    };
                }
            }

            EventBus.Publish(new SeasonalEventStartedEvent
            {
                EventId = config.eventId,
                EventName = config.eventName,
                Theme = config.theme
            });

            Debug.Log($"[SeasonalEvent] Started: {config.eventName} ({config.theme})");
        }

        // For local config, start the next event in rotation
        public void StartNextEvent()
        {
            if (_eventConfigs == null || _eventConfigs.Count == 0) return;

            int nextIndex = 0;
            if (_activeEvent != null)
            {
                for (int i = 0; i < _eventConfigs.Count; i++)
                {
                    if (_eventConfigs[i].eventId == _activeEvent.eventId)
                    {
                        nextIndex = (i + 1) % _eventConfigs.Count;
                        break;
                    }
                }
            }

            StartEvent(_eventConfigs[nextIndex]);
        }

        void EndEvent()
        {
            if (_activeEvent == null) return;

            EventBus.Publish(new SeasonalEventEndedEvent
            {
                EventId = _activeEvent.eventId,
                EventName = _activeEvent.eventName,
                TotalCurrencyEarned = _eventCurrency,
                StagesCompleted = _eventStageProgress
            });

            Debug.Log($"[SeasonalEvent] Ended: {_activeEvent.eventName}");
            _activeEvent = null;
            _inEventCombat = false;
        }

        public long GetTimeRemaining()
        {
            if (!IsEventActive) return 0;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Mathf.Max(0, (int)(_eventEndTimestamp - now));
        }

        // --- Event Island Combat ---

        public void EnterEventIsland()
        {
            if (!IsEventActive) return;

            _inEventCombat = true;
            GameManager.Instance?.SetState(GameState.SeasonalEvent);
            SpawnEventStage();
        }

        void SpawnEventStage()
        {
            EnemyManager.Instance?.ClearAll();

            int stage = _eventStageProgress + 1;
            int stageCount = _activeEvent?.eventStageCount ?? 20;

            if (stage > stageCount)
            {
                // All stages complete
                _inEventCombat = false;
                GameManager.Instance?.SetState(GameState.Combat);
                return;
            }

            // Event boss on final stage
            if (stage == stageCount && !_eventBossDefeated)
            {
                // Boss fight - spawn tougher enemies
                int bossLevel = 80 + stage * 2;
                _spawner?.SpawnWave(bossLevel);
            }
            else
            {
                int enemyLevel = 30 + stage * 3;
                if (_activeEvent?.eventEnemies != null && _activeEvent.eventEnemies.Count > 0)
                    _spawner?.SpawnWaveWithEnemies(enemyLevel, _activeEvent.eventEnemies);
                else
                    _spawner?.SpawnWave(enemyLevel);
            }
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (!_inEventCombat) return;
            if (GameManager.Instance?.CurrentState != GameState.SeasonalEvent) return;

            // Award event currency on kill
            int currencyDrop = Mathf.Max(1, Mathf.RoundToInt(_activeEvent?.eventCurrencyDropRate ?? 1f));
            _eventCurrency += currencyDrop;

            // Update quest progress for kills
            UpdateQuestProgress("event_kills", 1);

            if (EnemyManager.Instance != null && EnemyManager.Instance.AliveCount <= 0)
            {
                OnEventStageClear();
            }
        }

        void OnEventStageClear()
        {
            _eventStageProgress++;

            int stageCount = _activeEvent?.eventStageCount ?? 20;
            if (_eventStageProgress >= stageCount)
            {
                _eventBossDefeated = true;
            }

            EventBus.Publish(new SeasonalEventStageCompletedEvent
            {
                StageNumber = _eventStageProgress,
                TotalStages = stageCount,
                EventCurrencyEarned = Mathf.RoundToInt(_activeEvent?.eventCurrencyDropRate ?? 1f) * 5
            });

            // Award bonus event currency per stage clear
            _eventCurrency += 10;
            UpdateQuestProgress("event_stages", 1);

            // Continue to next stage or end
            if (_eventStageProgress < stageCount)
                SpawnEventStage();
            else
            {
                _inEventCombat = false;
                GameManager.Instance?.SetState(GameState.Combat);
            }
        }

        public void LeaveEventIsland()
        {
            _inEventCombat = false;
            GameManager.Instance?.SetState(GameState.Combat);
        }

        // --- Event Shop ---

        public bool CanPurchaseItem(EventShopItem item)
        {
            if (item == null) return false;
            if (_eventCurrency < item.eventCurrencyCost) return false;
            if (item.isLimited && _purchasedItems.Contains(item.itemId)) return false;
            return true;
        }

        public bool PurchaseItem(EventShopItem item)
        {
            if (!CanPurchaseItem(item)) return false;

            _eventCurrency -= item.eventCurrencyCost;
            _purchasedItems.Add(item.itemId);

            // Award the item's rewards
            if (item.rewardAmount > 0)
            {
                Economy.CurrencyManager.Instance?.AddCurrency(item.rewardCurrencyType, item.rewardAmount);
            }

            EventBus.Publish(new EventShopPurchaseEvent
            {
                ItemId = item.itemId,
                ItemName = item.itemName,
                Cost = item.eventCurrencyCost
            });

            return true;
        }

        public bool IsItemPurchased(string itemId)
        {
            return _purchasedItems.Contains(itemId);
        }

        // --- Event Quests ---

        public void UpdateQuestProgress(string questId, int amount)
        {
            if (!_questProgress.TryGetValue(questId, out var progress)) return;
            if (progress.Completed) return;

            progress.CurrentProgress += amount;

            var questDef = GetQuestDef(questId);
            if (questDef != null && progress.CurrentProgress >= questDef.requiredProgress)
            {
                progress.Completed = true;
                _eventCurrency += questDef.eventCurrencyReward;

                EventBus.Publish(new EventQuestCompletedEvent
                {
                    QuestId = questId,
                    QuestName = questDef.questName,
                    CurrencyReward = questDef.eventCurrencyReward
                });
            }
        }

        EventQuestDef GetQuestDef(string questId)
        {
            if (_activeEvent?.eventQuests == null) return null;
            foreach (var q in _activeEvent.eventQuests)
                if (q.questId == questId) return q;
            return null;
        }

        public List<EventQuestProgress> GetAllQuestProgress()
        {
            return new List<EventQuestProgress>(_questProgress.Values);
        }

        // --- Save/Load ---

        public SeasonalEventSaveData GatherSaveData()
        {
            var data = new SeasonalEventSaveData
            {
                ActiveEventId = _activeEvent?.eventId ?? "",
                EventStartTimestampUnix = _eventStartTimestamp,
                EventEndTimestampUnix = _eventEndTimestamp,
                EventCurrency = _eventCurrency,
                EventStageProgress = _eventStageProgress,
                PurchasedShopItems = new List<string>(_purchasedItems),
                EventBossDefeated = _eventBossDefeated
            };

            data.QuestProgress = new List<EventQuestProgress>(_questProgress.Values);
            return data;
        }

        public void LoadSaveData(SeasonalEventSaveData data)
        {
            if (data == null) return;

            // Find the active event config
            if (!string.IsNullOrEmpty(data.ActiveEventId) && _eventConfigs != null)
            {
                foreach (var config in _eventConfigs)
                {
                    if (config.eventId == data.ActiveEventId)
                    {
                        _activeEvent = config;
                        break;
                    }
                }
            }

            _eventStartTimestamp = data.EventStartTimestampUnix;
            _eventEndTimestamp = data.EventEndTimestampUnix;
            _eventCurrency = data.EventCurrency;
            _eventStageProgress = data.EventStageProgress;
            _eventBossDefeated = data.EventBossDefeated;

            _purchasedItems.Clear();
            if (data.PurchasedShopItems != null)
                foreach (var id in data.PurchasedShopItems)
                    _purchasedItems.Add(id);

            _questProgress.Clear();
            if (data.QuestProgress != null)
                foreach (var qp in data.QuestProgress)
                    _questProgress[qp.QuestId] = qp;

            CheckEventSchedule();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
