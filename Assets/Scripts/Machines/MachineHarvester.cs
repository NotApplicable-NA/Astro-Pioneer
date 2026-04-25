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
        private int frameSliceOffset;

        // ─── IPowerConsumer ───
        public float PowerRequired => powerRequired;
        public bool IsPowered => isPowered;
        public Vector3 Position => transform.position;

        public void ReceivePower(float amount) => isPowered = amount >= powerRequired;

        void Start()
        {
            if (GridManager.Instance != null)
                gridPos = GridManager.Instance.WorldToGridPosition(transform.position);

            frameSliceOffset = Mathf.Abs(GetInstanceID()) % 10;

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
            if ((Time.frameCount + frameSliceOffset) % 2 != 0) return;

            scanTimer += Time.deltaTime * 2f;
            if (scanTimer >= scanInterval)
            {
                scanTimer -= scanInterval;
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
                    Vector2Int pos = gridPos + new Vector2Int(x, y);
                    if (CropManager.Instance.TryHarvestCropAt(pos, out AstroPioneer.Data.CropStructureData data))
                    {
                        if (data != null && AstroPioneer.Managers.InventoryManager.Instance != null && data.harvestItem != null)
                            AstroPioneer.Managers.InventoryManager.Instance.AddItem(data.harvestItem, data.harvestQuantity);

                        if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Managers.ObjectPoolManager>(out var pool))
                        {
                            var vfxObj = pool.SpawnFromPool(AstroPioneer.Core.GameConstants.POOL_HARVEST_VFX, null, AstroPioneer.Managers.GridManager.Instance.GridToWorldPosition(pos));
                        }
                    }
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
