using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Idle
{
    [Serializable]
    public class ActiveExpedition
    {
        public string ExpeditionId;
        public int HeroSlot; // hero deployed (NOT in active party)
        public int HeroLevel;
        public long StartTimestampUnix;
        public float DurationSeconds;
        public bool Completed;
        public bool Collected;
    }

    public class ExpeditionManager : MonoBehaviour
    {
        public static ExpeditionManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] ExpeditionData[] _availableExpeditions;
        [SerializeField] int _maxSlots = 3;
        [SerializeField] int _initialUnlockedSlots = 1;

        int _unlockedSlots;
        List<ActiveExpedition> _activeExpeditions = new();

        public int MaxSlots => _maxSlots;
        public int UnlockedSlots => _unlockedSlots;
        public IReadOnlyList<ActiveExpedition> ActiveExpeditions => _activeExpeditions;
        public ExpeditionData[] AvailableExpeditions => _availableExpeditions;

        const string PrefKeyUnlockedSlots = "expedition_unlocked_slots";
        const string PrefKeyActiveExpeditions = "expedition_active_data";

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
            _unlockedSlots = PlayerPrefs.GetInt(PrefKeyUnlockedSlots, _initialUnlockedSlots);
            LoadExpeditions();
            CheckCompletedExpeditions();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveExpeditions();
            }
            else
            {
                CheckCompletedExpeditions();
            }
        }

        void Update()
        {
            long now = GetCurrentUnixTimestamp();
            for (int i = 0; i < _activeExpeditions.Count; i++)
            {
                var exp = _activeExpeditions[i];
                if (!exp.Completed && !exp.Collected)
                {
                    long elapsed = now - exp.StartTimestampUnix;
                    if (elapsed >= exp.DurationSeconds)
                    {
                        exp.Completed = true;
                        EventBus.Publish(new ExpeditionCompletedEvent
                        {
                            SlotIndex = i,
                            ExpeditionId = exp.ExpeditionId
                        });
                    }
                }
            }
        }

        public bool CanStartExpedition()
        {
            int activeCount = 0;
            foreach (var exp in _activeExpeditions)
                if (!exp.Collected) activeCount++;
            return activeCount < _unlockedSlots;
        }

        public bool StartExpedition(ExpeditionData data, int heroLevel)
        {
            if (!CanStartExpedition()) return false;
            if (data == null) return false;

            var expedition = new ActiveExpedition
            {
                ExpeditionId = data.expeditionId,
                HeroLevel = heroLevel,
                StartTimestampUnix = GetCurrentUnixTimestamp(),
                DurationSeconds = data.GetDurationSeconds(),
                Completed = false,
                Collected = false
            };

            _activeExpeditions.Add(expedition);
            SaveExpeditions();

            int slotIndex = _activeExpeditions.Count - 1;
            EventBus.Publish(new ExpeditionStartedEvent
            {
                SlotIndex = slotIndex,
                ExpeditionId = data.expeditionId
            });

            // Schedule notification
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ScheduleExpeditionComplete(
                    data.expeditionId,
                    TimeSpan.FromSeconds(data.GetDurationSeconds())
                );
            }

            return true;
        }

        public bool CollectExpedition(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _activeExpeditions.Count) return false;

            var exp = _activeExpeditions[slotIndex];
            if (!exp.Completed || exp.Collected) return false;

            var data = FindExpeditionData(exp.ExpeditionId);
            if (data == null) return false;

            float rewardMult = data.GetRewardMultiplier(exp.HeroLevel);

            // Award rewards via CurrencyManager
            var currencyManager = Economy.CurrencyManager.Instance;
            if (currencyManager != null)
            {
                if (data.baseGoldReward > 0)
                    currencyManager.AddGold(data.baseGoldReward * rewardMult);
                if (data.baseXpReward > 0)
                    currencyManager.AddXp(data.baseXpReward * rewardMult);
            }

            exp.Collected = true;

            // Remove collected expeditions
            _activeExpeditions.RemoveAt(slotIndex);
            SaveExpeditions();

            EventBus.Publish(new ExpeditionCollectedEvent { SlotIndex = slotIndex });
            return true;
        }

        public float GetExpeditionProgress(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _activeExpeditions.Count) return 0f;

            var exp = _activeExpeditions[slotIndex];
            if (exp.Completed) return 1f;

            long now = GetCurrentUnixTimestamp();
            long elapsed = now - exp.StartTimestampUnix;
            return Mathf.Clamp01(elapsed / exp.DurationSeconds);
        }

        public float GetRemainingSeconds(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _activeExpeditions.Count) return 0f;

            var exp = _activeExpeditions[slotIndex];
            if (exp.Completed) return 0f;

            long now = GetCurrentUnixTimestamp();
            long elapsed = now - exp.StartTimestampUnix;
            return Mathf.Max(0f, exp.DurationSeconds - elapsed);
        }

        public void UnlockSlot()
        {
            if (_unlockedSlots >= _maxSlots) return;
            _unlockedSlots++;
            PlayerPrefs.SetInt(PrefKeyUnlockedSlots, _unlockedSlots);
            PlayerPrefs.Save();
        }

        ExpeditionData FindExpeditionData(string expeditionId)
        {
            if (_availableExpeditions == null) return null;
            foreach (var data in _availableExpeditions)
                if (data.expeditionId == expeditionId)
                    return data;
            return null;
        }

        void CheckCompletedExpeditions()
        {
            long now = GetCurrentUnixTimestamp();
            for (int i = 0; i < _activeExpeditions.Count; i++)
            {
                var exp = _activeExpeditions[i];
                if (!exp.Completed && !exp.Collected)
                {
                    long elapsed = now - exp.StartTimestampUnix;
                    if (elapsed >= exp.DurationSeconds)
                    {
                        exp.Completed = true;
                        EventBus.Publish(new ExpeditionCompletedEvent
                        {
                            SlotIndex = i,
                            ExpeditionId = exp.ExpeditionId
                        });
                    }
                }
            }
        }

        void SaveExpeditions()
        {
            var wrapper = new ExpeditionSaveWrapper { Expeditions = _activeExpeditions };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PrefKeyActiveExpeditions, json);
            PlayerPrefs.Save();
        }

        void LoadExpeditions()
        {
            string json = PlayerPrefs.GetString(PrefKeyActiveExpeditions, "");
            if (string.IsNullOrEmpty(json))
            {
                _activeExpeditions = new List<ActiveExpedition>();
                return;
            }

            var wrapper = JsonUtility.FromJson<ExpeditionSaveWrapper>(json);
            _activeExpeditions = wrapper?.Expeditions ?? new List<ActiveExpedition>();
        }

        static long GetCurrentUnixTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        [Serializable]
        class ExpeditionSaveWrapper
        {
            public List<ActiveExpedition> Expeditions;
        }
    }
}
