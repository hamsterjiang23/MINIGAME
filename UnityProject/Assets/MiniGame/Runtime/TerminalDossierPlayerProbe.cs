#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Explicitly requested dossier checks against the formal scene and screen-coordinate raycasts.</summary>
    public sealed class TerminalDossierPlayerProbe : MonoBehaviour
    {
        private static string checkpointDirectory, outputDirectory;
        private static bool requested;
        private static bool nativeInput;
        internal static string IsolatedSaveDirectory { get; private set; }
        private MiniGameBootstrap host;
        private readonly Report report = new Report();
        private string[] checkpoints;
        private int checkpointIndex = -1, rowIndex, step;
        private string beforeBrowsing, captureLabel;
        private float began, nextAction;
        private TerminalDossierSummary summary;
        public bool Completed { get; private set; }
        public bool Passed => Completed && report.result == "Passed";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            IsolatedSaveDirectory = null;
            var args = Environment.GetCommandLineArgs();
            requested = Array.IndexOf(args, "--terminal-v0826-smoke") >= 0;
            nativeInput = Array.IndexOf(args, "--terminal-v0826-native-input") >= 0;
            if (!requested) return;
            checkpointDirectory = Argument(args, "--terminal-v0826-checkpoints");
            outputDirectory = Argument(args, "--terminal-v0826-output");
            MainMenuController.SkipStartup = true;
            IsolatedSaveDirectory = Path.Combine(outputDirectory, "isolated-saves");
            MainMenuController.SaveDirectoryOverride = IsolatedSaveDirectory;
        }

        private static string Argument(string[] args, string key)
        {
            int index = Array.IndexOf(args, key);
            if (index < 0 || index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1]))
                throw new ArgumentException("Expected absolute path after " + key);
            return Path.GetFullPath(args[index + 1]);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (requested) new GameObject("Dossier Player probe").AddComponent<TerminalDossierPlayerProbe>();
        }

#if UNITY_EDITOR
        public static TerminalDossierPlayerProbe StartForEditor(string checkpointFolder, string outputFolder)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play Combined before starting the probe.");
            if (FindObjectsOfType<TerminalDossierPlayerProbe>().Any(item => !item.Completed))
                throw new InvalidOperationException("A dossier probe is already running.");
            checkpointDirectory = Path.GetFullPath(checkpointFolder); outputDirectory = Path.GetFullPath(outputFolder);
            MainMenuController.SkipStartup = true;
            IsolatedSaveDirectory = Path.Combine(outputDirectory, "isolated-saves");
            MainMenuController.SaveDirectoryOverride = IsolatedSaveDirectory;
            MainMenuController.Current?.TryDismissForEditorDebug();
            return new GameObject("Dossier Editor probe").AddComponent<TerminalDossierPlayerProbe>();
        }
