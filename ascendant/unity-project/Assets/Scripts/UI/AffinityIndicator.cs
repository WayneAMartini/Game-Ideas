using UnityEngine;
using UnityEngine.UI;
using Ascendant.Combat;

namespace Ascendant.UI
{
    public class AffinityIndicator : MonoBehaviour
    {
        [SerializeField] Image _borderImage;
        [SerializeField] Image _iconImage;

        public void SetAffinity(Affinity affinity)
        {
            Color affinityColor = GetAffinityColor(affinity);

            if (_borderImage != null)
                _borderImage.color = affinityColor;

            if (_iconImage != null)
                _iconImage.color = affinityColor;
        }

        static Color GetAffinityColor(Affinity affinity)
        {
            return affinity switch
            {
                Affinity.Flame => new Color(1f, 0.4f, 0.1f),
                Affinity.Frost => new Color(0.3f, 0.7f, 1f),
                Affinity.Storm => new Color(0.6f, 0.4f, 1f),
                Affinity.Nature => new Color(0.2f, 0.8f, 0.3f),
                Affinity.Shadow => new Color(0.5f, 0.2f, 0.7f),
                Affinity.Radiance => new Color(1f, 0.9f, 0.4f),
                _ => Color.gray
            };
        }
    }
}
