using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class WorldBossUI : MonoBehaviour
    {
        [Header("Boss Info")]
        [SerializeField] TextMeshProUGUI _bossNameText;
        [SerializeField] Slider _globalHpBar;
        [SerializeField] TextMeshProUGUI _globalHpText;
        [SerializeField] TextMeshProUGUI _eventTimerText;

        [Header("Player Info")]
        [SerializeField] TextMeshProUGUI _personalBestText;
        [SerializeField] TextMeshProUGUI _attemptsText;
        [SerializeField] TextMeshProUGUI _totalDamageText;

        [Header("Fight")]
        [SerializeField] Button _fightButton;
        [SerializeField] GameObject _fightPanel;
        [SerializeField] Slider _fightTimerBar;
        [SerializeField] TextMeshProUGUI _fightTimerText;
        [SerializeField] TextMeshProUGUI _fightDamageText;

        [Header("Leaderboard")]
        [SerializeField] Transform _leaderboardContainer;
        [SerializeField] GameObject _leaderboardEntryPrefab;
        [SerializeField] Button _refreshLeaderboardButton;

        [Header("Rewards")]
        [SerializeField] TextMeshProUGUI _rewardTierText;

        [Header("Inactive State")]
        [SerializeField] GameObject _activePanel;
        [SerializeField] GameObject _inactivePanel;
        [SerializeField] TextMeshProUGUI _inactiveText;

        void Start()
        {
            if (_fightButton != null)
                _fightButton.onClick.AddListener(OnFightButton);
            if (_refreshLeaderboardButton != null)
                _refreshLeaderboardButton.onClick.AddListener(RefreshLeaderboard);

            RefreshView();
        }

        void OnEnable()
        {
            EventBus.Subscribe<WorldBossDamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<WorldBossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<WorldBossEventStartedEvent>(OnEventStarted);
            RefreshView();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<WorldBossDamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<WorldBossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<WorldBossEventStartedEvent>(OnEventStarted);
        }

        void Update()
        {
            var manager = WorldBossManager.Instance;
            if (manager == null) return;

            // Update fight timer during fight
            if (manager.IsFighting)
            {
                float remaining = manager.FightTimeRemaining;
                float total = manager.FightDuration;

                if (_fightTimerBar != null)
                    _fightTimerBar.value = remaining / total;
                if (_fightTimerText != null)
                    _fightTimerText.text = $"{remaining:F1}s";
                if (_fightDamageText != null)
                    _fightDamageText.text = WorldBossManager.FormatDamage(manager.CurrentFightDamage);
            }

            // Update event timer
            if (manager.CurrentBoss != null && manager.IsEventActive)
            {
                long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long remaining = manager.CurrentBoss.EventEndUnix - now;
                if (remaining > 0 && _eventTimerText != null)
                {
                    int hours = (int)(remaining / 3600);
                    int minutes = (int)((remaining % 3600) / 60);
                    _eventTimerText.text = $"Ends in {hours}h {minutes}m";
                }
            }
        }

        void OnDamageDealt(WorldBossDamageDealtEvent evt)
        {
            UpdateBossHp();
        }

        void OnBossDefeated(WorldBossDefeatedEvent evt)
        {
            RefreshView();
        }

        void OnEventStarted(WorldBossEventStartedEvent evt)
        {
            RefreshView();
        }

        void RefreshView()
        {
            var manager = WorldBossManager.Instance;
            if (manager == null) return;

            manager.RefreshBossState(() =>
            {
                bool active = manager.IsEventActive;

                if (_activePanel != null) _activePanel.SetActive(active);
                if (_inactivePanel != null) _inactivePanel.SetActive(!active);

                if (!active)
                {
                    if (_inactiveText != null)
                        _inactiveText.text = "No World Boss event active.\nCheck back soon!";
                    return;
                }

                var boss = manager.CurrentBoss;
                if (boss == null) return;

                if (_bossNameText != null) _bossNameText.text = boss.BossName;
                UpdateBossHp();

                if (_personalBestText != null)
                    _personalBestText.text = $"Personal Best: {WorldBossManager.FormatDamage(manager.PersonalBestDamage)}";
                if (_attemptsText != null)
                    _attemptsText.text = $"Attempts: {manager.RemainingAttempts}/{manager.AttemptsPerDay}";
                if (_totalDamageText != null)
                    _totalDamageText.text = $"Total: {WorldBossManager.FormatDamage(manager.TotalDamageDealt)}";

                if (_fightButton != null)
                    _fightButton.interactable = manager.CanAttempt;

                bool fighting = manager.IsFighting;
                if (_fightPanel != null) _fightPanel.SetActive(fighting);

                RefreshLeaderboard();
            });
        }

        void UpdateBossHp()
        {
            var manager = WorldBossManager.Instance;
            if (manager?.CurrentBoss == null) return;

            if (_globalHpBar != null)
                _globalHpBar.value = manager.BossHpPercent;

            if (_globalHpText != null)
            {
                string current = WorldBossManager.FormatDamage(manager.CurrentBoss.CurrentHp);
                string max = WorldBossManager.FormatDamage(manager.CurrentBoss.MaxHp);
                _globalHpText.text = $"{current} / {max}";
            }
        }

        void OnFightButton()
        {
            var manager = WorldBossManager.Instance;
            if (manager == null || !manager.CanAttempt) return;

            if (manager.StartFight())
            {
                if (_fightPanel != null) _fightPanel.SetActive(true);
                if (_fightButton != null) _fightButton.interactable = false;

                if (_attemptsText != null)
                    _attemptsText.text = $"Attempts: {manager.RemainingAttempts}/{manager.AttemptsPerDay}";
            }
        }

        void RefreshLeaderboard()
        {
            var manager = WorldBossManager.Instance;
            if (manager == null) return;

            manager.GetDamageLeaderboard(entries =>
            {
                PopulateLeaderboard(entries);
            });
        }

        void PopulateLeaderboard(List<LeaderboardEntry> entries)
        {
            if (_leaderboardContainer == null || _leaderboardEntryPrefab == null) return;

            ClearContainer(_leaderboardContainer);

            if (entries == null) return;

            string currentUserId = AuthManager.Instance?.UserId ?? "";

            foreach (var entry in entries)
            {
                var go = Instantiate(_leaderboardEntryPrefab, _leaderboardContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{entry.Rank}";
                    texts[1].text = entry.DisplayName;
                    texts[2].text = WorldBossManager.FormatDamage(entry.Score);
                }

                if (entry.UserId == currentUserId)
                {
                    var bg = go.GetComponent<Image>();
                    if (bg != null) bg.color = new Color(1f, 0.84f, 0f, 0.2f);
                }
            }
        }

        void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        void OnDestroy()
        {
            if (_fightButton != null) _fightButton.onClick.RemoveAllListeners();
            if (_refreshLeaderboardButton != null) _refreshLeaderboardButton.onClick.RemoveAllListeners();
        }
    }
}
