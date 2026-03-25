#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ascendant.Progression;

namespace Ascendant.Editor
{
    public static class Phase4DataCreator
    {
        [MenuItem("Ascendant/Create Phase 4 Data")]
        public static void CreateAll()
        {
            CreateXPCurve();
            CreateGrowthRates();
            CreateSkillTrees();
            CreateClassMasteryData();
            CreateSampleEquipment();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase4DataCreator] All Phase 4 data created.");
        }

        static void CreateXPCurve()
        {
            var xp = ScriptableObject.CreateInstance<XPCurve>();
            xp.baseXP = 100f;
            xp.exponent = 1.5f;
            xp.levelCap = 100;
            xp.milestoneLevels = new[] { 10, 25, 50, 75, 100 };
            CreateAsset(xp, "Assets/Data/Progression/XPCurveConfig.asset");
        }

        static void CreateGrowthRates()
        {
            // Warrior: ATK++, DEF++, HP++, SPD+
            CreateGrowthRate("warrior", Heroes.StatGrowth.Medium, Heroes.StatGrowth.Medium, Heroes.StatGrowth.Medium, Heroes.StatGrowth.Low);
            // Mage: ATK+++, DEF+, HP+, SPD++
            CreateGrowthRate("mage", Heroes.StatGrowth.High, Heroes.StatGrowth.Low, Heroes.StatGrowth.Low, Heroes.StatGrowth.Medium);
            // Priest: ATK+, DEF++, HP++, SPD++
            CreateGrowthRate("priest", Heroes.StatGrowth.Low, Heroes.StatGrowth.Medium, Heroes.StatGrowth.Medium, Heroes.StatGrowth.Medium);
            // Rogue: ATK+++, DEF+, HP+, SPD+++
            CreateGrowthRate("rogue", Heroes.StatGrowth.High, Heroes.StatGrowth.Low, Heroes.StatGrowth.Low, Heroes.StatGrowth.High);
        }

        static void CreateGrowthRate(string classId, Heroes.StatGrowth atk, Heroes.StatGrowth def, Heroes.StatGrowth hp, Heroes.StatGrowth spd)
        {
            var gr = ScriptableObject.CreateInstance<ClassGrowthRates>();
            gr.classId = classId;
            gr.atkGrowth = atk;
            gr.defGrowth = def;
            gr.hpGrowth = hp;
            gr.spdGrowth = spd;
            string cap = char.ToUpper(classId[0]) + classId[1..];
            CreateAsset(gr, $"Assets/Data/Progression/{cap}GrowthRates.asset");
        }

        // --- SKILL TREES ---

        static void CreateSkillTrees()
        {
            CreateWarriorSkillTree();
            CreateMageSkillTree();
            CreatePriestSkillTree();
            CreateRogueSkillTree();
        }

