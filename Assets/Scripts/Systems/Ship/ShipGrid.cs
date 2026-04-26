using UnityEngine;
using System;
using System.Collections.Generic;
using AstroPioneer.Data;

namespace AstroPioneer.Systems.Ship
{
    public enum ShipCellState
    {
        Locked,     // Not yet unlocked (expansion zone)
        Empty,      // Unlocked and available for placement
        Occupied    // Has a machine/furniture placed
    }

    [System.Serializable]
    public struct ShipCell
    {
        public ShipCellState state;
        public ShipRoom room;          // Which room this cell belongs to
        public GameObject occupant;    // Machine/furniture placed here

        public bool IsPlaceable => state == ShipCellState.Empty;
    }

    /// <summary>
    /// Ship interior grid system. Manages expandable grid with rooms and machine placement.
    /// Separate from farm GridManager — this is for the ship interior scene.
    /// </summary>
    public class ShipGrid : MonoBehaviour
    {
        public static ShipGrid Instance { get; private set; }

        [Header("Grid Configuration")]
        [Tooltip("Maximum grid dimensions (full ship at max tier)")]
        [SerializeField] private Vector2Int maxGridSize = new Vector2Int(20, 20);

        [Tooltip("Cell size in Unity units (PPU 16)")]
        [SerializeField] private float cellSize = 1.0f;

        [Tooltip("World position of grid origin (bottom-left)")]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Starting Layout")]
        [Tooltip("Initial unlocked area (starter hull)")]
        [SerializeField] private Vector2Int startingSize = new Vector2Int(6, 6);
        [SerializeField] private Vector2Int startingOffset = new Vector2Int(7, 7);

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color emptyColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0.5f, 0f, 0.3f);

        // Grid data
        private ShipCell[,] grid;

        // Events
        public event Action<Vector2Int> OnCellChanged;
        public event Action<ShipRoom, Vector2Int> OnRoomUnlocked;

        // Properties
        public Vector2Int MaxGridSize => maxGridSize;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeGrid();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Initialization ──────────────────────────

        private void InitializeGrid()
        {
            grid = new ShipCell[maxGridSize.x, maxGridSize.y];

            // Initialize all cells as locked
            for (int x = 0; x < maxGridSize.x; x++)
            {
                for (int y = 0; y < maxGridSize.y; y++)
                {
                    grid[x, y].state = ShipCellState.Locked;
                }
            }

            // Unlock starting area
            for (int x = startingOffset.x; x < startingOffset.x + startingSize.x; x++)
            {
                for (int y = startingOffset.y; y < startingOffset.y + startingSize.y; y++)
                {
                    if (IsWithinBounds(new Vector2Int(x, y)))
                    {
                        grid[x, y].state = ShipCellState.Empty;
                    }
                }
            }
        }

        // ── Position Conversion ─────────────────────

        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            float worldX = gridOrigin.x + (gridPos.x * cellSize) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (gridPos.y * cellSize) + (cellSize * 0.5f);
            return new Vector3(worldX, worldY, gridOrigin.z);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
            int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        // ── Cell Queries ────────────────────────────

