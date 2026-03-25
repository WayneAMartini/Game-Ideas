using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ascendant.Backend
{
    public enum OfflineActionType
    {
        CloudSave,
        LeaderboardSubmit,
        ArenaResult,
        WorldBossDamage,
        GuildContribution,
        GuildChat
    }

    [Serializable]
    public class OfflineAction
    {
        public OfflineActionType Type;
        public string Payload;
        public long TimestampUnix;
        public string Id;
    }

    [Serializable]
    class OfflineQueueData
    {
        public List<OfflineAction> Actions = new();
    }

    public class OfflineQueue : MonoBehaviour
    {
        public static OfflineQueue Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _maxQueueSize = 500;

        readonly List<OfflineAction> _queue = new();
        string _persistPath;
        bool _isFlushing;

        public bool HasPending => _queue.Count > 0;
        public int PendingCount => _queue.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _persistPath = Path.Combine(Application.persistentDataPath, "offline_queue.json");

            var config = FirebaseManager.Instance?.Config;
            if (config != null)
                _maxQueueSize = config.maxOfflineQueueSize;

            LoadQueue();
        }

        public void Enqueue(OfflineAction action)
        {
            if (_queue.Count >= _maxQueueSize)
            {
                // Remove oldest action to make room
                _queue.RemoveAt(0);
            }

            action.Id = Guid.NewGuid().ToString("N")[..8];
            if (action.TimestampUnix == 0)
                action.TimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Deduplicate: skip if same type + payload exists
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (_queue[i].Type == action.Type && _queue[i].Payload == action.Payload)
                {
                    _queue[i] = action; // Update timestamp
                    SaveQueue();
                    return;
                }
            }

            _queue.Add(action);
            SaveQueue();
        }

        public void FlushQueue(Action onComplete = null)
        {
            if (_isFlushing || _queue.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                onComplete?.Invoke();
                return;
            }

            _isFlushing = true;
            FlushNext(service, 0, onComplete);
        }

        void FlushNext(IBackendService service, int index, Action onComplete)
        {
            if (index >= _queue.Count)
            {
                _queue.Clear();
                SaveQueue();
                _isFlushing = false;
                onComplete?.Invoke();
                return;
            }

            var action = _queue[index];
            ExecuteAction(service, action, success =>
            {
                if (success)
                {
                    FlushNext(service, index + 1, onComplete);
                }
                else
                {
                    // Keep remaining items in queue for later retry
                    if (index > 0)
                    {
                        _queue.RemoveRange(0, index);
                        SaveQueue();
                    }
                    _isFlushing = false;
                    onComplete?.Invoke();
                }
            });
        }

        void ExecuteAction(IBackendService service, OfflineAction action, Action<bool> callback)
        {
            switch (action.Type)
            {
                case OfflineActionType.CloudSave:
                    service.SavePlayerData(action.Payload, action.TimestampUnix, callback);
                    break;

                case OfflineActionType.LeaderboardSubmit:
                    var parts = action.Payload.Split('|');
                    if (parts.Length == 2 && long.TryParse(parts[1], out long score))
                        service.SubmitScore(parts[0], score, callback);
                    else
                        callback?.Invoke(false);
                    break;

                case OfflineActionType.ArenaResult:
                    var arenaParts = action.Payload.Split('|');
                    if (arenaParts.Length == 2)
                        service.ReportArenaResult(arenaParts[0], arenaParts[1] == "1", (s, _) => callback?.Invoke(s));
                    else
                        callback?.Invoke(false);
                    break;

                case OfflineActionType.WorldBossDamage:
                    if (double.TryParse(action.Payload, out double damage))
                        service.ReportWorldBossDamage(damage, callback);
                    else
                        callback?.Invoke(false);
                    break;

                case OfflineActionType.GuildContribution:
                    var guildParts = action.Payload.Split('|');
                    if (guildParts.Length == 2 && int.TryParse(guildParts[1], out int amount))
                        service.ContributeToTechTree(guildParts[0], amount, callback);
                    else
                        callback?.Invoke(false);
                    break;

                case OfflineActionType.GuildChat:
                    service.SendGuildChat(action.Payload, callback);
                    break;

                default:
                    callback?.Invoke(false);
                    break;
            }
        }

        void SaveQueue()
        {
            try
            {
                var data = new OfflineQueueData { Actions = new List<OfflineAction>(_queue) };
                string json = JsonUtility.ToJson(data, false);
                File.WriteAllText(_persistPath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[OfflineQueue] Failed to persist queue: {e.Message}");
            }
        }

        void LoadQueue()
        {
            if (!File.Exists(_persistPath)) return;

            try
            {
                string json = File.ReadAllText(_persistPath);
                var data = JsonUtility.FromJson<OfflineQueueData>(json);
                if (data?.Actions != null)
                {
                    _queue.Clear();
                    _queue.AddRange(data.Actions);
                    Debug.Log($"[OfflineQueue] Loaded {_queue.Count} pending actions");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[OfflineQueue] Failed to load queue: {e.Message}");
            }
        }

        void OnDestroy()
        {
            if (_queue.Count > 0)
                SaveQueue();

            if (Instance == this) Instance = null;
        }
    }
}
