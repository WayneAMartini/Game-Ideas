using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Islands
{
    public class BiomeEffectSystem : MonoBehaviour
    {
        public static BiomeEffectSystem Instance { get; private set; }

        BiomeData _activeBiome;
        float _tickTimer;

        // Cached modifiers for current biome
        float _healingModifier = 1f;
        float _speedModifier = 1f;
        float _accuracyModifier = 1f;
        float _damageReflectPercent;

        public float HealingModifier => _healingModifier;
        public float SpeedModifier => _speedModifier;
        public float AccuracyModifier => _accuracyModifier;
        public BiomeData ActiveBiome => _activeBiome;

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
            EventBus.Subscribe<IslandChangedEvent>(OnIslandChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<IslandChangedEvent>(OnIslandChanged);
        }

        void OnIslandChanged(IslandChangedEvent evt)
        {
            SetBiome(evt.IslandData?.biomeData);
        }

        public void SetBiome(BiomeData biome)
        {
            _activeBiome = biome;
            _tickTimer = 0f;
            _healingModifier = 1f;
            _speedModifier = 1f;
            _accuracyModifier = 1f;
            _damageReflectPercent = 0f;

            if (biome == null) return;

            switch (biome.effectType)
            {
                case BiomeEffectType.HealingModifier:
                    _healingModifier = 1f + biome.modifierValue;
                    break;
                case BiomeEffectType.SpeedModifier:
                    _speedModifier = 1f + biome.modifierValue;
                    break;
                case BiomeEffectType.AccuracyDebuff:
                    _accuracyModifier = 1f + biome.modifierValue;
                    break;
                case BiomeEffectType.DamageReflect:
                    _damageReflectPercent = biome.effectValue;
                    break;
            }
        }

        void Update()
        {
            if (_activeBiome == null) return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Combat) return;

            _tickTimer += Time.deltaTime;
            if (_tickTimer < _activeBiome.tickInterval) return;
            _tickTimer = 0f;

            ApplyTickEffect();
        }

        void ApplyTickEffect()
        {
            if (_activeBiome == null) return;

            var party = PartyManager.Instance;
            if (party == null) return;

            switch (_activeBiome.effectType)
            {
                case BiomeEffectType.DamageOverTime:
                    ApplyDotToParty(party);
                    break;
                case BiomeEffectType.PeriodicRooting:
                    ApplyPeriodicRooting(party);
                    break;
                case BiomeEffectType.RandomAoE:
                    ApplyRandomAoE();
                    break;
                case BiomeEffectType.HpRegen:
                    ApplyHpRegen(party);
                    break;
                case BiomeEffectType.ShieldGrant:
                    // Shield logic would integrate with a shield system
                    break;
            }

            EventBus.Publish(new BiomeEffectAppliedEvent
            {
                EffectType = _activeBiome.effectType,
                Value = _activeBiome.effectValue
            });
        }

        void ApplyDotToParty(PartyManager party)
        {
            for (int i = 0; i < 4; i++)
            {
                var hero = party.GetHero(i);
                if (hero == null || !hero.IsAlive) continue;

                // Immune affinity heroes skip the DoT
                if (hero.Affinity == _activeBiome.immuneAffinity) continue;

                float damage = hero.MaxHp * _activeBiome.effectValue;
                hero.TakeDamage(damage);
            }
        }

        void ApplyPeriodicRooting(PartyManager party)
        {
            // Root a random non-immune hero for the tick duration
            var alive = party.GetAllAliveHeroes();
            if (alive.Length == 0) return;

            var target = alive[Random.Range(0, alive.Length)];
            if (target.Affinity == _activeBiome.immuneAffinity) return;

            EventBus.Publish(new RootAppliedEvent
            {
                EnemyId = -1, // Indicates biome root, not enemy
                Duration = _activeBiome.modifierValue > 0 ? _activeBiome.modifierValue : 2f
            });
        }

        void ApplyRandomAoE()
        {
            // Lightning strikes random enemies
            var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
            if (enemies == null || enemies.Count == 0) return;

            var target = enemies[Random.Range(0, enemies.Count)];
            float damage = target.MaxHp * _activeBiome.effectValue;
            target.TakeDamage(damage);
        }

        void ApplyHpRegen(PartyManager party)
        {
            for (int i = 0; i < 4; i++)
            {
                var hero = party.GetHero(i);
                if (hero == null || !hero.IsAlive) continue;

                float healAmount = hero.MaxHp * _activeBiome.effectValue;
                hero.Heal(healAmount);
            }
        }

        public bool IsHeroImmuneToEffect(Affinity heroAffinity)
        {
            if (_activeBiome == null) return false;
            return heroAffinity == _activeBiome.immuneAffinity;
        }

        public float GetDamageReflectPercent(Affinity heroAffinity)
        {
            if (_activeBiome == null || _activeBiome.effectType != BiomeEffectType.DamageReflect)
                return 0f;
            if (heroAffinity == _activeBiome.immuneAffinity)
                return 0f;
            return _damageReflectPercent;
        }

        public float GetAccuracyModifier(Affinity heroAffinity)
        {
            if (_activeBiome == null || _activeBiome.effectType != BiomeEffectType.AccuracyDebuff)
                return 1f;
            if (heroAffinity == _activeBiome.immuneAffinity)
                return 1f;
            return _accuracyModifier;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
