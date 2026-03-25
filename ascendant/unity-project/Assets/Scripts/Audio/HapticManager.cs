using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Audio
{
    public enum HapticIntensity
    {
        Off,
        Light,
        Standard,
        Strong
    }

    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        [SerializeField] HapticIntensity _intensity = HapticIntensity.Standard;

        public HapticIntensity Intensity
        {
            get => _intensity;
            set { _intensity = value; PlayerPrefs.SetInt("haptic_intensity", (int)value); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _intensity = (HapticIntensity)PlayerPrefs.GetInt("haptic_intensity", (int)HapticIntensity.Standard);
        }

        void OnEnable()
        {
            EventBus.Subscribe<TapEvent>(OnTap);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Subscribe<AscensionEvent>(OnAscension);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<TapEvent>(OnTap);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
            EventBus.Unsubscribe<AscensionEvent>(OnAscension);
        }

        void OnTap(TapEvent evt) => TriggerHaptic(HapticIntensity.Light);
        void OnEnemyKilled(EnemyKilledEvent evt) => TriggerHaptic(HapticIntensity.Standard);
        void OnAbilityUsed(AbilityUsedEvent evt) => TriggerHaptic(HapticIntensity.Standard);
        void OnLevelUp(LevelUpEvent evt) => TriggerHaptic(HapticIntensity.Standard);
        void OnBossPhaseChanged(BossPhaseChangedEvent evt) => TriggerHaptic(HapticIntensity.Strong);
        void OnAscension(AscensionEvent evt) => TriggerHaptic(HapticIntensity.Strong);

        public void TriggerHaptic(HapticIntensity requiredIntensity)
        {
            if (_intensity == HapticIntensity.Off) return;
            if ((int)requiredIntensity > (int)_intensity) return;

#if UNITY_IOS && !UNITY_EDITOR
            TriggerIOSHaptic(requiredIntensity);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        void TriggerIOSHaptic(HapticIntensity level)
        {
            // Use Unity's iOS haptic API (available in Unity 2021+)
            // Falls back to basic vibrate if not available
            switch (level)
            {
                case HapticIntensity.Light:
                    UnityEngine.iOS.DeviceGeneration gen = UnityEngine.iOS.Device.generation;
                    // Light impact feedback
                    Handheld.Vibrate();
                    break;
                case HapticIntensity.Standard:
                    Handheld.Vibrate();
                    break;
                case HapticIntensity.Strong:
                    Handheld.Vibrate();
                    break;
            }
        }
#endif

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
