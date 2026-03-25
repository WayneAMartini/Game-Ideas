using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class ArenaUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject _mainPanel;
        [SerializeField] GameObject _opponentSelectPanel;
        [SerializeField] GameObject _battlePanel;
        [SerializeField] GameObject _resultPanel;
        [SerializeField] GameObject _defenseSetupPanel;

        [Header("Main Panel")]
        [SerializeField] TextMeshProUGUI _rankText;
        [SerializeField] TextMeshProUGUI _eloText;
        [SerializeField] TextMeshProUGUI _recordText;
        [SerializeField] TextMeshProUGUI _attemptsText;
        [SerializeField] TextMeshProUGUI _seasonText;
        [SerializeField] Button _findOpponentButton;
        [SerializeField] Button _defenseSetupButton;
        [SerializeField] Button _refreshAttemptsButton;

        [Header("Opponent Selection")]
        [SerializeField] Transform _opponentContainer;
        [SerializeField] GameObject _opponentCardPrefab;
        [SerializeField] Button _backFromOpponentsButton;

        [Header("Battle")]
        [SerializeField] TextMeshProUGUI _battleLogText;
        [SerializeField] Slider _playerHpBar;
        [SerializeField] Slider _opponentHpBar;
        [SerializeField] TextMeshProUGUI _playerTeamLabel;
        [SerializeField] TextMeshProUGUI _opponentTeamLabel;

        [Header("Result")]
        [SerializeField] TextMeshProUGUI _resultTitle;
        [SerializeField] TextMeshProUGUI _eloChangeText;
        [SerializeField] TextMeshProUGUI _newRankText;
        [SerializeField] Button _resultOkButton;

        [Header("Defense Setup")]
        [SerializeField] TextMeshProUGUI _defenseInfoText;
        [SerializeField] Button _autoSetDefenseButton;
        [SerializeField] Button _backFromDefenseButton;

        ArenaDefenseTeam _selectedOpponent;

        void Start()
        {
            if (_findOpponentButton != null)
                _findOpponentButton.onClick.AddListener(OnFindOpponents);
            if (_defenseSetupButton != null)
                _defenseSetupButton.onClick.AddListener(() => ShowPanel(_defenseSetupPanel));
            if (_refreshAttemptsButton != null)
                _refreshAttemptsButton.onClick.AddListener(OnRefreshAttempts);
            if (_resultOkButton != null)
                _resultOkButton.onClick.AddListener(() => ShowPanel(_mainPanel));
            if (_backFromOpponentsButton != null)
                _backFromOpponentsButton.onClick.AddListener(() => ShowPanel(_mainPanel));
            if (_autoSetDefenseButton != null)
                _autoSetDefenseButton.onClick.AddListener(OnAutoSetDefense);
            if (_backFromDefenseButton != null)
                _backFromDefenseButton.onClick.AddListener(() => ShowPanel(_mainPanel));

            ShowPanel(_mainPanel);
        }

        void OnEnable()
        {
            EventBus.Subscribe<ArenaMatchResultEvent>(OnMatchResult);
            RefreshMainPanel();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ArenaMatchResultEvent>(OnMatchResult);
        }

        void OnMatchResult(ArenaMatchResultEvent evt)
        {
            RefreshMainPanel();
        }

        void ShowPanel(GameObject panel)
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_opponentSelectPanel != null) _opponentSelectPanel.SetActive(false);
            if (_battlePanel != null) _battlePanel.SetActive(false);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (_defenseSetupPanel != null) _defenseSetupPanel.SetActive(false);

            if (panel != null) panel.SetActive(true);

            if (panel == _mainPanel) RefreshMainPanel();
        }

        void RefreshMainPanel()
        {
            var arena = ArenaManager.Instance;
            if (arena == null) return;

            if (_rankText != null) _rankText.text = arena.CurrentRank.ToString();
            if (_eloText != null) _eloText.text = $"ELO: {arena.Elo}";
            if (_recordText != null) _recordText.text = $"W: {arena.Wins} / L: {arena.Losses}";
            if (_attemptsText != null) _attemptsText.text = $"Attempts: {arena.RemainingAttempts}/{arena.AttemptsPerDay}";

            if (_findOpponentButton != null)
                _findOpponentButton.interactable = arena.CanAttack;

            if (_defenseInfoText != null)
            {
                var defense = arena.PlayerDefense;
                if (defense?.Heroes != null && defense.Heroes.Count > 0)
                {
                    string heroList = "";
                    foreach (var h in defense.Heroes)
                        heroList += $"{h.ClassId} Lv.{h.Level}, ";
                    _defenseInfoText.text = $"Defense Team: {heroList.TrimEnd(',', ' ')}";
                }
                else
                {
                    _defenseInfoText.text = "No defense team set";
                }
            }
        }

        void OnFindOpponents()
        {
            var arena = ArenaManager.Instance;
            if (arena == null) return;

            arena.FindOpponents(opponents =>
            {
                PopulateOpponents(opponents);
                ShowPanel(_opponentSelectPanel);
            });
        }

        void PopulateOpponents(List<ArenaDefenseTeam> opponents)
        {
            if (_opponentContainer == null || _opponentCardPrefab == null) return;

            ClearContainer(_opponentContainer);

            var arena = ArenaManager.Instance;
            int playerElo = arena?.Elo ?? 1000;

            ArenaMatchmaker.CategorizeOpponents(opponents, playerElo,
                out var easy, out var medium, out var hard);

            if (easy != null) CreateOpponentCard(easy, ArenaDifficulty.Easy);
            if (medium != null) CreateOpponentCard(medium, ArenaDifficulty.Medium);
            if (hard != null) CreateOpponentCard(hard, ArenaDifficulty.Hard);
        }

        void CreateOpponentCard(ArenaDefenseTeam opponent, ArenaDifficulty difficulty)
        {
            var go = Instantiate(_opponentCardPrefab, _opponentContainer);
            go.SetActive(true);

            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = opponent.DisplayName;
                texts[1].text = $"{opponent.Rank} ({opponent.EloRating})";

                float power = ArenaMatchmaker.EstimateTeamPower(opponent);
                texts[2].text = $"Power: {power:F0} [{difficulty}]";
            }

            // Color by difficulty
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = difficulty switch
                {
                    ArenaDifficulty.Easy => new Color(0.3f, 0.8f, 0.3f, 0.3f),
                    ArenaDifficulty.Hard => new Color(0.8f, 0.3f, 0.3f, 0.3f),
                    _ => new Color(0.8f, 0.8f, 0.3f, 0.3f)
                };
            }

            var btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                var captured = opponent;
                btn.onClick.AddListener(() => OnSelectOpponent(captured));
            }
        }

        void OnSelectOpponent(ArenaDefenseTeam opponent)
        {
            _selectedOpponent = opponent;
            StartBattle(opponent);
        }

        void StartBattle(ArenaDefenseTeam opponent)
        {
            ShowPanel(_battlePanel);

            if (_playerTeamLabel != null)
                _playerTeamLabel.text = "Your Team";
            if (_opponentTeamLabel != null)
                _opponentTeamLabel.text = opponent.DisplayName;
            if (_playerHpBar != null)
                _playerHpBar.value = 1f;
            if (_opponentHpBar != null)
                _opponentHpBar.value = 1f;
            if (_battleLogText != null)
                _battleLogText.text = "Battle starting...";

            var arena = ArenaManager.Instance;
            arena?.Attack(opponent, (success, result) =>
            {
                if (success && result != null)
                {
                    ShowBattleResult(result);
                }
                else
                {
                    ShowPanel(_mainPanel);
                }
            });
        }

        void ShowBattleResult(ArenaBattleResult result)
        {
            // Update battle panel HP bars
            if (_playerHpBar != null)
                _playerHpBar.value = result.PlayerHpRemaining;
            if (_opponentHpBar != null)
                _opponentHpBar.value = result.OpponentHpRemaining;

            // Show battle log
            if (_battleLogText != null && result.BattleLog.Count > 0)
            {
                // Show last few log entries
                int start = Mathf.Max(0, result.BattleLog.Count - 8);
                string log = "";
                for (int i = start; i < result.BattleLog.Count; i++)
                    log += result.BattleLog[i] + "\n";
                _battleLogText.text = log;
            }

            // Show result panel
            ShowPanel(_resultPanel);

            var arena = ArenaManager.Instance;

            if (_resultTitle != null)
                _resultTitle.text = result.PlayerWon ? "VICTORY!" : "DEFEAT";

            if (_eloChangeText != null)
            {
                int change = result.PlayerWon ? 25 : -20;
                string sign = change > 0 ? "+" : "";
                _eloChangeText.text = $"ELO: {sign}{change}";
            }

            if (_newRankText != null && arena != null)
                _newRankText.text = $"Rank: {arena.CurrentRank} ({arena.Elo})";
        }

        void OnRefreshAttempts()
        {
            var arena = ArenaManager.Instance;
            if (arena != null && arena.RefreshAttempts())
                RefreshMainPanel();
        }

        void OnAutoSetDefense()
        {
            ArenaManager.Instance?.AutoSetDefenseTeam();
            RefreshMainPanel();
            ShowPanel(_mainPanel);
        }

        void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        void OnDestroy()
        {
            if (_findOpponentButton != null) _findOpponentButton.onClick.RemoveAllListeners();
            if (_defenseSetupButton != null) _defenseSetupButton.onClick.RemoveAllListeners();
            if (_refreshAttemptsButton != null) _refreshAttemptsButton.onClick.RemoveAllListeners();
            if (_resultOkButton != null) _resultOkButton.onClick.RemoveAllListeners();
            if (_backFromOpponentsButton != null) _backFromOpponentsButton.onClick.RemoveAllListeners();
            if (_autoSetDefenseButton != null) _autoSetDefenseButton.onClick.RemoveAllListeners();
            if (_backFromDefenseButton != null) _backFromDefenseButton.onClick.RemoveAllListeners();
        }
    }
}
