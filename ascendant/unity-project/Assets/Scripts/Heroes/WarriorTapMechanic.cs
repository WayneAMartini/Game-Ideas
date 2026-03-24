using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class WarriorTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Shockwave Config")]
        [SerializeField] int _shockwaveEveryNTaps = 5;
        [SerializeField] float _shockwaveDamagePercent = 0.5f; // 50% of tap damage

        int _tapCounter;

        public int TapCounter => _tapCounter;
        public int TapsUntilShockwave => _shockwaveEveryNTaps - (_tapCounter % _shockwaveEveryNTaps);

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            _tapCounter++;

            if (_tapCounter % _shockwaveEveryNTaps == 0)
            {
                TriggerShockwave(damage, worldPosition);
            }
        }

        void TriggerShockwave(float tapDamage, Vector3 origin)
        {
            float shockwaveDamage = tapDamage * _shockwaveDamagePercent;

            // Hit all alive enemies
            var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                enemy.TakeDamage(shockwaveDamage);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = shockwaveDamage,
                    IsCritical = false,
                    IsAoE = true,
                    WorldPosition = enemy.transform.position
                });
            }

            EventBus.Publish(new ShockwaveTriggeredEvent
            {
                Damage = shockwaveDamage,
                Origin = origin
            });
        }

        public void Reset()
        {
            _tapCounter = 0;
        }
    }
}
