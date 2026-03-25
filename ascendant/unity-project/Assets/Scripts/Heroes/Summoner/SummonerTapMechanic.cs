using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Heroes
{
    public enum FamiliarType
    {
        EmberSprite,
        FrostGolem,
        StormHawk,
        StoneSentinel
    }

    public class SummonerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Familiar Config")]
        [SerializeField] FamiliarType _activeFamiliar = FamiliarType.EmberSprite;
        [SerializeField] float _swapCooldownDuration = 2f;

        [Header("EmberSprite")]
        [SerializeField] float _emberAoEPercent = 0.5f;

        [Header("FrostGolem")]
        [SerializeField] float _golemMaxHp = 500f;

        [Header("StormHawk")]
        [SerializeField] float _hawkCritChance = 0.3f;
        [SerializeField] float _hawkCritMultiplier = 2f;

        [Header("StoneSentinel")]
        [SerializeField] float _sentinelHealPercent = 0.05f;

        [Header("Passive - Elemental Attunement")]
        [SerializeField] float _attunementBonus = 0.15f;

        // Runtime state
        float _swapCooldownTimer = 0f;
        float _golemCurrentHp;

        // ---- Public API ----

        public FamiliarType ActiveFamiliar => _activeFamiliar;

        public float SwapCooldown => _swapCooldownTimer;

        public bool CanSwap => _swapCooldownTimer <= 0f;

        // ---- Unity lifecycle ----

        void Awake()
        {
            _golemCurrentHp = _golemMaxHp;
        }

        void Update()
        {
            if (_swapCooldownTimer > 0f)
                _swapCooldownTimer -= Time.deltaTime;
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            // Apply Elemental Attunement passive bonus
            float attunedDamage = damage * (1f + _attunementBonus);

            switch (_activeFamiliar)
            {
                case FamiliarType.EmberSprite:
                    HandleEmberSprite(attunedDamage, worldPosition);
                    break;
                case FamiliarType.FrostGolem:
                    HandleFrostGolem(attunedDamage, worldPosition);
                    break;
                case FamiliarType.StormHawk:
                    HandleStormHawk(attunedDamage, worldPosition);
                    break;
                case FamiliarType.StoneSentinel:
                    HandleStoneSentinel(attunedDamage, worldPosition);
                    break;
            }
        }

        public void Reset()
        {
            _swapCooldownTimer = 0f;
            _golemCurrentHp = _golemMaxHp;
            _activeFamiliar = FamiliarType.EmberSprite;
        }

        // ---- Familiar Actions ----

        void HandleEmberSprite(float damage, Vector3 worldPosition)
        {
            var enemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (enemies == null || enemies.Count == 0) return;

            // Primary target takes full damage; AoE targets take 50%
            var primary = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                bool isPrimary = primary != null && enemy.Id == primary.Id;
                float dealt = isPrimary ? damage : damage * _emberAoEPercent;
                enemy.TakeDamage(dealt, DamageType.Magical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = dealt,
                    IsCritical = false,
                    IsAoE = !isPrimary,
                    WorldPosition = enemy.transform.position
                });
            }
        }

        void HandleFrostGolem(float damage, Vector3 worldPosition)
        {
            // Golem taunts by taking hits - represent as healing its HP pool
            _golemCurrentHp = Mathf.Min(_golemCurrentHp + damage * 0.5f, _golemMaxHp);

            // Still deal damage to nearest enemy
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage, DamageType.Physical);
                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = damage,
                    IsCritical = false,
                    IsAoE = false,
                    WorldPosition = enemy.transform.position
                });
            }
        }

        void HandleStormHawk(float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            bool isCrit = Random.value < _hawkCritChance;
            float finalDamage = isCrit ? damage * _hawkCritMultiplier : damage;

            enemy.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = isCrit,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });
        }

        void HandleStoneSentinel(float damage, Vector3 worldPosition)
        {
            // Deal damage
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage, DamageType.Physical);
                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = damage,
                    IsCritical = false,
                    IsAoE = false,
                    WorldPosition = enemy.transform.position
                });
            }

            // Shield: heal lowest ally for 5% max HP
            var hero = PartyManager.Instance.GetLowestHpAliveHero();
            if (hero != null && hero.IsAlive)
            {
                float healAmount = hero.MaxHp * _sentinelHealPercent;
                hero.Heal(healAmount);
                EventBus.Publish(new HeroHealedEvent
                {
                    HeroSlot = hero.Slot,
                    Amount = healAmount,
                    CurrentHp = hero.CurrentHp,
                    MaxHp = hero.MaxHp
                });
            }
        }

        // ---- Swap API ----

        /// <summary>
        /// Swaps to a new familiar. Triggers Transition Burst (farewell AoE from departing familiar).
        /// Enforces 2s swap cooldown.
        /// </summary>
        public void SwapFamiliar(FamiliarType newFamiliar)
        {
            if (!CanSwap || newFamiliar == _activeFamiliar) return;

            FamiliarType departing = _activeFamiliar;
            TriggerTransitionBurst(departing);

            _activeFamiliar = newFamiliar;
            _swapCooldownTimer = _swapCooldownDuration;

            EventBus.Publish(new FamiliarSwappedEvent
            {
                HeroSlot = 0,
                FamiliarName = newFamiliar.ToString()
            });
        }

        void TriggerTransitionBurst(FamiliarType departing)
        {
            // Farewell AoE: deal reduced damage to all enemies from the departing familiar
            var enemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                // Farewell burst is a flat small magical AoE
                float burstDamage = 10f;
                enemy.TakeDamage(burstDamage, DamageType.Magical);
                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = burstDamage,
                    IsCritical = false,
                    IsAoE = true,
                    WorldPosition = enemy.transform.position
                });
            }
        }
    }
}
