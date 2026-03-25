using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Progression
{
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance { get; private set; }

        [Header("Config")]
        [SerializeField] int _maxInventorySize = 200;

        readonly List<Equipment> _inventory = new();

        // Per-hero equipment: heroSlot -> slotType -> equipment
        readonly Dictionary<int, Dictionary<EquipmentSlot, Equipment>> _equipped = new();

        public IReadOnlyList<Equipment> Inventory => _inventory;

        // Enhancement cost: goldBase * (1 + level)^2
        const float EnhanceGoldBase = 100f;
        const int MaxEnhanceLevel = 15;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            // Roll for equipment drop
            int stage = StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1;
            TryDropEquipment(stage);
        }

        void TryDropEquipment(int stage)
        {
            // Base 10% chance to drop equipment per kill
            if (Random.value > 0.10f) return;
            if (_inventory.Count >= _maxInventorySize) return;

            var rarity = RollRarity();
            var equipment = Equipment.Generate(stage, rarity);
            _inventory.Add(equipment);

            EventBus.Publish(new EquipmentDropEvent
            {
                Rarity = rarity,
                EquipmentName = equipment.equipmentName
            });
        }

        public static EquipmentRarity RollRarity()
        {
            float roll = Random.value;
            float cumulative = 0f;

            // Order: Mythic (0.1%), Legendary (0.9%), Epic (4%), Rare (10%), Uncommon (25%), Common (60%)
            cumulative += 0.001f;
            if (roll < cumulative) return EquipmentRarity.Mythic;
            cumulative += 0.009f;
            if (roll < cumulative) return EquipmentRarity.Legendary;
            cumulative += 0.04f;
            if (roll < cumulative) return EquipmentRarity.Epic;
            cumulative += 0.10f;
            if (roll < cumulative) return EquipmentRarity.Rare;
            cumulative += 0.25f;
            if (roll < cumulative) return EquipmentRarity.Uncommon;

            return EquipmentRarity.Common;
        }

        public bool Equip(int heroSlot, Equipment equipment)
        {
            if (equipment == null) return false;

            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero == null) return false;
            if (hero.Level < equipment.levelRequirement) return false;
            if (!string.IsNullOrEmpty(equipment.classRestriction) &&
                hero.Data != null && hero.Data.classId != equipment.classRestriction)
                return false;

            if (!_equipped.ContainsKey(heroSlot))
                _equipped[heroSlot] = new Dictionary<EquipmentSlot, Equipment>();

            // Unequip current item in that slot
            if (_equipped[heroSlot].TryGetValue(equipment.slot, out var current))
            {
                current.equippedHeroSlot = -1;
                if (!_inventory.Contains(current))
                    _inventory.Add(current);
            }

            // Equip new item
            _equipped[heroSlot][equipment.slot] = equipment;
            equipment.equippedHeroSlot = heroSlot;
            _inventory.Remove(equipment);

            hero.RecalculateStats();

            EventBus.Publish(new EquipmentChangedEvent
            {
                HeroSlot = heroSlot,
                Slot = equipment.slot
            });

            return true;
        }

        public bool Unequip(int heroSlot, EquipmentSlot slot)
        {
            if (!_equipped.ContainsKey(heroSlot)) return false;
            if (!_equipped[heroSlot].TryGetValue(slot, out var equipment)) return false;
            if (_inventory.Count >= _maxInventorySize) return false;

            _equipped[heroSlot].Remove(slot);
            equipment.equippedHeroSlot = -1;
            _inventory.Add(equipment);

            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            hero?.RecalculateStats();

            EventBus.Publish(new EquipmentChangedEvent
            {
                HeroSlot = heroSlot,
                Slot = slot
            });

            return true;
        }

        public Equipment GetEquipped(int heroSlot, EquipmentSlot slot)
        {
            if (_equipped.TryGetValue(heroSlot, out var slots))
                if (slots.TryGetValue(slot, out var eq))
                    return eq;
            return null;
        }

        public Dictionary<EquipmentSlot, Equipment> GetAllEquipped(int heroSlot)
        {
            if (_equipped.TryGetValue(heroSlot, out var slots))
                return slots;
            return null;
        }

        // Get total stat bonus from all equipped items for a hero
        public float GetEquipmentStatBonus(int heroSlot, StatType stat)
        {
            if (!_equipped.TryGetValue(heroSlot, out var slots)) return 0f;

            float total = 0f;
            foreach (var kvp in slots)
                total += kvp.Value.GetTotalStat(stat);
            return total;
        }

        // Enhancement
        public float GetEnhanceCost(int currentLevel)
        {
            return EnhanceGoldBase * Mathf.Pow(1 + currentLevel, 2f);
        }

        public bool TryEnhance(Equipment equipment)
        {
            if (equipment == null || equipment.enhanceLevel >= MaxEnhanceLevel) return false;

            float cost = GetEnhanceCost(equipment.enhanceLevel);
            var currency = Economy.CurrencyManager.Instance;
            if (currency == null || !currency.SpendGold(cost)) return false;

            equipment.enhanceLevel++;

            // Recalculate hero stats if equipped
            if (equipment.equippedHeroSlot >= 0)
            {
                var hero = Party.PartyManager.Instance?.GetHero(equipment.equippedHeroSlot);
                hero?.RecalculateStats();
            }

            EventBus.Publish(new EquipmentEnhancedEvent
            {
                EquipmentId = equipment.id,
                NewEnhanceLevel = equipment.enhanceLevel
            });

            return true;
        }

        public void RemoveFromInventory(Equipment equipment)
        {
            if (equipment == null) return;
            _inventory.Remove(equipment);
        }

        // Sort helpers
        public void SortByPower()
        {
            _inventory.Sort((a, b) => b.GetPowerScore().CompareTo(a.GetPowerScore()));
        }

        public void SortByRarity()
        {
            _inventory.Sort((a, b) => ((int)b.rarity).CompareTo((int)a.rarity));
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
