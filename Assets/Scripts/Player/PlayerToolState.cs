using UnityEngine;
using AstroPioneer.Systems;
using AstroPioneer.Managers;
using AstroPioneer.Data;

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
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
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
            
            CropManager.Instance.WaterCropAt(gridPos);
            
            // Trigger watering VFX
            // TODO: Implement VFX system (TICKET-012)
            
            Debug.Log($"[PlayerToolState] Watered crop at {gridPos}");
        }
        
        void HarvestCrop(Vector2Int gridPos, CropInstance crop)
        {
            if (crop == null) return;
            
            CropData cropData = crop.GetCropData();
            if (cropData == null) return;
            
            // Log harvest (placeholder for inventory - Sprint 4)
            Debug.Log($"[PlayerToolState] Harvested {cropData.displayName}! " +
                      $"Item: {cropData.harvestItemID} x{cropData.harvestQuantity}");
            
            // TODO: Add to inventory (Sprint 4 - TICKET-016)
            // InventoryManager.Instance?.AddItem(cropData.harvestItemID, cropData.harvestQuantity);
            
            // Trigger harvest VFX (TICKET-012)
            // TODO: HarvestVFX.Play(gridPos);
            
            // TODO: Play harvest SFX (Sprint 9)
            
            // Remove crop (calls CropInstance.Harvest() internally)
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
