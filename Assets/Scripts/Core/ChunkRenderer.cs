using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Machines.Automation;

namespace AstroPioneer.Core
{
    /// <summary>
    /// ChunkRenderer — Bridges DATA to VISUAL.
    /// V24.9: Support for separate EntityRegistry for "Reincarnation".
    /// </summary>
    public class ChunkRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StructureRegistry structureRegistry;
        [SerializeField] private EntityRegistry entityRegistry;

        private readonly Dictionary<ChunkCoord, List<SpawnedVisual>> visualMap
            = new Dictionary<ChunkCoord, List<SpawnedVisual>>();
        private readonly Stack<List<SpawnedVisual>> visualListPool = new Stack<List<SpawnedVisual>>();

        void Awake()
        {
            ServiceLocator.Register(this);
        }

        void OnDestroy()
        {
            ServiceLocator.Unregister<ChunkRenderer>();
        }

        public void OnChunkLoaded(Chunk chunk)
        {
            if (chunk == null) return;

            var visuals = visualListPool.Count > 0 ? visualListPool.Pop() : new List<SpawnedVisual>(128);
            visuals.Clear();
            int size = GameConstants.CHUNK_SIZE;

            // 1. Spawn Structures
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    SpawnLayerVisual(chunk, chunk.FloorLayer, x, y, visuals);
                    SpawnLayerVisual(chunk, chunk.UtilityLayer, x, y, visuals);
                    SpawnLayerVisual(chunk, chunk.StructureLayer, x, y, visuals);
                }
            }

            // 2. Spawn Bots (V24.9 Data-Driven)
            if (BotSimulationManager.Instance != null)
            {
                var bots = BotSimulationManager.Instance.GetBotsInChunk(chunk.Coord);
                foreach (var b in bots) SpawnBotVisual(b, visuals);
            }

            visualMap[chunk.Coord] = visuals;
        }

        public void OnChunkUnloaded(ChunkCoord coord)
        {
            if (!visualMap.TryGetValue(coord, out var visuals)) return;

            if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
            {
                foreach (var v in visuals)
                {
                    if (v.go != null) pool.ReturnToPool(v.structureID, v.go);
                }
            }

            visuals.Clear();
            visualMap.Remove(coord);
            visualListPool.Push(visuals);
        }

        private void SpawnBotVisual(BotData data, List<SpawnedVisual> visuals)
        {
            if (entityRegistry == null) 
            {
                Debug.LogWarning("[ChunkRenderer] entityRegistry is null!");
                return;
            }

            // V24.9: Use the restored original Registry API
            GameObject prefab = entityRegistry.GetPrefab(data.entityTypeID);
            if (prefab == null) return;

            Vector3 worldPos = new Vector3(data.currentPos.x, data.currentPos.y, -0.2f);
            ushort poolID = (ushort)(data.entityTypeID + 10000);

            if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
            {
                GameObject botGO = pool.SpawnFromPool(poolID, prefab, worldPos);
                if (botGO != null)
                {
                    var botScript = botGO.GetComponentInChildren<AstroPioneer.Machines.Automation.TransportBot>();
                    if (botScript != null) botScript.BindToSimulation(data.id);
                    else Debug.LogWarning($"[ChunkRenderer] Spawned bot prefab {prefab.name} does not have TransportBot component!");

                    visuals.Add(new SpawnedVisual { localX = -1, localY = -1, structureID = (ushort)poolID, go = botGO });
                }
            }
        }

        public void SpawnVisualAt(int worldX, int worldY, ushort structureID)
        {
            if (structureID == GameConstants.STRUCTURE_EMPTY) return;
            if (structureRegistry == null) ServiceLocator.TryGet(out structureRegistry);
            if (structureRegistry == null) return;

            var entry = structureRegistry.Get(structureID);
            if (entry == null || entry.visualPrefab == null) return;

            Vector3 worldPos = new Vector3(worldX + 0.5f, worldY + 0.5f, -0.1f);
            if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
            {
                GameObject visual = pool.SpawnFromPool(structureID, entry.visualPrefab, worldPos);
                if (visual == null) return;

                var sr = visual.GetComponent<SpriteRenderer>();
                if (sr != null) 
                { 
                    sr.material = new Material(Shader.Find("Sprites/Default")); 
                    sr.sortingOrder = 100; 

                    if (entry.category == AstroPioneer.Data.StructureCategory.Crop && entry.sprites != null)
                    {
                        // Newly placed crop is at stage 0
                        if (entry.sprites.Length > 0) sr.sprite = entry.sprites[0];
                    }
                    else if (entry.sprites != null && entry.sprites.Length > 0) 
                    {
                        sr.sprite = entry.sprites[0];
                    }
                }

                visual.transform.SetParent(null);
                visual.transform.position = worldPos;

                var coord = ChunkCoord.FromGridPos(worldX, worldY);
                if (!visualMap.ContainsKey(coord)) visualMap[coord] = new List<SpawnedVisual>();
                ChunkCoord.WorldToLocal(worldX, worldY, out int lx, out int ly);
                visualMap[coord].Add(new SpawnedVisual { localX = lx, localY = ly, structureID = structureID, go = visual, spriteRenderer = sr });
            }
        }

        public void DespawnVisualAt(int worldX, int worldY, ushort structureID)
        {
            var coord = ChunkCoord.FromGridPos(worldX, worldY);
            if (!visualMap.TryGetValue(coord, out var visuals)) return;
            ChunkCoord.WorldToLocal(worldX, worldY, out int lx, out int ly);

            for (int i = visuals.Count - 1; i >= 0; i--)
            {
                // Only despawn the visual matching the specific structure ID being removed
                if (visuals[i].localX == lx && visuals[i].localY == ly && visuals[i].structureID == structureID)
                {
                    if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
                        pool.ReturnToPool(visuals[i].structureID, visuals[i].go);
                    visuals.RemoveAt(i);
                }
            }
        }

        public void UpdateVisualMetadata(int worldX, int worldY)
        {
            var coord = ChunkCoord.FromGridPos(worldX, worldY);
            if (!visualMap.TryGetValue(coord, out var visuals)) return;

            if (ServiceLocator.TryGet<ChunkManager>(out var cm) && cm.TryGetChunkAndLocal(worldX, worldY, out Chunk chunk, out int lx, out int ly))
            {
                ushort id = chunk.StructureLayer.Get(lx, ly);
                if (id == GameConstants.STRUCTURE_EMPTY) return;

                var entry = structureRegistry != null ? structureRegistry.Get(id) : null;
                if (entry == null || entry.visualPrefab == null) return;

                foreach (var v in visuals)
                {
                    if (v.localX == lx && v.localY == ly && v.go != null && entry.category == StructureCategory.Crop && v.spriteRenderer != null)
                    {
                        byte meta = chunk.MetadataLayer.Get(lx, ly);
                        int stage = meta & GameConstants.META_GROWTH_MASK;
                        if (entry.sprites != null && stage < entry.sprites.Length) v.spriteRenderer.sprite = entry.sprites[stage];
                        break;
                    }
                }
            }
        }

        private void SpawnLayerVisual(Chunk chunk, GridLayer<ushort> layer, int x, int y, List<SpawnedVisual> visuals)
        {
            ushort id = layer.Get(x, y);
            if (id == GameConstants.STRUCTURE_EMPTY) return;
            if (structureRegistry == null) ServiceLocator.TryGet(out structureRegistry);
            var entry = structureRegistry != null ? structureRegistry.Get(id) : null;
            if (entry == null || entry.visualPrefab == null) return;

            float worldX = chunk.Coord.WorldOriginX + x + 0.5f;
            float worldY = chunk.Coord.WorldOriginY + y + 0.5f;
            Vector3 worldPos = new Vector3(worldX, worldY, 0f);

            if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
            {
                GameObject visual = pool.SpawnFromPool(id, entry.visualPrefab, worldPos);
                if (visual == null) return;
                
                var machine = visual.GetComponent<AstroPioneer.Interfaces.ISavableMachine>();
                if (machine != null)
                {
                    byte[] state = GridManager.Instance.GetComplexState(new Vector2Int(Mathf.FloorToInt(worldX), Mathf.FloorToInt(worldY)));
                    if (state != null) machine.LoadState(new System.ReadOnlySpan<byte>(state));
                }

                var sr = visual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.material = new Material(Shader.Find("Sprites/Default"));
                    sr.sortingOrder = 100;
                    if (entry.category == StructureCategory.Crop && entry.sprites != null)
                    {
                        int stage = chunk.MetadataLayer.Get(x, y) & GameConstants.META_GROWTH_MASK;
                        if (stage < entry.sprites.Length) sr.sprite = entry.sprites[stage];
                    }
                    else if (entry.sprites != null && entry.sprites.Length > 0) sr.sprite = entry.sprites[0];
                }

                visual.transform.SetParent(null);
                visual.transform.position = worldPos;
                visuals.Add(new SpawnedVisual { localX = x, localY = y, structureID = id, go = visual, spriteRenderer = sr });
            }
        }

        private struct SpawnedVisual
        {
            public int localX, localY;
            public ushort structureID;
            public GameObject go;
            public SpriteRenderer spriteRenderer;
        }
    }
}
