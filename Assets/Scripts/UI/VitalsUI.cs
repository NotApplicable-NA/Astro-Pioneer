using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AstroPioneer.Systems.Survival;

namespace AstroPioneer.UI
{
    /// <summary>
    /// HUD display for Oxygen bar ONLY.
    /// GDD DESIGN LOCK: No HP bar. Player cannot die.
    /// </summary>
    public class VitalsUI : MonoBehaviour
    {
        private readonly System.Text.StringBuilder o2Buffer = new System.Text.StringBuilder(32);
        private readonly System.Text.StringBuilder fatigueBuffer = new System.Text.StringBuilder(32);
        
        [Header("Oxygen Bar")]
        [SerializeField] private Slider oxygenBar;
        [SerializeField] private Image oxygenFill;
        [SerializeField] private TextMeshProUGUI oxygenText;

        [Header("Warnings")]
        [SerializeField] private GameObject lowO2Warning;
        [SerializeField] private GameObject fatigueWarning;
        [SerializeField] private TextMeshProUGUI fatigueText;

        [Header("Colors")]
        [SerializeField] private Color o2NormalColor = Color.cyan;
        [SerializeField] private Color o2LowColor = new Color(1f, 0.5f, 0f);

        [Header("Visibility")]
        [SerializeField] private GameObject vitalsPanel;
        [SerializeField] private bool showOnlyOnPlanet = true;

        private bool eventsSubscribed = false;

        void Start()
        {
            // Auto-find fallback for QA testing
            if (fatigueWarning == null)
            {
                Transform fw = transform.Find("FatigueWarning");
                if (fw == null && vitalsPanel != null) fw = vitalsPanel.transform.Find("FatigueWarning");
                if (fw != null)
                {
                    fatigueWarning = fw.gameObject;
                    if (fatigueText == null) fatigueText = fatigueWarning.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                }
            }

            if (lowO2Warning != null) lowO2Warning.SetActive(false);
            if (fatigueWarning != null) fatigueWarning.SetActive(false);

            if (showOnlyOnPlanet && vitalsPanel != null)
                vitalsPanel.SetActive(false);

            TrySubscribe();
        }

        void Update()
        {
            if (!eventsSubscribed) TrySubscribe();
        }

        void OnDestroy()
        {
            if (PlayerVitals.Instance != null && eventsSubscribed)
            {
                PlayerVitals.Instance.OnOxygenChanged -= UpdateOxygen;
                PlayerVitals.Instance.OnLowOxygen -= ShowLowO2Warning;
                PlayerVitals.Instance.OnFatigueMultiplierChanged -= UpdateFatigueUI;
            }
        }

        private void TrySubscribe()
        {
            if (eventsSubscribed || PlayerVitals.Instance == null) return;

            PlayerVitals.Instance.OnOxygenChanged += UpdateOxygen;
            PlayerVitals.Instance.OnLowOxygen += ShowLowO2Warning;
            PlayerVitals.Instance.OnFatigueMultiplierChanged += UpdateFatigueUI;

            // Subscribe to exploration for visibility toggle
            if (Systems.Exploration.ExplorationManager.Instance != null)
            {
                Systems.Exploration.ExplorationManager.Instance.OnExplorationStarted += (_) => ShowVitals(true);
                Systems.Exploration.ExplorationManager.Instance.OnReturnedToShip += () => ShowVitals(false);
            }

            eventsSubscribed = true;

            // Initial update
            UpdateOxygen(PlayerVitals.Instance.CurrentOxygen, PlayerVitals.Instance.MaxOxygen);
            UpdateFatigueUI(PlayerVitals.Instance.GetFatigueMultiplier());
        }

        // ── Display Updates ─────────────────────────

        private void UpdateOxygen(float current, float max)
        {
            float percent = max > 0 ? current / max : 0f;
            if (oxygenBar != null) oxygenBar.value = percent;
            if (oxygenText != null) 
            {
                o2Buffer.Clear();
                o2Buffer.Append(Mathf.CeilToInt(current));
                o2Buffer.Append('/');
                o2Buffer.Append(Mathf.CeilToInt(max));
                oxygenText.SetText(o2Buffer);
            }
            if (oxygenFill != null) oxygenFill.color = percent <= 0.25f ? o2LowColor : o2NormalColor;

            if (lowO2Warning != null) lowO2Warning.SetActive(percent <= 0.25f && percent > 0f);
        }

        private void ShowLowO2Warning()
        {
            if (lowO2Warning != null) lowO2Warning.SetActive(true);
        }

        private void UpdateFatigueUI(float multiplier)
        {
            bool isFatigued = multiplier > 1.05f; // Small buffer for float precision
            
            if (fatigueWarning != null) fatigueWarning.SetActive(isFatigued);
            
            if (fatigueText != null)
            {
                if (isFatigued)
                {
                    fatigueBuffer.Clear();
                    fatigueBuffer.Append("FATIGUED: ");
                    // Simple manual rounding for float without .ToString("F2") which allocates
                    int rX = Mathf.FloorToInt(multiplier);
                    int rDec = Mathf.FloorToInt((multiplier - rX) * 100);
                    fatigueBuffer.Append(rX);
                    fatigueBuffer.Append('.');
                    if (rDec < 10) fatigueBuffer.Append('0');
                    fatigueBuffer.Append(rDec);
                    fatigueBuffer.Append("x Drain");
                    
                    fatigueText.SetText(fatigueBuffer);
                    fatigueText.color = Color.yellow;
                }
                else
                {
                    fatigueText.text = "";
                }
            }
        }

        private void ShowVitals(bool show)
        {
            if (showOnlyOnPlanet && vitalsPanel != null)
                vitalsPanel.SetActive(show);
        }
    }
}