        public bool IsWithinBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < maxGridSize.x &&
                   pos.y >= 0 && pos.y < maxGridSize.y;
        }

        public ShipCell? GetCell(Vector2Int pos)
        {
            if (!IsWithinBounds(pos)) return null;
            return grid[pos.x, pos.y];
        }

        public void SetCellState(Vector2Int pos, ShipCellState newState)
        {
            if (IsWithinBounds(pos))
                grid[pos.x, pos.y].state = newState;
        }

        public ShipCellState GetCellState(Vector2Int pos)
        {
            if (!IsWithinBounds(pos)) return ShipCellState.Locked;
            return grid[pos.x, pos.y].state;
        }

        public bool IsPlaceable(Vector2Int pos)
        {
            if (!IsWithinBounds(pos)) return false;
            return grid[pos.x, pos.y].IsPlaceable;
        }

        /// <summary>
        /// Check if a multi-cell area is fully placeable (for machines wider than 1x1).
        /// </summary>
        public bool IsAreaPlaceable(Vector2Int origin, Vector2Int size)
        {
            for (int x = origin.x; x < origin.x + size.x; x++)
            {
                for (int y = origin.y; y < origin.y + size.y; y++)
                {
                    if (!IsPlaceable(new Vector2Int(x, y)))
                        return false;
                }
            }
            return true;
        }

        // ── Placement ───────────────────────────────

        /// <summary>
        /// Place a machine/object on the ship grid. Returns true if successful.
        /// </summary>
        public bool TryPlace(Vector2Int pos, GameObject occupant, Vector2Int objectSize = default)
        {
            if (objectSize == default) objectSize = Vector2Int.one;

            if (!IsAreaPlaceable(pos, objectSize))
            {
                return false;
            }

            // Occupy all cells
            for (int x = pos.x; x < pos.x + objectSize.x; x++)
            {
                for (int y = pos.y; y < pos.y + objectSize.y; y++)
                {
                    grid[x, y].state = ShipCellState.Occupied;
                    grid[x, y].occupant = occupant;
                }
            }

            // Position the object
            occupant.transform.position = GridToWorldPosition(pos);

            OnCellChanged?.Invoke(pos);
            return true;
        }

        /// <summary>
        /// Remove a placed object from the grid.
        /// </summary>
        public bool TryRemove(Vector2Int pos, Vector2Int objectSize = default)
        {
            if (objectSize == default) objectSize = Vector2Int.one;

            if (!IsWithinBounds(pos)) return false;
            if (grid[pos.x, pos.y].state != ShipCellState.Occupied) return false;

            GameObject occupant = grid[pos.x, pos.y].occupant;

            for (int x = pos.x; x < pos.x + objectSize.x; x++)
            {
                for (int y = pos.y; y < pos.y + objectSize.y; y++)
                {
                    if (IsWithinBounds(new Vector2Int(x, y)))
                    {
                        grid[x, y].state = ShipCellState.Empty;
                        grid[x, y].occupant = null;
                    }
                }
            }

            OnCellChanged?.Invoke(pos);
            return true;
        }

        public GameObject GetOccupantAt(Vector2Int pos)
        {
            if (!IsWithinBounds(pos)) return null;
            return grid[pos.x, pos.y].occupant;
        }

        // ── Room / Expansion ────────────────────────

        /// <summary>
        /// Unlock a rectangular area as a room.
        /// </summary>
        public bool UnlockRoom(ShipRoom room, Vector2Int origin)
        {
            // Validate all cells are currently locked
            for (int x = origin.x; x < origin.x + room.size.x; x++)
            {
                for (int y = origin.y; y < origin.y + room.size.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!IsWithinBounds(pos))
                    {
                        return false;
                    }
                    if (grid[x, y].state != ShipCellState.Locked)
                    {
                        return false;
                    }
                }
            }

            // Unlock cells and assign room
            for (int x = origin.x; x < origin.x + room.size.x; x++)
            {
                for (int y = origin.y; y < origin.y + room.size.y; y++)
                {
                    grid[x, y].state = ShipCellState.Empty;
                    grid[x, y].room = room;
                }
            }

            OnRoomUnlocked?.Invoke(room, origin);
            return true;
        }

        /// <summary>
        /// Get all unlocked cells count.
        /// </summary>
        public int GetUnlockedCellCount()
        {
            int count = 0;
            for (int x = 0; x < maxGridSize.x; x++)
                for (int y = 0; y < maxGridSize.y; y++)
                    if (grid[x, y].state != ShipCellState.Locked)
                        count++;
            return count;
        }

        // ── Save/Load Support ───────────────────────

        /// <summary>
        /// Export grid state for save system.
        /// </summary>
        public ShipGridSaveData ExportSaveData()
        {
            var data = new ShipGridSaveData();
            data.cells = new List<ShipCellSaveData>();

            for (int x = 0; x < maxGridSize.x; x++)
            {
                for (int y = 0; y < maxGridSize.y; y++)
                {
                    var cell = grid[x, y];
                    if (cell.state != ShipCellState.Locked)
                    {
                        data.cells.Add(new ShipCellSaveData
                        {
                            x = x,
                            y = y,
                            state = cell.state,
                            roomID = cell.room != null ? cell.room.roomID : ""
                        });
                    }
                }
            }

            return data;
        }

        // ── Debug ───────────────────────────────────

        void OnDrawGizmos()
        {
            if (!showDebugGizmos || grid == null) return;

            for (int x = 0; x < maxGridSize.x; x++)
            {
                for (int y = 0; y < maxGridSize.y; y++)
                {
                    var cell = grid[x, y];
                    if (cell.state == ShipCellState.Locked) continue; // Skip locked for clarity

                    Vector3 worldPos = GridToWorldPosition(new Vector2Int(x, y));

                    Gizmos.color = cell.state == ShipCellState.Occupied ? occupiedColor : emptyColor;
                    Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                }
            }
        }
    }

    // ── Save Data Structures ────────────────────

    [System.Serializable]
    public class ShipGridSaveData
    {
        public List<ShipCellSaveData> cells;
    }

    [System.Serializable]
    public class ShipCellSaveData
    {
        public int x, y;
        public ShipCellState state;
        public string roomID;
    }
}
