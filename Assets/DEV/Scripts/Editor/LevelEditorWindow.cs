using System.Collections.Generic;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace DEV.Scripts.Editor
{
    public class LevelEditorWindow : OdinEditorWindow
    {
        [MenuItem("Window/Level Editor")]
        private static void OpenWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor").Show();
        }

        private LevelConfig levelConfig;
        private Vector2 leftScrollPosition;
        private const float LeftPanelWidth = 250f;
        
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = true,
            ShowIndexLabels = true,
            ShowPaging = false,
            ShowItemCount = true,
            CustomAddFunction = nameof(AddLevel)
        )]
        [ShowInInspector]
        [HideLabel]
        private List<LevelData> Levels
        {
            get => levelConfig != null ? levelConfig.Levels : null;
            set
            {
                if (levelConfig != null)
                {
                    levelConfig.Levels = value;
                    EditorUtility.SetDirty(levelConfig);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadLevelConfig();
        }

        private void LoadLevelConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                
                if (levelConfig != null && levelConfig.Levels == null)
                {
                    levelConfig.Levels = new List<LevelData>();
                }
            }
            else
            {
                Debug.LogError("LevelConfig bulunamadı! Assets > Create > Data > LevelConfig ile oluşturun.");
            }
        }

        protected override void OnImGUI()
        {
            if (levelConfig == null)
            {
                SirenixEditorGUI.ErrorMessageBox("LevelConfig bulunamadı!");
                
                if (GUILayout.Button("Yeniden Yükle", GUILayout.Height(30)))
                {
                    LoadLevelConfig();
                }
                return;
            }

            // Ana yatay layout - Sol ve Sağ panel
            GUILayout.BeginHorizontal();
            
            // SOL PANEL - Level Listesi
            DrawLeftPanel();
            
            // Ayırıcı çizgi
            SirenixEditorGUI.VerticalLineSeparator();
            
            // SAĞ PANEL - Detay/Bilgi alanı
            DrawRightPanel();
            
            GUILayout.EndHorizontal();
            
            // Değişiklikleri kaydet
            if (GUI.changed && levelConfig != null)
            {
                EditorUtility.SetDirty(levelConfig);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawLeftPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));
            
            // Başlık
            SirenixEditorGUI.Title("Level Listesi", "", TextAlignment.Center, true);
            
            EditorGUILayout.Space(5);
            
            // Config bilgisi
            GUILayout.BeginHorizontal();
            GUILayout.Label("Config:", EditorStyles.miniLabel, GUILayout.Width(50));
            EditorGUILayout.ObjectField(levelConfig, typeof(LevelConfig), false);
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            SirenixEditorGUI.DrawSolidRect(new Rect(0, 0, LeftPanelWidth, 1), new Color(0.5f, 0.5f, 0.5f, 0.5f));
            
            EditorGUILayout.Space(10);
            
            // Scroll view içinde level listesi
            leftScrollPosition = GUILayout.BeginScrollView(leftScrollPosition);
            
            // Odin'in özelliğini kullanarak listeyi çiz
            base.OnImGUI();
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            GUILayout.BeginVertical();
            
            SirenixEditorGUI.Title("Level Detayları", "", TextAlignment.Center, true);
            
            EditorGUILayout.Space(10);
            
            // Sağ panel içeriği - şimdilik bilgi gösterelim
            if (levelConfig != null && levelConfig.Levels != null)
            {
                SirenixEditorGUI.InfoMessageBox($"Toplam Level Sayısı: {levelConfig.Levels.Count}");
                
                EditorGUILayout.Space(10);
                
                if (GUILayout.Button("Level Config'i Inspector'da Aç", GUILayout.Height(30)))
                {
                    Selection.activeObject = levelConfig;
                    EditorGUIUtility.PingObject(levelConfig);
                }
            }
            
            GUILayout.EndVertical();
        }

        private LevelData AddLevel()
        {
            return null; // Odin'in default add davranışını kullan
        }
    }
}

