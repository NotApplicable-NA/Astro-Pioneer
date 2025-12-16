using UnityEngine;

namespace AstroPioneer.VFX
{
    /// <summary>
    /// HarvestableGlow - Looping pulse animation for harvestable crops.
    /// 4 frames, continuous loop at stage 3.
    /// </summary>
    public class HarvestableGlow : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private Sprite[] glowSprites; // 4 frames
        [SerializeField] private float frameRate = 8f; // 8 FPS for smooth pulse
        
        private SpriteRenderer spriteRenderer;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            spriteRenderer.sortingLayerName = "VFX";
            spriteRenderer.sortingOrder = 1; // Behind crop
            spriteRenderer.enabled = false;
        }
        
        void Update()
        {
            if (!isPlaying) return;
            
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;
                currentFrame = (currentFrame + 1) % glowSprites.Length; // Loop
                
                if (glowSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = glowSprites[currentFrame];
                }
            }
        }
        
        public void StartGlow()
        {
            if (glowSprites == null || glowSprites.Length == 0)
            {
                Debug.LogWarning("[HarvestableGlow] No sprites assigned!", this);
                return;
            }
            
            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;
            spriteRenderer.enabled = true;
            if (glowSprites[0] != null)
            {
                spriteRenderer.sprite = glowSprites[0];
            }
        }
        
        public void StopGlow()
        {
            isPlaying = false;
            spriteRenderer.enabled = false;
            currentFrame = 0;
        }
    }
}
