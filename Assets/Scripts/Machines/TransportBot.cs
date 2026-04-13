using UnityEngine;
using System.Collections;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Pathfinding;

namespace AstroPioneer.Machines.Automation
{
    public enum TransportState { Idle, MovingToSource, PickingUp, MovingToTarget, DroppingOff }

    /// <summary>
    /// TransportBot — Automated bot that picks up items from a source machine
    /// and delivers them to a target machine via pathfinding.
    /// </summary>
    [RequireComponent(typeof(BotController))]
    public class TransportBot : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private TransportState currentState = TransportState.Idle;
        [SerializeField] private InventoryItem heldItem;

        [Header("Config")]
        [SerializeField] private InventoryItem itemToTransport;
        [SerializeField] private float taskTimeout = 10f;

        private BotController botController;
        private Animator animator;
        private Vector3 sourcePosition;
        private Vector3 targetPosition;
        private bool hasTask;

        public TransportState GetState() => currentState;
        public bool IsAvailable => currentState == TransportState.Idle && !hasTask;

        void Start()
        {
            TryRegisterOnGrid();
            botController = GetComponent<BotController>();
            animator = GetComponent<Animator>();

            if (BotManager.Instance != null)
                BotManager.Instance.RegisterBot(this);

            ResetState();
        }

        public void AssignTask(Vector3 source, Vector3 target)
        {
            if (currentState != TransportState.Idle) return;

            sourcePosition = source;
            targetPosition = target;
            hasTask = true;
            StartCoroutine(ExecuteTransportTask());
        }

        private IEnumerator ExecuteTransportTask()
        {
            // 1. Move to source
            yield return StartCoroutine(MoveToWithTimeout(sourcePosition, TransportState.MovingToSource));
            if (!hasTask) yield break;

            // 2. Pickup
            currentState = TransportState.PickingUp;
            if (animator != null) animator.SetTrigger("Pickup");
            yield return new WaitForSeconds(0.5f);

            if (!TryPickupAtPosition(sourcePosition))
            {
                ResetState();
                yield break;
            }

            if (animator != null) animator.SetBool("IsCarrying", true);

            // 3. Move to target
            yield return StartCoroutine(MoveToWithTimeout(targetPosition, TransportState.MovingToTarget));
            if (!hasTask) yield break;

            // 4. Dropoff
            currentState = TransportState.DroppingOff;
            if (animator != null) animator.SetTrigger("Dropoff");
            yield return new WaitForSeconds(0.5f);

            TryDropoffAtPosition(targetPosition);
            if (animator != null) animator.SetBool("IsCarrying", false);

            ResetState();
        }

        private IEnumerator MoveToWithTimeout(Vector3 targetCenter, TransportState state)
        {
            currentState = state;
            Vector3 approachPos = GetValidInteractionPoint(targetCenter);

            if (!botController.SetDestination(approachPos))
            {
                ResetState();
                yield break;
            }

            float startTime = Time.time;
            yield return new WaitUntil(() => !botController.IsMoving || Time.time - startTime > taskTimeout);

            if (botController.IsMoving)
            {
                botController.StopMoving();
                ResetState();
                yield break;
            }

            yield return null;
        }

        private bool TryPickupAtPosition(Vector3 pos)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(pos, 0.5f))
            {
                if (hit.TryGetComponent<MachineWaterPump>(out var pump) && pump.TryTakeWater(1, out var waterItem))
                {
                    if (waterItem != null)
                    {
                        itemToTransport = waterItem;
                        heldItem = waterItem;
                    }
                    return true;
                }
            }
            return false;
        }

        private void TryDropoffAtPosition(Vector3 pos)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(pos, 0.5f))
            {
                if (hit.TryGetComponent<MachineStorage>(out var storage))
                {
                    storage.TryAddItem(itemToTransport, 1);
                    break;
                }
            }
        }

        private void ResetState()
        {
            currentState = TransportState.Idle;
            hasTask = false;
            heldItem = null;
            if (animator != null) animator.SetBool("IsCarrying", false);
        }

        private Vector3 GetValidInteractionPoint(Vector3 targetCenter)
        {
            Vector3[] offsets = { Vector3.down, Vector3.left, Vector3.right, Vector3.up };

            foreach (var offset in offsets)
            {
                Vector3 candidate = targetCenter + offset;
                if (PathfindingManager.Instance != null && PathfindingManager.Instance.IsWalkable(candidate))
                    return candidate;
            }
            return targetCenter + Vector3.down;
        }

        private void TryRegisterOnGrid()
        {
            if (GridManager.Instance == null) return;
            Vector2Int pos = GridManager.Instance.WorldToGridPosition(transform.position);
            if (!GridManager.Instance.GetOccupiedCells().ContainsKey(pos))
                GridManager.Instance.TryOccupyCell(pos, gameObject);
        }

        [ContextMenu("Debug State")]
        public void DebugState() => Debug.Log($"[TransportBot] State: {currentState}, HasTask: {hasTask}");

        [ContextMenu("Force Reset")]
        public void ForceReset()
        {
            StopAllCoroutines();
            botController?.StopMoving();
            ResetState();
        }
    }
}
