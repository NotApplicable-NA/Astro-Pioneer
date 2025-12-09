using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// Sistem untuk handle Mouse-to-Grid interaction menggunakan Raycast.
    /// Mengacu pada requirement: "Implementasi deteksi Mouse-to-Grid (Raycast)"
    /// </summary>
    public class MouseInteractionSystem : MonoBehaviour
    {
        [Header("Camera Reference")]
        [Tooltip("Camera untuk raycast (default: Main Camera)")]
        [SerializeField] private Camera mainCamera;
        
        [Header("Layer Settings")]
        [Tooltip("Layer mask untuk grid interaction")]
        [SerializeField] private LayerMask gridLayerMask = -1;
        
        [Header("Debug")]
        [Tooltip("Tampilkan debug ray di Scene view")]
        [SerializeField] private bool showDebugRay = true;
        
        [Tooltip("Warna debug ray")]
        [SerializeField] private Color debugRayColor = Color.yellow;
        
        // Events untuk interaction
        public System.Action<Vector2Int> OnGridCellClicked;
        public System.Action<Vector2Int> OnGridCellHovered;
        
        private Vector2Int lastHoveredGridPos = new Vector2Int(-1, -1);
        
        private void Awake()
        {
            // Auto-assign Main Camera jika belum di-assign
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                
                if (mainCamera == null)
                {
                    Debug.LogError("[MouseInteractionSystem] Main Camera tidak ditemukan! Pastikan ada Camera dengan tag 'MainCamera'.", this);
                }
            }
            
            // Validasi GridManager
            if (GridManager.Instance == null)
            {
                Debug.LogError("[MouseInteractionSystem] GridManager.Instance tidak ditemukan! Pastikan GridManager ada di scene.", this);
            }
        }
        
        private void Update()
        {
            HandleMouseInput();
        }
        
        /// <summary>
        /// Handle mouse input dan raycast ke grid
        /// </summary>
        private void HandleMouseInput()
        {
            if (mainCamera == null || GridManager.Instance == null)
                return;
            
            // Convert mouse position ke world position menggunakan raycast
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            
            if (mouseWorldPos == Vector3.zero)
                return;
            
            // Convert ke grid position
            Vector2Int gridPos = GridManager.Instance.GetGridPosition(mouseWorldPos);
            
            // Hover detection (untuk highlight nanti)
            if (gridPos != lastHoveredGridPos)
            {
                lastHoveredGridPos = gridPos;
                OnGridCellHovered?.Invoke(gridPos);
            }
            
            // Click detection
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                HandleGridClick(gridPos, mouseWorldPos);
            }
        }
        
        /// <summary>
        /// Raycast dari mouse position ke world position
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            // Untuk 2D orthographic camera, langsung convert screen to world
            if (mainCamera.orthographic)
            {
                Vector3 mouseScreenPos = Input.mousePosition;
                mouseScreenPos.z = mainCamera.nearClipPlane;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
                worldPos.z = 0f; // Lock Z untuk 2D
                
                // Debug ray visualization
                if (showDebugRay)
                {
                    Debug.DrawRay(mainCamera.transform.position, worldPos - mainCamera.transform.position, debugRayColor);
                }
                
                return worldPos;
            }
            else
            {
                // Untuk perspective camera, gunakan raycast
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
                {
                    if (showDebugRay)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * hit.distance, debugRayColor);
                    }
                    
                    return hit.point;
                }
                
                // Fallback: hitung intersection dengan Z=0 plane
                Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
                float distance;
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    if (showDebugRay)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * distance, debugRayColor);
                    }
                    return hitPoint;
                }
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Handle click pada grid cell
        /// </summary>
        private void HandleGridClick(Vector2Int gridPos, Vector3 worldPos)
        {
            // Validasi apakah position playable
            if (!GridManager.Instance.IsPositionPlayable(gridPos))
            {
                Debug.Log($"[MouseInteractionSystem] Clicked on locked area: {gridPos}");
                return;
            }
            
            // Validasi apakah position available
            if (!GridManager.Instance.IsPositionAvailable(gridPos))
            {
                Debug.Log($"[MouseInteractionSystem] Clicked on occupied cell: {gridPos}");
                return;
            }
            
            // Trigger event
            OnGridCellClicked?.Invoke(gridPos);
            
            Debug.Log($"[MouseInteractionSystem] Grid cell clicked: {gridPos} (World: {worldPos})");
        }
        
        /// <summary>
        /// Get current hovered grid position (untuk UI feedback)
        /// </summary>
        public Vector2Int GetHoveredGridPosition()
        {
            return lastHoveredGridPos;
        }
    }
}


