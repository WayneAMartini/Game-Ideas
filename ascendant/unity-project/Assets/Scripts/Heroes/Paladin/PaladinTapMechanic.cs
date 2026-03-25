using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class PaladinTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 0;
        [SerializeField] float _healPercent = 0.20f;
        [SerializeField] float _divineShieldMaxCooldown = 30f;
        [SerializeField] float _divineShieldHpThreshold = 0.20f;
        [SerializeField] float _divineShieldHealToPercent = 0.30f;

        [Header("State")]
        [SerializeField] float _divineShieldCooldown = 0f;

        // Expose properties
        public float HealPercent => _healPercent;
        public float DivineShieldCooldown => _divineShieldCooldown;
        public float DivineShieldMaxCooldown => _divineShieldMaxCooldown;
        public bool IsDivineShieldReady => _divineShieldCooldown <= 0f;

        void OnEnable()
        {
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
        }

        void Update()
        {
            // Tick Divine Shield cooldown down
            if (_divineShieldCooldown > 0f)
                _divineShieldCooldown = Mathf.Max(0f, _divineShieldCooldown - Time.deltaTime);
        }

        void OnHeroDamaged(HeroDamagedEvent e)
        {
            if (e.HeroSlot != _heroSlot) return;
            if (!IsDivineShieldReady) return;

            // Check if resulting HP is below the threshold
            float hpRatio = e.CurrentHp / e.MaxHp;
            if (hpRatio < _divineShieldHpThreshold)
            {
                var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
                if (hero == null || !hero.IsAlive) return;

                // Negate incoming damage by healing back to threshold
                float targetHp = hero.MaxHp * _divineShieldHealToPercent;
                float healAmount = Mathf.Max(0f, targetHp - e.CurrentHp);

                if (healAmount > 0f)
                    hero.Heal(healAmount);

                EventBus.Publish(new HeroHealedEvent
                {
                    HeroSlot = _heroSlot,
                    Amount = healAmount,
                    CurrentHp = hero.CurrentHp,
                    MaxHp = hero.MaxHp
                });

                // Trigger cooldown
                _divineShieldCooldown = _divineShieldMaxCooldown;
            }
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            // Deal holy damage
            enemy.TakeDamage(damage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Heal lowest-HP ally for 20% of damage dealt
            float healAmount = damage * _healPercent;
            var allyToHeal = Party.PartyManager.Instance.GetLowestHpAliveHero();
            if (allyToHeal != null && allyToHeal.IsAlive)
            {
                allyToHeal.Heal(healAmount);

                EventBus.Publish(new HeroHealedEvent
                {
                    HeroSlot = allyToHeal.Slot,
                    Amount = healAmount,
                    CurrentHp = allyToHeal.CurrentHp,
                    MaxHp = allyToHeal.MaxHp
                });
            }
        }

        public void Reset()
        {
            _divineShieldCooldown = 0f;
        }
    }
}
