using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// MouseInteractionSystem - Handles mouse input untuk grid-based interaction.
    /// Converts mouse clicks to grid positions dan trigger events.
    /// </summary>
    public class MouseInteractionSystem : MonoBehaviour
    {
        public static MouseInteractionSystem Instance { get; private set; }
        
        [Header("Input Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask uiLayerMask;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Vector2Int hoveredGridPos;
        
        // Events
        public delegate void GridCellEvent(Vector2Int gridPos);
        public static event GridCellEvent OnGridCellClicked;
        public static event GridCellEvent OnGridCellHovered;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy entire GameObject, not just component
                return;
            }
            Instance = this;
            
            // Auto-assign main camera if not set
            if (mainCamera == null)
                mainCamera = Camera.main;
        }
        
        void Update()
        {
            // Guard: Check if GridManager exists
            if (GridManager.Instance == null)
            {
                // Don't spam - only log once per frame if needed
                return;
            }
            
            // Get mouse world position
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            
            // Convert to grid position
            Vector2Int currentGridPos = GridManager.Instance.WorldToGridPosition(mouseWorldPos);
            
            // Check if valid position
            bool isValid = GridManager.Instance.IsValidGridPosition(currentGridPos);
            
            if (isValid)
            {
                // Hover event
                if (currentGridPos != hoveredGridPos)
                {
                    hoveredGridPos = currentGridPos;
                    OnGridCellHovered?.Invoke(currentGridPos);
                    Debug.Log($"[MouseInteractionSystem] Hover: {currentGridPos}"); // Debug for gizmo verification
                }
                
                // Click event
                if (Input.GetMouseButtonDown(0))
                {
                    if (!IsPointerOverUI())
                    {
                        OnGridCellClicked?.Invoke(currentGridPos);
                        Debug.Log($"[MouseInteractionSystem] Grid cell clicked: {currentGridPos}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if mouse is over UI elements
        /// </summary>
        bool IsPointerOverUI()
        {
            // Simple raycast check for UI
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        
        // Debug visualization
        void OnDrawGizmos()
        {
            if (!showDebugGizmos || GridManager.Instance == null) return;
            
            // Draw hovered cell
            if (GridManager.Instance.IsValidGridPosition(hoveredGridPos))
            {
                Vector3 worldPos = GridManager.Instance.GridToWorldPosition(hoveredGridPos);
                Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Yellow
                Gizmos.DrawCube(worldPos, new Vector3(0.9f, 0.9f, 0.1f));
            }
        }
        
        /// <summary>
        /// Get currently hovered grid position
        /// </summary>
        public Vector2Int GetHoveredGridPosition() => hoveredGridPos;
    }
}
