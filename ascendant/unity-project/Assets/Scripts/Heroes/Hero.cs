using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;
using Ascendant.Progression;
using Ascendant.Economy;

namespace Ascendant.Heroes
{
    public class Hero : MonoBehaviour
    {
        [SerializeField] HeroData _data;
        [SerializeField] int _slot; // party slot index (0-3)

        int _level = 1;
        float _currentHp;
        float _maxHp;
        float _currentAtk;
        float _currentDef;
        float _currentSpd;
        float _xp;
        float _xpToNextLevel;

        public HeroData Data => _data;
        public int Slot => _slot;
        public int Level => _level;
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public float CurrentAtk => _currentAtk;
        public float CurrentDef => _currentDef;
        public float CurrentSpd => _currentSpd;
        public float Xp => _xp;
        public float XpToNextLevel => _xpToNextLevel;
        public Affinity Affinity => _data != null ? _data.affinity : Affinity.None;
        public bool IsAlive => _currentHp > 0f;

        public void Initialize(HeroData data, int slot, int level = 1)
        {
            _data = data;
            _slot = slot;
            _level = level;
            RecalculateStats();
            _currentHp = _maxHp;
            _xpToNextLevel = CalculateXpForLevel(_level + 1);
        }

        public void RecalculateStats()
        {
            // Base stats from HeroData (level growth)
            float baseHp = _data.GetHp(_level);
            float baseAtk = _data.GetAtk(_level);
            float baseDef = _data.GetDef(_level);
            float baseSpd = _data.GetSpd(_level);

            // Equipment bonuses (flat)
            float equipAtk = 0f, equipDef = 0f, equipHp = 0f, equipSpd = 0f;
            var equipSystem = EquipmentSystem.Instance;
            if (equipSystem != null)
            {
                equipAtk = equipSystem.GetEquipmentStatBonus(_slot, StatType.ATK);
                equipDef = equipSystem.GetEquipmentStatBonus(_slot, StatType.DEF);
                equipHp = equipSystem.GetEquipmentStatBonus(_slot, StatType.HP);
                equipSpd = equipSystem.GetEquipmentStatBonus(_slot, StatType.SPD);
            }

            // Skill tree bonuses (flat)
            float skillAtk = 0f, skillDef = 0f, skillHp = 0f, skillSpd = 0f;
            var skillSystem = SkillTreeSystem.Instance;
            string classId = _data != null ? _data.classId : "";
            if (skillSystem != null && !string.IsNullOrEmpty(classId))
            {
                skillAtk = skillSystem.GetSkillTreeStatBonus(_slot, StatType.ATK, classId);
                skillDef = skillSystem.GetSkillTreeStatBonus(_slot, StatType.DEF, classId);
                skillHp = skillSystem.GetSkillTreeStatBonus(_slot, StatType.HP, classId);
                skillSpd = skillSystem.GetSkillTreeStatBonus(_slot, StatType.SPD, classId);
            }

            // Mastery bonus (percentage multiplier, permanent)
            float masteryMult = 1f;
            var masterySystem = ClassMasterySystem.Instance;
            if (masterySystem != null && !string.IsNullOrEmpty(classId))
            {
                float masteryPercent = masterySystem.GetMasteryStatBonusPercent(classId);
                masteryMult = 1f + masteryPercent / 100f;
            }

            // Star system bonus (percentage multiplier)
            float starMult = 1f;
            var starSystem = StarSystem.Instance;
            if (starSystem != null && !string.IsNullOrEmpty(classId))
            {
                starMult = starSystem.GetStatMultiplier(classId);
            }

            // Final stats: (base + equipment + skillTree) * mastery * starMult
            float prevMaxHp = _maxHp;
            _maxHp = (baseHp + equipHp + skillHp) * masteryMult * starMult;
            _currentAtk = (baseAtk + equipAtk + skillAtk) * masteryMult * starMult;
            _currentDef = (baseDef + equipDef + skillDef) * masteryMult * starMult;
            _currentSpd = (baseSpd + equipSpd + skillSpd) * masteryMult * starMult;

            // Scale current HP proportionally if max HP changed
            if (prevMaxHp > 0f && _maxHp != prevMaxHp)
            {
                float ratio = _currentHp / prevMaxHp;
                _currentHp = ratio * _maxHp;
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            float actual = Mathf.Max(1f, damage - _currentDef * 0.5f);
            _currentHp = Mathf.Max(0f, _currentHp - actual);

            EventBus.Publish(new HeroDamagedEvent
            {
                HeroSlot = _slot,
                Damage = actual,
                CurrentHp = _currentHp,
                MaxHp = _maxHp
            });
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;

            float before = _currentHp;
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            float healed = _currentHp - before;

            if (healed > 0f)
            {
                EventBus.Publish(new HeroHealedEvent
                {
                    HeroSlot = _slot,
                    Amount = healed,
                    CurrentHp = _currentHp,
                    MaxHp = _maxHp
                });
            }
        }

        public void AddXp(float amount)
        {
            int levelCap = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevelCap() : 100;

            _xp += amount;

            while (_xp >= _xpToNextLevel && _level < levelCap)
            {
                _xp -= _xpToNextLevel;
                _level++;
                RecalculateStats();
                _currentHp = _maxHp; // full heal on level up
                _xpToNextLevel = CalculateXpForLevel(_level + 1);

                // Publish level-up event for skill point awards, etc.
                string classId = _data != null ? _data.classId : "";
                bool isMilestone = LevelingSystem.Instance != null &&
                    LevelingSystem.Instance.XPCurve != null &&
                    LevelingSystem.Instance.XPCurve.IsMilestoneLevel(_level);

                EventBus.Publish(new LevelUpEvent
                {
                    HeroSlot = _slot,
                    NewLevel = _level,
                    ClassId = classId,
                    IsMilestone = isMilestone
                });
            }
        }

        static float CalculateXpForLevel(int level)
        {
            // Use LevelingSystem if available, fallback to default curve
            if (LevelingSystem.Instance != null)
                return LevelingSystem.Instance.GetXpForLevel(level);
            return 100f * Mathf.Pow(level, 1.5f);
        }
    }
}
