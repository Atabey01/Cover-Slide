using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DEV.Scripts.Gameplay;
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
        public Stickman StickmanPrefab;
        public GridObject GridObjectPrefab;
        
        [Header("Game Materials")]
        public SerializedDictionary<Enums.ColorType, Material> Materials = new();
        public List<Material> GridMaterials = new();
    }
}
