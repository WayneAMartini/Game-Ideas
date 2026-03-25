using System.Collections.Generic;
using UnityEngine;
using Ascendant.Combat;
using Ascendant.Islands;

namespace Ascendant.Events
{
    public static class ProceduralIslandGenerator
    {
        static readonly string[] IslandNamePrefixes =
        {
            "Shattered", "Forgotten", "Celestial", "Abyssal", "Eternal",
            "Crimson", "Spectral", "Ancient", "Twilight", "Ascended",
            "Void-Touched", "Tempest", "Prismatic", "Obsidian", "Luminous"
        };

        static readonly string[] IslandNameSuffixes =
        {
            "Pinnacle", "Abyss", "Citadel", "Expanse", "Throne",
            "Sanctuary", "Rift", "Bastion", "Nexus", "Dominion",
            "Maelstrom", "Summit", "Depths", "Haven", "Crucible"
        };

        static readonly string[] BossNamePrefixes =
        {
            "The", "Lord", "Queen", "King", "Ancient",
            "Grand", "Supreme", "Eternal", "Void", "Dread"
        };

        static readonly string[] BossNameSuffixes =
        {
            "Devourer", "Warden", "Sovereign", "Tyrant", "Archon",
            "Harbinger", "Colossus", "Wraith", "Titan", "Oracle",
            "Destroyer", "Guardian", "Overlord", "Fury", "Phantom"
        };

        public static ProceduralIsland Generate(int islandNumber, int seed)
        {
            var rng = new System.Random(seed + islandNumber * 137);

            // Pick affinities (dual-affinity for infinite mode)
            var affinities = GetAffinityValues();
            var primaryAffinity = affinities[rng.Next(affinities.Length)];
            Affinity secondaryAffinity;
            do { secondaryAffinity = affinities[rng.Next(affinities.Length)]; }
            while (secondaryAffinity == primaryAffinity);

            // Difficulty scales infinitely: +10% per island beyond Realm 3
            int islandsBeyondRealm3 = islandNumber - 36;
            float difficultyMultiplier = 3f + islandsBeyondRealm3 * 0.1f;

            // Generate name
            string name = IslandNamePrefixes[rng.Next(IslandNamePrefixes.Length)] + " " +
                           IslandNameSuffixes[rng.Next(IslandNameSuffixes.Length)];

            // Generate boss name
            string bossName = BossNamePrefixes[rng.Next(BossNamePrefixes.Length)] + " " +
                               BossNameSuffixes[rng.Next(BossNameSuffixes.Length)];

            // Determine biome effects
            var biomeEffect = (BiomeEffectType)rng.Next(1, 11); // Skip None

            // Boss phases (3-6 phases based on difficulty)
            int phaseCount = Mathf.Min(6, 3 + islandsBeyondRealm3 / 10);

            // Mini-boss mechanic count scales with difficulty
            int miniBossMechanics = Mathf.Min(5, 3 + islandsBeyondRealm3 / 20);

            // Generate boss phases
            var bossPhases = GenerateBossPhases(phaseCount, rng);

            return new ProceduralIsland
            {
                IslandNumber = islandNumber,
                IslandName = name,
                PrimaryAffinity = primaryAffinity,
                SecondaryAffinity = secondaryAffinity,
                DifficultyMultiplier = difficultyMultiplier,
                BiomeEffect = biomeEffect,
                BossName = bossName,
                BossPhases = bossPhases,
                MiniBossMechanicCount = miniBossMechanics,
                StageCount = 100,
                BossHpMultiplier = 15f + islandsBeyondRealm3 * 0.5f,
                BossAtkMultiplier = 4f + islandsBeyondRealm3 * 0.2f,
                GoldMultiplier = 1f + islandNumber * 0.15f,
                XpMultiplier = 1f + islandNumber * 0.12f
            };
        }

        static List<BossPhaseData> GenerateBossPhases(int count, System.Random rng)
        {
            var phases = new List<BossPhaseData>();
            var mechanicValues = GetMechanicValues();

            for (int i = 0; i < count; i++)
            {
                float hpThreshold = i == 0 ? 1f : 1f - (float)i / count;
                int mechanicCount = Mathf.Min(3, 1 + i / 2);

                var mechanics = new BossMechanicType[mechanicCount];
                var used = new HashSet<int>();
                for (int m = 0; m < mechanicCount; m++)
                {
                    int idx;
                    do { idx = rng.Next(mechanicValues.Length); } while (used.Contains(idx));
                    used.Add(idx);
                    mechanics[m] = mechanicValues[idx];
                }

                phases.Add(new BossPhaseData
                {
                    phaseName = $"Phase {i + 1}",
                    hpThreshold = hpThreshold,
                    description = $"Phase {i + 1} of the procedural boss.",
                    atkMultiplier = 1f + i * 0.3f,
                    attackSpeedMultiplier = 1f + i * 0.2f,
                    mechanics = mechanics
                });
            }

            return phases;
        }

        static Affinity[] GetAffinityValues()
        {
            return new[] { Affinity.Flame, Affinity.Frost, Affinity.Storm, Affinity.Nature, Affinity.Shadow, Affinity.Radiance };
        }

        static BossMechanicType[] GetMechanicValues()
        {
            return new[]
            {
                BossMechanicType.Enrage, BossMechanicType.ShieldPhase, BossMechanicType.AddSpawning,
                BossMechanicType.GroundSlam, BossMechanicType.LifeSteal, BossMechanicType.Split,
                BossMechanicType.Reflect
            };
        }
    }

    public class ProceduralIsland
    {
        public int IslandNumber;
        public string IslandName;
        public Affinity PrimaryAffinity;
        public Affinity SecondaryAffinity;
        public float DifficultyMultiplier;
        public BiomeEffectType BiomeEffect;
        public string BossName;
        public List<BossPhaseData> BossPhases;
        public int MiniBossMechanicCount;
        public int StageCount;
        public float BossHpMultiplier;
        public float BossAtkMultiplier;
        public float GoldMultiplier;
        public float XpMultiplier;
    }
}
