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
    public class MachineStorage : MonoBehaviour, IGridInteractable, ISavableMachine
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
            public void Clear() { item = null; quantity = 0; }
        }

        [SerializeField] private List<StorageSlot> slots;

        public event System.Action OnStorageChanged;

        void Awake()
        {
            if (slots == null || slots.Count == 0)
            {
                slots = new List<StorageSlot>(slotCount);
                for (int i = 0; i < slotCount; i++)
                    slots.Add(new StorageSlot());
            }
        }

        void OnEnable()
        {
            OnStorageChanged += TriggerSave;
        }

        void OnDisable()
        {
            OnStorageChanged -= TriggerSave;
        }

        public List<StorageSlot> GetSlots() => slots;

        /// <summary>Check if this storage can accept the given item without actually adding it. Used by BotStation for reservation.</summary>
        public bool CanAcceptItem(InventoryItem item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].item == item && slots[i].quantity + amount <= maxStackSize) return true;
                if (slots[i].IsEmpty) return true;
            }
            return false;
        }

        public bool TryAddItem(InventoryItem item, int amount)
        {
            if (item == null || amount <= 0) return false;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                // Stack onto existing
                if (!slot.IsEmpty && slot.item == item && slot.quantity < maxStackSize)
                {
                    int toAdd = Mathf.Min(maxStackSize - slot.quantity, amount);
                    slot.quantity += toAdd;
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

            int toRemove = Mathf.Min(amount, slot.quantity);
            slot.quantity -= toRemove;
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
            // V24.10: Sync from data layer before opening UI, in case Bots modified it
            if (GridManager.Instance != null)
            {
                var tag = GetComponent<AstroPioneer.Systems.MachineIDTag>();
                Vector2Int pos = tag != null ? tag.originGridPos : new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
                byte[] buffer = GridManager.Instance.GetComplexState(pos);
                if (buffer != null) LoadState(new System.ReadOnlySpan<byte>(buffer));
            }

            if (UIManager.Instance != null)
                UIManager.Instance.OpenStorageUI(this);
        }

        // ─── ISavableMachine Implementation ───

        public void SaveState(System.Span<byte> buffer)
        {
            // Format: [ushort slotCount] + Per Slot: [ushort itemID, int quantity]
            System.BitConverter.TryWriteBytes(buffer.Slice(0, 2), (ushort)slots.Count);
            int offset = 2;

            for (int i = 0; i < slots.Count; i++)
            {
                ushort id = ItemRegistry.Instance != null ? ItemRegistry.Instance.GetID(slots[i].item) : (ushort)0;
                System.BitConverter.TryWriteBytes(buffer.Slice(offset, 2), id);
                System.BitConverter.TryWriteBytes(buffer.Slice(offset + 2, 4), slots[i].quantity);
                offset += 6;
            }
        }

        public void LoadState(System.ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < 2) return;

            ushort savedCount = System.BitConverter.ToUInt16(buffer.Slice(0, 2));
            int offset = 2;

            // Resize list if needed (though usually 9)
            if (slots == null) slots = new List<StorageSlot>();
            while (slots.Count < savedCount) slots.Add(new StorageSlot());

            for (int i = 0; i < savedCount; i++)
            {
                if (offset + 6 > buffer.Length) break;

                ushort id = System.BitConverter.ToUInt16(buffer.Slice(offset, 2));
                int qty = System.BitConverter.ToInt32(buffer.Slice(offset + 2, 4));
                
                if (i < slots.Count)
                {
                    slots[i].item = (ItemRegistry.Instance != null && id != 0) ? ItemRegistry.Instance.Get(id) : null;
                    slots[i].quantity = qty;
                }
                offset += 6;
            }
        }

        private void TriggerSave()
        {
            // Find grid position via MachineIDTag (attached to visuals)
            var tag = GetComponent<AstroPioneer.Systems.MachineIDTag>();
            Vector2Int pos;
            if (tag != null)
            {
                pos = tag.originGridPos;
            }
            else
            {
                // Fallback to transform position if tag is missing
                pos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
            }

            if (GridManager.Instance != null)
            {
                byte[] buffer = GridManager.Instance.GetOrAllocateComplexState(pos);
                if (buffer != null)
                {
                    SaveState(new System.Span<byte>(buffer));
                    // GridManager/Chunk system handles the Dirty flag inside GetOrAllocateComplexState
                }
            }
        }
    }
}
