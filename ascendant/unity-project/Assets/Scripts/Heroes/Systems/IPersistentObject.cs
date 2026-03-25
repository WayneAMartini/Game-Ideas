using UnityEngine;

namespace Ascendant.Heroes.Systems
{
    public interface IPersistentObject
    {
        string ObjectName { get; }
        float Duration { get; }
        float RemainingTime { get; }
        bool IsActive { get; }
        Vector3 Position { get; }
        void Activate(Vector3 position);
        void Deactivate();
        void UpdateTick(float deltaTime);
    }
}
