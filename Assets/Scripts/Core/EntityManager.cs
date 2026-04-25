using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Core
{
    /// <summary>
    /// EntityManager — Manages all free-moving entities (Player, Fauna, Bots, AgriMech).
    /// 
    /// Unlike structures (grid-locked, stored as ushort IDs in Chunks),
    /// entities move freely in float-space and are tracked in a flat list.
    /// 
    /// Entities read grid data beneath them via:
    ///   Vector2Int gridPos = new Vector2Int(
    ///       Mathf.FloorToInt(transform.position.x),
    ///       Mathf.FloorToInt(transform.position.y));
    ///   ushort structure = GridManager.Instance.GetStructureAt(gridPos);
    /// </summary>
    public class EntityManager : MonoBehaviour
    {
        private readonly List<AstroPioneer.Interfaces.IEntity> entities 
            = new List<AstroPioneer.Interfaces.IEntity>();

        // Chunk tracking per entity (to fire Enter/Exit events)
        private struct EntityState
        {
            public ChunkCoord lastChunk;
            public Vector3 lastCheckPos;
        }

        private readonly Dictionary<AstroPioneer.Interfaces.IEntity, EntityState> entityStates
            = new Dictionary<AstroPioneer.Interfaces.IEntity, EntityState>();

        [Header("Optimization")]
        [SerializeField] private float moveThreshold = 0.5f; // Units to move before re-checking chunk boundary
        private float sqrMoveThreshold;

        void Awake()
        {
            sqrMoveThreshold = moveThreshold * moveThreshold;
        }

        void OnDestroy()
        {
            ServiceLocator.Unregister<EntityManager>();
        }

        // ─── Registration ───

        public void Register(AstroPioneer.Interfaces.IEntity entity)
        {
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
                // Track initial chunk and position
                var chunk = ChunkCoord.FromWorldPos(entity.WorldPosition.x, entity.WorldPosition.y);
                entityStates[entity] = new EntityState { lastChunk = chunk, lastCheckPos = entity.WorldPosition };
            }
        }

        public void Unregister(AstroPioneer.Interfaces.IEntity entity)
        {
            int idx = entities.IndexOf(entity);
            if (idx >= 0)
            {
                int last = entities.Count - 1;
                entities[idx] = entities[last];
                entities.RemoveAt(last);
            }
            entityStates.Remove(entity);
        }

        // ─── Optimized Per-Frame Chunk Boundary Check ───

        void LateUpdate()
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                if (object.ReferenceEquals(entity, null))
                {
                    int last = entities.Count - 1;
                    entities[i] = entities[last];
                    entities.RemoveAt(last);
                    continue;
                }

                if (!entityStates.TryGetValue(entity, out var state)) continue;

                // Optimization: Skip check if we haven't moved far enough
                float sqrDist = (entity.WorldPosition - state.lastCheckPos).sqrMagnitude;
                if (sqrDist < sqrMoveThreshold) continue;

                // Boundary reached, check chunk transition
                var currentChunk = ChunkCoord.FromWorldPos(entity.WorldPosition.x, entity.WorldPosition.y);

                if (state.lastChunk != currentChunk)
                {
                    entity.OnChunkExited(state.lastChunk);
                    entity.OnChunkEntered(currentChunk);
                }

                // Update state for next check
                entityStates[entity] = new EntityState { lastChunk = currentChunk, lastCheckPos = entity.WorldPosition };
            }
        }

        // ─── Queries ───

        /// <summary>Get the grid position directly underneath an entity's feet.</summary>
        public static Vector2Int GetGridBelowEntity(Vector3 entityWorldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(entityWorldPos.x), Mathf.FloorToInt(entityWorldPos.y));
        }

        /// <summary>Get all registered entities (for save/load).</summary>
        public IReadOnlyList<AstroPioneer.Interfaces.IEntity> GetAllEntities() => entities;

        /// <summary>Wipe all entities from the world (for debug reset).</summary>
        public void ClearAllEntities()
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                if (entity != null && entity is MonoBehaviour mb && mb != null)
                {
                    Destroy(mb.gameObject);
                }
            }
            entities.Clear();
            entityStates.Clear();
        }
    }
}
