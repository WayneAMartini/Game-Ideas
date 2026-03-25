using System;
using UnityEngine;

namespace Ascendant.Progression
{
    public enum SkillNodeType
    {
        PassiveStat,
        AbilityModifier,
        Capstone
    }

    public enum SkillNodeState
    {
        Locked,
        Available,
        Purchased
    }

    [Serializable]
    public class SkillNodeDefinition
    {
        public string nodeId;
        public string nodeName;
        [TextArea] public string description;
        public SkillNodeType nodeType;
        public int cost = 1; // 1-3 skill points
        public int unlockLevel; // hero level required
        public string[] prerequisiteNodeIds; // parent node IDs

        [Header("Stat Bonuses")]
        public StatRoll[] statBonuses;

        [Header("Ability Modifier (if applicable)")]
        public string modifiedAbilityId;
        public float abilityBonusMultiplier;

        [Header("Visual")]
        public Vector2 treePosition; // position in the tree layout
        public bool isCapstone;
    }

    [Serializable]
    public class SkillNodeSaveData
    {
        public string nodeId;
        public SkillNodeState state;

        public SkillNodeSaveData(string id, SkillNodeState s)
        {
            nodeId = id;
            state = s;
        }
    }
}
