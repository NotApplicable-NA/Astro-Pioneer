using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace AstroPioneer.Core
{
    /// <summary>
    /// ProceduralGenerator — Generates chunk data using deterministic seed-based math.
    /// 
    /// THREAD-SAFETY RULES:
    /// 1. This Job uses ONLY blittable types (int, ushort, bool) and NativeArrays.
    /// 2. ZERO UnityEngine API calls (no GameObject, Vector3, Mathf, MonoBehaviour).
    /// 3. All math uses System.Math or manual implementations.
    /// 4. After Job completes on worker thread, results are copied back to
    ///    GridLayer via CopyFrom() on the main thread.
    /// </summary>
    [BurstCompile]
    public struct ChunkGenerationJob : IJob
    {
        // ─── Input (read-only) ───
        [ReadOnly] public int worldSeed;
        [ReadOnly] public int chunkX;
        [ReadOnly] public int chunkY;
        [ReadOnly] public int chunkSize;

        // ─── Output (write) ───
        // Flattened 1D arrays matching GridLayer internal format: index = x + y * chunkSize
        public NativeArray<ushort> outFloor;
        public NativeArray<ushort> outUtility;
        public NativeArray<ushort> outStructure;
        public NativeArray<byte> outMetadata;
        public NativeArray<bool> outShadow;

        public void Execute()
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    int worldX = chunkX * chunkSize + x;
                    int worldY = chunkY * chunkSize + y;
                    int index = x + y * chunkSize;

                    // Deterministic hash per-cell
                    int cellHash = Hash(worldSeed, worldX, worldY);

                    // Default: empty
                    outFloor[index] = 0;
                    outUtility[index] = 0;
                    outStructure[index] = 0;
                    outMetadata[index] = 0;
                    outShadow[index] = false;

                    // ─── Generation Rules (example) ───
                    // These thresholds can be tuned via GameConstants or SO config

                    float normalized = (cellHash & 0xFFFF) / 65535f; // 0.0 to 1.0

                    // Shadow Canyons: ~3% of cells
                    if (normalized < 0.03f)
                    {
                        outShadow[index] = true;
                    }

                    // Natural resource nodes: ~1%
                    if (normalized > 0.98f)
                    {
                        // Place a decorative structure (tree, rock, crystal)
                        // Using a sub-hash to determine which type
                        int typeHash = Hash(cellHash, worldX, worldY + 1) & 0x3;
                        // TODO: Map typeHash to actual structure IDs when content catalog is defined
                    }
                }
            }
        }

        /// <summary>
        /// Deterministic integer hash. No UnityEngine dependency.
        /// Based on Robert Jenkins' 32-bit hash.
        /// Same inputs always produce the same output regardless of thread.
        /// </summary>
        private static int Hash(int seed, int x, int y)
        {
            int h = seed;
            h ^= x * 73856093;
            h ^= y * 19349663;
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;
            return h & 0x7FFFFFFF; // Ensure positive
        }
    }

    /// <summary>
    /// Helper to schedule and complete chunk generation jobs from ChunkManager.
    /// Runs on main thread but the actual computation happens on worker threads.
    /// </summary>
    public static class ProceduralGenerator
    {
        /// <summary>
        /// Schedule a chunk generation job. Returns a handle that can be completed later.
        /// 
        /// Usage in ChunkManager:
        ///   var (handle, arrays) = ProceduralGenerator.Schedule(seed, coord);
        ///   handle.Complete();  // Wait for background thread
        ///   chunk.FloorLayer.CopyFrom(arrays.floor.ToArray());
        ///   arrays.Dispose();  // MUST dispose NativeArrays
        /// </summary>
        public static (JobHandle handle, GenerationArrays arrays) Schedule(int worldSeed, ChunkCoord coord)
        {
            int totalCells = GameConstants.CHUNK_SIZE * GameConstants.CHUNK_SIZE;

            var arrays = new GenerationArrays
            {
                floor = new NativeArray<ushort>(totalCells, Allocator.TempJob),
                utility = new NativeArray<ushort>(totalCells, Allocator.TempJob),
                structure = new NativeArray<ushort>(totalCells, Allocator.TempJob),
                metadata = new NativeArray<byte>(totalCells, Allocator.TempJob),
                shadow = new NativeArray<bool>(totalCells, Allocator.TempJob)
            };

            var job = new ChunkGenerationJob
            {
                worldSeed = worldSeed,
                chunkX = coord.x,
                chunkY = coord.y,
                chunkSize = GameConstants.CHUNK_SIZE,
                outFloor = arrays.floor,
                outUtility = arrays.utility,
                outStructure = arrays.structure,
                outMetadata = arrays.metadata,
                outShadow = arrays.shadow
            };

            return (job.Schedule(), arrays);
        }
    }

    /// <summary>
    /// Container for NativeArrays used during chunk generation.
    /// MUST be Disposed after copying data to GridLayers.
    /// </summary>
    public struct GenerationArrays
    {
        public NativeArray<ushort> floor;
        public NativeArray<ushort> utility;
        public NativeArray<ushort> structure;
        public NativeArray<byte> metadata;
        public NativeArray<bool> shadow;

        public void Dispose()
        {
            if (floor.IsCreated) floor.Dispose();
            if (utility.IsCreated) utility.Dispose();
            if (structure.IsCreated) structure.Dispose();
            if (metadata.IsCreated) metadata.Dispose();
            if (shadow.IsCreated) shadow.Dispose();
        }
    }
}
