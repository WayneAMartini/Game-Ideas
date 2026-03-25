using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Party;

namespace Ascendant.Heroes
{
    /// <summary>
    /// Berserker Tier 2 tap mechanic.
    /// Rampage: builds Rage (1 per tap, max 100).
    ///   0-50 Rage  -> 1x damage
    ///   50-80 Rage -> 2x damage
    ///   80-100 Rage-> 3x damage, but each tap costs 2% of hero's max HP
    /// Passive - Blood Frenzy: +2% attack speed per 10% HP missing.
    /// Subscribes to HeroDamagedEvent to gain Rage from taking damage.
    /// </summary>
    public class BerserkerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Rampage Config")]
        [SerializeField] float _ragePerTap = 1f;
        [SerializeField] float _rageGainOnHit = 5f;
        [SerializeField] float _rageDecayPerSecond = 2f;

        [Header("Blood Frenzy Config")]
        [SerializeField] float _bloodFrenzySpeedBonusPer10PctMissing = 0.02f;

        [Header("HP Cost Config")]
        [SerializeField] float _highRageHpCostPercent = 0.02f;

        // Rage state
        float _rage = 0f;

        // Hero reference (resolved lazily via PartyManager)
        Hero _berserkerHero;

        public float Rage => _rage;
        public float MaxRage => 100f;

        /// <summary>
        /// Returns attack speed bonus from Blood Frenzy.
        /// hpPercent should be in range [0,1] where 0 = dead, 1 = full HP.
        /// </summary>
        public float GetBloodFrenzySpeedBonus(float hpPercent)
        {
            float missingPercent = 1f - Mathf.Clamp01(hpPercent);
            float tenPercentChunks = missingPercent / 0.1f;
            return tenPercentChunks * _bloodFrenzySpeedBonusPer10PctMissing;
        }

        void OnEnable()
        {
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
        }

        void Update()
        {
            // Rage decays over time so frenzied state doesn't persist indefinitely
            if (_rage > 0f)
            {
                _rage = Mathf.Max(0f, _rage - _rageDecayPerSecond * Time.deltaTime);
                PublishRageChanged();
            }
        }

        void OnHeroDamaged(HeroDamagedEvent evt)
        {
            // Gain bonus Rage whenever the Berserker takes damage
            Hero hero = ResolveBerserkerHero();
            if (hero == null) return;
            if (evt.HeroSlot != hero.Slot) return;

            AddRage(_rageGainOnHit);
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var target = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

            // Build Rage
            AddRage(_ragePerTap);

            // Determine damage multiplier based on Rage tier
            float damageMultiplier;
            if (_rage >= 80f)
            {
                damageMultiplier = 3f;
                // High Rage costs HP
                ApplyHighRageHpCost();
            }
            else if (_rage >= 50f)
            {
                damageMultiplier = 2f;
            }
            else
            {
                damageMultiplier = 1f;
            }

            float finalDamage = damage * damageMultiplier;
            target.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = target.Id,
                Damage = finalDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = target.transform.position
            });
        }

        public void Reset()
        {
            _rage = 0f;
            _berserkerHero = null;
            PublishRageChanged();
        }

        // ------------------------------------------------------------------ helpers

        void AddRage(float amount)
        {
            _rage = Mathf.Min(_rage + amount, MaxRage);
            PublishRageChanged();
        }

        void ApplyHighRageHpCost()
        {
            Hero hero = ResolveBerserkerHero();
            if (hero == null) return;

            float hpCost = hero.MaxHp * _highRageHpCostPercent;
            // Heal with a negative value is not guaranteed; use TakeDamage True to bypass armor
            // We model the HP drain as True damage to self — systems downstream handle death checks
            hero.TakeDamage(hpCost);
        }

        void PublishRageChanged()
        {
            EventBus.Publish(new ResourceChangedEvent
            {
                ResourceName = "Rage",
                Current = _rage,
                Max = MaxRage
            });
        }

        Hero ResolveBerserkerHero()
        {
            if (_berserkerHero != null) return _berserkerHero;
            // The Berserker is the hero this component is attached to
            _berserkerHero = GetComponentInParent<Hero>();
            return _berserkerHero;
        }
    }
}
