#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Editor
{
    public static class Phase2DataCreator
    {
        [MenuItem("Ascendant/Create Phase 2 Data")]
        public static void CreateAll()
        {
            CreateHeroData();
            CreateAbilityData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Phase 2 ScriptableObject data created successfully!");
        }

        [MenuItem("Ascendant/Create Phase 2 Hero Data")]
        static void CreateHeroData()
        {
            // --- MAGE ---
            var mage = ScriptableObject.CreateInstance<HeroData>();
            mage.heroName = "Mage";
            mage.className = "Mage";
            mage.classId = "mage";
            mage.role = HeroRole.Caster;
            mage.position = HeroPosition.Backline;
            mage.affinity = Affinity.Frost;
            mage.baseAtk = 18f;
            mage.baseDef = 6f;
            mage.baseHp = 80f;
            mage.baseSpd = 0.8f;
            mage.atkGrowth = StatGrowth.High;
            mage.defGrowth = StatGrowth.Low;
            mage.hpGrowth = StatGrowth.Low;
            mage.spdGrowth = StatGrowth.Medium;
            mage.baseTapBonus = 8f;
            CreateAsset(mage, "Assets/Data/Heroes/MageHeroData.asset");

            // --- PRIEST ---
            var priest = ScriptableObject.CreateInstance<HeroData>();
            priest.heroName = "Priest";
            priest.className = "Priest";
            priest.classId = "priest";
            priest.role = HeroRole.Support;
            priest.position = HeroPosition.Backline;
            priest.affinity = Affinity.Radiance;
            priest.baseAtk = 10f;
            priest.baseDef = 8f;
            priest.baseHp = 110f;
            priest.baseSpd = 0.7f;
            priest.atkGrowth = StatGrowth.Low;
            priest.defGrowth = StatGrowth.Medium;
            priest.hpGrowth = StatGrowth.Medium;
            priest.spdGrowth = StatGrowth.Low;
            priest.baseTapBonus = 4f;
            CreateAsset(priest, "Assets/Data/Heroes/PriestHeroData.asset");

            // --- ROGUE ---
            var rogue = ScriptableObject.CreateInstance<HeroData>();
            rogue.heroName = "Rogue";
            rogue.className = "Rogue";
            rogue.classId = "rogue";
            rogue.role = HeroRole.Striker;
            rogue.position = HeroPosition.Frontline;
            rogue.affinity = Affinity.Shadow;
            rogue.baseAtk = 20f;
            rogue.baseDef = 7f;
            rogue.baseHp = 85f;
            rogue.baseSpd = 1.3f;
            rogue.atkGrowth = StatGrowth.High;
            rogue.defGrowth = StatGrowth.Low;
            rogue.hpGrowth = StatGrowth.Low;
            rogue.spdGrowth = StatGrowth.High;
            rogue.baseTapBonus = 7f;
            CreateAsset(rogue, "Assets/Data/Heroes/RogueHeroData.asset");

            Debug.Log("Hero data created: Mage, Priest, Rogue");
        }

        [MenuItem("Ascendant/Create Phase 2 Ability Data")]
        static void CreateAbilityData()
        {
            // === MAGE ABILITIES ===

            var frostNova = ScriptableObject.CreateInstance<Ability>();
            frostNova.abilityName = "Frost Nova";
            frostNova.description = "Deals 150% ATK in an AoE circle; freezes all enemies hit for 1.5s";
            frostNova.slotIndex = 0;
            frostNova.damageMultiplier = 1.5f;
            frostNova.cooldown = 10f;
            frostNova.targetType = AbilityTargetType.AllEnemies;
            frostNova.isUltimate = false;
            frostNova.stunDuration = 1.5f;
            CreateAsset(frostNova, "Assets/Data/Abilities/Mage_FrostNova.asset");

            var blizzard = ScriptableObject.CreateInstance<Ability>();
            blizzard.abilityName = "Blizzard";
            blizzard.description = "Channels a storm over 5s, dealing 50% ATK/s to all enemies and slowing them 30%";
            blizzard.slotIndex = 1;
            blizzard.damageMultiplier = 0.5f; // per second, 5s duration
            blizzard.cooldown = 25f;
            blizzard.targetType = AbilityTargetType.AllEnemies;
            blizzard.isUltimate = false;
            blizzard.buffDuration = 5f;
            blizzard.buffMultiplier = 0.3f; // 30% slow
            CreateAsset(blizzard, "Assets/Data/Abilities/Mage_Blizzard.asset");

            var absoluteZero = ScriptableObject.CreateInstance<Ability>();
            absoluteZero.abilityName = "Absolute Zero";
            absoluteZero.description = "Massive ice explosion dealing 1000% ATK to all enemies; frozen enemies take triple damage";
            absoluteZero.slotIndex = 2;
            absoluteZero.damageMultiplier = 10f;
            absoluteZero.cooldown = 0f;
            absoluteZero.targetType = AbilityTargetType.AllEnemies;
            absoluteZero.isUltimate = true;
            absoluteZero.chargeRequired = 100f;
            CreateAsset(absoluteZero, "Assets/Data/Abilities/Mage_AbsoluteZero.asset");

            // === PRIEST ABILITIES ===

            var healingLight = ScriptableObject.CreateInstance<Ability>();
            healingLight.abilityName = "Healing Light";
            healingLight.description = "Heals all allies for 15% of their max HP";
            healingLight.slotIndex = 0;
            healingLight.damageMultiplier = 0f;
            healingLight.cooldown = 8f;
            healingLight.targetType = AbilityTargetType.PartyBuff;
            healingLight.isUltimate = false;
            healingLight.buffMultiplier = 0.15f; // 15% max HP heal
            CreateAsset(healingLight, "Assets/Data/Abilities/Priest_HealingLight.asset");

            var sacredShield = ScriptableObject.CreateInstance<Ability>();
            sacredShield.abilityName = "Sacred Shield";
            sacredShield.description = "Places a shield on the lowest-HP ally absorbing damage equal to 20% of Priest's max HP for 8s";
            sacredShield.slotIndex = 1;
            sacredShield.damageMultiplier = 0f;
            sacredShield.cooldown = 15f;
            sacredShield.targetType = AbilityTargetType.PartyBuff;
            sacredShield.isUltimate = false;
            sacredShield.buffMultiplier = 0.2f; // 20% of Priest max HP
            sacredShield.buffDuration = 8f;
            CreateAsset(sacredShield, "Assets/Data/Abilities/Priest_SacredShield.asset");

            var resurrection = ScriptableObject.CreateInstance<Ability>();
            resurrection.abilityName = "Resurrection";
            resurrection.description = "Revives all fallen allies at 50% HP and grants the party invincibility for 3s";
            resurrection.slotIndex = 2;
            resurrection.damageMultiplier = 0f;
            resurrection.cooldown = 0f;
            resurrection.targetType = AbilityTargetType.PartyBuff;
            resurrection.isUltimate = true;
            resurrection.chargeRequired = 100f;
            resurrection.buffDuration = 3f;
            CreateAsset(resurrection, "Assets/Data/Abilities/Priest_Resurrection.asset");

            // === ROGUE ABILITIES ===

            var ambush = ScriptableObject.CreateInstance<Ability>();
            ambush.abilityName = "Ambush";
            ambush.description = "Teleport behind the highest-ATK enemy, deal 250% ATK, and gain 3 Combo Points";
            ambush.slotIndex = 0;
            ambush.damageMultiplier = 2.5f;
            ambush.cooldown = 10f;
            ambush.targetType = AbilityTargetType.SingleEnemy;
            ambush.isUltimate = false;
            CreateAsset(ambush, "Assets/Data/Abilities/Rogue_Ambush.asset");

            var smokeBomb = ScriptableObject.CreateInstance<Ability>();
            smokeBomb.abilityName = "Smoke Bomb";
            smokeBomb.description = "Party gains 30% dodge chance for 5s; Rogue becomes untargetable for 3s";
            smokeBomb.slotIndex = 1;
            smokeBomb.damageMultiplier = 0f;
            smokeBomb.cooldown = 18f;
            smokeBomb.targetType = AbilityTargetType.PartyBuff;
            smokeBomb.isUltimate = false;
            smokeBomb.buffMultiplier = 0.3f; // 30% dodge
            smokeBomb.buffDuration = 5f;
            CreateAsset(smokeBomb, "Assets/Data/Abilities/Rogue_SmokeBomb.asset");

            var deathsEmbrace = ScriptableObject.CreateInstance<Ability>();
            deathsEmbrace.abilityName = "Death's Embrace";
            deathsEmbrace.description = "10 rapid strikes on the target, each dealing 150% ATK; if the target dies, cooldown resets";
            deathsEmbrace.slotIndex = 2;
            deathsEmbrace.damageMultiplier = 1.5f; // per strike, 10 strikes
            deathsEmbrace.cooldown = 0f;
            deathsEmbrace.targetType = AbilityTargetType.SingleEnemy;
            deathsEmbrace.isUltimate = true;
            deathsEmbrace.chargeRequired = 100f;
            CreateAsset(deathsEmbrace, "Assets/Data/Abilities/Rogue_DeathsEmbrace.asset");

            Debug.Log("Ability data created: Mage (3), Priest (3), Rogue (3)");
        }

        static void CreateAsset(Object obj, string path)
        {
            // Ensure directory exists
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string[] parts = dir.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    currentPath = nextPath;
                }
            }

            AssetDatabase.CreateAsset(obj, path);
        }
    }
}
#endif
