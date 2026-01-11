using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class SettingPopUp : BasePopUp
    {
        [Header("Toggles")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle hapticToggle;
        
        [Header("Toggle Handles (kaydırılacak kısım)")]
        [SerializeField] private RectTransform musicHandle;
        [SerializeField] private RectTransform hapticHandle;
        
        [Header("Toggle Icons")]
        [SerializeField] private Image musicIcon;
        [SerializeField] private Image hapticIcon;
        
        [Header("Toggle Backgrounds")]
        [SerializeField] private Image musicBackground;
        [SerializeField] private Image hapticBackground;
        
        [Header("Colors")]
        [SerializeField] private Color onColor = new Color(0.3f, 0.8f, 0.3f); // Yeşil
        [SerializeField] private Color offColor = new Color(0.8f, 0.3f, 0.3f); // Kırmızı
        
        [Header("Sound Volume")]
        [SerializeField] private Slider soundVolumeSlider;
        [SerializeField] private Image soundIcon;
        
        [Header("Buttons")]
        [SerializeField] private Button restartButton;

        public override void Initialize(GameConfig gameConfig)
        {
            base.Initialize(gameConfig);

            // calling this method because we are use this class twice in the game
            RemoveListeners();

            hapticToggle.onValueChanged.AddListener(HapticToggleChanged);
            musicToggle.onValueChanged.AddListener(MusicToggleChanged);
            restartButton.onClick.AddListener(RestartButtonClicked);
            
            // Ses seviyesi slider'ı
            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.onValueChanged.AddListener(SoundVolumeChanged);
            }
        }

        public override void Show(float delay = 0)
        {
            InitializeButtons();
            restartButton.gameObject.SetActive(StateManager.GetPopupState() == Enums.PopupState.SettingPopUp);
            base.Show(delay);
            StateManager.SetGameState(GameState.Pause);
            SfxManager.Instance.PlaySFX(SFXType.PopupOpen);
        }

        private void InitializeButtons()
        {
            bool hapticEnabled = DataSaver.GetEnableVibration();
            bool musicEnabled = DataSaver.GetEnableMusic();
            
            hapticToggle.isOn = !hapticEnabled;
            musicToggle.isOn = !musicEnabled;
            
            // Toggle görsellerini başlat (animasyon olmadan)
            UpdateToggleVisuals(hapticHandle, hapticIcon, hapticBackground, hapticEnabled, false);
            UpdateToggleVisuals(musicHandle, musicIcon, musicBackground, musicEnabled, false);
            
            // Ses seviyesi slider'ını başlat
            if (soundVolumeSlider != null)
            {
                float soundVolume = DataSaver.GetSoundVolume();
                soundVolumeSlider.value = soundVolume;
                UpdateSoundIconColor(soundVolume, false);
            }
        }

        #region Button Clicks

        private void HapticToggleChanged(bool value)
        {
            bool isEnabled = !value;
            DataSaver.SetEnableVibration(isEnabled);
            UpdateToggleVisuals(hapticHandle, hapticIcon, hapticBackground, isEnabled, true);
            SfxManager.Instance.PlaySFX(SFXType.ButtonClick);
        }

        private void MusicToggleChanged(bool value)
        {
            bool isEnabled = !value;
            DataSaver.SetEnableMusic(isEnabled);
            UpdateToggleVisuals(musicHandle, musicIcon, musicBackground, isEnabled, true);
            SfxManager.Instance.UpdateVolumeSettings();
            SfxManager.Instance.PlaySFX(SFXType.ButtonClick);
        }
        
        private void SoundVolumeChanged(float value)
        {
            DataSaver.SetSoundVolume(value);
            
            // Ses seviyesi 0'dan büyükse ses açık, değilse kapalı
            bool soundEnabled = value > 0f;
            DataSaver.SetEnableSound(soundEnabled);
            
            UpdateSoundIconColor(value, true);
            SfxManager.Instance.UpdateVolumeSettings();
            
            // Test için ses çal (eğer ses açıksa)
            if (soundEnabled)
            {
                SfxManager.Instance.PlaySFX(SFXType.SettingsSliderChange);
            }
        }
        
        /// <summary>
        /// Sound icon'unun rengini ses seviyesine göre değiştirir
        /// </summary>
        private void UpdateSoundIconColor(float volume, bool animate)
        {
            if (soundIcon == null) return;
            
            // Volume 0 ise kırmızı (kapalı), değilse yeşil (açık)
            Color targetColor = volume > 0f ? onColor : offColor;
            
            if (animate)
            {
                soundIcon.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
            }
            else
            {
                soundIcon.color = targetColor;
            }
        }
        
        /// <summary>
        /// Toggle handle'ını kaydırır, icon ve background rengini değiştirir
        /// </summary>
        /// <param name="handle">Toggle handle (kaydırılacak kısım)</param>
        /// <param name="icon">Toggle icon</param>
        /// <param name="background">Toggle background</param>
        /// <param name="isOn">Toggle açık mı?</param>
        /// <param name="animate">Animasyon yapılsın mı?</param>
        private void UpdateToggleVisuals(RectTransform handle, Image icon, Image background, bool isOn, bool animate)
        {
            if (handle == null) return;
            
            // Handle pozisyonu (sağa/sola kaydır)
            float targetX = isOn ? 60f : -60f; // Bu değeri toggle genişliğine göre ayarlayın
            Color targetColor = isOn ? onColor : offColor;
            
            if (animate)
            {
                // Handle animasyonu (OutBack ease ile esnek hareket)
                handle.DOAnchorPosX(targetX, 0.3f).SetEase(Ease.OutBack);
                
                // Icon renk animasyonu
                if (icon != null)
                {
                    icon.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
                }
                
                // Background renk animasyonu
                if (background != null)
                {
                    background.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
                }
            }
            else
            {
                // Animasyon olmadan direkt set et
                handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
                
                if (icon != null)
                {
                    icon.color = targetColor;
                }
                
                if (background != null)
                {
                    background.color = targetColor;
                }
            }
        }

        private void RestartButtonClicked()
        {
            if (!_isActivated) return;
            Close();
            StateManager.SetGameState(Enums.GameState.Restart);
            SfxManager.Instance.PlaySFX(SFXType.ButtonClick);
        }

        public override void Close()
        {
            if (!_isActivated) return;
            _isActivated = false;
            MainFrameRectTransform.DOScale(Vector3.one * 0.7f, 0.35f).OnComplete(CloseComplete).SetEase(Ease.InBack);
            SfxManager.Instance.PlaySFX(SFXType.PopupOpen);
        }

        private void CloseComplete()
        {
            gameObject.SetActive(false);
            StateManager.SetPopupState(Enums.PopupState.None);
            StateManager.SetGameState(Enums.GameState.Play);
        }

        #endregion

        private void OnDestroy()
        {
            RemoveListeners();
            
            // Sound icon animasyonunu temizle
            if (soundIcon != null)
            {
                soundIcon.DOKill();
            }
        }

        private void RemoveListeners()
        {
            hapticToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            
            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.onValueChanged.RemoveAllListeners();
            }
        }
    }
}