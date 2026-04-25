using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Interfaces;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// PowerManager — Distributes power from generators to consumers
    /// using a wireless range-based model each frame.
    /// </summary>
    public class PowerManager : MonoBehaviour
    {
        public static PowerManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        [SerializeField] private float hysteresisDuration = 2.0f; // Seconds machines stay on if power drops briefly

        private readonly List<IPowerGenerator> generators = new List<IPowerGenerator>();
        private readonly List<IPowerConsumer> consumers = new List<IPowerConsumer>();

        // Reusable per-frame budget & hysteresis tracking
        private readonly Dictionary<IPowerGenerator, float> remainingPower = new Dictionary<IPowerGenerator, float>();
        private readonly Dictionary<IPowerConsumer, float> consumerGraceTimers = new Dictionary<IPowerConsumer, float>();

        // V19: True Round-Robin state
        private int _consumerTickIndex = 0;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; ServiceLocator.Unregister<PowerManager>(); }
        }


        void Update()
        {
            // V19: True Round-Robin Ticking
            // Process exactly ~5% of consumers per frame, guaranteed even load.
            if (consumers.Count == 0) return;

            RebuildGeneratorBudget();

            int batchSize = Mathf.Max(1, consumers.Count / 20);
            for (int i = 0; i < batchSize; i++)
            {
                if (_consumerTickIndex >= consumers.Count)
                    _consumerTickIndex = 0;

                ProcessConsumer(consumers[_consumerTickIndex]);
                _consumerTickIndex++;
            }

            if (_consumerTickIndex >= consumers.Count)
                _consumerTickIndex = 0;
        }

        public void RegisterGenerator(IPowerGenerator gen)
        {
            if (!generators.Contains(gen)) generators.Add(gen);
        }

        public void UnregisterGenerator(IPowerGenerator gen) => generators.Remove(gen);

        public void RegisterConsumer(IPowerConsumer con)
        {
            if (!consumers.Contains(con)) consumers.Add(con);
        }

        public void UnregisterConsumer(IPowerConsumer con)
        {
            consumers.Remove(con);
            consumerGraceTimers.Remove(con);
            // V19: Clamp index to prevent out-of-bounds after list shrinks
            if (consumers.Count > 0)
                _consumerTickIndex = _consumerTickIndex % consumers.Count;
            else
                _consumerTickIndex = 0;
        }

        private void RebuildGeneratorBudget()
        {
            remainingPower.Clear();
            foreach (var gen in generators)
            {
                if (gen != null && gen.IsActive)
                    remainingPower[gen] = gen.PowerProduction;
            }
        }

        private void ProcessConsumer(IPowerConsumer consumer)
        {
            if (consumer == null) return;

            float needed = consumer.PowerRequired;
            float received = 0f;

            // Pass 1: True Generators (non-batteries)
            foreach (var gen in generators)
            {
                if (needed <= 0) break;
                if (gen == null || !gen.IsActive || gen is AstroPioneer.Machines.MachineBattery) continue;
                if (!remainingPower.TryGetValue(gen, out float available) || available <= 0) continue;

                float range = gen.PowerRange;
                if ((consumer.Position - gen.Position).sqrMagnitude > range * range) continue;

                float take = Mathf.Min(needed, available);
                gen.OnPowerProvided(take);
                remainingPower[gen] -= take;
                received += take;
                needed -= take;
            }

            // Pass 2: Batteries (acting as generators)
            if (needed > 0)
            {
                foreach (var gen in generators)
                {
                    if (needed <= 0) break;
                    if (gen == null || !gen.IsActive || !(gen is AstroPioneer.Machines.MachineBattery)) continue;
                    if (System.Object.ReferenceEquals(consumer, gen)) continue; // Avoid self-charging loop

                    if (!remainingPower.TryGetValue(gen, out float available) || available <= 0) continue;

                    float range = gen.PowerRange;
                    if ((consumer.Position - gen.Position).sqrMagnitude > range * range) continue;

                    float take = Mathf.Min(needed, available);
                    gen.OnPowerProvided(take);
                    remainingPower[gen] -= take;
                    received += take;
                    needed -= take;
                }
            }

            // Hysteresis: machine stays 'on' up to hysteresisDuration after power drops
            if (received >= consumer.PowerRequired)
            {
                consumerGraceTimers[consumer] = Time.time;
            }
            else if (consumerGraceTimers.TryGetValue(consumer, out float lastPoweredTime))
            {
                if (Time.time - lastPoweredTime < hysteresisDuration)
                    received = consumer.PowerRequired;
            }

            consumer.ReceivePower(received);
        }

        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            foreach (var gen in generators)
            {
                if (gen == null) continue;
                Gizmos.color = gen.IsActive ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(gen.Position, gen.PowerRange);
            }

            Gizmos.color = Color.yellow;
            foreach (var consumer in consumers)
            {
                if (consumer == null || !consumer.IsPowered) continue;

                float nearestDistSq = float.MaxValue;
                IPowerGenerator nearestGen = null;

                foreach (var gen in generators)
                {
                    if (gen == null || !gen.IsActive) continue;
                    float dSq = (consumer.Position - gen.Position).sqrMagnitude;
                    if (dSq <= gen.PowerRange * gen.PowerRange && dSq < nearestDistSq)
                    {
                        nearestDistSq = dSq;
                        nearestGen = gen;
                    }
                }

                if (nearestGen != null)
                    Gizmos.DrawLine(nearestGen.Position, consumer.Position);
            }
        }
    }
}
