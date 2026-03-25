using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;

namespace Ascendant.UI
{
    public enum TextSizePreset
    {
        Normal,
        Large,
        ExtraLarge
    }

    public enum ColorblindMode
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia
    }

    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] TextSizePreset _textSize = TextSizePreset.Normal;
        [SerializeField] ColorblindMode _colorblindMode = ColorblindMode.None;
        [SerializeField] bool _tapAssistEnabled;
        [SerializeField] bool _autoBattleEnabled;
        [SerializeField] bool _reducedMotion;

        [Header("Canvas Scaler Reference")]
        [SerializeField] CanvasScaler _mainCanvasScaler;

        public TextSizePreset TextSize
        {
            get => _textSize;
            set { _textSize = value; ApplyTextSize(); SavePrefs(); }
        }

        public ColorblindMode Colorblind
        {
            get => _colorblindMode;
            set { _colorblindMode = value; ApplyColorblindMode(); SavePrefs(); }
        }

        public bool TapAssistEnabled
        {
            get => _tapAssistEnabled;
            set { _tapAssistEnabled = value; SavePrefs(); }
        }

        public bool AutoBattleEnabled
        {
            get => _autoBattleEnabled;
            set
            {
                _autoBattleEnabled = value;
                SavePrefs();
                EventBus.Publish(new AutoBattleChangedEvent { Enabled = value });
            }
        }

        public bool ReducedMotion
        {
            get => _reducedMotion;
            set { _reducedMotion = value; SavePrefs(); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            LoadPrefs();
        }

        void Start()
        {
            ApplyTextSize();
            ApplyColorblindMode();
        }

        // --- Text Size ---

        void ApplyTextSize()
        {
            float scaleFactor = _textSize switch
            {
                TextSizePreset.Normal => 1f,
                TextSizePreset.Large => 1.25f,
                TextSizePreset.ExtraLarge => 1.5f,
                _ => 1f
            };

            if (_mainCanvasScaler != null)
                _mainCanvasScaler.scaleFactor = scaleFactor;
        }

        // --- Colorblind Mode ---

        void ApplyColorblindMode()
        {
            // Update all affinity indicators in the scene
            var indicators = FindObjectsByType<AffinityIndicator>(FindObjectsSortMode.None);
            foreach (var indicator in indicators)
                indicator.RefreshColor();
        }

        public Color RemapAffinityColor(Combat.Affinity affinity, Color originalColor)
        {
            if (_colorblindMode == ColorblindMode.None)
                return originalColor;

            return (_colorblindMode, affinity) switch
            {
                // Protanopia: red-blind — replace reds with blues
                (ColorblindMode.Protanopia, Combat.Affinity.Flame) => new Color(0.9f, 0.6f, 0f),
                (ColorblindMode.Protanopia, Combat.Affinity.Nature) => new Color(0f, 0.5f, 0.8f),

                // Deuteranopia: green-blind — replace greens with yellows
                (ColorblindMode.Deuteranopia, Combat.Affinity.Nature) => new Color(0.8f, 0.8f, 0f),
                (ColorblindMode.Deuteranopia, Combat.Affinity.Flame) => new Color(1f, 0.3f, 0f),

                // Tritanopia: blue-blind — replace blues with pinks
                (ColorblindMode.Tritanopia, Combat.Affinity.Frost) => new Color(1f, 0.5f, 0.7f),
                (ColorblindMode.Tritanopia, Combat.Affinity.Storm) => new Color(0.8f, 0.4f, 0.6f),

                _ => originalColor
            };
        }

        // --- Tap Assist ---

        public float GetTapTargetScale()
        {
            return _tapAssistEnabled ? 1.5f : 1f;
        }

        // --- VoiceOver Support ---

        public static void SetAccessibilityLabel(GameObject go, string label)
        {
            if (go == null) return;
            // Tag elements for VoiceOver. On iOS, Unity's AccessibilityNode handles this.
            // For now we store in the name for accessibility tools to pick up.
            var existingLabel = go.GetComponent<AccessibilityLabel>();
            if (existingLabel == null)
                existingLabel = go.AddComponent<AccessibilityLabel>();
            existingLabel.Label = label;
        }

        // --- Persistence ---

        void LoadPrefs()
        {
            _textSize = (TextSizePreset)PlayerPrefs.GetInt("a11y_text_size", 0);
            _colorblindMode = (ColorblindMode)PlayerPrefs.GetInt("a11y_colorblind", 0);
            _tapAssistEnabled = PlayerPrefs.GetInt("a11y_tap_assist", 0) == 1;
            _autoBattleEnabled = PlayerPrefs.GetInt("a11y_auto_battle", 0) == 1;
            _reducedMotion = PlayerPrefs.GetInt("a11y_reduced_motion", 0) == 1;
        }

        void SavePrefs()
        {
            PlayerPrefs.SetInt("a11y_text_size", (int)_textSize);
            PlayerPrefs.SetInt("a11y_colorblind", (int)_colorblindMode);
            PlayerPrefs.SetInt("a11y_tap_assist", _tapAssistEnabled ? 1 : 0);
            PlayerPrefs.SetInt("a11y_auto_battle", _autoBattleEnabled ? 1 : 0);
            PlayerPrefs.SetInt("a11y_reduced_motion", _reducedMotion ? 1 : 0);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    // Simple component for tagging UI elements with accessibility labels
    public class AccessibilityLabel : MonoBehaviour
    {
        public string Label;
    }

    // Event for auto-battle toggle
    public struct AutoBattleChangedEvent
    {
        public bool Enabled;
    }
}
