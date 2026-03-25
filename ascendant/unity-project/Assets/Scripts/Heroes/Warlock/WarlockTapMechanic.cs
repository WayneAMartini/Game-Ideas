using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class WarlockTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 0;
        [SerializeField] float _selfDamagePercent = 0.01f;
        [SerializeField] float _damageMultiplier = 1.5f;

        [Header("State")]
        [SerializeField] bool _isDarkPactActive = true;

        // Expose properties
        public float SelfDamagePercent => _selfDamagePercent;
        public float DamageMultiplier => _damageMultiplier;
        public bool IsDarkPactActive => _isDarkPactActive;

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null || !hero.IsAlive)
                return;

            // Self-damage: drain 1% of current HP
            float selfDmg = hero.CurrentHp * _selfDamagePercent;
            // Only drain if we won't kill the hero (keep at least 1 HP)
            if (hero.CurrentHp - selfDmg >= 1f)
            {
                EventBus.Publish(new HeroDamagedEvent
                {
                    HeroSlot = _heroSlot,
                    Damage = selfDmg,
                    CurrentHp = hero.CurrentHp - selfDmg,
                    MaxHp = hero.MaxHp
                });
            }

            // Deal 150% tap damage as dark fire magic to nearest enemy
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead)
                return;

            float outDamage = damage * _damageMultiplier;
            enemy.TakeDamage(outDamage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = outDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });
        }

        public void Reset()
        {
            _isDarkPactActive = true;
        }
    }
}
