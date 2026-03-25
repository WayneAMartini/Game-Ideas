using UnityEngine;

namespace Ascendant.Progression
{
    [CreateAssetMenu(fileName = "NewSkillTree", menuName = "Ascendant/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        [Header("Identity")]
        public string classId;
        public string className;

        [Header("Branches")]
        public SkillBranch[] branches = new SkillBranch[3];
    }

    [System.Serializable]
    public class SkillBranch
    {
        public string branchId;
        public string branchName;
        [TextArea] public string branchDescription;
        public SkillNodeDefinition[] nodes;
    }
}
