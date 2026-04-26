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

            // 3. Data-Driven Check for Interactables (No Physics2D)
            if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.ChunkRenderer>(out var chunkRenderer))
            {
                GameObject visual = chunkRenderer.GetVisualAt(gridPos);
                if (visual != null)
                {
                    IGridInteractable interactable = visual.GetComponentInChildren<IGridInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact(selectedItem);
                        return;
                    }
                }
            }

            if (selectedItem == null) return;

            // 4. V25.2 DOD Capability-Based Execution
            // We check for "What can this item do?" instead of "What category is this item?"

            // 4a. Capability: Placeable (Structures & Crops)
            if (selectedItem.placedStructure != null)
            {
                // If it's a crop, use the specialized planting logic (checks for hoed soil, etc.)
                if (selectedItem.placedStructure.isCrop)
                {
                    // V25.2: Use the structure data directly from the item
                    if (selectedItem.placedStructure is CropStructureData cropData)
                    {
                        PlantCrop(gridPos, cropData);
                    }
                    else
                    {
                        // Fallback for when data is not explicitly castable but marked as isCrop
                        PlantCrop(gridPos, GetFallbackCropData(selectedItem));
                    }
                }
                // Otherwise, it's a macro/micro structure (Generator, Pipe, Fence, etc.)
                else if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacingModeActive())
                {
                    if (PlacementManager.Instance.TryPlace(gridPos))
                    {
                        InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
                    }
                }
            }
            // 4b. Capability: Tool (Hoe, Watering Can)
            else if (selectedItem.isTool || selectedItem.toolAction != null)
            {
                if (selectedItem.toolAction != null)
                {
                    if (selectedItem.toolAction.Execute(gridPos, selectedItem, selectedHotbarIndex))
                    {
                        // Some tools might consume themselves, handled in Execute.
                    }
                }
            }
            // 4c. Capability: Consumable (Food, Medkit)
            else if (selectedItem.isConsumable)
            {
                Debug.Log($"[PlayerToolState] Consuming {selectedItem.displayName}...");
                // Add actual consumption logic here (Hunger/Health restore)
                InventoryManager.Instance?.RemoveItemAt(selectedHotbarIndex, 1);
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

            // Priority 1: Seeds (Visual feedback for planting)
            if (item.placedStructure != null && item.placedStructure.isCrop)
            {
                if (item.placedStructure == spacePotatoData) return ToolType.Seed_SpacePotato;
                if (item.placedStructure == neonCarrotData) return ToolType.Seed_NeonCarrot;
                return ToolType.Seed_SpacePotato; // Default
            }

            // Priority 2: Generic Tools
            if (item.isTool || item.toolAction != null)
            {
                return ToolType.WateringCan; // Visual feedback for generic tool usage
            }

            return ToolType.None;
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
