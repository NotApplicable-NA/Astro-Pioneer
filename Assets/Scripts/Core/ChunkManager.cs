using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using AstroPioneer.Data;

namespace AstroPioneer.Core
{
    /// <summary>
    /// ChunkManager — Manages chunk lifecycle: load, generate, save, unload.
    /// 
    /// Flow per frame:
    /// 1. Calculate which ChunkCoord the camera is in.
    /// 2. For chunks within LOAD_RADIUS: if not loaded → schedule generation Job.
    /// 3. For chunks beyond UNLOAD_RADIUS: save to disk → despawn visuals → remove.
    /// 4. Complete any pending generation Jobs → copy NativeArray data to Chunk → spawn visuals.
    /// 
    /// THREAD-SAFETY: All Job scheduling/completion happens on main thread.
    /// Only the IJob.Execute() runs on worker threads (pure math, no Unity API).
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        [Header("World Configuration")]
        [SerializeField] private int worldSeed = 12345;
        [SerializeField] private StructureRegistry structureRegistry;

        [Header("References")]
        [SerializeField] private Transform trackingTarget; // Player or Camera

        // Active chunks in memory
        private readonly Dictionary<ChunkCoord, Chunk> activeChunks = new Dictionary<ChunkCoord, Chunk>();

        // Pending chunk generation jobs
        private readonly Dictionary<ChunkCoord, PendingChunk> pendingChunks = new Dictionary<ChunkCoord, PendingChunk>();
        private readonly List<ChunkCoord> toUnloadBuffer = new List<ChunkCoord>(64);
        private readonly List<ChunkCoord> completedJobsBuffer = new List<ChunkCoord>(64);

        // V19: Save Rate Limiter — prevents I/O spike when many chunks unload at once
        private readonly Queue<Chunk> pendingSaveQueue = new Queue<Chunk>();

        // Cached coord to avoid GC from struct boxing
        private ChunkCoord lastPlayerChunk;
        private bool isFirstFrame = true;

        public event System.Action<Chunk> OnChunkLoadedEvent;

        // ─── Properties ───
        public int WorldSeed => worldSeed;
        public StructureRegistry Registry => structureRegistry;
        public IReadOnlyDictionary<ChunkCoord, Chunk> ActiveChunks => activeChunks;

        void Awake()
        {
        }

        void Start()
        {
            if (AstroPioneer.Managers.TimeManager.Instance != null)
            {
                AstroPioneer.Managers.TimeManager.Instance.OnDayChanged += HandleDayChanged;
            }
        }

        void OnDestroy()
        {
            if (AstroPioneer.Managers.TimeManager.Instance != null)
            {
                AstroPioneer.Managers.TimeManager.Instance.OnDayChanged -= HandleDayChanged;
            }

            // Flush all pending saves first (avoid data loss on shutdown)
            while (pendingSaveQueue.Count > 0)
            {
                var chunk = pendingSaveQueue.Dequeue();
                SaveSystem.SaveChunkBinary(chunk, worldSeed, useAsync: false);
            }

            // Save all still-active chunks
            foreach (var kvp in activeChunks)
            {
                if (kvp.Value.IsDirty)
                    SaveSystem.SaveChunkBinary(kvp.Value, worldSeed, useAsync: false);

                kvp.Value.ReleaseComplexStates();
            }

            // Dispose any pending jobs
            foreach (var pending in pendingChunks.Values)
            {
                pending.handle.Complete();
                pending.arrays.Dispose();
            }
            pendingChunks.Clear();

            ServiceLocator.Unregister<ChunkManager>();
        }

        private void HandleDayChanged(int currentDay)
        {
            // Sync all active chunks to the new day so they don't perform catch-up when reloaded
            foreach (var chunk in activeChunks.Values)
            {
                chunk.LastSimulatedDay = currentDay;
                chunk.IsDirty = true;
            }
        }

        void Update()
        {
            if (trackingTarget == null) return;

            // 1. Determine current chunk
            Vector3 pos = trackingTarget.position;
            ChunkCoord currentChunk = ChunkCoord.FromWorldPos(pos.x, pos.y);

            // Only recalculate if player moved to a different chunk
            if (!isFirstFrame && currentChunk == lastPlayerChunk) 
            {
                CompletePendingJobs();
                return;
            }
            
            lastPlayerChunk = currentChunk;
            isFirstFrame = false;

            // 2. Load chunks within radius
            for (int dx = -GameConstants.LOAD_RADIUS; dx <= GameConstants.LOAD_RADIUS; dx++)
            {
                for (int dy = -GameConstants.LOAD_RADIUS; dy <= GameConstants.LOAD_RADIUS; dy++)
                {
                    var coord = new ChunkCoord(currentChunk.x + dx, currentChunk.y + dy);
                    if (!activeChunks.ContainsKey(coord) && !pendingChunks.ContainsKey(coord))
                    {
                        BeginLoadChunk(coord);
                    }
                }
            }

            // 3. Unload chunks beyond radius
            toUnloadBuffer.Clear();
            foreach (var kvp in activeChunks)
            {
                int dist = Mathf.Max(
                    Mathf.Abs(kvp.Key.x - currentChunk.x),
                    Mathf.Abs(kvp.Key.y - currentChunk.y));

                if (dist > GameConstants.UNLOAD_RADIUS)
                    toUnloadBuffer.Add(kvp.Key);
            }

            foreach (var coord in toUnloadBuffer)
                UnloadChunk(coord);

            // 4. Flush save queue (rate-limited)
            FlushSaveQueue();

            // 5. Complete any ready Jobs
            CompletePendingJobs();

            // 6. Evict stale simulation cache entries
            EvictStaleSimulationCache();
        }

