using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Pathfinding
{
    /// <summary>
    /// PathfindingManager V23 — Chunk-Aware A* Pathfinding.
    /// 
    /// ARCHITECTURE:
    /// - NO persistent grid. Walkability is queried on-demand from GridManager.IsSolidAt().
    /// - PathNodes are created lazily during each A* search and pooled between calls.
    /// - Works with infinite world size — follows the chunk system automatically.
    /// </summary>
    public class PathfindingManager : MonoBehaviour
    {
        public static PathfindingManager Instance { get; private set; }

        // Standard A* costs
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        // Max search depth to prevent infinite loops on unreachable targets
        private const int MAX_ITERATIONS = 2000;

        // Reusable node pool — cleared between pathfinding calls
        private readonly Dictionary<Vector2Int, PathNode> nodePool = new Dictionary<Vector2Int, PathNode>(512);
        private readonly List<PathNode> openList = new List<PathNode>(256);
        private readonly HashSet<PathNode> closedSet = new HashSet<PathNode>();

        void Awake()
        {
            if (Instance != null)
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

        // ─── Public API ───

        /// <summary>
        /// Find a path from start to end. Results written to vectorPathBuffer.
        /// Uses GridManager.IsSolidAt() for walkability — fully chunk-aware.
        /// </summary>
        public void FindPath(Vector3 startWorldPos, Vector3 endWorldPos, List<Vector3> vectorPathBuffer)
        {
            vectorPathBuffer.Clear();

            Vector2Int startCell = WorldToCell(startWorldPos);
            Vector2Int endCell = WorldToCell(endWorldPos);

            // V24.10: We no longer abort if the destination is solid. 
            // We want the bot to pathfind to the closest ADJACENT walkable cell to interact with the machine.

            FindPathInternal(startCell, endCell, vectorPathBuffer);
        }

        /// <summary>
        /// Check if a world position is walkable.
        /// </summary>
        public bool IsWalkable(Vector3 worldPos)
        {
            if (GridManager.Instance == null) return true;
            return !GridManager.Instance.IsSolidAt(WorldToCell(worldPos));
        }

        /// <summary>
        /// V23 Data-Driven: Set walkability — now a no-op since we query GridManager directly.
        /// Kept for API compatibility. GridManager.IsSolidAt() is the source of truth.
        /// </summary>
        public void SetWalkableAtWorldPos(Vector3 worldPos, bool walkable)
        {
            // No-op: GridManager.IsSolidAt() is the live source of truth.
            // This method exists only so callers don't break.
        }

        // ─── A* Implementation ───

        private void FindPathInternal(Vector2Int start, Vector2Int end, List<Vector3> results)
        {
            // Clear pools from last call
            nodePool.Clear();
            openList.Clear();
            closedSet.Clear();

            PathNode startNode = GetOrCreateNode(start);
            PathNode endNode = GetOrCreateNode(end);

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(start, end);
            openList.Add(startNode);

            int iterations = 0;

            while (openList.Count > 0 && iterations < MAX_ITERATIONS)
            {
                iterations++;
                PathNode current = GetLowestFCostNode(openList);

                if (current.position == end)
                {
                    // Trace path
                    TracePath(current, results);
                    return;
                }

                openList.Remove(current);
                closedSet.Add(current);

                // Expand neighbors (8-directional)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        Vector2Int neighborPos = new Vector2Int(current.position.x + dx, current.position.y + dy);

                        // Check walkability via GridManager (chunk-aware)
                        if (GridManager.Instance != null && GridManager.Instance.IsSolidAt(neighborPos))
                        {
                            // V24.10: If the solid neighbor IS our target (e.g. a Machine),
                            // it means we are standing right next to it! Path is complete.
                            if (neighborPos == end)
                            {
                                // Trace path from CURRENT node (so the bot stops 1 tile away, facing the machine)
                                TracePath(current, results);
                                return;
                            }
                            continue;
                        }

                        PathNode neighbor = GetOrCreateNode(neighborPos);
                        if (closedSet.Contains(neighbor)) continue;

                        int moveCost = (dx != 0 && dy != 0) ? MOVE_DIAGONAL_COST : MOVE_STRAIGHT_COST;
                        int tentativeG = current.gCost + moveCost;

                        if (tentativeG < neighbor.gCost)
                        {
                            neighbor.parentNode = current;
                            neighbor.gCost = tentativeG;
                            neighbor.hCost = CalculateDistanceCost(neighborPos, end);

                            if (!openList.Contains(neighbor))
                                openList.Add(neighbor);
                        }
                    }
                }
            }

            // No path found
        }

        private void TracePath(PathNode endNode, List<Vector3> results)
        {
            PathNode current = endNode;
            while (current != null)
            {
                results.Add(CellToWorld(current.position));
                current = current.parentNode;
            }
            results.Reverse();
        }

        // ─── Node Pool ───

        private PathNode GetOrCreateNode(Vector2Int pos)
        {
            if (nodePool.TryGetValue(pos, out PathNode existing))
                return existing;

            PathNode node = new PathNode(pos.x, pos.y);
            node.position = pos;
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parentNode = null;
            nodePool[pos] = node;
            return node;
        }

        // ─── Math ───

        private int CalculateDistanceCost(Vector2Int a, Vector2Int b)
        {
            int xDist = Mathf.Abs(a.x - b.x);
            int yDist = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDist - yDist);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDist, yDist) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> list)
        {
            PathNode lowest = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].FCost < lowest.FCost)
                    lowest = list[i];
            }
            return lowest;
        }

        private Vector2Int WorldToCell(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
        }

        private Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        }
    }
}
