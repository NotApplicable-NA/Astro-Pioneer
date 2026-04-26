using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System.Buffers;
using System.Collections.Concurrent;

namespace AstroPioneer.Core
{
    /// <summary>
    /// SaveSystem — Binary serialization for chunk data and world metadata.
    /// Memory Hardened Version (V17.5): Using ArrayPool for snapshots and async error queue.
    /// </summary>
    public static class SaveSystem
    {
        private static string SaveRoot;
        private static string ChunkDir;
        private static string MetaPath;
        private static string EntitiesPath;
        private static string InventoryPath;

        // Async Error Reporting
        private static readonly ConcurrentQueue<string> saveErrors = new ConcurrentQueue<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePaths()
        {
            SaveRoot = Path.Combine(Application.persistentDataPath, GameConstants.SAVE_DIR);
            ChunkDir = Path.Combine(SaveRoot, GameConstants.SAVE_CHUNK_DIR);
            MetaPath = Path.Combine(SaveRoot, GameConstants.SAVE_META_FILE);
            EntitiesPath = Path.Combine(SaveRoot, GameConstants.SAVE_ENTITY_FILE);
            InventoryPath = Path.Combine(SaveRoot, "inventory.dat");
        }

        // ─── Error Reporting API ───

        /// <summary>
        /// Attempts to retrieve the next pending save error.
        /// Call this on the Main Thread (e.g., from UIManager) to show pop-up warnings.
        /// </summary>
        public static bool TryGetNextSaveError(out string errorMessage)
        {
            return saveErrors.TryDequeue(out errorMessage);
        }

        // ─── Meta & Entities (Binary) ───

