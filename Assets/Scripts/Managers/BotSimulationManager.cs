using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Core;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Pathfinding;

namespace AstroPioneer.Machines.Automation
{
    [System.Serializable]
    public class BotData
    {
        public string id;
        public int entityTypeID; // V24.9: Points to EntityRegistry
        public Vector2 currentPos;
        public ChunkCoord currentChunk; // V24.8: Track current chunk for visual binding
        public TransportState state = TransportState.Idle;
        public List<Vector3> path = new List<Vector3>();
        public int pathIndex = 0;
        
        public Vector2 sourcePos;
        public Vector2 targetPos;
        public ushort heldItemID = 0;
        
        public bool hasTask = false;
        public string stationID; // To which station it belongs
    }

    /// <summary>
    /// BotSimulationManager — The "Bot Logistics Specialist" (V24.8).
    /// Implements ISimulatedSystem to run on the global heartbeat.
    /// </summary>
    public class BotSimulationManager : MonoBehaviour, AstroPioneer.Core.Simulation.ISimulatedSystem
    {
        public static BotSimulationManager Instance { get; private set; }

        [Header("Simulation Config")]
        [SerializeField] private float moveSpeed = 3f;

        private List<BotData> simulatedBots = new List<BotData>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        void Start()
        {
            // Register ourselves to the Conductor
            if (AstroPioneer.Core.Simulation.SimulationMaster.Instance != null)
            {
                AstroPioneer.Core.Simulation.SimulationMaster.Instance.RegisterSystem(this);
            }
        }

        // ─── ISimulatedSystem Implementation ───

        public int Priority => 100; // Runs after infrastructure (Power/Fluid)

        public void OnTick()
        {
            // Pass 1: Process active bots (movement & interaction)
            foreach (var bot in simulatedBots)
            {
                if (!bot.hasTask) continue;
                UpdateBotLogic(bot);
            }

            // Pass 2: Re-dispatch idle bots (V25 — Brain-level dispatching)
            // This ensures bots continue receiving tasks even when BotStation 
            // MonoBehaviour is inactive (chunk unloaded).
            foreach (var bot in simulatedBots)
            {
                if (bot.hasTask) continue;
                TryRedispatch(bot);
            }
        }

        /// <summary>
        /// V25: Brain-level task dispatching for idle bots.
        /// Scans for source/sink machines near the bot's current position using
        /// simulation-safe APIs that work across unloaded chunks.
        /// </summary>
        private void TryRedispatch(BotData bot)
        {
            if (GridManager.Instance == null || StructureRegistry.Instance == null) return;

            int scanRadius = 8;
            Vector2Int center = new Vector2Int(Mathf.FloorToInt(bot.currentPos.x), Mathf.FloorToInt(bot.currentPos.y));

            Vector2Int sourcePos = Vector2Int.zero;
            Vector2Int sinkPos = Vector2Int.zero;
            bool foundSource = false;
            bool foundSink = false;

            for (int y = -scanRadius; y <= scanRadius && !(foundSource && foundSink); y++)
            {
                for (int x = -scanRadius; x <= scanRadius && !(foundSource && foundSink); x++)
                {
                    Vector2Int checkPos = center + new Vector2Int(x, y);
                    if (Vector2.Distance((Vector2)center, (Vector2)checkPos) > scanRadius) continue;

                    ushort structID = GridManager.Instance.GetStructureAtForSimulation(checkPos);
                    if (structID == AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) continue;

                    StructureData data = StructureRegistry.Instance.Get(structID);
                    if (data == null || data.category != StructureCategory.Machine || data.visualPrefab == null) continue;

                    if (!foundSource && data.visualPrefab.GetComponent<MachineWaterPump>() != null)
                    {
                        sourcePos = checkPos;
                        foundSource = true;
                    }
                    else if (!foundSink && data.visualPrefab.GetComponent<MachineStorage>() != null)
                    {
                        sinkPos = checkPos;
                        foundSink = true;
                    }
                }
            }

            if (foundSource && foundSink)
            {
                bot.sourcePos = (Vector2)sourcePos;
                bot.targetPos = (Vector2)sinkPos;
                bot.hasTask = true;
                bot.state = TransportState.Idle; // Will start moving on next tick
            }
        }

        private void UpdateBotLogic(BotData bot)
        {
            float tickRate = AstroPioneer.Core.Simulation.SimulationMaster.Instance != null ? 
                             AstroPioneer.Core.Simulation.SimulationMaster.Instance.TickRate : 0.2f;
                             
            switch (bot.state)
            {
                case TransportState.Idle:
                    // If has task but idle, start moving to source
                    if (bot.hasTask) StartMovingTo(bot, bot.sourcePos, TransportState.MovingToSource);
                    break;

                case TransportState.MovingToSource:
                    if (MoveStep(bot)) 
                        HandleInteraction(bot, true); // Arrived at source
                    break;

                case TransportState.MovingToTarget:
                    if (MoveStep(bot))
                        HandleInteraction(bot, false); // Arrived at target
                    break;
            }
        }

