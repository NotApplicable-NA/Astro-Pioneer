using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// UVLightPillar — Provides artificial sunlight in a radius for farming in Shadow Canyons.
    /// </summary>
    public class UVLightPillar : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int lightRadius = 1; // 1 = 3×3 area

        private Vector2Int gridPosition;
        private bool isRegistered;

        void Start()
        {
            TryRegisterOnGrid();
            RegisterLight();
        }

        void OnDestroy() => UnregisterLight();

        private void RegisterLight()
        {
            if (GridManager.Instance == null) return;
            gridPosition = GridManager.Instance.WorldToGridPosition(transform.position);
            SetLightCells(true);
            isRegistered = true;
        }

        private void UnregisterLight()
        {
            if (!isRegistered || GridManager.Instance == null) return;
            SetLightCells(false);
            isRegistered = false;
        }

        private void SetLightCells(bool add)
        {
            for (int x = -lightRadius; x <= lightRadius; x++)
            {
                for (int y = -lightRadius; y <= lightRadius; y++)
                {
                    Vector2Int pos = gridPosition + new Vector2Int(x, y);
                    if (add)
                        GridManager.Instance.AddLightSource(pos);
                    else
                        GridManager.Instance.RemoveLightSource(pos);
                }
            }
        }

        private void TryRegisterOnGrid()
        {
            // Grid registration is now managed DOD-wide by PlacementManager/ChunkManager
        }

        void OnDrawGizmosSelected()
        {
            if (GridManager.Instance == null) return;

            Gizmos.color = new Color(1f, 1f, 0.5f, 0.3f);
            Vector2Int center = GridManager.Instance.WorldToGridPosition(transform.position);

            for (int x = -lightRadius; x <= lightRadius; x++)
                for (int y = -lightRadius; y <= lightRadius; y++)
                    Gizmos.DrawCube(
                        GridManager.Instance.GridToWorldPosition(center + new Vector2Int(x, y)),
                        Vector3.one * 0.8f);
        }
    }
}
