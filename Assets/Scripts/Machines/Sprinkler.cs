using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// Sprinkler — Auto-waters adjacent tiles in cross or square pattern.
    /// Activated by TimeManager on morning rollover.
    /// </summary>
    public class Sprinkler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int waterRange = 1;
        [SerializeField] private bool useCrossPattern = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 0.5f, 1f, 0.5f);

        public static event System.Action<Sprinkler> OnSprinklerActivated;

        private Vector2Int gridPosition;

        public void SetGridPosition(Vector2Int pos) => gridPosition = pos;

        public List<Vector2Int> GetWateringTiles()
        {
            var tiles = new List<Vector2Int>();

            if (useCrossPattern)
            {
                tiles.Add(gridPosition + Vector2Int.up);
                tiles.Add(gridPosition + Vector2Int.down);
                tiles.Add(gridPosition + Vector2Int.left);
                tiles.Add(gridPosition + Vector2Int.right);
            }
            else
            {
                for (int x = -waterRange; x <= waterRange; x++)
                    for (int y = -waterRange; y <= waterRange; y++)
                        if (x != 0 || y != 0)
                            tiles.Add(gridPosition + new Vector2Int(x, y));
            }
            return tiles;
        }

        public void ActivateSprinkler()
        {
            // TODO: Integrate with CropManager watering
            OnSprinklerActivated?.Invoke(this);
        }

        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Gizmos.color = gizmoColor;
            foreach (Vector2Int tile in GetWateringTiles())
                Gizmos.DrawCube(new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f), new Vector3(0.9f, 0.9f, 0.1f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
            foreach (Vector2Int tile in GetWateringTiles())
                Gizmos.DrawCube(new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f), new Vector3(0.95f, 0.95f, 0.1f));
        }
    }
}
