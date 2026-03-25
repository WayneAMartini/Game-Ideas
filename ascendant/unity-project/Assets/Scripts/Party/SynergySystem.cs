using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Party
{
    public enum ResonanceType
    {
        None,
        Inferno,        // Flame 4-of-a-kind
        AbsoluteCold,   // Frost 4-of-a-kind
        Tempest,        // Storm 4-of-a-kind
        Overgrowth,     // Nature 4-of-a-kind
        VoidShroud,     // Shadow 4-of-a-kind
        DivineChorus    // Radiance 4-of-a-kind
    }

    [System.Serializable]
    public struct AffinitySynergyBonus
    {
        public Affinity Affinity;
        public int Count;
        public float DamageBonus;
        public float DefenseBonus;
        public ResonanceType Resonance;
    }

    [System.Serializable]
    public struct RoleSynergyBonus
    {
        public string Name;
        public string Description;
        public float Bonus;
    }

    public class SynergySystem : MonoBehaviour
    {
        public static SynergySystem Instance { get; private set; }

        // Active synergy state
        readonly List<AffinitySynergyBonus> _activeAffinitySynergies = new();
        readonly List<RoleSynergyBonus> _activeRoleSynergies = new();
        ResonanceType _activeResonance = ResonanceType.None;

        // Cached bonus totals
        float _totalDamageBonus;
        float _totalDefenseBonus;
        float _totalHealingBonus;
        float _allStatsBonus;

        public IReadOnlyList<AffinitySynergyBonus> ActiveAffinitySynergies => _activeAffinitySynergies;
        public IReadOnlyList<RoleSynergyBonus> ActiveRoleSynergies => _activeRoleSynergies;
        public ResonanceType ActiveResonance => _activeResonance;
        public float TotalDamageBonus => _totalDamageBonus;
        public float TotalDefenseBonus => _totalDefenseBonus;
        public float TotalHealingBonus => _totalHealingBonus;
        public float AllStatsBonus => _allStatsBonus;

        // Resonance tick timers
        float _resonanceTickTimer;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void Update()
        {
            if (_activeResonance == ResonanceType.None) return;

            _resonanceTickTimer += Time.deltaTime;
            if (_resonanceTickTimer >= 1f)
            {
                _resonanceTickTimer -= 1f;
                ApplyResonanceTick();
            }
        }

        void OnPartyChanged(PartyChangedEvent evt)
        {
            RecalculateSynergies();
        }

        public void RecalculateSynergies()
        {
            _activeAffinitySynergies.Clear();
            _activeRoleSynergies.Clear();
            _activeResonance = ResonanceType.None;
            _totalDamageBonus = 0f;
            _totalDefenseBonus = 0f;
            _totalHealingBonus = 0f;
            _allStatsBonus = 0f;

            var pm = PartyManager.Instance;
            if (pm == null) return;

            var heroes = pm.GetAllHeroes();
            if (heroes == null) return;

            CalculateAffinitySynergies(heroes);
            CalculateRoleSynergies(heroes);

            // Publish all activated synergies
            foreach (var syn in _activeAffinitySynergies)
            {
                EventBus.Publish(new SynergyActivatedEvent
                {
                    SynergyName = $"{syn.Affinity} x{syn.Count}",
                    Description = $"+{syn.DamageBonus * 100f:0}% {syn.Affinity} damage"
                });
            }

            foreach (var syn in _activeRoleSynergies)
            {
                EventBus.Publish(new SynergyActivatedEvent
                {
                    SynergyName = syn.Name,
                    Description = syn.Description
                });
            }
        }

        void CalculateAffinitySynergies(Hero[] heroes)
        {
            // Count affinities
            var counts = new Dictionary<Affinity, int>();
            foreach (var hero in heroes)
            {
                if (hero == null) continue;
                var aff = hero.Affinity;
                if (aff == Affinity.None) continue;

                if (!counts.ContainsKey(aff))
                    counts[aff] = 0;
                counts[aff]++;
            }

            foreach (var kvp in counts)
            {
                if (kvp.Value < 2) continue;

                var bonus = new AffinitySynergyBonus
                {
                    Affinity = kvp.Key,
                    Count = kvp.Value
                };

                switch (kvp.Value)
                {
                    case 2:
                        bonus.DamageBonus = 0.10f;  // +10% affinity damage
                        bonus.DefenseBonus = 0f;
                        break;
                    case 3:
                        bonus.DamageBonus = 0.10f;  // +10% damage
                        bonus.DefenseBonus = 0.15f;  // +15% defense on matching biome
                        break;
                    case >= 4:
                        bonus.DamageBonus = 0.25f;  // +25% damage
                        bonus.DefenseBonus = 0.20f;  // +20% defense
                        bonus.Resonance = GetResonanceForAffinity(kvp.Key);
                        _activeResonance = bonus.Resonance;
                        break;
                }

                _activeAffinitySynergies.Add(bonus);
                _totalDamageBonus += bonus.DamageBonus;
                _totalDefenseBonus += bonus.DefenseBonus;
            }
        }

        void CalculateRoleSynergies(Hero[] heroes)
        {
            // Count roles
            var roles = new HashSet<HeroRole>();
            var roleCounts = new Dictionary<HeroRole, int>();

            foreach (var hero in heroes)
            {
                if (hero == null || hero.Data == null) continue;
                var role = hero.Data.role;
                roles.Add(role);

                if (!roleCounts.ContainsKey(role))
                    roleCounts[role] = 0;
                roleCounts[role]++;
            }

            // Vanguard + Support = +10% survivability (damage reduction)
            if (roles.Contains(HeroRole.Vanguard) && roles.Contains(HeroRole.Support))
            {
                _activeRoleSynergies.Add(new RoleSynergyBonus
                {
                    Name = "Fortified Front",
                    Description = "+10% party damage reduction",
                    Bonus = 0.10f
                });
                _totalDefenseBonus += 0.10f;
            }

            // Striker/Caster + Ranger = +10% damage
            if ((roles.Contains(HeroRole.Striker) || roles.Contains(HeroRole.Caster))
                && roles.Contains(HeroRole.Ranger))
            {
                _activeRoleSynergies.Add(new RoleSynergyBonus
                {
                    Name = "Blitz Protocol",
                    Description = "+10% party damage",
                    Bonus = 0.10f
                });
                _totalDamageBonus += 0.10f;
            }

            // 2 different Supports = +20% healing
            if (roleCounts.TryGetValue(HeroRole.Support, out int supportCount) && supportCount >= 2)
            {
                _activeRoleSynergies.Add(new RoleSynergyBonus
                {
                    Name = "Dual Support",
                    Description = "+20% all healing done",
                    Bonus = 0.20f
                });
                _totalHealingBonus += 0.20f;
            }

            // 2 different Vanguards = +15% defense
            if (roleCounts.TryGetValue(HeroRole.Vanguard, out int vanguardCount) && vanguardCount >= 2)
            {
                _activeRoleSynergies.Add(new RoleSynergyBonus
                {
                    Name = "Double Wall",
                    Description = "+15% party defense",
                    Bonus = 0.15f
                });
                _totalDefenseBonus += 0.15f;
            }

            // All 4 roles different = +5% all stats
            if (roles.Count >= 4)
            {
                _activeRoleSynergies.Add(new RoleSynergyBonus
                {
                    Name = "Balanced Team",
                    Description = "+5% all stats",
                    Bonus = 0.05f
                });
                _allStatsBonus += 0.05f;
            }
        }

        static ResonanceType GetResonanceForAffinity(Affinity affinity)
        {
            return affinity switch
            {
                Affinity.Flame => ResonanceType.Inferno,
                Affinity.Frost => ResonanceType.AbsoluteCold,
                Affinity.Storm => ResonanceType.Tempest,
                Affinity.Nature => ResonanceType.Overgrowth,
                Affinity.Shadow => ResonanceType.VoidShroud,
                Affinity.Radiance => ResonanceType.DivineChorus,
                _ => ResonanceType.None
            };
        }

        void ApplyResonanceTick()
        {
            var pm = PartyManager.Instance;
            if (pm == null) return;

            switch (_activeResonance)
            {
                case ResonanceType.Inferno:
                    ApplyInfernoTick();
                    break;
                case ResonanceType.AbsoluteCold:
                    // Permanent 20% slow handled by combat system checking this
                    break;
                case ResonanceType.Tempest:
                    ApplyTempestTick();
                    break;
                case ResonanceType.Overgrowth:
                    ApplyOvergrowthTick(pm);
                    break;
                case ResonanceType.VoidShroud:
                    // 25% dodge handled by combat system checking this
                    break;
                case ResonanceType.DivineChorus:
                    ApplyDivineChorusTick(pm);
                    break;
            }
        }

        void ApplyInfernoTick()
        {
            // 5% max HP/s burn on all enemies
            var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                float burnDamage = enemy.MaxHp * 0.05f;
                enemy.TakeDamage(burnDamage, DamageType.Magical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = burnDamage,
                    IsCritical = false,
                    IsAoE = true,
                    WorldPosition = enemy.transform.position
                });
            }
        }

        void ApplyTempestTick()
        {
            // Lightning strikes random enemy every 2s (we tick every 1s, so 50% chance)
            if (Random.value > 0.5f) return;

            var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
            if (enemies == null || enemies.Count == 0) return;

            var target = enemies[Random.Range(0, enemies.Count)];
            var pm = PartyManager.Instance;
            float avgAtk = 0f;
            int count = 0;
            foreach (var hero in pm.GetAllAliveHeroes())
            {
                avgAtk += hero.CurrentAtk;
                count++;
            }
            if (count > 0) avgAtk /= count;

            float damage = avgAtk * 2f;
            target.TakeDamage(damage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = damage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });
        }

        void ApplyOvergrowthTick(PartyManager pm)
        {
            // 5% max HP/s regen for party
            foreach (var hero in pm.GetAllAliveHeroes())
            {
                float healAmount = hero.MaxHp * 0.05f;
                hero.Heal(healAmount);
            }
        }

        void ApplyDivineChorusTick(PartyManager pm)
        {
            // 3% max HP/s heal for party
            foreach (var hero in pm.GetAllAliveHeroes())
            {
                float healAmount = hero.MaxHp * 0.03f;
                hero.Heal(healAmount);
            }
        }

        // Query methods for combat system
        public float GetAffinityDamageBonus(Affinity affinity)
        {
            foreach (var syn in _activeAffinitySynergies)
            {
                if (syn.Affinity == affinity)
                    return syn.DamageBonus;
            }
            return 0f;
        }

        public bool HasResonance(ResonanceType type) => _activeResonance == type;

        public float GetTempestAttackSpeedBonus()
        {
            return _activeResonance == ResonanceType.Tempest ? 0.30f : 0f;
        }

        public float GetVoidShroudDodgeBonus()
        {
            return _activeResonance == ResonanceType.VoidShroud ? 0.25f : 0f;
        }

        public float GetDivineChorusDamageReduction()
        {
            return _activeResonance == ResonanceType.DivineChorus ? 0.15f : 0f;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
