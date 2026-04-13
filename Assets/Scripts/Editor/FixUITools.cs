using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FixUITools : EditorWindow
{
    [MenuItem("Tools/AstroPioneer/Fix UI Issues")]
    public static void CheckAndFixUI()
    {
        // 1. Check EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 2. Fix InventoryPanel Layout
        GameObject invPanel = GameObject.Find("InventoryPanel");
        if (invPanel != null)
        {
            // Uncheck Raycast Target on Panel Background
            Image bg = invPanel.GetComponent<Image>();
            if (bg != null) 
            {
                bg.raycastTarget = false;
            }

            // Fix Vertical Layout -> Grid
            Transform container = invPanel.transform.Find("SlotContainer");
            if (container != null)
            {
                GridLayoutGroup grid = container.GetComponent<GridLayoutGroup>();
                if (grid == null) grid = container.gameObject.AddComponent<GridLayoutGroup>();
                
                grid.cellSize = new Vector2(64, 64);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5; // 5 Columns
            }
        }

        // 3. Fix StoragePanel Raycast Blocking
        GameObject storePanel = GameObject.Find("StoragePanel");
        if (storePanel != null)
        {
            Image bg = storePanel.GetComponent<Image>();
            if (bg != null) 
            {
                bg.raycastTarget = true; // Storage needs to block clicks to world, but allow child buttons
                // Ideally, panel blocks world clicks. Buttons are on top.
                // If CloseButton is blocked, it means something is ON TOP of StoragePanel.
                // InventoryPanel might be rendering on top.
            }
        }
        
        // 4. Fix Slot Prefab "99" text
        string[] guids = AssetDatabase.FindAssets("Slot_Prefab t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // Edit Prefab text to empty
            Text txt = prefab.GetComponentInChildren<Text>();
            if (txt != null)
            {
                Undo.RecordObject(txt, "Fix Prefab Text");
                txt.text = ""; // Clear "99"
                EditorUtility.SetDirty(prefab);
            }
            
            // Ensure Image on Prefab root
            if (prefab.GetComponent<Image>() == null)
            {
                prefab.AddComponent<Image>().color = new Color(1,1,1,0); // Invisible but clickable
            }
        }
    }
}
