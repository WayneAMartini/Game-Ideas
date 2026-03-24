using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

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
            _maxHp = _data.GetHp(_level);
            _currentAtk = _data.GetAtk(_level);
            _currentDef = _data.GetDef(_level);
            _currentSpd = _data.GetSpd(_level);
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
            _xp += amount;

            while (_xp >= _xpToNextLevel && _level < 100)
            {
                _xp -= _xpToNextLevel;
                _level++;
                RecalculateStats();
                _currentHp = _maxHp; // full heal on level up
                _xpToNextLevel = CalculateXpForLevel(_level + 1);
            }
        }

        static float CalculateXpForLevel(int level)
        {
            // Exponential XP curve: 100 * level^1.8
            return 100f * Mathf.Pow(level, 1.8f);
        }
    }
}
