using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class DragonHunterTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Charge Config")]
        [SerializeField] float _minChargeTime = 0.5f;
        [SerializeField] float _maxChargeTime = 2f;
        [SerializeField] float _minDamageMultiplier = 1f;
        [SerializeField] float _maxDamageMultiplier = 3f;
        [SerializeField] float _bossMaxDamageMultiplier = 5f;

        [Header("Dragonbane Debuff")]
        [SerializeField] float _dragonbanePerStack = 0.05f;
        [SerializeField] int _maxDragonbaneStacks = 3;

        [Header("Boss Passive")]
        [SerializeField] float _baseBossDamageBonus = 0.5f;
        [SerializeField] float _failureBonusPerAttempt = 0.05f;
        [SerializeField] int _maxFailureBonus = 10; // 10 failures * 5% = +50%

        // Runtime state
        float _timeSinceLastTap = 0f;
        bool _isTargetBoss = false;

        // Dragonbane stacks per enemy ID
        readonly Dictionary<int, int> _dragonbaneStacks = new Dictionary<int, int>();

        // Failed boss attempts per boss ID
        readonly Dictionary<int, int> _bossFailureAttempts = new Dictionary<int, int>();

        // ---- Public API ----

        /// <summary>0..1 representing how charged the current shot is.</summary>
        public float ChargePercent
        {
            get
            {
                if (_timeSinceLastTap < _minChargeTime) return 0f;
                return Mathf.Clamp01((_timeSinceLastTap - _minChargeTime) / (_maxChargeTime - _minChargeTime));
            }
        }

        /// <summary>Set externally when the current target is a boss.</summary>
        public bool IsTargetBoss
        {
            get => _isTargetBoss;
            set => _isTargetBoss = value;
        }

        public int GetDragonbaneStacks(int enemyId)
        {
            return _dragonbaneStacks.TryGetValue(enemyId, out int stacks) ? stacks : 0;
        }

        /// <summary>Total boss damage bonus: base 50% + up to 50% from failure tracking.</summary>
        public float BossDamageBonus
        {
            get
            {
                // Aggregate failure bonuses across all tracked bosses (use max single-boss value)
                int maxFailures = 0;
                foreach (var kvp in _bossFailureAttempts)
                    if (kvp.Value > maxFailures) maxFailures = kvp.Value;
                int clampedFailures = Mathf.Min(maxFailures, _maxFailureBonus);
                return _baseBossDamageBonus + clampedFailures * _failureBonusPerAttempt;
            }
        }

        // ---- Unity lifecycle ----

        void Update()
        {
            _timeSinceLastTap += Time.deltaTime;
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead)
            {
                // Count as a failed boss attempt if we expected a boss target
                if (_isTargetBoss)
                    RecordBossFailure(enemy?.Id ?? -1);
                _timeSinceLastTap = 0f;
                return;
            }

            float multiplier = CalculateDamageMultiplier(enemy);
            float finalDamage = damage * multiplier;

            // Apply Slayer's Instinct passive vs bosses
            if (_isTargetBoss)
                finalDamage *= (1f + BossDamageBonus);

            // Apply Dragonbane incoming damage bonus
            int dbStacks = GetDragonbaneStacks(enemy.Id);
            if (dbStacks > 0)
                finalDamage *= (1f + dbStacks * _dragonbanePerStack);

            bool isCrit = _isTargetBoss && ChargePercent >= 1f;
            enemy.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = isCrit,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Apply Dragonbane debuff on fully charged shots
            if (ChargePercent >= 1f)
                ApplyDragonbane(enemy.Id);

            _timeSinceLastTap = 0f;
        }

        public void Reset()
        {
            _timeSinceLastTap = 0f;
            _dragonbaneStacks.Clear();
            _bossFailureAttempts.Clear();
            _isTargetBoss = false;
        }

        // ---- Helpers ----

        float CalculateDamageMultiplier(EnemyController enemy)
        {
            float charge = ChargePercent;
            float maxMult = _isTargetBoss ? _bossMaxDamageMultiplier : _maxDamageMultiplier;
            return Mathf.Lerp(_minDamageMultiplier, maxMult, charge);
        }

        void ApplyDragonbane(int enemyId)
        {
            if (!_dragonbaneStacks.ContainsKey(enemyId))
                _dragonbaneStacks[enemyId] = 0;

            if (_dragonbaneStacks[enemyId] < _maxDragonbaneStacks)
                _dragonbaneStacks[enemyId]++;
        }

        void RecordBossFailure(int bossId)
        {
            if (bossId < 0) return;
            if (!_bossFailureAttempts.ContainsKey(bossId))
                _bossFailureAttempts[bossId] = 0;
            _bossFailureAttempts[bossId]++;
        }
    }
}
