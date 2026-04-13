using UnityEngine;

namespace AstroPioneer.Systems.Pathfinding
{
    public class PathNode
    {
        public int x;
        public int y;
        
        public int gCost;
        public int hCost;
        public int FCost => gCost + hCost;
        
        public bool isWalkable;
        public PathNode parentNode;
        
        public PathNode(int x, int y)
        {
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public override string ToString()
        {
            return $"{x},{y}";
        }
    }
}
