using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Combat
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        readonly List<Enemy> _activeEnemies = new();

        public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;
        public int AliveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activeEnemies.Count; i++)
                {
                    if (_activeEnemies[i] != null && !_activeEnemies[i].IsDead)
                        count++;
                }
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

        public void Register(Enemy enemy)
        {
            _activeEnemies.Add(enemy);
        }

        public void Unregister(Enemy enemy)
        {
            _activeEnemies.Remove(enemy);
        }

        public Enemy GetNearestEnemy(Vector3 position)
        {
            Enemy nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.SqrMagnitude(enemy.transform.position - position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        public List<Enemy> GetAllAliveEnemies()
        {
            var alive = new List<Enemy>();
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }
                alive.Add(enemy);
            }
            return alive;
        }

        // Get the enemy with the highest ATK (for Rogue's Ambush targeting)
        public Enemy GetHighestAtkEnemy()
        {
            Enemy highest = null;
            float highestAtk = float.MinValue;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                if (enemy.Atk > highestAtk)
                {
                    highestAtk = enemy.Atk;
                    highest = enemy;
                }
            }

            return highest;
        }

        // Get a random frontline hero to attack (enemies target frontline first)
        public Heroes.Hero GetTargetHero(EnemyAttackType attackType)
        {
            var partyManager = Party.PartyManager.Instance;
            if (partyManager == null)
            {
                return Heroes.HeroManager.Instance?.GetPrimaryHero();
            }

            switch (attackType)
            {
                case EnemyAttackType.Melee:
                {
                    // Melee targets frontline first
                    var frontline = partyManager.GetAliveFrontline();
                    if (frontline.Length > 0)
                        return frontline[Random.Range(0, frontline.Length)];
                    // If no frontline alive, target backline
                    var backline = partyManager.GetAliveBackline();
                    if (backline.Length > 0)
                        return backline[Random.Range(0, backline.Length)];
                    return null;
                }

                case EnemyAttackType.Ranged:
                {
                    // Ranged can target anyone
                    var allAlive = partyManager.GetAllAliveHeroes();
                    if (allAlive.Length > 0)
                        return allAlive[Random.Range(0, allAlive.Length)];
                    return null;
                }

                case EnemyAttackType.AoE:
                {
                    // AoE hits all — return null to signal "hit everyone"
                    return null;
                }

                default:
                    return partyManager.GetHero(0);
            }
        }

        public void ClearAll()
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] != null)
                    Destroy(_activeEnemies[i].gameObject);
            }
            _activeEnemies.Clear();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
