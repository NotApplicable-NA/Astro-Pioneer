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
        
        [Header("Visual")]
        [Tooltip("Sprite per growth stage (4 sprites: Stage 0-3)")]
        public Sprite[] stageSprites = new Sprite[4];
        
        [Tooltip("Sorting layer untuk crop sprite")]
        public string sortingLayer = "Crops";
        
        [Tooltip("Order in layer")]
        public int orderInLayer = 0;
        
        // Validation
        void OnValidate()
        {
            if (growthTimePerStage.Length != 4)
            {
                Debug.LogWarning($"[CropData] {name}: Growth time array should have 4 stages!", this);
            }
            
            if (stageSprites.Length != 4)
            {
                Debug.LogWarning($"[CropData] {name}: Stage sprites array should have 4 sprites!", this);
            }
        }
    }
}