        // ─── Save Rate Limiter ───

        private void FlushSaveQueue()
        {
            int savesThisFrame = 0;
            while (pendingSaveQueue.Count > 0 && savesThisFrame < GameConstants.MAX_SAVES_PER_FRAME)
            {
                var chunk = pendingSaveQueue.Dequeue();
                SaveSystem.SaveChunkBinary(chunk, worldSeed);
                savesThisFrame++;
            }
        }

        // ─── Chunk Loading ───

        private void BeginLoadChunk(ChunkCoord coord)
        {
            // Try load from disk first
            Chunk saved = SaveSystem.LoadChunkBinary(coord, worldSeed);
            if (saved != null)
            {
                activeChunks[coord] = saved;
                SpawnChunkVisuals(saved);
                return;
            }

            // Schedule procedural generation on worker thread
            var (handle, arrays) = ProceduralGenerator.Schedule(worldSeed, coord);
            pendingChunks[coord] = new PendingChunk { handle = handle, arrays = arrays };
        }

        private void CompletePendingJobs()
        {
            if (pendingChunks.Count == 0) return;
            completedJobsBuffer.Clear();

            foreach (var kvp in pendingChunks)
            {
                if (!kvp.Value.handle.IsCompleted) continue;

                // Complete on main thread (required by Job System)
                kvp.Value.handle.Complete();

                // Create chunk and copy NativeArray data to GridLayers
                var chunk = new Chunk(kvp.Key);
                kvp.Value.arrays.floor.CopyTo(chunk.FloorLayer.GetRawArray());
                kvp.Value.arrays.utility.CopyTo(chunk.UtilityLayer.GetRawArray());
                kvp.Value.arrays.structure.CopyTo(chunk.StructureLayer.GetRawArray());
                kvp.Value.arrays.metadata.CopyTo(chunk.MetadataLayer.GetRawArray());

                // Bool layer needs manual copy (NativeArray<bool> → bool[])
                for (int i = 0; i < kvp.Value.arrays.shadow.Length; i++)
                {
                    if (kvp.Value.arrays.shadow[i])
                        chunk.ShadowLayer.Set(
                            i % GameConstants.CHUNK_SIZE,
                            i / GameConstants.CHUNK_SIZE, true);
                }

                chunk.IsGenerated = true;
                chunk.LastSimulatedDay = AstroPioneer.Managers.TimeManager.Instance != null ? AstroPioneer.Managers.TimeManager.Instance.DaysPassed : 1;

                // MUST dispose NativeArrays after copying
                kvp.Value.arrays.Dispose();

                activeChunks[kvp.Key] = chunk;
                SpawnChunkVisuals(chunk);
                completedJobsBuffer.Add(kvp.Key);
            }

            foreach (var coord in completedJobsBuffer)
                pendingChunks.Remove(coord);
        }

        // ─── Chunk Unloading ───

        private void UnloadChunk(ChunkCoord coord)
        {
            if (!activeChunks.TryGetValue(coord, out var chunk)) return;

            // V19: Save Rate Limiter — enqueue instead of saving immediately.
            // This prevents a spike when many chunks unload in one frame.
            if (chunk.IsDirty)
                pendingSaveQueue.Enqueue(chunk);

            // Return all visuals to Object Pool
            DespawnChunkVisuals(coord);

            // Release ComplexState buffers to the ArrayPool
            chunk.ReleaseComplexStates();

            activeChunks.Remove(coord);
        }

        // ─── Visual Bridge (delegates to ChunkRenderer) ───

        private void SpawnChunkVisuals(Chunk chunk)
        {
            // V26: Notify systems like CropManager to perform Catch-Up Simulation
            OnChunkLoadedEvent?.Invoke(chunk);

            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.OnChunkLoaded(chunk);
        }

        private void DespawnChunkVisuals(ChunkCoord coord)
        {
            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.OnChunkUnloaded(coord);
        }

        // ─── Public Query API ───

        /// <summary>
        /// Resolve a world grid position to its Chunk and local coordinates.
        /// Returns false if the chunk is not currently loaded.
        /// </summary>
        public bool TryGetChunkAndLocal(int worldX, int worldY, out Chunk chunk, out int localX, out int localY)
        {
            var coord = ChunkCoord.FromGridPos(worldX, worldY);
            ChunkCoord.WorldToLocal(worldX, worldY, out localX, out localY);
            return activeChunks.TryGetValue(coord, out chunk);
        }

