using UnityEngine;

namespace AstroPioneer.VFX
{
    /// <summary>
    /// GrowthTransitionVFX - One-shot sparkle animation for crop growth stage transitions.
    /// 6 frames, upward sparkle burst.
    /// </summary>
    public class GrowthTransitionVFX : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private Sprite[] growthSprites; // 6 frames
        [SerializeField] private float frameRate = 12f; // 12 FPS
        [SerializeField] private float duration = 0.5f; // 6 frames @ 12 FPS
        
        private SpriteRenderer spriteRenderer;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private float elapsedTime = 0f;
        private bool isPlaying = false;
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            spriteRenderer.sortingLayerName = "VFX";
            spriteRenderer.sortingOrder = 3;
            spriteRenderer.enabled = false;
        }
        
        void Update()
        {
            if (!isPlaying) return;

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= duration)
            {
                StopVFX();
                return;
            }

            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer -= 1f / frameRate;
                currentFrame++;
                
                if (currentFrame >= growthSprites.Length)
                {
                    StopVFX();
                    return;
                }
                
                if (growthSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = growthSprites[currentFrame];
                }
            }
        }
        
        public void PlayAtPosition(Vector3 worldPos)
        {
            if (growthSprites == null || growthSprites.Length == 0)
            {
                return;
            }
            
            transform.position = worldPos;
            isPlaying = true;
            enabled = true;
            currentFrame = 0;
            frameTimer = 0f;
            elapsedTime = 0f;
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = growthSprites[0];
        }

        void StopVFX()
        {
            isPlaying = false;
            spriteRenderer.enabled = false;
            enabled = false;
            currentFrame = 0;
        }
    }
}
