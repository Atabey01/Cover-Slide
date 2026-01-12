using UnityEngine;
using UnityEditor;
using DEV.Scripts.Data;

namespace DEV.Editor
{
    [CustomEditor(typeof(FrameShape))]
    public class FrameShapeEditor : UnityEditor.Editor
    {
        private const float CELL_SIZE = 25f;
        private const int MAX_GRID_SIZE = 20;
        private int gridWidth = 5;
        private int gridHeight = 5;
        private Vector2 scrollPosition;
        private bool[,] gridCells;

        private void OnEnable()
        {
            var shape = (FrameShape)target;

            // ShapeName'i her zaman asset dosyasının adından al (eğer asset path varsa)
            string assetPath = AssetDatabase.GetAssetPath(shape);
            if (!string.IsNullOrEmpty(assetPath))
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (!string.IsNullOrEmpty(fileName) && fileName != "NewFrameShape")
                {
                    // Asset dosyası adı varsa ve varsayılan değilse, shapeName'i güncelle
                    if (string.IsNullOrEmpty(shape.shapeName) || shape.shapeName == "New Shape" || shape.shapeName != fileName)
                    {
                        shape.shapeName = fileName;
                        EditorUtility.SetDirty(shape);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            
            // AssetDatabase'i refresh et (yeni shape'lerin görünmesi için)
            AssetDatabase.Refresh();

            // Grid boyutunu ScriptableObject'ten yükle (NotebookChaos yaklaşımı gibi)
            // Direkt field'a eriş (daha güvenilir)
            // SerializedObject kullanarak field'ı oku (daha güvenilir)
            SerializedObject serializedObject = new SerializedObject(shape);
            SerializedProperty gridSizeProperty = serializedObject.FindProperty("editorGridSize");
            
            if (gridSizeProperty != null && gridSizeProperty.vector2IntValue != Vector2Int.zero)
            {
                gridWidth = gridSizeProperty.vector2IntValue.x;
                gridHeight = gridSizeProperty.vector2IntValue.y;
            }
            else
            {
                // Fallback: direkt field'a eriş
                gridWidth = shape.editorGridSize.x;
                gridHeight = shape.editorGridSize.y;
            }

            // Eğer grid boyutu kaydedilmemişse (0,0) veya default değerse, hesapla
            // Ama eğer grid boyutu zaten ayarlanmışsa (0,0 değilse), onu kullan - KORU!
            if (gridWidth == 0 || gridHeight == 0)
            {
                if (shape.cells == null || shape.cells.Count == 0)
                {
                    // İlk yaratıldığında 5x5
                    gridWidth = 5;
                    gridHeight = 5;
                }
                else
                {
                    // Shape'de cells varsa, grid boyutunu shape'den hesapla
                    int minX = int.MaxValue, maxX = int.MinValue;
                    int minY = int.MaxValue, maxY = int.MinValue;

                    foreach (var cell in shape.cells)
                    {
                        minX = Mathf.Min(minX, cell.x);
                        maxX = Mathf.Max(maxX, cell.x);
                        minY = Mathf.Min(minY, cell.y);
                        maxY = Mathf.Max(maxY, cell.y);
                    }

                    // Shape sığacak minimum boyutu hesapla
                    int requiredWidth = maxX - minX + 3;
                    int requiredHeight = maxY - minY + 3;
                    
                    gridWidth = Mathf.Max(5, requiredWidth);
                    gridHeight = Mathf.Max(5, requiredHeight);
                }
                
                // Hesaplanan değerleri ScriptableObject'e kaydet (SerializedObject ile)
                SerializedObject serializedObjectSave = new SerializedObject(shape);
                SerializedProperty gridSizePropertySave = serializedObjectSave.FindProperty("editorGridSize");
                if (gridSizePropertySave != null)
                {
                    gridSizePropertySave.vector2IntValue = new Vector2Int(gridWidth, gridHeight);
                    serializedObjectSave.ApplyModifiedProperties();
                }
                else
                {
                    // Fallback: direkt field'a yaz
                    shape.editorGridSize = new Vector2Int(gridWidth, gridHeight);
                    EditorUtility.SetDirty(shape);
                }
                AssetDatabase.SaveAssets();
            }
            // Eğer grid boyutu zaten ayarlanmışsa (0,0 değilse), onu kullan - DEĞİŞTİRME!
            // Sadece shape sığmıyorsa genişlet (ama kullanıcının ayarladığı boyutu koru)
            // NOT: Grid boyutu kullanıcı tarafından ayarlanmışsa, onu koru - sadece minimum gereksinimleri karşıla

            InitializeGrid();
            LoadShapeToGrid(shape);
        }

        private void InitializeGrid()
        {
            gridCells = new bool[gridHeight, gridWidth];
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    gridCells[y, x] = false;
                }
            }
        }

