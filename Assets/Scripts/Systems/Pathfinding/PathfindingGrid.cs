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
        
        public PathfindingGrid(int width, int height, float cellSize, Vector3 originPosition)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new PathNode[width, height];

            // V23: All nodes start walkable. Actual walkability is set by
            // ChunkRenderer.SyncPathfindingForChunk() using GridManager data.
            // This eliminates the race condition where Physics2D scanned before visuals existed.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridArray[x, y] = new PathNode(x, y);
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
        /// Re-scan a single node's walkability using GridManager (Data-Driven).
        /// Call when an obstacle is placed or removed at runtime.
        /// </summary>
        public void RefreshNode(int x, int y)
        {
            PathNode node = GetNode(x, y);
            if (node == null) return;

            Vector3 worldPos = GetWorldPosition(x, y);
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
            
            if (AstroPioneer.Managers.GridManager.Instance != null)
            {
                node.isWalkable = !AstroPioneer.Managers.GridManager.Instance.IsSolidAt(gridPos);
            }
            else
            {
                node.isWalkable = true;
            }
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
