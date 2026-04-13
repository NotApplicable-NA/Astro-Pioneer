using UnityEngine;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// MachineGenerator — Infinite power source. Provides wireless power to consumers in range.
    /// </summary>
    public class MachineGenerator : MonoBehaviour, IPowerGenerator
    {
        [Header("Generator Settings")]
        [SerializeField] private float productionRate = 10f;
        [SerializeField] private float effectiveRange = 5f;
        [SerializeField] private bool autoActivate = true;

        private bool isActive;

        // ─── IPowerGenerator ───
        public float PowerProduction => productionRate;
        public float PowerRange => effectiveRange;
        public bool IsActive => isActive;
        public Vector3 Position => transform.position;
        public void OnPowerProvided(float amount) { }

        void Start()
        {
            TryRegisterOnGrid();
            if (autoActivate) isActive = true;

            if (PowerManager.Instance != null)
                PowerManager.Instance.RegisterGenerator(this);
        }

        void OnDestroy()
        {
            if (PowerManager.Instance != null)
                PowerManager.Instance.UnregisterGenerator(this);
        }

        public void TogglePower() => isActive = !isActive;

        private void TryRegisterOnGrid()
        {
            if (GridManager.Instance == null) return;
            Vector2Int pos = GridManager.Instance.WorldToGridPosition(transform.position);
            if (!GridManager.Instance.GetOccupiedCells().ContainsKey(pos))
                GridManager.Instance.TryOccupyCell(pos, gameObject);
        }
    }
}
