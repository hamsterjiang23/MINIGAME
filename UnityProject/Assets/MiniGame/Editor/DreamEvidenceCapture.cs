using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Deterministic, Editor-only evidence capture of production presentation components.</summary>
    public static class DreamEvidenceCapture
    {
        private const int Width = 1280, Height = 720, Fps = 24;
        private const float Delta = 1f / Fps;
        [Serializable]
        private sealed class PhaseEvent
        {
            public int frame;
            public float seconds;
            public string phase, detail;
        }
        [Serializable]
        private sealed class CaptureReport
        {
            public string mode, scene, outputDirectory, status, startedUtc, finishedUtc, error;
            public int day, fps = Fps, width = Width, height = Height, frames, plannedFrames;
            public bool completed, simulatedInput = true, audioCaptured;
            public string captureMethod = "Production camera RenderTexture with existing post effects. Root Overlay canvases temporarily use ScreenSpaceCamera, then restore.";
            public string clock = "Explicit 1/24 second presentation steps per Editor update; not a recording of real keyboard input or real-time frame pacing.";
            public List<PhaseEvent> phaseEvents = new List<PhaseEvent>();
        }
        private sealed class CanvasState
        {
            public Canvas Canvas;
            public RenderMode Mode;
            public Camera Camera;
            public float PlaneDistance;
        }

        private static CaptureReport report;
        private static Camera camera;
        private static RenderTexture target, oldTarget;
        private static Texture2D pixels;
        private static float oldAspect;
        private static DreamSequenceDirector dream;
        private static DreamEndingController ending;
        private static ApartmentPlayer player;
        private static bool oldExternalClock, oldPlayerEnabled, ticking;
        private static bool advanceEndingWalk;
        private static readonly List<CanvasState> canvases = new List<CanvasState>();
        private static string previousPhase;
        public static bool IsRecording { get; private set; }
        public static int FrameCount => report?.frames ?? 0;
        public static string Status => report?.status ?? "idle";

        public static void StartDream(int day, string outputDirectory)
        {
            RequirePlayAndIdle();
            var host = UnityEngine.Object.FindObjectsOfType<MiniGameBootstrap>()
                .SingleOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
            if (host == null || host.Dreams == null || host.Player?.ViewCamera == null)
                throw new InvalidOperationException("Combined 正式梦境宿主尚未就绪。");
            if (!host.Dreams.OwnsCamera || host.Dreams.IsPreview || host.Dreams.ActiveDay != day)
                throw new InvalidOperationException("请先把正式会话恢复到指定日期的梦境演出，再开始记录。");
            if (host.Runtime.GetView().Paused) throw new InvalidOperationException("请先解除暂停再记录梦境。");
            var profile = host.DreamProfile;
            if (profile?.GetDay(day) == null) throw new InvalidOperationException("该日梦境配置缺失。");
            float duration = profile.AlignSeconds + profile.GetDay(day).Duration + profile.ReturnSeconds + 1;
            Start(host.Player.ViewCamera, "dream", day, duration, outputDirectory);
            try
            {
                dream = host.Dreams;
                oldExternalClock = dream.UseExternalClock;
                dream.UseExternalClock = true;
                player = host.Player;
                oldPlayerEnabled = player.enabled;
                player.enabled = false;
                AddPhase(dream.Phase.ToString(), "Formal session; automatic presentation stepping.");
                Activate();
            }
            catch (Exception exception) { Finish("failed", exception.ToString()); throw; }
        }

        public static void StartEnding(string outputDirectory)
        {
            RequirePlayAndIdle();
            var controller = UnityEngine.Object.FindObjectsOfType<DreamEndingController>()
                .SingleOrDefault(item => item.gameObject.scene.name == "DreamEndingBeach");
            if (controller == null || controller.ViewCamera == null || controller.Audio == null)
                throw new InvalidOperationException("请先加载海滩终局并等待控制器初始化。");
            if (controller.Paused) throw new InvalidOperationException("请先解除暂停再记录终局。");
            Start(controller.ViewCamera, "ending", 0, 40, outputDirectory);
            try
            {
                ending = controller;
                advanceEndingWalk = true;
                oldExternalClock = ending.UseExternalClock;
                ending.UseExternalClock = true;
                AddPhase(ending.Phase.ToString(), "Simulate held W until in bed range, then submit one E-equivalent command.");
                Activate();
            }
            catch (Exception exception) { Finish("failed", exception.ToString()); throw; }
        }

        public static void StartSeaPreview(string outputDirectory, float seconds = 12)
        {
            RequirePlayAndIdle();
            var controller = UnityEngine.Object.FindObjectsOfType<DreamEndingController>()
                .SingleOrDefault(item => item.gameObject.scene.name == "DreamEndingBeach");
            if (controller == null || controller.ViewCamera == null || controller.Audio == null || controller.Paused
                || controller.Phase != BeachEndingPhase.Walking)
                throw new InvalidOperationException("请先打开海滩播放模式，解除暂停并保持站立状态。");
            if (float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            Start(controller.ViewCamera, "sea-preview", 0, Mathf.Clamp(seconds, 2, 30), outputDirectory);
            try
            {
                ending = controller;
                advanceEndingWalk = false;
                oldExternalClock = ending.UseExternalClock;
                ending.UseExternalClock = true;
                AddPhase(ending.Phase.ToString(), "Stationary production camera; advance only the presentation clock.");
                Activate();
            }
            catch (Exception exception) { Finish("failed", exception.ToString()); throw; }
        }

        private static void Start(Camera activeCamera, string mode, int day, float seconds, string directory)
        {
            string output = Path.GetFullPath(directory);
            Directory.CreateDirectory(output);
            if (File.Exists(Path.Combine(output, "capture.json")) || Directory.EnumerateFiles(output, "frame_*.png").Any())
                throw new InvalidOperationException("记录目录已有帧或 capture.json，请选择新的输出目录，避免混合旧证据。");
            report = new CaptureReport { mode = mode, day = day, scene = activeCamera.gameObject.scene.name,
                outputDirectory = output, status = "preparing", startedUtc = DateTime.UtcNow.ToString("O"),
                plannedFrames = Mathf.CeilToInt(seconds * Fps) };
            camera = activeCamera;
            previousPhase = null;
            try { PrepareRender(); WriteReport(); }
            catch (Exception exception) { Finish("failed", exception.ToString()); throw; }
        }

        private static void Activate()
        {
            IsRecording = true;
            report.status = "recording";
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            WriteReport();
        }

        private static void Tick()
        {
            if (!IsRecording || ticking) return;
            ticking = true;
            try
            {
                if (!EditorApplication.isPlaying || camera == null)
                { Finish("interrupted", "Play Mode ended or the recording camera was destroyed."); return; }
                if (dream != null)
                {
                    dream.Advance(Delta);
                    ObservePhase(dream.Phase.ToString());
                }
                else if (ending != null)
                {
                    ending.AdvanceForTests(advanceEndingWalk && ending.Phase == BeachEndingPhase.Walking ? 1 : 0, Delta);
                    if (advanceEndingWalk && ending.CanLieDown && ending.TryLieDown())
                        AddPhase(ending.Phase.ToString(), "Accepted one simulated bed interaction.");
                    ObservePhase(ending.Phase.ToString());
                }
                else throw new InvalidOperationException("Presentation component was destroyed during capture.");
                SaveFrame(Path.Combine(report.outputDirectory, "frame_" + report.frames.ToString("D5") + ".png"));
                report.frames++;
                if (report.frames % Fps == 0) WriteReport();
                if (report.frames >= report.plannedFrames) Finish("completed", null);
            }
            catch (Exception exception) { Finish("failed", exception.ToString()); Debug.LogException(exception); }
            finally { ticking = false; }
        }

        private static void ObservePhase(string phase)
        {
            if (phase != previousPhase) AddPhase(phase, null);
        }

        private static void AddPhase(string phase, string detail)
        {
            previousPhase = phase;
            report.phaseEvents.Add(new PhaseEvent { frame = report.frames, seconds = report.frames * Delta, phase = phase, detail = detail });
        }

        private static void PrepareRender()
        {
            oldTarget = camera.targetTexture;
            oldAspect = camera.aspect;
            target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
                { name = "Temporary dream evidence target", hideFlags = HideFlags.HideAndDontSave };
            target.Create();
            pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false)
                { name = "Temporary dream evidence readback", hideFlags = HideFlags.HideAndDontSave };
            camera.targetTexture = target;
            camera.aspect = (float)Width / Height;
            canvases.Clear();
            foreach (var canvas in UnityEngine.Object.FindObjectsOfType<Canvas>(true))
            {
                if (!canvas.isRootCanvas || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                canvases.Add(new CanvasState { Canvas = canvas, Mode = canvas.renderMode,
                    Camera = canvas.worldCamera, PlaneDistance = canvas.planeDistance });
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = camera.nearClipPlane + .01f;
            }
            Canvas.ForceUpdateCanvases();
        }

        private static void SaveFrame(string path)
        {
            Canvas.ForceUpdateCanvases();
            var previous = RenderTexture.active;
            try
            {
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
                pixels.Apply(false, false);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; }
        }

        /// <summary>Captures one production camera frame; the same Canvas conversion is temporary.</summary>
        public static void CaptureSingle(string path)
        {
            RequirePlayAndIdle();
            var beach = UnityEngine.Object.FindObjectOfType<DreamEndingController>();
            var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
            camera = beach != null ? beach.ViewCamera : host != null ? host.Player.ViewCamera : Camera.main;
            if (camera == null) throw new InvalidOperationException("No active production camera.");
            string output = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try { PrepareRender(); SaveFrame(output); }
            finally { RestoreRender(); camera = null; }
        }

        public static void Stop()
        {
            if (IsRecording || target != null) Finish("stopped", null);
        }

        private static void Finish(string status, string error)
        {
            IsRecording = false;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
            try
            {
                if (dream != null) dream.UseExternalClock = oldExternalClock;
                if (ending != null) ending.UseExternalClock = oldExternalClock;
                if (player != null) player.enabled = oldPlayerEnabled;
                RestoreRender();
            }
            finally
            {
                dream = null; ending = null; player = null; camera = null;
                if (report != null)
                {
                    report.status = status; report.error = error;
                    report.completed = status == "completed";
                    report.finishedUtc = DateTime.UtcNow.ToString("O");
                    WriteReport();
                }
            }
        }

        private static void RestoreRender()
        {
            foreach (var state in canvases)
            {
                if (state.Canvas == null) continue;
                state.Canvas.renderMode = state.Mode;
                state.Canvas.worldCamera = state.Camera;
                state.Canvas.planeDistance = state.PlaneDistance;
            }
            canvases.Clear();
            if (camera != null) { camera.targetTexture = oldTarget; camera.aspect = oldAspect; }
            if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
            if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
            target = null; pixels = null; oldTarget = null;
            Canvas.ForceUpdateCanvases();
        }

        private static void WriteReport()
        {
            if (report == null || string.IsNullOrEmpty(report.outputDirectory)) return;
            File.WriteAllText(Path.Combine(report.outputDirectory, "capture.json"), JsonUtility.ToJson(report, true));
        }

        private static void RequirePlayAndIdle()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Evidence capture requires Play Mode.");
            if (IsRecording) throw new InvalidOperationException("Stop the current capture before starting another.");
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                Finish("interrupted", "Play Mode ended.");
        }
    }
}
