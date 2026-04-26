using UnityEngine;
using AstroPioneer.Data;
using AstroPioneer.Managers;

namespace AstroPioneer.Systems.Exploration
{
    /// <summary>
    /// A mineable/gatherable resource node on a planet.
    /// Player hits it to collect resources.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    // Physics attributes removed for DOD architecture
    public class ResourceNode : MonoBehaviour, AstroPioneer.Interfaces.IGridInteractable
    {
        [Header("State")]
        [SerializeField] private int currentHP;
        [SerializeField] private bool isDepleted = false;
        private Coroutine activeShakeCoroutine;

        private Vector2Int gridPos;
        // Data from spawn entry
        private InventoryItem resource;
        private int minDrop;
        private int maxDrop;
        private int maxHP;

        private SpriteRenderer sr;

        // Properties
        public bool IsDepleted => isDepleted;
        public float HPPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void DestroyNode()
        {
        }

        void OnDestroy()
        {
        }

        /// <summary>
        /// Initialize from spawn data. Called by ExplorationManager.
        /// </summary>
        public void Initialize(ResourceSpawnEntry entry)
        {
            if (entry == null || entry.resource == null) return;

            resource = entry.resource;
            minDrop = entry.minDrop;
            maxDrop = entry.maxDrop;
            maxHP = entry.hitPoints;
            currentHP = maxHP;
            isDepleted = false;

            // Set sprite from resource icon if available
            if (sr != null && resource.icon != null)
                sr.sprite = resource.icon;

            TryRegisterToGrid();
        }

        private void TryRegisterToGrid()
        {
        }

        public void Interact(AstroPioneer.Data.InventoryItem heldItem)
        {
            Hit(1);
        }

        /// <summary>
        /// Hit the node (called by player mining tool).
        /// </summary>
        public void Hit(int damage = 1)
        {
            if (isDepleted) return;

            currentHP -= damage;

            // Visual feedback: shake
            if (activeShakeCoroutine != null) StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = StartCoroutine(ShakeEffect());

            if (currentHP <= 0)
            {
                Harvest();
            }
        }

        private void Harvest()
        {
            if (isDepleted) return;
            isDepleted = true;

            // Calculate drop amount
            int dropQty = Random.Range(minDrop, maxDrop + 1);

            // Add to inventory
            if (InventoryManager.Instance != null && resource != null)
            {
                bool added = InventoryManager.Instance.AddItem(resource, dropQty);
                if (added)
                {
                }
                else
                {
                }
            }

            // Visual: fade out and destroy
            StartCoroutine(DestroySequence());
        }

        private System.Collections.IEnumerator ShakeEffect()
        {
            Vector3 originalPos = transform.position;
            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                float offsetX = Random.Range(-0.05f, 0.05f);
                float offsetY = Random.Range(-0.05f, 0.05f);
                transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;
            activeShakeCoroutine = null;
        }

        private System.Collections.IEnumerator DestroySequence()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Color startColor = sr.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
