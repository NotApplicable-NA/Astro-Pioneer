using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// Manager untuk sistem Grid 2D.
    /// Grid scalable, tapi area playable dibatasi 10x10 (Starter Hull) sesuai GDD v2.1
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("Ukuran grid cell dalam Unity units (1 = 32 pixels dengan PPU 32)")]
        [SerializeField] private float cellSize = 1f;
        
        [Tooltip("Origin point grid (bottom-left corner)")]
        [SerializeField] private Vector2 gridOrigin = Vector2.zero;
        
        [Header("Playable Area (Starter Hull)")]
        [Tooltip("Area aktif yang bisa digunakan player (10x10 sesuai GDD)")]
        [SerializeField] private int playableWidth = 10;
        [SerializeField] private int playableHeight = 10;
        
        [Header("Debug")]
        [Tooltip("Tampilkan gizmo grid di Scene view")]
        [SerializeField] private bool showGridGizmo = true;
        
        [Tooltip("Warna untuk area playable")]
        [SerializeField] private Color playableAreaColor = new Color(0f, 1f, 0f, 0.2f);
        
        [Tooltip("Warna untuk area terkunci")]
        [SerializeField] private Color lockedAreaColor = new Color(1f, 0f, 0f, 0.2f);
        
        // Grid data structure: Dictionary untuk fleksibilitas (bisa expand nanti)
        private Dictionary<Vector2Int, GridCell> gridCells;
        
        // Singleton instance
        public static GridManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[GridManager] Multiple instances detected! Destroying duplicate.", this);
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            InitializeGrid();
        }
        
        /// <summary>
        /// Inisialisasi grid system
        /// </summary>
        private void InitializeGrid()
        {
            gridCells = new Dictionary<Vector2Int, GridCell>();
            
            // Pre-allocate playable area
            for (int x = 0; x < playableWidth; x++)
            {
                for (int y = 0; y < playableHeight; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    gridCells[gridPos] = new GridCell
                    {
                        gridPosition = gridPos,
                        isPlayable = true,
                        isOccupied = false
                    };
                }
            }
            
            Debug.Log($"[GridManager] Grid initialized. Playable area: {playableWidth}x{playableHeight}");
        }
        
        /// <summary>
        /// Convert world position ke grid position
        /// </summary>
        public Vector2Int GetGridPosition(Vector3 worldPos)
        {
            Vector2 localPos = worldPos - (Vector3)gridOrigin;
            int x = Mathf.FloorToInt(localPos.x / cellSize);
            int y = Mathf.FloorToInt(localPos.y / cellSize);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Convert grid position ke world position (center of cell)
        /// </summary>
        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            float worldX = gridOrigin.x + (gridPos.x * cellSize) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (gridPos.y * cellSize) + (cellSize * 0.5f);
            
            return new Vector3(worldX, worldY, 0f);
        }
        
        /// <summary>
        /// Cek apakah grid position berada di area playable
        /// </summary>
        public bool IsPositionPlayable(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < playableWidth &&
                   gridPos.y >= 0 && gridPos.y < playableHeight;
        }
        
        /// <summary>
        /// Cek apakah grid position tersedia (playable dan tidak occupied)
        /// </summary>
        public bool IsPositionAvailable(Vector2Int gridPos)
        {
            if (!IsPositionPlayable(gridPos))
                return false;
            
            if (gridCells.TryGetValue(gridPos, out GridCell cell))
            {
                return !cell.isOccupied;
            }
            
            return false;
        }
        
        /// <summary>
        /// Set cell sebagai occupied/unoccupied
        /// </summary>
        public void SetCellOccupied(Vector2Int gridPos, bool occupied)
        {
            if (gridCells.TryGetValue(gridPos, out GridCell cell))
            {
                cell.isOccupied = occupied;
            }
            else if (IsPositionPlayable(gridPos))
            {
                // Create new cell jika belum ada
                gridCells[gridPos] = new GridCell
                {
                    gridPosition = gridPos,
                    isPlayable = true,
                    isOccupied = occupied
                };
            }
            else
            {
                Debug.LogWarning($"[GridManager] Attempted to set cell outside playable area: {gridPos}");
            }
        }
        
        /// <summary>
        /// Get cell data
        /// </summary>
        public GridCell GetCell(Vector2Int gridPos)
        {
            if (gridCells.TryGetValue(gridPos, out GridCell cell))
            {
                return cell;
            }
            
            return null;
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showGridGizmo) return;
            
            // Draw playable area
            Gizmos.color = playableAreaColor;
            Vector3 playableSize = new Vector3(playableWidth * cellSize, playableHeight * cellSize, 0.1f);
            Vector3 playableCenter = (Vector3)gridOrigin + new Vector3(
                playableWidth * cellSize * 0.5f,
                playableHeight * cellSize * 0.5f,
                0f
            );
            Gizmos.DrawCube(playableCenter, playableSize);
            
            // Draw grid lines untuk playable area
            Gizmos.color = Color.green;
            for (int x = 0; x <= playableWidth; x++)
            {
                Vector3 start = (Vector3)gridOrigin + new Vector3(x * cellSize, 0f, 0f);
                Vector3 end = (Vector3)gridOrigin + new Vector3(x * cellSize, playableHeight * cellSize, 0f);
                Gizmos.DrawLine(start, end);
            }
            
            for (int y = 0; y <= playableHeight; y++)
            {
                Vector3 start = (Vector3)gridOrigin + new Vector3(0f, y * cellSize, 0f);
                Vector3 end = (Vector3)gridOrigin + new Vector3(playableWidth * cellSize, y * cellSize, 0f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
    
    /// <summary>
    /// Data structure untuk setiap cell di grid
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        public Vector2Int gridPosition;
        public bool isPlayable;
        public bool isOccupied;
        // Bisa ditambah data lain nanti (crop reference, tile type, dll)
    }
}


