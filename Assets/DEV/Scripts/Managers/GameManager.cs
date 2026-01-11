using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Handlers;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    public sealed class GameManager : MonoBehaviour
    {
        [Header("State Management")] [SerializeField]
        private GameStateReference gameStateReference;

        [SerializeField] private PopupStateReference popupStateReference;

        [Header("Configuration")] [SerializeField]
        private GameConfig gameConfig;

        [Header("Managers")] [SerializeField] private UIManager uiManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SfxManager sfxManager;

        private InputHandler _inputHandler;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            _inputHandler.Tick();

            if (Input.GetKeyDown(KeyCode.W))
            {
                StateManager.SetPopupState(PopupState.LevelSuccessPopUp);
                StateManager.SetGameState(GameState.End);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                StateManager.SetPopupState(PopupState.LevelFailPopUp);
                StateManager.SetGameState(GameState.End);
                StateManager.SetFailReason(LevelFailReason.OutOfMoves);
            }
        }

        private void Initialize()
        {
            // Additional initialization logic can go here

            _inputHandler = new InputHandler();

            levelManager = new LevelManager();

            StateManager.Initialize(gameStateReference, popupStateReference);
            DataSaver.Initialize(gameConfig);
            Factory.Initialize(gameConfig);
            
            _inputHandler.Initialize(gameConfig);
            
            // SFXManager'ı initialize et
            sfxManager?.Initialize(gameConfig);
            
            uiManager.Initialize(gameConfig);
            levelManager.Initialize(gameConfig, _inputHandler);
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }

        private void Dispose()
        {
            Factory.Dispose();
            StateManager.Dispose();
            levelManager.Dispose();
            _inputHandler.Dispose();
            uiManager.Dispose();
        }
    }
}