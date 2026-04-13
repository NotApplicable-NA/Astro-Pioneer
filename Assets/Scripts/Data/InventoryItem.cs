using UnityEngine;

namespace AstroPioneer.Data
{
    [CreateAssetMenu(fileName = "NewInventoryItem", menuName = "AstroPioneer/Inventory/Item")]
    public class InventoryItem : ScriptableObject
    {
        [Header("Item Details")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        
        [Header("Stacking")]
        public bool isStackable = true;
        public int maxStackSize = 64;
        
        [Header("Type")]
        public ItemType type;

        [Header("Economy")]
        [Tooltip("Credits received when selling this item")]
        public int sellPrice = 10;
        [Tooltip("Credits required to buy this item (0 = not buyable)")]
        public int buyPrice = 0;

        [Header("Seed Data (only used if type == Seed)")]
        [Tooltip("The CropData to plant when this seed item is used.")]
        public CropData plantData;

        [Header("Placement Data (only used if type == Crafted)")]
        [Tooltip("Prefab to instantiate when placed on the grid.")]
        public GameObject placeablePrefab;
        [Tooltip("Grid space occupied by this object (e.g. 1x1, 2x2, 3x3)")]
        public Vector2Int dimensions = Vector2Int.one;
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
