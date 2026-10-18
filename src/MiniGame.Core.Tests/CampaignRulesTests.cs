using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class CampaignRulesTests
    {
        private const string TutorialPatient = "tutorial";
        private const string IntroNode = "tutorial.introduction";

        [Test]
        public void DayZeroStartsAtComputerAndRequiresIntroductionBeforeItsOnlyDisposition()
        {
            var rules = TutorialRules();
            var session = new GameSession(rules);
            Assert.That(rules.StartDay, Is.Zero);
            Assert.That(rules.EndDay, Is.EqualTo(3));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Apartment));
            Assert.That(session.GetPlayerView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Computer }));
            var started = Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            var view = session.GetPlayerView();
            Assert.That(view.EncounterStage, Is.EqualTo(EncounterStage.Introduction));
            Assert.That(view.CurrentNodeId, Is.EqualTo(IntroNode));
            Assert.That(view.AvailableResponseIds, Is.Empty);
            Assert.That(view.AvailableDispositions, Is.Empty);
            Assert.That(started.Events.Any(e => e.Kind == GameEventKind.IntroductionStarted && e.ReferenceId == IntroNode), Is.True);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Purify), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new SelectResponse(view.CurrentEncounterId!, IntroNode, "invented"), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new CompleteIntroduction("wrong", IntroNode), RejectionReason.WrongEncounter);
            Scenario.RejectWithoutMutation(session, new CompleteIntroduction(view.CurrentEncounterId!, "wrong"), RejectionReason.WrongNode);
            Scenario.Accept(session, new CompleteIntroduction(view.CurrentEncounterId!, IntroNode));
            Assert.That(session.GetPlayerView().AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            Scenario.RejectWithoutMutation(session, new CompleteIntroduction(view.CurrentEncounterId!, IntroNode), RejectionReason.WrongStage);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Treat), RejectionReason.DispositionUnavailable);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(view.CurrentEncounterId!, Disposition.Release), RejectionReason.DispositionUnavailable);
        }

        [Test]
        public void IntroductionCanPrecedeNormalDialogueWithoutConsumingItsFirstRound()
        {
            var original = Scenario.SinglePatient(normalRounds: 1);
            var normal = original.Days[0].Meetings[0];
            var meeting = new MeetingDefinition(normal.PatientId, normal.NormalRounds, null, 0, 0,
                normal.ReactionNodes.ToDictionary(p => p.Key, p => p.Value), introNodeId: "opening");
            var rules = new GameRules("intro-normal", original.Patients,
                new[] { new DayDefinition(1, 0, new[] { meeting }) }, original.EndingPriority);
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            var view = session.GetPlayerView();
            Scenario.Accept(session, new CompleteIntroduction(view.CurrentEncounterId!, "opening"));
            Assert.That(session.GetPlayerView().CurrentNodeId, Is.EqualTo(normal.NormalRounds[0].NodeId));
            Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.NormalDialogue));
            Scenario.Reply(session, "neutral");
            Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
            Assert.That(session.CreateSnapshot().State.Encounters.Single().Responses.Count, Is.EqualTo(1));
        }

        [Test]
        public void TutorialPurificationChangesTrustAndQuotaExactlyOnceThenTransitionsToDayOne()
        {
            var session = new GameSession(TutorialRules());
            Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            Scenario.ReachDisposition(session);
            var id = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(id, Disposition.Purify));
            Assert.That(session.CreateSnapshot().State.Patients.Single(p => p.Id == TutorialPatient).Trust, Is.EqualTo(-1));
            Assert.That(session.CreateSnapshot().State.Patients.Single(p => p.Id == TutorialPatient).Presence, Is.EqualTo(PatientPresence.Purified));
            Assert.That(session.CreateSnapshot().State.Days.Single().Purifications, Is.EqualTo(1));
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(id, Disposition.Purify), RejectionReason.AlreadyDisposed);
            Scenario.Accept(session, new CompleteReaction(id));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Evening));
            Scenario.NextMorning(session);
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(1));
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(session.GetPlayerView().RemainingPurificationQuota, Is.EqualTo(1));
            Assert.That(session.CreateSnapshot().State.Queue, Does.Not.Contain(TutorialPatient));
        }

        [TestCase("introduction")]
        [TestCase("disposition")]
        [TestCase("reaction")]
        [TestCase("day-one")]
        public void TutorialCheckpointsReplayWithoutRepeatingTrustOrDisposition(string checkpoint)
        {
            var rules = TutorialRules();
            var session = new GameSession(rules);
            Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            if (checkpoint != "introduction") Scenario.ReachDisposition(session);
            if (checkpoint == "reaction" || checkpoint == "day-one")
                Scenario.Accept(session, new SubmitDisposition(session.GetPlayerView().CurrentEncounterId!, Disposition.Purify));
            if (checkpoint == "day-one")
            {
                Scenario.Accept(session, new CompleteReaction(session.GetPlayerView().CurrentEncounterId!));
                Scenario.NextMorning(session);
            }
            var restored = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Assert.That(SnapshotCodec.Serialize(restored), Is.EqualTo(SnapshotCodec.Serialize(session)));
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, CampaignPolicy);
            Scenario.DriveUntil(restored, v => v.Phase == GamePhase.Ending, CampaignPolicy);
            Assert.That(SnapshotCodec.Serialize(restored), Is.EqualTo(SnapshotCodec.Serialize(session)));
            Assert.That(restored.CreateSnapshot().State.Patients.Single(p => p.Id == TutorialPatient).Trust, Is.EqualTo(-1));
            Assert.That(restored.CreateSnapshot().State.Days.Single(d => d.Day == 0).Purifications, Is.EqualTo(1));
        }

        [TestCase(int.MinValue, -1)]
        [TestCase(int.MaxValue, 1)]
        public void DispositionTrustOverflowRejectsBeforeAnySideEffect(int initialTrust, int delta)
        {
            var session = new GameSession(TutorialRules(initialTrust: initialTrust, delta: delta));
            Scenario.Accept(session, new Interact(InteractionTarget.Computer));
            Scenario.ReachDisposition(session);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(session.GetPlayerView().CurrentEncounterId!, Disposition.Purify),
                RejectionReason.NumericOverflow);
        }

        [Test]
        public void FailedTutorialQuotaIsRecordedButExcludedFromFormalCampaignEnding()
        {
            var session = new GameSession(TutorialRules(quota: 2));
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, CampaignPolicy);
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(4));
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.TakenAway));
            var days = session.CreateSnapshot().State.Days;
            Assert.That(days.Select(d => d.Day), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(days.Single(d => d.Day == 0).QuotaMet, Is.False);
            Assert.That(days.Where(d => d.Day > 0).All(d => d.QuotaMet == true), Is.True);
        }

        [TestCase(false, false, EndingType.TakenAway)]
        [TestCase(true, false, EndingType.LockedInRoom)]
        [TestCase(false, true, EndingType.PriestAttack)]
        [TestCase(true, true, EndingType.PriestAttack)]
        public void DayZeroCalendarKeepsFinalDayKpiAndPriestPriority(bool missQuota, bool releasePriest, EndingType expected)
        {
            var session = new GameSession(TutorialRules());
            Disposition Policy(int day, string patient)
            {
                if (patient == TutorialPatient) return Disposition.Purify;
                if (releasePriest && patient == PrototypeRules.Priest && day == 3) return Disposition.Release;
                if (missQuota && day == 1) return Disposition.Treat;
                return Scenario.NormalPolicy(day, patient);
            }
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, Policy, "empathize");
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(4));
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(expected));
            Assert.That(session.CreateSnapshot().State.Days.Count, Is.EqualTo(4));
        }

        [Test]
        public void HistoricalFactsOnlyReportAcceptedChoicesAndActualOtherPatientDispositions()
        {
            var session = new GameSession(TutorialRules());
            Assert.That(session.HasDispositionOtherThan(PrototypeRules.Child, 0, Disposition.Purify), Is.False);
            Scenario.DriveUntil(session, v => v.Day == 1 && v.EncounterStage == EncounterStage.NormalDialogue, CampaignPolicy);
            Assert.That(session.HasDispositionOtherThan(PrototypeRules.Child, 0, Disposition.Purify), Is.True);
            Assert.That(session.HasDispositionOtherThan(TutorialPatient, 0, Disposition.Purify), Is.False);
            Assert.That(session.HasDispositionOtherThan(PrototypeRules.Child, 1, Disposition.Purify), Is.False);
            Assert.That(session.HasResponse(PrototypeRules.Child, 1, "neutral"), Is.False);
            var view = session.GetPlayerView();
            Scenario.RejectWithoutMutation(session, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, "invented"));
            Assert.That(session.HasResponse(PrototypeRules.Child, 1, "invented"), Is.False);
            Scenario.Reply(session, "neutral");
            Assert.That(session.HasResponse(PrototypeRules.Child, 1, "neutral"), Is.True);
            Assert.That(session.HasResponse(PrototypeRules.Child, 2, "neutral"), Is.False);
            Assert.That(session.HasResponse(PrototypeRules.Doctor, 1, "neutral"), Is.False);
        }

        [Test]
        public void LegacyFingerprintStaysCompatibleWhileIntroductionAndDispositionDeltasAreFingerprintInputs()
        {
            // Observed from the pre-campaign compiled Core before adding optional extensions.
            Assert.That(PrototypeRules.Create().Fingerprint,
                Is.EqualTo("500e6dbf2fac9f7da1c6ed75fa596afc2b642e064a8800f791a03f66f8ef5706"));
            var rules = TutorialRules();
            var changedDelta = TutorialRules(delta: -3);
            var changedIntro = TutorialRules(intro: "different.introduction");
            Assert.That(changedDelta.Fingerprint, Is.Not.EqualTo(rules.Fingerprint));
            Assert.That(changedIntro.Fingerprint, Is.Not.EqualTo(rules.Fingerprint));
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(SnapshotCodec.Serialize(new GameSession(rules)), changedDelta));
        }

        [Test]
        public void InvalidTutorialDefinitionsAreRejectedBeforeStartingASession()
        {
            var reactions = new Dictionary<Disposition, string> { [Disposition.Purify] = "reaction" };
            Assert.Throws<ArgumentException>(() => new MeetingDefinition(TutorialPatient, Array.Empty<RoundDefinition>(),
                null, 0, 0, reactions));
            Assert.Throws<ArgumentException>(() => new MeetingDefinition(TutorialPatient, Array.Empty<RoundDefinition>(),
                null, 0, 0, reactions, introNodeId: " "));
            Assert.Throws<ArgumentException>(() => new MeetingDefinition(TutorialPatient, Array.Empty<RoundDefinition>(),
                null, 0, 0, new Dictionary<Disposition, string>(), introNodeId: IntroNode));
            Assert.Throws<ArgumentException>(() => new MeetingDefinition(TutorialPatient, Array.Empty<RoundDefinition>(),
                null, 0, 0, new Dictionary<Disposition, string> { [(Disposition)99] = "reaction" }, introNodeId: IntroNode));
            Assert.Throws<ArgumentException>(() => new MeetingDefinition(TutorialPatient, Array.Empty<RoundDefinition>(),
                null, 0, 0, reactions, IntroNode, new Dictionary<Disposition, int> { [Disposition.Treat] = -1 }));
            var rules = TutorialRules();
            Assert.Throws<ArgumentException>(() => new GameRules("missing-day-one", rules.Patients,
                rules.Days.Where(d => d.Day != 1), rules.EndingPriority));
            Assert.Throws<ArgumentException>(() => new GameRules("starts-too-late", rules.Patients,
                rules.Days.Where(d => d.Day >= 2), rules.EndingPriority));
        }

        [Test]
        public void DispositionTrustConfigurationIsOwnedAndNormalizedForSaveCompatibility()
        {
            var original = Scenario.SinglePatient();
            var source = original.Days[0].Meetings[0];
            var reactions = source.ReactionNodes.ToDictionary(p => p.Key, p => p.Value);
            var deltas = new Dictionary<Disposition, int> { [Disposition.Treat] = 1, [Disposition.Purify] = -1 };
            var meeting = new MeetingDefinition(source.PatientId, source.NormalRounds, null, 0, 3,
                reactions, dispositionTrustDeltas: deltas);
            var reversed = new MeetingDefinition(source.PatientId, source.NormalRounds, null, 0, 3,
                reactions, dispositionTrustDeltas: deltas.Reverse().ToDictionary(p => p.Key, p => p.Value));
            deltas.Clear();
            Assert.That(meeting.DispositionTrustDeltas[Disposition.Purify], Is.EqualTo(-1));
            GameRules Wrap(MeetingDefinition value) => new GameRules("owned-config", original.Patients,
                new[] { new DayDefinition(1, 0, new[] { value }) }, original.EndingPriority);
            Assert.That(Wrap(meeting).Fingerprint, Is.EqualTo(Wrap(reversed).Fingerprint));
        }

        private static Disposition CampaignPolicy(int day, string patient)
            => patient == TutorialPatient ? Disposition.Purify : Scenario.NormalPolicy(day, patient);

        private static GameRules TutorialRules(int initialTrust = 0, int delta = -1, int quota = 1, string intro = IntroNode)
        {
            var original = PrototypeRules.Create();
            var patient = new PatientDefinition(TutorialPatient, "无名对象", 0, initialTrust, null, PatientPresence.TreatmentEnded);
            var meeting = new MeetingDefinition(patient.Id, Array.Empty<RoundDefinition>(), null, int.MaxValue, 3,
                new Dictionary<Disposition, string> { [Disposition.Purify] = "tutorial.purify" }, intro,
                new Dictionary<Disposition, int> { [Disposition.Purify] = delta });
            return new GameRules("campaign-fixture", new[] { patient }.Concat(original.Patients),
                new[] { new DayDefinition(0, quota, new[] { meeting }) }.Concat(original.Days), original.EndingPriority);
        }
    }
}
