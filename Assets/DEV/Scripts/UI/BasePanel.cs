using DEV.Scripts.Config;
using UnityEngine;

namespace DEV.Scripts.UI
{
    public class BasePanel : MonoBehaviour
    {
        protected GameConfig _gameConfig;

        public virtual void Initialize(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
        
        public virtual void Dispose()
        {
            // Override in derived classes if needed
        }
    }
}
