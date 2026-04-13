using UnityEngine;
using System;
using AstroPioneer.Systems;
using AstroPioneer.Managers;
using AstroPioneer.Data;
using AstroPioneer.VFX;
using AstroPioneer.Systems.Husbandry;
using AstroPioneer.Interfaces;

namespace AstroPioneer.Player
{
    /// <summary>
    /// PlayerToolState — Manages hotbar selection and dispatches grid click actions
    /// (planting, watering, harvesting, placing, destroying).
    /// </summary>
    public class PlayerToolState : MonoBehaviour
    {
        public static PlayerToolState Instance { get; private set; }

        public static event Action<int> OnHotbarSelectionChanged;
        public static event Action<ToolType> OnToolChanged;

        [Header("Hotbar")]
        [SerializeField] private int hotbarSlotCount = 6;
        private int selectedHotbarIndex = -1;

        [Header("Fallback CropData")]
        [SerializeField] private CropData spacePotatoData;
        [SerializeField] private CropData neonCarrotData;

        [Header("VFX")]
        [SerializeField] private WateringVFX wateringVFXPrefab;
        [SerializeField] private HarvestVFX harvestVFXPrefab;

        private WateringVFX wateringVFX;
        private HarvestVFX harvestVFX;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            wateringVFX = FindOrInstantiate(wateringVFXPrefab, "WateringVFX_Instance");
            harvestVFX = FindOrInstantiate(harvestVFXPrefab, "HarvestVFX_Instance");
        }

        void OnEnable() => MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
        void OnDisable() => MouseInteractionSystem.OnGridCellClicked -= HandleGridClick;

        void Update()
        {
            for (int i = 0; i < hotbarSlotCount; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectHotbarSlot(i);
        }

        // ─── Hotbar ───

        public void SelectHotbarSlot(int index)
        {
            if (selectedHotbarIndex == index)
            {
                selectedHotbarIndex = -1;
                OnHotbarSelectionChanged?.Invoke(-1);
                OnToolChanged?.Invoke(ToolType.None);
                return;
            }

            selectedHotbarIndex = index;
            OnHotbarSelectionChanged?.Invoke(index);

            var item = GetSelectedItem();
            PlacementManager.Instance?.SetPlacementItem(item);
            OnToolChanged?.Invoke(item != null ? MapItemToToolType(item) : ToolType.None);
        }

        public InventoryItem GetSelectedItem()
        {
            if (selectedHotbarIndex < 0 || InventoryManager.Instance == null) return null;
            var slot = InventoryManager.Instance.GetSlotAt(selectedHotbarIndex);
            return slot != null && !slot.IsEmpty ? slot.item : null;
        }

        public int GetSelectedHotbarIndex() => selectedHotbarIndex;

        // ─── Grid Click Handler ───

        void HandleGridClick(Vector2Int gridPos)
        {
            var selectedItem = GetSelectedItem();

            // 1. Hoe destruction (TODO: REMOVE BEFORE PUBLISH)
            if (selectedItem != null && selectedItem.type == ItemType.Tool && selectedItem.id.ToLower().Contains("hoe"))
            {
                if (TryHoeDestroy(gridPos)) return;
            }

            // 2. Harvest mature crops (any tool)
            if (CropManager.Instance != null)
            {
                CropInstance crop = CropManager.Instance.GetCropAt(gridPos);
                if (crop != null && crop.IsHarvestable())
                {
                    HarvestCrop(crop);
                    return;
                }
            }

            // 3. IGridInteractable (machines, storage)
            GameObject occupant = GridManager.Instance.GetOccupantAt(gridPos);
            if (occupant != null)
            {
                IGridInteractable interactable = occupant.GetComponent<IGridInteractable>()
                    ?? occupant.GetComponentInParent<IGridInteractable>();
                if (interactable != null) { interactable.Interact(selectedItem); return; }
            }

            // 4. Micro-grid interactables (pipes, fences)
            foreach (var micro in GridManager.Instance.GetMicroOccupantsAt(gridPos))
            {
                IGridInteractable interactable = micro.GetComponent<IGridInteractable>();
                if (interactable != null) { interactable.Interact(selectedItem); return; }
            }

            if (selectedItem == null) return;

            // 5. Item-type actions
            switch (selectedItem.type)
            {
                case ItemType.Seed:
                    CropData cropData = selectedItem.plantData ?? GetFallbackCropData(selectedItem);
                    if (cropData != null) PlantCrop(gridPos, cropData);
                    break;

                case ItemType.Tool:
                    if (!selectedItem.id.ToLower().Contains("hoe"))
                        WaterCrop(gridPos);
                    break;

                case ItemType.Crafted:
                    if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacingModeActive())
                        if (PlacementManager.Instance.TryPlace(gridPos))
                            InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
                    break;
            }
        }

