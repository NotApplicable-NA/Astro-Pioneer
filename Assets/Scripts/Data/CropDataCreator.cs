#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AstroPioneer.Data
{
    /// <summary>
    /// Editor helper untuk create CropData ScriptableObject instances.
    /// Mengacu pada Appendix B: Data Balancing dari GDD v2.1
    /// </summary>
    public static class CropDataCreator
    {
        [MenuItem("Astro-Pioneer/Create Crop Data/Space Potato (CRP_001)")]
        public static void CreateSpacePotato()
        {
            CropData cropData = ScriptableObject.CreateInstance<CropData>();
            
            // Data sesuai GDD v2.1 Appendix B
            cropData.cropID = "CRP_001";
            cropData.cropName = "Space Potato";
            cropData.growthTimeSeconds = 120f; // 2 menit
            cropData.seedCost = 10;
            cropData.sellPrice = 18;
            
            // Save asset
            string path = "Assets/Data/Crops/CRP_001_SpacePotato.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(cropData, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[CropDataCreator] Created: {path}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cropData;
        }
        
        [MenuItem("Astro-Pioneer/Create Crop Data/Neon Carrot (CRP_002)")]
        public static void CreateNeonCarrot()
        {
            CropData cropData = ScriptableObject.CreateInstance<CropData>();
            
            // Data sesuai GDD v2.1 Appendix B
            cropData.cropID = "CRP_002";
            cropData.cropName = "Neon Carrot";
            cropData.growthTimeSeconds = 300f; // 5 menit
            cropData.seedCost = 25;
            cropData.sellPrice = 60;
            
            // Save asset
            string path = "Assets/Data/Crops/CRP_002_NeonCarrot.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(cropData, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[CropDataCreator] Created: {path}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cropData;
        }
    }
}
#endif


