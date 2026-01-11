using System.Collections.Generic;
using DEV.Scripts.Data;
using UnityEngine;
#if ELEPHANT_ENABLE
#endif

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Data/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public int LoopStartLevel = 1;
        public LevelData TestLevel;
        public List<LevelData> Levels;
        private List<LevelData> _orderedLevels;

        public LevelData GetLevel()
        {
#if UNITY_EDITOR
            var testLevelIndex = PlayerPrefs.GetInt("TestLevelIndex", -1);
            if (testLevelIndex >= 0)
            {
                PlayerPrefs.DeleteKey("TestLevelIndex");
                return Levels[testLevelIndex % Levels.Count];
            }
#endif
            // _orderedLevels null ise veya boşsa, OrganizeLevelOrder() çağır
            if (_orderedLevels == null || _orderedLevels.Count == 0)
            {
                OrganizeLevelOrder();
            }
            
            // Hala null veya boşsa, Levels'i kullan
            if (_orderedLevels == null || _orderedLevels.Count == 0)
            {
                if (Levels == null || Levels.Count == 0)
                {
                    Debug.LogError("LevelConfig: No levels available!");
                    return null;
                }
                _orderedLevels = Levels;
            }
            
            var level = DataSaver.GetLevelId();
            var id = level;

            if (level >= _orderedLevels.Count)
            {
                // Döngüdeki kullanılabilir levelleri bul
                var availableLevels = new List<int>();
                for (int i = LoopStartLevel; i < _orderedLevels.Count; i++)
                {
                    if (!_orderedLevels[i].skipInLoop)
                    {
                        availableLevels.Add(i);
                    }
                }

                // Eğer kullanılabilir level yoksa, tüm levelleri sıfırla
                if (availableLevels.Count == 0)
                {
                    return _orderedLevels[0];
                }

                // Level indeksini hesapla
                var loopIndex = (level - LoopStartLevel) % availableLevels.Count;
                id = availableLevels[loopIndex];
            }
            else
            {
                id = level % _orderedLevels.Count;
            }

            // Debug.LogError($"Loaded Level ID: {_orderedLevels[id].name}");

            return _orderedLevels[id];
        }

        public void OrganizeLevelOrder()
        {
#if !ELEPHANT_ENABLE
            GenerateDefaultOrder();
            _orderedLevels = Levels;
            return;
#endif
#if ELEPHANT_ENABLE
            LoopStartLevel = RemoteConfig.GetInstance().GetInt("loop_start_level", LoopStartLevel);
            if (!RemoteConfig.GetInstance().GetBool("remote_level_order_enable", false))
            {
                _orderedLevels = Levels;
                return;
            }
            var levelOrder = CheckLevelOrder(RemoteConfig.GetInstance().Get("level_order", ""));
            if (levelOrder == null)
            {
                _orderedLevels = Levels;
                return;
            }
            _orderedLevels = new List<LevelData>();
            foreach (var i in levelOrder)
            {
                _orderedLevels.Add(Levels[i]);
            }
            if (LoopStartLevel >= _orderedLevels.Count)
            {
                LoopStartLevel = 1;
            }
#endif
        }


        private List<int> CheckLevelOrder(string order)
        {
            Debug.Log(order);
            var levelOrderList = new List<int>();
            var orderArray = order.Split(',');
            foreach (var s in orderArray)
            {
                var val = int.Parse(s);
                levelOrderList.Add(val);
                if (val < 0 || val >= Levels.Count)
                {
                    Debug.Log("Invalid Level Order: " + order);
                    return null;
                }
            }

            return levelOrderList;
        }


        private void GenerateDefaultOrder()
        {
            _orderedLevels = Levels;
        }

#if UNITY_EDITOR
        [ContextMenu("Rename Levels")]
        private void RenameLevels()
        {
            for (int i = 0; i < Levels.Count; i++)
            {
                if (Levels[i] != null)
                {
                    string newName = $"LevelM-{i + 1}";
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(Levels[i]);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
                        Levels[i].name = newName;
                        Debug.Log($"Renamed level to: {newName}");
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find asset path for level at index {i}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Level at index {i} is null");
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}