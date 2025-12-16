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
            
            // Growth progression
            growthTimer += Time.deltaTime;
            float stageTime = cropData.growthTimePerStage[currentStage];
            
            if (growthTimer >= stageTime)
            {
                AdvanceStage();
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
            
            // Trigger harvest event
            OnCropHarvested?.Invoke(this);
            
            // TODO: Add to inventory (Sprint 4 - TICKET-016)
            // InventoryManager.Instance?.AddItem(cropData.harvestItemID, cropData.harvestQuantity);
            
            // Remove from CropManager registry FIRST (critical!)
            if (CropManager.Instance != null)
            {
                CropManager.Instance.RemoveCrop(gridPosition);
            }
            
            // Note: CropManager.RemoveCrop() already calls GridManager.ReleaseCell()
            // No need to duplicate here
            
            // Remove crop
            Destroy(gameObject);
        }
        
        void UpdateVisual()
        {
            if (cropData != null && currentStage < cropData.stageSprites.Length)
            {
                if (cropData.stageSprites[currentStage] != null)
                {
                    spriteRenderer.sprite = cropData.stageSprites[currentStage];
                }
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
