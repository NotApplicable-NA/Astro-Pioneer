using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Core;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Machines;

namespace AstroPioneer.Machines.Automation
{
    /// <summary>
    /// BotStation V24.9 — Fully Data-Driven.
    /// 
    /// ZERO Physics2D usage. All scanning is done through GridManager (data layer).
    /// Works even when machines/bots are in unloaded chunks.
    /// </summary>
    public class BotStation : MonoBehaviour
    {
        [Header("Station Config")]
        [SerializeField] private float scanRadius = 8f;
        [SerializeField] private float scanInterval = 3f;

        // V24.9: Only IDs and coordinates. No MonoBehaviour references.
        private readonly List<string> assignedBotIDs = new List<string>();
        private readonly List<Vector2Int> foundSources = new List<Vector2Int>(16);
        private readonly List<Vector2Int> foundSinks = new List<Vector2Int>(16);

        void Start()
        {
            StartCoroutine(ScanLoop());
        }

        private IEnumerator ScanLoop()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, scanInterval));
            while (true)
            {
                AdoptNearbyBots();
                ScanAndDispatch();
                yield return new WaitForSeconds(scanInterval);
            }
        }

        // ─── V24.9: Data-Driven Bot Adoption (No Physics) ───

        private void AdoptNearbyBots()
        {
            if (BotSimulationManager.Instance == null) return;

            // Query ALL simulated bots from the data layer
            // and adopt any within our scan radius based on their DATA position
            var allBots = BotSimulationManager.Instance.GetAllBots();
            Vector2 stationPos = (Vector2)transform.position;

            for (int i = 0; i < allBots.Count; i++)
            {
                var bot = allBots[i];
                if (assignedBotIDs.Contains(bot.id)) continue;

                // Check distance using DATA coordinates, not Physics
                if (Vector2.Distance(stationPos, bot.currentPos) <= scanRadius)
                {
                    assignedBotIDs.Add(bot.id);
                    bot.stationID = gameObject.name;
                    Debug.Log($"[BotStation] Data-Adopted bot: {bot.id}");
                }
            }
        }

        // ─── V24.9: Data-Driven Machine Scan (No Physics) ───

        private void ScanAndDispatch()
        {
            if (BotSimulationManager.Instance == null) return;

            // 1. Find an idle bot from our adopted list
            BotData idleBot = null;
            for (int i = assignedBotIDs.Count - 1; i >= 0; i--)
            {
                var data = BotSimulationManager.Instance.GetBotData(assignedBotIDs[i]);
                if (data == null) { assignedBotIDs.RemoveAt(i); continue; }
                if (!data.hasTask) { idleBot = data; break; }
            }

            if (idleBot == null) return;

            // 2. Scan GridManager data for machines (No Physics!)
            foundSources.Clear();
            foundSinks.Clear();

            int radius = Mathf.CeilToInt(scanRadius);
            Vector2Int center = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y));

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector2Int checkPos = center + new Vector2Int(x, y);
                    if (Vector2.Distance((Vector2)center, (Vector2)checkPos) > scanRadius) continue;

                    ClassifyMachineAt(checkPos);
                }
            }

            // 3. Dispatch task using pure coordinates
            if (foundSources.Count > 0 && foundSinks.Count > 0)
            {
                Vector2Int sourcePos = foundSources[0];
                Vector2Int sinkPos = foundSinks[0];

                Debug.Log($"[BotStation] Dispatching {idleBot.id} to {sourcePos} -> {sinkPos}");

                idleBot.sourcePos = (Vector2)sourcePos;
                idleBot.targetPos = (Vector2)sinkPos;
                idleBot.hasTask = true;
                idleBot.state = TransportState.Idle;
            }
        }

        /// <summary>
        /// V24.9: Identifies machine type purely from data.
        /// Reads StructureID from GridManager → looks up StructureData from Registry
        /// → checks the PREFAB ASSET (always in memory) for component type.
        /// </summary>
        private void ClassifyMachineAt(Vector2Int pos)
        {
            if (GridManager.Instance == null || StructureRegistry.Instance == null) return;

            ushort structID = GridManager.Instance.GetStructureAt(pos);
            if (structID == GameConstants.STRUCTURE_EMPTY) return;

            StructureData data = StructureRegistry.Instance.Get(structID);
            if (data == null || data.category != StructureCategory.Machine) return;
            if (data.visualPrefab == null) return;

            // Check the PREFAB asset (not a scene instance — always valid)
            if (data.visualPrefab.GetComponent<MachineWaterPump>() != null)
                foundSources.Add(pos);
            else if (data.visualPrefab.GetComponent<MachineStorage>() != null)
                foundSinks.Add(pos);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
    }
}
