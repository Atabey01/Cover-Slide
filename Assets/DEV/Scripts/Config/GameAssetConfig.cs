using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameAssets", menuName = "Data/GameAssets")]
    public class GameAssetsConfig : ScriptableObject
    {
        [Header("Game Settings")] 
        [Space(10)] 
        
        [Header("Game Prefabs")] 
        [Space(10)]
        
        [Header("Game Materials")]
        public SerializedDictionary<Enums.ColorType, Material> Materials = new();
    }
}
