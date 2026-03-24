namespace Ascendant.Combat
{
    public enum Affinity
    {
        None,
        Flame,
        Frost,
        Storm,
        Nature,
        Shadow,
        Radiance
    }

    public static class AffinityHelper
    {
        // Returns the damage multiplier when attacker hits defender.
        // Advantage = +30% damage, Disadvantage = -30% damage
        // Shadow <-> Radiance = mutual +30%
        public static float GetMultiplier(Affinity attacker, Affinity defender)
        {
            if (attacker == Affinity.None || defender == Affinity.None)
                return 1f;

            // Flame > Nature > Storm > Frost > Flame
            if (attacker == Affinity.Flame && defender == Affinity.Nature) return 1.3f;
            if (attacker == Affinity.Nature && defender == Affinity.Storm) return 1.3f;
            if (attacker == Affinity.Storm && defender == Affinity.Frost) return 1.3f;
            if (attacker == Affinity.Frost && defender == Affinity.Flame) return 1.3f;

            // Reverse = disadvantage
            if (attacker == Affinity.Nature && defender == Affinity.Flame) return 0.7f;
            if (attacker == Affinity.Storm && defender == Affinity.Nature) return 0.7f;
            if (attacker == Affinity.Frost && defender == Affinity.Storm) return 0.7f;
            if (attacker == Affinity.Flame && defender == Affinity.Frost) return 0.7f;

            // Shadow <-> Radiance: mutual +30%
            if (attacker == Affinity.Shadow && defender == Affinity.Radiance) return 1.3f;
            if (attacker == Affinity.Radiance && defender == Affinity.Shadow) return 1.3f;

            return 1f;
        }
    }
}
