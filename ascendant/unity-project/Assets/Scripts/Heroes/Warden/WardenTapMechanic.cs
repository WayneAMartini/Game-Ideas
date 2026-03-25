using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Heroes
{
    public class WardenTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Earthen Slam Config")]
        [SerializeField] float _rootDuration = 1f;
        [SerializeField] float _rootedDamageBonus = 0.2f;

        [Header("Living Armor Passive")]
        [SerializeField] int _heroSlot = 0;
        [SerializeField] float _regenPercentPerSecond = 0.03f;
        [SerializeField] float _terrainRegenMultiplier = 2f;

        [Header("Terrain")]
        [SerializeField] bool _inTerrain = false;

        // Root timers per enemy ID (remaining root duration in seconds)
        readonly Dictionary<int, float> _rootTimers = new Dictionary<int, float>();

        // ---- Public API ----

        public float RegenRate => _regenPercentPerSecond * (_inTerrain ? _terrainRegenMultiplier : 1f);
        public bool IsInTerrain
        {
            get => _inTerrain;
            set => _inTerrain = value;
        }

        public bool IsEnemyRooted(int enemyId)
        {
            return _rootTimers.TryGetValue(enemyId, out float t) && t > 0f;
        }

        // ---- Unity lifecycle ----

        void Update()
        {
            float dt = Time.deltaTime;

            // Tick root timers
            var expired = new List<int>();
            foreach (var kvp in _rootTimers)
            {
                _rootTimers[kvp.Key] -= dt;
                if (_rootTimers[kvp.Key] <= 0f)
                    expired.Add(kvp.Key);
            }
            foreach (int id in expired)
                _rootTimers.Remove(id);

            // Living Armor: regen HP over time
            ApplyPassiveRegen(dt);
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            // Rooted enemies take 20% more damage
            float finalDamage = damage;
            if (IsEnemyRooted(enemy.Id))
                finalDamage *= (1f + _rootedDamageBonus);

            enemy.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Root the target
            ApplyRoot(enemy.Id);
        }

        public void Reset()
        {
            _rootTimers.Clear();
            _inTerrain = false;
        }

        // ---- Helpers ----

        void ApplyRoot(int enemyId)
        {
            _rootTimers[enemyId] = _rootDuration;

            EventBus.Publish(new RootAppliedEvent
            {
                EnemyId = enemyId,
                Duration = _rootDuration
            });
        }

        void ApplyPassiveRegen(float dt)
        {
            var hero = PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null || !hero.IsAlive) return;

            float regenPercent = _regenPercentPerSecond * (_inTerrain ? _terrainRegenMultiplier : 1f);
            float healAmount = hero.MaxHp * regenPercent * dt;

            if (healAmount <= 0f || hero.CurrentHp >= hero.MaxHp) return;

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
}
