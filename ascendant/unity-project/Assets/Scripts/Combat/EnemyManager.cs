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
