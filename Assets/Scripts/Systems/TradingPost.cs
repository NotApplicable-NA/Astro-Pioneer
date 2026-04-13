using UnityEngine;
using System;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems
{
    public class TradingPost : MonoBehaviour
    {
        [Header("Trading Settings")]
        [SerializeField] private List<InventoryItem> itemCatalog = new List<InventoryItem>();
        [SerializeField] private float priceFluctuationRange = 0.2f; // ±20%

        [Header("State")]
        [SerializeField] private int currentDay = 0;

        // Cached price multipliers (refreshed each day)
        private Dictionary<InventoryItem, float> priceMultipliers = new Dictionary<InventoryItem, float>();

        // Transaction log
        private List<TransactionRecord> transactionLog = new List<TransactionRecord>();

        // Events
        public event Action OnPricesUpdated;
        public event Action<TransactionRecord> OnTransaction;

        // Properties
        public IReadOnlyList<InventoryItem> Catalog => itemCatalog;
        public IReadOnlyList<TransactionRecord> TransactionLog => transactionLog;

        void Start()
        {
            RefreshPrices();
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged += OnNewDay;
        }

        void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged -= OnNewDay;
        }

        private void OnEnable()
        {
            AstroPioneer.Systems.MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
        }

        private void OnDisable()
        {
            AstroPioneer.Systems.MouseInteractionSystem.OnGridCellClicked -= HandleGridClick;
        }

        private void HandleGridClick(Vector2Int gridPos)
        {
            if (GridManager.Instance == null) return;
            Vector2Int myPos = GridManager.Instance.WorldToGridPosition(transform.position);
            
            if (gridPos == myPos)
            {
                var ui = FindObjectOfType<AstroPioneer.UI.TradingUI>(true);
                if (ui != null)
                {
                    ui.Toggle(this);
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Call this when a new day starts (from TimeManager).
        /// </summary>
        public void OnNewDay(int day)
        {
            currentDay = day;
            RefreshPrices();
        }

        /// <summary>
        /// Randomize price multipliers for the day.
        /// </summary>
        public void RefreshPrices()
        {
            priceMultipliers.Clear();
            foreach (var item in itemCatalog)
            {
                if (item == null) continue;
                float multiplier = UnityEngine.Random.Range(1f - priceFluctuationRange, 1f + priceFluctuationRange);
                priceMultipliers[item] = multiplier;
            }
            OnPricesUpdated?.Invoke();
        }

        /// <summary>
        /// Get the current buy price for an item (what player pays).
        /// </summary>
        public int GetBuyPrice(InventoryItem item)
        {
            if (item == null || item.buyPrice <= 0) return -1; // Not buyable
            float mult = priceMultipliers.ContainsKey(item) ? priceMultipliers[item] : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(item.buyPrice * mult));
        }

        /// <summary>
        /// Get the current sell price for an item (what player receives).
        /// </summary>
        public int GetSellPrice(InventoryItem item)
        {
            if (item == null || item.sellPrice <= 0) return 0;
            float mult = priceMultipliers.ContainsKey(item) ? priceMultipliers[item] : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(item.sellPrice * mult));
        }

        /// <summary>
        /// Player buys an item from the trading post.
        /// </summary>
        public bool BuyItem(InventoryItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int totalCost = GetBuyPrice(item) * quantity;
            if (totalCost < 0)
            {
                return false;
            }

            // Check credits
            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpendCredits(totalCost))
            {
                return false;
            }

            // Add to inventory
            if (InventoryManager.Instance == null || !InventoryManager.Instance.AddItem(item, quantity))
            {
                // Rollback credits
                CurrencyManager.Instance.AddCredits(totalCost);
                return false;
            }

            // Log
            var record = new TransactionRecord
            {
                item = item,
                quantity = quantity,
                totalPrice = totalCost,
                isBuy = true,
                day = currentDay
            };
            transactionLog.Add(record);
            OnTransaction?.Invoke(record);
            return true;
        }

        /// <summary>
        /// Player sells an item to the trading post.
        /// </summary>
        public bool SellItem(InventoryItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int totalValue = GetSellPrice(item) * quantity;
            if (totalValue <= 0)
            {
                return false;
            }

            // Remove from inventory first
            if (InventoryManager.Instance == null || !InventoryManager.Instance.RemoveItem(item, quantity))
            {
                return false;
            }

            // Add credits
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCredits(totalValue);
            }

            // Log
            var record = new TransactionRecord
            {
                item = item,
                quantity = quantity,
                totalPrice = totalValue,
                isBuy = false,
                day = currentDay
            };
            transactionLog.Add(record);
            OnTransaction?.Invoke(record);
            return true;
        }
    }

    [System.Serializable]
    public class TransactionRecord
    {
        public InventoryItem item;
        public int quantity;
        public int totalPrice;
        public bool isBuy;
        public int day;
    }
}
