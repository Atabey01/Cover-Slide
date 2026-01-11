using DEV.Scripts.Config;
using DEV.Scripts.UI;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        private GameConfig _gameConfig;
        [SerializeField] private PanelManager _panelManager;
        [SerializeField] private PopupManager _popupManager;

        public void Initialize(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            
            _panelManager.Initialize(_gameConfig);
            _popupManager.Initialize(_gameConfig);
        }

        public void Dispose()
        {
            // Cleanup if needed
            _panelManager.Dispose();
            _popupManager.Dispose();
        }
    }
}