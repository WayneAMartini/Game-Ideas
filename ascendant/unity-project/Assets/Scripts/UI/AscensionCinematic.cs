using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class AscensionCinematic : MonoBehaviour
    {
        public static AscensionCinematic Instance { get; private set; }

        [Header("Cinematic Elements")]
        [SerializeField] GameObject _cinematicPanel;
        [SerializeField] Image _heroSprite;
        [SerializeField] Image _goldenOverlay;
        [SerializeField] Image _tierEmblem;
        [SerializeField] TextMeshProUGUI _ascensionText;
        [SerializeField] CanvasGroup _fadeGroup;

        [Header("Tier Emblems")]
        [SerializeField] Sprite _awakenedEmblem;
        [SerializeField] Sprite _exaltedEmblem;
        [SerializeField] Sprite _mythicEmblem;
        [SerializeField] Sprite _demigodEmblem;

        [Header("Timing")]
        [SerializeField] float _riseDuration = 2f;
        [SerializeField] float _burstDuration = 0.5f;
        [SerializeField] float _emblemDuration = 1.5f;
        [SerializeField] float _fadeOutDuration = 0.5f;

        [Header("Skip")]
        [SerializeField] Button _skipButton;
        [SerializeField] GameObject _skipPrompt;

        Action _onComplete;
        Coroutine _activeSequence;
        bool _skipped;

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
            if (_skipButton != null)
                _skipButton.onClick.AddListener(SkipCinematic);
            if (_cinematicPanel != null)
                _cinematicPanel.SetActive(false);
        }

        public void PlayAscensionSequence(int heroSlot, Action onComplete)
        {
            _onComplete = onComplete;
            _skipped = false;

            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data?.portrait != null && _heroSprite != null)
                _heroSprite.sprite = hero.Data.portrait;

            // Determine tier for emblem
            var tierSystem = TierBonusSystem.Instance;
            int ascCount = tierSystem?.GetAscensionCount(heroSlot) ?? 0;
            var newTier = AscensionTierData.GetTierForAscensions(ascCount + 1);
            SetTierEmblem(newTier);

            if (_ascensionText != null)
                _ascensionText.text = $"Ascension #{ascCount + 1}";

            if (_cinematicPanel != null)
                _cinematicPanel.SetActive(true);
            if (_skipPrompt != null)
                _skipPrompt.SetActive(ascCount > 0); // Show skip after first ascension

            _activeSequence = StartCoroutine(AscensionSequence());
        }

        IEnumerator AscensionSequence()
        {
            // Phase 1: Hero rises through clouds
            if (_fadeGroup != null) _fadeGroup.alpha = 1f;
            if (_heroSprite != null)
            {
                var rt = _heroSprite.rectTransform;
                Vector2 startPos = new Vector2(rt.anchoredPosition.x, -300f);
                Vector2 endPos = new Vector2(rt.anchoredPosition.x, 100f);
                rt.anchoredPosition = startPos;

                float elapsed = 0f;
                while (elapsed < _riseDuration && !_skipped)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / _riseDuration);
                    rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }
                rt.anchoredPosition = endPos;
            }

            if (_skipped) { Complete(); yield break; }

            // Phase 2: Golden light burst
            if (_goldenOverlay != null)
            {
                _goldenOverlay.gameObject.SetActive(true);
                var color = _goldenOverlay.color;
                float elapsed = 0f;
                while (elapsed < _burstDuration && !_skipped)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _burstDuration;
                    color.a = Mathf.Sin(t * Mathf.PI); // Fade in and out
                    _goldenOverlay.color = color;
                    yield return null;
                }
                color.a = 0f;
                _goldenOverlay.color = color;
            }

            if (_skipped) { Complete(); yield break; }

            // Phase 3: Tier emblem appears
            if (_tierEmblem != null)
            {
                _tierEmblem.gameObject.SetActive(true);
                var rt = _tierEmblem.rectTransform;
                float elapsed = 0f;
                while (elapsed < _emblemDuration && !_skipped)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _emblemDuration;
                    float scale = 1f + 0.2f * Mathf.Sin(t * Mathf.PI * 2f); // Pulse
                    rt.localScale = Vector3.one * scale;
                    yield return null;
                }
                rt.localScale = Vector3.one;
            }

            if (_skipped) { Complete(); yield break; }

            // Phase 4: Fade out
            if (_fadeGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < _fadeOutDuration && !_skipped)
                {
                    elapsed += Time.deltaTime;
                    _fadeGroup.alpha = 1f - (elapsed / _fadeOutDuration);
                    yield return null;
                }
            }

            Complete();
        }

        void SkipCinematic()
        {
            _skipped = true;
        }

        void Complete()
        {
            if (_activeSequence != null)
            {
                StopCoroutine(_activeSequence);
                _activeSequence = null;
            }

            if (_cinematicPanel != null)
                _cinematicPanel.SetActive(false);
            if (_goldenOverlay != null)
                _goldenOverlay.gameObject.SetActive(false);
            if (_tierEmblem != null)
                _tierEmblem.gameObject.SetActive(false);

            _onComplete?.Invoke();
            _onComplete = null;
        }

        void SetTierEmblem(AscensionTierLevel tier)
        {
            if (_tierEmblem == null) return;

            _tierEmblem.sprite = tier switch
            {
                AscensionTierLevel.Awakened => _awakenedEmblem,
                AscensionTierLevel.Exalted => _exaltedEmblem,
                AscensionTierLevel.Mythic => _mythicEmblem,
                AscensionTierLevel.Demigod => _demigodEmblem,
                _ => null
            };

            _tierEmblem.gameObject.SetActive(false); // Will be shown during sequence
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
