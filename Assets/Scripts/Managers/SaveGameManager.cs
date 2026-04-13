using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AstroPioneer.Data;
using AstroPioneer.Machines;
using AstroPioneer.Managers;
using AstroPioneer.Systems;

namespace AstroPioneer.Managers
{
    #region Save Data Structures

    [System.Serializable]
    public class MachineData
    {
        public string itemID;
        public Vector2Int position;
        public string cropID;
        public string uniqueInstanceID;
    }

    [System.Serializable]
    public class CropSaveData
    {
        public string cropID;
        public Vector2Int position;
        public int currentStage;
        public bool isWatered;
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public int slotIndex;
        public string itemID;
        public int quantity;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<MachineData> machines = new List<MachineData>();
        public List<CropSaveData> crops = new List<CropSaveData>();
        public List<InventorySlotData> inventory = new List<InventorySlotData>();
    }

    #endregion

    /// <summary>
    /// SaveGameManager — Handles full game state serialization via PlayerPrefs.
    /// Hotkeys: K = Save, L = Load, Shift+K = Clear save data.
    /// </summary>
    public class SaveGameManager : MonoBehaviour
    {
        public static SaveGameManager Instance { get; private set; }

        [Header("Registries")]
        [Tooltip("All prefabs with MachineIDTag for spawning on load.")]
        public List<GameObject> placeablePrefabs = new List<GameObject>();

        [Tooltip("All CropData ScriptableObjects.")]
        public List<CropData> allCropsRegistry = new List<CropData>();

        [Tooltip("All InventoryItem ScriptableObjects.")]
        public List<InventoryItem> allItemsRegistry = new List<InventoryItem>();

        private const string SAVE_KEY = "AstroPioneer_SaveData";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => LoadGame();
        void OnApplicationQuit() => SaveGame();

        void Update()
        {
            // Shift+K: Clear save data (check first to avoid triggering normal save)
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
            {
                PlayerPrefs.DeleteKey(SAVE_KEY);
                PlayerPrefs.Save();
                return;
            }

            if (Input.GetKeyDown(KeyCode.K)) SaveGame();
            if (Input.GetKeyDown(KeyCode.L)) LoadGame();
        }

        // ─────────────────────────── SAVE ───────────────────────────

