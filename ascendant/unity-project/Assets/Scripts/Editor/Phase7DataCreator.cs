#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Editor
{
    public static class Phase7DataCreator
    {
        [MenuItem("Ascendant/Create Phase 7 Data (20 Classes + 13 Combos)")]
        public static void CreateAllPhase7Data()
        {
            CreateHeroDataAssets();
            CreateComboAbilityAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase7] All hero data and combo ability assets created.");
        }

        [MenuItem("Ascendant/Create Phase 7 Hero Data Only")]
        public static void CreateHeroDataAssets()
        {
            string heroPath = "Assets/Data/Heroes";
            EnsureDirectory(heroPath);

            // --- Tier 2: Apprentice Classes ---
            CreateHero(heroPath, "Marksman", "marksman", HeroRole.Ranger, HeroPosition.Backline,
                Affinity.Frost, 18f, 8f, 85f, 1.1f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.Medium, 8f);

            CreateHero(heroPath, "Defender", "defender", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Frost, 10f, 18f, 150f, 0.8f,
                StatGrowth.Low, StatGrowth.High, StatGrowth.High, StatGrowth.Low, 3f);

            CreateHero(heroPath, "Berserker", "berserker", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Flame, 20f, 6f, 120f, 1.2f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Medium, StatGrowth.Medium, 7f);

            CreateHero(heroPath, "Druid", "druid", HeroRole.Support, HeroPosition.Backline,
                Affinity.Nature, 12f, 10f, 100f, 1f,
                StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Low, 5f);

            CreateHero(heroPath, "Thief", "thief", HeroRole.Striker, HeroPosition.Frontline,
                Affinity.Shadow, 16f, 8f, 90f, 1.3f,
                StatGrowth.Medium, StatGrowth.Low, StatGrowth.Low, StatGrowth.High, 6f);

            CreateHero(heroPath, "Shaman", "shaman", HeroRole.Support, HeroPosition.Backline,
                Affinity.Storm, 14f, 10f, 95f, 1f,
                StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Low, 5f);

            // --- Tier 3: Adept Classes ---
            CreateHero(heroPath, "Warlock", "warlock", HeroRole.Caster, HeroPosition.Backline,
                Affinity.Flame, 22f, 5f, 80f, 1f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.Medium, 9f);

            CreateHero(heroPath, "Ranger", "ranger", HeroRole.Ranger, HeroPosition.Backline,
                Affinity.Storm, 17f, 9f, 95f, 1.1f,
                StatGrowth.High, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, 7f);

            CreateHero(heroPath, "Spell-Blade", "spellblade", HeroRole.Striker, HeroPosition.Frontline,
                Affinity.Storm, 19f, 10f, 100f, 1.2f,
                StatGrowth.High, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, 7f);

            CreateHero(heroPath, "Necromancer", "necromancer", HeroRole.Caster, HeroPosition.Backline,
                Affinity.Shadow, 18f, 7f, 85f, 0.9f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.Low, 8f);

            CreateHero(heroPath, "Monk", "monk", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Storm, 17f, 12f, 110f, 1.3f,
                StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.High, 6f);

            CreateHero(heroPath, "Paladin", "paladin", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Radiance, 14f, 15f, 130f, 0.9f,
                StatGrowth.Medium, StatGrowth.High, StatGrowth.High, StatGrowth.Low, 5f);

            CreateHero(heroPath, "Bard", "bard", HeroRole.Support, HeroPosition.Backline,
                Affinity.Radiance, 12f, 8f, 90f, 1f,
                StatGrowth.Medium, StatGrowth.Low, StatGrowth.Medium, StatGrowth.Medium, 4f);

            // --- Tier 4: Master Classes ---
            CreateHero(heroPath, "Dragon-Hunter", "dragonhunter", HeroRole.Specialist, HeroPosition.Frontline,
                Affinity.Nature, 20f, 10f, 110f, 1f,
                StatGrowth.High, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, 8f);

            CreateHero(heroPath, "Summoner", "summoner", HeroRole.Specialist, HeroPosition.Backline,
                Affinity.Nature, 14f, 8f, 90f, 1f,
                StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, StatGrowth.Medium, 5f);

            CreateHero(heroPath, "Alchemist", "alchemist", HeroRole.Support, HeroPosition.Backline,
                Affinity.Flame, 15f, 8f, 90f, 1f,
                StatGrowth.Medium, StatGrowth.Low, StatGrowth.Medium, StatGrowth.Medium, 5f);

            CreateHero(heroPath, "Chronomancer", "chronomancer", HeroRole.Caster, HeroPosition.Backline,
                Affinity.Frost, 16f, 7f, 85f, 1f,
                StatGrowth.Medium, StatGrowth.Low, StatGrowth.Low, StatGrowth.Medium, 7f);

            CreateHero(heroPath, "Gunslinger", "gunslinger", HeroRole.Ranger, HeroPosition.Backline,
                Affinity.Flame, 19f, 6f, 80f, 1.5f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.High, 8f);

            CreateHero(heroPath, "Warden", "warden", HeroRole.Vanguard, HeroPosition.Frontline,
                Affinity.Nature, 12f, 16f, 140f, 0.8f,
                StatGrowth.Low, StatGrowth.High, StatGrowth.High, StatGrowth.Low, 4f);

            CreateHero(heroPath, "Reaper", "reaper", HeroRole.Striker, HeroPosition.Frontline,
                Affinity.Shadow, 21f, 7f, 90f, 1.2f,
                StatGrowth.High, StatGrowth.Low, StatGrowth.Low, StatGrowth.Medium, 9f);

            Debug.Log("[Phase7] 20 HeroData assets created.");
        }

        static void CreateHero(string path, string heroName, string classId,
            HeroRole role, HeroPosition position, Affinity affinity,
            float baseAtk, float baseDef, float baseHp, float baseSpd,
            StatGrowth atkGrowth, StatGrowth defGrowth, StatGrowth hpGrowth, StatGrowth spdGrowth,
            float baseTapBonus)
        {
            string assetPath = $"{path}/{classId}HeroData.asset";
            var existing = AssetDatabase.LoadAssetAtPath<HeroData>(assetPath);
            if (existing != null) return; // Don't overwrite

            var data = ScriptableObject.CreateInstance<HeroData>();
            data.heroName = heroName;
            data.className = heroName;
            data.classId = classId;
            data.role = role;
            data.position = position;
            data.affinity = affinity;
            data.baseAtk = baseAtk;
            data.baseDef = baseDef;
            data.baseHp = baseHp;
            data.baseSpd = baseSpd;
            data.atkGrowth = atkGrowth;
            data.defGrowth = defGrowth;
            data.hpGrowth = hpGrowth;
            data.spdGrowth = spdGrowth;
            data.baseTapBonus = baseTapBonus;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        [MenuItem("Ascendant/Create Phase 7 Combo Data Only")]
        public static void CreateComboAbilityAssets()
        {
            string comboPath = "Assets/Data/Combos";
            EnsureDirectory(comboPath);

            // 13 defined combo abilities from the design doc
            CreateCombo(comboPath, "arcane_cleave", "Arcane Cleave",
                "warrior", "mage",
                "Enchanted blade wave dealing physical + magic damage to all enemies",
                Party.ComboTargetType.AllEnemies, 4f, DamageType.True, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "soul_judgment", "Soul Judgment",
                "priest", "necromancer",
                "Massive holy/dark explosion; heals party for damage dealt",
                Party.ComboTargetType.Hybrid, 5f, DamageType.True, 0.3f, 0f, 0f, 1f);

            CreateCombo(comboPath, "pinpoint_ambush", "Pinpoint Ambush",
                "rogue", "marksman",
                "Guaranteed critical strike from both, +100% crit damage",
                Party.ComboTargetType.SingleEnemy, 8f, DamageType.Physical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "wild_rampage", "Wild Rampage",
                "berserker", "druid",
                "Berserker shapeshifts into a flame bear for 8s, dealing massive AoE",
                Party.ComboTargetType.AllEnemies, 6f, DamageType.Physical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "unbreakable_wall", "Unbreakable Wall",
                "defender", "paladin",
                "Both gain immunity for 4s; all enemy aggro locked to them",
                Party.ComboTargetType.PartyBuff, 0f, DamageType.Physical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "harmonic_edge", "Harmonic Edge",
                "spellblade", "bard",
                "Every melee strike plays a damaging chord; every chord enhances the next strike",
                Party.ComboTargetType.AllEnemies, 5f, DamageType.Magical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "final_judgment", "Final Judgment",
                "monk", "reaper",
                "10-hit combo where each strike executes enemies below escalating thresholds",
                Party.ComboTargetType.AllEnemies, 7f, DamageType.True, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "primal_call", "Primal Call",
                "shaman", "summoner",
                "All totems and familiars merge into a massive elemental titan for 10s",
                Party.ComboTargetType.AllEnemies, 8f, DamageType.Magical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "temporal_curse", "Temporal Curse",
                "warlock", "chronomancer",
                "Curse applied to all enemies AND rewound to apply again 3s later",
                Party.ComboTargetType.AllEnemies, 6f, DamageType.Magical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "apex_predator", "Apex Predator",
                "dragonhunter", "ranger",
                "Both heroes and all pets focus one target for 500% combined damage/s for 5s",
                Party.ComboTargetType.SingleEnemy, 10f, DamageType.Physical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "heist", "Heist",
                "thief", "gunslinger",
                "Enemies drop 10x gold; Gunslinger fires celebratory rounds",
                Party.ComboTargetType.AllEnemies, 3f, DamageType.Physical, 0f, 0f, 0f, 10f);

            CreateCombo(comboPath, "volatile_arcana", "Volatile Arcana",
                "mage", "alchemist",
                "Spells become potions and potions become spells for 10s",
                Party.ComboTargetType.AllEnemies, 5f, DamageType.Magical, 0f, 0f, 0f, 1f);

            CreateCombo(comboPath, "cycle_life_death", "Cycle of Life and Death",
                "warden", "necromancer",
                "Roots grow from corpses; each root heals allies and spawns a nature minion",
                Party.ComboTargetType.Hybrid, 4f, DamageType.Magical, 0.2f, 0f, 0f, 1f);

            Debug.Log("[Phase7] 13 ComboAbilityData assets created.");
        }

        static void CreateCombo(string path, string comboId, string comboName,
            string classIdA, string classIdB, string description,
            Party.ComboTargetType targetType, float damageMultiplier,
            DamageType damageType, float healPercent, float buffMultiplier,
            float buffDuration, float goldMultiplier)
        {
            string assetPath = $"{path}/{comboId}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Party.ComboAbilityData>(assetPath);
            if (existing != null) return;

            var data = ScriptableObject.CreateInstance<Party.ComboAbilityData>();
            data.comboId = comboId;
            data.comboName = comboName;
            data.classIdA = classIdA;
            data.classIdB = classIdB;
            data.description = description;
            data.targetType = targetType;
            data.damageMultiplier = damageMultiplier;
            data.damageType = damageType;
            data.healPercent = healPercent;
            data.buffMultiplier = buffMultiplier;
            data.buffDuration = buffDuration;
            data.goldMultiplier = goldMultiplier;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
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
        }
    }
}
#endif
