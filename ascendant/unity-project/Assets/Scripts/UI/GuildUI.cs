using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Backend;

namespace Ascendant.UI
{
    public class GuildUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject _noGuildPanel;
        [SerializeField] GameObject _guildInfoPanel;
        [SerializeField] GameObject _createGuildPanel;
        [SerializeField] GameObject _searchGuildPanel;

        [Header("Guild Info")]
        [SerializeField] TextMeshProUGUI _guildNameText;
        [SerializeField] TextMeshProUGUI _guildLevelText;
        [SerializeField] TextMeshProUGUI _memberCountText;
        [SerializeField] TextMeshProUGUI _guildDescriptionText;

        [Header("Member List")]
        [SerializeField] Transform _memberListContainer;
        [SerializeField] GameObject _memberEntryPrefab;

        [Header("Tech Tree")]
        [SerializeField] Transform _techTreeContainer;
        [SerializeField] GameObject _techNodePrefab;

        [Header("Chat")]
        [SerializeField] Transform _chatContainer;
        [SerializeField] GameObject _chatMessagePrefab;
        [SerializeField] TMP_InputField _chatInput;
        [SerializeField] Button _sendChatButton;
        [SerializeField] ScrollRect _chatScroll;

        [Header("Create Guild")]
        [SerializeField] TMP_InputField _createNameInput;
        [SerializeField] TMP_InputField _createDescInput;
        [SerializeField] Button _createButton;
        [SerializeField] Button _showCreateButton;

        [Header("Search")]
        [SerializeField] TMP_InputField _searchInput;
        [SerializeField] Button _searchButton;
        [SerializeField] Transform _searchResultsContainer;
        [SerializeField] GameObject _searchResultPrefab;
        [SerializeField] Button _showSearchButton;

        [Header("Actions")]
        [SerializeField] Button _leaveGuildButton;
        [SerializeField] Button _backButton;

        void Start()
        {
            if (_sendChatButton != null)
                _sendChatButton.onClick.AddListener(OnSendChat);
            if (_createButton != null)
                _createButton.onClick.AddListener(OnCreateGuild);
            if (_searchButton != null)
                _searchButton.onClick.AddListener(OnSearchGuilds);
            if (_leaveGuildButton != null)
                _leaveGuildButton.onClick.AddListener(OnLeaveGuild);
            if (_showCreateButton != null)
                _showCreateButton.onClick.AddListener(() => ShowPanel(_createGuildPanel));
            if (_showSearchButton != null)
                _showSearchButton.onClick.AddListener(() => ShowPanel(_searchGuildPanel));
            if (_backButton != null)
                _backButton.onClick.AddListener(() => RefreshView());

            RefreshView();
        }

        void OnEnable()
        {
            EventBus.Subscribe<GuildJoinedEvent>(OnGuildJoined);
            EventBus.Subscribe<GuildLeftEvent>(OnGuildLeft);
            RefreshView();
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GuildJoinedEvent>(OnGuildJoined);
            EventBus.Unsubscribe<GuildLeftEvent>(OnGuildLeft);
        }

        void OnGuildJoined(GuildJoinedEvent evt) => RefreshView();
        void OnGuildLeft(GuildLeftEvent evt) => RefreshView();

        void RefreshView()
        {
            var manager = GuildManager.Instance;
            bool inGuild = manager != null && manager.IsInGuild;

            HideAllPanels();

            if (inGuild)
            {
                if (_guildInfoPanel != null) _guildInfoPanel.SetActive(true);
                PopulateGuildInfo(manager.CurrentGuild);
                PopulateMembers(manager.CurrentGuild?.Members);
                PopulateTechTree();
                PopulateChat(manager.ChatMessages);
            }
            else
            {
                if (_noGuildPanel != null) _noGuildPanel.SetActive(true);
            }
        }

        void ShowPanel(GameObject panel)
        {
            HideAllPanels();
            if (panel != null) panel.SetActive(true);
        }

        void HideAllPanels()
        {
            if (_noGuildPanel != null) _noGuildPanel.SetActive(false);
            if (_guildInfoPanel != null) _guildInfoPanel.SetActive(false);
            if (_createGuildPanel != null) _createGuildPanel.SetActive(false);
            if (_searchGuildPanel != null) _searchGuildPanel.SetActive(false);
        }

        void PopulateGuildInfo(GuildInfo info)
        {
            if (info == null) return;

            if (_guildNameText != null) _guildNameText.text = info.GuildName;
            if (_guildLevelText != null) _guildLevelText.text = $"Level {info.Level}";
            if (_memberCountText != null) _memberCountText.text = $"{info.Members.Count}/30 Members";
            if (_guildDescriptionText != null) _guildDescriptionText.text = info.Description;
        }

