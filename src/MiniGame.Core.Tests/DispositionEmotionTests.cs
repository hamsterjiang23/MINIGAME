using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    public sealed class DispositionEmotionTests
    {
        [TestCase(2, 100, false)]
        [TestCase(10, -100, true)]
        public void ReactionRestoreAndNextDayGatesUseOnlyDialogueTrust(int trust, int emotion, bool passes)
        {
            var rules = Rules(trust, emotion);
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            Scenario.ReachDisposition(session);
            var id = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(id, Disposition.Treat));
            var reading = session.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin;
            Assert.That(reading, Is.EqualTo(((double)trust + emotion - 3) / 3));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(trust));
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Assert.That(session.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin, Is.EqualTo(reading));
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(id, Disposition.Treat), RejectionReason.AlreadyDisposed);
            Scenario.Accept(session, new CompleteReaction(id));
            Scenario.NextMorning(session);
            Scenario.EnterWork(session);
            Assert.That(session.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin, Is.EqualTo((trust - 3d) / 3));
            Scenario.Reply(session, "neutral");
            Assert.That(session.GetPlayerView().EncounterStage,
                Is.EqualTo(passes ? EncounterStage.SpecialDialogue : EncounterStage.AwaitingDisposition));
            if (passes) Scenario.Reply(session, "neutral");
            Assert.That(session.GetPlayerView().AvailableDispositions.Contains(Disposition.Release), Is.EqualTo(passes));
        }

        [TestCase(9, 100, Disposition.Treat, GamePhase.AfterDream)]
        [TestCase(10, -100, Disposition.Treat, GamePhase.SelfReview)]
        [TestCase(9, 100, Disposition.Purify, GamePhase.AfterDream)]
        [TestCase(10, -100, Disposition.Purify, GamePhase.SelfReview)]
        public void FinalDreamIgnoresPositiveAndNegativeEmotionIncludingExitedHistory(
            int trust, int emotion, Disposition disposition, GamePhase expected)
        {
            var rules = Rules(trust, emotion).WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
            var session = new GameSession(rules);
            for (int step = 0; step < 200 && !(session.GetPlayerView().Day == 3
                && session.GetPlayerView().Phase == GamePhase.Dream); step++)
            {
                if (session.GetPlayerView().Phase == GamePhase.Evening)
                    Scenario.Accept(session, new BeginNightDream());
                else Scenario.Step(session, (day, patient) => disposition);
            }
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(GamePhase.Dream));
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(trust));
            Assert.That(session.GetPlayerView().DreamReturnPhase, Is.EqualTo(expected));
            Scenario.Accept(session, new CompleteDream());
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(expected));
        }

        private static GameRules Rules(int trust, int emotion)
        {
            var patient = new PatientDefinition(PrototypeRules.Child, "米洛", 1, trust, null, PatientPresence.Deceased);
            var reactions = new Dictionary<Disposition, string>
            {
                [Disposition.Treat] = "treat", [Disposition.Purify] = "purify", [Disposition.Release] = "release"
            };
            var round = new RoundDefinition("normal", new[] { new ResponseDefinition("neutral", "branch", 0) });
            var special = new RoundDefinition("special", new[] { new ResponseDefinition("neutral", "special.branch", 0) });
            var meeting = new MeetingDefinition(patient.Id, new[] { round }, special, 3, 3, reactions,
                dispositionTrustDeltas: new Dictionary<Disposition, int>
                {
                    [Disposition.Treat] = emotion, [Disposition.Purify] = emotion, [Disposition.Release] = emotion
                });
            return new GameRules("reaction-only-trust", new[] { patient },
                Enumerable.Range(1, 3).Select(day => new DayDefinition(day, 0, new[] { meeting })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }
    }
}
