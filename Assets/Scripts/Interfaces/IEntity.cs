using UnityEngine;

namespace AstroPioneer.Interfaces
{
    /// <summary>
    /// IEntity — Interface for all free-moving objects in the world.
    /// 
    /// Entities (Player, Fauna, Bot-E, AgriMech) live in float-space
    /// and are NOT stored in the Grid. They read grid data beneath them
    /// using Mathf.FloorToInt(transform.position) when needed.
    /// 
    /// Lifecycle managed by EntityManager, not by Chunks.
    /// 
    /// V22: Serialization uses BinaryWriter/BinaryReader directly
    /// for zero-allocation I/O (no intermediate byte[] buffers).
    /// </summary>
    public interface IEntity
    {
        /// <summary>Current world position (float precision).</summary>
        Vector3 WorldPosition { get; }

        /// <summary>Unique identifier for save/load.</summary>
        string EntityID { get; }

        /// <summary>Entity type ID for registry-based deserialization (matches index in EntityRegistry).</summary>
        int EntityTypeID { get; }

        /// <summary>Called when the entity enters a new chunk boundary.</summary>
        void OnChunkEntered(AstroPioneer.Core.ChunkCoord newChunk);

        /// <summary>Called when the entity exits a chunk boundary.</summary>
        void OnChunkExited(AstroPioneer.Core.ChunkCoord oldChunk);

        /// <summary>
        /// Serialize entity state directly to a BinaryWriter stream.
        /// Zero allocation — writes straight to disk.
        /// </summary>
        void SerializeState(System.IO.BinaryWriter writer);

        /// <summary>
        /// Deserialize entity state directly from a BinaryReader stream.
        /// Zero allocation — reads straight from disk.
        /// </summary>
        void DeserializeState(System.IO.BinaryReader reader);
    }
}
