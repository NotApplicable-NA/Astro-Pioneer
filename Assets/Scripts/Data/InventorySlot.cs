using System;

namespace AstroPioneer.Data
{
    [Serializable]
    public class InventorySlot
    {
        public InventoryItem item;
        public int quantity;

        public InventorySlot(InventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }

        public void AddQuantity(int amount)
        {
            quantity += amount;
        }

        public void RemoveQuantity(int amount)
        {
            quantity -= amount;
            if (quantity < 0) quantity = 0;
        }
        
        public bool IsEmpty => item == null || quantity <= 0;
        
        public void Clear()
        {
            item = null;
            quantity = 0;
        }
    }
}
