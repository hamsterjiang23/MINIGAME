using System;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class PurificationProgressTests
    {
        [Test]
        public void DailyProgressRetainsActualCountBeyondRequirementAndSurvivesRestoreBeforeResetting()
        {
            var profiles = new[]
            {
                new PatientDefinition("first", "对象甲", 1, 0, null, PatientPresence.TreatmentEnded),
                new PatientDefinition("second", "对象乙", 1, 0, null, PatientPresence.TreatmentEnded)
            };
            var rules = new GameRules("purification-progress-fixture", profiles,
                new[] { new DayDefinition(1, 1, profiles.Select(patient => PrototypeRules.CreateMeeting(patient.Id, 1, 1))),
                    new DayDefinition(2, 2, profiles.Select(patient => PrototypeRules.CreateMeeting(patient.Id, 2, 1))) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var session = new GameSession(rules);
            AssertProgress(session, 0, 1);
            Scenario.EnterWork(session);
            Scenario.ReachDisposition(session);
            string first = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(first, Disposition.Purify));
            AssertProgress(session, 1, 1);
            Scenario.RejectWithoutMutation(session, new SubmitDisposition(first, Disposition.Purify), RejectionReason.AlreadyDisposed);
            AssertProgress(session, 1, 1);

            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            AssertProgress(session, 1, 1);
            Scenario.Accept(session, new CompleteReaction(first));
            Scenario.ReachDisposition(session);
            string second = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(second, Disposition.Purify));
            AssertProgress(session, 2, 1);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            AssertProgress(session, 2, 1);
            Scenario.Accept(session, new CompleteReaction(second));
            Scenario.NextMorning(session);
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(2));
            AssertProgress(session, 0, 2);
        }

        private static void AssertProgress(GameSession session, int actual, int requirement)
        {
            var view = session.GetPlayerView();
            Assert.That(view.PurificationsToday, Is.EqualTo(actual));
            Assert.That(view.PurificationRequirement, Is.EqualTo(requirement));
            Assert.That(view.RemainingPurificationQuota, Is.EqualTo(Math.Max(0, requirement - actual)));
        }
    }
}