        /// <summary>
        /// Get a chunk by coordinate. Returns null if not loaded.
        /// </summary>
        public Chunk GetChunk(ChunkCoord coord)
        {
            return activeChunks.TryGetValue(coord, out var chunk) ? chunk : null;
        }

        /// <summary>
        /// Set tracking target at runtime (e.g., when player spawns).
        /// </summary>
        public void SetTrackingTarget(Transform target)
        {
            trackingTarget = target;
            isFirstFrame = true;
        }

        /// <summary>
        /// Clears all loaded chunks and visual state from memory without saving.
        /// Used primarily for Dev Resets (Shift+K).
        /// </summary>
        public void ClearAllData()
        {
            pendingSaveQueue.Clear();
            
            var coords = new System.Collections.Generic.List<ChunkCoord>(activeChunks.Keys);
            foreach (var coord in coords)
            {
                if (activeChunks.TryGetValue(coord, out var chunk))
                {
                    DespawnChunkVisuals(coord);
                    chunk.ReleaseComplexStates();
                }
            }
            
            activeChunks.Clear();
            pendingChunks.Clear();
        }

        // Helper struct
        private struct PendingChunk
        {
            public Unity.Jobs.JobHandle handle;
            public GenerationArrays arrays;
        }

        // ─── Simulation Cache: Off-Screen Chunk Access (V25 AAA) ───

        /// <summary>
        /// LRU cache for chunks loaded from disk purely for simulation queries.
        /// These chunks are NOT rendered — only their data is kept in memory temporarily.
        /// Evicted after MAX_CACHED frames without access.
        /// </summary>
        private readonly Dictionary<ChunkCoord, CachedChunk> simulationCache 
            = new Dictionary<ChunkCoord, CachedChunk>();
        private readonly List<ChunkCoord> cacheEvictionBuffer = new List<ChunkCoord>(8);
        private const int MAX_CACHE_AGE = 300; // Evict after ~5 seconds at 60fps

        private struct CachedChunk
        {
            public Chunk chunk;
            public int lastAccessFrame;
        }

        /// <summary>
        /// Simulation-safe chunk lookup. First checks activeChunks, then simulationCache,
        /// and finally loads from disk as a last resort. Returns false only if the chunk
        /// has never been saved to disk (e.g., unexplored territory).
        /// </summary>
        public bool TryGetChunkForSimulation(int worldX, int worldY, out Chunk chunk, out int localX, out int localY)
        {
            var coord = ChunkCoord.FromGridPos(worldX, worldY);
            ChunkCoord.WorldToLocal(worldX, worldY, out localX, out localY);

            // 1. Active chunk (already in memory with visuals)
            if (activeChunks.TryGetValue(coord, out chunk))
                return true;

            // 2. Simulation cache (previously loaded from disk for simulation)
            if (simulationCache.TryGetValue(coord, out var cached))
            {
                cached.lastAccessFrame = Time.frameCount;
                simulationCache[coord] = cached;
                chunk = cached.chunk;
                return true;
            }

            // 3. Load from disk (no visuals, data only)
            chunk = SaveSystem.LoadChunkBinary(coord, worldSeed);
            if (chunk != null)
            {
                simulationCache[coord] = new CachedChunk 
                { 
                    chunk = chunk, 
                    lastAccessFrame = Time.frameCount 
                };
                return true;
            }

            return false;
        }

        /// <summary>
        /// Flush any dirty simulation-cached chunks back to disk before eviction.
        /// Called from Update() to prevent stale cache buildup.
        /// </summary>
        private void EvictStaleSimulationCache()
        {
            if (simulationCache.Count == 0) return;

            cacheEvictionBuffer.Clear();
            int currentFrame = Time.frameCount;

            foreach (var kvp in simulationCache)
            {
                // If this chunk is now active (player walked back), discard the cache copy
                if (activeChunks.ContainsKey(kvp.Key))
                {
                    cacheEvictionBuffer.Add(kvp.Key);
                    continue;
                }

                if (currentFrame - kvp.Value.lastAccessFrame > MAX_CACHE_AGE)
                {
                    // Save dirty cached chunks back to disk before eviction
                    if (kvp.Value.chunk.IsDirty)
                        SaveSystem.SaveChunkBinary(kvp.Value.chunk, worldSeed, useAsync: false);
                    
                    kvp.Value.chunk.ReleaseComplexStates();
                    cacheEvictionBuffer.Add(kvp.Key);
                }
            }

            foreach (var coord in cacheEvictionBuffer)
                simulationCache.Remove(coord);
        }

        /// <summary>
        /// Mark a simulation-cached chunk as dirty so it gets saved on eviction.
        /// </summary>
        public void MarkSimulationCacheDirty(ChunkCoord coord)
        {
            if (simulationCache.TryGetValue(coord, out var cached))
            {
                cached.chunk.IsDirty = true;
                cached.lastAccessFrame = Time.frameCount;
                simulationCache[coord] = cached;
            }
        }
    }
}
