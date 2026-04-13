using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Interfaces;

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

        private readonly List<IPowerGenerator> generators = new List<IPowerGenerator>();
        private readonly List<IPowerConsumer> consumers = new List<IPowerConsumer>();

        // Reusable per-frame budget to avoid GC allocation
        private readonly Dictionary<IPowerGenerator, float> remainingPower = new Dictionary<IPowerGenerator, float>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update() => DistributePower();

        public void RegisterGenerator(IPowerGenerator gen)
        {
            if (!generators.Contains(gen)) generators.Add(gen);
        }

        public void UnregisterGenerator(IPowerGenerator gen) => generators.Remove(gen);

        public void RegisterConsumer(IPowerConsumer con)
        {
            if (!consumers.Contains(con)) consumers.Add(con);
        }

        public void UnregisterConsumer(IPowerConsumer con) => consumers.Remove(con);

        private void DistributePower()
        {
            // Build per-generator power budget
            remainingPower.Clear();
            foreach (var gen in generators)
            {
                if (gen != null && gen.IsActive)
                    remainingPower[gen] = gen.PowerProduction;
            }

            // Distribute to consumers
            foreach (var consumer in consumers)
            {
                if (consumer == null) continue;

                float needed = consumer.PowerRequired;
                float received = 0f;

                foreach (var gen in generators)
                {
                    if (needed <= 0) break;
                    if (gen == null || !gen.IsActive) continue;
                    if (!remainingPower.TryGetValue(gen, out float available) || available <= 0) continue;
                    if (Vector3.Distance(consumer.Position, gen.Position) > gen.PowerRange) continue;

                    float take = Mathf.Min(needed, available);
                    gen.OnPowerProvided(take);
                    remainingPower[gen] -= take;
                    received += take;
                    needed -= take;
                }

                consumer.ReceivePower(received);
            }
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

                float nearestDist = float.MaxValue;
                IPowerGenerator nearestGen = null;

                foreach (var gen in generators)
                {
                    if (gen == null || !gen.IsActive) continue;
                    float d = Vector3.Distance(consumer.Position, gen.Position);
                    if (d <= gen.PowerRange && d < nearestDist)
                    {
                        nearestDist = d;
                        nearestGen = gen;
                    }
                }

                if (nearestGen != null)
                    Gizmos.DrawLine(nearestGen.Position, consumer.Position);
            }
        }
    }
}
