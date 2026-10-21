using System.Collections;
using System.Collections.Generic;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalTypingRevealPlayModeTests
    {
        private GameObject owner;
        private MiniGameScreen screen;
        private MiniGameAudioDirector director;
        private GameRuntime runtime;
        private readonly List<MiniGameSound> typed = new List<MiniGameSound>();

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Terminal glyph sound integration");
            screen = owner.AddComponent<MiniGameScreen>();
            director = owner.AddComponent<MiniGameAudioDirector>();
            var rules = PrototypeRules.Create();
            runtime = new GameRuntime(rules, new PrototypeContent(rules));
            Accept(runtime.TryInteract(InteractionTarget.Door));
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            Accept(runtime.TryInteract(InteractionTarget.Computer));
            screen.Bind(runtime);
            screen.BindAudio(director);
            director.Bind(runtime);
            director.Service.SoundPlayed += Observe;
            typed.Clear();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            director.Service.SoundPlayed -= Observe;
            Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EachNewVisibleGlyphSoundsOnceWhileWhitespaceCursorAndPowerStagesStaySilent()
        {
            string saved = runtime.Save();
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A"));
            Assert.That(typed.Count, Is.EqualTo(1));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A B\nC", true));
            Assert.That(typed.Count, Is.EqualTo(3), "Only the new B and C should play, even when one frame reveals both.");
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A B\nC", false));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A B\nC", true));
            Assert.That(typed.Count, Is.EqualTo(3), "Cursor blinking and repeated rendering must remain silent.");
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot));
            Assert.That(typed.Count, Is.EqualTo(3), "Clearing the console has no letters to play.");
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Shutdown, "C"));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Shutdown, "Cl"));
            Assert.That(typed.Count, Is.EqualTo(5));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Safe, "System halted.\n\n>>>"));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Collapse, "System halted.\n\n>>>", true, collapse: .5f));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Off));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Loading, loaded: 14));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
            Assert.That(typed.Count, Is.EqualTo(5), "Partition loading, moved halt text and CRT collapse do not retype text.");
            Assert.That(runtime.Save(), Is.EqualTo(saved), "Power drawing and sounds must not change game state.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseSkipAndRebindDoNotQueueOrReplayOldLetters()
        {
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A"));
            Accept(runtime.SetPaused(true));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "AB"));
            Assert.That(typed.Count, Is.EqualTo(1));
            Accept(runtime.SetPaused(false));
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "AB"));
            Assert.That(typed.Count, Is.EqualTo(1), "Resuming must not play letters that were already present.");
            screen.SetTerminalPowerFrame(TerminalPowerTimeline.Boot(TerminalPowerTimeline.BootDuration));
            Assert.That(typed.Count, Is.EqualTo(1), "Skipping to partitions must not emit omitted self-test characters.");
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "OLD"));
            Assert.That(typed.Count, Is.EqualTo(4));
            screen.Bind(runtime);
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Boot, "A"));
            Assert.That(typed.Count, Is.EqualTo(5), "A restarted power segment gets fresh presentation progress.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator CompleteBootTimelinePlaysAll322VisibleCharactersExactlyOnce()
        {
            string saved = runtime.Save();
            bool sawClearedConsole = false;
            int frames = Mathf.CeilToInt((TerminalPowerTimeline.BootDuration + TerminalPowerTimeline.LoadingDuration) * 60);
            for (int frameIndex = 0; frameIndex <= frames; frameIndex++)
            {
                float elapsed = frameIndex / 60f;
                var frame = TerminalPowerTimeline.Boot(elapsed);
                screen.SetTerminalPowerFrame(frame);
                int emitted = typed.Count;
                screen.SetTerminalPowerFrame(frame);
                Assert.That(typed.Count, Is.EqualTo(emitted), "Redrawing a power frame must not replay its characters.");
                if (frameIndex == 63)
                    Assert.That(typed.Count, Is.EqualTo(23), "The heading excludes spaces and newlines but includes 3.14 and the hyphen.");
                if (frame.Phase == TerminalPowerPhase.Boot && elapsed >= 5.875f && elapsed < 6.25f)
                {
                    sawClearedConsole = true;
                    Assert.That(frame.ConsoleText, Is.Empty);
                    Assert.That(typed.Count, Is.EqualTo(301), "The complete self-test includes every countdown digit, period and prompt character.");
                }
                director.Service.Stop(MiniGameAudioChannel.TerminalTyping);
            }
            Assert.That(sawClearedConsole, Is.True, "The real timeline must pass through its clear-screen interval.");
            Assert.That(screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
            Assert.That(typed, Has.Count.EqualTo(322), "301 self-test characters plus 21 ready-screen characters must each play once.");
            foreach (var sound in typed)
                Assert.That(new[] { MiniGameSound.TerminalType1, MiniGameSound.TerminalType2,
                    MiniGameSound.TerminalType3, MiniGameSound.TerminalType4 }, Does.Contain(sound));
            Assert.That(runtime.Save(), Is.EqualTo(saved), "The boot timeline and its sounds must not change game state.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ConfiguredTypingPoolContainsFourLoadedMcQueenClips()
        {
            yield return null;
            var configured = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
            Assert.That(configured, Is.SameAs(director.Profile));
            var sounds = new[] { MiniGameSound.TerminalType1, MiniGameSound.TerminalType2,
                MiniGameSound.TerminalType3, MiniGameSound.TerminalType4 };
            var distinct = new HashSet<AudioClip>();
            for (int i = 0; i < sounds.Length; i++)
            {
                var clip = configured.GetClip(sounds[i]);
                Assert.That(clip, Is.Not.Null, sounds[i] + " must have a real imported clip.");
                Assert.That(clip.name, Is.EqualTo("UIType" + (i + 1)));
                Assert.That(clip.samples, Is.GreaterThan(0));
                Assert.That(clip.length, Is.GreaterThan(0));
                Assert.That(clip.frequency, Is.EqualTo(44100));
                Assert.That(clip.channels, Is.EqualTo(1));
                Assert.That(clip.loadState, Is.EqualTo(AudioDataLoadState.Loaded), clip.name + " must be preloaded for per-character playback.");
                Assert.That(distinct.Add(clip), Is.True, "Each typing sound must map to its own imported clip.");
            }
        }

        private void Observe(MiniGameAudioChannel channel, MiniGameSound sound)
        {
            if (channel == MiniGameAudioChannel.TerminalTyping) typed.Add(sound);
        }

        private static void Accept(RuntimeResult result) => Assert.That(result.Accepted, Is.True, result.Reason);
    }
}
