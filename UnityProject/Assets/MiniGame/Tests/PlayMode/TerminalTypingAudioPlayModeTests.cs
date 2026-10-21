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
    public sealed class TerminalTypingAudioPlayModeTests
    {
        private static readonly MiniGameSound[] TypingSounds = { MiniGameSound.TerminalType1, MiniGameSound.TerminalType2,
            MiniGameSound.TerminalType3, MiniGameSound.TerminalType4 };
        private readonly List<(MiniGameAudioChannel Channel, MiniGameSound Sound)> played = new List<(MiniGameAudioChannel, MiniGameSound)>();
        private readonly List<AudioClip> clips = new List<AudioClip>();
        private GameObject root;
        private MiniGameAudioProfile profile;
        private MiniGameAudioDirector director;
        private MiniGameAudioService service;
        private GameRuntime runtime;
        private GameRules rules;
        private PrototypeContent content;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var configured = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
            Assert.That(configured, Is.Not.Null);
            profile = Object.Instantiate(configured);
            var entries = new List<MiniGameSoundEntry>();
            foreach (var sound in TypingSounds.Concat(new[] { MiniGameSound.TerminalPowerOn, MiniGameSound.TerminalPowerOff }))
            {
                var clip = AudioClip.Create("Typing regression " + sound, 44100, 1, 44100, false);
                clips.Add(clip);
                entries.Add(new MiniGameSoundEntry { Id = sound, Clip = clip, Volume = .25f });
            }
            profile.Sounds = entries.ToArray();
            root = new GameObject("Terminal typing audio regression");
            director = root.AddComponent<MiniGameAudioDirector>();
            director.Configure(profile);
            rules = PrototypeRules.Create(); content = new PrototypeContent(rules);
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
            Object.Destroy(root); Object.Destroy(profile);
            foreach (var clip in clips) Object.Destroy(clip);
            clips.Clear();
            yield return null;
        }

        private void OnSound(MiniGameAudioChannel channel, MiniGameSound sound) => played.Add((channel, sound));

        [UnityTest]
        public IEnumerator EachCharacterChoosesOneOfFourClipsWithNaturalRepeatsAndIndependentRandomness()
        {
            var globalRandom = UnityEngine.Random.state;
            for (int i = 0; i < 256; i++)
            {
                director.PlayTerminalTyping();
                Assert.That(played.Count, Is.EqualTo(i + 1), "A character must produce exactly one chosen sound.");
                Assert.That(played[i].Channel, Is.EqualTo(MiniGameAudioChannel.TerminalTyping));
                Assert.That(TypingSounds, Has.Member(played[i].Sound));
                service.Stop(MiniGameAudioChannel.TerminalTyping);
            }
            Assert.That(played.Select(item => item.Sound).Distinct().Count(), Is.EqualTo(4), "All four configured clips must be reachable.");
            Assert.That(played.Zip(played.Skip(1), (a, b) => a.Sound == b.Sound).Any(equal => equal), Is.True,
                "Independent choices allow natural immediate repeats.");
            Assert.That(UnityEngine.Random.state, Is.EqualTo(globalRandom), "Typing must not alter gameplay's global random state.");
            Assert.That(service.GetSource(MiniGameAudioChannel.TerminalTyping).outputAudioMixerGroup, Is.EqualTo(profile.UIGroup));
            Assert.That((int)MiniGameSound.TerminalPowerOn, Is.EqualTo(15));
            Assert.That((int)MiniGameSound.TerminalPowerOff, Is.EqualTo(16));
            Assert.That((int)MiniGameAudioChannel.TerminalPower, Is.EqualTo(13));
            yield return null;
        }

        [UnityTest]
        public IEnumerator EmptyPoolsStaySilentAndMissingClipsAreExcluded()
        {
            profile.Sounds = Array.Empty<MiniGameSoundEntry>();
            director.PlayTerminalTyping();
            Assert.That(played, Is.Empty);
            Assert.That(service.GetSource(MiniGameAudioChannel.TerminalTyping).isPlaying, Is.False);
            profile.Sounds = new[] {
                new MiniGameSoundEntry { Id = MiniGameSound.TerminalType1, Clip = null },
                new MiniGameSoundEntry { Id = MiniGameSound.TerminalType3, Clip = clips[2], Volume = .31f }
            };
            for (int i = 0; i < 12; i++)
            {
                director.PlayTerminalTyping();
                service.Stop(MiniGameAudioChannel.TerminalTyping);
            }
            Assert.That(played, Has.Count.EqualTo(12));
            Assert.That(played.All(item => item.Sound == MiniGameSound.TerminalType3), Is.True);
            service.enabled = false;
            director.PlayTerminalTyping();
            Assert.That(played, Has.Count.EqualTo(12), "A disabled service must stay silent.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseRestoreAndDisableStopOldTypingWithoutReplayingIt()
        {
            var source = service.GetSource(MiniGameAudioChannel.TerminalTyping);
            director.PlayTerminalTyping();
            yield return null;
            Assert.That(source.isPlaying, Is.True);
            runtime.SetPaused(true);
            Assert.That(source.isPlaying, Is.False, "Pause must discard the short typing tail.");
            director.PlayTerminalTyping();
            Assert.That(played, Has.Count.EqualTo(1));
            runtime.SetPaused(false);
            yield return null;
            Assert.That(source.isPlaying, Is.False, "Resume must not replay a character already emitted before pause.");
            director.PlayTerminalTyping();
            yield return null;
            Assert.That(source.isPlaying, Is.True);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(runtime);
            Assert.That(source.isPlaying, Is.False);
            Assert.That(played, Has.Count.EqualTo(2));
            director.PlayTerminalTyping();
            yield return null;
            Assert.That(source.isPlaying, Is.True);
            director.enabled = false;
            Assert.That(source.isPlaying, Is.False);
            director.PlayTerminalTyping();
            Assert.That(played, Has.Count.EqualTo(3));
            director.enabled = true;
            yield return null;
            Assert.That(source.isPlaying, Is.False);
            Assert.That(played, Has.Count.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator TypingDoesNotReplaceOrRestartTheSeparatePowerSound()
        {
            var power = service.GetSource(MiniGameAudioChannel.TerminalPower);
            var typing = service.GetSource(MiniGameAudioChannel.TerminalTyping);
            Assert.That(power, Is.Not.SameAs(typing));
            director.PlayTerminalPowerOn();
            director.PlayTerminalTyping();
            yield return null;
            Assert.That(power.isPlaying && typing.isPlaying, Is.True);
            runtime.SetPaused(true);
            Assert.That(power.isPlaying || typing.isPlaying, Is.False);
            runtime.SetPaused(false);
            yield return null;
            Assert.That(power.isPlaying, Is.True, "Power resumes independently while old typing is discarded.");
            Assert.That(typing.isPlaying, Is.False);
            director.PlayTerminalPowerOff();
            director.PlayTerminalTyping();
            Assert.That(played.Count(item => item.Sound == MiniGameSound.TerminalPowerOn), Is.EqualTo(1));
            Assert.That(played.Count(item => item.Sound == MiniGameSound.TerminalPowerOff), Is.EqualTo(1));
            Assert.That(played.Count(item => item.Channel == MiniGameAudioChannel.TerminalTyping), Is.EqualTo(2));
            Assert.That(played.Any(item => item.Sound == MiniGameSound.Scan), Is.False);
        }
    }
}
