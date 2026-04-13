using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Machines;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// MouseInteractionSystem — Handles mouse input for grid-based interaction.
    /// Converts mouse position to grid coordinates, manages cursor visual, and fires events.
    /// </summary>
    public class MouseInteractionSystem : MonoBehaviour
    {
        public static MouseInteractionSystem Instance { get; private set; }
        
        [Header("Input Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask uiLayerMask;
        
        [Header("Visual Feedback")]
        [SerializeField] private Transform cursorVisual;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        [SerializeField] private Vector2Int hoveredGridPos;
        private Vector2Int highlightDimensions = Vector2Int.one;
        private Vector2Int highlightOrigin;
        private Vector2Int lastReportedGridPos = new Vector2Int(-999, -999);

        // Events
        public delegate void GridCellEvent(Vector2Int gridPos);
        public static event GridCellEvent OnGridCellClicked;
        public static event GridCellEvent OnGridCellHovered;
        
        public void SetHighlightDimensions(Vector2Int dim) => highlightDimensions = dim;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        void Update()
        {
            if (GridManager.Instance == null || mainCamera == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            
            Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(mouseWorldPos);
            bool isValid = GridManager.Instance.IsValidGridPosition(currentGridPos);

            // Resolve what the cursor is hovering over
            ResolveHoverTarget(currentGridPos, isValid);

            // Update cursor visual (position + scale)
            UpdateCursorVisual(currentGridPos, isValid);
            
            if (!isValid) return;

            // Hover event
            if (currentGridPos != lastReportedGridPos)
            {
                lastReportedGridPos = currentGridPos;
                OnGridCellHovered?.Invoke(currentGridPos);
            }
            
            // Click event
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
                OnGridCellClicked?.Invoke(currentGridPos);
        }

        /// <summary>
        /// Determines highlight origin and dimensions based on what's under the cursor.
        /// Checks grid occupant first, then falls back to a scene-wide machine footprint scan.
        /// </summary>
        private void ResolveHoverTarget(Vector2Int currentGridPos, bool isValid)
        {
            if (!isValid)
            {
                hoveredGridPos = currentGridPos;
                highlightDimensions = Vector2Int.one;
                highlightOrigin = currentGridPos;
                return;
            }

            // Check grid occupant
            GameObject occupant = GridManager.Instance.GetOccupantAt(currentGridPos);
            if (occupant != null)
            {
                MachineIDTag tag = occupant.GetComponent<MachineIDTag>();
                if (tag != null)
                {
                    SetHighlightFromTag(tag, currentGridPos);
                    return;
                }
            }

            // Fallback: scan all machines to see if any footprint covers this cell
            MachineIDTag foundTag = FindMachineAtCell(currentGridPos);
            if (foundTag != null)
            {
                SetHighlightFromTag(foundTag, currentGridPos);
                return;
            }

            // Default: single cell
            hoveredGridPos = currentGridPos;
            highlightDimensions = Vector2Int.one;
            highlightOrigin = currentGridPos;
        }

        private void SetHighlightFromTag(MachineIDTag tag, Vector2Int currentGridPos)
        {
            // For moving machines (AgriMech), use current position instead of origin
            AgriMech mech = tag.GetComponent<AgriMech>();
            Vector2Int basePos = mech != null ? mech.currentGridPos : tag.originGridPos;
            Vector2Int dims = tag.dimensions == Vector2Int.zero ? Vector2Int.one : tag.dimensions;

            hoveredGridPos = currentGridPos;
            highlightOrigin = basePos;
            highlightDimensions = dims;
        }

        /// <summary>
        /// Positions and scales the cursor visual to cover the highlighted area.
        /// For multi-tile machines, the cursor expands to cover the full footprint.
        /// </summary>
        private void UpdateCursorVisual(Vector2Int currentGridPos, bool isValid)
        {
            if (cursorVisual == null) return;

            bool hideDefault = PlacementManager.Instance != null && PlacementManager.Instance.IsPlacingModeActive();
            cursorVisual.gameObject.SetActive(isValid && !IsPointerOverUI() && !hideDefault);

            if (!isValid || hideDefault) return;

            float cSize = GridManager.Instance.CellSize;

            // Position at center of the highlighted footprint
            Vector3 originWorld = GridManager.Instance.GridToWorldPosition(highlightOrigin);
            Vector3 offset = new Vector3(
                (highlightDimensions.x - 1) * 0.5f * cSize,
                (highlightDimensions.y - 1) * 0.5f * cSize,
                0f);
            
            Vector3 targetPos = originWorld + offset;
            targetPos.z = -1f;
            cursorVisual.position = targetPos;

            // Scale to cover full footprint
            cursorVisual.localScale = new Vector3(highlightDimensions.x, highlightDimensions.y, 1f);
        }

        // ─── Static Utilities ───

        /// <summary>
        /// Finds any machine whose footprint covers the given cell, regardless of grid registration.
        /// Used when GetOccupantAt returns a non-machine object (e.g. a crop overlaying a machine).
        /// </summary>
        public static MachineIDTag FindMachineAtCell(Vector2Int cell)
        {
            foreach (MachineIDTag tag in FindObjectsOfType<MachineIDTag>())
            {
                AgriMech mech = tag.GetComponent<AgriMech>();
                Vector2Int basePos = mech != null ? mech.currentGridPos : tag.originGridPos;
                Vector2Int dims = tag.dimensions == Vector2Int.zero ? Vector2Int.one : tag.dimensions;

                if (cell.x >= basePos.x && cell.x < basePos.x + dims.x &&
                    cell.y >= basePos.y && cell.y < basePos.y + dims.y)
                {
                    return tag;
                }
            }
            return null;
        }

        bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        void OnDrawGizmos()
        {
            if (!showDebugGizmos || GridManager.Instance == null) return;
            if (!GridManager.Instance.IsValidGridPosition(highlightOrigin)) return;

            float cellSize = GridManager.Instance.CellSize;
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(highlightOrigin);
            Vector3 offset = new Vector3(
                (highlightDimensions.x - 1) * 0.5f * cellSize,
                (highlightDimensions.y - 1) * 0.5f * cellSize, 0f);
            Vector3 size = new Vector3(
                highlightDimensions.x * cellSize * 0.95f,
                highlightDimensions.y * cellSize * 0.95f, 0.1f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawCube(worldPos + offset, size);
        }
        
        public Vector2Int GetHoveredGridPosition() => hoveredGridPos;
    }
}
