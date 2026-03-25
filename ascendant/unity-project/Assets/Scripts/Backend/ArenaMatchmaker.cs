using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Backend
{
    public enum ArenaDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public static class ArenaMatchmaker
    {
        // Present 3 opponents: one easier, one similar, one harder
        public static void CategorizeOpponents(List<ArenaDefenseTeam> opponents, int playerElo,
            out ArenaDefenseTeam easy, out ArenaDefenseTeam medium, out ArenaDefenseTeam hard)
        {
            easy = null;
            medium = null;
            hard = null;

            if (opponents == null || opponents.Count == 0) return;

            // Sort by ELO distance from player
            var sorted = new List<ArenaDefenseTeam>(opponents);
            sorted.Sort((a, b) => a.EloRating.CompareTo(b.EloRating));

            // Assign: lowest ELO = easy, closest = medium, highest = hard
            if (sorted.Count >= 3)
            {
                easy = sorted[0];
                hard = sorted[^1];

                // Medium is the one closest to player ELO
                int closestIndex = 0;
                int closestDist = int.MaxValue;
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i] == easy || sorted[i] == hard) continue;
                    int dist = Mathf.Abs(sorted[i].EloRating - playerElo);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }
                medium = sorted[closestIndex];
            }
            else if (sorted.Count == 2)
            {
                easy = sorted[0];
                hard = sorted[1];
            }
            else
            {
                medium = sorted[0];
            }
        }

        public static ArenaDifficulty EstimateDifficulty(int playerElo, int opponentElo)
        {
            int diff = opponentElo - playerElo;
            if (diff < -100) return ArenaDifficulty.Easy;
            if (diff > 100) return ArenaDifficulty.Hard;
            return ArenaDifficulty.Medium;
        }

        public static float EstimateTeamPower(ArenaDefenseTeam team)
        {
            if (team?.Heroes == null || team.Heroes.Count == 0) return 0f;

            float totalPower = 0f;
            foreach (var hero in team.Heroes)
            {
                totalPower += hero.Atk + hero.Def + hero.Hp * 0.1f + hero.Spd * 10f;
                totalPower += hero.Level * 10f;
                totalPower += hero.StarRating * 100f;
            }
            return totalPower;
        }
    }
}
