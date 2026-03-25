using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Combat;

namespace Ascendant.Islands
{
    public class IslandManager : MonoBehaviour
    {
        public static IslandManager Instance { get; private set; }

        [Header("Realm 1 Islands (ordered 1-12)")]
        [SerializeField] List<IslandData> _realm1Islands;

        HashSet<int> _unlockedIslands = new();
        HashSet<int> _completedIslands = new();
        int _currentIslandIndex;

        public IslandData CurrentIsland =>
            _currentIslandIndex >= 0 && _currentIslandIndex < _realm1Islands.Count
                ? _realm1Islands[_currentIslandIndex]
                : null;

        public int CurrentIslandNumber => _currentIslandIndex + 1;
        public IReadOnlyList<IslandData> AllIslands => _realm1Islands;
        public bool AllIslandsCleared => _completedIslands.Count >= _realm1Islands.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeIslands();
        }

        void InitializeIslands()
        {
            // Island 1 is always unlocked
            _unlockedIslands.Add(0);
            _currentIslandIndex = 0;
        }

        public bool IsIslandUnlocked(int index)
        {
            return _unlockedIslands.Contains(index);
        }

        public bool IsIslandCompleted(int index)
        {
            return _completedIslands.Contains(index);
        }

        public IslandData GetIsland(int index)
        {
            if (index >= 0 && index < _realm1Islands.Count)
                return _realm1Islands[index];
            return null;
        }

        public void SetCurrentIsland(int index)
        {
            if (!IsIslandUnlocked(index)) return;
            _currentIslandIndex = index;

            EventBus.Publish(new IslandChangedEvent
            {
                IslandIndex = index,
                IslandData = CurrentIsland
            });
        }

        public void CompleteCurrentIsland()
        {
            _completedIslands.Add(_currentIslandIndex);

            // Unlock next island
            int nextIndex = _currentIslandIndex + 1;
            if (nextIndex < _realm1Islands.Count)
            {
                _unlockedIslands.Add(nextIndex);
                EventBus.Publish(new IslandUnlockedEvent { IslandIndex = nextIndex });
            }

            EventBus.Publish(new IslandCompletedEvent
            {
                IslandIndex = _currentIslandIndex,
                IslandData = CurrentIsland
            });

            // Check if all islands cleared (Realm Boss unlock)
            if (AllIslandsCleared)
            {
                EventBus.Publish(new RealmBossUnlockedEvent { RealmNumber = 1 });
            }
        }

        public void AdvanceToNextIsland()
        {
            int nextIndex = _currentIslandIndex + 1;
            if (nextIndex < _realm1Islands.Count && IsIslandUnlocked(nextIndex))
            {
                SetCurrentIsland(nextIndex);
            }
        }

        public Affinity GetCurrentBiomeAffinity()
        {
            return CurrentIsland != null ? CurrentIsland.affinity : Affinity.None;
        }

        public List<EnemyData> GetCurrentEnemyTypes()
        {
            return CurrentIsland?.enemyTypes;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
