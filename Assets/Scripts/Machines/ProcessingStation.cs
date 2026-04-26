using UnityEngine;
using System.Collections;
using System.Text;
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

        private void OnValidate()
        {
            // Note: craftTime lives on ScriptableObject CraftingRecipe,
            // but we add a guard here in case logic bypasses it.
        }

        private CraftingRecipe currentRecipe;
        private float processProgress;
        private readonly StringBuilder statusBuffer = new StringBuilder(64);

        [Header("Runtime (Durable)")]
        private byte[] stateBuffer;
        private float lastTickTime;

        // ─── IPowerConsumer ───
        public float PowerRequired => powerConsumption;
        public bool IsPowered => isPowered;
        public Vector3 Position => transform.position;
        public void ReceivePower(float amount) => isPowered = amount >= powerConsumption;

        // ─── Public Properties ───
        public bool IsProcessing => isProcessing;
        public CraftingRecipe CurrentRecipe => currentRecipe;
        public float ProcessProgress => processProgress;

        private void Start()
        {
            InitializeState();
            if (PowerManager.Instance != null)
                PowerManager.Instance.RegisterConsumer(this);
        }

        private void InitializeState()
        {
            Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(transform.position);
            stateBuffer = GridManager.Instance.GetOrAllocateComplexState(gridPos);
            
            LoadState();

            // Catch-up Logic
            if (isProcessing && currentRecipe != null && lastTickTime > 0)
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

            isProcessing = stateBuffer[0] != 0;
            processProgress = System.BitConverter.ToSingle(stateBuffer, 1);
            lastTickTime = System.BitConverter.ToSingle(stateBuffer, 5);

            int idLen = System.BitConverter.ToInt32(stateBuffer, 9);
            if (idLen > 0 && idLen < 100)
            {
                string id = System.Text.Encoding.UTF8.GetString(stateBuffer, 13, idLen);
                if (CraftingManager.Instance != null)
                    currentRecipe = CraftingManager.Instance.GetRecipeByID(id);
            }
        }

        private void SaveState()
        {
            if (stateBuffer == null || stateBuffer.Length < 128) return;

            stateBuffer[0] = (byte)(isProcessing ? 1 : 0);
            System.BitConverter.GetBytes(processProgress).CopyTo(stateBuffer, 1);
            System.BitConverter.GetBytes(TimeManager.Instance.TotalGameSeconds).CopyTo(stateBuffer, 5);

            if (currentRecipe != null)
            {
                byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(currentRecipe.recipeID);
                System.BitConverter.GetBytes(idBytes.Length).CopyTo(stateBuffer, 9);
                idBytes.CopyTo(stateBuffer, 13);
            }
            else
            {
                System.BitConverter.GetBytes(0).CopyTo(stateBuffer, 9);
            }
        }

        private void Update()
        {
            if (!isProcessing || currentRecipe == null) return;
            if (!isPowered) return;

            float craftTime = Mathf.Max(0.1f, currentRecipe.craftTime);
            float delta = Time.deltaTime; 
            processProgress += delta / craftTime;

            if (processProgress >= 1f)
            {
                CompleteCurrentRecipe();
            }

            SaveState();
        }

        private void PerformCatchUp(float elapsedSeconds)
        {
            if (currentRecipe == null) return;
            float craftTime = Mathf.Max(0.1f, currentRecipe.craftTime);
            
            // AAA Optimization: O(1) progress arithmetic
            float currentProgressTime = processProgress * craftTime;
            float totalTime = elapsedSeconds + currentProgressTime;
            
            if (totalTime >= craftTime)
            {
                CompleteCurrentRecipe();
            }
            else
            {
                processProgress = totalTime / craftTime;
            }
            
            SaveState();
        }

        private void CompleteCurrentRecipe()
        {
            processProgress = 1f;
            
            // Deliver result
            if (InventoryManager.Instance != null && currentRecipe.resultItem != null)
            {
                // In offline mode, we assume result fits or is buffered 
                // (for simplicity in this refactor, we add it immediately).
                InventoryManager.Instance.AddItem(currentRecipe.resultItem, currentRecipe.resultQuantity);
            }

            isProcessing = false;
            currentRecipe = null;
            processProgress = 0f;
        }

        void OnDestroy()
        {
            if (PowerManager.Instance != null)
                PowerManager.Instance.UnregisterConsumer(this);
        }

        public bool StartProcessing(CraftingRecipe recipe)
        {
            if (isProcessing) return false;
            if (recipe == null || recipe.requiredStation != CraftingStation.ProcessingStation) return false;

            currentRecipe = recipe;
            isProcessing = true;
            processProgress = 0f;
            
            SaveState();
            return true;
        }

        public StringBuilder GetStatusBuffer()
        {
            if (!isPowered)
            {
                statusBuffer.Clear();
                statusBuffer.Append("NO POWER");
                return statusBuffer;
            }

            if (isProcessing && currentRecipe != null)
            {
                statusBuffer.Clear();
                statusBuffer.Append("Processing: ");
                statusBuffer.Append(currentRecipe.displayName);
                statusBuffer.Append(" (");
                statusBuffer.Append(Mathf.FloorToInt(processProgress * 100));
                statusBuffer.Append("%)");
                return statusBuffer;
            }

            statusBuffer.Clear();
            statusBuffer.Append("Idle");
            return statusBuffer;
        }

        private void TryRegisterOnGrid()
        {
            // Grid registration is now managed DOD-wide by PlacementManager/ChunkManager
        }
    }
}
