using UnityEngine;
using System;
using System.Collections.Generic;

namespace AstroPioneer.Core.Simulation
{
    /// <summary>
    /// ISimulatedSystem — Interface for any modular system that needs to run 
    /// on the deterministic background tick (Bots, Fluids, Power, etc.)
    /// </summary>
    public interface ISimulatedSystem
    {
        void OnTick();
        int Priority { get; } // V24.6: Lower = Runs earlier
    }

    /// <summary>
    /// SimulationMaster — The "Conductor" (V24.5).
    /// Manages the global heartbeat of the game data layer.
    /// Does not know HOW systems work, only WHEN they should work.
    /// </summary>
    public class SimulationMaster : MonoBehaviour
    {
        public static SimulationMaster Instance { get; private set; }

        [Header("Global Heartbeat")]
        [SerializeField] private float tickRate = 0.2f; // 5 ticks per second

        private readonly List<ISimulatedSystem> systems = new List<ISimulatedSystem>();
        private float tickTimer;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        public void RegisterSystem(ISimulatedSystem system)
        {
            if (!systems.Contains(system))
            {
                systems.Add(system);
                // V24.6: Sort by priority (Ascending)
                systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                Debug.Log($"[SimulationMaster] Registered {system.GetType().Name} with Priority {system.Priority}");
            }
        }

        public void UnregisterSystem(ISimulatedSystem system)
        {
            systems.Remove(system);
        }

        void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickRate)
            {
                tickTimer = 0;
                ExecuteGlobalTick();
            }
        }

        private void ExecuteGlobalTick()
        {
            // The Conductor shouts "TICK!" and every musician plays their part
            for (int i = 0; i < systems.Count; i++)
            {
                try
                {
                    systems[i].OnTick();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SimulationMaster] System {systems[i].GetType().Name} crashed during tick: {e.Message}");
                }
            }
        }

        public float TickRate => tickRate;
    }
}
