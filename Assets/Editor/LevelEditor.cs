using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;

namespace DEV.Editor
{
    public class LevelEditor : EditorWindow
    {
        private const int TOOLBAR_HEIGHT = 35;
        private const int BUTTON_HEIGHT = 30;
        private const int SPACING = 10;

        private static LevelEditor instance;
        private LevelConfig levelConfig;
        private string levelConfigPath = "Assets/DEV/Data/Config/LevelConfig.asset";
        
        private LevelListPanel levelListPanel;
        private LevelData selectedLevel;
        private int selectedLevelIndex = -1;

        public static LevelEditor Instance
        {
            get
            {
                if (instance == null)
                    instance = GetWindow<LevelEditor>("Level Editor");
                return instance;
            }
        }

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            Instance.Show();
            Instance.LoadLevelConfig();
        }

        private void OnEnable()
        {
            instance = this;
            LoadLevelConfig();
            
            if (levelConfig != null)
            {
                levelListPanel = new LevelListPanel(this, levelConfig);
            }
        }

        private void LoadLevelConfig()
        {
            levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(levelConfigPath);
            
            if (levelConfig == null)
            {
                Debug.LogError($"LevelConfig bulunamadı: {levelConfigPath}");
            }
        }

        public bool IsLevelSelected(LevelData level)
        {
            return selectedLevel == level;
        }

        public void SelectLevel(LevelData level, int index)
        {
            selectedLevel = level;
            selectedLevelIndex = index;
            Repaint();
        }

        public void PlayLevel(LevelData level)
        {
            if (level == null) return;
            
            levelConfig.TestLevel = level;
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            EditorApplication.isPlaying = true;
            Debug.Log($"Test level olarak ayarlandı: {level.levelName}");
        }

        public void OnLevelDeleted()
        {
            selectedLevel = null;
            selectedLevelIndex = -1;
            Repaint();
        }

        private void OnGUI()
        {
            if (levelConfig == null)
            {
                EditorGUILayout.HelpBox("LevelConfig yüklenemedi! Lütfen path'i kontrol edin.", MessageType.Error);
                if (GUILayout.Button("LevelConfig'i Yükle"))
                {
                    LoadLevelConfig();
                    if (levelConfig != null)
                    {
                        levelListPanel = new LevelListPanel(this, levelConfig);
                    }
                }
                return;
            }

            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            
            // Sol Panel - Level Listesi
            if (levelListPanel != null)
            {
                levelListPanel.Draw();
            }
            
            // Sağ Panel - Level Detayları
            DrawLevelDetailsPanel();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            var toolbarStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = TOOLBAR_HEIGHT,
                padding = new RectOffset(SPACING, SPACING, 5, 5)
            };

            EditorGUILayout.BeginHorizontal(toolbarStyle);
            
            GUILayout.Label($"📋 Level Editor", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            var settingsContent = EditorGUIUtility.IconContent("Settings");
            settingsContent.tooltip = "LevelConfig'i Aç";
            if (GUILayout.Button(settingsContent, GUILayout.Width(30), GUILayout.Height(BUTTON_HEIGHT - 5)))
            {
                Selection.activeObject = levelConfig;
                EditorGUIUtility.PingObject(levelConfig);
            }
            
            EditorGUILayout.EndHorizontal();
        }


        private void DrawLevelDetailsPanel()
        {
            EditorGUILayout.BeginVertical();
            
            if (selectedLevel == null)
            {
                EditorGUILayout.Space(50);
                EditorGUILayout.LabelField("Bir level seçin...", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawSelectedLevelDetails();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedLevelDetails()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Level {selectedLevelIndex + 1} Detayları", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUI.BeginChangeCheck();
            
            // Level Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Adı:", GUILayout.Width(120));
            selectedLevel.levelName = EditorGUILayout.TextField(selectedLevel.levelName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Skip in Loop
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Loop'ta Atla:", GUILayout.Width(120));
            selectedLevel.skipInLoop = EditorGUILayout.Toggle(selectedLevel.skipInLoop);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Difficulty Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zorluk:", GUILayout.Width(120));
            selectedLevel.LevelDifficultyType = (LevelDifficultyType)EditorGUILayout.EnumPopup(selectedLevel.LevelDifficultyType);
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
            }
            
            EditorGUILayout.Space(20);
            
            // Asset butonları
            if (GUILayout.Button("🔍 Asset'i Seç", GUILayout.Height(30)))
            {
                Selection.activeObject = selectedLevel;
                EditorGUIUtility.PingObject(selectedLevel);
            }
            
            EditorGUILayout.EndVertical();
        }

        public void CreateNewLevel()
        {
            string savePath = "Assets/Resources/Levels";
            
            // Klasör yapısını oluştur
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            
            if (!AssetDatabase.IsValidFolder(savePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Levels");
            
            // Yeni level numarasını bul
            int levelNumber = GetNextLevelNumber();
            string levelName = $"Level{levelNumber}";
            string assetPath = $"{savePath}/{levelName}.asset";
            
            // Eğer aynı isimde dosya varsa, numara artır
            int counter = levelNumber;
            while (AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null)
            {
                counter++;
                levelName = $"Level{counter}";
                assetPath = $"{savePath}/{levelName}.asset";
            }
            
            // Yeni level oluştur
            var newLevel = CreateInstance<LevelData>();
            newLevel.levelName = levelName;
            
            AssetDatabase.CreateAsset(newLevel, assetPath);
            AssetDatabase.SaveAssets();
            
            // LevelConfig'e ekle
            levelConfig.Levels.Add(newLevel);
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            // Panel'i yenile ve seç
            levelListPanel?.Refresh();
            SelectLevel(newLevel, levelConfig.Levels.Count - 1);
            
            Debug.Log($"✅ Yeni level oluşturuldu: {levelName}");
        }

        public void DuplicateLevel(LevelData originalLevel)
        {
            if (originalLevel == null) return;

            string savePath = "Assets/Resources/Levels";
            
            // Yeni isim bul
            string baseName = originalLevel.levelName;
            int copyNumber = 1;
            string newName = $"{baseName}_Copy";
            string assetPath = $"{savePath}/{newName}.asset";
            
            while (AssetDatabase.LoadAssetAtPath<LevelData>(assetPath) != null)
            {
                copyNumber++;
                newName = $"{baseName}_Copy{copyNumber}";
                assetPath = $"{savePath}/{newName}.asset";
            }
            
            // Level'i kopyala
            var newLevel = Instantiate(originalLevel);
            newLevel.name = newName;
            newLevel.levelName = newName;
            
            AssetDatabase.CreateAsset(newLevel, assetPath);
            AssetDatabase.SaveAssets();
            
            // Orijinal level'in hemen sonrasına ekle
            int originalIndex = levelConfig.Levels.IndexOf(originalLevel);
            levelConfig.Levels.Insert(originalIndex + 1, newLevel);
            
            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            
            // Panel'i yenile ve seç
            levelListPanel?.Refresh();
            SelectLevel(newLevel, originalIndex + 1);
            
            Debug.Log($"✅ Level kopyalandı: {newName}");
        }

        private int GetNextLevelNumber()
        {
            int maxNumber = 0;
            
            foreach (var level in levelConfig.Levels)
            {
                if (level == null) continue;
                
                // Level isminden numarayı çıkar (Level1, Level2, vb.)
                string name = level.levelName;
                if (name.StartsWith("Level"))
                {
                    string numberPart = name.Substring(5);
                    if (int.TryParse(numberPart, out int number))
                    {
                        maxNumber = Mathf.Max(maxNumber, number);
                    }
                }
            }
            
            return maxNumber + 1;
        }

    }
}

