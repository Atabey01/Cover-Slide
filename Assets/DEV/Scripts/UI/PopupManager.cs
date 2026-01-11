using AYellowpaper.SerializedCollections;
using DEV.Scripts.Config;
using DEV.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private Button BlockerButton;
        [SerializeField] private SerializedDictionary<Enums.PopupState, BasePopUp> PopUps;

        private GameConfig _gameConfig;
        private BasePopUp _currentPopUp;

        public void Initialize(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;

            gameObject.SetActive(true);

            foreach (var popUp in PopUps) popUp.Value.Initialize(gameConfig);

            StateManager.OnPopupStateChanged -= OnPopStateUpdated;
            StateManager.OnPopupStateChanged += OnPopStateUpdated;

            BlockerButton.onClick.AddListener(BlockerButtonClicked);
        }

        private void OnPopStateUpdated(Enums.PopupState value)
        {
            if (value == Enums.PopupState.None)
            {
                _currentPopUp = null;
                BlockerButton.gameObject.SetActive(false);
                return;
            }

            PopUps.TryGetValue(value, out var popUp);
            if (popUp == null) return;

            CloseAll();

            _currentPopUp = popUp;
            _currentPopUp.Show();

            BlockerButton.gameObject.SetActive(true);
        }

        private void BlockerButtonClicked()
        {
            if (_currentPopUp == null) return;

            BlockerButton.gameObject.SetActive(false);
            _currentPopUp.Close();
            _currentPopUp = null;
        }

        private void CloseAll()
        {
            foreach (var popUp in PopUps) popUp.Value.CloseWithoutAnim();
            BlockerButton.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            if (_gameConfig) StateManager.OnPopupStateChanged -= OnPopStateUpdated;
            BlockerButton.onClick.RemoveListener(BlockerButtonClicked);
        }
    }
}
