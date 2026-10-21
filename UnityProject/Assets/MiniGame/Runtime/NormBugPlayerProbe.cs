#if UNITY_EDITOR || DEVELOPMENT_BUILD || MINIGAME_FEEDBACK_PROBE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in native Player checks. Add --norm-bugs-campaign-smoke for the fresh Day0 campaign.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class NormBugPlayerProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report
        {
            public bool passed, developmentBuild, supervisorWorldObject, noSupervisorIndoors, supervisorPresentBeforeDoor, supervisorRemainsAfterBriefing;
            public bool headerAmber, worldInstructionsHidden, manualOpenedByPointer, escapeClosesManualOnly, escapeResumes;
            public bool terminalDirectoryReached, campaignStart, dayZeroLogoRequested, dayZeroSlept;
            public string result, detail, unity, graphics;
            public string captureMode = "Actual room Camera + native UI Canvas to 1280x720 RenderTexture; no window backbuffer capture";
            public int targetFrameRate, vSyncCount, scanSounds, morningClicks, briefingClicks, dayZeroPurificationClicks;
            public string directoryKpi;
            public float seconds, supervisorBeyondDoorMetres, cameraInsideDistance, supervisorFeetGapFromStandingPlayer;
            public List<string> captures = new List<string>();
            public List<string> pointerChecks = new List<string>();
        }

        private readonly Report report = new Report();
        private MiniGameBootstrap host;
        private DoorViewCamera doorway;
        private SupervisorDoorwayView supervisor;
        private MiniGameAudioService soundService;
        private string folder;
        private float began, nextAction, quitAt;
        private int stage, pendingFrame;
        private bool finished, bootSkipped;
        private Button pendingButton;
        private Camera captureCamera, previousCanvasCamera;
        private Canvas captureCanvas;
        private RenderMode previousCanvasMode;
        private float previousCanvasDistance;
        private RenderTexture captureTarget, previousCameraTarget;
        private Texture2D capturePixels;
        private int captureReadyFrame, stageAfterCapture;
        private string captureName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--norm-bugs-smoke"))
                new GameObject("NORM native BUG verification").AddComponent<NormBugPlayerProbe>();
        }

        private void Start()
        {
            began = Time.realtimeSinceStartup;
            var root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            folder = Path.Combine(string.IsNullOrEmpty(root) ? Application.persistentDataPath : root,
                "Artifacts", "NormBugs", "Validation", "Player");
            Directory.CreateDirectory(folder);
            try
            {
                host = FindObjectOfType<MiniGameBootstrap>();
                Require(host != null && host.Player != null, "MainRoomCombined host is missing.");
                report.campaignStart = Environment.GetCommandLineArgs().Contains("--norm-bugs-campaign-smoke");
                if (report.campaignStart) host.StartCampaign();
                else host.StartTerminalDemo();
                host.Player.enabled = false;
                host.Screen.DialogueCharactersPerSecond = 0;
                doorway = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
                supervisor = host.GetComponent<SupervisorDoorwayView>();
                Require(doorway != null && doorway.InsidePose != null && supervisor != null, "Doorway bindings are missing.");
                report.supervisorPresentBeforeDoor = supervisor.IsVisible && !host.Runtime.GetView().RoomDoorOpen;
                Require(report.supervisorPresentBeforeDoor, "The supervisor must already stand outside the closed door.");
                soundService = host.Audio.Service;
                soundService.SoundPlayed += ObserveSound;
                if (report.campaignStart) stage = 0;
                else EnterMorningDoorway();
            }
            catch (Exception exception) { Finish(false, exception.ToString()); }
        }

        private void Update()
        {
            if (host != null && host.Player != null) host.Player.enabled = false;
            if (finished)
            {
                if (Time.realtimeSinceStartup >= quitAt) Application.Quit(report.passed ? 0 : 1);
                return;
            }
            try
            {
                Require(Time.realtimeSinceStartup - began < 80, "Native BUG verification timed out at stage " + stage);
                if (captureTarget != null)
                {
                    if (Time.frameCount >= captureReadyFrame) CompleteCapture();
                    return;
                }
                if (Time.realtimeSinceStartup < nextAction) return;
                Tick();
            }
            catch (Exception exception) { Finish(false, exception.ToString()); }
        }

        private void Tick()
        {
            var view = host.Runtime.GetView();
            if (stage == 0)
            {
                AdvanceCampaignOpening(view);
            }
            else if (stage == 1)
            {
                if (view.Cue?.Kind != PresentationKind.MorningInspection || !supervisor.IsVisible
                    || doorway.Phase != DoorCameraPhase.Viewing) return;
                CheckSupervisor(); CheckWorldUi();
                Require(report.scanSounds == 1, "Morning must play exactly one scanner sound.");
                BeginCapture("supervisor-outside", 2);
            }
            else if (stage == 2)
            {
                if (view.Cue?.Kind == PresentationKind.MorningInspection || view.Cue?.Kind == PresentationKind.Briefing)
                {
                    var button = host.Screen.GetComponentsInChildren<Button>(false)
                        .FirstOrDefault(item => item.name == "Continue" && item.IsInteractable());
                    if (button == null || !Click(button)) return;
                    if (view.Cue.Kind == PresentationKind.MorningInspection) report.morningClicks++;
                    else report.briefingClicks++;
                    return;
                }
                if (view.Cue != null || view.Game.Phase != GamePhase.Apartment) return;
                report.supervisorRemainsAfterBriefing = supervisor.IsVisible;
                Require(report.supervisorRemainsAfterBriefing && report.morningClicks > 0 && report.briefingClicks > 0,
                    "Both morning messages must finish while the supervisor remains outside.");
                Require(report.scanSounds == 1, "Briefing replayed the scanner sound.");
                Require(host.Player.InteractFocused(), "The actual door return interaction was unavailable.");
                stage = 3;
            }
            else if (stage == 3)
            {
                if (view.Input != InputContext.World || view.Cue != null) return;
                CheckWorldUi();
                host.HandleEscape();
                Require(host.Runtime.GetView().Paused, "Escape did not open the real pause menu.");
                stage = 4;
            }
            else if (stage == 4)
            {
                var menu = host.Screen.GetComponentInChildren<PauseMenuView>(true);
                var backdrop = menu.GetComponent<PauseMenuBackdrop>();
                if (!menu.gameObject.activeInHierarchy || menu.GetComponent<CanvasGroup>().alpha < .999f || backdrop.Capturing) return;
                BeginCapture("pause", 5);
            }
            else if (stage == 5)
            {
                var label = host.Screen.GetComponentInChildren<PauseMenuView>(true).GetComponentsInChildren<Text>(false)
                    .Single(item => item.text == "操作手册");
                if (!Click(label.GetComponentInParent<Button>())) return;
                report.manualOpenedByPointer = host.Screen.PauseManualVisible;
                Require(report.manualOpenedByPointer && host.Runtime.GetView().Paused, "Pointer did not open the pause operation manual.");
                BeginCapture("manual", 6);
            }
            else if (stage == 6)
            {
                string checkpoint = host.Runtime.Save();
                host.HandleEscape();
                report.escapeClosesManualOnly = !host.Screen.PauseManualVisible && host.Runtime.GetView().Paused
                    && host.Runtime.Save() == checkpoint;
                Require(report.escapeClosesManualOnly, "First Escape must close only the manual.");
                nextAction = Time.realtimeSinceStartup + .1f; stage = 7;
            }
            else if (stage == 7)
            {
                host.HandleEscape();
                report.escapeResumes = !host.Runtime.GetView().Paused;
                Require(report.escapeResumes, "Second Escape must resume gameplay.");
                nextAction = Time.realtimeSinceStartup + .1f; stage = 8;
            }
            else if (stage == 8)
            {
                Require(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, "Computer entry was rejected after morning.");
                stage = 9;
            }
            else if (stage == 9)
            {
                if (!bootSkipped && host.Screen.PowerPhase == TerminalPowerPhase.Boot)
                    bootSkipped = host.Screen.HandleTerminalKey(KeyCode.Return);
                if (view.Input != InputContext.TerminalDirectory || view.Cue != null) return;
                report.terminalDirectoryReached = true;
                report.directoryKpi = host.Screen.GetComponentsInChildren<Text>(true)
                    .Single(item => item.name == "DirectoryPurificationRequirement").text;
                if (report.campaignStart)
                    Require(view.Game.PurificationsToday == 0 && view.Game.PurificationRequirement == 1
                        && report.directoryKpi == "净化要求：0/1", "The formal Day1 campaign must display its actual 0/1 requirement.");
                Require(report.scanSounds == 1, "Opening the terminal replayed the morning scanner sound.");
                BeginCapture("terminal-directory", 10);
            }
            else if (stage == 10)
            {
                Require(Application.targetFrameRate == 30 && QualitySettings.vSyncCount == 0, "Player frame settings are not 30 FPS / vSync 0.");
                Finish(true, "Real doorway morning, outside supervisor, amber UI, pointer manual, two Escape stages and morning-only scanner sound.");
            }
        }

        private void AdvanceCampaignOpening(RuntimeView view)
        {
            if (host.Screen.DayZeroOpeningWaiting)
            {
                Require(view.Game.Day == 0 && !report.dayZeroLogoRequested, "Expected one fresh Day0 opening logo request.");
                var logo = host.Screen.GetComponentsInChildren<RawImage>(true).Single(item => item.name == "Day0SupervisorFeed");
                Require(logo.texture == Resources.Load<Texture2D>("MiniGame/NormLogo"), "The supplied NORM opening logo is missing.");
                report.dayZeroLogoRequested = host.Screen.RequestDayZeroOpening();
                Require(report.dayZeroLogoRequested, "The opening logo request was rejected.");
                return;
            }
            if (view.Game.Day == 0 && view.TerminalOpen && view.TerminalPage == TerminalPage.Review
                && !host.Screen.TransitionLocked && host.Screen.PowerPhase == TerminalPowerPhase.Running)
            {
                var buttons = host.Screen.GetComponentsInChildren<Button>(false).Where(button => button.IsActive()
                    && button.IsInteractable() && button.GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace).ToArray();
                Require(buttons.Length <= 1, "Day0 must expose only its current Continue or Purify command.");
                if (buttons.Length == 0) return;
                var button = buttons[0];
                Require(button.name == "Continue" || button.name == "Disposition-Purify", "Unexpected Day0 command: " + button.name);
                if (!Click(button)) return;
                if (button.name == "Disposition-Purify") report.dayZeroPurificationClicks++;
                return;
            }
            if (view.Game.Day == 0 && !view.TerminalOpen && BedtimeGuidance.IsSleepOnly(view) && !report.dayZeroSlept)
            {
                Require(report.dayZeroPurificationClicks == 1 && view.Transcript.Count(line => line.SourceNodeId == "disposition:Purify") == 1,
                    "Day0 must submit its unique purification once.");
                Require(view.Transcript.Count(line => !line.IsPlayer && line.Completed) == 7,
                    "The five introduction lines and two reactions must finish before sleeping.");
                Require(view.TerminalPower == TerminalPowerState.Off, "Day0 must shut down before its single sleep.");
                Require(view.AllowedInteractions.SequenceEqual(new[] { InteractionTarget.Bed }), "Only the bed may advance Day0.");
                report.dayZeroSlept = host.Runtime.TryInteract(InteractionTarget.Bed).Accepted;
                Require(report.dayZeroSlept && host.Runtime.GetView().Cue?.Kind == PresentationKind.Sleep, "Day0 bed must start one Sleep presentation.");
                return;
            }
            if (report.dayZeroSlept && view.Game.Day == 1 && view.Cue == null)
            {
                Require(view.Game.Phase == GamePhase.MorningInspection && !host.Effects.IsSeated,
                    "One Day0 sleep must reach the standing Day1 morning.");
                EnterMorningDoorway();
            }
        }

        private void EnterMorningDoorway()
        {
            StageAtDoor();
            Require(host.Player.InteractFocused(), "The actual door interaction rejected the staged player.");
            stage = 1;
        }

        private void StageAtDoor()
        {
            var interaction = FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Door);
            var marker = GameObject.Find("Door approach");
            Require(marker != null && interaction.GetComponent<Collider>() != null, "Authored door approach/collider is missing.");
            var controller = host.Player.GetComponent<CharacterController>();
            bool enabled = controller.enabled;
            controller.enabled = false;
            host.Player.transform.position = marker.transform.position;
            controller.enabled = enabled;
            Physics.SyncTransforms();
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Require(host.Player.FocusedInteraction == interaction, "The staged camera ray did not hit the real door collider.");
        }

        private void CheckSupervisor()
        {
            var surface = supervisor.Surface;
            report.supervisorWorldObject = surface != null && surface.transform.parent == null && surface.GetComponentInParent<Canvas>() == null;
            Require(report.supervisorWorldObject, "Supervisor must be a standalone world renderer.");
            var outward = Vector3.ProjectOnPlane(doorway.InsidePose.forward, Vector3.up).normalized;
            Vector3 doorPlane = doorway.InsidePose.position + outward * .85f;
            var bounds = surface.sprite.bounds;
            float minimum = float.PositiveInfinity;
            foreach (float x in new[] { bounds.min.x, bounds.max.x })
                foreach (float y in new[] { bounds.min.y, bounds.max.y })
                    minimum = Mathf.Min(minimum, Vector3.Dot(surface.transform.TransformPoint(new Vector3(x, y, bounds.center.z)) - doorPlane, outward));
            report.supervisorBeyondDoorMetres = minimum;
            report.noSupervisorIndoors = minimum >= .5f && surface.GetComponent<Collider>() == null
                && !host.Screen.GetComponentsInChildren<RawImage>(true).Any(item => item.name == "SupervisorFigure");
            report.cameraInsideDistance = Vector3.Distance(host.Player.ViewCamera.transform.position, doorway.InsidePose.position);
            var controller = host.Player.GetComponent<CharacterController>();
            Require(controller != null && controller.enabled, "The standing player controller must be active for the ground-height check.");
            report.supervisorFeetGapFromStandingPlayer = Mathf.Abs(surface.bounds.min.y - controller.bounds.min.y);
            Require(report.noSupervisorIndoors, "Supervisor portrait is inside the room or less than 0.5m beyond the door plane.");
            Require(report.cameraInsideDistance <= .01f, "The door camera advanced beyond the inside hold pose.");
            Require(Mathf.Abs(report.supervisorFeetGapFromStandingPlayer - .1f) < .02f,
                "The supervisor must be raised 0.1 metres above the standing player's ground height.");
        }

        private void CheckWorldUi()
        {
            var texts = host.Screen.GetComponentsInChildren<Text>(true);
            var headers = texts.Where(item => item.name == "ShiftHeader").ToArray();
            report.headerAmber = headers.Length > 0 && headers.All(item => item.color.r > item.color.g && item.color.g > item.color.b);
            var instructions = texts.Where(item => item.name == "WorldInstructions").ToArray();
            report.worldInstructionsHidden = instructions.Length > 0 && instructions.All(item => !item.gameObject.activeSelf);
            Require(report.headerAmber && report.worldInstructionsHidden, "World header must be amber and the old footer instructions hidden.");
        }

        private bool Click(Button button)
        {
            Require(button != null && button.IsActive() && button.IsInteractable(), "A visible interactive button is required.");
            if (pendingButton != button)
            {
                pendingButton = button; pendingFrame = Time.frameCount;
                return false;
            }
            if (Time.frameCount <= pendingFrame) return false;
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector3 world = rect.TransformPoint(rect.rect.center);
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = canvas.renderMode == RenderMode.WorldSpace ? host.Screen.TerminalDisplay.ScreenPointFor(rect)
                    : RectTransformUtility.WorldToScreenPoint(camera, world),
                button = PointerEventData.InputButton.Left, clickCount = 1
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
                "Top pointer hit missed " + button.name + ": " + string.Join(",", hits.Select(item => item.gameObject.name)));
            report.pointerChecks.Add(button.name + " -> " + hits[0].gameObject.name);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            pendingButton = null;
            nextAction = Time.realtimeSinceStartup + .2f;
            return true;
        }

        private void BeginCapture(string name, int nextStage)
        {
            captureName = name; stageAfterCapture = nextStage;
            captureCamera = host.Player.ViewCamera; captureCanvas = host.Screen.Canvas;
            previousCanvasMode = captureCanvas.renderMode; previousCanvasCamera = captureCanvas.worldCamera;
            previousCanvasDistance = captureCanvas.planeDistance; previousCameraTarget = captureCamera.targetTexture;
            captureTarget = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            capturePixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            captureTarget.Create(); captureCamera.targetTexture = captureTarget;
            captureCanvas.renderMode = RenderMode.ScreenSpaceCamera; captureCanvas.worldCamera = captureCamera; captureCanvas.planeDistance = .1f;
            captureReadyFrame = Time.frameCount + 3;
        }

        private void CompleteCapture()
        {
            var previous = RenderTexture.active;
            try
            {
                Canvas.ForceUpdateCanvases(); host.Screen.ReflowTerminal(); Canvas.ForceUpdateCanvases();
                captureCamera.Render(); RenderTexture.active = captureTarget;
                capturePixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); capturePixels.Apply();
                File.WriteAllBytes(Path.Combine(folder, captureName + ".png"), capturePixels.EncodeToPNG());
                report.captures.Add(captureName + ".png");
                stage = stageAfterCapture;
            }
            finally { RenderTexture.active = previous; RestoreCapture(); }
        }

        private void RestoreCapture()
        {
            if (captureTarget == null) return;
            if (captureCanvas != null)
            {
                captureCanvas.renderMode = previousCanvasMode; captureCanvas.worldCamera = previousCanvasCamera;
                captureCanvas.planeDistance = previousCanvasDistance;
            }
            if (captureCamera != null) captureCamera.targetTexture = previousCameraTarget;
            captureTarget.Release(); Destroy(captureTarget); Destroy(capturePixels);
            captureTarget = null; capturePixels = null;
        }

        private void ObserveSound(MiniGameAudioChannel channel, MiniGameSound sound)
        { if (sound == MiniGameSound.Scan) report.scanSounds++; }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }

        private void Finish(bool passed, string detail)
        {
            if (finished) return;
            RestoreCapture(); finished = true; quitAt = Time.realtimeSinceStartup + .25f;
            report.passed = passed; report.result = passed ? "PASSED" : "FAILED"; report.detail = detail;
            report.seconds = Time.realtimeSinceStartup - began; report.unity = Application.unityVersion;
            report.graphics = SystemInfo.graphicsDeviceType.ToString(); report.developmentBuild = Debug.isDebugBuild;
            report.targetFrameRate = Application.targetFrameRate; report.vSyncCount = QualitySettings.vSyncCount;
            File.WriteAllText(Path.Combine(folder, "verification.json"), Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));
            Debug.Log("MINIGAME_NORM_BUGS_" + report.result + " " + detail);
        }

        private void OnDestroy() { if (soundService != null) soundService.SoundPlayed -= ObserveSound; RestoreCapture(); }
    }
}
#endif
