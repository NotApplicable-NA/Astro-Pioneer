using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Data
{
    [System.Serializable]
    public struct LocalizationEntry
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class LanguageData
    {
        public string languageCode;
        public List<LocalizationEntry> entries = new List<LocalizationEntry>();
    }

    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "AstroPioneer/Database/LocalizationDatabase")]
    public class LocalizationDatabase : ScriptableObject
    {
        public List<LanguageData> languages = new List<LanguageData>();

        private Dictionary<string, Dictionary<string, string>> lookupCache;

        public void InitializeCache()
        {
            if (lookupCache != null) return;
            lookupCache = new Dictionary<string, Dictionary<string, string>>();

            foreach (var lang in languages)
            {
                var dict = new Dictionary<string, string>();
                foreach (var entry in lang.entries)
                {
                    dict[entry.key] = entry.value;
                }
                lookupCache[lang.languageCode] = dict;
            }
        }

        public string GetText(string langCode, string key, string fallback = null)
        {
            if (lookupCache == null) InitializeCache();

            if (lookupCache.TryGetValue(langCode, out var dict))
            {
                if (dict.TryGetValue(key, out string val))
                {
                    return val;
                }
            }

            return fallback ?? $"[{key}]";
        }
    }
}
