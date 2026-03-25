using UnityEngine;
using UnityEditor;
using Ascendant.Combat;
using Ascendant.Islands;
using Ascendant.Progression;
using System.Collections.Generic;

public class Phase5DataCreator
{
    [MenuItem("Ascendant/Create Phase 5 Data (Islands, Biomes, Enemies)")]
    public static void CreateAllPhase5Data()
    {
        CreateDirectories();
        var biomes = CreateBiomeData();
        var enemies = CreateEnemyData();
        var lootTables = CreateBossLootTables();
        CreateIslandData(biomes, enemies, lootTables);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Phase5] All data created successfully!");
    }

    static void CreateDirectories()
    {
        EnsureFolder("Assets/Data/Islands");
        EnsureFolder("Assets/Data/Biomes");
        EnsureFolder("Assets/Data/Enemies/Nature");
        EnsureFolder("Assets/Data/Enemies/Flame");
        EnsureFolder("Assets/Data/Enemies/Storm");
        EnsureFolder("Assets/Data/Enemies/Frost");
        EnsureFolder("Assets/Data/Enemies/Shadow");
        EnsureFolder("Assets/Data/Enemies/Radiance");
        EnsureFolder("Assets/Data/BossLoot");
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

    // ─── BIOME DATA ─────────────────────────────────────

    static Dictionary<string, BiomeData> CreateBiomeData()
    {
        var biomes = new Dictionary<string, BiomeData>();

        // Island 1: Verdant Shelf — HP regen
        biomes["VerdantShelf"] = CreateBiome("VerdantShelf", "Verdant Shelf", Affinity.Nature,
            "Healing herbs spawn, granting +5% HP regen",
            BiomeEffectType.HpRegen, 0.05f, 5f, 0f, Affinity.None,
            new Color(0.3f, 0.8f, 0.3f), new Color(0.5f, 1f, 0.5f));

        // Island 2: Ember Plateau — Fire DoT
        biomes["EmberPlateau"] = CreateBiome("EmberPlateau", "Ember Plateau", Affinity.Flame,
            "Fire damage over time (1% HP/5s) to non-Flame heroes",
            BiomeEffectType.DamageOverTime, 0.01f, 5f, 0f, Affinity.Flame,
            new Color(1f, 0.4f, 0.2f), new Color(1f, 0.6f, 0.2f));

        // Island 3: Stormspire Reach — Random AoE lightning
        biomes["StormspireReach"] = CreateBiome("StormspireReach", "Stormspire Reach", Affinity.Storm,
            "Random lightning strikes deal AoE damage to enemies (and occasionally allies)",
            BiomeEffectType.RandomAoE, 0.05f, 4f, 0f, Affinity.Storm,
            new Color(0.5f, 0.3f, 0.8f), new Color(0.8f, 0.8f, 1f));

        // Island 4: Frosthollow Drift — Healing reduced
        biomes["FrosthollowDrift"] = CreateBiome("FrosthollowDrift", "Frosthollow Drift", Affinity.Frost,
            "Healing reduced by 30% for all units; Frost heroes immune",
            BiomeEffectType.HealingModifier, 0f, 1f, -0.3f, Affinity.Frost,
            new Color(0.4f, 0.7f, 1f), new Color(0.8f, 0.9f, 1f));

        // Island 5: Duskwood Crossing — Accuracy debuff
        biomes["DuskwoodCrossing"] = CreateBiome("DuskwoodCrossing", "Duskwood Crossing", Affinity.Shadow,
            "Visibility reduced; -20% accuracy for non-Shadow heroes",
            BiomeEffectType.AccuracyDebuff, 0f, 1f, -0.2f, Affinity.Shadow,
            new Color(0.2f, 0.1f, 0.3f), new Color(0.4f, 0.2f, 0.5f));

        // Island 6: Sunstone Bastion — Healing increased
        biomes["SunstoneBastion"] = CreateBiome("SunstoneBastion", "Sunstone Bastion", Affinity.Radiance,
            "All healing increased by 20%; Radiance heroes gain shields",
            BiomeEffectType.HealingModifier, 0f, 1f, 0.2f, Affinity.None,
            new Color(1f, 0.9f, 0.5f), new Color(1f, 1f, 0.7f));

        // Island 7: Wildthorn Expanse — Periodic rooting
        biomes["WildthornExpanse"] = CreateBiome("WildthornExpanse", "Wildthorn Expanse", Affinity.Nature,
            "Vines periodically root random heroes for 2s; Nature heroes immune",
            BiomeEffectType.PeriodicRooting, 0f, 6f, 2f, Affinity.Nature,
            new Color(0.2f, 0.6f, 0.2f), new Color(0.4f, 0.8f, 0.3f));

        // Island 8: Cinderfall Heights — Enemy fire shields
        biomes["CinderfallHeights"] = CreateBiome("CinderfallHeights", "Cinderfall Heights", Affinity.Flame,
            "Enemies gain fire shields; Flame heroes bypass them",
            BiomeEffectType.EnemyFireShield, 0.15f, 1f, 0f, Affinity.Flame,
            new Color(0.8f, 0.3f, 0.1f), new Color(1f, 0.5f, 0.1f));

        // Island 9: Galebreak Summit — Speed increase
        biomes["GalebreakSummit"] = CreateBiome("GalebreakSummit", "Galebreak Summit", Affinity.Storm,
            "All attack speed increased by 15%; movement randomized by gusts",
            BiomeEffectType.SpeedModifier, 0f, 1f, 0.15f, Affinity.None,
            new Color(0.6f, 0.5f, 0.9f), new Color(0.7f, 0.6f, 1f));

        // Island 10: Crystalveil Tundra — Damage reflect
        biomes["CrystalveilTundra"] = CreateBiome("CrystalveilTundra", "Crystalveil Tundra", Affinity.Frost,
            "Enemies have reflective ice armor (10% damage reflected); Frost heroes bypass",
            BiomeEffectType.DamageReflect, 0.1f, 1f, 0f, Affinity.Frost,
            new Color(0.5f, 0.8f, 1f), new Color(0.7f, 0.9f, 1f));

        // Island 11: Abyssal Hollow — Accuracy debuff
        biomes["AbyssalHollow"] = CreateBiome("AbyssalHollow", "Abyssal Hollow", Affinity.Shadow,
            "Darkness debuff: -20% accuracy for non-Shadow heroes",
            BiomeEffectType.AccuracyDebuff, 0f, 1f, -0.2f, Affinity.Shadow,
            new Color(0.15f, 0.05f, 0.25f), new Color(0.3f, 0.1f, 0.4f));

        // Island 12: Zenith Spire — Radiance damage buff
        biomes["ZenithSpire"] = CreateBiome("ZenithSpire", "Zenith Spire", Affinity.Radiance,
            "Radiance heroes deal +30% damage. The Realm Boss awaits.",
            BiomeEffectType.DamageBuff, 0.3f, 1f, 0f, Affinity.Radiance,
            new Color(1f, 0.95f, 0.6f), new Color(1f, 1f, 0.8f));

        return biomes;
    }

    static BiomeData CreateBiome(string id, string name, Affinity affinity, string desc,
        BiomeEffectType effectType, float effectValue, float tickInterval,
        float modifierValue, Affinity immuneAffinity, Color ambient, Color particle)
    {
        string path = $"Assets/Data/Biomes/{id}Biome.asset";
        var biome = AssetDatabase.LoadAssetAtPath<BiomeData>(path);
        if (biome == null)
        {
            biome = ScriptableObject.CreateInstance<BiomeData>();
            AssetDatabase.CreateAsset(biome, path);
        }

        biome.biomeName = name;
        biome.biomeAffinity = affinity;
        biome.effectDescription = desc;
        biome.effectType = effectType;
        biome.effectValue = effectValue;
        biome.tickInterval = tickInterval;
        biome.modifierValue = modifierValue;
        biome.immuneAffinity = immuneAffinity;
        biome.ambientColor = ambient;
        biome.particleColor = particle;
        EditorUtility.SetDirty(biome);
        return biome;
    }

    // ─── ENEMY DATA ─────────────────────────────────────

    static Dictionary<string, List<EnemyData>> CreateEnemyData()
    {
        var enemies = new Dictionary<string, List<EnemyData>>();

        // Nature enemies (Islands 1, 7)
        enemies["Nature"] = new List<EnemyData>
        {
            CreateEnemy("Nature", "ForestWolf", "Forest Wolf", Affinity.Nature, EnemyCategory.Beast, EnemyAttackType.Melee, 60, 7, 3, 12, 6),
            CreateEnemy("Nature", "VineWraith", "Vine Wraith", Affinity.Nature, EnemyCategory.Ethereal, EnemyAttackType.Ranged, 45, 9, 2, 14, 7),
            CreateEnemy("Nature", "StoneGolem", "Stone Golem", Affinity.Nature, EnemyCategory.Construct, EnemyAttackType.Melee, 100, 5, 8, 10, 5),
            CreateEnemy("Nature", "ThornSprout", "Thorn Sprout", Affinity.Nature, EnemyCategory.Beast, EnemyAttackType.Ranged, 35, 11, 1, 15, 8),
        };

        // Flame enemies (Islands 2, 8)
        enemies["Flame"] = new List<EnemyData>
        {
            CreateEnemy("Flame", "FireImp", "Fire Imp", Affinity.Flame, EnemyCategory.Humanoid, EnemyAttackType.Ranged, 40, 10, 2, 13, 7),
            CreateEnemy("Flame", "LavaBeetle", "Lava Beetle", Affinity.Flame, EnemyCategory.Beast, EnemyAttackType.Melee, 70, 8, 5, 11, 6),
            CreateEnemy("Flame", "EmberConstruct", "Ember Construct", Affinity.Flame, EnemyCategory.Construct, EnemyAttackType.Melee, 90, 6, 7, 10, 5),
            CreateEnemy("Flame", "AshPhantom", "Ash Phantom", Affinity.Flame, EnemyCategory.Ethereal, EnemyAttackType.AoE, 50, 12, 2, 16, 8),
        };

        // Storm enemies (Islands 3, 9)
        enemies["Storm"] = new List<EnemyData>
        {
            CreateEnemy("Storm", "StormHarpy", "Storm Harpy", Affinity.Storm, EnemyCategory.Beast, EnemyAttackType.Ranged, 45, 10, 2, 14, 7),
            CreateEnemy("Storm", "ThunderGuard", "Thunder Guard", Affinity.Storm, EnemyCategory.Armored, EnemyAttackType.Melee, 80, 7, 6, 11, 6),
            CreateEnemy("Storm", "SparkElemental", "Spark Elemental", Affinity.Storm, EnemyCategory.Ethereal, EnemyAttackType.AoE, 55, 11, 2, 15, 8),
            CreateEnemy("Storm", "WindRunner", "Wind Runner", Affinity.Storm, EnemyCategory.Humanoid, EnemyAttackType.Melee, 50, 9, 3, 12, 7),
        };

        // Frost enemies (Islands 4, 10)
        enemies["Frost"] = new List<EnemyData>
        {
            CreateEnemy("Frost", "IceWraith", "Ice Wraith", Affinity.Frost, EnemyCategory.Ethereal, EnemyAttackType.Ranged, 50, 9, 3, 13, 7),
            CreateEnemy("Frost", "FrostBear", "Frost Bear", Affinity.Frost, EnemyCategory.Beast, EnemyAttackType.Melee, 85, 7, 5, 11, 6),
            CreateEnemy("Frost", "CrystalSentinel", "Crystal Sentinel", Affinity.Frost, EnemyCategory.Construct, EnemyAttackType.Melee, 95, 5, 8, 10, 5),
            CreateEnemy("Frost", "BlizzardMage", "Blizzard Mage", Affinity.Frost, EnemyCategory.Humanoid, EnemyAttackType.AoE, 40, 12, 2, 16, 8),
        };

        // Shadow enemies (Islands 5, 11)
        enemies["Shadow"] = new List<EnemyData>
        {
            CreateEnemy("Shadow", "ShadowStalker", "Shadow Stalker", Affinity.Shadow, EnemyCategory.Humanoid, EnemyAttackType.Melee, 55, 10, 3, 14, 7),
            CreateEnemy("Shadow", "DarkSpider", "Dark Spider", Affinity.Shadow, EnemyCategory.Beast, EnemyAttackType.Melee, 45, 8, 4, 12, 6),
            CreateEnemy("Shadow", "SpecterKnight", "Specter Knight", Affinity.Shadow, EnemyCategory.Armored, EnemyAttackType.Melee, 75, 7, 6, 11, 6),
            CreateEnemy("Shadow", "VoidLurker", "Void Lurker", Affinity.Shadow, EnemyCategory.Ethereal, EnemyAttackType.AoE, 50, 11, 2, 15, 8),
        };

        // Radiance enemies (Islands 6, 12)
        enemies["Radiance"] = new List<EnemyData>
        {
            CreateEnemy("Radiance", "LightWarden", "Light Warden", Affinity.Radiance, EnemyCategory.Armored, EnemyAttackType.Melee, 80, 6, 7, 10, 5),
            CreateEnemy("Radiance", "SunBird", "Sun Bird", Affinity.Radiance, EnemyCategory.Beast, EnemyAttackType.Ranged, 40, 11, 2, 15, 8),
            CreateEnemy("Radiance", "HolyAutomaton", "Holy Automaton", Affinity.Radiance, EnemyCategory.Construct, EnemyAttackType.Melee, 90, 5, 8, 10, 5),
            CreateEnemy("Radiance", "CelestialWisp", "Celestial Wisp", Affinity.Radiance, EnemyCategory.Ethereal, EnemyAttackType.Ranged, 35, 12, 1, 16, 8),
        };

        return enemies;
    }

    static EnemyData CreateEnemy(string affinityFolder, string id, string name, Affinity affinity,
        EnemyCategory category, EnemyAttackType attackType, float hp, float atk, float def,
        float gold, float xp)
    {
        string path = $"Assets/Data/Enemies/{affinityFolder}/{id}EnemyData.asset";
        var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (enemy == null)
        {
            enemy = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(enemy, path);
        }

        enemy.enemyName = name;
        enemy.affinity = affinity;
        enemy.category = category;
        enemy.attackType = attackType;
        enemy.baseHp = hp;
        enemy.baseAtk = atk;
        enemy.baseDef = def;
        enemy.baseGoldDrop = gold;
        enemy.baseXpDrop = xp;
        EditorUtility.SetDirty(enemy);
        return enemy;
    }

    // ─── BOSS LOOT TABLES ───────────────────────────────

    static Dictionary<string, BossLootTable> CreateBossLootTables()
    {
        var tables = new Dictionary<string, BossLootTable>();

        tables["Standard"] = CreateLoot("StandardBossLoot", 1000f, 500f, EquipmentRarity.Rare, 0.2f, 0.05f);
        tables["Advanced"] = CreateLoot("AdvancedBossLoot", 2000f, 1000f, EquipmentRarity.Rare, 0.3f, 0.1f);
        tables["RealmBoss"] = CreateLoot("RealmBossLoot", 10000f, 5000f, EquipmentRarity.Epic, 0.5f, 0.25f);

        return tables;
    }

    static BossLootTable CreateLoot(string id, float gold, float xp,
        EquipmentRarity minRarity, float epicChance, float legendaryChance)
    {
        string path = $"Assets/Data/BossLoot/{id}.asset";
        var loot = AssetDatabase.LoadAssetAtPath<BossLootTable>(path);
        if (loot == null)
        {
            loot = ScriptableObject.CreateInstance<BossLootTable>();
            AssetDatabase.CreateAsset(loot, path);
        }

        loot.goldReward = gold;
        loot.xpReward = xp;
        loot.guaranteedMinRarity = minRarity;
        loot.epicDropChance = epicChance;
        loot.legendaryDropChance = legendaryChance;
        EditorUtility.SetDirty(loot);
        return loot;
    }

    // ─── ISLAND DATA ────────────────────────────────────

    static void CreateIslandData(Dictionary<string, BiomeData> biomes,
        Dictionary<string, List<EnemyData>> enemies, Dictionary<string, BossLootTable> loot)
    {
        var islandDefs = new[]
        {
            new IslandDef(1, "Verdant Shelf", Affinity.Nature, "VerdantShelf",
                "Lush floating grasslands with cascading waterfalls. Tutorial island.",
                "Nature", "Verdant Guardian", "A massive tree elemental protecting the first island.",
                5f, 2f, "Standard"),
            new IslandDef(2, "Ember Plateau", Affinity.Flame, "EmberPlateau",
                "Volcanic mesa with rivers of lava and ash clouds.",
                "Flame", "Emberlord Kael", "A fire elemental lord wreathed in molten stone.",
                8f, 2.5f, "Standard"),
            new IslandDef(3, "Stormspire Reach", Affinity.Storm, "StormspireReach",
                "Lightning-struck peaks with perpetual thunderstorms.",
                "Storm", "Tempest Warden", "An ancient storm spirit controlling the lightning.",
                8f, 2.5f, "Standard"),
            new IslandDef(4, "Frosthollow Drift", Affinity.Frost, "FrosthollowDrift",
                "Frozen cavern network inside a massive ice island.",
                "Frost", "Glacial Sovereign", "A frozen monarch encased in living ice.",
                9f, 2.5f, "Standard"),
            new IslandDef(5, "Duskwood Crossing", Affinity.Shadow, "DuskwoodCrossing",
                "Haunted dead forest shrouded in perpetual twilight.",
                "Shadow", "The Hollow One", "A specter commanding shadow forces in the dead wood.",
                9f, 2.5f, "Standard"),
            new IslandDef(6, "Sunstone Bastion", Affinity.Radiance, "SunstoneBastion",
                "Golden temple complex bathed in eternal sunrise.",
                "Radiance", "Solar Archon", "A celestial guardian of the sacred bastion.",
                10f, 3f, "Advanced"),
            new IslandDef(7, "Wildthorn Expanse", Affinity.Nature, "WildthornExpanse",
                "Overgrown jungle island with enormous flora.",
                "Nature", "Briarmother", "A colossal thorned creature born from jungle overgrowth.",
                10f, 3f, "Advanced"),
            new IslandDef(8, "Cinderfall Heights", Affinity.Flame, "CinderfallHeights",
                "Magma rivers flowing between floating obsidian pillars.",
                "Flame", "Obsidian Tyrant", "An armored fire giant forged from volcanic glass.",
                11f, 3f, "Advanced"),
            new IslandDef(9, "Galebreak Summit", Affinity.Storm, "GalebreakSummit",
                "Wind-torn cliffs above the cloudline.",
                "Storm", "Stormcaller Prime", "The apex storm entity controlling all winds.",
                11f, 3f, "Advanced"),
            new IslandDef(10, "Crystalveil Tundra", Affinity.Frost, "CrystalveilTundra",
                "Crystalline ice fields that refract light into rainbows.",
                "Frost", "Prism Wyrm", "An ice dragon with crystalline scales that refract light.",
                12f, 3.5f, "Advanced"),
            new IslandDef(11, "Abyssal Hollow", Affinity.Shadow, "AbyssalHollow",
                "Deep caverns descending into a floating island's core.",
                "Shadow", "Void Harbinger", "An entity of pure darkness from the island's depths.",
                12f, 3.5f, "Advanced"),
            new IslandDef(12, "Zenith Spire", Affinity.Radiance, "ZenithSpire",
                "The culmination of Realm 1: a radiant citadel in the clouds.",
                "Radiance", "The Radiant Guardian", "A towering angelic construct guarding the boundary between Realms.",
                15f, 4f, "RealmBoss"),
        };

        IslandData previousIsland = null;
        for (int i = 0; i < islandDefs.Length; i++)
        {
            var def = islandDefs[i];
            string path = $"Assets/Data/Islands/Island{def.Number:D2}_{def.BiomeKey}.asset";

            var island = AssetDatabase.LoadAssetAtPath<IslandData>(path);
            if (island == null)
            {
                island = ScriptableObject.CreateInstance<IslandData>();
                AssetDatabase.CreateAsset(island, path);
            }

            island.islandName = def.Name;
            island.islandNumber = def.Number;
            island.realmNumber = 1;
            island.description = def.Description;
            island.affinity = def.Affinity;
            island.stageCount = 100;
            island.islandBossName = def.BossName;
            island.islandBossDescription = def.BossDesc;
            island.islandBossHpMultiplier = def.BossHpMult;
            island.islandBossAtkMultiplier = def.BossAtkMult;
            island.miniBossHpMultiplier = 3f;
            island.miniBossAtkMultiplier = 1.5f;
            island.miniBossGoldMultiplier = 2f;
            island.miniBossXpMultiplier = 2f;
            island.goldMultiplier = 1f + (def.Number - 1) * 0.1f;
            island.xpMultiplier = 1f + (def.Number - 1) * 0.08f;

            if (biomes.TryGetValue(def.BiomeKey, out var biome))
                island.biomeData = biome;

            if (enemies.TryGetValue(def.EnemyKey, out var enemyList))
                island.enemyTypes = new List<EnemyData>(enemyList);

            island.previousIsland = previousIsland;

            // Create boss phases
            island.bossPhases = CreateDefaultBossPhases(def.Number);

            EditorUtility.SetDirty(island);
            previousIsland = island;
        }
    }

    static List<BossPhaseData> CreateDefaultBossPhases(int islandNumber)
    {
        var phases = new List<BossPhaseData>();

        if (islandNumber == 2) // Emberlord Kael — the example boss
        {
            phases.Add(new BossPhaseData
            {
                phaseName = "Fire Breath",
                hpThreshold = 1f,
                description = "Basic melee + periodic fire breath. Enrages after 60s.",
                atkMultiplier = 1f,
                attackSpeedMultiplier = 1f,
                mechanics = new[] { BossMechanicType.GroundSlam, BossMechanicType.Enrage }
            });
            phases.Add(new BossPhaseData
            {
                phaseName = "Summon Elementals",
                hpThreshold = 0.6f,
                description = "Summons fire elementals. Fire breath leaves burning ground.",
                atkMultiplier = 1.5f,
                attackSpeedMultiplier = 1.2f,
                mechanics = new[] { BossMechanicType.AddSpawning }
            });
            phases.Add(new BossPhaseData
            {
                phaseName = "Inferno",
                hpThreshold = 0.3f,
                description = "Absorbs elementals. Attack speed doubles. Massive fireballs.",
                atkMultiplier = 2f,
                attackSpeedMultiplier = 2f,
                mechanics = new[] { BossMechanicType.Enrage }
            });
        }
        else
        {
            // Generic 3-phase boss pattern
            phases.Add(new BossPhaseData
            {
                phaseName = "Phase 1",
                hpThreshold = 1f,
                description = "Standard attacks with one mechanic.",
                atkMultiplier = 1f,
                attackSpeedMultiplier = 1f,
                mechanics = new[] { BossMechanicType.GroundSlam }
            });
            phases.Add(new BossPhaseData
            {
                phaseName = "Phase 2",
                hpThreshold = 0.6f,
                description = "Enhanced attacks with adds.",
                atkMultiplier = 1.5f,
                attackSpeedMultiplier = 1.3f,
                mechanics = new[] { BossMechanicType.AddSpawning, BossMechanicType.Enrage }
            });
            phases.Add(new BossPhaseData
            {
                phaseName = "Phase 3",
                hpThreshold = 0.3f,
                description = "Enraged final stand.",
                atkMultiplier = 2f,
                attackSpeedMultiplier = 1.5f,
                mechanics = new[] { BossMechanicType.Enrage, BossMechanicType.Reflect }
            });
        }

        return phases;
    }

    struct IslandDef
    {
        public int Number;
        public string Name;
        public Affinity Affinity;
        public string BiomeKey;
        public string Description;
        public string EnemyKey;
        public string BossName;
        public string BossDesc;
        public float BossHpMult;
        public float BossAtkMult;
        public string LootKey;

        public IslandDef(int num, string name, Affinity aff, string biome, string desc,
            string enemies, string bossName, string bossDesc, float bossHp, float bossAtk, string loot)
        {
            Number = num; Name = name; Affinity = aff; BiomeKey = biome; Description = desc;
            EnemyKey = enemies; BossName = bossName; BossDesc = bossDesc;
            BossHpMult = bossHp; BossAtkMult = bossAtk; LootKey = loot;
        }
    }
}
