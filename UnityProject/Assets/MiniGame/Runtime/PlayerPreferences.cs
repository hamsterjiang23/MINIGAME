using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Device preferences are separate from campaign progress and authored scene profiles.</summary>
    public static class PlayerPreferences
    {
        public const string BrightnessKey = "NORM.Display.Brightness.v1";
        public const string MasterVolumeKey = "NORM.Audio.MasterVolume.v1";
        private static bool loaded;
        private static float brightness = 1, masterVolume = 1;
        public static event Action Changed;
        public static float Brightness { get { EnsureLoaded(); return brightness; } }
        public static float MasterVolume { get { EnsureLoaded(); return masterVolume; } }

        public static void SetBrightness(float value)
        {
            EnsureLoaded();
            brightness = Clamp(value, .5f, 1.5f);
            PlayerPrefs.SetFloat(BrightnessKey, brightness);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void SetMasterVolume(float value)
        {
            EnsureLoaded();
            masterVolume = Clamp(value, 0, 1);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void Reload()
        {
            brightness = Clamp(PlayerPrefs.GetFloat(BrightnessKey, 1), .5f, 1.5f);
            masterVolume = Clamp(PlayerPrefs.GetFloat(MasterVolumeKey, 1), 0, 1);
            loaded = true;
            AudioListener.volume = masterVolume;
            Changed?.Invoke();
        }

        private static float Clamp(float value, float min, float max)
            => float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Clamp(value, min, max);
        private static void EnsureLoaded() { if (!loaded) Reload(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { loaded = false; Changed = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyAtStartup() => Reload();
    }
}
