using UnityEngine;

namespace AstroPioneer.VFX
{
    /// <summary>
    /// WateringVFX - One-shot water splash animation for watering action.
    /// 8 frames, plays when player waters a crop.
    /// </summary>
    public class WateringVFX : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private Sprite[] wateringSprites; // 8 frames
        [SerializeField] private float frameRate = 12f; // 12 FPS
        [SerializeField] private float duration = 0.67f; // 8 frames @ 12 FPS
        
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
            spriteRenderer.sortingOrder = 3; // Above crops
            spriteRenderer.enabled = false;
            
            // Scale to match grid cell size (same as crops)
            transform.localScale = Vector3.one * 0.03f;
        }
        
        void Update()
        {
            if (!isPlaying) return;
            
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;
                currentFrame++;
                
                if (currentFrame >= wateringSprites.Length)
                {
                    StopVFX();
                    return;
                }
                
                if (wateringSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = wateringSprites[currentFrame];
                }
            }
        }
        
        public void PlayAtPosition(Vector3 worldPos)
        {
            if (wateringSprites == null || wateringSprites.Length == 0)
            {
                Debug.LogWarning("[WateringVFX] No sprites assigned!", this);
                return;
            }
            
            transform.position = worldPos;
            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = wateringSprites[0];
            
            Invoke(nameof(StopVFX), duration);
        }
        
        void StopVFX()
        {
            CancelInvoke(nameof(StopVFX));
            isPlaying = false;
            spriteRenderer.enabled = false;
            currentFrame = 0;
        }
    }
}
