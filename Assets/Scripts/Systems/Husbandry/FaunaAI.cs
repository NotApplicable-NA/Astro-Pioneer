using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Husbandry
{
    public enum FaunaState
    {
        Idle,
        Wandering,
        Eating,
        Sleeping
    }

    /// <summary>
    /// AI controller for husbandry animals (e.g. Moo-Lien).
    /// Prevents wandering outside their Enclosure.
    /// </summary>
    public class FaunaAI : MonoBehaviour
    {
        [Header("Fauna Settings")]
        public string speciesName = "Moo-Lien";
        public float moveSpeed = 2.0f;
        public float waitTimeMin = 1.0f;
        public float waitTimeMax = 4.0f;
        
        [Header("Runtime State")]
        [SerializeField] private FaunaState currentState = FaunaState.Idle;
        private Vector2Int currentGridPos;
        private Enclosure myEnclosure;
        
        // Component references
        private Rigidbody2D rb;
        private Animator animator;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Assume grid position based on spawn
            if (GridManager.Instance != null)
            {
                currentGridPos = GridManager.Instance.WorldToGridPosition(transform.position);
                
                // Immediately snap to grid world pos for alignment
                transform.position = GridManager.Instance.GridToWorldPosition(currentGridPos);

                // Check if we are inside an enclosure
                if (EnclosureSystem.Instance != null)
                {
                    myEnclosure = EnclosureSystem.Instance.GetEnclosureAt(currentGridPos);
                }
            }

            StartCoroutine(BehaviorLoop());
        }

        private IEnumerator BehaviorLoop()
        {
            while (true)
            {
                switch (currentState)
                {
                    case FaunaState.Idle:
                        yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));
                        currentState = FaunaState.Wandering;
                        break;
                        
                    case FaunaState.Wandering:
                        yield return StartCoroutine(WanderRoutine());
                        currentState = FaunaState.Idle;
                        break;
                        
                    case FaunaState.Eating:
                        // Play eating animation
                        if (animator != null) animator.SetTrigger("Eat");
                        yield return new WaitForSeconds(3.0f);
                        currentState = FaunaState.Idle;
                        break;
                        
                    case FaunaState.Sleeping:
                        yield return new WaitForSeconds(10.0f);
                        currentState = FaunaState.Idle;
                        break;
                }
            }
        }

        private IEnumerator WanderRoutine()
        {
            if (GridManager.Instance == null) yield break;

            // Find a valid neighbor to move to
            List<Vector2Int> neighbors = GridManager.Instance.GetNeighbors(currentGridPos);
            List<Vector2Int> validMoves = new List<Vector2Int>();

            foreach (var n in neighbors)
            {
                // Must be available on Macro grid (Crops act as obstacles per user preference/default logic)
                if (GridManager.Instance.IsPositionAvailable(n))
                {
                    // If we belong to an enclosure, restrict to that enclosure
                    if (myEnclosure != null)
                    {
                        if (myEnclosure.Contains(n))
                            validMoves.Add(n);
                    }
                    else
                    {
                        // Free range! (If spawned outside enclosure)
                        validMoves.Add(n);
                    }
                }
            }

            if (validMoves.Count > 0)
            {
                Vector2Int targetGrid = validMoves[Random.Range(0, validMoves.Count)];
                Vector3 startPos = transform.position;
                Vector3 targetPos = GridManager.Instance.GridToWorldPosition(targetGrid);

                float t = 0;
                float distance = Vector3.Distance(startPos, targetPos);
                float duration = distance / moveSpeed;

                if (animator != null) animator.SetBool("IsWalking", true);

                // Face direction
                if (targetPos.x > startPos.x) transform.localScale = new Vector3(-1, 1, 1);
                else if (targetPos.x < startPos.x) transform.localScale = new Vector3(1, 1, 1);

                while (t < 1)
                {
                    t += Time.deltaTime / duration;
                    transform.position = Vector3.Lerp(startPos, targetPos, t);
                    yield return null;
                }

                if (animator != null) animator.SetBool("IsWalking", false);
                currentGridPos = targetGrid;
            }
        }
    }
}
