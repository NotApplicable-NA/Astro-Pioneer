using UnityEngine;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;
using AstroPioneer.Systems;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// MachineHarvester — Auto-harvests mature crops in a 3×3 area. Requires power.
    /// </summary>
    public class MachineHarvester : MonoBehaviour, IPowerConsumer
    {
        [Header("Settings")]
        [SerializeField] private float scanInterval = 5f;
        [SerializeField] private float powerRequired = 15f;
        [SerializeField] private bool bypassPower = true;

        private float scanTimer;
        private Vector2Int gridPos;
        private bool isPowered;

        // ─── IPowerConsumer ───
        public float PowerRequired => powerRequired;
        public bool IsPowered => isPowered;
        public Vector3 Position => transform.position;

        public void ReceivePower(float amount) => isPowered = amount >= powerRequired;

        void Start()
        {
            if (GridManager.Instance != null)
                gridPos = GridManager.Instance.WorldToGridPosition(transform.position);

            if (PowerManager.Instance != null)
                PowerManager.Instance.RegisterConsumer(this);
        }

        void OnDestroy()
        {
            if (PowerManager.Instance != null)
                PowerManager.Instance.UnregisterConsumer(this);
        }

        void Update()
        {
            if (!isPowered && !bypassPower) return;

            scanTimer += Time.deltaTime;
            if (scanTimer >= scanInterval)
            {
                scanTimer = 0;
                PerformHarvestScan();
            }
        }

        private void PerformHarvestScan()
        {
            if (CropManager.Instance == null) return;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    CropInstance crop = CropManager.Instance.GetCropAt(gridPos + new Vector2Int(x, y));
                    if (crop != null && crop.IsHarvestable())
                        crop.Harvest();
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (GridManager.Instance == null) return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Vector2Int pos = GridManager.Instance.WorldToGridPosition(transform.position);
            float cSize = GridManager.Instance.CellSize;

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    Gizmos.DrawCube(GridManager.Instance.GridToWorldPosition(pos + new Vector2Int(x, y)),
                        Vector3.one * cSize);
        }
    }
}
