using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public enum BardSong
    {
        BalladOfValor,
        HymnOfFortitude,
        MarchOfHaste
    }

    public class BardTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] int _heroSlot = 3;
        [SerializeField] int _crescendoTapInterval = 8;
        [SerializeField] float _crescendoDuration = 5f;
        [SerializeField] float _songTransitionDuration = 3f;
        [SerializeField] float _encoreChance = 0.10f;

        [Header("Song Bonuses (base, before Crescendo)")]
        [SerializeField] float _balladAtkBonus = 0.15f;
        [SerializeField] float _hymnDefBonus = 0.15f;
        [SerializeField] float _hymnHpRegenPercent = 0.03f;
        [SerializeField] float _marchAttackSpeedBonus = 0.20f;
        [SerializeField] float _marchCooldownReduction = 0.15f;

        [Header("State")]
        [SerializeField] BardSong _activeSong = BardSong.BalladOfValor;
        [SerializeField] int _tapCounter = 0;
        [SerializeField] bool _isCrescendoActive = false;
        [SerializeField] float _crescendoTimer = 0f;
        [SerializeField] bool _isTransitioning = false;
        [SerializeField] float _transitionTimer = 0f;
        [SerializeField] BardSong _pendingSong = BardSong.BalladOfValor;

        // Expose properties
        public BardSong ActiveSong => _activeSong;
        public int TapsUntilCrescendo => _crescendoTapInterval - (_tapCounter % _crescendoTapInterval);
        public bool IsCrescendoActive => _isCrescendoActive;
        public float CrescendoTimer => _crescendoTimer;
        public bool IsTransitioning => _isTransitioning;

        void OnEnable()
        {
            // Publish initial song state
            EventBus.Publish(new SongChangedEvent
            {
                HeroSlot = _heroSlot,
                SongName = _activeSong.ToString()
            });
        }

        void Update()
        {
            // Crescendo countdown
            if (_isCrescendoActive)
            {
                _crescendoTimer -= Time.deltaTime;
                if (_crescendoTimer <= 0f)
                {
                    _isCrescendoActive = false;
                    _crescendoTimer = 0f;
                }
            }

            // Song transition countdown
            if (_isTransitioning)
            {
                _transitionTimer -= Time.deltaTime;
                if (_transitionTimer <= 0f)
                {
                    _isTransitioning = false;
                    _transitionTimer = 0f;
                    _activeSong = _pendingSong;

                    EventBus.Publish(new SongChangedEvent
                    {
                        HeroSlot = _heroSlot,
                        SongName = _activeSong.ToString()
                    });
                }
            }

            // Hymn of Fortitude passive HP regen
            if (_activeSong == BardSong.HymnOfFortitude && !_isTransitioning)
            {
                float regenMultiplier = _isCrescendoActive ? 3f : 1f;
                float regenRate = _hymnHpRegenPercent * regenMultiplier;

                var allHeroes = Party.PartyManager.Instance.GetAllAliveHeroes();
                if (allHeroes != null)
                {
                    foreach (var hero in allHeroes)
                    {
                        if (hero == null || !hero.IsAlive) continue;
                        float regenAmount = hero.MaxHp * regenRate * Time.deltaTime;
                        hero.Heal(regenAmount);

                        EventBus.Publish(new HeroHealedEvent
                        {
                            HeroSlot = hero.Slot,
                            Amount = regenAmount,
                            CurrentHp = hero.CurrentHp,
                            MaxHp = hero.MaxHp
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Switch to a different song. Begins a 3s transition.
        /// </summary>
        public void SetSong(BardSong newSong)
        {
            if (newSong == _activeSong && !_isTransitioning) return;

            _pendingSong = newSong;
            _isTransitioning = true;
            _transitionTimer = _songTransitionDuration;
        }

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
            if (enemy == null || enemy.IsDead) return;

            // Apply song ATK bonus for Ballad of Valor (and Crescendo multiplier)
            float outDamage = damage;
            if (_activeSong == BardSong.BalladOfValor && !_isTransitioning)
            {
                float bonus = _balladAtkBonus * (_isCrescendoActive ? 3f : 1f);
                outDamage *= (1f + bonus);
            }

            enemy.TakeDamage(outDamage, DamageType.Magical);

            EventBus.Publish(new EnemyDamagedEvent
            {
                EnemyId = enemy.Id,
                Damage = outDamage,
                IsCritical = false,
                IsAoE = false,
                WorldPosition = enemy.transform.position
            });

            // Increment tap counter
            _tapCounter++;

            // Every 8th tap: trigger Crescendo
            if (_tapCounter % _crescendoTapInterval == 0)
            {
                TriggerCrescendo();
            }

            // Encore passive: 10% chance to signal ability duplication
            if (Random.value < _encoreChance)
            {
                // Publish a Crescendo event at 50% to signal Encore duplication
                EventBus.Publish(new CrescendoTriggeredEvent
                {
                    HeroSlot = _heroSlot,
                    SongName = "Encore_" + _activeSong.ToString()
                });
            }
        }

        void TriggerCrescendo()
        {
            _isCrescendoActive = true;
            _crescendoTimer = _crescendoDuration;

            EventBus.Publish(new CrescendoTriggeredEvent
            {
                HeroSlot = _heroSlot,
                SongName = _activeSong.ToString()
            });
        }

        public void Reset()
        {
            _tapCounter = 0;
            _isCrescendoActive = false;
            _crescendoTimer = 0f;
            _isTransitioning = false;
            _transitionTimer = 0f;
            _activeSong = BardSong.BalladOfValor;
            _pendingSong = BardSong.BalladOfValor;
        }
    }
}
