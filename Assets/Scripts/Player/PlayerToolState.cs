using UnityEngine;
using AstroPioneer.Systems;
using AstroPioneer.Managers;
using AstroPioneer.Data;
using AstroPioneer.VFX;

namespace AstroPioneer.Player
{
    /// <summary>
    /// PlayerToolState - Manages player tool selection and actions.
    /// Handles planting seeds, watering crops, and harvesting.
    /// </summary>
    public class PlayerToolState : MonoBehaviour
    {
        public static PlayerToolState Instance { get; private set; }
        
        [Header("Tool Settings")]
        [SerializeField] private ToolType currentTool = ToolType.None;
        [SerializeField] private CropData selectedCropData;
        
        [Header("References")]
        [SerializeField] private CropData spacePotatoData;
        [SerializeField] private CropData neonCarrotData;
        
        [Header("VFX References")]
        [SerializeField] private WateringVFX wateringVFXPrefab;
        [SerializeField] private HarvestVFX harvestVFXPrefab;
        
        // VFX instances
        private WateringVFX wateringVFX;
        private HarvestVFX harvestVFX;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy entire GameObject, not just component
                return;
            }
            Instance = this;
            
            // Issue 2 Fix: Reuse VFX instances to prevent DontDestroyOnLoad leaks
            // Check if instances already exist before creating new ones
            wateringVFX = FindObjectOfType<WateringVFX>();
            if (wateringVFX == null && wateringVFXPrefab != null)
            {
                GameObject vfxObj = Instantiate(wateringVFXPrefab.gameObject);
                vfxObj.name = "WateringVFX_Instance";
                wateringVFX = vfxObj.GetComponent<WateringVFX>();
                DontDestroyOnLoad(vfxObj);
            }
            
            harvestVFX = FindObjectOfType<HarvestVFX>();
            if (harvestVFX == null && harvestVFXPrefab != null)
            {
                GameObject vfxObj = Instantiate(harvestVFXPrefab.gameObject);
                vfxObj.name = "HarvestVFX_Instance";
                harvestVFX = vfxObj.GetComponent<HarvestVFX>();
                DontDestroyOnLoad(vfxObj);
            }
        }
        
        void OnEnable()
        {
            // Subscribe to grid click events
            MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
        }
        
        void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            MouseInteractionSystem.OnGridCellClicked -= HandleGridClick;
        }
        
        void HandleGridClick(Vector2Int gridPos)
        {
            // Priority: Check for harvestable crops first (any tool can harvest)
            if (CropManager.Instance != null)
            {
                CropInstance crop = CropManager.Instance.GetCropAt(gridPos);
                if (crop != null && crop.IsHarvestable())
                {
                    HarvestCrop(gridPos, crop);
                    return; // Don't process tool actions after harvest
                }
            }
            
            // Tool-based actions (only if no harvest)
            switch (currentTool)
            {
                case ToolType.Seed_SpacePotato:
                    PlantCrop(gridPos, spacePotatoData);
                    break;
                    
                case ToolType.Seed_NeonCarrot:
                    PlantCrop(gridPos, neonCarrotData);
                    break;
                    
                case ToolType.WateringCan:
                    WaterCrop(gridPos);
                    break;
            }
        }
        
        void PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            if (cropData == null)
            {
                Debug.LogWarning("[PlayerToolState] CropData not assigned!", this);
                return;
            }
            
            if (CropManager.Instance == null)
            {
                Debug.LogError("[PlayerToolState] CropManager not found!", this);
                return;
            }
            
            bool success = CropManager.Instance.PlantCrop(gridPos, cropData);
            if (success)
            {
                Debug.Log($"[PlayerToolState] Planted {cropData.displayName} at {gridPos}");
                // TODO: Play planting SFX (Sprint 9)
            }
            else
            {
                Debug.LogWarning($"[PlayerToolState] Failed to plant at {gridPos}");
            }
        }
        
        void WaterCrop(Vector2Int gridPos)
        {
            if (CropManager.Instance == null)
            {
                Debug.LogError("[PlayerToolState] CropManager not found!", this);
                return;
            }

            CropInstance crop = CropManager.Instance.GetCropAt(gridPos);
            if (crop != null)
            {
                crop.WaterCrop();
                Debug.Log($"[PlayerToolState] Watered crop at {gridPos}");
                
                // Issue 3 Fix: GridManager null guard before VFX
                if (wateringVFX != null && wateringVFX.gameObject != null)
                {
                    if (GridManager.Instance != null)
                    {
                        Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
                        wateringVFX.PlayAtPosition(worldPos);
                    }
                    else
                    {
                        Debug.LogError("[PlayerToolState] GridManager.Instance is null, cannot play WateringVFX", this);
                    }
                }
                else if (wateringVFX == null)
                {
                    Debug.LogWarning("[PlayerToolState] WateringVFX not assigned - follow vfx_completion_guide.md");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerToolState] No crop at {gridPos} to water");
            }
        }
        
        void HarvestCrop(Vector2Int gridPos, CropInstance crop)
        {
            if (crop == null) return;
            
            CropData cropData = crop.GetCropData();
            if (cropData == null) return;
            
            Debug.Log($"[PlayerToolState] Harvesting crop at {gridPos}");
            
            // Play harvest VFX before destroying crop
            if (harvestVFX != null)
            {
                harvestVFX.PlayAtPosition(crop.transform.position);
            }
            
            // TODO: TICKET-016 (Sprint 4) - Add to inventory
            // InventoryManager.Instance?.AddItem(crop.CropData.harvestItemID, crop.CropData.harvestQuantity);
            
            // TODO: Play harvest SFX (Sprint 9)
            
            // Harvest the crop (this will destroy it)
            crop.Harvest();
        }
        
        // Public API
        public void SetTool(ToolType tool)
        {
            currentTool = tool;
            Debug.Log($"[PlayerToolState] Tool changed to: {tool}");
        }
        
        public ToolType GetCurrentTool() => currentTool;
    }
}
