using UnityEngine;

namespace Ascendant.Economy
{
    public enum QuestType
    {
        KillEnemies,
        ClearStages,
        UseAbilities,
        CollectGold,
        CompleteExpeditions
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "Ascendant/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string questId;
        public string questName;
        public string description;
        public QuestType questType;
        public int requiredAmount;
        public bool isWeekly;

        [Header("Rewards")]
        public int battlePassXP = 50;
        public int goldReward;
        public int stardustReward;
    }
}
