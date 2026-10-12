using System;
using System.Linq;
using MiniGame.Core;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class EndingTests
    {
        [Test]
        public void ThreeCompletedDaysWithAllQuotasMetEndInTakenAwayOnDayFour()
        {
            var session = new GameSession(PrototypeRules.Create());
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending);
            var state = session.CreateSnapshot().State;
            Assert.That(state.Day, Is.EqualTo(4));
            Assert.That(state.Ending, Is.EqualTo(EndingType.TakenAway));
            Assert.That(state.Days.Select(d => d.Day), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(state.Days.All(d => d.Completed && d.QuotaMet == true), Is.True);
            Assert.That(state.Encounters.Any(e => e.PatientId == PrototypeRules.Priest), Is.True);
            Assert.That(state.Days.Any(d => d.Day == 4), Is.False);
            Scenario.RejectWithoutMutation(session, new CompleteMorningInspection(), RejectionReason.GameEnded);
        }

        [Test]
        public void AnEarlierQuotaFailureIsRememberedEvenWhenLaterDaysMeetQuota()
        {
            var session = new GameSession(PrototypeRules.Create());
            Disposition Policy(int day, string id) => day == 1 ? Disposition.Treat : Scenario.NormalPolicy(day, id);
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, Policy);
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(4));
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.LockedInRoom));
            Assert.That(session.CreateSnapshot().State.Days.Select(d => d.QuotaMet), Is.EqualTo(new bool?[] { false, true, true }));
        }

        [Test]
        public void PriestReleasedOnDayTwoAttacksAtTheNextMorning()
        {
            var session = new GameSession(PrototypeRules.Create());
            Disposition Policy(int day, string id) => id == PrototypeRules.Priest ? Disposition.Release : Scenario.NormalPolicy(day, id);
            Scenario.DriveUntil(session, v => v.Day == 2 && v.Phase == GamePhase.AfterDream, Policy, "empathize");
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.None));
            var result = Scenario.Accept(session, new Interact(InteractionTarget.Bed));
            Assert.That(session.GetPlayerView().Day, Is.EqualTo(3));
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(result.Events.Count(e => e.Kind == GameEventKind.GameEnded), Is.EqualTo(1));
            Assert.That(result.Events.Count(e => e.Kind == GameEventKind.ScheduledEventExecuted), Is.EqualTo(1));
            Assert.That(session.CreateSnapshot().State.Days.Count, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DayThreePriestReleaseTakesPriorityOverNormalAndQuotaFailureEndings(bool missQuota)
        {
            var session = new GameSession(PrototypeRules.Create());
            Disposition Policy(int day, string id)
            {
                if (id == PrototypeRules.Priest && day == 3) return Disposition.Release;
                if (missQuota && day == 1) return Disposition.Treat;
                return Scenario.NormalPolicy(day, id);
            }
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, Policy, "empathize");
            var state = session.CreateSnapshot().State;
            Assert.That(state.Day, Is.EqualTo(4));
            Assert.That(state.Days.Count, Is.EqualTo(3));
            Assert.That(state.Ending, Is.EqualTo(EndingType.PriestAttack));
            Assert.That(state.PendingEvents, Is.Empty);
            Assert.That(state.ExecutedEventIds.Distinct().Count(), Is.EqualTo(state.ExecutedEventIds.Count));
            if (missQuota) Assert.That(state.Days.First().QuotaMet, Is.False);
        }

        [Test]
        public void CustomEndingPriorityIsUsedInsteadOfBeingHardCodedInTheSession()
        {
            var original = PrototypeRules.Create();
            var rules = new GameRules("different-priority", original.Patients, original.Days,
                new[] { EndingType.TakenAway, EndingType.PriestAttack, EndingType.LockedInRoom });
            var session = new GameSession(rules);
            Disposition Policy(int day, string id) => id == PrototypeRules.Priest && day == 3
                ? Disposition.Release : Scenario.NormalPolicy(day, id);
            Scenario.DriveUntil(session, v => v.Phase == GamePhase.Ending, Policy, "empathize");
            Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.TakenAway));
        }
    }
}
