using System;

namespace AstroPioneer.Core
{
    /// <summary>
    /// GridLayer — Generic fixed-size grid data container using a FLAT 1D ARRAY.
    /// 
    /// Design decisions:
    /// 1. Uses T[] (1D) instead of T[,] (2D) because Unity's C# Job System
    ///    requires NativeArray which only works with flat arrays.
    /// 2. Access pattern: cells[x + y * size] for cache-friendly row-major layout.
    /// 3. Pure C# — no UnityEngine dependency, safe for Job threads.
    /// </summary>
    public class GridLayer<T> where T : struct
    {
        private readonly T[] cells;
        public int Size { get; }

        public GridLayer(int size)
        {
            Size = size;
            cells = new T[size * size];
        }

        /// <summary>
        /// Get value at (x, y). Returns default(T) if out of bounds.
        /// </summary>
        public T Get(int x, int y)
        {
            if (!IsValid(x, y)) return default;
            return cells[x + y * Size];
        }

        /// <summary>
        /// Set value at (x, y). No-op if out of bounds.
        /// </summary>
        public void Set(int x, int y, T value)
        {
            if (!IsValid(x, y)) return;
            cells[x + y * Size] = value;
        }

        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }

        /// <summary>
        /// Direct access to the underlying flat array.
        /// Used by:
        /// - Job System: copy into NativeArray before scheduling, copy back after completion.
        /// - Save System: write directly via BinaryWriter without intermediate allocation.
        /// </summary>
        public T[] GetRawArray() => cells;

        /// <summary>
        /// Bulk-copy data from a source array (e.g. from NativeArray.ToArray() after Job completes).
        /// </summary>
        public void CopyFrom(T[] source)
        {
            if (source == null || source.Length != cells.Length)
                throw new ArgumentException(
                    $"Source array length ({source?.Length}) must match grid size ({cells.Length}).");
            Array.Copy(source, cells, cells.Length);
        }

        /// <summary>
        /// Reset all cells to default(T). Zero-alloc.
        /// </summary>
        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
        }

        /// <summary>
        /// Total number of cells (Size * Size).
        /// </summary>
        public int Length => cells.Length;
    }
}
