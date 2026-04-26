using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Managers;
using AstroPioneer.Core;

namespace AstroPioneer.Systems.Husbandry
{
    public class Enclosure
    {
        public string id;
        public HashSet<Vector2Int> tiles;
        public int capacity => tiles.Count;
        
        public Enclosure(string id, HashSet<Vector2Int> tiles)
        {
            this.id = id;
            this.tiles = tiles;
        }

        public bool Contains(Vector2Int pos)
        {
            return tiles.Contains(pos);
        }
    }

    /// <summary>
    /// Detects closed loops of fences and creates Enclosures for Fauna AI constraints.
    /// Utilizes GridManager Micro-grid for fence tracking.
    /// </summary>
    public class EnclosureSystem : MonoBehaviour
    {
        public static EnclosureSystem Instance { get; private set; }

        // Caller-owned buffer — zero GC, safe for single-threaded use
        private readonly List<Vector2Int> neighborBuffer = new List<Vector2Int>(4);
        private readonly HashSet<Vector2Int> evaluatedTilesBuffer = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> fenceBuffer = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> fillAreaBuffer = new HashSet<Vector2Int>();
        private readonly Queue<Vector2Int> fillQueueBuffer = new Queue<Vector2Int>();

        private int maxEnclosureSize => GameConstants.MAX_ENCLOSURE_SIZE;
        
        // Use string ID for fenced enclosures
        private Dictionary<string, Enclosure> activeEnclosures = new Dictionary<string, Enclosure>();
        private int enclosureCounter = 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Call this when a fence is placed or destroyed to re-evaluate enclosures nearby.
        /// </summary>
        public void ReevaluateEnclosuresAround(Vector2Int gridPos)
        {

            // Simplified: Re-calculate all enclosures from scratch or run local flood fill.
            // For Astro-Pioneer cozy scale, we'll run a flood fill on the adjacent empty cells.
            
            // Clear existing ones that might have broken
            activeEnclosures.Clear();

            // Find all seeds (all empty cells adjacent to ANY fence)
            // A more optimized approach is needed for large grids, but this is fine for PPU 16 cozy games.
            HashSet<Vector2Int> allFences = GetAllFences();
            evaluatedTilesBuffer.Clear();

            foreach (var fencePos in allFences)
            {
                GridManager.Instance.GetNeighbors(fencePos, neighborBuffer);
                foreach (var n in neighborBuffer)
                {
                    if (!allFences.Contains(n) && !evaluatedTilesBuffer.Contains(n))
                    {
                        if (FloodFillFindEnclosure(n, allFences, out bool isClosed))
                        {
                            // We found a closed loop!
                            string eId = $"Enclosure_{enclosureCounter++}";
                            // Only allocate a NEW HashSet when we are 100% sure it's a valid enclosure
                            activeEnclosures[eId] = new Enclosure(eId, new HashSet<Vector2Int>(fillAreaBuffer));
                            evaluatedTilesBuffer.UnionWith(fillAreaBuffer);
                        }
                        else
                        {
                            evaluatedTilesBuffer.UnionWith(fillAreaBuffer);
                        }
                    }
                }
            }
        }

        private bool FloodFillFindEnclosure(Vector2Int startPoint, HashSet<Vector2Int> fenceMap, out bool isClosed)
        {
            fillAreaBuffer.Clear();
            fillQueueBuffer.Clear();
            
            fillQueueBuffer.Enqueue(startPoint);
            fillAreaBuffer.Add(startPoint);
            isClosed = true;

            int safety = 0;

            while (fillQueueBuffer.Count > 0)
            {
                safety++;
                if (safety > maxEnclosureSize)
                {
                    // Too big to be an enclosure (or world is huge)
                    isClosed = false;
                    return false;
                }

                Vector2Int curr = fillQueueBuffer.Dequeue();

                // Check out of bounds (Grid edges)
                if (IsGridEdge(curr))
                {
                    isClosed = false;
                }

                // Check 8 neighbors (including diagonals) to allow "leaks" through diagonal gaps
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        Vector2Int neighbor = curr + new Vector2Int(x, y);
                        int limit = GameConstants.WORLD_BOUNDARY_LIMIT;
                        if (Mathf.Abs(neighbor.x) < limit && Mathf.Abs(neighbor.y) < limit)
                        {
                            if (!fillAreaBuffer.Contains(neighbor) && !fenceMap.Contains(neighbor))
                            {
                                fillAreaBuffer.Add(neighbor);
                                fillQueueBuffer.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            return isClosed && fillAreaBuffer.Count > 0;
        }

        private bool IsGridEdge(Vector2Int pos)
        {
            int limit = GameConstants.WORLD_BOUNDARY_LIMIT;
            return Mathf.Abs(pos.x) >= limit || Mathf.Abs(pos.y) >= limit;
        }

        private HashSet<Vector2Int> GetAllFences()
        {
            fenceBuffer.Clear();
            
            if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.ChunkManager>(out var cm))
            {
                foreach(var chunk in cm.ActiveChunks.Values)
                {
                    Vector2Int origin = new Vector2Int((int)chunk.Coord.WorldOriginX, (int)chunk.Coord.WorldOriginY);
                    int w = AstroPioneer.Core.GameConstants.CHUNK_SIZE;
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < w; y++)
                        {
                            Vector2Int pos = origin + new Vector2Int(x, y);
                            ushort structID = chunk.StructureLayer.Get(x, y);
                            ushort utilID = chunk.UtilityLayer.Get(x, y);

                            if (structID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY || 
                                utilID != AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY)
                            {
                                fenceBuffer.Add(pos);
                            }
                        }
                    }
                }
            }
            return fenceBuffer;
        }

        public Enclosure GetEnclosureAt(Vector2Int pos)
        {
            foreach (var kvp in activeEnclosures)
            {
                if (kvp.Value.Contains(pos)) return kvp.Value;
            }
            return null;
        }
    }
}
