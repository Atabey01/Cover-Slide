using System;

namespace DEV.Scripts.Data
{
    [Serializable]
    public class GameData
    {
        public int BestScoreCount;
        public int CoinCount;
        public int LevelId;
        public DateTime LastActiveDate;
        public DateTime LastDailyRewardCollect;
        public int LastCollectedDailyCount;
    }
}