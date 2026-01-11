using System.Collections.Generic;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Managers
{
    /// <summary>
    /// Profesyonel SFX ve Music yönetim sistemi.
    /// Singleton pattern kullanır ve AudioSource pooling ile performans sağlar.
    /// Enum tabanlı tip güvenli sistem.
    /// 
    /// Kullanım:
    /// SFXManager.Instance.PlaySFX(SFXType.ButtonClick);
    /// SFXManager.Instance.PlayMusic(MusicType.MenuMusic, true);
    /// </summary>
    public class SfxManager : MonoBehaviour
    {
        private static SfxManager _instance;

        public static SfxManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SfxManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SFXManager");
                        _instance = go.AddComponent<SfxManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        [Header("Audio Configuration")] [SerializeField]
        private AudioConfig audioConfig;

        [Header("Audio Source Settings")] [Tooltip("Pool'da tutulacak maksimum AudioSource sayısı")] [SerializeField]
        private int maxAudioSources = 20;

        [Tooltip("SFX için varsayılan volume (0-1 arası)")] [Range(0f, 1f)] [SerializeField]
        private float defaultSFXVolume = 1f;

        [Tooltip("Music için varsayılan volume (0-1 arası)")] [Range(0f, 1f)] [SerializeField]
        private float defaultMusicVolume = 0.7f;

        // AudioSource pooling
        private Queue<AudioSource> _sfxAudioSourcePool;
        private List<AudioSource> _activeSFXSources;
        private AudioSource _musicAudioSource;
        private MusicType _currentMusicType = MusicType.None;

        // Volume multipliers (settings'ten gelen değerler)
        private float _sfxVolumeMultiplier = 1f;
        private float _musicVolumeMultiplier = 1f;

        private bool _isInitialized = false;

        private void Awake()
        {
            // Singleton kontrolü - sadece instance'ı ayarla, initialize etme
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// SFXManager'ı initialize eder. GameManager tarafından çağrılmalıdır.
        /// </summary>
        public void Initialize(GameConfig gameConfig)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("SFXManager: Already initialized!");
                return;
            }

            // Pool'u başlat
            _sfxAudioSourcePool = new Queue<AudioSource>();
            _activeSFXSources = new List<AudioSource>();

            // Music için özel AudioSource oluştur
            _musicAudioSource = gameObject.AddComponent<AudioSource>();
            _musicAudioSource.loop = true;
            _musicAudioSource.playOnAwake = false;

            // Settings'ten volume değerlerini al
            UpdateVolumeSettings();

            _isInitialized = true;
            Debug.Log("SFXManager: Initialized");
        }

        /// <summary>
        /// Settings'ten volume değerlerini günceller
        /// </summary>
        public void UpdateVolumeSettings()
        {
            if (!_isInitialized) return;

            // Ses açıksa DataSaver'dan volume değerini al, kapalıysa 0
            _sfxVolumeMultiplier = DataSaver.GetEnableSound() ? DataSaver.GetSoundVolume() : 0f;
            _musicVolumeMultiplier = DataSaver.GetEnableMusic() ? 1f : 0f;

            // Mevcut müziğin volume'unu güncelle
            if (_musicAudioSource != null && _musicAudioSource.isPlaying)
            {
                UpdateMusicVolume();
            }
        }

        #region SFX Methods

        /// <summary>
        /// SFX ses efektini çalar
        /// </summary>
        /// <param name="sfxType">AudioConfig'de tanımlı SFX enum değeri</param>
        /// <param name="volume">Özel volume (opsiyonel, varsayılan: config'deki değer)</param>
        /// <param name="pitch">Özel pitch (opsiyonel, varsayılan: config'deki değer)</param>
        /// <param name="position">3D ses için pozisyon (opsiyonel)</param>
        /// <returns>Çalan AudioSource (null ise çalınamadı)</returns>
        public AudioSource PlaySFX(SFXType sfxType, float? volume = null, float? pitch = null, Vector3? position = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("SFXManager: Not initialized! Call Initialize() first.");
                return null;
            }

            if (audioConfig == null)
            {
                Debug.LogError("SFXManager: AudioConfig is not assigned!");
                return null;
            }

            if (!DataSaver.GetEnableSound())
            {
                return null; // Ses kapalıysa çalma
            }

            if (sfxType == SFXType.None)
            {
                return null;
            }

            var clipData = audioConfig.GetSFXClip(sfxType);
            if (clipData == null || clipData.clip == null)
            {
                return null;
            }

            AudioSource audioSource = GetPooledAudioSource();
            if (audioSource == null)
            {
                return null;
            }

            // 3D ses için pozisyon ayarla
            if (position.HasValue)
            {
                // AudioSource component'i GameObject'e bağlı, transform'u GameObject'ten al
                audioSource.gameObject.transform.position = position.Value;
                audioSource.spatialBlend = 1f; // 3D
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D
            }

            // Clip ve ayarları uygula
            audioSource.clip = clipData.clip;
            audioSource.volume = (volume ?? clipData.volume) * defaultSFXVolume * _sfxVolumeMultiplier;
            audioSource.pitch = pitch ?? clipData.pitch;
            audioSource.Play();

            // Clip bitince pool'a geri döndür
            StartCoroutine(ReturnToPoolWhenFinished(audioSource));

            return audioSource;
        }

        /// <summary>
        /// SFX ses efektini belirli bir AudioSource ile çalar (loop için)
        /// </summary>
        public AudioSource PlaySFXLoop(SFXType sfxType, float? volume = null, float? pitch = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("SFXManager: Not initialized! Call Initialize() first.");
                return null;
            }

            if (audioConfig == null || !DataSaver.GetEnableSound())
            {
                return null;
            }

            if (sfxType == SFXType.None)
            {
                return null;
            }

            var clipData = audioConfig.GetSFXClip(sfxType);
            if (clipData == null || clipData.clip == null)
            {
                return null;
            }

            AudioSource audioSource = GetPooledAudioSource();
            if (audioSource == null)
            {
                return null;
            }

            audioSource.clip = clipData.clip;
            audioSource.volume = (volume ?? clipData.volume) * defaultSFXVolume * _sfxVolumeMultiplier;
            audioSource.pitch = pitch ?? clipData.pitch;
            audioSource.loop = true;
            audioSource.Play();

            return audioSource;
        }

        /// <summary>
        /// Belirli bir AudioSource'u durdurur
        /// </summary>
        public void StopSFX(AudioSource audioSource)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                ReturnToPool(audioSource);
            }
        }

        /// <summary>
        /// Tüm SFX'leri durdurur
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in _activeSFXSources.ToArray())
            {
                if (source != null)
                {
                    source.Stop();
                    ReturnToPool(source);
                }
            }
        }

        #endregion

        #region Music Methods

        /// <summary>
        /// Müzik çalar
        /// </summary>
        /// <param name="musicType">AudioConfig'de tanımlı Music enum değeri</param>
        /// <param name="loop">Loop yapılsın mı?</param>
        /// <param name="fadeIn">Fade in efekti olsun mu?</param>
        /// <param name="fadeDuration">Fade süresi (saniye)</param>
        public void PlayMusic(MusicType musicType, bool loop = true, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("SFXManager: Not initialized! Call Initialize() first.");
                return;
            }

            if (audioConfig == null)
            {
                Debug.LogError("SFXManager: AudioConfig is not assigned!");
                return;
            }

            if (musicType == MusicType.None)
            {
                return;
            }

            var clipData = audioConfig.GetMusicClip(musicType);
            if (clipData == null || clipData.clip == null)
            {
                return;
            }

            // Aynı müzik çalıyorsa tekrar başlatma
            if (_musicAudioSource.clip == clipData.clip && _musicAudioSource.isPlaying)
            {
                return;
            }

            _musicAudioSource.clip = clipData.clip;
            _musicAudioSource.loop = loop;
            _currentMusicType = musicType;
            UpdateMusicVolume();

            if (fadeIn)
            {
                _musicAudioSource.volume = 0f;
                _musicAudioSource.Play();
                StartCoroutine(FadeMusicVolume(defaultMusicVolume * _musicVolumeMultiplier * clipData.volume,
                    fadeDuration));
            }
            else
            {
                _musicAudioSource.Play();
            }
        }

        /// <summary>
        /// Müziği durdurur
        /// </summary>
        /// <param name="fadeOut">Fade out efekti olsun mu?</param>
        /// <param name="fadeDuration">Fade süresi (saniye)</param>
        public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
        {
            if (_musicAudioSource == null || !_musicAudioSource.isPlaying)
            {
                return;
            }

            if (fadeOut)
            {
                StartCoroutine(FadeMusicVolume(0f, fadeDuration, true));
            }
            else
            {
                _musicAudioSource.Stop();
                _currentMusicType = MusicType.None;
            }
        }

        /// <summary>
        /// Müziği duraklatır
        /// </summary>
        public void PauseMusic()
        {
            if (_musicAudioSource != null && _musicAudioSource.isPlaying)
            {
                _musicAudioSource.Pause();
            }
        }

        /// <summary>
        /// Duraklatılmış müziği devam ettirir
        /// </summary>
        public void ResumeMusic()
        {
            if (_musicAudioSource != null && _musicAudioSource.time > 0)
            {
                _musicAudioSource.UnPause();
            }
        }

        private void UpdateMusicVolume()
        {
            if (_musicAudioSource != null && _musicAudioSource.clip != null && _currentMusicType != MusicType.None)
            {
                var clipData = audioConfig.GetMusicClip(_currentMusicType);
                float baseVolume = clipData != null ? clipData.volume : 1f;
                _musicAudioSource.volume = baseVolume * defaultMusicVolume * _musicVolumeMultiplier;
            }
        }

        private System.Collections.IEnumerator FadeMusicVolume(float targetVolume, float duration,
            bool stopAfterFade = false)
        {
            float startVolume = _musicAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            _musicAudioSource.volume = targetVolume;

            if (stopAfterFade)
            {
                _musicAudioSource.Stop();
                _currentMusicType = MusicType.None;
            }
        }

        #endregion

        #region Pool Management

        private AudioSource GetPooledAudioSource()
        {
            AudioSource audioSource;

            // Pool'dan al veya yeni oluştur
            if (_sfxAudioSourcePool.Count > 0)
            {
                audioSource = _sfxAudioSourcePool.Dequeue();
            }
            else
            {
                // Maksimum sayıya ulaşılmadıysa yeni oluştur
                if (_activeSFXSources.Count < maxAudioSources)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                else
                {
                    // En eski aktif source'u kullan
                    audioSource = _activeSFXSources[0];
                    _activeSFXSources.RemoveAt(0);
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }

            // Ayarları sıfırla
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            _activeSFXSources.Add(audioSource);
            return audioSource;
        }

        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;

            audioSource.Stop();
            audioSource.clip = null;
            _activeSFXSources.Remove(audioSource);
            _sfxAudioSourcePool.Enqueue(audioSource);
        }

        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource)
        {
            if (audioSource == null) yield break;

            yield return new WaitWhile(() => audioSource != null && audioSource.isPlaying);

            if (audioSource != null)
            {
                ReturnToPool(audioSource);
            }
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// SFX volume'unu ayarlar (0-1 arası)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            defaultSFXVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Music volume'unu ayarlar (0-1 arası)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            defaultMusicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        #endregion

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}