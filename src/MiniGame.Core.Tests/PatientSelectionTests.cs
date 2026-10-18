using System;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    public sealed class PatientSelectionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void DailyOrderIsChosenFreelyAndEachPatientHasExactlyOneEncounter(bool reverse)
        {
            var rules = FivePatientRules();
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            var order = reverse ? new[] { "e", "d", "c", "b", "a" } : new[] { "c", "a", "e", "b", "d" };
            foreach (var id in order)
            {
                Assert.That(session.CanReceiveToday(id), Is.True);
                session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
                Scenario.Accept(session, new SelectPatient(id));
                Assert.That(session.GetPlayerView().CurrentPatientId, Is.EqualTo(id));
                Assert.That(session.CreateSnapshot().State.Encounters.Count(e => e.PatientId == id && e.Day == 1), Is.EqualTo(1));
                Assert.That(rules.Patients.All(p => !session.CanReceiveToday(p.Id)), Is.True);
                session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
                Finish(session, Disposition.Treat);
            }
            var snapshot = session.CreateSnapshot();
            Assert.That(snapshot.State.Encounters.Select(e => e.PatientId), Is.EqualTo(order));
            Assert.That(snapshot.State.Encounters.All(e => e.Stage == EncounterStage.Completed && e.ExplicitlySelected), Is.True);
            Assert.That(snapshot.State.Queue, Is.EqualTo(order));
            Assert.That(snapshot.State.Patients.All(p => p.TreatmentCount == 1), Is.True);
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Evening));
            Scenario.RejectWithoutMutation(session, new SelectPatient("a"), RejectionReason.WrongPhase);
        }

        [Test]
        public void SelectionLocksImmediatelyAndRejectsUnknownCompletedAndMidConversationTargets()
        {
            var session = new GameSession(FivePatientRules());
            Scenario.RejectWithoutMutation(session, new SelectPatient("a"), RejectionReason.WrongPhase);
            Scenario.EnterWork(session);
            Scenario.RejectWithoutMutation(session, new SelectPatient("unknown"), RejectionReason.PatientUnavailable);
            Scenario.Accept(session, new SelectPatient("a"));
            Scenario.RejectWithoutMutation(session, new SelectPatient("a"), RejectionReason.PatientUnavailable);
            Scenario.RejectWithoutMutation(session, new SelectPatient("b"), RejectionReason.PatientUnavailable);
            var current = session.GetPlayerView();
            Scenario.Accept(session, new SelectResponse(current.CurrentEncounterId!, current.CurrentNodeId!, "neutral"));
            Scenario.RejectWithoutMutation(session, new SelectPatient("b"), RejectionReason.PatientUnavailable);
            Finish(session, Disposition.Purify);
            Scenario.RejectWithoutMutation(session, new SelectPatient("a"), RejectionReason.PatientUnavailable);
            Assert.That(session.CanReceiveToday("b"), Is.True);
            Scenario.Accept(session, new SelectPatient("e"));
            Scenario.RejectWithoutMutation(session, new CompleteReaction(current.CurrentEncounterId!), RejectionReason.WrongEncounter);
        }

        [Test]
        public void LegacyConversationCannotBeReplacedAfterAReplyOrIntroduction()
        {
            var rules = FivePatientRules();
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            Scenario.Reply(session, "neutral");
            Scenario.RejectWithoutMutation(session, new SelectPatient("b"), RejectionReason.PatientUnavailable);

            var first = rules.Days[0].Meetings[0];
            var intro = new MeetingDefinition(first.PatientId, first.NormalRounds, first.SpecialRound,
                first.SpecialTrustThreshold, first.ReleaseTrustThreshold, first.ReactionNodes.ToDictionary(p => p.Key, p => p.Value), "intro");
            var introRules = new GameRules("free-selection-intro", rules.Patients,
                new[] { new DayDefinition(1, 0, new[] { intro }.Concat(rules.Days[0].Meetings.Skip(1))) }, rules.EndingPriority);
            session = new GameSession(introRules);
            Scenario.EnterWork(session);
            Scenario.Accept(session, new CompleteIntroduction(session.GetPlayerView().CurrentEncounterId!, "intro"));
            Scenario.RejectWithoutMutation(session, new SelectPatient("b"), RejectionReason.PatientUnavailable);
        }

        [Test]
        public void LaterDaysSkipRetiredPatientsAndKeepActualHistoryInChosenOrder()
        {
            var rules = FivePatientRules();
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            foreach (var id in new[] { "d", "b", "e", "a", "c" })
            {
                Scenario.Accept(session, new SelectPatient(id));
                Finish(session, id == "d" ? Disposition.Release : id == "b" ? Disposition.Purify : Disposition.Treat, "empathize");
            }
            Scenario.NextMorning(session);
            Scenario.EnterWork(session);
            Assert.That(session.CanReceiveToday("d"), Is.False);
            Assert.That(session.CanReceiveToday("b"), Is.False);
            Scenario.RejectWithoutMutation(session, new SelectPatient("d"), RejectionReason.PatientUnavailable);
            Assert.That(session.HasDispositionOtherThan("a", 1, Disposition.Release), Is.True);
            Assert.That(session.HasDispositionOtherThan("b", 1, Disposition.Purify), Is.False);
            Assert.That(session.HasResponse("d", 1, "empathize"), Is.True);
            foreach (var id in new[] { "c", "e", "a" })
            {
                Scenario.Accept(session, new SelectPatient(id));
                Finish(session, Disposition.Treat);
            }
            Scenario.NextMorning(session);
            Scenario.EnterWork(session);
            foreach (var id in new[] { "e", "a", "c" })
            {
                Scenario.Accept(session, new SelectPatient(id));
                Finish(session, Disposition.Treat);
            }
            Scenario.NextMorning(session);
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.TakenAway));
            var records = session.CreateSnapshot().State.Encounters;
            Assert.That(records.Count, Is.EqualTo(11));
            Assert.That(records.Select(e => e.Day + ":" + e.PatientId).Distinct().Count(), Is.EqualTo(records.Count));
            Assert.That(records.Where(e => e.Day == 2).Select(e => e.PatientId), Is.EqualTo(new[] { "c", "e", "a" }));
        }

        [Test]
        public void UnselectedReservationsAndLegacyJsonRemainCompatibleButSelectionTamperingFailsReplay()
        {
            var rules = FivePatientRules();
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            var oldShape = SnapshotCodec.Serialize(session);
            Assert.That(oldShape, Does.Not.Contain("ExplicitlySelected"));
            Assert.That(JObject.Parse(oldShape)["CommandHistory"]!.All(record => record["PatientId"] == null), Is.True);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(oldShape, rules)), Is.EqualTo(oldShape));
            var forged = JObject.Parse(oldShape);
            forged["State"]!["Encounters"]![0]!["ExplicitlySelected"] = true;
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(forged.ToString(), rules));
            Scenario.Accept(session, new SelectPatient("c"));
            var selected = SnapshotCodec.Serialize(session);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(selected, rules)), Is.EqualTo(selected));
            forged = JObject.Parse(selected);
            forged["CommandHistory"]!.Last!["PatientId"] = "a";
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(forged.ToString(), rules));
            Assert.That(PrototypeRules.Create().Fingerprint, Is.EqualTo("500e6dbf2fac9f7da1c6ed75fa596afc2b642e064a8800f791a03f66f8ef5706"));
        }

        private static void Finish(GameSession session, Disposition disposition, string response = "neutral")
        {
            Scenario.ReachDisposition(session, response);
            var id = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(id, disposition));
            Scenario.Accept(session, new CompleteReaction(id));
        }

        private static GameRules FivePatientRules()
        {
            var patients = new[] { "a", "b", "c", "d", "e" }.Select(id =>
                new PatientDefinition(id, id.ToUpperInvariant(), 1, 0, 3, PatientPresence.TreatmentEnded)).ToArray();
            return new GameRules("free-selection-fixture", patients,
                Enumerable.Range(1, 3).Select(day => new DayDefinition(day, 0, patients.Select(patient =>
                    PrototypeRules.CreateMeeting(patient.Id, day, normalRounds: 1, specialThreshold: int.MaxValue, releaseThreshold: 3)))),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }
    }
}
