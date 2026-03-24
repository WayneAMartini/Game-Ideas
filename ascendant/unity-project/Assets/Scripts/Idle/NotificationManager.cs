using System;
using UnityEngine;
using Ascendant.Core;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Ascendant.Idle
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        const string VaultFullId = "afk_vault_full";
        const string DailyResetId = "daily_quest_reset";
        const string ExpeditionCompleteId = "expedition_complete";
        const int MaxNotificationsPerDay = 2;

        int _notificationsScheduledToday;
        int _lastScheduledDay = -1;

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
            RequestAuthorization();
        }

        void OnEnable()
        {
            EventBus.Subscribe<AFKVaultCollectedEvent>(OnVaultCollected);
            EventBus.Subscribe<ExpeditionStartedEvent>(OnExpeditionStarted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<AFKVaultCollectedEvent>(OnVaultCollected);
            EventBus.Unsubscribe<ExpeditionStartedEvent>(OnExpeditionStarted);
        }

        void RequestAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
            using var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);
#endif
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ScheduleAllPendingNotifications();
            }
            else
            {
                CancelAllScheduled();
            }
        }

        void ScheduleAllPendingNotifications()
        {
            ResetDailyCounterIfNeeded();
            CancelAllScheduled();

            var settings = NotificationSettings.Instance;
            if (settings == null || !settings.MasterEnabled) return;

            // Vault nearly full: 8 hours after now
            if (settings.VaultFullEnabled)
            {
                ScheduleNotification(
                    VaultFullId,
                    "Your heroes have been fighting!",
                    "The AFK Vault is nearly full. Claim your rewards before they overflow!",
                    TimeSpan.FromHours(8)
                );
            }

            // Daily quest reset: schedule for next configured morning time
            if (settings.DailyResetEnabled)
            {
                var now = DateTime.Now;
                var resetTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
                if (resetTime <= now) resetTime = resetTime.AddDays(1);
                var delay = resetTime - now;

                ScheduleNotification(
                    DailyResetId,
                    "Daily Quests Reset!",
                    "New daily quests are available. Come collect your rewards!",
                    delay
                );
            }
        }

        void OnVaultCollected(AFKVaultCollectedEvent evt)
        {
            // Reschedule vault notification for 8 hours from collection
            CancelNotification(VaultFullId);

            var settings = NotificationSettings.Instance;
            if (settings != null && settings.VaultFullEnabled)
            {
                ScheduleNotification(
                    VaultFullId,
                    "Your heroes have been fighting!",
                    "The AFK Vault is nearly full. Claim your rewards before they overflow!",
                    TimeSpan.FromHours(8)
                );
            }
        }

        void OnExpeditionStarted(ExpeditionStartedEvent evt)
        {
            // Schedule expedition complete notification is handled by ExpeditionManager
        }

        public void ScheduleExpeditionComplete(string expeditionId, TimeSpan duration)
        {
            var settings = NotificationSettings.Instance;
            if (settings == null || !settings.ExpeditionCompleteEnabled) return;

            ScheduleNotification(
                ExpeditionCompleteId + "_" + expeditionId,
                "Expedition Complete!",
                "Your heroes have returned from their expedition. Collect the rewards!",
                duration
            );
        }

        void ScheduleNotification(string identifier, string title, string body, TimeSpan delay)
        {
            if (_notificationsScheduledToday >= MaxNotificationsPerDay) return;

            var settings = NotificationSettings.Instance;
            var fireTime = DateTime.Now + delay;

            // Respect quiet hours: push to end of quiet period
            if (settings != null && settings.IsInQuietHours(fireTime))
            {
                var endHour = settings.QuietHoursEnd;
                fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day, endHour, 0, 0);
                if (fireTime <= DateTime.Now) fireTime = fireTime.AddDays(1);
                delay = fireTime - DateTime.Now;
            }

#if UNITY_IOS && !UNITY_EDITOR
            var timeTrigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = delay,
                Repeats = false
            };

            var notification = new iOSNotification
            {
                Identifier = identifier,
                Title = title,
                Body = body,
                ShowInForeground = false,
                CategoryIdentifier = "ascendant_idle",
                Trigger = timeTrigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#else
            Debug.Log($"[NotificationManager] Would schedule: '{title}' in {delay.TotalHours:F1}h");
#endif

            _notificationsScheduledToday++;
        }

        void CancelNotification(string identifier)
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSNotificationCenter.RemoveScheduledNotification(identifier);
#endif
        }

        void CancelAllScheduled()
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        void ResetDailyCounterIfNeeded()
        {
            int today = DateTime.Now.DayOfYear;
            if (today != _lastScheduledDay)
            {
                _notificationsScheduledToday = 0;
                _lastScheduledDay = today;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
