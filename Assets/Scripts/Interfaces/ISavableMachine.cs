using System;

namespace AstroPioneer.Interfaces
{
    /// <summary>
    /// ISavableMachine — Interface for grid-based structures that require 
    /// complex state persistence (inventories, power levels, etc.) 
    /// using the Chunk.ComplexStates system.
    /// </summary>
    public interface ISavableMachine
    {
        /// <summary>
        /// Write machine state into a pre-allocated byte buffer.
        /// Zero GC — Implementations should use System.BitConverter.TryWriteBytes 
        /// with Span to avoid intermediate allocations.
        /// </summary>
        /// <param name="buffer">Target buffer of size GameConstants.MACHINE_STATE_BUFFER_SIZE.</param>
        void SaveState(Span<byte> buffer);

        /// <summary>
        /// Read machine state from a pre-allocated byte buffer.
        /// </summary>
        /// <param name="buffer">Source buffer of size GameConstants.MACHINE_STATE_BUFFER_SIZE.</param>
        void LoadState(ReadOnlySpan<byte> buffer);
    }
}
