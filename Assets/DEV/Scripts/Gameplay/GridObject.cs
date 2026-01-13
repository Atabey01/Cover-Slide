using DEV.Scripts.Config;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Gameplay
{
    public class GridObject : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        public ColorType ColorType;
        public Stickman Stickman;
        public void Initialize(int row, int col, ColorType colorType, GameConfig gameConfig)
        {
            ColorType = colorType;
            SetColor(row, col, gameConfig);
        }

        private void SetColor(int row, int col, GameConfig gameConfig)
        {
            var materialIndex = (row + col) % gameConfig.GameAssetsConfig.GridMaterials.Count;
            
            if (renderer != null)
            {
                renderer.material = gameConfig.GameAssetsConfig.GridMaterials[materialIndex];
            }
        }
    }
}