using DEV.Scripts.Config;
using DEV.Scripts.Enums;
using DEV.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class GamePanel : BasePanel
    {
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button restartButton;
        [Space] [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image levelBackground;
        [SerializeField] private Image devilImage;

        private Sequence _devilWobbleSequence;

        public override void Initialize(GameConfig gameConfig)
        {
            base.Initialize(gameConfig);
            
            settingsButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            
            settingsButton.onClick.AddListener(SettingButtonClicked);
            restartButton.onClick.AddListener(RestartButtonClicked);
        }

        public override void Show()
        {
            base.Show();
            UpdateLevelUI();
        }

        private void UpdateLevelUI()
        {
            levelText.text = "Level " + (_gameConfig.PlayerConfig.LevelId.Value + 1);
            StartDevilWobble();

            // var levelData = Config.GameplayConfig.CurrentLevelData;
            // if (levelData == null)
            // {
            //     levelData = Config.GameplayConfig.LevelConfig.GetLevel();
            //     Config.GameplayConfig.CurrentLevelData = levelData;
            // }
            //
            // var isHardLevel = levelData.LevelType == Enums.LevelType.Hard;
            //
            // devilImage.gameObject.SetActive(isHardLevel);
            // levelBackground.color = isHardLevel ? new Color(.90f, .32f, .27f) : Color.white;
            // levelText.color = isHardLevel ? Color.white : new Color(0.09f, 0.24f, 0.39f);
            //
            // if (isHardLevel)
            // {
            //     StartDevilWobble();
            // }
            // else
            // {
            //     StopDevilWobble();
            // }
        }

        private void SettingButtonClicked()
        {
            StateManager.SetPopupState(PopupState.SettingPopUp);
        }
        private void RestartButtonClicked()
        {
            StateManager.SetPopupState(PopupState.None);
            StateManager.SetGameState(GameState.Restart);
        }

        private void StartDevilWobble()
        {
            StopDevilWobble();

            _devilWobbleSequence = DOTween.Sequence();

            _devilWobbleSequence.AppendInterval(9f)
                .Append(devilImage.transform.DORotate(new Vector3(0, 0, 9f), 0.2f).SetEase(Ease.InOutSine))
                .Append(devilImage.transform.DORotate(new Vector3(0, 0, -9f), 0.4f).SetEase(Ease.InOutSine))
                .Append(devilImage.transform.DORotate(new Vector3(0, 0, 6f), 0.3f).SetEase(Ease.InOutSine))
                .Append(devilImage.transform.DORotate(new Vector3(0, 0, -6f), 0.3f).SetEase(Ease.InOutSine))
                .Append(devilImage.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.InOutSine))
                .SetLoops(-1);
        }

        private void StopDevilWobble()
        {
            _devilWobbleSequence.Kill();
            _devilWobbleSequence = null;
            
            devilImage.transform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            StopDevilWobble();
        }
    }
}
