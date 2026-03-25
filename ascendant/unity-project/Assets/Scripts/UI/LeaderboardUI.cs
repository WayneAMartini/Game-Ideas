using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] Button[] _tabButtons;
        [SerializeField] Color _activeTabColor = new(1f, 0.84f, 0f);
        [SerializeField] Color _inactiveTabColor = new(0.5f, 0.5f, 0.5f);

        [Header("View Toggle")]
        [SerializeField] Button _weeklyButton;
        [SerializeField] Button _allTimeButton;
        [SerializeField] TextMeshProUGUI _viewLabel;

        [Header("Entries")]
        [SerializeField] Transform _entryContainer;
        [SerializeField] GameObject _entryPrefab;

        [Header("Player Info")]
        [SerializeField] TextMeshProUGUI _playerRankText;
        [SerializeField] TextMeshProUGUI _playerPercentileText;

        [Header("Loading")]
        [SerializeField] GameObject _loadingIndicator;
        [SerializeField] TextMeshProUGUI _emptyText;

        int _currentTabIndex;
        bool _isWeekly;
        const int MaxEntries = 100;

        void Start()
        {
            SetupTabs();

            if (_weeklyButton != null)
                _weeklyButton.onClick.AddListener(() => SetView(true));
            if (_allTimeButton != null)
                _allTimeButton.onClick.AddListener(() => SetView(false));

            // Default to first tab, all-time
            SelectTab(0);
        }

        void SetupTabs()
        {
            for (int i = 0; i < _tabButtons.Length && i < LeaderboardManager.AllLeaderboards.Length; i++)
            {
                int tabIndex = i;
                if (_tabButtons[i] != null)
                {
                    var label = _tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null && i < LeaderboardManager.LeaderboardDisplayNames.Length)
                        label.text = LeaderboardManager.LeaderboardDisplayNames[i];

                    _tabButtons[i].onClick.AddListener(() => SelectTab(tabIndex));
                }
            }
        }

        void SelectTab(int index)
        {
            _currentTabIndex = index;

            // Update tab visuals
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] != null)
                {
                    var img = _tabButtons[i].GetComponent<Image>();
                    if (img != null)
                        img.color = i == index ? _activeTabColor : _inactiveTabColor;
                }
            }

            RefreshLeaderboard();
        }

        void SetView(bool weekly)
        {
            _isWeekly = weekly;
            if (_viewLabel != null)
                _viewLabel.text = weekly ? "Weekly" : "All Time";
            RefreshLeaderboard();
        }

        void RefreshLeaderboard()
        {
            if (_currentTabIndex >= LeaderboardManager.AllLeaderboards.Length) return;

            string leaderboardId = LeaderboardManager.AllLeaderboards[_currentTabIndex];

            ClearEntries();
            if (_loadingIndicator != null) _loadingIndicator.SetActive(true);
            if (_emptyText != null) _emptyText.gameObject.SetActive(false);

            var manager = LeaderboardManager.Instance;
            if (manager == null)
            {
                ShowEmpty();
                return;
            }

            manager.GetLeaderboard(leaderboardId, _isWeekly, MaxEntries, entries =>
            {
                if (_loadingIndicator != null) _loadingIndicator.SetActive(false);

                if (entries == null || entries.Count == 0)
                {
                    ShowEmpty();
                    return;
                }

                PopulateEntries(entries);
            });

            // Get player rank
            manager.GetPlayerRank(leaderboardId, _isWeekly, (rank, percentile) =>
            {
                if (_playerRankText != null)
                    _playerRankText.text = rank > 0 ? $"#{rank}" : "Unranked";
                if (_playerPercentileText != null)
                    _playerPercentileText.text = rank > 0 ? $"Top {100f - percentile:F0}%" : "";
            });
        }

        void PopulateEntries(List<LeaderboardEntry> entries)
        {
            ClearEntries();

            string currentUserId = AuthManager.Instance?.UserId ?? "";

            foreach (var entry in entries)
            {
                if (_entryPrefab == null || _entryContainer == null) break;

                var go = Instantiate(_entryPrefab, _entryContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{entry.Rank}";
                    texts[1].text = entry.DisplayName;
                    texts[2].text = FormatScore(entry.Score);
                }

                // Highlight current player
                if (entry.UserId == currentUserId)
                {
                    var bg = go.GetComponent<Image>();
                    if (bg != null) bg.color = new Color(1f, 0.84f, 0f, 0.2f);
                }
            }
        }

        string FormatScore(long score)
        {
            string leaderboardId = LeaderboardManager.AllLeaderboards[_currentTabIndex];

            return leaderboardId switch
            {
                LeaderboardManager.LB_HIGHEST_ISLAND =>
                    $"Island {score / 100} - Stage {score % 100}",
                LeaderboardManager.LB_BOSS_DAMAGE =>
                    FormatLargeNumber(score),
                _ => score.ToString("N0")
            };
        }

        static string FormatLargeNumber(long value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000.0:F1}B";
            if (value >= 1_000_000) return $"{value / 1_000_000.0:F1}M";
            if (value >= 1_000) return $"{value / 1_000.0:F1}K";
            return value.ToString("N0");
        }

        void ClearEntries()
        {
            if (_entryContainer == null) return;
            for (int i = _entryContainer.childCount - 1; i >= 0; i--)
                Destroy(_entryContainer.GetChild(i).gameObject);
        }

        void ShowEmpty()
        {
            if (_loadingIndicator != null) _loadingIndicator.SetActive(false);
            if (_emptyText != null)
            {
                _emptyText.text = "No entries yet";
                _emptyText.gameObject.SetActive(true);
            }
        }

        void OnDestroy()
        {
            // Cleanup listeners
            if (_weeklyButton != null) _weeklyButton.onClick.RemoveAllListeners();
            if (_allTimeButton != null) _allTimeButton.onClick.RemoveAllListeners();
            foreach (var btn in _tabButtons)
                if (btn != null) btn.onClick.RemoveAllListeners();
        }
    }
}
