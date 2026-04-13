using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace AstroPioneer.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject hotbarPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject holographicTabletPanel;

        [Header("State")]
        private bool isInventoryOpen = false;
        private bool isPaused = false;
        private bool isStorageOpen = false;
        private bool isTabletOpen = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Only persist across scenes if this is a root-level object
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ReassignPanelReferences();
        }

        private void ReassignPanelReferences()
        {
            if (inventoryPanel == null) inventoryPanel = GameObject.Find("InventoryPanel");
            if (hotbarPanel == null) hotbarPanel = GameObject.Find("HotbarPanel");
            if (holographicTabletPanel == null) holographicTabletPanel = GameObject.Find("HolographicTabletPanel");
            if (inventoryUI == null && inventoryPanel != null)
                inventoryUI = inventoryPanel.GetComponent<AstroPioneer.UI.InventoryUI>();
            if (holographicTabletUI == null)
            {
                if (holographicTabletPanel != null)
                    holographicTabletUI = holographicTabletPanel.GetComponent<AstroPioneer.UI.HolographicTabletUI>();
                if (holographicTabletUI == null)
                    holographicTabletUI = Object.FindObjectOfType<AstroPioneer.UI.HolographicTabletUI>(true);
            }
            if (storageUI == null)
            {
                var found = Object.FindObjectOfType<AstroPioneer.UI.StorageUI>();
                if (found != null) storageUI = found;
            }
        }

        void Start()
        {
            ReassignPanelReferences();

            // Initial state
            if (hotbarPanel != null) hotbarPanel.SetActive(true);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (holographicTabletPanel != null) holographicTabletPanel.SetActive(false);
        }

        void Update()
        {
            // Input for Toggle Inventory (Tab or I)
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }

            // Input for Pause (Esc)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            // Input for Map Tablet (M)
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleTablet();
            }
        }

        [SerializeField] private AstroPioneer.UI.StorageUI storageUI;
        [SerializeField] private AstroPioneer.UI.InventoryUI inventoryUI;
        [SerializeField] private AstroPioneer.UI.HolographicTabletUI holographicTabletUI;

        public void OpenStorageUI(AstroPioneer.Machines.MachineStorage storage)
        {
            if (storageUI != null)
            {
                storageUI.Open(storage);
                isStorageOpen = true;
                if (inventoryUI != null) inventoryUI.SetActiveStorage(storage);
                if (!isInventoryOpen) 
                {
                    ToggleInventory();
                }
            }
            else
            {
            }
        }

        public void CloseStorageUI()
        {
             if (storageUI != null)
             {
                 storageUI.CloseUI();
             }
             isStorageOpen = false;
             if (inventoryUI != null) inventoryUI.ClearActiveStorage();
        }

        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
            }
            else
            {
            }

            // When closing inventory, also close storage if open
            if (!isInventoryOpen && isStorageOpen)
            {
                CloseStorageUI();
            }

            // Hide standalone hotbar when inventory is open (row 1 of grid IS the hotbar)
            if (hotbarPanel != null)
            {
                hotbarPanel.SetActive(!isInventoryOpen);
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(isPaused);
                Time.timeScale = isPaused ? 0 : 1f;
            }
            
            if (isPaused) CloseStorageUI();
        }

        public void ToggleTablet()
        {
            // Retry find if not yet assigned
            if (holographicTabletUI == null)
            {
                holographicTabletUI = Object.FindObjectOfType<AstroPioneer.UI.HolographicTabletUI>(true);
            }

            if (holographicTabletUI != null)
            {
                isTabletOpen = !isTabletOpen;
                holographicTabletUI.Toggle();
                
                // TODO: Re-enable Time.timeScale pause once tablet UI has proper visuals
                // Time.timeScale = isTabletOpen ? 0 : 1f;

                if (isTabletOpen && isInventoryOpen) ToggleInventory();
            }
            else
            {
            }
        }
    }
}
