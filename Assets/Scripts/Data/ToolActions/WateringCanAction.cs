using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Core;
using AstroPioneer.VFX;

namespace AstroPioneer.Data
{
    /// <summary>
    /// WateringCanAction — Waters a crop at the target grid cell and spawns VFX.
    /// Migrated from PlayerToolState.WaterCrop() to follow the Strategy Pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolAction_WateringCan", menuName = "AstroPioneer/Tools/Watering Can Action")]
    public class WateringCanAction : ToolBehaviour
    {
        [Header("Watering Can VFX")]
        [Tooltip("VFX prefab to spawn when watering. Drag WateringVFX prefab here.")]
        public WateringVFX wateringVFXPrefab;

        public override bool Execute(Vector2Int gridPos, InventoryItem sourceItem, int hotbarSlotIndex)
        {
            if (CropManager.Instance == null) return false;

            ushort cropID = CropManager.Instance.GetCropAt(gridPos);
            if (cropID == 0) return false; // Not a crop

            CropManager.Instance.WaterCropAt(gridPos);

            // Spawn VFX
            if (wateringVFXPrefab != null && GridManager.Instance != null && ObjectPoolManager.Instance != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
                var vfxObj = ObjectPoolManager.Instance.SpawnFromPool(
                    GameConstants.POOL_WATERING_VFX,
                    wateringVFXPrefab.gameObject,
                    worldPos);
                vfxObj.GetComponent<WateringVFX>()?.PlayAtPosition(vfxObj.transform.position);
            }

            return true;
        }
    }
}
