using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Managers;
using AstroPioneer.Machines;
using AstroPioneer.Data;
using AstroPioneer.Player;

namespace AstroPioneer.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Hotbar Visual")]
        [SerializeField] private int hotbarSlotCount = 6;
        [SerializeField] private Color hotbarSlotTint = new Color(1f, 0.92f, 0.7f, 1f);
        [SerializeField] private Color normalSlotTint = Color.white;

        [Header("Hotbar Highlight")]
        [Tooltip("Same UI_Selector sprite used in ToolBarUI")]
        [SerializeField] private Sprite highlightSprite;

        private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
        private MachineStorage activeStorage;

        // Drag & Drop
        private GameObject dragGhost;
        private InventorySlotUI dragSourceSlot;
        private Canvas rootCanvas;
        private StorageUI cachedStorageUI;

        public InventorySlotUI DragSourceSlot => dragSourceSlot;

        // Hotbar highlight overlays (first N slots)
        private List<GameObject> hotbarHighlights = new List<GameObject>();

        private bool needsInit = false;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated += RefreshUI;
                if (gameObject.activeInHierarchy)
                    Invoke(nameof(InitializeSlots), 0.1f);
                else
                    needsInit = true;
            }
            
            // Deferred init if previously flagged
            if (needsInit && gameObject.activeInHierarchy)
            {
                needsInit = false;
                Invoke(nameof(InitializeSlots), 0.1f);
            }
            PlayerToolState.OnHotbarSelectionChanged += UpdateHotbarHighlight;

            // Immediately sync highlight to current selection (prevents visible shift during 0.1s delay)
            if (PlayerToolState.Instance != null)
                UpdateHotbarHighlight(PlayerToolState.Instance.GetSelectedHotbarIndex());
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryUpdated -= RefreshUI;
            }
            PlayerToolState.OnHotbarSelectionChanged -= UpdateHotbarHighlight;
            DestroyDragGhost();
        }

        void Update()
        {
            if (dragGhost != null)
            {
                dragGhost.transform.position = Input.mousePosition;
            }
        }

        public void SetActiveStorage(MachineStorage storage)
        {
            activeStorage = storage;
        }

        public void ClearActiveStorage()
        {
            activeStorage = null;
        }

        private void InitializeSlots()
        {
            if (InventoryManager.Instance == null) return;
            if (!gameObject.activeInHierarchy)
            {
                needsInit = true;
                return;
            }
            StartCoroutine(InitializeSlotsCoroutine());
        }

        private IEnumerator InitializeSlotsCoroutine()
        {
            var dataSlots = InventoryManager.Instance.GetSlots();

            rootCanvas = GetComponentInParent<Canvas>();
            cachedStorageUI = FindObjectOfType<StorageUI>();

            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }
            uiSlots.Clear();
            hotbarHighlights.Clear();

            int batchSize = 100; // Instantiate 100 slots per frame

            for (int i = 0; i < dataSlots.Count; i++)
            {
                int index = i;
                GameObject newSlotObj = Instantiate(slotPrefab, slotContainer);
                InventorySlotUI uiSlot = newSlotObj.GetComponent<InventorySlotUI>();

                if (uiSlot != null)
                {
                    uiSlot.Setup((clickType) => OnSlotClicked(index, clickType));
                    uiSlot.SetupDrag(index, OnSlotBeginDrag, OnSlotDrop, OnSlotEndDrag);
                    uiSlots.Add(uiSlot);
                }

                Image slotBg = newSlotObj.GetComponent<Image>();
                if (slotBg != null)
                    slotBg.color = (i < hotbarSlotCount) ? hotbarSlotTint : normalSlotTint;

                if (i < hotbarSlotCount && highlightSprite != null)
                {
                    GameObject hlObj = new GameObject("Highlight");
                    hlObj.transform.SetParent(newSlotObj.transform, false);

                    RectTransform hrt = hlObj.AddComponent<RectTransform>();
                    hrt.anchorMin = Vector2.zero;
                    hrt.anchorMax = Vector2.one;
                    hrt.offsetMin = Vector2.zero;
                    hrt.offsetMax = Vector2.zero;

                    Image hlImg = hlObj.AddComponent<Image>();
                    hlImg.sprite = highlightSprite;
                    hlImg.raycastTarget = false;
                    hlImg.preserveAspect = true;

                    hlObj.SetActive(false);
                    hotbarHighlights.Add(hlObj);
                }

                if (i % batchSize == 0 && i > 0)
                {
                    yield return null; // Wait for next frame
                }
            }

            RefreshUI();

            if (PlayerToolState.Instance != null)
                UpdateHotbarHighlight(PlayerToolState.Instance.GetSelectedHotbarIndex());
        }

        private void UpdateHotbarHighlight(int selectedIndex)
        {
            for (int i = 0; i < hotbarHighlights.Count; i++)
            {
                if (hotbarHighlights[i] != null)
                    hotbarHighlights[i].SetActive(i == selectedIndex);
            }
        }


        // --- Drag & Drop ---

        private void OnSlotBeginDrag(InventorySlotUI sourceSlot)
        {
            if (sourceSlot.CurrentItem == null) return;
            dragSourceSlot = sourceSlot;
            CreateDragGhost(sourceSlot.CurrentItem.icon);
        }

        private void OnSlotDrop(InventorySlotUI targetSlot)
        {
            if (InventoryManager.Instance == null) return;

            // Cross-panel: Storage → Inventory (Remove-first to prevent duplication)
            if (cachedStorageUI != null && cachedStorageUI.DragSourceSlot != null && cachedStorageUI.CurrentStorage != null)
            {
                int storageSlotIdx = cachedStorageUI.DragSourceSlot.SlotIndex;
                var storageSlots = cachedStorageUI.CurrentStorage.GetSlots();
                if (storageSlotIdx >= 0 && storageSlotIdx < storageSlots.Count)
                {
                    var srcSlot = storageSlots[storageSlotIdx];
                    if (!srcSlot.IsEmpty)
                    {
                        string itemName = srcSlot.item.displayName;
                        InventoryItem itemRef = srcSlot.item;
                        int qty = srcSlot.quantity;

                        // Step 1: Remove from storage FIRST
                        cachedStorageUI.CurrentStorage.RemoveItem(storageSlotIdx, qty, out _);

                        // Step 2: Try add to inventory
                        if (!InventoryManager.Instance.AddItem(itemRef, qty))
                        {
                            // Rollback: put it back in storage
                            cachedStorageUI.CurrentStorage.TryAddItem(itemRef, qty);
                        }
                        else
                        {
                        }
                    }
                }
                return;
            }

            // Within-inventory swap
            if (dragSourceSlot == null || dragSourceSlot == targetSlot) return;
            InventoryManager.Instance.SwapSlots(dragSourceSlot.SlotIndex, targetSlot.SlotIndex);
        }

        private void OnSlotEndDrag()
        {
            dragSourceSlot = null;
            DestroyDragGhost();
        }

        private void CreateDragGhost(Sprite icon)
        {
            DestroyDragGhost();
            dragGhost = new GameObject("DragGhost");
            if (rootCanvas != null)
                dragGhost.transform.SetParent(rootCanvas.transform, false);

            Image img = dragGhost.AddComponent<Image>();
            img.sprite = icon;
            img.raycastTarget = false;

            RectTransform rt = dragGhost.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48, 48);
            img.color = new Color(1f, 1f, 1f, 0.7f);

            Canvas ghostCanvas = dragGhost.AddComponent<Canvas>();
            ghostCanvas.overrideSorting = true;
            ghostCanvas.sortingOrder = 100;

            dragGhost.transform.position = Input.mousePosition;
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
            if (InventoryManager.Instance == null) return;

            var slots = InventoryManager.Instance.GetSlots();
            if (index < 0 || index >= slots.Count) return;

            var slot = slots[index];
            if (slot.IsEmpty) return;

            if (activeStorage == null) return;

            int transferAmount = GetTransferAmount(slot.quantity, clickType);
            string itemName = slot.item.displayName;

            if (activeStorage.TryAddItem(slot.item, transferAmount))
            {
                InventoryManager.Instance.RemoveItem(slot.item, transferAmount);
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
            if (InventoryManager.Instance == null) return;
            var dataSlots = InventoryManager.Instance.GetSlots();

            for (int i = 0; i < dataSlots.Count && i < uiSlots.Count; i++)
            {
                var dataSlot = dataSlots[i];
                var uiSlot = uiSlots[i];

                if (!dataSlot.IsEmpty)
                    uiSlot.SetItem(dataSlot.item, dataSlot.quantity);
                else
                    uiSlot.ClearSlot();
            }
        }
    }
}
