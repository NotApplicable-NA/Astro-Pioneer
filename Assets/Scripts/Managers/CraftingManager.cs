using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Data;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// CraftingManager — Queue-based crafting system.
    /// Pre-checks inventory space atomically before consuming ingredients.
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        [Header("Recipe Registry")]
        [SerializeField] private List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

        [Header("Queue")]
        [SerializeField] private int maxQueueSize = 3;

        private readonly Queue<CraftingRecipe> craftingQueue = new Queue<CraftingRecipe>();
        private CraftingRecipe currentlyCrafting;
        private float craftProgress;
        private Coroutine craftCoroutine;

        // Events
        public event Action<CraftingRecipe> OnCraftStarted;
        public event Action<CraftingRecipe> OnCraftCompleted;
        public event Action<CraftingRecipe> OnCraftFailed;
        public event Action OnQueueChanged;

        // Properties
        public IReadOnlyList<CraftingRecipe> AllRecipes => allRecipes;
        public CraftingRecipe CurrentlyCrafting => currentlyCrafting;
        public float CraftProgress => craftProgress;
        public int QueueCount => craftingQueue.Count;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ───

        public List<CraftingRecipe> GetRecipesForStation(CraftingStation station)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in allRecipes)
                if (recipe.requiredStation == station) result.Add(recipe);
            return result;
        }

        public List<CraftingRecipe> GetUnlockedRecipes()
        {
            int trust = CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentTrust : 0;
            var result = new List<CraftingRecipe>();
            foreach (var recipe in allRecipes)
                if (recipe.trustRequired <= trust) result.Add(recipe);
            return result;
        }

        public bool CanCraft(CraftingRecipe recipe)
        {
            if (recipe == null || InventoryManager.Instance == null) return false;
            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.HasTrust(recipe.trustRequired))
                return false;

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item == null) continue;
                if (InventoryManager.Instance.GetItemCount(ingredient.item) < ingredient.quantity)
                    return false;
            }
            return true;
        }

        public bool StartCraft(CraftingRecipe recipe)
        {
            if (recipe == null || InventoryManager.Instance == null)
            {
                OnCraftFailed?.Invoke(recipe);
                return false;
            }

            int totalQueued = craftingQueue.Count + (currentlyCrafting != null ? 1 : 0);
            if (totalQueued >= maxQueueSize)
            {
                OnCraftFailed?.Invoke(recipe);
                return false;
            }

            // Atomic dry-run: verify inventory space for result
            if (!InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultQuantity))
            {
                OnCraftFailed?.Invoke(recipe);
                return false;
            }
            InventoryManager.Instance.RemoveItem(recipe.resultItem, recipe.resultQuantity);

            if (!CanCraft(recipe))
            {
                OnCraftFailed?.Invoke(recipe);
                return false;
            }

            // Consume ingredients
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item != null)
                    InventoryManager.Instance.RemoveItem(ingredient.item, ingredient.quantity);
            }

            craftingQueue.Enqueue(recipe);
            OnQueueChanged?.Invoke();

            if (craftCoroutine == null)
                craftCoroutine = StartCoroutine(ProcessQueue());

            return true;
        }

        // ─── Queue Processing ───

        private IEnumerator ProcessQueue()
        {
            while (craftingQueue.Count > 0)
            {
                currentlyCrafting = craftingQueue.Dequeue();
                craftProgress = 0f;
                OnCraftStarted?.Invoke(currentlyCrafting);
                OnQueueChanged?.Invoke();

                float elapsed = 0f;
                while (elapsed < currentlyCrafting.craftTime)
                {
                    elapsed += Time.deltaTime;
                    craftProgress = Mathf.Clamp01(elapsed / currentlyCrafting.craftTime);
                    yield return null;
                }

                craftProgress = 1f;

                // Deliver result (retry if inventory full)
                if (InventoryManager.Instance != null && currentlyCrafting.resultItem != null)
                {
                    if (!InventoryManager.Instance.AddItem(currentlyCrafting.resultItem, currentlyCrafting.resultQuantity))
                        yield return new WaitUntil(() =>
                            InventoryManager.Instance.AddItem(currentlyCrafting.resultItem, currentlyCrafting.resultQuantity));
                }

                OnCraftCompleted?.Invoke(currentlyCrafting);
                currentlyCrafting = null;
                craftProgress = 0f;
                OnQueueChanged?.Invoke();
            }

            craftCoroutine = null;
        }
    }
}