        static void CreateWarriorSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "warrior";
            tree.className = "Warrior";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("warrior_blade", "Blade", "Raw damage, cleave radius, critical strike chance.", new SkillNodeDefinition[]
                {
                    Node("wb1", "Sharpened Edge", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("wb2", "Keen Strikes", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"wb1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("wb3", "Blade Mastery", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wb2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("wb4", "Wide Cleave", "Cleaving Strike +25% radius", SkillNodeType.AbilityModifier, 2, 15, new[]{"wb3"}, Stat(StatType.ATK, 5), new Vector2(0,3), modAbility: "cleaving_strike"),
                    Node("wb5", "Ruthless Strikes", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"wb3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("wb6", "Lethal Precision", "Crit Rate +5%", SkillNodeType.PassiveStat, 2, 30, new[]{"wb4","wb5"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,4)),
                    Node("wb7", "Devastating Blow", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"wb6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("wb8", "Whirlwind", "ATK +20, unlock Whirlwind passive", SkillNodeType.PassiveStat, 2, 50, new[]{"wb7"}, Stat(StatType.ATK, 20), new Vector2(0,6)),
                    Node("wb9", "Sword Saint", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"wb8"}, Stats(new StatRoll[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warrior_shield", "Shield", "Defense, counterattack chance, taunt duration.", new SkillNodeDefinition[]
                {
                    Node("ws1", "Iron Skin", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("ws2", "Bulwark", "HP +50", SkillNodeType.PassiveStat, 1, 5, new[]{"ws1"}, Stat(StatType.HP, 50), new Vector2(0,1)),
                    Node("ws3", "Shield Wall", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"ws2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("ws4", "Counter Strike", "Chance to counter-attack on block", SkillNodeType.AbilityModifier, 2, 15, new[]{"ws3"}, Stat(StatType.DEF, 5), new Vector2(0,3)),
                    Node("ws5", "Fortified", "HP +100", SkillNodeType.PassiveStat, 1, 20, new[]{"ws3"}, Stat(StatType.HP, 100), new Vector2(1,3)),
                    Node("ws6", "Unyielding", "DEF +15, Dodge +3%", SkillNodeType.PassiveStat, 2, 30, new[]{"ws4","ws5"}, Stats(new[]{Stat(StatType.DEF, 15), Stat(StatType.DodgeChance, 0.03f)}), new Vector2(0,4)),
                    Node("ws7", "Taunt Mastery", "War Cry taunt duration +3s", SkillNodeType.AbilityModifier, 1, 40, new[]{"ws6"}, Stat(StatType.DEF, 8), new Vector2(0,5), modAbility: "war_cry"),
                    Node("ws8", "Living Fortress", "HP +200, DEF +10", SkillNodeType.PassiveStat, 2, 50, new[]{"ws7"}, Stats(new[]{Stat(StatType.HP, 200), Stat(StatType.DEF, 10)}), new Vector2(0,6)),
                    Node("ws9", "Immortal Guard", "DEF +25, Life Steal +5%", SkillNodeType.Capstone, 3, 75, new[]{"ws8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.LifeSteal, 0.05f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warrior_commander", "Commander", "Party-wide buffs, War Cry enhancements, leadership auras.", new SkillNodeDefinition[]
                {
                    Node("wc1", "Inspiring Presence", "Party ATK +2%", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 3), new Vector2(0,0)),
                    Node("wc2", "Rally Cry", "War Cry ATK bonus +5%", SkillNodeType.AbilityModifier, 1, 5, new[]{"wc1"}, Stat(StatType.ATK, 3), new Vector2(0,1), modAbility: "war_cry"),
                    Node("wc3", "Leadership", "Party DEF +5", SkillNodeType.PassiveStat, 1, 10, new[]{"wc2"}, Stat(StatType.DEF, 5), new Vector2(0,2)),
                    Node("wc4", "Battle Standard", "Party HP +50", SkillNodeType.PassiveStat, 2, 15, new[]{"wc3"}, Stat(StatType.HP, 50), new Vector2(0,3)),
                    Node("wc5", "Tactical Mind", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"wc3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("wc6", "Bolstering Shout", "War Cry also heals party 5% HP", SkillNodeType.AbilityModifier, 2, 30, new[]{"wc4","wc5"}, Stat(StatType.HP, 75), new Vector2(0,4), modAbility: "war_cry"),
                    Node("wc7", "Commanding Aura", "Party ATK +10", SkillNodeType.PassiveStat, 1, 40, new[]{"wc6"}, Stat(StatType.ATK, 10), new Vector2(0,5)),
                    Node("wc8", "Undying Will", "Party Dodge +3%", SkillNodeType.PassiveStat, 2, 50, new[]{"wc7"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,6)),
                    Node("wc9", "Warlord", "ATK +20, DEF +20, party-wide stat aura", SkillNodeType.Capstone, 3, 75, new[]{"wc8"}, Stats(new[]{Stat(StatType.ATK, 20), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/WarriorSkillTree.asset");
        }

        static void CreateMageSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "mage";
            tree.className = "Mage";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("mage_frost", "Frost", "Freeze duration, shatter damage, AoE radius.", new SkillNodeDefinition[]
                {
                    Node("mf1", "Chilling Touch", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("mf2", "Frost Bite", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"mf1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("mf3", "Deep Freeze", "Frost Nova duration +1s", SkillNodeType.AbilityModifier, 1, 10, new[]{"mf2"}, Stat(StatType.ATK, 5), new Vector2(0,2), modAbility: "frost_nova"),
                    Node("mf4", "Shatter", "Bonus damage to frozen targets", SkillNodeType.PassiveStat, 2, 15, new[]{"mf3"}, Stat(StatType.ATK, 12), new Vector2(0,3)),
                    Node("mf5", "Ice Barrier", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"mf3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("mf6", "Glacial Mastery", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"mf4","mf5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("mf7", "Permafrost", "DEF +12, HP +80", SkillNodeType.PassiveStat, 1, 40, new[]{"mf6"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 80)}), new Vector2(0,5)),
                    Node("mf8", "Avalanche", "Blizzard damage +50%", SkillNodeType.AbilityModifier, 2, 50, new[]{"mf7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "blizzard"),
                    Node("mf9", "Absolute Mastery", "ATK +30, freeze all on Absolute Zero", SkillNodeType.Capstone, 3, 75, new[]{"mf8"}, Stat(StatType.ATK, 30), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("mage_arcane", "Arcane", "Raw spell damage, missile count, mana efficiency.", new SkillNodeDefinition[]
                {
                    Node("ma1", "Arcane Power", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("ma2", "Focused Mind", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"ma1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("ma3", "Split Bolt", "Extra missile at high Momentum", SkillNodeType.AbilityModifier, 1, 10, new[]{"ma2"}, Stat(StatType.ATK, 5), new Vector2(0,2)),
                    Node("ma4", "Arcane Infusion", "ATK +12", SkillNodeType.PassiveStat, 2, 15, new[]{"ma3"}, Stat(StatType.ATK, 12), new Vector2(0,3)),
                    Node("ma5", "Spell Haste", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"ma3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("ma6", "Overcharge", "Crit Rate +5%", SkillNodeType.PassiveStat, 2, 30, new[]{"ma4","ma5"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,4)),
                    Node("ma7", "Arcane Barrage", "ATK +18", SkillNodeType.PassiveStat, 1, 40, new[]{"ma6"}, Stat(StatType.ATK, 18), new Vector2(0,5)),
                    Node("ma8", "Spell Mastery", "Crit Damage +20%", SkillNodeType.PassiveStat, 2, 50, new[]{"ma7"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,6)),
                    Node("ma9", "Archmage", "ATK +35, all spells +25% damage", SkillNodeType.Capstone, 3, 75, new[]{"ma8"}, Stat(StatType.ATK, 35), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("mage_ether", "Ether", "Mana regeneration, party magic buff, spell resistance.", new SkillNodeDefinition[]
                {
                    Node("me1", "Mana Well", "HP +30", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 30), new Vector2(0,0)),
                    Node("me2", "Arcane Shield", "DEF +5", SkillNodeType.PassiveStat, 1, 5, new[]{"me1"}, Stat(StatType.DEF, 5), new Vector2(0,1)),
                    Node("me3", "Ether Flow", "SPD +0.03", SkillNodeType.PassiveStat, 1, 10, new[]{"me2"}, Stat(StatType.SPD, 0.03f), new Vector2(0,2)),
                    Node("me4", "Spell Resistance", "DEF +10", SkillNodeType.PassiveStat, 2, 15, new[]{"me3"}, Stat(StatType.DEF, 10), new Vector2(0,3)),
                    Node("me5", "Mana Surge Boost", "Mana Surge threshold -10%", SkillNodeType.AbilityModifier, 1, 20, new[]{"me3"}, Stat(StatType.ATK, 8), new Vector2(1,3)),
                    Node("me6", "Ethereal Ward", "HP +100, DEF +8", SkillNodeType.PassiveStat, 2, 30, new[]{"me4","me5"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.DEF, 8)}), new Vector2(0,4)),
                    Node("me7", "Mana Font", "Party magic damage aura", SkillNodeType.PassiveStat, 1, 40, new[]{"me6"}, Stat(StatType.ATK, 10), new Vector2(0,5)),
                    Node("me8", "Astral Form", "Dodge +5%", SkillNodeType.PassiveStat, 2, 50, new[]{"me7"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,6)),
                    Node("me9", "Ether Lord", "ATK +20, DEF +20, spell aura", SkillNodeType.Capstone, 3, 75, new[]{"me8"}, Stats(new[]{Stat(StatType.ATK, 20), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/MageSkillTree.asset");
        }

        static void CreatePriestSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "priest";
            tree.className = "Priest";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("priest_restoration", "Restoration", "Heal power, HoT effects, overheal shields.", new SkillNodeDefinition[]
                {
                    Node("pr1", "Gentle Touch", "HP +40", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 40), new Vector2(0,0)),
                    Node("pr2", "Healing Hands", "ATK +5 (heal power)", SkillNodeType.PassiveStat, 1, 5, new[]{"pr1"}, Stat(StatType.ATK, 5), new Vector2(0,1)),
                    Node("pr3", "Renewal", "Healing Light +3% HP", SkillNodeType.AbilityModifier, 1, 10, new[]{"pr2"}, Stat(StatType.ATK, 5), new Vector2(0,2), modAbility: "healing_light"),
                    Node("pr4", "Rejuvenation", "Heal over Time effect", SkillNodeType.PassiveStat, 2, 15, new[]{"pr3"}, Stat(StatType.HP, 80), new Vector2(0,3)),
                    Node("pr5", "Vitality", "HP +100", SkillNodeType.PassiveStat, 1, 20, new[]{"pr3"}, Stat(StatType.HP, 100), new Vector2(1,3)),
                    Node("pr6", "Overheal Shield", "Excess healing becomes shield", SkillNodeType.AbilityModifier, 2, 30, new[]{"pr4","pr5"}, Stat(StatType.HP, 60), new Vector2(0,4)),
                    Node("pr7", "Divine Favor", "ATK +12 (heal power)", SkillNodeType.PassiveStat, 1, 40, new[]{"pr6"}, Stat(StatType.ATK, 12), new Vector2(0,5)),
                    Node("pr8", "Miracle", "Resurrection heals to 75% HP", SkillNodeType.AbilityModifier, 2, 50, new[]{"pr7"}, Stat(StatType.HP, 150), new Vector2(0,6), modAbility: "resurrection"),
                    Node("pr9", "Archpriest", "HP +300, party heal aura", SkillNodeType.Capstone, 3, 75, new[]{"pr8"}, Stat(StatType.HP, 300), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("priest_protection", "Protection", "Shield strength, damage reduction, cleanse.", new SkillNodeDefinition[]
                {
                    Node("pp1", "Faith Shield", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("pp2", "Sacred Armor", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"pp1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("pp3", "Reinforced Shield", "Sacred Shield +10% strength", SkillNodeType.AbilityModifier, 1, 10, new[]{"pp2"}, Stat(StatType.DEF, 5), new Vector2(0,2), modAbility: "sacred_shield"),
                    Node("pp4", "Divine Protection", "Party damage reduction +3%", SkillNodeType.PassiveStat, 2, 15, new[]{"pp3"}, Stat(StatType.DEF, 10), new Vector2(0,3)),
                    Node("pp5", "Purify", "Cleanse debuffs on heal", SkillNodeType.AbilityModifier, 1, 20, new[]{"pp3"}, Stat(StatType.HP, 60), new Vector2(1,3)),
                    Node("pp6", "Aegis", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"pp4","pp5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("pp7", "Ward of Light", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"pp6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("pp8", "Bastion", "DEF +20, Dodge +3%", SkillNodeType.PassiveStat, 2, 50, new[]{"pp7"}, Stats(new[]{Stat(StatType.DEF, 20), Stat(StatType.DodgeChance, 0.03f)}), new Vector2(0,6)),
                    Node("pp9", "Divine Bulwark", "DEF +30, party shield on Resurrection", SkillNodeType.Capstone, 3, 75, new[]{"pp8"}, Stat(StatType.DEF, 30), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("priest_wrath", "Wrath", "Holy damage, bonus vs Shadow/Undead, smite.", new SkillNodeDefinition[]
                {
                    Node("pw1", "Smite", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("pw2", "Holy Fire", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"pw1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("pw3", "Righteous Fury", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 10, new[]{"pw2"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,2)),
                    Node("pw4", "Holy Wrath", "ATK +12", SkillNodeType.PassiveStat, 2, 15, new[]{"pw3"}, Stat(StatType.ATK, 12), new Vector2(0,3)),
                    Node("pw5", "Consecration", "AoE holy damage zone", SkillNodeType.AbilityModifier, 1, 20, new[]{"pw3"}, Stat(StatType.ATK, 8), new Vector2(1,3)),
                    Node("pw6", "Exorcism", "Bonus damage vs Shadow", SkillNodeType.PassiveStat, 2, 30, new[]{"pw4","pw5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("pw7", "Judgment", "ATK +18", SkillNodeType.PassiveStat, 1, 40, new[]{"pw6"}, Stat(StatType.ATK, 18), new Vector2(0,5)),
                    Node("pw8", "Divine Storm", "Crit Damage +20%", SkillNodeType.PassiveStat, 2, 50, new[]{"pw7"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,6)),
                    Node("pw9", "Avatar of Wrath", "ATK +35, holy AoE on ult", SkillNodeType.Capstone, 3, 75, new[]{"pw8"}, Stat(StatType.ATK, 35), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/PriestSkillTree.asset");
        }

        static void CreateRogueSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "rogue";
            tree.className = "Rogue";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("rogue_assassination", "Assassination", "Finishing Blow damage, crit rate, crit damage.", new SkillNodeDefinition[]
                {
                    Node("ra1", "Sharp Blades", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("ra2", "Precision", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"ra1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("ra3", "Exploit Weakness", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 10, new[]{"ra2"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,2)),
                    Node("ra4", "Deadly Finisher", "Finishing Blow +50% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"ra3"}, Stat(StatType.ATK, 10), new Vector2(0,3)),
                    Node("ra5", "Assassin's Mark", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"ra3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("ra6", "Execute", "Extra damage below 30% HP", SkillNodeType.PassiveStat, 2, 30, new[]{"ra4","ra5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("ra7", "Lethality", "ATK +18", SkillNodeType.PassiveStat, 1, 40, new[]{"ra6"}, Stat(StatType.ATK, 18), new Vector2(0,5)),
                    Node("ra8", "Cold Blood", "Crit Damage +25%", SkillNodeType.PassiveStat, 2, 50, new[]{"ra7"}, Stat(StatType.CritDamage, 0.25f), new Vector2(0,6)),
                    Node("ra9", "Death's Whisper", "ATK +30, guaranteed crit on Finishing Blow", SkillNodeType.Capstone, 3, 75, new[]{"ra8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("rogue_subtlety", "Subtlety", "Dodge chance, movement speed, survivability.", new SkillNodeDefinition[]
                {
                    Node("rs1", "Quick Reflexes", "Dodge +3%", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,0)),
                    Node("rs2", "Shadow Step", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"rs1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("rs3", "Evasion", "Dodge +4%", SkillNodeType.PassiveStat, 1, 10, new[]{"rs2"}, Stat(StatType.DodgeChance, 0.04f), new Vector2(0,2)),
                    Node("rs4", "Smoke Screen", "Smoke Bomb +2s duration", SkillNodeType.AbilityModifier, 2, 15, new[]{"rs3"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,3), modAbility: "smoke_bomb"),
                    Node("rs5", "Fleet Footed", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"rs3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("rs6", "Ghostly", "Dodge +5%", SkillNodeType.PassiveStat, 2, 30, new[]{"rs4","rs5"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,4)),
                    Node("rs7", "Shadow Meld", "HP +80", SkillNodeType.PassiveStat, 1, 40, new[]{"rs6"}, Stat(StatType.HP, 80), new Vector2(0,5)),
                    Node("rs8", "Phantom", "Dodge +5%, SPD +0.05", SkillNodeType.PassiveStat, 2, 50, new[]{"rs7"}, Stats(new[]{Stat(StatType.DodgeChance, 0.05f), Stat(StatType.SPD, 0.05f)}), new Vector2(0,6)),
                    Node("rs9", "Shadow Lord", "Dodge +10%, untargetable 2s after dodge", SkillNodeType.Capstone, 3, 75, new[]{"rs8"}, Stat(StatType.DodgeChance, 0.10f), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("rogue_venom", "Venom", "Poison DoTs, weakening debuffs, AoE poison.", new SkillNodeDefinition[]
                {
                    Node("rv1", "Toxic Blade", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("rv2", "Poison Coating", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"rv1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("rv3", "Noxious Strikes", "Attacks apply DoT", SkillNodeType.AbilityModifier, 1, 10, new[]{"rv2"}, Stat(StatType.ATK, 6), new Vector2(0,2)),
                    Node("rv4", "Crippling Poison", "Enemy ATK debuff on hit", SkillNodeType.PassiveStat, 2, 15, new[]{"rv3"}, Stat(StatType.ATK, 10), new Vector2(0,3)),
                    Node("rv5", "Venomous Glands", "ATK +10", SkillNodeType.PassiveStat, 1, 20, new[]{"rv3"}, Stat(StatType.ATK, 10), new Vector2(1,3)),
                    Node("rv6", "Spreading Poison", "DoT spreads to nearby enemies", SkillNodeType.AbilityModifier, 2, 30, new[]{"rv4","rv5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("rv7", "Lethal Dose", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"rv6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("rv8", "Plague Cloud", "AoE poison on Smoke Bomb", SkillNodeType.AbilityModifier, 2, 50, new[]{"rv7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "smoke_bomb"),
                    Node("rv9", "Venom Lord", "ATK +30, all attacks poison", SkillNodeType.Capstone, 3, 75, new[]{"rv8"}, Stat(StatType.ATK, 30), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/RogueSkillTree.asset");
        }

        // --- CLASS MASTERY ---

        static void CreateClassMasteryData()
        {
            CreateMastery("warrior", "Warrior");
            CreateMastery("mage", "Mage");
            CreateMastery("priest", "Priest");
            CreateMastery("rogue", "Rogue");
        }

        static void CreateMastery(string classId, string className)
        {
            var data = ScriptableObject.CreateInstance<ClassMasteryData>();
            data.classId = classId;
            data.className = className;
            data.tierRewards = new MasteryTierReward[]
            {
                new() { tier = MasteryTier.Novice, stageThreshold = 0, statBonusPercent = 0f, rewardDescription = $"{className} unlocked" },
                new() { tier = MasteryTier.Apprentice, stageThreshold = 100, statBonusPercent = 5f, rewardDescription = $"+5% {className} stat bonus" },
                new() { tier = MasteryTier.Journeyman, stageThreshold = 500, statBonusPercent = 8f, unlocksAlternateAbility = true, rewardDescription = "Alternate ability variant" },
                new() { tier = MasteryTier.Expert, stageThreshold = 2000, statBonusPercent = 10f, unlocksCosmeticSkin = true, rewardDescription = $"{className} skin + 10% stat" },
                new() { tier = MasteryTier.Master, stageThreshold = 5000, statBonusPercent = 15f, unlocksPassiveAbility = true, rewardDescription = "Mastery passive ability" },
                new() { tier = MasteryTier.Grandmaster, stageThreshold = 10000, statBonusPercent = 20f, unlocksCosmeticSkin = true, rewardDescription = $"Ultimate {className} cosmetic + title" },
            };
            CreateAsset(data, $"Assets/Data/Mastery/{className}MasteryData.asset");
        }

        // --- SAMPLE EQUIPMENT ---

        static void CreateSampleEquipment()
        {
            CreateEquip("Iron Sword", EquipmentSlot.Weapon, EquipmentRarity.Common, StatType.ATK, 10f);
            CreateEquip("Sturdy Shield", EquipmentSlot.Armor, EquipmentRarity.Common, StatType.DEF, 8f);
            CreateEquip("Leather Cap", EquipmentSlot.Helm, EquipmentRarity.Common, StatType.HP, 50f);
            CreateEquip("Steel Greatsword", EquipmentSlot.Weapon, EquipmentRarity.Uncommon, StatType.ATK, 18f, new StatRoll { stat = StatType.CritRate, value = 0.03f });
            CreateEquip("Knight's Plate", EquipmentSlot.Armor, EquipmentRarity.Uncommon, StatType.DEF, 15f, new StatRoll { stat = StatType.HP, value = 40f });
            CreateEquip("Mage's Circlet", EquipmentSlot.Helm, EquipmentRarity.Rare, StatType.HP, 100f, new StatRoll { stat = StatType.ATK, value = 10f });
            CreateEquip("Shadow Ring", EquipmentSlot.Accessory, EquipmentRarity.Rare, StatType.CritRate, 0.06f, new StatRoll { stat = StatType.CritDamage, value = 0.12f });
            CreateEquip("Dragonslayer", EquipmentSlot.Weapon, EquipmentRarity.Epic, StatType.ATK, 40f, new StatRoll { stat = StatType.CritRate, value = 0.08f }, new StatRoll { stat = StatType.CritDamage, value = 0.20f });
            CreateEquip("Ancient Relic", EquipmentSlot.Relic, EquipmentRarity.Legendary, StatType.ATK, 55f, new StatRoll { stat = StatType.HP, value = 200f }, new StatRoll { stat = StatType.DEF, value = 20f }, new StatRoll { stat = StatType.SPD, value = 0.05f });
            CreateEquip("Phoenix Steed", EquipmentSlot.Mount, EquipmentRarity.Legendary, StatType.SPD, 0.15f, new StatRoll { stat = StatType.ATK, value = 30f }, new StatRoll { stat = StatType.HP, value = 150f });
        }

        static void CreateEquip(string name, EquipmentSlot slot, EquipmentRarity rarity, StatType primaryType, float primaryValue, params StatRoll[] secondary)
        {
            var eq = ScriptableObject.CreateInstance<EquipmentData>();
            eq.equipmentName = name;
            eq.slot = slot;
            eq.rarity = rarity;
            eq.primaryStat = new StatRoll { stat = primaryType, value = primaryValue };
            eq.secondaryStats = secondary;
            eq.levelRequirement = rarity switch
            {
                EquipmentRarity.Common => 1,
                EquipmentRarity.Uncommon => 5,
                EquipmentRarity.Rare => 15,
                EquipmentRarity.Epic => 30,
                EquipmentRarity.Legendary => 50,
                EquipmentRarity.Mythic => 75,
                _ => 1
            };

            string safeName = name.Replace(" ", "").Replace("'", "");
            CreateAsset(eq, $"Assets/Data/Equipment/{safeName}.asset");
        }

        // --- HELPERS ---

        static SkillBranch CreateBranch(string id, string name, string desc, SkillNodeDefinition[] nodes)
        {
            return new SkillBranch
            {
                branchId = id,
                branchName = name,
                branchDescription = desc,
                nodes = nodes
            };
        }

        static SkillNodeDefinition Node(string id, string name, string desc, SkillNodeType type, int cost, int level, string[] prereqs, StatRoll stat, Vector2 pos, bool capstone = false, string modAbility = "")
        {
            return new SkillNodeDefinition
            {
                nodeId = id,
                nodeName = name,
                description = desc,
                nodeType = type,
                cost = cost,
                unlockLevel = level,
                prerequisiteNodeIds = prereqs ?? new string[0],
                statBonuses = new[] { stat },
                treePosition = pos,
                isCapstone = capstone,
                modifiedAbilityId = modAbility,
                abilityBonusMultiplier = type == SkillNodeType.AbilityModifier ? 0.25f : 0f
            };
        }

        static SkillNodeDefinition Node(string id, string name, string desc, SkillNodeType type, int cost, int level, string[] prereqs, StatRoll[] stats, Vector2 pos, bool capstone = false, string modAbility = "")
        {
            return new SkillNodeDefinition
            {
                nodeId = id,
                nodeName = name,
                description = desc,
                nodeType = type,
                cost = cost,
                unlockLevel = level,
                prerequisiteNodeIds = prereqs ?? new string[0],
                statBonuses = stats,
                treePosition = pos,
                isCapstone = capstone,
                modifiedAbilityId = modAbility,
                abilityBonusMultiplier = type == SkillNodeType.AbilityModifier ? 0.25f : 0f
            };
        }

        static StatRoll Stat(StatType type, float value) => new() { stat = type, value = value };
        static StatRoll[] Stats(StatRoll[] s) => s;

        static void CreateAsset(Object asset, string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string[] parts = dir.Replace("\\", "/").Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
