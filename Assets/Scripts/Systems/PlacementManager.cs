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
        private SpriteRenderer ghostSpriteRenderer;
        private Camera cachedMainCamera;

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

        public void SetPlacementItem(InventoryItem item)
        {
            currentPlacementItem = item;
            
            // Cleanup existing ghost
            if (ghostVisual != null)
            {
                Destroy(ghostVisual);
                ghostVisual = null;
                ghostSpriteRenderer = null;
            }

            // Create new ghost if placing a crafted item
            if (item != null && item.type == ItemType.Crafted)
            {
                if (item.placedStructure != null && item.placedStructure.visualPrefab != null)
                {
                    ghostVisual = Instantiate(item.placedStructure.visualPrefab);
                    ghostVisual.name = "Ghost_" + item.placedStructure.visualPrefab.name;
                    
                    // Auto-disable colliders and scripts on ghost
                    var colliders = ghostVisual.GetComponentsInChildren<Collider2D>();
                    foreach (var c in colliders) c.enabled = false;
                    
                    var scripts = ghostVisual.GetComponentsInChildren<MonoBehaviour>();
                    foreach (var s in scripts) 
                    {
                        if (s != null) s.enabled = false;
                    }
                    ghostSpriteRenderer = ghostVisual.GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        public bool IsPlacingModeActive()
        {
            return currentPlacementItem != null && currentPlacementItem.type == ItemType.Crafted;
        }

        /// <summary>
        /// Resets placement state. Called on scene load to prevent
        /// ghost placements from a previous save state.
        /// </summary>
        public void ClearPlacement()
        {
            currentPlacementItem = null;
            if (ghostVisual != null)
            {
                Destroy(ghostVisual);
                ghostVisual = null;
                ghostSpriteRenderer = null;
            }
        }

        public bool IsPlacementValid(Vector2Int origin, Vector2 dimensions)
        {
            int w = Mathf.RoundToInt(dimensions.x == 0 ? 1 : dimensions.x);
            int h = Mathf.RoundToInt(dimensions.y == 0 ? 1 : dimensions.y);
            bool isMicro = currentPlacementItem != null && currentPlacementItem.isMicroGridItem;
            var registry = AstroPioneer.Data.StructureRegistry.Instance;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = origin + new Vector2Int(x, y);
                    
                    if (isMicro)
                    {
                        // Primary: UtilityLayer must be empty
                        if (GridManager.Instance.GetUtilityAt(pos) != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) return false;
                        
                        // Cross-layer: StructureLayer must not have a blocking structure
                        ushort structID = GridManager.Instance.GetStructureAt(pos);
                        if (structID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY && registry != null)
                        {
                            var data = registry.Get(structID);
                            if (data != null && data.blocksMacroPlacement) return false;
                        }
                    }
                    else
                    {
                        // Primary: StructureLayer must be empty. OCCUPIED_PART also blocks.
                        if (GridManager.Instance.GetStructureAt(pos) != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) return false;
                        
                        // Cross-layer: UtilityLayer must not have a blocking structure
                        ushort utilID = GridManager.Instance.GetUtilityAt(pos);
                        if (utilID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY && registry != null)
                        {
                            var data = registry.Get(utilID);
                            if (data != null && data.blocksMacroPlacement) return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool TryPlace(Vector2Int gridPos)
        {
            if (!IsPlacingModeActive() || currentPlacementItem.placedStructure == null || currentPlacementItem.placedStructure.visualPrefab == null) return false;

            Vector2 dimensions = currentPlacementItem.placedStructure.dimensions == Vector2.zero ? Vector2.one : currentPlacementItem.placedStructure.dimensions;

            if (!IsPlacementValid(gridPos, dimensions))
            {
                return false;
            }
            
            ushort structID = currentPlacementItem.StructureID;
            bool isMicro = currentPlacementItem.isMicroGridItem;
            int w = Mathf.RoundToInt(dimensions.x);
            int h = Mathf.RoundToInt(dimensions.y);

            // ─── V23: Separation of Concerns ───
            // Path 1: Free-moving Entity (Bot, Vehicle) — uses isEntity flag, NOT ID 0
            if (currentPlacementItem.isEntity)
            {
                Vector3 center = GridManager.Instance.GridToWorldPosition(gridPos);
                Vector3 offset = new Vector3((dimensions.x - 1) * 0.5f * GridManager.Instance.CellSize, (dimensions.y - 1) * 0.5f * GridManager.Instance.CellSize, 0);
                Vector3 spawnPos = center + offset;
                
                GameObject prefab = currentPlacementItem.placedStructure.visualPrefab;
                GameObject newObj;
                if (AstroPioneer.Managers.ObjectPoolManager.Instance != null)
                {
                    newObj = AstroPioneer.Managers.ObjectPoolManager.Instance.SpawnFromPool(prefab.name, prefab, spawnPos);
                    newObj.transform.rotation = Quaternion.identity;
                }
                else
                {
                    newObj = Instantiate(prefab, spawnPos, Quaternion.identity);
                }
                
                AstroPioneer.Systems.MachineIDTag tag = newObj.GetComponent<AstroPioneer.Systems.MachineIDTag>();
                if (tag != null) { tag.originGridPos = gridPos; tag.EnsureUniqueID(); }
                
                AstroPioneer.Machines.AgriMech mech = newObj.GetComponent<AstroPioneer.Machines.AgriMech>();
                if (mech != null) mech.Initialize(newObj.transform.position);

                // ─── V24.11: Data-First Bot Creation (Pintu Depan) ───
                // Use GetComponentInChildren in case the user put the script on a child object
                var botScript = newObj.GetComponentInChildren<AstroPioneer.Machines.Automation.TransportBot>();
                if (botScript != null)
                {
                    if (AstroPioneer.Machines.Automation.BotSimulationManager.Instance != null)
                    {
                        string newBotID = $"Bot_{System.Guid.NewGuid():N}";
                        int entityTypeID = 1; // Default fallback
                        if (AstroPioneer.Data.EntityRegistry.Instance != null)
                        {
                            entityTypeID = AstroPioneer.Data.EntityRegistry.Instance.GetIDByType(prefab);
                            if (entityTypeID == 65535) 
                            {
                                Debug.LogWarning($"[PlacementManager] Prefab {prefab.name} tidak terdaftar di EntityRegistry! Visual bot tidak akan tersave.");
                                entityTypeID = 1; // Fallback
                            }
                        }
                        
                        // Step 1: CREATE data in the Brain (Otak)
                        AstroPioneer.Machines.Automation.BotSimulationManager.Instance.RegisterBot(newBotID, spawnPos, entityTypeID);
                        
                        // Step 2: BIND the visual (Wayang) to the data
                        botScript.BindToSimulation(newBotID);
                        
                        Debug.Log($"[PlacementManager] Data-First Bot Created: {newBotID} at {spawnPos}");
                    }
                    else
                    {
                        Debug.LogError("[PlacementManager] GAGAL MEMBUAT BOT: BotSimulationManager tidak ditemukan di scene! Pastikan ada GameObject dengan script BotSimulationManager.");
                    }
                }
            }
            // Path 2: Grid Structure/Utility — requires valid StructureID
            else
            {
                // Guard: ID 0 means EMPTY — something is misconfigured
                if (structID == AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY)
                {
                    Debug.LogError($"[PlacementManager] BLOCKED: '{currentPlacementItem.displayName}' has StructureID=0. " +
                        "Either register it in StructureRegistry or mark isEntity=true if it's a free entity.");
                    return false;
                }

                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        // ONLY the origin (0,0) gets the actual Structure ID. 
                        // The rest are marked as OCCUPIED_PART to prevent duplicate visuals.
                        ushort idToPlace = (x == 0 && y == 0) ? structID : AstroPioneer.Core.GameConstants.STRUCTURE_OCCUPIED_PART;

                        if (isMicro)
                            GridManager.Instance.TryPlaceUtility(gridPos + new Vector2Int(x, y), idToPlace);
                        else
                        {
                            GridManager.Instance.TryPlaceStructure(gridPos + new Vector2Int(x, y), idToPlace);
                            
                            // Pre-allocate Complex State for machines with persistence (V23)
                            if (x == 0 && y == 0 && currentPlacementItem.placedStructure.visualPrefab != null)
                            {
                                if (currentPlacementItem.placedStructure.visualPrefab.GetComponent<AstroPioneer.Interfaces.ISavableMachine>() != null)
                                {
                                    GridManager.Instance.GetOrAllocateComplexState(gridPos);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        void Update()
        {
            if (IsPlacingModeActive() && ghostVisual != null)
            {
                if (cachedMainCamera == null) cachedMainCamera = Camera.main;
                if (cachedMainCamera == null) return;

                Vector3 mouseWorldPos = cachedMainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cachedMainCamera.transform.position.z));
                float cSize = GridManager.Instance.CellSize;
                
                Vector2 dimensions = (currentPlacementItem.placedStructure != null && currentPlacementItem.placedStructure.dimensions != Vector2.zero) ? currentPlacementItem.placedStructure.dimensions : Vector2.one;
                Vector3 mouseOffset = Vector3.zero;
                
                Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(mouseWorldPos - mouseOffset);
                
                Vector3 corner = GridManager.Instance.GridToWorldPosition(gridPos);
                Vector3 centerOffsetExtents = new Vector3(dimensions.x * 0.5f * cSize, dimensions.y * 0.5f * cSize, 0);
                ghostVisual.transform.position = corner + centerOffsetExtents;

                // Validation checks
                bool isInsideBounds = true; // DOD chunk map is infinite
                bool isSpaceFree = true;
                
                int w = Mathf.RoundToInt(dimensions.x == 0 ? 1 : dimensions.x);
                int h = Mathf.RoundToInt(dimensions.y == 0 ? 1 : dimensions.y);
                bool isMicro = currentPlacementItem != null && currentPlacementItem.isMicroGridItem;
                var registry = AstroPioneer.Data.StructureRegistry.Instance;
                
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        Vector2Int pos = gridPos + new Vector2Int(x, y);
                        
                        if (isMicro)
                        {
                            if (GridManager.Instance.GetUtilityAt(pos) != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) isSpaceFree = false;
                            
                            // Cross-layer: check StructureLayer for blocking structures
                            ushort sID = GridManager.Instance.GetStructureAt(pos);
                            if (sID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY && registry != null)
                            {
                                var data = registry.Get(sID);
                                if (data != null && data.blocksMacroPlacement) isSpaceFree = false;
                            }
                        }
                        else
                        {
                            if (GridManager.Instance.GetStructureAt(pos) != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) isSpaceFree = false;
                            
                            // Cross-layer: check UtilityLayer for blocking structures
                            ushort uID = GridManager.Instance.GetUtilityAt(pos);
                            if (uID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY && registry != null)
                            {
                                var data = registry.Get(uID);
                                if (data != null && data.blocksMacroPlacement) isSpaceFree = false;
                            }
                        }
                    }
                }

                // Hide completely if out of bounds. Show RED if colliding inside bounds.
                ghostVisual.SetActive(isInsideBounds);
                var sr = ghostSpriteRenderer;
                if (sr != null)
                {
                    sr.color = (isInsideBounds && isSpaceFree) ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                }
            }
        }
    }
}
