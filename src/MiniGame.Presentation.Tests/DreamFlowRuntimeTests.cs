using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class DreamFlowRuntimeTests
    {
        [Test]
        public void FinalBedStartedDreamWaitsForTheSecondSleepBeforeTheOrdinaryEndingPresentation()
        {
            var rules = Rules(0); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Day == 3 && view.Game.Phase == GamePhase.Evening
                && !view.TerminalOpen && view.Cue == null);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            RuntimeScenario.Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(3));
            Assert.That(runtime.GetView().Cue, Is.Null);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Ending));
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().Cue, Is.Null);
        }

        [Test]
        public void DayZeroBedRestoresOneSleepWithoutDreamOrSecondBedInteraction()
        {
            var rules = Rules(0); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Day == 0 && view.Game.Phase == GamePhase.Evening
                && !view.TerminalOpen && view.Cue == null);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Sleep));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            string token = runtime.GetView().Cue!.Token;
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(runtime.GetView().Cue, Is.Null, "Day0 has no dream video.");
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void NightlyBedCycleRequiresSecondSleepAndPreservesPauseCheckpointAndStaleCueGuards(int day)
        {
            var rules = Rules(0); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Day == day && view.Game.Phase == GamePhase.Evening
                && !view.TerminalOpen && view.Cue == null);
            var beforeFirstBed = Core(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(JToken.DeepEquals(Core(runtime), beforeFirstBed), Is.True, "Entering the sleep fade cannot start the dream early.");
            string sleepToken = runtime.GetView().Cue!.Token;
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Sleep));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(sleepToken));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().Paused, Is.True);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(sleepToken));
            string firstSleepCompletion = runtime.GetView().Cue!.Token;
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Dream));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(firstSleepCompletion));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            string dreamToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(dreamToken));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(dreamToken));
            string dreamCompletion = runtime.GetView().Cue!.Token;
            RuntimeScenario.Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day), "Finishing the film must leave the player in the same night's room.");
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            Assert.That(JObject.Parse(runtime.Save())["DreamFromBed"]?.Value<bool>() ?? false, Is.False);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(dreamCompletion));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            string waiting = runtime.Save();
            runtime = GameRuntime.Restore(waiting, rules, content);
            Assert.That(runtime.Save(), Is.EqualTo(waiting));
            RuntimeScenario.Accept(runtime.SetPaused(false));

            var beforeSecondBed = Core(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Sleep));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(JToken.DeepEquals(Core(runtime), beforeSecondBed), Is.True, "The second bed interaction must wait for its sleep fade before waking.");
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            string wakeToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(wakeToken));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().Paused, Is.True);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Sleep));
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(wakeToken));
            string wakeCompletion = runtime.GetView().Cue!.Token;
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day + 1));
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(day == 3 ? GamePhase.Ending : GamePhase.MorningInspection));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(wakeCompletion));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(dreamCompletion));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
        }

        [Test]
        public void NightCannotStartUntilTerminalExitCompletesAndDayZeroSkipsItsVideo()
        {
            var rules = Rules(0);
            var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            Assert.That(runtime.UsesDreamFlow, Is.True);
            Assert.That(runtime.GetView().UsesDreamFlow, Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            DriveTo(runtime, view => view.Game.Phase == GamePhase.Evening);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            Assert.That(runtime.GetView().LeavingTerminal, Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.Accept(runtime.BeginNightDream());
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(runtime.GetView().DreamTraversal, Is.True);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.BeginNightDream);
            var restored = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(restored.Save(), Is.EqualTo(runtime.Save()));
            RuntimeScenario.Accept(restored.TryInteract(InteractionTarget.Bed));
            Assert.That(restored.GetView().Game.Day, Is.Zero);
            RuntimeScenario.Complete(restored, PresentationKind.Sleep);
            Assert.That(restored.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(restored.GetView().DreamTraversal, Is.False);
        }

        [Test]
        public void FormalNightEmitsDreamBeforeBedAndRestoreInvalidatesOldCompletion()
        {
            var rules = Rules(0); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Evening && !view.TerminalOpen && view.Cue == null);
            RuntimeScenario.Accept(runtime.BeginNightDream());
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            Assert.That(runtime.GetView().DreamTraversal, Is.False);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Dream));
            string token = runtime.GetView().Cue!.Token;
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Assert.That(runtime.GetView().Paused, Is.True);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(token));
            RuntimeScenario.Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(runtime.GetView().DreamTraversal, Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(Core(runtime)["CommandHistory"]!.Any(item => item["Kind"]!.Value<string>() == "CompleteSleep"), Is.False);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Sleep));
            RuntimeScenario.Complete(runtime, PresentationKind.Sleep);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
        }

        [Test]
        public void AfterDreamSleepCheckpointCannotReuseTheInitialBedDreamFlag()
        {
            var rules = Rules(0); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.AfterDream);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
            string checkpoint = runtime.Save();
            Assert.That(GameRuntime.Restore(checkpoint, rules, content).Save(), Is.EqualTo(checkpoint));
            var invalid = JObject.Parse(checkpoint);
            invalid["DreamFromBed"] = true;
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(invalid.ToString(), rules, content));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SelfReviewRequiresItsOwnTerminalAndCommitsThePlayerEndingExactlyOnce(bool directory)
        {
            var rules = Rules(100); var content = new FixtureContent();
            var endings = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, endings, directory);
            DriveTo(runtime, view => view.Game.Phase == GamePhase.SelfReview);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(3));
            Assert.That(runtime.GetView().Game.Rebellion, Is.EqualTo(100));
            Assert.That(runtime.GetView().DreamTraversal, Is.True);
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Computer }));
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off), "Self review begins at the dormant terminal.");
            Assert.That(runtime.GetView().TerminalBootRequired, Is.False);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content, endings);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.False, "Self review does not start another daily boot.");
            Assert.That(runtime.GetView().DreamTraversal, Is.False);
            Assert.That(runtime.GetView().Game.CurrentEncounterId, Is.Null);
            Assert.That(runtime.GetView().PresentedPatientId, Is.EqualTo(GameRuntime.SelfPatientId));
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            string enterToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content, endings);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(enterToken));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.TerminalDirectory));
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.LeaveTerminal);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ReturnToDirectory);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient("patient"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition("self", Disposition.Purify));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse("self", "self", "pick"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.Inspect(InspectionKind.Records));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content, endings);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId));
            foreach (var line in endings.Get(GameRuntime.SelfPatientId).Lines)
                RuntimeScenario.Accept(runtime.CompleteEndingLine(line.SourceId));
            var before = Core(runtime);
            var confirmed = RuntimeScenario.Accept(runtime.ConfirmSelfPurification());
            Assert.That(confirmed.Events.Count(item => item.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(4));
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.Ending));
            var after = Core(runtime);
            Assert.That(JToken.DeepEquals(before["State"]!["Patients"], after["State"]!["Patients"]), Is.True);
            Assert.That(JToken.DeepEquals(before["State"]!["Days"], after["State"]!["Days"]), Is.True);
            Assert.That(JToken.DeepEquals(before["State"]!["Encounters"], after["State"]!["Encounters"]), Is.True);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            Assert.That(GameRuntime.Restore(runtime.Save(), rules, content, endings).Save(), Is.EqualTo(runtime.Save()));
        }

        [Test]
        public void CorruptSelfReviewPagesAndLegacyRuleLoadsAreRejected()
        {
            var rules = Rules(100); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            var wrongPhase = JObject.Parse(runtime.Save());
            wrongPhase["TerminalOpen"] = true;
            wrongPhase["Page"] = "SelfReview";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(wrongPhase.ToString(), rules, content));
            DriveTo(runtime, view => view.Game.Phase == GamePhase.SelfReview);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            foreach (string wrongPage in new[] { "SelfReview", "Closed", "Archive" })
            {
                var changed = JObject.Parse(runtime.Save());
                changed["Page"] = wrongPage;
                Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(changed.ToString(), rules, content));
            }
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(runtime.Save(), Rules(100, false), content));
            var legacy = new GameRuntime(Rules(100, false), content, true);
            Assert.That(legacy.GetView().UsesDreamFlow, Is.False);
            Assert.That(legacy.GetView().DreamTraversal, Is.False);
            RuntimeScenario.RejectWithoutMutation(legacy, legacy.BeginNightDream);
            RuntimeScenario.RejectWithoutMutation(legacy, legacy.ConfirmSelfPurification);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RestoringUnpoweredSelfReviewPreservesItsDormantTerminalWithoutBootOrCoreChanges(bool missingPowerFields)
        {
            var rules = Rules(100); var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            DriveTo(runtime, view => view.Game.Phase == GamePhase.SelfReview);
            var saved = JObject.Parse(runtime.Save());
            if (missingPowerFields) { saved.Remove("TerminalPower"); saved.Remove("TerminalPowerDay"); }
            else saved["TerminalPower"] = "Off";
            var core = saved["CoreJson"]!.Value<string>();
            runtime = GameRuntime.Restore(saved.ToString(), rules, content);
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.False);
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>(), Is.EqualTo(core));
            Assert.That(GameRuntime.Restore(runtime.Save(), rules, content).Save(), Is.EqualTo(runtime.Save()));
        }

        private static JObject Core(GameRuntime runtime) => JObject.Parse(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!);

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop)
        {
            for (int step = 0; step < 150; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                if (view.Cue != null) RuntimeScenario.Accept(runtime.CompleteCue(view.Cue.Token));
                else if (view.TerminalPage == TerminalPage.Archive) RuntimeScenario.Accept(runtime.LeaveTerminal());
                else if (view.TerminalPage == TerminalPage.Directory) RuntimeScenario.Accept(runtime.EnterSelectedPatient(view.Game.CurrentPatientId!));
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                        RuntimeScenario.Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!, Disposition.Treat));
                    else RuntimeScenario.Accept(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, "pick"));
                }
                else if (view.Game.Phase == GamePhase.Evening) RuntimeScenario.Accept(runtime.BeginNightDream());
                else if (view.Game.Phase == GamePhase.MorningInspection) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
                else if (view.Game.Phase == GamePhase.AfterDream) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
                else if (view.Game.Phase == GamePhase.Apartment) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
                else Assert.Fail("No dream-flow step available in " + view.Game.Phase + "/" + view.Input);
            }
            Assert.Fail("Dream-flow runtime scenario exceeded its command bound.");
        }

        private static GameRules Rules(int initial, bool enabled = true)
        {
            var patient = new PatientDefinition("patient", "测试病人", 0, 0, null, PatientPresence.TreatmentEnded);
            var formal = new PatientDefinition(PrototypeRules.Child, "正式病人", 1, initial, null, PatientPresence.TreatmentEnded);
            var rules = new GameRules("dream-runtime-fixture", new[] { patient, formal }, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, 0, new[] { new MeetingDefinition(day == 0 ? patient.Id : formal.Id,
                    new[] { new RoundDefinition("day" + day, new[] { new ResponseDefinition("pick", "branch", 1) }) },
                    null, 100, 0, new Dictionary<Disposition, string> { [Disposition.Treat] = "treated" }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            return enabled ? rules.WithDreamFlow(initial, 100, 100, new Dictionary<string, int>()) : rules;
        }

        private sealed class FixtureContent : IGameContent
        {
            public string Revision => "dream-runtime-fixture-content";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId!, "测试病人", "测试对白",
                view.AvailableResponseIds.Select(id => new ResponseContent(id, "回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
