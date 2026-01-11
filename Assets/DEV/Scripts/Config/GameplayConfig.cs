using UnityEngine;
using UnityEngine.Rendering;

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "GamePlayConfig", menuName = "Data/GamePlayConfig")]
    public class GamePlayConfig : ScriptableObject
    {
        public LevelConfig LevelConfig;

        #region Level events

        public float LevelCompleteDelay;
        public float LevelFailDelay;

        #endregion

        #region Loading Screen

        [Header("Loading Screen Settings")] public bool LoadingScreenIsEnabled = true;

        [Tooltip("Total progress bar animation duration in seconds (will be divided into 3 random stages)")]
        public float LoadingScreenProgressDuration = 1.5f;

        #endregion

        [Header("FailTexts")] public AYellowpaper.SerializedCollections.SerializedDictionary<Enums.LevelFailReason, string> levelFailMessages;
    }
}