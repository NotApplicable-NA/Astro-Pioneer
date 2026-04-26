using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AstroPioneer.Data;
using AstroPioneer.Machines;
using AstroPioneer.Managers;
using AstroPioneer.Systems;

namespace AstroPioneer.Managers
{
// Removed legacy SaveData JSON structs. DOD architecture uses raw binary streams.

    /// <summary>
    /// SaveGameManager — Handles full game state serialization via PlayerPrefs.
    /// Hotkeys: K = Save, L = Load, Shift+K = Clear save data.
    /// </summary>
    public class SaveGameManager : MonoBehaviour
    {
        public static SaveGameManager Instance { get; private set; }

        // Registries have been moved to the V20 Data-Driven system (e.g. StructureRegistry).
        // SaveGameManager no longer manages asset lists directly.

        private const string SAVE_KEY = "AstroPioneer_SaveData";

        // Runtime flag persisted to meta.dat — true means starter kit already given this save slot
        private bool _starterItemsGiven = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Core.ServiceLocator.Register(this);
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Core.ServiceLocator.Unregister<SaveGameManager>(); }
        }

        void Start() => LoadGame();
        void OnApplicationQuit() => SaveGame();

        void Update()
        {
            // Shift+K: Full dev reset — clears memory, sets flag so next session gives starter items
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
            {
                _starterItemsGiven = false;
                AstroPioneer.Core.SaveSystem.DeleteAllSaves();
                PlayerPrefs.DeleteKey(SAVE_KEY);
                PlayerPrefs.Save();
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.ClearAndResetForNewGame();
                // Re-save meta immediately with _starterItemsGiven = false.
                // OnApplicationQuit will also call SaveGame(), but this ensures
                // the flag is written even if the user quits via Alt+F4 or Editor Stop.
                int seed = 0;
                if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.ChunkManager>(out var cm2))
                {
                    seed = cm2.WorldSeed;
                    cm2.ClearAllData(); // Force wipe memory chunks and visuals
                }
                if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.EntityManager>(out var em))
                {
                    em.ClearAllEntities();
                }
                
                if (AstroPioneer.Machines.Automation.BotSimulationManager.Instance != null)
                {
                    // Cast to List to use Clear(), since GetAllBots() returns IReadOnlyList
                    var bots = AstroPioneer.Machines.Automation.BotSimulationManager.Instance.GetAllBots() as System.Collections.Generic.List<AstroPioneer.Machines.Automation.BotData>;
                    if (bots != null) bots.Clear();
                }
                
                AstroPioneer.Core.SaveSystem.SaveMeta(seed, 1, 0, 0.25f, starterItemsGiven: false);
                Debug.Log("[SaveGameManager] Full reset complete. Starter items will be given on next session.");
                return;
            }

            if (Input.GetKeyDown(KeyCode.K)) SaveGame();
            if (Input.GetKeyDown(KeyCode.L)) LoadGame();
        }

        // ─────────────────────────── SAVE ───────────────────────────

        public void SaveGame()
        {
            // 1. Save Metadata (including starter flag)
            int seed = AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.ChunkManager>(out var cm) ? cm.WorldSeed : 0;
            int days = AstroPioneer.Managers.TimeManager.Instance != null ? AstroPioneer.Managers.TimeManager.Instance.DaysPassed : 1;
            int creds = AstroPioneer.Managers.CurrencyManager.Instance != null ? AstroPioneer.Managers.CurrencyManager.Instance.CurrentCredits : 0;
            float tod = AstroPioneer.Managers.TimeManager.Instance != null ? AstroPioneer.Managers.TimeManager.Instance.CurrentTime : 0.25f;

            AstroPioneer.Core.SaveSystem.SaveMeta(seed, days, creds, tod, _starterItemsGiven);

            // 2. V23: Flush ALL active machine states to ComplexState buffers BEFORE saving chunks.
            //    This guarantees data written even if the player saves without walking away.
            FlushAllMachineStates();

            // 3. Save Active Chunks (Grid structures, crops, utilities + ComplexStates)
            if (cm != null)
            {
                foreach (var chunk in cm.ActiveChunks.Values)
                {
                    if (chunk.IsDirty)
                        AstroPioneer.Core.SaveSystem.SaveChunkBinary(chunk, cm.WorldSeed, useAsync: false);
                }
            }

            // 3. Save Inventory
            if (InventoryManager.Instance != null)
            {
                AstroPioneer.Core.SaveSystem.SaveInventoryBinary(InventoryManager.Instance.Slots);
            }

            // 4. Save Entities (AgriMech, etc.)
            if (AstroPioneer.Core.ServiceLocator.TryGet<AstroPioneer.Core.EntityManager>(out var em) && AstroPioneer.Data.EntityRegistry.Instance != null)
            {
                AstroPioneer.Core.SaveSystem.SaveEntities(em.GetAllEntities(), AstroPioneer.Data.EntityRegistry.Instance);
            }

            // 5. Save Bots (Simulation Brain Data)
            if (AstroPioneer.Machines.Automation.BotSimulationManager.Instance != null)
            {
                AstroPioneer.Core.SaveSystem.SaveBotsBinary(AstroPioneer.Machines.Automation.BotSimulationManager.Instance);
            }

            Debug.Log("[SaveGameManager] Binary Game State Saved Successfully!");
        }

        public void LoadGame()
        {
            if (AstroPioneer.Core.SaveSystem.TryLoadMeta(out int seed, out int days, out int creds, out float tod, out bool starterGiven))
            {
                _starterItemsGiven = starterGiven;
                
                if (AstroPioneer.Managers.TimeManager.Instance != null)
                {
                    AstroPioneer.Managers.TimeManager.Instance.LoadTime(days, tod);
                }
                
                if (AstroPioneer.Managers.CurrencyManager.Instance != null)
                {
                    AstroPioneer.Managers.CurrencyManager.Instance.LoadCredits(creds);
                }
                
                Debug.Log($"[SaveGameManager] Loaded Meta: Seed={seed}, Day={days}, Credits={creds}, Time={tod}, StarterGiven={starterGiven}");
            }
            else
            {
                // No meta file = brand new game
                _starterItemsGiven = false;
                Debug.LogWarning("[SaveGameManager] No Save Meta found. Starting new game.");
            }

            // Give starter items if this is a new/reset game session
            if (!_starterItemsGiven && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.GiveStarterItems();
                _starterItemsGiven = true;
            }
            else if (_starterItemsGiven && InventoryManager.Instance != null)
            {
                // Load existing inventory
                AstroPioneer.Core.SaveSystem.TryLoadInventoryBinary(InventoryManager.Instance.Slots);
                // Trigger UI update
                InventoryManager.Instance.SetSlot(0, InventoryManager.Instance.Slots[0].item, InventoryManager.Instance.Slots[0].quantity); // Hack to trigger OnInventoryUpdated
            }

            // Chunks auto-load via ChunkManager as player moves.

            // Load Entities
            if (AstroPioneer.Data.EntityRegistry.Instance != null && AstroPioneer.Managers.ObjectPoolManager.Instance != null)
            {
                AstroPioneer.Core.SaveSystem.TryLoadEntities(AstroPioneer.Data.EntityRegistry.Instance, AstroPioneer.Managers.ObjectPoolManager.Instance);
            }

            // Load Bots (Simulation Brain Data)
            if (AstroPioneer.Machines.Automation.BotSimulationManager.Instance != null)
            {
                AstroPioneer.Core.SaveSystem.TryLoadBotsBinary(AstroPioneer.Machines.Automation.BotSimulationManager.Instance);
            }
        }

        // ─── V23: Machine State Flush ───

        /// <summary>
        /// Flush ALL active machine states (storage inventories, pump levels, etc.)
        /// from their GameObjects into Chunk.ComplexStates byte[] buffers.
        /// Called BEFORE saving chunks to disk.
        /// 
        /// This is the "Master Save" — it guarantees no data is lost
        /// regardless of whether chunks were unloaded or not.
        /// </summary>
        private void FlushAllMachineStates()
        {
            // Find all active ISavableMachine components in the scene
            var machines = FindObjectsOfType<MonoBehaviour>();
            int flushed = 0;

            foreach (var mb in machines)
            {
                if (mb is AstroPioneer.Interfaces.ISavableMachine savable)
                {
                    // Get grid position from MachineIDTag (all grid machines should have this)
                    var tag = mb.GetComponent<AstroPioneer.Systems.MachineIDTag>();
                    Vector2Int pos;
                    if (tag != null)
                    {
                        pos = tag.originGridPos;
                    }
                    else
                    {
                        pos = new Vector2Int(
                            Mathf.FloorToInt(mb.transform.position.x),
                            Mathf.FloorToInt(mb.transform.position.y));
                    }

                    byte[] buffer = GridManager.Instance?.GetOrAllocateComplexState(pos);
                    if (buffer != null)
                    {
                        savable.SaveState(new System.Span<byte>(buffer));
                        flushed++;
                    }
                }
            }

            if (flushed > 0)
                Debug.Log($"[SaveGameManager] Flushed {flushed} machine states to ComplexState buffers.");
        }
    }
}
