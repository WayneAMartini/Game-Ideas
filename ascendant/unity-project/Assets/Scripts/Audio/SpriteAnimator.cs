using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Audio
{
    public class SpriteAnimator : MonoBehaviour
    {
        public static SpriteAnimator Instance { get; private set; }

        [Header("Screen Shake")]
        [SerializeField] float _shakeIntensity = 0.1f;
        [SerializeField] float _shakeDuration = 0.15f;

        Camera _camera;
        Vector3 _cameraOriginalPos;
        float _shakeTimer;
        bool _isShaking;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            _camera = GameManager.Instance?.MainCamera;
            if (_camera != null)
                _cameraOriginalPos = _camera.transform.localPosition;
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<BossPhaseChangedEvent>(OnBossPhaseChanged);
        }

        void Update()
        {
            if (_isShaking)
            {
                _shakeTimer -= Time.deltaTime;
                if (_shakeTimer <= 0f)
                {
                    _isShaking = false;
                    if (_camera != null)
                        _camera.transform.localPosition = _cameraOriginalPos;
                }
                else if (_camera != null)
                {
                    float t = _shakeTimer / _shakeDuration;
                    float offsetX = Random.Range(-_shakeIntensity, _shakeIntensity) * t;
                    float offsetY = Random.Range(-_shakeIntensity, _shakeIntensity) * t;
                    _camera.transform.localPosition = _cameraOriginalPos + new Vector3(offsetX, offsetY, 0f);
                }
            }
        }

        // --- Public animation methods ---

        public void TriggerScreenShake(float intensity = -1f, float duration = -1f)
        {
            _isShaking = true;
            _shakeTimer = duration > 0 ? duration : _shakeDuration;
            if (intensity > 0) _shakeIntensity = intensity;
            if (_camera != null)
                _cameraOriginalPos = _camera.transform.localPosition;
        }

        public void AnimateHeroIdle(Transform heroTransform)
        {
            if (heroTransform == null) return;
            StartCoroutine(IdleBobCoroutine(heroTransform));
        }

        public void AnimateHeroAttack(Transform heroTransform)
        {
            if (heroTransform == null) return;
            StartCoroutine(AttackLungeCoroutine(heroTransform));
        }

        public void AnimateEnemyHitFlash(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null) return;
            StartCoroutine(HitFlashCoroutine(spriteRenderer));
        }

        public void AnimateEnemyDeath(Transform enemyTransform, SpriteRenderer spriteRenderer)
        {
            if (enemyTransform == null) return;
            StartCoroutine(DeathCoroutine(enemyTransform, spriteRenderer));
        }

        public void AnimateAbilityCast(Transform heroTransform, SpriteRenderer spriteRenderer)
        {
            if (heroTransform == null) return;
            StartCoroutine(AbilityCastCoroutine(heroTransform, spriteRenderer));
        }

        // --- Coroutines ---

        System.Collections.IEnumerator IdleBobCoroutine(Transform t)
        {
            Vector3 startPos = t.localPosition;
            float bobAmount = 0.05f;
            float bobSpeed = 2f;
            float elapsed = 0f;

            while (t != null && t.gameObject.activeInHierarchy)
            {
                elapsed += Time.deltaTime;
                float offsetY = Mathf.Sin(elapsed * bobSpeed) * bobAmount;
                t.localPosition = startPos + new Vector3(0f, offsetY, 0f);
                yield return null;
            }
        }

        System.Collections.IEnumerator AttackLungeCoroutine(Transform t)
        {
            if (t == null) yield break;
            Vector3 startPos = t.localPosition;
            float lungeDistance = 0.3f;
            float lungeTime = 0.08f;
            float returnTime = 0.12f;

            // Lunge forward
            float elapsed = 0f;
            while (elapsed < lungeTime && t != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / lungeTime;
                t.localPosition = startPos + new Vector3(lungeDistance * progress, 0f, 0f);
                yield return null;
            }

            // Return
            elapsed = 0f;
            Vector3 lungePos = t != null ? t.localPosition : startPos;
            while (elapsed < returnTime && t != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / returnTime;
                t.localPosition = Vector3.Lerp(lungePos, startPos, progress);
                yield return null;
            }

            if (t != null) t.localPosition = startPos;
        }

        System.Collections.IEnumerator HitFlashCoroutine(SpriteRenderer sr)
        {
            if (sr == null) yield break;
            Color originalColor = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (sr != null) sr.color = originalColor;
        }

        System.Collections.IEnumerator DeathCoroutine(Transform t, SpriteRenderer sr)
        {
            if (t == null) yield break;
            float duration = 0.4f;
            float elapsed = 0f;
            Vector3 startScale = t.localScale;
            Color startColor = sr != null ? sr.color : Color.white;
            float startRotation = t.eulerAngles.z;

            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Shrink
                t.localScale = startScale * (1f - progress);
                // Rotate
                t.eulerAngles = new Vector3(0f, 0f, startRotation + progress * 180f);
                // Fade
                if (sr != null)
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - progress);

                yield return null;
            }
        }

        System.Collections.IEnumerator AbilityCastCoroutine(Transform t, SpriteRenderer sr)
        {
            if (t == null) yield break;
            Vector3 startScale = t.localScale;
            Color startColor = sr != null ? sr.color : Color.white;
            float duration = 0.3f;
            float elapsed = 0f;

            // Scale up + glow
            while (elapsed < duration / 2f && t != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);
                t.localScale = startScale * (1f + 0.2f * progress);
                if (sr != null)
                    sr.color = Color.Lerp(startColor, Color.white, progress * 0.5f);
                yield return null;
            }

            // Scale back down
            elapsed = 0f;
            while (elapsed < duration / 2f && t != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration / 2f);
                t.localScale = startScale * (1.2f - 0.2f * progress);
                if (sr != null)
                    sr.color = Color.Lerp(Color.Lerp(startColor, Color.white, 0.5f), startColor, progress);
                yield return null;
            }

            if (t != null) t.localScale = startScale;
            if (sr != null) sr.color = startColor;
        }

        // --- Event handlers ---

        void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (evt.IsCritical)
                TriggerScreenShake(0.05f, 0.1f);
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            // Death animation handled by Enemy.cs calling AnimateEnemyDeath
        }

        void OnBossPhaseChanged(BossPhaseChangedEvent evt)
        {
            TriggerScreenShake(0.15f, 0.3f);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
