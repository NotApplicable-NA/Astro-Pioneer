using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Data
{
    /// <summary>
    /// StructureRegistry — Singleton ScriptableObject mapping indices to StructureData.
    /// The index of a StructureData in 'allStructures' IS its structureID.
    /// 
    /// THE GOLDEN RULE: NEVER remove or reorder items in 'allStructures'.
    /// If an item is obsolete, set it to null or a dummy [KOSONG] data.
    /// </summary>
    [CreateAssetMenu(fileName = "StructureRegistry", menuName = "AstroPioneer/Data/Structure Registry")]
    public class StructureRegistry : ScriptableObject
    {
        private static StructureRegistry instance;
        public static StructureRegistry Instance 
        {
            get 
            {
                if (instance == null)
                {
                    // Fallback to ServiceLocator if needed, but preferable to rely on ChunkRenderer's injected dependency.
                    AstroPioneer.Core.ServiceLocator.TryGet<StructureRegistry>(out instance);
                }
                return instance;
            }
            set => instance = value;
        }

        [Header("All Structure Definitions (Index = ID)")]
        [Tooltip("Index 0 MUST be left empty or assigned to a dummy [EMPTY] object. Do NOT reorder elements once save system is live.")]
        public List<StructureData> allStructures = new List<StructureData>();

        // Runtime inverse lookup for fast ID retrieval O(1)
        private Dictionary<StructureData, ushort> inverseLookup;
        private bool isInitialized;

        private void EnsureInitialized()
        {
            if (isInitialized && inverseLookup != null) return;

            inverseLookup = new Dictionary<StructureData, ushort>();
            for (int i = 0; i < allStructures.Count; i++)
            {
                if (allStructures[i] != null)
                {
                    inverseLookup[allStructures[i]] = (ushort)i;
                }
            }

            // Also register self to Singleton for easy access by non-MonoBehaviours like InventoryItem property
            if (Instance == null) Instance = this;
            
            isInitialized = true;
        }

        public void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Get a StructureData by its ushort ID. Returns null if not found or empty.
        /// </summary>
        public StructureData Get(ushort id)
        {
            EnsureInitialized();
            if (id < 0 || id >= allStructures.Count) return null;
            return allStructures[id];
        }

        /// <summary>
        /// Get the ID (Index) of a StructureData. O(1) time complexity.
        /// Returns 0 (Empty) if not found.
        /// </summary>
        public ushort GetID(StructureData data)
        {
            if (data == null) return 0;
            EnsureInitialized();
            if (inverseLookup.TryGetValue(data, out ushort id))
            {
                return id;
            }
            return 0; // Not registered
        }

        /// <summary>
        /// Check if an ID exists and is not null.
        /// </summary>
        public bool Contains(ushort id)
        {
            EnsureInitialized();
            return id >= 0 && id < allStructures.Count && allStructures[id] != null;
        }

        /// <summary>
        /// Get all registered entries.
        /// </summary>
        public IReadOnlyList<StructureData> GetAll() => allStructures;

        /// <summary>
        /// Check if the given ID corresponds to a Crop structure. O(1).
        /// Replaces the legacy hardcoded range check (id > 0 && id < 100).
        /// </summary>
        public bool IsCrop(ushort id)
        {
            EnsureInitialized();
            if (id == 0 || id >= allStructures.Count) return false;
            var data = allStructures[id];
            return data != null && data.category == StructureCategory.Crop;
        }

        /// <summary>
        /// Get the category of a structure by ID. Returns null if not found.
        /// </summary>
        public StructureCategory? GetCategory(ushort id)
        {
            EnsureInitialized();
            if (id == 0 || id >= allStructures.Count || allStructures[id] == null) return null;
            return allStructures[id].category;
        }

        public void Rebuild()
        {
            isInitialized = false;
            EnsureInitialized();
        }

        // ─── Runtime Tracking (V25 QA Requirement) ───

        private Vector2Int sleepPodPos;

        public void RegisterSleepPodPosition(Vector2Int pos)
        {
            sleepPodPos = pos;
        }

        public Vector2Int GetSleepPodPosition()
        {
            return sleepPodPos;
        }
    }
}
