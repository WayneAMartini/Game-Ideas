using UnityEngine;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Party
{
    public enum FormationSlot
    {
        FrontLeft = 0,
        FrontRight = 1,
        BackLeft = 2,
        BackRight = 3
    }

    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }

        [Header("Party Slots (2x2 Grid)")]
        [SerializeField] Hero[] _partySlots = new Hero[4];

        const float FrontlineDefBonus = 0.2f;   // +20% DEF
        const float BacklineDamageBonus = 0.1f;  // +10% damage

        public int PartySize
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _partySlots.Length; i++)
                    if (_partySlots[i] != null) count++;
                return count;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public Hero GetHero(int slot)
        {
            if (slot >= 0 && slot < _partySlots.Length)
                return _partySlots[slot];
            return null;
        }

        public Hero[] GetAllHeroes() => _partySlots;

        public bool IsFrontline(int slot)
        {
            return slot == (int)FormationSlot.FrontLeft || slot == (int)FormationSlot.FrontRight;
        }

        public bool IsBackline(int slot)
        {
            return slot == (int)FormationSlot.BackLeft || slot == (int)FormationSlot.BackRight;
        }

        public float GetDefenseBonus(int slot)
        {
            return IsFrontline(slot) ? FrontlineDefBonus : 0f;
        }

        public float GetDamageBonus(int slot)
        {
            return IsBackline(slot) ? BacklineDamageBonus : 0f;
        }

        // Swap two heroes between slots (only allowed between stages)
        public bool TrySwapSlots(int slotA, int slotB)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Combat)
                return false;

            if (slotA < 0 || slotA >= 4 || slotB < 0 || slotB >= 4)
                return false;

            (_partySlots[slotA], _partySlots[slotB]) = (_partySlots[slotB], _partySlots[slotA]);

            EventBus.Publish(new PartyChangedEvent());
            return true;
        }

        // Get all alive frontline heroes (enemies target these first)
        public Hero[] GetAliveFrontline()
        {
            int count = 0;
            Hero[] temp = new Hero[2];

            for (int i = 0; i <= 1; i++)
            {
                var hero = _partySlots[i];
                if (hero != null && hero.IsAlive)
                    temp[count++] = hero;
            }

            var result = new Hero[count];
            System.Array.Copy(temp, result, count);
            return result;
        }

        // Get all alive backline heroes
        public Hero[] GetAliveBackline()
        {
            int count = 0;
            Hero[] temp = new Hero[2];

            for (int i = 2; i <= 3; i++)
            {
                var hero = _partySlots[i];
                if (hero != null && hero.IsAlive)
                    temp[count++] = hero;
            }

            var result = new Hero[count];
            System.Array.Copy(temp, result, count);
            return result;
        }

        public Hero[] GetAllAliveHeroes()
        {
            int count = 0;
            Hero[] temp = new Hero[4];

            for (int i = 0; i < _partySlots.Length; i++)
            {
                var hero = _partySlots[i];
                if (hero != null && hero.IsAlive)
                    temp[count++] = hero;
            }

            var result = new Hero[count];
            System.Array.Copy(temp, result, count);
            return result;
        }

        public Hero GetLowestHpAliveHero()
        {
            Hero lowest = null;
            float lowestHp = float.MaxValue;

            for (int i = 0; i < _partySlots.Length; i++)
            {
                var hero = _partySlots[i];
                if (hero != null && hero.IsAlive && hero.CurrentHp < lowestHp)
                {
                    lowestHp = hero.CurrentHp;
                    lowest = hero;
                }
            }

            return lowest;
        }

        public bool AnyAllyBelowHpPercent(float percent)
        {
            for (int i = 0; i < _partySlots.Length; i++)
            {
                var hero = _partySlots[i];
                if (hero != null && hero.IsAlive && (hero.CurrentHp / hero.MaxHp) < percent)
                    return true;
            }
            return false;
        }

        public Hero GetHighestAtkEnemy()
        {
            // This is used by Rogue's Ambush to target highest-ATK enemy
            // Returning null here — actual enemy targeting is in EnemyManager
            return null;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
