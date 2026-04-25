#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AstroPioneer.Data;

namespace AstroPioneer.Editor
{
    public static class Sprint6AssetGenerator
    {
        [MenuItem("AstroPioneer/Sprint 6/Generate All Assets")]
        public static void GenerateAll()
        {
            GenerateItems();
            GenerateRecipes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("AstroPioneer/Sprint 6/Generate Items Only")]
        public static void GenerateItems()
        {
            string itemsPath = "Assets/Data/Items/";
            EnsureFolder(itemsPath);

            // Crafted food items
            CreateItem(itemsPath, "Item_AstroStew", "astro_stew", "Astro Stew",
                "A hearty stew made from Space Potatoes. Restores energy.",
                ItemType.Crafted, sellPrice: 25, buyPrice: 0,
                iconPath: "Assets/Art/Sprites/Items/Icon_AstroStew.png");

            CreateItem(itemsPath, "Item_NeonSalad", "neon_salad", "Neon Salad",
                "A vibrant salad made from Neon Carrots. Light and refreshing.",
                ItemType.Crafted, sellPrice: 30, buyPrice: 0,
                iconPath: "Assets/Art/Sprites/Items/Icon_NeonSalad.png");

            CreateItem(itemsPath, "Item_NutrientPack", "nutrient_pack", "Nutrient Pack",
                "A premium meal combining Astro Stew and Neon Salad. Full nutrition.",
                ItemType.Crafted, sellPrice: 80, buyPrice: 0,
                iconPath: "Assets/Art/Sprites/Items/Icon_NutrientPack.png");

            // Refined resource
            CreateItem(itemsPath, "Item_BioFuelCell", "bio_fuel_cell", "Bio Fuel Cell",
                "Refined energy cell processed from Space Potatoes. Powers machines.",
                ItemType.Resource, sellPrice: 40, buyPrice: 60,
                iconPath: "Assets/Art/Sprites/Items/Icon_BioFuelCell.png");

            // Machine upgrade
            CreateItem(itemsPath, "Item_SprinklerMk2", "sprinkler_mk2", "Sprinkler Mk2",
                "An upgraded sprinkler with extended range and efficiency.",
                ItemType.Tool, sellPrice: 100, buyPrice: 150, isStackable: false, maxStack: 1,
                iconPath: "Assets/Art/Sprites/Items/Icon_SprinklerMk2.png");
        }

