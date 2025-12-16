using UnityEngine;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// VFX component for Sprinkler water spray animation.
    /// Plays sprite sheet animation when sprinkler activates.
    /// </summary>
    public class SprinklerVFX : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private Sprite[] waterSprites; // 8 frames dari sprite sheet
        [SerializeField] private float frameRate = 8f; // 8 FPS
        [SerializeField] private float duration = 2.5f; // 2-3 seconds
        
        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "VFX";
        [SerializeField] private int orderInLayer = 1;
        
        private SpriteRenderer spriteRenderer;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;
        
        void Awake()
        {
            // Setup sprite renderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // Configure rendering
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = orderInLayer;
            spriteRenderer.enabled = false; // Hidden by default
        }
        
        void OnEnable()
        {
            // Subscribe ke sprinkler activation event
            Sprinkler.OnSprinklerActivated += HandleSprinklerActivated;
        }
        
        void OnDisable()
        {
            // Unsubscribe untuk prevent memory leaks
            Sprinkler.OnSprinklerActivated -= HandleSprinklerActivated;
        }
        
        void Update()
        {
            if (!isPlaying) return;
            
            // Animate sprite frames
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;  // ✅ FIXED: Preserve remainder for smooth animation
                currentFrame++;
                
                // Loop animation
                if (currentFrame >= waterSprites.Length)
                {
                    currentFrame = 0;
                }
                
                spriteRenderer.sprite = waterSprites[currentFrame];
            }
        }
        
        void HandleSprinklerActivated(Sprinkler sprinkler)
        {
            // Only play VFX jika ini sprinkler yang benar
            if (sprinkler.transform == transform.parent)
            {
                PlayVFX();
            }
        }
        
        /// <summary>
        /// Play animation dengan duration tertentu
        /// </summary>
        public void PlayVFX()
        {
            if (waterSprites == null || waterSprites.Length == 0)
            {
                Debug.LogWarning("[SprinklerVFX] Water sprites not assigned!", this);
                return;
            }
            
            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;
            spriteRenderer.enabled = true;
            enabled = true;  // ✅ OPTIMIZATION: Enable Update() calls
            
            // Auto-stop setelah duration
            Invoke(nameof(StopVFX), duration);
        }
        
        /// <summary>
        /// Stop animation dan hide sprite
        /// </summary>
        public void StopVFX()
        {
            CancelInvoke(nameof(StopVFX));  // ✅ FIXED: Clear pending invocations
            isPlaying = false;
            spriteRenderer.enabled = false;
            currentFrame = 0;
            enabled = false;  // ✅ OPTIMIZATION: Stop Update() calls
        }
        
        // Debug helper - test VFX dari Inspector
        [ContextMenu("Test VFX")]
        void TestVFX()
        {
            PlayVFX();
        }
    }
}
