using System;
using System.Collections;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public sealed class ReleaseDisplayStartup : MonoBehaviour
    {
        private int requestedWidth, requestedHeight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (!ShouldUseNativeResolution(Environment.GetCommandLineArgs())) return;
            var owner = new GameObject("NORM release display startup");
            DontDestroyOnLoad(owner);
            owner.AddComponent<ReleaseDisplayStartup>().ApplyNativeResolution();
#endif
        }

        internal static bool ShouldUseNativeResolution(string[] arguments)
        {
            foreach (var argument in arguments)
                if (string.Equals(argument, "-nographics", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(argument, "-screen-fullscreen", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(argument, "-screen-width", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(argument, "-screen-height", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(argument, "-window-mode", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private void ApplyNativeResolution()
        {
            // In fullscreen mode currentResolution can describe the cached low-resolution
            // render target. Display.systemWidth/systemHeight describe the actual monitor.
            int width = Display.main.systemWidth, height = Display.main.systemHeight;
            if (width <= 0 || height <= 0 || width == requestedWidth && height == requestedHeight) return;
            requestedWidth = width; requestedHeight = height;
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }

        private IEnumerator Start()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            int stableFrames = 0;
            // SetResolution applies at the end of a frame; the display can also finish
            // initializing after BeforeSceneLoad. Confirm the resulting render dimensions.
            for (int frame = 0; frame < 120; frame++)
            {
                ApplyNativeResolution();
                yield return null;
                bool ready = requestedWidth > 0 && requestedHeight > 0
                    && Screen.width == requestedWidth && Screen.height == requestedHeight
                    && Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
                stableFrames = ready ? stableFrames + 1 : 0;
                if (stableFrames < 3) continue;
                Debug.Log("NORM_DISPLAY_READY render=" + Screen.width + "x" + Screen.height
                    + " monitor=" + requestedWidth + "x" + requestedHeight + " mode=" + Screen.fullScreenMode
                    + " pixel=" + PlayerGraphicsSettings.PixelSize);
                Destroy(gameObject);
                yield break;
            }
            Debug.LogWarning("NORM_DISPLAY_UNSETTLED render=" + Screen.width + "x" + Screen.height
                + " requested=" + requestedWidth + "x" + requestedHeight + " mode=" + Screen.fullScreenMode);
            Destroy(gameObject);
#endif
            yield break;
        }
    }
}
