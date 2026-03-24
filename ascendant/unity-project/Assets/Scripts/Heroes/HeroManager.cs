using UnityEngine;

namespace Ascendant.Heroes
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        [SerializeField] Hero[] _heroes; // party slots (up to 4, Phase 1 uses 1)

        public Hero GetPrimaryHero()
        {
            if (_heroes != null && _heroes.Length > 0)
                return _heroes[0];
            return null;
        }

        public Hero GetHero(int slot)
        {
            if (_heroes != null && slot >= 0 && slot < _heroes.Length)
                return _heroes[slot];
            return null;
        }

        public int HeroCount => _heroes != null ? _heroes.Length : 0;

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
