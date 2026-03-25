using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class GunslingerTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Fan the Hammer Config")]
        [SerializeField] int _maxAmmo = 6;
        [SerializeField] float _reloadDuration = 1.5f;
        [SerializeField] float _reloadCritMultiplier = 3f;

        [Header("Quick Draw Passive")]
        [SerializeField] float _quickDrawMultiplier = 3f;

        // Runtime state
        int _currentAmmo;
        bool _isReloading = false;
        float _reloadTimer = 0f;
        bool _isFirstShot = false; // First shot after reload
        int _lastTargetedEnemyId = -1;
        bool _quickDrawReady = false; // First attack on new enemy

        // ---- Public API ----

        public int CurrentAmmo => _currentAmmo;
        public int MaxAmmo => _maxAmmo;
        public bool IsReloading => _isReloading;
        public float ReloadProgress => _isReloading ? Mathf.Clamp01(1f - (_reloadTimer / _reloadDuration)) : 1f;
        public bool IsFirstShot => _isFirstShot;

        // ---- Unity lifecycle ----

        void Awake()
        {
            _currentAmmo = _maxAmmo;
            _isFirstShot = true;
        }

        void Update()
        {
            if (_isReloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                {
                    _isReloading = false;
                    _reloadTimer = 0f;
                    _currentAmmo = _maxAmmo;
                    _isFirstShot = true; // Next shot after reload is guaranteed crit

                    EventBus.Publish(new AmmoChangedEvent
                    {
                        HeroSlot = 0,
                        CurrentAmmo = _currentAmmo,
                        MaxAmmo = _maxAmmo,
                        IsReloading = false
                    });
                }
            }
        }

        // ---- ITapMechanic ----

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            if (_isReloading) return;

            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            float finalDamage = damage;
            bool isCrit = false;

            // Quick Draw passive: first attack on a new enemy
            bool isNewTarget = enemy.Id != _lastTargetedEnemyId;
            if (isNewTarget)
            {
                _lastTargetedEnemyId = enemy.Id;
                _quickDrawReady = true;
            }

            if (_quickDrawReady && isNewTarget)
            {
                finalDamage *= _quickDrawMultiplier;
                isCrit = true;
                _quickDrawReady = false;
            }

            // First shot after reload is guaranteed crit (3x), overrides Quick Draw if both apply
            if (_isFirstShot)
            {
                finalDamage = damage * _reloadCritMultiplier;
                isCrit = true;
                _isFirstShot = false;
            }

            enemy.TakeDamage(finalDamage, DamageType.Physical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = finalDamage,
                IsCritical = isCrit,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            _currentAmmo--;

            EventBus.Publish(new AmmoChangedEvent
            {
                HeroSlot = 0,
                CurrentAmmo = _currentAmmo,
                MaxAmmo = _maxAmmo,
                IsReloading = false
            });

            if (_currentAmmo <= 0)
                BeginReload();
        }

        public void Reset()
        {
            _currentAmmo = _maxAmmo;
            _isReloading = false;
            _reloadTimer = 0f;
            _isFirstShot = true;
            _lastTargetedEnemyId = -1;
            _quickDrawReady = false;
        }

        // ---- Helpers ----

        void BeginReload()
        {
            _isReloading = true;
            _reloadTimer = _reloadDuration;
            _currentAmmo = 0;

            EventBus.Publish(new AmmoChangedEvent
            {
                HeroSlot = 0,
                CurrentAmmo = 0,
                MaxAmmo = _maxAmmo,
                IsReloading = true
            });
        }
    }
}
