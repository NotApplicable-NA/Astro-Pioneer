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
        [SerializeField] private CropData cropData;
        
        [Header("Runtime State")]
        [SerializeField] private int currentStage = 0;
        [SerializeField] private bool isWatered = false;
        [SerializeField] private Vector2Int gridPosition;
        
        private SpriteRenderer spriteRenderer;
        private GrowthTransitionVFX growthVFX;
        private HarvestableGlow harvestableGlow;
        private bool _isHarvesting = false;
        private bool _gridPositionSet = false;
        
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
                spriteRenderer.sortingLayerName = cropData.sortingLayer;
                spriteRenderer.sortingOrder = cropData.orderInLayer + 10;
            }
        }
        
        void Start()
        {
            transform.localScale = Vector3.one * 0.0625f; // PPU 16, cell size 1.0

            growthVFX = GetComponentInChildren<GrowthTransitionVFX>();
            harvestableGlow = GetComponentInChildren<HarvestableGlow>();
            UpdateVisual();

            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged += HandleDayChanged;

            // Auto-register only for scene-placed crops not initialized via PlantCrop
            if (GridManager.Instance != null && !_gridPositionSet)
            {
                gridPosition = GridManager.Instance.WorldToGridPosition(transform.position);
                if (!GridManager.Instance.GetOccupiedCells().ContainsKey(gridPosition))
                    GridManager.Instance.TryOccupyCell(gridPosition, gameObject);
            }
        }

        void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged -= HandleDayChanged;
        }

        // ─── Growth Logic ───

        private void HandleDayChanged(int day)
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
        /// Uses a hybrid approach: GridManager cell lighting → Physics overlap → Distance fallback.
        /// </summary>
        private bool IsInDarkness()
        {
            bool isDim = GridManager.Instance != null && !GridManager.Instance.IsCellLit(gridPosition);

            // Physics overlap check
            bool inShadowZone = false;
            bool hasUVLight = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (var hit in hits)
            {
                if (hit.GetComponent<AstroPioneer.Systems.Environment.ShadowZone>() != null) inShadowZone = true;
                if (hit.GetComponent<AstroPioneer.Machines.UVLightPillar>() != null) hasUVLight = true;
            }

            // Distance-based fallback
            foreach (var shadow in FindObjectsOfType<AstroPioneer.Systems.Environment.ShadowZone>())
            {
                if (Vector3.Distance(transform.position, shadow.transform.position) < 2f) inShadowZone = true;
            }
            foreach (var uv in FindObjectsOfType<AstroPioneer.Machines.UVLightPillar>())
            {
                if (Vector3.Distance(transform.position, uv.transform.position) < 3f) hasUVLight = true;
            }

            return (isDim || inShadowZone) && !hasUVLight;
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
            if (cropData == null || cropData.growthStageSprites == null) return;
            if (currentStage >= cropData.growthStageSprites.Length) return;

            Sprite stageSprite = cropData.growthStageSprites[currentStage];
            if (stageSprite == null) return;

            spriteRenderer.sprite = stageSprite;
            spriteRenderer.color = IsInDarkness()
                ? new Color(0.4f, 0.4f, 0.6f, 1.0f)
                : Color.white;
        }
        
        // ─── Accessors ───

        public CropData GetCropData() => cropData;
        public void SetCropData(CropData data) => cropData = data;
        public int GetCurrentStage() => currentStage;
        public bool GetIsWatered() => isWatered;
        public Vector2Int GetGridPosition() => gridPosition;

        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
            _gridPositionSet = true;
        }

        public void SetStageData(int stage, bool watered)
        {
            currentStage = stage;
            isWatered = watered;
            UpdateVisual();
        }
    }
}
