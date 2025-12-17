using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// CropData - ScriptableObject untuk define crop types.
    /// Contains growth stages, timing, yield, dan visual data.
    /// </summary>
    [CreateAssetMenu(fileName = "New Crop", menuName = "AstroPioneer/Crop Data")]
    public class CropData : ScriptableObject
    {
        [Header("Crop Identity")]
        [Tooltip("Unique crop ID (e.g., 'space_potato', 'neon_carrot')")]
        public string cropID;
        
        [Tooltip("Display name untuk UI")]
        public string displayName;
        
        [Header("Growth Configuration")]
        [Tooltip("Growth time per stage in seconds (4 stages: Seed, Sprout, Mature, Harvestable)")]
        public float[] growthTimePerStage = new float[4] { 30f, 30f, 30f, 30f }; // Default: 2 minutes total
        
        [Tooltip("Water required per growth cycle")]
        public bool requiresWater = true;
        
        [Header("Harvest Configuration")]
        [Tooltip("Item ID yang dihasilkan saat harvest")]
        public string harvestItemID;
        
        [Tooltip("Quantity per harvest")]
        public int harvestQuantity = 1;

        [Tooltip("Sell price of the harvested item")]
        public int sellPrice = 10;
        
        [Header("Visuals")]
        [Tooltip("Sprite index 0: Seed, 1: Seedling, 2: Mature, 3: Harvestable")]
        public Sprite[] growthStageSprites = new Sprite[4]; // Must have 4 sprites
        
        [Tooltip("Sorting layer untuk crop sprite")]
        public string sortingLayer = "Crops";
        
        [Tooltip("Order in layer")]
        public int orderInLayer = 0;
        
        // Validation
        void OnValidate()
        {
            // Array length validation
            if (growthTimePerStage.Length != 4)
            {
                Debug.LogWarning($"[CropData] {name}: Growth time array should have 4 stages!", this);
            }
            
            if (growthStageSprites.Length != 4)
            {
                Debug.LogWarning($"[CropData] {name}: Stage sprites array should have 4 sprites!", this);
            }
            
            // QA Fix: Value validation
            for (int i = 0; i < growthTimePerStage.Length; i++)
            {
                if (growthTimePerStage[i] <= 0)
                {
                    Debug.LogWarning($"[CropData] {name} - Growth time stage {i} must be > 0, setting to 1.0", this);
                    growthTimePerStage[i] = 1f;
                }
            }
            
            if (harvestQuantity <= 0)
            {
                Debug.LogWarning($"[CropData] {name} - Harvest quantity must be > 0, setting to 1", this);
                harvestQuantity = 1;
            }
            
            if (sellPrice < 0)
            {
                Debug.LogWarning($"[CropData] {name} - Sell price cannot be negative, setting to 0", this);
                sellPrice = 0;
            }
        }
    }
}
