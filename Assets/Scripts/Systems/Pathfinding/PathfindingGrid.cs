using UnityEngine;
using UnityEngine.Tilemaps;

namespace AstroPioneer.Systems.Pathfinding
{
    public class PathfindingGrid
    {
        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        private PathNode[,] gridArray;
        
        // Physics-based collision
        private LayerMask obstacleLayerMask;

        public PathfindingGrid(int width, int height, float cellSize, Vector3 originPosition, LayerMask obstacleMask)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            this.obstacleLayerMask = obstacleMask;

            gridArray = new PathNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    PathNode node = new PathNode(x, y);
                    gridArray[x, y] = node;
                    
                    // Auto-detect walkability using Physics2D
                    Vector3 worldPos = GetWorldPosition(x, y) + new Vector3(cellSize * 0.5f, cellSize * 0.5f);
                    // Check if any collider in obstacle layer overlaps this cell center (radius 0.3 to avoid edge bleed)
                    if (Physics2D.OverlapCircle(worldPos, cellSize * 0.3f, obstacleLayerMask) != null)
                    {
                        node.isWalkable = false;
                    }
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }

        public PathNode GetNode(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            return null;
        }

        public int GetWidth() => width;
        public int GetHeight() => height;

        /// <summary>
        /// Re-scan a single node's walkability using Physics2D.
        /// Call when an obstacle is placed or removed at runtime.
        /// </summary>
        public void RefreshNode(int x, int y)
        {
            PathNode node = GetNode(x, y);
            if (node == null) return;

            Vector3 worldPos = GetWorldPosition(x, y) + new Vector3(cellSize * 0.5f, cellSize * 0.5f);
            node.isWalkable = Physics2D.OverlapCircle(worldPos, cellSize * 0.3f, obstacleLayerMask) == null;
        }

        /// <summary>
        /// Refresh a node at a given world position.
        /// </summary>
        public void RefreshNodeAtWorldPos(Vector3 worldPos)
        {
            GetXY(worldPos, out int x, out int y);
            RefreshNode(x, y);
        }

        /// <summary>
        /// Manually set walkability for a node.
        /// </summary>
        public void SetWalkable(int x, int y, bool walkable)
        {
            PathNode node = GetNode(x, y);
            if (node != null) node.isWalkable = walkable;
        }

        /// <summary>
        /// Rebuild the entire grid. Use sparingly (expensive).
        /// </summary>
        public void RebuildGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    RefreshNode(x, y);
                }
            }
        }
    }
}
