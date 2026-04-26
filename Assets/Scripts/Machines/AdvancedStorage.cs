using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Player;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// A multi-page/filtered storage bin.
    /// Inherits basic storage functionality from UI routing, but intercepts drops.
    /// </summary>
    public class AdvancedStorage : MonoBehaviour, AstroPioneer.Interfaces.IGridInteractable
    {
        [Header("Storage Settings")]
        [SerializeField] private int maxCapacity = 64;
        [SerializeField] private ItemType allowedFilter = ItemType.Resource; // Filter type
        
        public int Capacity => maxCapacity;

        public void Interact(InventoryItem heldItem)
        {
            // Connect to StorageUI but passes its specific filter rules
            
            // Note: Actual UI routing requires hooking up to UIManager and StorageUI to support filters.
            if (heldItem != null && heldItem.type == allowedFilter)
            {
                InventoryManager.Instance.RemoveItem(heldItem, 1);
            }
            else if (heldItem != null)
            {
            }
        }
    }
}
