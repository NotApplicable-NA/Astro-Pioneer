using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Systems;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// CropManager — Singleton that manages the crop lifecycle:
    /// planting, tracking, watering, and removal.
    /// </summary>
    public class CropManager : MonoBehaviour
    {
        public static CropManager Instance { get; private set; }
        
        [Header("Crop Prefab")]
        [SerializeField] private GameObject cropPrefab;
        
        private readonly Dictionary<Vector2Int, CropInstance> activeCrops = new Dictionary<Vector2Int, CropInstance>();
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        
        /// <summary>
        /// Plants a crop at the given grid position. Always registers in activeCrops;
        /// grid cell occupancy is best-effort (skipped silently if already occupied).
        /// </summary>
        public bool PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            if (GridManager.Instance == null)
            {
                return false;
            }

            // Instantiate crop object
            GameObject cropObj = cropPrefab != null
                ? Instantiate(cropPrefab)
                : CreateFallbackCrop();
            cropObj.name = $"Crop_{cropData.cropID}_{gridPos}";

            CropInstance crop = cropObj.GetComponent<CropInstance>();
            if (crop == null)
            {
                DestroyImmediate(cropObj);
                return false;
            }

            // Initialize data & position before visual activation
            crop.SetCropData(cropData);
            crop.SetGridPosition(gridPos);

            // Calculate deterministic world position aligned to grid bottom
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
            float yBottom = worldPos.y - (GridManager.Instance.CellSize * 0.5f);
            float yPivotOffset = CalculatePivotOffset(cropData);
            cropObj.transform.position = new Vector3(worldPos.x, yBottom + yPivotOffset, worldPos.z);

            // Occupy grid cell if available (overlay-safe: skip silently if occupied)
            if (GridManager.Instance.IsPositionAvailable(gridPos))
                GridManager.Instance.TryOccupyCell(gridPos, cropObj);

            activeCrops[gridPos] = crop;
            return true;
        }

        public CropInstance GetCropAt(Vector2Int gridPos)
        {
            activeCrops.TryGetValue(gridPos, out CropInstance crop);
            return crop;
        }

        /// <summary>
        /// Removes a crop from the registry and releases its grid cell.
        /// </summary>
        public void RemoveCrop(Vector2Int gridPos)
        {
            if (!activeCrops.ContainsKey(gridPos)) return;

            if (GridManager.Instance != null)
                GridManager.Instance.ReleaseCell(gridPos);

            activeCrops.Remove(gridPos);
        }
        
        public void WaterCropAt(Vector2Int gridPos) => GetCropAt(gridPos)?.WaterCrop();

        /// <summary>
        /// Returns a shallow copy of all active crops for iteration (e.g. saving).
        /// </summary>
        public Dictionary<Vector2Int, CropInstance> GetAllCrops()
        {
            return new Dictionary<Vector2Int, CropInstance>(activeCrops);
        }

        /// <summary>
        /// Destroys all crop GameObjects and clears the registry.
        /// Uses DestroyImmediate for same-frame cleanup during load.
        /// </summary>
        public void ClearAllCrops()
        {
            foreach (var crop in activeCrops.Values)
            {
                if (crop != null && crop.gameObject != null)
                    DestroyImmediate(crop.gameObject);
            }
            activeCrops.Clear();
        }

        // ─── Helpers ───

        private GameObject CreateFallbackCrop()
        {
            var obj = new GameObject();
            obj.AddComponent<CropInstance>();
            return obj;
        }

        private float CalculatePivotOffset(CropData cropData)
        {
            if (cropData?.growthStageSprites == null || cropData.growthStageSprites.Length == 0) return 0f;

            Sprite s = cropData.growthStageSprites[0];
            if (s == null) return 0f;

            return (s.pivot.y / s.pixelsPerUnit) * 0.0625f;
        }
    }
}
