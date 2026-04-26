using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AstroPioneer.Player;
using AstroPioneer.Managers;
using AstroPioneer.Data;

namespace AstroPioneer.UI
{
    /// <summary>
    /// ToolBarUI — Dynamically generates hotbar slots from a prefab.
    /// Mirrors the first N inventory slots. Uses a highlight sprite overlay for selection.
    /// </summary>
    public class ToolBarUI : MonoBehaviour
    {
        [Header("Slot Generation")]
        [Tooltip("Prefab used for each hotbar slot (same as inventory Slot_Prefab)")]
        [SerializeField] private GameObject slotPrefab;
        
        [Tooltip("Container for hotbar slots (this GameObject or a child)")]
        [SerializeField] private Transform slotContainer;

        [Header("Highlight")]
        [Tooltip("Sprite used for selected slot highlight (e.g. UI_Selector)")]
        [SerializeField] private Sprite highlightSprite;

        // Runtime data
        private List<GameObject> highlightOverlays = new List<GameObject>();
        private List<Image> slotIcons = new List<Image>();

        void Start()
        {
            if (slotContainer == null)
                slotContainer = transform;

            GenerateSlots();

            PlayerToolState.OnHotbarSelectionChanged += UpdateHighlight;
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryUpdated += RefreshIcons;

            RefreshIcons();
            if (PlayerToolState.Instance != null)
                UpdateHighlight(PlayerToolState.Instance.GetSelectedHotbarIndex());
        }

        void OnDestroy()
        {
            PlayerToolState.OnHotbarSelectionChanged -= UpdateHighlight;
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryUpdated -= RefreshIcons;
        }

        private void GenerateSlots()
        {
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }
            highlightOverlays.Clear();
            slotIcons.Clear();

            if (slotPrefab == null)
            {
                return;
            }

            int slotCount = (PlayerToolState.Instance != null)
                ? 6  // Match InventoryUI.hotbarSlotCount
                : 6;

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                slotObj.name = $"HotbarSlot_{i + 1}";

                Transform iconTransform = slotObj.transform.Find("Icon");
                Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
                slotIcons.Add(iconImage);

                if (highlightSprite != null)
                {
                    GameObject highlightObj = new GameObject("Highlight");
                    highlightObj.transform.SetParent(slotObj.transform, false);

                    RectTransform hrt = highlightObj.AddComponent<RectTransform>();
                    hrt.anchorMin = Vector2.zero;
                    hrt.anchorMax = Vector2.one;
                    hrt.offsetMin = Vector2.zero;
                    hrt.offsetMax = Vector2.zero;

                    Image highlightImg = highlightObj.AddComponent<Image>();
                    highlightImg.sprite = highlightSprite;
                    highlightImg.raycastTarget = false;
                    highlightImg.preserveAspect = true;

                    highlightObj.SetActive(false);
                    highlightOverlays.Add(highlightObj);
                }
                else
                {
                    highlightOverlays.Add(null);
                }

                // Disable InventorySlotUI on hotbar (no click-to-transfer)
                var slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null) slotUI.enabled = false;

                Transform qtyTransform = slotObj.transform.Find("QuantityText");
                if (qtyTransform != null)
                {
                    Text qtyText = qtyTransform.GetComponent<Text>();
                    if (qtyText != null) qtyText.enabled = false;
                }
            }
        }

        private void UpdateHighlight(int selectedIndex)
        {
            for (int i = 0; i < highlightOverlays.Count; i++)
            {
                if (highlightOverlays[i] != null)
                    highlightOverlays[i].SetActive(i == selectedIndex);
            }
        }

        private void RefreshIcons()
        {
            if (InventoryManager.Instance == null) return;

            for (int i = 0; i < slotIcons.Count; i++)
            {
                if (slotIcons[i] == null) continue;

                InventorySlot slot = InventoryManager.Instance.GetSlotAt(i);
                if (slot != null && !slot.IsEmpty && slot.item != null)
                {
                    slotIcons[i].sprite = slot.item.icon;
                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].enabled = false;
                }
            }
        }
    }
}
