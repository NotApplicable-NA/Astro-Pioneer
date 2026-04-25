using UnityEngine;
using TMPro;
using AstroPioneer.Managers;

namespace AstroPioneer.UI
{
    public class EconomyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI trustText;

        void OnEnable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCreditsChanged += UpdateCreditsDisplay;
                CurrencyManager.Instance.OnTrustChanged += UpdateTrustDisplay;
                
                UpdateCreditsDisplay(CurrencyManager.Instance.CurrentCredits);
                UpdateTrustDisplay(CurrencyManager.Instance.CurrentTrust);
            }
        }

        void OnDisable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCreditsChanged -= UpdateCreditsDisplay;
                CurrencyManager.Instance.OnTrustChanged -= UpdateTrustDisplay;
            }
        }

        void OnDestroy()
        {
        }

        private void UpdateCreditsDisplay(int amount)
        {
            if (creditsText != null)
            {
                creditsText.SetText("{0}", amount);
            }
        }

        private void UpdateTrustDisplay(int amount)
        {
            if (trustText != null)
            {
                trustText.SetText("{0}", amount);
            }
        }
    }
}
