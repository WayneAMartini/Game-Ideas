using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Shaman Tier 2 tap mechanic.
    /// Lightning Bolt: chain lightning bounces between enemies.
    ///   Base bounces = 1. Each active totem adds 1 additional bounce.
    ///   Each bounce deals 70% of the previous bounce's damage.
    /// Passive - Elemental Harmony: each active totem grants the party +5% to a random stat.
    /// Totem management: up to 3 totems, each lasts 15s. Expired totems publish TotemExpiredEvent.
    /// </summary>
    public class ShamanTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Lightning Bolt Config")]
        [SerializeField] float _chainDamageDecay = 0.70f;

        [Header("Totem Config")]
        [SerializeField] float _totemDuration = 15f;
        [SerializeField] int _maxTotems = 3;

        [Header("Elemental Harmony Config")]
        [SerializeField] float _harmonyBonusPerTotem = 0.05f;
        [SerializeField] int _heroSlot;

        // Totem tracking
        class TotemInstance
        {
            public string Type;
            public Vector3 Position;
            public float ExpireTime;
            public string UniqueId;
        }

        readonly List<TotemInstance> _activeTotems = new List<TotemInstance>();
        int _totemCounter = 0;

        // Public properties for UI
        public int ActiveTotemCount => _activeTotems.Count;

        /// <summary>
        /// Total Elemental Harmony stat bonus for the party (applied externally by stat system).
        /// </summary>
        public float ElementalHarmonyBonus => _activeTotems.Count * _harmonyBonusPerTotem;

        void Update()
        {
            ExpireOldTotems();
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            ExpireOldTotems();

            var enemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (enemies == null || enemies.Count == 0) return;

            // Find nearest enemy as chain starting point
            var firstTarget = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (firstTarget == null || firstTarget.IsDead) return;

            int totalBounces = 1 + _activeTotems.Count; // base 1 + totem count
            ChainLightning(firstTarget, damage, totalBounces, new HashSet<int>(), enemies);
        }

        public void Reset()
        {
            // Expire all totems cleanly before clearing
            float now = Time.time;
            foreach (var totem in _activeTotems)
            {
                EventBus.Publish(new TotemExpiredEvent
                {
                    HeroSlot = _heroSlot,
                    TotemType = totem.Type
                });
            }
            _activeTotems.Clear();
        }

        /// <summary>
        /// Places a new totem of the given type at the specified world position.
        /// Replaces the oldest totem if the max cap is already reached.
        /// </summary>
        public void PlaceTotem(string type, Vector3 position)
        {
            ExpireOldTotems();

            // Remove oldest totem if at cap
            if (_activeTotems.Count >= _maxTotems)
            {
                var oldest = _activeTotems[0];
                _activeTotems.RemoveAt(0);
                EventBus.Publish(new TotemExpiredEvent
                {
                    HeroSlot = _heroSlot,
                    TotemType = oldest.Type
                });
            }

            _totemCounter++;
            var newTotem = new TotemInstance
            {
                Type = type,
                Position = position,
                ExpireTime = Time.time + _totemDuration,
                UniqueId = $"{type}_{_totemCounter}"
            };
            _activeTotems.Add(newTotem);

            EventBus.Publish(new TotemPlacedEvent
            {
                HeroSlot = _heroSlot,
                TotemType = type,
                Position = position
            });
        }

        // ------------------------------------------------------------------ Chain Lightning

        void ChainLightning(Enemy target, float damage, int bouncesRemaining, HashSet<int> hitIds, IList<Enemy> allEnemies)
        {
            if (target == null || target.IsDead || bouncesRemaining <= 0) return;
            if (hitIds.Contains(target.Id)) return;

            hitIds.Add(target.Id);
            target.TakeDamage(damage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });

            if (bouncesRemaining <= 1) return;

            // Find nearest unhit alive enemy for next bounce
            Enemy nextTarget = FindNearestUnhit(target.transform.position, allEnemies, hitIds);
            if (nextTarget == null) return;

            ChainLightning(nextTarget, damage * _chainDamageDecay, bouncesRemaining - 1, hitIds, allEnemies);
        }

        Enemy FindNearestUnhit(Vector3 origin, IList<Enemy> enemies, HashSet<int> hitIds)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;

            foreach (var e in enemies)
            {
                if (e.IsDead) continue;
                if (hitIds.Contains(e.Id)) continue;

                float dist = Vector3.Distance(origin, e.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = e;
                }
            }

            return best;
        }

        // ------------------------------------------------------------------ Totem expiry

        void ExpireOldTotems()
        {
            float now = Time.time;

            for (int i = _activeTotems.Count - 1; i >= 0; i--)
            {
                if (now >= _activeTotems[i].ExpireTime)
                {
                    var expired = _activeTotems[i];
                    _activeTotems.RemoveAt(i);

                    EventBus.Publish(new TotemExpiredEvent
                    {
                        HeroSlot = _heroSlot,
                        TotemType = expired.Type
                    });
                }
            }
        }
    }
}
