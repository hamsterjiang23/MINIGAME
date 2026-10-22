using System;
using System.IO;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class EndingTransitionPlayModeTests
    {
        [Test]
        public void FullBlackHoldStartsAfterFadeAndWaitsForReadyScene()
        {
            var clock = new RoomEndingTransitionClock();
            clock.Advance(.34f, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.FadingOut));
            clock.Advance(.02f, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            Assert.That(clock.State.Elapsed, Is.Zero, "Fade overshoot must not shorten the full-black interval.");
            Assert.That(clock.Opacity, Is.EqualTo(1));
            clock.Advance(1.99f, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            clock.Advance(.02f, false, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            Assert.That(clock.Opacity, Is.EqualTo(1));
            clock.Advance(0, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.Entering));
            Assert.That(clock.State.Elapsed, Is.Zero);
            Assert.That(clock.Opacity, Is.EqualTo(1));
            clock.Advance(.5f, true, false);
            Assert.That(clock.Opacity, Is.EqualTo(.5f));
            clock.Advance(.5f, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.Exploring));
            Assert.That(clock.Opacity, Is.Zero);
        }

        [TestCase(RoomEndingTransitionPhase.FadingOut, .2f)]
        [TestCase(RoomEndingTransitionPhase.BlackHold, 1.4f)]
        [TestCase(RoomEndingTransitionPhase.Entering, .6f)]
        public void PauseAndSerializationPreserveOnlyRemainingTransition(RoomEndingTransitionPhase phase, float elapsed)
        {
            var saved = new RoomEndingTransitionCheckpoint
            {
                Phase = phase, Elapsed = elapsed, HasEntryPose = true,
                EntryStartPosition = new Vector3(1, 2, 3), EntryTargetPosition = new Vector3(2, 2, 3),
                EntryStartRotation = Quaternion.Euler(8, 12, 0), EntryTargetRotation = Quaternion.Euler(8, 24, 0)
            };
            Assert.That(saved.IsValid(), Is.True);
            var restored = JsonUtility.FromJson<RoomEndingTransitionCheckpoint>(JsonUtility.ToJson(saved));
            var clock = new RoomEndingTransitionClock(restored);
            float opacity = clock.Opacity;
            clock.Advance(20, true, true);
            Assert.That(clock.State.Elapsed, Is.EqualTo(elapsed));
            Assert.That(clock.State.Phase, Is.EqualTo(phase));
            Assert.That(clock.Opacity, Is.EqualTo(opacity));
            Assert.That(clock.State.EntryStartPosition, Is.EqualTo(saved.EntryStartPosition));
            float duration = phase == RoomEndingTransitionPhase.FadingOut ? RoomEndingTransitionClock.FadeOutSeconds
                : phase == RoomEndingTransitionPhase.BlackHold ? RoomEndingTransitionClock.BlackHoldSeconds : RoomEndingTransitionClock.EntrySeconds;
            clock.Advance(duration - elapsed, true, false);
            Assert.That(clock.State.Phase, Is.Not.EqualTo(phase));
        }

        [Test]
        public void FailedInitializationRemainsBlackUntilExplicitRetry()
        {
            var clock = new RoomEndingTransitionClock();
            clock.Fail();
            clock.Advance(100, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.Failed));
            Assert.That(clock.Opacity, Is.EqualTo(1));
            clock.Retry();
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
            Assert.That(clock.State.Elapsed, Is.Zero);
            clock.Advance(1.99f, true, false);
            Assert.That(clock.State.Phase, Is.EqualTo(RoomEndingTransitionPhase.BlackHold));
        }

        [Test]
        public void InvalidTransitionPoseAndElapsedCannotBeRestored()
        {
            Assert.That(new RoomEndingTransitionCheckpoint
                { Phase = RoomEndingTransitionPhase.BlackHold, Elapsed = 2.01f }.IsValid(), Is.False);
            Assert.That(new RoomEndingTransitionCheckpoint
                { Phase = RoomEndingTransitionPhase.Entering, Elapsed = .5f }.IsValid(), Is.False);
            Assert.That(new RoomEndingTransitionCheckpoint
            {
                Phase = RoomEndingTransitionPhase.Entering, HasEntryPose = true,
                EntryStartRotation = new Quaternion(0, 0, 0, 0)
            }.IsValid(), Is.False);
            Assert.That(new EndingThanksCheckpoint
                { Phase = EndingThanksPhase.Visible, Elapsed = float.NaN }.IsValid(), Is.False);
        }

        [Test]
        public void ThanksRequiresFreshClickAndCompletesOnce()
        {
            int completions = 0;
            var view = EndingThanksView.Show(() => completions++);
            view.UseExternalClock = true;
            try
            {
                view.AdvanceForTests(.6f, true, true);
                Assert.That(view.Opacity, Is.EqualTo(.5f));
                view.AdvanceForTests(10, false, false, true);
                Assert.That(view.Opacity, Is.EqualTo(.5f));
                var saved = JsonUtility.FromJson<EndingThanksCheckpoint>(JsonUtility.ToJson(view.CaptureCheckpoint()));
                Assert.That(saved.IsValid(), Is.True);
                view.RestoreCheckpoint(saved);
                view.AdvanceForTests(.6f, true, true);
                Assert.That(view.IsPageVisible, Is.True);
                Assert.That(completions, Is.Zero);
                view.AdvanceForTests(.1f, true, false);
                Assert.That(completions, Is.Zero);
                view.AdvanceForTests(.1f, false, false);
                view.AdvanceForTests(.1f, true, true);
                view.AdvanceForTests(.1f, true, true);
                Assert.That(completions, Is.EqualTo(1));
                Assert.That(view.IsComplete, Is.True);
            }
            finally { Object.DestroyImmediate(view.gameObject); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PausingArmedThanksRequiresReleaseBeforeAnotherClick(bool resumeBeforeThanksUpdate)
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules));
            int completions = 0;
            var view = EndingThanksView.Show(() => completions++,
                new EndingThanksCheckpoint { Phase = EndingThanksPhase.Visible }, runtime);
            view.UseExternalClock = true;
            try
            {
                view.AdvanceForTests(.1f, false, false);
                Assert.That(view.IsPageVisible, Is.True);
                float elapsed = view.CaptureCheckpoint().Elapsed;
                if (resumeBeforeThanksUpdate)
                {
                    // Both notifications can arrive from UI callbacks before the next thanks update.
                    runtime.SetPaused(true);
                    runtime.SetPaused(false);
                }
                else
                {
                    view.AdvanceForTests(10, false, false, true);
                    Assert.That(view.CaptureCheckpoint().Elapsed, Is.EqualTo(elapsed));
                }
                view.AdvanceForTests(.1f, true, true);
                Assert.That(completions, Is.Zero, "The click that resumes the game cannot dismiss thanks.");
                Assert.That(view.IsComplete, Is.False);
                view.AdvanceForTests(.1f, true, false);
                Assert.That(completions, Is.Zero, "Holding the resume click cannot arm dismissal.");
                view.AdvanceForTests(.1f, false, false);
                Assert.That(completions, Is.Zero);
                runtime.SetPaused(false); // An unchanged pause state must not disarm the newly released input.
                view.AdvanceForTests(.1f, true, true);
                view.AdvanceForTests(.1f, true, true);
                Assert.That(completions, Is.EqualTo(1));
                Assert.That(view.IsComplete, Is.True);
            }
            finally { Object.DestroyImmediate(view.gameObject); }
        }

        [Test]
        public void CompletedPeriodDisablesContinueWithoutDeletingSaves()
        {
            string directory = Path.Combine(Path.GetTempPath(), "minigame-ending-completion-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string manual = Path.Combine(directory, "campaign-save.json");
            string automatic = Path.Combine(directory, "campaign-continue.json");
            try
            {
                File.WriteAllText(manual, "manual remains");
                File.WriteAllText(automatic, "automatic remains");
                MainMenuContinueStore.MarkCompleted(directory);
                Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.Null);
                Assert.That(File.ReadAllText(manual), Is.EqualTo("manual remains"));
                Assert.That(File.ReadAllText(automatic), Is.EqualTo("automatic remains"));
                MainMenuContinueStore.StartNewInspection(directory);
                Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.EqualTo(automatic));
                Assert.That(File.ReadAllText(manual), Is.EqualTo("manual remains"));
            }
            finally
            {
                MainMenuContinueStore.StartNewInspection(directory);
                if (File.Exists(manual)) File.Delete(manual);
                if (File.Exists(automatic)) File.Delete(automatic);
                Directory.Delete(directory, false);
            }
        }
    }
}
