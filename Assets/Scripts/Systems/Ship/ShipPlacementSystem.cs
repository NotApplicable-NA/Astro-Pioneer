using UnityEngine;
using System;
using AstroPioneer.Data;
using AstroPioneer.Machines;

namespace AstroPioneer.Systems.Ship
{
    /// <summary>
    /// Handles placing/removing machines on the ship grid.
    /// Ghost preview, rotation, validation.
    /// </summary>
    public class ShipPlacementSystem : MonoBehaviour
    {
        public static ShipPlacementSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private LayerMask gridLayerMask;
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

        [Header("State")]
        [SerializeField] private bool isPlacing = false;

        // Current placement state
        private GameObject ghostPreview;
        private SpriteRenderer ghostRenderer;
        private Camera cachedMainCamera;
        private GameObject prefabToPlace;
        private Vector2Int objectSize = Vector2Int.one;
        private FacingDirection currentDirection = FacingDirection.South;

        // Events
        public event Action<GameObject, Vector2Int> OnObjectPlaced;
#pragma warning disable CS0067 // Will be used when removal UI is implemented
        public event Action<Vector2Int> OnObjectRemoved;
#pragma warning restore CS0067

        public bool IsPlacing => isPlacing;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cachedMainCamera = Camera.main;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (!isPlacing || ghostPreview == null) return;

            // Update ghost position
            if (cachedMainCamera == null) cachedMainCamera = Camera.main;
            if (cachedMainCamera == null) return;
            Vector3 mouseWorld = cachedMainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            if (ShipGrid.Instance != null)
            {
                Vector2Int gridPos = ShipGrid.Instance.WorldToGridPosition(mouseWorld);
                Vector3 snappedPos = ShipGrid.Instance.GridToWorldPosition(gridPos);
                ghostPreview.transform.position = snappedPos;

                // Update color based on validity
                bool canPlace = ShipGrid.Instance.IsAreaPlaceable(gridPos, objectSize);
                ghostRenderer.color = canPlace ? validColor : invalidColor;

                // Rotate with R
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RotatePreview();
                }

                // Place with LMB
                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    ConfirmPlacement(gridPos);
                }

                // Cancel with RMB or Escape
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelPlacement();
                }
            }
        }

        // ── Public API ──────────────────────────────

        /// <summary>
        /// Start placement mode with a machine prefab.
        /// </summary>
        public void StartPlacement(GameObject prefab, Vector2Int size = default)
        {
            if (isPlacing) CancelPlacement();

            prefabToPlace = prefab;
            objectSize = size == default ? Vector2Int.one : size;
            currentDirection = FacingDirection.South;

            // Create ghost preview
            ghostPreview = new GameObject("PlacementGhost");
            ghostRenderer = ghostPreview.AddComponent<SpriteRenderer>();

            // Copy sprite from prefab
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            if (prefabRenderer != null)
            {
                ghostRenderer.sprite = prefabRenderer.sprite;
                ghostRenderer.sortingLayerName = prefabRenderer.sortingLayerName;
                ghostRenderer.sortingOrder = 999; // Above everything
            }

            ghostRenderer.color = validColor;
            isPlacing = true;
        }

        /// <summary>
        /// Cancel current placement.
        /// </summary>
        public void CancelPlacement()
        {
            if (ghostPreview != null)
                Destroy(ghostPreview);

            ghostPreview = null;
            ghostRenderer = null;
            prefabToPlace = null;
            isPlacing = false;
        }

        // ── Internal ────────────────────────────────

        private void ConfirmPlacement(Vector2Int gridPos)
        {
            if (prefabToPlace == null || ShipGrid.Instance == null) return;

            // Instantiate the actual object
            Vector3 worldPos = ShipGrid.Instance.GridToWorldPosition(gridPos);
            GameObject placed = Instantiate(prefabToPlace, worldPos, Quaternion.identity);

            // Apply direction
            var machineDir = placed.GetComponent<MachineDirection>();
            if (machineDir != null)
                machineDir.SetDirection(currentDirection);

            // Register on grid
            if (ShipGrid.Instance.TryPlace(gridPos, placed, objectSize))
            {
                OnObjectPlaced?.Invoke(placed, gridPos);
            }
            else
            {
                // Shouldn't happen since we checked, but safety net
                Destroy(placed);
            }

            CancelPlacement();
        }

        private void RotatePreview()
        {
            currentDirection = (FacingDirection)(((int)currentDirection + 1) % 4);

            // Update ghost sprite to match direction
            if (prefabToPlace != null)
            {
                var machineDir = prefabToPlace.GetComponent<MachineDirection>();
                if (machineDir != null)
                {
                    // Temporarily apply direction to get the correct sprite
                    // We need to read the sprite from the prefab's MachineDirection
                    // For now, just log the rotation
                }
            }

            // Swap size dimensions for 90° rotations
            if (objectSize.x != objectSize.y)
            {
                objectSize = new Vector2Int(objectSize.y, objectSize.x);
            }
        }
    }
}
