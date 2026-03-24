using UnityEngine;
using Ascendant.Party;
using Ascendant.Heroes;
using Ascendant.Combat;

namespace Ascendant.Core
{
    /// <summary>
    /// Initializes all heroes in the party when the GameScene loads.
    /// Placed on the PartyManager GameObject so it runs after PartyManager.Awake().
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Start()
        {
            InitializeParty();
            SetupTapMechanics();
        }

        void InitializeParty()
        {
            var pm = PartyManager.Instance;
            if (pm == null) return;

            var heroes = pm.GetAllHeroes();
            for (int i = 0; i < heroes.Length; i++)
            {
                var hero = heroes[i];
                if (hero != null && hero.Data != null)
                {
                    hero.Initialize(hero.Data, i);
                }
            }
        }

        void SetupTapMechanics()
        {
            var tap = FindAnyObjectByType<TapInputController>();
            if (tap == null) return;

            var pm = PartyManager.Instance;
            if (pm == null) return;

            var heroes = pm.GetAllHeroes();
            var mechanics = new ITapMechanic[4];

            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] != null)
                    mechanics[i] = heroes[i].GetComponent<ITapMechanic>();
            }

            tap.SetPartyTapMechanics(mechanics);
        }
    }
}
