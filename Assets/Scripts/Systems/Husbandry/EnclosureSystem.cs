using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Managers;

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

        [SerializeField] private int maxEnclosureSize = 100;
        
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
            HashSet<Vector2Int> evaluatedTiles = new HashSet<Vector2Int>();

            foreach (var fencePos in allFences)
            {
                var neighbors = GridManager.Instance.GetNeighbors(fencePos);
                foreach (var n in neighbors)
                {
                    if (!allFences.Contains(n) && !evaluatedTiles.Contains(n))
                    {
                        var area = FloodFillFindEnclosure(n, allFences, out bool isClosed);
                        if (isClosed && area.Count > 0)
                        {
                            // We found a closed loop!
                            string eId = $"Enclosure_{enclosureCounter++}";
                            activeEnclosures[eId] = new Enclosure(eId, area);
                        }

                        // Mark these as evaluated whether open or closed to avoid redundant fills
                        evaluatedTiles.UnionWith(area);
                    }
                }
            }
        }

        private HashSet<Vector2Int> FloodFillFindEnclosure(Vector2Int startPoint, HashSet<Vector2Int> fenceMap, out bool isClosed)
        {
            HashSet<Vector2Int> area = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            
            queue.Enqueue(startPoint);
            area.Add(startPoint);
            isClosed = true;

            int safety = 0;

            while (queue.Count > 0)
            {
                safety++;
                if (safety > maxEnclosureSize)
                {
                    // Too big to be an enclosure (or world is huge)
                    isClosed = false;
                    return area;
                }

                Vector2Int curr = queue.Dequeue();

                // Check out of bounds (Grid edges)
                if (IsGridEdge(curr))
                {
                    isClosed = false;
                    // Keep filling to mark all connected open space as evaluated, but we know it's not closed.
                }

                // Check 8 neighbors (including diagonals) to allow "leaks" through diagonal gaps
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        Vector2Int neighbor = curr + new Vector2Int(x, y);
                        if (GridManager.Instance.IsValidGridPosition(neighbor))
                        {
                            if (!area.Contains(neighbor) && !fenceMap.Contains(neighbor))
                            {
                                area.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            return area;
        }

        private bool IsGridEdge(Vector2Int pos)
        {
            return pos.x == 0 || pos.y == 0 || 
                   pos.x == GridManager.Instance.GridDimensions.x - 1 || 
                   pos.y == GridManager.Instance.GridDimensions.y - 1;
        }

        private HashSet<Vector2Int> GetAllFences()
        {
            HashSet<Vector2Int> fences = new HashSet<Vector2Int>();
            
            for (int x = 0; x < GridManager.Instance.GridDimensions.x; x++)
            {
                for (int y = 0; y < GridManager.Instance.GridDimensions.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    // 1. Check Macro Grid (Machines/Structures)
                    GameObject macroObj = GridManager.Instance.GetOccupantAt(pos);
                    if (macroObj != null && macroObj.name.Contains("Fence"))
                    {
                        fences.Add(pos);
                        continue;
                    }

                    // 2. Check Micro Grid (Pipes/Legacy Fences)
                    var micro = GridManager.Instance.GetMicroOccupantsAt(pos);
                    foreach (var obj in micro)
                    {
                        if (obj != null && obj.name.Contains("Fence"))
                        {
                            fences.Add(pos);
                            break;
                        }
                    }
                }
            }
            return fences;
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
