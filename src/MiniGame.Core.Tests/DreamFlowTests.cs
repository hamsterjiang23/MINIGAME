using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class DreamFlowTests
    {
        [Test]
        public void DayZeroSkipsTheDreamAndFormalDaysDreamBeforeTheBedCanAdvance()
        {
            var session = new GameSession(Rules().WithDreamFlow(0, 100, 100, EmptyDeltas()));
            Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Evening));
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed));
            Scenario.Accept(session, new BeginNightDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
            Scenario.RejectWithoutMutation(session, new BeginNightDream());
            Scenario.RejectWithoutMutation(session, new CompleteDream());
            Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(1));
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.Evening);
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed));
            Scenario.Accept(session, new BeginNightDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Dream));
            Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed));
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(2));
        }

        [Test]
        public void RebellionBelongsToThePlayerAcrossPatientsDaysAndRestoreIndependentlyOfTrust()
        {
            var rules = Rules().WithDreamFlow(10, 100, 100, new Dictionary<string, int> { ["raise"] = 25, ["lower"] = -20 });
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.Work);
            Scenario.Reply(session, "raise");
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(35));
            Assert.That(session.CreateSnapshot().State.Patients.Single(item => item.Id == "first").Trust, Is.EqualTo(3));
            FinishPatient(session);
            Assert.That(session.GetPlayerView().CurrentPatientId, Is.EqualTo("second"));
            Scenario.Reply(session, "raise");
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(60));
            FinishPatient(session);
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(60), "Dispositions cannot add a second rebellion delta.");
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            DriveTo(session, view => view.Day == 2 && view.Phase == GamePhase.Work);
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(60));
            Scenario.Reply(session, "lower");
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(40));
            Assert.That(session.CreateSnapshot().State.Patients.Single(item => item.Id == "first").Trust, Is.EqualTo(1));
            var saved = SnapshotCodec.Serialize(session);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(saved, rules)), Is.EqualTo(saved));
        }

        [TestCase(int.MaxValue, 100)]
        [TestCase(int.MinValue, 0)]
        [TestCase(0, 50)]
        public void AcceptedResponseClampsSafelyAndRepeatedOrUnknownResponsesDoNotApplyAgain(int delta, int expected)
        {
            var rules = Rules().WithDreamFlow(50, 100, 100, new Dictionary<string, int> { ["raise"] = delta });
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.Work);
            var view = session.GetPlayerView();
            Scenario.RejectWithoutMutation(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, "unknown"));
            var selected = new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, "raise");
            Scenario.Accept(session, selected);
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(expected));
            Scenario.RejectWithoutMutation(session, selected);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(expected));
            Scenario.RejectWithoutMutation(session, selected);
        }

        [Test]
        public void MissingOptionDeltaDefaultsToZeroAndFailedTrustArithmeticDoesNotChangeRebellion()
        {
            var rules = Rules().WithDreamFlow(17, 100, 100, EmptyDeltas());
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 1 && view.Phase == GamePhase.Work);
            Scenario.Reply(session, "raise");
            Assert.That(session.GetPlayerView().Rebellion, Is.EqualTo(17));
            var overflowRules = Scenario.SinglePatient(initialTrust: int.MaxValue)
                .WithDreamFlow(10, 100, 100, new Dictionary<string, int> { ["empathize"] = 25 });
            var overflow = new GameSession(overflowRules);
            Scenario.EnterWork(overflow);
            var view = overflow.GetPlayerView();
            Scenario.RejectWithoutMutation(overflow, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, "empathize"),
                RejectionReason.NumericOverflow);
            Assert.That(overflow.GetPlayerView().Rebellion, Is.EqualTo(10));
        }

        [TestCase(false, false, 100, EndingType.DreamSea)]
        [TestCase(false, false, 99, EndingType.TakenAway)]
        [TestCase(true, false, 100, EndingType.PriestAttack)]
        [TestCase(false, true, 100, EndingType.LockedInRoom)]
        [TestCase(true, true, 100, EndingType.PriestAttack)]
        public void FinalDreamUsesTheSamePriestQuotaDreamThenTakenAwayPriority(bool attack, bool quotaFailure, int rebellion, EndingType expected)
        {
            var rules = Rules(attack, quotaFailure).WithDreamFlow(rebellion, 100, 100, EmptyDeltas());
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream, attack);
            var before = session.CreateSnapshot().State;
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(3), "Completing projection cannot commit day four early.");
            Assert.That(JToken.DeepEquals(JToken.FromObject(before.PendingEvents), JToken.FromObject(session.CreateSnapshot().State.PendingEvents)), Is.True,
                "Previewing priority cannot consume next-dawn events.");
            var saved = SnapshotCodec.Serialize(session);
            session = SnapshotCodec.Deserialize(saved, rules);
            if (expected == EndingType.DreamSea)
            {
                Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.SelfReview));
                Scenario.RejectWithoutMutation(session, new Interact(InteractionTarget.Bed));
                Scenario.RejectWithoutMutation(session, new SubmitDisposition("self", Disposition.Purify));
                Scenario.Accept(session, new ConfirmSelfPurification());
                var after = session.CreateSnapshot();
                Assert.That(JToken.DeepEquals(JToken.FromObject(before.Patients), JToken.FromObject(after.State.Patients)), Is.True);
                Assert.That(JToken.DeepEquals(JToken.FromObject(before.Days), JToken.FromObject(after.State.Days)), Is.True);
                Assert.That(after.State.Encounters.Count, Is.EqualTo(before.Encounters.Count));
                Assert.That(after.State.CurrentEncounterId, Is.Null);
                Assert.That(after.CommandHistory.Count(item => item.Kind == CommandKind.ConfirmSelfPurification), Is.EqualTo(1));
            }
            else
            {
                Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.AfterDream));
                Scenario.RejectWithoutMutation(session, new ConfirmSelfPurification());
                Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            }
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(expected));
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(4));
            Scenario.RejectWithoutMutation(session, new ConfirmSelfPurification(), RejectionReason.GameEnded);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules)),
                Is.EqualTo(SnapshotCodec.Serialize(session)));
        }

        [Test]
        public void RulesCloneIsOptInDeterministicAndProtectsLegacySaveIdentity()
        {
            var original = Rules();
            var originalSave = SnapshotCodec.Serialize(new GameSession(original));
            var deltas = new Dictionary<string, int> { ["raise"] = 5, ["lower"] = -2 };
            var first = original.WithDreamFlow(0, 100, 100, deltas);
            deltas["raise"] = 99;
            var second = original.WithDreamFlow(0, 100, 100, new Dictionary<string, int> { ["lower"] = -2, ["raise"] = 5 });
            Assert.That(first.DreamFlowEnabled, Is.True);
            Assert.That(first.RebellionOptionDeltas["raise"], Is.EqualTo(5));
            Assert.That(first.RulesId, Is.EqualTo(second.RulesId).And.Not.EqualTo(original.RulesId));
            Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint).And.Not.EqualTo(original.Fingerprint));
            Assert.That(first.WithDreamFlow(0, 100, 100, second.RebellionOptionDeltas).Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(original.DreamFlowEnabled, Is.False);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(originalSave, original)), Is.EqualTo(originalSave));
            Assert.That(JObject.Parse(originalSave)["State"]!["Rebellion"], Is.Null);
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(originalSave, first));
            Scenario.RejectWithoutMutation(new GameSession(original), new BeginNightDream());
            Assert.Throws<ArgumentException>(() => original.WithDreamFlow(0, 100, 100, new Dictionary<string, int> { ["typo"] = 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => original.WithDreamFlow(-1, 100, 100, EmptyDeltas()));
            Assert.Throws<ArgumentOutOfRangeException>(() => original.WithDreamFlow(0, 0, 100, EmptyDeltas()));
            Assert.Throws<ArgumentOutOfRangeException>(() => original.WithDreamFlow(0, 100, 101, EmptyDeltas()));
        }

        [Test]
        public void SelfPurificationDoesNotExecutePendingPatientTreatmentOrCreateADayFourRecord()
        {
            var rules = Scenario.SinglePatient(treatmentLimit: 3, delay: 1, normalRounds: 1)
                .WithDreamFlow(100, 100, 100, EmptyDeltas());
            var session = new GameSession(rules);
            DriveTo(session, view => view.Day == 3 && view.Phase == GamePhase.Dream);
            var before = session.CreateSnapshot().State;
            Assert.That(before.PendingEvents.Single().Kind, Is.EqualTo(ScheduledEventKind.TreatmentTermination));
            Assert.That(before.PendingEvents.Single().DueDay, Is.EqualTo(4));
            Scenario.Accept(session, new CompleteDream());
            Scenario.Accept(session, new ConfirmSelfPurification());
            var after = session.CreateSnapshot().State;
            Assert.That(after.Patients.Single().Presence, Is.EqualTo(PatientPresence.Active));
            Assert.That(JToken.DeepEquals(JToken.FromObject(before.Patients), JToken.FromObject(after.Patients)), Is.True);
            Assert.That(JToken.DeepEquals(JToken.FromObject(before.PendingEvents), JToken.FromObject(after.PendingEvents)), Is.True);
            Assert.That(after.Days.Any(day => day.Day == 4), Is.False);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules)),
                Is.EqualTo(SnapshotCodec.Serialize(session)));
        }

        [Test]
        public void RestoreRejectsTamperedRebellionAndIllegalSelfPurificationHistory()
        {
            var rules = Rules().WithDreamFlow(50, 100, 100, EmptyDeltas());
            var session = new GameSession(rules);
            var changed = JObject.Parse(SnapshotCodec.Serialize(session));
            changed["State"]!["Rebellion"] = 100;
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(changed.ToString(), rules));
            var snapshot = session.CreateSnapshot();
            snapshot.CommandHistory.Add(new CommandRecord { Kind = CommandKind.ConfirmSelfPurification });
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(snapshot, rules));
        }

        private static Dictionary<string, int> EmptyDeltas() => new Dictionary<string, int>();

        private static void FinishPatient(GameSession session)
        {
            Scenario.Accept(session, new SubmitDisposition(session.GetPlayerView().CurrentEncounterId!, Disposition.Treat));
            Scenario.Accept(session, new CompleteReaction(session.GetPlayerView().CurrentEncounterId!));
        }

        private static void DriveTo(GameSession session, Func<PlayerView, bool> stop, bool releasePriest = false)
        {
            for (int step = 0; step < 150; step++)
            {
                if (stop(session.GetPlayerView())) return;
                if (session.GetPlayerView().Phase == GamePhase.Evening) Scenario.Accept(session, new BeginNightDream());
                else Scenario.Step(session, (day, patient) => releasePriest && day == 3 && patient == "second" ? Disposition.Release : Disposition.Treat);
            }
            Assert.Fail("Dream-flow scenario did not reach its requested checkpoint.");
        }

        private static GameRules Rules(bool attack = false, bool quotaFailure = false)
        {
            var patients = new[]
            {
                new PatientDefinition("first", "第一位", 1, 0, null, PatientPresence.TreatmentEnded),
                new PatientDefinition("second", "第二位", 1, 0, null, PatientPresence.TreatmentEnded, attacksAfterRelease: attack)
            };
            MeetingDefinition Meeting(string patient, int day) => new MeetingDefinition(patient,
                new[] { new RoundDefinition(patient + "." + day, new[]
                {
                    new ResponseDefinition("raise", patient + ".raise", 3), new ResponseDefinition("lower", patient + ".lower", -2),
                    new ResponseDefinition("neutral", patient + ".neutral", 0)
                }) }, null, 100, 0, new Dictionary<Disposition, string>
                { [Disposition.Treat] = patient + ".treat", [Disposition.Purify] = patient + ".purify", [Disposition.Release] = patient + ".release" });
            return new GameRules("dream-flow-fixture", patients, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, quotaFailure && day == 3 ? 1 : 0,
                    day == 0 ? Array.Empty<MeetingDefinition>() : patients.Select(patient => Meeting(patient.Id, day)))),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }
    }
}
