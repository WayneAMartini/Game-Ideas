using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Combat
{
    public class MomentumSystem : MonoBehaviour
    {
        public static MomentumSystem Instance { get; private set; }

        const int MaxStacks = 100;
        const float TapWindow = 0.5f;
        const float DecayRate = 5f; // stacks per second

        int _stacks;
        float _timeSinceLastTap;
        bool _isDecaying;

        public int Stacks => _stacks;
        public float Multiplier => 1f + (_stacks * 0.01f); // +1% per stack

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            if (_stacks <= 0) return;

            _timeSinceLastTap += Time.deltaTime;

            if (_timeSinceLastTap > TapWindow)
            {
                _isDecaying = true;
                float decay = DecayRate * Time.deltaTime;
                int stacksToRemove = Mathf.FloorToInt(decay);

                // Accumulate fractional decay
                float fractional = decay - stacksToRemove;
                if (Random.value < fractional) stacksToRemove++;

                if (stacksToRemove > 0)
                {
                    _stacks = Mathf.Max(0, _stacks - stacksToRemove);
                    PublishChange();
                }
            }
        }

        public void RegisterTap()
        {
            _timeSinceLastTap = 0f;
            _isDecaying = false;

            if (_stacks < MaxStacks)
            {
                _stacks++;
                PublishChange();
            }
        }

        public void Reset()
        {
            _stacks = 0;
            _timeSinceLastTap = 0f;
            _isDecaying = false;
            PublishChange();
        }

        void PublishChange()
        {
            EventBus.Publish(new MomentumChangedEvent
            {
                Stacks = _stacks,
                Multiplier = Multiplier
            });
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
