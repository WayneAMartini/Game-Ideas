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

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemyTransform = enemy.transform;
            _slider.value = 1f;
        }

        void LateUpdate()
        {
            if (_enemy == null || _enemy.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            // Follow enemy
            transform.position = _enemyTransform.position + _offset;

            // Update fill
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
