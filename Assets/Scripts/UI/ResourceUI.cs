using UnityEngine;
using UnityEngine.UI;
using AstroPioneer.Managers;

namespace AstroPioneer.UI
{
    public class ResourceUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private Slider energySlider;
        [SerializeField] private Slider waterSlider;

        void OnEnable()
        {
            // Subscribe to events
            if (ResourceManager.Instance != null)
            {
                SubscribeEvents();
            }
        }
        
        void Start()
        {
            // Safety check if Manager initialized after UI
            if (ResourceManager.Instance != null)
            {
                SubscribeEvents();
                ResourceManager.Instance.UpdateAllUI();
            }
        }

        void OnDisable()
        {
             if (ResourceManager.Instance != null)
             {
                 ResourceManager.Instance.OnOxygenChanged -= UpdateOxygen;
                 ResourceManager.Instance.OnEnergyChanged -= UpdateEnergy;
                 ResourceManager.Instance.OnWaterChanged -= UpdateWater;
             }
        }
        
        private void SubscribeEvents()
        {
            // Unsub first to prevent double subscription
            ResourceManager.Instance.OnOxygenChanged -= UpdateOxygen;
            ResourceManager.Instance.OnEnergyChanged -= UpdateEnergy;
            ResourceManager.Instance.OnWaterChanged -= UpdateWater;
            
            ResourceManager.Instance.OnOxygenChanged += UpdateOxygen;
            ResourceManager.Instance.OnEnergyChanged += UpdateEnergy;
            ResourceManager.Instance.OnWaterChanged += UpdateWater;
        }

        private void UpdateOxygen(float current, float max)
        {
            if (oxygenSlider != null) oxygenSlider.value = current / max;
        }

        private void UpdateEnergy(float current, float max)
        {
            if (energySlider != null) energySlider.value = current / max;
        }

        private void UpdateWater(float current, float max)
        {
            if (waterSlider != null) waterSlider.value = current / max;
        }
    }
}
