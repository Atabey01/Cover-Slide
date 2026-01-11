using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using DEV.Scripts.Config;
using DEV.Scripts.Data;

namespace DEV.Editor
{
    public class LevelListPanel
    {
        private const int PANEL_WIDTH = 250;
        private const int BUTTON_HEIGHT = 30;

        private ReorderableList levelList;
        private LevelConfig levelConfig;
        private LevelEditor editor;
        private Vector2 scrollPosition;
        private string searchText = "";

        public LevelListPanel(LevelEditor editor, LevelConfig levelConfig)
        {
            this.editor = editor;
            this.levelConfig = levelConfig;
            SetupReorderableList();
        }

        private void SetupReorderableList()
        {
            if (levelConfig == null || levelConfig.Levels == null)
            {
                Debug.LogError("LevelConfig veya Levels listesi null!");
                return;
            }

            levelList = new ReorderableList(levelConfig.Levels, typeof(LevelData), true, true, true, false);

            // Yeniden sıralama callback
            levelList.onReorderCallback = (ReorderableList list) =>
            {
                SaveLevelOrder();
            };

            // Header çizimi
            levelList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, $"📋 Levels ({GetFilteredLevels().Count})", EditorStyles.boldLabel);
            };

            // Element çizimi
            levelList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var filteredLevels = GetFilteredLevels();
                if (index >= filteredLevels.Count) return;

                var level = filteredLevels[index];
                if (level == null)
                {
                    DrawNullElement(rect, index);
                    return;
                }

