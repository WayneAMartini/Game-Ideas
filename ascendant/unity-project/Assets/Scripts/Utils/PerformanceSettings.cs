using UnityEngine;

namespace Ascendant.Utils
{
    public enum QualityPreset
    {
        Low,
        Medium,
        High
    }

    [CreateAssetMenu(fileName = "PerformanceSettings", menuName = "Ascendant/Performance Settings")]
    public class PerformanceSettings : ScriptableObject
    {
        [Header("Quality Preset")]
        public QualityPreset preset = QualityPreset.Medium;

        [Header("Frame Rate")]
        public int targetFrameRate = 60;

        [Header("Particle Settings")]
        [Range(0.1f, 1f)] public float particleDensity = 1f;

        [Header("Animation Quality")]
        public bool enableScreenShake = true;
        public bool enableIdleAnimations = true;

        [Header("Memory")]
        public int memoryBudgetMB = 300;

        public static PerformanceSettings FromPreset(QualityPreset preset)
        {
            var settings = CreateInstance<PerformanceSettings>();
            settings.preset = preset;

            switch (preset)
            {
                case QualityPreset.Low:
                    settings.targetFrameRate = 30;
                    settings.particleDensity = 0.3f;
                    settings.enableScreenShake = false;
                    settings.enableIdleAnimations = false;
                    settings.memoryBudgetMB = 200;
                    break;
                case QualityPreset.Medium:
                    settings.targetFrameRate = 60;
                    settings.particleDensity = 0.7f;
                    settings.enableScreenShake = true;
                    settings.enableIdleAnimations = true;
                    settings.memoryBudgetMB = 300;
                    break;
                case QualityPreset.High:
                    settings.targetFrameRate = 60;
                    settings.particleDensity = 1f;
                    settings.enableScreenShake = true;
                    settings.enableIdleAnimations = true;
                    settings.memoryBudgetMB = 400;
                    break;
            }

            return settings;
        }
    }
}
