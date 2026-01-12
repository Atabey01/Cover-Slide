using System.Collections.Generic;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// Tüm frame shape'leri tutan container
    /// </summary>
    [CreateAssetMenu(fileName = "FrameShapes", menuName = "Data/Frame Shapes")]
    public class FrameShapes : ScriptableObject
    {
        [Header("Frame Shapes Collection")]
        [Tooltip("Tüm frame shape'lerin listesi")]
        [SerializeField] public List<FrameShape> shapes = new List<FrameShape>();
        
        /// <summary>
        /// Shape adına göre shape bul
        /// </summary>
        public FrameShape GetShapeByName(string name)
        {
            if (shapes == null) return null;
            
            return shapes.Find(s => s != null && s.shapeName == name);
        }
        
        /// <summary>
        /// Index'e göre shape al
        /// </summary>
        public FrameShape GetShape(int index)
        {
            if (shapes == null || index < 0 || index >= shapes.Count) return null;
            return shapes[index];
        }
    }
}
