using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Combat
{
    public class AbilityController : MonoBehaviour
    {
        [Header("Warrior Abilities")]
        [SerializeField] Ability _cleavingStrike;
        [SerializeField] Ability _warCry;
        [SerializeField] Ability _ascendantBlade;

        readonly List<AbilitySlot> _slots = new();

        float _ultimateChargePerDamage = 0.1f; // charge gained per point of damage dealt

        void Start()
        {
            if (_cleavingStrike != null) _slots.Add(new AbilitySlot(_cleavingStrike));
            if (_warCry != null) _slots.Add(new AbilitySlot(_warCry));
            if (_ascendantBlade != null) _slots.Add(new AbilitySlot(_ascendantBlade));
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void Update()
        {
            foreach (var slot in _slots)
                slot.UpdateCooldown(Time.deltaTime);
        }

        public bool TryActivateAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return false;

            var hero = HeroManager.Instance?.GetPrimaryHero();
            if (hero == null || !hero.IsAlive) return false;

            return _slots[slotIndex].TryActivate(hero);
        }

        public AbilitySlot GetSlot(int index)
        {
            if (index >= 0 && index < _slots.Count) return _slots[index];
            return null;
        }

        public int SlotCount => _slots.Count;

        void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            // Build ultimate charge from dealing damage
            foreach (var slot in _slots)
            {
                if (slot.Data.isUltimate)
                {
                    slot.AddUltimateCharge(evt.Damage * _ultimateChargePerDamage);

                    EventBus.Publish(new UltimateChargeChangedEvent
                    {
                        HeroSlot = 0,
                        ChargePercent = slot.UltimateChargePercent
                    });
                }
            }
        }
    }
}
