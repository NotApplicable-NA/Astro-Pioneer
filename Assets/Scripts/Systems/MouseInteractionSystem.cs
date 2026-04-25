using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Machines;
using AstroPioneer.Core;

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
        
        // Zero-Allocation Buffer
        private readonly Collider2D[] overlapResults = new Collider2D[16];

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

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (GridManager.Instance == null || mainCamera == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            
            Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(mouseWorldPos);
            // In infinite DOD grid, all positions are technically valid if a chunk can be loaded,
            // but we can bounds-check if we want. For now, assume true.
            bool isValid = true; 

            // Resolve what the cursor is hovering over
            ResolveHoverTarget(currentGridPos, isValid);

            // Update cursor visual (position + scale)
            UpdateCursorVisual(currentGridPos, isValid);
            
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

        private void ResolveHoverTarget(Vector2Int currentGridPos, bool isValid)
        {
            if (!isValid)
            {
                hoveredGridPos = currentGridPos;
                highlightDimensions = Vector2Int.one;
                highlightOrigin = currentGridPos;
                return;
            }

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            int hitCount = Physics2D.OverlapPointNonAlloc(mouseWorldPos, overlapResults);

            // In DOD, visual prefabs spawned by ChunkRenderer will have MachineIDTag and a Collider2D.
            // AgriMech (IEntity) also has a Collider2D.
            for(int i = 0; i < hitCount; i++)
            {
                Collider2D hit = overlapResults[i];
                MachineIDTag tag = hit.GetComponentInParent<MachineIDTag>();
                if (tag != null)
                {
                    SetHighlightFromTag(tag, currentGridPos);
                    return;
                }

                AgriMech mech = hit.GetComponentInParent<AgriMech>();
                if (mech != null)
                {
                    hoveredGridPos = currentGridPos;
                    highlightOrigin = EntityManager.GetGridBelowEntity(mech.transform.position);
                    highlightDimensions = mech.dimensions;
                    return;
                }
            }

            // Default: single cell if hovering over dirt or empty structural cell
            hoveredGridPos = currentGridPos;
            highlightDimensions = Vector2Int.one;
            highlightOrigin = currentGridPos;
        }

        private void SetHighlightFromTag(MachineIDTag tag, Vector2Int currentGridPos)
        {
            // For moving machines (AgriMech), use current position instead of origin
            AgriMech mech = tag.GetComponent<AgriMech>();
            Vector2Int basePos = mech != null ? EntityManager.GetGridBelowEntity(mech.transform.position) : tag.originGridPos;
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

        bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        void OnDrawGizmos()
        {
            if (!showDebugGizmos || GridManager.Instance == null) return;

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
