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
            // Validate dengan GridManager
            if (GridManager.Instance == null)
            {
                Debug.LogError("[CropManager] GridManager not found!", this);
                return false;
            }
            
            // Check if position is available
            if (!GridManager.Instance.IsPositionAvailable(gridPos))
            {
                Debug.LogWarning($"[CropManager] Position {gridPos} not available!", this);
                return false;
            }
            
            // Check if already has crop
            if (activeCrops.ContainsKey(gridPos))
            {
                Debug.LogWarning($"[CropManager] Position {gridPos} already has crop!", this);
                return false;
            }
            
            // Create crop instance from prefab
            GameObject cropObj;
            if (cropPrefab != null)
            {
                // Use prefab (recommended - includes VFX children)
                cropObj = Instantiate(cropPrefab);
                cropObj.name = $"Crop_{cropData.cropID}_{gridPos}";
            }
            else
            {
                // Fallback: Create manually (old method)
                cropObj = new GameObject($"Crop_{cropData.cropID}_{gridPos}");
                cropObj.AddComponent<CropInstance>();
            }
            
            CropInstance crop = cropObj.GetComponent<CropInstance>();
            if (crop == null)
            {
                crop = cropObj.AddComponent<CropInstance>();
            }
            
            crop.SetGridPosition(gridPos);
            crop.SetCropData(cropData);
            
            // Position di world space
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
            cropObj.transform.position = worldPos;
            
            // Occupy grid cell
            GridManager.Instance.TryOccupyCell(gridPos, cropObj);
            
            // Register
            activeCrops[gridPos] = crop;
            
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
