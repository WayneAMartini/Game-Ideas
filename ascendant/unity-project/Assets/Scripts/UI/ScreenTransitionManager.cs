using UnityEngine;
using UnityEngine.UI;
using System;

namespace Ascendant.UI
{
    public class ScreenTransitionManager : MonoBehaviour
    {
        public static ScreenTransitionManager Instance { get; private set; }

        [Header("Fade Overlay")]
        [SerializeField] Image _fadeOverlay;
        [SerializeField] float _fadeDuration = 0.3f;

        bool _isFading;
        float _fadeTimer;
        bool _fadingIn; // true = going from black to clear
        Action _onFadeComplete;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_fadeOverlay != null)
            {
                _fadeOverlay.color = Color.clear;
                _fadeOverlay.raycastTarget = false;
            }
        }

        void Update()
        {
            if (!_isFading || _fadeOverlay == null) return;

            _fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_fadeTimer / _fadeDuration);

            float alpha = _fadingIn ? (1f - t) : t;
            _fadeOverlay.color = new Color(0f, 0f, 0f, alpha);
            _fadeOverlay.raycastTarget = true;

            if (t >= 1f)
            {
                _isFading = false;
                _fadeOverlay.raycastTarget = alpha > 0.01f;
                _onFadeComplete?.Invoke();
                _onFadeComplete = null;
            }
        }

        public void FadeOut(Action onComplete = null)
        {
            _fadingIn = false;
            _fadeTimer = 0f;
            _isFading = true;
            _onFadeComplete = onComplete;
        }

        public void FadeIn(Action onComplete = null)
        {
            _fadingIn = true;
            _fadeTimer = 0f;
            _isFading = true;
            _onFadeComplete = onComplete;
        }

        public void TransitionTo(Action midTransitionAction)
        {
            FadeOut(() =>
            {
                midTransitionAction?.Invoke();
                FadeIn();
            });
        }

        public void SlidePanel(RectTransform panel, Vector2 from, Vector2 to, float duration, Action onComplete = null)
        {
            if (panel == null) return;
            StartCoroutine(SlidePanelCoroutine(panel, from, to, duration, onComplete));
        }

        System.Collections.IEnumerator SlidePanelCoroutine(RectTransform panel, Vector2 from, Vector2 to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            panel.anchoredPosition = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out cubic
                t = 1f - (1f - t) * (1f - t) * (1f - t);
                panel.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }

            panel.anchoredPosition = to;
            onComplete?.Invoke();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
