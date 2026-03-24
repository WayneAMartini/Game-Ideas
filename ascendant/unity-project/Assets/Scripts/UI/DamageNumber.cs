using UnityEngine;
using TMPro;

namespace Ascendant.UI
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] TextMeshPro _text;
        [SerializeField] float _riseSpeed = 1.5f;
        [SerializeField] float _lifetime = 0.8f;
        [SerializeField] float _fadeStartPercent = 0.5f;
        [SerializeField] float _baseFontSize = 4f;
        [SerializeField] float _critFontSize = 6f;

        float _timer;
        Color _baseColor;
        System.Action<DamageNumber> _onComplete;

        public void Show(float damage, Vector3 worldPos, bool isCritical,
            System.Action<DamageNumber> onComplete)
        {
            _onComplete = onComplete;
            _timer = 0f;

            transform.position = worldPos + new Vector3(
                Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.2f), 0f);

            _text.text = FormatDamage(damage);

            if (isCritical)
            {
                _text.fontSize = _critFontSize;
                _baseColor = new Color(1f, 0.85f, 0f); // gold
            }
            else
            {
                // Scale font size with damage magnitude
                float scale = Mathf.Clamp(Mathf.Log10(Mathf.Max(1f, damage)) / 4f, 0.7f, 1.5f);
                _text.fontSize = _baseFontSize * scale;
                _baseColor = Color.white;
            }

            _text.color = _baseColor;
        }

        void Update()
        {
            _timer += Time.deltaTime;

            // Rise
            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

            // Fade
            float progress = _timer / _lifetime;
            if (progress > _fadeStartPercent)
            {
                float fadeProgress = (progress - _fadeStartPercent) / (1f - _fadeStartPercent);
                var c = _baseColor;
                c.a = 1f - fadeProgress;
                _text.color = c;
            }

            if (_timer >= _lifetime)
            {
                _onComplete?.Invoke(this);
            }
        }

        static string FormatDamage(float damage)
        {
            if (damage >= 1_000_000f) return $"{damage / 1_000_000f:F1}M";
            if (damage >= 1_000f) return $"{damage / 1_000f:F1}K";
            return Mathf.RoundToInt(damage).ToString();
        }
    }
}
