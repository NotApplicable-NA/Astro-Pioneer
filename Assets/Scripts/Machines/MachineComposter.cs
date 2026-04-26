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

        [Header("State (Durable)")]
        [SerializeField] private int maxOutputCapacity = 20;
        private byte[] stateBuffer;
        private float lastTickTime;
        private float currentProcessTimer;

        [Header("Runtime")]
        [SerializeField] private int currentInputCount;
        [SerializeField] private int currentOutputCount;
        [SerializeField] private bool isProcessing;

        private void OnValidate()
        {
            if (processingTime < 0.1f) processingTime = 0.1f;
        }

        private void Start()
        {
            InitializeState();
        }

        private void InitializeState()
        {
            Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(transform.position);
            stateBuffer = GridManager.Instance.GetOrAllocateComplexState(gridPos);
            
            LoadState();

            // Catch-up Logic (Offline Progression)
            if (isProcessing && lastTickTime > 0)
            {
                float now = TimeManager.Instance.TotalGameSeconds;
                float elapsed = now - lastTickTime;
                
                if (elapsed > 0)
                {
                    PerformCatchUp(elapsed);
                }
            }
        }

        private void LoadState()
        {
            if (stateBuffer == null || stateBuffer.Length < 16) return;
            
            currentInputCount = System.BitConverter.ToInt32(stateBuffer, 0);
            currentOutputCount = System.BitConverter.ToInt32(stateBuffer, 4);
            currentProcessTimer = System.BitConverter.ToSingle(stateBuffer, 8);
            lastTickTime = System.BitConverter.ToSingle(stateBuffer, 12);
            
            isProcessing = currentInputCount > 0 || currentProcessTimer > 0;
        }

        private void SaveState()
        {
            if (stateBuffer == null || stateBuffer.Length < 16) return;

            System.BitConverter.GetBytes(currentInputCount).CopyTo(stateBuffer, 0);
            System.BitConverter.GetBytes(currentOutputCount).CopyTo(stateBuffer, 4);
            System.BitConverter.GetBytes(currentProcessTimer).CopyTo(stateBuffer, 8);
            System.BitConverter.GetBytes(TimeManager.Instance.TotalGameSeconds).CopyTo(stateBuffer, 12);
        }

        private void Update()
        {
            if (!isProcessing || processingTime < 0.1f) return;

            if (currentOutputCount >= maxOutputCapacity)
            {
                // Idle while full
                return;
            }

            currentProcessTimer += Time.deltaTime; 
            
            if (currentProcessTimer >= processingTime)
            {
                currentProcessTimer -= processingTime;
                currentInputCount--;
                currentOutputCount++;
                
                if (currentInputCount <= 0)
                {
                    isProcessing = false;
                    currentProcessTimer = 0f;
                }
            }

            SaveState();
        }

        private void PerformCatchUp(float elapsedSeconds)
        {
            if (processingTime < 0.1f) return;
            
            float totalAvailableTime = elapsedSeconds + currentProcessTimer;
            
            // AAA Optimization: O(1) Math replaces O(N) while loop
            // Calculate how many units COULD be produced in this time
            int possibleUnits = Mathf.FloorToInt(totalAvailableTime / processingTime);
            
            // Clamp by actual resources (Input and Storage)
            int actualUnits = Mathf.Min(possibleUnits, currentInputCount);
            actualUnits = Mathf.Min(actualUnits, maxOutputCapacity - currentOutputCount);

            if (actualUnits > 0)
            {
                currentInputCount -= actualUnits;
                currentOutputCount += actualUnits;
            }

            // If we still have input and storage room, the remainder is the new timer
            if (currentInputCount > 0 && currentOutputCount < maxOutputCapacity)
            {
                currentProcessTimer = totalAvailableTime % processingTime;
            }
            else
            {
                currentProcessTimer = 0f;
            }

            isProcessing = currentInputCount > 0;
            
            SaveState();
        }

        public void Interact(InventoryItem heldItem)
        {
            if (heldItem != null && heldItem == inputItem)
                TryAddInput();
            else
                TryCollectOutput();
        }

        private void TryAddInput()
        {
            if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem(inputItem, 1)) return;

            InventoryManager.Instance.RemoveItem(inputItem, 1);
            currentInputCount++;
            isProcessing = true;
            SaveState();
        }

        private void TryCollectOutput()
        {
            if (currentOutputCount <= 0) return;

            if (InventoryManager.Instance.AddItem(outputItem, currentOutputCount))
            {
                currentOutputCount = 0;
                // Resume processing if was full
                isProcessing = currentInputCount > 0;
                SaveState();
            }
        }
    }
}
