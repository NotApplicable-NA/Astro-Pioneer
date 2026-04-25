using UnityEngine;
using UnityEditor;
using AstroPioneer.Data;
using AstroPioneer.Machines;
using AstroPioneer.Systems;
using System.IO;

namespace AstroPioneer.EditorUtilities
{
    public static class ContentGenerationSetup
    {
        [MenuItem("Astro-Pioneer/Generate Expansion Content (Sprint 8 & 9)")]
        public static void GenerateExpansionContent()
        {
            EnsureDirectories();
            GeneratePlanets();
            GenerateLateGameCrops();
            GenerateAdvancedItems();
            GenerateSpriteAsset(); // 👈 Baru: Generate sprite 1x1 Unit beneran!
            GenerateMachinePrefabs();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectories()
        {
            void SafelyCreateDir(string folder)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            SafelyCreateDir("Assets/Data/Planets");
            SafelyCreateDir("Assets/Data/Crops");
            SafelyCreateDir("Assets/Data/Items");
            SafelyCreateDir("Assets/Prefab/Machines");
            SafelyCreateDir("Assets/Prefab/Machines/Textures");
        }
        
        // ═══════════════════════════════════════════════════════
        //  ASSET GENERATION
        // ═══════════════════════════════════════════════════════
        
        private static void GenerateSpriteAsset()
        {
            string texPath = "Assets/Prefab/Machines/Textures/SolidBlock.png";
            if (!File.Exists(texPath))
            {
                Texture2D tex = new Texture2D(16, 16);
                Color[] colors = new Color[16 * 16];
                for(int i=0; i<colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                AssetDatabase.Refresh();
            }
            
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f; 
                importer.filterMode = FilterMode.Point; 
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }

        private static void GeneratePlanets()
        {
            CreatePlanet("Planet_Grassland", "P-101 (Starter)", "Pink Grassland with safe conditions.", BiomeType.Lush, 1.0f, 1, 0, false);
            CreatePlanet("Planet_Desert", "D-X2 (Blue Desert)", "A scorching desert rich with silicates and ores.", BiomeType.Desert, 1.5f, 3, 500, false);
            CreatePlanet("Planet_Canyons", "C-V0 (Shadow Canyons)", "Dark canyons with intense gravity and zero sunlight. Solar power fails here.", BiomeType.Volcanic, 2.0f, 5, 2000, true);
        }

        private static void CreatePlanet(string assetName, string displayName, string desc, BiomeType biome, float o2Drain, int difficulty, int trust, bool hasCanyons)
        {
            string path = $"Assets/Data/Planets/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<PlanetData>(path) != null) return;

            PlanetData p = ScriptableObject.CreateInstance<PlanetData>();
            p.planetID = assetName;
            p.displayName = displayName;
            p.description = desc;
            p.biome = biome;
            p.oxygenDrainRate = o2Drain;
            p.difficultyLevel = difficulty;
            p.trustRequired = trust;
            p.hasShadowCanyons = hasCanyons;
            p.sceneName = "SampleScene"; // using standard scene for testing
            
            AssetDatabase.CreateAsset(p, path);
        }

        private static void GenerateLateGameCrops()
        {
            CreateCrop("CropData_FluxBerry", "Flux Berry", new int[] { 1, 1, 2, 2 });
            CreateCrop("CropData_QuantumCorn", "Quantum Corn", new int[] { 2, 2, 2, 2 });
            CreateCrop("CropData_IronRoot", "Iron Root", new int[] { 3, 3, 3, 3 });
        }

        private static void CreateCrop(string assetName, string displayName, int[] times)
        {
            string path = $"Assets/Data/Crops/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<CropStructureData>(path) != null) return;

            CropStructureData c = ScriptableObject.CreateInstance<CropStructureData>();
            c.displayName = displayName;
            c.daysToGrowPerStage = times;
            c.sprites = new Sprite[4];

            AssetDatabase.CreateAsset(c, path);
            
            CreateInventoryItem($"Item_Seed_{assetName.Replace("CropData_", "")}", $"{displayName} Seed", "A high tier seed.", ItemType.Seed, 99, 10, 5);
            CreateInventoryItem($"Item_{assetName.Replace("CropData_", "")}", $"{displayName}", "High yield crop output.", ItemType.Resource, 99, 50, 15);
        }

        private static void GenerateAdvancedItems()
        {
            // Essential Tools (Starter Kit)
            CreateInventoryItem("Item_Hoe", "Cangkul (Hoe)", "A sturdy hoe for tilling soil and dismantling machines.", ItemType.Tool, 1, 0, 0);
            CreateInventoryItem("Item_WateringCan", "Watering Can", "Waters crops to help them grow.", ItemType.Tool, 1, 0, 0);

            // Tier 3 Machines
            CreateInventoryItem("Item_AgriMech", "Agri-Mech Drone", "Automated 2x2 farming rover.", ItemType.Crafted, 1, 500, 100);
            CreateInventoryItem("Item_Harvester", "Auto Harvester", "Scans 3x3 areas to pick crops.", ItemType.Crafted, 1, 600, 150);
            CreateInventoryItem("Item_Composter", "Bio-Composter", "Recycles organic waste.", ItemType.Crafted, 1, 250, 50);
            CreateInventoryItem("Item_MooLien", "Moo-Lien Calf", "A space cow for animal husbandry.", ItemType.Crafted, 5, 800, 200);
            CreateInventoryItem("Item_Fence", "Energy Fence", "Used to create enclosures.", ItemType.Crafted, 99, 10, 2);
            CreateInventoryItem("Item_BioFertilizer", "Bio-Fertilizer", "Accelerates growth.", ItemType.Resource, 99, 50, 10);
        }

        // ═══════════════════════════════════════════════════════
        //  PREFAB GENERATION
        // ═══════════════════════════════════════════════════════
        private static void GenerateMachinePrefabs()
        {
            // AgriMech — 2x2 rover planter
            CreateMachinePrefab<AgriMech>(
                "Prefab_AgriMech", "Item_AgriMech",
                new Vector2Int(2, 2), new Color(0.2f, 0.6f, 1f) // Blue
            );

            // Harvester — 1x1, scans 3x3
            CreateMachinePrefab<MachineHarvester>(
                "Prefab_Harvester", "Item_Harvester",
                new Vector2Int(1, 1), new Color(1f, 0.5f, 0.1f) // Orange
            );

            // Composter — 1x1, converts waste
            CreateMachinePrefab<MachineComposter>(
                "Prefab_Composter", "Item_Composter",
                new Vector2Int(1, 1), new Color(0.4f, 0.8f, 0.2f) // Green
            );

            // Fence — 1x1, for enclosures (no machine script, just a grid blocker)
            CreateMachinePrefab(
                "Prefab_Fence", "Item_Fence",
                new Vector2Int(1, 1), new Color(1f, 1f, 0.3f) // Yellow
            );
        }

        private static void SetupPrefabVisualsAndLink(GameObject go, string prefabPath, string itemID, Vector2Int dims, Color tint)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prefab/Machines/Textures/SolidBlock.png");
            
            // Fallback just in case
            if(customSprite == null) customSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            
            sr.sprite = customSprite;
            sr.color = tint;
            sr.sortingOrder = 5;

            // Karena kita pakai PPU 16 dengan tex 16x16, skala 1 = persis 1 unit grid (100% full!)
            go.transform.localScale = new Vector3(dims.x, dims.y, 1);
            go.AddComponent<BoxCollider2D>();

            // Configure Rigidbody2D (added by RequireComponent if script exists, or manually if needed)
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null && (itemID == "Item_AgriMech" || itemID == "Item_Harvester")) 
                rb = go.AddComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            }

