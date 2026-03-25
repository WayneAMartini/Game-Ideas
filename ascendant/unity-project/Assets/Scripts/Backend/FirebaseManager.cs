using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        [SerializeField] FirebaseConfig _config;

        IBackendService _service;
        ConnectionState _lastConnectionState;

        public IBackendService Service => _service;
        public FirebaseConfig Config => _config;
        public bool IsOnline => _service?.Connection == ConnectionState.Online;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeService();
        }

        void InitializeService()
        {
            if (_config != null && _config.IsFirebaseConfigured)
            {
                // When Firebase SDK is integrated, create FirebaseBackendService here
                // For now, always use local stub
                Debug.Log("[FirebaseManager] Firebase config present but SDK not integrated — using local backend");
            }

            _service = new LocalBackendService();
            _lastConnectionState = _service.Connection;
            _service.OnConnectionChanged += OnConnectionStateChanged;

            Debug.Log("[FirebaseManager] Initialized with LocalBackendService");
        }

        void OnConnectionStateChanged(ConnectionState state)
        {
            if (state == _lastConnectionState) return;

            var oldState = _lastConnectionState;
            _lastConnectionState = state;

            EventBus.Publish(new BackendConnectionChangedEvent
            {
                OldState = oldState,
                NewState = state
            });

            Debug.Log($"[FirebaseManager] Connection: {oldState} -> {state}");
        }

        void OnDestroy()
        {
            if (_service != null)
                _service.OnConnectionChanged -= OnConnectionStateChanged;

            if (Instance == this) Instance = null;
        }
    }
}
