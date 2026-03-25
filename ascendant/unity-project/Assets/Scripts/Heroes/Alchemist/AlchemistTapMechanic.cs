using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Heroes
{
    public enum PotionType
    {
        Healing,
        Explosive,
        Buffing,
        Corrosive
    }

    public class AlchemistTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Potion Cycle")]
        [SerializeField] float _potionCycleDuration = 5f;

        [Header("Healing Potion")]
        [SerializeField] float _healPercent = 0.1f;

        [Header("Explosive Potion")]
        [SerializeField] float _explosiveDamageMultiplier = 2f;

        [Header("Buffing Potion")]
        [SerializeField] float _buffAtkPercent = 0.15f;
        [SerializeField] float _buffDuration = 5f;

        [Header("Corrosive Potion")]
        [SerializeField] float _corrosiveDamageMultiplier = 1f;
        [SerializeField] float _defDebuffPercent = 0.2f;
        [SerializeField] float _defDebuffDuration = 8f;

        [Header("Passive - Transmutation")]
        [SerializeField] float _transmutationChance = 0.1f;

        // Runtime state
        float _potionTimer = 0f;
        PotionType _currentPotion = PotionType.Healing;
        bool _isTransmutationActive = false;

        // Corrosive DEF debuffs: enemyId -> remaining time
        readonly Dictionary<int, float> _defDebuffTimers = new Dictionary<int, float>();

        // Buff timer
        float _buffTimer = 0f;

        // ---- Public API ----

        public PotionType CurrentPotion => _currentPotion;
        public float PotionTimer => _potionTimer;
        public float PotionCycleDuration => _potionCycleDuration;
        public bool IsTransmutationActive => _isTransmutationActive;

        // ---- Unity lifecycle ----

        void Update()
        {
            // Auto-rotate potion
            _potionTimer += Time.deltaTime;
            if (_potionTimer >= _potionCycleDuration)
            {
                _potionTimer = 0f;
                AdvancePotion();
            }

            // Tick buff timer
            if (_buffTimer > 0f)
                _buffTimer -= Time.deltaTime;

            // Tick corrosive debuffs
            var toRemove = new List<int>();
            foreach (var kvp in _defDebuffTimers)
            {
                _defDebuffTimers[kvp.Key] -= Time.deltaTime;
                if (_defDebuffTimers[kvp.Key] <= 0f)
                    toRemove.Add(kvp.Key);
            }
            foreach (int id in toRemove)
                _defDebuffTimers.Remove(id);
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            // Roll Transmutation passive
            _isTransmutationActive = Random.value < _transmutationChance;
            float multiplier = _isTransmutationActive ? 2f : 1f;

            int heroSlot = 0; // Alchemist assumed slot 0; expose as SerializeField if needed

            switch (_currentPotion)
            {
                case PotionType.Healing:
                    ThrowHealingPotion(multiplier, heroSlot);
                    break;
                case PotionType.Explosive:
                    ThrowExplosivePotion(damage, multiplier, worldPosition, heroSlot);
                    break;
                case PotionType.Buffing:
                    ThrowBuffingPotion(multiplier, heroSlot);
                    break;
                case PotionType.Corrosive:
                    ThrowCorrosivePotion(damage, multiplier, worldPosition, heroSlot);
                    break;
            }
        }

        public void Reset()
        {
            _potionTimer = 0f;
            _currentPotion = PotionType.Healing;
            _isTransmutationActive = false;
            _defDebuffTimers.Clear();
            _buffTimer = 0f;
        }

        // ---- Potion Handlers ----

        void ThrowHealingPotion(float multiplier, int heroSlot)
        {
            var hero = PartyManager.Instance.GetLowestHpAliveHero();
            if (hero == null || !hero.IsAlive) return;

            float healAmount = hero.MaxHp * _healPercent * multiplier;
            hero.Heal(healAmount);

            EventBus.Publish(new HeroHealedEvent
            {
                HeroSlot = hero.Slot,
                Amount = healAmount,
                CurrentHp = hero.CurrentHp,
                MaxHp = hero.MaxHp
            });

            EventBus.Publish(new PotionThrownEvent
            {
                HeroSlot = heroSlot,
                PotionType = PotionType.Healing.ToString()
            });
        }

        void ThrowExplosivePotion(float damage, float multiplier, Vector3 worldPosition, int heroSlot)
        {
            var enemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (enemies == null) return;

            float finalDamage = damage * _explosiveDamageMultiplier * multiplier;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                enemy.TakeDamage(finalDamage, DamageType.Magical);
                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = finalDamage,
                    IsCritical = false,
                    IsAoE = true,
                    WorldPosition = enemy.transform.position
                });
            }

            EventBus.Publish(new PotionThrownEvent
            {
                HeroSlot = heroSlot,
                PotionType = PotionType.Explosive.ToString()
            });
        }

        void ThrowBuffingPotion(float multiplier, int heroSlot)
        {
            // Publish a resource/buff event to signal party +15% ATK (doubled if transmutation)
            float buffValue = _buffAtkPercent * multiplier;
            float duration = _buffDuration;
            _buffTimer = duration;

            // Publish per hero in party
            var heroes = PartyManager.Instance.GetAllAliveHeroes();
            if (heroes != null)
            {
                foreach (var hero in heroes)
                {
                    if (!hero.IsAlive) continue;
                    EventBus.Publish(new ResourceChangedEvent
                    {
                        HeroSlot = hero.Slot,
                        ResourceName = "AtkBuff",
                        Current = buffValue,
                        Max = 1f
                    });
                }
            }

            EventBus.Publish(new PotionThrownEvent
            {
                HeroSlot = heroSlot,
                PotionType = PotionType.Buffing.ToString()
            });
        }

        void ThrowCorrosivePotion(float damage, float multiplier, Vector3 worldPosition, int heroSlot)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float finalDamage = damage * _corrosiveDamageMultiplier * multiplier;
            enemy.TakeDamage(finalDamage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Apply DEF debuff (tracked locally)
            float debuffDuration = _defDebuffDuration * multiplier;
            _defDebuffTimers[enemy.Id] = debuffDuration;

            EventBus.Publish(new PotionThrownEvent
            {
                HeroSlot = heroSlot,
                PotionType = PotionType.Corrosive.ToString()
            });
        }

        // ---- Helpers ----

        void AdvancePotion()
        {
            int next = ((int)_currentPotion + 1) % 4;
            _currentPotion = (PotionType)next;
        }

        /// <summary>Returns true if the given enemy currently has a corrosive DEF debuff active.</summary>
        public bool HasDefDebuff(int enemyId) => _defDebuffTimers.ContainsKey(enemyId);

        /// <summary>Returns true if the party ATK buff is currently active.</summary>
        public bool IsBuffActive => _buffTimer > 0f;
    }
}
