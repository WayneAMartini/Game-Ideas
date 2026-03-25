using UnityEngine;

namespace Ascendant.Heroes.Systems
{
    public interface IPetCompanion
    {
        string PetName { get; }
        float CurrentHp { get; }
        float MaxHp { get; }
        float Atk { get; }
        bool IsAlive { get; }
        void CommandAttack(Combat.Enemy target);
        void TakeDamage(float damage);
        void Revive();
        void Reset();
    }
}