        // ─── Actions ───

        private bool TryHoeDestroy(Vector2Int gridPos)
        {
            // Machine detection: occupant or footprint scan
            MachineIDTag tag = null;
            GameObject occupant = GridManager.Instance.GetOccupantAt(gridPos);

            if (occupant != null) tag = occupant.GetComponent<MachineIDTag>();
            if (tag == null) tag = MouseInteractionSystem.FindMachineAtCell(gridPos);

            if (tag != null)
            {
                AstroPioneer.Machines.AgriMech mech = tag.GetComponent<AstroPioneer.Machines.AgriMech>();
                Vector2Int basePos = mech != null ? mech.currentGridPos : tag.originGridPos;
                int w = Mathf.RoundToInt(tag.dimensions.x == 0 ? 1 : tag.dimensions.x);
                int h = Mathf.RoundToInt(tag.dimensions.y == 0 ? 1 : tag.dimensions.y);

                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        GridManager.Instance.ReleaseCell(basePos + new Vector2Int(x, y));

                Destroy(tag.gameObject);

                if (EnclosureSystem.Instance != null && tag.itemID.Contains("Fence"))
                    EnclosureSystem.Instance.ReevaluateEnclosuresAround(gridPos);

                return true;
            }

            // Crop destruction
            if (occupant == null && CropManager.Instance != null)
            {
                var c = CropManager.Instance.GetCropAt(gridPos);
                if (c != null) occupant = c.gameObject;
            }

            if (occupant != null)
            {
                CropInstance crop = occupant.GetComponentInParent<CropInstance>();
                if (crop != null)
                {
                    CropManager.Instance.RemoveCrop(gridPos);
                    Destroy(crop.gameObject);
                    return true;
                }
            }

            return false;
        }

        private void PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            if (cropData == null || CropManager.Instance == null) return;

            if (CropManager.Instance.PlantCrop(gridPos, cropData))
                InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
        }

        private void WaterCrop(Vector2Int gridPos)
        {
            if (CropManager.Instance == null) return;

            CropInstance crop = CropManager.Instance.GetCropAt(gridPos);
            if (crop == null) return;

            crop.WaterCrop();

            if (wateringVFX != null && GridManager.Instance != null)
                wateringVFX.PlayAtPosition(GridManager.Instance.GridToWorldPosition(gridPos));
        }

        private void HarvestCrop(CropInstance crop)
        {
            CropData data = crop.GetCropData();
            if (data == null) return;

            harvestVFX?.PlayAtPosition(crop.transform.position);

            if (InventoryManager.Instance != null && data.harvestItem != null)
                InventoryManager.Instance.AddItem(data.harvestItem, data.harvestQuantity);

            crop.Harvest();
        }

        // ─── Helpers ───

        private T FindOrInstantiate<T>(T prefab, string name) where T : MonoBehaviour
        {
            T found = FindObjectOfType<T>();
            if (found != null) return found;
            if (prefab == null) return null;

            GameObject obj = Instantiate(prefab.gameObject);
            obj.name = name;
            DontDestroyOnLoad(obj);
            return obj.GetComponent<T>();
        }

        private ToolType MapItemToToolType(InventoryItem item)
        {
            if (item == null) return ToolType.None;
            switch (item.type)
            {
                case ItemType.Seed:
                    if (item.plantData == spacePotatoData) return ToolType.Seed_SpacePotato;
                    if (item.plantData == neonCarrotData) return ToolType.Seed_NeonCarrot;
                    return ToolType.Seed_SpacePotato;
                case ItemType.Tool:
                    return ToolType.WateringCan;
                default:
                    return ToolType.None;
            }
        }

        private CropData GetFallbackCropData(InventoryItem item)
        {
            if (item.displayName.Contains("Potato")) return spacePotatoData;
            if (item.displayName.Contains("Carrot")) return neonCarrotData;
            return null;
        }

        // ─── Legacy API ───
        public ToolType GetCurrentTool() => GetSelectedItem() is var item && item != null ? MapItemToToolType(item) : ToolType.None;
        public void SetTool(ToolType tool) => OnToolChanged?.Invoke(tool);
    }
}
