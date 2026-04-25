using UnityEngine;
using System.Collections;
using AstroPioneer.Systems.Exploration;
using AstroPioneer.Player;
using AstroPioneer.Machines;
using AstroPioneer.Machines.Automation;

namespace AstroPioneer.Systems.Survival
{
    /// <summary>
    /// RescueProtocol — When O2 depletes, Bot-E rescues the player
    /// and teleports them back to the SleepPod. Zero penalties (GDD 3.5).
    /// </summary>
    public class RescueProtocol : MonoBehaviour
    {
        public static RescueProtocol Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isRescuing;
        
        // V25: Removed direct MonoBehaviour references (PlayerMovement, SleepPod)
        // Communication is now via EntityRegistry and StructureRegistry.

        private readonly WaitForSecondsRealtime blackoutWait = new WaitForSecondsRealtime(2f);
        private readonly WaitForSecondsRealtime botArrivalWait = new WaitForSecondsRealtime(1f);
        private readonly WaitForSecondsRealtime finishWait = new WaitForSecondsRealtime(0.5f);

        public event System.Action OnRescueTriggered;
        public event System.Action OnRescueComplete;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (PlayerVitals.Instance != null)
                PlayerVitals.Instance.OnOxygenDepleted += TriggerRescue;
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            // V25: References are now handled via Data Registries at runtime.
            // No scanning for MonoBehaviours in the scene.
        }

        public void TriggerRescue()
        {
            if (isRescuing) return;
            StartCoroutine(RescueSequence());
        }

        private IEnumerator RescueSequence()
        {
            isRescuing = true;
            OnRescueTriggered?.Invoke();
            ResolveReferences();

            // Phase 1: Blackout
            yield return blackoutWait;

            // Phase 2: Bot-E arrives at player via Emergency Override
            Vector3 playerPos = AstroPioneer.Data.EntityRegistry.Instance.GetEntityPosition("PLAYER_MAIN");
            
            if (AstroPioneer.Managers.BotManager.Instance != null && AstroPioneer.Managers.BotManager.Instance.TotalBotCount > 0)
            {
                var bot = AstroPioneer.Managers.BotManager.Instance.GetAllBots()[0];
                if (bot != null)
                    bot.ExecuteEmergencyRescue(playerPos, playerPos);
            }

            yield return botArrivalWait;

            // Phase 3: Return to ship
            if (ExplorationManager.Instance != null &&
                ExplorationManager.Instance.CurrentState == ExplorationState.Exploring)
            {
                ExplorationManager.Instance.ReturnToShip();

                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (!sceneName.Contains("SampleScene"))
                {
                    float timeout = Time.unscaledTime + 5f;
                    while (ExplorationManager.Instance.CurrentState != ExplorationState.OnShip && Time.unscaledTime <= timeout)
                        yield return null;
                }
            }

            // Phase 4: Teleport to SleepPod
            Vector2Int podGridPos = AstroPioneer.Data.StructureRegistry.Instance.GetSleepPodPosition();
            Vector3 podWorldPos = AstroPioneer.Managers.GridManager.Instance != null ? 
                                  AstroPioneer.Managers.GridManager.Instance.GridToWorldPosition(podGridPos) : 
                                  new Vector3(podGridPos.x, podGridPos.y, 0);

            // Teleport Player via EntityManager (Data-Driven Search)
            if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.EntityManager>(out var em))
            {
                foreach (var entity in em.GetAllEntities())
                {
                    if (entity.EntityID == "PLAYER_MAIN" && entity is MonoBehaviour mb)
                    {
                        mb.transform.position = podWorldPos;
                        break;
                    }
                }
            }

            // Teleport Bot
            if (AstroPioneer.Managers.BotManager.Instance != null && AstroPioneer.Managers.BotManager.Instance.TotalBotCount > 0)
            {
                var bot = AstroPioneer.Managers.BotManager.Instance.GetAllBots()[0];
                if (bot != null && bot is MonoBehaviour mb)
                {
                    mb.transform.position = podWorldPos + Vector3.down;
                    bot.ForceReset();
                }
            }

            PlayerVitals.Instance?.FullRestore();

            yield return finishWait;

            isRescuing = false;
            OnRescueComplete?.Invoke();
        }
    }
}
