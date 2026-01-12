using System;
using System.Collections.Generic;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// Grid hücresindeki veri yapısı
    /// </summary>
    [Serializable]
    public class CellData
    {
        [SerializeField] public Vector2Int gridPosition;
        [SerializeField] public ColorType colorType;
        
        public CellData(Vector2Int position, ColorType color)
        {
            gridPosition = position;
            colorType = color;
        }
    }
    
    /// <summary>
    /// Objective Column (her sütun bir renk listesi)
    /// </summary>
    [Serializable]
    public class ObjectiveColumn
    {
        [SerializeField] public List<ColorType> colors = new List<ColorType>();
        
        public ObjectiveColumn()
        {
            colors = new List<ColorType>();
        }
    }
    
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Data/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Settings")] 
        [SerializeField] public string levelName;
        [SerializeField] public bool skipInLoop = false;
        [SerializeField] public LevelDifficultyType LevelDifficultyType = LevelDifficultyType.Normal;
        
        [Header("Grid Area")]
        [Tooltip("Grid row count")]
        [SerializeField] public int gridSatirSayisi = 5;
        
        [Tooltip("Grid column count")]
        [SerializeField] public int gridSutunSayisi = 5;
        
        [Header("Cell Data")]
        [Tooltip("Colored cells data")]
        [SerializeField] public List<CellData> cellDataList = new List<CellData>();
        
        [Header("Objective Columns")]
        [Tooltip("Objective columns grid row count (vertical arrangement of columns)")]
        [SerializeField] public int objectiveColumnGridRowCount = 1;
        
        [Tooltip("Objective columns (each column contains a list of color types)")]
        [SerializeField] public List<ObjectiveColumn> objectiveColumns = new List<ObjectiveColumn>();
        
        [Header("Frame Shapes")]
        [Tooltip("Frame shapes database")]
        [SerializeField] public FrameShapes frameShapes;
        
        [Tooltip("Level'da yerleştirilmiş frame'ler (pozisyon + shape referansı)")]
        [SerializeField] public List<FramePlacement> framePlacements = new List<FramePlacement>();
    }
    
    /// <summary>
    /// Grid'de yerleştirilmiş bir frame'in bilgisi
    /// </summary>
    [System.Serializable]
    public class FramePlacement
    {
        [Tooltip("Frame'in grid pozisyonu")]
        [SerializeField] public Vector2Int gridPosition;
        
        [Tooltip("Kullanılan shape'in referansı")]
        [SerializeField] public FrameShape shape;
        
        public FramePlacement(Vector2Int position, FrameShape frameShape)
        {
            gridPosition = position;
            shape = frameShape;
        }
    }
}