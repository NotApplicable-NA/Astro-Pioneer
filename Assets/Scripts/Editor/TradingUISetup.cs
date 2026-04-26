using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using AstroPioneer.UI;

namespace AstroPioneer.EditorUtilities
{
    public class TradingUISetup
    {
        [MenuItem("Astro-Pioneer/Generate Trading UI Prefab")]
        public static void GenerateTradingUI()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("Trading_Canvas_Auto", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // 2. Create Panel
            GameObject panelObj = new GameObject("Trading_Panel", typeof(RectTransform), typeof(Image));
            panelObj.transform.SetParent(canvasObj.transform, false);
            SetRect(panelObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 400));
            panelObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add TradingUI Script to the PANEL itself (so it survives being dragged around)
            TradingUI tradingUI = panelObj.AddComponent<TradingUI>();
            SerializedObject so = new SerializedObject(tradingUI);
            so.FindProperty("tradingPanel").objectReferenceValue = panelObj;

            // 3. Create Tabs
            GameObject buyTabObj = CreateButton("Buy Tab Button", panelObj.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(100, 40), new Vector2(50, -20));
            GameObject sellTabObj = CreateButton("Sell Tab Button", panelObj.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(100, 40), new Vector2(160, -20));
            so.FindProperty("buyTabButton").objectReferenceValue = buyTabObj.GetComponent<Button>();
            so.FindProperty("sellTabButton").objectReferenceValue = sellTabObj.GetComponent<Button>();

            // 4. Create Sub-Panels
            GameObject buyPanelObj = new GameObject("Buy Panel", typeof(RectTransform));
            buyPanelObj.transform.SetParent(panelObj.transform, false);
            SetRect(buyPanelObj, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, new Vector2(0, -50), Vector2.zero);
            
            GameObject sellPanelObj = new GameObject("Sell Panel", typeof(RectTransform));
            sellPanelObj.transform.SetParent(panelObj.transform, false);
            SetRect(sellPanelObj, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, new Vector2(0, -50), Vector2.zero);
            sellPanelObj.SetActive(false);

            so.FindProperty("buyPanel").objectReferenceValue = buyPanelObj;
            so.FindProperty("sellPanel").objectReferenceValue = sellPanelObj;

            // 5. Create Containers
            GameObject buyListContainer = new GameObject("Buy List Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buyListContainer.transform.SetParent(buyPanelObj.transform, false);
            SetRect(buyListContainer, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, Vector2.zero);
            buyListContainer.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            
            GameObject sellListContainer = new GameObject("Sell List Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            sellListContainer.transform.SetParent(sellPanelObj.transform, false);
            SetRect(sellListContainer, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, Vector2.zero);
            sellListContainer.GetComponent<VerticalLayoutGroup>().childControlHeight = false;

            so.FindProperty("buyListContainer").objectReferenceValue = buyListContainer.transform;
            so.FindProperty("sellListContainer").objectReferenceValue = sellListContainer.transform;

            // 6. Create Texts
            GameObject balanceText = CreateText("Balance Text", panelObj.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(150, 30), new Vector2(-80, -20), "1000cr");
            GameObject trustText = CreateText("Trust Text", panelObj.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(150, 30), new Vector2(-80, -50), "Trust: 0");
            GameObject statusText = CreateText("Status Text", panelObj.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(400, 30), new Vector2(0, 20), "Welcome!");

            so.FindProperty("balanceText").objectReferenceValue = balanceText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("trustText").objectReferenceValue = trustText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();

            // 7. Generate Prefabs for Rows
            GameObject buyRowPrefab = GenerateRowPrefab("TradeRowPrefab_Buy");
            GameObject sellRowPrefab = GenerateRowPrefab("TradeRowPrefab_Sell");

            so.FindProperty("buyItemRowPrefab").objectReferenceValue = buyRowPrefab;
            so.FindProperty("sellItemRowPrefab").objectReferenceValue = sellRowPrefab;

            // Apply all bindings
            so.ApplyModifiedProperties();

            // Disable canvas by default so it waits for the click
            panelObj.SetActive(false);
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pos)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            SetRect(obj, anchorMin, anchorMax, size, pos, pos);
            
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(obj.transform, false);
            SetRect(textObj, Vector2.zero, Vector2.one, Vector2.zero);
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = name.Replace(" Button", "");
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private static GameObject CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pos, string defaultText)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            SetRect(obj, anchorMin, anchorMax, size, pos, pos);
            var tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject GenerateRowPrefab(string name)
        {
            GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image));
            SetRect(row, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 40));
            row.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

            CreateText("Name", row.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(150, 30), new Vector2(80, 0), "Item Name");
            CreateText("Price", row.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(80, 30), new Vector2(0, 0), "100cr");
            CreateText("Quantity", row.transform, new Vector2(0.7f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(50, 30), new Vector2(0, 0), "x1");
            CreateButton("ActionButton", row.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(80, 30), new Vector2(-50, 0));

            return row;
        }

        private static void SetRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition = default, Vector2 localPos = default)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
            if (anchoredPosition != default) rt.anchoredPosition = anchoredPosition;
            if (localPos != default) rt.localPosition = new Vector3(localPos.x, localPos.y, 0f);
        }
    }
}
