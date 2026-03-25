using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Backend
{
    public class ArenaBattleResult
    {
        public bool PlayerWon;
        public int TurnsPlayed;
        public float PlayerHpRemaining; // percentage
        public float OpponentHpRemaining; // percentage
        public List<string> BattleLog = new();
    }

    public static class ArenaBattleSimulator
    {
        // Normalized stat range: compress to 80-120% of median
        const float NormalizedMin = 0.8f;
        const float NormalizedMax = 1.2f;
        const int MaxTurns = 50;

        public static ArenaBattleResult SimulateBattle(ArenaDefenseTeam attacker, ArenaDefenseTeam defender)
        {
            var result = new ArenaBattleResult();

            if (attacker?.Heroes == null || defender?.Heroes == null ||
                attacker.Heroes.Count == 0 || defender.Heroes.Count == 0)
            {
                result.PlayerWon = true;
                result.BattleLog.Add("Opponent has no defense team. Victory by default!");
                return result;
            }

            // Normalize stats to compress advantage
            var atkTeam = NormalizeTeam(attacker.Heroes);
            var defTeam = NormalizeTeam(defender.Heroes);

            // Simple turn-based simulation
            float atkTotalHp = 0f, atkMaxHp = 0f;
            float defTotalHp = 0f, defMaxHp = 0f;

            foreach (var h in atkTeam) { atkTotalHp += h.Hp; atkMaxHp += h.Hp; }
            foreach (var h in defTeam) { defTotalHp += h.Hp; defMaxHp += h.Hp; }

            int turn = 0;
            var rng = new System.Random();

            while (turn < MaxTurns && atkTotalHp > 0 && defTotalHp > 0)
            {
                turn++;

                // Attacker's turn: each alive hero attacks
                foreach (var hero in atkTeam)
                {
                    if (hero.Hp <= 0) continue;

                    // Find alive defender target (random)
                    var target = FindAliveTarget(defTeam, rng);
                    if (target == null) break;

                    // Damage = ATK * (1 + random variance 0-20%)
                    float damage = hero.Atk * (1f + (float)rng.NextDouble() * 0.2f);
                    damage = Mathf.Max(1f, damage - target.Def * 0.3f);

                    // Ability usage: 30% chance for bonus damage
                    if (rng.NextDouble() < 0.3f)
                    {
                        damage *= 1.5f;
                        result.BattleLog.Add($"[T{turn}] {hero.ClassId} uses ability for {damage:F0} damage!");
                    }

                    target.Hp -= damage;
                    defTotalHp -= damage;

                    if (target.Hp <= 0)
                    {
                        result.BattleLog.Add($"[T{turn}] {target.ClassId} defeated!");
                    }
                }

                if (defTotalHp <= 0) break;

                // Defender's turn
                foreach (var hero in defTeam)
                {
                    if (hero.Hp <= 0) continue;

                    var target = FindAliveTarget(atkTeam, rng);
                    if (target == null) break;

                    float damage = hero.Atk * (1f + (float)rng.NextDouble() * 0.2f);
                    damage = Mathf.Max(1f, damage - target.Def * 0.3f);

                    if (rng.NextDouble() < 0.3f)
                        damage *= 1.5f;

                    target.Hp -= damage;
                    atkTotalHp -= damage;

                    if (target.Hp <= 0)
                    {
                        result.BattleLog.Add($"[T{turn}] Your {target.ClassId} defeated!");
                    }
                }
            }

            result.TurnsPlayed = turn;
            result.PlayerHpRemaining = Mathf.Max(0f, atkTotalHp / atkMaxHp);
            result.OpponentHpRemaining = Mathf.Max(0f, defTotalHp / defMaxHp);
            result.PlayerWon = atkTotalHp > defTotalHp;

            if (result.PlayerWon)
                result.BattleLog.Add("Victory!");
            else
                result.BattleLog.Add("Defeat...");

            return result;
        }

        static List<SimHero> NormalizeTeam(List<ArenaHeroSnapshot> heroes)
        {
            // Calculate median stats across both teams
            float medianAtk = 0f, medianDef = 0f, medianHp = 0f, medianSpd = 0f;
            int count = heroes.Count;

            foreach (var h in heroes)
            {
                medianAtk += h.Atk;
                medianDef += h.Def;
                medianHp += h.Hp;
                medianSpd += h.Spd;
            }

            if (count > 0)
            {
                medianAtk /= count;
                medianDef /= count;
                medianHp /= count;
                medianSpd /= count;
            }

            // Ensure non-zero medians
            if (medianAtk <= 0) medianAtk = 100f;
            if (medianDef <= 0) medianDef = 50f;
            if (medianHp <= 0) medianHp = 1000f;
            if (medianSpd <= 0) medianSpd = 10f;

            var normalized = new List<SimHero>();
            foreach (var h in heroes)
            {
                normalized.Add(new SimHero
                {
                    ClassId = h.ClassId,
                    Atk = NormalizeStat(h.Atk, medianAtk),
                    Def = NormalizeStat(h.Def, medianDef),
                    Hp = NormalizeStat(h.Hp, medianHp) * 10f, // Scale HP for battle duration
                    Spd = NormalizeStat(h.Spd, medianSpd)
                });
            }

            return normalized;
        }

        static float NormalizeStat(float value, float median)
        {
            if (median <= 0) return 100f;
            float ratio = value / median;
            float clamped = Mathf.Clamp(ratio, NormalizedMin, NormalizedMax);
            return clamped * 100f; // Base 100 normalized
        }

        static SimHero FindAliveTarget(List<SimHero> team, System.Random rng)
        {
            var alive = new List<SimHero>();
            foreach (var h in team)
                if (h.Hp > 0) alive.Add(h);

            if (alive.Count == 0) return null;
            return alive[rng.Next(alive.Count)];
        }

        class SimHero
        {
            public string ClassId;
            public float Atk;
            public float Def;
            public float Hp;
            public float Spd;
        }
    }
}
