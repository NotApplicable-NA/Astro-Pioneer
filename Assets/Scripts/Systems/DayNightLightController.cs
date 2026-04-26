using UnityEngine;
using UnityEngine.Rendering.Universal; // For Light2D
using AstroPioneer.Managers;

namespace AstroPioneer.Systems
{
    public class DayNightLightController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Light2D globalLight;
        
        [Header("Color Gradient")]
        [Tooltip("Gradient representing light color over a Full Day (0.0 to 1.0)")]
        [SerializeField] private Gradient dayNightGradient;
        
        void Start()
        {
            if (globalLight == null)
            {
                globalLight = GetComponent<Light2D>();
            }
            
            if (TimeManager.Instance != null)
            {
                // Initial update
                UpdateLight(TimeManager.Instance.CurrentTime);
                TimeManager.Instance.OnTimeChanged += UpdateLight; // Event-driven
            }
        }

        void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged -= UpdateLight;
            }
        }

        private void UpdateLight(float timePercent)
        {
            if (dayNightGradient != null && globalLight != null)
            {
                Color targetColor = dayNightGradient.Evaluate(timePercent);
                globalLight.color = targetColor;
            }
        }
    }
}
