using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class ChronomancerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Temporal Bolt Config")]
        [SerializeField] float _slowPerStack = 0.05f;
        [SerializeField] int _maxSlowStacks = 10;
        [SerializeField] float _freezeDuration = 3f;
        [SerializeField] float _slowImmunityDuration = 5f;

        [Header("Passive - Time Dilation")]
        [SerializeField] float _cooldownReductionPercent = 0.1f;

        // Slow stacks per enemy ID
        readonly Dictionary<int, int> _slowStacks = new Dictionary<int, int>();

        // Freeze timers per enemy ID (remaining freeze time)
        readonly Dictionary<int, float> _freezeTimers = new Dictionary<int, float>();

        // Slow immunity timers per enemy ID (remaining immunity time after freeze ends)
        readonly Dictionary<int, float> _immunityTimers = new Dictionary<int, float>();

        // ---- Public API ----

        public float CooldownReductionPercent => _cooldownReductionPercent;

        public int GetSlowStacks(int enemyId)
        {
            return _slowStacks.TryGetValue(enemyId, out int stacks) ? stacks : 0;
        }

        public bool IsEnemyFrozen(int enemyId)
        {
            return _freezeTimers.TryGetValue(enemyId, out float t) && t > 0f;
        }

        bool IsEnemySlowImmune(int enemyId)
        {
            return _immunityTimers.TryGetValue(enemyId, out float t) && t > 0f;
        }

        // ---- Unity lifecycle ----

        void Update()
        {
            float dt = Time.deltaTime;

            // Tick freeze timers
            var expiredFreezes = new List<int>();
            foreach (var kvp in _freezeTimers)
            {
                _freezeTimers[kvp.Key] -= dt;
                if (_freezeTimers[kvp.Key] <= 0f)
                    expiredFreezes.Add(kvp.Key);
            }
            foreach (int id in expiredFreezes)
            {
                _freezeTimers.Remove(id);
                // Begin slow immunity after freeze ends
                _immunityTimers[id] = _slowImmunityDuration;
            }

            // Tick immunity timers
            var expiredImmunities = new List<int>();
            foreach (var kvp in _immunityTimers)
            {
                _immunityTimers[kvp.Key] -= dt;
                if (_immunityTimers[kvp.Key] <= 0f)
                    expiredImmunities.Add(kvp.Key);
            }
            foreach (int id in expiredImmunities)
                _immunityTimers.Remove(id);
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            // Deal base damage
            enemy.TakeDamage(damage, DamageType.Magical);
            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Apply slow (if not frozen and not immune)
            if (!IsEnemyFrozen(enemy.Id) && !IsEnemySlowImmune(enemy.Id))
                ApplySlow(enemy.Id, enemy.transform.position);
        }

        public void Reset()
        {
            _slowStacks.Clear();
            _freezeTimers.Clear();
            _immunityTimers.Clear();
        }

        // ---- Helpers ----

        void ApplySlow(int enemyId, Vector3 enemyPos)
        {
            if (!_slowStacks.ContainsKey(enemyId))
                _slowStacks[enemyId] = 0;

            _slowStacks[enemyId]++;

            if (_slowStacks[enemyId] >= _maxSlowStacks)
            {
                // Trigger Frozen in Time
                _slowStacks[enemyId] = 0;
                _freezeTimers[enemyId] = _freezeDuration;

                EventBus.Publish(new TimeFreezeTriggerEvent
                {
                    EnemyId = enemyId,
                    Duration = _freezeDuration
                });
            }
        }
    }
}
