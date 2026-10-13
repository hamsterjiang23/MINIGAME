using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class RuntimePersistenceTests
    {
        [TestCase(PresentationKind.MorningInspection)]
        [TestCase(PresentationKind.Briefing)]
        [TestCase(PresentationKind.EnterTerminal)]
        [TestCase(PresentationKind.ResponseBranch)]
        [TestCase(PresentationKind.Reaction)]
        [TestCase(PresentationKind.ExitTerminal)]
        [TestCase(PresentationKind.Sleep)]
        [TestCase(PresentationKind.Dream)]
        public void RestoredPresentationCheckpointsInvalidateOldCallbacksAndContinueWithoutReplayingEffects(PresentationKind checkpoint)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.DriveUntil(original, view => view.Cue?.Kind == checkpoint);
            var saved = original.Save();
            var oldToken = original.GetView().Cue!.Token;
            var restored = GameRuntime.Restore(saved, rules, content);
            Assert.That(restored.Save(), Is.EqualTo(saved));
            Assert.That(restored.GetView().Cue!.Kind, Is.EqualTo(checkpoint));
            Assert.That(restored.GetView().Cue!.Token, Is.Not.EqualTo(oldToken));
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.CompleteCue(oldToken));
            var originalEvents = Finish(original);
            var restoredEvents = Finish(restored);
            Assert.That(restoredEvents, Is.EqualTo(originalEvents));
            Assert.That(restored.Save(), Is.EqualTo(original.Save()));
            Assert.That(restored.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            if (checkpoint == PresentationKind.Reaction)
            {
                var committedEncounter = JObject.Parse(JObject.Parse(saved)["CoreJson"]!.Value<string>()!)
                    ["State"]!["CurrentEncounterId"]!.Value<string>();
                Assert.That(restoredEvents.Any(item => item.StartsWith($"DispositionCommitted:1:{committedEncounter}:", StringComparison.Ordinal)), Is.False);
            }
        }

        [Test]
        public void PausedInspectionRestoresBothLocksAndObservationDeduplication()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.OpenTerminal(original);
            RuntimeScenario.Accept(original.Inspect(InspectionKind.Hands));
            RuntimeScenario.Accept(original.SetPaused(true));
            var restored = GameRuntime.Restore(original.Save(), rules, content);
            Assert.That(restored.Save(), Is.EqualTo(original.Save()));
            Assert.That(restored.GetView().Input, Is.EqualTo(InputContext.Paused));
            Assert.That(restored.GetView().Inspection!.Kind, Is.EqualTo(InspectionKind.Hands));
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.CloseInspection());
            RuntimeScenario.Accept(restored.SetPaused(false));
            Assert.That(restored.GetView().Input, Is.EqualTo(InputContext.Inspection));
            RuntimeScenario.Accept(restored.CloseInspection());
            RuntimeScenario.Accept(restored.Inspect(InspectionKind.Hands));
            Assert.That(restored.GetView().ObservedSigns, Is.EqualTo(original.GetView().ObservedSigns));
        }

        [Test]
        public void RestoringReactionCannotResubmitTheAlreadyCommittedDisposition()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.OpenTerminal(original);
            RuntimeScenario.ReachDisposition(original);
            var encounter = original.GetView().Game.CurrentEncounterId!;
            RuntimeScenario.Accept(original.SubmitDisposition(encounter, Disposition.Purify));
            var restored = GameRuntime.Restore(original.Save(), rules, content);
            RuntimeScenario.RejectWithoutMutation(restored, () => restored.SubmitDisposition(encounter, Disposition.Purify));
            var completed = RuntimeScenario.Complete(restored, PresentationKind.Reaction);
            Assert.That(completed.Events.Any(item => item.Kind == GameEventKind.DispositionCommitted), Is.False);
            Assert.That(restored.GetView().Game.RemainingPurificationQuota, Is.Zero);
            var core = JObject.Parse(JObject.Parse(restored.Save())["CoreJson"]!.Value<string>()!);
            Assert.That(core["State"]!["Days"]![0]!["Purifications"]!.Value<int>(), Is.EqualTo(1));
        }

        [TestCase("")]
        [TestCase("{")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{} {}")]
        public void MalformedOrIncompleteHostJsonIsRejected(string json)
        {
            var rules = PrototypeRules.Create();
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json, rules, new PrototypeContent(rules)));
        }

        [TestCase("version")]
        [TestCase("content-revision")]
        [TestCase("unknown-field")]
        [TestCase("missing-cue")]
        [TestCase("wrong-cue")]
        [TestCase("closed-terminal")]
        [TestCase("inspection-with-cue")]
        [TestCase("unknown-cue-enum")]
        [TestCase("numeric-cue-enum")]
        [TestCase("missing-required-field")]
        [TestCase("missing-nullable-field")]
        [TestCase("duplicate-observation")]
        [TestCase("future-observation")]
        [TestCase("unknown-observation-patient")]
        [TestCase("unmet-observation-patient")]
        [TestCase("unknown-observation-enum")]
        [TestCase("core-ledger-corruption")]
        public void HostAndCoreCheckpointCorruptionIsRejectedWithoutChangingTheOriginal(string corruption)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var original = new GameRuntime(rules, content);
            RuntimeScenario.OpenTerminal(original);
            RuntimeScenario.Accept(original.Inspect(InspectionKind.Records));
            RuntimeScenario.Accept(original.CloseInspection());
            RuntimeScenario.ReachDisposition(original);
            RuntimeScenario.Accept(original.SubmitDisposition(original.GetView().Game.CurrentEncounterId!, Disposition.Purify));
            var before = original.Save();
            var json = JObject.Parse(before);
            switch (corruption)
            {
                case "version": json["Version"] = 2; break;
                case "content-revision": json["ContentRevision"] = "another-content-version"; break;
                case "unknown-field": json["Unexpected"] = true; break;
                case "missing-cue": json["CueKind"] = null; break;
                case "wrong-cue": json["CueKind"] = "Dream"; break;
                case "closed-terminal": json["TerminalOpen"] = false; break;
                case "inspection-with-cue": json["OpenInspectionKind"] = "Records"; break;
                case "unknown-cue-enum": json["CueKind"] = "NonexistentCue"; break;
                case "numeric-cue-enum": json["CueKind"] = 4; break;
                case "missing-required-field": json.Remove("Paused"); break;
                case "missing-nullable-field": json.Remove("OpenInspectionKind"); break;
                case "duplicate-observation": ((JArray)json["Observations"]!).Add(json["Observations"]![0]!.DeepClone()); break;
                case "future-observation": json["Observations"]![0]!["Day"] = 2; break;
                case "unknown-observation-patient": json["Observations"]![0]!["PatientId"] = "unknown-patient"; break;
                case "unmet-observation-patient": json["Observations"]![0]!["PatientId"] = PrototypeRules.Diplomat; break;
                case "unknown-observation-enum": json["Observations"]![0]!["Kind"] = "NotAnInspection"; break;
                case "core-ledger-corruption":
                    var core = JObject.Parse(json["CoreJson"]!.Value<string>()!);
                    core["State"]!["Patients"]![0]!["Trust"] = 999;
                    json["CoreJson"] = core.ToString();
                    break;
            }
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(json.ToString(), rules, content));
            Assert.That(original.Save(), Is.EqualTo(before));
        }

        [Test]
        public void AdditionalJsonAfterAValidHostCheckpointIsRejected()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(runtime.Save() + " {}", rules, content));
        }

        [TestCase("close-mid-dialogue")]
        [TestCase("enter-mid-dialogue")]
        [TestCase("exit-without-completed-patient")]
        public void CameraCheckpointCannotSkipTheEncounterLifecycle(string corruption)
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            RuntimeScenario.OpenTerminal(runtime);
            if (corruption != "exit-without-completed-patient") RuntimeScenario.Reply(runtime, "neutral");
            var checkpoint = JObject.Parse(runtime.Save());
            if (corruption == "close-mid-dialogue") checkpoint["TerminalOpen"] = false;
            if (corruption == "enter-mid-dialogue") checkpoint["CueKind"] = "EnterTerminal";
            if (corruption == "exit-without-completed-patient") checkpoint["CueKind"] = "ExitTerminal";
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(checkpoint.ToString(), rules, content));
        }

        [Test]
        public void DifferentContentRevisionCannotResumeAnExistingSave()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            Assert.Throws<SnapshotValidationException>(() => GameRuntime.Restore(runtime.Save(), rules, new RevisedContent(content)));
        }

        [Test]
        public void HostJsonDefaultsCannotChangeTheCheckpointOrCoreRuleFingerprint()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var runtime = new GameRuntime(rules, content);
            RuntimeScenario.OpenTerminal(runtime);
            RuntimeScenario.Reply(runtime, "empathize");
            var json = runtime.Save();
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
                Assert.That(runtime.Save(), Is.EqualTo(json));
                Assert.That(GameRuntime.Restore(json, rules, content).Save(), Is.EqualTo(json));
            }
            finally { JsonConvert.DefaultSettings = previous; }
        }

        private static IReadOnlyList<string> Finish(GameRuntime runtime)
        {
            var events = new List<string>();
            for (var guard = 0; guard < 1000; guard++)
            {
                if (runtime.GetView().Game.Phase == GamePhase.Ending) return events;
                var result = RuntimeScenario.Step(runtime, RuntimeScenario.NormalPolicy, "neutral");
                events.AddRange(result.Events.Select(item => $"{item.Kind}:{item.Day}:{item.EncounterId}:{item.PatientId}:{item.ReferenceId}"));
            }
            throw new InvalidOperationException("Runtime continuation exceeded its command limit.");
        }

        private sealed class RevisedContent : IGameContent
        {
            private readonly IGameContent _original;
            public RevisedContent(IGameContent original) { _original = original; }
            public string Revision => _original.Revision + ":modified";
            public DialogueContent GetDialogue(PlayerView view) => _original.GetDialogue(view);
            public string GetPresentation(PresentationKind kind, PlayerView view) => _original.GetPresentation(kind, view);
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind)
                => _original.GetInspection(patientId, day, kind);
        }
    }
}
