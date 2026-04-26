using UnityEngine;
using AstroPioneer.Interfaces;
using AstroPioneer.Core;

namespace AstroPioneer.Player
{
    /// <summary>
    /// PlayerMovement - 2D Top-Down 8-directional movement.
    /// Uses Rigidbody2D (Kinematic) for smooth grid-world navigation.
    /// Implements IEntity for EntityManager registration and chunk tracking.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour, IEntity
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Sprite")]
        [SerializeField] private bool flipSpriteOnDirection = true;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 movementInput;
        private string entityID;

        [Header("Entity Setup")]
        [Tooltip("The index of this prefab in the EntityRegistry")]
        [SerializeField] private int registryTypeID = 2;

        // ─── IEntity Implementation ───
        public Vector3 WorldPosition => transform.position;
        public string EntityID => entityID;
        public int EntityTypeID => registryTypeID;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
            entityID = $"Player_{System.Guid.NewGuid():N}";
        }

        void OnEnable()
        {
            if (ServiceLocator.TryGet<EntityManager>(out var em))
                em.Register(this);

            // Set as tracking target for ChunkManager
            if (ServiceLocator.TryGet<ChunkManager>(out var cm))
                cm.SetTrackingTarget(transform);
        }

        void OnDisable()
        {
            if (ServiceLocator.TryGet<EntityManager>(out var em))
                em.Unregister(this);
        }

        void Update()
        {
            if (AstroPioneer.Systems.Survival.PlayerVitals.Instance != null && AstroPioneer.Systems.Survival.PlayerVitals.Instance.IsIncapacitated)
            {
                movementInput = Vector2.zero;
                return;
            }

            // Read WASD / Arrow Key input
            movementInput.x = Input.GetAxisRaw("Horizontal");
            movementInput.y = Input.GetAxisRaw("Vertical");

            // Normalize to prevent faster diagonal movement
            if (movementInput.sqrMagnitude > 1f)
                movementInput.Normalize();

            // Flip sprite based on horizontal direction
            if (flipSpriteOnDirection && spriteRenderer != null && movementInput.x != 0)
            {
                spriteRenderer.flipX = movementInput.x < 0;
            }
        }

        void FixedUpdate()
        {
            // Gunakan Time.fixedDeltaTime agar kecepatan dinormalisasi per detik (unit/s)
            Vector2 newPos = rb.position + (movementInput * moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            // V25 QA Requirement: Register position in Data Layer
            if (AstroPioneer.Data.EntityRegistry.Instance != null)
                AstroPioneer.Data.EntityRegistry.Instance.RegisterEntityPosition(entityID, newPos);
        }

        // ─── IEntity Callbacks ───

        public void OnChunkEntered(ChunkCoord newChunk)
        {
            // Player entering new chunk — could trigger exploration reveal, etc.
        }

        public void OnChunkExited(ChunkCoord oldChunk)
        {
            // Player leaving chunk — chunk may be unloaded by ChunkManager
        }

        public void SerializeState(System.IO.BinaryWriter writer)
        {
            // Write position (8 bytes: 2 floats)
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);
        }

        public void DeserializeState(System.IO.BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            transform.position = new Vector3(x, y, 0);
        }

        // Public API for external systems (e.g. cutscenes, rescue)
        public bool IsMoving => movementInput.sqrMagnitude > 0.01f;
        public Vector2 FacingDirection => movementInput.normalized;
    }
}
