using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Core;
using Ascendant.Progression;

namespace Ascendant.UI
{
    public class MasteryUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject _masteryPanel;

        [Header("Current Tier")]
        [SerializeField] TextMeshProUGUI _classNameText;
        [SerializeField] TextMeshProUGUI _currentTierText;
        [SerializeField] Slider _progressBar;
        [SerializeField] TextMeshProUGUI _progressText;

        [Header("Stats")]
        [SerializeField] TextMeshProUGUI _totalStagesText;
        [SerializeField] TextMeshProUGUI _totalAscensionsText;
        [SerializeField] TextMeshProUGUI _totalDamageText;
        [SerializeField] TextMeshProUGUI _totalKillsText;

        [Header("Tier Rewards List")]
        [SerializeField] Transform _rewardsContent;
        [SerializeField] GameObject _rewardEntryPrefab;

        string _selectedClassId;

        void OnEnable()
        {
            EventBus.Subscribe<ClassMasteryTierUpEvent>(OnTierUp);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ClassMasteryTierUpEvent>(OnTierUp);
        }

        void OnTierUp(ClassMasteryTierUpEvent evt)
        {
            if (evt.ClassId == _selectedClassId)
                Refresh();
        }

        public void Open(string classId)
        {
            _selectedClassId = classId;
            if (_masteryPanel != null) _masteryPanel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_masteryPanel != null) _masteryPanel.SetActive(false);
        }

        void Refresh()
        {
            var system = ClassMasterySystem.Instance;
            if (system == null) return;

            var data = system.GetMasteryData(_selectedClassId);
            var progress = system.GetProgress(_selectedClassId);

            if (_classNameText != null)
                _classNameText.text = data != null ? data.className : _selectedClassId;

            if (progress != null)
            {
                if (_currentTierText != null)
                    _currentTierText.text = progress.currentTier.ToString();

                int stages = progress.totalStagesCleared;
                int nextThreshold = data != null ? data.GetNextTierThreshold(stages) : 100;

                // Find current tier's threshold for progress bar
                int currentThreshold = 0;
                if (data?.tierRewards != null)
                {
                    for (int i = 0; i < data.tierRewards.Length; i++)
                        if (stages >= data.tierRewards[i].stageThreshold)
                            currentThreshold = data.tierRewards[i].stageThreshold;
                }

                float progressValue = 0f;
                if (nextThreshold > currentThreshold)
                    progressValue = (float)(stages - currentThreshold) / (nextThreshold - currentThreshold);

                if (_progressBar != null) _progressBar.value = Mathf.Clamp01(progressValue);
                if (_progressText != null) _progressText.text = $"{stages} / {nextThreshold} stages";

                if (_totalStagesText != null) _totalStagesText.text = $"Stages: {stages:N0}";
                if (_totalAscensionsText != null) _totalAscensionsText.text = $"Ascensions: {progress.totalAscensions}";
                if (_totalDamageText != null) _totalDamageText.text = $"Damage: {progress.totalDamageDealt:N0}";
                if (_totalKillsText != null) _totalKillsText.text = $"Kills: {progress.totalEnemiesKilled:N0}";
            }
            else
            {
                if (_currentTierText != null) _currentTierText.text = "Novice";
                if (_progressBar != null) _progressBar.value = 0f;
                if (_progressText != null) _progressText.text = "0 / 100 stages";
            }

            RefreshRewardsList(data, progress);
        }

        void RefreshRewardsList(ClassMasteryData data, ClassMasteryProgress progress)
        {
            if (_rewardsContent == null || data?.tierRewards == null) return;

            // Clear existing
            for (int i = _rewardsContent.childCount - 1; i >= 0; i--)
                Destroy(_rewardsContent.GetChild(i).gameObject);

            int currentStages = progress?.totalStagesCleared ?? 0;

            for (int i = 0; i < data.tierRewards.Length; i++)
            {
                var reward = data.tierRewards[i];
                if (_rewardEntryPrefab == null) continue;

                var go = Instantiate(_rewardEntryPrefab, _rewardsContent);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    bool earned = currentStages >= reward.stageThreshold;
                    string status = earned ? "<color=green>[Earned]</color>" : "<color=grey>[Locked]</color>";
                    text.text = $"{status} {reward.tier}: {reward.rewardDescription} ({reward.stageThreshold} stages)";
                }
            }
        }
    }
}
