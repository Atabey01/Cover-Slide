using System;
using CandyCoded;
using DEV.Scripts.Config;
using UnityEngine;

namespace DEV.Scripts.Data
{
    public static class DataSaver
    {
        private static GameData _gameData;
        private static GameConfig _gameConfig;
        private static SettingData _settingData;

        public static void Initialize(GameConfig config)
        {
            _gameConfig = config;
            // RemoteConfigsUpdated();
            LoadGameData();
            LoadSettingData();
        }

//         private static void RemoteConfigsUpdated()
//         {
// // #if ELEPHANT_ENABLE
// //              var initialCoin = RemoteConfig.GetInstance().GetInt("initial_coin", _gameConfig.GameEconData.initialCoin);
// //              _gameConfig.GameEconData.initialCoin = initialCoin;
// //
// //             var levelCompleteCoin = RemoteConfig.GetInstance().GetInt("level_complete_coin", _gameConfig.GameEconData.levelCompleteCoin);
// //             _gameConfig.GameEconData.levelCompleteCoin = levelCompleteCoin;
// //
// //             var keepPlayingDefaultCoin = RemoteConfig.GetInstance().GetInt("keep_playing_default_coin", _gameConfig.GameEconData.KeepPlayingDefaultCoin);
// //             _gameConfig.GameEconData.KeepPlayingDefaultCoin = keepPlayingDefaultCoin;
// //             
// //             var keepPlayingCoinIncrease = RemoteConfig.GetInstance().GetInt("keep_playing_coin_increase", _gameConfig.GameEconData.KeepPlayingCoinIncrease);
// //              _gameConfig.GameEconData.KeepPlayingCoinIncrease = keepPlayingCoinIncrease;
// // #endif
//            
//         }

        #region LoadAndSave

        private static void LoadGameData()
        {
            try
            {
                _gameData = SaveManager.LoadData<GameData>("gameData.dat");
            }
            catch (Exception)
            {
                _gameData = new GameData
                {
                    BestScoreCount = 0,
                    CoinCount = _gameConfig.GameEconData.initialCoin,
                    LevelId = 0,
                    LastActiveDate = DateTime.Now,
                    LastDailyRewardCollect = DateTime.Now,
                    LastCollectedDailyCount = 0
                };
                Save();
            }

            _gameConfig.PlayerConfig.LevelId.Value = _gameData.LevelId;
            _gameConfig.PlayerConfig.BestScoreCount = _gameData.BestScoreCount;
            _gameConfig.PlayerConfig.LastActiveDate = _gameData.LastActiveDate;
            _gameConfig.PlayerConfig.LastDailyRewardCollect = _gameData.LastDailyRewardCollect;
            _gameConfig.PlayerConfig.LastCollectedDailyCount = _gameData.LastCollectedDailyCount;
            _gameConfig.PlayerConfig.Coin.Value = _gameData.CoinCount;
        }

        private static void Save()
        {
            if (_gameData != null) SaveManager.SaveData(_gameData, "gameData.dat");
        }


        private static void LoadSettingData()
        {
            try
            {
                _settingData = SaveManager.LoadData<SettingData>("settingData.dat");
            }
            catch (Exception)
            {
                _settingData = new SettingData()
                {
                    EnableMusic = true,
                    EnableSound = true,
                    EnableVibration = true,
                    SoundVolume = 1f
                };
                SaveSettingData();
            }
        }

        private static void SaveSettingData()
        {
            if (_settingData != null) SaveManager.SaveData(_settingData, "settingData.dat");
        }

        #endregion

        #region Coin

        public static int GetCoinCount() => _gameData.CoinCount;

        public static void AddCoin(int count)
        {
            _gameData.CoinCount += count;
            _gameConfig.PlayerConfig.Coin.Value += count;
            Save();
        }

        public static void SpendCoin(int count)
        {
            _gameData.CoinCount -= count;
            _gameConfig.PlayerConfig.Coin.Value -= count;
            Save();
        }

        #endregion

        #region Level

        public static int GetLevelId()
        {
            if (_gameConfig == null || _gameConfig.PlayerConfig == null || _gameConfig.PlayerConfig.LevelId == null)
            {
                Debug.LogError("DataSaver: GetLevelId() called before Initialize() or config is null!");
                return 0;
            }
            return _gameConfig.PlayerConfig.LevelId.Value;
        }

        public static int GetLevelIdForAnalytics() => _gameConfig.PlayerConfig.LevelId.Value + 1;

        public static void IncreaseLevelId()
        {
            _gameData.LevelId += 1;
            _gameConfig.PlayerConfig.LevelId.Value = _gameData.LevelId;
            Save();
        }

        public static void DecreaseLevelId()
        {
            _gameData.LevelId -= 1;
            if (_gameData.LevelId < 0) _gameData.LevelId = 0;
            _gameConfig.PlayerConfig.LevelId.Value = _gameData.LevelId;
            Save();
        }

        public static void ResetLevelId()
        {
            _gameData.LevelId = 0;
            _gameConfig.PlayerConfig.LevelId.Value = _gameData.LevelId;
            Save();
        }

        public static void SetLevelId(int levelId)
        {
            _gameData.LevelId = levelId - 1;
            if (_gameData.LevelId < 0) _gameData.LevelId = 0;
            _gameConfig.PlayerConfig.LevelId.Value = _gameData.LevelId;
            Save();
        }

        #endregion

        #region BestScore

        public static int GetBestScore() => _gameConfig.PlayerConfig.BestScoreCount;

        public static void SetBestScore(int score)
        {
            _gameData.BestScoreCount = score;
            Save();
        }

        #endregion

        #region DailyReward

        public static void UpdateCollectDailyRewardTime()
        {
            _gameData.LastDailyRewardCollect = DateTime.Now;
            Save();
        }

        public static DateTime GetLastCollectDailyRewardTime() => _gameData.LastDailyRewardCollect;

        public static void ResetDailyRewardCount()
        {
            _gameData.LastCollectedDailyCount = 0;
            Save();
        }

        public static void IncreaseDailyRewardCount()
        {
            _gameData.LastCollectedDailyCount += 1;
            Save();
        }

        public static int GetDailyCollectCount() => _gameData.LastCollectedDailyCount;

        #endregion

        #region Setting

        public static bool GetEnableMusic() => _settingData.EnableMusic;

        public static void SetEnableMusic(bool enable)
        {
            _settingData.EnableMusic = enable;
            SaveSettingData();
        }

        public static void SetEnableSound(bool enable)
        {
            _settingData.EnableSound = enable;
            SaveSettingData();
        }

        public static bool GetEnableSound() => _settingData.EnableSound;

        public static void SetEnableVibration(bool enable)
        {
            _settingData.EnableVibration = enable;
            SaveSettingData();
        }

        public static bool GetEnableVibration() => _settingData.EnableVibration;

        public static void SetSoundVolume(float volume)
        {
            _settingData.SoundVolume = Mathf.Clamp01(volume);
            SaveSettingData();
        }

        public static float GetSoundVolume() => _settingData.SoundVolume;

        #endregion
    }
}