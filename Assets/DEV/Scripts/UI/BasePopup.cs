using DEV.Scripts.Config;
using DEV.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public abstract class BasePopUp : MonoBehaviour
    {
        [SerializeField] protected RectTransform MainFrameRectTransform;
        [SerializeField] protected Button _closeButton;
        protected bool _isActivated;
        protected GameConfig Config;

        public virtual void Initialize(GameConfig gameConfig)
        {
            Config = gameConfig;
            if (_closeButton) _closeButton.onClick.AddListener(Close);
            _isActivated = false;

            CloseWithoutAnim();
        }

        public virtual void Show(float delay = 0)
        {
            MainFrameRectTransform.transform.localScale = Vector3.one * 0.7f;
            _isActivated = true;
            SetActive(true);
            ShowAnimation(delay);
        }

        private void ShowAnimation(float delay)
        {
            MainFrameRectTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack)
                .SetDelay(delay);
        }

        public virtual void Close()
        {
            if (!_isActivated) return;
            _isActivated = false;
            MainFrameRectTransform.DOScale(Vector3.one * 0.7f, 0.25f).OnComplete(CloseComplete).SetEase(Ease.InBack);
            StateManager.SetPopupState(Enums.PopupState.None);
        }

        private void CloseComplete()
        {
            SetActive(false);
        }

        public virtual void CloseWithoutAnim()
        {
            SetActive(false);
        }

        private void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }
    }
}
