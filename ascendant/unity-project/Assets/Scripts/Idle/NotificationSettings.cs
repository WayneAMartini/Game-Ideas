using UnityEngine;

namespace Ascendant.Idle
{
    public class NotificationSettings : MonoBehaviour
    {
        public static NotificationSettings Instance { get; private set; }

        const string PrefKeyMasterEnabled = "notif_master_enabled";
        const string PrefKeyVaultFull = "notif_vault_full";
        const string PrefKeyDailyReset = "notif_daily_reset";
        const string PrefKeyExpeditionComplete = "notif_expedition_complete";
        const string PrefKeyQuietStart = "notif_quiet_start";
        const string PrefKeyQuietEnd = "notif_quiet_end";

        bool _masterEnabled = true;
        bool _vaultFullEnabled = true;
        bool _dailyResetEnabled = true;
        bool _expeditionCompleteEnabled = true;
        int _quietHoursStart = 22; // 10 PM
        int _quietHoursEnd = 8;    // 8 AM

        public bool MasterEnabled => _masterEnabled;
        public bool VaultFullEnabled => _vaultFullEnabled && _masterEnabled;
        public bool DailyResetEnabled => _dailyResetEnabled && _masterEnabled;
        public bool ExpeditionCompleteEnabled => _expeditionCompleteEnabled && _masterEnabled;
        public int QuietHoursStart => _quietHoursStart;
        public int QuietHoursEnd => _quietHoursEnd;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadSettings();
        }

        void LoadSettings()
        {
            _masterEnabled = PlayerPrefs.GetInt(PrefKeyMasterEnabled, 1) == 1;
            _vaultFullEnabled = PlayerPrefs.GetInt(PrefKeyVaultFull, 1) == 1;
            _dailyResetEnabled = PlayerPrefs.GetInt(PrefKeyDailyReset, 1) == 1;
            _expeditionCompleteEnabled = PlayerPrefs.GetInt(PrefKeyExpeditionComplete, 1) == 1;
            _quietHoursStart = PlayerPrefs.GetInt(PrefKeyQuietStart, 22);
            _quietHoursEnd = PlayerPrefs.GetInt(PrefKeyQuietEnd, 8);
        }

        void SaveSettings()
        {
            PlayerPrefs.SetInt(PrefKeyMasterEnabled, _masterEnabled ? 1 : 0);
            PlayerPrefs.SetInt(PrefKeyVaultFull, _vaultFullEnabled ? 1 : 0);
            PlayerPrefs.SetInt(PrefKeyDailyReset, _dailyResetEnabled ? 1 : 0);
            PlayerPrefs.SetInt(PrefKeyExpeditionComplete, _expeditionCompleteEnabled ? 1 : 0);
            PlayerPrefs.SetInt(PrefKeyQuietStart, _quietHoursStart);
            PlayerPrefs.SetInt(PrefKeyQuietEnd, _quietHoursEnd);
            PlayerPrefs.Save();
        }

        public void SetMasterEnabled(bool enabled)
        {
            _masterEnabled = enabled;
            SaveSettings();
        }

        public void SetVaultFullEnabled(bool enabled)
        {
            _vaultFullEnabled = enabled;
            SaveSettings();
        }

        public void SetDailyResetEnabled(bool enabled)
        {
            _dailyResetEnabled = enabled;
            SaveSettings();
        }

        public void SetExpeditionCompleteEnabled(bool enabled)
        {
            _expeditionCompleteEnabled = enabled;
            SaveSettings();
        }

        public void SetQuietHours(int startHour, int endHour)
        {
            _quietHoursStart = Mathf.Clamp(startHour, 0, 23);
            _quietHoursEnd = Mathf.Clamp(endHour, 0, 23);
            SaveSettings();
        }

        public bool IsInQuietHours(System.DateTime time)
        {
            int hour = time.Hour;
            if (_quietHoursStart <= _quietHoursEnd)
                return hour >= _quietHoursStart && hour < _quietHoursEnd;
            // Wraps midnight (e.g., 22 to 8)
            return hour >= _quietHoursStart || hour < _quietHoursEnd;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
