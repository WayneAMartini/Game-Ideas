using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Combat
{
    public class AbilitySlot
    {
        readonly Ability _ability;
        float _cooldownRemaining;
        float _ultimateCharge;
        bool _isReady;

        public Ability Data => _ability;
        public float CooldownRemaining => _cooldownRemaining;
        public float CooldownPercent => _ability.cooldown > 0 ? _cooldownRemaining / _ability.cooldown : 0f;
        public float UltimateChargePercent => _ability.isUltimate ? _ultimateCharge / _ability.chargeRequired : 0f;
        public bool IsReady => _ability.isUltimate ? _ultimateCharge >= _ability.chargeRequired : _cooldownRemaining <= 0f;

        public AbilitySlot(Ability ability)
        {
            _ability = ability;
            _cooldownRemaining = 0f;
            _ultimateCharge = 0f;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining > 0f)
                _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - deltaTime);
        }

        public void AddUltimateCharge(float amount)
        {
            if (!_ability.isUltimate) return;
            _ultimateCharge = Mathf.Min(_ability.chargeRequired, _ultimateCharge + amount);
        }

        public bool TryActivate(Heroes.Hero hero)
        {
            if (!IsReady) return false;

            if (_ability.isUltimate)
                _ultimateCharge = 0f;
            else
                _cooldownRemaining = _ability.cooldown;

            ExecuteAbility(hero);
            return true;
        }

        void ExecuteAbility(Heroes.Hero hero)
        {
            float baseDamage = hero.CurrentAtk * _ability.damageMultiplier;

            switch (_ability.targetType)
            {
                case AbilityTargetType.SingleEnemy:
                {
                    var target = EnemyManager.Instance?.GetNearestEnemy(hero.transform.position);
                    if (target != null)
                    {
                        float damage = DamageCalculator.CalculateAbilityDamage(
                            hero.CurrentAtk, _ability.damageMultiplier, target.Def);
                        target.TakeDamage(damage);

                        EventBus.Publish(new EnemyDamagedEvent
                        {
                            EnemyId = target.Id,
                            Damage = damage,
                            IsCritical = false,
                            IsAoE = false,
                            WorldPosition = target.transform.position
                        });
                    }
                    break;
                }

                case AbilityTargetType.AllEnemies:
                case AbilityTargetType.FrontlineEnemies:
                {
                    var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
                    if (enemies != null)
                    {
                        foreach (var enemy in enemies)
                        {
                            float damage = DamageCalculator.CalculateAbilityDamage(
                                hero.CurrentAtk, _ability.damageMultiplier, enemy.Def);
                            enemy.TakeDamage(damage);

                            EventBus.Publish(new EnemyDamagedEvent
                            {
                                EnemyId = enemy.Id,
                                Damage = damage,
                                IsCritical = false,
                                IsAoE = true,
                                WorldPosition = enemy.transform.position
                            });
                        }
                    }
                    break;
                }

                case AbilityTargetType.PartyBuff:
                    // Phase 1: buff is tracked via events; actual stat modification
                    // will be expanded in Phase 2 with the full party system
                    break;

                case AbilityTargetType.Self:
                    break;
            }

            EventBus.Publish(new AbilityUsedEvent
            {
                HeroSlot = hero.Slot,
                AbilitySlot = _ability.slotIndex,
                AbilityName = _ability.abilityName
            });
        }
    }
}
