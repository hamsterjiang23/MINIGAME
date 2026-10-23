using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class GmEndingRuntimeTests
    {
        [Test]
        public void GmRequiresDreamFlowEvenWhenPaused()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterGmEndingFromPause(EndingType.TakenAway));
        }

        [TestCase(EndingType.TakenAway)]
        [TestCase(EndingType.LockedInRoom)]
        [TestCase(EndingType.PriestAttack)]
        [TestCase(EndingType.DreamSea)]
        public void PausedGmEntryClearsAbandonedPresentationAndPreservesHistory(EndingType ending)
        {
            var rules = EndingRuntimeFixture.Rules();
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog);
            EndingRuntimeFixture.DriveTo(runtime, view => view.Game.Phase == GamePhase.Work && view.Cue != null);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterGmEndingFromPause(ending));
            string token = runtime.GetView().Cue!.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            var before = EndingRuntimeFixture.Core(runtime);
            RuntimeScenario.Accept(runtime.EnterGmEndingFromPause(ending));
            var view = runtime.GetView();
            Assert.That(view.Paused, Is.False);
            Assert.That(view.TerminalOpen, Is.False);
            Assert.That(view.TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(view.Cue, Is.Null);
            Assert.That(view.Inspection, Is.Null);
            Assert.That(view.DoorViewActive, Is.False);
            Assert.That(view.RoomDoorOpen, Is.False);
            Assert.That(view.Game.CurrentEncounterId, Is.Null);
            Assert.That(view.Game.SelfReviewLineIndex, Is.Zero);
            Assert.That(runtime.EndingLineIndex, Is.Zero);
            Assert.That(runtime.EndingStage, Is.EqualTo(ending == EndingType.DreamSea ? EndingStage.None : EndingStage.AwaitingDoor));
            Assert.That(view.Game.Ending, Is.EqualTo(ending == EndingType.DreamSea ? EndingType.None : ending));
            Assert.That(view.Game.Phase, Is.EqualTo(ending == EndingType.DreamSea ? GamePhase.SelfReview : GamePhase.Ending));
            Assert.That(view.AllowedInteractions, Is.EqualTo(new[] { ending == EndingType.DreamSea ? InteractionTarget.Computer : InteractionTarget.Door }));
            var after = EndingRuntimeFixture.Core(runtime);
            foreach (string key in new[] { "Day", "Patients", "Days", "Encounters", "PendingEvents", "Rebellion" })
                Assert.That(JToken.DeepEquals(before["State"]![key], after["State"]![key]), Is.True, key);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            string saved = runtime.Save();
            Assert.That(GameRuntime.Restore(saved, rules, content, catalog).Save(), Is.EqualTo(saved));
            if (ending == EndingType.DreamSea)
            {
                RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
                RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
                RuntimeScenario.Accept(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId));
                RuntimeScenario.Accept(runtime.CompleteEndingLine(runtime.CurrentEndingLine!.SourceId));
                Assert.That(runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(1));
                RuntimeScenario.Accept(runtime.SetPaused(true));
                RuntimeScenario.Accept(runtime.EnterGmEndingFromPause(EndingType.DreamSea));
                Assert.That(runtime.GetView().Game.SelfReviewLineIndex, Is.Zero);
                Assert.That(runtime.GetView().Transcript, Is.Empty);
            }
        }

        [Test]
        public void GmCanReplaceCompletedHiddenEndingButLegacyShortcutAndInvalidRequestsStayGuarded()
        {
            var rules = EndingRuntimeFixture.Rules();
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog);
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterGmEndingFromPause(EndingType.None));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterGmEndingFromPause((EndingType)999));
            RuntimeScenario.Accept(runtime.EnterHiddenEndingFromPause());
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.Transition));
            RuntimeScenario.Accept(runtime.MarkEndingSpaceReady());
            RuntimeScenario.Accept(runtime.CompleteHiddenBed());
            RuntimeScenario.Accept(runtime.MarkEndingCompleted());
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.EnterHiddenEndingFromPause);
            RuntimeScenario.Accept(runtime.EnterGmEndingFromPause(EndingType.PriestAttack));
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(EndingRuntimeFixture.Core(runtime)["CommandHistory"]!.Last()!["Kind"]!.Value<string>(), Is.EqualTo("EnterGmEnding"));
        }
    }
}
