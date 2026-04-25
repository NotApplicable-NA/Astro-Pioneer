using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Systems;

namespace AstroPioneer.Core
{
    /// <summary>
    /// Bootstrapper — Central initialization point.
    /// Ensures all core services are registered in the correct order BEFORE any consumer needs them.
    /// This eliminates race conditions caused by unpredictable Awake() execution order.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Core Managers")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ChunkManager chunkManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private TechTreeManager techTreeManager;
        [SerializeField] private ShipUpgradeManager shipUpgradeManager;
        [SerializeField] private AstroPioneer.Data.StructureRegistry structureRegistry;
        [SerializeField] private AstroPioneer.Data.ItemRegistry itemRegistry;
        [SerializeField] private AstroPioneer.Data.EntityRegistry entityRegistry;

        [Header("Systems")]
        [SerializeField] private PlacementManager placementManager;
        [SerializeField] private CropManager cropManager;
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private ObjectPoolManager objectPoolManager;
        [SerializeField] private EntityManager entityManager;

        private void Awake()
        {
            AssertAllServicesAssigned();
            InitializeServices();
        }

        private void AssertAllServicesAssigned()
        {
            bool hasErrors = false;
            // Core Managers
            if (gridManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'gridManager' is not assigned!"); hasErrors = true; }
            if (chunkManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'chunkManager' is not assigned!"); hasErrors = true; }
            if (timeManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'timeManager' is not assigned!"); hasErrors = true; }
            if (inventoryManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'inventoryManager' is not assigned!"); hasErrors = true; }
            if (currencyManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'currencyManager' is not assigned!"); hasErrors = true; }
            if (techTreeManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'techTreeManager' is not assigned!"); hasErrors = true; }
            if (shipUpgradeManager == null) { Debug.LogWarning("[Bootstrapper] Warning: 'shipUpgradeManager' is not assigned (OK if not implemented yet)."); }

            // Systems
            if (placementManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'placementManager' is not assigned!"); hasErrors = true; }
            if (cropManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'cropManager' is not assigned!"); hasErrors = true; }
            if (powerManager == null) { Debug.LogWarning("[Bootstrapper] Warning: 'powerManager' is not assigned (OK if not implemented yet)."); }
            if (objectPoolManager == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'objectPoolManager' is not assigned!"); hasErrors = true; }
            if (entityManager == null) { Debug.LogWarning("[Bootstrapper] Warning: 'entityManager' is not assigned (OK if not implemented yet)."); }
            if (structureRegistry == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'structureRegistry' is not assigned in the Inspector!"); hasErrors = true; }
            if (itemRegistry == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'itemRegistry' is not assigned in the Inspector!"); hasErrors = true; }
            if (entityRegistry == null) { Debug.LogError("[Bootstrapper] CRITICAL: 'entityRegistry' is not assigned in the Inspector!"); hasErrors = true; }

            if (!hasErrors) Debug.Log("[Bootstrapper] All core services verified.");
        }

        private void InitializeServices()
        {
            Debug.Log("[Bootstrapper] Initializing Core Services...");

            // 1. Core Data & Infrastructure
            if (structureRegistry != null) 
            {
                AstroPioneer.Data.StructureRegistry.Instance = structureRegistry;
                ServiceLocator.Register(structureRegistry);
            }
            if (itemRegistry != null) 
            {
                AstroPioneer.Data.ItemRegistry.Instance = itemRegistry;
                ServiceLocator.Register(itemRegistry);
            }
            if (entityRegistry != null)
            {
                AstroPioneer.Data.EntityRegistry.Instance = entityRegistry;
                ServiceLocator.Register(entityRegistry);
            }
            if (chunkManager != null) ServiceLocator.Register(chunkManager);
            if (gridManager != null) ServiceLocator.Register(gridManager);
            if (objectPoolManager != null) ServiceLocator.Register(objectPoolManager);

            // 2. Simulation & Systems
            if (timeManager != null) ServiceLocator.Register(timeManager);
            if (powerManager != null) ServiceLocator.Register(powerManager);
            if (placementManager != null) ServiceLocator.Register(placementManager);
            if (cropManager != null) ServiceLocator.Register(cropManager);
            if (entityManager != null) ServiceLocator.Register(entityManager);

            // 3. Progression & Economy
            if (inventoryManager != null) ServiceLocator.Register(inventoryManager);
            if (currencyManager != null) ServiceLocator.Register(currencyManager);
            if (techTreeManager != null) ServiceLocator.Register(techTreeManager);
            if (shipUpgradeManager != null) ServiceLocator.Register(shipUpgradeManager);

            Debug.Log("[Bootstrapper] Service Registration Complete.");
        }
    }
}
