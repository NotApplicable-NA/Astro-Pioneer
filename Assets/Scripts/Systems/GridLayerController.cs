using UnityEngine;
using System;

namespace AstroPioneer.Systems
{
    public enum GridLayerMode { Macro, Micro }

    /// <summary>
    /// GridLayerController toggles the interaction and visual state 
    /// between the main farm grid (Macro) and the utility wire grid (Micro).
    /// </summary>
    public class GridLayerController : MonoBehaviour
    {
        public static GridLayerController Instance { get; private set; }

        public GridLayerMode CurrentMode { get; private set; } = GridLayerMode.Macro;

        public event Action<GridLayerMode> OnLayerModeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleLayerMode();
            }
        }

        public void ToggleLayerMode()
        {
            CurrentMode = CurrentMode == GridLayerMode.Macro ? GridLayerMode.Micro : GridLayerMode.Macro;
            OnLayerModeChanged?.Invoke(CurrentMode);
            
            // Note: Visual fading logic would hook into OnLayerModeChanged from the visual controllers.
        }
    }
}
