using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AstroPioneer.Interfaces;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Survival;

namespace AstroPioneer.Machines
{
    /// <summary>
    /// SleepPod — Ship interactable that resets fatigue and skips time to morning.
    /// </summary>
    public class SleepPod : MonoBehaviour, IGridInteractable
    {
        [Header("Settings")]
        [SerializeField] private float sleepDuration = 2.0f;
        [SerializeField] private float interactionDistance = 2.5f;

        private bool isSleeping;
        private Transform playerTransform;

        void Start()
        {
            if (AstroPioneer.Data.StructureRegistry.Instance != null)
            {
                Vector2Int gridPos = new Vector2Int(
                    Mathf.FloorToInt(transform.position.x),
                    Mathf.FloorToInt(transform.position.y));
                AstroPioneer.Data.StructureRegistry.Instance.RegisterSleepPodPosition(gridPos);
            }
        }

        // Global cache for WaitForSeconds to prevent GC allocation in Coroutines
        private static readonly Dictionary<float, WaitForSeconds> waitCache = new Dictionary<float, WaitForSeconds>();
        private static WaitForSeconds GetWait(float seconds)
        {
            if (!waitCache.TryGetValue(seconds, out var wait))
            {
                wait = new WaitForSeconds(seconds);
                waitCache[seconds] = wait;
            }
            return wait;
        }

        public void Interact(AstroPioneer.Data.InventoryItem heldItem)
        {
            if (isSleeping) return;

            if (playerTransform == null && AstroPioneer.Player.PlayerToolState.Instance != null)
                playerTransform = AstroPioneer.Player.PlayerToolState.Instance.transform;

            if (playerTransform != null)
            {
                Vector3 delta = transform.position - playerTransform.position;
                if (delta.sqrMagnitude > interactionDistance * interactionDistance)
                    return;
            }
            else
                return;

            StartCoroutine(SleepSequence());
        }

        private IEnumerator SleepSequence()
        {
            isSleeping = true;

            yield return GetWait(sleepDuration * 0.5f);

            TimeManager.Instance?.SkipToMorning();
            yield return GetWait(0.1f);

            if (PlayerVitals.Instance != null)
            {
                PlayerVitals.Instance.ResetFatigue();
                PlayerVitals.Instance.FullRestore();
            }

            yield return GetWait(sleepDuration * 0.5f);

            isSleeping = false;
        }
    }
}
