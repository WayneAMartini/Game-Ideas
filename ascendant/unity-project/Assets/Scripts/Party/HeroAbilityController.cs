using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Party
{
    public class HeroAbilityController : MonoBehaviour
    {
        [Header("Hero")]
        [SerializeField] int _heroSlot;

        [Header("Abilities (3 slots)")]
        [SerializeField] Ability _ability1;
        [SerializeField] Ability _ability2;
        [SerializeField] Ability _ultimate;

        [Header("Auto-Cast")]
        [SerializeField] bool _autoCastAbility1;
        [SerializeField] bool _autoCastAbility2;
        [SerializeField] float _autoCastEfficiency = 0.9f; // 90% effectiveness

        readonly List<AbilitySlot> _slots = new();
        float _ultimateChargePerDamage = 0.1f;

        public int HeroSlot => _heroSlot;
        public int SlotCount => _slots.Count;

        void Start()
        {
            if (_ability1 != null) _slots.Add(new AbilitySlot(_ability1));
            if (_ability2 != null) _slots.Add(new AbilitySlot(_ability2));
            if (_ultimate != null) _slots.Add(new AbilitySlot(_ultimate));
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
            // Get cooldown recovery multiplier (Mage passive)
            float cdMultiplier = 1f;
            var hero = GetHero();
            if (hero != null)
            {
                var mageMechanic = hero.GetComponent<MageTapMechanic>();
                if (mageMechanic != null)
                    cdMultiplier = mageMechanic.CooldownRecoveryMultiplier;
            }

            foreach (var slot in _slots)
                slot.UpdateCooldown(Time.deltaTime * cdMultiplier);

            ProcessAutoCast();
        }

        void ProcessAutoCast()
        {
            var hero = GetHero();
            if (hero == null || !hero.IsAlive) return;

            if (_autoCastAbility1 && _slots.Count > 0 && _slots[0].IsReady)
                _slots[0].TryActivate(hero);

            if (_autoCastAbility2 && _slots.Count > 1 && _slots[1].IsReady)
                _slots[1].TryActivate(hero);
        }

        public bool TryActivateAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return false;

            var hero = GetHero();
            if (hero == null || !hero.IsAlive) return false;

            return _slots[slotIndex].TryActivate(hero);
        }

        public AbilitySlot GetSlot(int index)
        {
            if (index >= 0 && index < _slots.Count) return _slots[index];
            return null;
        }

        public void ToggleAutoCast(int slotIndex, bool enabled)
        {
            if (slotIndex == 0) _autoCastAbility1 = enabled;
            else if (slotIndex == 1) _autoCastAbility2 = enabled;
        }

        Hero GetHero()
        {
            var partyManager = PartyManager.Instance;
            if (partyManager != null)
                return partyManager.GetHero(_heroSlot);

            return HeroManager.Instance?.GetHero(_heroSlot);
        }

        void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            // Build ultimate charge from any damage dealt
            foreach (var slot in _slots)
            {
                if (slot.Data.isUltimate)
                {
                    slot.AddUltimateCharge(evt.Damage * _ultimateChargePerDamage);

                    EventBus.Publish(new UltimateChargeChangedEvent
                    {
                        HeroSlot = _heroSlot,
                        ChargePercent = slot.UltimateChargePercent
                    });
                }
            }
        }
    }
}
