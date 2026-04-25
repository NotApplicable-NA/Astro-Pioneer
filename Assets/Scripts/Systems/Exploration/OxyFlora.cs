using UnityEngine;
using AstroPioneer.Systems.Survival;

namespace AstroPioneer.Systems.Exploration
{
    /// <summary>
    /// Oxy-Flora: Oxygen-producing plant placeable on planet surfaces.
    /// GDD 3.5: Creates "oxygen oasis" zones that refill player O2 when nearby.
    /// V25: Fully Data-Driven. Registers Oasis with PlayerVitals mathematically, no Physics2D.
    /// </summary>
    public class OxyFlora : MonoBehaviour
    {
        [Header("Oxygen Production")]
        [Tooltip("O2 refill per second while player is in range")]
        [SerializeField] private float o2RefillRate = 3f;

        [Tooltip("Radius of the oxygen oasis (math check)")]
        [SerializeField] private float oasisRadius = 2f;

        [Header("Growth")]
        [Tooltip("Time in seconds to grow from planted to active")]
        [SerializeField] private float growthTime = 30f;

        [Header("Light Requirement")]
        [Tooltip("Does this plant have access to sunlight/UV light?")]
        [SerializeField] private bool hasLightSource = true;

        [Header("State")]
        [SerializeField] private bool isGrown = false;

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
            // Start as seedling
            if (!isGrown)
            {
                if (sr != null && seedlingSprite != null)
                    sr.sprite = seedlingSprite;
                if (glowRenderer != null)
                    glowRenderer.enabled = false;
            }
            else
            {
                // If loaded already grown
                if (PlayerVitals.Instance != null)
                    PlayerVitals.Instance.RegisterOasis(transform.position, oasisRadius, o2RefillRate);
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
        }

        void OnDestroy()
        {
            if (isGrown && PlayerVitals.Instance != null)
                PlayerVitals.Instance.UnregisterOasis(transform.position);
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

            // Register Oasis for Data-Driven Distance Check
            if (PlayerVitals.Instance != null)
                PlayerVitals.Instance.RegisterOasis(transform.position, oasisRadius, o2RefillRate);
        }

        /// <summary>
        /// Set light availability (used by Shadow Canyon / UV Light Pillar system).
        /// Plant stops growing without light, resumes when light is provided.
        /// </summary>
        public void SetLightSource(bool hasLight)
        {
            hasLightSource = hasLight;
        }
    }
}
