using UnityEngine;
using System.Collections;
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

        public void Interact(AstroPioneer.Data.InventoryItem heldItem)
        {
            if (isSleeping) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) > interactionDistance)
                return;

            StartCoroutine(SleepSequence());
        }

        private IEnumerator SleepSequence()
        {
            isSleeping = true;

            yield return new WaitForSeconds(sleepDuration * 0.5f);

            TimeManager.Instance?.SkipToMorning();
            yield return new WaitForSeconds(0.1f);

            if (PlayerVitals.Instance != null)
            {
                PlayerVitals.Instance.ResetFatigue();
                PlayerVitals.Instance.FullRestore();
            }

            yield return new WaitForSeconds(sleepDuration * 0.5f);

            isSleeping = false;
        }
    }
}
