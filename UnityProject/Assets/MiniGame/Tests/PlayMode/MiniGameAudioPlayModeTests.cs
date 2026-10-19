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
        public IEnumerator SerializedCombinedSceneStartsBackgroundMusicWithoutPlayerInput()
        {
            Object.Destroy(root);
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.Player.enabled = false;
            Assert.That(host.Audio, Is.Not.Null);
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic).clip,
                Is.EqualTo(host.Audio.Profile.GetClip(MiniGameSound.Gameplay)));
            Assert.That(host.Audio.Service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True,
                "Serialized component Awake/OnEnable order must not leave initial music silent.");
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
        public IEnumerator CheckpointRestoresLoopsWithoutReplayingRecordedLinesScanOrLamp()
        {
            EnterTerminal();
            var line = runtime.GetView().Dialogue;
            Assert.That(line, Is.Not.Null);
            runtime.RecordDialogueLine(line.Speaker, line.Text, line.NodeId, "");
            runtime.RecordDialogueLine(line.Speaker, line.Text, line.NodeId, "");
            Assert.That(played.Count(sound => sound == MiniGameSound.DialoguePop), Is.EqualTo(1));
            Assert.That(played.Count(sound => sound == MiniGameSound.Scan), Is.EqualTo(1));
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
        public IEnumerator DreamUsesDayMusicAndProjectorUntilDreamEnds()
        {
            for (int guard = 0; runtime.GetView().Game.Phase != GamePhase.Dream && guard < 500; guard++) Step();
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).clip, Is.EqualTo(profile.GetClip(MiniGameSound.Dream1)));
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).loop, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.ComputerNoise).isPlaying, Is.False);
            director.Bind(GameRuntime.Restore(runtime.Save(), rules, content));
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).clip, Is.EqualTo(profile.GetClip(MiniGameSound.Projector)));
            director.Bind(runtime);
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.False);
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
