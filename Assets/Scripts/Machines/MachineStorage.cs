using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// MachineStorage — Player-interactable storage container with slotted inventory.
    /// </summary>
    public class MachineStorage : MonoBehaviour, IGridInteractable
    {
        [Header("Settings")]
        [SerializeField] private int slotCount = 9;
        [SerializeField] private int maxStackSize = 64;

        [System.Serializable]
        public class StorageSlot
        {
            public InventoryItem item;
            public int quantity;
            public bool IsEmpty => item == null || quantity <= 0;
        }

        [SerializeField] private List<StorageSlot> slots;

        public event System.Action OnStorageChanged;

        void Awake()
        {
            slots = new List<StorageSlot>(slotCount);
            for (int i = 0; i < slotCount; i++)
                slots.Add(new StorageSlot());
        }

        public List<StorageSlot> GetSlots() => slots;

        public bool TryAddItem(InventoryItem item, int amount)
        {
            if (item == null || amount <= 0) return false;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                // Stack onto existing
                if (!slot.IsEmpty && slot.item == item && slot.quantity < maxStackSize)
                {
                    slot.quantity += Mathf.Min(maxStackSize - slot.quantity, amount);
                    OnStorageChanged?.Invoke();
                    return true;
                }

                // Place into empty
                if (slot.IsEmpty)
                {
                    slot.item = item;
                    slot.quantity = Mathf.Min(amount, maxStackSize);
                    OnStorageChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveItem(int slotIndex, int amount, out InventoryItem item)
        {
            item = null;
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;

            var slot = slots[slotIndex];
            if (slot.IsEmpty) return false;

            slot.quantity -= Mathf.Min(amount, slot.quantity);
            item = slot.item;

            if (slot.quantity <= 0)
            {
                slot.item = null;
                slot.quantity = 0;
            }

            OnStorageChanged?.Invoke();
            return true;
        }

        public void SwapSlots(int a, int b)
        {
            if (a < 0 || a >= slots.Count || b < 0 || b >= slots.Count || a == b) return;

            (slots[a].item, slots[b].item) = (slots[b].item, slots[a].item);
            (slots[a].quantity, slots[b].quantity) = (slots[b].quantity, slots[a].quantity);
            OnStorageChanged?.Invoke();
        }

        public void Interact(InventoryItem heldItem)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OpenStorageUI(this);
        }
    }
}
