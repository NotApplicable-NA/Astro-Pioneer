namespace AstroPioneer.Core
{
    /// <summary>
    /// GameConstants — Centralized magic values eliminating fragile hard-coded strings
    /// and numbers scattered across the codebase.
    /// </summary>
    public static class GameConstants
    {
        // ─── Chunk System ───
        public const int CHUNK_SIZE = 16;
        public const int LOAD_RADIUS = 2;     // Chunks around player to keep loaded
        public const int UNLOAD_RADIUS = 3;   // Distance before chunk is saved & destroyed

        // ─── Structure IDs (ushort) ───
        public const ushort STRUCTURE_EMPTY = 0;
        // Crops: 1-99
        public const ushort STRUCTURE_SPACE_POTATO = 1;
        public const ushort STRUCTURE_NEON_CARROT = 2;
        // Machines: 100-199
        public const ushort STRUCTURE_SPRINKLER = 100;
        public const ushort STRUCTURE_WATER_PUMP = 101;
        public const ushort STRUCTURE_COMPOSTER = 102;
        public const ushort STRUCTURE_HARVESTER = 103;
        public const ushort STRUCTURE_GENERATOR = 104;
        public const ushort STRUCTURE_BATTERY = 105;
        public const ushort STRUCTURE_UV_LIGHT = 106;
        public const ushort STRUCTURE_STORAGE = 107;
        public const ushort STRUCTURE_ADV_STORAGE = 108;
        public const ushort STRUCTURE_PROCESSING = 109;
        public const ushort STRUCTURE_SHIPPING_BIN = 110;
        public const ushort STRUCTURE_TRADING_POST = 111;
        public const ushort STRUCTURE_SLEEP_POD = 112;
        // Utility (Micro-Grid): 200-299
        public const ushort STRUCTURE_POWER_CABLE = 200;
        public const ushort STRUCTURE_WATER_PIPE = 201;
        public const ushort STRUCTURE_FENCE = 202;
        // Floor: 300-399
        public const ushort STRUCTURE_IRON_FLOOR = 300;
        public const ushort STRUCTURE_DIRT_FLOOR = 301;
        public const ushort STRUCTURE_OCCUPIED_PART = 65535; // Marks tiles occupied by multi-tile machines

        // ─── Metadata Encoding (byte) ───
        // For crops: lower 4 bits = growth stage (0-15), upper 4 bits = flags
        public const byte META_WATERED_FLAG = 0b_1000_0000;   // bit 7
        public const byte META_GROWTH_MASK = 0b_0000_1111;    // bits 0-3

        // ─── Pool Tags ───
        public const string POOL_HARVEST_VFX = "HarvestVFX";
        public const string POOL_WATERING_VFX = "WateringVFX";
        public const string POOL_CROP_VISUAL = "CropVisual";
        public const string POOL_MACHINE_VISUAL = "MachineVisual";

        // ─── Tool IDs (Legacy — replaced by ToolBehaviour V22) ───
        // Tool behaviour is now determined by ScriptableObject assets, not string constants.

        // ─── Save System ───
        public const string SAVE_DIR = "saves";
        public const string SAVE_META_FILE = "meta.dat";
        public const string SAVE_CHUNK_DIR = "chunks";
        public const string SAVE_ENTITY_FILE = "entities.dat";

        // ─── Machine State Buffer ───
        /// <summary>
        /// Max bytes for a single machine's complex state.
        /// Keeps byte[] allocations predictable and reusable.
        /// </summary>
        public const int MACHINE_STATE_BUFFER_SIZE = 256;

        // ─── World & Systems ───
        public const int WORLD_BOUNDARY_LIMIT = 500;   // Boundary for world-coord-aware systems
        public const int MAX_ENCLOSURE_SIZE = 100;     // Max tiles for flood-fill enclosure detection
        public const int MAX_SAVES_PER_FRAME = 2;      // Max chunk saves flushed per frame (V19 Rate Limiter)

        // ─── Rendering & Z-Ordering ───
        public const int SORTING_ORDER_FLOOR = 0;
        public const int SORTING_ORDER_STRUCTURE = 100;
        public const int SORTING_ORDER_UTILITY = 150;
        public const int SORTING_ORDER_HOLOGRAM = 1000;
    }
}
