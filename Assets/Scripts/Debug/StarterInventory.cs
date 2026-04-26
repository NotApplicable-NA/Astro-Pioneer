using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Data;

/// <summary>
/// Debug helper: gives the player starting items in inventory slots 0-2.
/// Attach to any persistent GameObject and assign the item ScriptableObjects.
/// </summary>
public class StarterInventory : MonoBehaviour
{
    [Header("Starting Items (order = hotbar slot)")]
    [SerializeField] private InventoryItem slot0Item;
    [SerializeField] private int slot0Amount = 10;
    
    [SerializeField] private InventoryItem slot1Item;
    [SerializeField] private int slot1Amount = 1;
    
    [SerializeField] private InventoryItem slot2Item;
    [SerializeField] private int slot2Amount = 10;

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        if (slot0Item != null)
            InventoryManager.Instance.AddItem(slot0Item, slot0Amount);
        if (slot1Item != null)
            InventoryManager.Instance.AddItem(slot1Item, slot1Amount);
        if (slot2Item != null)
            InventoryManager.Instance.AddItem(slot2Item, slot2Amount);
    }
}
