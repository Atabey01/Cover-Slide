using System;
using System.Collections.Generic;
using DEV.Scripts.Config;
using DEV.Scripts.Controllers;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Handlers;
using DEV.Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    public class LevelManager
    {
        private GameConfig _gameConfig;
        private InputHandler _inputHandler;
        private GameController _gameController;
        private Dictionary<Type, IController> _controllers;
        private bool _isInitialized;

        public void Initialize(GameConfig gameConfig, InputHandler inputHandler)
        {
            _gameConfig = gameConfig;
            _inputHandler = inputHandler;
            _controllers = new Dictionary<Type, IController>();
            CreateControllers();
            AddListeners(_inputHandler);

            StateManager.OnGameStateChanged += OnGameStateChange;
            
            StateManager.SetGameState(GameState.Pause);
            StateManager.SetGameState(GameState.Loading);
            StateManager.SetPopupState(PopupState.None);
        }

        private void StartNewLevel()
        {
            _isInitialized = true;
            var levelData = _gameConfig.GamePlayConfig.LevelConfig.GetLevel();

            if (levelData.LevelDifficultyType == LevelDifficultyType.Hard)
            {
                StateManager.SetPopupState(PopupState.HardLevelPopUp);
            }

            var levelID = DataSaver.GetLevelId() + 1;
            
            Debug.Log("Starting Level: " + levelID + " | LavelName: " + levelData.name);

            StartControllers(levelData);
        }

        private void DestroyLevel()
        {
            _isInitialized = false;
            Factory.DestroyAll();
            DestroyControllers();
        }

        private void OnGameStateChange(GameState state)
        {
            if (state == GameState.Start)
            {
                if (_isInitialized)
                {
                    DestroyLevel();
                }

                StartNewLevel();
            }
            else if (state == GameState.Restart && _isInitialized)
            {
                DestroyLevel();
                _isInitialized = false;
                StartLevel();
            }
            else
            {
                _isInitialized = false;
            }
        }

        #region LevelEvents

        private void StartLevel()
        {
            StateManager.SetGameState(GameState.Start);
        }

        public void LevelComplete()
        {
            StateManager.SetGameState(GameState.End);
            DataSaver.IncreaseLevelId();
            DOVirtual.DelayedCall(_gameConfig.GamePlayConfig.LevelCompleteDelay,
                () => { StateManager.SetPopupState(PopupState.LevelSuccessPopUp); });
        }

        public void LevelFailed()
        {
            RestartLevel();
            DOVirtual.DelayedCall(_gameConfig.GamePlayConfig.LevelFailDelay,
                () => { StateManager.SetPopupState(Enums.PopupState.LevelFailPopUp); });
        }

        private void RestartLevel()
        {
            StateManager.SetGameState(GameState.Restart);
        }

        #endregion

        private void StartInput()
        {
            // Logic to handle the start of input
        }


        void AddListeners(InputHandler inputHandler)
        {
            inputHandler.OnMouseDown += _gameController.MouseDown;
            inputHandler.OnMouseUp += _gameController.MouseUp;
            inputHandler.OnMouseDrag += _gameController.MouseDrag;
            inputHandler.StartInput += StartInput;
        }

        void RemoveListeners(InputHandler inputHandler)
        {
            if (_gameController != null)
            {
                inputHandler.OnMouseDown -= _gameController.MouseDown;
                inputHandler.OnMouseUp -= _gameController.MouseUp;
                inputHandler.OnMouseDrag -= _gameController.MouseDrag;
            }

            inputHandler.StartInput -= StartInput;
        }

        #region Controller Methods

        public void AddController<T>(T controller) where T : IController
        {
            _controllers.Add(typeof(T), controller);
        }

        public T GetController<T>() where T : IController
        {
            if (_controllers.TryGetValue(typeof(T), out var controller))
            {
                return (T)controller;
            }

            return default;
        }

        private void CreateControllers()
        {
            _gameController = new GameController();
            AddController(_gameController);
        }

        private void StartControllers(LevelData levelData)
        {
            foreach (var controller in _controllers)
                controller.Value.StartNewLevel(levelData);
        }

        private void DestroyControllers()
        {
            foreach (var controller in _controllers)
                controller.Value.LevelDestroy();
        }

        private void DisposeControllers()
        {
            foreach (var controller in _controllers)
                controller.Value.Dispose();
        }

        #endregion


        public void Dispose()
        {
            RemoveListeners(_inputHandler);
            DisposeControllers();
        }
    }
}