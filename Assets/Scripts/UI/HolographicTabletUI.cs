using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Managers;

namespace AstroPioneer.UI
{
    /// <summary>
    /// HolographicTabletUI - Manages the diegetic map tablet (Toggle [M]).
    /// Renders a simplified top-down view of the current active grid.
    /// </summary>
    public class HolographicTabletUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject tabletPanel;
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private GameObject mapIconPrefab;

        [Header("Map Icons (Placeholders)")]
        [SerializeField] private Sprite playerIcon;
        [SerializeField] private Sprite machineIcon;
        [SerializeField] private Sprite cropIcon;
        [SerializeField] private Sprite floraIcon;

        [Header("Colors")]
        public Color hologramColor = new Color(0f, 0.8f, 1f, 0.5f);
        public Color playerColor = Color.yellow;
        public Color machineColor = Color.white;
        public Color cropColor = Color.green;
        [Tooltip("Warna saat area masih tertutup kabut")]
        public Color fogColor = new Color(0f, 0f, 0f, 0.8f); // Hitam pekat agar beda dari background map

        private List<GameObject> activeIcons = new List<GameObject>();

        void Start()
        {
            AutoGenerateUI();
            if (tabletPanel != null) tabletPanel.SetActive(false);
        }

        private void AutoGenerateUI()
        {
            // Find or create a Canvas parent
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            if (canvas == null) return;

            // Auto-create tablet panel if not assigned — use self
            if (tabletPanel == null)
            {
                tabletPanel = this.gameObject;

                // Ensure we have an Image for the dark background
                Image panelImg = tabletPanel.GetComponent<Image>();
                if (panelImg == null) panelImg = tabletPanel.AddComponent<Image>();
                panelImg.color = new Color(0.02f, 0.05f, 0.1f, 0.85f);

                // Stretch fullscreen
                RectTransform panelRT = tabletPanel.GetComponent<RectTransform>();
                panelRT.anchorMin = Vector2.zero;
                panelRT.anchorMax = Vector2.one;
                panelRT.offsetMin = Vector2.zero;
                panelRT.offsetMax = Vector2.zero;
            }

            // Auto-create map container if not assigned
            if (mapContainer == null)
            {
                GameObject mapArea = new GameObject("MapArea_Auto", typeof(RectTransform), typeof(Image));
                mapArea.transform.SetParent(tabletPanel.transform, false);

                mapContainer = mapArea.GetComponent<RectTransform>();
                mapContainer.anchorMin = new Vector2(0.15f, 0.1f);
                mapContainer.anchorMax = new Vector2(0.85f, 0.9f);
                mapContainer.offsetMin = Vector2.zero;
                mapContainer.offsetMax = Vector2.zero;

                // Make map background BRIGHT so explored empty space is clearly visible
                Image mapImg = mapArea.GetComponent<Image>();
                mapImg.color = new Color(0.8f, 0.8f, 0.8f, 0.9f); // Bright Gray

                // Add title label
                GameObject titleObj = new GameObject("Title_Auto", typeof(RectTransform));
                titleObj.transform.SetParent(tabletPanel.transform, false);
                var titleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
                titleText.text = "HOLOGRAPHIC MAP";
                titleText.fontSize = 24;
                titleText.color = new Color(0f, 0.9f, 1f, 0.9f);
                titleText.alignment = TMPro.TextAlignmentOptions.Center;

                RectTransform titleRT = titleObj.GetComponent<RectTransform>();
                titleRT.anchorMin = new Vector2(0.2f, 0.92f);
                titleRT.anchorMax = new Vector2(0.8f, 0.98f);
                titleRT.offsetMin = Vector2.zero;
                titleRT.offsetMax = Vector2.zero;
            }
        }

        private bool needsRefresh = false;

        void OnEnable()
        {
            // Delay refresh by marking a flag
            needsRefresh = true;
        }

