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
    /// <summary>Opt-in asynchronous recording only; the campaign and its smoke driver own all actions.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class TerminalClosingRecorder : MonoBehaviour
    {
        private const int Width = 1280, Height = 720, BufferCount = 256;
        private const double Interval = 1.0 / 60;
        private readonly RenderTexture[] targets = new RenderTexture[3];
        private readonly bool[] busy = new bool[3];
        private readonly List<CaptureInfo> captures = new List<CaptureInfo>(BufferCount);
        private CaptureBuffer[] buffers;
        private MiniGameBootstrap host;
        private string folder, failure;
        private double began, shutdownAt, nextCapture, firstClosing = -1, firstClosed = -1;
        private bool started, sawOff, sawSafe, sawCollapse, sawHalted, captureFailed, completed, disposed;
        private bool closingLocked = true, controlledRate;
        private float minimumClosingReveal = 1;
        private int used, pending, skipped, busyDropped, capacityDropped, closingUpdates;
        private int originalTargetFrameRate, originalVSyncCount;

        [Serializable] private sealed class CaptureInfo
        {
            public string file, phase, consoleText;
            public int frame, brightConsolePixels;
            public double seconds;
            public float milliseconds, reveal;
            public bool transitionLocked, readbackError;
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
            public string result, detail, unity, graphicsDevice;
            public string captureMode = "Native UI RenderTexture after TerminalDisplay.LateUpdate, actual CRT material, 1280x720 letterbox; asynchronous RGBA32 readback with preallocated buffers; PNG encoding and disk writes after Off";
            public string consolePixelRegion = "Exported image top-left x=[180,600), y=[12,130); amber RGB: red>60, green>35, red>blue*1.4, green>blue*1.15";
            public int width, height, capturedFrames, skippedBy60FpsLimit, droppedBusy, droppedCapacity;
            public int closingUpdates, visibleClosingFrames, closingFrames, minimumClosingPixels, maximumClosingPixels;
            public float averageClosingPixels, minimumClosingReveal;
            public double firstClosingSeconds, firstClosedSeconds, closingTextSeconds, recordedSeconds;
            public bool asyncReadbackSupported, closingInputLocked, safeObserved, collapseObserved, offObserved, haltedObserved;
            public int targetFrameRate, vSyncCount, originalTargetFrameRate, originalVSyncCount;
            public CaptureInfo[] captures;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--terminal-closing-recorder") >= 0)
                new GameObject("Terminal closing recorder").AddComponent<TerminalClosingRecorder>();
        }

        private void Start()
        {
            began = Time.realtimeSinceStartupAsDouble;
            var root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            folder = Path.Combine(string.IsNullOrEmpty(root) ? Application.persistentDataPath : root,
                "Artifacts", "TerminalClosing", "Player");
            originalTargetFrameRate = Application.targetFrameRate;
            originalVSyncCount = QualitySettings.vSyncCount;
            try
            {
                Directory.CreateDirectory(Path.Combine(folder, "frames"));
                host = FindObjectOfType<MiniGameBootstrap>();
                if (host == null) throw new InvalidOperationException("Missing campaign host");
                if (!SystemInfo.supportsAsyncGPUReadback)
                    throw new NotSupportedException("Asynchronous GPU readback is unavailable");
                buffers = new CaptureBuffer[BufferCount];
                for (int i = 0; i < buffers.Length; i++)
                {
                    var buffer = new CaptureBuffer
                    {
                        data = new NativeArray<byte>(Width * Height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory)
                    };
                    buffer.callback = request => ReadbackFinished(buffer, request);
                    buffers[i] = buffer;
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i] = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32)
                        { name = "Closing asynchronous capture " + i, hideFlags = HideFlags.DontSave };
                    if (!targets[i].Create()) throw new InvalidOperationException("Capture render target allocation failed");
                }
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                controlledRate = true;
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
            if (failure != null) { if (pending == 0) Complete(); return; }
            if (sawOff) { if (pending == 0) Complete(); return; }
            if (now - began > 85) throw new TimeoutException("Campaign did not complete terminal shutdown within 85 seconds");
            var screen = host.Screen;
            var phase = screen.PowerPhase;
            if (!started)
            {
                if (phase != TerminalPowerPhase.Shutdown) return;
                started = true; shutdownAt = now; nextCapture = now;
            }
            string text = screen.TerminalConsoleText;
            if (text.Contains("Closing session..."))
            {
                if (firstClosing < 0) firstClosing = now - shutdownAt;
                closingUpdates++;
                closingLocked &= screen.TransitionLocked;
                minimumClosingReveal = Mathf.Min(minimumClosingReveal, screen.TerminalReveal);
            }
            if (text.Contains("Session closed.") && firstClosed < 0) firstClosed = now - shutdownAt;
            sawHalted |= text.Contains("System halted.");
            sawSafe |= phase == TerminalPowerPhase.Safe;
            sawCollapse |= phase == TerminalPowerPhase.Collapse;
            bool endpoint = phase == TerminalPowerPhase.Off;
            if (endpoint || now + .00001 >= nextCapture)
            {
                Capture(now, phase, text);
                nextCapture = shutdownAt + (Math.Floor((now - shutdownAt) / Interval) + 1) * Interval;
            }
            else skipped++;
            if (endpoint) { sawOff = true; if (pending == 0) Complete(); }
        }

        private void Capture(double now, TerminalPowerPhase phase, string text)
        {
            if (used >= buffers.Length) { capacityDropped++; return; }
            int slot = Array.FindIndex(busy, value => !value);
            if (slot < 0) { busyDropped++; return; }
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
            var buffer = buffers[used];
            buffer.info = new CaptureInfo { file = "frames/" + used.ToString("000") + ".png", phase = phase.ToString(),
                consoleText = text, frame = Time.frameCount, seconds = now - shutdownAt,
                milliseconds = Time.unscaledDeltaTime * 1000, reveal = host.Screen.TerminalReveal,
                transitionLocked = host.Screen.TransitionLocked };
            buffer.slot = slot; busy[slot] = true; pending++; captures.Add(buffer.info); used++;
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
            failure = error.ToString(); Debug.LogError("MINIGAME_TERMINAL_CLOSING_FAILED " + failure);
            if (pending == 0) Complete();
        }

        private void Complete()
        {
            if (completed) return;
            completed = true;
            int closingFrames = 0, visibleFrames = 0, minPixels = int.MaxValue, maxPixels = 0;
            long sumPixels = 0;
            try
            {
                if (used > 0)
                {
                    var pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                    try
                    {
                        for (int i = 0; i < used; i++)
                        {
                            var buffer = buffers[i];
                            if (buffer.info.readbackError) continue;
                            buffer.info.brightConsolePixels = CountConsolePixels(buffer.data);
                            if (buffer.info.consoleText.Contains("Closing session..."))
                            {
                                int count = buffer.info.brightConsolePixels;
                                closingFrames++; if (count > 0) visibleFrames++;
                                minPixels = Math.Min(minPixels, count); maxPixels = Math.Max(maxPixels, count); sumPixels += count;
                            }
                            pixels.LoadRawTextureData(buffer.data); pixels.Apply(false, false);
                            File.WriteAllBytes(Path.Combine(folder, buffer.info.file), pixels.EncodeToPNG());
                        }
                    }
                    finally { Destroy(pixels); }
                }
                bool passed = failure == null && !captureFailed && started && sawOff && sawSafe && sawCollapse && sawHalted
                    && firstClosing >= 0 && firstClosed > firstClosing && closingLocked && minimumClosingReveal >= .999f
                    && closingFrames > 0 && visibleFrames == closingFrames && busyDropped == 0 && capacityDropped == 0;
                var report = new Report { result = passed ? "passed" : "failed", detail = failure ?? (passed
                        ? "Actual campaign Closing session text remained visible and input locked through shutdown" : "One or more shutdown visibility, phase, input-lock or capture checks failed"),
                    unity = Application.unityVersion, graphicsDevice = SystemInfo.graphicsDeviceName, width = Width, height = Height,
                    capturedFrames = used, skippedBy60FpsLimit = skipped, droppedBusy = busyDropped, droppedCapacity = capacityDropped,
                    closingUpdates = closingUpdates, visibleClosingFrames = visibleFrames, closingFrames = closingFrames,
                    minimumClosingPixels = closingFrames == 0 ? 0 : minPixels, maximumClosingPixels = maxPixels,
                    averageClosingPixels = closingFrames == 0 ? 0 : (float)sumPixels / closingFrames, minimumClosingReveal = minimumClosingReveal,
                    firstClosingSeconds = firstClosing, firstClosedSeconds = firstClosed,
                    closingTextSeconds = firstClosing >= 0 && firstClosed >= 0 ? firstClosed - firstClosing : 0,
                    recordedSeconds = captures.Count == 0 ? 0 : captures[captures.Count - 1].seconds,
                    asyncReadbackSupported = SystemInfo.supportsAsyncGPUReadback, closingInputLocked = closingLocked,
                    safeObserved = sawSafe, collapseObserved = sawCollapse, offObserved = sawOff, haltedObserved = sawHalted,
                    targetFrameRate = Application.targetFrameRate, vSyncCount = QualitySettings.vSyncCount,
                    originalTargetFrameRate = originalTargetFrameRate, originalVSyncCount = originalVSyncCount, captures = captures.ToArray() };
                File.WriteAllText(Path.Combine(folder, "verification.json"), JsonUtility.ToJson(report, true));
                Debug.Log("MINIGAME_TERMINAL_CLOSING_" + (passed ? "PASSED " : "FAILED ") + report.detail);
            }
            catch (Exception error) { Debug.LogException(error); }
            finally { DisposeBuffers(); }
        }

        private static int CountConsolePixels(NativeArray<byte> data)
        {
            int count = 0;
            for (int top = 12; top < 130; top++)
                for (int x = 180; x < 600; x++)
                {
                    int offset = ((Height - 1 - top) * Width + x) * 4;
                    byte red = data[offset], green = data[offset + 1], blue = data[offset + 2];
                    if (red > 60 && green > 35 && red > blue * 1.4f && green > blue * 1.15f) count++;
                }
            return count;
        }

        private void OnDestroy()
        {
            if (!completed && folder != null)
            {
                WaitForReadbacks();
                failure = failure ?? "Recorder destroyed before Off was captured";
                Complete();
            }
            else DisposeBuffers();
        }

        private void WaitForReadbacks()
        {
            if (buffers == null) return;
            foreach (var buffer in buffers)
                if (buffer != null && buffer.requested && !buffer.request.done) buffer.request.WaitForCompletion();
        }

        private void DisposeBuffers()
        {
            if (disposed) return;
            WaitForReadbacks(); disposed = true;
            if (controlledRate)
            {
                QualitySettings.vSyncCount = originalVSyncCount;
                Application.targetFrameRate = originalTargetFrameRate;
                controlledRate = false;
            }
            if (buffers != null)
                foreach (var buffer in buffers) if (buffer != null && buffer.data.IsCreated) buffer.data.Dispose();
            foreach (var target in targets) if (target != null) { target.Release(); Destroy(target); }
        }
    }
}
#endif
