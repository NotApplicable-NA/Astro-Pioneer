using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Managers;
using AstroPioneer.Interfaces;
using AstroPioneer.Core;

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
    /// Implements IEntity for EntityManager registration — lives in float-space.
    /// </summary>
    public class FaunaAI : MonoBehaviour, IEntity
    {
        [Header("Fauna Settings")]
        public string speciesName = "Moo-Lien";
        public float moveSpeed = 2.0f;
        public float waitTimeMin = 1.0f;
        public float waitTimeMax = 4.0f;
        
        [Header("Entity Setup")]
        [Tooltip("The index of this prefab in the EntityRegistry")]
        public int registryTypeID = 3;
        
        [Header("Runtime State")]
        [SerializeField] private FaunaState currentState = FaunaState.Idle;
        private Enclosure myEnclosure;
        private string entityID;
        
        // Movement state
        private bool isWanderingTargetActive = false;
        private Vector3 targetWanderPos;

        // Caller-owned buffers — zero GC, reused across wander cycles
        private readonly List<Vector2Int> neighborBuffer = new List<Vector2Int>(4);
        private readonly List<Vector2Int> validMovesBuffer = new List<Vector2Int>(4);
        private readonly WaitForSeconds eatingWait = new WaitForSeconds(3.0f);
        private readonly WaitForSeconds sleepingWait = new WaitForSeconds(10.0f);
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int EatHash = Animator.StringToHash("Eat");
        
        // Component references
        private Rigidbody2D rb;
        private Animator animator;

        // ─── IEntity Implementation ───
        public Vector3 WorldPosition => transform.position;
        public string EntityID => entityID;
        public int EntityTypeID => registryTypeID;

        void Awake()
        {
            entityID = $"Fauna_{System.Guid.NewGuid():N}";
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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Check if we are inside an enclosure
            if (EnclosureSystem.Instance != null)
            {
                Vector2Int gridPos = EntityManager.GetGridBelowEntity(transform.position);
                myEnclosure = EnclosureSystem.Instance.GetEnclosureAt(gridPos);
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
                        if (animator != null) animator.SetTrigger(EatHash);
                        yield return eatingWait;
                        currentState = FaunaState.Idle;
                        break;
                        
                    case FaunaState.Sleeping:
                        yield return sleepingWait;
                        currentState = FaunaState.Idle;
                        break;
                }
            }
        }

        private IEnumerator WanderRoutine()
        {
            if (GridManager.Instance == null) yield break;

            // Get current grid position from world position (entity reads grid beneath its feet)
            Vector2Int currentGridPos = EntityManager.GetGridBelowEntity(transform.position);

            // Find a valid neighbor to move to
            GridManager.Instance.GetNeighbors(currentGridPos, neighborBuffer);
            validMovesBuffer.Clear();

            foreach (var n in neighborBuffer)
            {
                // Must be available on Macro grid
                if (GridManager.Instance.IsPositionAvailable(n))
                {
                    // If we belong to an enclosure, restrict to that enclosure
                    if (myEnclosure != null)
                    {
                        if (myEnclosure.Contains(n))
                            validMovesBuffer.Add(n);
                    }
                    else
                    {
                        validMovesBuffer.Add(n);
                    }
                }
            }

            if (validMovesBuffer.Count > 0)
            {
                Vector2Int targetGrid = validMovesBuffer[Random.Range(0, validMovesBuffer.Count)];
                Vector3 startPos = transform.position;
                targetWanderPos = GridManager.Instance.GridToWorldPosition(targetGrid);
                isWanderingTargetActive = true;

                if (animator != null) animator.SetBool(IsWalkingHash, true);

                // Face direction
                if (targetWanderPos.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
                else if (targetWanderPos.x < transform.position.x) transform.localScale = new Vector3(1, 1, 1);

                while (isWanderingTargetActive)
                {
                    yield return null;
                }

                if (animator != null) animator.SetBool(IsWalkingHash, false);
            }
        }

        void FixedUpdate()
        {
            if (isWanderingTargetActive && rb != null)
            {
                Vector2 currentPos = rb.position;
                Vector2 newPos = Vector2.MoveTowards(currentPos, (Vector2)targetWanderPos, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);

                if (Vector2.Distance(currentPos, (Vector2)targetWanderPos) < 0.05f)
                {
                    isWanderingTargetActive = false;
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
            writer.Write((byte)currentState);
        }

        public void DeserializeState(System.IO.BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            Vector3 pos = new Vector3(x, y, 0);
            transform.position = pos;
            if (rb != null) rb.position = pos;
            currentState = (FaunaState)reader.ReadByte();
        }
    }
}
