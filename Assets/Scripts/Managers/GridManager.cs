using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// GridManager - Singleton untuk manage 2D grid system.
    /// Handles grid-based placement, position conversion, dan cell occupation.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        
        [Header("Grid Configuration")]
        [Tooltip("Grid dimensions in cells (10x10 = Starter Hull)")]
        [SerializeField] private Vector2Int gridDimensions = new Vector2Int(10, 10);
        
        [Tooltip("Cell size in Unity units (1 unit = 32 pixels, PPU 32)")]
        [SerializeField] private float cellSize = 1.0f;
        
        [Tooltip("World position of grid origin (bottom-left corner)")]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f, 0.3f);
        
        // Cell occupation tracking
        private Dictionary<Vector2Int, GameObject> occupiedCells = new Dictionary<Vector2Int, GameObject>();
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Convert grid position to world position
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            float worldX = gridOrigin.x + (gridPos.x * cellSize) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (gridPos.y * cellSize) + (cellSize * 0.5f);
            return new Vector3(worldX, worldY, gridOrigin.z);
        }
        
        /// <summary>
        /// Convert world position to grid position
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float gridX = (worldPos.x - gridOrigin.x) / cellSize;
            float gridY = (worldPos.y - gridOrigin.y) / cellSize;
            
            // Round to nearest integer
            int x = Mathf.FloorToInt(gridX);
            int y = Mathf.FloorToInt(gridY);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Check if grid position is valid (within playable area)
        /// </summary>
        public bool IsValidGridPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridDimensions.x &&
                   gridPos.y >= 0 && gridPos.y < gridDimensions.y;
        }
        
        /// <summary>
        /// Check if position is available (not occupied)
        /// </summary>
        public bool IsPositionAvailable(Vector2Int gridPos)
        {
            if (!IsValidGridPosition(gridPos))
                return false;
            
            return !occupiedCells.ContainsKey(gridPos);
        }
        
        /// <summary>
        /// Try to occupy a cell. Returns true if successful.
        /// </summary>
        public bool TryOccupyCell(Vector2Int gridPos, GameObject occupant)
        {
            if (!IsValidGridPosition(gridPos))
            {
                Debug.LogWarning($"[GridManager] Invalid grid position: {gridPos}", this);
                return false;
            }
            
            if (occupiedCells.ContainsKey(gridPos))
            {
                Debug.LogWarning($"[GridManager] Position {gridPos} already occupied by {occupiedCells[gridPos].name}", this);
                return false;
            }
            
            occupiedCells[gridPos] = occupant;
            return true;
        }
        
        /// <summary>
        /// Release a cell (remove occupation)
        /// </summary>
        public void ReleaseCell(Vector2Int gridPos)
        {
            if (occupiedCells.ContainsKey(gridPos))
            {
                occupiedCells.Remove(gridPos);
            }
        }
        
        /// <summary>
        /// Get occupant at grid position
        /// </summary>
        public GameObject GetOccupantAt(Vector2Int gridPos)
        {
            occupiedCells.TryGetValue(gridPos, out GameObject occupant);
            return occupant;
        }
        
        /// <summary>
        /// Get all neighbors of a grid position (4-directional)
        /// </summary>
        public List<Vector2Int> GetNeighbors(Vector2Int gridPos)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = gridPos + dir;
                if (IsValidGridPosition(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Clear all occupied cells (for testing/reset)
        /// </summary>
        public void ClearAllCells()
        {
            occupiedCells.Clear();
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // Draw grid cells
            for (int x = 0; x < gridDimensions.x; x++)
            {
                for (int y = 0; y < gridDimensions.y; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorldPosition(gridPos);
                    
                    // Check if occupied
                    bool isOccupied = occupiedCells.ContainsKey(gridPos);
                    Gizmos.color = isOccupied ? occupiedColor : gizmoColor;
                    
                    // Draw cell
                    Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                }
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw grid boundaries
            Gizmos.color = Color.yellow;
            Vector3 bottomLeft = gridOrigin;
            Vector3 bottomRight = gridOrigin + new Vector3(gridDimensions.x * cellSize, 0, 0);
            Vector3 topLeft = gridOrigin + new Vector3(0, gridDimensions.y * cellSize, 0);
            Vector3 topRight = gridOrigin + new Vector3(gridDimensions.x * cellSize, gridDimensions.y * cellSize, 0);
            
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}
