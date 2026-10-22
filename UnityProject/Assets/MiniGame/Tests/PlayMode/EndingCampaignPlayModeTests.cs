using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    /// <summary>Production content reaches checkpoints through commands; scene input owns every ending line.</summary>
    public sealed class EndingCampaignPlayModeTests
    {
        private MiniGameBootstrap host;
        private DreamSequenceProfile dreams;
        private PlayerRebellionProfile rebellion;
        private CompiledDialoguePackage compiled;
        private string temporaryDirectory, previousSaveDirectory;
        private bool previousSkipStartup;

        [UnitySetUp]
        public IEnumerator LoadCombined()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            previousSaveDirectory = MainMenuController.SaveDirectoryOverride;
            temporaryDirectory = Path.Combine(Path.GetTempPath(), "minigame-ending-integration-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            MainMenuController.SaveDirectoryOverride = temporaryDirectory;
            MainMenuController.SkipStartup = true;
            MiniGameSessionHost.Release();
            if (MainMenuController.Current != null) Object.Destroy(MainMenuController.Current.gameObject);
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            dreams = Object.Instantiate(Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault"));
            rebellion = Object.Instantiate(Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion"));
            dreams.EnableDreamFlow = true;
            dreams.UseRoomCubemapEnding = true;
            host.ConfigureDreams(dreams, rebellion);
            host.StartCampaign();
            compiled = EndingCampaignScenario.LoadPackage();
            Assert.That(host.Runtime.Endings, Is.Not.Null, "EndingContentImport must install the current catalog.");
            Assert.That(MainMenuController.Current.ContinuePath, Does.StartWith(temporaryDirectory));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (host != null)
            {
                host.enabled = false;
                host.Player.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            if (MainMenuController.Current != null) Object.Destroy(MainMenuController.Current.gameObject);
            yield return null;
            var current = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Ending integration cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (current.IsValid() && current.isLoaded) yield return SceneManager.UnloadSceneAsync(current);
            if (dreams != null) Object.Destroy(dreams);
            if (rebellion != null) Object.Destroy(rebellion);
            MainMenuController.SkipStartup = previousSkipStartup;
            MainMenuController.SaveDirectoryOverride = previousSaveDirectory;
            if (temporaryDirectory != null)
            {
                string path = Path.GetFullPath(temporaryDirectory);
                Assert.That(Path.GetFileName(path), Does.StartWith("minigame-ending-integration-"));
                Assert.That(path.StartsWith(Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), Is.True);
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator NormalDayFourOpensDoorPlaysSevenSourceLinesAndReturnsWithoutContinue()
        {
            yield return DoorEnding(EndingCampaignRoute.Normal, EndingType.TakenAway, "supervisor-ending", 7);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator ReleasedDayTwoPriestUsesEightEndingLinesAndFinalBlackScreen()
        {
            yield return DoorEnding(EndingCampaignRoute.Priest, EndingType.PriestAttack, "priest-ending", 8);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator MissedDayOneQuotaUsesTheSupervisorEndingAndReturnsWithoutContinue()
        {
            yield return DoorEnding(EndingCampaignRoute.Quota, EndingType.LockedInRoom, "supervisor-ending", 7);
        }

        private IEnumerator DoorEnding(EndingCampaignRoute route, EndingType ending, string conversationId, int count)
        {
            var fixture = EndingCampaignScenario.Create(compiled, rebellion);
            EndingCampaignScenario.Drive(fixture, compiled.Rules, route, view => view.Game.Phase == GamePhase.Ending);
            Assert.That(fixture.GetView().Game.Ending, Is.EqualTo(ending));
            Assert.That(fixture.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
            Assert.That(fixture.GetView().Game.Day, Is.EqualTo(route == EndingCampaignRoute.Priest ? 3 : 4));
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            yield return null;
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
            Interact(InteractionTarget.Door, "Door approach");
            yield return Until(() => host.Runtime.EndingStage == EndingStage.Dialogue && !host.Screen.TransitionLocked,
                "the physical ending door completes its motion", 15);
            var expected = host.Runtime.Endings.Get(conversationId).Lines;
            Assert.That(expected.Length, Is.EqualTo(count));
            // Let the authored actor return from the door's clearance position before inspecting the first portrait.
            yield return new WaitForSecondsRealtime(.7f);
            for (int index = 0; index < count; index++)
            {
                yield return null;
                Assert.That(host.Runtime.CurrentEndingLine.SourceId, Is.EqualTo(expected[index].SourceId));
                if (index == 0)
                    yield return EndingVisualCapture.Capture(route == EndingCampaignRoute.Priest ? "priest-PE01"
                        : route.ToString().ToLowerInvariant() + "-supervisor", host.Runtime);
                if (route == EndingCampaignRoute.Priest && expected[index].PortraitId == "PE02")
                    yield return EndingVisualCapture.Capture("priest-PE02", host.Runtime);
                if (expected[index].BlackScreen)
                {
                    var black = host.GetComponentsInChildren<Transform>(true)
                        .Single(item => item.name == "Priest final black screen");
                    Assert.That(black.gameObject.activeInHierarchy, Is.True);
                    Assert.That(black.GetComponentInChildren<Image>().color, Is.EqualTo(Color.black));
                    yield return EndingVisualCapture.Capture("priest-final-black", host.Runtime);
                }
                yield return AdvanceSceneLine(expected[index].SourceId, false);
            }
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Thanks));
            yield return FinishThanksWithFreshClick(route.ToString().ToLowerInvariant());
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenEndingRequiresSelfDirectorySixteenLinesAndPurificationThenEntersRoomAndBedThanks()
        {
            var fixture = EndingCampaignScenario.CreateHidden(compiled, dreams, rebellion);
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            yield return null;
            var view = host.Runtime.GetView();
            Assert.That(view.Game.Day, Is.EqualTo(3));
            Assert.That(view.TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(view.TerminalOpen, Is.False);
            Assert.That(view.Patients.Select(patient => patient.Id), Is.EqualTo(new[] { GameRuntime.SelfPatientId }));
            Assert.That(view.AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Computer }));
            var terminal = host.Screen.GetComponentsInChildren<Transform>(true).Single(item => item.name == "Terminal");
            Assert.That(terminal.gameObject.activeInHierarchy, Is.False, "The green physical monitor must not open its UI before interaction.");
            var crt = Object.FindObjectsOfType<MeshRenderer>()
                .Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var properties = new MaterialPropertyBlock(); crt.GetPropertyBlock(properties, 0);
            Assert.That(properties.GetFloat("_CrtPower"), Is.InRange(.1f, 1f), "The unopened monitor must glow without opening the terminal UI.");
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.False);
            FocusInteraction(InteractionTarget.Computer, "Computer approach");
            yield return EndingVisualCapture.Capture("hidden-green-monitor-no-ui", host.Runtime);
            Interact(InteractionTarget.Computer, "Computer approach");
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.Directory, "self directory opens", 15);
            yield return EndingVisualCapture.Capture("hidden-self-directory", host.Runtime);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
            yield return null;
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(host.Runtime.GetView().PresentedEncounterId, Is.EqualTo(GameRuntime.SelfPatientId));
            var lines = host.Runtime.Endings.Get(GameRuntime.SelfPatientId).Lines;
            Assert.That(lines.Length, Is.EqualTo(16));
            for (int index = 0; index < lines.Length; index++)
            {
                yield return null;
                Assert.That(host.Runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(index));
                Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Any(text => text.gameObject.activeInHierarchy
                    && text.name.StartsWith("TranscriptText", StringComparison.Ordinal)
                    && text.text == lines[index].Text.Replace("\r\n", "\n").Replace('\r', '\n')), Is.True,
                    "The current source line must be visible in the ordinary transcript before confirmation: " + lines[index].SourceId);
                Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                Assert.That(ActiveDispositionButtons().Any(), Is.False);
                if (index == 0 || index == 3)
                    yield return EndingVisualCapture.Capture("hidden-self-dialogue-" + lines[index].PortraitId, host.Runtime);
                yield return AdvanceSceneLine(lines[index].SourceId, true);
            }
            Assert.That(host.Runtime.GetView().SelfReviewReady, Is.True);
            var dispositions = ActiveDispositionButtons();
            Assert.That(dispositions.Select(button => button.name), Is.EqualTo(new[] { "Disposition-Purify" }));
            yield return EndingVisualCapture.Capture("hidden-purify-only", host.Runtime);
            Assert.That(((MiniGameAudioButton)dispositions.Single()).ActivateWithAudio(), Is.True);
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null, "hidden room owner", 10);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            yield return VerifyRoomEntry(room);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(MiniGameSessionHost.CombinedScene));
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
            yield return EndingVisualCapture.Capture("hidden-space-ready", host.Runtime);
            var bounds = room.ExplorationBounds;
            Assert.That(bounds.Contains(room.PlayerPosition + Vector3.up * .1f), Is.True);
            // Walk with the room's real collision/clamp path until the existing bed interaction is available.
            for (int step = 0; step < 300 && !room.Ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(room.Ending.CanLieDown, Is.True);
            Assert.That(room.Ending.TryLieDown(), Is.True);
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Thanks));
            yield return RestorePartialHiddenThanks(room);
            yield return FinishThanksWithFreshClick("hidden");
        }

        private Button[] ActiveDispositionButtons() => host.Screen.GetComponentsInChildren<Button>(true)
            .Where(button => button.name.StartsWith("Disposition-", StringComparison.Ordinal)
                && button.gameObject.activeInHierarchy && button.interactable).ToArray();

        [UnityTest, Timeout(90000)]
        public IEnumerator MissingHiddenCaptureSourceKeepsBlackAndRetriesOnlyOnce()
        {
            var fixture = EndingCampaignScenario.CreateHidden(compiled, dreams, rebellion);
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            Interact(InteractionTarget.Computer, "Computer approach");
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.Directory, "failure fixture self directory", 15);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
            yield return Until(() => host.Runtime.GetView().Cue == null, "failure fixture self dialogue", 10);
            EndingCampaignScenario.DriveSelfReview(host.Runtime);
            var originalSky = dreams.RoomPanoramaSkybox;
            var originalPrefab = dreams.CubemapSourcePrefab;
            try
            {
                dreams.RoomPanoramaSkybox = null;
                dreams.CubemapSourcePrefab = null;
                LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: 海滩全景缺少捕获源预制体"));
                Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
                yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null
                    && Object.FindObjectOfType<RoomDreamEnding>().TransitionPhase == RoomEndingTransitionPhase.Failed,
                    "missing source fails beneath the opaque curtain", 10);
                var room = Object.FindObjectOfType<RoomDreamEnding>();
                Assert.That(room.Ready, Is.False);
                Assert.That(room.CompleteAtBed(), Is.False);
                Assert.That(room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a, Is.EqualTo(1));
                var blockedPosition = room.PlayerPosition;
                room.AdvanceExploration(Vector2.one, Vector2.one, 10);
                Assert.That(room.PlayerPosition, Is.EqualTo(blockedPosition));
                yield return EndingVisualCapture.Capture("hidden-source-failure-black", host.Runtime);
                yield return null; // Let the session observe the owner's failed initialization.
                dreams.RoomPanoramaSkybox = originalSky;
                dreams.CubemapSourcePrefab = originalPrefab;
                var session = MiniGameSessionHost.Current;
                Assert.That(session.RetryRoomEnding(), Is.True);
                Assert.That(session.RetryRoomEnding(), Is.False, "Repeated retry input must not start a second initialization.");
                Assert.That(room.RetryInitialization(), Is.False);
                Assert.That(room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a, Is.EqualTo(1));
                yield return VerifyRoomEntry(room);
                Assert.That(room.Ready, Is.True);
                Assert.That(room.Cubemap.IsReady, Is.True);
                Assert.That(session.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
                Assert.That(session.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                yield return EndingVisualCapture.Capture("hidden-source-retry-space", session.Runtime);
                yield return EndingVisualCapture.Flush();
            }
            finally
            {
                dreams.RoomPanoramaSkybox = originalSky;
                dreams.CubemapSourcePrefab = originalPrefab;
            }
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenThanksRestorationRetriesMissingSourceWithoutRepeatingPurificationOrBed()
        {
            yield return ReachHiddenPurification();
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && host.Runtime.EndingStage == EndingStage.Space, "initial hidden space", 15);
            var originalRoom = Object.FindObjectOfType<RoomDreamEnding>();
            for (int step = 0; step < 300 && !originalRoom.Ending.CanLieDown; step++)
                originalRoom.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(originalRoom.Ending.TryLieDown(), Is.True);
            Assert.That(originalRoom.CompleteAtBed(), Is.False);
            yield return Until(() => Object.FindObjectOfType<EndingThanksView>() != null, "thanks before failed restore", 5);
            var originalThanks = Object.FindObjectOfType<EndingThanksView>();
            originalThanks.UseExternalClock = true;
            originalThanks.RestoreCheckpoint(new EndingThanksCheckpoint { Phase = EndingThanksPhase.FadingOut, Elapsed = .4f });
            float opacity = originalThanks.Opacity;
            Vector3 savedBedCamera = originalRoom.ViewCamera.transform.position;
            Quaternion savedBedRotation = originalRoom.ViewCamera.transform.rotation;
            originalRoom.Ending.SetPaused(true);
            var session = MiniGameSessionHost.Current;
            string before = session.Runtime.Save();
            string path = Path.Combine(temporaryDirectory, "hidden-thanks-failed-restore.json");
            Assert.That(session.TrySaveTo(path, out var saveError), Is.True, saveError);
            var originalSky = dreams.RoomPanoramaSkybox;
            var originalPrefab = dreams.CubemapSourcePrefab;
            try
            {
                dreams.RoomPanoramaSkybox = null;
                dreams.CubemapSourcePrefab = null;
                LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: 海滩全景缺少捕获源预制体"));
                Assert.That(session.TryLoadFrom(path, out var loadError), Is.True, loadError);
                yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.TransitionPhase == RoomEndingTransitionPhase.Failed,
                    "thanks reconstruction fails under black", 10);
                yield return null;
                var restoredRoom = Object.FindObjectOfType<RoomDreamEnding>();
                Assert.That(Object.FindObjectOfType<EndingThanksView>(), Is.Null);
                Assert.That(restoredRoom.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a, Is.EqualTo(1));
                Assert.That(restoredRoom.CompleteAtBed(), Is.False);
                Assert.That(session.Runtime.Save(), Is.EqualTo(before));
                dreams.RoomPanoramaSkybox = originalSky;
                dreams.CubemapSourcePrefab = originalPrefab;
                session.Runtime.SetPaused(false);
                Assert.That(session.RetryRoomEnding(), Is.True, "Thanks reconstruction must support the same retry as initial entry.");
                Assert.That(session.RetryRoomEnding(), Is.False);
                Assert.That(restoredRoom.RetryInitialization(), Is.False);
                yield return Until(() => restoredRoom.TransitionPhase == RoomEndingTransitionPhase.Exploring,
                    "retried thanks space finishes reconstruction", 15);
                session.Runtime.SetPaused(true); // Freeze the saved thanks opacity as soon as reconstruction finishes.
                yield return Until(() => Object.FindObjectOfType<EndingThanksView>() != null,
                    "successful retry restores the original thanks fade", 5);
                var restoredThanks = Object.FindObjectOfType<EndingThanksView>();
                restoredThanks.UseExternalClock = true;
                Assert.That(restoredThanks.Opacity, Is.EqualTo(opacity).Within(.001f));
                Assert.That(restoredRoom.RoomSurface, Is.Not.Null);
                Assert.That(restoredRoom.Cubemap.IsReady, Is.True);
                Assert.That(restoredRoom.Ready, Is.False);
                Assert.That(Vector3.Distance(restoredRoom.ViewCamera.transform.position, savedBedCamera), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(restoredRoom.ViewCamera.transform.rotation, savedBedRotation), Is.LessThan(.01f));
                Assert.That(session.Runtime.Save(), Is.EqualTo(before), "Neither purification nor bed completion may run during retry.");
                Assert.That(session.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                Assert.That(restoredRoom.CompleteAtBed(), Is.False);
                restoredRoom.Ending.SetPaused(false);
                yield return FinishThanksWithFreshClick("hidden-thanks-retry");
            }
            finally
            {
                dreams.RoomPanoramaSkybox = originalSky;
                dreams.CubemapSourcePrefab = originalPrefab;
            }
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenFadingOutCrossSceneRestorePreservesTheFirstRenderedCameraPose()
        {
            yield return ReachHiddenPurification();
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null
                && Object.FindObjectOfType<RoomDreamEnding>().CaptureTransitionCheckpoint().Elapsed >= .08f,
                "partially transparent hidden fade", 5);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.FadingOut));
            var session = MiniGameSessionHost.Current;
            session.Runtime.SetPaused(true);
            var camera = host.Player.ViewCamera;
            Vector3 savedPosition = camera.transform.position;
            Quaternion savedRotation = camera.transform.rotation;
            float savedFov = camera.fieldOfView;
            float savedElapsed = room.CaptureTransitionCheckpoint().Elapsed;
            float savedOpacity = room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a;
            Assert.That(savedOpacity, Is.InRange(.01f, .99f));
            string before = session.Runtime.Save();
            string path = Path.Combine(temporaryDirectory, "hidden-apartment-fade.json");
            Assert.That(session.TrySaveTo(path, out var saveError), Is.True, saveError);
            yield return EndingVisualCapture.Capture("hidden-fading-out-before-save", session.Runtime);

            session.enabled = false; // Keep the surviving session from automatically reopening Combined during the deliberate unload.
            var previousScene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Hidden fade cross-scene restore " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(previousScene);
            host = null;
            bool rendered = false;
            Vector3 firstPosition = default;
            Quaternion firstRotation = default;
            float firstFov = 0;
            void ObserveCamera(Camera candidate)
            {
                var currentHost = Object.FindObjectOfType<MiniGameBootstrap>();
                if (rendered || currentHost == null || candidate != currentHost.Player.ViewCamera) return;
                rendered = true;
                firstPosition = candidate.transform.position;
                firstRotation = candidate.transform.rotation;
                firstFov = candidate.fieldOfView;
            }
            void ObservePipeline(ScriptableRenderContext context, Camera candidate) => ObserveCamera(candidate);
            Camera.onPreCull += ObserveCamera;
            RenderPipelineManager.beginCameraRendering += ObservePipeline;
            try
            {
                Assert.That(session.TryLoadFrom(path, out var loadError), Is.True, loadError);
                session.enabled = true;
                yield return Until(() => rendered && Object.FindObjectOfType<RoomDreamEnding>() != null,
                    "first actual camera render after loading the saved partial fade", 15);
                host = Object.FindObjectOfType<MiniGameBootstrap>();
                Assert.That(host, Is.Not.Null);
                host.Player.enabled = false;
                Assert.That(Vector3.Distance(firstPosition, savedPosition), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(firstRotation, savedRotation), Is.LessThan(.01f));
                Assert.That(firstFov, Is.EqualTo(savedFov).Within(.001f));
                room = Object.FindObjectOfType<RoomDreamEnding>();
                Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.FadingOut));
                Assert.That(room.CaptureTransitionCheckpoint().Elapsed, Is.EqualTo(savedElapsed).Within(.001f));
                Assert.That(room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a,
                    Is.EqualTo(savedOpacity).Within(.001f));
                Assert.That(session.Runtime.Save(), Is.EqualTo(before));
                yield return EndingVisualCapture.Capture("hidden-fading-out-cross-scene-restored", session.Runtime);
                Assert.That(Vector3.Distance(host.Player.ViewCamera.transform.position, savedPosition), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(host.Player.ViewCamera.transform.rotation, savedRotation), Is.LessThan(.01f));
                session.Runtime.SetPaused(false);
                yield return Until(() => room.Ready && session.Runtime.EndingStage == EndingStage.Space,
                    "restored partial fade continues into hidden space", 15);
                Assert.That(session.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                yield return EndingVisualCapture.Flush();
            }
            finally
            {
                Camera.onPreCull -= ObserveCamera;
                RenderPipelineManager.beginCameraRendering -= ObservePipeline;
                session.enabled = true;
                if (host == null) host = Object.FindObjectOfType<MiniGameBootstrap>();
            }
        }

        private IEnumerator ReachHiddenPurification()
        {
            host.RestoreCheckpoint(EndingCampaignScenario.CreateHidden(compiled, dreams, rebellion).Save());
            yield return null;
            Interact(InteractionTarget.Computer, "Computer approach");
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.Directory, "self directory for restoration fixture", 15);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
            yield return Until(() => host.Runtime.GetView().Cue == null, "self dialogue for restoration fixture", 10);
            EndingCampaignScenario.DriveSelfReview(host.Runtime);
        }

        private IEnumerator AdvanceSceneLine(string sourceId, bool terminal)
        {
            // One source line may wrap over multiple UI pages; every page uses the live input handler.
            for (int page = 0; page < 24 && host.Runtime.CurrentEndingLine?.SourceId == sourceId; page++)
            {
                yield return null;
                bool accepted = terminal ? host.Screen.HandleTerminalKey(KeyCode.Return) : host.Screen.AdvanceWorldDialogue();
                Assert.That(accepted, Is.True, "The visible dialogue page must accept one confirmation: " + sourceId);
                yield return null;
            }
            Assert.That(host.Runtime.CurrentEndingLine?.SourceId, Is.Not.EqualTo(sourceId));
        }

        private IEnumerator VerifyRoomEntry(RoomDreamEnding room)
        {
            yield return Until(() => room.TransitionPhase == RoomEndingTransitionPhase.BlackHold, "black hold begins", 10);
            float blackStarted = Time.realtimeSinceStartup;
            EndingVisualCapture.Request("hidden-full-black", host.Runtime);
            while (room.TransitionPhase == RoomEndingTransitionPhase.BlackHold)
            {
                Assert.That(room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a, Is.EqualTo(1));
                Assert.That(room.Ready, Is.False);
                yield return null;
            }
            Assert.That(Time.realtimeSinceStartup - blackStarted, Is.GreaterThanOrEqualTo(1.9f));
            Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.Entering));
            float entryStarted = Time.realtimeSinceStartup;
            int interpolatedFrames = 0;
            bool entryCaptured = false;
            while (room.TransitionPhase == RoomEndingTransitionPhase.Entering)
            {
                var state = room.CaptureTransitionCheckpoint();
                float t = Mathf.SmoothStep(0, 1, state.Elapsed / RoomEndingTransitionClock.EntrySeconds);
                Assert.That(Vector3.Distance(room.ViewCamera.transform.position,
                    Vector3.Lerp(state.EntryStartPosition, state.EntryTargetPosition, t)), Is.LessThan(.01f));
                Assert.That(Quaternion.Angle(room.ViewCamera.transform.rotation,
                    Quaternion.Slerp(state.EntryStartRotation, state.EntryTargetRotation, t)), Is.LessThan(.1f));
                if (t > .05f && t < .95f) interpolatedFrames++;
                if (!entryCaptured && t >= .35f && t < .95f)
                {
                    EndingVisualCapture.Request("hidden-entry-camera", host.Runtime);
                    entryCaptured = true;
                }
                yield return null;
            }
            Assert.That(Time.realtimeSinceStartup - entryStarted, Is.GreaterThanOrEqualTo(.85f));
            Assert.That(interpolatedFrames, Is.GreaterThan(0));
            yield return Until(() => room.Ready && host.Runtime.EndingStage == EndingStage.Space, "room exploration ready", 5);
        }

        private IEnumerator RestorePartialHiddenThanks(RoomDreamEnding originalRoom)
        {
            yield return Until(() => Object.FindObjectOfType<EndingThanksView>() != null, "hidden thanks begins fading", 5);
            var originalThanks = Object.FindObjectOfType<EndingThanksView>();
            originalThanks.UseExternalClock = true;
            originalThanks.RestoreCheckpoint(new EndingThanksCheckpoint { Phase = EndingThanksPhase.FadingOut, Elapsed = .4f });
            float opacity = originalThanks.Opacity;
            Assert.That(opacity, Is.EqualTo(.4f / EndingThanksView.FadeSeconds).Within(.001f));
            yield return EndingVisualCapture.Capture("hidden-thanks-partial-before-save", host.Runtime);
            originalRoom.Ending.SetPaused(true);
            var session = MiniGameSessionHost.Current;
            string before = session.Runtime.Save();
            string path = Path.Combine(temporaryDirectory, "hidden-thanks-partial.json");
            Assert.That(session.TrySaveTo(path, out var saveError), Is.True, saveError);
            Assert.That(session.TryLoadFrom(path, out var loadError), Is.True, loadError);
            Assert.That(session.Runtime.EndingStage, Is.EqualTo(EndingStage.Thanks));
            Assert.That(session.Runtime.Save(), Is.EqualTo(before), "Loading the visual fade must not resubmit purification or advance the campaign.");
            yield return Until(() => Object.FindObjectOfType<EndingThanksView>() != null
                && Object.FindObjectOfType<EndingThanksView>() != originalThanks
                && Object.FindObjectOfType<RoomDreamEnding>() != null
                && Object.FindObjectOfType<RoomDreamEnding>() != originalRoom,
                "hidden thanks restores over its space instead of the ordinary apartment", 15);
            var restoredRoom = Object.FindObjectOfType<RoomDreamEnding>();
            var restoredThanks = Object.FindObjectOfType<EndingThanksView>();
            restoredThanks.UseExternalClock = true;
            Assert.That(restoredRoom.RoomSurface, Is.Not.Null);
            Assert.That(restoredRoom.Cubemap.IsReady, Is.True);
            Assert.That(restoredRoom.Ready, Is.False, "The thanks page keeps exploration locked.");
            Assert.That(restoredRoom.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.Exploring));
            Assert.That(restoredThanks.Opacity, Is.EqualTo(opacity).Within(.001f));
            Assert.That(session.Runtime.GetView().Paused, Is.True);
            Assert.That(session.Runtime.ConfirmSelfPurification().Accepted, Is.False);
            restoredRoom.Ending.SetPaused(false);
            yield return EndingVisualCapture.Capture("hidden-thanks-partial-restored", session.Runtime);
        }

        private IEnumerator FinishThanksWithFreshClick(string route)
        {
            yield return Until(() => Object.FindObjectOfType<EndingThanksView>() != null, "shared thanks appears", 5);
            var thanks = Object.FindObjectOfType<EndingThanksView>();
            thanks.UseExternalClock = true;
            thanks.AdvanceForTests(EndingThanksView.FadeSeconds + .1f, true, true);
            Assert.That(thanks.IsPageVisible, Is.True);
            Assert.That(thanks.IsComplete, Is.False, "The click that finishes the fade cannot dismiss the page.");
            yield return EndingVisualCapture.Capture(route + "-thanks", MiniGameSessionHost.Current.Runtime);
            thanks.AdvanceForTests(.1f, true, true);
            Assert.That(thanks.IsComplete, Is.False, "A held click cannot dismiss the page.");
            thanks.AdvanceForTests(.1f, false, false);
            thanks.AdvanceForTests(.1f, true, true);
            yield return null;
            Assert.That(MainMenuController.Current.IsVisible, Is.True);
            Assert.That(MainMenuController.Current.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(MainMenuController.Current.View.ContinueButton.interactable, Is.False);
            Assert.That(MainMenuController.Current.HasRetainedGame, Is.False);
            Assert.That(MainMenuContinueStore.HasCompletedInspection(temporaryDirectory), Is.True);
            yield return EndingVisualCapture.Capture(route + "-main-menu", MiniGameSessionHost.Current.Runtime);
            yield return EndingVisualCapture.Flush();
        }

        private void Interact(InteractionTarget target, string anchorName)
        {
            FocusInteraction(target, anchorName);
            Assert.That(host.Player.InteractFocused(), Is.True);
        }

        private void FocusInteraction(InteractionTarget target, string anchorName)
        {
            var objects = host.gameObject.scene.GetRootGameObjects();
            var interaction = objects.SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(item => item.Target == target);
            var anchor = objects.SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Single(item => item.name == anchorName);
            var character = host.Player.GetComponent<CharacterController>();
            bool enabled = character.enabled;
            character.enabled = false; host.Player.transform.position = anchor.position; character.enabled = enabled;
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(interaction));
        }

        private static IEnumerator Until(Func<bool> condition, string description, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, description + " timed out");
        }
    }

    internal enum EndingCampaignRoute { Normal, Priest, Quota, Hidden }

    internal static class EndingCampaignScenario
    {
        internal static CompiledDialoguePackage LoadPackage()
        {
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(package, Is.Not.Null);
            return DialoguePackageCompiler.Compile(package.PackageSource.text);
        }

        internal static GameRuntime Create(CompiledDialoguePackage compiled, PlayerRebellionProfile rebellion)
        {
            var rules = compiled.Rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            var runtime = new GameRuntime(rules, compiled.Content, true);
            runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            return runtime;
        }

        internal static GameRuntime CreateHidden(CompiledDialoguePackage compiled, DreamSequenceProfile dreams, PlayerRebellionProfile rebellion)
        {
            Assert.That(dreams.EnableDreamFlow, Is.True);
            var runtime = Create(compiled, rebellion);
            Drive(runtime, compiled.Rules, EndingCampaignRoute.Hidden, view => view.IsSelfReview);
            return runtime;
        }

        internal static void DriveToSelfReview(GameRuntime runtime)
        {
            if (runtime.Endings == null) runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            Drive(runtime, LoadPackage().Rules, EndingCampaignRoute.Hidden, view => view.IsSelfReview);
        }

        internal static void DriveSelfReview(GameRuntime runtime)
        {
            Assert.That(runtime.GetView().PresentedEncounterId, Is.EqualTo(GameRuntime.SelfPatientId));
            for (int index = 0; index < 16 && runtime.CurrentEndingLine != null; index++)
                Assert.That(runtime.CompleteEndingLine(runtime.CurrentEndingLine.SourceId).Accepted, Is.True);
            Assert.That(runtime.GetView().SelfReviewReady, Is.True);
        }

        internal static void Drive(GameRuntime runtime, GameRules rules, EndingCampaignRoute route, Func<RuntimeView, bool> stop)
        {
            var deltas = rules.Days.SelectMany(day => day.Meetings).SelectMany(meeting => meeting.NormalRounds
                .Concat(meeting.SpecialRound == null ? Array.Empty<RoundDefinition>() : new[] { meeting.SpecialRound }))
                .SelectMany(round => round.Responses).GroupBy(response => response.Id)
                .ToDictionary(group => group.Key, group => group.First().TrustDelta);
            for (int step = 0; step < 3000; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                string quotaPatient = view.Game.Day == 0 ? "tutorial" : view.Game.Day == 1 ? "battlefield-doctor"
                    : view.Game.Day == 2 ? route == EndingCampaignRoute.Normal ? "priest" : "former-ruler" : "child";
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                {
                    var patients = view.Patients.Where(patient => patient.CanReceiveToday).ToArray();
                    Assert.That(patients, Is.Not.Empty);
                    result = runtime.EnterSelectedPatient((patients.FirstOrDefault(patient => patient.Id == quotaPatient) ?? patients[0]).Id);
                }
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        var available = view.Game.AvailableDispositions;
                        bool releasePriest = route == EndingCampaignRoute.Priest && view.Game.Day == 2 && view.Game.CurrentPatientId == "priest";
                        bool purify = view.Game.CurrentPatientId == quotaPatient && (route != EndingCampaignRoute.Quota || view.Game.Day == 0);
                        var action = releasePriest ? Disposition.Release : purify ? Disposition.Purify : Disposition.Treat;
                        Assert.That(available, Does.Contain(action), "Real source route lacks the required disposition.");
                        result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, action);
                    }
                    else
                    {
                        bool highTrust = route == EndingCampaignRoute.Hidden || route == EndingCampaignRoute.Priest && view.Game.CurrentPatientId == "priest";
                        var ordered = view.Game.AvailableResponseIds.OrderBy(id => highTrust ? -deltas[id] : deltas[id]);
                        result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, ordered.First());
                    }
                }
                else if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                    result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("Ending route reached an unexpected phase: " + route + "/" + view.Game.Phase); return; }
                Assert.That(result.Accepted, Is.True, "Real campaign command rejected: " + route + "/" + step + "/" + view.Game.Phase);
            }
            Assert.Fail("Ending route exceeded its bounded command count: " + route);
        }
    }
}