        private void LoadShapeToGrid(FrameShape shape)
        {
            if (shape.cells == null || shape.cells.Count == 0) return;

            // Shape'i grid'e yükle (sol alt köşe pivot - 0,0)
            // En sol alt hücreyi bul ve grid'in sol alt köşesine (0,0) hizala
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            
            foreach (var cell in shape.cells)
            {
                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);
            }
            
            // Offset hesapla (sol alt köşeyi 0,0'a getir)
            int offsetX = -minX;
            int offsetY = -minY;

            foreach (var cell in shape.cells)
            {
                int x = cell.x + offsetX;
                int y = cell.y + offsetY;

                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    gridCells[y, x] = true;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var shape = (FrameShape)target;

            EditorGUI.BeginChangeCheck();

            // Shape Name
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape Settings", EditorStyles.boldLabel);
            shape.shapeName = EditorGUILayout.TextField("Shape Name", shape.shapeName);
            
            // Frame Type
            shape.frameType = (DEV.Scripts.Enums.FrameType)EditorGUILayout.EnumPopup("Frame Type", shape.frameType);

            EditorGUILayout.Space(10);

            // Grid Size Controls
            EditorGUILayout.LabelField("Grid Size", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width:", GUILayout.Width(60));
            int newWidth = EditorGUILayout.IntField(Mathf.Clamp(gridWidth, 1, MAX_GRID_SIZE), GUILayout.Width(60));
            EditorGUILayout.LabelField("Height:", GUILayout.Width(60));
            int newHeight = EditorGUILayout.IntField(Mathf.Clamp(gridHeight, 1, MAX_GRID_SIZE), GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            if (newWidth != gridWidth || newHeight != gridHeight)
            {
                gridWidth = Mathf.Clamp(newWidth, 1, MAX_GRID_SIZE);
                gridHeight = Mathf.Clamp(newHeight, 1, MAX_GRID_SIZE);
                
                // Grid boyutunu ScriptableObject'e kaydet (SerializedObject ile)
                SerializedObject serializedObjectChange = new SerializedObject(shape);
                SerializedProperty gridSizePropertyChange = serializedObjectChange.FindProperty("editorGridSize");
                if (gridSizePropertyChange != null)
                {
                    gridSizePropertyChange.vector2IntValue = new Vector2Int(gridWidth, gridHeight);
                    serializedObjectChange.ApplyModifiedProperties();
                }
                else
                {
                    // Fallback: direkt field'a yaz
                    shape.editorGridSize = new Vector2Int(gridWidth, gridHeight);
                    EditorUtility.SetDirty(shape);
                }
                AssetDatabase.SaveAssets();
                
                InitializeGrid();
                LoadShapeToGrid(shape);
            }

            EditorGUILayout.Space(5);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All"))
            {
                InitializeGrid();
                SaveGridToShape(shape);
            }

            if (GUILayout.Button("Center Shape"))
            {
                CenterShapeInGrid(shape);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Grid Editor
            EditorGUILayout.LabelField("Shape Pattern (Click cells to toggle)", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Grid çiz
            DrawGrid(shape);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Info
            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Cells: {shape.cells?.Count ?? 0}\n" +
                                    $"Size: {shape.GetWidth()}x{shape.GetHeight()}", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(shape);
            }
        }

        private void DrawGrid(FrameShape shape)
        {
            float totalWidth = gridWidth * CELL_SIZE;
            float totalHeight = gridHeight * CELL_SIZE;

            Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight, GUILayout.ExpandWidth(false));

            // Grid arka planı
            EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f, 1f));

            // Grid çizgileri
            Handles.BeginGUI();
            Handles.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            for (int x = 0; x <= gridWidth; x++)
            {
                float xPos = gridRect.x + (x * CELL_SIZE);
                Handles.DrawLine(new Vector3(xPos, gridRect.y, 0), new Vector3(xPos, gridRect.yMax, 0));
            }

            for (int y = 0; y <= gridHeight; y++)
            {
                float yPos = gridRect.y + (y * CELL_SIZE);
                Handles.DrawLine(new Vector3(gridRect.x, yPos, 0), new Vector3(gridRect.xMax, yPos, 0));
            }

            // Hücreleri çiz (sol alt köşe pivot - 0,0)
            int pivotX = 0;
            int pivotY = 0; // Sol alt köşe (grid'de y=0 en alt)

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Y eksenini ters çevir (Unity'de y=0 üstte, ama shape'de y=0 altta olmalı)
                    int displayY = gridHeight - 1 - y;
                    
                    Rect cellRect = new Rect(
                        gridRect.x + (x * CELL_SIZE) + 1,
                        gridRect.y + (displayY * CELL_SIZE) + 1,
                        CELL_SIZE - 2,
                        CELL_SIZE - 2
                    );

                    // Hücre rengi
                    if (gridCells[y, x])
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0.2f, 0.8f, 0.2f, 1f)); // Yeşil
                    }
                    else
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0.3f, 0.3f, 0.3f, 1f)); // Koyu gri
                    }

                    // Pivot işareti (sol alt köşe - 0,0)
                    if (x == pivotX && y == pivotY)
                    {
                        Handles.color = Color.yellow;
                        Handles.DrawLine(
                            new Vector3(cellRect.x, cellRect.y, 0),
                            new Vector3(cellRect.xMax, cellRect.yMax, 0)
                        );
                        Handles.DrawLine(
                            new Vector3(cellRect.xMax, cellRect.y, 0),
                            new Vector3(cellRect.x, cellRect.yMax, 0)
                        );
                    }

                    // Tıklama kontrolü
                    Event currentEvent = Event.current;
                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        if (cellRect.Contains(currentEvent.mousePosition))
                        {
                            // Tıklanan hücreyi bul (displayY'den gerçek y'ye çevir)
                            int clickedY = gridHeight - 1 - displayY;
                            gridCells[clickedY, x] = !gridCells[clickedY, x];
                            SaveGridToShape(shape);
                            currentEvent.Use();
                            Repaint();
                        }
                    }
                }
            }

            Handles.EndGUI();
        }

        private void SaveGridToShape(FrameShape shape)
        {
            if (shape.cells == null)
            {
                shape.cells = new System.Collections.Generic.List<Vector2Int>();
            }
            else
            {
                shape.cells.Clear();
            }

            // Sol alt köşe pivot (0,0) - grid'de y=0 en alt satır
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (gridCells[y, x])
                    {
                        // Sol alt köşe (0,0) pivot - direkt koordinatları kullan
                        // Grid'de y=0 en alt satır, shape'de de y=0 en alt olmalı
                        shape.cells.Add(new Vector2Int(x, y));
                    }
                }
            }

            EditorUtility.SetDirty(shape);
        }

        private void CenterShapeInGrid(FrameShape shape)
        {
            if (shape.cells == null || shape.cells.Count == 0) return;

            InitializeGrid();
            LoadShapeToGrid(shape);
        }
    }
}