using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Party;
using Ascendant.Heroes;

namespace Ascendant.UI
{
    public class HeroesPanelController : MonoBehaviour
    {
        [Header("Hero Detail")]
        [SerializeField] GameObject _heroDetailPanel;
        [SerializeField] TextMeshProUGUI _statsText;
        [SerializeField] Button _backButton;
        [SerializeField] Button _skillTreeButton;
        [SerializeField] Button _equipmentButton;
        [SerializeField] Button _ascensionButton;

        [Header("Skill Tree UI")]
        [SerializeField] SkillTreeUI _skillTreeUI;

        int _selectedSlot = -1;
        string _selectedClassId;

        // Hero button names match UIScreenBuilder: HeroBtn_Warrior, HeroBtn_Mage, etc.
        static readonly string[] HeroButtonNames = { "HeroBtn_Warrior", "HeroBtn_Mage", "HeroBtn_Priest", "HeroBtn_Rogue" };

        void Start()
        {
            WireHeroButtons();

            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(HideDetail);
            }

            if (_skillTreeButton != null)
            {
                _skillTreeButton.onClick.RemoveAllListeners();
                _skillTreeButton.onClick.AddListener(OpenSkillTree);
            }

            if (_equipmentButton != null)
            {
                _equipmentButton.onClick.RemoveAllListeners();
                _equipmentButton.onClick.AddListener(() => Debug.Log("[HeroesPanelController] Equipment: Coming Soon"));
            }

            if (_ascensionButton != null)
            {
                _ascensionButton.onClick.RemoveAllListeners();
                _ascensionButton.onClick.AddListener(() => Debug.Log("[HeroesPanelController] Ascension: Coming Soon"));
            }
        }

        void WireHeroButtons()
        {
            var content = transform.Find("Content");
            if (content == null) return;

            for (int i = 0; i < HeroButtonNames.Length; i++)
            {
                var btnTransform = content.Find(HeroButtonNames[i]);
                if (btnTransform == null) continue;

                var btn = btnTransform.GetComponent<Button>();
                if (btn == null) continue;

                int slot = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowDetail(slot));
            }
        }

        void ShowDetail(int slot)
        {
            _selectedSlot = slot;

            var pm = PartyManager.Instance;
            if (pm == null) return;

            var hero = pm.GetHero(slot);
            if (hero != null)
            {
                _selectedClassId = hero.Data?.classId ?? "";
                if (_statsText != null)
                {
                    _statsText.text = $"ATK: {hero.CurrentAtk:F0}\nDEF: {hero.CurrentDef:F0}\nHP: {hero.MaxHp:F0}\nSPD: {hero.CurrentSpd:F2}";
                }
            }
            else
            {
                _selectedClassId = slot switch
                {
                    0 => "warrior",
                    1 => "mage",
                    2 => "priest",
                    3 => "rogue",
                    _ => ""
                };
                if (_statsText != null)
                    _statsText.text = "ATK: --\nDEF: --\nHP: --\nSPD: --";
            }

            if (_heroDetailPanel != null)
                _heroDetailPanel.SetActive(true);
        }

        void HideDetail()
        {
            if (_heroDetailPanel != null)
                _heroDetailPanel.SetActive(false);
            _selectedSlot = -1;
        }

        void OpenSkillTree()
        {
            if (_selectedSlot < 0 || _skillTreeUI == null) return;
            _skillTreeUI.Open(_selectedSlot, _selectedClassId);
        }
    }
}
