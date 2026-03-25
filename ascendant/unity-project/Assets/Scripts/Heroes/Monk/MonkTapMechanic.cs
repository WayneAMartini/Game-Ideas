using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class MonkTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 3;
        [SerializeField] float _comboResetTimeout = 1.0f;
        [SerializeField] float _innerPeaceRegenPercent = 0.02f;
        [SerializeField] float _chiPerCompletion = 20f;
        [SerializeField] float _innerPeaceIdleThreshold = 2.0f;

        // Combo multipliers: hits 1-5
        static readonly float[] _comboMultipliers = { 0.80f, 1.00f, 1.20f, 1.50f, 2.50f };

        [Header("State")]
        [SerializeField] int _comboPosition = 0;
        [SerializeField] float _chi = 0f;
        [SerializeField] float _timeSinceLastTap = 0f;
        [SerializeField] float _idleTimer = 0f;

        bool _isAttacking = false;

        // Expose properties
        public int ComboPosition => _comboPosition;
        public int MaxCombo => 5;
        public float Chi => _chi;
        public float MaxChi => 100f;
        public float[] ComboMultipliers => _comboMultipliers;

        void Update()
        {
            // Track time since last tap for combo reset
            _timeSinceLastTap += Time.deltaTime;
            if (_comboPosition > 0 && _timeSinceLastTap >= _comboResetTimeout)
            {
                _comboPosition = 0;
            }

            // Inner Peace: regen HP while not attacking
            if (!_isAttacking)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= _innerPeaceIdleThreshold)
                {
                    var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
                    if (hero != null && hero.IsAlive && hero.CurrentHp < hero.MaxHp)
                    {
                        float regenAmount = hero.MaxHp * _innerPeaceRegenPercent * Time.deltaTime;
                        hero.Heal(regenAmount);

                        EventBus.Publish(new HeroHealedEvent
                        {
                            HeroSlot = _heroSlot,
                            Amount = regenAmount,
                            CurrentHp = hero.CurrentHp,
                            MaxHp = hero.MaxHp
                        });
                    }
                }
            }
            else
            {
                _idleTimer = 0f;
                // Mark not attacking after a brief window
                if (_timeSinceLastTap >= 0.5f)
                    _isAttacking = false;
            }
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            _timeSinceLastTap = 0f;
            _isAttacking = true;
            _idleTimer = 0f;

            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float multiplier = _comboMultipliers[_comboPosition];
            float outDamage = damage * multiplier;

            enemy.TakeDamage(outDamage, DamageType.Physical);

            bool isPerfectStrike = (_comboPosition == MaxCombo - 1);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = outDamage,
                IsCritical = isPerfectStrike,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            _comboPosition++;

            if (_comboPosition >= MaxCombo)
            {
                // Combo complete: award Chi and reset
                _chi = Mathf.Min(_chi + _chiPerCompletion, MaxChi);
                _comboPosition = 0;

                var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
                if (hero != null)
                {
                    EventBus.Publish(new ResourceChangedEvent
                    {
                        HeroSlot = _heroSlot,
                        ResourceName = "Chi",
                        Current = _chi,
                        Max = MaxChi
                    });
                }
            }
        }

        public void Reset()
        {
            _comboPosition = 0;
            _chi = 0f;
            _timeSinceLastTap = 0f;
            _idleTimer = 0f;
            _isAttacking = false;
        }
    }
}
