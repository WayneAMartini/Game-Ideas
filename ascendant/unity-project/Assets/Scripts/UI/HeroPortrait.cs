using UnityEngine;
using UnityEngine.UI;
using Ascendant.Core;
using Ascendant.Heroes;

namespace Ascendant.UI
{
    public class HeroPortrait : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] Image _portraitImage;
        [SerializeField] Image _portraitBorder;
        [SerializeField] Slider _hpBar;
        [SerializeField] Image _hpFill;

        [Header("Config")]
        [SerializeField] int _heroSlot;

        Hero _hero;

        void OnEnable()
        {
            EventBus.Subscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Subscribe<HeroHealedEvent>(OnHeroHealed);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<HeroDamagedEvent>(OnHeroDamaged);
            EventBus.Unsubscribe<HeroHealedEvent>(OnHeroHealed);
        }

        public void Initialize(Hero hero)
        {
            _hero = hero;
            _heroSlot = hero.Slot;

            if (hero.Data.portrait != null && _portraitImage != null)
                _portraitImage.sprite = hero.Data.portrait;

            // Set border color based on affinity
            if (_portraitBorder != null)
                _portraitBorder.color = GetAffinityColor(hero.Data.affinity);

            UpdateHpBar();
        }

        void UpdateHpBar()
        {
            if (_hero == null || _hpBar == null) return;

            float pct = _hero.CurrentHp / _hero.MaxHp;
            _hpBar.value = pct;

            if (_hpFill != null)
            {
                if (pct > 0.5f)
                    _hpFill.color = Color.Lerp(Color.yellow, Color.green, (pct - 0.5f) * 2f);
                else
                    _hpFill.color = Color.Lerp(Color.red, Color.yellow, pct * 2f);
            }
        }

        void OnHeroDamaged(HeroDamagedEvent evt)
        {
            if (evt.HeroSlot == _heroSlot) UpdateHpBar();
        }

        void OnHeroHealed(HeroHealedEvent evt)
        {
            if (evt.HeroSlot == _heroSlot) UpdateHpBar();
        }

        static Color GetAffinityColor(Combat.Affinity affinity)
        {
            return affinity switch
            {
                Combat.Affinity.Flame => new Color(1f, 0.4f, 0.1f),
                Combat.Affinity.Frost => new Color(0.3f, 0.7f, 1f),
                Combat.Affinity.Storm => new Color(0.6f, 0.4f, 1f),
                Combat.Affinity.Nature => new Color(0.2f, 0.8f, 0.3f),
                Combat.Affinity.Shadow => new Color(0.5f, 0.2f, 0.7f),
                Combat.Affinity.Radiance => new Color(1f, 0.9f, 0.4f),
                _ => Color.gray
            };
        }
    }
}
