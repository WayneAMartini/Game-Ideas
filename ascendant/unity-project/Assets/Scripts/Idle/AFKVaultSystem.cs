using UnityEngine;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.Idle
{
    public class AFKVaultSystem : MonoBehaviour
    {
        public static AFKVaultSystem Instance { get; private set; }

        AFKRewards _pendingRewards;
        bool _hasPendingRewards;
        long _lastBackgroundTimestamp;
        long _lastCollectionTimestamp;

        public bool HasPendingRewards => _hasPendingRewards;
        public AFKRewards PendingRewards => _pendingRewards;

        const string PrefKeyLastBackground = "afk_last_background";
        const string PrefKeyLastCollection = "afk_last_collection";

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
            _lastBackgroundTimestamp = LoadTimestamp(PrefKeyLastBackground);
            _lastCollectionTimestamp = LoadTimestamp(PrefKeyLastCollection);
            CheckOfflineRewards();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                RecordBackgroundState();
            }
            else
            {
                CheckOfflineRewards();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                RecordBackgroundState();
            }
            else
            {
                CheckOfflineRewards();
            }
        }

        void RecordBackgroundState()
        {
            _lastBackgroundTimestamp = GetCurrentUnixTimestamp();
            SaveTimestamp(PrefKeyLastBackground, _lastBackgroundTimestamp);
            PlayerPrefs.Save();
        }

        void CheckOfflineRewards()
        {
            if (_lastBackgroundTimestamp <= 0) return;

            long now = GetCurrentUnixTimestamp();
            int stageLevel = StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1;
            float partyPower = CalculatePartyPower();

            var rewards = OfflineCalculator.Calculate(
                _lastBackgroundTimestamp,
                now,
                stageLevel,
                partyPower,
                _lastCollectionTimestamp
            );

            // Only show vault if there's meaningful time offline (at least 1 minute)
            if (rewards.OfflineHours >= 1.0 / 60.0)
            {
                _pendingRewards = rewards;
                _hasPendingRewards = true;
                EventBus.Publish(new AFKVaultReadyEvent { Rewards = rewards });
            }
        }

        public void CollectRewards()
        {
            if (!_hasPendingRewards) return;

            // Award gold
            var currencyManager = Economy.CurrencyManager.Instance;
            if (currencyManager != null)
            {
                currencyManager.AddGold(_pendingRewards.Gold);
                currencyManager.AddXp(_pendingRewards.Xp);
            }

            _lastCollectionTimestamp = GetCurrentUnixTimestamp();
            SaveTimestamp(PrefKeyLastCollection, _lastCollectionTimestamp);
            PlayerPrefs.Save();

            var collected = _pendingRewards;
            _hasPendingRewards = false;
            _pendingRewards = default;

            EventBus.Publish(new AFKVaultCollectedEvent { Rewards = collected });
        }

        float CalculatePartyPower()
        {
            var partyManager = Party.PartyManager.Instance;
            if (partyManager == null) return 100f;

            float totalPower = 0f;
            var heroes = partyManager.GetAllAliveHeroes();
            foreach (var hero in heroes)
            {
                totalPower += hero.CurrentAtk + hero.CurrentDef + hero.MaxHp * 0.1f;
            }
            return Mathf.Max(100f, totalPower);
        }

        static long GetCurrentUnixTimestamp()
        {
            return new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeSeconds();
        }

        static void SaveTimestamp(string key, long timestamp)
        {
            // PlayerPrefs doesn't support long, so store as two ints
            PlayerPrefs.SetInt(key + "_hi", (int)(timestamp >> 32));
            PlayerPrefs.SetInt(key + "_lo", (int)(timestamp & 0xFFFFFFFF));
        }

        static long LoadTimestamp(string key)
        {
            if (!PlayerPrefs.HasKey(key + "_hi")) return 0;
            long hi = (long)PlayerPrefs.GetInt(key + "_hi") << 32;
            long lo = (long)(uint)PlayerPrefs.GetInt(key + "_lo");
            return hi | lo;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
