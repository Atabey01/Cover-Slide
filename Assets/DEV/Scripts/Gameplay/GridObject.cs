using DEV.Scripts.Config;
using UnityEngine;

namespace DEV.Scripts.Gameplay
{
    public class GridObject : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        public void Initialize(int row, int col, GameConfig gameConfig)
        {
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