            // MachineIDTag for save/load tracking
            MachineIDTag tag = go.AddComponent<MachineIDTag>();
            tag.itemID = itemID;
            tag.dimensions = dims;

            // Save as Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go); // Cleanup temp object

            // Link Prefab to InventoryItem (V20: via placedStructure)
            string itemPath = $"Assets/Data/Items/{itemID}.asset";
            InventoryItem item = AssetDatabase.LoadAssetAtPath<InventoryItem>(itemPath);
            if (item != null && item.placedStructure != null && prefab != null)
            {
                item.placedStructure.visualPrefab = prefab;
                item.placedStructure.dimensions = dims;
                EditorUtility.SetDirty(item.placedStructure);
                EditorUtility.SetDirty(item);
            }
        }

        private static void CreateMachinePrefab<T>(string prefabName, string itemID, Vector2Int dims, Color tint) where T : Component
        {
            string prefabPath = $"Assets/Prefab/Machines/{prefabName}.prefab";
            // Do not DeleteAsset — SaveAsPrefabAsset handles overwriting and prevents meta-file locks.

            GameObject go = new GameObject(prefabName);
            go.AddComponent<T>(); // Machine Script
            SetupPrefabVisualsAndLink(go, prefabPath, itemID, dims, tint);
        }

        private static void CreateMachinePrefab(string prefabName, string itemID, Vector2Int dims, Color tint)
        {
            string prefabPath = $"Assets/Prefab/Machines/{prefabName}.prefab";
            // Do not DeleteAsset — SaveAsPrefabAsset handles overwriting and prevents meta-file locks.

            GameObject go = new GameObject(prefabName);
            SetupPrefabVisualsAndLink(go, prefabPath, itemID, dims, tint);
        }

        private static void CreateInventoryItem(string assetName, string displayName, string desc, ItemType type, int maxStack, int buyPrice, int sellPrice)
        {
            string path = $"Assets/Data/Items/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<InventoryItem>(path) != null) return;

            InventoryItem i = ScriptableObject.CreateInstance<InventoryItem>();
            i.displayName = displayName;
            i.description = desc;
            i.type = type;
            i.maxStackSize = maxStack;
            i.buyPrice = buyPrice;
            i.sellPrice = sellPrice;
            
            AssetDatabase.CreateAsset(i, path);
        }
    }
}
