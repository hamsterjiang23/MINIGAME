#if UNITY_EDITOR || DEVELOPMENT_BUILD || MINIGAME_DREAM_CUBEMAP_PROBE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    /// <summary>Explicit opt-in Player evidence. Earlier days use bounded legal commands, not visual playback.</summary>
    public sealed class DreamCubemapPlayerProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report
        {
            public string result, detail, unity, graphics, scope;
            public bool nondevelopment, actualEndingRoute, stayedInCombined, sixDirections, orientation24,
                onlyBedVisible, paused, walkingSaveRestore, ordinaryRestore, reentry, lyingRestore, driftingRestore, subtitlesComplete;
            public int sourceFaceSize, sourceCaptures, renderedFrames;
            public float refreshRate, measuredSeconds, averageFramesPerSecond, meanCaptureCpuMs, peakCaptureCpuMs;
        }
        private readonly Report report = new Report();
        private string folder;
        private MiniGameBootstrap host;
        private RoomDreamEnding room;
        private static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--dream-cubemap-smoke"))
                new GameObject("Dream cubemap Player validation").AddComponent<DreamCubemapPlayerProbe>();
        }
        private IEnumerator Start()
        {
            folder = Path.Combine(Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT") ?? Application.persistentDataPath,
                "Artifacts", "DreamCubemap", "Player");
            Directory.CreateDirectory(folder);
            report.unity = Application.unityVersion; report.graphics = SystemInfo.graphicsDeviceType.ToString();
            report.nondevelopment = !Debug.isDebugBuild;
            report.scope = "Legal-command fast-forward of earlier days, actual self-review and hidden-ending host route, native GPU room captures and six-face markers; no claim of visually replaying three earlier days. Capture timings are CPU call durations, not GPU queries.";
            var run = Run();
            while (true)
            {
                object next;
                try { if (!run.MoveNext()) break; next = run.Current; }
                catch (Exception exception) { Finish(false, exception.ToString()); yield break; }
                yield return next;
            }
            Finish(true, "Hidden ending route, frozen doorway panorama, room exploration, six-face orientation, pause and checkpoint lifecycle verified.");
        }
        private IEnumerator Run()
        {
            yield return null;
            host = FindObjectOfType<MiniGameBootstrap>(); Require(host != null, "Combined host missing");
            var settings = Instantiate(Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault"));
            var rebellion = Instantiate(Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
            rebellion.InitialValue = rebellion.Maximum = rebellion.Threshold = 100; rebellion.Choices.Clear();
            host.ConfigureDreams(settings, rebellion); host.StartCampaign();
            var session = MiniGameSessionHost.Current;
            string ordinaryFile = Path.Combine(folder, "ordinary-save.json"); session.SaveTo(ordinaryFile);
            Require(File.Exists(ordinaryFile), session.LastMessage);
            var fixture = new GameRuntime(session.Rules, session.Content, true);
            DriveToSelfReview(fixture);
            host.RestoreCheckpoint(fixture.Save()); session = MiniGameSessionHost.Current;
            Require(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, "Self-review computer entry rejected");
            float deadline = Time.realtimeSinceStartup + 15;
            while (host.Runtime.GetView().Cue != null && Time.realtimeSinceStartup < deadline) yield return null;
            Require(host.Runtime.GetView().Cue == null, "Self-review entry did not finish");
            Require(host.Runtime.ConfirmSelfPurification().Accepted, "Self purification rejected");
            deadline = Time.realtimeSinceStartup + 20;
            while ((room = FindObjectOfType<RoomDreamEnding>()) == null || !room.Ready || session.IsTransitioning)
            { Require(Time.realtimeSinceStartup < deadline, "Room ending entry timeout"); yield return null; }
            Require(!session.IsTransitioning, "Host still transitioning");
            report.actualEndingRoute = session.Runtime.GetView().Game.Ending == EndingType.DreamSea && room.Ending.IsRoomCubemapEnding;
            report.stayedInCombined = SceneManager.GetActiveScene().name == MiniGameSessionHost.CombinedScene;
            Require(report.actualEndingRoute && report.stayedInCombined, "Hidden ending did not remain in Combined");
            report.sourceFaceSize = room.Cubemap.Texture.width; report.refreshRate = 0;
            var originals = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .Where(value => !value.transform.IsChildOf(room.transform)
                    && !value.transform.IsChildOf(room.Cubemap.SourceRoot.transform)).ToArray();
            report.onlyBedVisible = originals.All(value => !value.enabled);
            Require(report.onlyBedVisible, "Original room geometry remains visible outside the bed");
            report.orientation24 = CheckOrientation(settings); Require(report.orientation24, "Native six-face marker orientation failed");
            var camera = room.ViewCamera; var originalRotation = camera.transform.rotation;
            for (int i = 0; i < Directions.Length; i++)
            {
                camera.transform.rotation = FaceRotation(i);
                Capture(camera, "room-direction-" + i);
            }
            camera.transform.rotation = originalRotation; report.sixDirections = true;
            int firstFrame = Time.frameCount, firstCapture = room.Cubemap.CapturedFrames;
            float started = Time.realtimeSinceStartup, sumCpu = room.Cubemap.LastCaptureMilliseconds;
            int countCpu = 1, seenCapture = firstCapture;
            while (Time.realtimeSinceStartup - started < 3)
            {
                yield return null;
                if (seenCapture == room.Cubemap.CapturedFrames) continue;
                seenCapture = room.Cubemap.CapturedFrames; sumCpu += room.Cubemap.LastCaptureMilliseconds; countCpu++;
                report.peakCaptureCpuMs = Mathf.Max(report.peakCaptureCpuMs, room.Cubemap.LastCaptureMilliseconds);
            }
            report.measuredSeconds = Time.realtimeSinceStartup - started;
            report.renderedFrames = Time.frameCount - firstFrame;
            report.averageFramesPerSecond = report.renderedFrames / report.measuredSeconds;
            report.meanCaptureCpuMs = sumCpu / Mathf.Max(1, countCpu);
            Require(firstCapture == 1 && room.Cubemap.CapturedFrames == firstCapture, "Doorway panorama was repainted after its single capture");
            var ending = room.Ending;
            ending.SetPaused(true); int pausedFrames = room.Cubemap.CapturedFrames; float pausedTime = ending.WorldTime;
            yield return new WaitForSecondsRealtime(.3f);
            report.paused = room.Cubemap.CapturedFrames == pausedFrames && ending.WorldTime == pausedTime;
            Require(report.paused, "Pause did not freeze world and capture clocks"); ending.SetPaused(false);
            ending.UseExternalClock = true;
            room.AdvanceExploration(new Vector2(.6f, -.8f), new Vector2(21, 12), .2f);
            string endingFile = Path.Combine(folder, "walking-ending-save.json"); session.SaveTo(endingFile);
            var saved = ending.CaptureCheckpoint(); room.AdvanceExploration(Vector2.one, Vector2.zero, .2f);
            session.LoadFrom(endingFile);
            report.walkingSaveRestore = Vector3.Distance(saved.RoomPosition, room.PlayerPosition) < .01f;
            Require(report.walkingSaveRestore, "Walking position did not restore");
            session.LoadFrom(ordinaryFile); yield return null;
            report.ordinaryRestore = session.Runtime.GetView().Game.Ending != EndingType.DreamSea
                && Resources.FindObjectsOfTypeAll<DreamCubemapSource>().All(value => !value.gameObject.activeInHierarchy);
            Require(report.ordinaryRestore, "Ordinary save left the panorama active");
            session.LoadFrom(endingFile); deadline = Time.realtimeSinceStartup + 20;
            while ((room = FindObjectOfType<RoomDreamEnding>()) == null || !room.Ready || session.IsTransitioning)
            { Require(Time.realtimeSinceStartup < deadline, "Ending reload timeout"); yield return null; }
            ending = room.Ending; ending.UseExternalClock = true;
            report.reentry = Vector3.Distance(saved.RoomPosition, room.PlayerPosition) < .01f;
            Require(report.reentry, "Ending reentry did not restore saved position");
            ending.RestoreCheckpoint(new BeachEndingCheckpoint());
            for (int i = 0; i < 500 && !ending.CanLieDown; i++)
            {
                var direction = ending.Bed.position - room.PlayerPosition;
                var pose = ending.CaptureCheckpoint(); pose.RoomYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                ending.RestoreCheckpoint(pose); room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            }
            Require(ending.TryLieDown(), "Bed cannot be reached"); ending.AdvanceForTests(0, .4f);
            Capture(room.ViewCamera, "lying");
            var lying = ending.CaptureCheckpoint(); var lyingCamera = room.ViewCamera.transform.position;
            ending.AdvanceForTests(0, .2f); ending.RestoreCheckpoint(lying);
            report.lyingRestore = Vector3.Distance(lyingCamera, room.ViewCamera.transform.position) < .01f;
            Require(report.lyingRestore, "Lying pose did not restore");
            ending.AdvanceForTests(0, settings.LieDownSeconds + settings.SubtitleDelay + .2f);
            Require(ending.Phase == BeachEndingPhase.Drifting && !string.IsNullOrEmpty(ending.CurrentSubtitle), "Drifting/subtitles not reached");
            Capture(room.ViewCamera, "drifting");
            string driftFile = Path.Combine(folder, "drifting-ending-save.json"); session.SaveTo(driftFile);
            var drifting = ending.CaptureCheckpoint(); ending.AdvanceForTests(0, 2); session.LoadFrom(driftFile);
            report.driftingRestore = ending.CaptureCheckpoint().DriftElapsed == drifting.DriftElapsed
                && ending.CaptureCheckpoint().SubtitleIndex == drifting.SubtitleIndex;
            Require(report.driftingRestore, "Drifting save did not restore");
            ending.AdvanceForTests(0, 300); report.subtitlesComplete = ending.CaptureCheckpoint().SubtitlesComplete;
            Require(report.subtitlesComplete, "Ending subtitles did not finish");
            report.sourceCaptures = room.Cubemap.CapturedFrames;
            session.enabled = false;
        }

        private static void DriveToSelfReview(GameRuntime runtime)
        {
            for (int step = 0; step < 2000; step++)
            {
                var v = runtime.GetView(); if (v.Game.Phase == GamePhase.SelfReview) return;
                RuntimeResult result;
                if (v.Cue != null) result = runtime.CompleteCue(v.Cue.Token);
                else if (v.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (v.TerminalPage == TerminalPage.Directory) result = runtime.EnterSelectedPatient(v.Patients.First(p => p.CanReceiveToday).Id);
                else if (v.Input == InputContext.Dialogue)
                    result = v.Game.EncounterStage == EncounterStage.AwaitingDisposition
                        ? runtime.SubmitDisposition(v.Game.CurrentEncounterId, v.Game.RemainingPurificationQuota > 0 && v.Game.AvailableDispositions.Contains(Disposition.Purify)
                            ? Disposition.Purify : v.Game.AvailableDispositions.Contains(Disposition.Treat) ? Disposition.Treat : v.Game.AvailableDispositions.First())
                        : runtime.SelectResponse(v.Game.CurrentEncounterId, v.Game.CurrentNodeId, v.Game.AvailableResponseIds.First());
                else if (v.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (v.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else if (v.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (v.Game.Phase == GamePhase.Apartment || v.Game.Phase == GamePhase.Work) result = runtime.TryInteract(InteractionTarget.Computer);
                else throw new InvalidOperationException("No legal fixture step: " + v.Game.Phase);
                Require(result.Accepted, "Fixture command rejected: " + result.Reason);
            }
            throw new InvalidOperationException("Fixture exceeded 2000 commands");
        }

        private bool CheckOrientation(DreamSequenceProfile settings)
        {
            var source = new GameObject("Native six-direction markers"); source.SetActive(false);
            var data = source.AddComponent<DreamCubemapSource>(); data.SkyMaterial = settings.CubemapSourcePrefab.GetComponent<DreamCubemapSource>().SkyMaterial;
            data.StandingPose = new GameObject("Standing").transform; data.StandingPose.SetParent(source.transform, false);
            var pivot = new GameObject("Virtual bed pivot").transform; pivot.SetParent(source.transform, false);
            data.PillowPose = new GameObject("Pillow").transform; data.PillowPose.SetParent(pivot, false);
            var colors = new[] { Color.red, Color.green, Color.blue, Color.yellow }; var materials = new List<Material>();
            var diagnosticProfile = Instantiate(settings); diagnosticProfile.CubemapSourcePrefab = source;
            var viewerObject = new GameObject("Native orientation viewer", typeof(Camera));
            var viewer = viewerObject.GetComponent<Camera>(); viewer.enabled = false; viewer.cullingMask = 0; viewer.aspect = 1; viewer.fieldOfView = 90;
            DreamCubemapRenderer renderer = null;
            try
            {
                for (int face = 0; face < 6; face++) for (int q = 0; q < 4; q++)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.transform.SetParent(source.transform, false);
                    cube.transform.localPosition = FaceRotation(face) * new Vector3(q % 2 == 0 ? -1.35f : 1.35f, q < 2 ? 1.35f : -1.35f, 4);
                    cube.transform.localRotation = FaceRotation(face); cube.transform.localScale = new Vector3(.8f, .8f, .03f);
                    var mat = new Material(Shader.Find("MiniGame/Room Art Standard")); mat.SetColor("_Color", Color.black);
                    mat.SetColor("_EmissionColor", colors[q]); mat.SetFloat("_CrtPower", 1); materials.Add(mat); cube.GetComponent<Renderer>().sharedMaterial = mat;
                }
                renderer = new DreamCubemapRenderer(diagnosticProfile, viewer); renderer.Render(0, BeachEndingPhase.Walking, 0, 0, true);
                for (int face = 0; face < 6; face++)
                {
                    viewer.transform.rotation = FaceRotation(face); var pixels = Capture(viewer, "orientation-" + face, 128, 128);
                    for (int q = 0; q < 4; q++)
                    {
                        var color = pixels.GetPixel(q % 2 == 0 ? 42 : 85, q < 2 ? 85 : 42);
                        Require(Vector3.Distance(new Vector3(color.r,color.g,color.b), new Vector3(colors[q].r,colors[q].g,colors[q].b)) < .25f,
                            "Native orientation face=" + face + " quadrant=" + q + " color=" + color);
                    }
                    Destroy(pixels);
                }
                return true;
            }
            finally
            {
                renderer?.Dispose(); source.SetActive(false); Destroy(source); Destroy(viewerObject); Destroy(diagnosticProfile);
                foreach (var mat in materials) Destroy(mat);
            }
        }

        private Texture2D Capture(Camera camera, string name, int width = 1280, int height = 720)
        {
            var old = camera.targetTexture; var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
                return image;
            }
            finally { camera.targetTexture = old; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); }
        }
        private static Quaternion FaceRotation(int face) => Quaternion.LookRotation(Directions[face], face == 4 ? Vector3.forward : face == 5 ? Vector3.back : Vector3.up);
        private static void Require(bool condition, string reason) { if (!condition) throw new InvalidOperationException(reason); }
        private void Finish(bool passed, string detail)
        {
            report.result = passed ? "Passed" : "Failed"; report.detail = detail;
            File.WriteAllText(Path.Combine(folder, "verification.json"), JsonUtility.ToJson(report, true));
            Debug.Log("DREAM_CUBEMAP_PLAYER_" + report.result.ToUpperInvariant() + " " + detail);
            Application.Quit(passed ? 0 : 1);
        }
    }
}
#endif
