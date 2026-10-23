#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in UI-only Player probe; captures terminal GPU output, never the whole window.</summary>
    public sealed class TerminalCommandPlayerProbe : MonoBehaviour
    {
        private const string Flag = "--terminal-v0824-smoke";
        private static bool requested;
        private static string checkpointPath, outputFolder, isolatedSaveFolder, startupError;
        internal static string IsolatedSaveDirectory => isolatedSaveFolder;
        private MiniGameBootstrap host;
        private readonly ProbeReport report = new ProbeReport();
        private ProbeStep step;
        private float began, nextActionAt, quitAt;
        private bool finished;
        private int exitCode, pendingFrame;
        private Button pendingButton, purificationButton;
        private PointerEventData purificationPointer;
        private GameObject purificationHit;
        private string beforePurification;
        public bool Completed => finished;
        public bool Passed => finished && exitCode == 0;

        private enum ProbeStep { Directory, Dialogue, Commit, VerifyCommitted }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            isolatedSaveFolder = null;
            string[] args = Environment.GetCommandLineArgs();
            requested = Array.IndexOf(args, Flag) >= 0;
            if (!requested) return;
            startupError = null;
            outputFolder = Path.Combine(Path.GetTempPath(), "MiniGame-terminal-v0824-" + Guid.NewGuid().ToString("N"));
            try
            {
                outputFolder = Path.GetFullPath(Argument(args, "--terminal-v0824-output"));
                checkpointPath = Argument(args, "--terminal-v0824-checkpoint");
                if (!Path.IsPathRooted(checkpointPath)) throw new ArgumentException("Checkpoint path must be absolute.");
                checkpointPath = Path.GetFullPath(checkpointPath);
            }
            catch (Exception error) { startupError = error.Message; }
            isolatedSaveFolder = Path.Combine(outputFolder, "isolated-saves-" + Guid.NewGuid().ToString("N"));
            MainMenuController.SkipStartup = true;
            MainMenuController.SaveDirectoryOverride = isolatedSaveFolder;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!requested || FindObjectOfType<TerminalCommandPlayerProbe>() != null) return;
            var owner = new GameObject("Terminal v0.8.24 Player probe");
            DontDestroyOnLoad(owner);
            owner.AddComponent<TerminalCommandPlayerProbe>();
        }

#if UNITY_EDITOR
        public static TerminalCommandPlayerProbe StartForEditor(string checkpointPath, string outputPath)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Start this probe in Combined Play Mode.");
            var previous = FindObjectOfType<TerminalCommandPlayerProbe>();
            if (previous != null && !previous.Completed) throw new InvalidOperationException("A terminal probe is already running.");
            if (!Path.IsPathRooted(checkpointPath)) throw new ArgumentException("Checkpoint path must be absolute.");
            TerminalCommandPlayerProbe.checkpointPath = Path.GetFullPath(checkpointPath);
            outputFolder = Path.GetFullPath(outputPath);
            startupError = null;
            isolatedSaveFolder = Path.Combine(outputFolder, "isolated-saves-" + Guid.NewGuid().ToString("N"));
            MainMenuController.SkipStartup = true;
            MainMenuController.SaveDirectoryOverride = isolatedSaveFolder;
            MainMenuController.Current?.TryDismissForEditorDebug();
            if (previous != null) Destroy(previous.gameObject);
            var owner = new GameObject("Terminal v0.8.24 Editor probe");
            DontDestroyOnLoad(owner);
            return owner.AddComponent<TerminalCommandPlayerProbe>();
        }
