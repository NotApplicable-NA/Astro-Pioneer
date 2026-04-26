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
        private int lastOxygenInt = -1;

        // Events
        public event Action<float, float> OnOxygenChanged;
        public event Action OnLowOxygen;
        public event Action OnOxygenDepleted;
        public event Action<float> OnFatigueMultiplierChanged;

        // V25 Oasis Tracking
        private struct Oasis
        {
            public Vector2 position;
            public float radiusSqr;
            public float refillRate;
        }
        private readonly System.Collections.Generic.List<Oasis> activeOases = new System.Collections.Generic.List<Oasis>();

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
            lastOxygenInt = Mathf.CeilToInt(currentOxygen);
        }

        void OnDestroy()
        {
            if (ExplorationManager.Instance != null)
            {
                ExplorationManager.Instance.OnExplorationStarted -= OnEnterPlanet;
                ExplorationManager.Instance.OnReturnedToShip -= OnLeavePlanet;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayChanged -= OnDayRollover;
            }

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

            // 1. Math-based Oasis checking (V25 QA)
            Vector2 playerPos = AstroPioneer.Data.EntityRegistry.Instance != null ? 
                                (Vector2)AstroPioneer.Data.EntityRegistry.Instance.GetEntityPosition("PLAYER_MAIN") : Vector2.zero;
            
            float refillAmount = 0f;
            foreach (var oasis in activeOases)
            {
                if ((playerPos - oasis.position).sqrMagnitude <= oasis.radiusSqr)
                {
                    refillAmount += oasis.refillRate;
                }
            }

            if (refillAmount > 0f)
            {
                currentOxygen = Mathf.Min(currentOxygen + refillAmount * Time.deltaTime, maxOxygen);
            }

            // 2. Normal Drain
            float drain = baseOxygenDrain;
            if (ExplorationManager.Instance?.CurrentPlanet != null)
                drain = ExplorationManager.Instance.CurrentPlanet.oxygenDrainRate;

            drain *= GetFatigueMultiplier();
            currentOxygen = Mathf.Max(0f, currentOxygen - drain * Time.deltaTime);
            EmitOxygenChangedIfNeeded();

            if (OxygenPercent <= 0.25f && OxygenPercent > 0f)
                OnLowOxygen?.Invoke();

            if (currentOxygen <= 0f)
            {
                isIncapacitated = true;
                OnOxygenDepleted?.Invoke();
            }
        }

        // ─── Public API ───

        public void RegisterOasis(Vector2 position, float radius, float refillRate)
        {
            activeOases.Add(new Oasis { position = position, radiusSqr = radius * radius, refillRate = refillRate });
        }

        public void UnregisterOasis(Vector2 position)
        {
            activeOases.RemoveAll(o => o.position == position);
        }

        public void RefillOxygen(float amount)
        {
            currentOxygen = Mathf.Min(currentOxygen + amount, maxOxygen);
            EmitOxygenChangedIfNeeded(force: true);
        }

        public void FullRestore()
        {
            currentOxygen = maxOxygen;
            isIncapacitated = false;
            EmitOxygenChangedIfNeeded(force: true);
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
            EmitOxygenChangedIfNeeded(force: true);
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

        private void EmitOxygenChangedIfNeeded(bool force = false)
        {
            int currentInt = Mathf.CeilToInt(currentOxygen);
            if (force || currentInt != lastOxygenInt)
            {
                lastOxygenInt = currentInt;
                OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
            }
        }
    }
}
