using UnityEngine;
using System;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float maxOxygen = 100f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float maxWater = 100f;

        [Header("Current Values")]
        [SerializeField] private float currentOxygen;
        [SerializeField] private float currentEnergy;
        [SerializeField] private float currentWater;

        // Events for UI updates
        public event Action<float, float> OnOxygenChanged; // current, max
        public event Action<float, float> OnEnergyChanged;
        public event Action<float, float> OnWaterChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            ServiceLocator.Register(this);

            // Initialize
            currentOxygen = maxOxygen;
            currentEnergy = maxEnergy;
            currentWater = maxWater;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        void Start()
        {
             // Broadcast initial values
             UpdateAllUI();
        }

        public void ConsumeOxygen(float amount)
        {
            currentOxygen = Mathf.Clamp(currentOxygen - amount, 0, maxOxygen);
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        public void ConsumeWater(float amount)
        {
            currentWater = Mathf.Clamp(currentWater - amount, 0, maxWater);
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }
        
        public void RegenerateEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        // Debug / Dev Tools
        public void UpdateAllUI()
        {
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }


    }
}
