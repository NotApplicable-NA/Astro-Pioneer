using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Environment
{
    /// <summary>
    /// ShadowZone - Utility script to mark cell as part of a Shadow Canyon.
    /// Can be used as a trigger zone or assigned to a specific prefab.
    /// </summary>
    public class ShadowZone : MonoBehaviour
    {
        [Header("Manual Grid Mode")]
        [SerializeField] private Vector2Int[] specificCells;
        
        [Header("Box Trigger Mode")]
        [SerializeField] private bool useCurrentPosition = true;
        [SerializeField] private Vector2Int shadowSize = new Vector2Int(1, 1);

        void Start()
        {
            RegisterShadow();
        }

        private void RegisterShadow()
        {
            if (GridManager.Instance == null) return;

            // 1. Manual cells
            if (specificCells != null)
            {
                foreach (var pos in specificCells)
                    GridManager.Instance.SetShadowCell(pos, true);
            }

            // 2. Box mode
            if (useCurrentPosition)
            {
                Vector2Int origin = GridManager.Instance.WorldToGridPosition(transform.position);
                for (int x = 0; x < shadowSize.x; x++)
                {
                    for (int y = 0; y < shadowSize.y; y++)
                    {
                        GridManager.Instance.SetShadowCell(origin + new Vector2Int(x, y), true);
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (GridManager.Instance == null) return;
            
            Gizmos.color = new Color(0f, 0f, 0.4f, 0.4f);
            
            if (useCurrentPosition)
            {
                Vector2Int origin = GridManager.Instance.WorldToGridPosition(transform.position);
                for (int x = 0; x < shadowSize.x; x++)
                {
                    for (int y = 0; y < shadowSize.y; y++)
                    {
                         Vector3 worldPos = GridManager.Instance.GridToWorldPosition(origin + new Vector2Int(x, y));
                         Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);
                    }
                }
            }
        }
    }
}
