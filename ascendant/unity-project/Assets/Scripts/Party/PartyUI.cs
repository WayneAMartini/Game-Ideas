using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.Party
{
    public class PartyUI : MonoBehaviour
    {
        [Header("Hero Portraits (4 slots)")]
        [SerializeField] UI.HeroPortrait[] _portraits = new UI.HeroPortrait[4];

        [Header("Ability Controllers (per hero)")]
        [SerializeField] HeroAbilityController[] _abilityControllers = new HeroAbilityController[4];

        [Header("Gesture Config")]
        [SerializeField] float _doubleTapWindow = 0.3f;
        [SerializeField] float _holdThreshold = 0.5f;

        float[] _lastTapTime = new float[4];
        int[] _tapCounts = new int[4];
        bool[] _isHolding = new bool[4];
        float[] _holdTimers = new float[4];

        void OnEnable()
        {
            EventBus.Subscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<PartyChangedEvent>(OnPartyChanged);
        }

        void Start()
        {
            RefreshPortraits();
        }

        void Update()
        {
            // Process hold gestures
            for (int i = 0; i < 4; i++)
            {
                if (_isHolding[i])
                {
                    _holdTimers[i] += Time.deltaTime;
                }

                // Reset tap count if double-tap window expired
                if (_tapCounts[i] > 0 && Time.time - _lastTapTime[i] > _doubleTapWindow)
                {
                    if (_tapCounts[i] == 1 && !_isHolding[i])
                    {
                        // Single tap confirmed — Ability 1
                        OnPortraitSingleTap(i);
                    }
                    _tapCounts[i] = 0;
                }
            }
        }

        // Called by portrait button's pointer down event
        public void OnPortraitPointerDown(int slot)
        {
            if (slot < 0 || slot >= 4) return;
            _isHolding[slot] = true;
            _holdTimers[slot] = 0f;
        }

        // Called by portrait button's pointer up event
        public void OnPortraitPointerUp(int slot)
        {
            if (slot < 0 || slot >= 4) return;

            if (_holdTimers[slot] >= _holdThreshold)
            {
                // Hold gesture — Ultimate
                OnPortraitHold(slot);
            }
            else
            {
                // Tap gesture — count for single/double tap
                _tapCounts[slot]++;
                _lastTapTime[slot] = Time.time;

                if (_tapCounts[slot] == 2)
                {
                    // Double tap — Ability 2
                    OnPortraitDoubleTap(slot);
                    _tapCounts[slot] = 0;
                }
            }

            _isHolding[slot] = false;
            _holdTimers[slot] = 0f;
        }

        void OnPortraitSingleTap(int slot)
        {
            if (slot < _abilityControllers.Length && _abilityControllers[slot] != null)
                _abilityControllers[slot].TryActivateAbility(0);
        }

        void OnPortraitDoubleTap(int slot)
        {
            if (slot < _abilityControllers.Length && _abilityControllers[slot] != null)
                _abilityControllers[slot].TryActivateAbility(1);
        }

        void OnPortraitHold(int slot)
        {
            if (slot < _abilityControllers.Length && _abilityControllers[slot] != null)
                _abilityControllers[slot].TryActivateAbility(2); // Ultimate
        }

        void OnPartyChanged(PartyChangedEvent evt)
        {
            RefreshPortraits();
        }

        void RefreshPortraits()
        {
            var partyManager = PartyManager.Instance;
            if (partyManager == null) return;

            for (int i = 0; i < 4; i++)
            {
                var hero = partyManager.GetHero(i);
                if (hero != null && i < _portraits.Length && _portraits[i] != null)
                {
                    _portraits[i].gameObject.SetActive(true);
                    _portraits[i].Initialize(hero);
                }
                else if (i < _portraits.Length && _portraits[i] != null)
                {
                    _portraits[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
