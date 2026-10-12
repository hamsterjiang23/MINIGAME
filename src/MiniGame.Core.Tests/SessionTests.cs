using System;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class SessionTests
    {
        private GameSession NewGame() => new GameSession(PrototypeRules.Create());

        [Test]
        public void InitialRosterContainsFivePatientsButPriestDebutsOnDayTwo()
        {
            var session = NewGame();
            var snapshot = session.CreateSnapshot();
            Assert.That(snapshot.State.Patients.Count, Is.EqualTo(5));
            Assert.That(snapshot.State.Queue, Does.Not.Contain(PrototypeRules.Priest));
            Assert.That(snapshot.State.Queue.Count, Is.EqualTo(4));
            Scenario.DriveUntil(session, v => v.Day == 2 && v.Phase == GamePhase.MorningInspection);
            Assert.That(session.CreateSnapshot().State.Queue, Does.Contain(PrototypeRules.Priest));
        }

        [Test]
        public void InvalidPhaseNullAndUnknownCommandsLeaveStateUnchanged()
        {
            var session = NewGame();
            Scenario.RejectWithoutMutation(session, new CompleteDream(), RejectionReason.WrongPhase);
            Scenario.RejectWithoutMutation(session, null, RejectionReason.InvalidCommand);
            Scenario.RejectWithoutMutation(session, new UnknownCommand(), RejectionReason.InvalidCommand);
            Scenario.RejectWithoutMutation(session, new Interact((InteractionTarget)999), RejectionReason.InvalidCommand);
        }

        [Test]
        public void ConversationAndBranchRejectPrematureDispositionAndStaleCallbacks()
        {
            var session = NewGame(); Scenario.EnterWork(session);
            var first = session.GetPlayerView();
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(first.CurrentEncounterId!, Disposition.Purify), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new SelectResponse("wrong", first.CurrentNodeId!, "neutral"), RejectionReason.WrongEncounter);
            Scenario.RejectWithoutMutation(session, new SelectResponse(first.CurrentEncounterId!, "wrong", "neutral"), RejectionReason.WrongNode);
            Scenario.RejectWithoutMutation(session, new SelectResponse(first.CurrentEncounterId!, first.CurrentNodeId!, "unknown"), RejectionReason.UnknownResponse);
            Scenario.Accept(session, new SelectResponse(first.CurrentEncounterId!, first.CurrentNodeId!, "empathize"));
            Scenario.RejectWithoutMutation(session, new SelectResponse(first.CurrentEncounterId!, first.CurrentNodeId!, "empathize"), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(first.CurrentEncounterId!, Disposition.Treat), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new CompleteResponseBranch(first.CurrentEncounterId!, "wrong"), RejectionReason.WrongNode);
            var branch = session.GetPlayerView();
            Scenario.Accept(session, new CompleteResponseBranch(branch.CurrentEncounterId!, branch.CurrentNodeId!));
            Scenario.RejectWithoutMutation(session, new CompleteResponseBranch(branch.CurrentEncounterId!, branch.CurrentNodeId!), RejectionReason.WrongStage);
        }

        [Test]
        public void RepliesHaveDifferentBranchesAndConvergeToTheSameNextRound()
        {
            var positive = NewGame(); var negative = NewGame();
            Scenario.EnterWork(positive); Scenario.EnterWork(negative);
            var p = positive.GetPlayerView(); var n = negative.GetPlayerView();
            Scenario.Accept(positive, new SelectResponse(p.CurrentEncounterId!, p.CurrentNodeId!, "empathize"));
            Scenario.Accept(negative, new SelectResponse(n.CurrentEncounterId!, n.CurrentNodeId!, "pressure"));
            Assert.That(positive.GetPlayerView().CurrentNodeId, Is.Not.EqualTo(negative.GetPlayerView().CurrentNodeId));
            p = positive.GetPlayerView(); n = negative.GetPlayerView();
            Scenario.Accept(positive, new CompleteResponseBranch(p.CurrentEncounterId!, p.CurrentNodeId!));
            Scenario.Accept(negative, new CompleteResponseBranch(n.CurrentEncounterId!, n.CurrentNodeId!));
            Assert.That(positive.GetPlayerView().CurrentNodeId, Is.EqualTo(negative.GetPlayerView().CurrentNodeId));
            Assert.That(positive.CreateSnapshot().State.Patients.Single(x => x.Id == PrototypeRules.Child).Trust, Is.EqualTo(10));
            Assert.That(negative.CreateSnapshot().State.Patients.Single(x => x.Id == PrototypeRules.Child).Trust, Is.EqualTo(-10));
        }

        [Test]
        public void TrustIsIndependentAndDoesNotUseAHardCodedZeroToHundredRange()
        {
            var session = NewGame(); Scenario.EnterWork(session); Scenario.Reply(session, "pressure");
            var patients = session.CreateSnapshot().State.Patients;
            Assert.That(patients.Single(x => x.Id == PrototypeRules.Child).Trust, Is.EqualTo(-10));
            Assert.That(patients.Where(x => x.Id != PrototypeRules.Child).All(x => x.Trust == 0), Is.True);
            var high = new GameSession(Scenario.SinglePatient(initialTrust: 100)); Scenario.EnterWork(high);
            Scenario.Reply(high, "empathize");
            Assert.That(high.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(110));
        }

        [TestCase(int.MaxValue, "empathize")]
        [TestCase(int.MinValue, "pressure")]
        public void TrustArithmeticOverflowIsRejectedWithoutPartialMutation(int initialTrust, string response)
        {
            var session = new GameSession(Scenario.SinglePatient(initialTrust)); Scenario.EnterWork(session);
            var view = session.GetPlayerView();
            Scenario.RejectWithoutMutation(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, response), RejectionReason.NumericOverflow);
        }

        [Test]
        public void SpecialRoundStartsAfterNormalDialogueAndBlocksDispositionUntilItsBranchCompletes()
        {
            var session = NewGame(); Scenario.EnterWork(session);
            for (var i = 0; i < 4; i++) Scenario.Reply(session, "empathize");
            var view = session.GetPlayerView();
            Assert.That(view.EncounterStage, Is.EqualTo(EncounterStage.SpecialDialogue));
            Assert.That(view.AvailableDispositions, Is.Empty);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Purify), RejectionReason.WrongStage);
            Scenario.Reply(session, "neutral"); view = session.GetPlayerView();
            Assert.That(view.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(view.AvailableDispositions, Does.Contain(Disposition.Release));
            var encounter = session.CreateSnapshot().State.Encounters.Single();
            Assert.That(encounter.SpecialStarted && encounter.SpecialCompleted, Is.True);
            Assert.That(encounter.Responses.Count, Is.EqualTo(5));
            Scenario.RejectWithoutMutation(session, new SelectResponse(view.CurrentEncounterId!, "day1.child.special", "empathize"), RejectionReason.WrongStage);
        }

        [Test]
        public void LowTrustSkipsSpecialDialogueAndPreventsRelease()
        {
            var session = NewGame(); Scenario.EnterWork(session); Scenario.ReachDisposition(session);
            var view = session.GetPlayerView();
            Assert.That(session.CreateSnapshot().State.Encounters.Single().SpecialStarted, Is.False);
            Assert.That(view.AvailableDispositions, Does.Not.Contain(Disposition.Release));
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Release), RejectionReason.ReleaseUnavailable);
        }

        [Test]
        public void SpecialReplyCanRemoveReleaseEligibilityBeforeDisposition()
        {
            var session = NewGame(); Scenario.EnterWork(session);
            Scenario.Reply(session, "empathize"); Scenario.Reply(session, "empathize");
            Scenario.Reply(session, "neutral"); Scenario.Reply(session, "neutral");
            Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.SpecialDialogue));
            Scenario.Reply(session, "pressure");
            var view = session.GetPlayerView();
            Assert.That(view.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(session.CreateSnapshot().State.Patients.Single(p => p.Id == PrototypeRules.Child).Trust, Is.EqualTo(10));
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Release), RejectionReason.ReleaseUnavailable);
        }

        [Test]
        public void EachOfTheFiveDayTwoPatientsHasIndependentPersistentTrust()
        {
            var session = NewGame();
            Scenario.DriveUntil(session, v => v.Day == 2 && v.Phase == GamePhase.MorningInspection, (_, _) => Disposition.Treat);
            Scenario.EnterWork(session);
            var visited = new System.Collections.Generic.HashSet<string>();
            while (session.GetPlayerView().Phase == GamePhase.Work)
            {
                var view = session.GetPlayerView(); var id = view.CurrentPatientId!;
                var before = session.CreateSnapshot().State.Patients.ToDictionary(p => p.Id, p => p.Trust);
                var response = id == PrototypeRules.Child || id == PrototypeRules.Diplomat ? "empathize" : "pressure";
                Scenario.Reply(session, response);
                foreach (var patient in session.CreateSnapshot().State.Patients)
                    Assert.That(patient.Trust, Is.EqualTo(before[patient.Id] +
                        (patient.Id == id ? (response == "empathize" ? 10 : -10) : 0)));
                Scenario.ReachDisposition(session);
                Scenario.Accept(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Treat));
                Scenario.Accept(session, new CompleteReaction(view.CurrentEncounterId!));
                visited.Add(id);
            }
            Assert.That(visited.Count, Is.EqualTo(5));
            var recordedTrust = session.CreateSnapshot().State.Patients.ToDictionary(p => p.Id, p => p.Trust);
            Scenario.NextMorning(session);
            foreach (var patient in session.CreateSnapshot().State.Patients)
                Assert.That(patient.Trust, Is.EqualTo(recordedTrust[patient.Id]));
        }

        [TestCase(Disposition.Treat, 1, 1)]
        [TestCase(Disposition.Purify, 0, 0)]
        [TestCase(Disposition.Release, 1, 0)]
        public void DispositionCommitsOnceAndReactionDoesNotRepeatItsEffects(Disposition disposition, int remaining, int treatments)
        {
            var session = NewGame(); Scenario.EnterWork(session); Scenario.ReachDisposition(session, "empathize");
            var id = session.GetPlayerView().CurrentEncounterId!;
            var result = Scenario.Accept(session, new SubmitDisposition(id, disposition));
            Assert.That(result.Events.Count(e => e.Kind == GameEventKind.DispositionCommitted), Is.EqualTo(1));
            Assert.That(session.GetPlayerView().RemainingPurificationQuota, Is.EqualTo(remaining));
            Assert.That(session.CreateSnapshot().State.Patients.Single(p => p.Id == PrototypeRules.Child).TreatmentCount, Is.EqualTo(treatments));
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(id, disposition), RejectionReason.AlreadyDisposed);
            Scenario.Accept(session, new CompleteReaction(id));
            Assert.That(session.GetPlayerView().RemainingPurificationQuota, Is.EqualTo(remaining));
            Scenario.RejectWithoutMutation(session, new CompleteReaction(id), RejectionReason.WrongEncounter);
        }

        [Test]
        public void InvalidDispositionIsRejectedBeforeMutation()
        {
            var session = NewGame(); Scenario.EnterWork(session); Scenario.ReachDisposition(session);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(session.GetPlayerView().CurrentEncounterId!, (Disposition)999), RejectionReason.InvalidCommand);
        }

        [Test]
        public void QuotaDisplayDoesNotGoBelowZeroAndTreatmentDoesNotConsumeQuota()
        {
            var session = NewGame();
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Evening, (_, _) => Disposition.Purify);
            Assert.That(session.GetPlayerView().RemainingPurificationQuota, Is.Zero);
            Assert.That(session.CreateSnapshot().State.Days.Single().Purifications, Is.EqualTo(4));
            Assert.That(session.CreateSnapshot().State.Days.Single().QuotaMet, Is.True);
        }

        [Test]
        public void TerminatedPatientsAreFilteredAndEmptyDaysStillReachTheBed()
        {
            var session = NewGame();
            Scenario.DriveUntil(session, v => v.Day == 3 && v.Phase == GamePhase.Apartment, (_, _) => Disposition.Purify);
            Assert.That(session.CreateSnapshot().State.Queue, Is.Empty);
            Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(session.CreateSnapshot().State.Days.Last().QuotaMet, Is.False);
            Scenario.NextMorning(session);
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.LockedInRoom));
        }

        [Test]
        public void AfterDreamOnlyBedIsAvailableAndTheSameInputCannotAdvanceTwoDays()
        {
            var session = NewGame(); Scenario.DriveUntil(session, v => v.Phase == GamePhase.AfterDream);
            Assert.That(session.GetPlayerView().AllowedInteractions, Is.EquivalentTo(new[] { InteractionTarget.Bed }));
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Computer), RejectionReason.InteractionUnavailable);
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Door), RejectionReason.InteractionUnavailable);
            Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(2));
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed), RejectionReason.InteractionUnavailable);
        }

        [Test]
        public void ThirdTreatmentTerminatesOrdinaryPatientsButDoesNotInventDiplomatDeath()
        {
            var session = NewGame(); Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, (_, _) => Disposition.Treat);
            var state = session.CreateSnapshot().State;
            foreach (var id in new[] { PrototypeRules.Child, PrototypeRules.FormerRuler, PrototypeRules.Doctor })
            {
                var patient = state.Patients.Single(p => p.Id == id);
                Assert.That(patient.TreatmentCount, Is.EqualTo(3));
                Assert.That(patient.Presence, Is.EqualTo(PatientPresence.Deceased));
            }
            Assert.That(state.Patients.Single(p => p.Id == PrototypeRules.Diplomat).Presence, Is.EqualTo(PatientPresence.TreatmentEnded));
            var priest = state.Patients.Single(p => p.Id == PrototypeRules.Priest);
            Assert.That(priest.TreatmentCount, Is.EqualTo(2));
            Assert.That(priest.Presence, Is.EqualTo(PatientPresence.Active));
        }

        [Test]
        public void TreatmentTerminationCanBeScheduledForTheNextDay()
        {
            var session = new GameSession(Scenario.SinglePatient(treatmentLimit: 1, delay: 1));
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Evening, (_, _) => Disposition.Treat);
            Assert.That(session.CreateSnapshot().State.Patients.Single().Presence, Is.EqualTo(PatientPresence.Active));
            Assert.That(session.CreateSnapshot().State.PendingEvents.Single().DueDay, Is.EqualTo(2));
            Scenario.NextMorning(session);
            var state = session.CreateSnapshot().State;
            Assert.That(state.Patients.Single().Presence, Is.EqualTo(PatientPresence.Deceased));
            Assert.That(state.Queue, Is.Empty);
            Assert.That(state.PendingEvents, Is.Empty);
            Assert.That(state.ExecutedEventIds.Count, Is.EqualTo(1));
        }

        [Test]
        public void AcceptedRepliesAreRecordedAndThePublicViewDoesNotExposeTrust()
        {
            var session = NewGame(); Scenario.EnterWork(session); Scenario.Reply(session, "pressure");
            var encounter = session.CreateSnapshot().State.Encounters.Single();
            Assert.That(encounter.Responses.Single().ResponseId, Is.EqualTo("pressure"));
            Assert.That(encounter.Responses.Single().TrustDelta, Is.EqualTo(-10));
            Assert.That(typeof(PlayerView).GetProperties().Any(p => p.Name.Contains("Trust")), Is.False);
            Assert.That(typeof(GameEvent).GetProperties().Any(p => p.Name.Contains("Trust")), Is.False);
        }

        private sealed class UnknownCommand : GameCommand { }
    }
}
