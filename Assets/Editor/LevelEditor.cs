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
        private string gameConfigPath = "Assets/DEV/Data/Config/GameConfig.asset";
        private GameConfig gameConfig;
        
        private LevelListPanel levelListPanel;
        private LevelData selectedLevel;
        private int selectedLevelIndex = -1;

        // Foldout durumları
        private bool levelDetailsFoldout = true;
        private bool gridAreaFoldout = true;
        private bool objectiveColumnsFoldout = true;
        
        // Objective Columns scroll positions (her sütun için ayrı scroll position)
        private Dictionary<int, Vector2> objectiveColumnScrollPositions = new Dictionary<int, Vector2>();
        // Objective Columns horizontal scroll position
        private Vector2 objectiveColumnsHorizontalScroll = Vector2.zero;
        
        // Painting mode
        private enum PaintingMode
        {
            Frame,
            Stickman
        }
        
        private PaintingMode currentPaintingMode = PaintingMode.Stickman;
        private ColorType selectedColorType = ColorType.Red;
        
        // Frame mode variables
        private FrameShape selectedFrameShape = null;
        private Vector2Int framePreviewPosition = Vector2Int.zero;
        private bool isFrameDragging = false;
        
        // Drag painting
        private bool isDragging = false;
        private HashSet<Vector2Int> paintedCellsThisDrag = new HashSet<Vector2Int>();
        
        // Grid view controls
        private float gridZoom = 1f;
        private Vector2 gridScrollPosition = Vector2.zero;
        private float baseCellSize = 30f;
        private bool showGridCoordinates = false;

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
            LoadGameConfig();
            
            if (levelConfig != null)
            {
                levelListPanel = new LevelListPanel(this, levelConfig);
            }
            
            // AssetDatabase'i refresh et (yeni shape'lerin görünmesi için)
            AssetDatabase.Refresh();
        }

        private void LoadLevelConfig()
        {
            levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(levelConfigPath);
            
            if (levelConfig == null)
            {
                Debug.LogError($"LevelConfig bulunamadı: {levelConfigPath}");
            }
        }
        
        private void LoadGameConfig()
        {
            gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath);
            
            if (gameConfig == null)
            {
                Debug.LogError($"GameConfig bulunamadı: {gameConfigPath}");
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
            
            // Save butonu
            if (selectedLevel != null)
            {
                if (GUILayout.Button("💾 Save", GUILayout.Height(BUTTON_HEIGHT - 5)))
                {
                    SaveSelectedLevel();
                }
            }
            
            var settingsContent = EditorGUIUtility.IconContent("Settings");
            settingsContent.tooltip = "LevelConfig'i Aç";
            if (GUILayout.Button(settingsContent, GUILayout.Width(30), GUILayout.Height(BUTTON_HEIGHT - 5)))
            {
                Selection.activeObject = levelConfig;
                EditorGUIUtility.PingObject(levelConfig);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSelectedLevel()
        {
            if (selectedLevel == null) return;
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ Level saved: {selectedLevel.levelName}");
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
            // Level Detayları Foldout
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            levelDetailsFoldout = EditorGUILayout.Foldout(levelDetailsFoldout, 
                $"Level {selectedLevelIndex + 1} Detayları", true, EditorStyles.foldoutHeader);
            
            if (levelDetailsFoldout)
            {
                EditorGUILayout.Space(5);
            
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
            
                EditorGUILayout.Space(10);
            
            // Asset butonları
            if (GUILayout.Button("🔍 Asset'i Seç", GUILayout.Height(30)))
            {
                Selection.activeObject = selectedLevel;
                EditorGUIUtility.PingObject(selectedLevel);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // Grid Alan Bölümü
            EditorGUILayout.Space(10);
            DrawGridAreaSection();
            
            // Objective Columns Bölümü
            EditorGUILayout.Space(10);
            DrawObjectiveColumnsSection();
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

        private void DrawGridAreaSection()
        {
            if (selectedLevel == null) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Grid Alan Foldout
            gridAreaFoldout = EditorGUILayout.Foldout(gridAreaFoldout, 
                "Grid Alan", true, EditorStyles.foldoutHeader);
            
            if (gridAreaFoldout)
            {
                EditorGUILayout.Space(5);
                
                EditorGUI.BeginChangeCheck();
                
                // Grid Boyutları
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Grid Boyutu:", GUILayout.Width(120));
                int newSatir = EditorGUILayout.IntField(selectedLevel.gridSatirSayisi, GUILayout.Width(60));
                EditorGUILayout.LabelField("x", GUILayout.Width(20));
                int newSutun = EditorGUILayout.IntField(selectedLevel.gridSutunSayisi, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
                
                if (newSatir != selectedLevel.gridSatirSayisi || newSutun != selectedLevel.gridSutunSayisi)
                {
                    selectedLevel.gridSatirSayisi = Mathf.Max(1, newSatir);
                    selectedLevel.gridSutunSayisi = Mathf.Max(1, newSutun);
                }
                
                EditorGUILayout.Space(5);
                
                // Grid Oluştur Butonu
                if (GUILayout.Button("Grid Oluştur", GUILayout.Height(30)))
                {
                    CreateGrid();
                }
                
                EditorGUILayout.Space(10);
                
                // Painting Mode Selection
                EditorGUILayout.LabelField("Painting Mode:", EditorStyles.boldLabel);
                currentPaintingMode = (PaintingMode)EditorGUILayout.EnumPopup(currentPaintingMode);
                
                EditorGUILayout.Space(5);
                
                // Color Palette (Stickman mode)
                if (currentPaintingMode == PaintingMode.Stickman)
                {
                    DrawColorPalette();
                }
                else if (currentPaintingMode == PaintingMode.Frame)
                {
                    DrawFrameModeUI();
                }
                
                EditorGUILayout.Space(10);
                
                // Grid View Controls
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("View Controls:", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Zoom:", GUILayout.Width(60));
                gridZoom = EditorGUILayout.Slider(gridZoom, 0.5f, 2f, GUILayout.Width(150));
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    gridZoom = 1f;
                    gridScrollPosition = Vector2.zero;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Cell Size:", GUILayout.Width(60));
                baseCellSize = EditorGUILayout.Slider(baseCellSize, 20f, 50f, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
                
                showGridCoordinates = EditorGUILayout.Toggle("Show Coordinates", showGridCoordinates);
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Grid Alan Görsel Gösterimi
                DrawGridVisualization();
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(selectedLevel);
                    AssetDatabase.SaveAssets();
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGridVisualization()
        {
            // Grid görselleştirmesi
            int satirSayisi = selectedLevel.gridSatirSayisi;
            int sutunSayisi = selectedLevel.gridSutunSayisi;
            
            // Hücre boyutu ve aralık (zoom ve base size ile)
            float cellSize = baseCellSize * gridZoom;
            float cellSpacing = 4f * gridZoom; // Hücreler arası boşluk
            float totalCellSize = cellSize + cellSpacing;
            
            // Grid alanı hesapla
            float totalWidth = (sutunSayisi * totalCellSize) - cellSpacing;
            float totalHeight = (satirSayisi * totalCellSize) - cellSpacing;
            
            // Padding
            float padding = 10f;
            
            // Scroll view için içerik boyutu
            Vector2 contentSize = new Vector2(totalWidth + (padding * 2), totalHeight + (padding * 2) + 20f);
            
            // Scroll view başlat (sabit yükseklik, horizontal scroll için)
            gridScrollPosition = EditorGUILayout.BeginScrollView(gridScrollPosition, 
                false, true, // horizontal ve vertical scroll
                GUILayout.Height(350), GUILayout.ExpandWidth(true));
            
            // İçerik alanı
            Rect contentRect = GUILayoutUtility.GetRect(contentSize.x, contentSize.y, GUILayout.ExpandWidth(false));
            
            // Mouse wheel zoom kontrolü
            Event currentEvent = Event.current;
            if (contentRect.Contains(currentEvent.mousePosition) && currentEvent.type == EventType.ScrollWheel && !currentEvent.shift)
            {
                float zoomDelta = -currentEvent.delta.y * 0.1f;
                gridZoom = Mathf.Clamp(gridZoom + zoomDelta, 0.5f, 2f);
                currentEvent.Use();
                Repaint();
            }
            
            // Arka plan
            EditorGUI.DrawRect(contentRect, new Color(0.12f, 0.12f, 0.12f, 1f));
            
            // Grid çizgileri
            Handles.BeginGUI();
            
            // Grid bilgisi (üstte kompakt)
            Handles.color = Color.white;
            string gridInfo = $"{satirSayisi} x {sutunSayisi} Grid | Zoom: {gridZoom:F2}x";
            GUI.Label(new Rect(contentRect.x + 5, contentRect.y + 2, 300, 18), 
                gridInfo, EditorStyles.miniLabel);
            
            // Grid'i ortala
            float gridAreaX = contentRect.x + padding + (contentRect.width - totalWidth - (padding * 2)) * 0.5f;
            float gridAreaY = contentRect.y + 20f + padding; // Label için sadece 20f
            
            // Her hücreyi buton gibi çiz (0,0 sol alt köşe - y eksenini ters çevir)
            for (int row = 0; row < satirSayisi; row++)
            {
                for (int col = 0; col < sutunSayisi; col++)
                {
                    float cellX = gridAreaX + (col * totalCellSize);
                    // Y eksenini ters çevir: row=0 en altta, row=satirSayisi-1 en üstte
                    float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                    Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                    
                    // Hücre rengi (buton stili)
                    Color cellColor = new Color(0.25f, 0.25f, 0.25f, 1f);
                    
                    // Hücre arka planı (buton gibi)
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Gölge efekti (alt ve sağ kenarlar)
                    Color shadowColor = new Color(0f, 0f, 0f, 0.3f);
                    Rect shadowRect = new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width, cellRect.height);
                    EditorGUI.DrawRect(shadowRect, shadowColor);
                    
                    // Parlak kenarlık (üst ve sol)
                    Color highlightColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    Handles.color = highlightColor;
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y, 0),
                        new Vector3(cellRect.xMax, cellRect.y, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.y, 0),
                        new Vector3(cellRect.x, cellRect.yMax, 0)
                    );
                    
                    // Koyu kenarlık (alt ve sağ)
                    Color darkBorderColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                    Handles.color = darkBorderColor;
                    Handles.DrawLine(
                        new Vector3(cellRect.xMax, cellRect.y, 0),
                        new Vector3(cellRect.xMax, cellRect.yMax, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(cellRect.x, cellRect.yMax, 0),
                        new Vector3(cellRect.xMax, cellRect.yMax, 0)
                    );
                    
                    // Koordinat gösterimi
                    if (showGridCoordinates && cellSize > 25f)
                    {
                        GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel);
                        coordStyle.alignment = TextAnchor.UpperLeft;
                        coordStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                        coordStyle.fontSize = Mathf.Max(8, (int)(8 * gridZoom));
                        GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width, 12), 
                            $"{col},{row}", coordStyle);
                    }
                }
            }
            
            // Tıklama ve sürükleme kontrolü (scroll view koordinatlarına göre)
            if (currentEvent.type == EventType.MouseDown && !currentEvent.alt && contentRect.Contains(currentEvent.mousePosition))
            {
                bool isRightClick = currentEvent.button == 1;
                Vector2 mousePos = currentEvent.mousePosition;
                
                // Frame mode için özel işlem
                if (currentPaintingMode == PaintingMode.Frame && !isRightClick && selectedFrameShape != null)
                {
                    isFrameDragging = true;
                    for (int row = 0; row < satirSayisi; row++)
                    {
                        for (int col = 0; col < sutunSayisi; col++)
                        {
                            float cellX = gridAreaX + (col * totalCellSize);
                            // Y eksenini ters çevir
                            float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                            Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                            
                            if (cellRect.Contains(mousePos))
                            {
                                framePreviewPosition = new Vector2Int(col, row);
                                currentEvent.Use();
                                Repaint();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    isDragging = true;
                    paintedCellsThisDrag.Clear();
                    
                    for (int row = 0; row < satirSayisi; row++)
                    {
                        for (int col = 0; col < sutunSayisi; col++)
                        {
                            float cellX = gridAreaX + (col * totalCellSize);
                            // Y eksenini ters çevir
                            float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                            Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                            
                            if (cellRect.Contains(mousePos))
                            {
                                Vector2Int cellPos = new Vector2Int(col, row);
                                
                                if (isRightClick)
                                {
                                    OnCellRightClicked(row, col);
                                }
                                else
                                {
                                    OnCellClicked(row, col);
                                    if (currentPaintingMode == PaintingMode.Stickman)
                                    {
                                        paintedCellsThisDrag.Add(cellPos);
                                    }
                                }
                                currentEvent.Use();
                                Repaint();
                                break;
                            }
                        }
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseDrag && contentRect.Contains(currentEvent.mousePosition))
            {
                Vector2 mousePos = currentEvent.mousePosition;
                
                // Frame mode drag
                if (currentPaintingMode == PaintingMode.Frame && isFrameDragging && selectedFrameShape != null)
                {
                    for (int row = 0; row < satirSayisi; row++)
                    {
                        for (int col = 0; col < sutunSayisi; col++)
                        {
                            float cellX = gridAreaX + (col * totalCellSize);
                            // Y eksenini ters çevir
                            float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                            Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                            
                            if (cellRect.Contains(mousePos))
                            {
                                framePreviewPosition = new Vector2Int(col, row);
                                currentEvent.Use();
                                Repaint();
                                break;
                            }
                        }
                    }
                }
                // Stickman mode drag
                else if (currentPaintingMode == PaintingMode.Stickman && isDragging && !currentEvent.alt)
                {
                    bool isRightClick = currentEvent.button == 1;
                    
                    for (int row = 0; row < satirSayisi; row++)
                    {
                        for (int col = 0; col < sutunSayisi; col++)
                        {
                            float cellX = gridAreaX + (col * totalCellSize);
                            // Y eksenini ters çevir
                            float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                            Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                            
                            if (cellRect.Contains(mousePos))
                            {
                                Vector2Int cellPos = new Vector2Int(col, row);
                                
                                // Aynı hücreyi tekrar boyamayı önle
                                if (!paintedCellsThisDrag.Contains(cellPos))
                                {
                                    if (isRightClick)
                                    {
                                        OnCellRightClicked(row, col);
                                    }
                                    else
                                    {
                                        OnCellClicked(row, col);
                                        paintedCellsThisDrag.Add(cellPos);
                                    }
                                    Repaint();
                                }
                                currentEvent.Use();
                                break;
                            }
                        }
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                // Frame mode - mouse bırakınca yerleştir
                if (currentPaintingMode == PaintingMode.Frame && isFrameDragging && selectedFrameShape != null)
                {
                    PlaceFrameAt(framePreviewPosition);
                    isFrameDragging = false;
                    currentEvent.Use();
                    Repaint();
                }
                else
                {
                    isDragging = false;
                    paintedCellsThisDrag.Clear();
                }
            }
            
            // Hücreleri renklendir (colored cells)
            DrawCellColors(gridAreaX, gridAreaY, cellSize, totalCellSize, cellSpacing, satirSayisi, sutunSayisi);
            
            Handles.EndGUI();
            
            // Scroll view bitir
            EditorGUILayout.EndScrollView();
        }

        private void DrawColorPalette()
        {
            EditorGUILayout.LabelField("Color Palette:", EditorStyles.miniLabel);
            EditorGUILayout.Space(3);
            
            // Tüm ColorType'ları yatayda göster
            ColorType[] colorTypes = (ColorType[])System.Enum.GetValues(typeof(ColorType));
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5);
            
            for (int i = 0; i < colorTypes.Length; i++)
            {
                ColorType colorType = colorTypes[i];
                Color color = GetColorFromColorType(colorType);
                
                // Seçili renk kontrolü
                bool isSelected = selectedColorType == colorType;
                
                // Renk butonu
                Rect buttonRect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40), GUILayout.Height(40));
                
                // Gölge efekti
                Rect shadowRect = new Rect(buttonRect.x + 2, buttonRect.y + 2, buttonRect.width, buttonRect.height);
                EditorGUI.DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.3f));
                
                // Arka plan (renk)
                EditorGUI.DrawRect(buttonRect, color);
                
                // Seçili ise kalın beyaz kenarlık
                if (isSelected)
                {
                    Handles.BeginGUI();
                    Handles.color = Color.white;
                    // Üst kenar
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.y, 0),
                        new Vector3(buttonRect.xMax, buttonRect.y, 0)
                    );
                    // Alt kenar
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.yMax, 0),
                        new Vector3(buttonRect.xMax, buttonRect.yMax, 0)
                    );
                    // Sol kenar
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.y, 0),
                        new Vector3(buttonRect.x, buttonRect.yMax, 0)
                    );
                    // Sağ kenar
                    Handles.DrawLine(
                        new Vector3(buttonRect.xMax, buttonRect.y, 0),
                        new Vector3(buttonRect.xMax, buttonRect.yMax, 0)
                    );
                    Handles.EndGUI();
                }
                else
                {
                    // Seçili değilse ince gri kenarlık
                    Handles.BeginGUI();
                    Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.y, 0),
                        new Vector3(buttonRect.xMax, buttonRect.y, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.yMax, 0),
                        new Vector3(buttonRect.xMax, buttonRect.yMax, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(buttonRect.x, buttonRect.y, 0),
                        new Vector3(buttonRect.x, buttonRect.yMax, 0)
                    );
                    Handles.DrawLine(
                        new Vector3(buttonRect.xMax, buttonRect.y, 0),
                        new Vector3(buttonRect.xMax, buttonRect.yMax, 0)
                    );
                    Handles.EndGUI();
                }
                
                // Tıklama kontrolü
                Event currentEvent = Event.current;
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    if (buttonRect.Contains(currentEvent.mousePosition))
                    {
                        selectedColorType = colorType;
                        currentEvent.Use();
                        Repaint();
                    }
                }
                
                // Hover efekti
                if (buttonRect.Contains(currentEvent.mousePosition) && !isSelected)
                {
                    EditorGUI.DrawRect(buttonRect, new Color(1f, 1f, 1f, 0.2f));
                }
                
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private Color GetColorFromColorType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.None:
                    return new Color(0.3f, 0.3f, 0.3f, 0.5f); // Gri, yarı saydam (boyanmamış)
                case ColorType.Red:
                    return new Color(0.8f, 0.2f, 0.2f, 1f);
                case ColorType.Blue:
                    return new Color(0.2f, 0.4f, 0.8f, 1f);
                case ColorType.Green:
                    return new Color(0.2f, 0.8f, 0.2f, 1f);
                case ColorType.Yellow:
                    return new Color(0.9f, 0.9f, 0.2f, 1f);
                case ColorType.Purple:
                    return new Color(0.6f, 0.2f, 0.8f, 1f);
                case ColorType.Orange:
                    return new Color(1f, 0.5f, 0f, 1f);
                case ColorType.Cyan:
                    return new Color(0.2f, 0.8f, 0.8f, 1f);
                case ColorType.Pink:
                    return new Color(1f, 0.4f, 0.7f, 1f);
                default:
                    return new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        private bool CanPlaceFrameAt(Vector2Int gridPosition, FrameShape shape, int gridWidth, int gridHeight)
        {
            if (selectedLevel == null || shape == null || shape.cells == null) return false;
            
            // Frame'in tüm hücrelerini kontrol et
            foreach (var cellOffset in shape.cells)
            {
                Vector2Int worldPos = gridPosition + cellOffset;
                
                // Grid sınırları dışında mı?
                if (worldPos.x < 0 || worldPos.x >= gridWidth || worldPos.y < 0 || worldPos.y >= gridHeight)
                {
                    return false; // Grid dışına çıkıyor
                }
                
                // Bu hücrede başka bir frame var mı?
                if (selectedLevel.framePlacements != null)
                {
                    foreach (var existingPlacement in selectedLevel.framePlacements)
                    {
                        if (existingPlacement == null || existingPlacement.shape == null || existingPlacement.shape.cells == null) continue;
                        
                        foreach (var existingCellOffset in existingPlacement.shape.cells)
                        {
                            Vector2Int existingWorldPos = existingPlacement.gridPosition + existingCellOffset;
                            
                            if (existingWorldPos == worldPos)
                            {
                                return false; // Çakışma var
                            }
                        }
                    }
                }
            }
            
            return true; // Geçerli
        }
        
        private void PlaceFrameAt(Vector2Int gridPosition)
        {
            if (selectedLevel == null || selectedFrameShape == null) return;
            
            // Geçerlilik kontrolü
            if (!CanPlaceFrameAt(gridPosition, selectedFrameShape, selectedLevel.gridSutunSayisi, selectedLevel.gridSatirSayisi))
            {
                Debug.LogWarning($"Frame cannot be placed at ({gridPosition.x}, {gridPosition.y}) - collision or out of bounds");
                return;
            }
            
            // Frame yerleştir
            if (selectedLevel.framePlacements == null)
            {
                selectedLevel.framePlacements = new List<FramePlacement>();
            }
            
            selectedLevel.framePlacements.Add(new FramePlacement(gridPosition, selectedFrameShape));
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"Frame placed at ({gridPosition.x}, {gridPosition.y}) with shape {selectedFrameShape.shapeName}");
        }
        
        private void RemoveFrameAt(Vector2Int clickedPosition)
        {
            if (selectedLevel == null || selectedLevel.framePlacements == null) return;
            
            // Tıklanan hücre frame'in herhangi bir hücresinde mi kontrol et
            FramePlacement toRemove = null;
            foreach (var placement in selectedLevel.framePlacements)
            {
                if (placement == null || placement.shape == null || placement.shape.cells == null) continue;
                
                // Frame'in tüm hücrelerini kontrol et
                foreach (var cellOffset in placement.shape.cells)
                {
                    Vector2Int worldPos = placement.gridPosition + cellOffset;
                    
                    // Tıklanan pozisyon bu frame'in bir hücresinde mi?
                    if (worldPos == clickedPosition)
                    {
                        toRemove = placement;
                        break;
                    }
                }
                
                if (toRemove != null) break;
            }
            
            if (toRemove != null)
            {
                selectedLevel.framePlacements.Remove(toRemove);
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
                Debug.Log($"Frame removed (clicked at {clickedPosition.x}, {clickedPosition.y}, frame pivot was at {toRemove.gridPosition.x}, {toRemove.gridPosition.y})");
            }
        }
        
        private void OnCellClicked(int row, int col)
        {
            if (selectedLevel == null) return;
            
            Vector2Int cellPosition = new Vector2Int(col, row);
            
            if (currentPaintingMode == PaintingMode.Frame)
            {
                // Frame mode - mouse drag ile çalışıyor, burada işlem yok
                return;
            }
            else if (currentPaintingMode == PaintingMode.Stickman)
            {
                // Cell data ekle veya güncelle
                CellData existingCell = selectedLevel.cellDataList.FirstOrDefault(c => c.gridPosition == cellPosition);
                
                if (existingCell != null)
                {
                    existingCell.colorType = selectedColorType;
                }
                else
                {
                    selectedLevel.cellDataList.Add(new CellData(cellPosition, selectedColorType));
                }
                
                EditorUtility.SetDirty(selectedLevel);
                AssetDatabase.SaveAssets();
                Debug.Log($"Cell painted: ({col}, {row}) with color {selectedColorType}");
            }
        }

        private void OnCellRightClicked(int row, int col)
        {
            if (selectedLevel == null) return;
            
            Vector2Int cellPosition = new Vector2Int(col, row);
            
            if (currentPaintingMode == PaintingMode.Frame)
            {
                // Frame mode - sağ tık ile frame sil
                RemoveFrameAt(cellPosition);
                return;
            }
            
            // Cell data'da varsa colorType'ı None yap, yoksa None ile ekle
            CellData existingCell = selectedLevel.cellDataList.FirstOrDefault(c => c.gridPosition == cellPosition);
            if (existingCell != null)
            {
                existingCell.colorType = ColorType.None;
            }
            else
            {
                // Eğer hücre listede yoksa ekle (None ile)
                selectedLevel.cellDataList.Add(new CellData(cellPosition, ColorType.None));
            }
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"Cell color cleared (set to None): ({col}, {row})");
        }

        private void DrawCellColors(float gridAreaX, float gridAreaY, float cellSize, float totalCellSize, float cellSpacing, int satirSayisi, int sutunSayisi)
        {
            // Önce bir dictionary oluştur (hızlı arama için)
            Dictionary<Vector2Int, ColorType> cellColorMap = new Dictionary<Vector2Int, ColorType>();
            if (selectedLevel.cellDataList != null)
            {
                foreach (var cellData in selectedLevel.cellDataList)
                {
                    cellColorMap[cellData.gridPosition] = cellData.colorType;
                }
            }
            
            // Tüm grid hücrelerini çiz (y eksenini ters çevir - 0,0 sol alt)
            for (int row = 0; row < satirSayisi; row++)
            {
                for (int col = 0; col < sutunSayisi; col++)
                {
                    Vector2Int pos = new Vector2Int(col, row);
                    ColorType cellColorType = cellColorMap.ContainsKey(pos) ? cellColorMap[pos] : ColorType.None;
                    
                    float cellX = gridAreaX + (col * totalCellSize);
                    // Y eksenini ters çevir: row=0 en altta
                    float cellY = gridAreaY + ((satirSayisi - 1 - row) * totalCellSize);
                    Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                    
                    Color cellColor = GetColorFromColorType(cellColorType);
                    EditorGUI.DrawRect(cellRect, cellColor);
                }
            }
            
            // Frame preview çiz (mouse basılı tutulurken)
            if (currentPaintingMode == PaintingMode.Frame && selectedFrameShape != null && isFrameDragging)
            {
                DrawFramePreview(gridAreaX, gridAreaY, cellSize, totalCellSize, satirSayisi, sutunSayisi);
            }
            
            // Yerleştirilmiş frame'leri çiz
            DrawPlacedFrames(gridAreaX, gridAreaY, cellSize, totalCellSize, cellSpacing, satirSayisi, sutunSayisi);
        }
        
        private void DrawFramePreview(float gridAreaX, float gridAreaY, float cellSize, float totalCellSize, int satirSayisi, int sutunSayisi)
        {
            if (selectedFrameShape == null || selectedFrameShape.cells == null) return;
            
            // Geçerlilik kontrolü
            bool isValid = CanPlaceFrameAt(framePreviewPosition, selectedFrameShape, sutunSayisi, satirSayisi);
            
            // Geçerli ise sarı, geçersiz ise kırmızı
            Color previewColor = isValid ? 
                new Color(1f, 1f, 0f, 0.5f) : // Sarı, yarı saydam
                new Color(1f, 0f, 0f, 0.6f);  // Kırmızı, yarı saydam
            
            foreach (var cellOffset in selectedFrameShape.cells)
            {
                Vector2Int worldPos = framePreviewPosition + cellOffset;
                
                // Hücre pozisyonunu hesapla (y eksenini ters çevir)
                float cellX = gridAreaX + (worldPos.x * totalCellSize);
                float cellY = gridAreaY + ((satirSayisi - 1 - worldPos.y) * totalCellSize);
                Rect cellRect = new Rect(cellX, cellY, cellSize, cellSize);
                
                // Grid sınırları içinde mi kontrol et
                bool isInBounds = worldPos.x >= 0 && worldPos.x < sutunSayisi && worldPos.y >= 0 && worldPos.y < satirSayisi;
                
                Color cellColor = previewColor;
                
                if (isInBounds)
                {
                    // Eğer bu hücrede başka bir frame varsa kırmızı göster
                    if (selectedLevel.framePlacements != null)
                    {
                        foreach (var existingPlacement in selectedLevel.framePlacements)
                        {
                            if (existingPlacement == null || existingPlacement.shape == null || existingPlacement.shape.cells == null) continue;
                            
                            foreach (var existingCellOffset in existingPlacement.shape.cells)
                            {
                                Vector2Int existingWorldPos = existingPlacement.gridPosition + existingCellOffset;
                                
                                if (existingWorldPos == worldPos)
                                {
                                    cellColor = new Color(1f, 0f, 0f, 0.6f); // Kırmızı (çakışma)
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Grid dışındaki hücreler her zaman kırmızı
                    cellColor = new Color(1f, 0f, 0f, 0.6f); // Kırmızı (grid dışı)
                }
                
                // Hücreyi çiz (sadece görünür alandaysa)
                float gridTotalWidth = sutunSayisi * totalCellSize;
                float gridTotalHeight = satirSayisi * totalCellSize;
                Rect gridBounds = new Rect(gridAreaX, gridAreaY, gridTotalWidth, gridTotalHeight);
                
                if (gridBounds.Overlaps(cellRect) || isInBounds)
                {
                    EditorGUI.DrawRect(cellRect, cellColor);
                }
            }
        }
        
        private void DrawPlacedFrames(float gridAreaX, float gridAreaY, float cellSize, float totalCellSize, float cellSpacing, int satirSayisi, int sutunSayisi)
        {
            if (selectedLevel.framePlacements == null) return;
            
            Color frameFillColor = new Color(0f, 1f, 1f, 0.3f); // Cyan, cam gibi, 0.3f opaklık
            Color frameOutlineColor = Color.white; // Beyaz, 1 opaklık
            float outlineThickness = 3f; // Kalın kenarlık
            
            foreach (var placement in selectedLevel.framePlacements)
            {
                if (placement == null || placement.shape == null || placement.shape.cells == null) continue;
                
                // Önce frame'in tamamının grid içinde olup olmadığını kontrol et
                bool isFrameValid = true;
                HashSet<Vector2Int> frameCells = new HashSet<Vector2Int>();
                
                foreach (var cellOffset in placement.shape.cells)
                {
                    Vector2Int worldPos = placement.gridPosition + cellOffset;
                    
                    // Grid sınırları dışında mı?
                    if (worldPos.x < 0 || worldPos.x >= sutunSayisi || worldPos.y < 0 || worldPos.y >= satirSayisi)
                    {
                        isFrameValid = false; // Frame grid dışına taşıyor
                        break; // Kontrolü durdur, frame geçersiz
                    }
                    
                    frameCells.Add(worldPos);
                }
                
                // Eğer frame grid dışına taşıyorsa, hiç çizme
                if (!isFrameValid || frameCells.Count == 0) continue;
                
                // Frame'in iç kısmını çiz
                // Grid hücreleriyle aynı pozisyon ve boyutta çiz (cellSize kullan)
                // Kesintisiz görünüm için komşu hücreler arasındaki boşluğu da doldur
                float gridRightEdge = gridAreaX + (sutunSayisi * totalCellSize) - cellSpacing;
                float gridTopEdge = gridAreaY + (satirSayisi * totalCellSize) - cellSpacing;
                
                foreach (var worldPos in frameCells)
                {
                    // Hücre pozisyonunu hesapla (grid hücreleriyle aynı şekilde)
                    float cellX = gridAreaX + (worldPos.x * totalCellSize);
                    float cellY = gridAreaY + ((satirSayisi - 1 - worldPos.y) * totalCellSize);
                    
                    // Hücre boyutunu hesapla - kesintisiz görünüm için komşu hücreler arasındaki boşluğu da dahil et
                    float cellWidth = cellSize;
                    float cellHeight = cellSize;
                    
                    // Sağda komşu hücre var mı? Varsa boşluğu doldur
                    Vector2Int rightNeighbor = new Vector2Int(worldPos.x + 1, worldPos.y);
                    if (frameCells.Contains(rightNeighbor))
                    {
                        cellWidth = totalCellSize; // Komşu hücreyle birleş
                    }
                    
                    // Üstte komşu hücre var mı? Varsa boşluğu doldur
                    Vector2Int topNeighbor = new Vector2Int(worldPos.x, worldPos.y + 1);
                    if (frameCells.Contains(topNeighbor))
                    {
                        cellHeight = totalCellSize; // Komşu hücreyle birleş
                    }
                    
                    // Grid sınırlarını aşmamak için kontrol et
                    if (cellX + cellWidth > gridRightEdge)
                    {
                        cellWidth = Mathf.Max(0, gridRightEdge - cellX);
                    }
                    if (cellY + cellHeight > gridTopEdge)
                    {
                        cellHeight = Mathf.Max(0, gridTopEdge - cellY);
                    }
                    
                    // Hücreyi çiz (grid sınırları içinde kalacak şekilde)
                    if (cellWidth > 0 && cellHeight > 0)
                    {
                        Rect cellRect = new Rect(cellX, cellY, cellWidth, cellHeight);
                        EditorGUI.DrawRect(cellRect, frameFillColor);
                    }
                }
                
                // Frame'in dış kenarlığını çiz (beyaz, kalın)
                // Her hücrenin komşularını kontrol et, eğer komşu frame'in parçası değilse o kenar dış kenardır
                Handles.color = frameOutlineColor;
                
                // gridRightEdge ve gridTopEdge zaten yukarıda tanımlı, tekrar tanımlamaya gerek yok
                
                foreach (var worldPos in frameCells)
                {
                    // Hücre pozisyonunu hesapla (grid hücreleriyle aynı şekilde)
                    float cellX = gridAreaX + (worldPos.x * totalCellSize);
                    float cellY = gridAreaY + ((satirSayisi - 1 - worldPos.y) * totalCellSize);
                    
                    // Hücre boyutu her zaman cellSize (grid hücreleriyle aynı)
                    float cellWidth = cellSize;
                    float cellHeight = cellSize;
                    
                    // Grid sınırlarını aşmamak için kontrol et
                    if (cellX + cellWidth > gridRightEdge)
                    {
                        cellWidth = Mathf.Max(0, gridRightEdge - cellX);
                    }
                    if (cellY + cellHeight > gridTopEdge)
                    {
                        cellHeight = Mathf.Max(0, gridTopEdge - cellY);
                    }
                    
                    if (cellWidth <= 0 || cellHeight <= 0) continue;
                    
                    // Üst kenar (eğer üstte komşu hücre yoksa)
                    Vector2Int topNeighbor = new Vector2Int(worldPos.x, worldPos.y + 1);
                    if (!frameCells.Contains(topNeighbor))
                    {
                        float lineEndX = Mathf.Min(cellX + cellWidth, gridRightEdge);
                        Handles.DrawAAPolyLine(outlineThickness,
                            new Vector3(cellX, cellY, 0),
                            new Vector3(lineEndX, cellY, 0)
                        );
                    }
                    
                    // Alt kenar (eğer altta komşu hücre yoksa)
                    Vector2Int bottomNeighbor = new Vector2Int(worldPos.x, worldPos.y - 1);
                    if (!frameCells.Contains(bottomNeighbor))
                    {
                        float lineY = cellY + cellHeight;
                        float lineEndX = Mathf.Min(cellX + cellWidth, gridRightEdge);
                        Handles.DrawAAPolyLine(outlineThickness,
                            new Vector3(cellX, lineY, 0),
                            new Vector3(lineEndX, lineY, 0)
                        );
                    }
                    
                    // Sol kenar (eğer solda komşu hücre yoksa)
                    Vector2Int leftNeighbor = new Vector2Int(worldPos.x - 1, worldPos.y);
                    if (!frameCells.Contains(leftNeighbor))
                    {
                        float lineEndY = Mathf.Min(cellY + cellHeight, gridTopEdge);
                        Handles.DrawAAPolyLine(outlineThickness,
                            new Vector3(cellX, cellY, 0),
                            new Vector3(cellX, lineEndY, 0)
                        );
                    }
                    
                    // Sağ kenar (eğer sağda komşu hücre yoksa)
                    Vector2Int rightNeighbor = new Vector2Int(worldPos.x + 1, worldPos.y);
                    if (!frameCells.Contains(rightNeighbor))
                    {
                        float lineX = cellX + cellWidth;
                        float lineEndY = Mathf.Min(cellY + cellHeight, gridTopEdge);
                        if (lineX <= gridRightEdge)
                        {
                            Handles.DrawAAPolyLine(outlineThickness,
                                new Vector3(lineX, cellY, 0),
                                new Vector3(lineX, lineEndY, 0)
                            );
                        }
                    }
                }
            }
        }
        
        private void DrawFrameModeUI()
        {
            EditorGUILayout.LabelField("Frame Shapes:", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            // GameConfig'ten FrameShapes'i al
            FrameShapes frameShapes = null;
            if (gameConfig != null)
            {
                frameShapes = gameConfig.FrameShapes;
            }
            
            // FrameShapes bilgilendirmesi
            if (frameShapes == null)
            {
                EditorGUILayout.HelpBox("FrameShapes database not assigned in GameConfig. Please assign it in GameConfig asset.", MessageType.Warning);
                EditorGUILayout.Space(5);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("FrameShapes DB:", GUILayout.Width(120));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(frameShapes, typeof(FrameShapes), false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            
            // Shape seçimi
            if (frameShapes != null && frameShapes.shapes != null)
            {
                EditorGUILayout.LabelField("Select Shape:", EditorStyles.miniLabel);
                
                int shapeCount = frameShapes.shapes.Count;
                if (shapeCount > 0)
                {
                    string[] shapeNames = new string[shapeCount + 1];
                    shapeNames[0] = "None";
                    int selectedIndex = 0;
                    
                    for (int i = 0; i < shapeCount; i++)
                    {
                        var shape = frameShapes.shapes[i];
                        if (shape != null)
                        {
                            // Her zaman önce asset adını kontrol et, sonra shapeName'i kullan
                            string displayName = null;
                            string assetPath = AssetDatabase.GetAssetPath(shape);
                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                                if (!string.IsNullOrEmpty(fileName) && fileName != "NewFrameShape")
                                {
                                    displayName = fileName;
                                }
                            }
                            
                            // Eğer asset adı yoksa veya geçersizse, shapeName'i kullan
                            if (string.IsNullOrEmpty(displayName))
                            {
                                displayName = shape.shapeName;
                            }
                            
                            // Eğer hala boşsa veya "New Shape" ise, varsayılan isim kullan
                            if (string.IsNullOrEmpty(displayName) || displayName == "New Shape")
                            {
                                displayName = $"Shape {i + 1}";
                            }
                            
                            shapeNames[i + 1] = displayName;
                            if (shape == selectedFrameShape)
                            {
                                selectedIndex = i + 1;
                            }
                        }
                        else
                        {
                            shapeNames[i + 1] = $"Null Shape {i}";
                        }
                    }
                    
                    int newIndex = EditorGUILayout.Popup(selectedIndex, shapeNames);
                    if (newIndex != selectedIndex)
                    {
                        if (newIndex == 0)
                        {
                            selectedFrameShape = null;
                        }
                        else
                        {
                            if (newIndex - 1 < frameShapes.shapes.Count)
                            {
                                selectedFrameShape = frameShapes.shapes[newIndex - 1];
                            }
                        }
                        Repaint();
                    }
                    
                    if (selectedFrameShape != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.HelpBox($"Selected: {selectedFrameShape.shapeName}\n" +
                            $"Size: {selectedFrameShape.GetWidth()}x{selectedFrameShape.GetHeight()}\n" +
                            $"Cells: {selectedFrameShape.cells.Count}", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No shapes available. Create FrameShape assets and add them to FrameShapes.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("FrameShapes database not assigned. Create a FrameShapes asset and assign it.", MessageType.Warning);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Click and drag on grid to place frame shape. Right-click to remove frames.", MessageType.Info);
        }

        private void CreateGrid()
        {
            if (selectedLevel == null) return;
            
            int satirSayisi = selectedLevel.gridSatirSayisi;
            int sutunSayisi = selectedLevel.gridSutunSayisi;
            
            // Mevcut cellDataList'i temizle
            selectedLevel.cellDataList.Clear();
            
            // Yeni grid oluştur - tüm hücreleri varsayılan renk (None) ile doldur
            for (int row = 0; row < satirSayisi; row++)
            {
                for (int col = 0; col < sutunSayisi; col++)
                {
                    Vector2Int cellPosition = new Vector2Int(col, row);
                    selectedLevel.cellDataList.Add(new CellData(cellPosition, ColorType.None));
                }
            }
            
            EditorUtility.SetDirty(selectedLevel);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"✅ {satirSayisi}x{sutunSayisi} grid oluşturuldu! Tüm hücreler None (boyanmamış) olarak eklendi.");
            Repaint();
        }

        private void DrawObjectiveColumnsSection()
        {
            if (selectedLevel == null) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Objective Columns Foldout
            objectiveColumnsFoldout = EditorGUILayout.Foldout(objectiveColumnsFoldout, 
                "Objective Columns", true, EditorStyles.foldoutHeader);
            
            if (objectiveColumnsFoldout)
            {
                EditorGUILayout.Space(5);
                
                EditorGUI.BeginChangeCheck();
                
                // Yatay Sütun Sayısı
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Yatay Sütun Sayısı:", GUILayout.Width(120));
                int horizontalColumnCount = EditorGUILayout.IntField(selectedLevel.objectiveColumnGridRowCount > 0 && selectedLevel.objectiveColumns != null ? 
                    (selectedLevel.objectiveColumns.Count / selectedLevel.objectiveColumnGridRowCount) : 0, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(3);
                
                // Dikey Satır Sayısı (sütunların dikey düzenlemesi için)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Dikey Satır Sayısı:", GUILayout.Width(120));
                int gridRowCount = EditorGUILayout.IntField(selectedLevel.objectiveColumnGridRowCount, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                // Grid satır sayısını güncelle
                if (gridRowCount != selectedLevel.objectiveColumnGridRowCount)
                {
                    selectedLevel.objectiveColumnGridRowCount = Mathf.Max(1, gridRowCount);
                }
                
                // Yatay sütun sayısını güncelle
                horizontalColumnCount = Mathf.Max(1, horizontalColumnCount);
                
                // Toplam sütun sayısını hesapla (yatay sütun sayısı x dikey satır sayısı)
                int totalColumnsNeeded = horizontalColumnCount * selectedLevel.objectiveColumnGridRowCount;
                
                // Sütun sayısını güncelle
                if (selectedLevel.objectiveColumns == null)
                {
                    selectedLevel.objectiveColumns = new List<ObjectiveColumn>();
                }
                
                // Toplam sütun sayısına göre listeyi güncelle
                while (selectedLevel.objectiveColumns.Count < totalColumnsNeeded)
                {
                    selectedLevel.objectiveColumns.Add(new ObjectiveColumn());
                }
                while (selectedLevel.objectiveColumns.Count > totalColumnsNeeded)
                {
                    selectedLevel.objectiveColumns.RemoveAt(selectedLevel.objectiveColumns.Count - 1);
                }
                
                EditorGUILayout.Space(5);
                
                // Sütunlar için ColorType seçimi (grid düzeninde)
                if (totalColumnsNeeded > 0)
                {
                    EditorGUILayout.LabelField("Sütun Renkleri:", EditorStyles.boldLabel);
                    EditorGUILayout.Space(3);
                    
                    // Horizontal scroll view for columns (scroll position'ı kaydet)
                    objectiveColumnsHorizontalScroll = EditorGUILayout.BeginScrollView(objectiveColumnsHorizontalScroll, false, true, GUILayout.Height(300));
                    
                    // Grid düzeninde sütunları göster
                    for (int rowIndex = 0; rowIndex < selectedLevel.objectiveColumnGridRowCount; rowIndex++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        for (int colIndex = 0; colIndex < horizontalColumnCount; colIndex++)
                        {
                            // Grid index hesapla: row * horizontalColumnCount + col
                            int gridIndex = rowIndex * horizontalColumnCount + colIndex;
                            
                            if (gridIndex >= selectedLevel.objectiveColumns.Count)
                                break;
                            
                            var column = selectedLevel.objectiveColumns[gridIndex];
                            if (column == null)
                            {
                                column = new ObjectiveColumn();
                                selectedLevel.objectiveColumns[gridIndex] = column;
                            }
                            
                            // Her sütun için vertical layout (dikine dikdörtgen)
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(120), GUILayout.Height(280));
                            
                            EditorGUILayout.LabelField($"Sütun {gridIndex + 1} (R{rowIndex + 1}C{colIndex + 1})", EditorStyles.boldLabel);
                            EditorGUILayout.Space(3);
                            
                            // Her sütun için scroll position'ı al veya oluştur
                            if (!objectiveColumnScrollPositions.ContainsKey(gridIndex))
                            {
                                objectiveColumnScrollPositions[gridIndex] = Vector2.zero;
                            }
                            
                            // Sütun içindeki renkler için scroll view (scroll position'ı kullan)
                            objectiveColumnScrollPositions[gridIndex] = EditorGUILayout.BeginScrollView(
                                objectiveColumnScrollPositions[gridIndex], 
                                GUILayout.Height(200));
                            
                            // Mevcut renkleri göster
                            for (int colorIndex = 0; colorIndex < column.colors.Count; colorIndex++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                column.colors[colorIndex] = (ColorType)EditorGUILayout.EnumPopup(column.colors[colorIndex], GUILayout.Width(100));
                                
                                // Sil butonu
                                if (GUILayout.Button("X", GUILayout.Width(20)))
                                {
                                    Undo.RecordObject(selectedLevel, "Remove Color from Column");
                                    column.colors.RemoveAt(colorIndex);
                                    colorIndex--;
                                    EditorUtility.SetDirty(selectedLevel);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            
                            EditorGUILayout.EndScrollView();
                            
                            EditorGUILayout.Space(5);
                            
                            // Renk ekle butonu
                            if (GUILayout.Button("+ Renk Ekle", GUILayout.Height(25)))
                            {
                                Undo.RecordObject(selectedLevel, "Add Color to Column");
                                column.colors.Add(ColorType.Red);
                                EditorUtility.SetDirty(selectedLevel);
                            }
                        
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.Space(5);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(selectedLevel, "Objective Columns Changed");
                    EditorUtility.SetDirty(selectedLevel);
                    AssetDatabase.SaveAssets();
                }
            }
            
            EditorGUILayout.EndVertical();
        }

    }
}
