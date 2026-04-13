using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AstroPioneer.Machines;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.UI
{
    public class StorageUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;

        private MachineStorage currentStorage;
        private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

        // Drag & Drop
        private GameObject dragGhost;
        private InventorySlotUI dragSourceSlot;
        private Canvas rootCanvas;
        private InventoryUI cachedInventoryUI;

        public InventorySlotUI DragSourceSlot => dragSourceSlot;
        public MachineStorage CurrentStorage => currentStorage;

        void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    if (AstroPioneer.Managers.UIManager.Instance != null)
                        AstroPioneer.Managers.UIManager.Instance.CloseStorageUI();
                    else
                        CloseUI();
                });
            }
            
            gameObject.SetActive(false);
        }

        public void Open(MachineStorage storage)
        {
            currentStorage = storage;
            gameObject.SetActive(true);
            
            if (titleText != null) titleText.text = storage.name;

            rootCanvas = GetComponentInParent<Canvas>();
            cachedInventoryUI = FindObjectOfType<InventoryUI>();

            currentStorage.OnStorageChanged += RefreshUI;
            
            InitializeSlots();
            RefreshUI();
        }

        public void CloseUI()
        {
            if (currentStorage != null)
            {
                currentStorage.OnStorageChanged -= RefreshUI;
                currentStorage = null;
            }
            DestroyDragGhost();
            gameObject.SetActive(false);
        }

        private void InitializeSlots()
        {
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }
            uiSlots.Clear();

            var storageSlots = currentStorage.GetSlots();
            for (int i = 0; i < storageSlots.Count; i++)
            {
                int index = i;
                GameObject newSlotObj = Instantiate(slotPrefab, slotContainer);
                InventorySlotUI uiSlot = newSlotObj.GetComponent<InventorySlotUI>();
                if (uiSlot != null)
                {
                    uiSlot.Setup((clickType) => OnSlotClicked(index, clickType));
                    uiSlot.SetupDrag(index, OnStorageBeginDrag, OnStorageDrop, OnStorageEndDrag);
                    uiSlots.Add(uiSlot);
                }
            }
        }

        // --- Drag & Drop ---

        private void OnStorageBeginDrag(InventorySlotUI sourceSlot)
        {
            if (sourceSlot.CurrentItem == null) return;
            dragSourceSlot = sourceSlot;

            // Create ghost
            DestroyDragGhost();
            dragGhost = new GameObject("StorageDragGhost");
            if (rootCanvas != null) dragGhost.transform.SetParent(rootCanvas.transform, false);

            Image img = dragGhost.AddComponent<Image>();
            img.sprite = sourceSlot.CurrentItem.icon;
            img.raycastTarget = false;

            RectTransform rt = dragGhost.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48, 48);
            img.color = new Color(1f, 1f, 1f, 0.7f);

            Canvas ghostCanvas = dragGhost.AddComponent<Canvas>();
            ghostCanvas.overrideSorting = true;
            ghostCanvas.sortingOrder = 100;

            dragGhost.transform.position = Input.mousePosition;
        }

        private void OnStorageDrop(InventorySlotUI targetSlot)
        {
            // Cross-panel: Inventory → Storage (Remove-first to prevent duplication)
            if (cachedInventoryUI != null && cachedInventoryUI.DragSourceSlot != null && InventoryManager.Instance != null)
            {
                int invSlotIndex = cachedInventoryUI.DragSourceSlot.SlotIndex;
                var invSlot = InventoryManager.Instance.GetSlotAt(invSlotIndex);
                if (invSlot != null && !invSlot.IsEmpty)
                {
                    string itemName = invSlot.item.displayName;
                    InventoryItem itemRef = invSlot.item;
                    int qty = invSlot.quantity;

                    // Step 1: Remove from inventory FIRST
                    InventoryManager.Instance.RemoveItem(itemRef, qty);

                    // Step 2: Try add to storage
                    if (!currentStorage.TryAddItem(itemRef, qty))
                    {
                        // Rollback: put it back in inventory
                        InventoryManager.Instance.AddItem(itemRef, qty);
                    }
                    else
                    {
                    }
                }
                return;
            }

            // Within-storage swap
            if (dragSourceSlot != null && dragSourceSlot != targetSlot)
            {
                currentStorage.SwapSlots(dragSourceSlot.SlotIndex, targetSlot.SlotIndex);
                RefreshUI();
            }
        }

        private void OnStorageEndDrag()
        {
            dragSourceSlot = null;
            DestroyDragGhost();
        }

        void Update()
        {
            if (dragGhost != null)
            {
                dragGhost.transform.position = Input.mousePosition;
            }
        }

        private void DestroyDragGhost()
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }
        }

        // --- Click Transfer ---

        private void OnSlotClicked(int index, SlotClickType clickType)
        {
            if (currentStorage == null) return;
            
            var slots = currentStorage.GetSlots();
            if (index < 0 || index >= slots.Count) return;

            var slot = slots[index];
            if (slot.IsEmpty) return;

            int transferAmount = GetTransferAmount(slot.quantity, clickType);

            if (InventoryManager.Instance != null)
            {
                string itemName = slot.item.displayName;

                if (InventoryManager.Instance.AddItem(slot.item, transferAmount))
                {
                    currentStorage.RemoveItem(index, transferAmount, out _);
                    RefreshUI();
                }
                else
                {
                }
            }
        }

        private int GetTransferAmount(int totalQuantity, SlotClickType clickType)
        {
            switch (clickType)
            {
                case SlotClickType.TransferOne:  return 1;
                case SlotClickType.TransferHalf: return Mathf.CeilToInt(totalQuantity / 2f);
                case SlotClickType.TransferAll:  return totalQuantity;
                default: return 1;
            }
        }

        private void RefreshUI()
        {
            if (currentStorage == null) return;

            var storageSlots = currentStorage.GetSlots();
            for (int i = 0; i < storageSlots.Count && i < uiSlots.Count; i++)
            {
                var dataSlot = storageSlots[i];
                var uiSlot = uiSlots[i];

                if (!dataSlot.IsEmpty)
                {
                    uiSlot.SetItem(dataSlot.item, dataSlot.quantity);
                }
                else
                {
                    uiSlot.ClearSlot();
                }
            }
        }
    }
}

