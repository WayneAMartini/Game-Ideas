using System;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] float _autoSyncInterval = 30f;

        float _syncTimer;
        bool _isSyncing;
        bool _hasPendingChanges;

        public bool IsSyncing => _isSyncing;
        public bool HasPendingChanges => _hasPendingChanges;

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
            // Load config interval if available
            var config = FirebaseManager.Instance?.Config;
            if (config != null)
                _autoSyncInterval = config.autoSaveIntervalSeconds;

            // Attempt initial sync from cloud
            SyncFromCloud();
        }

        void Update()
        {
            _syncTimer += Time.deltaTime;
            if (_syncTimer >= _autoSyncInterval)
            {
                _syncTimer = 0f;
                SyncToCloud();
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                SyncToCloud();
        }

        void OnApplicationQuit()
        {
            SyncToCloud();
        }

        public void MarkDirty()
        {
            _hasPendingChanges = true;
        }

        public void SyncToCloud()
        {
            if (_isSyncing) return;

            var service = FirebaseManager.Instance?.Service;
            if (service == null) return;

            // Gather save data from SaveManager
            var saveManager = SaveManager.Instance;
            if (saveManager == null) return;

            // Always do a local save first
            saveManager.Save();

            var saveData = saveManager.CurrentSave;
            if (saveData == null) return;

            string json = JsonUtility.ToJson(saveData, false);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _isSyncing = true;

            // Flush offline queue first
            if (OfflineQueue.Instance != null && OfflineQueue.Instance.HasPending)
            {
                OfflineQueue.Instance.FlushQueue(() =>
                {
                    UploadSave(service, json, timestamp);
                });
            }
            else
            {
                UploadSave(service, json, timestamp);
            }
        }

        void UploadSave(IBackendService service, string json, long timestamp)
        {
            service.SavePlayerData(json, timestamp, success =>
            {
                _isSyncing = false;

                if (success)
                {
                    _hasPendingChanges = false;
                    EventBus.Publish(new CloudSaveSyncedEvent
                    {
                        Success = true,
                        TimestampUnix = timestamp
                    });
                }
                else
                {
                    // Queue for retry
                    OfflineQueue.Instance?.Enqueue(new OfflineAction
                    {
                        Type = OfflineActionType.CloudSave,
                        Payload = json,
                        TimestampUnix = timestamp
                    });

                    Debug.LogWarning("[CloudSaveManager] Failed to sync — queued for retry");
                }
            });
        }

        public void SyncFromCloud()
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null) return;

            service.LoadPlayerData((success, json, serverTimestamp) =>
            {
                if (!success || string.IsNullOrEmpty(json)) return;

                var saveManager = SaveManager.Instance;
                if (saveManager == null) return;

                long localTimestamp = saveManager.CurrentSave?.SaveTimestampUnix ?? 0;

                // Conflict resolution: server timestamp wins
                if (serverTimestamp > localTimestamp)
                {
                    try
                    {
                        var serverSave = JsonUtility.FromJson<SaveData>(json);
                        if (serverSave != null)
                        {
                            EventBus.Publish(new CloudSaveConflictEvent
                            {
                                LocalTimestamp = localTimestamp,
                                ServerTimestamp = serverTimestamp
                            });

                            // Server wins — apply server data
                            Debug.Log($"[CloudSaveManager] Server save is newer ({serverTimestamp} > {localTimestamp}), applying server data");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CloudSaveManager] Failed to parse server save: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("[CloudSaveManager] Local save is newer or equal, keeping local data");
                }
            });
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
