using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Player;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// ShippingBin — Converts items into Trust Points when clicked with hotbar item.
    /// </summary>
    public class ShippingBin : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float sellMultiplier = 1.0f;

        void Start()
        {
            // Grid occupancy handled by DOD PlacementManager
        }

        void OnEnable() => MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
        void OnDisable() => MouseInteractionSystem.OnGridCellClicked -= HandleGridClick;

        public void SellItem(InventoryItem item, int quantity)
        {
            if (item == null || quantity <= 0 || item.sellPrice <= 0) return;

            if (InventoryManager.Instance == null || !InventoryManager.Instance.RemoveItem(item, quantity))
                return;

            int totalTrust = Mathf.FloorToInt(item.sellPrice * quantity * sellMultiplier);

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddTrust(totalTrust);
            }
            else
            {
                // Rollback: re-add items if CurrencyManager is missing
                InventoryManager.Instance.AddItem(item, quantity);
            }
        }

        private void HandleGridClick(Vector2Int gridPos)
        {
            if (GridManager.Instance == null) return;
            if (gridPos != GridManager.Instance.WorldToGridPosition(transform.position)) return;

            if (PlayerToolState.Instance == null || InventoryManager.Instance == null) return;

            int activeIndex = PlayerToolState.Instance.GetSelectedHotbarIndex();
            if (activeIndex < 0) return;

            var slot = InventoryManager.Instance.GetSlotAt(activeIndex);
            if (slot == null || slot.IsEmpty || slot.item == null || slot.item.sellPrice <= 0) return;

            InventoryManager.Instance.RemoveItemAt(activeIndex, 1);
            int trust = Mathf.FloorToInt(slot.item.sellPrice * sellMultiplier);
            CurrencyManager.Instance?.AddTrust(trust);
        }
    }
}
