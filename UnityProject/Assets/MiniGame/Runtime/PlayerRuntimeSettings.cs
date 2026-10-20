using UnityEngine;

namespace MiniGame.UnityHost
{
    public static class PlayerRuntimeSettings
    {
        public const int FrameLimit = 30;
        public const float DefaultMouseSensitivity = .7f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FrameLimit;
        }
    }
}
