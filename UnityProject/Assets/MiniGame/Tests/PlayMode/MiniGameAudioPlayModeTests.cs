using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class MiniGameAudioPlayModeTests
    {
        private GameObject root;
        private MiniGameAudioProfile profile;
        private MiniGameAudioDirector director;
        private MiniGameAudioService service;
        private GameRuntime runtime;
        private GameRules rules;
        private PrototypeContent content;
        private readonly List<MiniGameSound> played = new List<MiniGameSound>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var asset = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
            Assert.That(asset, Is.Not.Null, "Audio setup must import the actual mapped repository clips first.");
            profile = Object.Instantiate(asset);
            profile.FadeSeconds = .08f;
            root = new GameObject("Audio regression host");
            director = root.AddComponent<MiniGameAudioDirector>();
            director.Configure(profile);
            rules = PrototypeRules.Create();
            content = new PrototypeContent(rules);
            runtime = new GameRuntime(rules, content);
            director.Bind(runtime);
            service = director.Service;
            service.SoundPlayed += OnSound;
            played.Clear();
            yield return null;
            played.Clear();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (service != null) service.SoundPlayed -= OnSound;
            Object.Destroy(root);
            Object.Destroy(profile);
            yield return null;
        }

        private void OnSound(MiniGameAudioChannel channel, MiniGameSound sound) => played.Add(sound);

        [UnityTest]
        public IEnumerator SupervisorKnockRepeatsThreeRapidHitsThenPauses()
        {
            var times = new List<float>();
            Action<MiniGameAudioChannel, MiniGameSound> record = (channel, sound) =>
            {
                if (channel == MiniGameAudioChannel.DoorKnock) times.Add(Time.realtimeSinceStartup);
            };
            service.SoundPlayed += record;
            director.Bind(runtime);
            try
            {
                float deadline = Time.realtimeSinceStartup + 5;
                while (times.Count < 7 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(times.Count, Is.GreaterThanOrEqualTo(7));
                for (int index = 1; index < times.Count; index++)
                    Assert.That(times[index] - times[index - 1],
                        Is.EqualTo(index % 3 == 0 ? 1.2f : .14f).Within(.09f));
            }
            finally { service.SoundPlayed -= record; }
        }

        [UnityTest]
        public IEnumerator SupervisorKnocksRepeatBeforeOpeningAndPauseAndRestoreRespectTheCurrentPhase()
        {
            director.Bind(runtime);
            played.Clear();
            var knock = service.GetSource(MiniGameAudioChannel.DoorKnock);
            Assert.That(profile.GetClip(MiniGameSound.DoorKnock).name, Is.EqualTo("DoorKnockOnce"));
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(3));
            Assert.That(knock.outputAudioMixerGroup, Is.EqualTo(profile.SfxGroup));
            runtime.SetPaused(true);
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(3));
            runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(3), "The gap resumes without restarting the triplet.");
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            yield return null;
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(4));
            Accept(runtime.EnterDoorView());
            Assert.That(knock.isPlaying, Is.False, "Opening the door cancels the knock immediately.");
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(4),
                "Loading an open door or supervisor conversation must not resume knocking.");
        }

        [UnityTest]
        public IEnumerator NextMorningStartsKnockingAgainAndDaysWithoutMorningInspectionStaySilent()
        {
            EnterTerminal();
            played.Clear();
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(played, Has.No.Member(MiniGameSound.DoorKnock));
            for (int guard = 0; runtime.GetView().Game.Day < 2 && guard < 700; guard++) Step();
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(runtime.GetView().Cue, Is.Null);
            yield return null;
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(1));
            Accept(runtime.TryInteract(InteractionTarget.Door));
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(played.Count(sound => sound == MiniGameSound.DoorKnock), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SerializedCombinedSceneStartsBackgroundMusicWithoutPlayerInput()
        {
            Object.Destroy(root);
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.MainMenu.TryDismissForEditorDebug();
            host.StartCampaign();
            yield return null;
            host.Player.enabled = false;
            Assert.That(host.Audio, Is.Not.Null);
            Assert.That(host.Audio.SupervisorKnockInterval, Is.EqualTo(.14f).Within(.001f),
                "The serialized scene value must match the requested cadence, not an old Inspector value.");
            Assert.That(host.Audio.Profile.GetEntry(MiniGameSound.DoorKnock).Volume, Is.EqualTo(2));
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.False,
                "Title music belongs only to the main menu.");
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.True);
            yield return null;
            yield return new WaitForSecondsRealtime(host.Audio.Profile.FadeSeconds + .2f);
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.False);
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator OpeningMusicUsesSeparateSourcesAndStopsBeforeRoomResumes()
        {
            director.PlayMenuMusic();
            yield return new WaitForSecondsRealtime(.15f);
            var menu = director.GetComponentsInChildren<MiniGameAudioService>().Single(value => value != service)
                .GetSource(MiniGameAudioChannel.MenuMusic);
            var room = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            Assert.That(menu.clip, Is.EqualTo(profile.GetClip(MiniGameSound.StartMenu)));
            Assert.That(room.isPlaying, Is.False);
            float position = menu.time;
            runtime.SetPaused(true); runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(menu.time, Is.GreaterThan(position));
            director.EndMenuMusic();
            Assert.That(menu.isPlaying, Is.False, "Title music cannot leak into the room.");
            Assert.That(room.isPlaying, Is.True);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(menu.isPlaying, Is.False);
            Assert.That(room.volume, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator EveningKeepsRoomMusicWithoutRestartingThenCrossfadesIntoDream()
        {
            for (int guard = 0; runtime.GetView().Game.Phase != GamePhase.Evening && guard < 500; guard++) Step();
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            var room = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            Assert.That(room.isPlaying, Is.True);
            Assert.That(room.clip, Is.EqualTo(profile.GetClip(MiniGameSound.Gameplay)));
            float position = room.time;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(room.time, Is.GreaterThan(position));
            runtime.SetPaused(true); runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(played.Count(sound => sound == MiniGameSound.Gameplay), Is.Zero);
            director.Bind(GameRuntime.Restore(runtime.Save(), rules, content));
            Assert.That(room.clip, Is.EqualTo(profile.GetClip(MiniGameSound.Gameplay)));
            director.Bind(runtime);
            for (int guard = 0; runtime.GetView().Game.Phase != GamePhase.Dream && guard < 10; guard++) Step();
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(room.isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator AllTwelveClipsAndMixerGroupsArePresentAndVolumesApply()
        {
            Assert.That(profile.Sounds.Length, Is.GreaterThanOrEqualTo(12));
            foreach (MiniGameSound id in Enum.GetValues(typeof(MiniGameSound)))
            {
                if (id == MiniGameSound.EndingMusic || (id == MiniGameSound.Dream3 && profile.GetClip(id) == null)) continue;
                Assert.That(profile.GetClip(id), Is.Not.Null, id.ToString());
                Assert.That(profile.GetClip(id).length, Is.GreaterThan(0), id.ToString());
            }
            Assert.That(profile.Mixer, Is.Not.Null);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).outputAudioMixerGroup, Is.EqualTo(profile.MusicGroup));
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).outputAudioMixerGroup, Is.EqualTo(profile.AmbienceGroup));
            Assert.That(service.GetSource(MiniGameAudioChannel.World).outputAudioMixerGroup, Is.EqualTo(profile.SfxGroup));
            Assert.That(service.GetSource(MiniGameAudioChannel.Dialogue).outputAudioMixerGroup, Is.EqualTo(profile.UIGroup));
            profile.MusicVolume = .25f;
            profile.SfxVolume = 0;
            yield return null;
            Assert.That(profile.Mixer.GetFloat("MusicVolume", out var db), Is.True);
            Assert.That(db, Is.EqualTo(20 * Mathf.Log10(.25f)).Within(.01f));
            Assert.That(profile.Mixer.GetFloat("SfxVolume", out var muted), Is.True);
            Assert.That(muted, Is.EqualTo(-80));
        }

        [UnityTest]
        public IEnumerator RepeatedViewsDoNotRestartMusicAndComputerNoiseLayersOverGameplay()
        {
            var music = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            yield return new WaitForSecondsRealtime(.15f);
            float before = music.time;
            for (int i = 0; i < 5; i++) runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(played.Count(sound => sound == MiniGameSound.Gameplay), Is.Zero);
            Assert.That(music.time, Is.GreaterThan(before));
            EnterTerminal();
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).clip, Is.EqualTo(profile.GetClip(MiniGameSound.PCNoise)));
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.True);
            Assert.That(music.isPlaying, Is.True);
            Assert.That(played.Count(sound => sound == MiniGameSound.PCNoise), Is.EqualTo(1));
            runtime.SetPaused(true);
            float pausedAt = music.time;
            director.PlayWorldInteraction(); director.PlayDialoguePop();
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(music.time, Is.GreaterThan(pausedAt), "Reference-style background audio continues during pause.");
            Assert.That(played, Has.No.Member(MiniGameSound.WorldInteraction));
            Assert.That(played, Has.No.Member(MiniGameSound.DialoguePop));
        }

        [UnityTest]
        public IEnumerator NewLoopRequestCancelsOldFadeOutWithoutRestartingOrStoppingLater()
        {
            var source = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            yield return new WaitForSecondsRealtime(.15f);
            service.Stop(MiniGameAudioChannel.GameplayMusic, .2f);
            yield return new WaitForSecondsRealtime(.08f);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, .08f);
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(source.isPlaying, Is.True);
            Assert.That(source.volume, Is.EqualTo(profile.GetEntry(MiniGameSound.Gameplay).Volume).Within(.01f));
            Assert.That(played.Count(sound => sound == MiniGameSound.Gameplay), Is.Zero, "Cancelling an in-flight fade must retain its playhead.");
        }

        [UnityTest]
        public IEnumerator ScannerOnlyPlaysForPortrait04OncePerDayAcrossPauseAndRestore()
        {
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(played, Has.No.Member(MiniGameSound.Scan));
            Accept(runtime.TryInteract(InteractionTarget.Door));
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(played, Has.No.Member(MiniGameSound.Scan), "Starting the conversation must remain silent.");
            director.PresentSupervisorPortrait(runtime, "JD_01");
            director.PresentSupervisorPortrait(runtime, "JD_03");
            Assert.That(played, Has.No.Member(MiniGameSound.Scan));
            runtime.SetPaused(true);
            director.PresentSupervisorPortrait(runtime, "JD_04");
            Assert.That(played, Has.No.Member(MiniGameSound.Scan));
            runtime.SetPaused(false);
            director.PresentSupervisorPortrait(runtime, "JD_04");
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1));
            director.PresentSupervisorPortrait(runtime, "JD_04");
            director.PresentSupervisorPortrait(runtime, "JD_01");
            director.PresentSupervisorPortrait(runtime, "JD_04");

            runtime.SetPaused(true);
            runtime.SetPaused(false);
            runtime.SetPaused(false);
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1),
                "Pause and repeated view delivery must not retrigger the current scan.");
            var scannedDays = director.CaptureSupervisorScanDays(runtime);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            director.RestoreSupervisorScanDays(scannedDays);
            director.PresentSupervisorPortrait(runtime, "JD_04");
            director.enabled = false;
            director.enabled = true;
            director.PresentSupervisorPortrait(runtime, "JD_04");
            runtime.SetPaused(true); runtime.SetPaused(false);
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1),
                "Loading an in-flight morning scan must not replay its sound.");

            EnterTerminal();
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1),
                "Entering a patient review does not use the morning instrument.");
            Accept(runtime.Inspect(InspectionKind.Eyes));
            Accept(runtime.CloseInspection());
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1),
                "Reading a patient inspection is not a new instrument scan.");
            Step();
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1),
                "Confirming an answer must not use the scan sound.");

            for (int guard = 0; !(runtime.GetView().Game.Day == 2
                && runtime.GetView().Cue?.Kind == PresentationKind.MorningInspection) && guard < 700; guard++) Step();
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1));
            director.PresentSupervisorPortrait(runtime, "JD_04");
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(2),
                "The next day's instrument scan plays once after every review in the previous day stayed silent.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckpointRestoresLoopsWithoutReplayingRecordedLinesScanOrLamp()
        {
            EnterTerminal();
            var line = runtime.GetView().Dialogue;
            Assert.That(line, Is.Not.Null);
            runtime.RecordDialogueLine(line.Speaker, line.Text, line.NodeId, "");
            runtime.RecordDialogueLine(line.Speaker, line.Text, line.NodeId, "");
            Assert.That(played.Count(sound => sound == MiniGameSound.DialoguePop), Is.EqualTo(1));
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.Zero);
            for (int guard = 0; runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition && guard < 50; guard++) Step();
            Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(played.Count(sound => sound == MiniGameSound.LightBeep), Is.EqualTo(1));
            played.Clear();
            var old = runtime;
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            old.SetPaused(false);
            runtime.SetPaused(true); runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.True);
            Assert.That(played, Has.No.Member(MiniGameSound.DialoguePop));
            Assert.That(played, Has.No.Member(MiniGameSound.Scan));
            Assert.That(played, Has.No.Member(MiniGameSound.LightBeep));
        }

        [UnityTest]
        public IEnumerator ProjectorPlaysOpenOnceThenLoopsRunningAndPauseDoesNotAdvanceIntro()
        {
            // Exercise the service independently of the room director's loop synchronization.
            director.enabled = false;
            var open = AudioClip.Create("Short projector open", 22050, 1, 44100, false);
            var running = AudioClip.Create("Short projector running", 8820, 1, 44100, false);
            profile.GetEntry(MiniGameSound.ProjectorOpen).Clip = open;
            profile.GetEntry(MiniGameSound.Projector).Clip = running;
            var source = service.GetSource(MiniGameAudioChannel.Projector);
            try
            {
                service.SetLoopWithIntro(MiniGameAudioChannel.Projector, MiniGameSound.ProjectorOpen, MiniGameSound.Projector, 0);
                Assert.That(source.clip, Is.SameAs(open));
                Assert.That(source.loop, Is.False);
                yield return new WaitForSecondsRealtime(.1f);
                service.Pause(MiniGameAudioChannel.Projector);
                yield return new WaitForSecondsRealtime(.6f);
                Assert.That(source.clip, Is.SameAs(open), "Pause must not start the running loop.");
                service.Resume(MiniGameAudioChannel.Projector);
                for (int i = 0; i < 20 && source.clip != running; i++)
                {
                    service.SetLoopWithIntro(MiniGameAudioChannel.Projector, MiniGameSound.ProjectorOpen, MiniGameSound.Projector, 0);
                    yield return new WaitForSecondsRealtime(.05f);
                }
                Assert.That(source.clip, Is.SameAs(running));
                Assert.That(source.loop, Is.True);
                yield return new WaitForSecondsRealtime(.5f);
                Assert.That(source.isPlaying, Is.True);
                Assert.That(played.Count(sound => sound == MiniGameSound.ProjectorOpen), Is.EqualTo(1));
                Assert.That(played.Count(sound => sound == MiniGameSound.Projector), Is.EqualTo(1));
                service.Stop(MiniGameAudioChannel.Projector);
                service.SetLoopWithIntro(MiniGameAudioChannel.Projector, MiniGameSound.ProjectorOpen, MiniGameSound.Projector, 0);
                service.Stop(MiniGameAudioChannel.Projector);
                yield return new WaitForSecondsRealtime(.6f);
                Assert.That(source.isPlaying, Is.False, "Stopping during open must cancel the pending loop.");
                Assert.That(source.clip, Is.Null);
            }
            finally
            {
                Object.Destroy(open);
                Object.Destroy(running);
            }
        }

        [UnityTest]
        public IEnumerator DreamUsesDayMusicAndProjectorUntilDreamEnds()
        {
            for (int guard = 0; runtime.GetView().Game.Phase != GamePhase.Dream && guard < 500; guard++) Step();
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).clip, Is.EqualTo(profile.GetClip(MiniGameSound.Dream1)));
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).clip, Is.EqualTo(profile.GetClip(MiniGameSound.ProjectorOpen)));
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).loop, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.False);
            director.Bind(GameRuntime.Restore(runtime.Save(), rules, content));
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).clip, Is.EqualTo(profile.GetClip(MiniGameSound.ProjectorOpen)));
            director.Bind(runtime);
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
            for (int guard = 0; runtime.GetView().Game.Day == 1 && guard < 10; guard++) Step();
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator DisabledDirectorCannotBeRevivedByCallbacksOrRuntimeReplacement()
        {
            director.enabled = false;
            director.PlayUIInteraction(); director.PlayWorldInteraction(); director.PlayDialoguePop();
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            EnterTerminal();
            yield return null;
            Assert.That(played, Is.Empty);
            Assert.That(root.GetComponentsInChildren<AudioSource>().Any(source => source.isPlaying), Is.False);
            director.enabled = true;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.True);
            Assert.That(played, Has.No.Member(MiniGameSound.Scan));
        }

        [UnityTest]
        public IEnumerator ClearingAPlayingClipStopsItsSourceAndDestroyUnsubscribesRuntime()
        {
            var clip = profile.GetClip(MiniGameSound.Gameplay);
            profile.GetEntry(MiniGameSound.Gameplay).Clip = null;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            profile.GetEntry(MiniGameSound.Gameplay).Clip = clip;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True,
                "Assigning a previously empty slot must start immediately without a gameplay command.");
            var sources = root.GetComponentsInChildren<AudioSource>();
            Object.Destroy(root);
            yield return null;
            Assert.That(sources.All(source => source == null), Is.True);
            Assert.DoesNotThrow(() => runtime.SetPaused(false));
        }

        private void EnterTerminal()
        {
            for (int guard = 0; !(runtime.GetView().TerminalOpen && runtime.GetView().Cue == null) && guard < 20; guard++) Step();
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
        }

        private void Step()
        {
            var view = runtime.GetView();
            if (view.Cue != null) { Accept(runtime.CompleteCue(view.Cue.Token)); return; }
            if (view.Game.Phase == GamePhase.MorningInspection) { Accept(runtime.TryInteract(InteractionTarget.Door)); return; }
            if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
            { Accept(runtime.TryInteract(InteractionTarget.Bed)); return; }
            if (!view.TerminalOpen) { Accept(runtime.TryInteract(InteractionTarget.Computer)); return; }
            if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Treat));
            else Accept(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "neutral"));
        }

        private static void Accept(RuntimeResult result) => Assert.That(result.Accepted, Is.True, result.Reason);
    }
}