        public void SaveGame()
        {
            if (GridManager.Instance == null) return;

            SaveData data = new SaveData();
            SaveMachines(data);
            SaveCrops(data);
            SaveInventory(data);

            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data, true));
            PlayerPrefs.Save();
        }

        private void SaveMachines(SaveData data)
        {
            var savedIDs = new HashSet<string>();

            foreach (MachineIDTag tag in FindObjectsOfType<MachineIDTag>())
            {
                if (tag == null || string.IsNullOrEmpty(tag.itemID)) continue;

                tag.EnsureUniqueID();
                if (!savedIDs.Add(tag.uniqueInstanceID)) continue; // Skip duplicate GUID

                AgriMech mech = tag.GetComponent<AgriMech>();
                data.machines.Add(new MachineData
                {
                    itemID        = tag.itemID,
                    position      = mech != null ? mech.currentGridPos : tag.originGridPos,
                    cropID        = mech != null && mech.cropData != null ? mech.cropData.cropID : "",
                    uniqueInstanceID = tag.uniqueInstanceID
                });
            }
        }

        private void SaveCrops(SaveData data)
        {
            if (CropManager.Instance == null) return;

            foreach (var kvp in CropManager.Instance.GetAllCrops())
            {
                CropInstance crop = kvp.Value;
                if (crop == null || crop.GetCropData() == null) continue;

                data.crops.Add(new CropSaveData
                {
                    cropID       = crop.GetCropData().cropID,
                    position     = crop.GetGridPosition(),
                    currentStage = crop.GetCurrentStage(),
                    isWatered    = crop.GetIsWatered()
                });
            }
        }

        private void SaveInventory(SaveData data)
        {
            if (InventoryManager.Instance == null) return;

            var slots = InventoryManager.Instance.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty) continue;
                data.inventory.Add(new InventorySlotData
                {
                    slotIndex = i,
                    itemID    = slots[i].item.id,
                    quantity  = slots[i].quantity
                });
            }
        }

        // ─────────────────────────── LOAD ───────────────────────────

        public void LoadGame()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY) || GridManager.Instance == null)
            {
                return;
            }

            CleanupScene();

            SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY));

            int machines  = LoadMachines(data);
            int crops     = LoadCrops(data);
            int items     = LoadInventory(data);
        }

        /// <summary>
        /// Destroys all existing machines, orphan AgriMechs, and crops before loading fresh data.
        /// Uses DestroyImmediate to prevent deferred-destroy race conditions within the same frame.
        /// </summary>
        private void CleanupScene()
        {
            // Destroy all tagged machines
            foreach (MachineIDTag tag in FindObjectsOfType<MachineIDTag>())
            {
                if (tag != null && tag.gameObject != null)
                    DestroyImmediate(tag.gameObject);
            }

            // Destroy orphan AgriMechs without MachineIDTag (e.g. scene-placed debug objects)
            foreach (AgriMech mech in FindObjectsOfType<AgriMech>())
            {
                if (mech != null && mech.gameObject != null)
                {
                    DestroyImmediate(mech.gameObject);
                }
            }

            GridManager.Instance.ClearAllCells();

            if (CropManager.Instance != null)
                CropManager.Instance.ClearAllCrops();
        }

        private int LoadMachines(SaveData data)
        {
            var loadedIDs = new HashSet<string>();
            int count = 0;

            foreach (var mData in data.machines)
            {
                // GUID-based duplicate skip
                if (!string.IsNullOrEmpty(mData.uniqueInstanceID) && !loadedIDs.Add(mData.uniqueInstanceID))
                    continue;

                GameObject prefab = FindPrefab(mData.itemID);
                if (prefab == null)
                {
                    continue;
                }

                GameObject newObj = SpawnMachine(prefab, mData);
                RegisterMachineInGrid(newObj, prefab, mData.position);
                count++;
            }

            return count;
        }

        private GameObject FindPrefab(string itemID)
        {
            return placeablePrefabs.FirstOrDefault(p =>
            {
                var tag = p.GetComponent<MachineIDTag>();
                return tag != null && tag.itemID == itemID;
            });
        }

        private GameObject SpawnMachine(GameObject prefab, MachineData mData)
        {
            float cSize = GridManager.Instance.CellSize;
            MachineIDTag prefabTag = prefab.GetComponent<MachineIDTag>();
            Vector2 dims = prefabTag.dimensions == Vector2.zero ? Vector2.one : (Vector2)prefabTag.dimensions;

            Vector3 corner = GridManager.Instance.GridOrigin + new Vector3(mData.position.x * cSize, mData.position.y * cSize, 0);
            Vector3 offset = new Vector3(dims.x * 0.5f * cSize, dims.y * 0.5f * cSize, 0);

            GameObject obj = Instantiate(prefab, corner + offset, Quaternion.identity);

            // Restore tag data
            MachineIDTag tag = obj.GetComponent<MachineIDTag>();
            if (tag != null)
            {
                tag.originGridPos = mData.position;
                tag.uniqueInstanceID = !string.IsNullOrEmpty(mData.uniqueInstanceID)
                    ? mData.uniqueInstanceID
                    : System.Guid.NewGuid().ToString();
            }

            // Initialize AgriMech if applicable
            AgriMech mech = obj.GetComponent<AgriMech>();
            if (mech != null)
            {
                mech.Initialize(mData.position);
                if (!string.IsNullOrEmpty(mData.cropID))
                {
                    CropData crop = allCropsRegistry.FirstOrDefault(c => c.cropID == mData.cropID);
                    if (crop != null) mech.cropData = crop;
                }
            }

            return obj;
        }

        private void RegisterMachineInGrid(GameObject obj, GameObject prefab, Vector2Int origin)
        {
            MachineIDTag prefabTag = prefab.GetComponent<MachineIDTag>();
            Vector2 dims = prefabTag.dimensions == Vector2.zero ? Vector2.one : (Vector2)prefabTag.dimensions;
            int w = Mathf.RoundToInt(dims.x);
            int h = Mathf.RoundToInt(dims.y);

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    GridManager.Instance.TryOccupyCell(origin + new Vector2Int(x, y), obj);
        }

        private int LoadCrops(SaveData data)
        {
            if (data.crops.Count == 0 || CropManager.Instance == null) return 0;

            int count = 0;
            foreach (var cData in data.crops)
            {
                CropData template = allCropsRegistry.FirstOrDefault(c => c.cropID == cData.cropID);
                if (template == null)
                {
                    continue;
                }

                CropManager.Instance.PlantCrop(cData.position, template);
                CropInstance planted = CropManager.Instance.GetCropAt(cData.position);
                if (planted != null)
                {
                    planted.SetStageData(cData.currentStage, cData.isWatered);
                    count++;
                }
            }
            return count;
        }

        private int LoadInventory(SaveData data)
        {
            if (data.inventory.Count == 0 || InventoryManager.Instance == null) return 0;

            InventoryManager.Instance.ClearInventory();
            int count = 0;

            foreach (var iData in data.inventory)
            {
                InventoryItem item = allItemsRegistry.FirstOrDefault(i => i.id == iData.itemID);
                if (item != null)
                {
                    InventoryManager.Instance.SetSlot(iData.slotIndex, item, iData.quantity);
                    count++;
                }
            }
            return count;
        }
    }
}
