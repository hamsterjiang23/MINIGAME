#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in graphical Player regression; the capture path is skipped in batch mode.</summary>
    public sealed class PauseBackdropPlayerProbe : MonoBehaviour
    {
        [Serializable] private sealed class Sample
        {
            public string name, top, bottom;
            public int activeTargetWidth;
            public int width, height;
            public bool passed;
        }
        private readonly List<Sample> samples = new List<Sample>();
        private string folder;
        private float began;
        private bool finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var args = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(args, "--pause-backdrop-probe");
            if (flag < 0 || flag + 1 >= args.Length) return;
            var probe = new GameObject("Pause backdrop Player regression").AddComponent<PauseBackdropPlayerProbe>();
            probe.folder = args[flag + 1];
        }

        private IEnumerator Start()
        {
            began = Time.realtimeSinceStartup;
            Directory.CreateDirectory(folder);
            yield return Resize(1280, 720);
            var host = FindObjectOfType<MiniGameBootstrap>();
            if (host == null || host.Runtime == null) { Finish(false, "Missing campaign host"); yield break; }
            host.Runtime.SetPaused(false);
            var markers = new GameObject("Orientation reference", typeof(RectTransform));
            markers.transform.SetParent(host.Screen.Canvas.transform, false);
            var markerRect = (RectTransform)markers.transform;
            markerRect.anchorMin = Vector2.zero; markerRect.anchorMax = Vector2.one;
            markerRect.offsetMin = markerRect.offsetMax = Vector2.zero;
            markers.transform.SetSiblingIndex(host.Screen.GetComponentInChildren<PauseMenuView>(true).transform.GetSiblingIndex());
            Mark(markers.transform, "Screen top red", new Vector2(.88f, .74f), new Vector2(.995f, .96f), Color.red);
            Mark(markers.transform, "Screen bottom blue", new Vector2(.88f, .04f), new Vector2(.995f, .26f), Color.blue);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            Capture("before-pause");
            host.Runtime.SetPaused(true);
            yield return WaitForPause(host);
            yield return new WaitForEndOfFrame();
            Capture("paused");
            host.Runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.3f);
            yield return new WaitForEndOfFrame();
            Capture("resumed");
            host.Runtime.SetPaused(true);
            yield return WaitForPause(host);
            yield return new WaitForEndOfFrame();
            Capture("paused-again");
            yield return Resize(1024, 768);
            yield return WaitForPause(host);
            yield return new WaitForEndOfFrame();
            Capture("paused-resized");
            host.Runtime.SetPaused(false);
            Destroy(markers);
            bool resized = samples[0].width == 1280 && samples[0].height == 720
                && samples[samples.Count - 1].width == 1024 && samples[samples.Count - 1].height == 768;
            Finish(resized && samples.TrueForAll(sample => sample.passed), "Top red and bottom blue must retain their positions at 1280x720 and 1024x768");
        }

        private static IEnumerator Resize(int width, int height)
        {
            Screen.SetResolution(width, height, false);
            float end = Time.realtimeSinceStartup + 3;
            do { yield return null; }
            while ((Screen.width != width || Screen.height != height) && Time.realtimeSinceStartup < end);
            // The native swap chain can settle later than Screen.width/height.
            yield return new WaitForSecondsRealtime(.4f);
        }

        private static void Mark(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var marker = new GameObject(name, typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(parent, false);
            var rect = (RectTransform)marker.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = marker.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
        }

        private static IEnumerator WaitForPause(MiniGameBootstrap host)
        {
            yield return null;
            var backdrop = host.Screen.GetComponentInChildren<PauseMenuBackdrop>(true);
            float end = Time.realtimeSinceStartup + 3;
            while (Time.realtimeSinceStartup < end && (backdrop.Capturing
                || backdrop.GetComponent<CanvasGroup>().alpha < .999f)) yield return null;
        }

        private void Capture(string name)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = null;
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                Color top = texture.GetPixel(Mathf.RoundToInt(texture.width * .94f), Mathf.RoundToInt(texture.height * .84f));
                Color bottom = texture.GetPixel(Mathf.RoundToInt(texture.width * .94f), Mathf.RoundToInt(texture.height * .16f));
                samples.Add(new Sample { name = name, top = top.ToString(), bottom = bottom.ToString(),
                    width = texture.width, height = texture.height,
                    activeTargetWidth = previous != null ? previous.width : 0,
                    passed = top.r > top.b * 1.8f && bottom.b > bottom.r * 1.8f });
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Destroy(texture); }
        }

        private void Update()
        {
            if (!finished && began > 0 && Time.realtimeSinceStartup - began > 35) Finish(false, "Capture timed out");
        }

        private void Finish(bool passed, string detail)
        {
            finished = true;
            var report = new { passed, detail, graphics = SystemInfo.graphicsDeviceType.ToString(),
                graphicsUVStartsAtTop = SystemInfo.graphicsUVStartsAtTop, samples = samples.ToArray() };
            File.WriteAllText(Path.Combine(folder, "verification.json"),
                Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
            Debug.Log("MINIGAME_PAUSE_ORIENTATION_" + (passed ? "PASSED" : "FAILED"));
            Application.Quit(passed ? 0 : 1);
        }
    }
}
#endif
