using UnityEngine;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// GridManager — Thin facade that routes all grid queries to the chunk-based system.
    /// 
    /// ARCHITECTURE:
    /// - Old API signatures (GetOccupantAt, TryOccupyCell, etc.) are preserved
    ///   for backward compatibility during migration.
    /// - New API (GetStructureAt, TryPlaceStructure) works with ushort IDs directly.
    /// - All data lives in Chunk objects managed by ChunkManager.
    /// - Grid position == World position (1:1 linear, cellSize = 1.0).
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = false;

        // ─── Properties (backward compat) ───
        public float CellSize => 1.0f; // Fixed 1:1 mapping

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                ServiceLocator.Unregister<GridManager>();
            }
        }

        // ─── Position Conversion (1:1 linear mapping) ───

        /// <summary>Grid coordinate → world center (adds 0.5 offset for cell center).</summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
        }

        /// <summary>World position → grid coordinate (floor).</summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x),
                Mathf.FloorToInt(worldPos.y));
        }

        // ─── NEW API: Structure-based (ushort IDs) ───

        /// <summary>Get structure ID at a world grid position.</summary>
        public ushort GetStructureAt(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;
            return chunk.StructureLayer.Get(lx, ly);
        }

        /// <summary>Get utility (micro-grid) ID at a world grid position.</summary>
        public ushort GetUtilityAt(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;
            return chunk.UtilityLayer.Get(lx, ly);
        }

        /// <summary>Get floor ID at a world grid position.</summary>
        public ushort GetFloorAt(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;
            return chunk.FloorLayer.Get(lx, ly);
        }

        /// <summary>Place a structure. Returns false if cell is occupied or chunk not loaded.</summary>
        public bool TryPlaceStructure(Vector2Int worldGridPos, ushort structureID)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return false;

            if (chunk.StructureLayer.Get(lx, ly) != GameConstants.STRUCTURE_EMPTY)
                return false;

            chunk.StructureLayer.Set(lx, ly, structureID);
            chunk.IsDirty = true;

            // Spawn visual
            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.SpawnVisualAt(worldGridPos.x, worldGridPos.y, structureID);

            // V22: Refresh pathfinding grid so bots respect new obstacles
            RefreshPathfindingAt(worldGridPos);

            return true;
        }

        /// <summary>Remove a structure at position. Returns the ID that was removed.</summary>
        public ushort RemoveStructure(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;

            ushort removed = chunk.StructureLayer.Get(lx, ly);
            if (removed == GameConstants.STRUCTURE_EMPTY) return GameConstants.STRUCTURE_EMPTY;

            // V20: Multi-tile cleanup. If we remove the origin, clear the footprint.
            // If it's an OCCUPIED_PART, it shouldn't be removable directly via Grid (must target origin or via physics hit).
            if (removed == GameConstants.STRUCTURE_OCCUPIED_PART)
            {
                Debug.LogWarning($"[GridManager] Attempted to remove OCCUPIED_PART at {worldGridPos}. Interaction should target the origin.");
                return GameConstants.STRUCTURE_EMPTY; 
            }

            // Get dimensions from registry
            var entry = AstroPioneer.Data.StructureRegistry.Instance.Get(removed);
            int w = 1, h = 1;
            if (entry != null && entry.dimensions != Vector2.zero)
            {
                w = Mathf.RoundToInt(entry.dimensions.x);
                h = Mathf.RoundToInt(entry.dimensions.y);
            }

            // Clear the whole footprint
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = worldGridPos + new Vector2Int(x, y);
                    if (cm.TryGetChunkAndLocal(pos.x, pos.y, out var targetChunk, out int tlx, out int tly))
                    {
                        targetChunk.StructureLayer.Set(tlx, tly, GameConstants.STRUCTURE_EMPTY);
                        targetChunk.MetadataLayer.Set(tlx, tly, 0);
                        targetChunk.RemoveComplexState(tlx, tly);
                        targetChunk.IsDirty = true;
                    }
                }
            }

            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.DespawnVisualAt(worldGridPos.x, worldGridPos.y, removed);

            // V22: Refresh pathfinding — removed obstacle is now walkable
            RefreshPathfindingAt(worldGridPos);

            return removed;
        }

        /// <summary>Place a utility (micro-grid) structure. Returns false if cell is occupied.</summary>
        public bool TryPlaceUtility(Vector2Int worldGridPos, ushort utilityID)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return false;

            if (chunk.UtilityLayer.Get(lx, ly) != GameConstants.STRUCTURE_EMPTY)
                return false;

            chunk.UtilityLayer.Set(lx, ly, utilityID);
            chunk.IsDirty = true;
            
            // Visual spawn for utility if needed (assume ChunkRenderer handles it)
            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.SpawnVisualAt(worldGridPos.x, worldGridPos.y, utilityID);

            // V22: Refresh pathfinding for fences/utilities
            RefreshPathfindingAt(worldGridPos);

            return true;
        }

        /// <summary>Remove utility at position.</summary>
        public ushort RemoveUtility(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;

            ushort removed = chunk.UtilityLayer.Get(lx, ly);
            if (removed == GameConstants.STRUCTURE_EMPTY) return GameConstants.STRUCTURE_EMPTY;

            chunk.UtilityLayer.Set(lx, ly, GameConstants.STRUCTURE_EMPTY);
            chunk.IsDirty = true;

            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.DespawnVisualAt(worldGridPos.x, worldGridPos.y, removed);

            // V22: Refresh pathfinding — utility removed
            RefreshPathfindingAt(worldGridPos);

            return removed;
        }

        // ─── Pathfinding Integration (V23 Chunk-Aware) ───
        // PathfindingManager now queries IsSolidAt() live during A* search.
        // No push notifications needed — this method is kept to avoid removing call sites.
        private void RefreshPathfindingAt(Vector2Int worldGridPos) { /* no-op */ }

        /// <summary>
        /// Data-driven check: Is the tile at this position solid (non-walkable)?
        /// Uses StructureData.blocksMovement — set per-item by designers.
        /// No hardcoded ID ranges or category checks.
        /// </summary>
        public bool IsSolidAt(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return false;

            var registry = AstroPioneer.Data.StructureRegistry.Instance;
            if (registry == null) return false;

            // Check StructureLayer
            ushort structureID = chunk.StructureLayer.Get(lx, ly);
            if (structureID != GameConstants.STRUCTURE_EMPTY)
            {
                if (structureID == GameConstants.STRUCTURE_OCCUPIED_PART) return true;

                var data = registry.Get(structureID);
                if (data != null && data.blocksMovement) return true;
            }

            // Check UtilityLayer
            ushort utilityID = chunk.UtilityLayer.Get(lx, ly);
            if (utilityID != GameConstants.STRUCTURE_EMPTY)
            {
                var data = registry.Get(utilityID);
                if (data != null && data.blocksMovement) return true;
            }

            return false;
        }

        /// <summary>Check if a structure cell is empty (available for placement).</summary>
        public bool IsPositionAvailable(Vector2Int worldGridPos)
        {
            return GetStructureAt(worldGridPos) == GameConstants.STRUCTURE_EMPTY;
        }

        // ─── Metadata ───

        /// <summary>Get crop metadata byte (growth stage + flags).</summary>
        public byte GetMetadataAt(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return 0;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return 0;
            return chunk.MetadataLayer.Get(lx, ly);
        }

        /// <summary>Set crop metadata byte (in-place, no allocation).</summary>
        public void SetMetadataAt(Vector2Int worldGridPos, byte metadata)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return;
            chunk.MetadataLayer.Set(lx, ly, metadata);
            chunk.IsDirty = true;
        }

        /// <summary>Marks the chunk containing this position for saving.</summary>
        public void MarkChunkDirty(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return;
            chunk.IsDirty = true;
        }

        // ─── Complex Machine State ───

        /// <summary>
        /// Get or allocate a byte[] buffer for a complex machine.
        /// Buffer is pre-allocated; runtime mutations modify it IN-PLACE.
        /// </summary>
        public byte[] GetOrAllocateComplexState(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return null;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return null;
            return chunk.AllocateComplexState(lx, ly);
        }

        /// <summary>Read-only access to existing complex state buffer.</summary>
        public byte[] GetComplexState(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return null;
            if (!cm.TryGetChunkAndLocal(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return null;
            return chunk.GetComplexState(lx, ly);
        }

        // ─── Environment Layers ───

        public void AddLightSource(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return;
            chunk.LitLayer.Set(lx, ly, true);
            chunk.IsDirty = true;
        }

        public void RemoveLightSource(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return;
            chunk.LitLayer.Set(lx, ly, false);
            chunk.IsDirty = true;
        }

        public bool IsCellLit(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return false;
            return chunk.LitLayer.Get(lx, ly);
        }

        public void SetShadowCell(Vector2Int pos, bool isShadow)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return;
            chunk.ShadowLayer.Set(lx, ly, isShadow);
            chunk.IsDirty = true;
        }

        public bool IsShadowCell(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return false;
            return chunk.ShadowLayer.Get(lx, ly);
        }

        public void ExploreCell(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return;
            chunk.ExploredLayer.Set(lx, ly, true);
            chunk.IsDirty = true;
        }

        public bool IsCellExplored(Vector2Int pos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return false;
            if (!cm.TryGetChunkAndLocal(pos.x, pos.y, out var chunk, out int lx, out int ly)) return false;
            return chunk.ExploredLayer.Get(lx, ly);
        }

        // ─── Neighbor Query ───

        // Static direction array — allocated once, never GC'd
        private static readonly Vector2Int[] NeighborDirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        /// <summary>
        /// Fills caller-provided list with the 4 cardinal neighbors.
        /// Caller owns and reuses the buffer — zero GC, zero state corruption.
        /// </summary>
        public void GetNeighbors(Vector2Int pos, System.Collections.Generic.List<Vector2Int> resultsBuffer)
        {
            resultsBuffer.Clear();
            for (int i = 0; i < NeighborDirs.Length; i++)
                resultsBuffer.Add(pos + NeighborDirs[i]);
        }

        // ─── Debug ───

        void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;

            foreach (var kvp in cm.ActiveChunks)
            {
                var chunk = kvp.Value;
                var origin = new Vector3(chunk.Coord.WorldOriginX, chunk.Coord.WorldOriginY, 0);
                float size = GameConstants.CHUNK_SIZE;

                // Chunk borders
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(
                    origin + new Vector3(size * 0.5f, size * 0.5f, 0),
                    new Vector3(size, size, 0));
            }
        }
        // ─── Simulation-Safe API (V25 AAA — Works across unloaded chunks) ───

        /// <summary>
        /// Get structure ID at a world grid position, even if the chunk is unloaded.
        /// Falls back to loading chunk from disk via ChunkManager's simulation cache.
        /// Used exclusively by BotSimulationManager for off-screen operations.
        /// </summary>
        public ushort GetStructureAtForSimulation(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return GameConstants.STRUCTURE_EMPTY;
            if (!cm.TryGetChunkForSimulation(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return GameConstants.STRUCTURE_EMPTY;
            return chunk.StructureLayer.Get(lx, ly);
        }

        /// <summary>
        /// Get or allocate ComplexState for a position, even if the chunk is unloaded.
        /// Used exclusively by BotSimulationManager for off-screen storage writes.
        /// </summary>
        public byte[] GetOrAllocateComplexStateForSimulation(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return null;
            if (!cm.TryGetChunkForSimulation(worldGridPos.x, worldGridPos.y, out var chunk, out int lx, out int ly))
                return null;
            return chunk.AllocateComplexState(lx, ly);
        }

        /// <summary>
        /// Mark a chunk dirty from simulation context (may be cached, not active).
        /// Ensures the modified data is saved to disk on cache eviction.
        /// </summary>
        public void MarkChunkDirtyForSimulation(Vector2Int worldGridPos)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;
            
            var coord = ChunkCoord.FromGridPos(worldGridPos.x, worldGridPos.y);
            
            // If chunk is active, use normal dirty flag
            var activeChunk = cm.GetChunk(coord);
            if (activeChunk != null)
            {
                activeChunk.IsDirty = true;
                return;
            }
            
            // Otherwise mark the simulation cache entry dirty
            cm.MarkSimulationCacheDirty(coord);
        }
    }
}
