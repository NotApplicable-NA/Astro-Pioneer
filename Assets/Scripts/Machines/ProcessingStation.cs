using UnityEngine;
using System.Collections;
using AstroPioneer.Data;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// ProcessingStation — Crafts items from recipes over time. Pauses when unpowered.
    /// </summary>
    public class ProcessingStation : MonoBehaviour, IPowerConsumer
    {
        [Header("Settings")]
        [SerializeField] private float powerConsumption = 3f;

        [Header("State")]
        [SerializeField] private bool isProcessing;
        private bool isPowered;
        private CraftingRecipe currentRecipe;
        private float processProgress;

        // ─── IPowerConsumer ───
        public float PowerRequired => powerConsumption;
        public bool IsPowered => isPowered;
        public Vector3 Position => transform.position;
        public void ReceivePower(float amount) => isPowered = amount >= powerConsumption;

        // ─── Public Properties ───
        public bool IsProcessing => isProcessing;
        public CraftingRecipe CurrentRecipe => currentRecipe;
        public float ProcessProgress => processProgress;

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

        public bool StartProcessing(CraftingRecipe recipe)
        {
            if (isProcessing) return false;
            if (recipe.requiredStation != CraftingStation.ProcessingStation) return false;

            currentRecipe = recipe;
            isProcessing = true;
            processProgress = 0f;
            StartCoroutine(ProcessItem());
            return true;
        }

        private IEnumerator ProcessItem()
        {
            float elapsed = 0f;
            while (elapsed < currentRecipe.craftTime)
            {
                if (!isPowered)
                    yield return new WaitUntil(() => isPowered);

                elapsed += Time.deltaTime;
                processProgress = Mathf.Clamp01(elapsed / currentRecipe.craftTime);
                yield return null;
            }

            processProgress = 1f;

            // Deliver result (retry if inventory full)
            if (InventoryManager.Instance != null && currentRecipe.resultItem != null)
            {
                if (!InventoryManager.Instance.AddItem(currentRecipe.resultItem, currentRecipe.resultQuantity))
                    yield return new WaitUntil(() =>
                        InventoryManager.Instance.AddItem(currentRecipe.resultItem, currentRecipe.resultQuantity));
            }

            currentRecipe = null;
            isProcessing = false;
            processProgress = 0f;
        }

        public string GetStatusText()
        {
            if (!isPowered) return "NO POWER";
            if (isProcessing && currentRecipe != null)
                return $"Processing: {currentRecipe.displayName} ({Mathf.FloorToInt(processProgress * 100)}%)";
            return "Idle";
        }

        private void TryRegisterOnGrid()
        {
            if (GridManager.Instance == null) return;
            Vector2Int pos = GridManager.Instance.WorldToGridPosition(transform.position);
            if (!GridManager.Instance.GetOccupiedCells().ContainsKey(pos))
                GridManager.Instance.TryOccupyCell(pos, gameObject);
        }
    }
}
