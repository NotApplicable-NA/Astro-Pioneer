using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Machines;
using AstroPioneer.Core;
using AstroPioneer.Data;

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

            // V25.1 Data-Driven Hover Detection (No Physics2D)
            hoveredGridPos = currentGridPos;
            highlightDimensions = Vector2Int.one;
            highlightOrigin = currentGridPos;

            if (GridManager.Instance != null && StructureRegistry.Instance != null)
            {
                ushort id = GridManager.Instance.GetStructureAt(currentGridPos);
                if (id != GameConstants.STRUCTURE_EMPTY)
                {
                    StructureData data = StructureRegistry.Instance.Get(id);
                    if (data != null && (data.isMachine || data.isCrop))
                    {
                        highlightDimensions = data.dimensions;
                        // Note: For multi-tile structures, finding the true origin purely from data 
                        // without reading ComplexState means the highlight will start at the hovered cell.
                        // To fix this fully, a "RootPos" should be stored in ComplexState.
                        return;
                    }
                }
            }

            // Check for simulated bots (Entities)
            if (AstroPioneer.Machines.Automation.BotSimulationManager.Instance != null)
            {
                var bots = AstroPioneer.Machines.Automation.BotSimulationManager.Instance.GetAllBots();
                for (int i = 0; i < bots.Count; i++)
                {
                    var bot = bots[i];
                    Vector2Int botPos = new Vector2Int(Mathf.FloorToInt(bot.currentPos.x), Mathf.FloorToInt(bot.currentPos.y));
                    if (botPos == currentGridPos)
                    {
                        highlightDimensions = new Vector2Int(2, 2); // Default AgriMech size
                        highlightOrigin = botPos;
                        return;
                    }
                }
            }
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
