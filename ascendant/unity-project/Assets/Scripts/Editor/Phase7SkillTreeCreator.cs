#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ascendant.Progression;

namespace Ascendant.Editor
{
    public static class Phase7SkillTreeCreator
    {
        [MenuItem("Ascendant/Create Phase 7 Skill Trees")]
        public static void CreateAll()
        {
            CreateMarksmanSkillTree();
            CreateDefenderSkillTree();
            CreateBerserkerSkillTree();
            CreateDruidSkillTree();
            CreateThiefSkillTree();
            CreateShamanSkillTree();
            CreateWarlockSkillTree();
            CreateRangerSkillTree();
            CreateSpellBladeSkillTree();
            CreateNecromancerSkillTree();
            CreateMonkSkillTree();
            CreatePaladinSkillTree();
            CreateBardSkillTree();
            CreateDragonHunterSkillTree();
            CreateSummonerSkillTree();
            CreateAlchemistSkillTree();
            CreateChronomancerSkillTree();
            CreateGunslingerSkillTree();
            CreateWardenSkillTree();
            CreateReaperSkillTree();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase7SkillTreeCreator] All 20 advanced skill trees created.");
        }

        // ── Marksman ──────────────────────────────────────────────────
        static void CreateMarksmanSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "marksman";
            tree.className = "Marksman";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("marksman_sniper", "Sniper", "Precision shots, critical damage, long-range lethality.", new SkillNodeDefinition[]
                {
                    Node("mk_sn1", "Steady Aim", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("mk_sn2", "Precision Scope", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"mk_sn1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("mk_sn3", "Long Shot", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"mk_sn2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("mk_sn4", "Dead Eye", "Aimed Shot +25% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"mk_sn3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "aimed_shot"),
                    Node("mk_sn5", "Extended Range", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"mk_sn3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("mk_sn6", "Kill Confirmed", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"mk_sn4","mk_sn5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("mk_sn7", "Headhunter", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"mk_sn6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("mk_sn8", "Lethal Caliber", "Power Shot +50% crit damage", SkillNodeType.AbilityModifier, 2, 50, new[]{"mk_sn7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "power_shot"),
                    Node("mk_sn9", "One Shot One Kill", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"mk_sn8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("marksman_barrage", "Barrage", "Rapid fire, attack speed, multi-hit volleys.", new SkillNodeDefinition[]
                {
                    Node("mk_ba1", "Rapid Fire", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("mk_ba2", "Quick Load", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"mk_ba1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("mk_ba3", "Double Tap", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"mk_ba2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("mk_ba4", "Rain of Arrows", "Multi-shot +1 projectile", SkillNodeType.AbilityModifier, 2, 15, new[]{"mk_ba3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "multi_shot"),
                    Node("mk_ba5", "Swift Hands", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"mk_ba3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("mk_ba6", "Bullet Storm", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"mk_ba4","mk_ba5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("mk_ba7", "Suppressive Fire", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"mk_ba6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("mk_ba8", "Hail of Lead", "Volley +30% speed", SkillNodeType.AbilityModifier, 2, 50, new[]{"mk_ba7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "volley"),
                    Node("mk_ba9", "Lead Hurricane", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"mk_ba8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("marksman_tactical", "Tactical", "Defensive awareness, survivability, armored shots.", new SkillNodeDefinition[]
                {
                    Node("mk_ta1", "Combat Training", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("mk_ta2", "Fortified Position", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"mk_ta1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("mk_ta3", "Sharpshooter", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"mk_ta2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("mk_ta4", "Covering Fire", "Tactical Shot +25% suppression", SkillNodeType.AbilityModifier, 2, 15, new[]{"mk_ta3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "tactical_shot"),
                    Node("mk_ta5", "Armored Stance", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"mk_ta3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("mk_ta6", "Battle Hardened", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"mk_ta4","mk_ta5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("mk_ta7", "Iron Discipline", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"mk_ta6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("mk_ta8", "Siege Mode", "Overwatch +40% damage", SkillNodeType.AbilityModifier, 2, 50, new[]{"mk_ta7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "overwatch"),
                    Node("mk_ta9", "War Machine", "ATK +25, DEF +20", SkillNodeType.Capstone, 3, 75, new[]{"mk_ta8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/MarksmanSkillTree.asset");
        }

        // ── Defender ──────────────────────────────────────────────────
        static void CreateDefenderSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "defender";
            tree.className = "Defender";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("defender_fortress", "Fortress", "Maximum HP and defense, immovable bulwark.", new SkillNodeDefinition[]
                {
                    Node("df_ft1", "Stone Skin", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("df_ft2", "Iron Constitution", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"df_ft1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("df_ft3", "Stalwart", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"df_ft2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("df_ft4", "Impenetrable Wall", "Shield Wall +30% block", SkillNodeType.AbilityModifier, 2, 15, new[]{"df_ft3"}, Stat(StatType.DEF, 10), new Vector2(0,3), modAbility: "shield_wall"),
                    Node("df_ft5", "Endurance", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"df_ft3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("df_ft6", "Unbreakable", "HP +120", SkillNodeType.PassiveStat, 2, 30, new[]{"df_ft4","df_ft5"}, Stat(StatType.HP, 120), new Vector2(0,4)),
                    Node("df_ft7", "Titan's Resolve", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"df_ft6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("df_ft8", "Absolute Defense", "Fortify +50% duration", SkillNodeType.AbilityModifier, 2, 50, new[]{"df_ft7"}, Stats(new[]{Stat(StatType.HP, 150), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "fortify"),
                    Node("df_ft9", "Living Bastion", "HP +300, DEF +25", SkillNodeType.Capstone, 3, 75, new[]{"df_ft8"}, Stats(new[]{Stat(StatType.HP, 300), Stat(StatType.DEF, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("defender_retribution", "Retribution", "Counter-attacks, reflected damage, offensive defense.", new SkillNodeDefinition[]
                {
                    Node("df_rt1", "Thorned Armor", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("df_rt2", "Retaliate", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"df_rt1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("df_rt3", "Counter Force", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"df_rt2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("df_rt4", "Vengeance Strike", "Counter-attack +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"df_rt3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "counter_attack"),
                    Node("df_rt5", "Punishing Blow", "ATK +10", SkillNodeType.PassiveStat, 1, 20, new[]{"df_rt3"}, Stat(StatType.ATK, 10), new Vector2(1,3)),
                    Node("df_rt6", "Righteous Fury", "DEF +12", SkillNodeType.PassiveStat, 2, 30, new[]{"df_rt4","df_rt5"}, Stat(StatType.DEF, 12), new Vector2(0,4)),
                    Node("df_rt7", "Wrathful Guard", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"df_rt6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("df_rt8", "Retributive Blast", "Retribution AoE reflect", SkillNodeType.AbilityModifier, 2, 50, new[]{"df_rt7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "retribution"),
                    Node("df_rt9", "Divine Retribution", "DEF +20, ATK +25", SkillNodeType.Capstone, 3, 75, new[]{"df_rt8"}, Stats(new[]{Stat(StatType.DEF, 20), Stat(StatType.ATK, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("defender_sentinel", "Sentinel", "Protective auras, party-wide shields, vigilance.", new SkillNodeDefinition[]
                {
                    Node("df_sn1", "Watchful Eye", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("df_sn2", "Thick Hide", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"df_sn1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("df_sn3", "Vigilant", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"df_sn2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("df_sn4", "Guardian Aura", "Guard Stance +party DEF", SkillNodeType.AbilityModifier, 2, 15, new[]{"df_sn3"}, Stat(StatType.DEF, 8), new Vector2(0,3), modAbility: "guard_stance"),
                    Node("df_sn5", "Vital Guard", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"df_sn3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("df_sn6", "Shield Bearer", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"df_sn4","df_sn5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("df_sn7", "Protector's Oath", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"df_sn6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("df_sn8", "Sentinel's Watch", "Sentinel Shield +party HP", SkillNodeType.AbilityModifier, 2, 50, new[]{"df_sn7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "sentinel_shield"),
                    Node("df_sn9", "Eternal Sentinel", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"df_sn8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/DefenderSkillTree.asset");
        }

        // ── Berserker ─────────────────────────────────────────────────
        static void CreateBerserkerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "berserker";
            tree.className = "Berserker";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("berserker_fury", "Fury", "Critical strikes, rage generation, explosive damage.", new SkillNodeDefinition[]
                {
                    Node("bk_fu1", "Rage Strike", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("bk_fu2", "Bloodthirst", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"bk_fu1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("bk_fu3", "Savage Blow", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"bk_fu2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("bk_fu4", "Frenzy", "Rage +30% duration", SkillNodeType.AbilityModifier, 2, 15, new[]{"bk_fu3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "rage"),
                    Node("bk_fu5", "Reckless Abandon", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"bk_fu3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("bk_fu6", "Unstoppable Force", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"bk_fu4","bk_fu5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("bk_fu7", "Primal Fury", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"bk_fu6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("bk_fu8", "Berserk Rampage", "Berserk +50% crit damage", SkillNodeType.AbilityModifier, 2, 50, new[]{"bk_fu7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "berserk"),
                    Node("bk_fu9", "Avatar of Fury", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"bk_fu8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("berserker_bloodlust", "Bloodlust", "Life steal, sustain through damage, vampiric fury.", new SkillNodeDefinition[]
                {
                    Node("bk_bl1", "Crimson Strike", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("bk_bl2", "Blood Scent", "Life Steal +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"bk_bl1"}, Stat(StatType.LifeSteal, 0.03f), new Vector2(0,1)),
                    Node("bk_bl3", "Life Tap", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"bk_bl2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("bk_bl4", "Sanguine Slash", "Blood Strike heals on hit", SkillNodeType.AbilityModifier, 2, 15, new[]{"bk_bl3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "blood_strike"),
                    Node("bk_bl5", "Vampiric Fury", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"bk_bl3"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(1,3)),
                    Node("bk_bl6", "Blood Frenzy", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"bk_bl4","bk_bl5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("bk_bl7", "Exsanguinate", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"bk_bl6"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(0,5)),
                    Node("bk_bl8", "Bloodbath", "Drain Life AoE heal", SkillNodeType.AbilityModifier, 2, 50, new[]{"bk_bl7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "drain_life"),
                    Node("bk_bl9", "Blood God", "ATK +30, Life Steal +8%", SkillNodeType.Capstone, 3, 75, new[]{"bk_bl8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.LifeSteal, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("berserker_titan", "Titan", "Massive HP pool, damage reduction, unstoppable colossus.", new SkillNodeDefinition[]
                {
                    Node("bk_ti1", "Thick Skull", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("bk_ti2", "Enduring Rage", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"bk_ti1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("bk_ti3", "Juggernaut", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"bk_ti2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("bk_ti4", "War Stomp", "War Cry stun duration +1s", SkillNodeType.AbilityModifier, 2, 15, new[]{"bk_ti3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "war_cry"),
                    Node("bk_ti5", "Iron Will", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"bk_ti3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("bk_ti6", "Colossus", "HP +120", SkillNodeType.PassiveStat, 2, 30, new[]{"bk_ti4","bk_ti5"}, Stat(StatType.HP, 120), new Vector2(0,4)),
                    Node("bk_ti7", "Mountainous", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"bk_ti6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("bk_ti8", "Titan's Grip", "Titan Slam +50% AoE", SkillNodeType.AbilityModifier, 2, 50, new[]{"bk_ti7"}, Stats(new[]{Stat(StatType.HP, 150), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "titan_slam"),
                    Node("bk_ti9", "Titan Lord", "HP +300, DEF +25", SkillNodeType.Capstone, 3, 75, new[]{"bk_ti8"}, Stats(new[]{Stat(StatType.HP, 300), Stat(StatType.DEF, 25)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/BerserkerSkillTree.asset");
        }

        // ── Druid ─────────────────────────────────────────────────────
        static void CreateDruidSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "druid";
            tree.className = "Druid";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("druid_restoration", "Restoration", "Healing power, nature magic, life regeneration.", new SkillNodeDefinition[]
                {
                    Node("dr_rs1", "Nature's Touch", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("dr_rs2", "Regrowth", "ATK +6", SkillNodeType.PassiveStat, 1, 5, new[]{"dr_rs1"}, Stat(StatType.ATK, 6), new Vector2(0,1)),
                    Node("dr_rs3", "Healing Rain", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"dr_rs2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("dr_rs4", "Rejuvenating Bloom", "Heal +25% potency", SkillNodeType.AbilityModifier, 2, 15, new[]{"dr_rs3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "heal"),
                    Node("dr_rs5", "Vital Sap", "ATK +8", SkillNodeType.PassiveStat, 1, 20, new[]{"dr_rs3"}, Stat(StatType.ATK, 8), new Vector2(1,3)),
                    Node("dr_rs6", "Verdant Life", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"dr_rs4","dr_rs5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("dr_rs7", "Nature's Blessing", "ATK +12", SkillNodeType.PassiveStat, 1, 40, new[]{"dr_rs6"}, Stat(StatType.ATK, 12), new Vector2(0,5)),
                    Node("dr_rs8", "Circle of Life", "Rejuvenate party HoT", SkillNodeType.AbilityModifier, 2, 50, new[]{"dr_rs7"}, Stats(new[]{Stat(StatType.HP, 120), Stat(StatType.ATK, 10)}), new Vector2(0,6), modAbility: "rejuvenate"),
                    Node("dr_rs9", "World Tree", "HP +250, ATK +20", SkillNodeType.Capstone, 3, 75, new[]{"dr_rs8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.ATK, 20)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("druid_feral", "Feral", "Shapeshifting aggression, attack speed, predatory strikes.", new SkillNodeDefinition[]
                {
                    Node("dr_fe1", "Claw Strike", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("dr_fe2", "Wild Instinct", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"dr_fe1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("dr_fe3", "Predator's Mark", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"dr_fe2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("dr_fe4", "Savage Pounce", "Claw Attack +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"dr_fe3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "claw_attack"),
                    Node("dr_fe5", "Fleet Footed", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"dr_fe3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("dr_fe6", "Pack Hunter", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"dr_fe4","dr_fe5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("dr_fe7", "Razor Fangs", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"dr_fe6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("dr_fe8", "Primal Assault", "Feral Charge AoE", SkillNodeType.AbilityModifier, 2, 50, new[]{"dr_fe7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "feral_charge"),
                    Node("dr_fe9", "Alpha Predator", "ATK +30, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"dr_fe8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("druid_guardian", "Guardian", "Nature's armor, root barriers, HP and defense.", new SkillNodeDefinition[]
                {
                    Node("dr_gu1", "Bark Skin", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("dr_gu2", "Nature's Armor", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"dr_gu1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("dr_gu3", "Root Shield", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"dr_gu2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("dr_gu4", "Entangling Roots", "Shield of Thorns +25% reflect", SkillNodeType.AbilityModifier, 2, 15, new[]{"dr_gu3"}, Stat(StatType.DEF, 8), new Vector2(0,3), modAbility: "shield_of_thorns"),
                    Node("dr_gu5", "Deep Roots", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"dr_gu3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("dr_gu6", "Ancient Oak", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"dr_gu4","dr_gu5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("dr_gu7", "Living Fortress", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"dr_gu6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("dr_gu8", "Guardian Spirit", "Iron Bark +50% duration", SkillNodeType.AbilityModifier, 2, 50, new[]{"dr_gu7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "iron_bark"),
                    Node("dr_gu9", "Nature's Warden", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"dr_gu8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/DruidSkillTree.asset");
        }

        // ── Thief ─────────────────────────────────────────────────────
        static void CreateThiefSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "thief";
            tree.className = "Thief";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("thief_plunder", "Plunder", "Gold bonus, treasure finding, loot-enhanced attacks.", new SkillNodeDefinition[]
                {
                    Node("th_pl1", "Pickpocket", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("th_pl2", "Quick Fingers", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"th_pl1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("th_pl3", "Treasure Sense", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"th_pl2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("th_pl4", "Gold Rush", "Steal +50% gold bonus", SkillNodeType.AbilityModifier, 2, 15, new[]{"th_pl3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "steal"),
                    Node("th_pl5", "Lucky Find", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"th_pl3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("th_pl6", "Greed", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"th_pl4","th_pl5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("th_pl7", "Jackpot", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"th_pl6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("th_pl8", "Master Heist", "Heist double gold drops", SkillNodeType.AbilityModifier, 2, 50, new[]{"th_pl7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "heist"),
                    Node("th_pl9", "King of Thieves", "ATK +30, Crit Rate +8%", SkillNodeType.Capstone, 3, 75, new[]{"th_pl8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("thief_shadow", "Shadow", "Evasion, stealth, critical ambush strikes.", new SkillNodeDefinition[]
                {
                    Node("th_sh1", "Shadow Step", "Dodge +3%", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,0)),
                    Node("th_sh2", "Vanish", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"th_sh1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("th_sh3", "Cloak of Shadows", "Dodge +4%", SkillNodeType.PassiveStat, 1, 10, new[]{"th_sh2"}, Stat(StatType.DodgeChance, 0.04f), new Vector2(0,2)),
                    Node("th_sh4", "Backstab", "Stealth +50% first-hit damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"th_sh3"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,3), modAbility: "stealth"),
                    Node("th_sh5", "Nimble", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"th_sh3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("th_sh6", "Silent Kill", "Dodge +5%", SkillNodeType.PassiveStat, 2, 30, new[]{"th_sh4","th_sh5"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,4)),
                    Node("th_sh7", "Assassinate", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"th_sh6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("th_sh8", "Shadow Dance", "Shadow Strike triple hit", SkillNodeType.AbilityModifier, 2, 50, new[]{"th_sh7"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,6), modAbility: "shadow_strike"),
                    Node("th_sh9", "Phantom Thief", "Dodge +8%, Crit Rate +8%", SkillNodeType.Capstone, 3, 75, new[]{"th_sh8"}, Stats(new[]{Stat(StatType.DodgeChance, 0.08f), Stat(StatType.CritRate, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("thief_sabotage", "Sabotage", "Traps, explosives, tactical disruption.", new SkillNodeDefinition[]
                {
                    Node("th_sa1", "Trap Setting", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("th_sa2", "Smoke Screen", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"th_sa1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("th_sa3", "Disable", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"th_sa2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("th_sa4", "Explosive Trap", "Trap +30% AoE damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"th_sa3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "trap"),
                    Node("th_sa5", "Reinforced Gear", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"th_sa3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("th_sa6", "Dismantle", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"th_sa4","th_sa5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("th_sa7", "Saboteur's Mark", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"th_sa6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("th_sa8", "Chain Reaction", "Bomb chain explosions", SkillNodeType.AbilityModifier, 2, 50, new[]{"th_sa7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "bomb"),
                    Node("th_sa9", "Master Saboteur", "ATK +25, DEF +20", SkillNodeType.Capstone, 3, 75, new[]{"th_sa8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/ThiefSkillTree.asset");
        }

        // ── Shaman ────────────────────────────────────────────────────
        static void CreateShamanSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "shaman";
            tree.className = "Shaman";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("shaman_storm", "Storm", "Lightning damage, chain effects, devastating crits.", new SkillNodeDefinition[]
                {
                    Node("sm_st1", "Lightning Bolt", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("sm_st2", "Thunder Strike", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"sm_st1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("sm_st3", "Charged Air", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"sm_st2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("sm_st4", "Chain Lightning", "Lightning chains to 3 targets", SkillNodeType.AbilityModifier, 2, 15, new[]{"sm_st3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "lightning"),
                    Node("sm_st5", "Storm Surge", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"sm_st3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("sm_st6", "Tempest", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"sm_st4","sm_st5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("sm_st7", "Thunder God", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"sm_st6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("sm_st8", "Hurricane", "Thunder AoE +50% radius", SkillNodeType.AbilityModifier, 2, 50, new[]{"sm_st7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "thunder"),
                    Node("sm_st9", "Storm Lord", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"sm_st8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("shaman_spirit", "Spirit", "Ancestral power, spirit totems, HP and attack.", new SkillNodeDefinition[]
                {
                    Node("sm_sp1", "Spirit Link", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("sm_sp2", "Ancestral Wisdom", "ATK +6", SkillNodeType.PassiveStat, 1, 5, new[]{"sm_sp1"}, Stat(StatType.ATK, 6), new Vector2(0,1)),
                    Node("sm_sp3", "Totem Power", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"sm_sp2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("sm_sp4", "Spirit Walk", "Spirit Totem +25% range", SkillNodeType.AbilityModifier, 2, 15, new[]{"sm_sp3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "spirit_totem"),
                    Node("sm_sp5", "Ethereal Blessing", "ATK +10", SkillNodeType.PassiveStat, 1, 20, new[]{"sm_sp3"}, Stat(StatType.ATK, 10), new Vector2(1,3)),
                    Node("sm_sp6", "Soul Bond", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"sm_sp4","sm_sp5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("sm_sp7", "Ancestral Fury", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"sm_sp6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("sm_sp8", "Spirit Rage", "Spirit Blast +40% damage", SkillNodeType.AbilityModifier, 2, 50, new[]{"sm_sp7"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.ATK, 12)}), new Vector2(0,6), modAbility: "spirit_blast"),
                    Node("sm_sp9", "Spirit King", "HP +250, ATK +25", SkillNodeType.Capstone, 3, 75, new[]{"sm_sp8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.ATK, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("shaman_earth", "Earth", "Earthen defenses, stone shields, resilience.", new SkillNodeDefinition[]
                {
                    Node("sm_ea1", "Stone Shield", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("sm_ea2", "Earthen Grip", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"sm_ea1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("sm_ea3", "Rock Solid", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"sm_ea2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("sm_ea4", "Earthquake", "Earth Shield +30% absorb", SkillNodeType.AbilityModifier, 2, 15, new[]{"sm_ea3"}, Stat(StatType.DEF, 8), new Vector2(0,3), modAbility: "earth_shield"),
                    Node("sm_ea5", "Terra Firma", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"sm_ea3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("sm_ea6", "Mountain's Might", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"sm_ea4","sm_ea5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("sm_ea7", "Stone Golem", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"sm_ea6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("sm_ea8", "Tectonic Shift", "Quake AoE knockback", SkillNodeType.AbilityModifier, 2, 50, new[]{"sm_ea7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "quake"),
                    Node("sm_ea9", "Earth Titan", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"sm_ea8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/ShamanSkillTree.asset");
        }

        // ── Warlock ───────────────────────────────────────────────────
        static void CreateWarlockSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "warlock";
            tree.className = "Warlock";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("warlock_affliction", "Affliction", "Curses, damage over time, spreading plague.", new SkillNodeDefinition[]
                {
                    Node("wl_af1", "Curse", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("wl_af2", "Agony", "ATK +8", SkillNodeType.PassiveStat, 1, 5, new[]{"wl_af1"}, Stat(StatType.ATK, 8), new Vector2(0,1)),
                    Node("wl_af3", "Corruption", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wl_af2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("wl_af4", "Unstable Affliction", "Curse +DoT spread", SkillNodeType.AbilityModifier, 2, 15, new[]{"wl_af3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "curse"),
                    Node("wl_af5", "Festering Wound", "Crit Damage +12%", SkillNodeType.PassiveStat, 1, 20, new[]{"wl_af3"}, Stat(StatType.CritDamage, 0.12f), new Vector2(1,3)),
                    Node("wl_af6", "Pandemic", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"wl_af4","wl_af5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("wl_af7", "Creeping Death", "Crit Damage +18%", SkillNodeType.PassiveStat, 1, 40, new[]{"wl_af6"}, Stat(StatType.CritDamage, 0.18f), new Vector2(0,5)),
                    Node("wl_af8", "Soul Rot", "Plague AoE +50%", SkillNodeType.AbilityModifier, 2, 50, new[]{"wl_af7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "plague"),
                    Node("wl_af9", "Lord of Affliction", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"wl_af8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warlock_destruction", "Destruction", "Raw shadow and fire damage, devastating crits.", new SkillNodeDefinition[]
                {
                    Node("wl_de1", "Shadow Bolt", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("wl_de2", "Immolate", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"wl_de1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("wl_de3", "Incinerate", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wl_de2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("wl_de4", "Chaos Bolt", "Shadow Bolt +40% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"wl_de3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "shadow_bolt"),
                    Node("wl_de5", "Dark Power", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"wl_de3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("wl_de6", "Hellfire", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"wl_de4","wl_de5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("wl_de7", "Shadowburn", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"wl_de6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("wl_de8", "Rain of Fire", "Chaos Bolt AoE explosion", SkillNodeType.AbilityModifier, 2, 50, new[]{"wl_de7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "chaos_bolt"),
                    Node("wl_de9", "Avatar of Destruction", "ATK +35, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"wl_de8"}, Stats(new[]{Stat(StatType.ATK, 35), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warlock_drain", "Drain", "Life siphon, soul drain, sustain through dark magic.", new SkillNodeDefinition[]
                {
                    Node("wl_dr1", "Drain Life", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("wl_dr2", "Siphon Soul", "Life Steal +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"wl_dr1"}, Stat(StatType.LifeSteal, 0.03f), new Vector2(0,1)),
                    Node("wl_dr3", "Dark Pact", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wl_dr2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("wl_dr4", "Soul Leech", "Drain +25% heal", SkillNodeType.AbilityModifier, 2, 15, new[]{"wl_dr3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "drain"),
                    Node("wl_dr5", "Vampiric Embrace", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"wl_dr3"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(1,3)),
                    Node("wl_dr6", "Life Funnel", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"wl_dr4","wl_dr5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("wl_dr7", "Harvest Soul", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"wl_dr6"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(0,5)),
                    Node("wl_dr8", "Death Coil", "Siphon AoE drain", SkillNodeType.AbilityModifier, 2, 50, new[]{"wl_dr7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "siphon"),
                    Node("wl_dr9", "Soul Harvester", "ATK +30, Life Steal +8%", SkillNodeType.Capstone, 3, 75, new[]{"wl_dr8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.LifeSteal, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/WarlockSkillTree.asset");
        }

        // ── Ranger ────────────────────────────────────────────────────
        static void CreateRangerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "ranger";
            tree.className = "Ranger";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("ranger_beastmaster", "Beastmaster", "Companion bond, beast commands, shared vitality.", new SkillNodeDefinition[]
                {
                    Node("rg_bm1", "Companion Bond", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("rg_bm2", "Wild Call", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"rg_bm1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("rg_bm3", "Pack Tactics", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"rg_bm2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("rg_bm4", "Beast Charge", "Summon Beast +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"rg_bm3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "summon_beast"),
                    Node("rg_bm5", "Animal Vigor", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"rg_bm3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("rg_bm6", "Bestial Fury", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"rg_bm4","rg_bm5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("rg_bm7", "Alpha Command", "HP +100", SkillNodeType.PassiveStat, 1, 40, new[]{"rg_bm6"}, Stat(StatType.HP, 100), new Vector2(0,5)),
                    Node("rg_bm8", "Stampede", "Command beast AoE charge", SkillNodeType.AbilityModifier, 2, 50, new[]{"rg_bm7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "command"),
                    Node("rg_bm9", "Beastlord", "ATK +25, HP +200", SkillNodeType.Capstone, 3, 75, new[]{"rg_bm8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.HP, 200)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("ranger_survival", "Survival", "Traps, evasion, wilderness cunning.", new SkillNodeDefinition[]
                {
                    Node("rg_sv1", "Tracker's Eye", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("rg_sv2", "Camouflage", "Dodge +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"rg_sv1"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,1)),
                    Node("rg_sv3", "Quick Escape", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"rg_sv2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("rg_sv4", "Bear Trap", "Trap +30% snare duration", SkillNodeType.AbilityModifier, 2, 15, new[]{"rg_sv3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "trap"),
                    Node("rg_sv5", "Evasive Maneuver", "Dodge +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"rg_sv3"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(1,3)),
                    Node("rg_sv6", "Wilderness Lore", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"rg_sv4","rg_sv5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("rg_sv7", "Resourceful", "Dodge +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"rg_sv6"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,5)),
                    Node("rg_sv8", "Survival Instinct", "Escape auto-dodge at low HP", SkillNodeType.AbilityModifier, 2, 50, new[]{"rg_sv7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DodgeChance, 0.05f)}), new Vector2(0,6), modAbility: "escape"),
                    Node("rg_sv9", "Apex Survivor", "ATK +25, Dodge +8%", SkillNodeType.Capstone, 3, 75, new[]{"rg_sv8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DodgeChance, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("ranger_pack", "Pack", "Coordinated strikes, speed, group hunting.", new SkillNodeDefinition[]
                {
                    Node("rg_pk1", "Scout Ahead", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("rg_pk2", "Swift Strike", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"rg_pk1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("rg_pk3", "Coordinated Attack", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"rg_pk2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("rg_pk4", "Flanking Shot", "Multi-shot +flank bonus", SkillNodeType.AbilityModifier, 2, 15, new[]{"rg_pk3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "multi_shot"),
                    Node("rg_pk5", "Quick Draw", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"rg_pk3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("rg_pk6", "Volley", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"rg_pk4","rg_pk5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("rg_pk7", "Multishot", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"rg_pk6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("rg_pk8", "Pack Volley", "Pack Attack party speed buff", SkillNodeType.AbilityModifier, 2, 50, new[]{"rg_pk7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "pack_attack"),
                    Node("rg_pk9", "Pack Alpha", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"rg_pk8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/RangerSkillTree.asset");
        }

        // ── SpellBlade ────────────────────────────────────────────────
        static void CreateSpellBladeSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "spellblade";
            tree.className = "SpellBlade";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("spellblade_tempest", "Tempest", "Lightning-infused strikes, speed, storm power.", new SkillNodeDefinition[]
                {
                    Node("sb_tp1", "Charged Blade", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("sb_tp2", "Lightning Slash", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"sb_tp1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("sb_tp3", "Wind Step", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"sb_tp2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("sb_tp4", "Tempest Strike", "Lightning Slash chain +2 targets", SkillNodeType.AbilityModifier, 2, 15, new[]{"sb_tp3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "lightning_slash"),
                    Node("sb_tp5", "Flash Step", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"sb_tp3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("sb_tp6", "Storm Edge", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"sb_tp4","sb_tp5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("sb_tp7", "Cyclone Blade", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"sb_tp6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("sb_tp8", "Thunder Fury", "Tempest Strike AoE storm", SkillNodeType.AbilityModifier, 2, 50, new[]{"sb_tp7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "tempest_strike"),
                    Node("sb_tp9", "Tempest Lord", "ATK +30, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"sb_tp8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("spellblade_battlemage", "Battlemage", "Arcane-enhanced melee, critical focus.", new SkillNodeDefinition[]
                {
                    Node("sb_bm1", "Arcane Edge", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("sb_bm2", "Spell Strike", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"sb_bm1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("sb_bm3", "Magic Infusion", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"sb_bm2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("sb_bm4", "Arcane Burst", "Arcane Slash +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"sb_bm3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "arcane_slash"),
                    Node("sb_bm5", "Critical Focus", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"sb_bm3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("sb_bm6", "Mystic Blade", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"sb_bm4","sb_bm5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("sb_bm7", "Enchanted Steel", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"sb_bm6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("sb_bm8", "Spell Combo", "Spell Burst chain attacks", SkillNodeType.AbilityModifier, 2, 50, new[]{"sb_bm7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "spell_burst"),
                    Node("sb_bm9", "Grand Battlemage", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"sb_bm8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("spellblade_mysticknight", "Mystic Knight", "Magic defense, spell wards, armored spellcaster.", new SkillNodeDefinition[]
                {
                    Node("sb_mk1", "Magic Shield", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("sb_mk2", "Ward Blade", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"sb_mk1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("sb_mk3", "Spell Guard", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"sb_mk2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("sb_mk4", "Mana Shield", "Mana Shield +30% absorb", SkillNodeType.AbilityModifier, 2, 15, new[]{"sb_mk3"}, Stat(StatType.DEF, 8), new Vector2(0,3), modAbility: "mana_shield"),
                    Node("sb_mk5", "Mystic Armor", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"sb_mk3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("sb_mk6", "Enchanted Defense", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"sb_mk4","sb_mk5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("sb_mk7", "Arcane Bulwark", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"sb_mk6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("sb_mk8", "Nullification", "Ward nullifies magic damage", SkillNodeType.AbilityModifier, 2, 50, new[]{"sb_mk7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "ward"),
                    Node("sb_mk9", "Arcane Guardian", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"sb_mk8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/SpellBladeSkillTree.asset");
        }

        // ── Necromancer ───────────────────────────────────────────────
        static void CreateNecromancerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "necromancer";
            tree.className = "Necromancer";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("necromancer_undeath", "Undeath", "Undead minions, bone armor, unholy strength.", new SkillNodeDefinition[]
                {
                    Node("nc_un1", "Raise Dead", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("nc_un2", "Bone Armor", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"nc_un1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("nc_un3", "Ghoul Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"nc_un2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("nc_un4", "Army of the Dead", "Raise Dead +2 minions", SkillNodeType.AbilityModifier, 2, 15, new[]{"nc_un3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "raise_dead"),
                    Node("nc_un5", "Undying Minion", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"nc_un3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("nc_un6", "Death's Embrace", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"nc_un4","nc_un5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("nc_un7", "Unholy Strength", "HP +100", SkillNodeType.PassiveStat, 1, 40, new[]{"nc_un6"}, Stat(StatType.HP, 100), new Vector2(0,5)),
                    Node("nc_un8", "Plague of Undeath", "Corpse Explosion chain reaction", SkillNodeType.AbilityModifier, 2, 50, new[]{"nc_un7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "corpse_explosion"),
                    Node("nc_un9", "Lich King", "ATK +30, HP +200", SkillNodeType.Capstone, 3, 75, new[]{"nc_un8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.HP, 200)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("necromancer_decay", "Decay", "Necrotic damage, blight, critical dark magic.", new SkillNodeDefinition[]
                {
                    Node("nc_dc1", "Necrotic Touch", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("nc_dc2", "Festering Strike", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"nc_dc1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("nc_dc3", "Death Blight", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"nc_dc2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("nc_dc4", "Corpse Explosion", "Blight +AoE on kill", SkillNodeType.AbilityModifier, 2, 15, new[]{"nc_dc3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "blight"),
                    Node("nc_dc5", "Dark Power", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"nc_dc3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("nc_dc6", "Withering Curse", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"nc_dc4","nc_dc5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("nc_dc7", "Blight Storm", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"nc_dc6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("nc_dc8", "Soul Shatter", "Soul Shatter execute threshold", SkillNodeType.AbilityModifier, 2, 50, new[]{"nc_dc7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "soul_shatter"),
                    Node("nc_dc9", "Lord of Decay", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"nc_dc8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("necromancer_lich", "Lich", "Dark wards, spell resistance, armored undead.", new SkillNodeDefinition[]
                {
                    Node("nc_li1", "Soul Cage", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("nc_li2", "Phylactery", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"nc_li1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("nc_li3", "Dark Ward", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"nc_li2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("nc_li4", "Lich's Grasp", "Soul Cage +freeze effect", SkillNodeType.AbilityModifier, 2, 15, new[]{"nc_li3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "soul_cage"),
                    Node("nc_li5", "Spell Resistance", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"nc_li3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("nc_li6", "Necrotic Shield", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"nc_li4","nc_li5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("nc_li7", "Dark Fortress", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"nc_li6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("nc_li8", "Lich Form", "Lich Form +immunity 3s", SkillNodeType.AbilityModifier, 2, 50, new[]{"nc_li7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DEF, 12)}), new Vector2(0,6), modAbility: "lich_form"),
                    Node("nc_li9", "Archlich", "ATK +25, DEF +20", SkillNodeType.Capstone, 3, 75, new[]{"nc_li8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/NecromancerSkillTree.asset");
        }

        // ── Monk ──────────────────────────────────────────────────────
        static void CreateMonkSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "monk";
            tree.className = "Monk";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("monk_ironfist", "Iron Fist", "Devastating strikes, pressure points, critical blows.", new SkillNodeDefinition[]
                {
                    Node("mn_if1", "Iron Palm", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("mn_if2", "Pressure Point", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"mn_if1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("mn_if3", "Dragon Fist", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"mn_if2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("mn_if4", "Rising Dragon", "Dragon Fist +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"mn_if3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "dragon_fist"),
                    Node("mn_if5", "Chi Strike", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"mn_if3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("mn_if6", "Power Blow", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"mn_if4","mn_if5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("mn_if7", "Crushing Palm", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"mn_if6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("mn_if8", "Hundred Fists", "Tiger Palm rapid strikes", SkillNodeType.AbilityModifier, 2, 50, new[]{"mn_if7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "tiger_palm"),
                    Node("mn_if9", "Grandmaster Fist", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"mn_if8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("monk_flowingriver", "Flowing River", "Fluid evasion, speed, redirecting force.", new SkillNodeDefinition[]
                {
                    Node("mn_fr1", "Flowing Dodge", "Dodge +3%", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,0)),
                    Node("mn_fr2", "Water Step", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"mn_fr1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("mn_fr3", "Mist Form", "Dodge +4%", SkillNodeType.PassiveStat, 1, 10, new[]{"mn_fr2"}, Stat(StatType.DodgeChance, 0.04f), new Vector2(0,2)),
                    Node("mn_fr4", "Redirecting Force", "Redirect counter-attack", SkillNodeType.AbilityModifier, 2, 15, new[]{"mn_fr3"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,3), modAbility: "redirect"),
                    Node("mn_fr5", "Wind Walker", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"mn_fr3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("mn_fr6", "Cloud Step", "Dodge +5%", SkillNodeType.PassiveStat, 2, 30, new[]{"mn_fr4","mn_fr5"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,4)),
                    Node("mn_fr7", "Serene Flow", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"mn_fr6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("mn_fr8", "River's Current", "Flowing Strike multi-hit", SkillNodeType.AbilityModifier, 2, 50, new[]{"mn_fr7"}, Stats(new[]{Stat(StatType.DodgeChance, 0.05f), Stat(StatType.SPD, 0.03f)}), new Vector2(0,6), modAbility: "flowing_strike"),
                    Node("mn_fr9", "One With Flow", "Dodge +8%, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"mn_fr8"}, Stats(new[]{Stat(StatType.DodgeChance, 0.08f), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("monk_zen", "Zen", "Inner peace, meditation, fortified body and spirit.", new SkillNodeDefinition[]
                {
                    Node("mn_zn1", "Inner Peace", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("mn_zn2", "Meditation", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"mn_zn1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("mn_zn3", "Iron Body", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"mn_zn2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("mn_zn4", "Tranquil Fury", "Meditation +25% regen", SkillNodeType.AbilityModifier, 2, 15, new[]{"mn_zn3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "meditation"),
                    Node("mn_zn5", "Fortified Spirit", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"mn_zn3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("mn_zn6", "Enlightened Guard", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"mn_zn4","mn_zn5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("mn_zn7", "Diamond Body", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"mn_zn6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("mn_zn8", "Zen Mastery", "Inner Peace full heal", SkillNodeType.AbilityModifier, 2, 50, new[]{"mn_zn7"}, Stats(new[]{Stat(StatType.HP, 120), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "inner_peace"),
                    Node("mn_zn9", "Ascended Monk", "HP +250, DEF +25", SkillNodeType.Capstone, 3, 75, new[]{"mn_zn8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.DEF, 25)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/MonkSkillTree.asset");
        }

        // ── Paladin ───────────────────────────────────────────────────
        static void CreatePaladinSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "paladin";
            tree.className = "Paladin";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("paladin_crusader", "Crusader", "Holy offense, zealous strikes, critical smites.", new SkillNodeDefinition[]
                {
                    Node("pl_cr1", "Holy Strike", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("pl_cr2", "Righteous Zeal", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"pl_cr1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("pl_cr3", "Crusade", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"pl_cr2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("pl_cr4", "Divine Smite", "Divine Smite +30% holy damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"pl_cr3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "divine_smite"),
                    Node("pl_cr5", "Holy Fervor", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"pl_cr3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("pl_cr6", "Zealot's Fury", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"pl_cr4","pl_cr5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("pl_cr7", "Sacred Weapon", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"pl_cr6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("pl_cr8", "Judgment Day", "Holy Strike AoE judgment", SkillNodeType.AbilityModifier, 2, 50, new[]{"pl_cr7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "holy_strike"),
                    Node("pl_cr9", "Holy Crusader", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"pl_cr8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("paladin_guardian", "Guardian", "Sacred shields, devotion aura, divine barriers.", new SkillNodeDefinition[]
                {
                    Node("pl_gd1", "Sacred Shield", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("pl_gd2", "Devotion", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"pl_gd1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("pl_gd3", "Holy Armor", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"pl_gd2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("pl_gd4", "Blessing of Protection", "Shield of Faith +party DEF", SkillNodeType.AbilityModifier, 2, 15, new[]{"pl_gd3"}, Stat(StatType.DEF, 10), new Vector2(0,3), modAbility: "shield_of_faith"),
                    Node("pl_gd5", "Sacred Ground", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"pl_gd3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("pl_gd6", "Divine Barrier", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"pl_gd4","pl_gd5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("pl_gd7", "Consecrated Shield", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"pl_gd6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("pl_gd8", "Eternal Devotion", "Holy Shield invulnerability 2s", SkillNodeType.AbilityModifier, 2, 50, new[]{"pl_gd7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "holy_shield"),
                    Node("pl_gd9", "Divine Guardian", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"pl_gd8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("paladin_templar", "Templar", "Holy radiance, blessed strikes, inspiring auras.", new SkillNodeDefinition[]
                {
                    Node("pl_tm1", "Templar's Might", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("pl_tm2", "Holy Radiance", "ATK +6", SkillNodeType.PassiveStat, 1, 5, new[]{"pl_tm1"}, Stat(StatType.ATK, 6), new Vector2(0,1)),
                    Node("pl_tm3", "Blessed Strike", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"pl_tm2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("pl_tm4", "Divine Intervention", "Holy Aura +party ATK", SkillNodeType.AbilityModifier, 2, 15, new[]{"pl_tm3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "holy_aura"),
                    Node("pl_tm5", "Inspired Courage", "ATK +10", SkillNodeType.PassiveStat, 1, 20, new[]{"pl_tm3"}, Stat(StatType.ATK, 10), new Vector2(1,3)),
                    Node("pl_tm6", "Aura of Light", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"pl_tm4","pl_tm5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("pl_tm7", "Blessed Hammer", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"pl_tm6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("pl_tm8", "Final Stand", "Divine Hammer +50% AoE", SkillNodeType.AbilityModifier, 2, 50, new[]{"pl_tm7"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.ATK, 12)}), new Vector2(0,6), modAbility: "divine_hammer"),
                    Node("pl_tm9", "Templar Lord", "HP +250, ATK +25", SkillNodeType.Capstone, 3, 75, new[]{"pl_tm8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.ATK, 25)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/PaladinSkillTree.asset");
        }

        // ── Bard ──────────────────────────────────────────────────────
        static void CreateBardSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "bard";
            tree.className = "Bard";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("bard_ballad", "Ballad", "Defensive songs, shields, fortitude melodies.", new SkillNodeDefinition[]
                {
                    Node("bd_bl1", "Soothing Melody", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("bd_bl2", "Shield Song", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"bd_bl1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("bd_bl3", "Hymn of Fortitude", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"bd_bl2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("bd_bl4", "Ballad of Protection", "Ballad +party shield", SkillNodeType.AbilityModifier, 2, 15, new[]{"bd_bl3"}, Stat(StatType.DEF, 10), new Vector2(0,3), modAbility: "ballad"),
                    Node("bd_bl5", "Harmonize", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"bd_bl3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("bd_bl6", "Resilience Chorus", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"bd_bl4","bd_bl5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("bd_bl7", "Fortress Refrain", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"bd_bl6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("bd_bl8", "Epic Ballad", "War Song +50% shield", SkillNodeType.AbilityModifier, 2, 50, new[]{"bd_bl7"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "war_song"),
                    Node("bd_bl9", "Maestro of Defense", "HP +250, DEF +25", SkillNodeType.Capstone, 3, 75, new[]{"bd_bl8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.DEF, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("bard_wardrums", "War Drums", "Battle tempo, attack speed, offensive percussion.", new SkillNodeDefinition[]
                {
                    Node("bd_wd1", "Battle Beat", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("bd_wd2", "War Tempo", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"bd_wd1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("bd_wd3", "Drumroll", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"bd_wd2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("bd_wd4", "Thundering Drums", "War Drums +30% party ATK", SkillNodeType.AbilityModifier, 2, 15, new[]{"bd_wd3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "war_drums"),
                    Node("bd_wd5", "Quick Tempo", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"bd_wd3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("bd_wd6", "Rapid Cadence", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"bd_wd4","bd_wd5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("bd_wd7", "Fury Percussion", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"bd_wd6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("bd_wd8", "Warsong", "Battle Cry +party haste", SkillNodeType.AbilityModifier, 2, 50, new[]{"bd_wd7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "battle_cry"),
                    Node("bd_wd9", "Warlord's Anthem", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"bd_wd8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("bard_hymn", "Hymn", "Inspiring melodies, evasion, fortune.", new SkillNodeDefinition[]
                {
                    Node("bd_hy1", "Inspiring Verse", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("bd_hy2", "Lucky Tune", "Dodge +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"bd_hy1"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,1)),
                    Node("bd_hy3", "Nimble Melody", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"bd_hy2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("bd_hy4", "Dance of Blades", "Hymn +dodge bonus", SkillNodeType.AbilityModifier, 2, 15, new[]{"bd_hy3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "hymn"),
                    Node("bd_hy5", "Evasive Harmony", "Dodge +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"bd_hy3"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(1,3)),
                    Node("bd_hy6", "Fortune's Favor", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"bd_hy4","bd_hy5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("bd_hy7", "Anthem of Grace", "Dodge +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"bd_hy6"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,5)),
                    Node("bd_hy8", "Divine Hymn", "Serenade +party dodge", SkillNodeType.AbilityModifier, 2, 50, new[]{"bd_hy7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DodgeChance, 0.05f)}), new Vector2(0,6), modAbility: "serenade"),
                    Node("bd_hy9", "Legendary Bard", "ATK +25, Dodge +8%", SkillNodeType.Capstone, 3, 75, new[]{"bd_hy8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DodgeChance, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/BardSkillTree.asset");
        }

        // ── DragonHunter ──────────────────────────────────────────────
        static void CreateDragonHunterSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "dragonhunter";
            tree.className = "DragonHunter";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("dragonhunter_slayer", "Slayer", "Maximum damage, critical devastation, dragon slaying.", new SkillNodeDefinition[]
                {
                    Node("dh_sl1", "Dragonbane", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("dh_sl2", "Scale Piercer", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"dh_sl1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("dh_sl3", "Wyrm Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"dh_sl2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("dh_sl4", "Dragon Slayer", "Dragon Strike +50% vs bosses", SkillNodeType.AbilityModifier, 2, 15, new[]{"dh_sl3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "dragon_strike"),
                    Node("dh_sl5", "Lethal Focus", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"dh_sl3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("dh_sl6", "Mortal Wound", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"dh_sl4","dh_sl5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("dh_sl7", "Dragonfire", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"dh_sl6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("dh_sl8", "Execute Dragon", "Execute +instant kill below 10%", SkillNodeType.AbilityModifier, 2, 50, new[]{"dh_sl7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "execute"),
                    Node("dh_sl9", "Supreme Slayer", "ATK +35, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"dh_sl8"}, Stats(new[]{Stat(StatType.ATK, 35), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("dragonhunter_trapper", "Trapper", "Binding traps, defensive gear, tactical control.", new SkillNodeDefinition[]
                {
                    Node("dh_tr1", "Snare", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("dh_tr2", "Binding Chain", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"dh_tr1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("dh_tr3", "Reinforced Trap", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"dh_tr2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("dh_tr4", "Dragon Snare", "Trap +stun vs bosses", SkillNodeType.AbilityModifier, 2, 15, new[]{"dh_tr3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "trap"),
                    Node("dh_tr5", "Hardened Armor", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"dh_tr3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("dh_tr6", "Entrapment", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"dh_tr4","dh_tr5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("dh_tr7", "Iron Net", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"dh_tr6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("dh_tr8", "Dragon Cage", "Dragon Net +immobilize 3s", SkillNodeType.AbilityModifier, 2, 50, new[]{"dh_tr7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "dragon_net"),
                    Node("dh_tr9", "Master Trapper", "ATK +25, DEF +20", SkillNodeType.Capstone, 3, 75, new[]{"dh_tr8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("dragonhunter_hunter", "Hunter", "Tracking, precision, critical targeting.", new SkillNodeDefinition[]
                {
                    Node("dh_hu1", "Keen Eye", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("dh_hu2", "Tracking", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"dh_hu1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("dh_hu3", "Marked Prey", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"dh_hu2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("dh_hu4", "Hunter's Mark", "Hunter's Mark +30% damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"dh_hu3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "hunters_mark"),
                    Node("dh_hu5", "Critical Aim", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"dh_hu3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("dh_hu6", "Predator's Focus", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"dh_hu4","dh_hu5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("dh_hu7", "Eagle Eye", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"dh_hu6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("dh_hu8", "Finishing Shot", "Power Shot guaranteed crit on marked", SkillNodeType.AbilityModifier, 2, 50, new[]{"dh_hu7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "power_shot"),
                    Node("dh_hu9", "Apex Hunter", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"dh_hu8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/DragonHunterSkillTree.asset");
        }

        // ── Summoner ──────────────────────────────────────────────────
        static void CreateSummonerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "summoner";
            tree.className = "Summoner";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("summoner_conjuration", "Conjuration", "Stronger summons, familiar bond, shared vitality.", new SkillNodeDefinition[]
                {
                    Node("su_cj1", "Summon Imp", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("su_cj2", "Fiery Familiar", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"su_cj1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("su_cj3", "Elemental Bond", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"su_cj2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("su_cj4", "Greater Conjure", "Summon +25% summon damage", SkillNodeType.AbilityModifier, 2, 15, new[]{"su_cj3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "summon"),
                    Node("su_cj5", "Familiar Vitality", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"su_cj3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("su_cj6", "Empowered Summon", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"su_cj4","su_cj5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("su_cj7", "Arcane Familiar", "HP +100", SkillNodeType.PassiveStat, 1, 40, new[]{"su_cj6"}, Stat(StatType.HP, 100), new Vector2(0,5)),
                    Node("su_cj8", "Grand Conjuration", "Conjure elite summon", SkillNodeType.AbilityModifier, 2, 50, new[]{"su_cj7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "conjure"),
                    Node("su_cj9", "Supreme Conjurer", "ATK +25, HP +200", SkillNodeType.Capstone, 3, 75, new[]{"su_cj8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.HP, 200)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("summoner_duality", "Duality", "Twin summons, haste, mirror images.", new SkillNodeDefinition[]
                {
                    Node("su_du1", "Twin Summon", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("su_du2", "Haste Familiar", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"su_du1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("su_du3", "Dual Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"su_du2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("su_du4", "Mirror Image", "Twin Summon +clone", SkillNodeType.AbilityModifier, 2, 15, new[]{"su_du3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "twin_summon"),
                    Node("su_du5", "Quick Summon", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"su_du3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("su_du6", "Parallel Cast", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"su_du4","su_du5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("su_du7", "Shadow Clone", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"su_du6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("su_du8", "Duality Mastery", "Mirror +permanent clone", SkillNodeType.AbilityModifier, 2, 50, new[]{"su_du7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "mirror"),
                    Node("su_du9", "Master of Duality", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"su_du8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("summoner_synergy", "Synergy", "Symbiotic bond, shared defense, fusion power.", new SkillNodeDefinition[]
                {
                    Node("su_sy1", "Shared Bond", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("su_sy2", "Protective Familiar", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"su_sy1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("su_sy3", "Symbiosis", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"su_sy2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("su_sy4", "Fusion", "Fusion +merged familiar form", SkillNodeType.AbilityModifier, 2, 15, new[]{"su_sy3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "fusion"),
                    Node("su_sy5", "Hardened Familiar", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"su_sy3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("su_sy6", "United Front", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"su_sy4","su_sy5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("su_sy7", "Synergized Strike", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"su_sy6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("su_sy8", "Perfect Fusion", "Synergy Blast +AoE", SkillNodeType.AbilityModifier, 2, 50, new[]{"su_sy7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "synergy_blast"),
                    Node("su_sy9", "Grand Synthesizer", "ATK +25, DEF +20", SkillNodeType.Capstone, 3, 75, new[]{"su_sy8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DEF, 20)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/SummonerSkillTree.asset");
        }

        // ── Alchemist ─────────────────────────────────────────────────
        static void CreateAlchemistSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "alchemist";
            tree.className = "Alchemist";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("alchemist_pyromancy", "Pyromancy", "Fire flasks, explosive damage, burning crits.", new SkillNodeDefinition[]
                {
                    Node("al_py1", "Fire Flask", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("al_py2", "Volatile Mix", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"al_py1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("al_py3", "Ignite", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"al_py2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("al_py4", "Firebomb", "Firebomb +30% AoE", SkillNodeType.AbilityModifier, 2, 15, new[]{"al_py3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "firebomb"),
                    Node("al_py5", "Chemical Burn", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"al_py3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("al_py6", "Napalm", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"al_py4","al_py5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("al_py7", "Inferno Brew", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"al_py6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("al_py8", "Pyroclasm", "Inferno +50% burn duration", SkillNodeType.AbilityModifier, 2, 50, new[]{"al_py7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "inferno"),
                    Node("al_py9", "Grand Pyromancer", "ATK +30, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"al_py8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("alchemist_restoration", "Restoration", "Healing potions, protective elixirs, fortification.", new SkillNodeDefinition[]
                {
                    Node("al_rs1", "Healing Potion", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("al_rs2", "Elixir of Life", "DEF +8", SkillNodeType.PassiveStat, 1, 5, new[]{"al_rs1"}, Stat(StatType.DEF, 8), new Vector2(0,1)),
                    Node("al_rs3", "Tonic of Iron", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"al_rs2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("al_rs4", "Regeneration Serum", "Potion +25% heal", SkillNodeType.AbilityModifier, 2, 15, new[]{"al_rs3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "potion"),
                    Node("al_rs5", "Antidote", "DEF +10", SkillNodeType.PassiveStat, 1, 20, new[]{"al_rs3"}, Stat(StatType.DEF, 10), new Vector2(1,3)),
                    Node("al_rs6", "Fortification Brew", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"al_rs4","al_rs5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("al_rs7", "Shield Elixir", "DEF +15", SkillNodeType.PassiveStat, 1, 40, new[]{"al_rs6"}, Stat(StatType.DEF, 15), new Vector2(0,5)),
                    Node("al_rs8", "Philosopher's Stone", "Elixir +transmute shield", SkillNodeType.AbilityModifier, 2, 50, new[]{"al_rs7"}, Stats(new[]{Stat(StatType.HP, 120), Stat(StatType.DEF, 10)}), new Vector2(0,6), modAbility: "elixir"),
                    Node("al_rs9", "Master Alchemist", "HP +250, DEF +25", SkillNodeType.Capstone, 3, 75, new[]{"al_rs8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.DEF, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("alchemist_mutation", "Mutation", "Mutagens, adaptive formulas, speed enhancements.", new SkillNodeDefinition[]
                {
                    Node("al_mu1", "Mutagen", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("al_mu2", "Adaptive Formula", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"al_mu1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("al_mu3", "Haste Tincture", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"al_mu2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("al_mu4", "Transformation", "Mutagen +25% all stats 5s", SkillNodeType.AbilityModifier, 2, 15, new[]{"al_mu3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "mutagen"),
                    Node("al_mu5", "Quick Brew", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"al_mu3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("al_mu6", "Catalyst", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"al_mu4","al_mu5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("al_mu7", "Unstable Mutagen", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"al_mu6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("al_mu8", "Metamorphosis", "Catalyst +permanent buff", SkillNodeType.AbilityModifier, 2, 50, new[]{"al_mu7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "catalyst"),
                    Node("al_mu9", "Evolved Alchemist", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"al_mu8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/AlchemistSkillTree.asset");
        }

        // ── Chronomancer ──────────────────────────────────────────────
        static void CreateChronomancerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "chronomancer";
            tree.className = "Chronomancer";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("chronomancer_acceleration", "Acceleration", "Time haste, speed bursts, temporal rush.", new SkillNodeDefinition[]
                {
                    Node("ch_ac1", "Haste", "SPD +0.04", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.SPD, 0.04f), new Vector2(0,0)),
                    Node("ch_ac2", "Time Skip", "ATK +6", SkillNodeType.PassiveStat, 1, 5, new[]{"ch_ac1"}, Stat(StatType.ATK, 6), new Vector2(0,1)),
                    Node("ch_ac3", "Temporal Rush", "SPD +0.05", SkillNodeType.PassiveStat, 1, 10, new[]{"ch_ac2"}, Stat(StatType.SPD, 0.05f), new Vector2(0,2)),
                    Node("ch_ac4", "Time Warp", "Haste +party speed buff", SkillNodeType.AbilityModifier, 2, 15, new[]{"ch_ac3"}, Stat(StatType.SPD, 0.03f), new Vector2(0,3), modAbility: "haste"),
                    Node("ch_ac5", "Accelerated Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 20, new[]{"ch_ac3"}, Stat(StatType.ATK, 10), new Vector2(1,3)),
                    Node("ch_ac6", "Chrono Boost", "SPD +0.05", SkillNodeType.PassiveStat, 2, 30, new[]{"ch_ac4","ch_ac5"}, Stat(StatType.SPD, 0.05f), new Vector2(0,4)),
                    Node("ch_ac7", "Time Flux", "ATK +15", SkillNodeType.PassiveStat, 1, 40, new[]{"ch_ac6"}, Stat(StatType.ATK, 15), new Vector2(0,5)),
                    Node("ch_ac8", "Temporal Surge", "Time Warp +double action", SkillNodeType.AbilityModifier, 2, 50, new[]{"ch_ac7"}, Stats(new[]{Stat(StatType.SPD, 0.05f), Stat(StatType.ATK, 12)}), new Vector2(0,6), modAbility: "time_warp"),
                    Node("ch_ac9", "Time Lord", "SPD +0.10, ATK +25", SkillNodeType.Capstone, 3, 75, new[]{"ch_ac8"}, Stats(new[]{Stat(StatType.SPD, 0.10f), Stat(StatType.ATK, 25)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("chronomancer_entropy", "Entropy", "Temporal decay, time bombs, critical strikes.", new SkillNodeDefinition[]
                {
                    Node("ch_en1", "Temporal Decay", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("ch_en2", "Time Bomb", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"ch_en1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("ch_en3", "Chrono Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"ch_en2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("ch_en4", "Entropy Blast", "Time Bomb +30% detonation", SkillNodeType.AbilityModifier, 2, 15, new[]{"ch_en3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "time_bomb"),
                    Node("ch_en5", "Focused Time", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"ch_en3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("ch_en6", "Temporal Precision", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"ch_en4","ch_en5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("ch_en7", "Decay Field", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"ch_en6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("ch_en8", "Temporal Annihilation", "Entropy +execute below 20%", SkillNodeType.AbilityModifier, 2, 50, new[]{"ch_en7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "entropy"),
                    Node("ch_en9", "Entropy Master", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"ch_en8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("chronomancer_paradox", "Paradox", "Phase shifting, temporal dodges, reality bending.", new SkillNodeDefinition[]
                {
                    Node("ch_px1", "Phase Shift", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("ch_px2", "Temporal Dodge", "Dodge +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"ch_px1"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,1)),
                    Node("ch_px3", "Paradox Shift", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"ch_px2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("ch_px4", "Time Fold", "Phase Shift +invulnerable 1s", SkillNodeType.AbilityModifier, 2, 15, new[]{"ch_px3"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,3), modAbility: "phase_shift"),
                    Node("ch_px5", "Chrono Sidestep", "Dodge +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"ch_px3"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(1,3)),
                    Node("ch_px6", "Reality Bend", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"ch_px4","ch_px5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("ch_px7", "Temporal Cloak", "Dodge +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"ch_px6"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,5)),
                    Node("ch_px8", "Paradox Storm", "Paradox +AoE time rift", SkillNodeType.AbilityModifier, 2, 50, new[]{"ch_px7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DodgeChance, 0.05f)}), new Vector2(0,6), modAbility: "paradox"),
                    Node("ch_px9", "Paradox Lord", "ATK +25, Dodge +8%", SkillNodeType.Capstone, 3, 75, new[]{"ch_px8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DodgeChance, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/ChronomancerSkillTree.asset");
        }

        // ── Gunslinger ────────────────────────────────────────────────
        static void CreateGunslingerSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "gunslinger";
            tree.className = "Gunslinger";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("gunslinger_desperado", "Desperado", "Quick draw, lethal crits, last-stand power.", new SkillNodeDefinition[]
                {
                    Node("gs_dp1", "Quick Draw", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("gs_dp2", "Fan the Hammer", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"gs_dp1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("gs_dp3", "Dead Aim", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"gs_dp2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("gs_dp4", "Deadeye Shot", "Deadeye +guaranteed crit", SkillNodeType.AbilityModifier, 2, 15, new[]{"gs_dp3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "deadeye"),
                    Node("gs_dp5", "Lethal Caliber", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"gs_dp3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("gs_dp6", "Marksman", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"gs_dp4","gs_dp5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("gs_dp7", "Ricochet", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"gs_dp6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("gs_dp8", "Last Stand", "Last Stand +100% ATK below 25% HP", SkillNodeType.AbilityModifier, 2, 50, new[]{"gs_dp7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "last_stand"),
                    Node("gs_dp9", "Legendary Gunslinger", "ATK +35, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"gs_dp8"}, Stats(new[]{Stat(StatType.ATK, 35), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("gunslinger_trickshot", "Trickshot", "Rapid reload, speed shooting, showmanship.", new SkillNodeDefinition[]
                {
                    Node("gs_ts1", "Hip Shot", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("gs_ts2", "Rapid Reload", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"gs_ts1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("gs_ts3", "Showoff", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"gs_ts2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("gs_ts4", "Trick Volley", "Trick Shot +ricochet", SkillNodeType.AbilityModifier, 2, 15, new[]{"gs_ts3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "trick_shot"),
                    Node("gs_ts5", "Deft Hands", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"gs_ts3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("gs_ts6", "Bullet Time", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"gs_ts4","gs_ts5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("gs_ts7", "Silver Bullet", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"gs_ts6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("gs_ts8", "Barrage", "Rapid Fire +50% speed", SkillNodeType.AbilityModifier, 2, 50, new[]{"gs_ts7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "rapid_fire"),
                    Node("gs_ts9", "Master Trickshot", "ATK +25, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"gs_ts8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("gunslinger_demolition", "Demolition", "Explosives, grenades, critical detonations.", new SkillNodeDefinition[]
                {
                    Node("gs_dm1", "Explosive Round", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("gs_dm2", "Grenade", "Crit Rate +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"gs_dm1"}, Stat(StatType.CritRate, 0.03f), new Vector2(0,1)),
                    Node("gs_dm3", "Blast Zone", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"gs_dm2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("gs_dm4", "Dynamite", "Grenade +50% AoE", SkillNodeType.AbilityModifier, 2, 15, new[]{"gs_dm3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "grenade"),
                    Node("gs_dm5", "Shrapnel", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"gs_dm3"}, Stat(StatType.CritRate, 0.05f), new Vector2(1,3)),
                    Node("gs_dm6", "Demolition Charge", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"gs_dm4","gs_dm5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("gs_dm7", "Heavy Ordnance", "Crit Rate +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"gs_dm6"}, Stat(StatType.CritRate, 0.05f), new Vector2(0,5)),
                    Node("gs_dm8", "Carpet Bomb", "Explosive +chain detonation", SkillNodeType.AbilityModifier, 2, 50, new[]{"gs_dm7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "explosive"),
                    Node("gs_dm9", "Demolition Expert", "ATK +30, Crit Rate +10%", SkillNodeType.Capstone, 3, 75, new[]{"gs_dm8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.CritRate, 0.10f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/GunslingerSkillTree.asset");
        }

        // ── Warden ────────────────────────────────────────────────────
        static void CreateWardenSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "warden";
            tree.className = "Warden";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("warden_ironbark", "Ironbark", "Bark armor, root walls, maximum defense.", new SkillNodeDefinition[]
                {
                    Node("wd_ib1", "Bark Armor", "DEF +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.DEF, 5), new Vector2(0,0)),
                    Node("wd_ib2", "Root Defense", "HP +60", SkillNodeType.PassiveStat, 1, 5, new[]{"wd_ib1"}, Stat(StatType.HP, 60), new Vector2(0,1)),
                    Node("wd_ib3", "Ironwood", "DEF +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wd_ib2"}, Stat(StatType.DEF, 10), new Vector2(0,2)),
                    Node("wd_ib4", "Living Wall", "Bark Shield +40% absorb", SkillNodeType.AbilityModifier, 2, 15, new[]{"wd_ib3"}, Stat(StatType.DEF, 10), new Vector2(0,3), modAbility: "bark_shield"),
                    Node("wd_ib5", "Heartwood", "HP +80", SkillNodeType.PassiveStat, 1, 20, new[]{"wd_ib3"}, Stat(StatType.HP, 80), new Vector2(1,3)),
                    Node("wd_ib6", "Ancient Bark", "DEF +15", SkillNodeType.PassiveStat, 2, 30, new[]{"wd_ib4","wd_ib5"}, Stat(StatType.DEF, 15), new Vector2(0,4)),
                    Node("wd_ib7", "Nature's Fortress", "HP +120", SkillNodeType.PassiveStat, 1, 40, new[]{"wd_ib6"}, Stat(StatType.HP, 120), new Vector2(0,5)),
                    Node("wd_ib8", "Petrified Wood", "Root Wall +immunity 3s", SkillNodeType.AbilityModifier, 2, 50, new[]{"wd_ib7"}, Stats(new[]{Stat(StatType.DEF, 12), Stat(StatType.HP, 100)}), new Vector2(0,6), modAbility: "root_wall"),
                    Node("wd_ib9", "Ironbark Lord", "DEF +25, HP +250", SkillNodeType.Capstone, 3, 75, new[]{"wd_ib8"}, Stats(new[]{Stat(StatType.DEF, 25), Stat(StatType.HP, 250)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warden_rootwalker", "Rootwalker", "Vine attacks, nature's agility, evasive roots.", new SkillNodeDefinition[]
                {
                    Node("wd_rw1", "Vine Lash", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("wd_rw2", "Root Step", "Dodge +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"wd_rw1"}, Stat(StatType.DodgeChance, 0.03f), new Vector2(0,1)),
                    Node("wd_rw3", "Thorn Strike", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"wd_rw2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("wd_rw4", "Entangle", "Vine Lash +root snare", SkillNodeType.AbilityModifier, 2, 15, new[]{"wd_rw3"}, Stat(StatType.ATK, 8), new Vector2(0,3), modAbility: "vine_lash"),
                    Node("wd_rw5", "Swift Root", "Dodge +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"wd_rw3"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(1,3)),
                    Node("wd_rw6", "Bramble Dance", "ATK +12", SkillNodeType.PassiveStat, 2, 30, new[]{"wd_rw4","wd_rw5"}, Stat(StatType.ATK, 12), new Vector2(0,4)),
                    Node("wd_rw7", "Vine Whip", "Dodge +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"wd_rw6"}, Stat(StatType.DodgeChance, 0.05f), new Vector2(0,5)),
                    Node("wd_rw8", "Root Storm", "Root Strike AoE entangle", SkillNodeType.AbilityModifier, 2, 50, new[]{"wd_rw7"}, Stats(new[]{Stat(StatType.ATK, 15), Stat(StatType.DodgeChance, 0.05f)}), new Vector2(0,6), modAbility: "root_strike"),
                    Node("wd_rw9", "Root King", "ATK +25, Dodge +8%", SkillNodeType.Capstone, 3, 75, new[]{"wd_rw8"}, Stats(new[]{Stat(StatType.ATK, 25), Stat(StatType.DodgeChance, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("warden_overgrowth", "Overgrowth", "Life absorption, natural regeneration, verdant power.", new SkillNodeDefinition[]
                {
                    Node("wd_og1", "Nature's Grasp", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("wd_og2", "Absorb Life", "Life Steal +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"wd_og1"}, Stat(StatType.LifeSteal, 0.03f), new Vector2(0,1)),
                    Node("wd_og3", "Verdant Growth", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"wd_og2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("wd_og4", "Bloom", "Bloom +party heal over time", SkillNodeType.AbilityModifier, 2, 15, new[]{"wd_og3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "bloom"),
                    Node("wd_og5", "Life Sap", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"wd_og3"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(1,3)),
                    Node("wd_og6", "Photosynthesis", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"wd_og4","wd_og5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("wd_og7", "Evergreen", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"wd_og6"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(0,5)),
                    Node("wd_og8", "Nature's Bounty", "Absorb +AoE life drain", SkillNodeType.AbilityModifier, 2, 50, new[]{"wd_og7"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.LifeSteal, 0.03f)}), new Vector2(0,6), modAbility: "absorb"),
                    Node("wd_og9", "Overgrowth Lord", "HP +250, Life Steal +8%", SkillNodeType.Capstone, 3, 75, new[]{"wd_og8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.LifeSteal, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/WardenSkillTree.asset");
        }

        // ── Reaper ────────────────────────────────────────────────────
        static void CreateReaperSkillTree()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.classId = "reaper";
            tree.className = "Reaper";
            tree.branches = new SkillBranch[]
            {
                CreateBranch("reaper_execution", "Execution", "Lethal strikes, execute thresholds, devastating crits.", new SkillNodeDefinition[]
                {
                    Node("rp_ex1", "Soul Rend", "ATK +6", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 6), new Vector2(0,0)),
                    Node("rp_ex2", "Harvest", "Crit Damage +10%", SkillNodeType.PassiveStat, 1, 5, new[]{"rp_ex1"}, Stat(StatType.CritDamage, 0.10f), new Vector2(0,1)),
                    Node("rp_ex3", "Death Mark", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"rp_ex2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("rp_ex4", "Execute", "Execute +instant kill below 15%", SkillNodeType.AbilityModifier, 2, 15, new[]{"rp_ex3"}, Stat(StatType.ATK, 12), new Vector2(0,3), modAbility: "execute"),
                    Node("rp_ex5", "Lethal Edge", "Crit Damage +15%", SkillNodeType.PassiveStat, 1, 20, new[]{"rp_ex3"}, Stat(StatType.CritDamage, 0.15f), new Vector2(1,3)),
                    Node("rp_ex6", "Grim Strike", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"rp_ex4","rp_ex5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("rp_ex7", "Reap", "Crit Damage +20%", SkillNodeType.PassiveStat, 1, 40, new[]{"rp_ex6"}, Stat(StatType.CritDamage, 0.20f), new Vector2(0,5)),
                    Node("rp_ex8", "Final Judgment", "Reap +AoE execute", SkillNodeType.AbilityModifier, 2, 50, new[]{"rp_ex7"}, Stat(StatType.ATK, 20), new Vector2(0,6), modAbility: "reap"),
                    Node("rp_ex9", "Death Incarnate", "ATK +35, Crit Damage +25%", SkillNodeType.Capstone, 3, 75, new[]{"rp_ex8"}, Stats(new[]{Stat(StatType.ATK, 35), Stat(StatType.CritDamage, 0.25f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("reaper_reaving", "Reaving", "Scythe speed, spectral dashes, relentless assault.", new SkillNodeDefinition[]
                {
                    Node("rp_rv1", "Swift Scythe", "ATK +5", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.ATK, 5), new Vector2(0,0)),
                    Node("rp_rv2", "Whirlwind", "SPD +0.03", SkillNodeType.PassiveStat, 1, 5, new[]{"rp_rv1"}, Stat(StatType.SPD, 0.03f), new Vector2(0,1)),
                    Node("rp_rv3", "Spectral Rush", "ATK +10", SkillNodeType.PassiveStat, 1, 10, new[]{"rp_rv2"}, Stat(StatType.ATK, 10), new Vector2(0,2)),
                    Node("rp_rv4", "Soul Rush", "Soul Rush +teleport strike", SkillNodeType.AbilityModifier, 2, 15, new[]{"rp_rv3"}, Stat(StatType.ATK, 10), new Vector2(0,3), modAbility: "soul_rush"),
                    Node("rp_rv5", "Shadow Dash", "SPD +0.05", SkillNodeType.PassiveStat, 1, 20, new[]{"rp_rv3"}, Stat(StatType.SPD, 0.05f), new Vector2(1,3)),
                    Node("rp_rv6", "Death Dance", "ATK +15", SkillNodeType.PassiveStat, 2, 30, new[]{"rp_rv4","rp_rv5"}, Stat(StatType.ATK, 15), new Vector2(0,4)),
                    Node("rp_rv7", "Soul Blitz", "SPD +0.05", SkillNodeType.PassiveStat, 1, 40, new[]{"rp_rv6"}, Stat(StatType.SPD, 0.05f), new Vector2(0,5)),
                    Node("rp_rv8", "Reaving Storm", "Reave AoE scythe storm", SkillNodeType.AbilityModifier, 2, 50, new[]{"rp_rv7"}, Stat(StatType.ATK, 18), new Vector2(0,6), modAbility: "reave"),
                    Node("rp_rv9", "Reaving Lord", "ATK +30, SPD +0.08", SkillNodeType.Capstone, 3, 75, new[]{"rp_rv8"}, Stats(new[]{Stat(StatType.ATK, 30), Stat(StatType.SPD, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
                CreateBranch("reaper_undying", "Undying", "Dark resilience, soul armor, life drain sustain.", new SkillNodeDefinition[]
                {
                    Node("rp_ud1", "Grim Resolve", "HP +50", SkillNodeType.PassiveStat, 1, 1, null, Stat(StatType.HP, 50), new Vector2(0,0)),
                    Node("rp_ud2", "Death's Touch", "Life Steal +3%", SkillNodeType.PassiveStat, 1, 5, new[]{"rp_ud1"}, Stat(StatType.LifeSteal, 0.03f), new Vector2(0,1)),
                    Node("rp_ud3", "Life Drain", "HP +80", SkillNodeType.PassiveStat, 1, 10, new[]{"rp_ud2"}, Stat(StatType.HP, 80), new Vector2(0,2)),
                    Node("rp_ud4", "Undying Will", "Drain +revive at 1 HP once", SkillNodeType.AbilityModifier, 2, 15, new[]{"rp_ud3"}, Stat(StatType.HP, 60), new Vector2(0,3), modAbility: "drain"),
                    Node("rp_ud5", "Soul Armor", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 20, new[]{"rp_ud3"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(1,3)),
                    Node("rp_ud6", "Dark Resilience", "HP +100", SkillNodeType.PassiveStat, 2, 30, new[]{"rp_ud4","rp_ud5"}, Stat(StatType.HP, 100), new Vector2(0,4)),
                    Node("rp_ud7", "Unholy Vitality", "Life Steal +5%", SkillNodeType.PassiveStat, 1, 40, new[]{"rp_ud6"}, Stat(StatType.LifeSteal, 0.05f), new Vector2(0,5)),
                    Node("rp_ud8", "Death's Embrace", "Undying +soul shield on kill", SkillNodeType.AbilityModifier, 2, 50, new[]{"rp_ud7"}, Stats(new[]{Stat(StatType.HP, 100), Stat(StatType.LifeSteal, 0.03f)}), new Vector2(0,6), modAbility: "undying"),
                    Node("rp_ud9", "Immortal Reaper", "HP +250, Life Steal +8%", SkillNodeType.Capstone, 3, 75, new[]{"rp_ud8"}, Stats(new[]{Stat(StatType.HP, 250), Stat(StatType.LifeSteal, 0.08f)}), new Vector2(0,7), capstone: true),
                }),
            };
            CreateAsset(tree, "Assets/Data/SkillTrees/ReaperSkillTree.asset");
        }

        // ── HELPERS (same pattern as Phase4DataCreator) ───────────────

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
