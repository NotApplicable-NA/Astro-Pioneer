using UnityEngine;
using System.Collections;
using AstroPioneer.Data;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// Composter — Recycles organic waste (Bio-Mass) into Bio-Fertilizer over time.
    /// Does not require power.
    /// </summary>
    public class MachineComposter : MonoBehaviour, IGridInteractable
    {
        [Header("Recipes")]
        [SerializeField] private InventoryItem inputItem;
        [SerializeField] private InventoryItem outputItem;
        [SerializeField] private float processingTime = 60f;

        [Header("Runtime")]
        [SerializeField] private int currentInputCount;
        [SerializeField] private int currentOutputCount;
        [SerializeField] private bool isProcessing;

        public void Interact(InventoryItem heldItem)
        {
            if (heldItem != null && heldItem.id == inputItem.id)
                TryAddInput();
            else
                TryCollectOutput();
        }

        private void TryAddInput()
        {
            if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem(inputItem, 1)) return;

            InventoryManager.Instance.RemoveItem(inputItem, 1);
            currentInputCount++;

            if (!isProcessing)
                StartCoroutine(ProcessRoutine());
        }

        private void TryCollectOutput()
        {
            if (currentOutputCount <= 0) return;

            if (InventoryManager.Instance.AddItem(outputItem, currentOutputCount))
                currentOutputCount = 0;
        }

        private IEnumerator ProcessRoutine()
        {
            isProcessing = true;

            while (currentInputCount > 0)
            {
                float elapsed = 0f;
                while (elapsed < processingTime)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                currentInputCount--;
                currentOutputCount++;
            }

            isProcessing = false;
        }
    }
}
