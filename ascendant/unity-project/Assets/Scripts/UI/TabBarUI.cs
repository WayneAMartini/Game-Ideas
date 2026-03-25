using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ascendant.UI
{
    public enum TabScreen
    {
        Combat,
        Islands,
        Heroes,
        Social,
        More
    }

    public class TabBarUI : MonoBehaviour
    {
        public static TabBarUI Instance { get; private set; }

        [System.Serializable]
        public class TabEntry
        {
            public TabScreen screen;
            public Button button;
            public Image icon;
            public TextMeshProUGUI label;
            public GameObject screenRoot;
        }

        [Header("Tabs")]
        [SerializeField] TabEntry[] _tabs;

        [Header("Colors")]
        [SerializeField] Color _activeColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f);

        TabScreen _currentScreen = TabScreen.Combat;
        public TabScreen CurrentScreen => _currentScreen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Wire up button listeners
            if (_tabs != null)
            {
                foreach (var tab in _tabs)
                {
                    if (tab.button != null)
                    {
                        var screen = tab.screen;
                        tab.button.onClick.AddListener(() => SwitchTo(screen));
                    }
                }
            }

            SwitchTo(TabScreen.Combat);
        }

        public void SwitchTo(TabScreen screen)
        {
            _currentScreen = screen;

            if (_tabs == null) return;

            foreach (var tab in _tabs)
            {
                bool active = tab.screen == screen;

                if (tab.screenRoot != null)
                    tab.screenRoot.SetActive(active);

                if (tab.icon != null)
                    tab.icon.color = active ? _activeColor : _inactiveColor;

                if (tab.label != null)
                {
                    tab.label.color = active ? _activeColor : _inactiveColor;
                    tab.label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
                }
            }

            // Play UI click sound
            if (Audio.SoundManager.Instance != null)
                Audio.SoundManager.Instance.PlayUI(Audio.SoundId.UIClick);

            // Transition animation
            if (ScreenTransitionManager.Instance != null)
                ScreenTransitionManager.Instance.FadeIn();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
