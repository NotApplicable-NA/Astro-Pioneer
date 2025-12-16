using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// Sprinkler machine - auto-water 4 adjacent tiles in cross pattern.
    /// Trigger: Morning time change.
    /// </summary>
    public class Sprinkler : MonoBehaviour
    {
        [Header("Sprinkler Settings")]
        [Tooltip("Range dalam grid cells (1 = immediate neighbors)")]
        [SerializeField] private int waterRange = 1;
        
        [Tooltip("Gunakan pattern cross (4 tiles) atau square (8 tiles)")]
        [SerializeField] private bool useCrossPattern = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 0.5f, 1f, 0.5f);
        
        // Event untuk integration dengan Day/Night system
        public static event System.Action<Sprinkler> OnSprinklerActivated;
        
        // Grid position (akan di-set oleh PlacementSystem nanti)
        private Vector2Int gridPosition;
        
        /// <summary>
        /// Set grid position untuk sprinkler (dipanggil saat placement)
        /// </summary>
        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
        }
        
        /// <summary>
        /// Get tiles yang akan di-water (cross pattern: Up, Down, Left, Right)
        /// </summary>
        public List<Vector2Int> GetWateringTiles()
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            
            if (useCrossPattern)
            {
                // Cross pattern: 4 adjacent tiles
                tiles.Add(gridPosition + Vector2Int.up);    // Atas
                tiles.Add(gridPosition + Vector2Int.down);  // Bawah
                tiles.Add(gridPosition + Vector2Int.left);  // Kiri
                tiles.Add(gridPosition + Vector2Int.right); // Kanan
            }
            else
            {
                // Square pattern: 8 surrounding tiles
                for (int x = -waterRange; x <= waterRange; x++)
                {
                    for (int y = -waterRange; y <= waterRange; y++)
                    {
                        if (x == 0 && y == 0) continue; // Skip self
                        tiles.Add(gridPosition + new Vector2Int(x, y));
                    }
                }
            }
            
            return tiles;
        }
        
        /// <summary>
        /// Activate sprinkler - dipanggil oleh TimeManager saat morning
        /// </summary>
        public void ActivateSprinkler()
        {
            List<Vector2Int> tilesToWater = GetWateringTiles();
            
            foreach (Vector2Int tilePos in tilesToWater)
            {
                // TODO: Integrate dengan CropSystem untuk water tiles
                // CropSystem.Instance?.WaterTile(tilePos);
                Debug.Log($"[Sprinkler] Watering tile at {tilePos}");
            }
            
            // Trigger event untuk VFX/SFX
            OnSprinklerActivated?.Invoke(this);
            
            Debug.Log($"[Sprinkler] Activated! Watered {tilesToWater.Count} tiles.");
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            Gizmos.color = gizmoColor;
            
            // Draw watering area
            List<Vector2Int> tiles = GetWateringTiles();
            foreach (Vector2Int tile in tiles)
            {
                Vector3 worldPos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
                Gizmos.DrawCube(worldPos, new Vector3(0.9f, 0.9f, 0.1f));
            }
            
            // Draw sprinkler position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // More visible when selected
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
            List<Vector2Int> tiles = GetWateringTiles();
            foreach (Vector2Int tile in tiles)
            {
                Vector3 worldPos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
                Gizmos.DrawCube(worldPos, new Vector3(0.95f, 0.95f, 0.1f));
            }
        }
    }
}
