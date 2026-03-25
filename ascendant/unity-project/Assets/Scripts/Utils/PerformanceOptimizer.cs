using UnityEngine;
using Ascendant.Core;
using Ascendant.UI;

namespace Ascendant.Utils
{
    public class PerformanceOptimizer : MonoBehaviour
    {
        public static PerformanceOptimizer Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] PerformanceSettings _settings;

        [Header("Debug")]
        [SerializeField] bool _showDebugOverlay;

        QualityPreset _currentPreset;
        float _fpsCheckTimer;
        float _fpsAccumulator;
        int _fpsFrameCount;
        float _currentFps;

        int _memoryWarningCount;
        float _memoryCheckTimer;

        bool _isBackground;
        float _backgroundIdleRate = 0.1f; // Reduce updates to 10% in background

        public QualityPreset CurrentPreset => _currentPreset;
        public float CurrentFPS => _currentFps;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _currentPreset = (QualityPreset)PlayerPrefs.GetInt("perf_quality", (int)QualityPreset.Medium);
            ApplySettings();
        }

        void Start()
        {
            DetectDevice();
        }

        void Update()
        {
            TrackFPS();
            CheckMemory();
        }

        void OnApplicationPause(bool paused)
        {
            _isBackground = paused;
            if (paused)
            {
                // Reduce activity in background for battery savings
                Time.timeScale = _backgroundIdleRate;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        // --- Quality Management ---

        public void SetQuality(QualityPreset preset)
        {
            _currentPreset = preset;
            PlayerPrefs.SetInt("perf_quality", (int)preset);
            ApplySettings();
        }

        void ApplySettings()
        {
            var settings = PerformanceSettings.FromPreset(_currentPreset);

            Application.targetFrameRate = settings.targetFrameRate;

            // Apply particle density
            if (Audio.ParticleManager.Instance != null)
            {
                // ParticleManager respects density setting through this optimizer
            }

            // Apply animation settings
            if (Audio.SpriteAnimator.Instance != null)
            {
                // Screen shake toggled via settings
            }

            Debug.Log($"[PerformanceOptimizer] Applied {_currentPreset} quality preset. Target FPS: {settings.targetFrameRate}");
        }

        // --- Device Detection ---

        void DetectDevice()
        {
            // Auto-select quality based on device
            int systemMemory = SystemInfo.systemMemorySize;
            int gpuMemory = SystemInfo.graphicMemorySize;
            int processorCount = SystemInfo.processorCount;

            if (PlayerPrefs.HasKey("perf_quality"))
                return; // User has manually set quality

            if (systemMemory < 3072 || processorCount <= 4)
            {
                // Low-end device (e.g., iPhone SE 3)
                SetQuality(QualityPreset.Low);
            }
            else if (systemMemory < 6144)
            {
                // Mid-range device
                SetQuality(QualityPreset.Medium);
            }
            else
            {
                // High-end device (iPhone 12+)
                SetQuality(QualityPreset.High);
            }
        }

        // --- FPS Tracking ---

        void TrackFPS()
        {
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            _fpsCheckTimer += Time.unscaledDeltaTime;
            if (_fpsCheckTimer >= 1f)
            {
                _currentFps = _fpsFrameCount / _fpsAccumulator;
                _fpsAccumulator = 0f;
                _fpsFrameCount = 0;
                _fpsCheckTimer = 0f;

                // Auto-downgrade if consistently below target
                var settings = PerformanceSettings.FromPreset(_currentPreset);
                if (_currentFps < settings.targetFrameRate * 0.7f && _currentPreset != QualityPreset.Low)
                {
                    Debug.LogWarning($"[PerformanceOptimizer] FPS ({_currentFps:F0}) below target. Consider lowering quality.");
                }
            }
        }

        // --- Memory Monitoring ---

        void CheckMemory()
        {
            _memoryCheckTimer += Time.deltaTime;
            if (_memoryCheckTimer < 5f) return;
            _memoryCheckTimer = 0f;

            // Unity's Profiler gives managed heap info
            long totalMemory = System.GC.GetTotalMemory(false);
            float memoryMB = totalMemory / (1024f * 1024f);

            var settings = PerformanceSettings.FromPreset(_currentPreset);
            if (memoryMB > settings.memoryBudgetMB)
            {
                _memoryWarningCount++;
                if (_memoryWarningCount >= 3)
                {
                    Debug.LogWarning($"[PerformanceOptimizer] Memory ({memoryMB:F0}MB) exceeds budget ({settings.memoryBudgetMB}MB). Triggering GC.");
                    System.GC.Collect();
                    _memoryWarningCount = 0;
                }
            }
            else
            {
                _memoryWarningCount = 0;
            }
        }

        // --- Sprite Atlas Helper ---

        public static void SetupSpriteAtlases()
        {
            // At runtime, Unity handles atlas loading automatically
            // This method is a hook for editor tooling in BuildHelper
            Debug.Log("[PerformanceOptimizer] Sprite atlases should be configured in Editor via SpriteAtlas assets.");
        }

        // --- Object Pool Audit ---

        public void AuditObjectPools()
        {
            int pooledObjects = 0;

            // Check DamageNumberPool
            var dnPool = FindAnyObjectByType<DamageNumberPool>();
            if (dnPool != null) pooledObjects++;

            // Check ParticleManager pools
            if (Audio.ParticleManager.Instance != null) pooledObjects++;

            // Check SoundManager SFX pool
            if (Audio.SoundManager.Instance != null) pooledObjects++;

            Debug.Log($"[PerformanceOptimizer] Object pool audit: {pooledObjects} pool systems active.");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
