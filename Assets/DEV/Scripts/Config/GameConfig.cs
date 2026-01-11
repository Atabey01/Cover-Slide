
using DEV.Scripts.Data;
using UnityEngine;

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Data/GameConfig", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Header("GamePlay Config")]
        public GamePlayConfig GamePlayConfig;
        
        [Header("Player Config")]
        public PlayerConfig PlayerConfig;
        
        [Header("Game Assets Config")]
        public GameAssetsConfig GameAssetsConfig;
        
        [Header("Game Econ Config")]
        public GameEconData GameEconData;
    }
}
