#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using AstroPioneer.Data;

namespace AstroPioneer.Editor
{
    public class LocalizationEditor : EditorWindow
    {
        private DefaultAsset tsvFolder;
        private LocalizationDatabase targetDatabase;

        [MenuItem("AstroPioneer/Tools/Localization Baker")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditor>("Localization Baker");
        }

        void OnGUI()
        {
            GUILayout.Label("Bake TSV Files to ScriptableObject", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            targetDatabase = (LocalizationDatabase)EditorGUILayout.ObjectField("Target Database", targetDatabase, typeof(LocalizationDatabase), false);
            tsvFolder = (DefaultAsset)EditorGUILayout.ObjectField("TSV Folder (Lang)", tsvFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Bake Localization"))
            {
                if (targetDatabase == null || tsvFolder == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign both the target Database and TSV Folder.", "OK");
                    return;
                }

                Bake();
            }
        }

        private void Bake()
        {
            string folderPath = AssetDatabase.GetAssetPath(tsvFolder);
            string[] files = Directory.GetFiles(folderPath, "*.tsv");

            targetDatabase.languages.Clear();

            foreach (string file in files)
            {
                string langCode = Path.GetFileNameWithoutExtension(file);
                var langData = new LanguageData { languageCode = langCode };

                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                    string[] split = line.Split('\t');
                    if (split.Length >= 2)
                    {
                        var entry = new LocalizationEntry
                        {
                            key = split[0].Trim(),
                            value = split[1].Trim().Replace("\\n", "\n")
                        };
                        langData.entries.Add(entry);
                    }
                }

                targetDatabase.languages.Add(langData);
                Debug.Log($"[Localization Baker] Baked {langData.entries.Count} entries for '{langCode}'");
            }

            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Baked {files.Length} language files into database.", "OK");
        }
    }
}
#endif
