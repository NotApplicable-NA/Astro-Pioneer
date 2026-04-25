using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Ship;

namespace AstroPioneer.UI
{
    /// <summary>
    /// UI for ship upgrades: tier upgrades and room purchases.
    /// </summary>
    public class ShipUpgradeUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject upgradePanel;

        [Header("Tier Section")]
        [SerializeField] private TextMeshProUGUI currentTierText;
        [SerializeField] private TextMeshProUGUI tierCostText;
        [SerializeField] private Button upgradeTierButton;

        [Header("Room Section")]
        [SerializeField] private Transform roomListContainer;
        [SerializeField] private GameObject roomRowPrefab;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI statusText;

        private bool isOpen = false;

        void Start()
        {
            if (upgradePanel != null) upgradePanel.SetActive(false);
            if (upgradeTierButton != null)
                upgradeTierButton.onClick.AddListener(OnUpgradeTierClicked);
        }

        void OnEnable()
        {
            if (ShipUpgradeManager.Instance != null)
            {
                ShipUpgradeManager.Instance.OnTierUpgraded += OnTierUpgraded;
                ShipUpgradeManager.Instance.OnRoomPurchased += OnRoomPurchased;
            }
        }

        void OnDisable()
        {
            if (ShipUpgradeManager.Instance != null)
            {
                ShipUpgradeManager.Instance.OnTierUpgraded -= OnTierUpgraded;
                ShipUpgradeManager.Instance.OnRoomPurchased -= OnRoomPurchased;
            }
        }

        void OnDestroy()
        {
        }

        // ── Panel Control ───────────────────────────

        public void Open()
        {
            isOpen = true;
            if (upgradePanel != null) upgradePanel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            isOpen = false;
            if (upgradePanel != null) upgradePanel.SetActive(false);
        }

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        // ── Refresh ─────────────────────────────────

        private void Refresh()
        {
            RefreshTierSection();
            RefreshRoomList();
            RefreshBalance();
        }

        private void RefreshTierSection()
        {
            if (ShipUpgradeManager.Instance == null) return;

            var mgr = ShipUpgradeManager.Instance;

            if (currentTierText != null)
                currentTierText.text = $"Ship Tier {mgr.CurrentTier}";

            int nextCost = mgr.GetNextTierCost();
            if (tierCostText != null)
                tierCostText.text = nextCost >= 0 ? $"Upgrade: {nextCost}cr" : "MAX TIER";

            if (upgradeTierButton != null)
                upgradeTierButton.interactable = mgr.CanUpgradeTier();
        }

        private void RefreshRoomList()
        {
            if (roomListContainer == null || ShipUpgradeManager.Instance == null) return;

            // Clear existing
            foreach (Transform child in roomListContainer)
                Destroy(child.gameObject);

            var rooms = ShipUpgradeManager.Instance.GetPurchasableRooms();
            foreach (var room in rooms)
            {
                if (roomRowPrefab == null) continue;

                var row = Instantiate(roomRowPrefab, roomListContainer);

                var nameText = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null) nameText.text = room.displayName;

                var costText = row.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
                if (costText != null) costText.text = $"{room.unlockCost}cr";

                var sizeText = row.transform.Find("Size")?.GetComponent<TextMeshProUGUI>();
                if (sizeText != null) sizeText.text = $"{room.size.x}x{room.size.y}";

                var icon = row.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && room.roomIcon != null) icon.sprite = room.roomIcon;

                var buyBtn = row.transform.Find("BuyButton")?.GetComponent<Button>();
                if (buyBtn != null)
                {
                    var capturedRoom = room;
                    buyBtn.onClick.AddListener(() => OnBuyRoomClicked(capturedRoom));
                    buyBtn.interactable = CurrencyManager.Instance != null &&
                                          CurrencyManager.Instance.CurrentCredits >= room.unlockCost;
                }
            }
        }

        private void RefreshBalance()
        {
            if (balanceText != null && CurrencyManager.Instance != null)
                balanceText.text = $"{CurrencyManager.Instance.CurrentCredits}cr";
        }

        private void SetStatus(string msg)
        {
            if (statusText != null) statusText.text = msg;
        }

        // ── Actions ─────────────────────────────────

        private void OnUpgradeTierClicked()
        {
            if (ShipUpgradeManager.Instance == null) return;

            if (ShipUpgradeManager.Instance.UpgradeTier())
            {
                SetStatus($"Upgraded to Tier {ShipUpgradeManager.Instance.CurrentTier}!");
                Refresh();
            }
            else
            {
                SetStatus("Not enough credits!");
            }
        }

        private void OnBuyRoomClicked(ShipRoom room)
        {
            // For now place at a default position — later integrate with placement preview
            // TODO: Let player choose room placement position
            if (ShipUpgradeManager.Instance == null) return;

            Vector2Int defaultOrigin = new Vector2Int(0, 0); // Placeholder
            if (ShipUpgradeManager.Instance.PurchaseRoom(room, defaultOrigin))
            {
                SetStatus($"Room purchased: {room.displayName}!");
                Refresh();
            }
            else
            {
                SetStatus($"Can't buy {room.displayName}.");
            }
        }

        // ── Event Handlers ──────────────────────────

        private void OnTierUpgraded(int newTier)
        {
            if (isOpen) Refresh();
        }

        private void OnRoomPurchased(ShipRoom room)
        {
            if (isOpen) Refresh();
        }
    }
}
