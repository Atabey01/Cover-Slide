using DEV.Scripts.Data;
using DEV.Scripts.Interfaces;
using UnityEngine;

namespace DEV.Scripts.Controllers
{
    public class GameController : IController
    {
        public void Initialize()
        {
        }

        public void StartNewLevel(LevelData levelData)
        {
        }
        
        public void MouseDown(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // var box = clickedGameObject?.GetComponent<Box>();
            // if (cube != null) { /* Cube işlemleri */ }
            // if (box != null) { /* Box işlemleri */ }
        }
        
        public void MouseUp(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube işlemleri */ }
        }

        public void MouseDrag(GameObject clickedGameObject)
        {
            // Generic kullanım örneği:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube işlemleri */ }
        }

        public void LevelDestroy()
        {
        }

        public void Dispose()
        {
            
        }
    }
}
