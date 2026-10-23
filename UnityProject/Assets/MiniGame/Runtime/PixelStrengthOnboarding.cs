using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Device setup is independent of campaign progress and ordinary settings changes.</summary>
    public static class PixelStrengthOnboarding
    {
        public const string PreferenceKey = "NORM.Graphics.PixelSetupCompleted.v1";
        public static bool Completed => PlayerPrefs.GetInt(PreferenceKey, 0) == 1;

        public static bool ShouldShow(string saveDirectory) => !Completed
            && !MainMenuContinueStore.HasCompletedInspection(saveDirectory)
            && MainMenuContinueStore.ResolveResumePath(saveDirectory) == null;

        public static void Confirm(PixelDetail detail)
        {
            PlayerGraphicsSettings.SetDetail(detail);
            PlayerPrefs.SetInt(PreferenceKey, 1);
            PlayerPrefs.Save();
        }
    }
}
