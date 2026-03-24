using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Ascendant.Core
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public long SaveTimestampUnix;

        // Hero states
        public List<HeroSaveData> Heroes = new();

        // Party formation (slot indices into Heroes list)
        public int[] PartySlots = new int[4] { -1, -1, -1, -1 };

        // Currencies
        public double Gold;
        public double Xp;

        // Progression
        public int CurrentIsland = 1;
        public int CurrentStage = 1;

        // AFK Vault
        public long AFKLastBackgroundTimestamp;
        public long AFKLastCollectionTimestamp;

        // Expeditions (serialized separately by ExpeditionManager via PlayerPrefs)

        // Notification settings
        public bool NotifMasterEnabled = true;
        public bool NotifVaultFull = true;
        public bool NotifDailyReset = true;
        public bool NotifExpeditionComplete = true;
        public int NotifQuietStart = 22;
        public int NotifQuietEnd = 8;
    }

    [Serializable]
    public class HeroSaveData
    {
        public string ClassId;
        public int Level;
        public float Xp;
        public float CurrentHp;
        public float MaxHp;
        public int Slot;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] float _autoSaveInterval = 30f;

        float _autoSaveTimer;
        string _savePath;
        SaveData _currentSave;

        static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("AscendantSaveKey"); // 16 bytes for AES-128
        static readonly byte[] EncryptionIV = Encoding.UTF8.GetBytes("AscendantIVector"); // 16 bytes IV

        public SaveData CurrentSave => _currentSave;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _savePath = Path.Combine(Application.persistentDataPath, "save.dat");
        }

        void Start()
        {
            Load();
        }

        void Update()
        {
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= _autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                Save();
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            _currentSave = GatherSaveData();

            string json = JsonUtility.ToJson(_currentSave, false);
            byte[] encrypted = Encrypt(json);

            try
            {
                File.WriteAllBytes(_savePath, encrypted);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        public void Load()
        {
            if (!File.Exists(_savePath))
            {
                _currentSave = new SaveData();
                return;
            }

            try
            {
                byte[] encrypted = File.ReadAllBytes(_savePath);
                string json = Decrypt(encrypted);
                _currentSave = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(_currentSave);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load save: {e.Message}");
                _currentSave = new SaveData();
            }
        }

        SaveData GatherSaveData()
        {
            var save = new SaveData
            {
                SaveTimestampUnix = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
            };

            // Currencies
            var currency = Economy.CurrencyManager.Instance;
            if (currency != null)
            {
                save.Gold = currency.Gold;
                save.Xp = currency.Xp;
            }

            // Progression
            var stage = Progression.StageManager.Instance;
            if (stage != null)
            {
                save.CurrentIsland = stage.CurrentIsland;
                save.CurrentStage = stage.CurrentStage;
            }

            // Heroes in party
            var party = Party.PartyManager.Instance;
            if (party != null)
            {
                var heroes = party.GetAllHeroes();
                for (int i = 0; i < heroes.Length; i++)
                {
                    var hero = heroes[i];
                    if (hero != null)
                    {
                        save.Heroes.Add(new HeroSaveData
                        {
                            ClassId = hero.Data != null ? hero.Data.classId : "",
                            Level = hero.Level,
                            Xp = hero.Xp,
                            CurrentHp = hero.CurrentHp,
                            MaxHp = hero.MaxHp,
                            Slot = hero.Slot
                        });
                        save.PartySlots[i] = save.Heroes.Count - 1;
                    }
                }
            }

            // AFK vault timestamps
            if (PlayerPrefs.HasKey("afk_last_background_hi"))
            {
                save.AFKLastBackgroundTimestamp = LoadTimestamp("afk_last_background");
                save.AFKLastCollectionTimestamp = LoadTimestamp("afk_last_collection");
            }

            // Notification settings
            var notif = Idle.NotificationSettings.Instance;
            if (notif != null)
            {
                save.NotifMasterEnabled = notif.MasterEnabled;
                save.NotifVaultFull = notif.VaultFullEnabled;
                save.NotifDailyReset = notif.DailyResetEnabled;
                save.NotifExpeditionComplete = notif.ExpeditionCompleteEnabled;
                save.NotifQuietStart = notif.QuietHoursStart;
                save.NotifQuietEnd = notif.QuietHoursEnd;
            }

            return save;
        }

        void ApplySaveData(SaveData save)
        {
            // Save data is applied by each system reading from SaveManager.CurrentSave on init.
            // This method triggers the event so systems can respond.
            Debug.Log($"[SaveManager] Loaded save from {DateTimeOffset.FromUnixTimeSeconds(save.SaveTimestampUnix).LocalDateTime}");
        }

        static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        static string Decrypt(byte[] cipherBytes)
        {
            using var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decrypted);
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
