using System;
using System.Collections.Generic;
using UnityEngine;
using Ascendant.Combat;
using Ascendant.Core;
using Ascendant.Islands;

namespace Ascendant.Events
{
    public enum SeasonalEventTheme
    {
        FlameFestival,
        FrostCarnival,
        StormTournament,
        NaturesBloom,
        ShadowMasquerade,
        RadianceCelebration
    }

    [Serializable]
    public class EventShopItem
    {
        public string itemId;
        public string itemName;
        [TextArea(1, 2)]
        public string description;
        public int eventCurrencyCost;
        public CurrencyType rewardCurrencyType;
        public double rewardAmount;
        public bool isCosmetic;
        public bool isLimited;
        public int stockLimit;
    }

    [CreateAssetMenu(fileName = "NewEventConfig", menuName = "Ascendant/Event Config")]
    public class EventConfig : ScriptableObject
    {
        [Header("Identity")]
        public string eventId;
        public string eventName;
        [TextArea(2, 4)]
        public string description;
        public SeasonalEventTheme theme;

        [Header("Schedule")]
        public int durationDays = 14;

        [Header("Event Island")]
        public string eventIslandName;
        public int eventStageCount = 20;
        public Affinity eventAffinity;
        public BiomeData eventBiome;
        public List<EnemyData> eventEnemies;

        [Header("Event Boss (Stage 20)")]
        public string eventBossName;
        [TextArea(1, 2)]
        public string eventBossDescription;
        public float eventBossHpMultiplier = 12f;
        public float eventBossAtkMultiplier = 3f;
        public List<BossPhaseData> eventBossPhases;

        [Header("Event Currency")]
        public string eventCurrencyName;
        public float eventCurrencyDropRate = 1f;

        [Header("Event Shop")]
        public List<EventShopItem> shopItems;

        [Header("Event Quests")]
        public List<EventQuestDef> eventQuests;

        [Header("Gacha Banner")]
        public string bannerHeroClassId;
        public float bannerRateBoost = 2f;
    }

    [Serializable]
    public class EventQuestDef
    {
        public string questId;
        public string questName;
        [TextArea(1, 2)]
        public string description;
        public int requiredProgress;
        public int eventCurrencyReward;
    }
}