#endif

        private void Start()
        {
            began = Time.realtimeSinceStartup;
            try
            {
                Directory.CreateDirectory(outputDirectory);
                checkpoints = Directory.GetFiles(checkpointDirectory, "*.json").OrderBy(path => path, StringComparer.Ordinal).ToArray();
                Require(checkpoints.Length >= 5, "Expected tutorial, three campaign days and self-review checkpoints.");
                host = FindObjectOfType<MiniGameBootstrap>();
                Require(host != null && host.gameObject.scene.name == "MainRoomCombined", "Expected the formal Combined scene.");
                host.StartCampaign();
                report.environment = Application.isEditor ? "Editor" : "Development Player";
                report.width = Screen.width; report.height = Screen.height;
                report.result = "Running";
                WriteReport();
            }
            catch (Exception error) { Finish(error); }
        }

        private void Update()
        {
            if (Completed)
            {
                if (!Application.isEditor && Time.realtimeSinceStartup > nextAction) Application.Quit(Passed ? 0 : 2);
                return;
            }
            try
            {
                Require(Time.realtimeSinceStartup - began < 150, "Dossier probe timed out at step " + step);
                if (step == 6 && Input.mouseScrollDelta.y < 0) report.nativeDownInputFrames++;
                if (step == 7 && Input.mouseScrollDelta.y > 0) report.nativeUpInputFrames++;
                if (Time.realtimeSinceStartup < nextAction) return;
                nextAction = Time.realtimeSinceStartup + .25f;
                Tick();
            }
            catch (Exception error) { Finish(error); }
        }

        private void Tick()
        {
            if (step == 0)
            {
                if (++checkpointIndex == checkpoints.Length)
                {
                    if (!nativeInput) { Finish(null); return; }
                    host.RestoreCheckpoint(File.ReadAllText(Path.Combine(checkpointDirectory, "day2.json")));
                    step = 5; return;
                }
                host.RestoreCheckpoint(File.ReadAllText(checkpoints[checkpointIndex]));
                rowIndex = 0; step = 1; return;
            }
            if (!host.Screen.TerminalPointerAllowed || host.Screen.TerminalReveal < .999f
                || host.Screen.PowerPhase != TerminalPowerPhase.Running || host.Screen.TransitionLocked) return;
            var view = host.Runtime.GetView();
            Require(view.TerminalOpen && view.TerminalPage == TerminalPage.Directory, "Expected an open directory.");
            if (step == 5)
            {
                rowIndex = view.Patients.ToList().FindIndex(patient => patient.Id == "priest");
                Require(rowIndex >= 0, "Native wheel check requires the priest.");
                var pointer = PointerFor((RectTransform)FindButton("Roster-" + rowIndex).transform, out var hit);
                ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerEnterHandler);
                summary = host.Screen.GetComponentInChildren<TerminalDossierSummary>(true);
                beforeBrowsing = host.Runtime.Save();
                report.awaitingNativeInput = true; began = Time.realtimeSinceStartup;
                Capture("native-wheel-before"); step = 6; return;
            }
            if (step == 6)
            {
                if (summary.FirstLine == 0) return;
                Require(Label("PreviewId").text == "对象档案 / " + PortraitCatalog.Registration("priest"), "Native wheel target changed.");
                Require(report.nativeDownInputFrames > 0, "Expected a native downward mouse wheel input frame.");
                Require(summary.FirstLine == 1 && host.Runtime.Save() == beforeBrowsing, "Native down wheel must only scroll the dossier.");
                report.nativeWheelDown = true; Capture("native-wheel-down"); step = 7; return;
            }
            if (step == 7)
            {
                if (summary.FirstLine != 0) return;
                Require(Label("PreviewId").text == "对象档案 / " + PortraitCatalog.Registration("priest"), "Native wheel target changed.");
                Require(report.nativeUpInputFrames > 0, "Expected a native upward mouse wheel input frame.");
                Require(host.Runtime.Save() == beforeBrowsing, "Native up wheel changed the campaign.");
                report.nativeWheelUp = true; report.awaitingNativeInput = false;
                Capture("native-wheel-up"); Finish(null); return;
            }
            if (step == 1) { beforeBrowsing = host.Runtime.Save(); step = 2; }
            if (step == 2)
            {
                if (rowIndex >= view.Patients.Count) { step = 0; return; }
                var row = FindButton("Roster-" + rowIndex);
                var pointer = PointerFor((RectTransform)row.transform, out var hit);
                Require(hit.GetComponentInParent<Button>() == row, "The roster row must be the top hit.");
                ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerEnterHandler);
                var patient = view.Patients[rowIndex];
                var catalog = Resources.Load<TerminalDossierCatalog>("MiniGame/TerminalDossiers");
                var expected = catalog.Find(patient.Id);
                summary = host.Screen.GetComponentInChildren<TerminalDossierSummary>(true);
                Require(summary.FullText == expected.summary && summary.FirstLine == 0, "Summary must start at the selected patient's first line.");
                Require(Label("DossierAge").text == expected.age && Label("DossierOccupation").text == expected.occupation,
                    "Age/occupation differ from the authored dossier.");
                var portrait = host.Screen.GetComponentsInChildren<Image>(true).Single(item => item.name == "DirectoryPortrait");
                Require(portrait.sprite != null && portrait.color == Color.white, "Portrait must be present with white tint.");
                if (patient.Id != "tutorial") Require(portrait.sprite == expected.portrait, "Dossier portrait mapping differs.");
                Require(Label("PurificationQuota").text == view.Game.RemainingPurificationQuota.ToString("00"), "Quota differs from runtime.");
                captureLabel = Path.GetFileNameWithoutExtension(checkpoints[checkpointIndex]) + "-" + patient.Id;
                Capture(captureLabel + "-top");
                step = 3; return;
            }
            if (step == 3)
            {
                if (summary.FirstLine < summary.MaximumFirstLine)
                {
                    int previous = summary.FirstLine;
                    var pointer = PointerFor((RectTransform)summary.transform, out var hit);
                    Require(hit.GetComponentInParent<TerminalDossierSummary>() == summary, "Summary must receive its own wheel hit.");
                    pointer.scrollDelta = Vector2.down;
                    ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.scrollHandler);
                    Require(summary.FirstLine == previous + 1, "One wheel event must move exactly one line.");
                    report.wheelEvents++;
                    if (summary.FirstLine == summary.MaximumFirstLine) Capture(captureLabel + "-bottom");
                    return;
                }
                step = 4;
            }
            if (step == 4)
            {
                if (summary.FirstLine > 0)
                {
                    var up = FindButton("SummaryScrollUp");
                    int previous = summary.FirstLine;
                    var pointer = PointerFor((RectTransform)up.transform, out var hit);
                    ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerClickHandler);
                    Require(summary.FirstLine == previous - 1, "Clicking the up arrow must move one line.");
                    report.arrowClicks++; return;
                }
                Require(host.Runtime.Save() == beforeBrowsing, "Dossier browsing changed the authoritative checkpoint.");
                report.readonlyChecks++; rowIndex++; step = 2;
            }
        }

        private PointerEventData PointerFor(RectTransform target, out GameObject hit)
        {
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current) { position = host.Screen.TerminalDisplay.ScreenPointFor(target),
                button = PointerEventData.InputButton.Left, clickCount = 1, eligibleForClick = true };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Require(hits.Count > 0, "No screen-coordinate UI raycast for " + target.name);
            hit = hits[0].gameObject; pointer.pointerCurrentRaycast = pointer.pointerPressRaycast = hits[0];
            pointer.pressPosition = pointer.position; pointer.rawPointerPress = hit;
            return pointer;
        }

        private Button FindButton(string name) => host.Screen.GetComponentsInChildren<Button>(true).Single(item => item.name == name);
        private Text Label(string name) => host.Screen.GetComponentsInChildren<Text>(true).Single(item => item.name == name);

        private void Capture(string name)
        {
            var display = host.Screen.TerminalDisplay; display.RenderFrame();
            var previous = RenderTexture.active;
            var image = new Texture2D(800, 600, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = display.Texture;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(outputDirectory, name + "-native.png"), image.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Destroy(image); }
            ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, name + "-screen.png"));
            report.captures.Add(new CaptureRecord { name = name, patient = host.Runtime.GetView().Patients[rowIndex].Id,
                age = Label("DossierAge").text, occupation = Label("DossierOccupation").text, fullSummary = summary.FullText,
                visibleSummary = Label("PreviewIntroduction").text, firstLine = summary.FirstLine,
                totalLines = summary.LineCount, width = Screen.width, height = Screen.height });
            WriteReport();
        }

        private void Finish(Exception error)
        {
            Completed = true; report.result = error == null ? "Passed" : "Failed"; report.error = error?.ToString();
            nextAction = Time.realtimeSinceStartup + 2; WriteReport();
            if (error != null) Debug.LogError(error); else Debug.Log("DOSSIER_V0826_PROBE_PASSED " + outputDirectory);
        }

        private void WriteReport() => File.WriteAllText(Path.Combine(outputDirectory, "report.json"), JsonUtility.ToJson(report, true));
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        [Serializable] private sealed class Report
        {
            public string environment, result, error;
            public int width, height, wheelEvents, arrowClicks, readonlyChecks;
            public bool awaitingNativeInput, nativeWheelDown, nativeWheelUp;
            public int nativeDownInputFrames, nativeUpInputFrames;
            public List<CaptureRecord> captures = new List<CaptureRecord>();
        }
        [Serializable] private sealed class CaptureRecord
        {
            public string name, patient, age, occupation, fullSummary, visibleSummary;
            public int firstLine, totalLines, width, height;
        }
    }
}
#endif
