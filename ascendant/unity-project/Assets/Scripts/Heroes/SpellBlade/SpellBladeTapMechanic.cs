using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Heroes
{
    public class SpellBladeTapMechanic : MonoBehaviour, ITapMechanic
    {
        [Header("Config")]
        [SerializeField] float _energyWaveMultiplier = 3.0f;
        [SerializeField] float _spellweaveStackBonus = 0.10f;
        [SerializeField] int _maxSpellweaveStacks = 5;

        [Header("State")]
        [SerializeField] int _arcaneEdgeCharges = 0;
        [SerializeField] int _meleeToSpellStacks = 0;
        [SerializeField] int _spellToMeleeStacks = 0;

        // Expose properties
        public int ArcaneEdgeCharges => _arcaneEdgeCharges;
        public int MaxCharges => 5;
        public int MeleeToSpellStacks => _meleeToSpellStacks;
        public int SpellToMeleeStacks => _spellToMeleeStacks;

        public void OnTap(int tapCount, float damage, Vector3 worldPosition)
        {
            if (_arcaneEdgeCharges >= MaxCharges)
            {
                // Fire Energy Wave: 300% ATK magic damage to all enemies
                FireEnergyWave(damage, worldPosition);
                _arcaneEdgeCharges = 0;
                // Spell hit: grant SpellToMelee stacks
                _spellToMeleeStacks = Mathf.Min(_spellToMeleeStacks + 1, _maxSpellweaveStacks);
                // Melee-to-spell stacks consumed by the spell
                _meleeToSpellStacks = 0;
            }
            else
            {
                // Normal melee hit
                float meleeDamage = damage;
                // Apply Spellweave spell->melee bonus
                if (_spellToMeleeStacks > 0)
                    meleeDamage *= (1f + _spellToMeleeStacks * _spellweaveStackBonus);

                var enemy = EnemyManager.Instance.GetNearestEnemy(worldPosition);
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(meleeDamage, DamageType.Physical);

                    EventBus.Publish(new EnemyDamagedEvent
                    {
                        EnemyId = enemy.Id,
                        Damage = meleeDamage,
                        IsCritical = false,
                        IsAoE = false,
                        WorldPosition = enemy.transform.position
                    });
                }

                // Build Arcane Edge charge
                _arcaneEdgeCharges = Mathf.Min(_arcaneEdgeCharges + 1, MaxCharges);

                // Spellweave: melee hit grants MeleeToSpell stack
                _meleeToSpellStacks = Mathf.Min(_meleeToSpellStacks + 1, _maxSpellweaveStacks);
                // Spell-to-melee stacks consumed by melee
                _spellToMeleeStacks = 0;
            }
        }

        void FireEnergyWave(float damage, Vector3 worldPosition)
        {
            float waveDamage = damage * _energyWaveMultiplier;
            // Apply Spellweave melee->spell bonus
            if (_meleeToSpellStacks > 0)
                waveDamage *= (1f + _meleeToSpellStacks * _spellweaveStackBonus);

            var allEnemies = EnemyManager.Instance.GetAllAliveEnemies();
            if (allEnemies == null) return;

            foreach (var enemy in allEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;

                enemy.TakeDamage(waveDamage, DamageType.Magical);

                EventBus.Publish(new EnemyDamagedEvent
                {
                    EnemyId = enemy.Id,
                    Damage = waveDamage,
                    IsCritical = false,
                    IsAoE = true,
                    WorldPosition = enemy.transform.position
                });
            }
        }

        public void Reset()
        {
            _arcaneEdgeCharges = 0;
            _meleeToSpellStacks = 0;
            _spellToMeleeStacks = 0;
        }
    }
}
