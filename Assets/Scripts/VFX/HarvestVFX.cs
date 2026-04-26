using UnityEngine;

namespace AstroPioneer.VFX
{
    /// <summary>
    /// HarvestVFX - One-shot sparkle burst animation for harvest action.
    /// Plays when player harvests a mature crop.
    /// </summary>
    public class HarvestVFX : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private Sprite[] harvestSprites; // Harvest sparkle frames
        [SerializeField] private float frameRate = 15f; // 15 FPS for quick burst
        [SerializeField] private float duration = 0.5f; // Quick sparkle burst
        
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
            spriteRenderer.sortingOrder = 4; // Above everything
            spriteRenderer.enabled = false;
            
            // Scale to match grid cell size (same as crops)
            transform.localScale = Vector3.one * 0.03f;
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
                
                if (currentFrame >= harvestSprites.Length)
                {
                    StopVFX();
                    return;
                }
                
                if (harvestSprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = harvestSprites[currentFrame];
                }
            }
        }
        
        public void PlayAtPosition(Vector3 worldPos)
        {
            if (harvestSprites == null || harvestSprites.Length == 0)
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
            spriteRenderer.sprite = harvestSprites[0];
        }

        void StopVFX()
        {
            isPlaying = false;
            spriteRenderer.enabled = false;
            enabled = false;
            currentFrame = 0;

            if (Managers.ObjectPoolManager.Instance != null)
                Managers.ObjectPoolManager.Instance.ReturnToPool(Core.GameConstants.POOL_HARVEST_VFX, gameObject);
            else
                Destroy(gameObject);
        }
    }
}
