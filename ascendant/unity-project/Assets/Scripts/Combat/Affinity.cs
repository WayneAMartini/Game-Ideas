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

    public enum BiomeAffinityRelation
    {
        Neutral,
        Home,        // Same affinity as biome
        Advantage,   // Hero's affinity beats biome affinity
        Disadvantage // Hero's affinity loses to biome affinity
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

        // Returns the relationship between a hero's affinity and a biome's affinity
        public static BiomeAffinityRelation GetBiomeRelation(Affinity heroAffinity, Affinity biomeAffinity)
        {
            if (heroAffinity == Affinity.None || biomeAffinity == Affinity.None)
                return BiomeAffinityRelation.Neutral;

            if (heroAffinity == biomeAffinity)
                return BiomeAffinityRelation.Home;

            // Check if hero has advantage over biome
            float multiplier = GetMultiplier(heroAffinity, biomeAffinity);
            if (multiplier > 1f)
                return BiomeAffinityRelation.Advantage;
            if (multiplier < 1f)
                return BiomeAffinityRelation.Disadvantage;

            return BiomeAffinityRelation.Neutral;
        }

        // Biome damage bonus based on hero-biome relationship
        // Home: +25% damage, Advantage: +50%, Disadvantage: -20%
        public static float GetBiomeDamageMultiplier(Affinity heroAffinity, Affinity biomeAffinity)
        {
            var relation = GetBiomeRelation(heroAffinity, biomeAffinity);
            return relation switch
            {
                BiomeAffinityRelation.Home => 1.25f,
                BiomeAffinityRelation.Advantage => 1.50f,
                BiomeAffinityRelation.Disadvantage => 0.80f,
                _ => 1f
            };
        }

        // Biome damage taken modifier
        // Home: -10% taken, Advantage: -20% taken, Disadvantage: +10% taken
        public static float GetBiomeDamageTakenMultiplier(Affinity heroAffinity, Affinity biomeAffinity)
        {
            var relation = GetBiomeRelation(heroAffinity, biomeAffinity);
            return relation switch
            {
                BiomeAffinityRelation.Home => 0.90f,
                BiomeAffinityRelation.Advantage => 0.80f,
                BiomeAffinityRelation.Disadvantage => 1.10f,
                _ => 1f
            };
        }
    }
}
