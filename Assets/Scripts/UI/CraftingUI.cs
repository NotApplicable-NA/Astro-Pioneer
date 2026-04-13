using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.UI
{
    public class CraftingUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject craftingPanel;
        
        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeButtonPrefab;

        [Header("Recipe Detail")]
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI recipeDescText;
        [SerializeField] private Transform ingredientContainer;
        [SerializeField] private GameObject ingredientRowPrefab;
        [SerializeField] private Image resultIcon;
        [SerializeField] private TextMeshProUGUI resultQuantityText;

        [Header("Crafting")]
        [SerializeField] private Button craftButton;
        [SerializeField] private TextMeshProUGUI craftButtonText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI queueText;

        [Header("Filter")]
        [SerializeField] private CraftingStation stationFilter = CraftingStation.Hand;

        private CraftingRecipe selectedRecipe;
        private bool isOpen = false;

        private bool eventsSubscribed = false;

        void Start()
        {
            if (craftingPanel != null) craftingPanel.SetActive(false);

            if (craftButton != null)
                craftButton.onClick.AddListener(OnCraftClicked);

            TrySubscribeEvents();
        }

        void OnDestroy()
        {
            if (CraftingManager.Instance != null && eventsSubscribed)
            {
                CraftingManager.Instance.OnCraftStarted -= OnCraftStarted;
                CraftingManager.Instance.OnCraftCompleted -= OnCraftCompleted;
                CraftingManager.Instance.OnQueueChanged -= UpdateQueueDisplay;
            }
        }

        void Update()
        {
            // FIX M2: Lazy subscription if CraftingManager wasn't ready at Start
            if (!eventsSubscribed) TrySubscribeEvents();

            if (!isOpen) return;

            // Update progress bar
            if (progressBar != null && CraftingManager.Instance != null)
            {
                progressBar.value = CraftingManager.Instance.CraftProgress;
            }

            // Update craftability
            if (selectedRecipe != null && craftButton != null)
            {
                craftButton.interactable = CraftingManager.Instance != null && CraftingManager.Instance.CanCraft(selectedRecipe);
            }
        }

        private void TrySubscribeEvents()
        {
            if (eventsSubscribed || CraftingManager.Instance == null) return;
            CraftingManager.Instance.OnCraftStarted += OnCraftStarted;
            CraftingManager.Instance.OnCraftCompleted += OnCraftCompleted;
            CraftingManager.Instance.OnQueueChanged += UpdateQueueDisplay;
            eventsSubscribed = true;
        }

        // ── Panel Control ────────────────────────────

        public void Open(CraftingStation station = CraftingStation.Hand)
        {
            stationFilter = station;
            isOpen = true;
            if (craftingPanel != null) craftingPanel.SetActive(true);
            PopulateRecipeList();
            ClearDetail();
        }

        public void Close()
        {
            isOpen = false;
            if (craftingPanel != null) craftingPanel.SetActive(false);
        }

        public void Toggle(CraftingStation station = CraftingStation.Hand)
        {
            if (isOpen) Close();
            else Open(station);
        }

        // ── Recipe List ──────────────────────────────

        private void PopulateRecipeList()
        {
            // Clear existing
            if (recipeListContainer != null)
            {
                foreach (Transform child in recipeListContainer)
                    Destroy(child.gameObject);
            }

            if (CraftingManager.Instance == null) return;

            var recipes = CraftingManager.Instance.GetRecipesForStation(stationFilter);
            foreach (var recipe in recipes)
            {
                if (recipeButtonPrefab == null || recipeListContainer == null) continue;

                var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = recipe.displayName;

                var icon = btn.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && recipe.resultItem != null && recipe.resultItem.icon != null)
                    icon.sprite = recipe.resultItem.icon;

                var button = btn.GetComponent<Button>();
                if (button != null)
                {
                    var r = recipe; // Capture for closure
                    button.onClick.AddListener(() => SelectRecipe(r));
                }

                // Visual: dim if can't craft
                bool canCraft = CraftingManager.Instance.CanCraft(recipe);
                var canvasGroup = btn.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                    canvasGroup.alpha = canCraft ? 1f : 0.5f;
            }
        }

        // ── Recipe Detail ────────────────────────────

        private void SelectRecipe(CraftingRecipe recipe)
        {
            selectedRecipe = recipe;

            if (recipeNameText != null) recipeNameText.text = recipe.displayName;
            if (recipeDescText != null) recipeDescText.text = recipe.description;

            // Result
            if (resultIcon != null && recipe.resultItem != null)
                resultIcon.sprite = recipe.resultItem.icon;
            if (resultQuantityText != null)
                resultQuantityText.text = $"x{recipe.resultQuantity}";

            // Ingredients
            if (ingredientContainer != null)
            {
                foreach (Transform child in ingredientContainer)
                    Destroy(child.gameObject);

                foreach (var ing in recipe.ingredients)
                {
                    if (ingredientRowPrefab == null) continue;
                    var row = Instantiate(ingredientRowPrefab, ingredientContainer);
                    
                    var nameText = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                    if (nameText != null) nameText.text = ing.item != null ? ing.item.displayName : "???";

                    var qtyText = row.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
                    int has = ing.item != null && InventoryManager.Instance != null 
                        ? InventoryManager.Instance.GetItemCount(ing.item) : 0;
                    if (qtyText != null)
                    {
                        qtyText.text = $"{has}/{ing.quantity}";
                        qtyText.color = has >= ing.quantity ? Color.green : Color.red;
                    }

                    var ingIcon = row.transform.Find("Icon")?.GetComponent<Image>();
                    if (ingIcon != null && ing.item != null && ing.item.icon != null)
                        ingIcon.sprite = ing.item.icon;
                }
            }

            // Craft button
            if (craftButton != null)
                craftButton.interactable = CraftingManager.Instance != null && CraftingManager.Instance.CanCraft(recipe);
            if (craftButtonText != null)
                craftButtonText.text = $"Craft ({recipe.craftTime}s)";
        }

        private void ClearDetail()
        {
            selectedRecipe = null;
            if (recipeNameText != null) recipeNameText.text = "Select a recipe";
            if (recipeDescText != null) recipeDescText.text = "";
            if (resultIcon != null) resultIcon.sprite = null;
            if (resultQuantityText != null) resultQuantityText.text = "";
            if (ingredientContainer != null)
            {
                foreach (Transform child in ingredientContainer)
                    Destroy(child.gameObject);
            }
            if (craftButton != null) craftButton.interactable = false;
            if (progressBar != null) progressBar.value = 0;
        }

        // ── Crafting Actions ─────────────────────────

        private void OnCraftClicked()
        {
            if (selectedRecipe == null || CraftingManager.Instance == null) return;
            CraftingManager.Instance.StartCraft(selectedRecipe);
            PopulateRecipeList(); // Refresh availability
            SelectRecipe(selectedRecipe); // Refresh detail
        }

        private void OnCraftStarted(CraftingRecipe recipe)
        {
        }

        private void OnCraftCompleted(CraftingRecipe recipe)
        {
            if (isOpen)
            {
                PopulateRecipeList();
                if (selectedRecipe != null) SelectRecipe(selectedRecipe);
            }
        }

        private void UpdateQueueDisplay()
        {
            if (queueText != null && CraftingManager.Instance != null)
            {
                int count = CraftingManager.Instance.QueueCount;
                string current = CraftingManager.Instance.CurrentlyCrafting != null 
                    ? CraftingManager.Instance.CurrentlyCrafting.displayName : "—";
                queueText.text = $"Queue: {count} | Now: {current}";
            }
        }
    }
}
