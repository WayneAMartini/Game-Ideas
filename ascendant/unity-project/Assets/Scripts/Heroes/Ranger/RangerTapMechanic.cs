using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class RangerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 1;
        [SerializeField] float _bondFillPerTap = 6.67f;
        [SerializeField] float _packAttackMultiplier = 3.0f;
        [SerializeField] float _petStatBonusPercent = 0.5f;
        [SerializeField] float _griefFuryAtkBonus = 0.30f;
        [SerializeField] float _griefFuryDuration = 10f;
        [SerializeField] float _petAutoAttackInterval = 1.5f;
        [SerializeField] float _petReviveDuration = 30f;
        [SerializeField] float _petHpPercent = 0.40f;
        [SerializeField] float _petAtkPercent = 0.50f;

        [Header("State")]
        [SerializeField] float _bondMeter = 0f;
        [SerializeField] bool _isPetAlive = true;
        [SerializeField] float _petCurrentHp = 0f;
        [SerializeField] float _petMaxHp = 0f;
        [SerializeField] float _petAtk = 0f;
        [SerializeField] bool _isGriefFuryActive = false;

        // Timers
        float _petAutoAttackTimer = 0f;
        float _petReviveTimer = 0f;
        float _griefFuryTimer = 0f;
        bool _packAttackReady = false;

        // Expose properties
        public float BondMeter => _bondMeter;
        public float BondMax => 100f;
        public bool IsPetAlive => _isPetAlive;
        public float PetHp => _petCurrentHp;
        public float PetMaxHp => _petMaxHp;
        public bool IsGriefFuryActive => _isGriefFuryActive;

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            InitPet();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void InitPet()
        {
            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null) return;

            _petMaxHp = hero.MaxHp * _petHpPercent;
            _petCurrentHp = _petMaxHp;
            _petAtk = hero.CurrentAtk * _petAtkPercent;
            _isPetAlive = true;
        }

        void Update()
        {
            // Refresh pet stats from ranger stats (Beast Bond passive)
            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero != null && hero.IsAlive)
            {
                _petMaxHp = hero.MaxHp * _petHpPercent;
                _petAtk = hero.CurrentAtk * _petAtkPercent;
                if (_isGriefFuryActive)
                    _petAtk = 0f; // pet is dead during grief fury
            }

            // Pet auto-attack
            if (_isPetAlive)
            {
                _petAutoAttackTimer += Time.deltaTime;
                if (_petAutoAttackTimer >= _petAutoAttackInterval)
                {
                    _petAutoAttackTimer = 0f;
                    PetAutoAttack();
                }
            }
            else
            {
                // Pet revive timer
                _petReviveTimer += Time.deltaTime;
                if (_petReviveTimer >= _petReviveDuration)
                {
                    RevivePet();
                }
            }

            // Grief Fury timer
            if (_isGriefFuryActive)
            {
                _griefFuryTimer += Time.deltaTime;
                if (_griefFuryTimer >= _griefFuryDuration)
                {
                    _isGriefFuryActive = false;
                    _griefFuryTimer = 0f;
                }
            }
        }

        void PetAutoAttack()
        {
            // Use a central position for nearest-enemy lookup
            var nearestEnemy = EnemyManager.Instance.GetNearestEnemy(transform.position);
            if (nearestEnemy == null || nearestEnemy.IsDead) return;

            nearestEnemy.TakeDamage(_petAtk, DamageType.Physical);

            EventBus.Publish(new PetAttackEvent
            {
                OwnerHeroSlot = _heroSlot,
                EnemyId = nearestEnemy.Id,
                Damage = _petAtk
            });
        }

        void RevivePet()
        {
            _petReviveTimer = 0f;
            _isPetAlive = true;
            _petCurrentHp = _petMaxHp;

            EventBus.Publish(new PetRevivedEvent
            {
                OwnerHeroSlot = _heroSlot
            });
        }

        void OnEnemyKilled(EnemyKilledEvent e)
        {
            // Bond meter gets a small bonus when pet participates in a kill
            if (_isPetAlive)
            {
                _bondMeter = Mathf.Min(_bondMeter + 2f, BondMax);
            }
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var hero = Party.PartyManager.Instance.GetHero(_heroSlot);
            if (hero == null || !hero.IsAlive) return;

            float rangerDmg = damage;
            if (_isGriefFuryActive)
                rangerDmg *= (1f + _griefFuryAtkBonus);

            // Check if Pack Attack ready
            if (_packAttackReady)
            {
                _packAttackReady = false;
                _bondMeter = 0f;
                TriggerPackAttack(rangerDmg, worldPosition);
                return;
            }

            // Fill Bond meter
            _bondMeter = Mathf.Min(_bondMeter + _bondFillPerTap, BondMax);
            if (_bondMeter >= BondMax)
                _packAttackReady = true;

            // Normal tap: ranger shot
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            enemy.TakeDamage(rangerDmg, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = rangerDmg,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });
        }

        void TriggerPackAttack(float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float totalDmg = damage * _packAttackMultiplier;
            enemy.TakeDamage(totalDmg, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = totalDmg,
                IsCritical = true,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Pet also strikes during pack attack
            if (_isPetAlive)
            {
                float petPackDmg = _petAtk * _packAttackMultiplier;
                enemy.TakeDamage(petPackDmg, DamageType.Physical);

                EventBus.Publish(new PetAttackEvent
                {
                    OwnerHeroSlot = _heroSlot,
                    EnemyId = enemy.Id,
                    Damage = petPackDmg
                });
            }
        }

        public void Reset()
        {
            _bondMeter = 0f;
            _packAttackReady = false;
            _isGriefFuryActive = false;
            _griefFuryTimer = 0f;
            _petAutoAttackTimer = 0f;
            _petReviveTimer = 0f;
            InitPet();
        }
    }
}
