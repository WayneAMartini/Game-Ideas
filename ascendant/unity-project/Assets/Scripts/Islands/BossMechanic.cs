using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Islands
{
    public abstract class BossMechanic
    {
        protected Enemy _boss;
        protected float _timer;
        protected bool _isActive;

        public bool IsActive => _isActive;
        public abstract BossMechanicType MechanicType { get; }

        public virtual void Initialize(Enemy boss)
        {
            _boss = boss;
            _isActive = false;
            _timer = 0f;
        }

        public virtual void Activate()
        {
            _isActive = true;
            EventBus.Publish(new BossMechanicActivatedEvent
            {
                MechanicType = MechanicType,
                WarningText = GetWarningText()
            });
        }

        public virtual void Deactivate()
        {
            _isActive = false;
        }

        public abstract void Update(float deltaTime);
        public abstract string GetWarningText();
    }

    // Boss gains +10% ATK every 10s; must be killed quickly
    public class EnrageMechanic : BossMechanic
    {
        float _enrageInterval = 10f;
        float _enrageTimer;
        int _enrageStacks;
        float _originalAtk;

        public override BossMechanicType MechanicType => BossMechanicType.Enrage;

        public override void Initialize(Enemy boss)
        {
            base.Initialize(boss);
            _originalAtk = boss.Atk;
        }

        public override void Activate()
        {
            base.Activate();
            _enrageTimer = _enrageInterval;
            _enrageStacks = 0;
        }

        public override void Update(float deltaTime)
        {
            if (!_isActive) return;

            _enrageTimer -= deltaTime;
            if (_enrageTimer <= 0f)
            {
                _enrageStacks++;
                _enrageTimer = _enrageInterval;
                // Boss ATK increases (tracked externally for damage calcs)
            }
        }

        public float GetAtkMultiplier()
        {
            return 1f + _enrageStacks * 0.1f;
        }

        public override string GetWarningText() => "The boss is enraging! Kill it fast!";
    }

    // Boss becomes immune until shield is broken
    public class ShieldPhaseMechanic : BossMechanic
    {
        float _shieldHp;
        float _maxShieldHp;
        bool _shieldActive;

        public override BossMechanicType MechanicType => BossMechanicType.ShieldPhase;
        public bool ShieldActive => _shieldActive;
        public float ShieldHp => _shieldHp;
        public float MaxShieldHp => _maxShieldHp;

        public override void Activate()
        {
            base.Activate();
            _maxShieldHp = _boss.MaxHp * 0.3f;
            _shieldHp = _maxShieldHp;
            _shieldActive = true;
        }

        public override void Update(float deltaTime)
        {
            // Shield logic handled via damage interception
        }

        public float AbsorbDamage(float damage)
        {
            if (!_shieldActive) return damage;

            _shieldHp -= damage;
            if (_shieldHp <= 0f)
            {
                _shieldActive = false;
                float overflow = -_shieldHp;
                _shieldHp = 0f;
                return overflow;
            }
            return 0f;
        }

        public override string GetWarningText() => "Shield activated! Break it to deal damage!";
    }

    // Boss summons 2 minions at 50% HP
    public class AddSpawningMechanic : BossMechanic
    {
        bool _hasSpawned;
        float _spawnThreshold = 0.5f;

        public override BossMechanicType MechanicType => BossMechanicType.AddSpawning;
        public bool HasSpawned => _hasSpawned;

        public override void Update(float deltaTime)
        {
            if (!_isActive || _hasSpawned) return;

            if (_boss.CurrentHp / _boss.MaxHp <= _spawnThreshold)
            {
                _hasSpawned = true;
                // Spawning handled by MiniBossController
            }
        }

        public bool ShouldSpawnAdds()
        {
            if (_hasSpawned) return false;
            if (_boss.CurrentHp / _boss.MaxHp <= _spawnThreshold)
            {
                _hasSpawned = true;
                return true;
            }
            return false;
        }

        public override string GetWarningText() => "The boss summons minions!";
    }

    // AoE attack with visual telegraph; tap to dodge
    public class GroundSlamMechanic : BossMechanic
    {
        float _slamInterval = 8f;
        float _slamTimer;
        float _telegraphDuration = 1.5f;
        bool _isTelegraphing;
        float _telegraphTimer;

        public override BossMechanicType MechanicType => BossMechanicType.GroundSlam;
        public bool IsTelegraphing => _isTelegraphing;

        public override void Activate()
        {
            base.Activate();
            _slamTimer = _slamInterval;
        }

        public override void Update(float deltaTime)
        {
            if (!_isActive) return;

            if (_isTelegraphing)
            {
                _telegraphTimer -= deltaTime;
                if (_telegraphTimer <= 0f)
                {
                    _isTelegraphing = false;
                    // Slam hits - damage applied by MiniBossController
                }
                return;
            }

            _slamTimer -= deltaTime;
            if (_slamTimer <= 0f)
            {
                _isTelegraphing = true;
                _telegraphTimer = _telegraphDuration;
                _slamTimer = _slamInterval;

                EventBus.Publish(new DodgePromptEvent
                {
                    TargetPosition = _boss.transform.position,
                    TimeWindow = _telegraphDuration
                });
            }
        }

        public override string GetWarningText() => "Ground Slam incoming! Tap to dodge!";
    }

    // Boss heals from damage dealt
    public class LifeStealMechanic : BossMechanic
    {
        float _lifeStealPercent = 0.3f;

        public override BossMechanicType MechanicType => BossMechanicType.LifeSteal;

        public float GetHealAmount(float damageDone)
        {
            if (!_isActive) return 0f;
            return damageDone * _lifeStealPercent;
        }

        public override void Update(float deltaTime) { }

        public override string GetWarningText() => "The boss drains life from attacks!";
    }

    // Boss splits into 2 weaker copies at 50% HP
    public class SplitMechanic : BossMechanic
    {
        bool _hasSplit;
        float _splitThreshold = 0.5f;

        public override BossMechanicType MechanicType => BossMechanicType.Split;
        public bool HasSplit => _hasSplit;

        public override void Update(float deltaTime)
        {
            if (!_isActive || _hasSplit) return;
        }

        public bool ShouldSplit()
        {
            if (_hasSplit) return false;
            if (_boss.CurrentHp / _boss.MaxHp <= _splitThreshold)
            {
                _hasSplit = true;
                return true;
            }
            return false;
        }

        public override string GetWarningText() => "The boss is splitting!";
    }

    // Boss reflects damage for 3s intervals
    public class ReflectMechanic : BossMechanic
    {
        float _reflectDuration = 3f;
        float _cooldownDuration = 5f;
        float _reflectTimer;
        bool _isReflecting;

        public override BossMechanicType MechanicType => BossMechanicType.Reflect;
        public bool IsReflecting => _isReflecting;

        public override void Activate()
        {
            base.Activate();
            _isReflecting = false;
            _reflectTimer = _cooldownDuration;
        }

        public override void Update(float deltaTime)
        {
            if (!_isActive) return;

            _reflectTimer -= deltaTime;
            if (_reflectTimer <= 0f)
            {
                _isReflecting = !_isReflecting;
                _reflectTimer = _isReflecting ? _reflectDuration : _cooldownDuration;

                if (_isReflecting)
                {
                    EventBus.Publish(new BossMechanicActivatedEvent
                    {
                        MechanicType = BossMechanicType.Reflect,
                        WarningText = "REFLECTING! Stop attacking!"
                    });
                }
            }
        }

        public float GetReflectedDamage(float incomingDamage)
        {
            if (!_isReflecting) return 0f;
            return incomingDamage;
        }

        public override string GetWarningText() => "The boss reflects damage! Stop attacking!";
    }
}
