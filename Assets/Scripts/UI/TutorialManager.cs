using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// Context-sensitive tutorial tooltip system.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private HashSet<string> completedTutorials = new HashSet<string>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowTutorial(string tutorialId, string message)
        {
            if (completedTutorials.Contains(tutorialId)) return;

            // Log or trigger UI tooltip
            completedTutorials.Add(tutorialId);
            
            // Note: Hook this up to a non-intrusive UI toast notification
        }
    }
}
