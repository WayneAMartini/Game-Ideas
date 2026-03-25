using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Druid Tier 2 tap mechanic.
    /// Nature's Touch:
    ///   Odd taps  -> Rejuvenation HoT on lowest-HP ally (3% max HP over 6s, stacks up to 3).
    ///   Even taps -> Thorny Vines on target enemy (50% tap damage over 4s, stacks up to 3).
    /// Passive - Wild Growth: all healing increases by 5% every 10s of combat, max +50%.
    /// </summary>
    public class DruidTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Rejuvenation Config")]
        [SerializeField] float _rejuvHealPercent = 0.03f;
        [SerializeField] float _rejuvDuration = 6f;
        [SerializeField] int _rejuvMaxStacks = 3;

        [Header("Thorny Vines Config")]
        [SerializeField] float _vinesDamagePercent = 0.50f;
        [SerializeField] float _vinesDuration = 4f;
        [SerializeField] int _vinesMaxStacks = 3;

        [Header("Wild Growth Config")]
        [SerializeField] float _wildGrowthBonusPer10s = 0.05f;
        [SerializeField] float _wildGrowthMaxBonus = 0.50f;
        [SerializeField] float _wildGrowthInterval = 10f;

        // Tap parity tracking (1-indexed so tap 1 is odd)
        int _localTapCount = 0;

        // HoT tracking: hero slot -> list of active HoT end times and per-tick heal amount
        class HotInstance { public float EndTime; public float TotalHeal; public float TickInterval; public float NextTickTime; }
        readonly Dictionary<int, List<HotInstance>> _activeHoTs = new Dictionary<int, List<HotInstance>>();

        // Vine tracking: enemy ID -> list of active vine instances
        class VineInstance { public float EndTime; public float TotalDamage; public float TickInterval; public float NextTickTime; int _enemyId; public int EnemyId => _enemyId; public VineInstance(int id) { _enemyId = id; } }
        readonly Dictionary<int, List<VineInstance>> _activeVines = new Dictionary<int, List<VineInstance>>();

        // Wild Growth
        float _combatTimer = 0f;
        float _nextWildGrowthTick = 0f;
        float _wildGrowthBonus = 0f;

        // Public properties for UI
        public float WildGrowthBonus => _wildGrowthBonus;
        public bool IsOddTap => (_localTapCount % 2) != 0;

        public int ActiveHoTs
        {
            get
            {
                int total = 0;
                foreach (var list in _activeHoTs.Values)
                    total += list.Count;
                return total;
            }
        }

        void Update()
        {
            float now = Time.time;

            // Wild Growth accumulates over combat time
            _combatTimer += Time.deltaTime;
            if (_combatTimer >= _nextWildGrowthTick + _wildGrowthInterval)
            {
                _nextWildGrowthTick += _wildGrowthInterval;
                _wildGrowthBonus = Mathf.Min(_wildGrowthBonus + _wildGrowthBonusPer10s, _wildGrowthMaxBonus);
            }

            // Tick active HoTs
            TickHoTs(now);

            // Tick active Vines
            TickVines(now);
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            _localTapCount++;

            if (_localTapCount % 2 != 0)
            {
                // Odd tap: Rejuvenation on lowest-HP ally
                ApplyRejuvenation();
            }
            else
            {
                // Even tap: Thorny Vines on nearest enemy
                var target = EnemyManager.Instance.GetNearestEnemy(worldPosition);
                if (target != null && !target.IsDead)
                    ApplyThornyVines(target, damage);
            }
        }

        public void Reset()
        {
            _localTapCount = 0;
            _activeHoTs.Clear();
            _activeVines.Clear();
            _combatTimer = 0f;
            _nextWildGrowthTick = 0f;
            _wildGrowthBonus = 0f;
        }

        // ------------------------------------------------------------------ Rejuvenation

        void ApplyRejuvenation()
        {
            var hero = PartyManager.Instance.GetLowestHpAliveHero();
            if (hero == null) return;

            if (!_activeHoTs.ContainsKey(hero.Slot))
                _activeHoTs[hero.Slot] = new List<HotInstance>();

            var list = _activeHoTs[hero.Slot];

            // Cap stacks
            if (list.Count >= _rejuvMaxStacks)
                list.RemoveAt(0); // remove oldest stack

            float totalHeal = hero.MaxHp * _rejuvHealPercent * (1f + _wildGrowthBonus);
            float tickInterval = _rejuvDuration / 6f; // 6 ticks over the duration

            list.Add(new HotInstance
            {
                EndTime = Time.time + _rejuvDuration,
                TotalHeal = totalHeal,
                TickInterval = tickInterval,
                NextTickTime = Time.time + tickInterval
            });
        }

        void TickHoTs(float now)
        {
            var expiredSlots = new List<int>();

            foreach (var kvp in _activeHoTs)
            {
                int slot = kvp.Key;
                var list = kvp.Value;

                // Find the hero by slot
                Hero hero = FindHeroBySlot(slot);

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var hot = list[i];

                    // Expire check
                    if (now >= hot.EndTime)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    // Tick check
                    if (now >= hot.NextTickTime && hero != null && !hero.IsDead)
                    {
                        float tickHeal = hot.TotalHeal * (hot.TickInterval / _rejuvDuration);
                        hero.Heal(tickHeal);
                        hot.NextTickTime += hot.TickInterval;

                        EventBus.Publish(new HeroHealedEvent
                        {
                            HeroSlot = hero.Slot,
                            Amount = tickHeal,
                            CurrentHp = hero.CurrentHp,
                            MaxHp = hero.MaxHp
                        });
                    }
                }

                if (list.Count == 0)
                    expiredSlots.Add(slot);
            }

            foreach (int slot in expiredSlots)
                _activeHoTs.Remove(slot);
        }

        // ------------------------------------------------------------------ Thorny Vines

        void ApplyThornyVines(Enemy target, float tapDamage)
        {
            if (!_activeVines.ContainsKey(target.Id))
                _activeVines[target.Id] = new List<VineInstance>();

            var list = _activeVines[target.Id];

            // Cap stacks
            if (list.Count >= _vinesMaxStacks)
                list.RemoveAt(0);

            float totalDamage = tapDamage * _vinesDamagePercent;
            float tickInterval = _vinesDuration / 4f; // 4 ticks

            var vine = new VineInstance(target.Id)
            {
                EndTime = Time.time + _vinesDuration,
                TotalDamage = totalDamage,
                TickInterval = tickInterval,
                NextTickTime = Time.time + tickInterval
            };
            list.Add(vine);
        }

        void TickVines(float now)
        {
            var expiredIds = new List<int>();

            foreach (var kvp in _activeVines)
            {
                int enemyId = kvp.Key;
                var list = kvp.Value;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var vine = list[i];

                    if (now >= vine.EndTime)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    if (now >= vine.NextTickTime)
                    {
                        // Find the enemy in the scene
                        Enemy target = FindEnemyById(enemyId);
                        if (target != null && !target.IsDead)
                        {
                            float tickDamage = vine.TotalDamage * (vine.TickInterval / _vinesDuration);
                            target.TakeDamage(tickDamage, DamageType.Magical);

                            EventBus.Publish(new EnemyDamagedEvent
                            {
                                EnemyId = enemyId,
                                Damage = tickDamage,
                                IsCritical = false,
                                IsAoE = false,
                                WorldPosition = target.transform.position
                            });
                        }

                        vine.NextTickTime += vine.TickInterval;
                    }
                }

                if (list.Count == 0)
                    expiredIds.Add(enemyId);
            }

            foreach (int id in expiredIds)
                _activeVines.Remove(id);
        }

        // ------------------------------------------------------------------ helpers

        Hero FindHeroBySlot(int slot)
        {
            var heroes = PartyManager.Instance.GetAllAliveHeroes();
            if (heroes == null) return null;
            foreach (var h in heroes)
                if (h.Slot == slot) return h;
            return null;
        }

        Enemy FindEnemyById(int id)
        {
            var enemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (enemies == null) return null;
            foreach (var e in enemies)
                if (e.Id == id) return e;
            return null;
        }
    }
}
