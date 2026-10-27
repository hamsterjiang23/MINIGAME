using System;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    public sealed class SocialAdaptationTests
    {
        [Test]
        public void InstrumentFollowsAcceptedRepliesAndRestoreWithoutChangingReleaseRules()
        {
            var rules = Scenario.SinglePatient();
            var session = new GameSession(rules);
            Assert.That(session.GetPlayerView().SocialAdaptation, Is.Null);
            Scenario.EnterWork(session);
            Assert.That(session.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin, Is.EqualTo(-1));
            Scenario.Reply(session, "pressure");
            var low = session.GetPlayerView().SocialAdaptation!;
            Assert.That(low.RelativeReleaseMargin, Is.LessThan(-1));
            Assert.That(low.MeetsReleaseThreshold, Is.False);
            Scenario.RejectWithoutMutation(session, new SelectResponse("stale", "stale", "empathize"));
            Assert.That(session.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin, Is.EqualTo(low.RelativeReleaseMargin));
            var restored = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Assert.That(restored.GetPlayerView().SocialAdaptation!.RelativeReleaseMargin, Is.EqualTo(low.RelativeReleaseMargin));
            Scenario.ReachDisposition(session, "empathize");
            var view = session.GetPlayerView();
            Assert.That(view.AvailableDispositions.Contains(Disposition.Release), Is.EqualTo(view.SocialAdaptation!.MeetsReleaseThreshold));
        }

        [TestCase(29, 30, false)]
        [TestCase(30, 30, true)]
        [TestCase(-4, -3, false)]
        [TestCase(-3, -3, true)]
        [TestCase(-1, 0, false)]
        [TestCase(0, 0, true)]
        [TestCase(int.MinValue, int.MaxValue, false)]
        [TestCase(int.MaxValue, int.MinValue, true)]
        public void ReadingUsesTheActualSignedMeetingThresholdWithoutOverflow(int trust, int threshold, bool meets)
        {
            var original = Scenario.SinglePatient(initialTrust: trust, normalRounds: 1);
            var meeting = original.Days.Single(d => d.Day == 1).Meetings.Single();
            var adjusted = new MeetingDefinition(meeting.PatientId, meeting.NormalRounds, null, int.MaxValue,
                threshold, meeting.ReactionNodes.ToDictionary(pair => pair.Key, pair => pair.Value));
            var rules = new GameRules("instrument-threshold", original.Patients,
                new[] { new DayDefinition(1, 0, new[] { adjusted }) }, original.EndingPriority);
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            var reading = session.GetPlayerView().SocialAdaptation!;
            Assert.That(double.IsNaN(reading.RelativeReleaseMargin) || double.IsInfinity(reading.RelativeReleaseMargin), Is.False);
            Assert.That(reading.MeetsReleaseThreshold, Is.EqualTo(meets));
            // The reading must not grant early disposition while dialogue is still running.
            Assert.That(session.GetPlayerView().AvailableDispositions, Is.Empty);
        }
    }
}
