using System;
using AYellowpaper.SerializedCollections;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Config
{
    /// <summary>
    /// ScriptableObject ile ses clip'lerini yönetmek için kullanılır.
    /// Unity Editor'da Create > Data > Audio Config ile oluşturulabilir.
    /// Enum tabanlı sistem kullanır, tip güvenli ve refactor-friendly.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Data/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Serializable]
        public class AudioClipData
        {
            [Tooltip("Ses clip'i")]
            public AudioClip clip;
            
            [Tooltip("Varsayılan volume (0-1 arası)")]
            [Range(0f, 1f)]
            public float volume = 1f;
            
            [Tooltip("Varsayılan pitch (0.5-2 arası önerilir)")]
            [Range(0.5f, 2f)]
            public float pitch = 1f;
        }

        [Header("SFX Clips")]
        [Tooltip("Oyun içi ses efektleri - Enum'a göre atanır")]
        [SerializeField] private SerializedDictionary<SFXType, AudioClipData> sfxClips = new();

        [Header("Music Clips")]
        [Tooltip("Müzik parçaları - Enum'a göre atanır")]
        [SerializeField] private SerializedDictionary<MusicType, AudioClipData> musicClips = new();

        /// <summary>
        /// SFX clip'ini enum'a göre bulur
        /// </summary>
        public AudioClipData GetSFXClip(SFXType sfxType)
        {
            if (sfxClips == null || !sfxClips.ContainsKey(sfxType))
            {
                Debug.LogWarning($"AudioConfig: SFX clip '{sfxType}' not found!");
                return null;
            }

            return sfxClips[sfxType];
        }

        /// <summary>
        /// Music clip'ini enum'a göre bulur
        /// </summary>
        public AudioClipData GetMusicClip(MusicType musicType)
        {
            if (musicClips == null || !musicClips.ContainsKey(musicType))
            {
                Debug.LogWarning($"AudioConfig: Music clip '{musicType}' not found!");
                return null;
            }

            return musicClips[musicType];
        }
    }
}

