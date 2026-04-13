using UnityEngine;

namespace AstroPioneer.Managers
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public float MasterVolume { get; private set; } = 1.0f;
        public float SfxVolume { get; private set; } = 1.0f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        public void SetMasterVolume(float vol)
        {
            MasterVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            // Apply to AudioListener or AudioMixer
            AudioListener.volume = MasterVolume;
        }

        public void SetSfxVolume(float vol)
        {
            SfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
            // Apply logic to SFX source groups
        }

        private void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);
            AudioListener.volume = MasterVolume;
        }

        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }
    }
}
