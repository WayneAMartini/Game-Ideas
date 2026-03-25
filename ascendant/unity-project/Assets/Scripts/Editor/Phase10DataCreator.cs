using UnityEngine;
using UnityEditor;
using Ascendant.Combat;
using Ascendant.Islands;
using Ascendant.Events;
using System.Collections.Generic;

public class Phase10DataCreator
{
    [MenuItem("Ascendant/Create Phase 10 Data (Realm 2-3, Tower Modifiers, Rift Configs)")]
    public static void CreateAllPhase10Data()
    {
        CreateDirectories();
        CreateTowerModifiers();
        CreateRiftModifiers();
        CreateRealm2Islands();
        CreateRealm3Islands();
        CreateEventConfigs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Phase10] All data created successfully!");
    }

    static void CreateDirectories()
    {
        EnsureFolder("Assets/Data/Events");
        EnsureFolder("Assets/Data/Events/TowerModifiers");
        EnsureFolder("Assets/Data/Events/RiftModifiers");
        EnsureFolder("Assets/Data/Events/Configs");
        EnsureFolder("Assets/Data/Islands/Realm2");
        EnsureFolder("Assets/Data/Islands/Realm3");
        EnsureFolder("Assets/Data/Biomes/Realm2");
        EnsureFolder("Assets/Data/Biomes/Realm3");
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    // ─── TOWER MODIFIERS ──────────────────────────────────

    static void CreateTowerModifiers()
    {
        var defs = new[]
        {
            new ModDef("BerserkersRage", "Berserker's Rage", "+50% ATK, -30% DEF", 1.5f, 0.7f, 1f, 1f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("GlassCannon", "Glass Cannon", "+80% ATK, -50% HP", 1.8f, 1f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("IronFortress", "Iron Fortress", "+60% DEF, -30% ATK", 0.7f, 1.6f, 1f, 1f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("SwiftStrike", "Swift Strike", "+40% Speed, -20% DEF", 1f, 0.8f, 1f, 1.4f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("HealingSurge", "Healing Surge", "+50% Healing, -20% ATK", 0.8f, 1f, 1f, 1f, 1.5f, 1f, 1f, 1f, 1f),
            new ModDef("EnemyFrenzy", "Enemy Frenzy", "Enemies have +40% ATK", 1f, 1f, 1f, 1f, 1f, 1.4f, 1f, 1f, 1f),
            new ModDef("Armored", "Armored Foes", "Enemies have +50% DEF", 1f, 1f, 1f, 1f, 1f, 1f, 1.5f, 1f, 1f),
            new ModDef("Resilient", "Resilient Enemies", "Enemies have +60% HP", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.6f, 1f),
            new ModDef("QuickEnemies", "Quick Enemies", "Enemies have +30% Speed", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1.3f),
            new ModDef("DoubleGold", "Double Gold", "Gold drops doubled", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, true),
            new ModDef("HalfCooldowns", "Half Cooldowns", "All cooldowns halved", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, false, true),
            new ModDef("ExplosiveDeaths", "Explosive Deaths", "Enemies explode on death for 15% HP", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, false, false, true),
            new ModDef("NoHealing", "No Healing", "All healing disabled", 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, false, false, false, true),
            new ModDef("Reflective", "Reflective", "10% damage reflected back", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, false, false, false, false, true),
            new ModDef("Balanced", "Balanced", "+20% ATK, +20% DEF, +20% HP", 1.2f, 1.2f, 1.2f, 1f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("ChaosMode", "Chaos Mode", "+30% ATK to both sides", 1.3f, 1f, 1f, 1f, 1f, 1.3f, 1f, 1f, 1f),
            new ModDef("Endurance", "Endurance", "+50% HP, -20% ATK", 0.8f, 1f, 1.5f, 1f, 1f, 1f, 1f, 1f, 1f),
            new ModDef("SpeedDemon", "Speed Demon", "+60% Speed both sides", 1f, 1f, 1f, 1.6f, 1f, 1f, 1f, 1f, 1.6f),
            new ModDef("TankBuster", "Tank Buster", "+70% ATK, enemies have +40% HP", 1.7f, 1f, 1f, 1f, 1f, 1f, 1f, 1.4f, 1f),
            new ModDef("Nightmare", "Nightmare", "+40% enemy ATK, +40% enemy HP, +40% enemy DEF", 1f, 1f, 1f, 1f, 1f, 1.4f, 1.4f, 1.4f, 1f),
        };

        foreach (var def in defs)
        {
            string path = $"Assets/Data/Events/TowerModifiers/{def.Id}.asset";
            var mod = AssetDatabase.LoadAssetAtPath<TowerModifier>(path);
            if (mod == null)
            {
                mod = ScriptableObject.CreateInstance<TowerModifier>();
                AssetDatabase.CreateAsset(mod, path);
            }

            mod.modifierName = def.Name;
            mod.description = def.Desc;
            mod.atkMultiplier = def.Atk;
            mod.defMultiplier = def.Def;
            mod.hpMultiplier = def.Hp;
            mod.speedMultiplier = def.Spd;
            mod.healingMultiplier = def.Heal;
            mod.enemyAtkMultiplier = def.EAtk;
            mod.enemyDefMultiplier = def.EDef;
            mod.enemyHpMultiplier = def.EHp;
            mod.enemySpeedMultiplier = def.ESpd;
            mod.doubleGold = def.DoubleGold;
            mod.halfCooldowns = def.HalfCD;
            mod.enemiesExplodeOnDeath = def.Explode;
            mod.explosionDamagePercent = def.Explode ? 0.15f : 0f;
            mod.noHealing = def.NoHeal;
            mod.reflectDamagePercent = def.Reflect;
            mod.reflectAmount = def.Reflect ? 0.1f : 0f;
            EditorUtility.SetDirty(mod);
        }

        Debug.Log($"[Phase10] Created {defs.Length} Tower Modifiers");
    }

    struct ModDef
    {
        public string Id, Name, Desc;
        public float Atk, Def, Hp, Spd, Heal, EAtk, EDef, EHp, ESpd;
        public bool DoubleGold, HalfCD, Explode, NoHeal, Reflect;

        public ModDef(string id, string name, string desc, float atk, float def, float hp, float spd, float heal,
            float eatk, float edef, float ehp, float espd,
            bool gold = false, bool halfCd = false, bool explode = false, bool noHeal = false, bool reflect = false)
        {
            Id = id; Name = name; Desc = desc;
            Atk = atk; Def = def; Hp = hp; Spd = spd; Heal = heal;
            EAtk = eatk; EDef = edef; EHp = ehp; ESpd = espd;
            DoubleGold = gold; HalfCD = halfCd; Explode = explode; NoHeal = noHeal; Reflect = reflect;
        }
    }

    // ─── RIFT MODIFIERS ──────────────────────────────────

    static void CreateRiftModifiers()
    {
        CreateRift("VoidOfSilence", "Void of Silence", "All abilities disabled. Tap damage only.",
            true, 1f, 1f, false, false, 1f, 1f, 1f, new Color(0.2f, 0f, 0.3f, 0.3f));
        CreateRift("TemporalRift", "Temporal Rift", "Cooldowns halved but enemies move at 2x speed.",
            false, 0.5f, 2f, false, false, 1f, 1f, 1f, new Color(0f, 0.3f, 0.5f, 0.3f));
        CreateRift("ShadowRealm", "Shadow Realm", "Visibility reduced. Enemies invisible until attacked.",
            false, 1f, 1f, true, true, 1f, 1f, 1f, new Color(0.1f, 0.05f, 0.15f, 0.5f));
        CreateRift("ChaosVortex", "Chaos Vortex", "All damage increased by 50% for both sides.",
            false, 1f, 1f, false, false, 1.5f, 1.5f, 1f, new Color(0.5f, 0f, 0f, 0.3f));
        CreateRift("AbyssalDepths", "Abyssal Depths", "Enemies have double HP. Aether Crystal drops increased.",
            false, 1f, 1f, false, false, 1f, 1f, 2f, new Color(0f, 0f, 0.2f, 0.4f));
    }

    static void CreateRift(string id, string name, string desc, bool noAbilities, float cdMult, float eSpd,
        bool lowVis, bool invisible, float pDmg, float eDmg, float eHp, Color overlay)
    {
        string path = $"Assets/Data/Events/RiftModifiers/{id}.asset";
        var mod = AssetDatabase.LoadAssetAtPath<RiftModifier>(path);
        if (mod == null)
        {
            mod = ScriptableObject.CreateInstance<RiftModifier>();
            AssetDatabase.CreateAsset(mod, path);
        }

        mod.modifierName = name;
        mod.description = desc;
        mod.abilitiesDisabled = noAbilities;
        mod.cooldownMultiplier = cdMult;
        mod.enemySpeedMultiplier = eSpd;
        mod.reducedVisibility = lowVis;
        mod.enemiesInvisibleUntilAttacked = invisible;
        mod.playerDamageMultiplier = pDmg;
        mod.enemyDamageMultiplier = eDmg;
        mod.enemyHpMultiplier = eHp;
        mod.overlayColor = overlay;
        EditorUtility.SetDirty(mod);
    }

    // ─── REALM 2 ISLANDS (13-24) ─────────────────────────

    static void CreateRealm2Islands()
    {
        var defs = new[]
        {
            new R2Def(13, "Ashveil Highlands", Affinity.Flame, Affinity.Shadow, "Volcanic peaks shrouded in dark mist.", "Pyroshade Titan", 12f, 4f),
            new R2Def(14, "Frostfire Basin", Affinity.Frost, Affinity.Flame, "A frozen lake above a magma chamber.", "Cryoflame Hydra", 13f, 4f),
            new R2Def(15, "Thunderbloom Grove", Affinity.Storm, Affinity.Nature, "Electrified ancient forest.", "Stormseed Treant", 13f, 4f),
            new R2Def(16, "Moonshade Valley", Affinity.Shadow, Affinity.Frost, "A frozen valley in permanent eclipse.", "Eclipsed Behemoth", 14f, 4.5f),
            new R2Def(17, "Solarwind Mesa", Affinity.Radiance, Affinity.Storm, "Sun-blasted cliffs with howling winds.", "Luminous Tempest", 14f, 4.5f),
            new R2Def(18, "Briarflame Thicket", Affinity.Nature, Affinity.Flame, "Burning jungle with regenerating thorns.", "Infernal Briarmother", 15f, 4.5f),
            new R2Def(19, "Obsidian Stormspire", Affinity.Flame, Affinity.Storm, "Black glass towers crackling with lightning.", "Voltite Colossus", 15f, 5f),
            new R2Def(20, "Glacial Sanctuary", Affinity.Frost, Affinity.Radiance, "Crystal ice cathedral bathed in light.", "Prism Frostlord", 16f, 5f),
            new R2Def(21, "Shadowthorn Hollow", Affinity.Shadow, Affinity.Nature, "Corrupted forest of living darkness.", "Blighted Archon", 16f, 5f),
            new R2Def(22, "Radiant Tempest", Affinity.Radiance, Affinity.Frost, "Shimmering aurora above frozen peaks.", "Aurora Sovereign", 17f, 5.5f),
            new R2Def(23, "Maelstrom Reach", Affinity.Storm, Affinity.Shadow, "Perpetual storm above a void chasm.", "Void Stormcaller", 17f, 5.5f),
            new R2Def(24, "Infernal Zenith", Affinity.Flame, Affinity.Radiance, "The Realm 2 summit: holy fire citadel.", "The Void Sovereign", 25f, 6f),
        };

        foreach (var def in defs)
        {
            string path = $"Assets/Data/Islands/Realm2/Island{def.Num:D2}.asset";
            var island = AssetDatabase.LoadAssetAtPath<IslandData>(path);
            if (island == null)
            {
                island = ScriptableObject.CreateInstance<IslandData>();
                AssetDatabase.CreateAsset(island, path);
            }

            island.islandName = def.Name;
            island.islandNumber = def.Num;
            island.realmNumber = 2;
            island.description = def.Desc;
            island.affinity = def.Primary;
            island.secondaryAffinity = def.Secondary;
            island.stageCount = 100;
            island.islandBossName = def.Boss;
            island.islandBossDescription = $"Realm 2 boss of {def.Name}.";
            island.islandBossHpMultiplier = def.BossHp;
            island.islandBossAtkMultiplier = def.BossAtk;
            island.miniBossHpMultiplier = 5f;  // 2x Realm 1
            island.miniBossAtkMultiplier = 2.5f;
            island.miniBossGoldMultiplier = 3f;
            island.miniBossXpMultiplier = 3f;
            island.miniBossMechanicCount = 2;  // 2 mechanics per mini-boss in Realm 2
            island.goldMultiplier = 1f + (def.Num - 1) * 0.15f;
            island.xpMultiplier = 1f + (def.Num - 1) * 0.12f;

            // Boss phases: 4-5 phases for Realm 2
            island.bossPhases = CreateRealm2BossPhases(def.Num);

            EditorUtility.SetDirty(island);
        }

        Debug.Log("[Phase10] Created 12 Realm 2 Islands (13-24)");
    }

    struct R2Def
    {
        public int Num;
        public string Name;
        public Affinity Primary, Secondary;
        public string Desc, Boss;
        public float BossHp, BossAtk;

        public R2Def(int n, string name, Affinity p, Affinity s, string desc, string boss, float bossHp, float bossAtk)
        {
            Num = n; Name = name; Primary = p; Secondary = s; Desc = desc;
            Boss = boss; BossHp = bossHp; BossAtk = bossAtk;
        }
    }

    static List<BossPhaseData> CreateRealm2BossPhases(int islandNumber)
    {
        var phases = new List<BossPhaseData>();

        if (islandNumber == 24) // Realm 2 Boss: The Void Sovereign
        {
            phases.Add(new BossPhaseData { phaseName = "Sovereign's Gaze", hpThreshold = 1f, description = "Opens with void beams and shadow clones.", atkMultiplier = 1.5f, attackSpeedMultiplier = 1f, mechanics = new[] { BossMechanicType.GroundSlam, BossMechanicType.AddSpawning } });
            phases.Add(new BossPhaseData { phaseName = "Dimensional Rift", hpThreshold = 0.75f, description = "Tears open portals, enemies pour through.", atkMultiplier = 2f, attackSpeedMultiplier = 1.3f, mechanics = new[] { BossMechanicType.AddSpawning, BossMechanicType.Enrage } });
            phases.Add(new BossPhaseData { phaseName = "Void Shield", hpThreshold = 0.5f, description = "Generates an impenetrable void shield.", atkMultiplier = 2.5f, attackSpeedMultiplier = 1.5f, mechanics = new[] { BossMechanicType.ShieldPhase, BossMechanicType.Reflect } });
            phases.Add(new BossPhaseData { phaseName = "Reality Shatter", hpThreshold = 0.25f, description = "Fragments reality itself. All mechanics active.", atkMultiplier = 3f, attackSpeedMultiplier = 1.8f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.LifeSteal, BossMechanicType.GroundSlam } });
            phases.Add(new BossPhaseData { phaseName = "Annihilation", hpThreshold = 0.1f, description = "Final desperate assault.", atkMultiplier = 4f, attackSpeedMultiplier = 2f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.Reflect } });
        }
        else
        {
            phases.Add(new BossPhaseData { phaseName = "Phase 1", hpThreshold = 1f, description = "Standard dual-affinity attacks.", atkMultiplier = 1.5f, attackSpeedMultiplier = 1f, mechanics = new[] { BossMechanicType.GroundSlam } });
            phases.Add(new BossPhaseData { phaseName = "Phase 2", hpThreshold = 0.7f, description = "Elemental fury unleashed.", atkMultiplier = 2f, attackSpeedMultiplier = 1.3f, mechanics = new[] { BossMechanicType.AddSpawning, BossMechanicType.Enrage } });
            phases.Add(new BossPhaseData { phaseName = "Phase 3", hpThreshold = 0.4f, description = "Desperate enhanced attacks.", atkMultiplier = 2.5f, attackSpeedMultiplier = 1.5f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.ShieldPhase } });
            phases.Add(new BossPhaseData { phaseName = "Phase 4", hpThreshold = 0.15f, description = "Enraged final stand.", atkMultiplier = 3f, attackSpeedMultiplier = 1.8f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.Reflect } });
        }

        return phases;
    }

    // ─── REALM 3 ISLANDS (25-36) ─────────────────────────

    static void CreateRealm3Islands()
    {
        var defs = new[]
        {
            new R2Def(25, "Primordial Crucible", Affinity.Flame, Affinity.Nature, "Where creation fire meets primal growth.", "Genesis Flame", 20f, 6f),
            new R2Def(26, "Eternity's Edge", Affinity.Frost, Affinity.Storm, "Frozen lightning bolts in suspended time.", "Chrono Frost Wyrm", 21f, 6.5f),
            new R2Def(27, "Umbral Canopy", Affinity.Shadow, Affinity.Nature, "Forest of shadow-trees that consume light.", "The Hollow Root", 21f, 6.5f),
            new R2Def(28, "Celestial Forge", Affinity.Radiance, Affinity.Flame, "Divine smithy where stars are born.", "Astral Forgemaster", 22f, 7f),
            new R2Def(29, "Tempest Abyss", Affinity.Storm, Affinity.Shadow, "Storm that descends into infinite darkness.", "Abyssal Thunderlord", 22f, 7f),
            new R2Def(30, "Glacial Bloom", Affinity.Frost, Affinity.Nature, "Frozen garden of crystal flowers.", "The Crystal Archdruid", 23f, 7.5f),
            new R2Def(31, "Scorching Shadows", Affinity.Flame, Affinity.Shadow, "Infernal plane where fire casts shadow.", "Shadowflame Demon", 23f, 7.5f),
            new R2Def(32, "Radiant Tundra", Affinity.Radiance, Affinity.Frost, "Aurora-lit permafrost wasteland.", "The Boreal Saint", 24f, 8f),
            new R2Def(33, "Verdant Tempest", Affinity.Nature, Affinity.Storm, "Living hurricane of vines and lightning.", "Stormroot Titan", 24f, 8f),
            new R2Def(34, "Eclipse Throne", Affinity.Shadow, Affinity.Radiance, "Where light and dark exist in perfect balance.", "The Equilibrium", 25f, 8.5f),
            new R2Def(35, "Elemental Convergence", Affinity.Storm, Affinity.Flame, "All elements clash in eternal war.", "Omni-Elemental", 25f, 8.5f),
            new R2Def(36, "Apex of Creation", Affinity.Radiance, Affinity.Shadow, "The pinnacle of existence. The final Realm.", "The Primordial Chaos", 40f, 10f),
        };

        foreach (var def in defs)
        {
            string path = $"Assets/Data/Islands/Realm3/Island{def.Num:D2}.asset";
            var island = AssetDatabase.LoadAssetAtPath<IslandData>(path);
            if (island == null)
            {
                island = ScriptableObject.CreateInstance<IslandData>();
                AssetDatabase.CreateAsset(island, path);
            }

            island.islandName = def.Name;
            island.islandNumber = def.Num;
            island.realmNumber = 3;
            island.description = def.Desc;
            island.affinity = def.Primary;
            island.secondaryAffinity = def.Secondary;
            island.stageCount = 100;
            island.islandBossName = def.Boss;
            island.islandBossDescription = $"Realm 3 boss of {def.Name}.";
            island.islandBossHpMultiplier = def.BossHp;
            island.islandBossAtkMultiplier = def.BossAtk;
            island.miniBossHpMultiplier = 8f;  // 3x Realm 1
            island.miniBossAtkMultiplier = 4f;
            island.miniBossGoldMultiplier = 4f;
            island.miniBossXpMultiplier = 4f;
            island.miniBossMechanicCount = 3;  // 3 simultaneous mechanics
            island.goldMultiplier = 1f + (def.Num - 1) * 0.2f;
            island.xpMultiplier = 1f + (def.Num - 1) * 0.15f;

            // Boss phases: 5-6 phases for Realm 3
            island.bossPhases = CreateRealm3BossPhases(def.Num);

            EditorUtility.SetDirty(island);
        }

        Debug.Log("[Phase10] Created 12 Realm 3 Islands (25-36)");
    }

    static List<BossPhaseData> CreateRealm3BossPhases(int islandNumber)
    {
        var phases = new List<BossPhaseData>();

        if (islandNumber == 36) // The Primordial Chaos — most complex encounter
        {
            phases.Add(new BossPhaseData { phaseName = "Genesis", hpThreshold = 1f, description = "The Chaos creates reality.", atkMultiplier = 2f, attackSpeedMultiplier = 1f, mechanics = new[] { BossMechanicType.GroundSlam, BossMechanicType.AddSpawning } });
            phases.Add(new BossPhaseData { phaseName = "Elemental Storm", hpThreshold = 0.85f, description = "All affinities attack at once.", atkMultiplier = 2.5f, attackSpeedMultiplier = 1.3f, mechanics = new[] { BossMechanicType.AddSpawning, BossMechanicType.Enrage, BossMechanicType.GroundSlam } });
            phases.Add(new BossPhaseData { phaseName = "Void Collapse", hpThreshold = 0.6f, description = "Reality folds in on itself.", atkMultiplier = 3f, attackSpeedMultiplier = 1.5f, mechanics = new[] { BossMechanicType.ShieldPhase, BossMechanicType.Reflect } });
            phases.Add(new BossPhaseData { phaseName = "Dimensional Tear", hpThreshold = 0.4f, description = "Tears between dimensions.", atkMultiplier = 3.5f, attackSpeedMultiplier = 1.8f, mechanics = new[] { BossMechanicType.LifeSteal, BossMechanicType.Split, BossMechanicType.Enrage } });
            phases.Add(new BossPhaseData { phaseName = "Primordial Fury", hpThreshold = 0.2f, description = "Unleashes primordial fury.", atkMultiplier = 4f, attackSpeedMultiplier = 2f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.AddSpawning, BossMechanicType.GroundSlam } });
            phases.Add(new BossPhaseData { phaseName = "Final Entropy", hpThreshold = 0.05f, description = "The universe trembles. Enrage timer active.", atkMultiplier = 5f, attackSpeedMultiplier = 2.5f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.Reflect, BossMechanicType.LifeSteal } });
        }
        else
        {
            phases.Add(new BossPhaseData { phaseName = "Phase 1", hpThreshold = 1f, description = "Dual-element assault.", atkMultiplier = 2f, attackSpeedMultiplier = 1f, mechanics = new[] { BossMechanicType.GroundSlam, BossMechanicType.AddSpawning } });
            phases.Add(new BossPhaseData { phaseName = "Phase 2", hpThreshold = 0.75f, description = "Multi-mechanic assault.", atkMultiplier = 2.5f, attackSpeedMultiplier = 1.3f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.ShieldPhase } });
            phases.Add(new BossPhaseData { phaseName = "Phase 3", hpThreshold = 0.5f, description = "Adds and shields.", atkMultiplier = 3f, attackSpeedMultiplier = 1.5f, mechanics = new[] { BossMechanicType.AddSpawning, BossMechanicType.Reflect } });
            phases.Add(new BossPhaseData { phaseName = "Phase 4", hpThreshold = 0.25f, description = "Life steal and enrage.", atkMultiplier = 3.5f, attackSpeedMultiplier = 1.8f, mechanics = new[] { BossMechanicType.LifeSteal, BossMechanicType.Enrage } });
            phases.Add(new BossPhaseData { phaseName = "Phase 5", hpThreshold = 0.1f, description = "Enrage timer. Kill or die.", atkMultiplier = 4f, attackSpeedMultiplier = 2f, mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.Reflect, BossMechanicType.GroundSlam } });
        }

        return phases;
    }

    // ─── EVENT CONFIGS ────────────────────────────────────

    static void CreateEventConfigs()
    {
        CreateEventConfig("FlameFestival", "Flame Festival", "The fires of celebration burn bright! Flame-themed challenges and rewards.",
            SeasonalEventTheme.FlameFestival, Affinity.Flame, "Inferno Tokens", "Eternal Flame Wyrm");
        CreateEventConfig("FrostCarnival", "Frost Carnival", "A winter wonderland of ice and wonder. Frost-themed challenges await.",
            SeasonalEventTheme.FrostCarnival, Affinity.Frost, "Frost Medallions", "Blizzard King");
        CreateEventConfig("StormTournament", "Storm Tournament", "Lightning strikes! Compete in storm-charged combat trials.",
            SeasonalEventTheme.StormTournament, Affinity.Storm, "Thunder Crests", "Stormbreaker Champion");
        CreateEventConfig("NaturesBloom", "Nature's Bloom", "The world blossoms! Harvest-themed gathering and combat events.",
            SeasonalEventTheme.NaturesBloom, Affinity.Nature, "Bloom Petals", "Ancient Treant Lord");
        CreateEventConfig("ShadowMasquerade", "Shadow Masquerade", "Darkness descends! Uncover mysteries in shadow-themed challenges.",
            SeasonalEventTheme.ShadowMasquerade, Affinity.Shadow, "Shadow Masks", "The Masked Phantom");
        CreateEventConfig("RadianceCelebration", "Radiance Celebration", "Light shines eternal! Radiance-themed festivities and rewards.",
            SeasonalEventTheme.RadianceCelebration, Affinity.Radiance, "Radiance Stars", "Celestial Herald");

        Debug.Log("[Phase10] Created 6 Event Configs");
    }

    static void CreateEventConfig(string id, string name, string desc, SeasonalEventTheme theme,
        Affinity affinity, string currencyName, string bossName)
    {
        string path = $"Assets/Data/Events/Configs/{id}.asset";
        var config = AssetDatabase.LoadAssetAtPath<EventConfig>(path);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<EventConfig>();
            AssetDatabase.CreateAsset(config, path);
        }

        config.eventId = id;
        config.eventName = name;
        config.description = desc;
        config.theme = theme;
        config.durationDays = 14;
        config.eventIslandName = $"{name} Island";
        config.eventStageCount = 20;
        config.eventAffinity = affinity;
        config.eventBossName = bossName;
        config.eventBossDescription = $"The final boss of the {name} event.";
        config.eventBossHpMultiplier = 12f;
        config.eventBossAtkMultiplier = 3f;
        config.eventCurrencyName = currencyName;
        config.eventCurrencyDropRate = 1f;

        config.shopItems = new List<EventShopItem>
        {
            new EventShopItem { itemId = $"{id}_gold", itemName = "Gold Cache", description = "10,000 Gold", eventCurrencyCost = 50, rewardCurrencyType = CurrencyType.Gold, rewardAmount = 10000 },
            new EventShopItem { itemId = $"{id}_stardust", itemName = "Stardust Pouch", description = "100 Stardust", eventCurrencyCost = 100, rewardCurrencyType = CurrencyType.Stardust, rewardAmount = 100 },
            new EventShopItem { itemId = $"{id}_aether", itemName = "Aether Crystal", description = "5 Aether Crystals", eventCurrencyCost = 200, rewardCurrencyType = CurrencyType.AetherCrystals, rewardAmount = 5, isLimited = true, stockLimit = 3 },
            new EventShopItem { itemId = $"{id}_cosmetic", itemName = $"{name} Cosmetic", description = $"Exclusive {name} cosmetic item", eventCurrencyCost = 500, isCosmetic = true, isLimited = true, stockLimit = 1 },
        };

        config.eventQuests = new List<EventQuestDef>
        {
            new EventQuestDef { questId = "event_kills", questName = "Event Slayer", description = "Defeat 100 event enemies", requiredProgress = 100, eventCurrencyReward = 50 },
            new EventQuestDef { questId = "event_stages", questName = "Stage Runner", description = "Complete 10 event stages", requiredProgress = 10, eventCurrencyReward = 75 },
        };

        EditorUtility.SetDirty(config);
    }
}
