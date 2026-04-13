using UnityEngine;
using AstroPioneer.Systems.Survival;

namespace AstroPioneer.Systems.Exploration
{
    /// <summary>
    /// Oxy-Flora: Oxygen-producing plant placeable on planet surfaces.
    /// GDD 3.5: Creates "oxygen oasis" zones that refill player O2 when nearby.
    /// Requires sunlight — cannot grow in Shadow Canyons without UV Light Pillars.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OxyFlora : MonoBehaviour
    {
        [Header("Oxygen Production")]
        [Tooltip("O2 refill per second while player is in range")]
        [SerializeField] private float o2RefillRate = 3f;

        [Tooltip("Radius of the oxygen oasis (trigger collider size)")]
        [SerializeField] private float oasisRadius = 2f;

        [Header("Growth")]
        [Tooltip("Time in seconds to grow from planted to active")]
        [SerializeField] private float growthTime = 30f;

        [Header("Light Requirement")]
        [Tooltip("Does this plant have access to sunlight/UV light?")]
        [SerializeField] private bool hasLightSource = true;

        [Header("State")]
        [SerializeField] private bool isGrown = false;
        [SerializeField] private bool playerInRange = false;

        [Header("Visuals")]
        [SerializeField] private Sprite seedlingSprite;
        [SerializeField] private Sprite grownSprite;
        [SerializeField] private SpriteRenderer glowRenderer;  // Child glow sprite

        private SpriteRenderer sr;
        private float growthTimer = 0f;

        // Properties
        public bool IsGrown => isGrown;
        public bool HasLightSource => hasLightSource;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        }

        void Start()
        {
            // Setup trigger collider for oasis zone
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                if (col is CircleCollider2D circle)
                    circle.radius = oasisRadius;
            }

            // Start as seedling
            if (!isGrown)
            {
                if (sr != null && seedlingSprite != null)
                    sr.sprite = seedlingSprite;
                if (glowRenderer != null)
                    glowRenderer.enabled = false;
            }
        }

        void Update()
        {
            // Growth phase
            if (!isGrown && hasLightSource)
            {
                growthTimer += Time.deltaTime;
                if (growthTimer >= growthTime)
                {
                    Grow();
                }
            }

            // O2 refill while player in range and plant is grown
            if (isGrown && playerInRange)
            {
                if (PlayerVitals.Instance != null)
                {
                    PlayerVitals.Instance.RefillOxygen(o2RefillRate * Time.deltaTime);
                }
            }
        }

        // ── Growth ──────────────────────────────

        private void Grow()
        {
            isGrown = true;
            growthTimer = growthTime;

            if (sr != null && grownSprite != null)
                sr.sprite = grownSprite;

            // Enable glow
            if (glowRenderer != null)
                glowRenderer.enabled = true;
        }

        /// <summary>
        /// Set light availability (used by Shadow Canyon / UV Light Pillar system).
        /// Plant stops growing without light, resumes when light is provided.
        /// </summary>
        public void SetLightSource(bool hasLight)
        {
            hasLightSource = hasLight;
        }

        // ── Trigger ─────────────────────────────

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && isGrown)
            {
                playerInRange = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
    }
}
