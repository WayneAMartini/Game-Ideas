using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Combat
{
    public class AutoAttackSystem : MonoBehaviour
    {
        float[] _attackTimers = new float[4];

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Combat)
                return;

            var partyManager = Party.PartyManager.Instance;
            if (partyManager == null)
            {
                // Fallback to HeroManager for backward compatibility
                FallbackUpdate();
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                var hero = partyManager.GetHero(i);
                if (hero == null || !hero.IsAlive) continue;

                ProcessHeroAutoAttack(hero, i);
            }
        }

        void FallbackUpdate()
        {
            var heroManager = HeroManager.Instance;
            if (heroManager == null) return;

            for (int i = 0; i < heroManager.HeroCount; i++)
            {
                var hero = heroManager.GetHero(i);
                if (hero == null || !hero.IsAlive) continue;

                ProcessHeroAutoAttack(hero, i);
            }
        }

        void ProcessHeroAutoAttack(Hero hero, int timerIndex)
        {
            if (timerIndex < 0 || timerIndex >= _attackTimers.Length) return;

            float attackInterval = 1f / Mathf.Max(0.1f, hero.CurrentSpd);
            _attackTimers[timerIndex] += Time.deltaTime;

            if (_attackTimers[timerIndex] >= attackInterval)
            {
                _attackTimers[timerIndex] -= attackInterval;
                PerformAutoAttack(hero);
            }
        }

        void PerformAutoAttack(Hero hero)
        {
            var target = EnemyManager.Instance?.GetNearestEnemy(hero.transform.position);
            if (target == null || target.IsDead) return;

            // Determine damage type from hero class
            DamageType damageType = GetHeroDamageType(hero);

            // Get position bonus from PartyManager
            float positionBonus = 0f;
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null)
                positionBonus = partyManager.GetDamageBonus(hero.Slot);

            float damage = DamageCalculator.CalculateAutoAttackFull(
                hero.CurrentAtk, damageType, hero.Affinity, target, positionBonus);

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

        static DamageType GetHeroDamageType(Hero hero)
        {
            if (hero.Data == null) return DamageType.Physical;

            return hero.Data.role switch
            {
                HeroRole.Caster => DamageType.Magical,
                HeroRole.Support => DamageType.Magical,
                _ => DamageType.Physical
            };
        }
    }
}
