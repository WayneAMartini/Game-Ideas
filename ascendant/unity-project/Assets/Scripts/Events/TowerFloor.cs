using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Combat;

namespace Ascendant.Events
{
    [Serializable]
    public class TowerFloor
    {
        public int floorNumber;
        public int enemyCount;
        public int enemyBaseLevel;
        public TowerModifier modifier;
        public bool isMilestone;
    }

    [Serializable]
    public class TowerBuff
    {
        public string buffName;
        public string description;
        public float atkBonus;
        public float defBonus;
        public float hpBonus;
        public float healingBonus;

        public static TowerBuff[] GenerateBuffChoices(int seed)
        {
            var rng = new System.Random(seed);
            var pool = GetBuffPool();
            var choices = new TowerBuff[3];
            var used = new HashSet<int>();

            for (int i = 0; i < 3; i++)
            {
                int idx;
                do { idx = rng.Next(pool.Length); } while (used.Contains(idx));
                used.Add(idx);
                choices[i] = pool[idx];
            }

            return choices;
        }

        static TowerBuff[] GetBuffPool()
        {
            return new[]
            {
                new TowerBuff { buffName = "Iron Will", description = "+15% DEF", defBonus = 0.15f },
                new TowerBuff { buffName = "Battle Fury", description = "+20% ATK", atkBonus = 0.20f },
                new TowerBuff { buffName = "Vitality Surge", description = "+25% Max HP", hpBonus = 0.25f },
                new TowerBuff { buffName = "Healing Wind", description = "+30% Healing", healingBonus = 0.30f },
                new TowerBuff { buffName = "Berserker Rage", description = "+35% ATK, -10% DEF", atkBonus = 0.35f, defBonus = -0.10f },
                new TowerBuff { buffName = "Fortified", description = "+25% DEF, -5% ATK", defBonus = 0.25f, atkBonus = -0.05f },
                new TowerBuff { buffName = "Bloodthirst", description = "+10% ATK, +10% Healing", atkBonus = 0.10f, healingBonus = 0.10f },
                new TowerBuff { buffName = "Titan's Strength", description = "+15% ATK, +15% HP", atkBonus = 0.15f, hpBonus = 0.15f },
                new TowerBuff { buffName = "Guardian's Blessing", description = "+20% DEF, +20% HP", defBonus = 0.20f, hpBonus = 0.20f },
                new TowerBuff { buffName = "Arcane Focus", description = "+25% ATK, +15% Healing", atkBonus = 0.25f, healingBonus = 0.15f },
            };
        }
    }

    [Serializable]
    public class TowerReward
    {
        public int floorMilestone;
        public double goldReward;
        public double materialReward;
        public int aetherCrystals;
        public bool guaranteedEquipment;
    }
}
