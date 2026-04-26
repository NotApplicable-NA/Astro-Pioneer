using UnityEngine;

namespace AstroPioneer.Data
{
    [CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "AstroPioneer/Crafting/Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Recipe Identity")]
        public string recipeID;
        public string displayName;
        [TextArea] public string description;

        [Header("Ingredients")]
        public Ingredient[] ingredients;

        [Header("Result")]
        public InventoryItem resultItem;
        public int resultQuantity = 1;

        [Header("Crafting Settings")]
        [Tooltip("Time in seconds to craft this recipe")]
        public float craftTime = 2f;
        
        [Tooltip("Which station is required (None = hand craft)")]
        public CraftingStation requiredStation = CraftingStation.Hand;

        [Header("Unlock")]
        [Tooltip("Trust points required to unlock this recipe (0 = always available)")]
        public int trustRequired = 0;

        void OnValidate()
        {
            if (resultQuantity <= 0)
            {
                resultQuantity = 1;
            }
            if (craftTime < 0)
            {
                craftTime = 0f;
            }
        }
    }

    [System.Serializable]
    public class Ingredient
    {
        public InventoryItem item;
        public int quantity = 1;
    }

    public enum CraftingStation
    {
        Hand,           // No station needed
        ProcessingStation
    }
}
