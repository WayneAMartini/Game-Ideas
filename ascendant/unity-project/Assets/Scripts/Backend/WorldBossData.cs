using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Combat;
using Ascendant.Islands;

namespace Ascendant.Backend
{
    [CreateAssetMenu(fileName = "WorldBossData", menuName = "Ascendant/Backend/World Boss Data")]
    public class WorldBossData : ScriptableObject
    {
        [Header("Boss Identity")]
        public string bossId;
        public string bossName;
        public string description;
        public Affinity affinity;
        public Sprite bossSprite;

        [Header("Stats")]
        public double globalHpPool = 100_000_000_000; // 100B HP
        public float baseAtk = 5000f;
        public float baseDef = 2000f;

        [Header("Phases")]
        public List<WorldBossPhase> phases = new();

        [Header("Rewards")]
        public WorldBossRewardTable bronzeRewards;
        public WorldBossRewardTable silverRewards;
        public WorldBossRewardTable goldRewards;
        public WorldBossRewardTable diamondRewards;

        [Header("Visual")]
        public Color themeColor = Color.red;
        public string environmentEffect;
    }

    [Serializable]
    public class WorldBossPhase
    {
        public string phaseName;
        [Range(0f, 1f)]
        public float hpThreshold = 1f; // Phase starts at this HP %
        public float atkMultiplier = 1f;
        public float damageModifier = 1f; // Player damage multiplier in this phase
        public List<BossMechanicType> mechanics = new();
        public string phaseDescription;
    }

    [Serializable]
    public class WorldBossRewardTable
    {
        public int gold;
        public int stardust;
        public int aetherCrystals;
        public int guildCoins;
        public int classTokens;
        public float epicEquipmentChance;
        public float legendaryEquipmentChance;
    }

