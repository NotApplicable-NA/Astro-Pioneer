using UnityEngine;
using System;
using System.Text;
using AstroPioneer.Core;

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
        public float TotalGameSeconds => (daysPassed * 86400f) + (currentTime * 86400f);
        
        // Events
        public event Action<float> OnTimeChanged;
        public event Action<int> OnDayChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; ServiceLocator.Unregister<TimeManager>(); }
        }

        // Throttling State
        private int lastMinuteCheck = -1;
        private readonly StringBuilder timeBuffer = new StringBuilder(5);

        void Update()
        {
            // Calculate time increment based on frame time
            float timeIncrement = Time.deltaTime / realSecondsPerDay;
            
            currentTime += timeIncrement;

            // Check for day rollover
            if (currentTime >= 1f)
            {
                currentTime -= 1f;
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
            timeBuffer.Clear();
            if (h < 10) timeBuffer.Append('0');
            timeBuffer.Append(h);
            timeBuffer.Append(':');
            if (m < 10) timeBuffer.Append('0');
            timeBuffer.Append(m);
            return timeBuffer.ToString();
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

        /// <summary>
        /// Restores saved time and day from SaveGameManager.
        /// </summary>
        public void LoadTime(int savedDays, float savedTime)
        {
            daysPassed = savedDays;
            currentTime = savedTime;
            lastMinuteCheck = -1; // Force UI update

            OnDayChanged?.Invoke(daysPassed);
            OnTimeChanged?.Invoke(currentTime);
        }
    }
}
