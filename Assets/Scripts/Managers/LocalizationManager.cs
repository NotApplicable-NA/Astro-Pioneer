using UnityEngine;
using System.Collections.Generic;
using System;
using AstroPioneer.Core;
using AstroPioneer.Data;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// LocalizationManager — Handles loading string tables for multi-language support.
    /// Can load from Resources/Lang/[lang].json or simple TSV.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public event Action OnLanguageChanged;

        [SerializeField] private string defaultLanguage = "en";
        [SerializeField] private LocalizationDatabase database;
        
        private string currentLanguage;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            
            LoadLanguage(defaultLanguage);
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; ServiceLocator.Unregister<LocalizationManager>(); }
        }

        public void LoadLanguage(string langCode)
        {
            currentLanguage = langCode;

            if (database != null)
            {
                database.InitializeCache();
                Debug.Log($"[Localization] Language set to '{langCode}'");
            }
            else
            {
                // Database not yet assigned in Inspector — localization falls back to key names.
                // To fix: assign a LocalizationDatabase ScriptableObject to the 'Database' field.
#if UNITY_EDITOR
                Debug.LogWarning("[Localization] Database is missing! Assign it in the Inspector on LocalizationManager. Game will run with key fallbacks.");
#endif
            }

            OnLanguageChanged?.Invoke();
        }

        public string GetText(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            
            if (database != null)
            {
                return database.GetText(currentLanguage, key, fallback);
            }
            
            return fallback ?? $"[{key}]";
        }
    }
}
