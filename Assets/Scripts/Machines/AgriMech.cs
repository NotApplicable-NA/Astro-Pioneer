using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// AgriMech — Mobile farming machine that traverses horizontally across the grid,
    /// planting crops on every cell it passes over.
    /// </summary>
    public class AgriMech : MonoBehaviour
    {
        [Header("Settings")]
        public Vector2Int dimensions = new Vector2Int(2, 2);

        [Header("Farming")]
        public CropData cropData;

        [Header("Movement")]
        public float moveInterval = 1f;
        public int direction = 1; // 1 = Right, -1 = Left

        [Header("Runtime")]
        public Vector2Int currentGridPos;
        public bool isInitialized;

        private float moveTimer;

        public void Initialize(Vector2Int spawnPos)
        {
            currentGridPos = spawnPos;
            isInitialized = true;
            UpdateVisualPosition();
        }

        public void Interact(InventoryItem item)
        {
            if (item != null && item.type == ItemType.Seed)
                cropData = item.plantData;
        }

        void Update()
        {
            if (!isInitialized || cropData == null) return;

            moveTimer += Time.deltaTime;
            if (moveTimer >= moveInterval)
            {
                moveTimer = 0f;
                MoveMachine();
            }
        }

        // ─── Movement ───

        private void MoveMachine()
        {
            if (GridManager.Instance == null) return;

            Vector2Int nextPos = currentGridPos + new Vector2Int(direction, 0);

            if (IsBlocked(nextPos))
            {
                direction *= -1;
                return;
            }

            ReleaseOccupiedCells();
            currentGridPos = nextPos;
            UpdateVisualPosition();
            OccupyCurrentCells();
            PlantCropsAtCurrentPosition();
        }

        private bool IsBlocked(Vector2Int origin)
        {
            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    if (!GridManager.Instance.IsValidGridPosition(origin + new Vector2Int(x, y)))
                        return true;
            return false;
        }

        private void ReleaseOccupiedCells()
        {
            ForEachCell(currentGridPos, cell =>
            {
                if (GridManager.Instance.GetOccupantAt(cell) == gameObject)
                    GridManager.Instance.ReleaseCell(cell);
            });
        }

        private void OccupyCurrentCells()
        {
            ForEachCell(currentGridPos, cell =>
            {
                if (GridManager.Instance.GetOccupantAt(cell) == null)
                    GridManager.Instance.TryOccupyCell(cell, gameObject);
            });
        }

        private void PlantCropsAtCurrentPosition()
        {
            if (cropData == null || CropManager.Instance == null) return;

            ForEachCell(currentGridPos, cell =>
            {
                if (CropManager.Instance.GetCropAt(cell) == null)
                    CropManager.Instance.PlantCrop(cell, cropData);
            });
        }

        // ─── Helpers ───

        public void UpdateVisualPosition()
        {
            if (!isInitialized || GridManager.Instance == null) return;

            float cSize = GridManager.Instance.CellSize;
            Vector3 corner = GridManager.Instance.GridOrigin +
                new Vector3(currentGridPos.x * cSize, currentGridPos.y * cSize, 0);
            Vector3 center = new Vector3(dimensions.x * 0.5f * cSize, dimensions.y * 0.5f * cSize, 0);
            transform.position = corner + center;
        }

        private void ForEachCell(Vector2Int origin, System.Action<Vector2Int> action)
        {
            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    action(origin + new Vector2Int(x, y));
        }
    }
}
