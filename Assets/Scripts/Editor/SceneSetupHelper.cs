#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AstroPioneer.Managers;
using AstroPioneer.Systems;
using AstroPioneer.Player;

namespace AstroPioneer.Editor
{
    /// <summary>
    /// Editor helper untuk auto-setup GameObjects di scene.
    /// Memudahkan setup GridManager, MouseInteractionSystem, dan PlayerStatus.
    /// Bisa digunakan untuk create Prefab atau setup langsung di scene.
    /// </summary>
    public static class SceneSetupHelper
    {
        [MenuItem("Astro-Pioneer/Setup Scene/Create All Core Systems")]
        public static void CreateAllCoreSystems()
        {
            // Create GridManager
            GameObject gridManagerObj = CreateGridManager();
            
            // Create MouseInteractionSystem
            GameObject mouseInteractionObj = CreateMouseInteractionSystem();
            
            // Create Player with PlayerStatus
            GameObject playerObj = CreatePlayer();
            
            // Select semua yang baru dibuat
            Selection.objects = new Object[] { gridManagerObj, mouseInteractionObj, playerObj };
            
            Debug.Log("[SceneSetupHelper] Core systems created! GridManager, MouseInteractionSystem, and Player are ready.");
        }
        
        [MenuItem("Astro-Pioneer/Setup Scene/Create GridManager")]
        public static void CreateGridManager()
        {
            // Check if already exists
            GridManager existing = Object.FindObjectOfType<GridManager>();
            if (existing != null)
            {
                Debug.LogWarning("[SceneSetupHelper] GridManager already exists in scene!", existing);
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            
            GameObject gridManagerObj = new GameObject("GridManager");
            GridManager gridManager = gridManagerObj.AddComponent<GridManager>();
            
            // Set default position
            gridManagerObj.transform.position = Vector3.zero;
            
            Debug.Log("[SceneSetupHelper] GridManager created!");
            Selection.activeGameObject = gridManagerObj;
            
            return gridManagerObj;
        }
        
        [MenuItem("Astro-Pioneer/Setup Scene/Create MouseInteractionSystem")]
        public static void CreateMouseInteractionSystem()
        {
            // Check if already exists
            MouseInteractionSystem existing = Object.FindObjectOfType<MouseInteractionSystem>();
            if (existing != null)
            {
                Debug.LogWarning("[SceneSetupHelper] MouseInteractionSystem already exists in scene!", existing);
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            
            GameObject mouseInteractionObj = new GameObject("MouseInteractionSystem");
            MouseInteractionSystem mouseInteraction = mouseInteractionObj.AddComponent<MouseInteractionSystem>();
            
            // Auto-assign Main Camera jika ada
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // Use reflection to set private field (or make it public/protected)
                // For now, just log - user can assign manually
                Debug.Log("[SceneSetupHelper] Main Camera found. Please assign it in Inspector if needed.");
            }
            
            Debug.Log("[SceneSetupHelper] MouseInteractionSystem created!");
            Selection.activeGameObject = mouseInteractionObj;
            
            return mouseInteractionObj;
        }
        
        [MenuItem("Astro-Pioneer/Setup Scene/Create Player")]
        public static void CreatePlayer()
        {
            // Check if already exists
            PlayerStatus existing = Object.FindObjectOfType<PlayerStatus>();
            if (existing != null)
            {
                Debug.LogWarning("[SceneSetupHelper] Player already exists in scene!", existing);
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            
            GameObject playerObj = new GameObject("Player");
            PlayerStatus playerStatus = playerObj.AddComponent<PlayerStatus>();
            
            // Create Ship Spawn Point as child
            GameObject spawnPointObj = new GameObject("ShipSpawnPoint");
            spawnPointObj.transform.SetParent(playerObj.transform);
            spawnPointObj.transform.localPosition = Vector3.zero;
            
            // Assign spawn point (using reflection or make SetShipSpawnPoint public)
            playerStatus.SetShipSpawnPoint(spawnPointObj.transform);
            
            // Set default position
            playerObj.transform.position = Vector3.zero;
            
            Debug.Log("[SceneSetupHelper] Player created with ShipSpawnPoint!");
            Selection.activeGameObject = playerObj;
            
            return playerObj;
        }
        
        [MenuItem("Astro-Pioneer/Setup Scene/Create Prefabs Folder Structure")]
        public static void CreatePrefabsFolderStructure()
        {
            // Create folder structure untuk prefabs
            string[] folders = {
                "Assets/Prefabs",
                "Assets/Prefabs/Managers",
                "Assets/Prefabs/Systems",
                "Assets/Prefabs/Player"
            };
            
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parentFolder = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
                    string folderName = System.IO.Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetupHelper] Prefabs folder structure created!");
        }
        
        [MenuItem("Astro-Pioneer/Setup Scene/Create Prefab from Selected")]
        public static void CreatePrefabFromSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[SceneSetupHelper] No GameObject selected!");
                return;
            }
            
            // Determine folder based on component
            string folder = "Assets/Prefabs";
            if (selected.GetComponent<GridManager>() != null)
                folder = "Assets/Prefabs/Managers";
            else if (selected.GetComponent<MouseInteractionSystem>() != null)
                folder = "Assets/Prefabs/Systems";
            else if (selected.GetComponent<PlayerStatus>() != null)
                folder = "Assets/Prefabs/Player";
            
            // Create prefab
            string path = $"{folder}/{selected.name}.prefab";
            Object prefab = PrefabUtility.SaveAsPrefabAsset(selected, path);
            
            Debug.Log($"[SceneSetupHelper] Prefab created: {path}");
            Selection.activeObject = prefab;
        }
    }
}
#endif