        void PopulateMembers(List<GuildMemberInfo> members)
        {
            if (_memberListContainer == null || _memberEntryPrefab == null || members == null) return;

            ClearContainer(_memberListContainer);

            foreach (var member in members)
            {
                var go = Instantiate(_memberEntryPrefab, _memberListContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = member.DisplayName;
                    texts[1].text = member.Role.ToString();
                }
            }
        }

        void PopulateTechTree()
        {
            if (_techTreeContainer == null || _techNodePrefab == null) return;

            ClearContainer(_techTreeContainer);

            var manager = GuildManager.Instance;
            if (manager == null) return;

            for (int i = 0; i < GuildManager.TechIds.Length; i++)
            {
                string techId = GuildManager.TechIds[i];
                string techName = GuildManager.TechNames[i];
                int level = manager.GetTechLevel(techId);
                float bonus = manager.GetTechBonusPercent(techId);

                var go = Instantiate(_techNodePrefab, _techTreeContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = techName;
                    texts[1].text = $"Lv.{level}/{GuildManager.MaxTechLevel}";
                    texts[2].text = $"+{bonus:F0}%";
                }

                // Contribute button
                var btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    string capturedTechId = techId;
                    btn.interactable = level < GuildManager.MaxTechLevel;
                    btn.onClick.AddListener(() => OnContribute(capturedTechId));
                }
            }
        }

        void PopulateChat(IReadOnlyList<GuildChatMessage> messages)
        {
            if (_chatContainer == null || _chatMessagePrefab == null || messages == null) return;

            ClearContainer(_chatContainer);

            foreach (var msg in messages)
            {
                var go = Instantiate(_chatMessagePrefab, _chatContainer);
                go.SetActive(true);

                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    var time = System.DateTimeOffset.FromUnixTimeSeconds(msg.TimestampUnix).LocalDateTime;
                    text.text = $"<b>{msg.SenderName}</b> [{time:HH:mm}]: {msg.Message}";
                }
            }

            // Scroll to bottom
            if (_chatScroll != null)
                Canvas.ForceUpdateCanvases();
        }

        void OnSendChat()
        {
            if (_chatInput == null || string.IsNullOrEmpty(_chatInput.text)) return;

            string message = _chatInput.text;
            _chatInput.text = "";

            GuildManager.Instance?.SendChat(message, success =>
            {
                if (success) RefreshView();
            });
        }

        void OnCreateGuild()
        {
            if (_createNameInput == null || string.IsNullOrEmpty(_createNameInput.text)) return;

            string name = _createNameInput.text;
            string desc = _createDescInput != null ? _createDescInput.text : "";

            GuildManager.Instance?.CreateGuild(name, desc, success =>
            {
                if (success) RefreshView();
            });
        }

        void OnSearchGuilds()
        {
            if (_searchInput == null || string.IsNullOrEmpty(_searchInput.text)) return;

            GuildManager.Instance?.SearchGuilds(_searchInput.text, results =>
            {
                PopulateSearchResults(results);
            });
        }

        void PopulateSearchResults(List<GuildInfo> results)
        {
            if (_searchResultsContainer == null || _searchResultPrefab == null) return;

            ClearContainer(_searchResultsContainer);

            foreach (var guild in results)
            {
                var go = Instantiate(_searchResultPrefab, _searchResultsContainer);
                go.SetActive(true);

                var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = guild.GuildName;
                    texts[1].text = $"{guild.Members.Count}/30";
                }

                var btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    string guildId = guild.GuildId;
                    btn.onClick.AddListener(() =>
                    {
                        GuildManager.Instance?.JoinGuild(guildId, success =>
                        {
                            if (success) RefreshView();
                        });
                    });
                }
            }
        }

        void OnLeaveGuild()
        {
            GuildManager.Instance?.LeaveGuild(success =>
            {
                if (success) RefreshView();
            });
        }

        void OnContribute(string techId)
        {
            int amount = 100; // Contribute 100 guild coins at a time
            GuildManager.Instance?.ContributeToTech(techId, amount, success =>
            {
                if (success) PopulateTechTree();
            });
        }

        void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        void OnDestroy()
        {
            if (_sendChatButton != null) _sendChatButton.onClick.RemoveAllListeners();
            if (_createButton != null) _createButton.onClick.RemoveAllListeners();
            if (_searchButton != null) _searchButton.onClick.RemoveAllListeners();
            if (_leaveGuildButton != null) _leaveGuildButton.onClick.RemoveAllListeners();
        }
    }
}
