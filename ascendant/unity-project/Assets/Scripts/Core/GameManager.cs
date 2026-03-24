using UnityEngine;

namespace Ascendant.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] Camera _mainCamera;

        GameState _currentState = GameState.Loading;
        public GameState CurrentState => _currentState;
        public Camera MainCamera => _mainCamera ? _mainCamera : Camera.main;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            SetState(GameState.Combat);
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            var oldState = _currentState;
            _currentState = newState;

            EventBus.Publish(new GameStateChangedEvent
            {
                OldState = oldState,
                NewState = newState
            });
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                EventBus.Clear();
                Instance = null;
            }
        }
    }
}
