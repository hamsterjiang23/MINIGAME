using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class MenuAudioSuspensionPlayModeTests
    {
        private GameObject root;
        private MiniGameAudioProfile profile;
        private MiniGameAudioService service;
        private AudioClip clip;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("Menu audio suspension regression");
            if (Object.FindObjectOfType<AudioListener>() == null) root.AddComponent<AudioListener>();
            clip = AudioClip.Create("Menu suspension test tone", 44100 * 4, 1, 44100, false);
            var samples = new float[clip.samples];
            for (int i = 0; i < samples.Length; i++) samples[i] = .01f * Mathf.Sin(i * .05f);
            clip.SetData(samples, 0);
            profile = ScriptableObject.CreateInstance<MiniGameAudioProfile>();
            profile.FadeSeconds = 0;
            profile.Sounds = new[]
            {
                Entry(MiniGameSound.Gameplay), Entry(MiniGameSound.Dream0), Entry(MiniGameSound.OceanWaves),
                Entry(MiniGameSound.WorldInteraction), Entry(MiniGameSound.StartMenu), Entry(MiniGameSound.UIInteraction)
            };
            service = root.AddComponent<MiniGameAudioService>();
            service.Configure(profile);
            yield return null;
        }

        private MiniGameSoundEntry Entry(MiniGameSound sound)
            => new MiniGameSoundEntry { Id = sound, Clip = clip, Volume = .7f };

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root);
            Object.Destroy(profile);
            Object.Destroy(clip);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MenuSuspendsLoopPlayheadAndFadeThenResumesBoth()
        {
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, .8f);
            yield return new WaitForSecondsRealtime(.12f);
            var source = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            Assert.That(source.isPlaying, Is.True);
            service.SetMenuSuspended(true);
            int pausedSample = source.timeSamples;
            float pausedGain = source.volume;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(service.MenuSuspended, Is.True);
            Assert.That(source.isPlaying, Is.False);
            Assert.That(source.timeSamples, Is.EqualTo(pausedSample));
            Assert.That(source.volume, Is.EqualTo(pausedGain).Within(.00001f));
            Assert.That(source.loop, Is.True);

            service.SetMenuSuspended(false);
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(source.isPlaying, Is.True);
            Assert.That(source.timeSamples, Is.GreaterThan(pausedSample));
            Assert.That(source.volume, Is.GreaterThan(pausedGain));
        }

        [UnityTest]
        public IEnumerator LogicalPausesAreIndependentAndStoppedSourcesStayStopped()
        {
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, 0);
            service.SetLoop(MiniGameAudioChannel.DreamMusic, MiniGameSound.Dream0, 0);
            service.SetLoop(MiniGameAudioChannel.Ocean, MiniGameSound.OceanWaves, 0);
            service.SetLoop(MiniGameAudioChannel.Projector, MiniGameSound.Dream0, 0);
            yield return null;
            service.Pause(MiniGameAudioChannel.DreamMusic);
            service.Pause(MiniGameAudioChannel.Projector);
            service.Stop(MiniGameAudioChannel.Ocean);
            service.SetMenuSuspended(true);
            service.Pause(MiniGameAudioChannel.GameplayMusic);
            service.Resume(MiniGameAudioChannel.DreamMusic);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.False,
                "Runtime resume must not physically unpause a source while the menu owns audio.");
            service.SetMenuSuspended(false);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.Ocean).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.Projector).isPlaying, Is.False,
                "A source paused before menu entry must remain paused after menu release.");
            service.Resume(MiniGameAudioChannel.GameplayMusic);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator NewLoopVoiceAndOneShotWaitForMenuRelease()
        {
            service.SetMenuSuspended(true);
            service.SetLoop(MiniGameAudioChannel.DreamMusic, MiniGameSound.Dream0, 0);
            service.PlayVoice(clip, .4f);
            Assert.That(service.PlayOneShot(MiniGameAudioChannel.World, MiniGameSound.WorldInteraction), Is.True);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.Voice).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.World).isPlaying, Is.False);
            service.SetMenuSuspended(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(service.GetSource(MiniGameAudioChannel.DreamMusic).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.Voice).isPlaying, Is.True);
            Assert.That(service.GetSource(MiniGameAudioChannel.Voice).time, Is.GreaterThanOrEqualTo(.4f));
            Assert.That(service.GetSource(MiniGameAudioChannel.World).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator ExistingOneShotAndVoiceSurviveRepeatedMenuToggles()
        {
            service.PlayVoice(clip, .3f);
            service.PlayOneShot(MiniGameAudioChannel.World, MiniGameSound.WorldInteraction);
            yield return new WaitForSecondsRealtime(.1f);
            service.SetMenuSuspended(true);
            service.SetMenuSuspended(true);
            int voiceSample = service.GetSource(MiniGameAudioChannel.Voice).timeSamples;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(service.GetSource(MiniGameAudioChannel.Voice).timeSamples, Is.EqualTo(voiceSample));
            Assert.That(service.GetSource(MiniGameAudioChannel.World).isPlaying, Is.False);
            service.SetMenuSuspended(false);
            service.SetMenuSuspended(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(service.GetSource(MiniGameAudioChannel.Voice).timeSamples, Is.GreaterThan(voiceSample));
            Assert.That(service.GetSource(MiniGameAudioChannel.World).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator ConfigureAndDisableDiscardSuspendedAndDeferredPlayback()
        {
            service.SetMenuSuspended(true);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, 0);
            service.PlayOneShot(MiniGameAudioChannel.World, MiniGameSound.WorldInteraction);
            service.Configure(profile);
            Assert.That(service.MenuSuspended, Is.False);
            service.SetMenuSuspended(false);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            Assert.That(service.GetSource(MiniGameAudioChannel.World).isPlaying, Is.False);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, 0);
            service.SetMenuSuspended(true);
            service.enabled = false;
            Assert.That(service.MenuSuspended, Is.False);
            service.enabled = true;
            service.SetMenuSuspended(false);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.False);
            service.SetLoop(MiniGameAudioChannel.GameplayMusic, MiniGameSound.Gameplay, 0);
            yield return null;
            Assert.That(service.GetSource(MiniGameAudioChannel.GameplayMusic).isPlaying, Is.True);
        }

        [UnityTest]
        public IEnumerator DirectorMenuHelperPreservesWorldAudioAcrossRuntimeNotifications()
        {
            var director = root.AddComponent<MiniGameAudioDirector>();
            director.Configure(profile);
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules));
            director.Bind(runtime);
            yield return new WaitForSecondsRealtime(.1f);
            var worldMusic = service.GetSource(MiniGameAudioChannel.GameplayMusic);
            director.PlayMenuMusic();
            director.PlayMenuMusic();
            int sample = worldMusic.timeSamples;
            runtime.SetPaused(true);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(worldMusic.clip, Is.EqualTo(clip));
            Assert.That(worldMusic.timeSamples, Is.EqualTo(sample));
            Assert.That(service.MenuSuspended, Is.True);
            var menuService = root.transform.Find("Menu Audio").GetComponent<MiniGameAudioService>();
            Assert.That(menuService.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.True);
            director.StopMenuMusic();
            runtime.SetPaused(false);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(worldMusic.isPlaying, Is.True);
            Assert.That(worldMusic.timeSamples, Is.GreaterThan(sample));
            Assert.That(menuService.GetSource(MiniGameAudioChannel.MenuMusic).isPlaying, Is.False);
            director.PlayMenuMusic();
            director.enabled = false;
            Assert.That(service.MenuSuspended, Is.False);
            Assert.That(worldMusic.isPlaying, Is.False);
        }
    }
}
