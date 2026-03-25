using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Progression
{
    [System.Serializable]
    public class DemigodInfo
    {
        public string classId;
        public string buffDescription;
        public float buffValue;
    }

    public class DemigodSystem : MonoBehaviour
    {
        public static DemigodSystem Instance { get; private set; }

        // Retired demigods: classId -> info
        readonly Dictionary<string, DemigodInfo> _retiredDemigods = new();

        // Demigod buffs per class (defined by design doc)
        static readonly Dictionary<string, DemigodBuffDef> DemigodBuffs = new()
        {
            { "warrior",       new DemigodBuffDef("+5% All Physical Damage",       5f) },
            { "mage",          new DemigodBuffDef("+5% All Magical Damage",        5f) },
            { "priest",        new DemigodBuffDef("+5% All Healing",               5f) },
            { "rogue",         new DemigodBuffDef("+3% Critical Strike Chance",    3f) },
            { "marksman",      new DemigodBuffDef("+5% Damage to Bosses",          5f) },
            { "defender",      new DemigodBuffDef("+5% All Damage Reduction",      5f) },
            { "berserker",     new DemigodBuffDef("+3% Attack Speed",              3f) },
            { "druid",         new DemigodBuffDef("+3% HP Regeneration",           3f) },
            { "thief",         new DemigodBuffDef("+10% Gold Income",              10f) },
            { "shaman",        new DemigodBuffDef("+3% Ability Cooldown Reduction",3f) },
            { "warlock",       new DemigodBuffDef("+5% DoT Damage",               5f) },
            { "ranger",        new DemigodBuffDef("+5% Pet/Companion Damage",      5f) },
            { "spellblade",    new DemigodBuffDef("+3% Hybrid Damage",             3f) },
            { "necromancer",   new DemigodBuffDef("+5% Summon Damage",             5f) },
            { "monk",          new DemigodBuffDef("+3% Dodge Chance",              3f) },
            { "paladin",       new DemigodBuffDef("+3% Healing Done as Damage",    3f) },
            { "bard",          new DemigodBuffDef("+3% Party Buff Effectiveness",  3f) },
            { "dragonhunter",  new DemigodBuffDef("+8% Boss Damage",              8f) },
            { "summoner",      new DemigodBuffDef("+5% Familiar/Summon Stats",     5f) },
            { "alchemist",     new DemigodBuffDef("+5% Potion Effectiveness",      5f) },
            { "chronomancer",  new DemigodBuffDef("+3% Party CDR",                 3f) },
            { "gunslinger",    new DemigodBuffDef("+5% Ranged Attack Damage",      5f) },
            { "warden",        new DemigodBuffDef("+3% Max HP",                    3f) },
            { "reaper",        new DemigodBuffDef("+5% Damage to Low-HP Enemies",  5f) }
        };

        // Pantheon milestones
        static readonly PantheonMilestone[] Milestones =
        {
            new PantheonMilestone(6,  "Harmony of Elements",  "+10% damage on affinity-advantaged islands", 10f),
            new PantheonMilestone(12, "Ascendant's Blessing", "AFK rewards doubled",                        0f),
            new PantheonMilestone(18, "Mythic Awakening",     "All heroes start at Awakened tier",           0f),
            new PantheonMilestone(24, "Eternal Pantheon",     "+25% all stats, unlock Realm 4",              25f)
        };

        public const int MaxPantheonSlots = 24;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsClassRetired(string classId)
        {
            return _retiredDemigods.ContainsKey(classId);
        }

        public int PantheonSlotsFilled => _retiredDemigods.Count;

        public bool CanAttemptTranscendence(int heroSlot)
        {
            var hero = Party.PartyManager.Instance?.GetHero(heroSlot);
            if (hero?.Data == null) return false;

            string classId = hero.Data.classId;

            // Already retired?
            if (IsClassRetired(classId)) return false;

            // Needs 10 ascensions
            var tierSystem = TierBonusSystem.Instance;
            if (tierSystem == null) return false;
            return tierSystem.GetAscensionCount(heroSlot) >= 10;
        }

        public bool RetireDemigod(string classId)
        {
            if (IsClassRetired(classId)) return false;

            var buffDef = GetBuffDef(classId);
            var info = new DemigodInfo
            {
                classId = classId,
                buffDescription = buffDef.Description,
                buffValue = buffDef.Value
            };

            _retiredDemigods[classId] = info;

            int slotsFilled = _retiredDemigods.Count;

            EventBus.Publish(new DemigodRetiredEvent
            {
                ClassId = classId,
                DemigodBuffDescription = buffDef.Description,
                PantheonSlotsFilled = slotsFilled
            });

            // Check milestone
            CheckMilestones(slotsFilled);

            // Recalculate all hero stats (new global buff)
            RecalculateAllHeroStats();

            return true;
        }

        void CheckMilestones(int slotsFilled)
        {
            for (int i = 0; i < Milestones.Length; i++)
            {
                if (slotsFilled == Milestones[i].SlotsRequired)
                {
                    EventBus.Publish(new PantheonMilestoneEvent
                    {
                        SlotsFilled = slotsFilled,
                        MilestoneName = Milestones[i].Name,
                        Description = Milestones[i].Description
                    });
                }
            }
        }

        public float GetGlobalStatBonusPercent()
        {
            float total = 0f;

            // Milestone stat bonuses
            int filled = _retiredDemigods.Count;
            for (int i = 0; i < Milestones.Length; i++)
            {
                if (filled >= Milestones[i].SlotsRequired)
                    total += Milestones[i].StatBonusPercent;
            }

            return total;
        }

        public DemigodInfo GetDemigod(string classId)
        {
            return _retiredDemigods.TryGetValue(classId, out var info) ? info : null;
        }

        public IReadOnlyDictionary<string, DemigodInfo> AllDemigods => _retiredDemigods;

        public PantheonMilestone[] GetAllMilestones() => Milestones;

        public PantheonMilestone? GetNextMilestone()
        {
            int filled = _retiredDemigods.Count;
            for (int i = 0; i < Milestones.Length; i++)
            {
                if (filled < Milestones[i].SlotsRequired)
                    return Milestones[i];
            }
            return null;
        }

        public bool IsMilestoneReached(int slotsRequired)
        {
            return _retiredDemigods.Count >= slotsRequired;
        }

        static DemigodBuffDef GetBuffDef(string classId)
        {
            if (DemigodBuffs.TryGetValue(classId, out var buff))
                return buff;
            return new DemigodBuffDef("Unknown buff", 0f);
        }

        public List<DemigodSaveData> GatherSaveData()
        {
            var list = new List<DemigodSaveData>();
            foreach (var kvp in _retiredDemigods)
            {
                list.Add(new DemigodSaveData
                {
                    ClassId = kvp.Key,
                    BuffDescription = kvp.Value.buffDescription
                });
            }
            return list;
        }

        public void LoadSaveData(List<DemigodSaveData> data)
        {
            _retiredDemigods.Clear();
            if (data == null) return;
            foreach (var d in data)
            {
                var buffDef = GetBuffDef(d.ClassId);
                _retiredDemigods[d.ClassId] = new DemigodInfo
                {
                    classId = d.ClassId,
                    buffDescription = buffDef.Description,
                    buffValue = buffDef.Value
                };
            }
        }

        void RecalculateAllHeroStats()
        {
            var party = Party.PartyManager.Instance;
            if (party == null) return;
            var heroes = party.GetAllHeroes();
            for (int i = 0; i < heroes.Length; i++)
                heroes[i]?.RecalculateStats();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public struct DemigodBuffDef
    {
        public string Description;
        public float Value;

        public DemigodBuffDef(string desc, float val)
        {
            Description = desc;
            Value = val;
        }
    }

    public struct PantheonMilestone
    {
        public int SlotsRequired;
        public string Name;
        public string Description;
        public float StatBonusPercent;

        public PantheonMilestone(int slots, string name, string desc, float bonus)
        {
            SlotsRequired = slots;
            Name = name;
            Description = desc;
            StatBonusPercent = bonus;
        }
    }
}
