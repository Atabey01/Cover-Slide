using System.Collections.Generic;
using AssetKits.ParticleImage;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class LevelFailedPopUp : BasePopUp
    {
        // [SerializeField] private Button _skipLevelButton;
        // [SerializeField] private Button _keepPlayingButton;
        // [SerializeField] private GameObject _noMoneyButton;

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button _retryButton;

        // [Space(10)] [SerializeField] private TextMeshProUGUI CoinText;
        [Space(10)] [SerializeField] private TextMeshProUGUI FailText;
        [Space(10)] [SerializeField] private ParticleImage particle;

        [Space] [SerializeField] private Button BlockerButton;
        [SerializeField] private CanvasGroup[] affectedCanvasGroups;

        // private bool skipLevelActive;
        private Tween _fadeTween;
        private GameConfig _gameConfig;
        // private int _currentCoinDisplay;


        public override void Initialize(GameConfig gameConfig)
        {
            base.Initialize(gameConfig);
            _gameConfig = gameConfig;
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(OnRetryButtonClicked);
            }
            BlockerButton.onClick.RemoveAllListeners();
            // _skipLevelButton.onClick.RemoveAllListeners();
            // _skipLevelButton.onClick.AddListener(OnSkipLevelButtonClicked);
            // _keepPlayingButton.onClick.RemoveAllListeners();
            // skipLevelActive = true;

            SetupBlockerButton();
        }

        private void SetupBlockerButton()
        {
            if (!BlockerButton) return;

            var eventTrigger = BlockerButton.GetComponent<EventTrigger>();
            if (!eventTrigger)
                eventTrigger = BlockerButton.gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.Clear();

            BlockerButton.onClick.RemoveAllListeners();

            var pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((data) => OnBlockerPressed());
            eventTrigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => OnBlockerReleased());
            eventTrigger.triggers.Add(pointerUp);
        }


        private void OnRetryButtonClicked()
        {
            Close();
        }

        public override void Show(float delay = 0)
        {
            canvasGroup.alpha = 0;
            // _keepPlayingButton.gameObject.SetActive(true);
            // _noMoneyButton.gameObject.SetActive(false);
            // _currentCoinDisplay = _gameConfig.PlayerConfig.Coin.Value;
            // CoinText.text = _currentCoinDisplay.ToString();

            var failTextIndex = _gameConfig.GamePlayConfig.levelFailMessages[StateManager.GetFailReason()];
            FailText.text = failTextIndex;
            gameObject.SetActive(true);
            _isActivated = true;

            canvasGroup.DOFade(1, 0.3f).SetId("LevelFailedPopUpFadeIn" + GetInstanceID());
        }

        private void OnDestroy()
        {
            _retryButton.onClick.RemoveAllListeners();
            _fadeTween?.Kill();
            DOTween.Kill("LevelFailedPopUpFadeIn" + GetInstanceID());
            DOTween.Kill("LevelFailedPopUpFadeOut" + GetInstanceID());

            // Affected canvas groups animasyonlarını temizle
            foreach (var cg in affectedCanvasGroups)
            {
                if (cg != null)
                {
                    cg.DOKill();
                }
            }
        }

        public override void Close()
        {
            if (!_isActivated) return;
            _isActivated = false;
            CloseComplete();

            // canvasGroup.DOFade(0, 0.3f).SetId("LevelFailedPopUpFadeOut" + GetInstanceID())
            //     .OnComplete(CloseComplete);
        }


        private void CloseComplete()
        {
            DOTween.Kill("LevelFailedPopUpFadeIn" + GetInstanceID());
            DOTween.Kill("LevelFailedPopUpFadeOut" + GetInstanceID());

            StateManager.SetPopupState(Enums.PopupState.None);
            StateManager.SetGameState(Enums.GameState.Restart);

            gameObject.SetActive(false);
        }

        private void OnBlockerPressed()
        {
            _fadeTween?.Kill();
            _fadeTween = DOTween.To(() => 1f, alpha =>
            {
                for (var i = 0; i < affectedCanvasGroups.Length; i++)
                {
                    if (affectedCanvasGroups[i])
                        affectedCanvasGroups[i].alpha = alpha;
                }
            }, 0f, 0.3f).SetEase(Ease.InQuad);
        }

        private void OnBlockerReleased()
        {
            _fadeTween?.Kill();

            if (affectedCanvasGroups == null || affectedCanvasGroups.Length == 0)
                return;

            var currentAlpha = affectedCanvasGroups[0] ? affectedCanvasGroups[0].alpha : 0f;

            _fadeTween = DOTween.To(() => currentAlpha, alpha =>
            {
                for (var i = 0; i < affectedCanvasGroups.Length; i++)
                {
                    if (affectedCanvasGroups[i])
                        affectedCanvasGroups[i].alpha = alpha;
                }
            }, 1f, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}