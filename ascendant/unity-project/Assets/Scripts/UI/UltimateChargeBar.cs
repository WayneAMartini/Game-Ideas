using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;

namespace Ascendant.UI
{
    public class UltimateChargeBar : MonoBehaviour
    {
        [SerializeField] Slider _chargeSlider;
        [SerializeField] Image _fillImage;
        [SerializeField] int _heroSlot;

        [Header("Colors")]
        [SerializeField] Color _chargingColor = new Color(0.3f, 0.5f, 1f);
        [SerializeField] Color _readyColor = new Color(1f, 0.85f, 0f);

        float _currentCharge;

        void OnEnable()
        {
            EventBus.Subscribe<UltimateChargeChangedEvent>(OnChargeChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<UltimateChargeChangedEvent>(OnChargeChanged);
        }

        void OnChargeChanged(UltimateChargeChangedEvent evt)
        {
            if (evt.HeroSlot != _heroSlot) return;

            _currentCharge = evt.ChargePercent;

            if (_chargeSlider != null)
                _chargeSlider.value = _currentCharge;

            if (_fillImage != null)
                _fillImage.color = _currentCharge >= 1f ? _readyColor : _chargingColor;
        }
    }
}
