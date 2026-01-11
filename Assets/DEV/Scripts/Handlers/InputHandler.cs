
using System;
using DEV.Scripts.Config;
using DEV.Scripts.Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace DEV.Scripts.Handlers
{
    public class InputHandler
    {
        private Camera _camera;
        private GameConfig _gameConfig;

        public event Action<GameObject> OnMouseDown;
        public event Action<GameObject> OnMouseUp;
        public event Action<GameObject> OnMouseDrag;
        
        public Action StartInput;

        public void Initialize(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            _camera = Camera.main;
        }

        public void Tick()
        {
            if(StateManager.GetGameState() != Enums.GameState.Play) return;
            if(StateManager.GetPopupState() != Enums.PopupState.None) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    GameObject clickedGameObject = hitInfo.collider.gameObject;
                    OnMouseDown?.Invoke(clickedGameObject);
                    StartInput?.Invoke();
                }
                else
                {
                    OnMouseDown?.Invoke(null);
                    StartInput?.Invoke();
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
