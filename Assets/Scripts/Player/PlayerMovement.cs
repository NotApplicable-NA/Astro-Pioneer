using UnityEngine;

namespace AstroPioneer.Player
{
    /// <summary>
    /// PlayerMovement - 2D Top-Down 8-directional movement.
    /// Uses Rigidbody2D (Kinematic) for smooth grid-world navigation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Sprite")]
        [SerializeField] private bool flipSpriteOnDirection = true;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 movementInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
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
            // Move using Rigidbody2D for clean physics interaction
            Vector2 newPos = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        // Public API for external systems (e.g. cutscenes, rescue)
        public bool IsMoving => movementInput.sqrMagnitude > 0.01f;
        public Vector2 FacingDirection => movementInput.normalized;
    }
}
