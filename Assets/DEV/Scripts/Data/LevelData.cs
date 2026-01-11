using UnityEngine;

namespace DEV.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Data/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Settings")] 
        [SerializeField] public string levelName;
        [SerializeField] public bool skipInLoop = false;
        [SerializeField] public Enums.LevelDifficultyType LevelDifficultyType = Enums.LevelDifficultyType.Normal;
    }
}