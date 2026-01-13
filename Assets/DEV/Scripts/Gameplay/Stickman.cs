using DEV.Scripts.Config;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Gameplay
{
    public class Stickman : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private Animator animator;

        private GameConfig _gameConfig;
        private ColorType _colorType;
        public void Initialize(ColorType colorType, GameConfig gameConfig)
        {
            _colorType = colorType;
            _gameConfig = gameConfig;
            SetColor();
        }

        private void SetColor()
        {
            if (renderer != null)
            {
                renderer.material = _gameConfig.GameAssetsConfig.Materials[_colorType];
            }
        }
        
        public void SetAnimation(StickmanAnimationType type)
        {
            if (animator != null)
            {
                animator.SetTrigger(type.ToString());
            }
        }
    }
}