using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// GridManager — Core singleton managing the 2D grid system.
    /// Handles cell occupation, position conversion, lighting, shadows, and exploration state.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Configuration")]
        [SerializeField] private Vector2Int gridDimensions = new Vector2Int(10, 10);
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f, 0.3f);

        // Cell state tracking
        private readonly Dictionary<Vector2Int, GameObject> occupiedCells = new Dictionary<Vector2Int, GameObject>();
        private readonly Dictionary<Vector2Int, List<GameObject>> microCells = new Dictionary<Vector2Int, List<GameObject>>();
        private readonly HashSet<Vector2Int> litCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> shadowCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> exploredCells = new HashSet<Vector2Int>();

        // ─── Properties ───
        public float CellSize => cellSize;
        public Vector2Int GridDimensions => gridDimensions;
        public Vector3 GridOrigin => gridOrigin;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Position Conversion ───

        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                gridOrigin.x + (gridPos.x + 0.5f) * cellSize,
                gridOrigin.y + (gridPos.y + 0.5f) * cellSize,
                gridOrigin.z);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize),
                Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize));
        }

        // ─── Cell Queries ───

        public bool IsValidGridPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridDimensions.x &&
                   pos.y >= 0 && pos.y < gridDimensions.y;
        }

        public bool IsPositionAvailable(Vector2Int pos) => IsValidGridPosition(pos) && !occupiedCells.ContainsKey(pos);

        public GameObject GetOccupantAt(Vector2Int pos)
        {
            occupiedCells.TryGetValue(pos, out GameObject occupant);
            return occupant;
        }

        public Dictionary<Vector2Int, GameObject> GetOccupiedCells() => occupiedCells;

        public List<GameObject> GetMicroOccupantsAt(Vector2Int pos)
        {
            return microCells.TryGetValue(pos, out var list) ? list : new List<GameObject>();
        }

        public List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            var neighbors = new List<Vector2Int>(4);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int n = pos + dir;
                if (IsValidGridPosition(n)) neighbors.Add(n);
            }
            return neighbors;
        }

        // ─── Cell Mutation ───

        public bool TryOccupyCell(Vector2Int pos, GameObject occupant)
        {
            if (!IsValidGridPosition(pos) || occupiedCells.ContainsKey(pos)) return false;
            occupiedCells[pos] = occupant;
            return true;
        }

        public void ReleaseCell(Vector2Int pos) => occupiedCells.Remove(pos);
        public void ClearAllCells() => occupiedCells.Clear();

        // ─── Lighting ───
        public void AddLightSource(Vector2Int pos) => litCells.Add(pos);
        public void RemoveLightSource(Vector2Int pos) => litCells.Remove(pos);
        public bool IsCellLit(Vector2Int pos) => litCells.Contains(pos);

        // ─── Shadows ───
        public void SetShadowCell(Vector2Int pos, bool isShadow)
        {
            if (isShadow) shadowCells.Add(pos); else shadowCells.Remove(pos);
        }
        public bool IsShadowCell(Vector2Int pos) => shadowCells.Contains(pos);

        // ─── Exploration (Fog of War) ───
        public void ExploreCell(Vector2Int pos) => exploredCells.Add(pos);
        public bool IsCellExplored(Vector2Int pos) => exploredCells.Contains(pos);

        // ─── Debug ───

        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            for (int x = 0; x < gridDimensions.x; x++)
            {
                for (int y = 0; y < gridDimensions.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    Gizmos.color = occupiedCells.ContainsKey(pos) ? occupiedColor : gizmoColor;
                    Gizmos.DrawCube(GridToWorldPosition(pos), new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 size = new Vector3(gridDimensions.x * cellSize, gridDimensions.y * cellSize, 0);
            Vector3 center = gridOrigin + size * 0.5f;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