        public void Toggle()
        {
            if (tabletPanel != null)
            {
                // Force parent active if it was disabled by mistake in Inspector
                if (tabletPanel.transform.parent != null && !tabletPanel.transform.parent.gameObject.activeInHierarchy)
                {
                    tabletPanel.transform.parent.gameObject.SetActive(true);
                }
                
                tabletPanel.SetActive(!tabletPanel.activeSelf);
            }
            else
            {
                // If it's on this very object
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

        private Vector2Int lastPlayerCell = new Vector2Int(-999, -999);
        private RawImage fogOverlay;
        private Texture2D fogTexture;
        private Color32[] fogPixels;

        private void InitializeFogTexture()
        {
            if (GridManager.Instance == null || mapContainer == null) return;

            var gridDims = GridManager.Instance.GridDimensions;

            // Create RawImage for smooth fog if missing
            if (fogOverlay == null)
            {
                GameObject fogObj = new GameObject("FogOverlay", typeof(RectTransform), typeof(RawImage));
                fogObj.transform.SetParent(mapContainer, false);
                fogOverlay = fogObj.GetComponent<RawImage>();
                
                RectTransform rt = fogObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.SetAsLastSibling(); // Fog covers POIs unless they are revealed
            }

            // Init Texture matching grid dimensions (low res + bilinear = smooth)
            if (fogTexture == null || fogTexture.width != gridDims.x)
            {
                fogTexture = new Texture2D(gridDims.x, gridDims.y, TextureFormat.RGBA32, false);
                fogTexture.filterMode = FilterMode.Bilinear; // IMPORTANT: This makes it smooth!
                fogTexture.wrapMode = TextureWrapMode.Clamp;
                fogOverlay.texture = fogTexture;
                fogPixels = new Color32[gridDims.x * gridDims.y];
            }
        }

        void LateUpdate()
        {
            if (needsRefresh)
            {
                needsRefresh = false;
                Canvas.ForceUpdateCanvases();
                InitializeFogTexture();
                RefreshMap();
            }
        }

        void Update()
        {
            // Update runs natively only when the gameobject is active
            UpdatePlayerMarker();
            UpdateFogTexture();

            // Periodic check for new POIs
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && GridManager.Instance != null)
            {
                Vector2Int currentCell = GridManager.Instance.WorldToGridPosition(player.transform.position);
                if (currentCell != lastPlayerCell)
                {
                    lastPlayerCell = currentCell;
                    var tracker = player.GetComponent<AstroPioneer.Systems.Exploration.ExplorationTracker>();
                    if (tracker != null) tracker.ForceReveal();
                    RefreshMap(); // Update POIs
                }
            }
        }

        private Dictionary<Vector2Int, GameObject> poiIcons = new Dictionary<Vector2Int, GameObject>();
        private GameObject playerMarker;

        private void ClearAllIcons()
        {
            foreach (var kvp in poiIcons) if (kvp.Value != null) Destroy(kvp.Value);
            poiIcons.Clear();
            if (playerMarker != null) Destroy(playerMarker);
        }

        private void UpdateFogTexture()
        {
            if (fogTexture == null || GridManager.Instance == null) return;

            var dims = GridManager.Instance.GridDimensions;
            bool changed = false;

            for (int y = 0; y < dims.y; y++)
            {
                for (int x = 0; x < dims.x; x++)
                {
                    int index = y * dims.x + x;
                    bool explored = GridManager.Instance.IsCellExplored(new Vector2Int(x, y));
                    
                    // Fog is black, explored is transparent
                    byte targetAlpha = explored ? (byte)0 : (byte)200;
                    byte currentAlpha = fogPixels[index].a;
                    
                    if (currentAlpha != targetAlpha)
                    {
                        // Melt fog smoothly over time (~1 second to clear)
                        float speed = 200f * Time.deltaTime;
                        byte newAlpha = (byte)Mathf.MoveTowards(currentAlpha, targetAlpha, speed);
                        
                        fogPixels[index] = new Color32(0, 0, 0, newAlpha);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                fogTexture.SetPixels32(fogPixels);
                fogTexture.Apply();
            }
        }

        private void RefreshMap()
        {
            if (GridManager.Instance == null) return;

            var gridDims = GridManager.Instance.GridDimensions;
            var occupied = GridManager.Instance.GetOccupiedCells();

            float cellWidth = mapContainer.rect.width / gridDims.x;
            float cellHeight = mapContainer.rect.height / gridDims.y;

            // Player Marker
            if (playerMarker == null)
            {
                playerMarker = CreateIcon(Vector2Int.zero, playerIcon, playerColor, cellWidth * 0.8f, cellHeight * 0.8f, false);
                playerMarker.transform.SetAsLastSibling(); // Player on top of fog
            }

            // POIs
            for (int x = 0; x < gridDims.x; x++)
            {
                for (int y = 0; y < gridDims.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    bool isExplored = GridManager.Instance.IsCellExplored(pos);

                    if (isExplored)
                    {
                        if (occupied.TryGetValue(pos, out GameObject occupant))
                        {
                            if (!poiIcons.ContainsKey(pos))
                            {
                                RenderPOI(pos, occupant, cellWidth, cellHeight);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator FadeOutAndDisable(Image img)
        {
            if (img == null) yield break;
            
            Color c = img.color;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (img == null) break;
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(c.a, 0f, elapsed / duration);
                img.color = c;
                yield return null;
            }

            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }

        private void RenderPOI(Vector2Int pos, GameObject occupant, float w, float h)
        {
            Sprite iconSprite = machineIcon;
            Color iconColor = machineColor;

            if (occupant.name.Contains("Crop"))
            {
                iconSprite = cropIcon;
                iconColor = cropColor;
            }
            else if (occupant.name.Contains("Oxy"))
            {
                iconSprite = floraIcon;
                iconColor = Color.magenta;
            }

            GameObject poiObj = CreateIcon(pos, iconSprite, iconColor, w * 0.7f, h * 0.7f, false);
            poiIcons[pos] = poiObj;
        }

        private GameObject CreateIcon(Vector2Int gridPos, Sprite sprite, Color color, float w, float h, bool isFog)
        {
            GameObject iconObj;

            if (mapIconPrefab != null)
            {
                iconObj = Instantiate(mapIconPrefab, mapContainer);
            }
            else
            {
                iconObj = new GameObject("MapIcon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(mapContainer, false);
            }

            var gridDims = GridManager.Instance.GridDimensions;
            float cellW = mapContainer.rect.width / gridDims.x;
            float cellH = mapContainer.rect.height / gridDims.y;

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            
            // Center icon in each grid cell
            rt.anchoredPosition = new Vector2(
                gridPos.x * cellW + cellW * 0.5f,
                gridPos.y * cellH + cellH * 0.5f
            );

            Image img = iconObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
                img.color = color;
            }

            if (isFog)
            {
                iconObj.name = $"Fog_{gridPos.x}_{gridPos.y}";
                iconObj.transform.SetAsFirstSibling();
            }

            return iconObj;
        }

        private void UpdatePlayerMarker()
        {
            if (playerMarker == null || GridManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Use continuous exact float position instead of discrete grid position
            Vector3 worldPos = player.transform.position;
            Vector3 origin = GridManager.Instance.GridOrigin;
            float cellSize = GridManager.Instance.CellSize;
            
            float exactX = (worldPos.x - origin.x) / cellSize;
            float exactY = (worldPos.y - origin.y) / cellSize;

            RectTransform rt = playerMarker.GetComponent<RectTransform>();

            float cellW = mapContainer.rect.width / GridManager.Instance.GridDimensions.x;
            float cellH = mapContainer.rect.height / GridManager.Instance.GridDimensions.y;

            Vector2 rawTargetPos = new Vector2(
                exactX * cellW + cellW * 0.5f,
                exactY * cellH + cellH * 0.5f
            );

            // Clamp so the marker doesn't fly out of the UI frame when player walks off the planetary grid
            Vector2 targetPos = new Vector2(
                Mathf.Clamp(rawTargetPos.x, 0f, mapContainer.rect.width),
                Mathf.Clamp(rawTargetPos.y, 0f, mapContainer.rect.height)
            );
            
            // If it's too far, snap it (e.g. initial load or teleport)
            if (Vector2.Distance(rt.anchoredPosition, targetPos) > cellW * 3f)
            {
                rt.anchoredPosition = targetPos;
            }
            else
            {
                // Smooth follow
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, Time.deltaTime * 15f);
            }
        }
    }
}
