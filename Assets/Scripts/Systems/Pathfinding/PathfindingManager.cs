using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace AstroPioneer.Systems.Pathfinding
{
    public class PathfindingManager : MonoBehaviour
    {
        public static PathfindingManager Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 originPosition = new Vector3(-25, -25, 0);
        
        [Header("Collision")]
        [SerializeField] private LayerMask obstacleLayerMask;
        
        private PathfindingGrid grid;
        
        // Standard A* costs
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            grid = new PathfindingGrid(width, height, cellSize, originPosition, obstacleLayerMask);
        }

        public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            grid.GetXY(startWorldPos, out int startX, out int startY);
            grid.GetXY(endWorldPos, out int endX, out int endY);

            List<PathNode> pathNodes = FindPath(startX, startY, endX, endY);
            
            if (pathNodes == null) return null;

            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode node in pathNodes)
            {
                // Return center of cell
                vectorPath.Add(grid.GetWorldPosition(node.x, node.y) + new Vector3(cellSize * 0.5f, cellSize * 0.5f));
            }
            return vectorPath;
        }

        public bool IsWalkable(Vector3 worldPos)
        {
            grid.GetXY(worldPos, out int x, out int y);
            PathNode node = grid.GetNode(x, y);
            return node != null && node.isWalkable;
        }

        private List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNode startNode = grid.GetNode(startX, startY);
            PathNode endNode = grid.GetNode(endX, endY);

            if (startNode == null || endNode == null) return null;

            List<PathNode> openList = new List<PathNode> { startNode };
            HashSet<PathNode> closedList = new HashSet<PathNode>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetNode(x, y);
                    pathNode.gCost = int.MaxValue;
                    pathNode.parentNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);

                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!neighbourNode.isWalkable) 
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.parentNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        private List<PathNode> GetNeighbourList(PathNode currentNode)
        {
            List<PathNode> neighbourList = new List<PathNode>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(grid.GetNode(currentNode.x - 1, currentNode.y));
                // Left Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetNode(currentNode.x - 1, currentNode.y - 1));
                // Left Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.x + 1 < grid.GetWidth())
            {
                // Right
                neighbourList.Add(grid.GetNode(currentNode.x + 1, currentNode.y));
                // Right Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetNode(currentNode.x + 1, currentNode.y - 1));
                // Right Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            // Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(grid.GetNode(currentNode.x, currentNode.y - 1));
            // Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(grid.GetNode(currentNode.x, currentNode.y + 1));

            return neighbourList;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
            while (currentNode.parentNode != null)
            {
                path.Add(currentNode.parentNode);
                currentNode = currentNode.parentNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].FCost < lowestFCostNode.FCost)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }
        
        // Debug Gizmos
        void OnDrawGizmos()
        {
            if (grid != null)
            {
                Gizmos.color = Color.white;
                // Only draw a subset to avoid lag? Or just draw bounds?
                Gizmos.DrawWireCube(originPosition + new Vector3(width * cellSize * 0.5f, height * cellSize * 0.5f), new Vector3(width * cellSize, height * cellSize));
            }
        }
    }
}
