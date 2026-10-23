using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class GmEndingTests
    {
        [TestCase(EndingType.TakenAway)]
        [TestCase(EndingType.LockedInRoom)]
        [TestCase(EndingType.PriestAttack)]
        [TestCase(EndingType.DreamSea)]
        public void GmRoutesReplayWithoutForgingDayOrPatientHistoryAndCanReplaceAnEnding(EndingType ending)
        {
            var rules = PrototypeRules.Create().WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
            var session = new GameSession(rules);
            Scenario.EnterWork(session);
            var before = JObject.Parse(SnapshotCodec.Serialize(session));
            Scenario.Accept(session, new EnterGmEnding(EndingType.TakenAway));
            Scenario.RejectWithoutMutation(session, new EnterHiddenEndingCheat(), RejectionReason.GameEnded);
            Scenario.RejectWithoutMutation(session, new CompleteDream(), RejectionReason.GameEnded);
            Scenario.Accept(session, new EnterGmEnding(ending));
            var view = session.GetPlayerView();
            Assert.That(view.Phase, Is.EqualTo(ending == EndingType.DreamSea ? GamePhase.SelfReview : GamePhase.Ending));
            Assert.That(view.Ending, Is.EqualTo(ending == EndingType.DreamSea ? EndingType.None : ending));
            Assert.That(view.SelfReviewLineIndex, Is.Zero);
            Assert.That(view.CurrentEncounterId, Is.Null);
            var after = JObject.Parse(SnapshotCodec.Serialize(session));
            foreach (string key in new[] { "Day", "Patients", "Days", "Encounters", "PendingEvents", "ExecutedEventIds", "Rebellion" })
                Assert.That(JToken.DeepEquals(before["State"]![key], after["State"]![key]), Is.True, key);
            Assert.That(session.CreateSnapshot().CommandHistory.Last().Ending, Is.EqualTo(ending));
            string saved = SnapshotCodec.Serialize(session);
            Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(saved, rules)), Is.EqualTo(saved));
        }

        [Test]
        public void InvalidGmRoutesAndMalformedHistoryAreRejectedWithoutMutation()
        {
            var rules = PrototypeRules.Create().WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
            var session = new GameSession(rules);
            Scenario.RejectWithoutMutation(session, new EnterGmEnding(EndingType.None), RejectionReason.InvalidCommand);
            Scenario.RejectWithoutMutation(session, new EnterGmEnding((EndingType)999), RejectionReason.InvalidCommand);
            Scenario.RejectWithoutMutation(new GameSession(PrototypeRules.Create()), new EnterGmEnding(EndingType.TakenAway), RejectionReason.WrongPhase);
            Scenario.Accept(session, new EnterGmEnding(EndingType.TakenAway));
            foreach (bool missingEnding in new[] { false, true })
            {
                var snapshot = session.CreateSnapshot();
                if (missingEnding) snapshot.CommandHistory.Last().Ending = null;
                else snapshot.CommandHistory.Last().Target = InteractionTarget.Bed;
                Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(snapshot, rules));
            }
        }
    }
}
