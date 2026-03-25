using UnityEngine;

namespace Ascendant.Events
{
    [CreateAssetMenu(fileName = "NewTowerModifier", menuName = "Ascendant/Tower Modifier")]
    public class TowerModifier : ScriptableObject
    {
        public string modifierName;
        [TextArea(2, 4)]
        public string description;

        [Header("Stat Effects")]
        public float atkMultiplier = 1f;
        public float defMultiplier = 1f;
        public float hpMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float healingMultiplier = 1f;

        [Header("Enemy Stat Effects")]
        public float enemyAtkMultiplier = 1f;
        public float enemyDefMultiplier = 1f;
        public float enemyHpMultiplier = 1f;
        public float enemySpeedMultiplier = 1f;

        [Header("Special Effects")]
        public bool enemiesExplodeOnDeath;
        public float explosionDamagePercent;
        public bool allDamageIsFire;
        public bool doubleGold;
        public bool halfCooldowns;
        public bool noHealing;
        public bool reflectDamagePercent;
        public float reflectAmount;

        [Header("Visual")]
        public Color tintColor = Color.white;
    }
}
