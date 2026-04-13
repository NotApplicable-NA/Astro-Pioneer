using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems;

namespace AstroPioneer.UI
{
    public class TradingUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject tradingPanel;

        [Header("Tabs")]
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button sellTabButton;
        [SerializeField] private GameObject buyPanel;
        [SerializeField] private GameObject sellPanel;

        [Header("Buy Panel")]
        [SerializeField] private Transform buyListContainer;
        [SerializeField] private GameObject buyItemRowPrefab;

        [Header("Sell Panel")]
        [SerializeField] private Transform sellListContainer;
        [SerializeField] private GameObject sellItemRowPrefab;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI trustText;
        [SerializeField] private TextMeshProUGUI statusText;

        private TradingPost tradingPost;
        private bool isOpen = false;
        private bool isBuyTab = true;

        void Start()
        {
            if (tradingPanel != null) tradingPanel.SetActive(false);

            if (buyTabButton != null)
                buyTabButton.onClick.AddListener(() => SwitchTab(true));
            if (sellTabButton != null)
                sellTabButton.onClick.AddListener(() => SwitchTab(false));
        }

        // ── Panel Control ────────────────────────────

        public void Open(TradingPost post)
        {
            tradingPost = post;
            isOpen = true;
            if (tradingPanel != null) tradingPanel.SetActive(true);
            SwitchTab(true);
            UpdateBalance();
        }

        public void Close()
        {
            isOpen = false;
            if (tradingPanel != null) tradingPanel.SetActive(false);
            tradingPost = null;
        }

        public void Toggle(TradingPost post)
        {
            if (isOpen) Close();
            else Open(post);
        }

        // ── Tabs ─────────────────────────────────────

        private void SwitchTab(bool toBuy)
        {
            isBuyTab = toBuy;
            if (buyPanel != null) buyPanel.SetActive(toBuy);
            if (sellPanel != null) sellPanel.SetActive(!toBuy);

            if (toBuy) PopulateBuyList();
            else PopulateSellList();
        }

        // ── Buy List ─────────────────────────────────

        private void PopulateBuyList()
        {
            if (buyListContainer != null)
            {
                foreach (Transform child in buyListContainer)
                    Destroy(child.gameObject);
            }

            if (tradingPost == null) return;

            foreach (var item in tradingPost.Catalog)
            {
                if (item == null || item.buyPrice <= 0) continue;

                int price = tradingPost.GetBuyPrice(item);
                CreateTradeRow(buyListContainer, buyItemRowPrefab, item, price, true);
            }
        }

        // ── Sell List ────────────────────────────────

        private void PopulateSellList()
        {
            if (sellListContainer != null)
            {
                foreach (Transform child in sellListContainer)
                    Destroy(child.gameObject);
            }

            if (tradingPost == null || InventoryManager.Instance == null) return;

            // Show all inventory items that have sell value
            HashSet<InventoryItem> shown = new HashSet<InventoryItem>();
            var slots = InventoryManager.Instance.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.IsEmpty || slot.item == null) continue;
                if (shown.Contains(slot.item)) continue;
                if (slot.item.sellPrice <= 0) continue;

                shown.Add(slot.item);
                int price = tradingPost.GetSellPrice(slot.item);
                int qty = InventoryManager.Instance.GetItemCount(slot.item);
                CreateTradeRow(sellListContainer, sellItemRowPrefab, slot.item, price, false, qty);
            }
        }

        // ── Row Creation ─────────────────────────────

        private void CreateTradeRow(Transform container, GameObject prefab, InventoryItem item, int price, bool isBuy, int ownedQty = 0)
        {
            if (prefab == null || container == null) return;

            var row = Instantiate(prefab, container);

            // Icon
            var icon = row.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && item.icon != null) icon.sprite = item.icon;

            // Name
            var nameText = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = item.displayName;

            // Price
            var priceText = row.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null) priceText.text = $"{price}cr";

            // Quantity (sell mode)
            var qtyText = row.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
            if (qtyText != null)
                qtyText.text = isBuy ? "" : $"x{ownedQty}";

            // Action button
            var actionBtn = row.transform.Find("ActionButton")?.GetComponent<Button>();
            var actionBtnText = actionBtn?.GetComponentInChildren<TextMeshProUGUI>();
            if (actionBtnText != null)
                actionBtnText.text = isBuy ? "Buy" : "Sell";

            if (actionBtn != null)
            {
                var capturedItem = item;
                if (isBuy)
                    actionBtn.onClick.AddListener(() => OnBuyClicked(capturedItem));
                else
                    actionBtn.onClick.AddListener(() => OnSellClicked(capturedItem));
            }
        }

        // ── Actions ──────────────────────────────────

        private void OnBuyClicked(InventoryItem item)
        {
            if (tradingPost == null) return;

            if (tradingPost.BuyItem(item, 1))
            {
                SetStatus($"Bought 1x {item.displayName}!");
                PopulateBuyList();
                UpdateBalance();
            }
            else
            {
                SetStatus($"Can't buy {item.displayName}.");
            }
        }

        private void OnSellClicked(InventoryItem item)
        {
            if (tradingPost == null) return;

            if (tradingPost.SellItem(item, 1))
            {
                SetStatus($"Sold 1x {item.displayName}!");
                PopulateSellList();
                UpdateBalance();
            }
            else
            {
                SetStatus($"Can't sell {item.displayName}.");
            }
        }

        // ── Display ──────────────────────────────────

        private void UpdateBalance()
        {
            if (CurrencyManager.Instance != null)
            {
                if (balanceText != null) balanceText.text = $"{CurrencyManager.Instance.CurrentCredits}cr";
                if (trustText != null) trustText.text = $"Trust: {CurrencyManager.Instance.CurrentTrust}";
            }
        }

        private void SetStatus(string msg)
        {
            if (statusText != null) statusText.text = msg;
        }
    }
}
