using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Data
{
    /// <summary>
    /// ItemRegistry — Singleton ScriptableObject mapping indices to InventoryItem.
    /// The index of an InventoryItem in 'allItems' IS its ItemID (ushort).
    /// 
    /// THE GOLDEN RULE: NEVER remove or reorder items in 'allItems'.
    /// If an item is obsolete, set it to null.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemRegistry", menuName = "AstroPioneer/Data/Item Registry")]
    public class ItemRegistry : ScriptableObject
    {
        private static ItemRegistry instance;
        public static ItemRegistry Instance 
        {
            get 
            {
                if (instance == null)
                {
                    AstroPioneer.Core.ServiceLocator.TryGet<ItemRegistry>(out instance);
                }
                return instance;
            }
            set => instance = value;
        }

        [Header("All Items (Index = ID)")]
        [Tooltip("Index 0 MUST be left empty or assigned to a dummy [EMPTY] object. Do NOT reorder elements once save system is live.")]
        public List<InventoryItem> allItems = new List<InventoryItem>();

        // Runtime inverse lookup for fast ID retrieval O(1)
        private Dictionary<InventoryItem, ushort> inverseLookup;
        private bool isInitialized;

        private void EnsureInitialized()
        {
            if (isInitialized && inverseLookup != null) return;

            inverseLookup = new Dictionary<InventoryItem, ushort>();
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                {
                    inverseLookup[allItems[i]] = (ushort)i;
                }
            }

            if (Instance == null) Instance = this;
            
            isInitialized = true;
        }

        public void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Get an InventoryItem by its ushort ID. Returns null if not found or empty.
        /// </summary>
        public InventoryItem Get(ushort id)
        {
            EnsureInitialized();
            if (id < 0 || id >= allItems.Count) return null;
            return allItems[id];
        }

        /// <summary>
        /// Get the ID (Index) of an InventoryItem. O(1) time complexity.
        /// Returns 0 (Empty) if not found.
        /// </summary>
        public ushort GetID(InventoryItem item)
        {
            if (item == null) return 0;
            EnsureInitialized();
            if (inverseLookup.TryGetValue(item, out ushort id))
            {
                return id;
            }
            return 0; // Not registered
        }

        public void Rebuild()
        {
            isInitialized = false;
            EnsureInitialized();
        }
    }
}