        public static void SaveMeta(int worldSeed, int daysPassed, int credits, float timeOfDay, bool starterItemsGiven)
        {
            Directory.CreateDirectory(SaveRoot);
            try
            {
                using (var fs = new FileStream(MetaPath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(worldSeed);
                    bw.Write(daysPassed);
                    bw.Write(credits);
                    bw.Write(timeOfDay);
                    bw.Write(starterItemsGiven); // Appended last for backward compatibility
                }
            }
            catch (System.Exception e)
            {
                saveErrors.Enqueue($"Meta Save Failed: {e.Message}");
            }
        }

        public static bool TryLoadMeta(out int worldSeed, out int daysPassed, out int credits, out float timeOfDay, out bool starterItemsGiven)
        {
            worldSeed = 0; daysPassed = 1; credits = 0; timeOfDay = 0.25f;
            starterItemsGiven = false;
            if (!File.Exists(MetaPath)) return false;

            using (var fs = new FileStream(MetaPath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                worldSeed = br.ReadInt32();
                daysPassed = br.ReadInt32();
                credits = br.ReadInt32();
                timeOfDay = br.ReadSingle();
                // Backward compatibility: older saves without this field default to true
                // (assume starter was given so returning players don't get double items)
                starterItemsGiven = (fs.Length > fs.Position) ? br.ReadBoolean() : true;
            }
            return true;
        }

        // ─── Inventory (Binary) ───

        public static void SaveInventoryBinary(System.Collections.Generic.List<AstroPioneer.Data.InventorySlot> slots)
        {
            Directory.CreateDirectory(SaveRoot);
            try
            {
                using (var fs = new FileStream(InventoryPath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write((ushort)slots.Count);
                    for (int i = 0; i < slots.Count; i++)
                    {
                        var slot = slots[i];
                        if (slot.IsEmpty || slot.item == null || AstroPioneer.Data.ItemRegistry.Instance == null)
                        {
                            bw.Write((ushort)0); // Item ID 0 = Empty
                            bw.Write((int)0);    // Quantity
                        }
                        else
                        {
                            ushort itemID = AstroPioneer.Data.ItemRegistry.Instance.GetID(slot.item);
                            bw.Write(itemID);
                            bw.Write(slot.quantity);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                saveErrors.Enqueue($"Inventory Save Failed: {e.Message}");
            }
        }

        public static bool TryLoadInventoryBinary(System.Collections.Generic.List<AstroPioneer.Data.InventorySlot> slots)
        {
            if (!File.Exists(InventoryPath)) return false;

            try
            {
                using (var fs = new FileStream(InventoryPath, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    ushort count = br.ReadUInt16();
                    int maxIter = Mathf.Min(count, slots.Count);
                    for (int i = 0; i < maxIter; i++)
                    {
                        ushort itemID = br.ReadUInt16();
                        int quantity = br.ReadInt32();

                        if (itemID == 0 || AstroPioneer.Data.ItemRegistry.Instance == null)
                        {
                            slots[i].Clear();
                        }
                        else
                        {
                            var item = AstroPioneer.Data.ItemRegistry.Instance.Get(itemID);
                            if (item != null)
                            {
                                slots[i].item = item;
                                slots[i].quantity = quantity;
                            }
                            else
                            {
                                slots[i].Clear(); // Invalid item ID
                            }
                        }
                    }
                    // Clear remaining slots if save had fewer slots
                    for (int i = maxIter; i < slots.Count; i++)
                    {
                        slots[i].Clear();
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Inventory Load Failed: {e.Message}");
                return false;
            }
        }

        // ─── Entities (Binary) ───

        public static void SaveEntities(System.Collections.Generic.IReadOnlyList<AstroPioneer.Interfaces.IEntity> entities, AstroPioneer.Data.EntityRegistry registry)
        {
            Directory.CreateDirectory(SaveRoot);
            try
            {
                using (var fs = new FileStream(EntitiesPath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write((ushort)entities.Count);
                    foreach (var entity in entities)
                    {
                        ushort typeID = (ushort)entity.EntityTypeID;
                        bw.Write(typeID);

                        // We need to write the length of the state data so we can skip it if the prefab is missing during load
                        long lengthPos = bw.BaseStream.Position;
                        bw.Write((ushort)0); // Placeholder for length
                        
                        long dataStart = bw.BaseStream.Position;
                        entity.SerializeState(bw); // Zero allocation write
                        long dataEnd = bw.BaseStream.Position;

                        ushort stateLength = (ushort)(dataEnd - dataStart);

                        // Seek back and write the actual length
                        bw.BaseStream.Position = lengthPos;
                        bw.Write(stateLength);
                        bw.BaseStream.Position = dataEnd;
                    }
                }
            }
            catch (System.Exception e)
            {
                saveErrors.Enqueue($"Entity Save Failed: {e.Message}");
            }
        }

        public static bool TryLoadEntities(AstroPioneer.Data.EntityRegistry registry, AstroPioneer.Managers.ObjectPoolManager pool)
        {
            if (!File.Exists(EntitiesPath)) return false;

            try
            {
                using (var fs = new FileStream(EntitiesPath, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    ushort count = br.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        ushort typeID = br.ReadUInt16();
                        ushort stateLength = br.ReadUInt16();

                        GameObject prefab = registry.GetPrefab(typeID);
                        if (prefab == null)
                        {
                            // Unknown entity type or removed from game, skip its data
                            br.BaseStream.Position += stateLength;
                            continue;
                        }

                        // Use ObjectPoolManager to avoid Instantiate GC spikes
                        GameObject obj = pool.SpawnFromPool(prefab.name, prefab, Vector3.zero);
                        var entity = obj.GetComponent<AstroPioneer.Interfaces.IEntity>();
                        if (entity != null)
                        {
                            long dataStart = br.BaseStream.Position;
                            entity.DeserializeState(br);
                            
                            // Ensure we consume exactly stateLength bytes, even if DeserializeState read fewer
                            long readBytes = br.BaseStream.Position - dataStart;
                            if (readBytes < stateLength)
                            {
                                br.BaseStream.Position += (stateLength - readBytes);
                            }
                        }
                        else
                        {
                            br.BaseStream.Position += stateLength;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Entity Load Failed: {e.Message}");
                return false;
            }
        }

        // ─── Bot Simulation Save (Binary) ───
        
        private static string BotsPath => Path.Combine(SaveRoot, "bots.dat");

        public static void SaveBotsBinary(AstroPioneer.Machines.Automation.BotSimulationManager manager)
        {
            Directory.CreateDirectory(SaveRoot);
            try
            {
                using (var fs = new FileStream(BotsPath, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    manager.SerializeBots(bw);
                }
            }
            catch (System.Exception e)
            {
                saveErrors.Enqueue($"Bot Save Failed: {e.Message}");
            }
        }

        public static bool TryLoadBotsBinary(AstroPioneer.Machines.Automation.BotSimulationManager manager)
        {
            if (!File.Exists(BotsPath)) return false;

            try
            {
                using (var fs = new FileStream(BotsPath, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    manager.DeserializeBots(br);
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Bot Load Failed: {e.Message}");
                return false;
            }
        }

        // ─── Chunk Save (Binary) ───

        /// <summary>
        /// Save a single chunk to disk using BinaryWriter.
        /// Atomic Snapshot: Uses ArrayPool for Zero-GC and offloads I/O to Task.Run.
        /// </summary>
        public static void SaveChunkBinary(Chunk chunk, int worldSeed, bool useAsync = true)
        {
            Directory.CreateDirectory(ChunkDir);
            string path = Path.Combine(ChunkDir, chunk.Coord.ToFileName());
            chunk.IsDirty = false;

            int totalCells = GameConstants.CHUNK_SIZE * GameConstants.CHUNK_SIZE;

            // Use ArrayPool to eliminate GC heap churn
            var poolUshort = ArrayPool<ushort>.Shared;
            var poolByte = ArrayPool<byte>.Shared;
            var poolBool = ArrayPool<bool>.Shared;

            ushort[] floorSnap = poolUshort.Rent(totalCells);
            ushort[] utilSnap = poolUshort.Rent(totalCells);
            ushort[] structSnap = poolUshort.Rent(totalCells);
            byte[] metaSnap = poolByte.Rent(totalCells);
            bool[] shadowSnap = poolBool.Rent(totalCells);
            bool[] litSnap = poolBool.Rent(totalCells);
            bool[] exploresSnap = poolBool.Rent(totalCells);

            System.Array.Copy(chunk.FloorLayer.GetRawArray(), floorSnap, totalCells);
            System.Array.Copy(chunk.UtilityLayer.GetRawArray(), utilSnap, totalCells);
            System.Array.Copy(chunk.StructureLayer.GetRawArray(), structSnap, totalCells);
            System.Array.Copy(chunk.MetadataLayer.GetRawArray(), metaSnap, totalCells);
            System.Array.Copy(chunk.ShadowLayer.GetRawArray(), shadowSnap, totalCells);
            System.Array.Copy(chunk.LitLayer.GetRawArray(), litSnap, totalCells);
            System.Array.Copy(chunk.ExploredLayer.GetRawArray(), exploresSnap, totalCells);

            // Clone ComplexStates (Pre-cloned for isolation)
            var complexStatesSnap = new System.Collections.Generic.Dictionary<Vector2Int, byte[]>();
            foreach (var kvp in chunk.ComplexStates)
            {
                byte[] cloned = new byte[kvp.Value.Length];
                System.Array.Copy(kvp.Value, cloned, kvp.Value.Length);
                complexStatesSnap[kvp.Key] = cloned;
            }

            int cx = chunk.Coord.x;
            int cy = chunk.Coord.y;
            bool isGen = chunk.IsGenerated;

            System.Action saveAction = () =>
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create))
                    using (var bw = new BinaryWriter(fs))
                    {
                        bw.Write(worldSeed);
                        bw.Write(cx);
                        bw.Write(cy);
                        bw.Write(isGen);

                        for (int i = 0; i < totalCells; i++) bw.Write(floorSnap[i]);
                        for (int i = 0; i < totalCells; i++) bw.Write(utilSnap[i]);
                        for (int i = 0; i < totalCells; i++) bw.Write(structSnap[i]);
                        bw.Write(metaSnap, 0, totalCells);

                        WriteBoolArraySnapshot(bw, shadowSnap, totalCells);
                        WriteBoolArraySnapshot(bw, litSnap, totalCells);
                        WriteBoolArraySnapshot(bw, exploresSnap, totalCells);

                        bw.Write(complexStatesSnap.Count);
                        foreach (var kvp in complexStatesSnap)
                        {
                            bw.Write((short)kvp.Key.x);
                            bw.Write((short)kvp.Key.y);
                            
                            // AAA Hotfix V18.1: ArrayPool Safety
                            // Renting from ArrayPool often returns a buffer LARGER than requested.
                            // We MUST write exactly the buffer size, NOT kvp.Value.Length,
                            // to avoid writing 'dirty' junk memory from previous usage.
                            int fixedSize = GameConstants.MACHINE_STATE_BUFFER_SIZE;
                            bw.Write(fixedSize);
                            bw.Write(kvp.Value, 0, fixedSize);
                        }

                        // V26: Append LastSimulatedDay at the end
                        bw.Write(chunk.LastSimulatedDay);
                    }
                }
                catch (System.Exception e)
                {
                    string msg = $"Save Failed: Chunk {cx},{cy}. {e.Message}";
                    Debug.LogError($"[SaveSystem] {msg}");
                    saveErrors.Enqueue(msg);
                }
                finally
                {
                    // MUST return buffers to the shared pool
                    poolUshort.Return(floorSnap);
                    poolUshort.Return(utilSnap);
                    poolUshort.Return(structSnap);
                    poolByte.Return(metaSnap);
                    poolBool.Return(shadowSnap);
                    poolBool.Return(litSnap);
                    poolBool.Return(exploresSnap);
                }
            }; // End of saveAction

            // 2. Execute Action (Async for gameplay, Sync for quitting)
            if (useAsync)
            {
                Task.Run(saveAction);
            }
            else
            {
                saveAction();
            }
        }

        /// <summary>
        /// Load a chunk from disk. Returns null if file doesn't exist.
        /// </summary>
        public static Chunk LoadChunkBinary(ChunkCoord coord, int expectedSeed)
        {
            string path = Path.Combine(ChunkDir, coord.ToFileName());
            if (!File.Exists(path)) return null;

            using (var fs = new FileStream(path, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                // Header
                int fileSeed = br.ReadInt32();
                int cx = br.ReadInt32();
                int cy = br.ReadInt32();
                bool isGenerated = br.ReadBoolean();

                // Seed mismatch = world was regenerated, discard old chunk
                if (fileSeed != expectedSeed) return null;

                var chunk = new Chunk(new ChunkCoord(cx, cy));
                chunk.IsGenerated = isGenerated;

                int totalCells = GameConstants.CHUNK_SIZE * GameConstants.CHUNK_SIZE;

                // Grid layers
                ReadUshortArray(br, chunk.FloorLayer.GetRawArray(), totalCells);
                ReadUshortArray(br, chunk.UtilityLayer.GetRawArray(), totalCells);
                ReadUshortArray(br, chunk.StructureLayer.GetRawArray(), totalCells);
                ReadByteArray(br, chunk.MetadataLayer.GetRawArray(), totalCells);

                // Bool layers
                ReadBoolArray(br, chunk.ShadowLayer, totalCells);
                ReadBoolArray(br, chunk.LitLayer, totalCells);
                ReadBoolArray(br, chunk.ExploredLayer, totalCells);

                // Complex states
                int stateCount = br.ReadInt32();
                for (int i = 0; i < stateCount; i++)
                {
                    int sx = br.ReadInt16();
                    int sy = br.ReadInt16();
                    int bufLen = br.ReadInt32();
                    
                    // Zero GC: Rent from pool to match ReleaseComplexStates logic
                    byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(GameConstants.MACHINE_STATE_BUFFER_SIZE);
                    // Read exactly bufLen into the rented buffer
                    br.Read(buffer, 0, Mathf.Min(bufLen, GameConstants.MACHINE_STATE_BUFFER_SIZE));
                    
                    chunk.ComplexStates[new Vector2Int(sx, sy)] = buffer;
                }

                // V26: Safe read for LastSimulatedDay (Backward Compatibility)
                if (br.BaseStream.Position < br.BaseStream.Length)
                {
                    chunk.LastSimulatedDay = br.ReadInt32();
                }
                else
                {
                    // For old saves, assume day 1 or current day.
                    chunk.LastSimulatedDay = AstroPioneer.Managers.TimeManager.Instance != null ? AstroPioneer.Managers.TimeManager.Instance.DaysPassed : 1;
                }

                chunk.IsDirty = false;
                return chunk;
            }
        }

        /// <summary>
        /// Check if a chunk file exists on disk without reading it.
        /// </summary>
        public static bool HasChunkFile(ChunkCoord coord)
        {
            return File.Exists(Path.Combine(ChunkDir, coord.ToFileName()));
        }

        /// <summary>
        /// Check if a save game exists (meta.dat is present).
        /// Use this to determine new game vs returning player — no PlayerPrefs needed.
        /// </summary>
        public static bool HasMetaFile()
        {
            return File.Exists(MetaPath);
        }

        /// <summary>
        /// Delete all save data (new game).
        /// </summary>
        public static void DeleteAllSaves()
        {
            if (Directory.Exists(SaveRoot))
                Directory.Delete(SaveRoot, true);
        }

        // ─── Binary Write Helpers (Zero Allocation) ───

        private static void WriteUshortArray(BinaryWriter bw, ushort[] data, int count)
        {
            for (int i = 0; i < count; i++)
                bw.Write(data[i]);
        }

        private static void WriteByteArray(BinaryWriter bw, byte[] data, int count)
        {
            bw.Write(data, 0, count);
        }

        private static void WriteBoolArray(BinaryWriter bw, GridLayer<bool> layer, int totalCells)
        {
            WriteBoolArraySnapshot(bw, layer.GetRawArray(), totalCells);
        }

        private static void WriteBoolArraySnapshot(BinaryWriter bw, bool[] raw, int totalCells)
        {
            // Pack 8 bools per byte (bit packing)
            int byteCount = (totalCells + 7) / 8;
            for (int byteIdx = 0; byteIdx < byteCount; byteIdx++)
            {
                byte packed = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    int idx = byteIdx * 8 + bit;
                    if (idx < totalCells && raw[idx])
                        packed |= (byte)(1 << bit);
                }
                bw.Write(packed);
            }
        }

        // ─── Binary Read Helpers ───

        private static void ReadUshortArray(BinaryReader br, ushort[] target, int count)
        {
            for (int i = 0; i < count; i++)
                target[i] = br.ReadUInt16();
        }

        private static void ReadByteArray(BinaryReader br, byte[] target, int count)
        {
            br.Read(target, 0, count);
        }

        private static void ReadBoolArray(BinaryReader br, GridLayer<bool> layer, int totalCells)
        {
            int byteCount = (totalCells + 7) / 8;
            var raw = layer.GetRawArray();
            for (int byteIdx = 0; byteIdx < byteCount; byteIdx++)
            {
                byte packed = br.ReadByte();
                for (int bit = 0; bit < 8; bit++)
                {
                    int idx = byteIdx * 8 + bit;
                    if (idx < totalCells)
                        raw[idx] = (packed & (1 << bit)) != 0;
                }
            }
        }
    }
}
