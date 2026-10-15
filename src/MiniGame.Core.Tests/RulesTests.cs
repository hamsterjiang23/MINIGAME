using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class RulesTests
    {
        [Test]
        public void FiveNormalRoundsAreSupportedWithoutChangingTheSession()
        {
            var session = new GameSession(Scenario.SinglePatient(normalRounds: 5));
            Scenario.EnterWork(session); Scenario.ReachDisposition(session);
            Assert.That(session.CreateSnapshot().State.Encounters.Single().Responses.Count, Is.EqualTo(5));
        }

        [Test]
        public void MissingSpecialRoundGoesDirectlyToDispositionEvenWithHighTrust()
        {
            var original = Scenario.SinglePatient(initialTrust: 100);
            var day = original.Days[0]; var meeting = day.Meetings[0];
            var withoutSpecial = new MeetingDefinition(meeting.PatientId, meeting.NormalRounds, null, 20, 30,
                meeting.ReactionNodes.ToDictionary(p => p.Key, p => p.Value));
            var rules = new GameRules("no-special", original.Patients,
                new[] { new DayDefinition(1, 0, new[] { withoutSpecial }) }, original.EndingPriority);
            var session = new GameSession(rules); Scenario.EnterWork(session); Scenario.ReachDisposition(session);
            Assert.That(session.CreateSnapshot().State.Encounters.Single().SpecialStarted, Is.False);
            Assert.That(session.GetPlayerView().AvailableDispositions, Does.Contain(Disposition.Release));
        }

        [TestCase(0)]
        [TestCase(5)]
        public void InvalidResponseCountsAreRejected(int count)
            => Assert.Throws<ArgumentException>(() => new RoundDefinition("round", Enumerable.Range(0, count)
                .Select(i => new ResponseDefinition("r" + i, "branch" + i, 0))));

        [TestCase(0)]
        public void InvalidNormalRoundCountsAreRejected(int count)
            => Assert.Throws<ArgumentException>(() => PrototypeRules.CreateMeeting("patient", 1, count));

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(6)]
        public void RoundCountComesFromContentInsteadOfThePrototypeFourToFiveRoundConvention(int count)
        {
            var session = new GameSession(Scenario.SinglePatient(normalRounds: count));
            Scenario.EnterWork(session);
            Scenario.ReachDisposition(session);
            Assert.That(session.CreateSnapshot().State.Encounters.Single().Responses.Count, Is.EqualTo(count));
        }

        [Test]
        public void DuplicateResponseAndDialogueNodeIdsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new RoundDefinition("n", new[]
            { new ResponseDefinition("same", "b1", 0), new ResponseDefinition("same", "b2", 10) }));
            var original = PrototypeRules.CreateMeeting("patient", 1);
            Assert.Throws<ArgumentException>(() => new MeetingDefinition("patient",
                Enumerable.Repeat(original.NormalRounds[0], 4), original.SpecialRound, 20, 30,
                original.ReactionNodes.ToDictionary(p => p.Key, p => p.Value)));
        }

        [TestCase("duplicate-patient")]
        [TestCase("unknown-patient")]
        [TestCase("duplicate-day")]
        [TestCase("early-priest")]
        [TestCase("negative-quota")]
        [TestCase("duplicate-ending")]
        public void InvalidRuleSetsAreRejectedAtConstruction(string fault)
        {
            var original = PrototypeRules.Create();
            Assert.Catch<ArgumentException>(() =>
            {
                switch (fault)
                {
                    case "duplicate-patient":
                        _ = new GameRules("bad", original.Patients.Concat(new[] { original.Patients[0] }), original.Days, original.EndingPriority); break;
                    case "unknown-patient":
                        _ = new GameRules("bad", original.Patients,
                            new[] { new DayDefinition(1, 1, new[] { PrototypeRules.CreateMeeting("unknown", 1) }), original.Days[1], original.Days[2] }, original.EndingPriority); break;
                    case "duplicate-day":
                        _ = new GameRules("bad", original.Patients, new[] { original.Days[0], original.Days[0] }, original.EndingPriority); break;
                    case "early-priest":
                        _ = new GameRules("bad", original.Patients,
                            new[] { new DayDefinition(1, 1, original.Days[0].Meetings.Concat(new[] { PrototypeRules.CreateMeeting(PrototypeRules.Priest, 1) })), original.Days[1], original.Days[2] }, original.EndingPriority); break;
                    case "negative-quota": _ = new DayDefinition(1, -1, original.Days[0].Meetings); break;
                    case "duplicate-ending":
                        _ = new GameRules("bad", original.Patients, original.Days,
                            new[] { EndingType.PriestAttack, EndingType.PriestAttack, EndingType.TakenAway }); break;
                }
            });
        }

        [Test]
        public void RuleCollectionsAreCopiedAndReactionOrderingDoesNotAffectFingerprint()
        {
            var original = Scenario.SinglePatient();
            var meeting = original.Days[0].Meetings[0];
            var reactions = meeting.ReactionNodes.Reverse().ToDictionary(p => p.Key, p => p.Value);
            var copiedMeeting = new MeetingDefinition(meeting.PatientId, meeting.NormalRounds, meeting.SpecialRound,
                meeting.SpecialTrustThreshold, meeting.ReleaseTrustThreshold, reactions);
            reactions.Clear();
            Assert.That(copiedMeeting.ReactionNodes.Count, Is.EqualTo(3));
            var profiles = original.Patients.ToList();
            var days = original.Days.Select(d => d.Day == 1 ? new DayDefinition(1, d.PurificationQuota, new[] { copiedMeeting }) : d).ToList();
            var copied = new GameRules(original.RulesId, profiles, days, original.EndingPriority);
            profiles.Clear(); days.Clear();
            Assert.That(copied.Patients.Count, Is.EqualTo(1));
            Assert.That(copied.Fingerprint, Is.EqualTo(original.Fingerprint));
        }
    }
}
