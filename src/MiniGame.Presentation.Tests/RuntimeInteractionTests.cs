using System;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RuntimeInteractionTests
    {
        [Test]
        public void DoorInspectionAndBriefingMustCompleteBeforeComputerIsAvailable()
        {
            var runtime = RuntimeScenario.Create();
            Assert.That(runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Door }));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Bed));
            var result = RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Door));
            Assert.That(result.Events, Is.Empty);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.MorningInspection);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Briefing));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.Complete(runtime, PresentationKind.Briefing);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            Assert.That(runtime.GetView().AllowedInteractions, Does.Contain(InteractionTarget.Computer));
        }

        [Test]
        public void EnterCameraCueBlocksDialogueAndWorldCommandsUntilItsOwnTokenCompletes()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.EnterApartment(runtime);
            RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            var view = runtime.GetView();
            Assert.That(view.Cue!.Kind, Is.EqualTo(PresentationKind.EnterTerminal));
            Assert.That(view.Input, Is.EqualTo(InputContext.Presentation));
            Assert.That(view.AllowedInteractions, Is.Empty);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, "neutral"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.Inspect(InspectionKind.Eyes));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue("wrong-token"));
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
            Assert.That(runtime.GetView().Dialogue!.NodeId, Is.EqualTo(runtime.GetView().Game.CurrentNodeId));
        }

        [TestCase("encounter")]
        [TestCase("node")]
        [TestCase("response")]
        public void ReplyIdentifiersMustBelongToTheActiveEncounterAndDialogueNode(string invalidPart)
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            var game = runtime.GetView().Game;
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                invalidPart == "encounter" ? "old-encounter" : game.CurrentEncounterId!,
                invalidPart == "node" ? "other-node" : game.CurrentNodeId!,
                invalidPart == "response" ? "unregistered-response" : "neutral"));
        }

        [Test]
        public void ReplyCallbackFromThePreviousNodeCannotApplyToTheNextNode()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            var previous = runtime.GetView().Game;
            RuntimeScenario.Reply(runtime, "neutral");
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.Not.EqualTo(previous.CurrentNodeId));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                previous.CurrentEncounterId!, previous.CurrentNodeId!, "empathize"));
        }

        [Test]
        public void DispositionCommitsOnceAndExitRequiresASeparateComputerInteractionForTheNextPatient()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.ReachDisposition(runtime);
            var game = runtime.GetView().Game;
            var result = RuntimeScenario.Accept(runtime.SubmitDisposition(game.CurrentEncounterId!, Disposition.Purify));
            Assert.That(result.Events.Count(e => e.Kind == GameEventKind.DispositionCommitted), Is.EqualTo(1));
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.Zero);
            var reactionToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(game.CurrentEncounterId!, Disposition.Purify));
            var reaction = RuntimeScenario.Complete(runtime, PresentationKind.Reaction);
            Assert.That(reaction.Events.Count(e => e.Kind == GameEventKind.EncounterCompleted), Is.EqualTo(1));
            Assert.That(reaction.Events.Any(e => e.Kind == GameEventKind.DispositionCommitted), Is.False);
            Assert.That(runtime.GetView().Cue!.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(reactionToken));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            var exitToken = runtime.GetView().Cue!.Token;
            RuntimeScenario.Complete(runtime, PresentationKind.ExitTerminal);
            var next = runtime.GetView();
            Assert.That(next.Input, Is.EqualTo(InputContext.World));
            Assert.That(next.TerminalOpen, Is.False);
            Assert.That(next.Game.Phase, Is.EqualTo(GamePhase.Work));
            Assert.That(next.Game.CurrentPatientId, Is.Not.EqualTo(game.CurrentPatientId));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(exitToken));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                next.Game.CurrentEncounterId!, next.Game.CurrentNodeId!, "neutral"));
            var reopen = RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(reopen.Events, Is.Empty);
            RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.GetView().Dialogue!.NodeId, Is.EqualTo(next.Game.CurrentNodeId));
            Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.Zero);
        }

        [TestCase(InspectionKind.Eyes)]
        [TestCase(InspectionKind.Hands)]
        [TestCase(InspectionKind.Teeth)]
        [TestCase(InspectionKind.Photo)]
        [TestCase(InspectionKind.DailySigns)]
        [TestCase(InspectionKind.Records)]
        public void InspectionIsAnExclusiveOverlayAndRecordsObservationWithoutSpendingQuota(InspectionKind kind)
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            var before = runtime.GetView();
            RuntimeScenario.Accept(runtime.Inspect(kind));
            var inspected = runtime.GetView();
            Assert.That(inspected.Input, Is.EqualTo(InputContext.Inspection));
            Assert.That(inspected.Inspection!.Kind, Is.EqualTo(kind));
            Assert.That(inspected.Inspection.Summary, Is.Not.Empty);
            Assert.That(inspected.ObservedSigns.Count, Is.EqualTo(before.ObservedSigns.Count + 1));
            Assert.That(inspected.Game.RemainingPurificationQuota, Is.EqualTo(before.Game.RemainingPurificationQuota));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                before.Game.CurrentEncounterId!, before.Game.CurrentNodeId!, "neutral"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(before.Game.CurrentEncounterId!, Disposition.Treat));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.Inspect(InspectionKind.Records));
            RuntimeScenario.Accept(runtime.CloseInspection());
            Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Dialogue));
            RuntimeScenario.Accept(runtime.Inspect(kind));
            Assert.That(runtime.GetView().ObservedSigns.Count, Is.EqualTo(inspected.ObservedSigns.Count));
        }

        [Test]
        public void InspectionCanOpenAtDispositionCheckpointAndDoesNotChooseADisposition()
        {
            var runtime = RuntimeScenario.Create();
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.ReachDisposition(runtime);
            var encounter = runtime.GetView().Game.CurrentEncounterId!;
            RuntimeScenario.Accept(runtime.Inspect(InspectionKind.Records));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(encounter, Disposition.Purify));
            RuntimeScenario.Accept(runtime.CloseInspection());
            Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            RuntimeScenario.Accept(runtime.SubmitDisposition(encounter, Disposition.Treat));
        }

        [TestCase("world")]
        [TestCase("enter")]
        [TestCase("dialogue")]
        [TestCase("disposition")]
        [TestCase("inspection")]
        public void PausePreventsAllGameplayInputAndResumingPreservesTheCheckpoint(string checkpoint)
        {
            var runtime = RuntimeScenario.Create();
            if (checkpoint != "world")
            {
                RuntimeScenario.EnterApartment(runtime);
                RuntimeScenario.Accept(runtime.TryInteract(InteractionTarget.Computer));
                if (checkpoint != "enter") RuntimeScenario.Complete(runtime, PresentationKind.EnterTerminal);
                if (checkpoint == "disposition") RuntimeScenario.ReachDisposition(runtime);
                if (checkpoint == "inspection") RuntimeScenario.Accept(runtime.Inspect(InspectionKind.Photo));
            }
            var unpaused = runtime.GetView();
            if (checkpoint == "disposition") Assert.That(unpaused.Game.AvailableDispositions, Does.Contain(Disposition.Treat));
            RuntimeView? notified = null;
            runtime.Changed += view => notified = view;
            RuntimeScenario.Accept(runtime.SetPaused(true));
            Assert.That(notified!.Input, Is.EqualTo(InputContext.Paused));
            Assert.That(runtime.GetView().AllowedInteractions, Is.Empty);
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.TryInteract(InteractionTarget.Computer));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SelectResponse(
                unpaused.Game.CurrentEncounterId ?? "none", unpaused.Game.CurrentNodeId ?? "none", "neutral"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.SubmitDisposition(unpaused.Game.CurrentEncounterId ?? "none", Disposition.Treat));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CompleteCue(unpaused.Cue?.Token ?? "none"));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.Inspect(InspectionKind.Eyes));
            RuntimeScenario.RejectWithoutMutation(runtime, () => runtime.CloseInspection());
            RuntimeScenario.Accept(runtime.SetPaused(false));
            Assert.That(runtime.GetView().Input, Is.EqualTo(unpaused.Input));
            Assert.That(runtime.GetView().Game.CurrentNodeId, Is.EqualTo(unpaused.Game.CurrentNodeId));
            Assert.That(runtime.GetView().Cue?.Token, Is.EqualTo(unpaused.Cue?.Token));
            Assert.That(runtime.GetView().Inspection?.Kind, Is.EqualTo(unpaused.Inspection?.Kind));
            if (checkpoint == "disposition")
                RuntimeScenario.Accept(runtime.SubmitDisposition(unpaused.Game.CurrentEncounterId!, Disposition.Treat));
        }

        [Test]
        public void RuntimeViewAndPatientViewDoNotExposeHiddenTrustOrAssessmentHistory()
        {
            var publicMembers = new[] { typeof(RuntimeView), typeof(PatientView), typeof(PlayerView) }
                .SelectMany(type => type.GetProperties().Select(property => property.Name));
            Assert.That(publicMembers.Any(name => name.IndexOf("Trust", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);
            Assert.That(publicMembers.Any(name => name.IndexOf("Assessment", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);
            var runtime = RuntimeScenario.Create();
            Assert.That(runtime.GetView().Patients.Select(patient => patient.Id).Distinct().Count(), Is.EqualTo(4));
            Assert.That(runtime.GetView().Patients.Any(patient => patient.Id == PrototypeRules.Priest), Is.False);
        }
    }
}
