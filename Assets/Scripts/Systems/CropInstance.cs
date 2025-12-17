using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.VFX;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// CropInstance - Runtime instance untuk individual crop.
    /// Handles growth progression, watering, dan harvesting.
    /// </summary>
    public class CropInstance : MonoBehaviour
    {
        [Header("Crop Configuration")]
        [SerializeField] private CropData cropData;
        
        [Header("Runtime State")]
        [SerializeField] private int currentStage = 0; // 0-3
        [SerializeField] private float growthTimer = 0f;
        [SerializeField] private bool isWatered = false;
        [SerializeField] private Vector2Int gridPosition;
        
        private SpriteRenderer spriteRenderer;
        private GrowthTransitionVFX growthVFX;
        private HarvestableGlow harvestableGlow;
        private bool _isHarvesting = false; // Guard flag for double harvest prevention
        
        // Events
        public delegate void CropEvent(CropInstance crop);
        public static event CropEvent OnCropStageChanged;
        public static event CropEvent OnCropHarvested;
        
        void Awake()
        {
            // Setup sprite renderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // Configure rendering
            if (cropData != null)
            {
                spriteRenderer.sortingLayerName = cropData.sortingLayer;
                spriteRenderer.sortingOrder = cropData.orderInLayer;
            }
        }
        
        void Start()
        {
            // Scale to fit grid cell (PPU 32, cell size 1.0)
            // Crop sprites are ~680-800px, need scaling down
            transform.localScale = Vector3.one * 0.03f;
            
            // Setup VFX components
            growthVFX = GetComponentInChildren<GrowthTransitionVFX>();
            harvestableGlow = GetComponentInChildren<HarvestableGlow>();
            
            UpdateVisual();
        }
        
        void Update()
        {
            if (cropData == null) return;
            if (currentStage >= 3) return; // Fully grown, no more progression
            
            // Issue 4 Fix: Null and bounds check for array access
            if (cropData.growthTimePerStage == null)
            {
                Debug.LogError($"[CropInstance] growthTimePerStage is null for crop {cropData.displayName}", this);
                return;
            }
            
            if (currentStage >= cropData.growthTimePerStage.Length)
            {
                Debug.LogError($"[CropInstance] Invalid stage {currentStage} for crop {cropData.displayName} (array length: {cropData.growthTimePerStage.Length})", this);
                return;
            }
            
            // QA Fix: Watering Logic (Option B - Require water to grow)
            if (!isWatered) return; // Crops don't grow without water
            
            // Growth progression
            growthTimer += Time.deltaTime;
            float stageTime = cropData.growthTimePerStage[currentStage];
            
            if (growthTimer >= stageTime)
            {
                AdvanceStage();
                isWatered = false; // Reset after stage advance (requires re-watering)
            }
        }
        
        void AdvanceStage()
        {
            if (currentStage < 3)
            {
                currentStage++;
                growthTimer = 0f;
                UpdateVisual();
                
                // Trigger growth transition VFX
                if (growthVFX != null)
                {
                    growthVFX.PlayAtPosition(transform.position);
                }
                
                OnCropStageChanged?.Invoke(this);
                
                // Enable harvestable glow at stage 3
                if (currentStage >= 3 && harvestableGlow != null)
                {
                    harvestableGlow.StartGlow();
                }
            }
        }
        
        public void WaterCrop()
        {
            isWatered = true;
            // TODO: Visual feedback (TICKET-012)
        }
        
        public bool IsHarvestable()
        {
            return currentStage >= 3; // Fully grown
        }
        
        public void Harvest()
        {
            if (!IsHarvestable()) return;
            if (_isHarvesting) return; // QA Fix: Guard flag to prevent double harvest
            _isHarvesting = true;
            
            Debug.Log($"[CropInstance] Harvesting {cropData.displayName} at {gridPosition}");
            
            // TODO: TICKET-016 (Sprint 4) - Add to inventory
            // InventoryManager.Instance?.AddItem(cropData.harvestItemID, cropData.harvestQuantity);
            
            // TODO: TICKET-012 - Harvest VFX
            // HarvestVFX.PlayAtPosition(transform.position);
            
            // Trigger event
            OnCropHarvested?.Invoke(this);
            
            // Issue 5 Fix: Don't duplicate ReleaseCell - CropManager.RemoveCrop already handles it
            // Cleanup - remove from registry first, then destroy
            if (CropManager.Instance != null)
            {
                CropManager.Instance.RemoveCrop(gridPosition);
            }
            
            Destroy(gameObject);
        }
        
        void UpdateVisual()
        {
            // Issue 4 Fix: Comprehensive null and bounds checks
            if (cropData == null)
            {
                Debug.LogWarning("[CropInstance] cropData is null, cannot update visual", this);
                return;
            }
            
            if (cropData.growthStageSprites == null)
            {
                Debug.LogWarning($"[CropInstance] growthStageSprites is null for crop {cropData.displayName}", this);
                return;
            }
            
            if (currentStage >= cropData.growthStageSprites.Length)
            {
                Debug.LogWarning($"[CropInstance] currentStage {currentStage} out of bounds for growthStageSprites (length: {cropData.growthStageSprites.Length})", this);
                return;
            }
            
            if (cropData.growthStageSprites[currentStage] != null)
            {
                spriteRenderer.sprite = cropData.growthStageSprites[currentStage];
            }
            else
            {
                Debug.LogWarning($"[CropInstance] Sprite for stage {currentStage} is null for crop {cropData.displayName}", this);
            }
        }
        
        // Getters & Setters
        public CropData GetCropData() => cropData;
        public void SetCropData(CropData data) => cropData = data;
        public int GetCurrentStage() => currentStage;
        public Vector2Int GetGridPosition() => gridPosition;
        public void SetGridPosition(Vector2Int pos) => gridPosition = pos;
    }
}
