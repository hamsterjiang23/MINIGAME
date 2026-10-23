#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniGame.Presentation;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in timing and asynchronous GPU recording of the actual fresh campaign opening.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class OpeningMotionProbe : MonoBehaviour
    {
        private const int Width = 1280, Height = 720, BufferCount = 180;
        private const double CaptureInterval = 1.0 / 60;
        private readonly List<FrameTiming> timings = new List<FrameTiming>(4096);
        private readonly List<CaptureInfo> captures = new List<CaptureInfo>(BufferCount);
        private readonly RenderTexture[] targets = new RenderTexture[3];
        private readonly bool[] busy = new bool[3];
        private CaptureBuffer[] buffers;
        private MiniGameBootstrap host;
        private MainMenuController menu;
        private RectTransform logo;
        private readonly Vector3[] corners = new Vector3[4];
        private RenderTexture screenCapture;
        private string folder;
        private double began, readyAt = -1, clickedAt, settledAt, nextCapture, previousFrameAt;
        private bool recording, clicked, observedMotion, settled, completed, captureFailed, disposed;
        private bool mainMenuEntry, initialCaptured, finalCaptured, captureQueued, queuedEndpoint;
        private string queuedStage, previousSaveDirectory;
        private float queuedProgress, startActionMs;
        private double queuedAt;
        private int bufferIndex, pending, rateSkipped, busyDropped, capacityDropped;
        private int originalTargetFrameRate, originalVSyncCount;
        private bool controlledCaptureRate;
        private string failure;

        [Serializable] private struct FrameTiming
        {
            public int frame;
            public double seconds;
            public float milliseconds, unityDeltaMilliseconds, progress, transitionClock, controlsAlpha, backdropAlpha;
            public Rect logoRect, startButtonRect, continueButtonRect, newPeriodButtonRect, settingsButtonRect;
            public bool menuVisible, startButtonVisible, menuButtonsVisible;
            public string stage;
        }

        [Serializable] private sealed class CaptureInfo
        {
            public string file, stage;
            public int frame;
            public double seconds;
            public float progress;
            public float transitionClock;
            public bool readbackError;
        }

        private sealed class CaptureBuffer
        {
            public NativeArray<byte> data;
            public AsyncGPUReadbackRequest request;
            public Action<AsyncGPUReadbackRequest> callback;
            public CaptureInfo info;
            public int slot;
            public bool requested;
        }

        [Serializable] private sealed class Report
        {
            public string result, detail, unity, graphicsDevice, captureMode;
            public int count, over33ms, over50ms, targetFrameRate, vSyncCount, originalTargetFrameRate, originalVSyncCount, screenWidth, screenHeight;
            public int captureWidth, captureHeight;
            public float medianMs, p95Ms, maximumMs, startActionMilliseconds, expectedTransitionClock, finalTransitionClock;
            public double wallSeconds;
            public bool captureRequested, asyncReadbackSupported, tutorialUnlocked, firstTutorialLinePresented, mainMenuEntry, graphicsUVStartsAtTop;
            public int capturedFrames, skippedBy60FpsLimit, droppedBusy, droppedCapacity, emptyFrames;
            public FrameTiming[] frames;
            public CaptureInfo[] captures;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "--opening-motion-probe") >= 0)
                new GameObject("Opening motion probe").AddComponent<OpeningMotionProbe>();
        }

        private void Start()
        {
            began = Time.realtimeSinceStartupAsDouble;
            var root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            var label = Environment.GetEnvironmentVariable("MINIGAME_OPENING_PROBE_LABEL");
            if (string.IsNullOrWhiteSpace(label)) label = "measured";
            foreach (char invalid in Path.GetInvalidFileNameChars()) label = label.Replace(invalid, '_');
            folder = Path.Combine(string.IsNullOrEmpty(root) ? Application.persistentDataPath : root,
                "Artifacts", "Opening", "Smooth", label);
            Directory.CreateDirectory(folder);
            recording = Array.IndexOf(Environment.GetCommandLineArgs(), "--opening-capture") >= 0;
            originalTargetFrameRate = Application.targetFrameRate;
            originalVSyncCount = QualitySettings.vSyncCount;
            try
            {
                host = FindObjectOfType<MiniGameBootstrap>();
                if (host == null) throw new InvalidOperationException("Missing fresh campaign host");
                menu = MainMenuController.Current;
                mainMenuEntry = menu != null && menu.IsVisible;
                if (mainMenuEntry)
                {
                    // The opt-in probe must never replace the player's normal continuation or completion marker.
                    previousSaveDirectory = MainMenuController.SaveDirectoryOverride;
                    MainMenuController.SaveDirectoryOverride = Path.Combine(folder, "save");
                    Directory.CreateDirectory(MainMenuController.SaveDirectoryOverride);
                    menu.ShowStartup();
                    foreach (var image in menu.View.GetComponentsInChildren<RawImage>(true))
                        if (image.name == "NormLogo") { logo = image.rectTransform; break; }
                }
                if (!recording) return;
                // Pace only this opt-in recorder. Hidden batch players otherwise run their update loop uncapped.
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                controlledCaptureRate = true;
                if (!SystemInfo.supportsAsyncGPUReadback)
                    throw new NotSupportedException("Asynchronous GPU readback is unavailable; recording was not substituted with a synchronous path");
                Directory.CreateDirectory(Path.Combine(folder, "frames"));
                buffers = new CaptureBuffer[BufferCount];
                for (int i = 0; i < buffers.Length; i++)
                {
                    var buffer = new CaptureBuffer { data = new NativeArray<byte>(Width * Height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory) };
                    buffer.callback = request => ReadbackFinished(buffer, request);
                    buffers[i] = buffer;
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i] = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32)
                        { name = "Opening asynchronous capture " + i, hideFlags = HideFlags.DontSave };
                    targets[i].Create();
                }
                screenCapture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32)
                    { name = "Opening final screen capture", hideFlags = HideFlags.DontSave };
                screenCapture.Create();
                StartCoroutine(CaptureAfterScreenRender());
            }
            catch (Exception error) { Fail(error); }
        }

        private void LateUpdate()
        {
            if (completed) return;
            try { Tick(); }
            catch (Exception error) { Fail(error); }
        }

        private void Tick()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (failure != null) { if (pending == 0) Complete(false); return; }
            if (now - began > 35) throw new TimeoutException("Fresh campaign opening did not settle within 35 seconds");
            if (!clicked)
            {
                if (mainMenuEntry ? !menu.IsVisible : !host.Screen.DayZeroOpeningWaiting) return;
                if (readyAt < 0) readyAt = now;
                if (now - readyAt < 1) return;
                if (recording && !initialCaptured)
                {
                    QueueCapture("start", now, 0, true);
                    return;
                }
                clicked = true; clickedAt = previousFrameAt = now; nextCapture = now + CaptureInterval;
                if (mainMenuEntry)
                {
                    if (menu.View.Page == MainMenuPage.FirstStart) menu.View.StartButton.onClick.Invoke();
                    else
                    {
                        menu.RequestNewInspection();
                        menu.View.ConfirmButton.onClick.Invoke();
                    }
                }
                else if (!host.Screen.RequestDayZeroOpening()) throw new InvalidOperationException("Opening click was rejected");
                startActionMs = (float)((Time.realtimeSinceStartupAsDouble - now) * 1000);
                return;
            }
            if (!settled)
            {
                float progress = host.Screen.TerminalDisplay.OpeningProgress;
                observedMotion |= progress > 0 && progress < 1;
                string stage = mainMenuEntry && menu.TransitionElapsed <= MainMenuController.ControlsFadeSeconds ? "controls"
                    : progress < 1 ? "pullback" : "tutorial";
                RecordFrame(now, progress, stage);
                var view = host.Runtime?.GetView();
                if (observedMotion && !MainMenuController.BlocksGameplayInput && !host.Screen.TransitionLocked
                    && !host.Screen.DayZeroOpeningWaiting && view?.Cue?.Kind == PresentationKind.Introduction && HasTutorialLine())
                {
                    settledAt = now;
                    settled = true;
                    if (recording) QueueCapture("end", now, progress, true);
                }
                else if (recording)
                {
                    if (now + .00001 >= nextCapture)
                    {
                        QueueCapture(stage, now, progress, false);
                        nextCapture = clickedAt + (Math.Floor((now - clickedAt) / CaptureInterval) + 1) * CaptureInterval;
                    }
                    else rateSkipped++;
                }
            }
            if (!settled) return;
            if (recording && !finalCaptured)
            {
                if (!captureQueued) QueueCapture("end", now, 1, true);
                return;
            }
            if (pending == 0) Complete(!captureFailed);
        }

        private void RecordFrame(double now, float progress, string stage)
        {
            var view = mainMenuEntry ? menu.View : null;
            timings.Add(new FrameTiming
            {
                frame = Time.frameCount, seconds = now - clickedAt,
                milliseconds = (float)((now - previousFrameAt) * 1000), unityDeltaMilliseconds = Time.unscaledDeltaTime * 1000,
                progress = progress, transitionClock = mainMenuEntry ? menu.TransitionElapsed : host.Effects.TerminalTransitionProgress * 2.2f,
                controlsAlpha = view != null ? view.ControlsAlpha : 0, backdropAlpha = view != null ? view.ReleaseAlpha : 0,
                logoRect = ScreenRect(logo), startButtonRect = ScreenRect(view != null ? view.StartButton.transform as RectTransform : null),
                continueButtonRect = ScreenRect(view != null ? view.ContinueButton.transform as RectTransform : null),
                newPeriodButtonRect = ScreenRect(view != null ? view.NewPeriodButton.transform as RectTransform : null),
                settingsButtonRect = ScreenRect(view != null ? view.SettingsButton.transform as RectTransform : null),
                menuVisible = menu != null && menu.IsVisible, startButtonVisible = view != null && view.StartButton.gameObject.activeInHierarchy,
                menuButtonsVisible = view != null && view.ContinueButton.gameObject.activeInHierarchy, stage = stage
            });
            previousFrameAt = now;
        }

        private Rect ScreenRect(RectTransform source)
        {
            if (source == null) return default;
            source.GetWorldCorners(corners);
            var bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            var topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            return new Rect(bottomLeft, topRight - bottomLeft);
        }

        private bool HasTutorialLine()
        {
            if (host == null || host.Runtime == null) return false;
            foreach (var line in host.Runtime.GetView().Transcript) if (!line.IsPlayer) return true;
            return false;
        }

        private void QueueCapture(string stage, double now, float progress, bool endpoint)
        {
            if (captureQueued) return;
            queuedStage = stage; queuedAt = now; queuedProgress = progress; queuedEndpoint = endpoint;
            captureQueued = true;
        }

        private IEnumerator CaptureAfterScreenRender()
        {
            var endOfFrame = new WaitForEndOfFrame();
            while (!completed)
            {
                yield return endOfFrame;
                if (!captureQueued || failure != null) continue;
                captureQueued = false;
                try
                {
                    if (recording && Capture(queuedStage, queuedAt, queuedProgress, queuedEndpoint))
                    {
                        if (queuedStage == "start") initialCaptured = true;
                        else if (queuedStage == "end") finalCaptured = true;
                    }
                }
                catch (Exception error) { Fail(error); }
            }
        }

        private bool Capture(string stage, double now, float progress, bool endpoint)
        {
            int capacity = endpoint ? BufferCount : BufferCount - 1;
            if (bufferIndex >= capacity) { capacityDropped++; return false; }
            int slot = Array.FindIndex(busy, value => !value);
            if (slot < 0) { busyDropped++; return false; }
            var target = targets[slot];
            if (screenCapture.width != Screen.width || screenCapture.height != Screen.height)
                throw new InvalidOperationException("Screen resolution changed during the opening recording");
            ScreenCapture.CaptureScreenshotIntoRenderTexture(screenCapture);
            Graphics.Blit(screenCapture, target, new Vector2(1, SystemInfo.graphicsUVStartsAtTop ? -1 : 1),
                new Vector2(0, SystemInfo.graphicsUVStartsAtTop ? 1 : 0));
            var buffer = buffers[bufferIndex];
            buffer.info = new CaptureInfo { file = "frames/" + bufferIndex.ToString("000") + ".png", stage = stage,
                frame = Time.frameCount, seconds = clicked ? now - clickedAt : 0, progress = progress,
                transitionClock = mainMenuEntry ? menu.TransitionElapsed : host.Effects.TerminalTransitionProgress * 2.2f };
            buffer.slot = slot; busy[slot] = true; pending++;
            captures.Add(buffer.info); bufferIndex++;
            try
            {
                buffer.request = AsyncGPUReadback.RequestIntoNativeArray(ref buffer.data, target, 0, TextureFormat.RGBA32, buffer.callback);
                buffer.requested = true;
            }
            catch
            {
                buffer.info.readbackError = true; busy[slot] = false; pending--; captureFailed = true;
                throw;
            }
            return true;
        }

        private void ReadbackFinished(CaptureBuffer buffer, AsyncGPUReadbackRequest request)
        {
            if (disposed) return;
            buffer.info.readbackError = request.hasError;
            captureFailed |= request.hasError;
            busy[buffer.slot] = false; pending--;
        }

        private void Fail(Exception error)
        {
            failure = error.ToString();
            Debug.LogError("MINIGAME_OPENING_MOTION_FAILED " + failure);
            if (pending == 0) Complete(false);
        }

        private void Complete(bool passed)
        {
            completed = true;
            try
            {
                string preparation = host != null ? host.OpeningPreparationTimingJson : null;
                if (!string.IsNullOrEmpty(preparation)) File.WriteAllText(Path.Combine(folder, "preparation-timings.json"), preparation);
                int emptyFrames = 0;
                if (recording && buffers != null)
                {
                    var pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                    try
                    {
                        for (int i = 0; i < bufferIndex; i++)
                        {
                            var buffer = buffers[i];
                            if (buffer.info.readbackError) continue;
                            if (!HasVisiblePixels(buffer.data)) emptyFrames++;
                            pixels.LoadRawTextureData(buffer.data); pixels.Apply(false, false);
                            File.WriteAllBytes(Path.Combine(folder, buffer.info.file), pixels.EncodeToPNG());
                        }
                    }
                    finally { Destroy(pixels); }
                }
                var sorted = new List<float>(timings.Count);
                int over33 = 0, over50 = 0;
                foreach (var frame in timings)
                { sorted.Add(frame.milliseconds); if (frame.milliseconds > 33) over33++; if (frame.milliseconds > 50) over50++; }
                sorted.Sort();
                bool unlocked = host != null && !host.Screen.TransitionLocked
                    && host.Runtime != null && !MainMenuController.BlocksGameplayInput
                    && host.Runtime.GetView().Cue?.Kind == PresentationKind.Introduction;
                float expectedClock = mainMenuEntry ? MainMenuController.ControlsFadeSeconds + MainMenuController.MonitorPullbackSeconds : 2.2f;
                float finalClock = mainMenuEntry && menu != null ? menu.TransitionElapsed
                    : host != null ? host.Effects.TerminalTransitionProgress * 2.2f : 0;
                passed &= unlocked && HasTutorialLine() && timings.Count > 0 && emptyFrames == 0 && Mathf.Abs(finalClock - expectedClock) < .01f;
                var report = new Report { result = passed ? "passed" : "failed", detail = failure ?? (emptyFrames > 0 ? "Captured frames contain no visible rendered pixels" : captureFailed ? "GPU readback failed"
                        : "Opening reached its first native tutorial line; timing statistics are measurements, not an automatic smoothness verdict"),
                    unity = Application.unityVersion, graphicsDevice = SystemInfo.graphicsDeviceName,
                    captureMode = recording ? "Capture-only 60fps pacing; final screen after WaitForEndOfFrame including overlay logo/buttons, GPU resampled to 1280x720, asynchronous RGBA32 readback; PNG encoding and disk writes after the first tutorial line" : "Timing only; raw wall-clock frame intervals, no frame capture or pacing override",
                    count = timings.Count, medianMs = Percentile(sorted, .5f), p95Ms = Percentile(sorted, .95f), maximumMs = Percentile(sorted, 1),
                    over33ms = over33, over50ms = over50, wallSeconds = settledAt > 0 ? settledAt - clickedAt : 0,
                    targetFrameRate = Application.targetFrameRate, vSyncCount = QualitySettings.vSyncCount,
                    originalTargetFrameRate = originalTargetFrameRate, originalVSyncCount = originalVSyncCount,
                    screenWidth = Screen.width, screenHeight = Screen.height, captureRequested = recording,
                    captureWidth = recording ? Width : 0, captureHeight = recording ? Height : 0,
                    startActionMilliseconds = startActionMs, expectedTransitionClock = expectedClock, finalTransitionClock = finalClock,
                    mainMenuEntry = mainMenuEntry, graphicsUVStartsAtTop = SystemInfo.graphicsUVStartsAtTop,
                    asyncReadbackSupported = SystemInfo.supportsAsyncGPUReadback, tutorialUnlocked = unlocked, firstTutorialLinePresented = HasTutorialLine(),
                    capturedFrames = captures.Count, skippedBy60FpsLimit = rateSkipped, droppedBusy = busyDropped, droppedCapacity = capacityDropped, emptyFrames = emptyFrames,
                    frames = timings.ToArray(), captures = captures.ToArray() };
                File.WriteAllText(Path.Combine(folder, "verification.json"), JsonUtility.ToJson(report, true));
                Debug.Log("MINIGAME_OPENING_MOTION_" + (passed ? "PASSED " : "FAILED ") + report.detail);
            }
            catch (Exception error) { passed = false; Debug.LogException(error); }
            finally { DisposeBuffers(); Application.Quit(passed ? 0 : 1); }
        }

        private static float Percentile(List<float> sorted, float fraction)
        {
            if (sorted.Count == 0) return 0;
            float index = (sorted.Count - 1) * fraction;
            return Mathf.Lerp(sorted[Mathf.FloorToInt(index)], sorted[Mathf.CeilToInt(index)], index % 1);
        }

        private void OnDestroy() { DisposeBuffers(); }

        private static bool HasVisiblePixels(NativeArray<byte> rgba)
        {
            int visible = 0;
            for (int index = 0; index + 2 < rgba.Length; index += 256)
                if (rgba[index] > 32 || rgba[index + 1] > 32 || rgba[index + 2] > 32)
                    if (++visible >= 16) return true;
            return false;
        }

        private void DisposeBuffers()
        {
            if (disposed) return;
            // Requests own native memory until the GPU has completed its writes, including on early exit.
            if (buffers != null)
                foreach (var buffer in buffers)
                    if (buffer != null && buffer.requested && !buffer.request.done) buffer.request.WaitForCompletion();
            disposed = true;
            if (controlledCaptureRate)
            {
                QualitySettings.vSyncCount = originalVSyncCount;
                Application.targetFrameRate = originalTargetFrameRate;
                controlledCaptureRate = false;
            }
            if (buffers != null)
                foreach (var buffer in buffers) if (buffer != null && buffer.data.IsCreated) buffer.data.Dispose();
            foreach (var target in targets) if (target != null) { target.Release(); Destroy(target); }
            if (screenCapture != null) { screenCapture.Release(); Destroy(screenCapture); }
            if (mainMenuEntry) MainMenuController.SaveDirectoryOverride = previousSaveDirectory;
        }
    }
}
#endif
