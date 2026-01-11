using DEV.Scripts.Managers;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    /// <summary>
    /// SFXManager kullanım örnekleri
    /// Bu dosya sadece referans içindir, projeye dahil edilmesi gerekmez.
    /// </summary>
    public class SFXManagerUsage : MonoBehaviour
    {
        private void Start()
        {
            // ============================================
            // SFX (Ses Efektleri) Kullanımı
            // ============================================
            
            // Basit SFX çalma (Enum tabanlı - tip güvenli)
            SfxManager.Instance.PlaySFX(Enums.SFXType.ButtonClick);
            
            
            // Loop yapan SFX (örneğin motor sesi)
            AudioSource motorSound = SfxManager.Instance.PlaySFXLoop(Enums.SFXType.ButtonClick);
            
            // Loop SFX'i durdurma
            if (motorSound != null)
            {
                SfxManager.Instance.StopSFX(motorSound);
            }
            
            // Tüm SFX'leri durdurma
            SfxManager.Instance.StopAllSFX();
            
            // ============================================
            // Music (Müzik) Kullanımı
            // ============================================
            
            // Basit müzik çalma (loop açık)
            SfxManager.Instance.PlayMusic(Enums.MusicType.GameplayMusic);
            
            // Fade in ile müzik çalma
            SfxManager.Instance.PlayMusic(Enums.MusicType.MenuMusic, loop: true, fadeIn: true, fadeDuration: 2f);
            
            // Müziği durdurma
            SfxManager.Instance.StopMusic();
            
            // Fade out ile müzik durdurma
            SfxManager.Instance.StopMusic(fadeOut: true, fadeDuration: 1.5f);
            
            // Müziği duraklatma
            SfxManager.Instance.PauseMusic();
            
            // Duraklatılmış müziği devam ettirme
            SfxManager.Instance.ResumeMusic();
            
            // ============================================
            // Volume Kontrolü
            // ============================================
            
            // SFX volume ayarlama (0-1 arası)
            SfxManager.Instance.SetSFXVolume(0.8f);
            
            // Music volume ayarlama (0-1 arası)
            SfxManager.Instance.SetMusicVolume(0.6f);
            
            // Settings değiştiğinde volume'ları güncelle
            SfxManager.Instance.UpdateVolumeSettings();
        }
        
        // ============================================
        // Button Click Örneği
        // ============================================
        public void OnButtonClick()
        {
            SfxManager.Instance.PlaySFX(Enums.SFXType.ButtonClick);
        }
        
        // ============================================
        // Level Başlangıcı Örneği
        // ============================================
        public void OnLevelStart()
        {
            SfxManager.Instance.PlayMusic(Enums.MusicType.GameplayMusic, loop: true, fadeIn: true);
        }
        
        // ============================================
        // Level Bitişi Örneği
        // ============================================
        public void OnLevelEnd(bool success)
        {
            if (success)
            {
                SfxManager.Instance.PlaySFX(Enums.SFXType.LevelComplete);
                SfxManager.Instance.StopMusic(fadeOut: true, fadeDuration: 1f);
            }
            else
            {
                SfxManager.Instance.PlaySFX(Enums.SFXType.LevelFail);
            }
        }
    }
}

