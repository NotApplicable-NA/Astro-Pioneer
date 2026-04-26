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
    /// V22: Tool actions now use the Strategy Pattern via ToolBehaviour ScriptableObjects.
    /// </summary>
    public class PlayerToolState : MonoBehaviour
    {
        public static PlayerToolState Instance { get; private set; }

        // Pre-allocated buffer for Physics2D NonAlloc queries (zero GC)
        private static readonly Collider2D[] overlapBuffer = new Collider2D[16];

        public static event Action<int> OnHotbarSelectionChanged;
        public static event Action<ToolType> OnToolChanged;

        [Header("Hotbar")]
        [SerializeField] private int hotbarSlotCount = 6;
        private int selectedHotbarIndex = -1;

        [Header("Fallback CropData")]
        [SerializeField] private CropStructureData spacePotatoData;
        [SerializeField] private CropStructureData neonCarrotData;

        [Header("VFX")]
        [SerializeField] private HarvestVFX harvestVFXPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Reset tool state on scene load to prevent residual hotbar selection
        /// from triggering placement/destruction at the player's spawn point.
        /// </summary>
        void Start()
        {
            selectedHotbarIndex = -1;
            OnHotbarSelectionChanged?.Invoke(-1);
            OnToolChanged?.Invoke(ToolType.None);
            PlacementManager.Instance?.ClearPlacement();
        }

        void OnEnable() 
        {
            MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryUpdated += HandleInventoryUpdated;
        }
        
        void OnDisable() 
        {
            MouseInteractionSystem.OnGridCellClicked -= HandleGridClick;
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryUpdated -= HandleInventoryUpdated;
        }

        private void HandleInventoryUpdated()
        {
            if (selectedHotbarIndex >= 0)
            {
                var item = GetSelectedItem();
                if (item == null)
                {
                    // Item was consumed completely, deselect to clear ghost
                    SelectHotbarSlot(selectedHotbarIndex);
                }
            }
        }

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
                PlacementManager.Instance?.ClearPlacement();
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

            // 1. Tool with ToolBehaviour — execute BEFORE harvest check
            //    (e.g., Hoe should destroy, not harvest)
            if (selectedItem != null && selectedItem.type == ItemType.Tool && selectedItem.toolAction != null)
            {
                if (selectedItem.toolAction.Execute(gridPos, selectedItem, selectedHotbarIndex))
                {
                    if (selectedItem.toolAction.consumesItem)
                        InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
                    return;
                }
            }

            // 2. Harvest mature crops (any tool / bare hand)
            if (CropManager.Instance != null && CropManager.Instance.TryHarvestCropAt(gridPos, out CropStructureData harvestedData))
            {
                if (harvestedData != null) HarvestCrop(gridPos, harvestedData);
                return;
            }

            // 3. Physics-based Check for Interactables (replaces GetOccupantAt)
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
            int hitCount = Physics2D.OverlapPointNonAlloc(worldPos, overlapBuffer);
            
            for (int i = 0; i < hitCount; i++)
            {
                var hit = overlapBuffer[i];
                IGridInteractable interactable = hit.GetComponentInParent<IGridInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(selectedItem);
                    return;
                }
            }

            if (selectedItem == null) return;

            // 4. Item-type actions (Seed, Crafted)
            switch (selectedItem.type)
            {
                case ItemType.Seed:
                    CropStructureData cropData = (selectedItem.placedStructure as CropStructureData) ?? GetFallbackCropData(selectedItem);
                    if (cropData != null) PlantCrop(gridPos, cropData);
                    break;

                case ItemType.Crafted:
                    if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacingModeActive())
                        if (PlacementManager.Instance.TryPlace(gridPos))
                            InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
                    break;

                // ItemType.Tool is already handled above via ToolBehaviour
            }
        }

        // ─── Actions ───

        private void PlantCrop(Vector2Int gridPos, CropStructureData cropData)
        {
            if (cropData == null || CropManager.Instance == null) return;

            if (CropManager.Instance.PlantCrop(gridPos, cropData))
                InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
        }

        private void HarvestCrop(Vector2Int gridPos, CropStructureData data)
        {
            if (data == null) return;

            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(gridPos);
            if (harvestVFXPrefab != null && ObjectPoolManager.Instance != null)
            {
                var vfxObj = ObjectPoolManager.Instance.SpawnFromPool(Core.GameConstants.POOL_HARVEST_VFX, harvestVFXPrefab.gameObject, worldPos);
                vfxObj.GetComponent<HarvestVFX>()?.PlayAtPosition(vfxObj.transform.position);
            }

            if (InventoryManager.Instance != null && data.harvestItem != null)
                InventoryManager.Instance.AddItem(data.harvestItem, data.harvestQuantity);
        }

        // ─── Helpers ───

        private ToolType MapItemToToolType(InventoryItem item)
        {
            if (item == null) return ToolType.None;
            switch (item.type)
            {
                case ItemType.Seed:
                    if (item.placedStructure == spacePotatoData) return ToolType.Seed_SpacePotato;
                    if (item.placedStructure == neonCarrotData) return ToolType.Seed_NeonCarrot;
                    return ToolType.Seed_SpacePotato;
                case ItemType.Tool:
                    return ToolType.WateringCan;
                default:
                    return ToolType.None;
            }
        }

        private CropStructureData GetFallbackCropData(InventoryItem item)
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
