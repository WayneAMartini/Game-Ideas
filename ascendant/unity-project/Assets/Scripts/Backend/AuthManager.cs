using UnityEngine;
using Ascendant.Core;

namespace Ascendant.Backend
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        AuthState _authState = AuthState.SignedOut;
        string _userId;

        public AuthState CurrentAuthState => _authState;
        public string UserId => _userId;
        public bool IsSignedIn => _authState != AuthState.SignedOut;
        public string DisplayName => GetService()?.GetCurrentPlayer()?.DisplayName ?? "Player";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Attempt Game Center first, fall back to anonymous
            SignIn();
        }

        IBackendService GetService()
        {
            return FirebaseManager.Instance?.Service;
        }

        public void SignIn()
        {
            var service = GetService();
            if (service == null)
            {
                Debug.LogWarning("[AuthManager] No backend service available");
                return;
            }

            // Try Game Center first
            service.SignInWithGameCenter((success, userId) =>
            {
                if (success)
                {
                    _authState = AuthState.SignedIn;
                    _userId = userId;
                    PublishAuthEvent();
                    Debug.Log($"[AuthManager] Signed in with Game Center: {userId}");
                    return;
                }

                // Fall back to anonymous
                service.SignInAnonymous((anonSuccess, anonUserId) =>
                {
                    if (anonSuccess)
                    {
                        _authState = AuthState.Anonymous;
                        _userId = anonUserId;
                        PublishAuthEvent();
                        Debug.Log($"[AuthManager] Signed in anonymously: {anonUserId}");
                    }
                    else
                    {
                        _authState = AuthState.SignedOut;
                        Debug.LogWarning("[AuthManager] Failed to sign in");
                    }
                });
            });
        }

        public void LinkToGameCenter()
        {
            if (_authState != AuthState.Anonymous)
            {
                Debug.LogWarning("[AuthManager] Can only link anonymous accounts to Game Center");
                return;
            }

            var service = GetService();
            service?.LinkAnonymousToGameCenter(success =>
            {
                if (success)
                {
                    _authState = AuthState.SignedIn;
                    PublishAuthEvent();
                    Debug.Log("[AuthManager] Linked anonymous account to Game Center");
                }
            });
        }

        void PublishAuthEvent()
        {
            EventBus.Publish(new AuthStateChangedEvent
            {
                State = _authState,
                UserId = _userId
            });
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
