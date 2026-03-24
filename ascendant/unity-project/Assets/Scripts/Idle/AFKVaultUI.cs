using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;

namespace Ascendant.Idle
{
    public class AFKVaultUI : MonoBehaviour
    {
        [Header("Vault Icon (top-right of combat screen)")]
        [SerializeField] GameObject _vaultIconRoot;
        [SerializeField] Image _vaultIcon;
        [SerializeField] float _pulseSpeed = 2f;
        [SerializeField] float _pulseMinScale = 0.9f;
        [SerializeField] float _pulseMaxScale = 1.1f;

        [Header("Reward Popup")]
        [SerializeField] GameObject _rewardPanel;
        [SerializeField] TMPro.TextMeshProUGUI _goldText;
        [SerializeField] TMPro.TextMeshProUGUI _xpText;
        [SerializeField] TMPro.TextMeshProUGUI _materialsText;
        [SerializeField] TMPro.TextMeshProUGUI _equipmentText;
        [SerializeField] TMPro.TextMeshProUGUI _offlineTimeText;
        [SerializeField] TMPro.TextMeshProUGUI _bonusText;
        [SerializeField] Button _collectButton;

        bool _isPulsing;

        void OnEnable()
        {
            EventBus.Subscribe<AFKVaultReadyEvent>(OnVaultReady);
            EventBus.Subscribe<AFKVaultCollectedEvent>(OnVaultCollected);

            if (_collectButton != null)
                _collectButton.onClick.AddListener(OnCollectPressed);

            if (_rewardPanel != null)
                _rewardPanel.SetActive(false);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<AFKVaultReadyEvent>(OnVaultReady);
            EventBus.Unsubscribe<AFKVaultCollectedEvent>(OnVaultCollected);

            if (_collectButton != null)
                _collectButton.onClick.RemoveListener(OnCollectPressed);
        }

        void Start()
        {
            // Check if vault already has rewards on start
            if (AFKVaultSystem.Instance != null && AFKVaultSystem.Instance.HasPendingRewards)
            {
                ShowVaultReady(AFKVaultSystem.Instance.PendingRewards);
            }
            else
            {
                SetVaultIconVisible(false);
            }
        }

        void Update()
        {
            if (_isPulsing && _vaultIconRoot != null)
            {
                float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                float scale = Mathf.Lerp(_pulseMinScale, _pulseMaxScale, t);
                _vaultIconRoot.transform.localScale = Vector3.one * scale;
            }
        }

        void OnVaultReady(AFKVaultReadyEvent evt)
        {
            ShowVaultReady(evt.Rewards);
        }

        void ShowVaultReady(AFKRewards rewards)
        {
            SetVaultIconVisible(true);
            _isPulsing = true;

            // Auto-show the reward panel on app launch (vault is first thing visible)
            ShowRewardPanel(rewards);
        }

        void OnVaultCollected(AFKVaultCollectedEvent evt)
        {
            SetVaultIconVisible(false);
            _isPulsing = false;

            if (_rewardPanel != null)
                _rewardPanel.SetActive(false);
        }

        void OnCollectPressed()
        {
            AFKVaultSystem.Instance?.CollectRewards();
        }

        void ShowRewardPanel(AFKRewards rewards)
        {
            if (_rewardPanel != null)
                _rewardPanel.SetActive(true);

            if (_goldText != null)
                _goldText.text = FormatNumber(rewards.Gold) + " Gold";

            if (_xpText != null)
                _xpText.text = FormatNumber(rewards.Xp) + " XP";

            if (_materialsText != null)
                _materialsText.text = rewards.MaterialDrops + " Materials";

            if (_equipmentText != null)
            {
                int count = rewards.EquipmentDrops != null ? rewards.EquipmentDrops.Count : 0;
                _equipmentText.text = count > 0 ? count + " Equipment" : "";
                _equipmentText.gameObject.SetActive(count > 0);
            }

            if (_offlineTimeText != null)
            {
                int hours = Mathf.FloorToInt((float)rewards.OfflineHours);
                int minutes = Mathf.FloorToInt((float)((rewards.OfflineHours - hours) * 60));
                _offlineTimeText.text = $"Your heroes fought for {hours}h {minutes}m";
            }

            if (_bonusText != null)
            {
                _bonusText.gameObject.SetActive(rewards.ReturnBonus);
                if (rewards.ReturnBonus)
                    _bonusText.text = "Return Bonus: +25%!";
            }
        }

        void SetVaultIconVisible(bool visible)
        {
            if (_vaultIconRoot != null)
                _vaultIconRoot.SetActive(visible);
        }

        static string FormatNumber(double value)
        {
            if (value >= 1_000_000) return (value / 1_000_000).ToString("F1") + "M";
            if (value >= 1_000) return (value / 1_000).ToString("F1") + "K";
            return value.ToString("F0");
        }
    }
}