                DrawLevelElement(rect, level, index, isActive);
            };

            // Background çizimi
            levelList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var style = index % 2 == 0 ? new GUIStyle("CN EntryBackEven") : new GUIStyle("CN EntryBackOdd");
                    
                    var filteredLevels = GetFilteredLevels();
                    if (index < filteredLevels.Count)
                    {
                        var level = filteredLevels[index];
                        if (level != null && editor.IsLevelSelected(level))
                        {
                            // Seçili level için özel renk
                            var selectedColor = EditorGUIUtility.isProSkin ? 
                                new Color(0.3f, 0.5f, 0.8f, 0.5f) : 
                                new Color(0.6f, 0.8f, 1f, 0.5f);
                            EditorGUI.DrawRect(rect, selectedColor);
                        }
                        else
                        {
                            style.Draw(rect, false, isActive, isActive, isFocused);
                        }
                    }
                    else
                    {
                        style.Draw(rect, false, isActive, isActive, isFocused);
                    }
                }
            };

            // Yeni level ekleme
            levelList.onAddCallback = (ReorderableList list) =>
            {
                editor.CreateNewLevel();
            };
        }

        private void DrawLevelElement(Rect rect, LevelData level, int index, bool isActive)
        {
            float padding = 5f;
            rect.x += padding;
            rect.width -= padding * 2;
            rect.y += 2;
            rect.height -= 4;

            float buttonWidth = 30f;
            float buttonSpacing = 2f;
            float totalButtonWidth = (buttonWidth * 3) + (buttonSpacing * 2);
            
            var labelRect = new Rect(rect.x, rect.y, rect.width - totalButtonWidth - 5, rect.height);
            var duplicateRect = new Rect(rect.x + rect.width - totalButtonWidth, rect.y, buttonWidth, rect.height - 2);
            var deleteRect = new Rect(duplicateRect.x + buttonWidth + buttonSpacing, rect.y, buttonWidth, rect.height - 2);
            var playRect = new Rect(deleteRect.x + buttonWidth + buttonSpacing, rect.y, buttonWidth, rect.height - 2);

            // Level ismi ve özellikleri
            var style = new GUIStyle(GUI.skin.label);
            if (editor.IsLevelSelected(level))
            {
                style.fontStyle = FontStyle.Bold;
            }

            string label = $"{GetRealIndex(level) + 1}. {level.levelName}";
            if (level.skipInLoop) label += " 🚫";

            if (GUI.Button(labelRect, label, style))
            {
                editor.SelectLevel(level, GetRealIndex(level));
            }

            // Duplicate butonu (icon)
            var duplicateContent = EditorGUIUtility.IconContent("TreeEditor.Duplicate");
            duplicateContent.tooltip = "Level'i Kopyala";
            if (GUI.Button(duplicateRect, duplicateContent, EditorStyles.miniButtonLeft))
            {
                DuplicateLevel(GetRealIndex(level));
            }

            // Delete butonu (çöp kovası icon)
            var deleteContent = EditorGUIUtility.IconContent("TreeEditor.Trash");
            deleteContent.tooltip = "Level'i Sil";
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUI.Button(deleteRect, deleteContent, EditorStyles.miniButtonMid))
            {
                if (EditorUtility.DisplayDialog("Level Sil", 
                    $"{level.levelName} level'ini silmek istediğinize emin misiniz?\n\nAsset dosyası da silinecek!", 
                    "Sil", "İptal"))
                {
                    DeleteLevel(GetRealIndex(level));
                }
            }
            GUI.backgroundColor = Color.white;

            // Play butonu (icon)
            var playContent = EditorGUIUtility.IconContent("PlayButton");
            playContent.tooltip = "Bu Level'i Test Et";
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUI.Button(playRect, playContent, EditorStyles.miniButtonRight))
                {
                    editor.PlayLevel(level);
                }
            }
        }

        private void DrawNullElement(Rect rect, int index)
        {
            var labelRect = new Rect(rect.x + 5, rect.y + 2, rect.width - 40, rect.height - 4);
            var removeRect = new Rect(rect.x + rect.width - 30, rect.y + 2, 25, rect.height - 4);

            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            GUI.Label(labelRect, $"{index + 1}. ⚠️ NULL LEVEL", EditorStyles.helpBox);
            if (GUI.Button(removeRect, "X", EditorStyles.miniButton))
            {
                levelConfig.Levels.RemoveAt(index);
                SaveLevelOrder();
            }
            GUI.backgroundColor = Color.white;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(PANEL_WIDTH));

            // Search bar
            DrawSearchBar();

            // Level listesi
            EditorGUILayout.Space(5);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (levelConfig != null && levelConfig.Levels != null)
            {
                var filteredLevels = GetFilteredLevels();
                if (filteredLevels.Count > 0 || string.IsNullOrEmpty(searchText))
                {
                    // ReorderableList'i filtrelenmiş liste ile yeniden oluştur
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var tempList = new ReorderableList(filteredLevels, typeof(LevelData), false, true, false, false);
                        SetupFilteredListCallbacks(tempList);
                        tempList.DoLayoutList();
                    }
                    else
                    {
                        levelList.DoLayoutList();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Arama kriterine uygun level bulunamadı.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("LevelConfig yüklenemedi!", MessageType.Error);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            string newSearchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            
            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                newSearchText = "";
                GUI.FocusControl(null);
            }
            
            EditorGUILayout.EndHorizontal();

            if (newSearchText != searchText)
            {
                searchText = newSearchText;
                editor.Repaint();
            }
        }

        private List<LevelData> GetFilteredLevels()
        {
            if (levelConfig == null || levelConfig.Levels == null)
                return new List<LevelData>();

            if (string.IsNullOrEmpty(searchText))
                return levelConfig.Levels;

            var searchLower = searchText.ToLower();
            return levelConfig.Levels.Where(level =>
            {
                if (level == null) return false;

                // İsme göre arama
                if (level.levelName.ToLower().Contains(searchLower))
                    return true;

                // Numara ile arama
                if (int.TryParse(searchText, out int searchNumber))
                {
                    int levelIndex = levelConfig.Levels.IndexOf(level) + 1;
                    if (levelIndex == searchNumber)
                        return true;
                }

                return false;
            }).ToList();
        }

        private void SetupFilteredListCallbacks(ReorderableList list)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, $"🔍 Arama Sonuçları ({list.count})", EditorStyles.boldLabel);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= list.count) return;
                var level = list.list[index] as LevelData;
                if (level == null) return;

                DrawLevelElement(rect, level, index, isActive);
            };

            list.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var style = index % 2 == 0 ? new GUIStyle("CN EntryBackEven") : new GUIStyle("CN EntryBackOdd");
                    style.Draw(rect, false, isActive, isActive, isFocused);
                }
            };
        }

        private int GetRealIndex(LevelData level)
        {
            return levelConfig.Levels.IndexOf(level);
        }

        private void DuplicateLevel(int index)
        {
            if (index < 0 || index >= levelConfig.Levels.Count) return;

            var originalLevel = levelConfig.Levels[index];
            if (originalLevel == null) return;

            editor.DuplicateLevel(originalLevel);
        }

        private void DeleteLevel(int index)
        {
            if (index < 0 || index >= levelConfig.Levels.Count) return;

            var level = levelConfig.Levels[index];
            if (level == null) return;

            // Asset'i sil
            var assetPath = AssetDatabase.GetAssetPath(level);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Listeden çıkar
            levelConfig.Levels.RemoveAt(index);
            SaveLevelOrder();

            editor.OnLevelDeleted();
        }

        private void SaveLevelOrder()
        {
            if (levelConfig == null) return;

            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
            editor.Repaint();
        }

        public void Refresh()
        {
            SetupReorderableList();
        }
    }
}

