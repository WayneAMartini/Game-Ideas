using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class RogueTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Combo Point Config")]
        [SerializeField] int _maxComboPoints = 5;
        [SerializeField] float _finishingBlowMultiplier = 5f; // 500% tap damage
        [SerializeField] float _comboDecayTime = 3f; // CP decay after 3s

        [Header("Passive — Cloak of Shadows")]
        [SerializeField] float _baseDodgeChance = 0.15f; // 15%
        [SerializeField] float _lowHpDodgeChance = 0.30f; // 30% below 50% HP
        [SerializeField] float _lowHpThreshold = 0.5f;

        int _comboPoints;
        float _timeSinceLastTap;

        public int ComboPoints => _comboPoints;
        public int MaxComboPoints => _maxComboPoints;
        public bool IsFinishingBlowReady => _comboPoints >= _maxComboPoints;

        void Update()
        {
            if (_comboPoints > 0)
            {
                _timeSinceLastTap += Time.deltaTime;
                if (_timeSinceLastTap >= _comboDecayTime)
                {
                    _comboPoints = 0;
                    EventBus.Publish(new ComboPointsChangedEvent
                    {
                        ComboPoints = _comboPoints,
                        MaxComboPoints = _maxComboPoints
                    });
                }
            }
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            _timeSinceLastTap = 0f;

            if (_comboPoints >= _maxComboPoints)
            {
                // Finishing Blow — 500% damage, consume all CP
                TriggerFinishingBlow(damage, worldPosition);
            }
            else
            {
                // Build combo point
                _comboPoints++;

                EventBus.Publish(new ComboPointsChangedEvent
                {
                    ComboPoints = _comboPoints,
                    MaxComboPoints = _maxComboPoints
                });
            }
        }

        void TriggerFinishingBlow(float baseDamage, Vector3 worldPosition)
        {
            float finishingDamage = baseDamage * _finishingBlowMultiplier;
            _comboPoints = 0;

            var target = EnemyManager.Instance?.GetNearestEnemy(worldPosition);
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(finishingDamage, DamageType.Physical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = target.Id,
                    Damage = finishingDamage,
                    IsCritical = true,
                    IsAoE = false,
                    WorldPosition = target.transform.position
                });
            }

            EventBus.Publish(new ComboPointsChangedEvent
            {
                ComboPoints = 0,
                MaxComboPoints = _maxComboPoints
            });

            EventBus.Publish(new FinishingBlowEvent
            {
                Damage = finishingDamage,
                WorldPosition = worldPosition
            });
        }

        // Called when Rogue dodges an attack (from passive)
        public void OnDodge()
        {
            if (_comboPoints < _maxComboPoints)
            {
                _comboPoints++;
                _timeSinceLastTap = 0f;

                EventBus.Publish(new ComboPointsChangedEvent
                {
                    ComboPoints = _comboPoints,
                    MaxComboPoints = _maxComboPoints
                });
            }
        }

        // Adds combo points (used by Ambush ability)
        public void AddComboPoints(int amount)
        {
            _comboPoints = Mathf.Min(_maxComboPoints, _comboPoints + amount);
            _timeSinceLastTap = 0f;

            EventBus.Publish(new ComboPointsChangedEvent
            {
                ComboPoints = _comboPoints,
                MaxComboPoints = _maxComboPoints
            });
        }

        public float GetDodgeChance(float currentHpPercent)
        {
            return currentHpPercent < _lowHpThreshold ? _lowHpDodgeChance : _baseDodgeChance;
        }

        public void Reset()
        {
            _comboPoints = 0;
            _timeSinceLastTap = 0f;
        }
    }
}
