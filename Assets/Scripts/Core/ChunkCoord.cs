using System;

namespace AstroPioneer.Core
{
    /// <summary>
    /// ChunkCoord — Immutable value type identifying a chunk's position in chunk-space.
    /// 
    /// Conversion rules (grid position == world position, cellSize = 1.0):
    ///   World (35.7, 42.3) → Grid (35, 42) → Chunk (2, 2) with local (3, 10)
    ///   because 35 / 16 = 2, remainder 3; and 42 / 16 = 2, remainder 10.
    ///   
    /// Pure C# — no UnityEngine references. Safe for Job System threads.
    /// </summary>
    public readonly struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public readonly int x;
        public readonly int y;

        public ChunkCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // ─── Conversions ───

        /// <summary>
        /// Convert a world position (float) to the chunk coordinate that contains it.
        /// Uses FloorToInt manually (no Mathf dependency) for thread safety.
        /// </summary>
        public static ChunkCoord FromWorldPos(float worldX, float worldY)
        {
            return new ChunkCoord(
                FloorDiv((int)System.Math.Floor(worldX), GameConstants.CHUNK_SIZE),
                FloorDiv((int)System.Math.Floor(worldY), GameConstants.CHUNK_SIZE));
        }

        /// <summary>
        /// Convert a grid position (int) to the chunk coordinate that contains it.
        /// </summary>
        public static ChunkCoord FromGridPos(int gridX, int gridY)
        {
            return new ChunkCoord(
                FloorDiv(gridX, GameConstants.CHUNK_SIZE),
                FloorDiv(gridY, GameConstants.CHUNK_SIZE));
        }

        /// <summary>
        /// Convert world grid position to local position within this chunk.
        /// Result is always in range [0, CHUNK_SIZE).
        /// </summary>
        public static void WorldToLocal(int worldX, int worldY, out int localX, out int localY)
        {
            localX = FloorMod(worldX, GameConstants.CHUNK_SIZE);
            localY = FloorMod(worldY, GameConstants.CHUNK_SIZE);
        }

        /// <summary>
        /// World origin (bottom-left corner) of this chunk in world coordinates.
        /// </summary>
        public float WorldOriginX => x * GameConstants.CHUNK_SIZE;
        public float WorldOriginY => y * GameConstants.CHUNK_SIZE;

        // ─── Equality ───

        public bool Equals(ChunkCoord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is ChunkCoord other && Equals(other);

        /// <summary>
        /// Fast hash using large primes for minimal collisions in Dictionary lookups.
        /// </summary>
        public override int GetHashCode() => x * 73856093 ^ y * 19349663;

        public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
        public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);

        public override string ToString() => $"Chunk({x},{y})";

        /// <summary>
        /// Get the file name for saving this chunk to disk.
        /// </summary>
        public string ToFileName() => $"{x}_{y}.chunk";

        // ─── Math Helpers (Thread-safe, no Mathf) ───

        /// <summary>
        /// Integer floor division that handles negative numbers correctly.
        /// C# division truncates towards zero, but we need towards negative infinity.
        /// Example: -1 / 16 = 0 in C#, but we need -1 (chunk -1 contains grid pos -1).
        /// </summary>
        private static int FloorDiv(int a, int b)
        {
            return a >= 0 ? a / b : (a - b + 1) / b;
        }

        /// <summary>
        /// Modulo that always returns positive. 
        /// C#: -1 % 16 = -1, but we need 15 (local pos within the chunk).
        /// </summary>
        private static int FloorMod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }
}
