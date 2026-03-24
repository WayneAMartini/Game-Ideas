using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Combat
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] int _minEnemiesPerWave = 3;
        [SerializeField] int _maxEnemiesPerWave = 5;
        [SerializeField] Transform _spawnParent;

        [Header("Spawn Area (Combat Viewport)")]
        [SerializeField] float _spawnXMin = 1f;
        [SerializeField] float _spawnXMax = 3f;
        [SerializeField] float _spawnYMin = -1f;
        [SerializeField] float _spawnYMax = 2f;

        [Header("Enemy Prefab")]
        [SerializeField] Enemy _enemyPrefab;

        [Header("Enemy Types")]
        [SerializeField] List<EnemyData> _enemyTypes;

        public void SpawnWave(int stage)
        {
            if (_enemyTypes == null || _enemyTypes.Count == 0)
            {
                Debug.LogWarning("EnemySpawner: No enemy types configured!");
                return;
            }

            int count = Random.Range(_minEnemiesPerWave, _maxEnemiesPerWave + 1);

            for (int i = 0; i < count; i++)
            {
                var data = _enemyTypes[Random.Range(0, _enemyTypes.Count)];
                SpawnEnemy(data, stage, i, count);
            }
        }

        void SpawnEnemy(EnemyData data, int stage, int index, int total)
        {
            // Distribute enemies vertically within the spawn area
            float t = total > 1 ? (float)index / (total - 1) : 0.5f;
            float x = Random.Range(_spawnXMin, _spawnXMax);
            float y = Mathf.Lerp(_spawnYMin, _spawnYMax, t);

            Vector3 pos = new Vector3(x, y, 0f);

            var parent = _spawnParent != null ? _spawnParent : transform;
            var enemy = Instantiate(_enemyPrefab, pos, Quaternion.identity, parent);
            enemy.Initialize(data, stage);

            EnemyManager.Instance?.Register(enemy);
        }
    }
}
