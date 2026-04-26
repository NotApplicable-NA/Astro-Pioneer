using UnityEngine;

namespace AstroPioneer.Data
{
    [CreateAssetMenu(fileName = "NewInventoryItem", menuName = "AstroPioneer/Inventory/Item")]
    public class InventoryItem : ScriptableObject
    {
        [Header("Item Details")]
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        
        [Header("Stacking")]
        public bool isStackable = true;
        public int maxStackSize = 64;
        
        [Header("Grid Layering")]
        [Tooltip("If true, placed on the utility Micro-Grid (e.g. cables, pipes) instead of the Macro-Grid.")]
        public bool isMicroGridItem = false;

        [Header("Entity (V23)")]
        [Tooltip("If true, this is a free-moving entity (Bot, Vehicle) — NOT a grid structure. Spawned via EntityManager, NOT written to chunk data.")]
        public bool isEntity = false;
        
        [Header("Type")]
        public ItemType type;

        [Header("Tool Behaviour (V22 Strategy Pattern)")]
        [Tooltip("Drag a ToolBehaviour ScriptableObject here. Only used when type == Tool.")]
        public ToolBehaviour toolAction;
        
        [Header("Data-Driven Architecture (V20)")]
        [Tooltip("The StructureData associated with this item. Replaces legacy Prefab, CropData, and manual ID.")]
        public StructureData placedStructure;
        
        [Header("Economy")]
        [Tooltip("Credits received when selling this item")]
        public int sellPrice = 10;
        [Tooltip("Credits required to buy this item (0 = not buyable)")]
        public int buyPrice = 0;

        /// <summary>
        /// Gets the dynamic auto-ID from the StructureRegistry. Returns 0 if not found.
        /// </summary>
        public ushort StructureID => placedStructure != null && StructureRegistry.Instance != null 
            ? StructureRegistry.Instance.GetID(placedStructure) 
            : AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY;
    }

    public enum ItemType
    {
        Resource,
        Tool,
        Consumable,
        Seed,
        Crafted,
        Material    // Planet-gathered resources (ores, crystals)
    }
}
