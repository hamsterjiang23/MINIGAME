using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamCampaignIntegrationTests
    {
        private MiniGameBootstrap host;
        private DreamSequenceProfile dreams, originalDreams;
        private PlayerRebellionProfile rebellion, originalRebellion;
        private MiniGameAudioProfile audioOverride;
        private RetroScreenProfile retroOverride;
        private CompiledDialoguePackage compiled;
        private string originalDreamJson, originalRebellionJson;
        private bool previousSkipStartup;
        private readonly List<string> saveFiles = new List<string>();

        [UnitySetUp]
        public IEnumerator LoadCombined()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            MiniGameSessionHost.Release();
            yield return null;
            yield return SceneManager.LoadSceneAsync(MiniGameSessionHost.CombinedScene, LoadSceneMode.Single);
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            Assert.That(host.Dreams, Is.Not.Null, "Install the production Combined dream bindings before running integration tests.");
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            originalDreams = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            originalRebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
            Assert.That(originalDreams, Is.Not.Null);
            Assert.That(originalRebellion, Is.Not.Null);
            originalDreamJson = JsonUtility.ToJson(originalDreams);
            originalRebellionJson = JsonUtility.ToJson(originalRebellion);
            dreams = Object.Instantiate(originalDreams);
            // Keep the original beach-scene compatibility scenarios; the room path has its own suite.
            dreams.UseRoomCubemapEnding = false;
            rebellion = Object.Instantiate(originalRebellion);
            dreams.EnableDreamFlow = true;
            dreams.BlackScreenSeconds = 5;
            dreams.WakeBlackSeconds = 1;
            foreach (var day in dreams.Days) day.Duration = 5;
            rebellion.InitialValue = 0; rebellion.Maximum = 100; rebellion.Threshold = 100;
            rebellion.Choices = new List<RebellionChoiceDelta>();
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(package, Is.Not.Null);
            compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
            StartConfiguredCampaign(false);
            host.Dreams.UseExternalClock = true;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return EndingVisualCapture.Flush();
            MainMenuController.SkipStartup = previousSkipStartup;
            if (host != null)
            {
                host.enabled = false;
                if (host.Player != null) host.Player.enabled = false;
                if (host.Dreams != null) host.Dreams.PrepareForRuntimeChange();
                if (host.Effects != null) host.Effects.PrepareForRuntimeChange();
            }
            var ending = Object.FindObjectOfType<DreamEndingController>();
            if (ending != null) ending.enabled = false;
            MiniGameSessionHost.Release();
            yield return null;
            var current = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Dream integration cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            if (current.IsValid() && current.isLoaded && (current.name == MiniGameSessionHost.CombinedScene || current.name == MiniGameSessionHost.BeachScene))
                yield return SceneManager.UnloadSceneAsync(current);
            if (dreams != null) Object.Destroy(dreams);
            if (rebellion != null) Object.Destroy(rebellion);
            if (audioOverride != null) Object.Destroy(audioOverride);
            if (retroOverride != null) Object.Destroy(retroOverride);
            foreach (string file in saveFiles)
            {
                Assert.That(Path.GetFileName(file), Does.StartWith("minigame-dream-integration-"));
                Assert.That(Path.GetFullPath(file).StartsWith(Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), Is.True);
                if (File.Exists(file)) File.Delete(file);
            }
            saveFiles.Clear();
            if (originalDreams != null && originalDreamJson != null) Assert.That(JsonUtility.ToJson(originalDreams), Is.EqualTo(originalDreamJson));
            if (originalRebellion != null && originalRebellionJson != null) Assert.That(JsonUtility.ToJson(originalRebellion), Is.EqualTo(originalRebellionJson));
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            yield return null;
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator DayZeroWaitsForTheBedAndOneInteractionSleepsWithoutShowingADream()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => EveningClosed(view, 0));
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            yield return null;
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(host.Runtime.GetView().Cue, Is.Null, "Day0 must wait for the player's bed interaction.");
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Dreams.OwnsCamera, Is.False);
            Assert.That(host.Dreams.StageTexture, Is.Null);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            var bed = Interaction(InteractionTarget.Bed);
            TeleportTo("Bed approach", bed.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(bed));
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.Sleep));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero, "Sleep advances the day after its fade completes.");
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            yield return Until(() => host.Runtime.GetView().Game.Day == 1 && host.Runtime.GetView().Cue == null,
                "one sleep completes into the first morning", 5);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            host.Player.InteractFocused();
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1), "Repeated E must not advance past the first morning.");
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator DayOneBedProjectsOnTheAuthoredWallThenWakesOnceAtFullBlack()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => EveningClosed(view, 1));
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(host.Dreams.OwnsCamera, Is.False);
            var original = InteractWithBed();
            yield return Until(() => host.Runtime.GetView().Cue?.Kind == PresentationKind.Dream, "Day1 first-bed dream", 8);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(host.Dreams.OwnsCamera, Is.True);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.Empty);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            var projection = host.transform.Find("Dream wall projection").GetComponent<Renderer>();
            var wall = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .Single(item => item.name == "B_Wall__3_.001");
            Assert.That(Mathf.Abs(projection.bounds.center.z - wall.bounds.center.z), Is.LessThan(.2f));
            Assert.That(projection.transform.localScale.x / projection.transform.localScale.y, Is.EqualTo(16f / 9f).Within(.001f));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            if (host.Dreams.Phase == DreamPlaybackPhase.Aligning) host.Dreams.Advance(dreams.BlackScreenSeconds);
            Assert.That(host.Dreams.IsPlaying, Is.True);
            host.Dreams.Advance(.04f);
            Assert.That(projection.enabled, Is.True);
            Assert.That(projection.sharedMaterial.mainTexture, Is.SameAs(host.Dreams.StageTexture));
            Assert.That(host.Dreams.StageTexture.width, Is.EqualTo(1920));
            Assert.That(host.Dreams.StageTexture.height, Is.EqualTo(1080));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            float seconds = host.Dreams.PlaybackSeconds;
            host.Dreams.Advance(10);
            yield return null;
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(seconds));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            host.Dreams.Advance(dreams.GetDay(1).Duration);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            host.Dreams.Advance(dreams.ReturnSeconds);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.None));
            Assert.That(host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade").color.a,
                Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Effects.CaptureSleepCheckpoint()?.Phase, Is.EqualTo(SleepTransitionPhase.Waking));
            Assert.That(host.GetComponent<BedtimeGuidance>().Isolation, Is.Zero,
                "Morning lighting must already be selected under the full-black handoff.");
            Assert.That(Vector3.Dot(host.Player.ViewCamera.transform.forward, Vector3.up), Is.GreaterThan(.999f));
            AssertNoDreamBedEffects();
            Assert.That(projection.enabled, Is.False);
            EndingVisualCapture.Request("day1-wake-black", host.Runtime);
            host.Dreams.Advance(60);
            bool capturedEyesOpening = false;
            yield return Until(() =>
            {
                if (!capturedEyesOpening)
                {
                    float opacity = host.Screen.GetComponentsInChildren<Image>(true)
                        .Single(image => image.name == "PresentationFade").color.a;
                    if (opacity > 0 && opacity < 1)
                    {
                        EndingVisualCapture.Request("day1-wake-eyes-opening", host.Runtime);
                        capturedEyesOpening = true;
                    }
                }
                return !host.Effects.SleepCameraActive;
            }, "Wake finishes without another bed interaction", 5);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            original.AssertPositionAndFieldOfView(host.Player);
            AssertNoDreamBedEffects();
            yield return EndingVisualCapture.Capture("day1-wake-completed", host.Runtime);
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator WakeFadeTurnAndStandRestoreTheirExactPausedFrameWithoutAdvancingTheDayAgain()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream);
            host.RestoreCheckpoint(fixture.Save());
            host.Dreams.Advance(dreams.BlackScreenSeconds);
            host.Dreams.Advance(dreams.GetDay(1).Duration);
            host.Dreams.Advance(dreams.ReturnSeconds);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var camera = host.Player.ViewCamera;
            var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
            var checkpoint = host.Effects.CaptureSleepCheckpoint();
            Assert.That(checkpoint?.Phase, Is.EqualTo(SleepTransitionPhase.Waking));
            string saved = host.Runtime.Save();
            foreach (float elapsed in new[] { 0f, .2f, .95f, 1.95f })
            {
                checkpoint.WakeElapsed = elapsed;
                Assert.That(host.Effects.RestoreSleepCheckpoint(checkpoint), Is.True);
                var position = camera.transform.position;
                var rotation = camera.transform.rotation;
                float opacity = fade.color.a;
                host.RestoreCheckpoint(saved);
                Assert.That(host.Effects.RestoreSleepCheckpoint(checkpoint), Is.True);
                Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
                Assert.That(fade.color.a, Is.EqualTo(opacity).Within(.0001f));
                yield return new WaitForSecondsRealtime(.08f);
                Assert.That(host.Effects.CaptureSleepCheckpoint().WakeElapsed, Is.EqualTo(elapsed));
                Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
                AssertNoDreamBedEffects();
                Assert.That(host.Runtime.Save(), Is.EqualTo(saved));
            }
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(() => !host.Effects.SleepCameraActive, "restored wake completes", 5);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Runtime.GetView().Cue, Is.Null);
            Assert.That(host.Screen.TransitionLocked, Is.False);
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator OldDreamTailCheckpointsWaitUnderBlackWhilePausedAndWakeWithoutRevealingABed()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream);
            Assert.That(fixture.SetPaused(true).Accepted, Is.True);
            string saved = fixture.Save();
            foreach (var phase in new[] { DreamPlaybackPhase.Returning, DreamPlaybackPhase.BlackHold, DreamPlaybackPhase.Revealing })
            {
                host.RestoreCheckpoint(saved);
                var checkpoint = host.Dreams.CaptureCheckpoint();
                checkpoint.Phase = phase;
                checkpoint.PlaybackSeconds = dreams.GetDay(1).Duration;
                checkpoint.Elapsed = phase == DreamPlaybackPhase.Returning ? dreams.ReturnSeconds * .4f : .3f;
                host.Dreams.RestoreCheckpoint(checkpoint);
                Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
                var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
                if (phase != DreamPlaybackPhase.Returning) Assert.That(fade.color.a, Is.EqualTo(1));
                AssertNoDreamBedEffects();
                float elapsed = host.Dreams.CaptureCheckpoint().Elapsed;
                host.Dreams.Advance(60);
                yield return null;
                Assert.That(host.Dreams.CaptureCheckpoint().Elapsed, Is.EqualTo(elapsed));
                Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
                Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
                host.Dreams.Advance(dreams.ReturnSeconds);
                Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
                Assert.That(host.Dreams.OwnsCamera, Is.False);
                Assert.That(fade.color.a, Is.EqualTo(1));
                Assert.That(host.Effects.CaptureSleepCheckpoint()?.Phase, Is.EqualTo(SleepTransitionPhase.Waking));
                AssertNoDreamBedEffects();
            }
        }

        [UnityTest, Timeout(90000), Category("InteractionWakeGm")]
        public IEnumerator ResidentTrustSelfPurificationKeepsTheRuntimeAcrossBeachAndRestoresEndingProgress()
        {
            StartConfiguredCampaign(false);
            dreams.LieDownSeconds = .15f;
            dreams.SubtitleDelay = .1f;
            foreach (var line in dreams.EndingLines) { line.Duration = 1; line.Voice = null; }
            var fixture = EndingCampaignScenario.CreateHidden(compiled, dreams, rebellion);
            AssertResidentTrustEligibility(fixture, MiniGameSessionHost.Current.Rules);
            Assert.That(fixture.GetView().Game.Rebellion, Is.Zero, "The resident trust sum, not the global counter, qualifies this real command path.");
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            yield return null;
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Computer }));
            Assert.That(host.Runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.False);
            Assert.That(host.Runtime.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(new[] { GameRuntime.SelfPatientId }));
            Assert.That(host.Runtime.GetView().TerminalBootRequired, Is.False);
            var physicalScreen = Object.FindObjectsOfType<MeshRenderer>().Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var screenProperties = new MaterialPropertyBlock(); physicalScreen.GetPropertyBlock(screenProperties, 0);
            Assert.That(screenProperties.HasProperty(Shader.PropertyToID("_CrtPower")), Is.True);
            Assert.That(screenProperties.GetFloat("_CrtPower"), Is.InRange(.1f, 1f), "The unbooted CRT surface must still emit the self-review guidance light.");
            yield return null;
            physicalScreen.GetPropertyBlock(screenProperties, 0);
            Assert.That(screenProperties.GetFloat("_CrtPower"), Is.InRange(.1f, 1f), "Bedtime and room lighting must preserve the unbooted monitor's guidance light.");
            yield return EndingVisualCapture.Capture("day3-self-review-no-bed-light", host.Runtime);
            var computer = Interaction(InteractionTarget.Computer);
            TeleportTo("Computer approach", computer.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(computer));
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.Directory
                && !host.Screen.WorldInputBlockedThisFrame && host.Screen.PowerPhase == TerminalPowerPhase.Running,
                "actual self-review directory transition and fresh input frame", 12);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
            yield return null;
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(host.Runtime.GetView().PresentedEncounterId, Is.EqualTo(GameRuntime.SelfPatientId));
            var authored = host.Runtime.Endings.Get(GameRuntime.SelfPatientId).Lines;
            Assert.That(authored.Length, Is.EqualTo(16));
            for (int index = 0; index < authored.Length; index++)
            {
                yield return null;
                Assert.That(host.Runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(index));
                Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                string sourceId = authored[index].SourceId;
                for (int page = 0; page < 24 && host.Runtime.CurrentEndingLine?.SourceId == sourceId; page++)
                {
                    yield return null;
                    Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True, sourceId);
                    yield return null;
                }
                Assert.That(host.Runtime.CurrentEndingLine?.SourceId, Is.Not.EqualTo(sourceId));
            }
            Assert.That(host.Runtime.GetView().SelfReviewReady, Is.True);
            var buttons = host.Screen.GetComponentsInChildren<Button>(true).Where(item => item.name.StartsWith("Disposition-", StringComparison.Ordinal)
                && item.IsActive() && item.IsInteractable()).ToArray();
            Assert.That(buttons.Select(item => item.name), Is.EqualTo(new[] { "Disposition-Purify" }));
            Assert.That(buttons[0].GetComponentInChildren<Text>().text, Does.Contain("净化"));
            var sameRuntime = host.Runtime;
            var session = MiniGameSessionHost.Current;
            Assert.That(buttons[0], Is.InstanceOf<MiniGameAudioButton>());
            Assert.That(((MiniGameAudioButton)buttons[0]).ActivateWithAudio(), Is.True);
            Assert.That(sameRuntime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            yield return Until(() => SceneManager.GetActiveScene().name == MiniGameSessionHost.BeachScene && !session.IsTransitioning,
                "real beach scene transition", 15);
            yield return null;
            var ending = Object.FindObjectOfType<DreamEndingController>();
            Assert.That(ending, Is.Not.Null);
            ending.enabled = false;
            Assert.That(MiniGameSessionHost.Current, Is.SameAs(session));
            Assert.That(session.Runtime, Is.SameAs(sameRuntime));
            Assert.That(session.Runtime.GetView().Game.Rebellion, Is.Zero);
            Assert.That(ending.Profile, Is.SameAs(dreams));
            ending.AdvanceForTests(1, .4f);
            var walk = ending.CaptureCheckpoint();
            Assert.That(walk.WalkDistance, Is.GreaterThan(0));
            string walkingSave = Save(session);
            ending.AdvanceForTests(1, 1);
            session.LoadFrom(walkingSave);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            Assert.That(ending.CaptureCheckpoint().WalkDistance, Is.EqualTo(walk.WalkDistance).Within(.0001f));
            Assert.That(session.Runtime.GetView().Game.Rebellion, Is.Zero);
            ending.AdvanceForTests(1, 100);
            Assert.That(ending.TryLieDown(), Is.True);
            Assert.That(ending.TryLieDown(), Is.False);
            ending.AdvanceForTests(0, dreams.LieDownSeconds + dreams.SubtitleDelay + .25f);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            string caption = ending.CurrentSubtitle;
            Assert.That(caption, Is.Not.Empty);
            var drift = ending.CaptureCheckpoint();
            string floatingSave = Save(session);
            ending.AdvanceForTests(0, 2.5f);
            session.LoadFrom(floatingSave);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            var resumed = ending.CaptureCheckpoint();
            Assert.That(resumed.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            Assert.That(resumed.DriftElapsed, Is.EqualTo(drift.DriftElapsed).Within(.0001f));
            Assert.That(resumed.SubtitleIndex, Is.EqualTo(drift.SubtitleIndex));
            Assert.That(resumed.SubtitleElapsed, Is.EqualTo(drift.SubtitleElapsed).Within(.0001f));
            Assert.That(ending.CurrentSubtitle, Is.EqualTo(caption));
            Assert.That(session.Runtime.GetView().Game.Rebellion, Is.Zero);
            AssertResidentTrustEligibility(session.Runtime, session.Rules);
        }

        private static void AssertResidentTrustEligibility(GameRuntime runtime, GameRules rules)
        {
            // Deserialize and replay a separate read-only snapshot; never edit the running session's DTO.
            string core = JsonUtility.FromJson<ReadOnlyCoreEnvelope>(runtime.Save()).CoreJson;
            var state = SnapshotCodec.Deserialize(core, rules).CreateSnapshot().State;
            var formal = rules.Patients.Where(patient => patient.FirstDay > 0 && patient.FirstDay <= state.Day)
                .Select(patient => patient.Id).ToArray();
            var remaining = state.Patients.Where(patient => patient.Presence == PatientPresence.Active && formal.Contains(patient.Id)).ToArray();
            Assert.That(remaining, Is.Not.Empty);
            Assert.That(remaining.Sum(patient => (long)patient.Trust), Is.GreaterThanOrEqualTo(10));
        }

        [Serializable]
        private sealed class ReadOnlyCoreEnvelope
        {
            public string CoreJson = "";
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator WorldSavePoseOverridesLaterTerminalDoorAndDreamReturnCaches()
        {
            // This case exercises world-pose caches independently of the new-game monitor gate.
            host.RestoreCheckpoint(Fixture().Save());
            yield return null;
            var session = MiniGameSessionHost.Current;
            var original = Pose.Capture(host.Player);
            string saved = Save(session);
            foreach (string mode in new[] { "terminal", "door", "dream" })
            {
                if (mode == "terminal")
                {
                    var computer = Interaction(InteractionTarget.Computer);
                    TeleportTo("Computer approach", computer.Surface.bounds.center);
                    Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                    yield return Until(() => host.Effects.EntryPhase == TerminalEntryPhase.Active && host.Runtime.GetView().Cue == null,
                        "terminal captures position B", 12);
                }
                else if (mode == "door")
                {
                    var door = Interaction(InteractionTarget.Door);
                    TeleportTo("Door approach", door.GetComponent<Collider>().bounds.center);
                    Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
                    yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "door captures position B", 10);
                }
                else
                {
                    var computer = Interaction(InteractionTarget.Computer);
                    TeleportTo("Computer approach", computer.Surface.bounds.center);
                    var fixture = Fixture();
                    DriveTo(fixture, view => EveningClosed(view, 1));
                    host.RestoreCheckpoint(fixture.Save());
                    InteractWithBed();
                    yield return Until(() => host.Dreams.OwnsCamera, "dream captures position B", 8);
                }
                session.LoadFrom(saved);
                Assert.That(session.LastMessage, Does.Contain("已恢复"), mode);
                Assert.That(host.Runtime, Is.SameAs(session.Runtime));
                Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
                original.AssertMatches(host.Player, mode);
                yield return null;
                original.AssertMatches(host.Player, mode + " after adapter update");
            }
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator OutsideDoorSaveReplacesLaterDoorCaptureButRuntimeOnlyRebindPreservesIt()
        {
            host.RestoreCheckpoint(Fixture().Save());
            var session = MiniGameSessionHost.Current;
            var door = Interaction(InteractionTarget.Door);
            var doorCamera = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            var camera = host.Player.ViewCamera;
            TeleportTo("Door approach", door.GetComponent<Collider>().bounds.center);
            yield return null;
            var roomA = Pose.Capture(host.Player);
            var cameraA = camera.transform.position;
            var rotationA = camera.transform.rotation;
            var bodyA = host.Player.transform.position;
            float fovA = camera.fieldOfView;
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "door hold entered from A", 10);
            string savedOutsideA = Save(session);
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "return to A before moving", 10);
            roomA.AssertMatches(host.Player, "initial return to A");

            var controller = host.Player.GetComponent<CharacterController>();
            bool controllerEnabled = controller.enabled;
            controller.enabled = false;
            host.Player.transform.position += camera.transform.right * .25f;
            controller.enabled = controllerEnabled;
            host.Player.FacePoint(door.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            camera.fieldOfView = Mathf.Min(170, fovA + 5);
            var cameraB = camera.transform.position;
            float fovB = camera.fieldOfView;
            Assert.That(Vector3.Distance(cameraA, cameraB), Is.GreaterThan(.2f), "The second entry must use a distinct room pose.");
            Assert.That(doorCamera.TryEnter(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "door hold entered from B", 10);
            Assert.That(Vector3.Distance(doorCamera.ReturnPosition, cameraB), Is.LessThan(.001f));
            Assert.That(doorCamera.ReturnFieldOfView, Is.EqualTo(fovB).Within(.001f));

            host.RestoreCheckpoint(host.Runtime.Save());
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(Vector3.Distance(doorCamera.ReturnPosition, cameraB), Is.LessThan(.001f),
                "Runtime-only rebinding has no replacement base pose and must retain B.");
            Assert.That(doorCamera.ReturnFieldOfView, Is.EqualTo(fovB).Within(.001f));

            session.LoadFrom(savedOutsideA);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            Assert.That(host.Runtime, Is.SameAs(session.Runtime));
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(doorCamera.HasRoomPose, Is.True);
            Assert.That(Vector3.Distance(doorCamera.ReturnPosition, cameraA), Is.LessThan(.001f),
                "An explicit saved base pose must replace the outgoing B capture.");
            Assert.That(Quaternion.Angle(doorCamera.ReturnRotation, rotationA), Is.LessThan(.05f));
            Assert.That(doorCamera.ReturnFieldOfView, Is.EqualTo(fovA).Within(.001f));
            Assert.That(Vector3.Distance(host.Player.transform.position, bodyA), Is.LessThan(.001f));
            Assert.That(Vector3.Distance(camera.transform.position, doorCamera.OutsidePose.position), Is.LessThan(.001f));
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "E returns to saved A after replacing B", 10);
            roomA.AssertMatches(host.Player, "saved outside A return");
            yield return null;
            roomA.AssertMatches(host.Player, "saved outside A return after adapter update");
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator CrossSceneLoadsRestoreRoomAndFloatingEndingWithoutStartingAnotherCampaign()
        {
            StartConfiguredCampaign(false);
            var session = MiniGameSessionHost.Current;
            var sessionRules = session.Rules;
            var roomPose = Pose.Capture(host.Player);
            string roomSave = Save(session);
            dreams.LieDownSeconds = .15f;
            dreams.SubtitleDelay = .1f;
            foreach (var line in dreams.EndingLines) { line.Duration = 1; line.Voice = null; }
            var fixture = EndingCampaignScenario.CreateHidden(compiled, dreams, rebellion);
            AssertResidentTrustEligibility(fixture, sessionRules);
            host.RestoreCheckpoint(fixture.Save());
            yield return null;
            yield return null;
            Assert.That(host.Runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.False);
            var computer = Interaction(InteractionTarget.Computer);
            TeleportTo("Computer approach", computer.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Cue == null && !host.Effects.IsTransitioning
                && host.Runtime.GetView().TerminalPage == TerminalPage.Directory, "cross-scene self-review directory", 12);
            Assert.That(host.Runtime.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(new[] { GameRuntime.SelfPatientId }));
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
            yield return null;
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(host.Runtime.GetView().PresentedEncounterId, Is.EqualTo(GameRuntime.SelfPatientId));
            var authored = host.Runtime.Endings.Get(GameRuntime.SelfPatientId).Lines;
            Assert.That(authored.Length, Is.EqualTo(16));
            for (int index = 0; index < authored.Length; index++)
            {
                yield return null;
                Assert.That(host.Runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(index));
                Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.False);
                string sourceId = authored[index].SourceId;
                for (int page = 0; page < 24 && host.Runtime.CurrentEndingLine?.SourceId == sourceId; page++)
                {
                    yield return null;
                    Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True, sourceId);
                    yield return null;
                }
                Assert.That(host.Runtime.CurrentEndingLine?.SourceId, Is.Not.EqualTo(sourceId));
            }
            Assert.That(host.Runtime.GetView().SelfReviewReady, Is.True);
            var dispositions = host.Screen.GetComponentsInChildren<MiniGameAudioButton>(true)
                .Where(button => button.name.StartsWith("Disposition-", StringComparison.Ordinal)
                    && button.IsActive() && button.IsInteractable()).ToArray();
            Assert.That(dispositions.Select(button => button.name), Is.EqualTo(new[] { "Disposition-Purify" }));
            Assert.That(dispositions.Single().ActivateWithAudio(), Is.True);
            yield return Until(() => SceneManager.GetActiveScene().name == MiniGameSessionHost.BeachScene && !session.IsTransitioning,
                "initial beach scene", 15);
            yield return null;
            var ending = Object.FindObjectOfType<DreamEndingController>();
            Assert.That(ending, Is.Not.Null);
            ending.enabled = false;
            ending.AdvanceForTests(1, 100);
            Assert.That(ending.TryLieDown(), Is.True);
            ending.AdvanceForTests(0, dreams.LieDownSeconds + dreams.SubtitleDelay + .3f);
            var floating = ending.CaptureCheckpoint();
            string caption = ending.CurrentSubtitle;
            Assert.That(floating.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            Assert.That(caption, Is.Not.Empty);
            string endingSave = Save(session);

            session.LoadFrom(roomSave);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            var incomingRoomRuntime = session.Runtime;
            Assert.That(incomingRoomRuntime.GetView().Game.Ending, Is.Not.EqualTo(EndingType.DreamSea));
            Assert.That(session.EndingCheckpoint, Is.Null);
            // The old controller can still receive a frame while its asynchronous scene unload is pending.
            ending.AdvanceForTests(0, 3);
            Assert.That(session.EndingCheckpoint, Is.Null, "An outgoing beach controller must not overwrite the room save's checkpoint.");
            Assert.That(incomingRoomRuntime.SetPaused(true).Accepted, Is.True);
            yield return Until(() => SceneManager.GetActiveScene().name == MiniGameSessionHost.CombinedScene && !session.IsTransitioning,
                "beach save back to room", 15);
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            Assert.That(MiniGameSessionHost.Current, Is.SameAs(session));
            Assert.That(session.Rules, Is.SameAs(sessionRules));
            Assert.That(host.Runtime, Is.SameAs(incomingRoomRuntime), "The rebuilt Combined host must bind the incoming runtime, not start a campaign.");
            Assert.That(session.PendingCombinedRestore, Is.False);
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.GetView().Game.Rebellion, Is.Zero);
            roomPose.AssertMatches(host.Player, "cross-scene room restore");
            Assert.That(session.EndingCheckpoint, Is.Null);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);

            session.LoadFrom(endingSave);
            Assert.That(session.LastMessage, Does.Contain("已恢复"));
            var incomingEndingRuntime = session.Runtime;
            Assert.That(incomingEndingRuntime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(incomingEndingRuntime.SetPaused(true).Accepted, Is.True);
            yield return Until(() => SceneManager.GetActiveScene().name == MiniGameSessionHost.BeachScene && !session.IsTransitioning,
                "room save back to floating beach", 15);
            yield return null;
            ending = Object.FindObjectOfType<DreamEndingController>();
            Assert.That(ending, Is.Not.Null);
            ending.enabled = false;
            Assert.That(MiniGameSessionHost.Current, Is.SameAs(session));
            Assert.That(session.Runtime, Is.SameAs(incomingEndingRuntime));
            Assert.That(session.Runtime.GetView().Game.Day, Is.EqualTo(4));
            Assert.That(session.Runtime.GetView().Game.Rebellion, Is.Zero);
            AssertResidentTrustEligibility(session.Runtime, sessionRules);
            Assert.That(session.PendingCombinedRestore, Is.False);
            Assert.That(ending.Profile, Is.SameAs(dreams));
            var resumed = ending.CaptureCheckpoint();
            Assert.That(resumed.Phase, Is.EqualTo(BeachEndingPhase.Drifting));
            Assert.That(resumed.WalkDistance, Is.EqualTo(floating.WalkDistance).Within(.0001f));
            Assert.That(resumed.DriftElapsed, Is.EqualTo(floating.DriftElapsed).Within(.0001f));
            Assert.That(resumed.SubtitleIndex, Is.EqualTo(floating.SubtitleIndex));
            Assert.That(resumed.SubtitleElapsed, Is.EqualTo(floating.SubtitleElapsed).Within(.0001f));
            Assert.That(ending.CurrentSubtitle, Is.EqualTo(caption));
            Assert.That(Vector3.Dot(ending.ViewCamera.transform.forward, Vector3.up), Is.GreaterThan(.99f));
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator FirstBedBlackScreenPlaysMusicAndPausedRestorePreservesTheRemainingFiveSecondWait()
        {
            audioOverride = Object.Instantiate(host.Audio.Profile);
            audioOverride.FadeSeconds = .05f;
            dreams.Audio = audioOverride;
            host.Audio.Configure(audioOverride);
            var fixture = Fixture();
            DriveTo(fixture, view => EveningClosed(view, 1));
            host.RestoreCheckpoint(fixture.Save());
            InteractWithBed();
            yield return Until(() => host.Dreams.Phase == DreamPlaybackPhase.Aligning, "first-bed black screen", 5);
            var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
            var projection = host.transform.Find("Dream wall projection").GetComponent<Renderer>();
            var music = host.Audio.Service.GetSource(MiniGameAudioChannel.DreamMusic);
            Assert.That(fade.color.a, Is.EqualTo(1));
            Assert.That(projection.enabled, Is.False);
            Assert.That(host.Dreams.PlaybackSeconds, Is.Zero);
            yield return Until(() => music.isPlaying, "music during the black screen", 3);
            Assert.That(music.clip, Is.SameAs(host.Audio.Profile.GetClip(dreams.GetDay(1).Music)));

            host.Dreams.Advance(2.25f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var checkpoint = host.Dreams.CaptureCheckpoint();
            host.Dreams.Advance(20);
            yield return null;
            Assert.That(host.Dreams.CaptureCheckpoint().Elapsed, Is.EqualTo(2.25f));
            Assert.That(fade.color.a, Is.EqualTo(1));
            Assert.That(music.isPlaying, Is.False);
            host.RestoreCheckpoint(host.Runtime.Save());
            host.Dreams.RestoreCheckpoint(checkpoint);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(host.Dreams.CaptureCheckpoint().Elapsed, Is.EqualTo(2.25f));
            Assert.That(host.Dreams.PlaybackSeconds, Is.Zero);
            Assert.That(projection.enabled, Is.False);
            Assert.That(fade.color.a, Is.EqualTo(1));
            host.Dreams.Advance(20);
            yield return null;
            Assert.That(host.Dreams.CaptureCheckpoint().Elapsed, Is.EqualTo(2.25f));
            Assert.That(music.isPlaying, Is.False);

            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(() => music.isPlaying, "restored black-screen music resumes", 3);
            host.Dreams.Advance(2.5f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(fade.color.a, Is.EqualTo(1));
            Assert.That(projection.enabled, Is.False);
            host.Dreams.Advance(.25f);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Playing));
            Assert.That(host.Dreams.PlaybackSeconds, Is.Zero);
            Assert.That(projection.enabled, Is.True);
            Assert.That(fade.color.a, Is.Zero);
            Assert.That(projection.sharedMaterial.GetFloat("_Opacity"), Is.EqualTo(1));
            Assert.That(music.isPlaying, Is.True);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator DreamAudioDisabledSourcesStayStoppedAndReenableSynchronizesActualPlayback()
        {
            audioOverride = Object.Instantiate(host.Audio.Profile);
            audioOverride.FadeSeconds = .05f;
            dreams.Audio = audioOverride;
            host.Audio.Configure(audioOverride);
            var fixture = Fixture();
            DriveTo(fixture, view => EveningClosed(view, 1));
            host.RestoreCheckpoint(fixture.Save());
            InteractWithBed();
            yield return Until(() => host.Runtime.GetView().Cue?.Kind == PresentationKind.Dream, "audio dream cue", 8);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var audio = host.Audio;
            var projector = audio.Service.GetSource(MiniGameAudioChannel.Projector);
            var music = audio.Service.GetSource(MiniGameAudioChannel.DreamMusic);
            Assert.That(audio.Profile.GetClip(MiniGameSound.Projector), Is.Not.Null);
            Assert.That(audio.Profile.GetClip(dreams.GetDay(1).Music), Is.Not.Null);
            audio.enabled = false;
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            if (host.Dreams.Phase == DreamPlaybackPhase.Aligning) host.Dreams.Advance(dreams.BlackScreenSeconds);
            Assert.That(host.Dreams.IsPlaying, Is.True);
            host.Dreams.Advance(.1f);
            yield return null;
            Assert.That(projector.isPlaying, Is.False);
            Assert.That(music.isPlaying, Is.False);
            Assert.That(projector.clip, Is.Null, "A disabled audio director must ignore the visual director's playback request.");
            audio.enabled = true;
            yield return Until(() => projector.isPlaying && music.isPlaying, "audio enable synchronizes active projection", 5);
            Assert.That(projector.clip, Is.SameAs(audio.Profile.GetClip(MiniGameSound.ProjectorOpen)));
            Assert.That(projector.loop, Is.False);
            Assert.That(music.clip, Is.SameAs(audio.Profile.GetClip(dreams.GetDay(1).Music)));
            Assert.That(projector.outputAudioMixerGroup, Is.SameAs(audio.Profile.AmbienceGroup));
            Assert.That(music.outputAudioMixerGroup, Is.SameAs(audio.Profile.MusicGroup));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            float seconds = host.Dreams.PlaybackSeconds;
            yield return null;
            Assert.That(projector.isPlaying, Is.False);
            Assert.That(music.isPlaying, Is.False);
            Assert.That(host.Dreams.PlaybackSeconds, Is.EqualTo(seconds));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(() => projector.isPlaying && music.isPlaying, "paused sources resume", 5);
            audio.enabled = false;
            yield return null;
            Assert.That(projector.isPlaying, Is.False);
            Assert.That(music.isPlaying, Is.False);
            audio.enabled = true;
            yield return Until(() => projector.isPlaying && music.isPlaying, "second audio enable resynchronizes", 5);
            host.Dreams.Advance(dreams.GetDay(1).Duration);
            yield return Until(() => !projector.isPlaying && !music.isPlaying, "dream completion stops projector and music", 5);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator ActualDreamPlaybackBypassesRetroPixelsWithoutChangingTheProfile()
        {
            var retro = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            Assert.That(retro, Is.Not.Null);
            var sourceProfile = retro.Profile;
            Assert.That(sourceProfile, Is.Not.Null);
            string sourceJson = JsonUtility.ToJson(sourceProfile);
            retroOverride = Object.Instantiate(sourceProfile);
            retroOverride.EffectEnabled = true; retroOverride.Strength = 1;
            retroOverride.PixelSize = 4; retroOverride.Levels = 4;
            retroOverride.GammaBefore = retroOverride.GammaAfter = 1;
            retroOverride.FilmGrainStrength = 0; // Compare the same processed pixels across different rendered frames.
            retro.Profile = retroOverride;
            retro.enabled = true; retro.PreviewOriginal = false;
            string settings = JsonUtility.ToJson(retroOverride);
            retro.PresentationBypass = true;
            var originalPixels = RenderRetroProbe(retro);
            retro.PresentationBypass = false;
            var processedPixels = RenderRetroProbe(retro);
            Assert.That(PixelDifference(originalPixels, processedPixels), Is.GreaterThan(.01f), "The test must exercise a visible pixelation/quantization effect.");
            host.Dreams.UseExternalClock = true;
            var fixture = Fixture();
            DriveTo(fixture, view => EveningClosed(view, 1));
            host.RestoreCheckpoint(fixture.Save());
            InteractWithBed();
            yield return Until(() => host.Runtime.GetView().Cue?.Kind == PresentationKind.Dream, "retro bypass dream cue", 8);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(retro.PresentationBypass, Is.False, "Camera alignment retains the ordinary room effect.");
            host.Dreams.Advance(dreams.BlackScreenSeconds);
            host.Dreams.Advance(.3f);
            Assert.That(host.Dreams.IsPlaying, Is.True);
            Assert.That(retro.PresentationBypass, Is.True);
            Assert.That(PixelDifference(RenderRetroProbe(retro), originalPixels), Is.LessThan(.00001f));
            Assert.That(retro.enabled, Is.True);
            Assert.That(retro.PreviewOriginal, Is.False);
            Assert.That(retro.Profile, Is.SameAs(retroOverride));
            Assert.That(JsonUtility.ToJson(retroOverride), Is.EqualTo(settings));
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var checkpoint = host.Dreams.CaptureCheckpoint();
            yield return null;
            Assert.That(retro.PresentationBypass, Is.True, "Pausing the film must not re-enable room pixelation.");
            Assert.That(PixelDifference(RenderRetroProbe(retro), originalPixels), Is.LessThan(.00001f));
            host.RestoreCheckpoint(host.Runtime.Save());
            Assert.That(retro.PresentationBypass, Is.False, "Rebinding first releases the outgoing presentation bypass.");
            host.Dreams.RestoreCheckpoint(checkpoint);
            Assert.That(host.Dreams.IsPlaying, Is.True);
            Assert.That(retro.PresentationBypass, Is.True);
            Assert.That(PixelDifference(RenderRetroProbe(retro), originalPixels), Is.LessThan(.00001f));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            host.Dreams.Advance(dreams.GetDay(1).Duration);
            Assert.That(host.Dreams.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
            Assert.That(retro.PresentationBypass, Is.True, "Dream pixels retain their presentation until fully black.");
            host.Dreams.Advance(dreams.ReturnSeconds);
            Assert.That(retro.PresentationBypass, Is.False);
            Assert.That(PixelDifference(RenderRetroProbe(retro), processedPixels), Is.LessThan(.00001f));
            Assert.That(JsonUtility.ToJson(retroOverride), Is.EqualTo(settings));
            Assert.That(JsonUtility.ToJson(sourceProfile), Is.EqualTo(sourceJson), "The user's saved retro profile must remain unchanged.");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator DreamPreviewStopRebindAndDisableRestoreExistingRetroFlags()
        {
            var retro = host.Player.ViewCamera.GetComponent<RetroScreenEffect>();
            Assert.That(retro, Is.Not.Null);
            var sourceProfile = retro.Profile;
            string settings = JsonUtility.ToJson(sourceProfile);
            retro.enabled = false;
            retro.PreviewOriginal = true;
            host.Dreams.UseExternalClock = true;
            foreach (bool previousBypass in new[] { false, true })
            {
                retro.PresentationBypass = previousBypass;
                Assert.That(host.Dreams.PreviewDay(1), Is.True);
                host.Dreams.Advance(dreams.BlackScreenSeconds);
                Assert.That(retro.PresentationBypass, Is.True);
                host.Dreams.StopPreview();
                Assert.That(retro.PresentationBypass, Is.EqualTo(previousBypass));
                Assert.That(host.Dreams.PreviewDay(2), Is.True);
                host.Dreams.Advance(dreams.BlackScreenSeconds);
                host.Dreams.Bind(host.Runtime, host.Screen, host.Player, host.Audio, dreams);
                Assert.That(retro.PresentationBypass, Is.EqualTo(previousBypass));
                Assert.That(host.Dreams.PreviewDay(3), Is.True);
                host.Dreams.Advance(dreams.BlackScreenSeconds);
                host.Dreams.enabled = false;
                Assert.That(retro.PresentationBypass, Is.EqualTo(previousBypass));
                Assert.That(host.Dreams.OwnsCamera, Is.False);
                Assert.That(retro.enabled, Is.False, "Dream playback must not enable a deliberately disabled post-process component.");
                Assert.That(retro.PreviewOriginal, Is.True, "The user's original-image comparison choice is independent of presentation bypass.");
                Assert.That(retro.Profile, Is.SameAs(sourceProfile));
                Assert.That(JsonUtility.ToJson(sourceProfile), Is.EqualTo(settings));
                host.Dreams.enabled = true;
                yield return null;
            }
        }

        private static Color[] RenderRetroProbe(RetroScreenEffect effect)
        {
            const int width = 32, height = 16;
            var input = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var output = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var destination = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                var pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++) pixels[y * width + x] = new Color((x + .2f) / width, (y + .4f) / height, .31f, .7f);
                input.SetPixels(pixels); input.Apply();
                Graphics.Blit(input, source);
                effect.Render(source, destination);
                RenderTexture.active = destination;
                output.ReadPixels(new Rect(0, 0, width, height), 0, 0); output.Apply();
                return output.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(input); Object.DestroyImmediate(output);
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(destination);
            }
        }

        private static float PixelDifference(Color[] first, Color[] second)
        {
            float difference = 0;
            for (int index = 0; index < first.Length; index++) difference += Vector4.Distance(first[index], second[index]);
            return difference / first.Length;
        }

        [UnityTest, Timeout(90000), Category("InteractionWakeGm")]
        public IEnumerator DayOneShutdownAndDreamWakeAutomaticallyWithPausedWakeCheckpoint()
        {
            var fixture = Fixture();
            DriveTo(fixture, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Evening
                && view.TerminalOpen && !view.LeavingTerminal && view.Cue?.Kind == PresentationKind.ExitTerminal);
            host.Effects.PrepareForRuntimeChange();
            var computer = Interaction(InteractionTarget.Computer);
            TeleportTo("Computer approach", computer.GetComponent<Collider>().bounds.center);
            host.RestoreCheckpoint(fixture.Save());
            host.Dreams.UseExternalClock = false;
            yield return Until(() => !host.Runtime.GetView().TerminalOpen && !host.Screen.TransitionLocked,
                "Day1 shutdown camera return", 12);
            var guidance = host.GetComponent<BedtimeGuidance>();
            var camera = host.Player.ViewCamera;
            Assert.That(Vector3.Dot(camera.transform.forward,
                (guidance.SleepFocusPoint - camera.transform.position).normalized), Is.GreaterThan(.99f),
                "Daily shutdown must return to the illuminated bed, not a black computer-facing wall.");
            var returnedRotation = camera.transform.rotation;
            host.Player.enabled = true;
            yield return null;
            yield return null;
            Assert.That(Quaternion.Angle(camera.transform.rotation, returnedRotation), Is.LessThan(.1f),
                "The first player look update must retain the new return heading.");
            host.Player.enabled = false;
            var view = host.Runtime.GetView();
            Debug.Log("DAY1_SHUTDOWN_STATE " + JsonUtility.ToJson(new ShutdownDiagnostic {
                phase = view.Game.Phase.ToString(), cue = view.Cue?.Kind.ToString(), input = view.Input.ToString(),
                locked = host.Screen.TransitionLocked, camera = host.Player.ViewCamera.transform.position,
                dream = host.Dreams.Phase.ToString(), power = host.Screen.PowerPhase.ToString()
            }));
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "day1-after-shutdown");
            Assert.That(view.Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(view.AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
            Assert.That(fade.color.a, Is.LessThan(.01f));
            var bed = Interaction(InteractionTarget.Bed);
            TeleportTo("Bed approach", bed.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(bed));
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Dreams.IsPlaying, "Day1 dream starts on normal clock", dreams.BlackScreenSeconds + 5);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "day1-natural-dream");
            yield return Until(() => host.Runtime.GetView().Game.Day == 2,
                "Day1 dream finishes on normal clock", dreams.GetDay(1).Duration + dreams.ReturnSeconds + 5);
            Assert.That(host.Screen.TransitionLocked, Is.True);
            Assert.That(host.Effects.CaptureSleepCheckpoint()?.Phase, Is.EqualTo(SleepTransitionPhase.Waking));
            AssertNoDreamBedEffects();
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var checkpoint = host.Effects.CaptureSleepCheckpoint();
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            float opacity = fade.color.a;
            string saved = host.Runtime.Save();
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(host.Effects.CaptureSleepCheckpoint().WakeElapsed, Is.EqualTo(checkpoint.WakeElapsed));
            host.RestoreCheckpoint(saved);
            Assert.That(host.Effects.RestoreSleepCheckpoint(checkpoint), Is.True);
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
            Assert.That(fade.color.a, Is.EqualTo(opacity).Within(.0001f));
            AssertNoDreamBedEffects();
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(() => !host.Effects.SleepCameraActive, "Day2 wake returns camera to its relocated actor", 5);
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.False);
            Assert.That(Vector3.Distance(camera.transform.localPosition, host.Player.StandingEyeLocalPosition), Is.LessThan(.01f));
            host.Player.enabled = true;
            yield return null;
            yield return null;
            Assert.That(Vector3.Distance(camera.transform.localPosition, host.Player.StandingEyeLocalPosition), Is.LessThan(.01f));
        }

        [Serializable] private sealed class ShutdownDiagnostic
        {
            public string phase, cue, input, dream, power;
            public bool locked;
            public Vector3 camera;
        }

        private void AssertNoDreamBedEffects()
        {
            var guidance = host.GetComponent<BedtimeGuidance>();
            Assert.That(guidance.Spotlight.enabled, Is.False);
            Assert.That(guidance.Atmosphere.BeamRenderer.enabled, Is.False);
            Assert.That(guidance.Atmosphere.ParticleRenderer.enabled, Is.False);
        }

        private void StartConfiguredCampaign(bool high)
        {
            rebellion.Choices.Clear();
            if (high)
            {
                var options = compiled.Rules.Days.SelectMany(day => day.Meetings).SelectMany(meeting =>
                    meeting.NormalRounds.Concat(meeting.SpecialRound == null ? Array.Empty<RoundDefinition>() : new[] { meeting.SpecialRound }))
                    .SelectMany(round => round.Responses).Select(option => option.Id).Distinct(StringComparer.Ordinal);
                rebellion.Choices = options.Select(id => new RebellionChoiceDelta { OptionId = id, Delta = 100 }).ToList();
                Assert.That(rebellion.Choices, Is.Not.Empty);
            }
            host.ConfigureDreams(dreams, rebellion);
            host.StartCampaign();
            Assert.That(host.Runtime.UsesDreamFlow, Is.True);
            Assert.That(MiniGameSessionHost.Current.Runtime, Is.SameAs(host.Runtime));
        }

        private GameRuntime Fixture()
        {
            var rules = compiled.Rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            Assert.That(rules.RulesId, Is.EqualTo(MiniGameSessionHost.Current.Rules.RulesId));
            Assert.That(rules.Fingerprint, Is.EqualTo(MiniGameSessionHost.Current.Rules.Fingerprint));
            var runtime = new GameRuntime(rules, compiled.Content, true);
            runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            return runtime;
        }

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView,bool> stop)
        {
            for (int step = 0; step < 2000; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory)
                {
                    var patient = view.Patients.FirstOrDefault(item => item.CanReceiveToday);
                    Assert.That(patient, Is.Not.Null, "The campaign directory must expose a receivable patient.");
                    result = runtime.EnterSelectedPatient(patient.Id);
                }
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        var available = view.Game.AvailableDispositions;
                        var disposition = view.Game.RemainingPurificationQuota > 0 && available.Contains(Disposition.Purify) ? Disposition.Purify
                            : available.Contains(Disposition.Treat) ? Disposition.Treat : available.First();
                        result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition);
                    }
                    else result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                }
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work) result = runtime.TryInteract(InteractionTarget.Computer);
                else { Assert.Fail("No legal fixture step in " + view.Game.Phase + "/" + view.Input); return; }
                Assert.That(result.Accepted, Is.True, "Rejected real campaign command at step " + step + ": " + view.Game.Phase + "/" + view.Input);
            }
            Assert.Fail("Real campaign driver exceeded its 2000-command bound.");
        }

        private static bool EveningClosed(RuntimeView view, int day) => view.Game.Day == day && view.Game.Phase == GamePhase.Evening
            && !view.TerminalOpen && view.Cue == null;

        private WorldInteraction Interaction(InteractionTarget target) => host.gameObject.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true)).Single(item => item.Target == target);

        private Pose InteractWithBed()
        {
            var bed = Interaction(InteractionTarget.Bed);
            TeleportTo("Bed approach", bed.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.SameAs(bed));
            var original = Pose.Capture(host.Player);
            Assert.That(host.Player.InteractFocused(), Is.True);
            return original;
        }

        private void AssertFacingIlluminatedBed()
        {
            var guidance = host.GetComponent<BedtimeGuidance>();
            Assert.That(guidance, Is.Not.Null);
            var camera = host.Player.ViewCamera;
            Assert.That(Vector3.Dot(camera.transform.forward,
                (guidance.SleepFocusPoint - camera.transform.position).normalized), Is.GreaterThan(.99f));
            Assert.That(guidance.Spotlight.enabled, Is.True);
            Assert.That(guidance.Spotlight.intensity, Is.GreaterThan(0));
        }

        private void TeleportTo(string anchorName, Vector3 lookAt)
        {
            var anchor = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(item => item.name == anchorName);
            var controller = host.Player.GetComponent<CharacterController>();
            bool enabled = controller.enabled;
            controller.enabled = false;
            host.Player.transform.position = anchor.position;
            controller.enabled = enabled;
            host.Player.FacePoint(lookAt);
            Physics.SyncTransforms();
        }

        private string Save(MiniGameSessionHost session)
        {
            string path = Path.Combine(Path.GetTempPath(), "minigame-dream-integration-" + Guid.NewGuid().ToString("N") + ".json");
            saveFiles.Add(path);
            Assert.That(Path.GetFullPath(path), Is.Not.EqualTo(Path.GetFullPath(session.SavePath)));
            session.SaveTo(path);
            Assert.That(session.LastMessage, Does.Contain("已保存"));
            Assert.That(File.Exists(path), Is.True);
            return path;
        }

        private static IEnumerator Until(Func<bool> condition, string label, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, label + " timed out.");
        }

        private sealed class Pose
        {
            private Vector3 bodyPosition, cameraPosition;
            private Quaternion bodyRotation, cameraRotation;
            private float fov;
            public static Pose Capture(ApartmentPlayer player) => new Pose
            {
                bodyPosition = player.transform.position, bodyRotation = player.transform.rotation,
                cameraPosition = player.ViewCamera.transform.position, cameraRotation = player.ViewCamera.transform.rotation,
                fov = player.ViewCamera.fieldOfView
            };
            public void AssertMatches(ApartmentPlayer player, string label = "room return")
            {
                AssertPositionAndFieldOfView(player, label);
                Assert.That(Quaternion.Angle(player.transform.rotation, bodyRotation), Is.LessThan(.05f), label + " body rotation");
                Assert.That(Quaternion.Angle(player.ViewCamera.transform.rotation, cameraRotation), Is.LessThan(.05f), label + " camera rotation");
            }
            public void AssertPositionAndFieldOfView(ApartmentPlayer player, string label = "room return")
            {
                Assert.That(Vector3.Distance(player.transform.position, bodyPosition), Is.LessThan(.002f), label + " body position");
                Assert.That(Vector3.Distance(player.ViewCamera.transform.position, cameraPosition), Is.LessThan(.002f), label + " camera position");
                Assert.That(player.ViewCamera.fieldOfView, Is.EqualTo(fov).Within(.05f), label + " camera FOV");
            }
        }
    }
}
