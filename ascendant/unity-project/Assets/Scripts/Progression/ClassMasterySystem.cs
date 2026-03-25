using System.Collections.Generic;
using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Progression
{
    [System.Serializable]
    public class ClassMasteryProgress
    {
        public string classId;
        public int totalStagesCleared;
        public int totalAscensions;
        public double totalDamageDealt;
        public int totalEnemiesKilled;
        public MasteryTier currentTier = MasteryTier.Novice;
    }

    public class ClassMasterySystem : MonoBehaviour
    {
        public static ClassMasterySystem Instance { get; private set; }

        [Header("Mastery Definitions")]
        [SerializeField] ClassMasteryData[] _masteryDatas;

        // classId -> progress (PERMANENT, never resets)
        readonly Dictionary<string, ClassMasteryProgress> _progress = new();

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
            EventBus.Subscribe<WaveClearedEvent>(OnWaveCleared);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<WaveClearedEvent>(OnWaveCleared);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
        }

        void OnWaveCleared(WaveClearedEvent evt)
        {
            // Credit stage clear to all active heroes
            var party = Party.PartyManager.Instance;
            if (party == null) return;

            var heroes = party.GetAllAliveHeroes();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i]?.Data == null) continue;
                string classId = heroes[i].Data.classId;
                var progress = GetOrCreateProgress(classId);
                progress.totalStagesCleared++;
                CheckTierUp(classId, progress);
            }
        }

        void OnEnemyKilled(EnemyKilledEvent evt)
        {
            var party = Party.PartyManager.Instance;
            if (party == null) return;

            var heroes = party.GetAllAliveHeroes();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i]?.Data == null) continue;
                var progress = GetOrCreateProgress(heroes[i].Data.classId);
                progress.totalEnemiesKilled++;
            }
        }

        void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            var party = Party.PartyManager.Instance;
            if (party == null) return;

            var heroes = party.GetAllAliveHeroes();
            if (heroes.Length == 0) return;

            // Attribute damage equally among alive heroes
            double perHero = evt.Damage / heroes.Length;
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i]?.Data == null) continue;
                var progress = GetOrCreateProgress(heroes[i].Data.classId);
                progress.totalDamageDealt += perHero;
            }
        }

        public void RecordAscension(string classId)
        {
            var progress = GetOrCreateProgress(classId);
            progress.totalAscensions++;
            CheckTierUp(classId, progress);
        }

        void CheckTierUp(string classId, ClassMasteryProgress progress)
        {
            var data = GetMasteryData(classId);
            if (data == null) return;

            var newTier = data.GetTierForStages(progress.totalStagesCleared);
            if (newTier != progress.currentTier)
            {
                progress.currentTier = newTier;
                EventBus.Publish(new ClassMasteryTierUpEvent
                {
                    ClassId = classId,
                    NewTier = newTier
                });
            }
        }

        public ClassMasteryData GetMasteryData(string classId)
        {
            if (_masteryDatas == null) return null;
            for (int i = 0; i < _masteryDatas.Length; i++)
                if (_masteryDatas[i] != null && _masteryDatas[i].classId == classId)
                    return _masteryDatas[i];
            return null;
        }

        public ClassMasteryProgress GetProgress(string classId)
        {
            return _progress.TryGetValue(classId, out var p) ? p : null;
        }

        public float GetMasteryStatBonusPercent(string classId)
        {
            var data = GetMasteryData(classId);
            if (data == null) return 0f;
            var progress = GetProgress(classId);
            if (progress == null) return 0f;
            return data.GetTotalStatBonus(progress.totalStagesCleared);
        }

        ClassMasteryProgress GetOrCreateProgress(string classId)
        {
            if (!_progress.TryGetValue(classId, out var p))
            {
                p = new ClassMasteryProgress { classId = classId };
                _progress[classId] = p;
            }
            return p;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
