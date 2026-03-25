using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;

namespace Ascendant.UI
{
    public class UIPolish : MonoBehaviour
    {
        public static UIPolish Instance { get; private set; }

        [Header("Toast Notification")]
        [SerializeField] RectTransform _toastPanel;
        [SerializeField] TextMeshProUGUI _toastText;
        [SerializeField] float _toastDuration = 2f;
        [SerializeField] float _toastSlideSpeed = 600f;

        float _toastTimer;
        bool _toastVisible;
        Vector2 _toastHiddenPos;
        Vector2 _toastVisiblePos;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_toastPanel != null)
            {
                _toastVisiblePos = _toastPanel.anchoredPosition;
                _toastHiddenPos = _toastVisiblePos + new Vector2(0f, 100f);
                _toastPanel.anchoredPosition = _toastHiddenPos;
                _toastPanel.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<AscensionEvent>(OnAscension);
            EventBus.Subscribe<EquipmentDropEvent>(OnEquipmentDrop);
            EventBus.Subscribe<SynergyActivatedEvent>(OnSynergyActivated);
            EventBus.Subscribe<ComboDiscoveredEvent>(OnComboDiscovered);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<AscensionEvent>(OnAscension);
            EventBus.Unsubscribe<EquipmentDropEvent>(OnEquipmentDrop);
            EventBus.Unsubscribe<SynergyActivatedEvent>(OnSynergyActivated);
            EventBus.Unsubscribe<ComboDiscoveredEvent>(OnComboDiscovered);
        }

        void Update()
        {
            UpdateToast();
        }

        // --- Toast Notification ---

        public void ShowToast(string message)
        {
            if (_toastPanel == null || _toastText == null) return;

            _toastText.text = message;
            _toastPanel.gameObject.SetActive(true);
            _toastVisible = true;
            _toastTimer = _toastDuration;
        }

        void UpdateToast()
        {
            if (_toastPanel == null) return;

            if (_toastVisible)
            {
                // Slide in
                _toastPanel.anchoredPosition = Vector2.MoveTowards(
                    _toastPanel.anchoredPosition, _toastVisiblePos,
                    _toastSlideSpeed * Time.deltaTime);

                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f)
                    _toastVisible = false;
            }
            else if (_toastPanel.gameObject.activeSelf)
            {
                // Slide out
                _toastPanel.anchoredPosition = Vector2.MoveTowards(
                    _toastPanel.anchoredPosition, _toastHiddenPos,
                    _toastSlideSpeed * Time.deltaTime);

                if (Vector2.Distance(_toastPanel.anchoredPosition, _toastHiddenPos) < 1f)
                    _toastPanel.gameObject.SetActive(false);
            }
        }

        // --- Button Press Animation ---

        public static void AnimateButtonPress(RectTransform button)
        {
            if (button == null) return;
            var anim = button.gameObject.GetComponent<ButtonPressAnimator>();
            if (anim == null)
                anim = button.gameObject.AddComponent<ButtonPressAnimator>();
            anim.TriggerPress();
        }

        // --- Number Counter Animation ---

        public static void AnimateCounter(TextMeshProUGUI text, double fromValue, double toValue, float duration = 0.5f)
        {
            if (text == null) return;
            var anim = text.gameObject.GetComponent<CounterAnimator>();
            if (anim == null)
                anim = text.gameObject.AddComponent<CounterAnimator>();
            anim.Animate(fromValue, toValue, duration);
        }

        // --- Progress Bar Smooth Fill ---

        public static void AnimateProgressBar(Slider slider, float targetValue, float duration = 0.3f)
        {
            if (slider == null) return;
            var anim = slider.gameObject.GetComponent<ProgressBarAnimator>();
            if (anim == null)
                anim = slider.gameObject.AddComponent<ProgressBarAnimator>();
            anim.Animate(targetValue, duration);
        }

        // --- Event Handlers ---

        void OnLevelUp(LevelUpEvent evt) =>
            ShowToast($"Level Up! {evt.ClassId} reached level {evt.NewLevel}");

        void OnBossDefeated(BossDefeatedEvent evt) =>
            ShowToast($"Boss Defeated: {evt.BossName}!");

        void OnAscension(AscensionEvent evt) =>
            ShowToast($"Ascension #{evt.AscensionCount}! Earned {evt.ShardsEarned:F0} shards");

        void OnEquipmentDrop(EquipmentDropEvent evt) =>
            ShowToast($"New {evt.Rarity} Equipment: {evt.EquipmentName}");

        void OnSynergyActivated(SynergyActivatedEvent evt) =>
            ShowToast($"Synergy: {evt.SynergyName}!");

        void OnComboDiscovered(ComboDiscoveredEvent evt) =>
            ShowToast($"New Combo Discovered: {evt.ComboName}!");

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    // --- Helper components ---

    public class ButtonPressAnimator : MonoBehaviour
    {
        float _timer;
        bool _animating;
        Vector3 _originalScale;
        const float PressDuration = 0.1f;
        const float ReturnDuration = 0.15f;
        const float PressScale = 0.95f;

        void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void TriggerPress()
        {
            _animating = true;
            _timer = 0f;
        }

        void Update()
        {
            if (!_animating) return;

            _timer += Time.deltaTime;

            if (_timer < PressDuration)
            {
                float t = _timer / PressDuration;
                transform.localScale = Vector3.Lerp(_originalScale, _originalScale * PressScale, t);
            }
            else if (_timer < PressDuration + ReturnDuration)
            {
                float t = (_timer - PressDuration) / ReturnDuration;
                // Bounce back with slight overshoot
                float scale = Mathf.Lerp(PressScale, 1f, t) + Mathf.Sin(t * Mathf.PI) * 0.02f;
                transform.localScale = _originalScale * scale;
            }
            else
            {
                transform.localScale = _originalScale;
                _animating = false;
            }
        }
    }

    public class CounterAnimator : MonoBehaviour
    {
        TextMeshProUGUI _text;
        double _from, _to;
        float _duration, _elapsed;
        bool _animating;

        void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void Animate(double from, double to, float duration)
        {
            _from = from;
            _to = to;
            _duration = duration;
            _elapsed = 0f;
            _animating = true;
        }

        void Update()
        {
            if (!_animating || _text == null) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            // Ease out
            t = 1f - (1f - t) * (1f - t);
            double current = _from + (_to - _from) * t;
            _text.text = FormatNumber(current);

            if (_elapsed >= _duration)
                _animating = false;
        }

        static string FormatNumber(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:F1}M";
            if (value >= 1_000) return $"{value / 1_000:F1}K";
            return Mathf.RoundToInt((float)value).ToString();
        }
    }

    public class ProgressBarAnimator : MonoBehaviour
    {
        Slider _slider;
        float _target, _duration, _elapsed, _startValue;
        bool _animating;

        void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        public void Animate(float target, float duration)
        {
            _startValue = _slider != null ? _slider.value : 0f;
            _target = target;
            _duration = duration;
            _elapsed = 0f;
            _animating = true;
        }

        void Update()
        {
            if (!_animating || _slider == null) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _slider.value = Mathf.Lerp(_startValue, _target, t);

            if (_elapsed >= _duration)
                _animating = false;
        }
    }
}
