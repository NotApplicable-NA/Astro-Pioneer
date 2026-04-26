using UnityEngine;
using UnityEditor;
using AstroPioneer.Data;
using System.IO;

namespace AstroPioneer.Editor
{
    public class TransportDebugTool
    {
        [MenuItem("Tools/AstroPioneer/Create Basic Items")]
        public static void CreateBasicItems()
        {
            EnsureFolderExists("Assets/Data/Items");

            CreateItem("Item_Water", "Water", "water", 64, "Essential for life.");
            CreateItem("Item_Energy", "Energy Cell", "energy_cell", 100, "Stored power.");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateItem(string fileName, string displayName, string id, int stackSize, string desc)
        {
            string path = $"Assets/Data/Items/{fileName}.asset";
            if (File.Exists(path))
            {
                return;
            }

            InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
            newItem.displayName = displayName;
            newItem.maxStackSize = stackSize;
            newItem.description = desc;
            // Note: Icon will be null, user still needs to assign it if they care about UI

            AssetDatabase.CreateAsset(newItem, path);
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
