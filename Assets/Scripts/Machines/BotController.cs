using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Systems.Pathfinding;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// BotController — Low-level pathfinding movement driver for automated bots.
    /// Follows waypoints from PathfindingManager and handles sprite flipping.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BotController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float reachThreshold = 0.1f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private List<Vector3> currentPath = new List<Vector3>(128);
        private int currentPathIndex;
        private bool isMoving;
        private System.Action onDestinationReached;
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private Rigidbody2D rb;

        public bool IsMoving => isMoving;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (!isMoving || currentPath == null) return;
            HandleMovement();
            UpdateAnimation();
        }

        public bool SetDestination(Vector3 targetPos, System.Action onReached = null)
        {
            if (PathfindingManager.Instance == null) return false;

            PathfindingManager.Instance.FindPath(transform.position, targetPos, currentPath);
            if (currentPath.Count <= 1)
            {
                isMoving = false;
                return false;
            }

            currentPathIndex = 0;
            isMoving = true;
            onDestinationReached = onReached;
            return true;
        }

        public void StopMoving()
        {
            isMoving = false;
            currentPath.Clear();
        }

        private void HandleMovement()
        {
            if (rb == null) return;
            Vector3 target = currentPath[currentPathIndex];

            // V23: Check if next waypoint is still walkable (obstacle placed mid-path)
            if (PathfindingManager.Instance != null && !PathfindingManager.Instance.IsWalkable(target))
            {
                // Recalculate path to final destination
                Vector3 finalDest = currentPath[currentPath.Count - 1];
                PathfindingManager.Instance.FindPath(transform.position, finalDest, currentPath);
                currentPathIndex = 0;

                if (currentPath.Count <= 1)
                {
                    // No valid path — abort
                    StopMoving();
                    onDestinationReached?.Invoke();
                    return;
                }
                target = currentPath[currentPathIndex];
            }
            
            Vector2 currentPos = rb.position;
            Vector2 targetPos2D = new Vector2(target.x, target.y);
            
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos2D, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            Vector2 delta = currentPos - targetPos2D;
            if (delta.sqrMagnitude < reachThreshold * reachThreshold)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    StopMoving();
                    onDestinationReached?.Invoke();
                }
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            animator.SetBool(IsMovingHash, isMoving);

            if (isMoving && currentPath != null && currentPathIndex < currentPath.Count)
            {
                float dirX = (currentPath[currentPathIndex] - transform.position).normalized.x;
                if (dirX != 0 && spriteRenderer != null)
                    spriteRenderer.flipX = dirX < 0;
            }
        }

        void OnDrawGizmos()
        {
            if (currentPath == null) return;
            Gizmos.color = Color.green;
            for (int i = currentPathIndex; i < currentPath.Count - 1; i++)
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }
}
