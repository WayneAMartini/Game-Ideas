using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;

namespace Ascendant.UI
{
    public class ComboPointUI : MonoBehaviour
    {
        [Header("Combo Point Pips")]
        [SerializeField] Image[] _pips = new Image[5];

        [Header("Colors")]
        [SerializeField] Color _inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] Color _activeColor = new Color(0.8f, 0.2f, 1f); // purple
        [SerializeField] Color _fullColor = new Color(1f, 0.85f, 0f); // gold when full

        int _currentComboPoints;
        int _maxComboPoints = 5;

        void OnEnable()
        {
            EventBus.Subscribe<ComboPointsChangedEvent>(OnComboPointsChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ComboPointsChangedEvent>(OnComboPointsChanged);
        }

        void OnComboPointsChanged(ComboPointsChangedEvent evt)
        {
            _currentComboPoints = evt.ComboPoints;
            _maxComboPoints = evt.MaxComboPoints;
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            bool isFull = _currentComboPoints >= _maxComboPoints;

            for (int i = 0; i < _pips.Length; i++)
            {
                if (_pips[i] == null) continue;

                if (i < _currentComboPoints)
                    _pips[i].color = isFull ? _fullColor : _activeColor;
                else
                    _pips[i].color = _inactiveColor;
            }
        }
    }
}
