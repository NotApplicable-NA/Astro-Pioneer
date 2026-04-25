using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AstroPioneer.UI
{
    /// <summary>
    /// LocalizedText — Attaches to a UI Text or TextMeshPro component
    /// to automatically fetch translated strings from LocalizationManager.
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;
        [Tooltip("Optional formatting arguments (e.g. {0} for variables).")]
        [SerializeField] private string[] formatArgs;

        private Text legacyText;
        private TMP_Text tmproText;

        void Awake()
        {
            legacyText = GetComponent<Text>();
            tmproText = GetComponent<TMP_Text>();
        }

        void Start()
        {
            UpdateText();
        }

        void OnEnable()
        {
            if (Managers.LocalizationManager.Instance != null)
            {
                Managers.LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            }
            // Fallback in case language changed while disabled
            UpdateText();
        }

        void OnDisable()
        {
            if (Managers.LocalizationManager.Instance != null)
            {
                Managers.LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            }
        }

        public void SetKey(string newKey, params string[] newFormatArgs)
        {
            localizationKey = newKey;
            
            if (newFormatArgs != null && newFormatArgs.Length > 0)
            {
                formatArgs = newFormatArgs;
            }
            
            UpdateText();
        }

        private void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey) || Managers.LocalizationManager.Instance == null) return;

            string translated = Managers.LocalizationManager.Instance.GetText(localizationKey);

            if (formatArgs != null && formatArgs.Length > 0)
            {
                try
                {
                    translated = string.Format(translated, formatArgs);
                }
                catch
                {
                    Debug.LogWarning($"[LocalizedText] Formatting error for key: {localizationKey}");
                }
            }

            if (tmproText != null) tmproText.text = translated;
            else if (legacyText != null) legacyText.text = translated;
        }
    }
}
