using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AstroPioneer.Data;

namespace AstroPioneer.UI
{
    /// <summary>
    /// Click type passed to parent UI for partial transfer logic.
    /// </summary>
    public enum SlotClickType
    {
        TransferOne,    // Left-click: transfer 1 item
        TransferHalf,   // Shift + Left-click: transfer half the stack
        TransferAll     // Right-click: transfer entire stack
    }

    [RequireComponent(typeof(Button))]
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text quantityText;
        [SerializeField] private Button slotButton;

        private InventoryItem currentItem;
        private System.Action<SlotClickType> onSlotClicked;

        // Drag & Drop
        private System.Action<InventorySlotUI> onBeginDrag;
        private System.Action<InventorySlotUI> onDrop;
        private System.Action onEndDrag;
        private int _slotIndex = -1;
        private bool isDragging = false;

        public int SlotIndex => _slotIndex;
        public InventoryItem CurrentItem => currentItem;

        void Awake()
        {
            if (slotButton == null)
            {
                slotButton = GetComponent<Button>();
                if (slotButton == null) slotButton = gameObject.AddComponent<Button>();
            }
        }

        /// <summary>
        /// IPointerClickHandler — only fires if NOT dragging.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;
            if (onSlotClicked == null) return;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onSlotClicked.Invoke(SlotClickType.TransferAll);
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    onSlotClicked.Invoke(SlotClickType.TransferHalf);
                else
                    onSlotClicked.Invoke(SlotClickType.TransferOne);
            }
        }

        // --- Drag & Drop ---
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (currentItem == null) return;

            isDragging = true;
            onBeginDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Ghost icon movement is handled by InventoryUI
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;
            onEndDrag?.Invoke();
        }

        public void OnDrop(PointerEventData eventData)
        {
            // This slot is the DROP TARGET
            onDrop?.Invoke(this);
        }

        public void Setup(System.Action<SlotClickType> onClick)
        {
            onSlotClicked = onClick;
        }

        public void SetupDrag(int index, System.Action<InventorySlotUI> beginDrag,
            System.Action<InventorySlotUI> drop, System.Action endDrag)
        {
            _slotIndex = index;
            onBeginDrag = beginDrag;
            onDrop = drop;
            onEndDrag = endDrag;
        }

        public void SetItem(InventoryItem item, int quantity)
        {
            currentItem = item;
            
            if (item != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }

                if (quantityText != null)
                {
                    bool showText = quantity > 1;
                    quantityText.text = showText ? quantity.ToString() : "";
                    quantityText.enabled = showText;
                }
            }
            else
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            currentItem = null;
            
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
        }
    }
}

