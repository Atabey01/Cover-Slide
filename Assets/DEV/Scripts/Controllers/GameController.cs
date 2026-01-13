using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Gameplay;
using DEV.Scripts.Interfaces;
using DEV.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace DEV.Scripts.Controllers
{
    public class GameController
    {
        private const float CELL_SIZE = 1f; // World space size of each grid cell
        private const float CELL_SPACING = 0.0f; // Spacing between cells

        public GameObject LevelParent;
        public GameObject GridParent;
        public GameObject ObjectsParent;
        public GameObject StickmanParent;

        private GameConfig _gameConfig;
        private Dictionary<Vector2Int, GridObject> _gridObjects = new Dictionary<Vector2Int, GridObject>();
        private Dictionary<Vector2Int, Stickman> _stickmans = new Dictionary<Vector2Int, Stickman>();

        public void StartNewLevel(LevelData levelData)
        {
            _gameConfig = Factory.GetGameConfig();

            if (_gameConfig == null || _gameConfig.GameAssetsConfig == null)
            {
                Debug.LogError("GameController: GameConfig or GameAssetsConfig not found!");
                return;
            }

            CreateParents();
            CreateGrids(levelData);
            CreateStickmans(levelData);
        }

        private void CreateParents()
        {
            LevelParent = new GameObject("Level - " + (DataSaver.GetLevelId() + 1));

            GridParent = new GameObject("Grid");
            GridParent.transform.parent = LevelParent.transform;

            ObjectsParent = new GameObject("Objects");
            ObjectsParent.transform.parent = LevelParent.transform;

            StickmanParent = new GameObject("Stickmans");
            StickmanParent.transform.parent = LevelParent.transform;
        }

        private void CreateGrids(LevelData levelData)
        {
            if (levelData == null || _gameConfig?.GameAssetsConfig?.GridObjectPrefab == null)
            {
                Debug.LogError("GameController.CreateGrids: LevelData or GridObjectPrefab is null!");
                return;
            }

            int rowCount = levelData.gridSatirSayisi;
            int columnCount = levelData.gridSutunSayisi;

            // Calculate grid dimensions
            float gridWidth = (columnCount * (CELL_SIZE + CELL_SPACING)) - CELL_SPACING;
            float gridHeight = (rowCount * (CELL_SIZE + CELL_SPACING)) - CELL_SPACING;

            // GridParent stays at Vector3.zero
            GridParent.transform.position = Vector3.zero;

            // Create CellData dictionary (for fast access)
            Dictionary<Vector2Int, ColorType> cellDataDict = new Dictionary<Vector2Int, ColorType>();
            if (levelData.cellDataList != null)
            {
                foreach (var cellData in levelData.cellDataList)
                {
                    if (cellData != null)
                    {
                        cellDataDict[cellData.gridPosition] = cellData.colorType;
                    }
                }
            }

            // Create all grid cells in local space
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    Vector2Int gridPos = new Vector2Int(col, row);

                    // Get colorType from CellData (None if not found)
                    ColorType colorType = cellDataDict.ContainsKey(gridPos)
                        ? cellDataDict[gridPos]
                        : ColorType.None;

                    // Calculate local position (0,0 as bottom-left corner)
                    // X: centered (negative half width + column offset)
                    // Z: row 0 at bottom (0), larger rows go negative (upward in world)
                    float localX = -(gridWidth * 0.5f) + (col * (CELL_SIZE + CELL_SPACING)) + (CELL_SIZE * 0.5f);
                    float localZ = 0 - (rowCount - 1) + (row * (CELL_SIZE + CELL_SPACING));
                    Vector3 localPos = new Vector3(localX, 0f, localZ);

                    // Create GridObject using Factory with pooling
                    GridObject gridObj = Factory.Create<GridObject>(
                        _gameConfig.GameAssetsConfig.GridObjectPrefab.gameObject,
                        GridParent.transform,
                        usePooling: true
                    );

                    if (gridObj != null)
                    {
                        gridObj.transform.localPosition = localPos;
                        gridObj.name = $"GridCell_{col}_{row}";
                        _gridObjects[gridPos] = gridObj;

                        gridObj.Initialize(row, col, colorType, _gameConfig);
                    }
                }
            }

            // Create frames (if FrameShapes exists)
            if (levelData.framePlacements != null && levelData.framePlacements.Count > 0)
            {
                if (_gameConfig?.FrameShapes != null)
                {
                    CreateFrames(levelData);
                }
                else
                {
                    Debug.LogWarning("GameController.CreateGrids: FramePlacements exist but FrameShapes is null!");
                }
            }
        }

        private void CreateFrames(LevelData levelData)
        {
            if (_gameConfig?.GameAssetsConfig?.GridObjectPrefab == null)
            {
                Debug.LogWarning("GameController.CreateFrames: GridObjectPrefab is null!");
                return;
            }

            // Calculate grid dimensions for frame positioning
            int rowCount = levelData.gridSatirSayisi;
            int columnCount = levelData.gridSutunSayisi;
            float gridWidth = (columnCount * (CELL_SIZE + CELL_SPACING)) - CELL_SPACING;

            // Create a special parent for frames (as child of GridParent to use local space)
            GameObject framesParent = new GameObject("Frames");
            framesParent.transform.parent = GridParent.transform;
            framesParent.transform.localPosition = Vector3.zero;

            // Track frame cells to avoid duplicates
            HashSet<Vector2Int> frameCells = new HashSet<Vector2Int>();

            foreach (var framePlacement in levelData.framePlacements)
            {
                if (framePlacement == null || framePlacement.shape == null || framePlacement.shape.cells == null)
                    continue;

                // Create each cell of the frame
                foreach (var cellOffset in framePlacement.shape.cells)
                {
                    Vector2Int worldGridPos = framePlacement.gridPosition + cellOffset;

                    // Skip if already processed (multiple frames can overlap)
                    if (frameCells.Contains(worldGridPos))
                        continue;

                    frameCells.Add(worldGridPos);

                    // Calculate grid dimensions for frame positioning
                    int frameRowCount = levelData.gridSatirSayisi;

                    // Calculate local position (same as grid cells)
                    float localX = -(gridWidth * 0.5f) + (worldGridPos.x * (CELL_SIZE + CELL_SPACING)) +
                                   (CELL_SIZE * 0.5f);
                    float localZ = frameRowCount - 1 - (worldGridPos.y * (CELL_SIZE + CELL_SPACING)) -
                                   (CELL_SIZE * 0.5f);
                    Vector3 localPos = new Vector3(localX, 0.1f, localZ); // Slightly elevated

                    // Create frame cell using Factory with pooling (using GridObject prefab for now)
                    GridObject frameCell = Factory.Create<GridObject>(
                        _gameConfig.GameAssetsConfig.GridObjectPrefab.gameObject,
                        framesParent.transform,
                        usePooling: true
                    );

                    if (frameCell != null)
                    {
                        frameCell.transform.localPosition = localPos;
                        frameCell.name = $"FrameCell_{worldGridPos.x}_{worldGridPos.y}";

                        // Optionally differentiate frame cells visually
                        // For example, change material or add custom component
                    }
                }
            }
        }

        private void CreateStickmans(LevelData levelData)
        {
            if (levelData == null || _gameConfig?.GameAssetsConfig?.StickmanPrefab == null)
            {
                Debug.LogError("GameController.CreateStickmans: LevelData or StickmanPrefab is null!");
                return;
            }

            // Iterate through grid objects and create stickmans for non-None color types
            foreach (var kvp in _gridObjects)
            {
                Vector2Int gridPos = kvp.Key;
                GridObject gridObj = kvp.Value;

                // Skip if colorType is None
                if (gridObj.ColorType == ColorType.None)
                    continue;

                // Use grid cell's position (slightly elevated)
                Vector3 gridPos3D = gridObj.transform.position;
                Vector3 stickmanPos = new Vector3(gridPos3D.x, gridPos3D.y, gridPos3D.z);

                // Create stickman using Factory with pooling
                Stickman stickman = Factory.Create<Stickman>(
                    _gameConfig.GameAssetsConfig.StickmanPrefab.gameObject,
                    StickmanParent.transform,
                    usePooling: true
                );

                if (stickman != null)
                {
                    stickman.transform.position = stickmanPos;
                    stickman.name = $"Stickman_{gridObj.ColorType}";

                    // Initialize stickman with grid cell's colorType
                    stickman.Initialize(gridObj.ColorType, _gameConfig);
                    gridObj.Stickman = stickman;

                    _stickmans[gridPos] = stickman;
                }
            }
        }

        public void MouseDown(GameObject clickedGameObject)
        {
            // Generic usage example:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // var box = clickedGameObject?.GetComponent<Box>();
            // if (cube != null) { /* Cube operations */ }
            // if (box != null) { /* Box operations */ }
        }

        public void MouseUp(GameObject clickedGameObject)
        {
            // Generic usage example:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube operations */ }
        }

        public void MouseDrag(GameObject clickedGameObject)
        {
            // Generic usage example:
            // var cube = clickedGameObject?.GetComponent<Cube>();
            // if (cube != null) { /* Cube operations */ }
        }

        public void LevelDestroy()
        {
            // Destroy all tracked objects using Factory (pooled objects will be despawned)
            Factory.DestroyAll<GridObject>(usePooling: true);
            Factory.DestroyAll<Stickman>(usePooling: true);

            // Destroy LevelParent (which contains all children)
            if (LevelParent != null)
            {
                Object.Destroy(LevelParent);
            }

            // Clear dictionaries
            _gridObjects.Clear();
            _stickmans.Clear();

            // Reset parent references
            LevelParent = null;
            GridParent = null;
            ObjectsParent = null;
            StickmanParent = null;
        }

        public void Dispose()
        {
            LevelDestroy();
            _gameConfig = null;
        }
    }
}