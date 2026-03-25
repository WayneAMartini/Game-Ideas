using UnityEngine;
using UnityEngine.UI;
using Ascendant.Combat;

namespace Ascendant.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] Slider _slider;
        [SerializeField] Image _fillImage;
        [SerializeField] Vector3 _offset = new Vector3(0f, 0.8f, 0f);

        Enemy _enemy;
        Transform _enemyTransform;
        Canvas _healthCanvas;

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemyTransform = enemy.transform;
            if (_slider != null)
                _slider.value = 1f;
        }

        void Start()
        {
            // Auto-initialize from sibling Enemy component when on the same GameObject
            if (_enemy == null)
            {
                var enemy = GetComponent<Enemy>();
                if (enemy != null)
                    Initialize(enemy);
            }
            _healthCanvas = GetComponentInChildren<Canvas>();
        }

        void LateUpdate()
        {
            if (_enemy == null)
                return; // Not initialized yet — skip, don't destroy

            if (_enemy.IsDead)
            {
                // Hide health bar canvas, don't destroy the enemy root
                if (_healthCanvas != null)
                    _healthCanvas.gameObject.SetActive(false);
                return;
            }

            // Health bar canvas follows via local offset (child of enemy)
            // No need to reposition if it's a child object

            // Update fill
            if (_slider == null) return;
            float pct = _enemy.CurrentHp / _enemy.MaxHp;
            _slider.value = pct;

            // Color: green -> yellow -> red
            if (_fillImage != null)
            {
                if (pct > 0.5f)
                    _fillImage.color = Color.Lerp(Color.yellow, Color.green, (pct - 0.5f) * 2f);
                else
                    _fillImage.color = Color.Lerp(Color.red, Color.yellow, pct * 2f);
            }
        }
    }
}
