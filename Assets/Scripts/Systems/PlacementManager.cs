using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        private InventoryItem currentPlacementItem;
        private GameObject ghostVisual;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetPlacementItem(InventoryItem item)
        {
            currentPlacementItem = item;
            
            // Cleanup existing ghost
            if (ghostVisual != null)
            {
                Destroy(ghostVisual);
                ghostVisual = null;
            }

            // Create new ghost if placing a crafted item
            if (item != null && item.type == ItemType.Crafted)
            {
                if (item.placeablePrefab != null)
                {
                    ghostVisual = Instantiate(item.placeablePrefab);
                    
                    // Auto-disable colliders and scripts on ghost
                    var colliders = ghostVisual.GetComponentsInChildren<Collider2D>();
                    foreach (var c in colliders) c.enabled = false;
                    
                    var scripts = ghostVisual.GetComponentsInChildren<MonoBehaviour>();
                    foreach (var s in scripts) 
                    {
                        if (s != null) s.enabled = false;
                    }
                }
            }
        }

        public bool IsPlacingModeActive()
        {
            return currentPlacementItem != null && currentPlacementItem.type == ItemType.Crafted;
        }

        public bool IsPlacementValid(Vector2Int origin, Vector2 dimensions)
        {
            int w = Mathf.RoundToInt(dimensions.x == 0 ? 1 : dimensions.x);
            int h = Mathf.RoundToInt(dimensions.y == 0 ? 1 : dimensions.y);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = origin + new Vector2Int(x, y);
                    if (!GridManager.Instance.IsValidGridPosition(pos)) return false;
                    if (GridManager.Instance.GetOccupantAt(pos) != null) return false;
                }
            }
            return true;
        }

        public bool TryPlace(Vector2Int gridPos)
        {
            if (!IsPlacingModeActive() || currentPlacementItem.placeablePrefab == null) return false;

            Vector2 dimensions = currentPlacementItem.dimensions == Vector2.zero ? Vector2.one : currentPlacementItem.dimensions;

            if (!IsPlacementValid(gridPos, dimensions))
            {
                return false;
            }
            
            Vector3 corner = GridManager.Instance.GridOrigin + new Vector3(gridPos.x * GridManager.Instance.CellSize, gridPos.y * GridManager.Instance.CellSize, 0);
            Vector3 offset = new Vector3(dimensions.x * 0.5f * GridManager.Instance.CellSize, dimensions.y * 0.5f * GridManager.Instance.CellSize, 0);
            GameObject newObj = Instantiate(currentPlacementItem.placeablePrefab, corner + offset, Quaternion.identity);

            // Register in grid for ALL cells occupied by the machine
            int w = Mathf.RoundToInt(dimensions.x);
            int h = Mathf.RoundToInt(dimensions.y);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    GridManager.Instance.TryOccupyCell(gridPos + new Vector2Int(x, y), newObj);
                }
            }
            
            // Initialize specific machine properties so it immediately functions and tracks its position accurately for saving
            AstroPioneer.Systems.MachineIDTag tag = newObj.GetComponent<AstroPioneer.Systems.MachineIDTag>();
            if (tag != null) 
            {
                tag.originGridPos = gridPos;
                tag.EnsureUniqueID(); // Generate GUID for this new instance
            }
            
            AstroPioneer.Machines.AgriMech mech = newObj.GetComponent<AstroPioneer.Machines.AgriMech>();
            if (mech != null)
            {
                mech.Initialize(gridPos);
            }
            return true;
        }

        void Update()
        {
            if (IsPlacingModeActive() && ghostVisual != null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;

                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
                float cSize = GridManager.Instance.CellSize;
                
                Vector2 dimensions = currentPlacementItem.dimensions == Vector2.zero ? Vector2.one : currentPlacementItem.dimensions;
                Vector3 mouseOffset = Vector3.zero;
                
                Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(mouseWorldPos - mouseOffset);
                
                Vector3 corner = GridManager.Instance.GridOrigin + new Vector3(gridPos.x * cSize, gridPos.y * cSize, 0);
                Vector3 centerOffsetExtents = new Vector3(dimensions.x * 0.5f * cSize, dimensions.y * 0.5f * cSize, 0);
                ghostVisual.transform.position = corner + centerOffsetExtents;

                // Validation checks
                bool isInsideBounds = true;
                bool isSpaceFree = true;
                
                int w = Mathf.RoundToInt(dimensions.x == 0 ? 1 : dimensions.x);
                int h = Mathf.RoundToInt(dimensions.y == 0 ? 1 : dimensions.y);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        Vector2Int pos = gridPos + new Vector2Int(x, y);
                        if (!GridManager.Instance.IsValidGridPosition(pos)) isInsideBounds = false;
                        else if (GridManager.Instance.GetOccupantAt(pos) != null) isSpaceFree = false;
                    }
                }

                // Hide completely if out of bounds. Show RED if colliding inside bounds.
                ghostVisual.SetActive(isInsideBounds);
                var sr = ghostVisual.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = (isInsideBounds && isSpaceFree) ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                }
            }
        }
    }
}
