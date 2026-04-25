using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// CropStructureData - Data-driven crop definition inheriting from StructureData.
    /// Uses the Auto-ID system from StructureRegistry.
    /// </summary>
    [CreateAssetMenu(fileName = "New Crop", menuName = "AstroPioneer/Data/Crop Structure Data")]
    public class CropStructureData : StructureData
    {
        [Header("Growth Configuration")]
        [Tooltip("Number of days required to advance each stage (4 stages: Seed, Sprout, Mature, Harvestable)")]
        public int[] daysToGrowPerStage = new int[4] { 1, 1, 1, 1 }; // Default: 1 day per stage
        
        [Tooltip("Water required per growth cycle")]
        public bool requiresWater = true;
        
        [Header("Harvest Configuration")]
        public InventoryItem harvestItem;
        
        [Tooltip("Quantity per harvest")]
        public int harvestQuantity = 1;

        // Validation
        void OnValidate()
        {
            // Array length validation
            if (daysToGrowPerStage.Length != 4)
            {
                System.Array.Resize(ref daysToGrowPerStage, 4);
            }
            
            // QA Fix: Value validation
            for (int i = 0; i < daysToGrowPerStage.Length; i++)
            {
                if (daysToGrowPerStage[i] <= 0)
                {
                    daysToGrowPerStage[i] = 1;
                }
            }
            
            if (harvestQuantity <= 0)
            {
                harvestQuantity = 1;
            }
            
            if (sellPrice < 0)
            {
                sellPrice = 0;
            }
        }
    }
}
