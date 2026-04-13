using UnityEngine;
using System;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Exploration;

namespace AstroPioneer.Systems.Survival
{
    /// <summary>
    /// PlayerVitals — Manages oxygen and fatigue.
    /// Design Lock: No HP/death. O2 depletion triggers Bot-E rescue with zero penalty.
    /// </summary>
    public class PlayerVitals : MonoBehaviour
    {
        public static PlayerVitals Instance { get; private set; }

        [Header("Oxygen")]
        [SerializeField] private float maxOxygen = 100f;
        [SerializeField] private float currentOxygen;

        [Header("O2 Drain")]
        [SerializeField] private float baseOxygenDrain = 1f;

        [Header("Fatigue")]
        [SerializeField] private int daysSinceLastSleep = 1;
        [SerializeField] private float fatigueScalingPerDay = 0.25f;

        [Header("State")]
        [SerializeField] private bool isOnPlanet;
        [SerializeField] private bool isIncapacitated;

        // Events
        public event Action<float, float> OnOxygenChanged;
        public event Action OnLowOxygen;
        public event Action OnOxygenDepleted;
        public event Action<float> OnFatigueMultiplierChanged;

        // Properties
        public float CurrentOxygen => currentOxygen;
        public float MaxOxygen => maxOxygen;
        public float OxygenPercent => maxOxygen > 0 ? currentOxygen / maxOxygen : 0f;
        public bool IsOnPlanet => isOnPlanet;
        public bool IsIncapacitated => isIncapacitated;
        public int DaysSinceLastSleep => daysSinceLastSleep;
        public float GetFatigueMultiplier() => 1.0f + Mathf.Max(0, daysSinceLastSleep - 1) * fatigueScalingPerDay;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            currentOxygen = maxOxygen;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (ExplorationManager.Instance != null)
            {
                ExplorationManager.Instance.OnExplorationStarted += OnEnterPlanet;
                ExplorationManager.Instance.OnReturnedToShip += OnLeavePlanet;
            }
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged += OnDayRollover;
        }

        void Update()
        {
            if (isIncapacitated || !isOnPlanet || currentOxygen <= 0) return;

            float drain = baseOxygenDrain;
            if (ExplorationManager.Instance?.CurrentPlanet != null)
                drain = ExplorationManager.Instance.CurrentPlanet.oxygenDrainRate;

            drain *= GetFatigueMultiplier();
            currentOxygen = Mathf.Max(0f, currentOxygen - drain * Time.deltaTime);
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);

            if (OxygenPercent <= 0.25f && OxygenPercent > 0f)
                OnLowOxygen?.Invoke();

            if (currentOxygen <= 0f)
            {
                isIncapacitated = true;
                OnOxygenDepleted?.Invoke();
            }
        }

        // ─── Public API ───

        public void RefillOxygen(float amount)
        {
            currentOxygen = Mathf.Min(currentOxygen + amount, maxOxygen);
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        }

        public void FullRestore()
        {
            currentOxygen = maxOxygen;
            isIncapacitated = false;
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        }

        public void ResetFatigue()
        {
            daysSinceLastSleep = 1;
            OnFatigueMultiplierChanged?.Invoke(GetFatigueMultiplier());
        }

        // ─── Internal ───

        private void OnEnterPlanet(Data.PlanetData planet)
        {
            isOnPlanet = true;
            currentOxygen = maxOxygen;
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        }

        private void OnLeavePlanet()
        {
            isOnPlanet = false;
            FullRestore();
        }

        private void OnDayRollover(int totalDays)
        {
            daysSinceLastSleep++;
            OnFatigueMultiplierChanged?.Invoke(GetFatigueMultiplier());
        }
    }
}
