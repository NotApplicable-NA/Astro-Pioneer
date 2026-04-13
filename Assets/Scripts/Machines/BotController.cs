using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Systems.Pathfinding;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// BotController — Low-level pathfinding movement driver for automated bots.
    /// Follows waypoints from PathfindingManager and handles sprite flipping.
    /// </summary>
    public class BotController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float reachThreshold = 0.1f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private List<Vector3> currentPath;
        private int currentPathIndex;
        private bool isMoving;
        private System.Action onDestinationReached;

        public bool IsMoving => isMoving;

        void Update()
        {
            if (!isMoving || currentPath == null) return;
            HandleMovement();
            UpdateAnimation();
        }

        public bool SetDestination(Vector3 targetPos, System.Action onReached = null)
        {
            if (PathfindingManager.Instance == null) return false;

            currentPath = PathfindingManager.Instance.FindPath(transform.position, targetPos);
            if (currentPath == null || currentPath.Count <= 1)
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
            currentPath = null;
        }

        private void HandleMovement()
        {
            Vector3 target = currentPath[currentPathIndex];
            target.z = transform.position.z;

            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < reachThreshold)
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

            animator.SetBool("IsMoving", isMoving);

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
