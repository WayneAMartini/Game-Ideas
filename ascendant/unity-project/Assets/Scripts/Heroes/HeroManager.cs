using UnityEngine;

namespace Ascendant.Heroes
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        [SerializeField] Hero[] _heroes; // party slots (up to 4)

        public Hero GetPrimaryHero()
        {
            // Try PartyManager first, fall back to local array
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null)
                return partyManager.GetHero(0);

            if (_heroes != null && _heroes.Length > 0)
                return _heroes[0];
            return null;
        }

        public Hero GetHero(int slot)
        {
            var partyManager = Party.PartyManager.Instance;
            if (partyManager != null)
                return partyManager.GetHero(slot);

            if (_heroes != null && slot >= 0 && slot < _heroes.Length)
                return _heroes[slot];
            return null;
        }

        public int HeroCount
        {
            get
            {
                var partyManager = Party.PartyManager.Instance;
                if (partyManager != null)
                    return partyManager.PartySize;
                return _heroes != null ? _heroes.Length : 0;
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

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
