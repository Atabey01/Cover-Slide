using UnityEngine;

namespace DEV.Scripts.Data
{
    [CreateAssetMenu(fileName = "GameEcon", menuName = "Data/GameEcon", order = 0)]
    public class GameEconData : ScriptableObject
    {
        [Header("Daily")] public int dailyWinCoin = 101;
        public int oneMoreDayCoin = 40;

        [Header("Ad")] public int adWinCoin = 30;

        [Header("InitialCoin")] public int initialCoin = 130;

        [Header("Level Rewards")] public int levelCompleteCoin = 50;
    }
}