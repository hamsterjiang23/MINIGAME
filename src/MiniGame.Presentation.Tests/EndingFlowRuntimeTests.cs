using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class EndingFlowRuntimeTests
    {
        [TestCase(-1, -1, EndingType.TakenAway, 4, "supervisor-ending", 7)]
        [TestCase(1, -1, EndingType.LockedInRoom, 4, "supervisor-ending", 7)]
        [TestCase(2, 2, EndingType.PriestAttack, 3, "priest-ending", 8)]
        [TestCase(3, 3, EndingType.PriestAttack, 4, "priest-ending", 8)]
        public void DawnEndingRequiresDoorAndEveryAuthoredLineBeforeThanks(int quotaDay, int releaseDay,
            EndingType ending, int day, string conversation, int lineCount)
        {
            var rules = EndingRuntimeFixture.Rules(quotaDay: quotaDay);
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog);
            EndingRuntimeFixture.DriveTo(runtime, view => view.Game.Phase == GamePhase.Ending, releaseDay);
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(ending));
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(day));
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.AwaitingDoor));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(runtime.CurrentEndingLine, Is.Null);
            Assert.That(runtime.CurrentEndingConversation!.Id, Is.EqualTo(conversation));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(catalog.Get(conversation).Lines[0].SourceId));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.CompleteHiddenBed);
            runtime = RoundTrip(runtime, rules, content, catalog);
            var coreBefore = EndingRuntimeFixture.Core(runtime);
            RuntimeScenario.Accept(runtime.EnterDoorView());
            string doorToken = runtime.GetView().Cue!.Token;
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.DoorMotion));
            if (quotaDay < 0) RuntimeScenario.Complete(runtime, PresentationKind.DoorMotion);
            else runtime = GameRuntime.Restore(runtime.Save(), rules, content, catalog);
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.Dialogue), "The opened door must resume its conversation, including after restore.");
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(doorToken));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(runtime.GetView().Cue!.Token));
            for (int index = 0; index < lineCount; index++)
            {
                var expected = catalog.Get(conversation).Lines[index];
                Assert.That(runtime.CurrentEndingLine!.SourceId, Is.EqualTo(expected.SourceId));
                Assert.That(runtime.EndingLineIndex, Is.EqualTo(index));
                RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine("wrong-conversation__" + index));
                RuntimeScenario.Accept(runtime.SetPaused(true));
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(expected.SourceId));
                runtime = RoundTrip(runtime, rules, content, catalog);
                RuntimeScenario.Accept(runtime.SetPaused(false));
                RuntimeScenario.Accept(runtime.CompleteEndingLine(expected.SourceId));
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(expected.SourceId));
                runtime = RoundTrip(runtime, rules, content, catalog);
            }
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.Thanks));
            Assert.That(runtime.CurrentEndingLine, Is.Null);
            Assert.That(runtime.GetView().Cue, Is.Null);
            Assert.That(JToken.DeepEquals(EndingRuntimeFixture.Core(runtime), coreBefore), Is.True,
                "The door, ending dialogue and presentation restore must not execute core outcomes twice.");
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.Accept(runtime.MarkEndingCompleted());
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
            Assert.That(RoundTrip(runtime, rules, content, catalog).EndingStage, Is.EqualTo(EndingStage.Completed));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SelfReviewUsesOrdinaryDirectoryAndSixteenPersistedLinesBeforeItsOnlyDisposition(bool directory)
        {
            var rules = EndingRuntimeFixture.Rules(trust: 10);
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog, directory);
            EndingRuntimeFixture.DriveTo(runtime, view => view.IsSelfReview);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(3));
            Assert.That(runtime.GetView().Patients.Select(item => item.Id), Is.EqualTo(new[] { GameRuntime.SelfPatientId }));
            Assert.That(runtime.GetView().Transcript, Is.Empty, "Self-review dialogue must not appear before opening its personnel review.");
            Assert.That(runtime.GetView().TerminalPower, Is.EqualTo(TerminalPowerState.Off));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(runtime.GetView().TerminalBootRequired, Is.False);
            Assert.That(runtime.GetView().Transcript, Is.Empty, "The ordinary directory must not reveal the first self-review line.");
            runtime = RoundTrip(runtime, rules, content, catalog);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Transcript, Is.Empty);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.EnterSelectedPatient("formal"));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.LeaveTerminal);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId));
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
            Assert.That(runtime.GetView().PresentedEncounterId, Is.EqualTo(GameRuntime.SelfPatientId));
            Assert.That(runtime.GetView().ConfiguredDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            AssertSelfReviewTranscript(runtime, catalog, 0);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(GameRuntime.SelfPatientId, Disposition.Treat));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(GameRuntime.SelfPatientId, Disposition.Release));
            var before = EndingRuntimeFixture.Core(runtime);
            for (int index = 0; index < 16; index++)
            {
                string sourceId = catalog.Get(GameRuntime.SelfPatientId).Lines[index].SourceId;
                Assert.That(runtime.GetView().SelfReviewReady, Is.False);
                Assert.That(runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(index));
                AssertSelfReviewTranscript(runtime, catalog, index);
                RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(GameRuntime.SelfPatientId, Disposition.Purify));
                if (index < 15)
                    RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(catalog.Get(GameRuntime.SelfPatientId).Lines[index + 1].SourceId));
                RuntimeScenario.Accept(runtime.SetPaused(true));
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(sourceId));
                RuntimeScenario.Accept(runtime.SetPaused(false));
                RuntimeScenario.Accept(runtime.CompleteEndingLine(sourceId));
                AssertSelfReviewTranscript(runtime, catalog, index + 1);
                runtime = RoundTrip(runtime, rules, content, catalog);
                AssertSelfReviewTranscript(runtime, catalog, index + 1);
                var restoredCore = EndingRuntimeFixture.Core(runtime);
                foreach (string key in new[] { "Patients", "Days", "Encounters" })
                    Assert.That(JToken.DeepEquals(before["State"]![key], restoredCore["State"]![key]), Is.True,
                        "Displaying and restoring self-review dialogue must not change " + key);
                RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteEndingLine(sourceId));
            }
            Assert.That(runtime.GetView().SelfReviewReady, Is.True);
            Assert.That(runtime.CurrentEndingLine, Is.Null);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition("stale-patient-encounter", Disposition.Purify));
            var confirmed = RuntimeScenario.Accept(runtime.SubmitDisposition(GameRuntime.SelfPatientId, Disposition.Purify));
            Assert.That(confirmed.Events.Count(item => item.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.Transition));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.ConfirmSelfPurification);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(runtime.GetView().Cue!.Token));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.CompleteHiddenBed);
            var after = EndingRuntimeFixture.Core(runtime);
            foreach (string key in new[] { "Patients", "Days", "Encounters" })
                Assert.That(JToken.DeepEquals(before["State"]![key], after["State"]![key]), Is.True, key);
            runtime = RoundTrip(runtime, rules, content, catalog);
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingSpaceReady);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.Accept(runtime.MarkEndingSpaceReady());
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingSpaceReady);
            runtime = RoundTrip(runtime, rules, content, catalog);
            Assert.That(runtime.EndingStage, Is.EqualTo(EndingStage.Space));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
            RuntimeScenario.Accept(runtime.SetPaused(true));
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.CompleteHiddenBed);
            RuntimeScenario.Accept(runtime.SetPaused(false));
            RuntimeScenario.Accept(runtime.CompleteHiddenBed());
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.CompleteHiddenBed);
            runtime = RoundTrip(runtime, rules, content, catalog);
            RuntimeScenario.Accept(runtime.MarkEndingCompleted());
            RuntimeScenario.RejectWithoutMutation(runtime, runtime.MarkEndingCompleted);
            Assert.That(RoundTrip(runtime, rules, content, catalog).EndingStage, Is.EqualTo(EndingStage.Completed));
        }

        [Test]
        public void RestoreRejectsMismatchedCatalogAndImpossibleEndingProgress()
        {
            var rules = EndingRuntimeFixture.Rules();
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog);
            var normal = JObject.Parse(runtime.Save());
            normal["EndingStage"] = "Thanks";
            normal["EndingLineIndex"] = 7;
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(normal.ToString(), rules, content, catalog));
            EndingRuntimeFixture.DriveTo(runtime, view => view.Game.Phase == GamePhase.Ending);
            string saved = runtime.Save();
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(saved, rules, content));
            var changedCatalog = EndingRuntimeFixture.Catalog();
            changedCatalog.Revision += "-changed";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(saved, rules, content, changedCatalog));
            foreach (var invalid in new[] { ("None", 0), ("AwaitingDoor", 1), ("Dialogue", 7),
                ("Dialogue", -1), ("Thanks", 6), ("Completed", 8), ("Transition", 0), ("Space", 0) })
            {
                var changed = JObject.Parse(saved);
                changed["EndingStage"] = invalid.Item1;
                changed["EndingLineIndex"] = invalid.Item2;
                Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(changed.ToString(), rules, content, catalog), invalid.ToString());
            }
        }

        [Test]
        public void RuntimeRestoreCannotForgeCoreSelfReviewCompletionOrNonhiddenRoute()
        {
            var rules = EndingRuntimeFixture.Rules(trust: 10);
            var content = new EndingRuntimeFixture.Content();
            var catalog = EndingRuntimeFixture.Catalog();
            var runtime = EndingRuntimeFixture.Create(rules, content, catalog);
            EndingRuntimeFixture.DriveTo(runtime, view => view.IsSelfReview);
            var forged = JObject.Parse(runtime.Save());
            var core = JObject.Parse(forged["CoreJson"]!.Value<string>()!);
            core["State"]!["SelfReviewLineIndex"] = 16;
            forged["CoreJson"] = core.ToString();
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(forged.ToString(), rules, content, catalog));
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            RuntimeScenario.Accept(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId));
            foreach (var line in catalog.Get(GameRuntime.SelfPatientId).Lines)
                RuntimeScenario.Accept(runtime.CompleteEndingLine(line.SourceId));
            RuntimeScenario.Accept(runtime.ConfirmSelfPurification());
            foreach (var invalid in new[] { ("AwaitingDoor", 0), ("Dialogue", 0), ("Transition", 1) })
            {
                var changed = JObject.Parse(runtime.Save());
                changed["EndingStage"] = invalid.Item1;
                changed["EndingLineIndex"] = invalid.Item2;
                Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(changed.ToString(), rules, content, catalog));
            }
        }

        private static void AssertSelfReviewTranscript(GameRuntime runtime, EndingDialogueCatalog catalog, int completedLines)
        {
            var conversation = catalog.Get(GameRuntime.SelfPatientId);
            var view = runtime.GetView();
            int visibleLines = Math.Min(completedLines + 1, conversation.Lines.Length);
            Assert.That(view.Transcript.Count, Is.EqualTo(visibleLines),
                "The transcript renders completed lines and the current incomplete line, including the very first line.");
            Assert.That(view.Transcript.Select(entry => entry.SourceNodeId),
                Is.EqualTo(conversation.Lines.Take(visibleLines).Select(line => line.SourceId)));
            Assert.That(view.Transcript.Select(entry => entry.Text),
                Is.EqualTo(conversation.Lines.Take(visibleLines).Select(line => line.Text)));
            Assert.That(view.Transcript.Select(entry => entry.Completed),
                Is.EqualTo(Enumerable.Range(0, visibleLines).Select(index => index < completedLines)));
            Assert.That(view.Transcript.All(entry => entry.EncounterId == GameRuntime.SelfPatientId
                && entry.Speaker == view.PresentedPatientName && !entry.IsPlayer), Is.True);
            Assert.That(view.SelfReviewReady, Is.EqualTo(completedLines == conversation.Lines.Length));
            Assert.That(runtime.CurrentEndingLine?.SourceId,
                Is.EqualTo(completedLines < conversation.Lines.Length ? conversation.Lines[completedLines].SourceId : null));
        }

        private static GameRuntime RoundTrip(GameRuntime runtime, GameRules rules, IGameContent content, EndingDialogueCatalog catalog)
        {
            string saved = runtime.Save();
            var restored = GameRuntime.Restore(saved, rules, content, catalog);
            Assert.That(restored.Save(), Is.EqualTo(saved));
            return restored;
        }
    }

    internal static class EndingRuntimeFixture
    {
        internal static EndingDialogueCatalog Catalog()
        {
            EndingConversation Conversation(string id, int count) => new EndingConversation
            {
                Id = id, Node = id, Speaker = id, FinalAction = id == GameRuntime.SelfPatientId ? "purify" : "",
                Lines = Enumerable.Range(0, count).Select(index => new EndingDialogueLine
                { Id = "line" + index, SourceId = id + "__line" + index, SourceRow = index + 2,
                    SourceCell = "D" + (index + 2), Text = "测试对白" + index }).ToArray()
            };
            return EndingDialogueCatalog.Parse(JsonConvert.SerializeObject(new EndingDialogueCatalog
            {
                FormatVersion = 1, Revision = "ending-runtime-fixture-v1", ContentVersion = "fixture",
                Conversations = new[] { Conversation(GameRuntime.SelfPatientId, 16), Conversation("priest-ending", 8),
                    Conversation("supervisor-ending", 7) }
            }));
        }

        internal static GameRuntime Create(GameRules rules, IGameContent content, EndingDialogueCatalog catalog, bool directory = true)
        {
            var runtime = new GameRuntime(rules, content, directory);
            runtime.ConfigureEndings(catalog);
            return runtime;
        }

        internal static GameRules Rules(int trust = 0, int quotaDay = -1)
        {
            var patients = new[] { new PatientDefinition("tutorial", "教学", 0, 100, null, PatientPresence.TreatmentEnded),
                new PatientDefinition(PrototypeRules.Child, "正式病人", 1, trust, null, PatientPresence.TreatmentEnded),
                new PatientDefinition("priest", "神父", 2, 0, null, PatientPresence.TreatmentEnded, attacksAfterRelease: true) };
            return new GameRules("ending-runtime-fixture", patients, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, day == quotaDay ? 1 : 0, patients.Where(patient => day == 0
                    ? patient.FirstDay == 0 : patient.FirstDay > 0 && patient.FirstDay <= day).Select(patient =>
                    new MeetingDefinition(patient.Id, new[] { new RoundDefinition(patient.Id + day,
                        new[] { new ResponseDefinition("pick", patient.Id + ".reply", 0) }) }, null, 100, 0,
                        new Dictionary<Disposition, string> { [Disposition.Treat] = "treat", [Disposition.Purify] = "purify", [Disposition.Release] = "release" })))),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
        }

        internal static JObject Core(GameRuntime runtime) => JObject.Parse(JObject.Parse(runtime.Save())["CoreJson"]!.Value<string>()!);

        internal static void DriveTo(GameRuntime runtime, Func<RuntimeView, bool> stop, int releaseDay = -1)
        {
            for (int guard = 0; guard < 400; guard++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                if (view.Cue != null) RuntimeScenario.Accept(runtime.CompleteCue(view.Cue.Token));
                else if (view.TerminalPage == TerminalPage.Archive) RuntimeScenario.Accept(runtime.LeaveTerminal());
                else if (view.TerminalPage == TerminalPage.Directory) RuntimeScenario.Accept(runtime.EnterSelectedPatient(view.Game.CurrentPatientId!));
                else if (view.Input == InputContext.Dialogue)
                {
                    if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                        RuntimeScenario.Accept(runtime.SubmitDisposition(view.Game.CurrentEncounterId!, view.Game.Day == releaseDay
                            && view.Game.CurrentPatientId == "priest" ? Disposition.Release : Disposition.Treat));
                    else RuntimeScenario.Accept(runtime.SelectResponse(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, "pick"));
                }
                else if (view.Game.Phase == GamePhase.Evening) RuntimeScenario.Accept(runtime.BeginNightDream());
                else if (view.Game.Phase == GamePhase.MorningInspection) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
                else if (view.Game.Phase == GamePhase.AfterDream) RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Bed));
                else if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                    RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
                else Assert.Fail("No ending scenario action for " + view.Game.Phase);
            }
            Assert.Fail("Ending scenario exceeded its bounded command count.");
        }

        internal sealed class Content : IGameContent
        {
            public string Revision => "ending-runtime-fixture";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId!, "测试病人", "测试对白",
                view.AvailableResponseIds.Select(id => new ResponseContent(id, "回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }
    }
}
