using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class EndingPolicyTests
    {
        [TestCase(4, 5, false)]
        [TestCase(4, 6, true)]
        [TestCase(-4, 14, true)]
        [TestCase(-4, 13, false)]
        [TestCase(int.MaxValue, int.MaxValue, true)]
        public void HiddenEndingUsesTheFourPatientHistoriesIncludingSignedValuesWithoutOverflow(int first, int second, bool eligible)
        {
            var rules = Rules(new[] { Patient(PrototypeRules.FormerRuler, first), Patient(PrototypeRules.Doctor, second),
                Patient(PrototypeRules.Child, -1), Patient(PrototypeRules.Diplomat, 1) }, rebellion: eligible ? 0 : 100);
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(eligible ? GamePhase.SelfReview : GamePhase.AfterDream));
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.None));
        }

        [TestCase(PrototypeRules.FormerRuler, Disposition.Release, PatientPresence.TreatmentEnded, PatientPresence.Released)]
        [TestCase(PrototypeRules.Doctor, Disposition.Purify, PatientPresence.TreatmentEnded, PatientPresence.Purified)]
        [TestCase(PrototypeRules.Child, Disposition.Treat, PatientPresence.Deceased, PatientPresence.Deceased)]
        [TestCase(PrototypeRules.Diplomat, Disposition.Treat, PatientPresence.TreatmentEnded, PatientPresence.TreatmentEnded)]
        public void DepartedPatientHistoryStillQualifiesAfterSaveRestore(string patientId, Disposition disposition,
            PatientPresence outcome, PatientPresence expectedPresence)
        {
            var rules = Rules(new[] { Patient(patientId, 10, limit: disposition == Disposition.Treat ? 1 : null, outcome: outcome) });
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream,
                (_, __) => disposition);
            var saved = SnapshotCodec.Serialize(session);
            session = SnapshotCodec.Deserialize(saved, rules);
            Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(saved));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Presence, Is.EqualTo(expectedPresence));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(10));
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.SelfReview));
        }

        [Test]
        public void PriestTutorialPlayerAndUnknownPatientTrustCannotMeetTheHiddenGate()
        {
            var session = new GameSession(Rules(new[] { Patient(PrototypeRules.Child, 9), Patient(PrototypeRules.Priest, 100),
                Patient("tutorial", 100, firstDay: 0), Patient("self", 100), Patient("unknown", 100) }));
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
        }

        [Test]
        public void HistoricalTrustIncludesSignedResponsesAndDispositionsAcrossDaysAndRestore()
        {
            int ResponseDelta(int day, string id) => id switch
            {
                PrototypeRules.FormerRuler => 8,
                PrototypeRules.Doctor => 8,
                PrototypeRules.Child => day == 1 ? -3 : day == 2 ? 2 : 0,
                PrototypeRules.Diplomat => 2,
                _ => 0
            };
            int DispositionDelta(int day, string id) => id switch
            {
                PrototypeRules.FormerRuler => -2,
                PrototypeRules.Doctor => -4,
                PrototypeRules.Diplomat => -1,
                _ => 0
            };
            Disposition Policy(int day, string id) => id == PrototypeRules.FormerRuler ? Disposition.Release
                : id == PrototypeRules.Doctor ? Disposition.Purify : Disposition.Treat;
            var rules = Rules(new[] { Patient(PrototypeRules.FormerRuler, 0), Patient(PrototypeRules.Doctor, 0),
                Patient(PrototypeRules.Child, 0), Patient(PrototypeRules.Diplomat, 0, firstDay: 3),
                Patient(PrototypeRules.Priest, 100) }, responseDelta: ResponseDelta, dispositionDelta: DispositionDelta);
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 2 && view.Phase == GamePhase.MorningInspection, Policy);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream, Policy);
            var patients = session.CreateSnapshot().State.Patients.ToDictionary(patient => patient.Id);
            Assert.That(patients[PrototypeRules.FormerRuler].Trust, Is.EqualTo(6));
            Assert.That(patients[PrototypeRules.FormerRuler].Presence, Is.EqualTo(PatientPresence.Released));
            Assert.That(patients[PrototypeRules.Doctor].Trust, Is.EqualTo(4));
            Assert.That(patients[PrototypeRules.Doctor].Presence, Is.EqualTo(PatientPresence.Purified));
            Assert.That(patients[PrototypeRules.Child].Trust, Is.EqualTo(-1));
            Assert.That(patients[PrototypeRules.Diplomat].Trust, Is.EqualTo(1));
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.SelfReview));
            Scenario.RejectWithoutMutation(session, new CompleteDream());
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules)),
                Is.EqualTo(SnapshotCodec.Serialize(session)));
        }

        [Test]
        public void TutorialAndNotYetIntroducedPatientsDoNotContribute()
        {
            var rules = Rules(new[] { Patient("tutorial", 100, firstDay: 0), Patient(PrototypeRules.Child, 9),
                Patient("future", 100, firstDay: 4) }, lastDay: 4);
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream);
            Assert.That(session.CreateSnapshot().State.Patients.All(patient => patient.Presence == PatientPresence.Active), Is.True);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
            Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            DriveTo(session, view => view.Day == 4 && view.Phase == GamePhase.Dream);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream), "Hidden eligibility is evaluated only after Day 3's dream.");
        }

        [Test]
        public void TutorialAloneCannotMeetTheFourPatientHistoryRequirement()
        {
            var session = new GameSession(Rules(new[] { Patient("tutorial", 100, firstDay: 0) }));
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void EarlierDreamsDoNotTriggerHiddenEnding(int day)
        {
            var session = new GameSession(Rules(new[] { Patient(PrototypeRules.Child, 100) }));
            DriveTo(session, view => view.Day == day && view.Phase == GamePhase.Dream);
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
            Scenario.RejectWithoutMutation(session, new AdvanceSelfReviewCommand(0), RejectionReason.WrongPhase);
            Scenario.RejectWithoutMutation(session, new ConfirmSelfPurification(), RejectionReason.WrongPhase);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void FailedKpiFinishesThatDaysDreamThenJumpsToDayFourWithoutInventingWorkDays(int failedDay)
        {
            var rules = Rules(new[] { Patient("formal", 0) }, quotaDay: failedDay);
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == failedDay && view.Phase == GamePhase.Evening);
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.None));
            Scenario.Accept(session, new BeginNightDream());
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(failedDay));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Dream));
            Scenario.Accept(session, new CompleteDream());
            var before = session.CreateSnapshot().State;
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            var result = Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            var after = session.CreateSnapshot().State;
            Assert.That(after.Day, Is.EqualTo(4));
            Assert.That(after.Ending, Is.EqualTo(EndingType.LockedInRoom));
            Assert.That(after.Days.Select(day => day.Day), Is.EqualTo(Enumerable.Range(1, failedDay)));
            Assert.That(JToken.DeepEquals(JToken.FromObject(before.Days), JToken.FromObject(after.Days)), Is.True);
            Assert.That(result.Events.Count(item => item.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed), RejectionReason.GameEnded);
        }

        [Test]
        public void DayZeroQuotaFailureIsIgnored()
        {
            var session = new GameSession(Rules(new[] { Patient("formal", 0) }, quotaDay: 0, startDay: 0));
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.MorningInspection);
            Assert.That(session.CreateSnapshot().State.Days.Single(day => day.Day == 0).QuotaMet, Is.False);
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.None));
        }

        [TestCase(2)]
        [TestCase(3)]
        public void PriestAttackAtNaturalNextDawnWinsOverKpiAndExecutesExactlyOnce(int releaseDay)
        {
            var rules = Rules(new[] { Patient("priest", 0, firstDay: 2, attacks: true), Patient("formal", 0) }, quotaDay: releaseDay);
            var session = new GameSession(rules);
            Disposition Policy(int day, string patient) => day == releaseDay && patient == "priest" ? Disposition.Release : Disposition.Treat;
            DriveTo(session, view => view.Day == releaseDay && view.Phase == GamePhase.Work
                && view.CurrentPatientId == "priest" && view.EncounterStage == EncounterStage.AwaitingDisposition, Policy);
            var command = new SubmitDisposition(session.GetPlayerView().CurrentEncounterId!, Disposition.Release);
            Scenario.Accept(session, command);
            Scenario.RejectWithoutMutation(session, command, RejectionReason.AlreadyDisposed);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Scenario.RejectWithoutMutation(session, command, RejectionReason.AlreadyDisposed);
            Assert.That(session.CreateSnapshot().State.PendingEvents.Count, Is.EqualTo(1));
            var reaction = new CompleteReaction(session.GetPlayerView().CurrentEncounterId!);
            Scenario.Accept(session, reaction);
            Scenario.RejectWithoutMutation(session, reaction);
            DriveTo(session, view => view.Day == releaseDay && view.Phase == GamePhase.AfterDream, Policy);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            var result = Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            var state = session.CreateSnapshot().State;
            Assert.That(state.Day, Is.EqualTo(releaseDay + 1));
            Assert.That(state.Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(state.PendingEvents, Is.Empty);
            Assert.That(state.ExecutedEventIds.Count, Is.EqualTo(1));
            Assert.That(state.Days.Count, Is.EqualTo(releaseDay));
            Assert.That(result.Events.Count(item => item.Kind == GameEventKind.ScheduledEventExecuted), Is.EqualTo(1));
            Assert.That(result.Events.Count(item => item.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules)),
                Is.EqualTo(SnapshotCodec.Serialize(session)));
        }

        [Test]
        public void KpiJumpDoesNotExecuteEventsThatWereNotDueAtTheNaturalNextDawn()
        {
            var rules = Rules(new[] { Patient("priest", 0, attacks: true, attackDelay: 2) }, quotaDay: 1);
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.AfterDream, (_, __) => Disposition.Release);
            var result = Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            var state = session.CreateSnapshot().State;
            Assert.That(state.Ending, Is.EqualTo(EndingType.LockedInRoom));
            Assert.That(state.Day, Is.EqualTo(4));
            Assert.That(state.PendingEvents.Single().DueDay, Is.EqualTo(3));
            Assert.That(state.ExecutedEventIds, Is.Empty);
            Assert.That(result.Events.Any(item => item.Kind == GameEventKind.ScheduledEventExecuted), Is.False);
        }

        [Test]
        public void SelfReviewProgressRequiresEveryExpectedLineAndRestoresWithoutReplaySideEffects()
        {
            var rules = Rules(new[] { Patient(PrototypeRules.Child, 10) });
            var session = new GameSession(rules);
            DriveTo(session, view => view.Phase == GamePhase.SelfReview);
            var original = session.CreateSnapshot().State;
            Assert.That(session.GetPlayerView().SelfReviewLineIndex, Is.Zero);
            Assert.That(session.GetPlayerView().SelfReviewLineCount, Is.EqualTo(16));
            Scenario.RejectWithoutMutation(session, new AdvanceSelfReviewCommand(-1), RejectionReason.WrongNode);
            Scenario.RejectWithoutMutation(session, new AdvanceSelfReviewCommand(1), RejectionReason.WrongNode);
            for (int index = 0; index < 16; index++)
            {
                Assert.That(session.GetPlayerView().SelfReviewCompleted, Is.False);
                Scenario.RejectWithoutMutation(session, new ConfirmSelfPurification(), RejectionReason.WrongStage);
                var result = Scenario.Accept(session, new AdvanceSelfReviewCommand(index));
                Assert.That(result.Events.Count(item => item.Kind == GameEventKind.SelfReviewLineAdvanced), Is.EqualTo(1));
                var saved = SnapshotCodec.Serialize(session);
                session = SnapshotCodec.Deserialize(saved, rules);
                Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(saved));
                Assert.That(session.GetPlayerView().SelfReviewLineIndex, Is.EqualTo(index + 1));
                Scenario.RejectWithoutMutation(session, new AdvanceSelfReviewCommand(index));
            }
            Assert.That(session.GetPlayerView().SelfReviewCompleted, Is.True);
            Scenario.RejectWithoutMutation(session, new AdvanceSelfReviewCommand(16), RejectionReason.WrongStage);
            Scenario.Accept(session, new ConfirmSelfPurification());
            var final = session.CreateSnapshot();
            Assert.That(final.State.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(final.State.Day, Is.EqualTo(4));
            Assert.That(final.State.SelfReviewLineIndex, Is.EqualTo(16));
            Assert.That(final.CommandHistory.Count(item => item.Kind == CommandKind.AdvanceSelfReview), Is.EqualTo(16));
            Assert.That(final.CommandHistory.Count(item => item.Kind == CommandKind.ConfirmSelfPurification), Is.EqualTo(1));
            Assert.That(JToken.DeepEquals(JToken.FromObject(original.Patients), JToken.FromObject(final.State.Patients)), Is.True);
            Assert.That(JToken.DeepEquals(JToken.FromObject(original.Days), JToken.FromObject(final.State.Days)), Is.True);
            Assert.That(JToken.DeepEquals(JToken.FromObject(original.Encounters), JToken.FromObject(final.State.Encounters)), Is.True);
            Scenario.RejectWithoutMutation(session, new ConfirmSelfPurification(), RejectionReason.GameEnded);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules)),
                Is.EqualTo(SnapshotCodec.Serialize(session)));
        }

        [Test]
        public void RestoreRejectsForgedSelfReviewProgressSkippedRepeatedAndMissingLineArguments()
        {
            var rules = Rules(new[] { Patient(PrototypeRules.Child, 10) });
            var session = new GameSession(rules);
            DriveTo(session, view => view.Phase == GamePhase.SelfReview);
            var forgedState = session.CreateSnapshot();
            forgedState.State.SelfReviewLineIndex = 16;
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(forgedState, rules));
            foreach (int? index in new int?[] { null, -1, 1, 16 })
            {
                var forgedHistory = session.CreateSnapshot();
                forgedHistory.CommandHistory.Add(new CommandRecord { Kind = CommandKind.AdvanceSelfReview, ExpectedLineIndex = index });
                Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(forgedHistory, rules));
            }
            var prematurePurification = session.CreateSnapshot();
            prematurePurification.CommandHistory.Add(new CommandRecord { Kind = CommandKind.ConfirmSelfPurification });
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(prematurePurification, rules));
            Scenario.Accept(session, new AdvanceSelfReviewCommand(0));
            var repeated = session.CreateSnapshot();
            repeated.CommandHistory.Add(new CommandRecord { Kind = CommandKind.AdvanceSelfReview, ExpectedLineIndex = 0 });
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(repeated, rules));
            var extraArgument = session.CreateSnapshot();
            extraArgument.CommandHistory.Last().PatientId = "formal";
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(extraArgument, rules));
        }

        private static PatientDefinition Patient(string id, int trust, int firstDay = 1, int? limit = null,
            PatientPresence outcome = PatientPresence.TreatmentEnded, bool attacks = false, int attackDelay = 1)
            => new PatientDefinition(id, id, firstDay, trust, limit, outcome, attacksAfterRelease: attacks, attackDelayDays: attackDelay);

        private static GameRules Rules(IReadOnlyList<PatientDefinition> patients, int quotaDay = -1, int startDay = 1,
            int lastDay = 3, int rebellion = 0, Func<int, string, int>? responseDelta = null,
            Func<int, string, int>? dispositionDelta = null)
        {
            MeetingDefinition Meeting(PatientDefinition patient, int day) => new MeetingDefinition(patient.Id,
                new[] { new RoundDefinition(patient.Id + "." + day, new[]
                { new ResponseDefinition("neutral", patient.Id + ".reply", responseDelta?.Invoke(day, patient.Id) ?? 0) }) },
                null, 100, 0, new Dictionary<Disposition, string>
                { [Disposition.Treat] = patient.Id + ".treat", [Disposition.Purify] = patient.Id + ".purify", [Disposition.Release] = patient.Id + ".release" },
                dispositionTrustDeltas: dispositionDelta == null ? null : new Dictionary<Disposition, int>
                { [Disposition.Treat] = dispositionDelta(day, patient.Id), [Disposition.Purify] = dispositionDelta(day, patient.Id),
                    [Disposition.Release] = dispositionDelta(day, patient.Id) });
            return new GameRules("ending-policy-fixture", patients,
                Enumerable.Range(startDay, lastDay - startDay + 1).Select(day => new DayDefinition(day, day == quotaDay ? 1 : 0,
                    patients.Where(patient => patient.FirstDay <= day).Select(patient => Meeting(patient, day)))),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(rebellion, 100, 100, new Dictionary<string, int>());
        }

        private static void DriveTo(GameSession session, Func<PlayerView, bool> stop, Func<int, string, Disposition>? policy = null)
        {
            for (int guard = 0; guard < 500; guard++)
            {
                if (stop(session.GetPlayerView())) return;
                if (session.GetPlayerView().Phase == GamePhase.Evening) Scenario.Accept(session, new BeginNightDream());
                else Scenario.Step(session, policy ?? ((_, __) => Disposition.Treat));
            }
            Assert.Fail("Ending scenario did not reach its requested checkpoint.");
        }
    }
}
