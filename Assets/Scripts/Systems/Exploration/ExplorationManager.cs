using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Exploration
{
    public enum ExplorationState
    {
        OnShip,
        Landing,
        Exploring,
        Returning
    }

    /// <summary>
    /// Manages planet exploration: scene loading, transitions, resource node spawning.
    /// Uses additive scene loading for clean separation.
    /// </summary>
    public class ExplorationManager : MonoBehaviour
    {
        public static ExplorationManager Instance { get; private set; }

        [Header("Available Planets")]
        [SerializeField] private List<PlanetData> discoveredPlanets = new List<PlanetData>();

        [Header("Exploration Settings")]
        [SerializeField] private string shipSceneName = "ShipInterior";
        [SerializeField] private float transitionDuration = 1.5f;

        [Header("Resource Spawning")]
        [SerializeField] private GameObject resourceNodePrefab;
        [SerializeField] private int maxResourceNodes = 15;
        [SerializeField] private float spawnRadius = 20f;

        [Header("State")]
        [SerializeField] private ExplorationState currentState = ExplorationState.OnShip;

        // Current exploration
        private PlanetData currentPlanet;
        private List<GameObject> spawnedNodes = new List<GameObject>();
        private Vector3 lastExplorationPosition; // For Walk of Shame

        // Events
        public event Action<PlanetData> OnLandingStarted;
        public event Action<PlanetData> OnExplorationStarted;
        public event Action OnReturnStarted;
        public event Action OnReturnedToShip;

        // Properties
        public ExplorationState CurrentState => currentState;
        public PlanetData CurrentPlanet => currentPlanet;
        public IReadOnlyList<PlanetData> DiscoveredPlanets => discoveredPlanets;
        /// <summary>Where the player was when O2 ran out. Used for Walk of Shame.</summary>
        public Vector3 LastExplorationPosition => lastExplorationPosition;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Planet Travel ───────────────────────────

        /// <summary>
        /// Start traveling to a planet.
        /// </summary>
        public bool TravelToPlanet(PlanetData planet)
        {
            if (planet == null || currentState != ExplorationState.OnShip)
            {
                return false;
            }

            // Check trust requirement
            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.HasTrust(planet.trustRequired))
            {
                return false;
            }

            currentPlanet = planet;
            StartCoroutine(LandOnPlanet(planet));
            return true;
        }

        /// <summary>
        /// Return to ship from planet.
        /// </summary>
        public bool ReturnToShip()
        {
            if (currentState != ExplorationState.Exploring)
            {
                return false;
            }

            // Save last position for Walk of Shame
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                lastExplorationPosition = player.transform.position;

            StartCoroutine(ReturnToShipSequence());
            return true;
        }

        // ── Scene Transitions ───────────────────────

        private IEnumerator LandOnPlanet(PlanetData planet)
        {
            currentState = ExplorationState.Landing;
            OnLandingStarted?.Invoke(planet);

            // TODO: Play landing animation/fade

            // Load planet scene additively
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(planet.sceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                currentState = ExplorationState.OnShip;
                yield break;
            }

            yield return loadOp;

            // Set planet scene as active (lighting, physics)
            Scene planetScene = SceneManager.GetSceneByName(planet.sceneName);
            if (planetScene.IsValid())
                SceneManager.SetActiveScene(planetScene);

            // Spawn resource nodes
            SpawnResourceNodes(planet);

            // Transition complete
            yield return new WaitForSeconds(transitionDuration);

            currentState = ExplorationState.Exploring;
            OnExplorationStarted?.Invoke(planet);
        }

        private IEnumerator ReturnToShipSequence()
        {
            currentState = ExplorationState.Returning;
            OnReturnStarted?.Invoke();

            // Cleanup spawned nodes
            ClearSpawnedNodes();

            // Unload planet scene
            if (currentPlanet != null)
            {
                Scene planetScene = SceneManager.GetSceneByName(currentPlanet.sceneName);
                if (planetScene.IsValid() && planetScene.isLoaded)
                {
                    AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(planetScene);
                    if (unloadOp != null)
                        yield return unloadOp;
                }
            }

            // Set ship scene as active
            Scene shipScene = SceneManager.GetSceneByName(shipSceneName);
            if (shipScene.IsValid())
                SceneManager.SetActiveScene(shipScene);

            yield return new WaitForSeconds(transitionDuration);

            currentPlanet = null;
            currentState = ExplorationState.OnShip;
            OnReturnedToShip?.Invoke();
        }

        // ── Resource Node Spawning ──────────────────

        private void SpawnResourceNodes(PlanetData planet)
        {
            if (resourceNodePrefab == null || planet.resourceNodes.Count == 0) return;

            // Calculate total spawn weight
            float totalWeight = 0f;
            foreach (var entry in planet.resourceNodes)
                totalWeight += entry.spawnWeight;

            for (int i = 0; i < maxResourceNodes; i++)
            {
                // Pick random resource based on weight
                float roll = UnityEngine.Random.Range(0f, totalWeight);
                ResourceSpawnEntry selected = planet.resourceNodes[0];
                float cumulative = 0f;
                foreach (var entry in planet.resourceNodes)
                {
                    cumulative += entry.spawnWeight;
                    if (roll <= cumulative)
                    {
                        selected = entry;
                        break;
                    }
                }

                // Random position within radius
                Vector2 randomPos = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = new Vector3(randomPos.x, randomPos.y, 0f);

                GameObject node = Instantiate(resourceNodePrefab, spawnPos, Quaternion.identity);
                var resourceNode = node.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    resourceNode.Initialize(selected);
                }

                spawnedNodes.Add(node);
            }
        }

        private void ClearSpawnedNodes()
        {
            foreach (var node in spawnedNodes)
            {
                if (node != null) Destroy(node);
            }
            spawnedNodes.Clear();
        }
    }
}
