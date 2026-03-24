using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Combat
{
    public class AutoAttackSystem : MonoBehaviour
    {
        float _attackTimer;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Combat)
                return;

            var heroManager = HeroManager.Instance;
            if (heroManager == null) return;

            // Process auto-attacks for all heroes
            for (int i = 0; i < heroManager.HeroCount; i++)
            {
                var hero = heroManager.GetHero(i);
                if (hero == null || !hero.IsAlive) continue;

                ProcessHeroAutoAttack(hero);
            }
        }

        void ProcessHeroAutoAttack(Hero hero)
        {
            // SPD = attacks per second
            float attackInterval = 1f / Mathf.Max(0.1f, hero.CurrentSpd);

            // Use a shared timer approach — in Phase 1 with one hero this is simple.
            // For multi-hero we'd track per-hero timers.
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= attackInterval)
            {
                _attackTimer -= attackInterval;
                PerformAutoAttack(hero);
            }
        }

        void PerformAutoAttack(Hero hero)
        {
            var target = EnemyManager.Instance?.GetNearestEnemy(hero.transform.position);
            if (target == null || target.IsDead) return;

            float damage = DamageCalculator.CalculateAutoAttackDamage(hero.CurrentAtk, target.Def);

            // Apply affinity bonus
            float affinityBonus = AffinityHelper.GetMultiplier(hero.Affinity, target.Affinity);
            damage *= affinityBonus;

            target.TakeDamage(damage);

            EventBus.Publish(new AutoAttackEvent
            {
                HeroSlot = hero.Slot,
                EnemyId = target.Id,
                Damage = damage
            });

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });
        }
    }
}
