using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.VFX;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// CropInstance — Runtime instance for an individual crop.
    /// Handles growth progression, watering, light checks, and harvesting.
    /// </summary>
    public class CropInstance : MonoBehaviour
    {
        [Header("Crop Configuration")]
        [SerializeField] private CropStructureData cropData;
        
        [Header("Runtime State")]
        [SerializeField] private int currentStage = 0;
        [SerializeField] private bool isWatered = false;
        [SerializeField] private Vector2Int gridPosition;
        
        private SpriteRenderer spriteRenderer;
        private GrowthTransitionVFX growthVFX;
        private HarvestableGlow harvestableGlow;
        private bool _isHarvesting = false;
        
        // Events
        public delegate void CropEvent(CropInstance crop);
        public static event CropEvent OnCropStageChanged;
        public static event CropEvent OnCropHarvested;
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (cropData != null)
            {
                spriteRenderer.sortingLayerName = "Crops";
                spriteRenderer.sortingOrder = 10;
            }
        }
        
        void Start()
        {
            transform.localScale = Vector3.one * 0.0625f; // PPU 16, cell size 1.0

            growthVFX = GetComponentInChildren<GrowthTransitionVFX>();
            harvestableGlow = GetComponentInChildren<HarvestableGlow>();
            UpdateVisual();

            // Auto-register only for scene-placed crops not initialized via PlantCrop
            TryRegisterToGrid();
        }

        private void TryRegisterToGrid()
        {
            // CropManager DOD directly sets IDs
        }

        // ─── Growth Logic ───

        public void SimulateDayPass()
        {
            if (cropData == null || currentStage >= 3) return;
            if (!isWatered) return;

            if (IsInDarkness())
            {
                UpdateVisual();
                return;
            }

            AdvanceStage();
            isWatered = false;
            UpdateVisual();
        }
        
        private void AdvanceStage()
        {
            if (currentStage >= 3) return;

            currentStage++;
            UpdateVisual();

            if (growthVFX != null)
                growthVFX.PlayAtPosition(transform.position);

            OnCropStageChanged?.Invoke(this);

            if (currentStage >= 3 && harvestableGlow != null)
                harvestableGlow.StartGlow();
        }

        // ─── Light Check ───

        /// <summary>
        /// Returns true if this crop is in a dark zone without UV light coverage.
        /// Purely relies on GridManager $O(1)$ lookups instead of Physics2D Raycasts.
        /// </summary>
        private bool IsInDarkness()
        {
            if (GridManager.Instance == null) return false;
            return GridManager.Instance.IsShadowCell(gridPosition) && !GridManager.Instance.IsCellLit(gridPosition);
        }
        
        // ─── Player Actions ───

        public void WaterCrop() => isWatered = true;
        public bool IsHarvestable() => currentStage >= 3;
        
        public void Harvest()
        {
            if (!IsHarvestable() || _isHarvesting) return;
            _isHarvesting = true;
            
            OnCropHarvested?.Invoke(this);

            if (CropManager.Instance != null)
                CropManager.Instance.RemoveCrop(gridPosition);

            Destroy(gameObject);
        }
        
        // ─── Visual ───

        private void UpdateVisual()
        {
            if (cropData == null || cropData.sprites == null) return;
            if (currentStage >= cropData.sprites.Length) return;

            Sprite stageSprite = cropData.sprites[currentStage];
            if (stageSprite == null) return;

            spriteRenderer.sprite = stageSprite;
            spriteRenderer.color = IsInDarkness()
                ? new Color(0.4f, 0.4f, 0.6f, 1.0f)
                : Color.white;
        }
        
        // ─── Accessors ───

        public CropStructureData GetCropData() => cropData;
        public void SetCropData(CropStructureData data) => cropData = data;
        public int GetCurrentStage() => currentStage;
        public bool GetIsWatered() => isWatered;
        public Vector2Int GetGridPosition() => gridPosition;

        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
        }

        public void SetStageData(int stage, bool watered)
        {
            currentStage = stage;
            isWatered = watered;
            UpdateVisual();
        }
    }
}
