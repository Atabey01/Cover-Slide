using System.Collections.Generic;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// Frame shape tanımı - grid'de hangi hücrelerin dolu olduğunu tutar
    /// </summary>
    [CreateAssetMenu(fileName = "NewFrameShape", menuName = "Data/Frame Shape")]
    public class FrameShape : ScriptableObject
    {
        [Header("Shape Settings")]
        [Tooltip("Shape'in adı")]
        [SerializeField] public string shapeName = "New Shape";
        
        [Header("Editor Settings")]
        [Tooltip("Editor'da görüntülenen grid boyutu (sadece editor için)")]
        [SerializeField] public Vector2Int editorGridSize = new Vector2Int(5, 5);
        
        [Header("Shape Pattern")]
        [Tooltip("Shape'in kapladığı hücreler (grid pozisyonları, 0,0 merkez olarak)")]
        [SerializeField] public List<Vector2Int> cells = new List<Vector2Int>();
        
        /// <summary>
        /// Editor grid boyutunu al
        /// </summary>
        public Vector2Int GetEditorGridSize()
        {
            return editorGridSize;
        }
        
        /// <summary>
        /// Editor grid boyutunu set et
        /// </summary>
        public void SetEditorGridSize(Vector2Int size)
        {
            editorGridSize = size;
        }
        
        /// <summary>
        /// Shape'in genişliğini hesapla
        /// </summary>
        public int GetWidth()
        {
            if (cells == null || cells.Count == 0) return 0;
            
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            
            foreach (var cell in cells)
            {
                minX = Mathf.Min(minX, cell.x);
                maxX = Mathf.Max(maxX, cell.x);
            }
            
            return maxX - minX + 1;
        }
        
        /// <summary>
        /// Shape'in yüksekliğini hesapla
        /// </summary>
        public int GetHeight()
        {
            if (cells == null || cells.Count == 0) return 0;
            
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            
            foreach (var cell in cells)
            {
                minY = Mathf.Min(minY, cell.y);
                maxY = Mathf.Max(maxY, cell.y);
            }
            
            return maxY - minY + 1;
        }
        
        /// <summary>
        /// Belirli bir pozisyonda shape'in hücresi var mı kontrol et
        /// </summary>
        public bool HasCellAt(Vector2Int position)
        {
            return cells != null && cells.Contains(position);
        }
    }
}
