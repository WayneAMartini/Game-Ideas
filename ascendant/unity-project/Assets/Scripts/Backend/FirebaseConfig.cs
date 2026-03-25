using UnityEngine;

namespace Ascendant.Backend
{
    [CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Ascendant/Backend/Firebase Config")]
    public class FirebaseConfig : ScriptableObject
    {
        [Header("Firebase Project")]
        public string projectId = "";
        public string apiKey = "";
        public string appId = "";

        [Header("Feature Flags")]
        public bool useCloudSave = false;
        public bool useLeaderboards = false;
        public bool useGuilds = false;
        public bool useArena = false;
        public bool useWorldBoss = false;

        [Header("Cloud Save")]
        public float autoSaveIntervalSeconds = 30f;
        public int maxOfflineQueueSize = 500;

        [Header("Arena")]
        public int arenaAttemptsPerDay = 5;
        public int arenaSeasonDurationDays = 30;
        public float arenaNormalizedStatMin = 0.8f;
        public float arenaNormalizedStatMax = 1.2f;

        [Header("World Boss")]
        public int worldBossAttemptsPerDay = 3;
        public float worldBossFightDurationSeconds = 60f;
        public int worldBossEventDurationHours = 48;

        [Header("Guild")]
        public int maxGuildMembers = 30;
        public int guildExpeditionGridSize = 5;
        public int guildExpeditionNodesPerDay = 3;

        public bool IsFirebaseConfigured => !string.IsNullOrEmpty(projectId) && !string.IsNullOrEmpty(apiKey);
    }
}
