using UnityEngine;

namespace Ascendant.Heroes
{
    public interface ITapMechanic
    {
        // Called on each tap with the running tap count, calculated damage, and world position
        void OnTap(int tapCount, float damage, Vector3 worldPosition);

        // Reset state (e.g., on new stage)
        void Reset();
    }
}
