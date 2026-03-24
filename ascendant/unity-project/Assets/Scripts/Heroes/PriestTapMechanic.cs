using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class PriestTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Holy Touch Config")]
        [SerializeField] float _healThreshold = 0.5f; // heal if ally below 50% HP

        [Header("Passive — Divine Grace")]
        [SerializeField] float _damageReduction = 0.1f; // 10% reduced damage
        [SerializeField] float _lingerDuration = 5f; // lingers 5s after death

        bool _isPriestAlive = true;
        float _lingerTimer;

        public bool IsDivineGraceActive => _isPriestAlive || _lingerTimer > 0f;
        public float DamageReduction => IsDivineGraceActive ? _damageReduction : 0f;

        void Update()
        {
            if (!_isPriestAlive && _lingerTimer > 0f)
                _lingerTimer -= Time.deltaTime;
        }

        public void OnPriestDied()
        {
            _isPriestAlive = false;
            _lingerTimer = _lingerDuration;
        }

        public void OnPriestRevived()
        {
            _isPriestAlive = true;
            _lingerTimer = 0f;
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            // Auto-triage: if any ally <50% HP, heal lowest-HP ally
            // Otherwise deal holy damage to target enemy
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null && partyManager.AnyAllyBelowHpPercent(_healThreshold))
            {
                HealLowestAlly(damage);
            }
            else
            {
                DealHolyDamage(damage, worldPosition);
            }
        }

        void HealLowestAlly(float healPower)
        {
            var partyManager = Party.PartyManager.Instance;
            if (partyManager == null) return;

            var target = partyManager.GetLowestHpAliveHero();
            if (target == null) return;

            target.Heal(healPower);

            EventBus.Publish(new HeroHealedEvent
            {
                HeroSlot = target.Slot,
                Amount = healPower,
                CurrentHp = target.CurrentHp,
                MaxHp = target.MaxHp
            });
        }

        void DealHolyDamage(float damage, Vector3 worldPosition)
        {
            var target = EnemyManager.Instance?.GetNearestEnemy(worldPosition);
            if (target == null || target.IsDead) return;

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

        public void Reset()
        {
            _isPriestAlive = true;
            _lingerTimer = 0f;
        }
    }
}