        [MenuItem("AstroPioneer/Sprint 6/Generate Recipes Only")]
        public static void GenerateRecipes()
        {
            string recipesPath = "Assets/Data/Recipes/";
            string itemsPath = "Assets/Data/Items/";
            EnsureFolder(recipesPath);
            EnsureFolder(itemsPath);

            // Create harvest items for crops if they don't exist
            CreateItem(itemsPath, "Item_SpacePotato", "space_potato", "Space Potato",
                "A hearty potato grown in space soil. Versatile cooking ingredient.",
                ItemType.Resource, sellPrice: 18, buyPrice: 25);

            CreateItem(itemsPath, "Item_NeonCarrot", "neon_carrot", "Neon Carrot",
                "A vibrant carrot with bioluminescent properties. Fresh and crunchy.",
                ItemType.Resource, sellPrice: 60, buyPrice: 85);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Load ingredient items by direct path
            var spacePotato = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_SpacePotato.asset");
            var neonCarrot = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_NeonCarrot.asset");
            var water = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Item_Water.asset");

            // Load result items
            var astroStew = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_AstroStew.asset");
            var neonSalad = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_NeonSalad.asset");
            var bioFuel = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_BioFuelCell.asset");
            var nutrientPack = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_NutrientPack.asset");
            var sprinklerMk2 = AssetDatabase.LoadAssetAtPath<InventoryItem>("Assets/Data/Items/Item_SprinklerMk2.asset");

            // Recipe 1: Astro Stew (Hand)
            CreateRecipe(recipesPath, "Recipe_AstroStew", "recipe_astro_stew", "Astro Stew",
                "Cook Space Potatoes into a hearty stew.",
                new Ingredient[] {
                    new Ingredient { item = spacePotato, quantity = 2 },
                    new Ingredient { item = water, quantity = 1 }
                },
                astroStew, 1, 3f, CraftingStation.Hand);

            // Recipe 2: Neon Salad (Hand)
            CreateRecipe(recipesPath, "Recipe_NeonSalad", "recipe_neon_salad", "Neon Salad",
                "Toss Neon Carrots into a fresh salad.",
                new Ingredient[] {
                    new Ingredient { item = neonCarrot, quantity = 2 },
                    new Ingredient { item = water, quantity = 1 }
                },
                neonSalad, 1, 3f, CraftingStation.Hand);

            // Recipe 3: Bio Fuel Cell (Processing)
            CreateRecipe(recipesPath, "Recipe_BioFuelCell", "recipe_bio_fuel", "Bio Fuel Cell",
                "Process Space Potatoes into refined energy.",
                new Ingredient[] {
                    new Ingredient { item = spacePotato, quantity = 3 }
                },
                bioFuel, 1, 5f, CraftingStation.ProcessingStation);

            // Recipe 4: Nutrient Pack (Hand)
            CreateRecipe(recipesPath, "Recipe_NutrientPack", "recipe_nutrient_pack", "Nutrient Pack",
                "Combine Astro Stew and Neon Salad into a complete meal.",
                new Ingredient[] {
                    new Ingredient { item = astroStew, quantity = 1 },
                    new Ingredient { item = neonSalad, quantity = 1 }
                },
                nutrientPack, 1, 4f, CraftingStation.Hand);

            // Recipe 5: Sprinkler Mk2 (Processing)
            CreateRecipe(recipesPath, "Recipe_SprinklerMk2", "recipe_sprinkler_mk2", "Sprinkler Mk2",
                "Upgrade a sprinkler using Bio Fuel Cells.",
                new Ingredient[] {
                    new Ingredient { item = bioFuel, quantity = 2 },
                    new Ingredient { item = water, quantity = 1 }
                },
                sprinklerMk2, 1, 6f, CraftingStation.ProcessingStation);
        }

        // ── Helpers ──────────────────────────────────

        private static void CreateItem(string path, string filename, string id, string displayName,
            string description, ItemType type, int sellPrice, int buyPrice,
            string iconPath = null, bool isStackable = true, int maxStack = 99)
        {
            string fullPath = path + filename + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<InventoryItem>(fullPath);
            if (existing != null)
            {
                return;
            }

            var item = ScriptableObject.CreateInstance<InventoryItem>();
            item.displayName = displayName;
            item.description = description;
            item.type = type;
            item.sellPrice = sellPrice;
            item.buyPrice = buyPrice;
            item.isStackable = isStackable;
            item.maxStackSize = maxStack;

            if (!string.IsNullOrEmpty(iconPath))
            {
                item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            }

            AssetDatabase.CreateAsset(item, fullPath);
        }

        private static void CreateRecipe(string path, string filename, string id, string displayName,
            string description, Ingredient[] ingredients, InventoryItem result, int resultQty,
            float craftTime, CraftingStation station, int trustRequired = 0)
        {
            string fullPath = path + filename + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<CraftingRecipe>(fullPath);
            if (existing != null)
            {
                return;
            }

            var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            recipe.recipeID = id;
            recipe.displayName = displayName;
            recipe.description = description;
            recipe.ingredients = ingredients;
            recipe.resultItem = result;
            recipe.resultQuantity = resultQty;
            recipe.craftTime = craftTime;
            recipe.requiredStation = station;
            recipe.trustRequired = trustRequired;

            AssetDatabase.CreateAsset(recipe, fullPath);
        }

        private static InventoryItem FindItem(string searchName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:InventoryItem {searchName}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<InventoryItem>(path);
                if (item != null) return item;
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            // Remove trailing slash
            string clean = path.TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(clean))
            {
                string[] parts = clean.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
#endif
