#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using MiniGame.Presentation;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in timing and asynchronous GPU recording of the actual fresh campaign opening.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class OpeningMotionProbe : MonoBehaviour
    {
        private const int Width = 1280, Height = 720, BufferCount = 180;
        private const double CaptureInterval = 1.0 / 60;
        private readonly List<FrameTiming> timings = new List<FrameTiming>(512);
        private readonly List<CaptureInfo> captures = new List<CaptureInfo>(BufferCount);
        private readonly RenderTexture[] targets = new RenderTexture[3];
        private readonly bool[] busy = new bool[3];
        private CaptureBuffer[] buffers;
        private MiniGameBootstrap host;
        private string folder;
        private double began, readyAt = -1, clickedAt, settledAt, nextCapture;
        private bool recording, clicked, observedMotion, settled, completed, captureFailed, disposed;
        private int bufferIndex, pending, rateSkipped, busyDropped, capacityDropped;
        private int originalTargetFrameRate, originalVSyncCount;
        private bool controlledCaptureRate;
        private string failure;

        [Serializable] private struct FrameTiming
        {
            public int frame;
            public double seconds;
            public float milliseconds, progress;
        }

        [Serializable] private sealed class CaptureInfo
        {
            public string file, stage;
            public int frame;
            public double seconds;
            public float progress;
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
            public float medianMs, p95Ms, maximumMs;
            public double wallSeconds;
            public bool captureRequested, asyncReadbackSupported, tutorialUnlocked;
            public int capturedFrames, skippedBy60FpsLimit, droppedBusy, droppedCapacity;
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
                if (!host.Screen.DayZeroOpeningWaiting) return;
                if (readyAt < 0) readyAt = now;
                if (now - readyAt < 1) return;
                if (recording) Capture("start", now, 0, true);
                if (!host.Screen.RequestDayZeroOpening()) throw new InvalidOperationException("Opening click was rejected");
                clicked = true; clickedAt = now; nextCapture = now + CaptureInterval;
                return;
            }
            if (host.Effects.EntryPhase == TerminalEntryPhase.OpeningPullback)
            {
                observedMotion = true;
                float progress = host.Screen.TerminalDisplay.OpeningProgress;
                timings.Add(new FrameTiming { frame = Time.frameCount, seconds = now - clickedAt,
                    milliseconds = Time.unscaledDeltaTime * 1000, progress = progress });
                if (recording)
                {
                    if (now + .00001 >= nextCapture)
                    {
                        Capture("pullback", now, progress, false);
                        nextCapture = clickedAt + (Math.Floor((now - clickedAt) / CaptureInterval) + 1) * CaptureInterval;
                    }
                    else rateSkipped++;
                }
                return;
            }
            if (!observedMotion) return;
            if (!settled)
            {
                var view = host.Runtime.GetView();
                if (host.Screen.TransitionLocked || view.Cue?.Kind != PresentationKind.Introduction) return;
                if (settledAt <= 0) settledAt = now;
                if (recording && !Capture("end", now, 1, true)) return;
                settled = true;
            }
            if (pending == 0) Complete(!captureFailed);
        }

        private bool Capture(string stage, double now, float progress, bool endpoint)
        {
            int capacity = endpoint ? BufferCount : BufferCount - 1;
            if (bufferIndex >= capacity) { capacityDropped++; return false; }
            int slot = Array.FindIndex(busy, value => !value);
            if (slot < 0) { busyDropped++; return false; }
            var display = host.Screen.TerminalDisplay;
            var target = targets[slot];
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target; GL.Clear(true, true, Color.black);
                float aspect = display.OutputRect.rect.width / display.OutputRect.rect.height;
                float drawWidth = Mathf.Min(Width, Height * aspect), drawHeight = drawWidth / aspect;
                GL.PushMatrix();
                try
                {
                    GL.LoadPixelMatrix(0, Width, Height, 0);
                    Graphics.DrawTexture(new Rect((Width - drawWidth) / 2, (Height - drawHeight) / 2, drawWidth, drawHeight),
                        display.Texture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.white, display.ScreenMaterial);
                }
                finally { GL.PopMatrix(); }
            }
            finally { RenderTexture.active = previous; }
            var buffer = buffers[bufferIndex];
            buffer.info = new CaptureInfo { file = "frames/" + bufferIndex.ToString("000") + ".png", stage = stage,
                frame = Time.frameCount, seconds = clicked ? now - clickedAt : 0, progress = progress };
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
                if (recording && buffers != null)
                {
                    var pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                    try
                    {
                        for (int i = 0; i < bufferIndex; i++)
                        {
                            var buffer = buffers[i];
                            if (buffer.info.readbackError) continue;
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
                    && host.Runtime.GetView().Cue?.Kind == PresentationKind.Introduction;
                passed &= unlocked && timings.Count > 0;
                var report = new Report { result = passed ? "passed" : "failed", detail = failure ?? (captureFailed ? "GPU readback failed" : "Fresh campaign pullback settled into tutorial"),
                    unity = Application.unityVersion, graphicsDevice = SystemInfo.graphicsDeviceName,
                    captureMode = recording ? "Capture-only 60fps pacing; native UI RT after TerminalDisplay.LateUpdate, actual CRT material, 1280x720 letterboxed, asynchronous RGBA32 readback; PNG encoding and disk writes after motion" : "Timing only; no frame capture or pacing override",
                    count = timings.Count, medianMs = Percentile(sorted, .5f), p95Ms = Percentile(sorted, .95f), maximumMs = Percentile(sorted, 1),
                    over33ms = over33, over50ms = over50, wallSeconds = settledAt > 0 ? settledAt - clickedAt : 0,
                    targetFrameRate = Application.targetFrameRate, vSyncCount = QualitySettings.vSyncCount,
                    originalTargetFrameRate = originalTargetFrameRate, originalVSyncCount = originalVSyncCount,
                    screenWidth = Screen.width, screenHeight = Screen.height, captureRequested = recording,
                    asyncReadbackSupported = SystemInfo.supportsAsyncGPUReadback, tutorialUnlocked = unlocked,
                    capturedFrames = captures.Count, skippedBy60FpsLimit = rateSkipped, droppedBusy = busyDropped, droppedCapacity = capacityDropped,
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
        }
    }
}
#endif
