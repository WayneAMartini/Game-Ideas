using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;
using Ascendant.Progression;
using Ascendant.Islands;

namespace Ascendant.Backend
{
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        // Leaderboard IDs
        public const string LB_HIGHEST_ISLAND = "highest_island";
        public const string LB_SPEED_ASCENSION = "speed_ascension";
        public const string LB_BOSS_DAMAGE = "boss_damage";
        public const string LB_PANTHEON_RACE = "pantheon_race";
        public const string LB_ARENA_RANK = "arena_rank";
        public const string LB_TOWER_FLOOR = "tower_floor";
        public const string LB_INFINITE_ISLAND = "infinite_island";

        public static readonly string[] AllLeaderboards =
        {
            LB_HIGHEST_ISLAND, LB_SPEED_ASCENSION, LB_BOSS_DAMAGE, LB_PANTHEON_RACE, LB_ARENA_RANK,
            LB_TOWER_FLOOR, LB_INFINITE_ISLAND
        };

        public static readonly string[] LeaderboardDisplayNames =
        {
            "Highest Island", "Speed Ascension", "Boss Damage", "Pantheon Race", "Arena Rank",
            "Tower of Trials", "Infinite Ascension"
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Subscribe<AscensionEvent>(OnAscension);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<DemigodRetiredEvent>(OnDemigodRetired);
            EventBus.Subscribe<ArenaMatchResultEvent>(OnArenaResult);
            EventBus.Subscribe<TowerFloorClearedEvent>(OnTowerFloorCleared);
            EventBus.Subscribe<InfiniteAscensionIslandCompletedEvent>(OnInfiniteIslandCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<StageAdvancedEvent>(OnStageAdvanced);
            EventBus.Unsubscribe<AscensionEvent>(OnAscension);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<DemigodRetiredEvent>(OnDemigodRetired);
            EventBus.Unsubscribe<ArenaMatchResultEvent>(OnArenaResult);
            EventBus.Unsubscribe<TowerFloorClearedEvent>(OnTowerFloorCleared);
            EventBus.Unsubscribe<InfiniteAscensionIslandCompletedEvent>(OnInfiniteIslandCompleted);
        }

        void OnStageAdvanced(StageAdvancedEvent evt)
        {
            // Score = island * 100 + stage
            long score = evt.Island * 100L + evt.NewStage;
            SubmitScore(LB_HIGHEST_ISLAND, score);
        }

        void OnAscension(AscensionEvent evt)
        {
            // Speed ascension: lower time = better (submit inverse)
            // For now, submit highest island as a proxy
            long score = evt.HighestIslandReached * 100L;
            SubmitScore(LB_SPEED_ASCENSION, score);
        }

        void OnBossDefeated(BossDefeatedEvent evt)
        {
            // Boss damage tracked separately during world boss fights
            // This is for island/realm boss completion tracking
        }

        void OnDemigodRetired(DemigodRetiredEvent evt)
        {
            long score = evt.PantheonSlotsFilled;
            SubmitScore(LB_PANTHEON_RACE, score);
        }

        void OnArenaResult(ArenaMatchResultEvent evt)
        {
            SubmitScore(LB_ARENA_RANK, evt.NewElo);
        }

        public void SubmitBossDamage(long damage)
        {
            SubmitScore(LB_BOSS_DAMAGE, damage);
        }

        void OnTowerFloorCleared(TowerFloorClearedEvent evt)
        {
            SubmitScore(LB_TOWER_FLOOR, evt.PersonalBest);
        }

        void OnInfiniteIslandCompleted(InfiniteAscensionIslandCompletedEvent evt)
        {
            SubmitScore(LB_INFINITE_ISLAND, evt.HighestIsland);
        }

        void SubmitScore(string leaderboardId, long score)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null) return;

            service.SubmitScore(leaderboardId, score, success =>
            {
                if (success)
                {
                    EventBus.Publish(new LeaderboardScoreSubmittedEvent
                    {
                        LeaderboardId = leaderboardId,
                        Score = score
                    });
                }
                else
                {
                    // Queue for offline retry
                    OfflineQueue.Instance?.Enqueue(new OfflineAction
                    {
                        Type = OfflineActionType.LeaderboardSubmit,
                        Payload = $"{leaderboardId}|{score}"
                    });
                }
            });
        }

        public void GetLeaderboard(string leaderboardId, bool weekly, int count,
            System.Action<List<LeaderboardEntry>> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            service.GetLeaderboard(leaderboardId, weekly, count, callback);
        }

        public void GetPlayerRank(string leaderboardId, bool weekly,
            System.Action<int, float> callback)
        {
            var service = FirebaseManager.Instance?.Service;
            if (service == null)
            {
                callback?.Invoke(-1, 0f);
                return;
            }

            service.GetPlayerRank(leaderboardId, weekly, callback);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
