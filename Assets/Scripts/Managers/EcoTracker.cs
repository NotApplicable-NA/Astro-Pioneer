using UnityEngine;
using System;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// EcoTracker — Tracks ecological footprint (0-100) and triggers end-game
    /// when both Trust and Eco thresholds are met.
    /// </summary>
    public class EcoTracker : MonoBehaviour
    {
        public static EcoTracker Instance { get; private set; }

        [Header("Eco Balance")]
        [SerializeField] private float currentEcoScore = 50f;
        [SerializeField] private float requiredEndGameTrust = 10000f;
        [SerializeField] private float requiredEndGameEco = 80f;

        public event Action OnEndGameTriggered;
        private bool hasTriggeredEndGame;

        public float CurrentEcoScore => currentEcoScore;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddEcoScore(float amount)
        {
            currentEcoScore = Mathf.Clamp(currentEcoScore + amount, 0f, 100f);
            CheckEndGameConditions();
        }

        private void CheckEndGameConditions()
        {
            if (hasTriggeredEndGame) return;
            if (CurrencyManager.Instance == null) return;

            if (CurrencyManager.Instance.HasTrust((int)requiredEndGameTrust) && currentEcoScore >= requiredEndGameEco)
            {
                hasTriggeredEndGame = true;
                OnEndGameTriggered?.Invoke();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying && !hasTriggeredEndGame)
                CheckEndGameConditions();
        }
#endif
    }
}
