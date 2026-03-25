using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Party
{
    public class ComboSystem : MonoBehaviour
    {
        public static ComboSystem Instance { get; private set; }

        [Header("Combo Definitions")]
        [SerializeField] ComboAbilityData[] _allCombos;

        [Header("Config")]
        [SerializeField] float _comboWindow = 3f;

        // Track recent ability uses: classId -> timestamp
        readonly Dictionary<string, float> _recentAbilityUses = new();

        // Track which combos have been discovered
        readonly HashSet<string> _discoveredCombos = new();

        // Track which combo pairs are currently in the party (for "?" indicator)
        readonly HashSet<string> _potentialCombos = new();

        public IReadOnlyCollection<string> DiscoveredCombos => _discoveredCombos;

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
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void OnPartyChanged(PartyChangedEvent evt)
        {
            DetectPotentialCombos();
        }

        void DetectPotentialCombos()
        {
            _potentialCombos.Clear();

            var pm = PartyManager.Instance;
            if (pm == null || _allCombos == null) return;

            // Collect class IDs of current party
            var classIds = new HashSet<string>();
            foreach (var hero in pm.GetAllHeroes())
            {
                if (hero != null && hero.Data != null)
                    classIds.Add(hero.Data.classId);
            }

            // Check which combos are possible
            foreach (var combo in _allCombos)
            {
                if (classIds.Contains(combo.classIdA) && classIds.Contains(combo.classIdB))
                    _potentialCombos.Add(combo.comboId);
            }
        }

        void OnAbilityUsed(AbilityUsedEvent evt)
        {
            var pm = PartyManager.Instance;
            if (pm == null) return;

            var hero = pm.GetHero(evt.HeroSlot);
            if (hero == null || hero.Data == null) return;

            string classId = hero.Data.classId;
            float now = Time.time;

            _recentAbilityUses[classId] = now;

            // Check for combo triggers
            CheckForCombo(classId, hero, now);
        }

        void CheckForCombo(string triggeringClassId, Hero triggeringHero, float now)
        {
            if (_allCombos == null) return;

            foreach (var combo in _allCombos)
            {
                string partnerClassId = null;

                if (combo.classIdA == triggeringClassId)
                    partnerClassId = combo.classIdB;
                else if (combo.classIdB == triggeringClassId)
                    partnerClassId = combo.classIdA;
                else
                    continue;

                // Check if partner used ability within window
                if (!_recentAbilityUses.TryGetValue(partnerClassId, out float partnerTime))
                    continue;

                if (now - partnerTime > _comboWindow)
                    continue;

                // Combo triggered!
                ExecuteCombo(combo, triggeringHero);

                // Clear to prevent re-triggering
                _recentAbilityUses.Remove(partnerClassId);
                _recentAbilityUses.Remove(triggeringClassId);

                // Track discovery
                if (_discoveredCombos.Add(combo.comboId))
                {
                    EventBus.Publish(new ComboDiscoveredEvent
                    {
                        ComboId = combo.comboId,
                        ComboName = combo.comboName
                    });
                }

                break; // One combo per ability use
            }
        }

        void ExecuteCombo(ComboAbilityData combo, Hero triggeringHero)
        {
            float baseDamage = triggeringHero.CurrentAtk * combo.damageMultiplier;
            float totalDamage = 0f;

            switch (combo.targetType)
            {
                case ComboTargetType.AllEnemies:
                {
                    var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
                    if (enemies != null)
                    {
                        foreach (var enemy in enemies)
                        {
                            enemy.TakeDamage(baseDamage, combo.damageType);
                            totalDamage += baseDamage;

                            EventBus.Publish(new EnemyDamagedEvent
                            {
                                EnemyId = enemy.Id,
                                Damage = baseDamage,
                                IsCritical = true,
                                IsAoE = true,
                                WorldPosition = enemy.transform.position
                            });
                        }
                    }
                    break;
                }

                case ComboTargetType.SingleEnemy:
                {
                    var target = EnemyManager.Instance?.GetNearestEnemy(triggeringHero.transform.position);
                    if (target != null)
                    {
                        target.TakeDamage(baseDamage, combo.damageType);
                        totalDamage = baseDamage;

                        EventBus.Publish(new EnemyDamagedEvent
                        {
                            EnemyId = target.Id,
                            Damage = baseDamage,
                            IsCritical = true,
                            IsAoE = false,
                            WorldPosition = target.transform.position
                        });
                    }
                    break;
                }

                case ComboTargetType.PartyBuff:
                {
                    // Apply heal if specified
                    if (combo.healPercent > 0f)
                    {
                        var pm = PartyManager.Instance;
                        foreach (var hero in pm.GetAllAliveHeroes())
                        {
                            float healAmount = hero.MaxHp * combo.healPercent;
                            hero.Heal(healAmount);
                        }
                    }
                    break;
                }

                case ComboTargetType.Hybrid:
                {
                    // Damage all enemies AND heal party
                    var enemies = EnemyManager.Instance?.GetAllAliveEnemies();
                    if (enemies != null)
                    {
                        foreach (var enemy in enemies)
                        {
                            enemy.TakeDamage(baseDamage, combo.damageType);
                            totalDamage += baseDamage;

                            EventBus.Publish(new EnemyDamagedEvent
                            {
                                EnemyId = enemy.Id,
                                Damage = baseDamage,
                                IsCritical = true,
                                IsAoE = true,
                                WorldPosition = enemy.transform.position
                            });
                        }
                    }

                    if (combo.healPercent > 0f)
                    {
                        var pm = PartyManager.Instance;
                        foreach (var hero in pm.GetAllAliveHeroes())
                        {
                            float healAmount = totalDamage * combo.healPercent;
                            hero.Heal(healAmount);
                        }
                    }
                    break;
                }
            }

            // Find partner hero slot
            int partnerSlot = -1;
            var partyMgr = PartyManager.Instance;
            if (partyMgr != null)
            {
                foreach (var hero in partyMgr.GetAllHeroes())
                {
                    if (hero != null && hero.Data != null && hero.Slot != triggeringHero.Slot)
                    {
                        if (hero.Data.classId == combo.classIdA || hero.Data.classId == combo.classIdB)
                        {
                            partnerSlot = hero.Slot;
                            break;
                        }
                    }
                }
            }

            EventBus.Publish(new ComboAbilityTriggeredEvent
            {
                ComboName = combo.comboName,
                HeroSlotA = triggeringHero.Slot,
                HeroSlotB = partnerSlot,
                Damage = totalDamage
            });
        }

        // UI query: is there an undiscovered combo possible with current party?
        public bool HasUndiscoveredCombo()
        {
            foreach (var comboId in _potentialCombos)
            {
                if (!_discoveredCombos.Contains(comboId))
                    return true;
            }
            return false;
        }

        public bool IsComboDiscovered(string comboId) => _discoveredCombos.Contains(comboId);

        public bool IsComboPossible(string comboId) => _potentialCombos.Contains(comboId);

        // Save/load discovered combos
        public string[] GetDiscoveredComboIds()
        {
            var arr = new string[_discoveredCombos.Count];
            _discoveredCombos.CopyTo(arr);
            return arr;
        }

        public void LoadDiscoveredCombos(string[] comboIds)
        {
            _discoveredCombos.Clear();
            if (comboIds != null)
            {
                foreach (var id in comboIds)
                    _discoveredCombos.Add(id);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
