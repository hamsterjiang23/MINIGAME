#if UNITY_EDITOR
namespace MiniGame.UnityHost
{
    /// <summary>Editor previews route manual saves away from the player's real campaign.</summary>
    public static class EditorCampaignDebugSession
    {
        public const string ActiveStateKey = "MiniGame.CampaignDebug.Active";
        public const string DirectoryStateKey = "MiniGame.CampaignDebug.Directory";
        private static string saveDirectory;
        public static string SaveDirectory
        {
            get => saveDirectory ?? (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode
                && UnityEditor.SessionState.GetBool(ActiveStateKey, false)
                ? UnityEditor.SessionState.GetString(DirectoryStateKey, null) : null);
            set => saveDirectory = value;
        }
    }
}
#endif
