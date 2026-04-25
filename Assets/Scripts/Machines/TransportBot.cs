using UnityEngine;
using System.Collections;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Pathfinding;
using AstroPioneer.Core;

namespace AstroPioneer.Machines.Automation
{
    public enum TransportState { Idle, MovingToSource, PickingUp, MovingToTarget, DroppingOff }

    /// <summary>
    /// TransportBot V24.11 — Pure Visual Chaser (Wayang).
    /// 
    /// HUKUM ARSITEKTUR:
    /// - Wayang ini TIDAK PERNAH membuat data baru di BotSimulationManager.
    /// - Ia hanya MENCARI data yang sudah ada berdasarkan ID.
    /// - Jika data tidak ditemukan, wayang ini MENGHANCURKAN DIRINYA SENDIRI.
    /// - Penciptaan data bot HANYA dilakukan melalui PlacementManager (Pintu Depan).
    /// </summary>
    [RequireComponent(typeof(BotController))]
    public class TransportBot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string botID;
        
        private BotData data;
        private BotController botController;
        private Animator animator;

        // Animator Hashes
        private static readonly int PickupHash = Animator.StringToHash("Pickup");
        private static readonly int DropoffHash = Animator.StringToHash("Dropoff");
        private static readonly int IsCarryingHash = Animator.StringToHash("IsCarrying");

        public string BotID => botID;
        public Vector3 WorldPosition => transform.position;
        public bool IsAvailable => data != null && !data.hasTask;
        public BotStation HomeStation { get; private set; }

        void Awake()
        {
            botController = GetComponent<BotController>();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Bind this visual to an existing BotData in the simulation brain.
        /// Called by ChunkRenderer when spawning visuals for bots in loaded chunks.
        /// This method ONLY looks up data — it NEVER creates new data.
        /// </summary>
        public void BindToSimulation(string id)
        {
            botID = id;
            if (BotSimulationManager.Instance != null)
            {
                data = BotSimulationManager.Instance.GetBotData(id);
            }

            if (data == null)
            {
                Debug.LogWarning($"[TransportBot] Wayang tanpa jiwa! No BotData found for '{id}'. Destroying visual.");
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Wayang HARUS sudah di-bind sebelum Start().
            // Jika belum, coba lookup terakhir. Jika gagal = mati.
            if (data == null && !string.IsNullOrEmpty(botID))
            {
                BindToSimulation(botID);
            }
            else if (data == null)
            {
                Debug.LogWarning("[TransportBot] Wayang tanpa jiwa! No botID assigned. Destroying visual.");
                Destroy(gameObject);
            }
        }

        // ─── Emergency & Rescue System (V24.9) ───

        public void ExecuteEmergencyRescue(Vector3 rescuePos, Vector3 targetPos)
        {
            if (data != null)
            {
                data.currentPos = targetPos;
                data.path.Clear();
                data.hasTask = false;
                data.state = TransportState.Idle;
            }
            transform.position = targetPos;
            Debug.Log($"[TransportBot] Emergency Rescue executed for {botID}");
        }

        public void ForceReset()
        {
            if (data != null)
            {
                data.path.Clear();
                data.hasTask = false;
                data.state = TransportState.Idle;
            }
            Debug.Log($"[TransportBot] Force Reset executed for {botID}");
        }

        void Update()
        {
            if (data == null) return;

            // 1. Follow the simulation position (smooth visual lerp)
            transform.position = Vector3.Lerp(transform.position, (Vector3)data.currentPos, Time.deltaTime * 10f);

            // 2. Sync Animations based on Simulation State
            SyncVisuals();
        }

        private void SyncVisuals()
        {
            if (animator == null) return;

            bool isCarrying = data.heldItemID != 0;
            animator.SetBool(IsCarryingHash, isCarrying);
        }

        public void SetHomeStation(BotStation station) => HomeStation = station;
    }
}
