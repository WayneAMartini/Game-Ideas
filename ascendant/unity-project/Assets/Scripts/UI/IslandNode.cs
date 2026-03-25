using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ascendant.Islands;

namespace Ascendant.UI
{
    public class IslandNode : MonoBehaviour
    {
        [SerializeField] Image _islandImage;
        [SerializeField] Image _checkmarkOverlay;
        [SerializeField] Image _lockOverlay;
        [SerializeField] Image _beaconPulse;
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] Button _button;

        IslandData _data;
        int _index;
        System.Action<int> _onTapped;
        bool _isCurrent;

        public void Initialize(IslandData data, int index, System.Action<int> onTapped)
        {
            _data = data;
            _index = index;
            _onTapped = onTapped;

            if (_nameText != null)
                _nameText.text = data.islandName;

            if (_button != null)
                _button.onClick.AddListener(() => _onTapped?.Invoke(_index));

            // Set island tint based on affinity
            if (_islandImage != null)
                _islandImage.color = GetAffinityColor(data.affinity);
        }

        public void SetState(bool completed, bool unlocked, bool isCurrent)
        {
            _isCurrent = isCurrent;

            if (_checkmarkOverlay != null)
                _checkmarkOverlay.gameObject.SetActive(completed);

            if (_lockOverlay != null)
                _lockOverlay.gameObject.SetActive(!unlocked);

            if (_beaconPulse != null)
                _beaconPulse.gameObject.SetActive(isCurrent);

            if (_islandImage != null)
            {
                var c = _islandImage.color;
                c.a = unlocked ? 1f : 0.4f;
                _islandImage.color = c;
            }

            if (_button != null)
                _button.interactable = unlocked;
        }

        void Update()
        {
            // Pulse effect for current island beacon
            if (_isCurrent && _beaconPulse != null)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f);
                var c = _beaconPulse.color;
                c.a = alpha;
                _beaconPulse.color = c;
            }
        }

        static Color GetAffinityColor(Combat.Affinity affinity)
        {
            return affinity switch
            {
                Combat.Affinity.Flame => new Color(1f, 0.4f, 0.2f),
                Combat.Affinity.Frost => new Color(0.4f, 0.7f, 1f),
                Combat.Affinity.Storm => new Color(0.6f, 0.4f, 1f),
                Combat.Affinity.Nature => new Color(0.3f, 0.8f, 0.3f),
                Combat.Affinity.Shadow => new Color(0.4f, 0.2f, 0.6f),
                Combat.Affinity.Radiance => new Color(1f, 0.9f, 0.4f),
                _ => Color.white
            };
        }
    }
}
