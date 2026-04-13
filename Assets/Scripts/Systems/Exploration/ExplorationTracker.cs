using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Exploration
{
    /// <summary>
    /// ExplorationTracker - Attached to Player.
    /// Reveals the GridManager discovery state (Fog of War) in a radius around the player.
    /// </summary>
    public class ExplorationTracker : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int revealRadius = 1; // 1 = 3x3, 2 = 5x5
        [SerializeField] private float updateInterval = 0.5f;

        private Vector2Int lastGridPos = new Vector2Int(-999, -999);
        private float timer = 0f;

        void Update()
        {
            if (GridManager.Instance == null) return;

            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                CheckAndReveal();
            }
        }

        private void CheckAndReveal()
        {
            Vector2Int currentPos = GridManager.Instance.WorldToGridPosition(transform.position);
            
            if (currentPos != lastGridPos)
            {
                lastGridPos = currentPos;
                RevealArea(currentPos);
            }
        }

        private void RevealArea(Vector2Int center)
        {
            for (int x = -revealRadius; x <= revealRadius; x++)
            {
                for (int y = -revealRadius; y <= revealRadius; y++)
                {
                    Vector2Int targetPos = center + new Vector2Int(x, y);
                    GridManager.Instance.ExploreCell(targetPos);
                }
            }
        }

        // Call this when starting a new session or planet
        public void ForceReveal()
        {
            if (GridManager.Instance != null)
            {
                RevealArea(GridManager.Instance.WorldToGridPosition(transform.position));
            }
        }
    }
}
