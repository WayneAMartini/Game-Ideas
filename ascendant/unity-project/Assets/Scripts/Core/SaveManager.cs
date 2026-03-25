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

        // Phase 6: Ascension data
        public Progression.AscensionSaveData AscensionData;

        // Phase 8: Economy data
        public Economy.WalletSaveData WalletData;
        public Economy.GachaSaveData GachaData;
        public Economy.StarSaveData StarData;
        public Economy.BattlePassSaveData BattlePassData;
        public Economy.QuestSaveData QuestData;
        public bool PatronBlessingActive;
        public long PatronBlessingExpiryUnix;

        // Phase 9: Social & Backend data
        public Backend.GuildSaveData GuildData;
        public Backend.ArenaSaveData ArenaData;
        public Backend.WorldBossSaveData WorldBossData;
        public Backend.GuildExpeditionSaveData GuildExpeditionData;
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

            // Phase 6: Ascension data
            save.AscensionData = GatherAscensionData();

            // Phase 8: Economy data
            var wallet = Economy.Wallet.Instance;
            if (wallet != null)
                save.WalletData = wallet.GatherSaveData();

            var gacha = Economy.GachaSystem.Instance;
            if (gacha != null)
                save.GachaData = gacha.GatherSaveData();

            var stars = Economy.StarSystem.Instance;
            if (stars != null)
                save.StarData = stars.GatherSaveData();

            var battlePass = Economy.BattlePassSystem.Instance;
            if (battlePass != null)
                save.BattlePassData = battlePass.GatherSaveData();

            var quests = Economy.DailyQuestSystem.Instance;
            if (quests != null)
                save.QuestData = quests.GatherSaveData();

            var iap = Economy.IAPManager.Instance;
            if (iap != null)
            {
                save.PatronBlessingActive = iap.GetPatronActive();
                save.PatronBlessingExpiryUnix = iap.GetPatronExpiryUnix();
            }

            // Phase 9: Social & Backend data
            var guild = Backend.GuildManager.Instance;
            if (guild != null)
                save.GuildData = guild.GatherSaveData();

            var arena = Backend.ArenaManager.Instance;
            if (arena != null)
                save.ArenaData = arena.GatherSaveData();

            var worldBoss = Backend.WorldBossManager.Instance;
            if (worldBoss != null)
                save.WorldBossData = worldBoss.GatherSaveData();

            var guildExpedition = Backend.GuildExpedition.Instance;
            if (guildExpedition != null)
                save.GuildExpeditionData = guildExpedition.GatherSaveData();

            return save;
        }

        void ApplySaveData(SaveData save)
        {
            // Phase 6: Restore ascension state
            ApplyAscensionData(save.AscensionData);

            // Phase 9: Restore social data
            Backend.GuildManager.Instance?.LoadSaveData(save.GuildData);
            Backend.ArenaManager.Instance?.LoadSaveData(save.ArenaData);
            Backend.WorldBossManager.Instance?.LoadSaveData(save.WorldBossData);
            Backend.GuildExpedition.Instance?.LoadSaveData(save.GuildExpeditionData);

            Debug.Log($"[SaveManager] Loaded save from {DateTimeOffset.FromUnixTimeSeconds(save.SaveTimestampUnix).LocalDateTime}");
        }

        Progression.AscensionSaveData GatherAscensionData()
        {
            var data = new Progression.AscensionSaveData();

            // Tier/ascension counts
            var tierSystem = Progression.TierBonusSystem.Instance;
            if (tierSystem != null)
            {
                foreach (var kvp in tierSystem.GetAllAscensionCounts())
                {
                    var hero = Party.PartyManager.Instance?.GetHero(kvp.Key);
                    data.HeroAscensions.Add(new Progression.HeroAscensionData
                    {
                        HeroSlot = kvp.Key,
                        ClassId = hero?.Data?.classId ?? "",
                        AscensionCount = kvp.Value,
                        HighestIslandReached = Progression.AscensionSystem.Instance?.GetHighestIsland(kvp.Key) ?? 1
                    });
                }
            }

            // Ascension skill tree
            var ascTree = Progression.AscensionSkillTree.Instance;
            if (ascTree != null)
            {
                foreach (var nodeId in ascTree.GetPurchasedNodeIds())
                {
                    data.SkillTreeNodes.Add(new Progression.AscensionSkillTreeSaveData
                    {
                        NodeId = nodeId
                    });
                }
            }

            // Demigods
            var demigodSystem = Progression.DemigodSystem.Instance;
            if (demigodSystem != null)
                data.Demigods = demigodSystem.GatherSaveData();

            return data;
        }

        void ApplyAscensionData(Progression.AscensionSaveData data)
        {
            if (data == null) return;

            // Restore ascension counts
            var tierSystem = Progression.TierBonusSystem.Instance;
            if (tierSystem != null && data.HeroAscensions != null)
            {
                var counts = new Dictionary<int, int>();
                foreach (var ha in data.HeroAscensions)
                {
                    counts[ha.HeroSlot] = ha.AscensionCount;
                    Progression.AscensionSystem.Instance?.SetHighestIsland(ha.HeroSlot, ha.HighestIslandReached);
                }
                tierSystem.LoadAscensionCounts(counts);
            }

            // Restore ascension skill tree
            var ascTree = Progression.AscensionSkillTree.Instance;
            if (ascTree != null && data.SkillTreeNodes != null)
            {
                var nodeIds = new List<string>();
                foreach (var n in data.SkillTreeNodes)
                    nodeIds.Add(n.NodeId);
                ascTree.LoadPurchasedNodes(nodeIds);
            }

            // Restore demigods
            var demigodSystem = Progression.DemigodSystem.Instance;
            if (demigodSystem != null && data.Demigods != null)
                demigodSystem.LoadSaveData(data.Demigods);
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
