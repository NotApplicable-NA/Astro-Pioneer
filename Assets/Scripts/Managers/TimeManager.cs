using UnityEngine;
using System;

namespace AstroPioneer.Managers
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [Tooltip("How many real-time seconds make up one in-game day")]
        [SerializeField] private float realSecondsPerDay = 900f; // 15 minutes
        
        [Header("Current Time")]
        [Range(0f, 1f)]
        [SerializeField] private float currentTime = 0.25f; // 0.0 = Midnight, 0.25 = 6AM, 0.5 = Noon
        [SerializeField] private int daysPassed = 1;
        
        // Public Properties
        public float CurrentTime => currentTime;
        public int DaysPassed => daysPassed;
        
        // Events
        public event Action<float> OnTimeChanged;
        public event Action<int> OnDayChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // Throttling State
        private int lastMinuteCheck = -1;

        void Update()
        {
            // Calculate time increment based on frame time
            float timeIncrement = Time.deltaTime / realSecondsPerDay;
            
            currentTime += timeIncrement;

            // Check for day rollover
            if (currentTime >= 1f)
            {
                currentTime = 0f;
                daysPassed++;
                OnDayChanged?.Invoke(daysPassed);
            }
            
            // Optimization: Only trigger event when minute changes, not every frame
            // 1 Day = 1440 Minutes
            int currentMinuteTotal = Mathf.FloorToInt(currentTime * 1440f);
            if (currentMinuteTotal != lastMinuteCheck)
            {
                lastMinuteCheck = currentMinuteTotal;
                OnTimeChanged?.Invoke(currentTime);
            }
        }
        
        // Helper to get formatted string (e.g., "06:30")
        public string GetFormattedTime()
        {
            float hours = currentTime * 24f;
            int h = Mathf.FloorToInt(hours);
            int m = Mathf.FloorToInt((hours - h) * 60f);
            return $"{h:00}:{m:00}";
        }

        /// <summary>
        /// Skip time to 06:00 AM.
        /// If currently before 06:00 AM, skip to 06:00 AM today.
        /// If currently after 06:00 AM, skip to 06:00 AM next day.
        /// </summary>
        public void SkipToMorning()
        {
            float targetTime = 0.25f; // 6 AM
            
            if (currentTime >= targetTime)
            {
                // Day rollover
                daysPassed++;
                OnDayChanged?.Invoke(daysPassed);
            }
            
            currentTime = targetTime;
            lastMinuteCheck = -1; // Force update
            OnTimeChanged?.Invoke(currentTime);
        }
    }
}
