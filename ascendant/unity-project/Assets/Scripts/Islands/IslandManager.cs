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

        [Header("Realm 2 Islands (ordered 13-24)")]
        [SerializeField] List<IslandData> _realm2Islands;

        [Header("Realm 3 Islands (ordered 25-36)")]
        [SerializeField] List<IslandData> _realm3Islands;

        HashSet<int> _unlockedIslands = new();
        HashSet<int> _completedIslands = new();
        int _currentIslandIndex;
        int _highestRealmUnlocked = 1;

        // Combined island list (all realms)
        List<IslandData> _allIslands;

        public IslandData CurrentIsland =>
            _currentIslandIndex >= 0 && _currentIslandIndex < _allIslands.Count
                ? _allIslands[_currentIslandIndex]
                : null;

        public int CurrentIslandNumber => _currentIslandIndex + 1;
        public IReadOnlyList<IslandData> AllIslands => _allIslands;
        public int HighestRealmUnlocked => _highestRealmUnlocked;
        public int TotalIslandCount => _allIslands.Count;

        public bool AllRealm1Cleared => AreRealmIslandsCleared(1);
        public bool AllRealm2Cleared => AreRealmIslandsCleared(2);
        public bool AllRealm3Cleared => AreRealmIslandsCleared(3);

        public bool AllIslandsCleared => _completedIslands.Count >= _allIslands.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildIslandList();
            InitializeIslands();
        }

        void BuildIslandList()
        {
            _allIslands = new List<IslandData>();
            if (_realm1Islands != null) _allIslands.AddRange(_realm1Islands);
            if (_realm2Islands != null) _allIslands.AddRange(_realm2Islands);
            if (_realm3Islands != null) _allIslands.AddRange(_realm3Islands);
        }

        void InitializeIslands()
        {
            _unlockedIslands.Add(0);
            _currentIslandIndex = 0;
        }

        void OnEnable()
        {
            EventBus.Subscribe<RealmBossDefeatedEvent>(OnRealmBossDefeated);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<RealmBossDefeatedEvent>(OnRealmBossDefeated);
        }

        void OnRealmBossDefeated(RealmBossDefeatedEvent evt)
        {
            if (evt.RealmNumber >= _highestRealmUnlocked)
            {
                _highestRealmUnlocked = evt.RealmNumber + 1;

                // Unlock first island of next realm
                int nextRealmStart = GetRealmStartIndex(evt.RealmNumber + 1);
                if (nextRealmStart >= 0 && nextRealmStart < _allIslands.Count)
                {
                    _unlockedIslands.Add(nextRealmStart);
                    EventBus.Publish(new IslandUnlockedEvent { IslandIndex = nextRealmStart });
                }

                Debug.Log($"[IslandManager] Realm {evt.RealmNumber + 1} unlocked!");
            }
        }

        int GetRealmStartIndex(int realmNumber)
        {
            return realmNumber switch
            {
                1 => 0,
                2 => _realm1Islands?.Count ?? 12,
                3 => (_realm1Islands?.Count ?? 12) + (_realm2Islands?.Count ?? 12),
                _ => -1
            };
        }

        int GetRealmEndIndex(int realmNumber)
        {
            return realmNumber switch
            {
                1 => (_realm1Islands?.Count ?? 12) - 1,
                2 => (_realm1Islands?.Count ?? 12) + (_realm2Islands?.Count ?? 12) - 1,
                3 => _allIslands.Count - 1,
                _ => -1
            };
        }

        public int GetIslandRealm(int index)
        {
            int r1 = _realm1Islands?.Count ?? 0;
            int r2 = _realm2Islands?.Count ?? 0;
            if (index < r1) return 1;
            if (index < r1 + r2) return 2;
            return 3;
        }

        bool AreRealmIslandsCleared(int realmNumber)
        {
            int start = GetRealmStartIndex(realmNumber);
            int end = GetRealmEndIndex(realmNumber);
            if (start < 0 || end < 0) return false;

            for (int i = start; i <= end; i++)
            {
                if (!_completedIslands.Contains(i)) return false;
            }
            return true;
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
            if (index >= 0 && index < _allIslands.Count)
                return _allIslands[index];
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
            if (nextIndex < _allIslands.Count)
            {
                _unlockedIslands.Add(nextIndex);
                EventBus.Publish(new IslandUnlockedEvent { IslandIndex = nextIndex });
            }

            EventBus.Publish(new IslandCompletedEvent
            {
                IslandIndex = _currentIslandIndex,
                IslandData = CurrentIsland
            });

            // Check realm completion for realm boss unlock
            int currentRealm = GetIslandRealm(_currentIslandIndex);
            if (AreRealmIslandsCleared(currentRealm))
            {
                EventBus.Publish(new RealmBossUnlockedEvent { RealmNumber = currentRealm });
            }
        }

        public void AdvanceToNextIsland()
        {
            int nextIndex = _currentIslandIndex + 1;
            if (nextIndex < _allIslands.Count && IsIslandUnlocked(nextIndex))
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

        public void ResetForAscension()
        {
            _completedIslands.Clear();
            _unlockedIslands.Clear();
            _unlockedIslands.Add(0);
            _currentIslandIndex = 0;

            EventBus.Publish(new IslandChangedEvent
            {
                IslandIndex = 0,
                IslandData = CurrentIsland
            });
        }

        public int HighestCompletedIsland
        {
            get
            {
                int highest = 0;
                foreach (int idx in _completedIslands)
                    if (idx + 1 > highest) highest = idx + 1;
                return highest;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
