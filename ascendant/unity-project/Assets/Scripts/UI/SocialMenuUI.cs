using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class SocialMenuUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] Button _guildButton;
        [SerializeField] Button _arenaButton;
        [SerializeField] Button _worldBossButton;
        [SerializeField] Button _leaderboardButton;

        [Header("Panels")]
        [SerializeField] GuildUI _guildPanel;
        [SerializeField] ArenaUI _arenaPanel;
        [SerializeField] WorldBossUI _worldBossPanel;
        [SerializeField] LeaderboardUI _leaderboardPanel;

        [Header("Notification Badges")]
        [SerializeField] GameObject _worldBossBadge;
        [SerializeField] GameObject _guildBadge;

        void Start()
        {
            if (_guildButton != null)
                _guildButton.onClick.AddListener(() => ShowPanel("guild"));
            if (_arenaButton != null)
                _arenaButton.onClick.AddListener(() => ShowPanel("arena"));
            if (_worldBossButton != null)
                _worldBossButton.onClick.AddListener(() => ShowPanel("worldboss"));
            if (_leaderboardButton != null)
                _leaderboardButton.onClick.AddListener(() => ShowPanel("leaderboard"));

            // Default: hide all
            HideAll();
        }

        void OnEnable()
        {
            EventBus.Subscribe<WorldBossEventStartedEvent>(OnWorldBossStarted);
            UpdateBadges();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<WorldBossEventStartedEvent>(OnWorldBossStarted);
        }

        void OnWorldBossStarted(WorldBossEventStartedEvent evt)
        {
            UpdateBadges();
        }

        void ShowPanel(string panelId)
        {
            HideAll();

            switch (panelId)
            {
                case "guild":
                    if (_guildPanel != null) _guildPanel.gameObject.SetActive(true);
                    break;
                case "arena":
                    if (_arenaPanel != null) _arenaPanel.gameObject.SetActive(true);
                    break;
                case "worldboss":
                    if (_worldBossPanel != null) _worldBossPanel.gameObject.SetActive(true);
                    if (_worldBossBadge != null) _worldBossBadge.SetActive(false);
                    break;
                case "leaderboard":
                    if (_leaderboardPanel != null) _leaderboardPanel.gameObject.SetActive(true);
                    break;
            }
        }

        void HideAll()
        {
            if (_guildPanel != null) _guildPanel.gameObject.SetActive(false);
            if (_arenaPanel != null) _arenaPanel.gameObject.SetActive(false);
            if (_worldBossPanel != null) _worldBossPanel.gameObject.SetActive(false);
            if (_leaderboardPanel != null) _leaderboardPanel.gameObject.SetActive(false);
        }

        void UpdateBadges()
        {
            // Show world boss badge when event is active
            if (_worldBossBadge != null)
            {
                var worldBoss = WorldBossManager.Instance;
                _worldBossBadge.SetActive(worldBoss != null && worldBoss.IsEventActive);
            }

            // Show guild badge when in guild with pending expedition
            if (_guildBadge != null)
            {
                var guild = GuildManager.Instance;
                var expedition = GuildExpedition.Instance;
                _guildBadge.SetActive(
                    guild != null && guild.IsInGuild &&
                    expedition != null && expedition.CanClearNode);
            }
        }

        void OnDestroy()
        {
            if (_guildButton != null) _guildButton.onClick.RemoveAllListeners();
            if (_arenaButton != null) _arenaButton.onClick.RemoveAllListeners();
            if (_worldBossButton != null) _worldBossButton.onClick.RemoveAllListeners();
            if (_leaderboardButton != null) _leaderboardButton.onClick.RemoveAllListeners();
        }
    }
}
