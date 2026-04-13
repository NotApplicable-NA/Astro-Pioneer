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

        void Start()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCreditsChanged += UpdateCreditsDisplay;
                CurrencyManager.Instance.OnTrustChanged += UpdateTrustDisplay;
                
                UpdateCreditsDisplay(CurrencyManager.Instance.CurrentCredits);
                UpdateTrustDisplay(CurrencyManager.Instance.CurrentTrust);
            }
        }

        void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnCreditsChanged -= UpdateCreditsDisplay;
                CurrencyManager.Instance.OnTrustChanged -= UpdateTrustDisplay;
            }
        }

        private void UpdateCreditsDisplay(int amount)
        {
            if (creditsText != null)
            {
                creditsText.text = amount.ToString();
            }
        }

        private void UpdateTrustDisplay(int amount)
        {
            if (trustText != null)
            {
                trustText.text = amount.ToString();
            }
        }
    }
}
