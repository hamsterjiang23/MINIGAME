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
        private int endNoteStarts;

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
            endNoteStarts = 0;
            foreach (var audio in Object.FindObjectsOfType<MiniGameAudioService>(true))
                audio.SoundPlayed += (channel, sound) => { if (sound == MiniGameSound.EndNote) endNoteStarts++; };
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
        public IEnumerator DreamSupervisorInterruptRestoresThirtySixSecondsAndFinalTwentySecondCredits()
        {
            host.Runtime.SetPaused(true);
            Assert.That(host.Runtime.EnterHiddenEndingFromPause().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && host.Runtime.EndingStage == EndingStage.Space, "hidden space", 15);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            var ending = room.Ending;
            ending.UseExternalClock = true;
            for (int step = 0; step < 300 && !ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(ending.TryLieDown(), Is.True);
            ending.AdvanceForTests(0, ending.LieDownDuration);
            ending.AdvanceForTests(0, 6.99f);
            Assert.That(ending.SupervisorDialogueActive, Is.False);
            ending.AdvanceForTests(0, 100);
            Assert.That(ending.SupervisorDialogueActive, Is.True);
            Assert.That(ending.CaptureCheckpoint().PhaseElapsed, Is.EqualTo(7));
            float world = ending.WorldTime;
            var music = ending.Audio.GetSource(MiniGameAudioChannel.EndingMusic);
            var ocean = ending.Audio.GetSource(MiniGameAudioChannel.Ocean);
            Assert.That(music.isPlaying, Is.False);
            Assert.That(ocean.isPlaying, Is.False);
            Assert.That(ending.Audio.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.False);
            for (int line = 0; line < 4; line++)
            {
                Assert.That(ending.SupervisorDialogueIndex, Is.EqualTo(line));
                Assert.That(ending.Audio.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.False,
                    "Sky dialogue has no computer noise, including after pause and restore.");
                ending.AdvanceForTests(0, 100);
                Assert.That(ending.WorldTime, Is.EqualTo(world));
                Assert.That(ending.Audio.GetSource(MiniGameAudioChannel.SupervisorVoice).clip, Is.Not.Null);
                if (line == 1)
                {
                    var save = ending.CaptureCheckpoint();
                    ending.SetPaused(true);
                    Assert.That(ending.HandleInteractionInput(false, true), Is.False);
                    ending.RestoreCheckpoint(save);
                    ending.SetPaused(false);
                    Assert.That(music.isPlaying, Is.False);
                    Assert.That(ocean.isPlaying, Is.False);
                    Assert.That(ending.Audio.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.False);
                }
                yield return null;
                // A click during typing reveals the line; the next click advances it.
                ending.HandleInteractionInput(false, true);
                CaptureDreamSupervisor(room, "dream-supervisor-line-" + line);
                yield return null;
                if (ending.SupervisorDialogueActive && ending.SupervisorDialogueIndex == line)
                    ending.HandleInteractionInput(false, true);
                yield return null;
            }
            yield return EndingVisualCapture.Flush();
            Assert.That(ending.SupervisorDialogueActive, Is.False);
            Assert.That(music.isPlaying, Is.True);
            Assert.That(ocean.isPlaying, Is.True);
            ending.AdvanceForTests(0, 15.99f);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.SkyHold));
            Assert.That(ending.Credits.IsVisible, Is.False, "The first sixteen seconds retain the unobstructed sky.");
            ending.AdvanceForTests(0, .02f);
            Assert.That(ending.Credits.IsVisible, Is.True);
            Assert.That(ending.Credits.Progress, Is.EqualTo(.0005f).Within(.0001f));
            var creditsStart = ending.Credits.ScrollPosition;
            ending.AdvanceForTests(0, 4.99f);
            CaptureEndingCredits(room, "dream-credits-opening");
            ending.AdvanceForTests(0, 5f);
            Assert.That(ending.Credits.Progress, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(ending.Credits.ScrollPosition.y, Is.GreaterThan(creditsStart.y), "Credits must scroll upward.");
            var creditsSave = ending.CaptureCheckpoint();
            var creditsPosition = ending.Credits.ScrollPosition;
            ending.SetPaused(true);
            ending.AdvanceForTests(0, 100);
            Assert.That(ending.Credits.IsVisible, Is.False, "The pause menu must cover the credits.");
            Assert.That(ending.Credits.ScrollPosition, Is.EqualTo(creditsPosition));
            ending.SetPaused(false);
            ending.AdvanceForTests(0, 1);
            ending.RestoreCheckpoint(creditsSave);
            Assert.That(ending.Credits.IsVisible, Is.True);
            Assert.That(ending.Credits.ScrollPosition, Is.EqualTo(creditsPosition), "Loading must restore the scroll, not restart it.");
            var creditLabels = ending.GetComponentsInChildren<TerminalBitmapText>(true)
                .Where(label => label.transform.parent.name == "Credits roll").ToArray();
            CollectionAssert.AreEqual(new[] { "制作人员", "策划", "马步庭", "时晨洋", "美术", "刘静怡", "刘皓喆", "赵耀",
                "程序", "李海江", "黄炜轩", "发行", "胡雅淳", "吴芷娴", "黎念祺", "运营", "陈瑶瑶", "朱睿雯", "时子彧", "黄郑志一" },
                creditLabels.Select(label => label.text).ToArray());
            foreach (var label in creditLabels)
            {
                Assert.That(label.ScaleToFontSize, Is.True);
                Assert.That((Color32)label.color, Is.EqualTo(new Color32(255, 207, 104, 255)));
                foreach (char character in label.text)
                    Assert.That(label.ArkTypeface.TryGet(character, 12, out _), Is.True, "Missing credits glyph: " + character);
            }
            CaptureEndingCredits(room, "dream-credits-middle");
            ending.AdvanceForTests(0, 5f);
            CaptureEndingCredits(room, "dream-credits-closing");
            ending.AdvanceForTests(0, 4.99f);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.SkyHold));
            string finalSave = Path.Combine(temporaryDirectory, "after-supervisor.json");
            var session = MiniGameSessionHost.Current;
            Assert.That(session.TrySaveTo(finalSave, out var error), Is.True, error);
            Assert.That(session.TryLoadFrom(finalSave, out error, true), Is.True, error);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && !session.IsTransitioning, "restored final thirty-six seconds", 15);
            ending = Object.FindObjectOfType<RoomDreamEnding>().Ending;
            ending.UseExternalClock = true;
            Assert.That(ending.CaptureCheckpoint().PhaseElapsed, Is.GreaterThan(42));
            Assert.That(ending.SupervisorDialogueActive, Is.False);
            ending.SetPaused(false);
            ending.AdvanceForTests(0, .02f);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.FadingOut));
            Assert.That(ending.Credits.IsVisible, Is.False);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator PauseTitleShortcutFromDayZeroReachesHiddenBedAndReturnsWithOneEndNote()
        {
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            yield return null;
            var menu = host.Screen.GetComponentsInChildren<PauseMenuView>(true).Single();
            var title = menu.transform.Find("PauseColumn/PauseTitle").GetComponent<UnityEngine.UI.Button>();
            for (int i = 0; i < 3; i++) { title.onClick.Invoke(); yield return null; }
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.None));
            title.onClick.Invoke();
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && host.Runtime.EndingStage == EndingStage.Space, "shortcut hidden space", 15);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            Assert.That(room.IsPreview, Is.False);
            var session = MiniGameSessionHost.Current;
            Assert.That(session.TrySaveTo(Path.Combine(temporaryDirectory, "shortcut.json"), out var error), Is.True, error);
            for (int step = 0; step < 300 && !room.Ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(room.Ending.TryLieDown(), Is.True);
            yield return FinishHiddenBedWithExternalClock(room);
            yield return FinishWithoutThanks("hidden-shortcut");
            Assert.That(endNoteStarts, Is.EqualTo(1));
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator NormalDayFourOpensDoorPlaysSevenSourceLinesAndReturnsWithoutContinue()
        {
            yield return DoorEnding(EndingCampaignRoute.Normal, EndingType.TakenAway, "supervisor-ending", 7);
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator GmReplacesEachFlowAndPreservesManualSaveAndOldCallbackIsolation()
        {
            var session = MiniGameSessionHost.Current;
            string manualPath = Path.Combine(temporaryDirectory, "campaign-dream-save.json");
            Assert.That(session.TrySaveTo(manualPath, out var error), Is.True, error);
            string manual = File.ReadAllText(manualPath);
            int day = host.Runtime.GetView().Game.Day;
            var oldRuntime = host.Runtime;
            string oldCue = oldRuntime.GetView().Cue?.Token;
            Assert.That(session.TryEnterGmEnding(EndingType.TakenAway, out error), Is.False,
                "The public host entrypoint cannot bypass the paused-menu requirement.");
            Assert.That(session.Runtime, Is.SameAs(oldRuntime));
            foreach (var ending in new[] { EndingType.TakenAway, EndingType.LockedInRoom, EndingType.PriestAttack, EndingType.DreamSea })
            {
                host.Runtime.SetPaused(true);
                Assert.That(session.TryEnterGmEnding(ending, out error), Is.True, error);
                Assert.That(host.Runtime, Is.SameAs(session.Runtime));
                Assert.That(host.Runtime.GetView().Paused, Is.False);
                Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(day));
                if (ending == EndingType.DreamSea)
                {
                    Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.SelfReview));
                    Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.None));
                    Assert.That(host.Runtime.GetView().Game.SelfReviewLineIndex, Is.Zero);
                    Assert.That(host.Runtime.GetView().TerminalOpen, Is.False);
                    Assert.That(host.Runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Computer));
                }
                else
                {
                    Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(ending));
                    Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
                    Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
                }
                Assert.That(session.DreamCheckpoint, Is.Null);
                Assert.That(session.SleepCheckpoint, Is.Null);
                Assert.That(session.EndingCheckpoint, Is.Null);
                Assert.That(session.RoomEndingTransition, Is.Null);
                Assert.That(session.ThanksCheckpoint, Is.Null);
                Assert.That(session.SupervisorCheckpoint, Is.Null);
                Assert.That(File.ReadAllText(manualPath), Is.EqualTo(manual));
                yield return null;
            }
            string newFlow = host.Runtime.Save();
            Assert.That(host.Runtime, Is.Not.SameAs(oldRuntime));
            oldRuntime.SetPaused(false);
            if (oldCue != null) oldRuntime.CompleteCue(oldCue);
            yield return null;
            Assert.That(host.Runtime.Save(), Is.EqualTo(newFlow), "A delayed previous-flow callback cannot advance the replacement runtime.");
            Assert.That(endNoteStarts, Is.Zero, "Switching routes must not complete the abandoned ending.");
        }

        [UnityTest, Timeout(60000), Category("InteractionWakeGm")]
        public IEnumerator GmFromFormalBeachRestoresRoomBeforeStartingAnotherDoorEnding()
        {
            var session = MiniGameSessionHost.Current;
            host.Runtime.SetPaused(true);
            Assert.That(host.Runtime.EnterHiddenEndingFromPause().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true,
                "formal beach before GM", 15);
            var beach = Object.FindObjectOfType<RoomDreamEnding>();
            var ending = beach.Ending;
            var endingAudio = ending.Audio;
            Assert.That(host.enabled, Is.False);
            ending.SetPaused(true);
            Assert.That(session.TryEnterGmEnding(EndingType.PriestAttack, out var error), Is.True, error);
            Assert.That(host.enabled, Is.True);
            Assert.That(host.Screen.enabled, Is.True);
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(endingAudio.GetSource(MiniGameAudioChannel.EndingMusic).isPlaying, Is.False);
            Assert.That(session.RoomEndingTransition, Is.Null);
            Assert.That(session.EndingCheckpoint, Is.Null);
            yield return null;
            Assert.That(Object.FindObjectOfType<RoomDreamEnding>(), Is.Null);
            Assert.That(endNoteStarts, Is.Zero);
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
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
            var knocking = host.Audio.Service.GetSource(MiniGameAudioChannel.DoorKnock);
            Assert.That(knocking.spatialBlend, Is.EqualTo(1));
            Assert.That(knocking.isPlaying, Is.True, "Ending visitors must use the same spatial door recording as the supervisor.");
            Interact(InteractionTarget.Door, "Door approach");
            yield return Until(() => host.Runtime.EndingStage == EndingStage.Dialogue && !host.Screen.TransitionLocked,
                "the physical ending door completes its motion", 15);
            Assert.That(knocking.isPlaying, Is.False);
            var expected = host.Runtime.Endings.Get(conversationId).Lines;
            Assert.That(expected.Length, Is.EqualTo(count));
            var actor = host.GetComponent<SupervisorDoorwayView>();
            var body = host.Screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DialogueBody");
            float readyDeadline = Time.realtimeSinceStartup + 3;
            while (!actor.IsReadyForDialogue && Time.realtimeSinceStartup < readyDeadline)
            {
                Assert.That(body.text, Is.Not.EqualTo(expected[0].Text),
                    "The first ending line must wait until the visitor stops sliding back from the door.");
                yield return null;
            }
            Assert.That(actor.IsReadyForDialogue, Is.True);
            yield return Until(() => body.text == expected[0].Text, "the settled visitor's first line", 3);
            var fixedActorPosition = actor.Surface.transform.position;
            for (int index = 0; index < count; index++)
            {
                yield return null;
                Assert.That(Vector3.Distance(actor.Surface.transform.position, fixedActorPosition), Is.LessThan(.001f),
                    "Advancing ending dialogue must keep the visitor stationary.");
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
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Thanks).Or.EqualTo(EndingStage.Completed));
            yield return FinishWithoutThanks(route.ToString().ToLowerInvariant());
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenEndingRequiresSelfReviewThenLiesDownHoldsSkyAndFadesToMenu()
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
            var lastTerminal = host.Screen.TerminalDisplay.Texture;
            var lastCameraPosition = host.Player.ViewCamera.transform.position;
            var lastCameraRotation = host.Player.ViewCamera.transform.rotation;
            Assert.That(((MiniGameAudioButton)dispositions.Single()).ActivateWithAudio(), Is.True);
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(host.Screen.TerminalDisplay.OutputFrozen, Is.True, "Submitting self review must preserve the visible terminal before the fade exists.");
            Assert.That(host.Screen.TerminalDisplay.Texture, Is.Not.SameAs(lastTerminal));
            Assert.That(host.Screen.TerminalDisplay.OutputRect.gameObject.activeInHierarchy, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null, "hidden room owner", 10);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            while (room.TransitionPhase == RoomEndingTransitionPhase.FadingOut)
            {
                Assert.That(host.Screen.TerminalDisplay.OutputFrozen, Is.True);
                int frames = host.Screen.TerminalDisplay.RenderedFrameCount;
                host.Screen.TerminalDisplay.RenderFrame();
                Assert.That(host.Screen.TerminalDisplay.RenderedFrameCount, Is.EqualTo(frames), "The held terminal frame cannot be overwritten by ending-state UI.");
                AssertCameraPose(host.Player.ViewCamera, lastCameraPosition, lastCameraRotation);
                yield return null;
            }
            yield return VerifyRoomEntry(room);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(MiniGameSessionHost.CombinedScene));
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
            yield return EndingVisualCapture.Capture("hidden-space-ready", host.Runtime);
            CapturePortalView(room, "room-after-monitor-turn");
            Assert.That(room.Portal, Is.Not.Null);
            Assert.That(room.Portal.Wall.GetDistanceToPoint(room.Ending.Bed.position), Is.EqualTo(0).Within(.001f),
                "The bed midpoint must meet the actual wall plane.");
            var pillow = room.Ending.Bed.Find("Room Pillow Eye Position");
            Assert.That(room.Portal.Wall.GetDistanceToPoint(pillow.position), Is.GreaterThan(.3f),
                "The pillow and final eye must be beyond the printed wall.");
            Assert.That(room.Portal.PrintedProps.GetComponentsInChildren<Renderer>()
                .Any(renderer => !renderer.name.EndsWith(" - printed head", StringComparison.Ordinal)), Is.False,
                "The authored panorama already contains the desk and monitor; do not duplicate them.");
            var bounds = room.ExplorationBounds;
            Assert.That(bounds.Contains(room.PlayerPosition + Vector3.up * .1f), Is.True);
            // Walk with the room's real collision/clamp path until the existing bed interaction is available.
            for (int step = 0; step < 300 && !room.Ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(room.Ending.CanLieDown, Is.True);
            CapturePortalView(room, "half-bed-at-wall");
            var facingBed = room.Ending.CaptureCheckpoint();
            var bedDirection = room.Ending.Bed.position + Vector3.up * .5f - room.ViewCamera.transform.position;
            var bedAngles = Quaternion.LookRotation(bedDirection).eulerAngles;
            facingBed.RoomYaw = bedAngles.y; facingBed.RoomPitch = Mathf.DeltaAngle(0, bedAngles.x);
            room.RestorePose(facingBed);
            room.Ending.AdvanceForTests(0, 0);
            Assert.That(room.Ending.BedInteractionFocused, Is.True);
            Assert.That(room.Ending.GetComponentsInChildren<Text>(true).Single(text => text.name == "提示").text, Is.Empty);
            yield return null;
            Assert.That(room.Ending.TryLieDown(), Is.True);
            yield return VerifyHiddenBedOnRealClock(room);
            yield return FinishWithoutThanks("hidden");
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
            yield return null;
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
        public IEnumerator CompletedHiddenSaveIsRejectedAndNewPeriodRestoresTheSameRoomWithoutAnotherEndNote()
        {
            yield return ReachHiddenPurification();
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && host.Runtime.EndingStage == EndingStage.Space, "hidden space", 15);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            for (int step = 0; step < 300 && !room.Ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(room.Ending.TryLieDown(), Is.True);
            yield return FinishHiddenBedWithExternalClock(room);
            yield return FinishWithoutThanks("hidden-completed");
            var session = MiniGameSessionHost.Current;
            string path = Path.Combine(temporaryDirectory, "hidden-completed.json");
            Assert.That(session.TrySaveTo(path, out var saveError), Is.True, saveError);
            string saved = File.ReadAllText(path);
            var completedRuntime = session.Runtime;
            Assert.That(session.TryLoadFrom(path, out var loadError), Is.False);
            Assert.That(loadError, Does.Contain("本次检查期已结束"));
            Assert.That(File.ReadAllText(path), Is.EqualTo(saved), "Rejected completion saves stay intact.");
            yield return null;
            yield return null;
            Assert.That(session.Runtime, Is.SameAs(completedRuntime));
            Assert.That(session.Runtime.EndingStage, Is.EqualTo(EndingStage.Completed));
            Assert.That(Object.FindObjectOfType<EndingThanksView>(), Is.Null);
            Assert.That(Object.FindObjectOfType<RoomDreamEnding>(), Is.SameAs(room), "Rejecting a completed save must not rebuild the retained room.");
            Assert.That(endNoteStarts, Is.EqualTo(1));
            int sceneHandle = host.gameObject.scene.handle;
            var previousHost = host;
            var menu = MainMenuController.Current;
            menu.RequestNewInspection();
            Assert.That(menu.View.Page, Is.EqualTo(MainMenuPage.ConfirmNew));
            menu.ConfirmNewInspection();
            yield return Until(() => !menu.IsVisible && !MainMenuController.BlocksGameplayInput
                && !host.Runtime.GetView().Paused, "new inspection after the completed hidden room", 15);
            Assert.That(host, Is.SameAs(previousHost));
            Assert.That(host.gameObject.scene.handle, Is.EqualTo(sceneHandle));
            Assert.That(host.Runtime, Is.Not.SameAs(completedRuntime));
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.GetView().Game.Ending, Is.EqualTo(EndingType.None));
            Assert.That(Object.FindObjectOfType<RoomDreamEnding>(), Is.Null);
            Assert.That(host.Player.ViewCamera.enabled, Is.True);
            Assert.That(host.Screen.isActiveAndEnabled, Is.True);
            Assert.That(endNoteStarts, Is.EqualTo(1), "Starting the next inspection must not replay the ending sting.");
        }
        [UnityTest, Timeout(90000)]
        public IEnumerator HiddenBedPresentationPausesAndRestoresEveryStageWithoutEarlyCompletion()
        {
            yield return ReachHiddenPurification();
            Assert.That(host.Runtime.ConfirmSelfPurification().Accepted, Is.True);
            yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>()?.Ready == true
                && host.Runtime.EndingStage == EndingStage.Space, "hidden space for bed checkpoints", 15);
            var room = Object.FindObjectOfType<RoomDreamEnding>();
            var ending = room.Ending;
            ending.UseExternalClock = true;
            for (int step = 0; step < 300 && !ending.CanLieDown; step++)
                room.AdvanceExploration(Vector2.up, Vector2.zero, .1f);
            Assert.That(ending.TryLieDown(), Is.True);
            ending.AdvanceForTests(0, .45f);

            foreach (var phase in new[] { BeachEndingPhase.Lying, BeachEndingPhase.SkyHold, BeachEndingPhase.FadingOut })
            {
                Assert.That(ending.Phase, Is.EqualTo(phase));
                AssertHiddenBedInputLocked(room);
                ending.SetPaused(true);
                yield return new WaitForSecondsRealtime(.05f); // Let the audio thread finish its current output buffer.
                var saved = ending.CaptureCheckpoint();
                Assert.That(saved.HasMusicPosition, Is.True);
                Assert.That(saved.MusicSeconds, Is.GreaterThan(1), "The checkpoint must contain a progressed musical prelude.");
                var position = ending.ViewCamera.transform.position;
                var rotation = ending.ViewCamera.transform.rotation;
                var bedPosition = ending.Bed.position;
                var bedRotation = ending.Bed.rotation;
                float opacity = ending.EndingFadeOpacity;
                string checkpointJson = JsonUtility.ToJson(saved);
                string path = Path.Combine(temporaryDirectory, "hidden-bed-" + phase + ".json");
                var session = MiniGameSessionHost.Current;
                Assert.That(session.TrySaveTo(path, out var saveError), Is.True, saveError);
                ending.AdvanceForTests(1, 30);
                room.AdvanceExploration(Vector2.one, Vector2.one, 30);
                yield return new WaitForSecondsRealtime(.05f);
                Assert.That(JsonUtility.ToJson(ending.CaptureCheckpoint()), Is.EqualTo(checkpointJson),
                    "Pause must freeze the entire saved bed presentation in " + phase);
                AssertCameraPose(ending.ViewCamera, position, rotation);
                Assert.That(ending.EndingFadeOpacity, Is.EqualTo(opacity).Within(.0001f));
                Assert.That(room.CompleteAtBed(), Is.False);
                Assert.That(endNoteStarts, Is.Zero);

                ending.SetPaused(false);
                ending.AdvanceForTests(0, .15f);
                ending.SetPaused(true);
                var previousRoom = room;
                Assert.That(session.TryLoadFrom(path, out var loadError), Is.True, loadError);
                yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() is RoomDreamEnding restored
                    && restored != previousRoom && restored.Ready, "restored bed presentation " + phase, 15);
                room = Object.FindObjectOfType<RoomDreamEnding>();
                ending = room.Ending;
                ending.UseExternalClock = true;
                Assert.That(session.Runtime.GetView().Paused, Is.True);
                var resumed = ending.CaptureCheckpoint();
                Assert.That(resumed.Phase, Is.EqualTo(phase));
                Assert.That(resumed.PhaseElapsed, Is.EqualTo(saved.PhaseElapsed).Within(.0001f));
                Assert.That(resumed.WorldTime, Is.EqualTo(saved.WorldTime).Within(.0001f));
                Assert.That(resumed.HasMusicPosition, Is.True);
                var restoredMusic = ending.Audio.GetSource(MiniGameAudioChannel.EndingMusic);
                Assert.That(restoredMusic.clip.name, Is.EqualTo("EndingAscent"));
                Assert.That(restoredMusic.isPlaying, Is.False, "A paused load must not resume the music early.");
                // Streaming MP3 seeks may land on an adjacent packet rather than an exact sample.
                Assert.That(restoredMusic.time, Is.EqualTo(saved.MusicSeconds).Within(.25f));
                Assert.That(resumed.MusicSeconds, Is.EqualTo(saved.MusicSeconds).Within(.25f));
                Assert.That(resumed.LieStartPosition, Is.EqualTo(saved.LieStartPosition));
                Assert.That(Quaternion.Angle(resumed.LieStartRotation, saved.LieStartRotation), Is.LessThan(.01f));
                AssertCameraPose(ending.ViewCamera, position, rotation);
                Assert.That(Vector3.Distance(ending.Bed.position, bedPosition), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(ending.Bed.rotation, bedRotation), Is.LessThan(.01f));
                Assert.That(ending.EndingFadeOpacity, Is.EqualTo(opacity).Within(.0001f));
                if (phase != BeachEndingPhase.Lying) AssertExteriorSky(room);
                Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
                Assert.That(endNoteStarts, Is.Zero);
                ending.SetPaused(false);
                AssertHiddenBedInputLocked(room);
                EndingVisualCapture.Request("hidden-bed-restored-" + phase.ToString().ToLowerInvariant(), session.Runtime);
                yield return null;
                float duration = phase == BeachEndingPhase.Lying ? ending.LieDownDuration
                    : phase == BeachEndingPhase.SkyHold ? DreamEndingController.SupervisorInterruptSeconds + DreamEndingController.PostDialogueSeconds
                    : DreamEndingController.EndingFadeSeconds;
                ending.AdvanceForTests(0, duration - ending.CaptureCheckpoint().PhaseElapsed);
                if (ending.SupervisorDialogueActive)
                {
                    yield return ClickThroughDreamSupervisor(ending);
                    ending.AdvanceForTests(0, DreamEndingController.PostDialogueSeconds);
                }
                if (phase != BeachEndingPhase.FadingOut) ending.AdvanceForTests(0, .35f);
            }

            Assert.That(ending.IsBedEndingComplete, Is.True);
            Assert.That(ending.EndingFadeOpacity, Is.EqualTo(1).Within(.0001f));
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space),
                "Completion must wait until the fully black frame has been presented.");
            Assert.That(endNoteStarts, Is.Zero);
            ending.SetPaused(true);
            yield return null;
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space), "Pause blocks the pending completion.");
            Assert.That(endNoteStarts, Is.Zero);
            ending.SetPaused(false);
            yield return FinishWithoutThanks("hidden-bed-restored");
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
                if (Application.isBatchMode)
                {
                    yield return Until(() => Object.FindObjectOfType<RoomDreamEnding>() != null,
                        "restore camera prepared for explicit batch render", 15);
                    Object.FindObjectOfType<MiniGameBootstrap>().Player.ViewCamera.Render();
                }
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
            yield return null;
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

        private IEnumerator VerifyHiddenBedOnRealClock(RoomDreamEnding room)
        {
            var ending = room.Ending;
            Assert.That(ending.UseExternalClock, Is.False);
            Assert.That(ending.LieDownDuration, Is.EqualTo(5f).Within(.0001f));
            Assert.That(ending.LieDownTurnDuration, Is.EqualTo(1f).Within(.0001f));
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Lying));
            var standingPosition = ending.ViewCamera.transform.position;
            var standingRotation = ending.ViewCamera.transform.rotation;
            float lyingStarted = Time.realtimeSinceStartup;
            int movingFrames = 0;
            bool capturedLying = false;
            bool crossedWall = false;
            while (ending.Phase == BeachEndingPhase.Lying)
            {
                AssertHiddenBedInputLocked(room);
                Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
                Assert.That(endNoteStarts, Is.Zero);
                float elapsed = ending.CaptureCheckpoint().PhaseElapsed;
                if (elapsed >= ending.LieDownTurnDuration && elapsed < ending.LieDownDuration * .95f)
                {
                    Assert.That(Vector3.Dot(ending.ViewCamera.transform.forward, room.Portal.WallNormal), Is.LessThan(0),
                        "During the crossing, the camera must recline backward with the pillow wall behind its gaze.");
                }
                if (!crossedWall && room.Portal.IsOutside)
                {
                    crossedWall = true;
                    EndingVisualCapture.Request("hidden-bed-crossed-wall", host.Runtime);
                    CapturePortalView(room, "eye-crossing-wall");
                }
                if (elapsed > .1f && elapsed < ending.LieDownDuration - .1f)
                {
                    if (elapsed < ending.LieDownTurnDuration)
                        Assert.That(Vector3.Distance(ending.ViewCamera.transform.position, standingPosition), Is.LessThan(.0001f),
                            "Turn in place before reclining.");
                    else if (elapsed > ending.LieDownTurnDuration + .3f)
                        Assert.That(Vector3.Distance(ending.ViewCamera.transform.position, standingPosition), Is.GreaterThan(.001f));
                    Assert.That(Quaternion.Angle(ending.ViewCamera.transform.rotation, standingRotation), Is.GreaterThan(.1f));
                    movingFrames++;
                    if (!capturedLying && elapsed >= 1.3f && elapsed <= 2.1f)
                    {
                        EndingVisualCapture.Request("hidden-bed-lying-camera", host.Runtime);
                        CapturePortalView(room, "lying-before-wall");
                        capturedLying = true;
                    }
                }
                yield return null;
            }
            Assert.That(movingFrames, Is.GreaterThan(0), "The player must see the camera move onto the bed.");
            Assert.That(capturedLying, Is.True, "Capture the visible middle of the lie-down motion.");
            Assert.That(crossedWall, Is.True, "The eye must physically pass through the wall during the motion.");
            Assert.That(Time.realtimeSinceStartup - lyingStarted, Is.GreaterThanOrEqualTo(4.85f));
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.SkyHold));
            var skyPosition = ending.ViewCamera.transform.position;
            var skyRotation = ending.ViewCamera.transform.rotation;
            var bedPosition = ending.Bed.position;
            var bedRotation = ending.Bed.rotation;
            Assert.That(Vector3.Dot(ending.ViewCamera.transform.forward, Vector3.up), Is.GreaterThan(.999f));
            float skyStarted = Time.realtimeSinceStartup;
            float minHeave = float.PositiveInfinity, maxHeave = float.NegativeInfinity;
            EndingVisualCapture.Request("hidden-bed-sky-hold", host.Runtime);
            CapturePortalView(room, "outside-sky");
            while (ending.Phase == BeachEndingPhase.SkyHold)
            {
                if (ending.SupervisorDialogueActive) ending.HandleInteractionInput(false, true);
                AssertHiddenBedInputLocked(room);
                minHeave = Mathf.Min(minHeave, ending.OceanFloatPose.x);
                maxHeave = Mathf.Max(maxHeave, ending.OceanFloatPose.x);
                AssertOceanFloatBounds(ending, skyPosition, skyRotation);
                AssertExteriorSky(room);
                Assert.That(Vector3.Distance(ending.Bed.position, bedPosition), Is.LessThan(.001f), "The final bed must not drift.");
                Assert.That(Quaternion.Angle(ending.Bed.rotation, bedRotation), Is.LessThan(.01f));
                Assert.That(ending.EndingFadeOpacity, Is.Zero);
                Assert.That(ending.CurrentSubtitle, Is.Empty);
                Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
                Assert.That(endNoteStarts, Is.Zero);
                yield return null;
            }
            Assert.That(maxHeave - minHeave, Is.GreaterThan(.005f), "The live ocean cursor must produce visible heave during the sky hold.");
            Assert.That(Time.realtimeSinceStartup - skyStarted, Is.GreaterThanOrEqualTo(9.8f));
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.FadingOut));
            float fadeStarted = Time.realtimeSinceStartup;
            float previousOpacity = 0;
            bool capturedFade = false;
            bool capturedFullBlack = false;
            while (host.Runtime.EndingStage == EndingStage.Space)
            {
                Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.FadingOut));
                AssertOceanFloatBounds(ending, skyPosition, skyRotation);
                AssertExteriorSky(room);
                Assert.That(ending.TryLieDown(), Is.False);
                Assert.That(ending.EndingFadeOpacity, Is.GreaterThanOrEqualTo(previousOpacity));
                previousOpacity = ending.EndingFadeOpacity;
                Assert.That(endNoteStarts, Is.Zero);
                if (!ending.IsBedEndingComplete) Assert.That(room.CompleteAtBed(), Is.False);
                if (!capturedFade && previousOpacity >= .35f && previousOpacity < .95f)
                {
                    EndingVisualCapture.Request("hidden-bed-fading-out", host.Runtime);
                    capturedFade = true;
                }
                if (!capturedFullBlack && previousOpacity >= .999f)
                {
                    EndingVisualCapture.Request("hidden-bed-fully-black", host.Runtime);
                    capturedFullBlack = true;
                }
                yield return null;
            }
            Assert.That(capturedFade, Is.True);
            Assert.That(capturedFullBlack, Is.True, "At least one fully black frame must precede the main menu.");
            Assert.That(Time.realtimeSinceStartup - fadeStarted, Is.GreaterThanOrEqualTo(.8f));
            Assert.That(previousOpacity, Is.EqualTo(1).Within(.0001f));
        }

        private IEnumerator FinishHiddenBedWithExternalClock(RoomDreamEnding room)
        {
            var ending = room.Ending;
            ending.UseExternalClock = true;
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.Lying));
            AssertHiddenBedInputLocked(room);
            ending.AdvanceForTests(0, ending.LieDownDuration);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.SkyHold));
            ending.AdvanceForTests(0, DreamEndingController.SupervisorInterruptSeconds);
            yield return ClickThroughDreamSupervisor(ending);
            ending.AdvanceForTests(0, DreamEndingController.PostDialogueSeconds);
            Assert.That(ending.Phase, Is.EqualTo(BeachEndingPhase.FadingOut));
            ending.AdvanceForTests(0, DreamEndingController.EndingFadeSeconds);
            Assert.That(ending.IsBedEndingComplete, Is.True);
            Assert.That(ending.EndingFadeOpacity, Is.EqualTo(1).Within(.0001f));
            Assert.That(host.Runtime.EndingStage, Is.EqualTo(EndingStage.Space));
            Assert.That(endNoteStarts, Is.Zero);
            yield return null;
        }

        // Batch mode has no presented framebuffer. Composite the live overlay after the real room
        // camera's effects, matching ScreenSpaceOverlay rather than tinting the credits with the sky.
        private static void CaptureEndingCredits(RoomDreamEnding room, string name)
        {
            string directory = Environment.GetEnvironmentVariable("MINIGAME_PORTAL_CAPTURE_DIR");
            if (string.IsNullOrWhiteSpace(directory)) return;
            Directory.CreateDirectory(directory);
            var canvas = room.Ending.GetComponentsInChildren<Canvas>().First(c => c.name == "海滩字幕与暂停");
            var mode = canvas.renderMode;
            var originalCamera = canvas.worldCamera;
            float planeDistance = canvas.planeDistance;
            var children = canvas.GetComponentsInChildren<Transform>(true);
            var layers = children.Select(child => child.gameObject.layer).ToArray();
            var camera = room.ViewCamera;
            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;
            float aspect = camera.aspect;
            var target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            var overlayRoot = new GameObject("Credits capture overlay", typeof(Camera));
            try
            {
                target.Create();
                canvas.gameObject.SetActive(false);
                camera.targetTexture = target; camera.aspect = 1920f / 1080;
                camera.Render();
                canvas.gameObject.SetActive(true);
                var overlay = overlayRoot.GetComponent<Camera>();
                overlay.enabled = false; overlay.targetTexture = target;
                overlay.clearFlags = CameraClearFlags.Depth; overlay.cullingMask = 1 << 31;
                overlay.orthographic = true; overlay.orthographicSize = 540;
                overlay.nearClipPlane = .1f; overlay.farClipPlane = 10;
                foreach (var child in children) child.gameObject.layer = 31;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = overlay; canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                overlay.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                canvas.gameObject.SetActive(true);
                canvas.renderMode = mode; canvas.worldCamera = originalCamera; canvas.planeDistance = planeDistance;
                for (int i = 0; i < children.Length; i++) children[i].gameObject.layer = layers[i];
                camera.targetTexture = originalTarget; camera.aspect = aspect;
                RenderTexture.active = originalActive;
                target.Release(); Object.Destroy(target); Object.Destroy(image); Object.Destroy(overlayRoot);
                Canvas.ForceUpdateCanvases();
            }
        }

        // Render the live ending canvas with its camera for opt-in batch preview evidence.
        private static void CaptureDreamSupervisor(RoomDreamEnding room, string name)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MINIGAME_PORTAL_CAPTURE_DIR"))) return;
            var canvas = room.Ending.GetComponentsInChildren<Canvas>().First(c => c.name == "海滩字幕与暂停");
            var mode = canvas.renderMode;
            var camera = canvas.worldCamera;
            float distance = canvas.planeDistance;
            int mask = room.ViewCamera.cullingMask;
            var children = canvas.GetComponentsInChildren<Transform>(true);
            var layers = children.Select(child => child.gameObject.layer).ToArray();
            try
            {
                room.ViewCamera.cullingMask |= 1 << 5;
                foreach (var child in children) child.gameObject.layer = 5;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = room.ViewCamera;
                canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                CapturePortalView(room, name, width: 1920, height: 1080);
            }
            finally
            {
                canvas.renderMode = mode; canvas.worldCamera = camera; canvas.planeDistance = distance;
                room.ViewCamera.cullingMask = mask;
                for (int i = 0; i < children.Length; i++) children[i].gameObject.layer = layers[i];
                Canvas.ForceUpdateCanvases();
            }
        }

        private static IEnumerator ClickThroughDreamSupervisor(DreamEndingController ending)
        {
            for (int click = 0; ending.SupervisorDialogueActive && click < 8; click++)
            {
                yield return null;
                ending.HandleInteractionInput(false, true);
            }
            Assert.That(ending.SupervisorDialogueActive, Is.False);
        }

        private static void AssertHiddenBedInputLocked(RoomDreamEnding room)
        {
            var ending = room.Ending;
            Assert.That(ending.CanLieDown, Is.False);
            Assert.That(ending.TryLieDown(), Is.False, "Repeated bed interaction cannot restart the ending.");
            Assert.That(room.CompleteAtBed(), Is.False, "The bed cannot complete the ending before its fade.");
            var position = ending.ViewCamera.transform.position;
            var rotation = ending.ViewCamera.transform.rotation;
            var playerPosition = room.PlayerPosition;
            room.AdvanceExploration(Vector2.one, Vector2.one, 1);
            Assert.That(room.PlayerPosition, Is.EqualTo(playerPosition));
            AssertCameraPose(ending.ViewCamera, position, rotation);
        }

        private static void AssertCameraPose(Camera camera, Vector3 position, Quaternion rotation)
        {
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.01f));
        }

        private static void AssertExteriorSky(RoomDreamEnding room)
        {
            Vector3 projectionOrigin = room.RoomSurface.Material.GetVector("_ProjectionOrigin");
            Assert.That(Vector3.Distance(projectionOrigin, room.PanoramaOrigin), Is.LessThan(.001f),
                "The wallpaper must remain anchored while the eye leaves the room.");
            Assert.That(room.Portal.IsOutside, Is.True);
            Assert.That(room.RoomSurface.Root.activeSelf, Is.False, "The printed ceiling must not cover the exterior sky.");
            Assert.That(room.ViewCamera.clearFlags, Is.EqualTo(CameraClearFlags.Skybox));
            Assert.That(room.ViewCamera.cullingMask & (1 << DreamWallPortal.ExteriorLayer), Is.Not.Zero);
            Assert.That(room.Portal.Exterior.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(0));
        }

        private IEnumerator VerifyRoomEntry(RoomDreamEnding room)
        {
            yield return Until(() => room.TransitionPhase == RoomEndingTransitionPhase.BlackHold, "black hold begins", 10);
            float blackStarted = Time.realtimeSinceStartup;
            yield return Until(() => room.Ending != null && room.Ending.Audio != null
                && room.Ending.Audio.GetSource(MiniGameAudioChannel.EndingMusic).isPlaying,
                "ending music begins beneath the black curtain", 2);
            Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            var music = room.Ending.Audio.GetSource(MiniGameAudioChannel.EndingMusic);
            var clip = music.clip;
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.name, Is.EqualTo("EndingAscent"));
            int firstMusicSample = music.timeSamples;
            int previousMusicSample = firstMusicSample;
            Assert.That(RoomEndingTransitionClock.BlackHoldSeconds, Is.EqualTo(1));
            float pauseStarted = Time.realtimeSinceStartup;
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            yield return null;
            yield return new WaitForSecondsRealtime(.05f); // AudioSource.Pause is serviced on the audio thread.
            float pausedElapsed = room.CaptureTransitionCheckpoint().Elapsed;
            int pausedSample = music.timeSamples;
            Assert.That(music.isPlaying, Is.False, "The black-screen prelude pauses with the game.");
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            Assert.That(room.CaptureTransitionCheckpoint().Elapsed, Is.EqualTo(pausedElapsed).Within(.0001f));
            Assert.That(music.timeSamples, Is.EqualTo(pausedSample));
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            float pausedSeconds = Time.realtimeSinceStartup - pauseStarted;
            yield return null;
            EndingVisualCapture.Request("hidden-full-black", host.Runtime);
            while (room.TransitionPhase == RoomEndingTransitionPhase.BlackHold)
            {
                Assert.That(room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a, Is.EqualTo(1));
                Assert.That(room.Ready, Is.False);
                AssertContinuousEndingMusic(room, music, clip, ref previousMusicSample);
                yield return null;
            }
            Assert.That(Time.realtimeSinceStartup - blackStarted - pausedSeconds, Is.GreaterThanOrEqualTo(.9f));
            Assert.That((music.timeSamples - firstMusicSample) / (float)clip.frequency, Is.GreaterThanOrEqualTo(.8f),
                "The player must hear the musical prelude while the scene remains black.");
            Assert.That(room.TransitionPhase, Is.EqualTo(RoomEndingTransitionPhase.Entering));
            float entryStarted = Time.realtimeSinceStartup;
            int interpolatedFrames = 0;
            bool entryCaptured = false;
            bool monitorCaptured = false;
            while (room.TransitionPhase == RoomEndingTransitionPhase.Entering)
            {
                AssertContinuousEndingMusic(room, music, clip, ref previousMusicSample);
                var state = room.CaptureTransitionCheckpoint();
                float p = state.Elapsed / RoomEndingTransitionClock.EntrySeconds;
                Assert.That(state.MonitorReturn, Is.True);
                if (!monitorCaptured)
                {
                    CapturePortalView(room, "printed-monitor-start");
                    monitorCaptured = true;
                }
                float t = state.MonitorReturn ? TerminalReturnMotion.Progress(p, false) : Mathf.SmoothStep(0, 1, p);
                float opacity = room.GetComponentsInChildren<Image>(true).Single(image => image.name == "Black").color.a;
                Assert.That(opacity, Is.EqualTo(1 - Mathf.Clamp01(state.Elapsed)).Within(.01f));
                Assert.That(Vector3.Distance(room.ViewCamera.transform.position,
                    state.MonitorReturn ? TerminalReturnMotion.Position(state.EntryStartPosition, state.EntryStartRotation,
                        state.EntryTargetPosition, state.EntryTargetRotation, p, false)
                        : Vector3.Lerp(state.EntryStartPosition, state.EntryTargetPosition, t)), Is.LessThan(.01f));
                Assert.That(Quaternion.Angle(room.ViewCamera.transform.rotation,
                    Quaternion.Slerp(state.EntryStartRotation, state.EntryTargetRotation, t)), Is.LessThan(.1f));
                if (t > .05f && t < .95f) interpolatedFrames++;
                if (!entryCaptured && t >= .35f && t < .95f)
                {
                    EndingVisualCapture.Request("hidden-entry-camera", host.Runtime);
                    CapturePortalView(room, "monitor-return-turn");
                    entryCaptured = true;
                }
                yield return null;
            }
            Assert.That(Time.realtimeSinceStartup - entryStarted, Is.GreaterThanOrEqualTo(3.85f));
            Assert.That(interpolatedFrames, Is.GreaterThan(0));
            yield return Until(() => room.Ready && host.Runtime.EndingStage == EndingStage.Space, "room exploration ready", 5);
            AssertContinuousEndingMusic(room, music, clip, ref previousMusicSample);
            Assert.That(music.volume, Is.EqualTo(.7f).Within(.01f));
            Assert.That(room.Ending.Audio.GetSource(MiniGameAudioChannel.Ocean).volume, Is.EqualTo(1).Within(.01f));
        }

        // An explicit GPU render of the actual camera, excluding ScreenSpaceOverlay UI.
        internal static void CapturePortalView(RoomDreamEnding room, string name, string outputDirectory = null, int width = 1280, int height = 0)
        {
            string directory = outputDirectory ?? Environment.GetEnvironmentVariable("MINIGAME_PORTAL_CAPTURE_DIR");
            if (string.IsNullOrWhiteSpace(directory)) return;
            Directory.CreateDirectory(directory);
            var camera = room.ViewCamera;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            float aspect = camera.aspect;
            var target = new RenderTexture(width, height > 0 ? height : Mathf.RoundToInt(width / aspect), 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                target.Create(); camera.targetTexture = target;
                camera.aspect = target.width / (float)target.height;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
                File.AppendAllText(Path.Combine(directory, "camera-poses.txt"), name + " position="
                    + camera.transform.position.ToString("F4") + " forward=" + camera.transform.forward.ToString("F4")
                    + " up=" + camera.transform.up.ToString("F4") + " roomOpacity=" + room.RoomSurface.Opacity.ToString("F4")
                    + " wallDistance=" + room.Portal.Wall.GetDistanceToPoint(camera.transform.position).ToString("F4")
                    + " wallpaperOrigin=" + room.PanoramaOrigin.ToString("F4") + Environment.NewLine);
            }
            finally
            {
                camera.targetTexture = previousTarget; camera.aspect = aspect; RenderTexture.active = previousActive;
                target.Release(); Object.Destroy(target); Object.Destroy(image);
            }
        }

        private static void AssertOceanFloatBounds(DreamEndingController ending, Vector3 basePosition, Quaternion baseRotation)
        {
            var displacement = ending.ViewCamera.transform.position - basePosition;
            Assert.That(new Vector2(displacement.x, displacement.z).magnitude, Is.LessThan(.001f));
            Assert.That(Mathf.Abs(displacement.y), Is.LessThanOrEqualTo(ending.Profile.OceanFloatHeight + .001f));
            Assert.That(Quaternion.Angle(ending.ViewCamera.transform.rotation, baseRotation),
                Is.LessThanOrEqualTo(ending.Profile.OceanFloatPitch + ending.Profile.OceanFloatRoll + .02f));
        }

        private static void AssertContinuousEndingMusic(RoomDreamEnding room, AudioSource music, AudioClip clip,
            ref int previousSample)
        {
            Assert.That(room.Ending.Audio.GetSource(MiniGameAudioChannel.EndingMusic), Is.SameAs(music));
            Assert.That(music.clip, Is.SameAs(clip));
            Assert.That(music.isPlaying, Is.True, "Entering the beach must not stop or replace the music.");
            Assert.That(music.timeSamples, Is.GreaterThanOrEqualTo(previousSample), "The ending track must not restart.");
            previousSample = music.timeSamples;
        }

        private IEnumerator FinishWithoutThanks(string route)
        {
            yield return Until(() => MainMenuController.Current.IsVisible
                && host.Runtime.EndingStage == EndingStage.Completed, "immediate menu", 2);
            Assert.That(Object.FindObjectOfType<EndingThanksView>(), Is.Null);
            Assert.That(endNoteStarts, Is.EqualTo(1), "Each ending must emit exactly one EndNote.");
            var menuAudio = MainMenuController.Current.GetComponentsInChildren<MiniGameAudioService>(true).Single();
            Assert.That(menuAudio.GetSource(MiniGameAudioChannel.EndNote).isPlaying, Is.True,
                "EndNote must survive the immediate menu cut.");
            Assert.That(MainMenuController.Current.View.Page, Is.EqualTo(MainMenuPage.Menu));
            Assert.That(MainMenuController.Current.View.ContinueButton.interactable, Is.False);
            Assert.That(MainMenuController.Current.HasRetainedGame, Is.False);
            Assert.That(MainMenuContinueStore.HasCompletedInspection(temporaryDirectory), Is.True);
            MainMenuController.Current.CompleteInspectionAndReturnToMenu(true);
            Assert.That(endNoteStarts, Is.EqualTo(1), "Repeated completion cannot replay the note.");
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
