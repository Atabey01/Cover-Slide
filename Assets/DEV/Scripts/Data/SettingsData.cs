using System;

namespace DEV.Scripts.Data
{
    [Serializable] public class SettingData
    {
        public bool EnableSound;
        public bool EnableVibration;
        public bool EnableMusic;
        
        [UnityEngine.Range(0f, 1f)]
        public float SoundVolume = 1f; // Ses seviyesi (0-1 arası)
    }
}