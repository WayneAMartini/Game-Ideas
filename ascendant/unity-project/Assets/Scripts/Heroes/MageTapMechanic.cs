using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class MageTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Arcane Bolt Config")]
        [SerializeField] float _chainDamagePercent = 0.5f; // 50% damage on chain
        [SerializeField] int _highMomentumThreshold = 60;
        [SerializeField] int _splitCount = 3;

        [Header("Passive — Mana Surge")]
        [SerializeField] float _manaSurgeThreshold = 0.8f; // 80% mana
        [SerializeField] float _cooldownRecoveryBonus = 0.3f; // 30% faster
        [SerializeField] float _magicDamageBonus = 0.15f; // +15%

        float _mana = 100f;
        float _maxMana = 100f;

        public float Mana => _mana;
        public float MaxMana => _maxMana;
        public float ManaPercent => _mana / _maxMana;
        public bool IsManaSurgeActive => ManaPercent > _manaSurgeThreshold;
        public float CooldownRecoveryMultiplier => IsManaSurgeActive ? (1f + _cooldownRecoveryBonus) : 1f;
        public float MagicDamageMultiplier => IsManaSurgeActive ? (1f + _magicDamageBonus) : 1f;

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            float finalDamage = damage * MagicDamageMultiplier;
            int momentum = MomentumSystem.Instance != null ? MomentumSystem.Instance.Stacks : 0;

            var target = EnemyManager.Instance?.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

            // High momentum: split into multiple projectiles
            if (momentum > _highMomentumThreshold)
            {
                float splitDamage = finalDamage / _splitCount;
                var enemies = EnemyManager.Instance.GetAllAliveEnemies();

                int hitCount = Mathf.Min(_splitCount, enemies.Count);
                for (int i = 0; i < hitCount; i++)
                {
                    var enemy = enemies[i];
                    enemy.TakeDamage(splitDamage, DamageType.Magical);

                    EventBus.Publish(new EnemyDamagedEvent
                    {
                        EnemyId = enemy.Id,
                        Damage = splitDamage,
                        IsCritical = false,
                        IsAoE = true,
                        WorldPosition = enemy.transform.position
                    });
                }
            }
            else
            {
                // Normal: single homing bolt
                target.TakeDamage(finalDamage, DamageType.Magical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = target.Id,
                    Damage = finalDamage,
                    IsCritical = false,
                    IsAoE = false,
                    WorldPosition = target.transform.position
                });

                // Chain on kill: if target dies, chain to 1 nearby enemy at 50%
                if (target.IsDead)
                {
                    TryChain(target, finalDamage, worldPosition);
                }
            }
        }

        void TryChain(Enemy killedEnemy, float originalDamage, Vector3 origin)
        {
            float chainDamage = originalDamage * _chainDamagePercent;
            var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
            if (enemies == null || enemies.Count == 0) return;

            // Find nearest alive enemy that isn't the killed one
            Enemy nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == killedEnemy || enemy.IsDead) continue;
                float dist = Vector3.SqrMagnitude(enemy.transform.position - origin);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            if (nearest != null)
            {
                nearest.TakeDamage(chainDamage, DamageType.Magical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = nearest.Id,
                    Damage = chainDamage,
                    IsCritical = false,
                    IsAoE = false,
                    WorldPosition = nearest.transform.position
                });
            }
        }

        public void ConsumeMana(float amount)
        {
            _mana = Mathf.Max(0f, _mana - amount);
        }

        public void RegenerateMana(float amount)
        {
            _mana = Mathf.Min(_maxMana, _mana + amount);
        }

        public void Reset()
        {
            _mana = _maxMana;
        }
    }
}