#endif

        private static string Argument(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            if (index < 0 || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1])
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("Missing required argument: " + name);
            return args[index + 1];
        }

        private void Start()
        {
            began = Time.realtimeSinceStartup;
            try
            {
                Directory.CreateDirectory(outputFolder);
                report.unity = Application.unityVersion;
                report.environment = Application.isEditor ? "Unity Editor Play Mode" : "Standalone Player";
                report.checkpoint = checkpointPath;
                report.output = outputFolder;
                report.isolatedSaveDirectory = isolatedSaveFolder;
                report.captureMode = "TerminalDisplay.Texture native GPU output plus current ScreenMaterial GPU CRT composite; neither image is a whole-window capture.";
                Require(startupError == null, startupError);
                Require(MainMenuController.SaveDirectoryOverride == isolatedSaveFolder, "Save directory isolation was lost before scene startup.");
                Require(File.Exists(checkpointPath), "Checkpoint file does not exist: " + checkpointPath);
                host = FindObjectsOfType<MiniGameBootstrap>().SingleOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
                Require(host != null, "Expected the formal MainRoomCombined bootstrap.");
                host.StartCampaign();
                host.RestoreCheckpoint(File.ReadAllText(checkpointPath));
                Require(Path.GetDirectoryName(Path.GetFullPath(host.SavePath)) == isolatedSaveFolder,
                    "Manual save path must share the isolated probe directory.");
                report.manualSavePath = host.SavePath;
                var view = host.Runtime.GetView();
                Require(view.Game.Day == 1 && view.TerminalPage == TerminalPage.Directory && view.TerminalOpen && !view.Paused,
                    "Checkpoint must be an unpaused Day1 terminal Directory from the formal campaign.");
                report.initialQuota = view.Game.RemainingPurificationQuota;
                report.initialRuntimeHash = Hash(host.Runtime.Save());
                report.result = "Running";
                WriteReport();
                nextActionAt = Time.realtimeSinceStartup + .1f;
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void Update()
        {
            if (finished)
            {
                if (!Application.isEditor && Time.realtimeSinceStartup >= quitAt) Application.Quit(exitCode);
                return;
            }
            try
            {
                Require(Time.realtimeSinceStartup - began < 90, "The 90-second terminal UI probe deadline expired at " + step + ".");
                if (Time.realtimeSinceStartup < nextActionAt || host == null || !Ready()) return;
                nextActionAt = Time.realtimeSinceStartup + .075f;
                Tick();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private bool Ready()
        {
            var view = host.Runtime.GetView();
            return !view.Paused && view.TerminalOpen && host.Screen.TerminalReveal >= .999f
                && host.Screen.PowerPhase == TerminalPowerPhase.Running && host.Screen.TerminalPointerAllowed
                && !host.Screen.TransitionLocked && !host.Screen.WorldInputBlockedThisFrame
                && !MainMenuController.BlocksGameplayInput;
        }

        private void Tick()
        {
            var view = host.Runtime.GetView();
            if (step == ProbeStep.Directory)
            {
                Require(view.TerminalPage == TerminalPage.Directory, "Directory changed before its UI click.");
                if (!report.captures.Any(item => item.label == "directory")) Capture("directory");
                if (Click(FindButton("Roster-0"))) step = ProbeStep.Dialogue;
                return;
            }
            if (step == ProbeStep.Dialogue)
            {
                Require(view.TerminalPage == TerminalPage.Review, "The first roster UI click must open Review.");
                if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    Require(view.Game.AvailableDispositions.Contains(Disposition.Purify), "The formal fixture must permit Purify.");
                    if (FindButton("Disposition-Purify") == null) return;
                    VerifyDispositionInput();
                    return;
                }
                if (!report.lockedCaptured)
                {
                    var commands = CommandButtons();
                    Require(commands.Length == 3 && commands.All(button => !button.IsInteractable()), "All three dispositions must be locked during the interview.");
                    Capture("locked");
                    report.lockedCaptured = true;
                }
                var next = FindButton("Continue");
                if (next != null) { Click(next); return; }
                var reply = host.Screen.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(button => button.name.StartsWith("Reply-", StringComparison.Ordinal) && button.IsActive() && button.IsInteractable());
                if (reply != null && Click(reply)) report.replyClicks++;
                return;
            }
            if (step == ProbeStep.Commit)
            {
                Require(host.Runtime.Save() == beforePurification, "Holding pointer down must not submit a disposition.");
                ExecuteEvents.ExecuteHierarchy(purificationHit, purificationPointer, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.ExecuteHierarchy(purificationHit, purificationPointer, ExecuteEvents.pointerClickHandler);
                report.uiClicks++;
                Require(host.Runtime.GetView().Game.RemainingPurificationQuota == report.initialQuota - 1, "A single Purify click must decrement quota exactly once.");
                Require(host.Runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition, "Purify must leave AwaitingDisposition.");
                VerifyOldButtonClick();
                step = ProbeStep.VerifyCommitted;
                nextActionAt = Time.realtimeSinceStartup + .15f;
                return;
            }
            VerifyOldButtonClick();
            Require(CommandButtons().All(button => !button.IsInteractable()), "Committed dispositions must remain locked.");
            Require(host.Runtime.GetView().Game.RemainingPurificationQuota == report.initialQuota - 1, "Quota changed after the committed response began.");
            Capture("committed");
            Finish(true, "UI raycasts advanced the formal Day1 interview; keyboard disposition submission was blocked, arrows selected, pointer-down rendered without committing, and one Purify click committed once.");
        }

        private void VerifyDispositionInput()
        {
            Require(report.lockedCaptured, "The locked review state was not captured.");
            Require(host.Runtime.GetView().Game.AvailableDispositions.Contains(Disposition.Purify), "The formal fixture must permit Purify.");
            Require(report.initialQuota > 0, "The fixture must have a positive initial quota.");
            string before = host.Runtime.Save();
            foreach (var key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space })
            {
                Require(host.Screen.HandleTerminalKey(key), "Expected the disposition handler to consume " + key + ".");
                Require(host.Runtime.Save() == before, key + " unexpectedly changed the runtime.");
            }
            report.keyboardSubmissionBlocked = true;
            string original = SelectedCommand();
            Require(host.Screen.HandleTerminalKey(KeyCode.DownArrow), "Down must navigate disposition selection.");
            string next = SelectedCommand();
            Require(next != original, "Down did not change the selected artwork.");
            Require(host.Screen.HandleTerminalKey(KeyCode.UpArrow), "Up must navigate disposition selection.");
            Require(SelectedCommand() == original && host.Runtime.Save() == before, "Arrow selection must round-trip without changing runtime state.");
            report.directionSelectionVerified = true;
            Capture("normal-selected");
            purificationButton = FindButton("Disposition-Purify");
            Require(purificationButton != null, "Purify must have an enabled visible button.");
            purificationPointer = PointerFor(purificationButton, out purificationHit);
            ExecuteEvents.ExecuteHierarchy(purificationHit, purificationPointer, ExecuteEvents.pointerEnterHandler);
            beforePurification = host.Runtime.Save();
            ExecuteEvents.ExecuteHierarchy(purificationHit, purificationPointer, ExecuteEvents.pointerDownHandler);
            Require(host.Runtime.Save() == beforePurification, "Pointer down must not execute Purify.");
            Require(CommandTexture(purificationButton)?.name == "Purification_Pressed", "Pointer down must display the supplied Pressed artwork.");
            Capture("pressed-before-submit");
            report.pressedWithoutCommit = true;
            step = ProbeStep.Commit;
        }

        private void VerifyOldButtonClick()
        {
            string before = host.Runtime.Save();
            // Deliver a deliberately stale callback to the old UI object; do not bypass its Button gate.
            ExecuteEvents.Execute(purificationButton.gameObject, purificationPointer, ExecuteEvents.pointerClickHandler);
            Require(host.Runtime.Save() == before, "A duplicate click on the old Purify button submitted again.");
            report.duplicateOldClickBlocked = true;
        }

        private Button[] CommandButtons() => host.Screen.GetComponentsInChildren<Button>(true)
            .Where(button => button.name.StartsWith("Disposition-", StringComparison.Ordinal) && button.gameObject.activeInHierarchy).ToArray();

        private static Texture CommandTexture(Button button) => button.transform.Find("CommandArtwork")?.GetComponent<RawImage>()?.texture;

        private string SelectedCommand()
        {
            var selected = CommandButtons().Where(button => button.IsInteractable()
                && CommandTexture(button)?.name.EndsWith("_Selected", StringComparison.Ordinal) == true).ToArray();
            Require(selected.Length == 1, "Expected exactly one selected disposition artwork, got " + selected.Length + ".");
            return selected[0].name;
        }

        private Button FindButton(string name) => host.Screen.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(button => button.name == name && button.IsActive() && button.IsInteractable());

        private bool Click(Button button)
        {
            if (button == null) return false;
            if (pendingButton != button) { pendingButton = button; pendingFrame = Time.frameCount; return false; }
            if (Time.frameCount <= pendingFrame) return false;
            var pointer = PointerFor(button, out var hit);
            pendingButton = null;
            ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerClickHandler);
            report.uiClicks++;
            return true;
        }

        private PointerEventData PointerFor(Button button, out GameObject hit)
        {
            Require(EventSystem.current != null, "No active EventSystem.");
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left, clickCount = 1, eligibleForClick = true,
                position = host.Screen.TerminalDisplay.ScreenPointFor((RectTransform)button.transform)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
                "Top screen-coordinate raycast must hit " + button.name + " at " + pointer.position
                + "; hits=" + string.Join(";", hits.Select(item => item.gameObject.name)));
            hit = hits[0].gameObject;
            pointer.pointerCurrentRaycast = pointer.pointerPressRaycast = hits[0];
            pointer.pressPosition = pointer.position;
            pointer.pointerPress = button.gameObject;
            pointer.rawPointerPress = hit;
            return pointer;
        }

        private void Capture(string label)
        {
            var display = host.Screen.TerminalDisplay;
            Require(display != null && display.Texture != null && display.ScreenMaterial != null, "Actual terminal GPU resources are missing.");
            int previousFrame = display.RenderedFrameCount;
            display.RenderFrame();
            Require(display.RenderedFrameCount > previousFrame, "The terminal source did not render a fresh frame.");
            Require(display.Texture.width == 800 && display.Texture.height == 600, "Expected the native 800x600 terminal texture.");
            string native = label + "-terminal-native.png", crt = label + "-terminal-crt-gpu.png";
            WriteTexture(display.Texture, Path.Combine(outputFolder, native));
            var target = RenderTexture.GetTemporary(1600, 1200, 0, RenderTextureFormat.ARGB32);
            try
            {
                Graphics.Blit(display.Texture, target, display.ScreenMaterial);
                WriteTexture(target, Path.Combine(outputFolder, crt));
            }
            finally { RenderTexture.ReleaseTemporary(target); }
            var view = host.Runtime.GetView();
            var snapshot = new CaptureState
            {
                label = label, nativeFile = native, crtFile = crt, frame = Time.frameCount,
                sourceFrame = display.RenderedFrameCount, terminalMode = display.Mode,
                shader = display.ScreenMaterial.shader.name, seconds = Time.realtimeSinceStartup - began,
                day = view.Game.Day, phase = view.Game.Phase.ToString(), stage = view.Game.EncounterStage.ToString(),
                page = view.TerminalPage.ToString(), power = host.Screen.PowerPhase.ToString(),
                patient = view.Game.CurrentPatientId, quota = view.Game.RemainingPurificationQuota,
                paused = view.Paused, transitionLocked = host.Screen.TransitionLocked,
                runtimeHash = Hash(host.Runtime.Save()),
                commands = CommandButtons().Select(button => new CommandState
                { name = button.name, interactable = button.IsInteractable(), texture = CommandTexture(button)?.name ?? "missing" }).ToArray()
            };
            report.captures.Add(snapshot);
            File.WriteAllText(Path.Combine(outputFolder, label + "-state.json"), JsonUtility.ToJson(snapshot, true));
            WriteReport();
        }

        private static void WriteTexture(RenderTexture source, string path)
        {
            var previous = RenderTexture.active;
            var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                image.Apply();
                Require(image.GetPixels32().Count(pixel => pixel.r > 80 && pixel.r > pixel.b) > 1000,
                    "Terminal GPU output lacks the expected nonempty amber UI: " + path);
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Destroy(image); }
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }

        private void WriteReport()
        {
            report.seconds = Time.realtimeSinceStartup - began;
            File.WriteAllText(Path.Combine(outputFolder, "report.json"), JsonUtility.ToJson(report, true));
        }

        private void Finish(bool success, string detail)
        {
            if (finished) return;
            report.result = success ? "Passed" : "Failed";
            report.detail = detail;
            if (host != null && host.Runtime != null)
            {
                report.finalQuota = host.Runtime.GetView().Game.RemainingPurificationQuota;
                report.finalRuntimeHash = Hash(host.Runtime.Save());
            }
            try { Directory.CreateDirectory(outputFolder); WriteReport(); }
            catch (Exception error) { Debug.LogError("Terminal probe report could not be written: " + error); }
            Debug.Log("MINIGAME_TERMINAL_V0824_" + report.result.ToUpperInvariant() + " output=" + outputFolder + " " + detail);
            exitCode = success ? 0 : 1;
            finished = true;
            quitAt = Time.realtimeSinceStartup + .25f;
            if (Application.isEditor) enabled = false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string result, detail, unity, environment, checkpoint, output, isolatedSaveDirectory, manualSavePath, captureMode, initialRuntimeHash, finalRuntimeHash;
            public float seconds;
            public int initialQuota, finalQuota, uiClicks, replyClicks;
            public bool lockedCaptured, keyboardSubmissionBlocked, directionSelectionVerified, pressedWithoutCommit, duplicateOldClickBlocked;
            public List<CaptureState> captures = new List<CaptureState>();
        }

        [Serializable]
        private sealed class CaptureState
        {
            public string label, nativeFile, crtFile, shader, phase, stage, page, power, patient, runtimeHash;
            public int frame, sourceFrame, terminalMode, day, quota;
            public float seconds;
            public bool paused, transitionLocked;
            public CommandState[] commands;
        }

        [Serializable]
        private sealed class CommandState
        {
            public string name, texture;
            public bool interactable;
        }
    }
}
#endif
