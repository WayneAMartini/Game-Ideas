using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Progression
{
    public class LevelingSystem : MonoBehaviour
    {
        public static LevelingSystem Instance { get; private set; }

        [SerializeField] XPCurve _xpCurve;

        public XPCurve XPCurve => _xpCurve;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AwardXpToHero(Hero hero, float amount)
        {
            if (hero == null || !hero.IsAlive) return;

            int levelBefore = hero.Level;
            hero.AddXp(amount);

            // Check for level-ups
            for (int lvl = levelBefore + 1; lvl <= hero.Level; lvl++)
            {
                EventBus.Publish(new LevelUpEvent
                {
                    HeroSlot = hero.Slot,
                    NewLevel = lvl,
                    ClassId = hero.Data != null ? hero.Data.classId : "",
                    IsMilestone = _xpCurve != null && _xpCurve.IsMilestoneLevel(lvl)
                });
            }
        }

        public float GetXpForLevel(int level)
        {
            if (_xpCurve != null) return _xpCurve.GetXpForLevel(level);
            return 100f * Mathf.Pow(level, 1.5f);
        }

        public int GetLevelCap()
        {
            return _xpCurve != null ? _xpCurve.levelCap : 100;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
