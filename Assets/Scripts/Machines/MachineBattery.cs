using UnityEngine;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// MachineBattery — Stores power and acts as both consumer (charging) and generator (discharging).
    /// Charge is deducted by PowerManager via OnPowerProvided callback.
    /// </summary>
    public class MachineBattery : MonoBehaviour, IPowerConsumer, IPowerGenerator
    {
        [Header("Capacity")]
        [SerializeField] private float maxCapacity = 1000f;
        [SerializeField] private float currentCharge = 0f;

        [Header("Rates")]
        [SerializeField] private float chargeRate = 10f;
        [SerializeField] private float dischargeRate = 5f;
        [SerializeField] private float range = 5f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer fillBar;

        private bool isDischarging = true;

        // ─── IPowerConsumer ───
        public float PowerRequired => currentCharge < maxCapacity ? chargeRate : 0f;
        public bool IsPowered => currentCharge > 0;
        public Vector3 Position => transform.position;

        public void ReceivePower(float amount)
        {
            float absorb = Mathf.Min(maxCapacity - currentCharge, amount, chargeRate);
            currentCharge += absorb;
        }

        // ─── IPowerGenerator ───
        public float PowerProduction => isDischarging && currentCharge > 0 ? Mathf.Min(currentCharge, dischargeRate) : 0f;
        public float PowerRange => range;
        public bool IsActive => isDischarging && currentCharge > 0;

        public void OnPowerProvided(float amount)
        {
            currentCharge = Mathf.Max(0, currentCharge - amount);
        }

        void OnEnable()
        {
            // Reset charge when placed/spawned from pool
            currentCharge = 0f;
            
            TryRegisterOnGrid();
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.RegisterConsumer(this);
                PowerManager.Instance.RegisterGenerator(this);
            }
        }

        void OnDisable()
        {
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.UnregisterConsumer(this);
                PowerManager.Instance.UnregisterGenerator(this);
            }
        }

        private void TryRegisterOnGrid()
        {
            // Grid registration is now managed DOD-wide by PlacementManager/ChunkManager
        }
    }
}
