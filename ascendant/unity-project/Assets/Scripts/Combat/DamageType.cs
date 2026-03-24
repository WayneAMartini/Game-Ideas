namespace Ascendant.Combat
{
    public enum EnemyAttackType
    {
        Melee,      // Targets frontline only
        Ranged,     // Can target backline
        AoE         // Hits all heroes
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }

    public enum EnemyCategory
    {
        Beast,
        Construct,
        Ethereal,
        Armored,
        Resistant,
        Humanoid
    }

    public static class DamageTypeHelper
    {
        // Returns the damage multiplier based on damage type vs enemy category
        // Physical: strong vs Beast/Construct (1.3x), weak vs Ethereal/Armored (0.7x)
        // Magical: strong vs Armored/Ethereal (1.3x), weak vs Beast/Resistant (0.7x)
        // True: always 1.0x (no resistance, no weakness)
        public static float GetTypeMultiplier(DamageType type, EnemyCategory category)
        {
            if (type == DamageType.True)
                return 1f;

            if (type == DamageType.Physical)
            {
                return category switch
                {
                    EnemyCategory.Beast => 1.3f,
                    EnemyCategory.Construct => 1.3f,
                    EnemyCategory.Ethereal => 0.7f,
                    EnemyCategory.Armored => 0.7f,
                    _ => 1f
                };
            }

            // Magical
            return category switch
            {
                EnemyCategory.Armored => 1.3f,
                EnemyCategory.Ethereal => 1.3f,
                EnemyCategory.Beast => 0.7f,
                EnemyCategory.Resistant => 0.7f,
                _ => 1f
            };
        }
    }
}
