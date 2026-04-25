using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Data
{
    [CreateAssetMenu(fileName = "Main Entity Registry", menuName = "AstroPioneer/Registry/Entity Registry")]
    public class EntityRegistry : ScriptableObject
    {
        // Bootstrapper needs to set this
        public static EntityRegistry Instance { get; set; }

        [Header("Entity Prefabs (Index = ID)")]
        public List<GameObject> entityPrefabs = new List<GameObject>();

        public void Initialize()
        {
            Instance = this;
        }

        public GameObject GetPrefab(int id)
        {
            if (id >= 0 && id < entityPrefabs.Count)
                return entityPrefabs[id];
            return null;
        }

        public ushort GetIDByType(GameObject prefab)
        {
            int index = entityPrefabs.IndexOf(prefab);
            return index >= 0 ? (ushort)index : (ushort)65535;
        }

        // ─── Runtime Tracking (V25 QA Requirement) ───
        
        private readonly Dictionary<string, Vector3> runtimePositions = new Dictionary<string, Vector3>();

        public void RegisterEntityPosition(string entityID, Vector3 pos)
        {
            runtimePositions[entityID] = pos;
        }

        public Vector3 GetEntityPosition(string entityID)
        {
            return runtimePositions.TryGetValue(entityID, out var pos) ? pos : Vector3.zero;
        }
    }
}
