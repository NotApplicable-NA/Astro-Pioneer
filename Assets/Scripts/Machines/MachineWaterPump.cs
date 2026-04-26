using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// MachineWaterPump — Produces water over time when powered.
    /// Bots can collect water via TryTakeWater.
    /// </summary>
    public class MachineWaterPump : MonoBehaviour, IPowerConsumer
    {
        [Header("Production")]
        [SerializeField] private float productionRate = 1f;
        [SerializeField] private int maxCapacity = 100;
        [SerializeField] private float powerConsumption = 2f;

        [Header("Item Data")]
        [SerializeField] private InventoryItem waterItemData;
        public InventoryItem WaterItemData => waterItemData;

        [Header("State")]
        [SerializeField] private float currentWaterStored;
        [SerializeField] private bool isActive = true;
        private bool isPowered;

        /// <summary>True if the pump has at least 1 unit of water available for pickup.</summary>
        public bool HasWater => currentWaterStored >= 1f;

        // ─── IPowerConsumer ───
        public float PowerRequired => powerConsumption;
        public bool IsPowered => isPowered;
        public Vector3 Position => transform.position;
        public void ReceivePower(float amount) => isPowered = amount >= powerConsumption;

        void Start()
        {
            TryRegisterOnGrid();
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
            if (isActive && isPowered && currentWaterStored < maxCapacity)
                currentWaterStored = Mathf.Min(currentWaterStored + productionRate * Time.deltaTime, maxCapacity);
        }

        public bool TryTakeWater(int amount, out InventoryItem item)
        {
            item = null;
            if (waterItemData == null || currentWaterStored < amount) return false;

            currentWaterStored -= amount;
            item = waterItemData;
            return true;
        }

        public string GetStatusText()
        {
            return $"Water: {Mathf.FloorToInt(currentWaterStored)}/{maxCapacity} [{(isPowered ? "ON" : "NO POWER")}]";
        }

        private void TryRegisterOnGrid()
        {
            // Grid registration is now managed DOD-wide by PlacementManager/ChunkManager
        }
    }
}
