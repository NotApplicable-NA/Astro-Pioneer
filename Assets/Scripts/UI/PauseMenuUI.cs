using UnityEngine;

namespace AstroPioneer.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        
        private bool isPaused = false;

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (pausePanel != null) pausePanel.SetActive(isPaused);

            // Pause game mechanics
            Time.timeScale = isPaused ? 0f : 1f;

            if (isPaused)
            {
            }
            else
            {
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
