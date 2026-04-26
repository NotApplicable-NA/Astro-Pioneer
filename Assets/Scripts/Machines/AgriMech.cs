using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Interfaces;
using AstroPioneer.Core;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// AgriMech — Mobile farming machine that traverses horizontally across the grid,
    /// planting crops on every cell it passes over.
    /// 
    /// Classified as ENTITY (not Structure) because it moves freely.
    /// Reads grid data beneath it via EntityManager.GetGridBelowEntity().
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AgriMech : MonoBehaviour, IEntity, IGridInteractable
    {
        [Header("Settings")]
        public Vector2Int dimensions = new Vector2Int(2, 2);

        [Header("Entity Setup")]
        [Tooltip("The index of this prefab in the EntityRegistry")]
        public int registryTypeID = 0;

        [Header("Farming")]
        public CropStructureData cropData;

        [Header("Movement")]
        public float moveInterval = 1f;
        public int roamRadius = 5; // Distance it can roam from its starting point
        public int direction = 1; // 0=Up, 1=Right, 2=Down, 3=Left

        [Header("Runtime")]
        public bool isInitialized;
        private Vector2Int originGridPos;

        private float moveTimer;
        private string entityID;
        private Rigidbody2D rb;

        // ─── IEntity Implementation ───
        public Vector3 WorldPosition => transform.position;
        public string EntityID => entityID;
        public int EntityTypeID => registryTypeID;

        void Awake()
        {
            entityID = $"AgriMech_{System.Guid.NewGuid():N}";
            rb = GetComponent<Rigidbody2D>();

            // Top-down game: disable gravity and use Kinematic body.
            // Movement is driven by rb.MovePosition(), not physics forces.
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }

        void OnEnable()
        {
            if (ServiceLocator.TryGet<EntityManager>(out var em))
                em.Register(this);
        }

        void OnDisable()
        {
            if (ServiceLocator.TryGet<EntityManager>(out var em))
                em.Unregister(this);
        }

        public void Initialize(Vector3 spawnWorldPos)
        {
            transform.position = spawnWorldPos;
            if (rb != null) rb.position = spawnWorldPos;
            originGridPos = EntityManager.GetGridBelowEntity(spawnWorldPos);
            isInitialized = true;
            PickRandomDirection();
        }

        public void Interact(InventoryItem item)
        {
            if (item != null && item.type == ItemType.Seed)
                cropData = item.placedStructure as CropStructureData;
        }

        void FixedUpdate()
        {
            if (!isInitialized || cropData == null || rb == null) return;

            moveTimer += Time.fixedDeltaTime;
            if (moveTimer >= moveInterval)
            {
                moveTimer = 0f;
                MoveMachine();
            }
        }

        // ─── Movement ───

        private void MoveMachine()
        {
            Vector2 currentPos = rb.position;
            Vector2 dirVec = GetDirectionVector();
            Vector2 nextPos2D = currentPos + dirVec * 1f;
            Vector3 nextWorldPos = new Vector3(nextPos2D.x, nextPos2D.y, transform.position.z);

            Vector2Int nextGridPos = EntityManager.GetGridBelowEntity(nextWorldPos);
            
            // Check if bounds exceeded (e.g. 10x10 area means +/- 5 from origin)
            bool outOfRoamBounds = Mathf.Abs(nextGridPos.x - originGridPos.x) > roamRadius || 
                                   Mathf.Abs(nextGridPos.y - originGridPos.y) > roamRadius;

            // Turn around if hitting boundary or an obstacle
            if (outOfRoamBounds || (GridManager.Instance != null && !GridManager.Instance.IsPositionAvailable(nextGridPos)))
            {
                PickRandomDirection();
                return;
            }

            rb.MovePosition(nextPos2D);
            PlantCropsAtCurrentPosition();
        }

        private void PickRandomDirection()
        {
            direction = UnityEngine.Random.Range(0, 4);
        }

        private Vector2 GetDirectionVector()
        {
            switch (direction)
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                case 2: return Vector2.down;
                case 3: return Vector2.left;
                default: return Vector2.right;
            }
        }

        private void PlantCropsAtCurrentPosition()
        {
            if (cropData == null || CropManager.Instance == null) return;

            // Read the grid beneath each cell of the mech's footprint
            Vector2Int baseGrid = EntityManager.GetGridBelowEntity(transform.position);
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    Vector2Int cell = baseGrid + new Vector2Int(x, y);
                    if (CropManager.Instance.GetCropAt(cell) == 0)
                        CropManager.Instance.PlantCrop(cell, cropData);
                }
            }
        }

        // ─── IEntity Callbacks ───

        public void OnChunkEntered(ChunkCoord newChunk) { }
        public void OnChunkExited(ChunkCoord oldChunk) { }

        public void SerializeState(System.IO.BinaryWriter writer)
        {
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);
            writer.Write((byte)direction);
            writer.Write(isInitialized);
        }

        public void DeserializeState(System.IO.BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector3 pos = new Vector3(x, y, 0);
            transform.position = pos;
            if (rb != null) rb.position = pos;
            direction = reader.ReadByte();
            if (direction < 0 || direction > 3) PickRandomDirection();
            originGridPos = EntityManager.GetGridBelowEntity(pos);
            isInitialized = reader.ReadBoolean();
        }

        // ─── Helpers ───

        private void ForEachCell(Vector2Int origin, System.Action<Vector2Int> action)
        {
            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    action(origin + new Vector2Int(x, y));
        }
    }
}
