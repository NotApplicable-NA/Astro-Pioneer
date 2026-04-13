using UnityEngine;
using System;
using System.Collections.Generic;
using AstroPioneer.Data;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// InventoryManager — Core item storage with atomic add/remove transactions
    /// and stack-merge support. Supports infinite/sandbox mode.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int slotCount = 64;
        [SerializeField] private List<InventorySlot> slots;

        [Header("Debug / Sandbox")]
        [Tooltip("Items are not consumed when used (e.g. planting seeds).")]
        public bool infiniteMode = false;

        [Header("Starter Kit")]
        public List<InventoryItem> starterItems = new List<InventoryItem>();

        public List<InventorySlot> Slots => slots;
        public event Action OnInventoryUpdated;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            slots = new List<InventorySlot>(slotCount);
            for (int i = 0; i < slotCount; i++)
                slots.Add(new InventorySlot(null, 0));
        }

        void Start()
        {
            foreach (var item in starterItems)
            {
                if (item == null) continue;
                int amount = (item.type == ItemType.Crafted || item.type == ItemType.Tool) ? 1 : item.maxStackSize;
                AddItem(item, amount);
            }
        }

        // ─── Core API ───

        /// <summary>
        /// Atomically add items. Verifies sufficient space before modifying state.
        /// Returns false if not all items can fit (no partial adds).
        /// </summary>
        public bool AddItem(InventoryItem item, int amount)
        {
            if (item == null || amount <= 0) return false;

            // Dry run: verify total capacity
            int remaining = amount;

            if (item.isStackable)
            {
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty && slot.item == item && slot.quantity < item.maxStackSize)
                    {
                        remaining -= Mathf.Min(item.maxStackSize - slot.quantity, remaining);
                        if (remaining <= 0) break;
                    }
                }
            }

            if (remaining > 0)
            {
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty)
                    {
                        remaining -= Mathf.Min(remaining, item.maxStackSize);
                        if (remaining <= 0) break;
                    }
                }
            }

            if (remaining > 0) return false; // Not enough space

            // Commit: actually add items
            int toAdd = amount;

            if (item.isStackable)
            {
                foreach (var slot in slots)
                {
                    if (toAdd <= 0) break;
                    if (!slot.IsEmpty && slot.item == item && slot.quantity < item.maxStackSize)
                    {
                        int add = Mathf.Min(item.maxStackSize - slot.quantity, toAdd);
                        slot.AddQuantity(add);
                        toAdd -= add;
                    }
                }
            }

            while (toAdd > 0)
            {
                InventorySlot empty = GetFirstEmptySlot();
                if (empty == null) break; // Should never happen after dry run
                int add = Mathf.Min(toAdd, item.maxStackSize);
                empty.item = item;
                empty.quantity = add;
                toAdd -= add;
            }

            OnInventoryUpdated?.Invoke();
            return true;
        }

        public bool RemoveItem(InventoryItem item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (infiniteMode) return true;
            if (!HasItem(item, amount)) return false;

            int toRemove = amount;
            foreach (var slot in slots)
            {
                if (toRemove <= 0) break;
                if (slot.IsEmpty || slot.item != item) continue;

                int take = Mathf.Min(slot.quantity, toRemove);
                slot.RemoveQuantity(take);
                toRemove -= take;
                if (slot.quantity <= 0) slot.Clear();
            }

            OnInventoryUpdated?.Invoke();
            return true;
        }

        public bool RemoveItemAt(int index, int amount)
        {
            if (index < 0 || index >= slots.Count) return false;
            if (infiniteMode) return true;

            var slot = slots[index];
            if (slot.IsEmpty || slot.quantity < amount) return false;

            slot.RemoveQuantity(amount);
            if (slot.quantity <= 0) slot.Clear();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        // ─── Queries ───

        public bool HasItem(InventoryItem item, int amount) => GetItemCount(item) >= amount;

        public int GetItemCount(InventoryItem item)
        {
            int count = 0;
            foreach (var slot in slots)
                if (!slot.IsEmpty && slot.item == item)
                    count += slot.quantity;
            return count;
        }

        public InventorySlot GetSlotAt(int index) => index >= 0 && index < slots.Count ? slots[index] : null;
        public List<InventorySlot> GetSlots() => slots;

        // ─── Slot Operations ───

        public void SetSlot(int index, InventoryItem item, int quantity)
        {
            if (index < 0 || index >= slots.Count) return;
            slots[index].item = item;
            slots[index].quantity = quantity;
            OnInventoryUpdated?.Invoke();
        }

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count || indexA == indexB) return;

            var a = slots[indexA];
            var b = slots[indexB];

            // Merge if same stackable item
            if (!a.IsEmpty && !b.IsEmpty && a.item == b.item && a.item.isStackable)
            {
                int space = b.item.maxStackSize - b.quantity;
                int transfer = Mathf.Min(a.quantity, space);
                if (transfer > 0)
                {
                    b.AddQuantity(transfer);
                    a.RemoveQuantity(transfer);
                    if (a.quantity <= 0) a.Clear();
                }
            }
            else
            {
                (a.item, b.item) = (b.item, a.item);
                (a.quantity, b.quantity) = (b.quantity, a.quantity);
            }

            OnInventoryUpdated?.Invoke();
        }

        public void ClearInventory()
        {
            foreach (var slot in slots) slot.Clear();
            OnInventoryUpdated?.Invoke();
        }

        // ─── Helpers ───

        private InventorySlot GetFirstEmptySlot()
        {
            foreach (var slot in slots)
                if (slot.IsEmpty) return slot;
            return null;
        }
    }
}
