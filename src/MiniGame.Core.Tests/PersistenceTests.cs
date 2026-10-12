using System;
using System.IO;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace MiniGame.Core.Tests
{
    [TestFixture]
    public sealed class PersistenceTests
    {
        [TestCase("response")]
        [TestCase("special")]
        [TestCase("disposition")]
        [TestCase("reaction")]
        [TestCase("before-day-change")]
        public void RestoredCheckpointsContinueToExactlyTheSameFinalState(string checkpoint)
        {
            var rules = PrototypeRules.Create(); var original = new GameSession(rules);
            Scenario.EnterWork(original);
            if (checkpoint == "response")
            {
                var view = original.GetPlayerView();
                Scenario.Accept(original, new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, "empathize"));
            }
            else if (checkpoint == "special")
                for (var i = 0; i < 4; i++) Scenario.Reply(original, "empathize");
            else if (checkpoint == "disposition" || checkpoint == "reaction")
            {
                Scenario.ReachDisposition(original, "empathize");
                if (checkpoint == "reaction")
                    Scenario.Accept(original, new SubmitDisposition(original.GetPlayerView().CurrentEncounterId!, Disposition.Purify));
            }
            else Scenario.DriveUntil(original, v => v.Phase == GamePhase.AfterDream);

            var restored = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(original), rules);
            Assert.That(SnapshotCodec.Serialize(restored), Is.EqualTo(SnapshotCodec.Serialize(original)));
            Scenario.DriveUntil(original, v => v.Phase == GamePhase.Ending);
            Scenario.DriveUntil(restored, v => v.Phase == GamePhase.Ending);
            Assert.That(SnapshotCodec.Serialize(restored), Is.EqualTo(SnapshotCodec.Serialize(original)));
            Assert.That(restored.GetPlayerView().Ending, Is.EqualTo(EndingType.TakenAway));
        }

        [Test]
        public void RestoringReactionDoesNotPublishOrResubmitTheCommittedDisposition()
        {
            var rules = PrototypeRules.Create(); var session = new GameSession(rules);
            Scenario.EnterWork(session); Scenario.ReachDisposition(session);
            var id = session.GetPlayerView().CurrentEncounterId!;
            Scenario.Accept(session, new SubmitDisposition(id, Disposition.Purify));
            var restored = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Scenario.RejectWithoutMutation(restored, new SubmitDisposition(id, Disposition.Purify), RejectionReason.AlreadyDisposed);
            var result = Scenario.Accept(restored, new CompleteReaction(id));
            Assert.That(result.Events.Any(e => e.Kind == GameEventKind.DispositionCommitted), Is.False);
            Assert.That(restored.CreateSnapshot().State.Days.Single().Purifications, Is.EqualTo(1));
        }

        [Test]
        public void SpecialDialogueAndExecutedDawnEventsRemainOneTimeAfterRestore()
        {
            var rules = PrototypeRules.Create(); var session = new GameSession(rules);
            Disposition Policy(int day, string id) => id == PrototypeRules.Priest ? Disposition.Release : Scenario.NormalPolicy(day, id);
            Scenario.DriveUntil(session, v => v.Day == 2 && v.Phase == GamePhase.AfterDream, Policy, "empathize");
            var restored = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), rules);
            Scenario.Accept(restored, new Interact(InteractionTarget.Bed));
            Assert.That(restored.GetPlayerView().Ending, Is.EqualTo(EndingType.PriestAttack));
            var finished = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(restored), rules);
            var state = finished.CreateSnapshot().State;
            Assert.That(state.ExecutedEventIds.Count, Is.EqualTo(1));
            Assert.That(state.Encounters.Where(e => e.SpecialStarted).All(e => e.SpecialCompleted), Is.True);
            Scenario.RejectWithoutMutation(finished, new Interact(InteractionTarget.Bed), RejectionReason.GameEnded);
            Assert.That(finished.CreateSnapshot().State.ExecutedEventIds.Count, Is.EqualTo(1));
        }

        [Test]
        public void SnapshotCaptureAndRestoreOwnTheirDataInsteadOfSharingMutableDtoLists()
        {
            var rules = PrototypeRules.Create(); var session = new GameSession(rules);
            var snapshot = session.CreateSnapshot(); snapshot.State.Patients[0].Trust = 999;
            Assert.That(session.CreateSnapshot().State.Patients[0].Trust, Is.Zero);
            snapshot = session.CreateSnapshot();
            var restored = GameSession.Restore(snapshot, rules);
            snapshot.State.Queue.Clear(); snapshot.State.Day = 99;
            Assert.That(restored.GetPlayerView().Day, Is.EqualTo(1));
            Assert.That(restored.CreateSnapshot().State.Queue.Count, Is.EqualTo(4));
        }

        [TestCase("schema")]
        [TestCase("rules-id")]
        [TestCase("fingerprint")]
        [TestCase("trust")]
        [TestCase("queue")]
        [TestCase("phase")]
        [TestCase("quota")]
        [TestCase("event-ledger")]
        [TestCase("missing-history")]
        [TestCase("duplicate-command")]
        [TestCase("command-arguments")]
        [TestCase("null-command")]
        public void InconsistentOrUnsupportedSnapshotsAreRejected(string corruption)
        {
            var rules = PrototypeRules.Create(); var session = new GameSession(rules);
            Scenario.EnterWork(session); Scenario.Reply(session, "empathize");
            var snapshot = session.CreateSnapshot();
            switch (corruption)
            {
                case "schema": snapshot.SchemaVersion++; break;
                case "rules-id": snapshot.RulesId = "unrelated"; break;
                case "fingerprint": snapshot.RulesFingerprint = "wrong"; break;
                case "trust": snapshot.State.Patients[0].Trust++; break;
                case "queue": snapshot.State.Queue.Reverse(); break;
                case "phase": snapshot.State.Phase = GamePhase.AfterDream; break;
                case "quota": snapshot.State.Days[0].Purifications++; break;
                case "event-ledger": snapshot.State.ExecutedEventIds.Add("invented-event"); break;
                case "missing-history": snapshot.CommandHistory.RemoveAt(0); break;
                case "duplicate-command": snapshot.CommandHistory.Add(snapshot.CommandHistory[0]); break;
                case "command-arguments": snapshot.CommandHistory[0].ResponseId = "unexpected"; break;
                case "null-command": snapshot.CommandHistory.Add(null!); break;
            }
            var before = SnapshotCodec.Serialize(session);
            Assert.Throws<SnapshotValidationException>(() => GameSession.Restore(snapshot, rules));
            Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(before));
        }

        [TestCase("")]
        [TestCase("{")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{} {}")]
        public void MalformedOrIncompleteJsonIsRejected(string json)
            => Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(json, PrototypeRules.Create()));

        [TestCase("unknown-field")]
        [TestCase("invalid-enum")]
        [TestCase("missing-required-field")]
        public void JsonShapeIsCheckedBeforeRestore(string corruption)
        {
            var rules = PrototypeRules.Create();
            var json = JObject.Parse(SnapshotCodec.Serialize(new GameSession(rules)));
            if (corruption == "unknown-field") json["unexpected"] = true;
            if (corruption == "invalid-enum") json["State"]!["Phase"] = "NotAGamePhase";
            if (corruption == "missing-required-field") json.Remove("CommandHistory");
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(json.ToString(), rules));
        }

        [Test]
        public void AChangedRuleWithTheSameNameDoesNotAcceptAnOldSave()
        {
            var rules = PrototypeRules.Create();
            var altered = new GameRules(rules.RulesId, rules.Patients,
                rules.Days.Select(d => new DayDefinition(d.Day, d.PurificationQuota + 1, d.Meetings)), rules.EndingPriority);
            Assert.That(altered.Fingerprint, Is.Not.EqualTo(rules.Fingerprint));
            Assert.Throws<SnapshotValidationException>(() => SnapshotCodec.Deserialize(
                SnapshotCodec.Serialize(new GameSession(rules)), altered));
        }

        [Test]
        public void HostJsonDefaultsCannotChangeTheSaveFormatOrRuleFingerprint()
        {
            var rules = PrototypeRules.Create(); var session = new GameSession(rules);
            var json = SnapshotCodec.Serialize(session);
            var previous = JsonConvert.DefaultSettings;
            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                Assert.That(PrototypeRules.Create().Fingerprint, Is.EqualTo(rules.Fingerprint));
                Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(json));
                Assert.That(SnapshotCodec.Serialize(SnapshotCodec.Deserialize(json, rules)), Is.EqualTo(json));
            }
            finally { JsonConvert.DefaultSettings = previous; }
        }

        [Test]
        public void FileSaveStoreCreatesAndAtomicallyReplacesUtf8SaveWithoutTempResidue()
        {
            var directory = Path.Combine(Path.GetTempPath(), "minigame-save-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                var rules = PrototypeRules.Create(); var session = new GameSession(rules);
                var store = new FileSaveStore(Path.Combine(directory, "save.json"));
                store.Save(session);
                Assert.That(SnapshotCodec.Serialize(store.Load(rules)), Is.EqualTo(SnapshotCodec.Serialize(session)));
                Scenario.EnterWork(session); Scenario.Reply(session, "empathize"); store.Save(session);
                Assert.That(SnapshotCodec.Serialize(store.Load(rules)), Is.EqualTo(SnapshotCodec.Serialize(session)));
                Assert.That(Directory.GetFiles(directory).Select(Path.GetFileName), Is.EquivalentTo(new[] { "save.json" }));
                File.WriteAllText(store.FilePath, "broken");
                Assert.Throws<SnapshotValidationException>(() => store.Load(rules));
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
    }
}
