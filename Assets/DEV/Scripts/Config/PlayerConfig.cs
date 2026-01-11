using System;
using CandyCoded;
using UnityEngine;
using UnityEngine.UIElements;

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Data/PlayerConfig", order = 0)]
    public class PlayerConfig : ScriptableObject
    {
        public IntReference LevelId;
        public IntReference Coin;

        public int BestScoreCount = 0;
        public DateTime LastActiveDate = DateTime.Now;
        public DateTime LastDailyRewardCollect = DateTime.Now;
        public int LastCollectedDailyCount;


        public bool EnableMusic = true;
        public bool EnableSound = true;
        public bool EnableVibration = true;
    }
}