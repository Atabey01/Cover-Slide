using System;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    /// <summary>
    /// Centralized state management system.
    /// Uses ScriptableObject for state storage (Unity's recommended approach).
    /// 
    /// Usage:
    /// 1. Initialize in GameManager: StateManager.Initialize(gameStateRef, popupStateRef);
    /// 2. Use anywhere: StateManager.SetGameState(GameState.Play);
    /// 3. Subscribe to events: StateManager.OnGameStateChanged += OnStateChanged;
    /// </summary>
    public static class StateManager
    {
        private static GameStateReference _gameStateRef;
        private static PopupStateReference _popupStateRef;
        private static bool _isInitialized;

        // Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<PopupState> OnPopupStateChanged;
        
        // FailReason (event yok, sadece simple storage)
        private static LevelFailReason _failReason = LevelFailReason.None;

        /// <summary>
        /// Initialize StateManager with ScriptableObject references.
        /// Should be called from GameManager.Initialize().
        /// </summary>
        public static void Initialize(
            GameStateReference gameStateRef,
            PopupStateReference popupStateRef)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("StateManager: Already initialized!");
                return;
            }

            _gameStateRef = gameStateRef;
            _popupStateRef = popupStateRef;

            // Subscribe to ScriptableObject events
            if (_gameStateRef != null)
            {
                _gameStateRef.UpdateEvent += OnGameStateRefChanged;
            }

            if (_popupStateRef != null)
            {
                _popupStateRef.UpdateEvent += OnPopupStateRefChanged;
            }

            _isInitialized = true;
            Debug.Log("StateManager: Initialized");
        }

        /// <summary>
        /// Dispose StateManager. Unsubscribes from events and clears references.
        /// Should be called from GameManager.Dispose().
        /// </summary>
        public static void Dispose()
        {
            if (!_isInitialized) return;

            // Unsubscribe from ScriptableObject events
            if (_gameStateRef != null)
                _gameStateRef.UpdateEvent -= OnGameStateRefChanged;
            if (_popupStateRef != null)
                _popupStateRef.UpdateEvent -= OnPopupStateRefChanged;

            // Clear references
            _gameStateRef = null;
            _popupStateRef = null;
            _isInitialized = false;

            // Clear events
            OnGameStateChanged = null;
            OnPopupStateChanged = null;

            Debug.Log("StateManager: Disposed");
        }

        // GameState Property
        public static GameState GameState
        {
            get
            {
                EnsureInitialized();
                return _gameStateRef != null ? _gameStateRef.Value : GameState.Menu;
            }
            set
            {
                EnsureInitialized();
                if (_gameStateRef != null)
                {
                    var currentState = _gameStateRef.Value;
                    if (currentState != value)
                    {
                        Debug.Log($"StateManager: GameState changing from {currentState} to {value}");
                        _gameStateRef.Value = value;
                        // OnGameStateRefChanged will trigger OnGameStateChanged event
                        // No need to call it here to avoid double invocation
                    }
                    else
                    {
                        Debug.LogWarning($"StateManager: GameState is already {value}, event not triggered");
                    }
                }
            }
        }

        // PopupState Property
        public static PopupState PopupState
        {
            get
            {
                EnsureInitialized();
                return _popupStateRef != null ? _popupStateRef.Value : PopupState.None;
            }
            set
            {
                EnsureInitialized();
                if (_popupStateRef != null && _popupStateRef.Value != value)
                {
                    _popupStateRef.Value = value;
                    OnPopupStateChanged?.Invoke(value);
                }
            }
        }

        // Helper Methods
        public static void SetGameState(GameState state) => GameState = state;
        public static void SetPopupState(PopupState state) => PopupState = state;
        public static GameState GetGameState() => GameState;
        public static PopupState GetPopupState() => PopupState;
        
        // FailReason Methods (basit get/set, event yok)
        public static void SetFailReason(LevelFailReason reason) => _failReason = reason;
        public static LevelFailReason GetFailReason() => _failReason;
        public static void ResetFailReason() => _failReason = LevelFailReason.None;

        public static void ResetGameState()
        {
            EnsureInitialized();
            _gameStateRef?.Reset();
        }

        public static void ResetPopupState()
        {
            EnsureInitialized();
            _popupStateRef?.Reset();
        }

        public static void ResetAll()
        {
            ResetGameState();
            ResetPopupState();
            ResetFailReason();
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogError("StateManager: Not initialized! Call StateManager.Initialize() first in GameManager.");
                throw new InvalidOperationException("StateManager is not initialized. Call Initialize() first.");
            }
        }

        // Event handlers for ScriptableObject events
        private static void OnGameStateRefChanged(GameState newValue)
        {
            Debug.Log($"StateManager: OnGameStateRefChanged called with {newValue}, invoking OnGameStateChanged event. Subscribers: {OnGameStateChanged?.GetInvocationList().Length ?? 0}");
            
            if (OnGameStateChanged != null)
            {
                var subscribers = OnGameStateChanged.GetInvocationList();
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        Debug.Log($"StateManager: Invoking subscriber: {subscriber.Method.DeclaringType?.Name}.{subscriber.Method.Name}");
                        ((Action<GameState>)subscriber).Invoke(newValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"StateManager: Error invoking subscriber {subscriber.Method.DeclaringType?.Name}.{subscriber.Method.Name}: {ex.Message}");
                    }
                }
            }
        }

        private static void OnPopupStateRefChanged(PopupState newValue)
        {
            OnPopupStateChanged?.Invoke(newValue);
        }
    }
}