    public static class WorldBossRotation
    {
        static readonly WorldBossTemplate[] Templates =
        {
            new()
            {
                BossId = "inferno_titan",
                BossName = "Inferno Titan",
                Description = "A colossal titan wreathed in eternal flames. Its molten core threatens to ignite the sky islands.",
                Affinity = Affinity.Flame,
                GlobalHp = 100_000_000_000,
                BaseAtk = 5000f,
                BaseDef = 2000f,
                ThemeColor = new Color(1f, 0.3f, 0f),
                Phases = new[]
                {
                    new PhaseTemplate("Awakening", 1f, 1f, 1f, new[] { BossMechanicType.Enrage }),
                    new PhaseTemplate("Inferno", 0.7f, 1.3f, 0.9f, new[] { BossMechanicType.GroundSlam, BossMechanicType.Enrage }),
                    new PhaseTemplate("Meltdown", 0.3f, 1.6f, 0.8f, new[] { BossMechanicType.AddSpawning, BossMechanicType.LifeSteal })
                },
                BronzeRewards = new WorldBossRewardTable { gold = 5000, stardust = 50, aetherCrystals = 5, guildCoins = 100 },
                SilverRewards = new WorldBossRewardTable { gold = 15000, stardust = 150, aetherCrystals = 15, guildCoins = 300, epicEquipmentChance = 0.1f },
                GoldRewards = new WorldBossRewardTable { gold = 50000, stardust = 500, aetherCrystals = 50, guildCoins = 1000, epicEquipmentChance = 0.3f, legendaryEquipmentChance = 0.05f },
                DiamondRewards = new WorldBossRewardTable { gold = 200000, stardust = 2000, aetherCrystals = 200, guildCoins = 5000, epicEquipmentChance = 0.5f, legendaryEquipmentChance = 0.15f }
            },
            new()
            {
                BossId = "frost_leviathan",
                BossName = "Frost Leviathan",
                Description = "An ancient serpent of living ice that rises from the frozen depths between islands.",
                Affinity = Affinity.Frost,
                GlobalHp = 120_000_000_000,
                BaseAtk = 4500f,
                BaseDef = 2500f,
                ThemeColor = new Color(0.3f, 0.7f, 1f),
                Phases = new[]
                {
                    new PhaseTemplate("Emergence", 1f, 1f, 1f, new[] { BossMechanicType.ShieldPhase }),
                    new PhaseTemplate("Blizzard", 0.6f, 1.2f, 0.85f, new[] { BossMechanicType.Reflect, BossMechanicType.ShieldPhase }),
                    new PhaseTemplate("Absolute Zero", 0.25f, 1.5f, 0.7f, new[] { BossMechanicType.Enrage, BossMechanicType.LifeSteal })
                },
                BronzeRewards = new WorldBossRewardTable { gold = 5000, stardust = 50, aetherCrystals = 5, guildCoins = 100 },
                SilverRewards = new WorldBossRewardTable { gold = 15000, stardust = 150, aetherCrystals = 15, guildCoins = 300, epicEquipmentChance = 0.1f },
                GoldRewards = new WorldBossRewardTable { gold = 50000, stardust = 500, aetherCrystals = 50, guildCoins = 1000, epicEquipmentChance = 0.3f, legendaryEquipmentChance = 0.05f },
                DiamondRewards = new WorldBossRewardTable { gold = 200000, stardust = 2000, aetherCrystals = 200, guildCoins = 5000, epicEquipmentChance = 0.5f, legendaryEquipmentChance = 0.15f }
            },
            new()
            {
                BossId = "storm_colossus",
                BossName = "Storm Colossus",
                Description = "A towering golem forged from thunderclouds and lightning. It commands the very storms that rage between islands.",
                Affinity = Affinity.Storm,
                GlobalHp = 110_000_000_000,
                BaseAtk = 5500f,
                BaseDef = 1800f,
                ThemeColor = new Color(0.8f, 0.8f, 0.2f),
                Phases = new[]
                {
                    new PhaseTemplate("Gathering Storm", 1f, 1f, 1f, new[] { BossMechanicType.GroundSlam }),
                    new PhaseTemplate("Tempest", 0.65f, 1.4f, 0.85f, new[] { BossMechanicType.Split, BossMechanicType.GroundSlam }),
                    new PhaseTemplate("Supercell", 0.2f, 1.8f, 0.75f, new[] { BossMechanicType.AddSpawning, BossMechanicType.Enrage })
                },
                BronzeRewards = new WorldBossRewardTable { gold = 5000, stardust = 50, aetherCrystals = 5, guildCoins = 100 },
                SilverRewards = new WorldBossRewardTable { gold = 15000, stardust = 150, aetherCrystals = 15, guildCoins = 300, epicEquipmentChance = 0.1f },
                GoldRewards = new WorldBossRewardTable { gold = 50000, stardust = 500, aetherCrystals = 50, guildCoins = 1000, epicEquipmentChance = 0.3f, legendaryEquipmentChance = 0.05f },
                DiamondRewards = new WorldBossRewardTable { gold = 200000, stardust = 2000, aetherCrystals = 200, guildCoins = 5000, epicEquipmentChance = 0.5f, legendaryEquipmentChance = 0.15f }
            },
            new()
            {
                BossId = "shadow_behemoth",
                BossName = "Shadow Behemoth",
                Description = "A nightmarish creature born from the void between worlds. It devours light and hope alike.",
                Affinity = Affinity.Shadow,
                GlobalHp = 90_000_000_000,
                BaseAtk = 6000f,
                BaseDef = 1500f,
                ThemeColor = new Color(0.4f, 0f, 0.6f),
                Phases = new[]
                {
                    new PhaseTemplate("Darkness Falls", 1f, 1f, 1f, new[] { BossMechanicType.LifeSteal }),
                    new PhaseTemplate("Void Hunger", 0.5f, 1.5f, 0.9f, new[] { BossMechanicType.LifeSteal, BossMechanicType.Reflect }),
                    new PhaseTemplate("Annihilation", 0.15f, 2.0f, 0.7f, new[] { BossMechanicType.Enrage, BossMechanicType.AddSpawning, BossMechanicType.LifeSteal })
                },
                BronzeRewards = new WorldBossRewardTable { gold = 5000, stardust = 50, aetherCrystals = 5, guildCoins = 100 },
                SilverRewards = new WorldBossRewardTable { gold = 15000, stardust = 150, aetherCrystals = 15, guildCoins = 300, epicEquipmentChance = 0.1f },
                GoldRewards = new WorldBossRewardTable { gold = 50000, stardust = 500, aetherCrystals = 50, guildCoins = 1000, epicEquipmentChance = 0.3f, legendaryEquipmentChance = 0.05f },
                DiamondRewards = new WorldBossRewardTable { gold = 200000, stardust = 2000, aetherCrystals = 200, guildCoins = 5000, epicEquipmentChance = 0.5f, legendaryEquipmentChance = 0.15f }
            }
        };

        public static WorldBossTemplate GetCurrentBoss()
        {
            // Rotate weekly based on current UTC week number
            int weekNumber = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (7 * 24 * 3600));
            int index = weekNumber % Templates.Length;
            return Templates[index];
        }

        public static WorldBossTemplate GetBossById(string bossId)
        {
            foreach (var t in Templates)
                if (t.BossId == bossId) return t;
            return Templates[0];
        }

        public struct PhaseTemplate
        {
            public string Name;
            public float HpThreshold;
            public float AtkMultiplier;
            public float DamageModifier;
            public BossMechanicType[] Mechanics;

            public PhaseTemplate(string name, float hpThreshold, float atkMult, float damageMod, BossMechanicType[] mechanics)
            {
                Name = name;
                HpThreshold = hpThreshold;
                AtkMultiplier = atkMult;
                DamageModifier = damageMod;
                Mechanics = mechanics;
            }
        }

        public class WorldBossTemplate
        {
            public string BossId;
            public string BossName;
            public string Description;
            public Affinity Affinity;
            public double GlobalHp;
            public float BaseAtk;
            public float BaseDef;
            public Color ThemeColor;
            public PhaseTemplate[] Phases;
            public WorldBossRewardTable BronzeRewards;
            public WorldBossRewardTable SilverRewards;
            public WorldBossRewardTable GoldRewards;
            public WorldBossRewardTable DiamondRewards;
        }
    }
}
