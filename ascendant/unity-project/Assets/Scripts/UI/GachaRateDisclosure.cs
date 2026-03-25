using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ascendant.UI
{
    public class GachaRateDisclosure : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject _disclosurePanel;
        [SerializeField] TextMeshProUGUI _ratesText;
        [SerializeField] Button _closeButton;

        void Start()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            if (_disclosurePanel != null)
                _disclosurePanel.SetActive(false);
        }

        public void Show()
        {
            if (_disclosurePanel != null)
                _disclosurePanel.SetActive(true);

            if (_ratesText != null)
                _ratesText.text = GetRatesText();
        }

        public void Hide()
        {
            if (_disclosurePanel != null)
                _disclosurePanel.SetActive(false);
        }

        static string GetRatesText()
        {
            return "SUMMON RATES\n\n" +
                   "Standard Banner:\n" +
                   "  Common:    60%\n" +
                   "  Uncommon:  25%\n" +
                   "  Rare:      10%\n" +
                   "  Epic:       4%\n" +
                   "  Legendary:  1%\n\n" +
                   "PITY SYSTEM:\n" +
                   "  Epic guaranteed within 30 pulls\n" +
                   "  Legendary guaranteed within 90 pulls\n\n" +
                   "SPARK SYSTEM:\n" +
                   "  At 200 total pulls on a banner,\n" +
                   "  choose any featured hero.\n\n" +
                   "Rate-up Banner:\n" +
                   "  Featured hero rate: 50% of\n" +
                   "  rarity pool when that rarity\n" +
                   "  is rolled.\n\n" +
                   "Duplicate heroes award Star Fragments\n" +
                   "that can be used in the Star Fragment Shop.";
        }
    }
}