        private bool MoveStep(BotData bot)
        {
            if (bot.path == null || bot.path.Count == 0 || bot.pathIndex >= bot.path.Count) 
            {
                // If we have no path but are already at the destination, return true
                return true;
            }

            float tickRate = AstroPioneer.Core.Simulation.SimulationMaster.Instance != null ? 
                             AstroPioneer.Core.Simulation.SimulationMaster.Instance.TickRate : 0.2f;

            Vector2 targetWaypoint = (Vector2)bot.path[bot.pathIndex];
            
            // V24.10: Dynamic Obstacle Check (Deterministic)
            Vector2Int waypointCell = new Vector2Int(Mathf.FloorToInt(targetWaypoint.x), Mathf.FloorToInt(targetWaypoint.y));
            if (GridManager.Instance != null && GridManager.Instance.IsSolidAt(waypointCell))
            {
                // If this is the last waypoint (or close to it), bot is adjacent to target — consider arrived
                if (bot.pathIndex >= bot.path.Count - 1)
                    return true;
                    
                // Otherwise, something got placed mid-path. Recalculate.
                StartMovingTo(bot, bot.state == TransportState.MovingToSource ? bot.sourcePos : bot.targetPos, bot.state);
                return false; 
            }

            float dist = Vector2.Distance(bot.currentPos, targetWaypoint);
            float step = moveSpeed * tickRate;

            if (dist <= step)
            {
                bot.currentPos = targetWaypoint;
                bot.pathIndex++;
                if (bot.pathIndex >= bot.path.Count) return true;
            }
            else
            {
                bot.currentPos = Vector2.MoveTowards(bot.currentPos, targetWaypoint, step);
            }

            // V24.8: Update current chunk reference
            bot.currentChunk = ChunkCoord.FromWorldPos(bot.currentPos.x, bot.currentPos.y);

            return false;
        }

        public List<BotData> GetBotsInChunk(ChunkCoord coord)
        {
            return simulatedBots.FindAll(b => b.currentChunk.x == coord.x && b.currentChunk.y == coord.y);
        }

        private void HandleInteraction(BotData bot, bool isSource)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(isSource ? bot.sourcePos.x : bot.targetPos.x), 
                                              Mathf.FloorToInt(isSource ? bot.sourcePos.y : bot.targetPos.y));

