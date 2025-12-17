using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Systems;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// CropManager - Singleton untuk manage crop system.
    /// Handles planting, tracking, dan removal.
    /// </summary>
    public class CropManager : MonoBehaviour
    {
        public static CropManager Instance { get; private set; }
        
        [Header("Crop Prefab")]
        [SerializeField] private GameObject cropPrefab; // Prefab with VFX children
        
        // Active crops registry
        private Dictionary<Vector2Int, CropInstance> activeCrops = new Dictionary<Vector2Int, CropInstance>();
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy entire GameObject, not just component
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Plant crop at grid position
        /// </summary>
        public bool PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            // Validate GridManager exists
            if (GridManager.Instance == null)
            {
                Debug.LogError("[CropManager] GridManager not found!", this);
                return false;
            }
            
            // Issue 6 Fix: Validate FIRST, then create and setup crop
            
            // Step 1: Check if position is available (without occupying yet)
            if (!GridManager.Instance.IsPositionAvailable(gridPos))
            {
                Debug.LogWarning($"[CropManager] Cannot plant - cell {gridPos} already occupied or invalid");
                return false;
            }
            
            // Step 2: Create crop GameObject AFTER validation
            GameObject cropObj;
            if (cropPrefab != null)
            {
                cropObj = Instantiate(cropPrefab);
                cropObj.name = $"Crop_{cropData.cropID}_{gridPos}";
            }
            else
            {
                // Fallback if prefab not assigned
                cropObj = new GameObject($"Crop_{cropData.cropID}_{gridPos}");
                cropObj.AddComponent<CropInstance>();
            }
            
            CropInstance crop = cropObj.GetComponent<CropInstance>();
            if (crop == null)
            {
                Debug.LogError("[CropManager] CropInstance component not found on prefab!");
                Destroy(cropObj); // Cleanup
                return false;
            }
            
            // Step 3: Setup crop data BEFORE activating (before position set)
            crop.SetCropData(cropData);
            crop.SetGridPosition(gridPos);
            
            // Step 4: Calculate and set world position
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
            cropObj.transform.position = worldPos;
            
            // Step 5: Occupy cell AFTER crop is fully setup
            if (!GridManager.Instance.TryOccupyCell(gridPos, cropObj))
            {
                Debug.LogError($"[CropManager] Failed to occupy cell {gridPos} - destroying crop");
                Destroy(cropObj);
                return false;
            }
            
            // Step 6: Register crop in tracking dictionary
            activeCrops[gridPos] = crop;
            
            Debug.Log($"[CropManager] Planted {cropData.displayName} at {gridPos}");
            return true;
        }
        
        /// <summary>
        /// Get crop at grid position
        /// </summary>
        public CropInstance GetCropAt(Vector2Int gridPos)
        {
            activeCrops.TryGetValue(gridPos, out CropInstance crop);
            return crop;
        }
        
        /// <summary>
        /// Remove crop from registry (called on harvest/destroy)
        /// </summary>
        public void RemoveCrop(Vector2Int gridPos)
        {
            if (activeCrops.ContainsKey(gridPos))
            {
                // Release grid cell
                if (GridManager.Instance != null)
                {
                    GridManager.Instance.ReleaseCell(gridPos);
                }
                
                activeCrops.Remove(gridPos);
            }
        }
        
        /// <summary>
        /// Water crop at position
        /// </summary>
        public void WaterCropAt(Vector2Int gridPos)
        {
            CropInstance crop = GetCropAt(gridPos);
            if (crop != null)
            {
                crop.WaterCrop();
            }
        }
        
        /// <summary>
        /// Get all active crops (for debugging/stats)
        /// </summary>
        public Dictionary<Vector2Int, CropInstance> GetAllCrops()
        {
            return new Dictionary<Vector2Int, CropInstance>(activeCrops);
        }
        
        /// <summary>
        /// Clear all crops (for testing/reset)
        /// </summary>
        public void ClearAllCrops()
        {
            foreach (var crop in activeCrops.Values)
            {
                if (crop != null)
                {
                    Destroy(crop.gameObject);
                }
            }
            activeCrops.Clear();
        }
    }
}
