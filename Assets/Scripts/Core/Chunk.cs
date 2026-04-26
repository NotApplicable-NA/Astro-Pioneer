using System.Collections.Generic;
using UnityEngine;

namespace AstroPioneer.Core
{
    /// <summary>
    /// Chunk — Pure data container for a 16×16 area of the world.
    /// 
    /// KEY DESIGN RULES:
    /// 1. NO GameObjects stored in Chunk. Only primitive IDs (ushort) and metadata (byte).
    /// 2. Multi-Layer architecture: Floor + Utility + Structure can coexist at same (x,y).
    /// 3. Complex machine state uses pre-allocated byte[] buffers (in-place modification).
    ///    New byte[] is NEVER created per frame — only during initial placement or Save.
    /// 4. All data is trivially serializable via BinaryWriter.
    /// </summary>
    public class Chunk
    {
        public ChunkCoord Coord { get; }
        public bool IsDirty { get; set; }
        public bool IsGenerated { get; set; }
        
        /// <summary>
        /// Tracks the last in-game day this chunk was simulated.
        /// Used for 'Catch-Up' simulation when a chunk is loaded from disk.
        /// </summary>
        public int LastSimulatedDay { get; set; } = 1;

        // ─── Multi-Layer Grid Data (ushort IDs) ───
        // Each layer is independent: a cell can have Floor + Utility + Structure simultaneously.

        /// <summary>
        /// Floor tiles: iron plates, dirt, concrete.
        /// Cosmetic base layer that doesn't block other placements.
        /// </summary>
        public GridLayer<ushort> FloorLayer { get; }

        /// <summary>
        /// Utility infrastructure: power cables, water pipes, fences.
        /// Overlaps with Structure layer without conflict.
        /// </summary>
        public GridLayer<ushort> UtilityLayer { get; }

        /// <summary>
        /// Main structures: crops, machines (sprinkler, harvester, etc.), trees.
        /// Only ONE structure per cell in this layer.
        /// </summary>
        public GridLayer<ushort> StructureLayer { get; }

        /// <summary>
        /// Lightweight metadata for simple structures (crops):
        ///   - Lower 4 bits: growth stage (0-15)
        ///   - Upper 4 bits: flags (watered, lit, etc.)
        /// Adequate for structures that only need a few status values.
        /// </summary>
        public GridLayer<byte> MetadataLayer { get; }

        // ─── Environment Layers ───

        /// <summary>Shadow Canyon zones. Generated procedurally.</summary>
        public GridLayer<bool> ShadowLayer { get; }

        /// <summary>UV Light coverage from UVLightPillar machines.</summary>
        public GridLayer<bool> LitLayer { get; }

        /// <summary>Fog of war: cells the player has visited.</summary>
        public GridLayer<bool> ExploredLayer { get; }

        // ─── Complex Machine State ───
        /// <summary>
        /// For machines with complex runtime state (Composter inventory, Battery charge, etc.)
        /// Memory management: using ArrayPool<byte> to eliminate GC churn during load/unload.
        /// </summary>
        public Dictionary<Vector2Int, byte[]> ComplexStates { get; } = new Dictionary<Vector2Int, byte[]>();

        // ─── Constructor ───

        public Chunk(ChunkCoord coord)
        {
            Coord = coord;
            int s = GameConstants.CHUNK_SIZE;

            FloorLayer = new GridLayer<ushort>(s);
            UtilityLayer = new GridLayer<ushort>(s);
            StructureLayer = new GridLayer<ushort>(s);
            MetadataLayer = new GridLayer<byte>(s);
            ShadowLayer = new GridLayer<bool>(s);
            LitLayer = new GridLayer<bool>(s);
            ExploredLayer = new GridLayer<bool>(s);
        }

        // ─── Complex State Helpers ───

        /// <summary>
        /// Rent a state buffer from the pool for a newly-placed machine.
        /// </summary>
        public byte[] AllocateComplexState(int localX, int localY)
        {
            var key = new Vector2Int(localX, localY);
            if (ComplexStates.TryGetValue(key, out var existing))
                return existing;

            // Rent from pool (using fixed size for DOD predictability)
            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(GameConstants.MACHINE_STATE_BUFFER_SIZE);
            // System zeroing might be needed if machines don't fully overwrite
            System.Array.Clear(buffer, 0, buffer.Length);
            
            ComplexStates[key] = buffer;
            IsDirty = true;
            return buffer;
        }

        /// <summary>
        /// Get the existing state buffer for in-place read/write.
        /// </summary>
        public byte[] GetComplexState(int localX, int localY)
        {
            var key = new Vector2Int(localX, localY);
            return ComplexStates.TryGetValue(key, out var buffer) ? buffer : null;
        }

        /// <summary>
        /// Remove complex state and return buffer to pool.
        /// </summary>
        public void RemoveComplexState(int localX, int localY)
        {
            var key = new Vector2Int(localX, localY);
            if (ComplexStates.TryGetValue(key, out var buffer))
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
                ComplexStates.Remove(key);
                IsDirty = true;
            }
        }

        /// <summary>
        /// Clears all complex states and returns all buffers to the pool.
        /// Called when the chunk is unloaded or the world is closed.
        /// </summary>
        public void ReleaseComplexStates()
        {
            foreach (var buffer in ComplexStates.Values)
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            }
            ComplexStates.Clear();
        }
    }
}
