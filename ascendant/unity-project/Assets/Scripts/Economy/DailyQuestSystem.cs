using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Economy
{
    [Serializable]
    public class QuestProgress
    {
        public string QuestId;
        public QuestType QuestType;
        public string QuestName;
        public int CurrentProgress;
        public int RequiredProgress;
        public bool IsCompleted;
        public bool IsCollected;
        public bool IsWeekly;
        public int BattlePassXP;
        public int GoldReward;
    }

    [Serializable]
    public class QuestSaveData
    {
        public long LastDailyRefreshUnix;
        public long LastWeeklyRefreshUnix;
        public List<QuestProgress> ActiveQuests = new();
    }

    public class DailyQuestSystem : MonoBehaviour
    {
        public static DailyQuestSystem Instance { get; private set; }

        [SerializeField] List<QuestData> _dailyQuestPool = new();
        [SerializeField] List<QuestData> _weeklyQuestPool = new();
        [SerializeField] int _dailyQuestCount = 5;
        [SerializeField] int _weeklyQuestCount = 3;
        [SerializeField] int _refreshHour = 5; // 5 AM local time

        readonly List<QuestProgress> _activeQuests = new();
        DateTime _lastDailyRefresh;
        DateTime _lastWeeklyRefresh;

        public IReadOnlyList<QuestProgress> ActiveQuests => _activeQuests;

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
            CheckRefresh();
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<WaveClearedEvent>(OnWaveCleared);
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<ExpeditionCollectedEvent>(OnExpeditionCollected);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<WaveClearedEvent>(OnWaveCleared);
            EventBus.Unsubscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<ExpeditionCollectedEvent>(OnExpeditionCollected);
        }

        void CheckRefresh()
        {
            var now = DateTime.Now;
            var todayRefreshTime = new DateTime(now.Year, now.Month, now.Day, _refreshHour, 0, 0);
            if (now.Hour < _refreshHour)
                todayRefreshTime = todayRefreshTime.AddDays(-1);

            // Daily refresh
            if (_lastDailyRefresh < todayRefreshTime)
            {
                RefreshDailyQuests();
                _lastDailyRefresh = todayRefreshTime;
            }

            // Weekly refresh (Monday)
            var lastMonday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Sunday)
                lastMonday = lastMonday.AddDays(-7);
            var weeklyRefreshTime = new DateTime(lastMonday.Year, lastMonday.Month, lastMonday.Day, _refreshHour, 0, 0);

            if (_lastWeeklyRefresh < weeklyRefreshTime)
            {
                RefreshWeeklyQuests();
                _lastWeeklyRefresh = weeklyRefreshTime;
            }
        }

        void RefreshDailyQuests()
        {
            // Remove old daily quests
            _activeQuests.RemoveAll(q => !q.IsWeekly);

            // Generate new dailies from pool
            var shuffled = new List<QuestData>(_dailyQuestPool);
            ShuffleList(shuffled);

            int count = Mathf.Min(_dailyQuestCount, shuffled.Count);
            for (int i = 0; i < count; i++)
            {
                var quest = shuffled[i];
                _activeQuests.Add(new QuestProgress
                {
                    QuestId = quest.questId,
                    QuestType = quest.questType,
                    QuestName = quest.questName,
                    CurrentProgress = 0,
                    RequiredProgress = quest.requiredAmount,
                    IsCompleted = false,
                    IsCollected = false,
                    IsWeekly = false,
                    BattlePassXP = quest.battlePassXP,
                    GoldReward = quest.goldReward
                });
            }

            // If pool is empty, generate default quests
            if (count == 0)
                GenerateDefaultDailyQuests();

            EventBus.Publish(new DailyQuestsRefreshedEvent());
        }

        void RefreshWeeklyQuests()
        {
            _activeQuests.RemoveAll(q => q.IsWeekly);

            var shuffled = new List<QuestData>(_weeklyQuestPool);
            ShuffleList(shuffled);

            int count = Mathf.Min(_weeklyQuestCount, shuffled.Count);
            for (int i = 0; i < count; i++)
            {
                var quest = shuffled[i];
                _activeQuests.Add(new QuestProgress
                {
                    QuestId = quest.questId,
                    QuestType = quest.questType,
                    QuestName = quest.questName,
                    CurrentProgress = 0,
                    RequiredProgress = quest.requiredAmount,
                    IsCompleted = false,
                    IsCollected = false,
                    IsWeekly = true,
                    BattlePassXP = quest.battlePassXP,
                    GoldReward = quest.goldReward
                });
            }

            if (count == 0)
                GenerateDefaultWeeklyQuests();

            EventBus.Publish(new WeeklyQuestsRefreshedEvent());
        }

        void GenerateDefaultDailyQuests()
        {
            _activeQuests.AddRange(new[]
            {
                CreateQuest("daily_kill_50", "Slay 50 Enemies", QuestType.KillEnemies, 50, false, 50, 1000),
                CreateQuest("daily_stages_10", "Clear 10 Stages", QuestType.ClearStages, 10, false, 50, 2000),
                CreateQuest("daily_abilities_20", "Use 20 Abilities", QuestType.UseAbilities, 20, false, 50, 1500),
                CreateQuest("daily_gold_10k", "Collect 10,000 Gold", QuestType.CollectGold, 10000, false, 50, 0),
                CreateQuest("daily_expedition_1", "Complete 1 Expedition", QuestType.CompleteExpeditions, 1, false, 50, 3000),
            });
        }

        void GenerateDefaultWeeklyQuests()
        {
            _activeQuests.AddRange(new[]
            {
                CreateQuest("weekly_kill_500", "Slay 500 Enemies", QuestType.KillEnemies, 500, true, 200, 10000),
                CreateQuest("weekly_stages_50", "Clear 50 Stages", QuestType.ClearStages, 50, true, 200, 20000),
                CreateQuest("weekly_abilities_100", "Use 100 Abilities", QuestType.UseAbilities, 100, true, 200, 15000),
            });
        }

        QuestProgress CreateQuest(string id, string name, QuestType type, int required, bool weekly, int bpXP, int gold)
        {
            return new QuestProgress
            {
                QuestId = id,
                QuestType = type,
                QuestName = name,
                CurrentProgress = 0,
                RequiredProgress = required,
                IsCompleted = false,
                IsCollected = false,
                IsWeekly = weekly,
                BattlePassXP = bpXP,
                GoldReward = gold
            };
        }

        void UpdateProgress(QuestType type, int amount = 1)
        {
            foreach (var quest in _activeQuests)
            {
                if (quest.QuestType != type || quest.IsCompleted) continue;

                quest.CurrentProgress += amount;

                EventBus.Publish(new QuestProgressEvent
                {
                    QuestId = quest.QuestId,
                    CurrentProgress = quest.CurrentProgress,
                    RequiredProgress = quest.RequiredProgress
                });

                if (quest.CurrentProgress >= quest.RequiredProgress)
                {
                    quest.IsCompleted = true;
                }
            }
        }

        public bool CollectQuestReward(string questId)
        {
            var quest = _activeQuests.Find(q => q.QuestId == questId);
            if (quest == null || !quest.IsCompleted || quest.IsCollected) return false;

            quest.IsCollected = true;

            var cm = CurrencyManager.Instance;
            if (cm != null)
            {
                if (quest.GoldReward > 0)
                    cm.AddCurrency(CurrencyType.Gold, quest.GoldReward);
            }

            EventBus.Publish(new QuestCompletedEvent
            {
                QuestId = quest.QuestId,
                IsWeekly = quest.IsWeekly,
                BattlePassXP = quest.BattlePassXP
            });

            return true;
        }

        // Event handlers
        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            UpdateProgress(QuestType.KillEnemies);
        }

        void OnWaveCleared(WaveClearedEvent evt)
        {
            UpdateProgress(QuestType.ClearStages);
        }

        void OnAbilityUsed(AbilityUsedEvent evt)
        {
            UpdateProgress(QuestType.UseAbilities);
        }

        void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == CurrencyType.Gold && evt.Delta > 0)
                UpdateProgress(QuestType.CollectGold, (int)evt.Delta);
        }

        void OnExpeditionCollected(ExpeditionCollectedEvent evt)
        {
            UpdateProgress(QuestType.CompleteExpeditions);
        }

        // Save/Load
        public QuestSaveData GatherSaveData()
        {
            return new QuestSaveData
            {
                LastDailyRefreshUnix = new DateTimeOffset(_lastDailyRefresh).ToUnixTimeSeconds(),
                LastWeeklyRefreshUnix = new DateTimeOffset(_lastWeeklyRefresh).ToUnixTimeSeconds(),
                ActiveQuests = new List<QuestProgress>(_activeQuests)
            };
        }

        public void LoadSaveData(QuestSaveData data)
        {
            if (data == null) return;
            _lastDailyRefresh = DateTimeOffset.FromUnixTimeSeconds(data.LastDailyRefreshUnix).LocalDateTime;
            _lastWeeklyRefresh = DateTimeOffset.FromUnixTimeSeconds(data.LastWeeklyRefreshUnix).LocalDateTime;
            _activeQuests.Clear();
            _activeQuests.AddRange(data.ActiveQuests);

            // Check if refresh is needed
            CheckRefresh();
        }

        static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