            if (isSource)
            {
                // Logic to take from pump
                if (TryDataPickup(gridPos, out ushort itemID))
                {
                    bot.heldItemID = itemID;
                    StartMovingTo(bot, bot.targetPos, TransportState.MovingToTarget);
                }
                else
                {
                    // Source empty? Wait or cancel
                    bot.state = TransportState.Idle; 
                }
            }
            else
            {
                // Logic to drop into storage
                if (TryDataDropoff(gridPos, bot.heldItemID))
                {
                    bot.heldItemID = 0;
                    bot.hasTask = false;
                    bot.state = TransportState.Idle;
                }
            }
        }

        private void StartMovingTo(BotData bot, Vector2 target, TransportState nextState)
        {
            bot.state = nextState;
            bot.path.Clear();
            PathfindingManager.Instance.FindPath((Vector3)bot.currentPos, (Vector3)target, bot.path);
            bot.pathIndex = 0;

            if (bot.path.Count == 0)
            {
                // Check if we are already essentially at the target
                if (Vector2.Distance(bot.currentPos, target) < 1.1f)
                {
                    // Treat as arrived immediately next tick
                }
                else
                {
                    Debug.LogWarning($"[BotSimulationManager] Bot {bot.id} failed to find path to {target}. Target may be blocked.");
                }
            }
        }

        // ─── Data Layer Interactions (Directly modifying Chunk Data) ───

        private bool TryDataPickup(Vector2Int pos, out ushort itemID)
        {
            itemID = 0;
            if (GridManager.Instance == null || StructureRegistry.Instance == null) return false;

            // V25: Use simulation-safe API to access unloaded chunks
            ushort structID = GridManager.Instance.GetStructureAtForSimulation(pos);
            if (structID == AstroPioneer.Core.GameConstants.STRUCTURE_EMPTY) return false;

            StructureData data = StructureRegistry.Instance.Get(structID);
            if (data != null && data.visualPrefab != null)
            {
                var pump = data.visualPrefab.GetComponent<MachineWaterPump>();
                if (pump != null && pump.WaterItemData != null)
                {
                    if (AstroPioneer.Data.ItemRegistry.Instance != null)
                    {
                        itemID = AstroPioneer.Data.ItemRegistry.Instance.GetID(pump.WaterItemData);
                        if (itemID > 0) return true;
                    }
                }
            }
            return false;
        }

        private bool TryDataDropoff(Vector2Int pos, ushort itemID)
        {
            if (GridManager.Instance == null) return false;
            
            // V25: Use simulation-safe API to access unloaded chunks
            byte[] state = GridManager.Instance.GetOrAllocateComplexStateForSimulation(pos);
            if (state == null) return false;

            ushort slotCount = System.BitConverter.ToUInt16(state, 0);
            
            // If completely uninitialized, format it as a 9-slot storage
            if (slotCount == 0)
            {
                slotCount = 9;
                System.BitConverter.TryWriteBytes(new System.Span<byte>(state, 0, 2), slotCount);
            }

            int offset = 2;
            for (int i = 0; i < slotCount; i++)
            {
                if (offset + 6 > state.Length) break;
                
                ushort slotItemID = System.BitConverter.ToUInt16(state, offset);
                int qty = System.BitConverter.ToInt32(state, offset + 2);

                if (slotItemID == 0 || (slotItemID == itemID && qty < 64)) // Assuming 64 is max stack
                {
                    System.BitConverter.TryWriteBytes(new System.Span<byte>(state, offset, 2), itemID);
                    System.BitConverter.TryWriteBytes(new System.Span<byte>(state, offset + 2, 4), qty + 1);
                    // V25: Use simulation-safe dirty marking
                    GridManager.Instance.MarkChunkDirtyForSimulation(pos);
                    return true;
                }
                offset += 6;
            }

            return false; 
        }

        // ─── Public API ───

        public BotData RegisterBot(string id, Vector2 startPos, int entityTypeID)
        {
            var data = new BotData { id = id, currentPos = startPos, entityTypeID = entityTypeID };
            data.currentChunk = AstroPioneer.Core.ChunkCoord.FromWorldPos(startPos.x, startPos.y);
            simulatedBots.Add(data);
            return data;
        }

        public BotData GetBotData(string id)
        {
            return simulatedBots.Find(b => b.id == id);
        }

        public void UnregisterBot(string id)
        {
            simulatedBots.RemoveAll(b => b.id == id);
        }

        public IReadOnlyList<BotData> GetAllBots()
        {
            return simulatedBots;
        }

        // ─── Data Persistence ───

        public void SerializeBots(System.IO.BinaryWriter writer)
        {
            writer.Write((ushort)simulatedBots.Count);
            foreach (var bot in simulatedBots)
            {
                writer.Write(bot.id ?? "");
                writer.Write(bot.entityTypeID);
                writer.Write(bot.currentPos.x);
                writer.Write(bot.currentPos.y);
                writer.Write(bot.sourcePos.x);
                writer.Write(bot.sourcePos.y);
                writer.Write(bot.targetPos.x);
                writer.Write(bot.targetPos.y);
                writer.Write(bot.heldItemID);
                writer.Write(bot.hasTask);
                writer.Write((byte)bot.state);
                writer.Write(bot.stationID ?? "");
                
                // Save Path
                ushort pathCount = bot.path != null ? (ushort)bot.path.Count : (ushort)0;
                writer.Write(pathCount);
                if (bot.path != null)
                {
                    foreach (var p in bot.path)
                    {
                        writer.Write(p.x);
                        writer.Write(p.y);
                    }
                }
                writer.Write(bot.pathIndex);
            }
        }

        public void DeserializeBots(System.IO.BinaryReader reader)
        {
            simulatedBots.Clear();
            ushort count = reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                var bot = new BotData();
                bot.id = reader.ReadString();
                bot.entityTypeID = reader.ReadInt32();
                bot.currentPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                bot.sourcePos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                bot.targetPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                bot.heldItemID = reader.ReadUInt16();
                bot.hasTask = reader.ReadBoolean();
                bot.state = (TransportState)reader.ReadByte();
                bot.stationID = reader.ReadString();
                
                // Load Path
                ushort pathCount = reader.ReadUInt16();
                bot.path = new List<Vector3>();
                for(int j = 0; j < pathCount; j++)
                {
                    bot.path.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0));
                }
                bot.pathIndex = reader.ReadInt32();
                
                bot.currentChunk = AstroPioneer.Core.ChunkCoord.FromWorldPos(bot.currentPos.x, bot.currentPos.y);
                simulatedBots.Add(bot);
            }
        }
    }
}
