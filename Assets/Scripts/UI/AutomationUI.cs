using UnityEngine;

namespace AstroPioneer.UI
{
    /// <summary>
    /// Centralized control panel showing machine states and power grids.
    /// </summary>
    public class AutomationUI : MonoBehaviour
    {
        [SerializeField] private GameObject automationPanel;

        void Start()
        {
            if (automationPanel != null) automationPanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePanel();
            }
        }

        public void TogglePanel()
        {
            if (automationPanel == null) return;
            
            bool isActive = !automationPanel.activeSelf;
            automationPanel.SetActive(isActive);
            
            if (isActive)
            {
                RafreshData();
            }
        }

        private void RafreshData()
        {
            // Placeholder for data visualization binding
        }
    }
}
