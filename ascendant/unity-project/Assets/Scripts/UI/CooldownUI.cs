using UnityEngine;
using UnityEngine.UI;
using Ascendant.Combat;

namespace Ascendant.UI
{
    public class CooldownUI : MonoBehaviour
    {
        [SerializeField] Image _cooldownOverlay; // radial fill overlay on portrait
        [SerializeField] AbilityController _abilityController;
        [SerializeField] int _slotIndex;

        void Update()
        {
            if (_abilityController == null) return;

            var slot = _abilityController.GetSlot(_slotIndex);
            if (slot == null) return;

            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.fillAmount = slot.CooldownPercent;
                _cooldownOverlay.gameObject.SetActive(slot.CooldownPercent > 0f);
            }
        }
    }
}
