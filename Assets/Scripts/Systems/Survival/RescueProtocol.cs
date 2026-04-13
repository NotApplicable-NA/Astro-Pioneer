using UnityEngine;
using System.Collections;
using AstroPioneer.Systems.Exploration;

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

            // Phase 1: Blackout
            yield return new WaitForSecondsRealtime(2f);

            // Phase 2: Bot-E arrives at player
            var playerMove = FindObjectOfType<AstroPioneer.Player.PlayerMovement>();
            var botE = FindObjectOfType<AstroPioneer.Machines.Automation.TransportBot>();

            if (playerMove != null && botE != null)
                botE.transform.position = playerMove.transform.position + Vector3.up;

            yield return new WaitForSecondsRealtime(1f);

            // Phase 3: Return to ship
            if (ExplorationManager.Instance != null &&
                ExplorationManager.Instance.CurrentState == ExplorationState.Exploring)
            {
                ExplorationManager.Instance.ReturnToShip();

                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (!sceneName.Contains("SampleScene"))
                {
                    float timeout = Time.unscaledTime + 5f;
                    yield return new WaitUntil(() =>
                        ExplorationManager.Instance.CurrentState == ExplorationState.OnShip ||
                        Time.unscaledTime > timeout);
                }
            }

            // Phase 4: Teleport to SleepPod
            var sleepPod = FindObjectOfType<AstroPioneer.Machines.SleepPod>();
            if (playerMove != null && sleepPod != null)
            {
                playerMove.transform.position = sleepPod.transform.position;
                if (botE != null)
                    botE.transform.position = sleepPod.transform.position + Vector3.down;
            }

            PlayerVitals.Instance?.FullRestore();

            yield return new WaitForSecondsRealtime(0.5f);

            isRescuing = false;
            OnRescueComplete?.Invoke();
        }
    }
}
