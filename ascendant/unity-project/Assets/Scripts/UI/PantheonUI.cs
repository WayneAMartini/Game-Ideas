using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class PantheonUI : MonoBehaviour
    {
        public static PantheonUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] GameObject _pantheonPanel;

        [Header("Grid (24 slots)")]
        [SerializeField] Transform _slotContainer;
        [SerializeField] GameObject _slotPrefab;

        [Header("Info")]
        [SerializeField] TextMeshProUGUI _totalDemigodsText;
        [SerializeField] TextMeshProUGUI _nextMilestoneText;
        [SerializeField] TextMeshProUGUI _currentBonusText;

        [Header("Milestone Display")]
        [SerializeField] Transform _milestoneContainer;
        [SerializeField] GameObject _milestonePrefab;

        [Header("Detail Panel")]
        [SerializeField] GameObject _detailPanel;
        [SerializeField] TextMeshProUGUI _detailClassText;
        [SerializeField] TextMeshProUGUI _detailBuffText;
        [SerializeField] Image _detailPortrait;
        [SerializeField] Button _closeDetailButton;

        [Header("Colors")]
        [SerializeField] Color _filledColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] Color _emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("Close")]
        [SerializeField] Button _closeButton;

        // All 24 class IDs in order
        static readonly string[] AllClassIds =
        {
            "warrior", "mage", "priest", "rogue",
            "marksman", "defender", "berserker", "druid", "thief", "shaman",
            "warlock", "ranger", "spellblade", "necromancer", "monk", "paladin", "bard",
            "dragonhunter", "summoner", "alchemist", "chronomancer", "gunslinger", "warden", "reaper"
        };

        static readonly string[] AllClassNames =
        {
            "Warrior", "Mage", "Priest", "Rogue",
            "Marksman", "Defender", "Berserker", "Druid", "Thief", "Shaman",
            "Warlock", "Ranger", "Spell-Blade", "Necromancer", "Monk", "Paladin", "Bard",
            "Dragon-Hunter", "Summoner", "Alchemist", "Chronomancer", "Gunslinger", "Warden", "Reaper"
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

        void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_closeDetailButton != null) _closeDetailButton.onClick.AddListener(CloseDetail);
            if (_pantheonPanel != null) _pantheonPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
        }

        void OnEnable()
        {
            EventBus.Subscribe<DemigodRetiredEvent>(OnDemigodRetired);
            EventBus.Subscribe<PantheonMilestoneEvent>(OnMilestoneReached);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<DemigodRetiredEvent>(OnDemigodRetired);
            EventBus.Unsubscribe<PantheonMilestoneEvent>(OnMilestoneReached);
        }

        public void Open()
        {
            if (_pantheonPanel != null) _pantheonPanel.SetActive(true);
            RefreshDisplay();
        }

        public void Close()
        {
            if (_pantheonPanel != null) _pantheonPanel.SetActive(false);
            if (_detailPanel != null) _detailPanel.SetActive(false);
        }

        void RefreshDisplay()
        {
            var demigodSystem = DemigodSystem.Instance;
            if (demigodSystem == null) return;

            int filled = demigodSystem.PantheonSlotsFilled;

            if (_totalDemigodsText != null)
                _totalDemigodsText.text = $"Demigods: {filled} / {DemigodSystem.MaxPantheonSlots}";

            var nextMilestone = demigodSystem.GetNextMilestone();
            if (_nextMilestoneText != null)
            {
                if (nextMilestone.HasValue)
                    _nextMilestoneText.text = $"Next Milestone: {nextMilestone.Value.SlotsRequired} ({nextMilestone.Value.Name})";
                else
                    _nextMilestoneText.text = "All Milestones Complete!";
            }

            if (_currentBonusText != null)
            {
                float bonus = demigodSystem.GetGlobalStatBonusPercent();
                _currentBonusText.text = bonus > 0 ? $"Pantheon Bonus: +{bonus}% All Stats" : "";
            }

            RefreshSlots();
            RefreshMilestones();
        }

        void RefreshSlots()
        {
            if (_slotContainer == null) return;

            // Clear existing
            for (int i = _slotContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotContainer.GetChild(i).gameObject);

            var demigodSystem = DemigodSystem.Instance;

            for (int i = 0; i < AllClassIds.Length; i++)
            {
                CreateSlot(AllClassIds[i], AllClassNames[i], demigodSystem);
            }
        }

        void CreateSlot(string classId, string className, DemigodSystem demigodSystem)
        {
            if (_slotPrefab == null || _slotContainer == null) return;

            var go = Instantiate(_slotPrefab, _slotContainer);
            var btn = go.GetComponent<Button>();
            var img = go.GetComponent<Image>();
            var label = go.GetComponentInChildren<TextMeshProUGUI>();

            bool isFilled = demigodSystem != null && demigodSystem.IsClassRetired(classId);

            if (img != null)
                img.color = isFilled ? _filledColor : _emptyColor;
            if (label != null)
                label.text = className;

            string id = classId;
            string name = className;
            if (btn != null)
                btn.onClick.AddListener(() => ShowDetail(id, name));
        }

        void ShowDetail(string classId, string className)
        {
            var demigodSystem = DemigodSystem.Instance;
            if (demigodSystem == null) return;

            if (_detailClassText != null)
                _detailClassText.text = className;

            var demigod = demigodSystem.GetDemigod(classId);
            if (demigod != null)
            {
                if (_detailBuffText != null)
                    _detailBuffText.text = $"Demigod Patron\n\n{demigod.buffDescription}";
            }
            else
            {
                if (_detailBuffText != null)
                    _detailBuffText.text = "Not yet ascended to Demigod status.\n\nRequires 10 ascensions + Transcendence Trial.";
            }

            if (_detailPanel != null) _detailPanel.SetActive(true);
        }

        void CloseDetail()
        {
            if (_detailPanel != null) _detailPanel.SetActive(false);
        }

        void RefreshMilestones()
        {
            if (_milestoneContainer == null || _milestonePrefab == null) return;
            var demigodSystem = DemigodSystem.Instance;
            if (demigodSystem == null) return;

            // Clear existing
            for (int i = _milestoneContainer.childCount - 1; i >= 0; i--)
                Destroy(_milestoneContainer.GetChild(i).gameObject);

            var milestones = demigodSystem.GetAllMilestones();
            for (int i = 0; i < milestones.Length; i++)
            {
                var go = Instantiate(_milestonePrefab, _milestoneContainer);
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                bool reached = demigodSystem.IsMilestoneReached(milestones[i].SlotsRequired);

                if (label != null)
                {
                    string status = reached ? "[ACTIVE]" : $"[{milestones[i].SlotsRequired} Demigods]";
                    label.text = $"{status} {milestones[i].Name}: {milestones[i].Description}";
                }

                var img = go.GetComponent<Image>();
                if (img != null)
                    img.color = reached ? _filledColor : _emptyColor;
            }
        }

        void OnDemigodRetired(DemigodRetiredEvent evt)
        {
            if (_pantheonPanel != null && _pantheonPanel.activeSelf)
                RefreshDisplay();
        }

        void OnMilestoneReached(PantheonMilestoneEvent evt)
        {
            if (_pantheonPanel != null && _pantheonPanel.activeSelf)
                RefreshDisplay();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
