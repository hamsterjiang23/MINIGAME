using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class AuthoringPackageTests
    {
        private static readonly string Workspace = FindWorkspace();
        private static string SampleJson() => File.ReadAllText(Path.Combine(Workspace,
            "UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json"));
        private static CompiledDialoguePackage Sample() => DialoguePackageCompiler.Compile(SampleJson());

        [Test]
        public void SuppliedHtmlDialogueAndCanonicalYarnMatchEveryLineOptionPortraitAndMeasure()
        {
            var compiled = Sample();
            var original = JObject.Parse(File.ReadAllText(Path.Combine(Workspace,
                "docs/.source-data/inspection-terminal-day1-v0.5.2.data.json")));
            var map = compiled.SourceNodes.ToDictionary(n => n.Id);
            Assert.That(compiled.SourceNodes.Count, Is.EqualTo(57));
            foreach (var property in ((JObject)original["nodes"]!).Properties())
            {
                var node = map[property.Name];
                Assert.Multiple(() =>
                {
                    Assert.That(node.Kind, Is.EqualTo("line"));
                    Assert.That(node.Text, Is.EqualTo((string)property.Value["text"]!));
                    Assert.That(node.Next, Is.EqualTo((string)property.Value["next"]!));
                    Assert.That(node.Round, Is.EqualTo((string)property.Value["round"]!));
                    Assert.That(node.PortraitId, Is.EqualTo((string)property.Value["portrait"]!));
                });
            }
            foreach (var property in ((JObject)original["choices"]!).Properties())
            {
                var node = map[property.Name];
                var options = (JArray)property.Value;
                Assert.That(node.Options.Count, Is.EqualTo(options.Count));
                for (var index = 0; index < options.Count; index++)
                {
                    var option = node.Options[index];
                    Assert.Multiple(() =>
                    {
                        Assert.That(option.Id, Is.EqualTo((string)options[index]["id"]!));
                        Assert.That(option.Text, Is.EqualTo((string)options[index]["text"]!));
                        Assert.That(option.Next, Is.EqualTo((string)options[index]["next"]!));
                        Assert.That(option.TrustDelta, Is.EqualTo((int)options[index]["delta"]!));
                    });
                }
            }
            var measures = map["D1_MEASURE"].Options;
            foreach (var property in ((JObject)original["measures"]!).Properties())
            {
                var option = measures.Single(o => o.Text == property.Name);
                Assert.That(option.Id, Is.EqualTo((string)property.Value["id"]!));
                Assert.That(option.Next, Is.EqualTo((string)property.Value["next"]!));
            }
            Assert.That(measures.Select(o => o.Disposition), Is.EquivalentTo(new[]
                { (Disposition?)Disposition.Treat, Disposition.Purify, Disposition.Release }));
            Assert.That(compiled.Diagnostics, Is.Empty);
            Assert.That(compiled.Package.YarnSource.Replace("\r\n", "\n"), Is.EqualTo(File.ReadAllText(Path.Combine(Workspace,
                "UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.yarn")).Replace("\r\n", "\n")));
        }

        [Test]
        public void IndependentDemoDoesNotExposeUnconfiguredPrototypePatientsOrDays()
        {
            var compiled = Sample();
            Assert.That(compiled.Rules.Patients.Select(p => p.Id), Is.EqualTo(new[] { "child" }));
            Assert.That(compiled.Rules.Days.Count, Is.EqualTo(1));
            Assert.That(compiled.Rules.Days[0].PurificationQuota, Is.EqualTo(5));
            var meeting = compiled.Rules.Days[0].Meetings.Single();
            Assert.That(meeting.NormalRounds.Count, Is.EqualTo(6));
            Assert.That(meeting.SpecialRound, Is.Null);
            Assert.That(meeting.ReleaseTrustThreshold, Is.EqualTo(3));
            Assert.That(compiled.Rules.Patients.Single().InitialTrust, Is.EqualTo(0));
        }

        [Test]
        public void All1296DialogueRoutesPreserveSourceOrderEffectsAndReleaseAvailability()
        {
            var compiled = Sample();
            var rounds = compiled.Rules.Days[0].Meetings.Single().NormalRounds;
            var map = compiled.SourceNodes.ToDictionary(n => n.Id);
            var paths = EnumeratePaths(rounds, 0, new List<ResponseDefinition>()).ToList();
            Assert.That(paths.Count, Is.EqualTo(1296));
            foreach (var path in paths)
            {
                var session = EnterWork(compiled.Rules);
                var actualTrace = new List<string>();
                var expectedTrust = 0;
                for (var index = 0; index < path.Count; index++)
                {
                    var before = session.GetPlayerView();
                    var response = path[index];
                    actualTrace.AddRange(compiled.Content.GetSegment(before.CurrentNodeId!).Lines.Select(l => l.SourceNodeId));
                    Accept(session, new SelectResponse(before.CurrentEncounterId!, before.CurrentNodeId!, response.Id));
                    var branch = session.GetPlayerView();
                    Assert.That(branch.CurrentNodeId, Is.EqualTo(response.BranchNodeId));
                    actualTrace.AddRange(compiled.Content.GetSegment(branch.CurrentNodeId!).Lines.Select(l => l.SourceNodeId));
                    Accept(session, new CompleteResponseBranch(branch.CurrentEncounterId!, branch.CurrentNodeId!));
                    expectedTrust += response.TrustDelta;
                }
                var expectedTrace = SourceTrace(map, compiled.Package.Profile.StartNode, path.Select(p => p.Id).ToArray());
                Assert.That(actualTrace, Is.EqualTo(expectedTrace), string.Join(",", path.Select(p => p.Id)));
                var final = session.GetPlayerView();
                Assert.That(final.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
                Assert.That(final.AvailableDispositions.Contains(Disposition.Release), Is.EqualTo(expectedTrust >= 3));
                Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(expectedTrust));
            }
        }

        [TestCase(Disposition.Treat)]
        [TestCase(Disposition.Purify)]
        [TestCase(Disposition.Release)]
        public void SaveRestoreAndRepeatedCommandsNeverApplyResponseOrDispositionTwice(Disposition disposition)
        {
            var compiled = Sample();
            var session = EnterWork(compiled.Rules);
            var rounds = compiled.Rules.Days[0].Meetings.Single().NormalRounds;
            foreach (var round in rounds)
            {
                var view = session.GetPlayerView();
                var response = round.Responses.OrderByDescending(r => r.TrustDelta).First();
                var command = new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, response.Id);
                Accept(session, command);
                session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), compiled.Rules);
                RejectUnchanged(session, command);
                var branch = session.GetPlayerView();
                Accept(session, new CompleteResponseBranch(branch.CurrentEncounterId!, branch.CurrentNodeId!));
            }
            var ready = session.GetPlayerView();
            var submit = new SubmitDisposition(ready.CurrentEncounterId!, disposition);
            Accept(session, submit);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), compiled.Rules);
            RejectUnchanged(session, submit);
            var reaction = session.GetPlayerView();
            var lines = compiled.Content.GetSegment(reaction.CurrentNodeId!).Lines;
            Assert.That(lines.Count, Is.GreaterThan(0));
            Assert.That(lines.Last().SourceNodeId, Does.StartWith(disposition == Disposition.Treat ? "D1_M_TREAT" :
                disposition == Disposition.Purify ? "D1_M_PURIFY" : "D1_M_RELEASE"));
            Accept(session, new CompleteReaction(reaction.CurrentEncounterId!));
            var snapshot = session.CreateSnapshot();
            Assert.That(snapshot.State.Patients.Single().TreatmentCount, Is.EqualTo(disposition == Disposition.Treat ? 1 : 0));
            Assert.That(snapshot.State.Days.Single().Purifications, Is.EqualTo(disposition == Disposition.Purify ? 1 : 0));
            Assert.That(snapshot.State.Encounters.Single().Responses.Count, Is.EqualTo(6));
        }

        [TestCase("missing-target", "<<jump D1_R1_OPEN2>>", "<<jump MISSING>>")]
        [TestCase("unknown-portrait", "portrait: ML01", "portrait: UNKNOWN")]
        [TestCase("cycle", "<<jump D1_R1_OPEN2>>", "<<jump D1_R1_OPEN1>>")]
        [TestCase("unsupported-command", "<<jump D1_R1_OPEN2>>", "<<set $trust = 99>>")]
        [TestCase("trust-delta", "#trust:3", "#trust:NaN")]
        [TestCase("unsupported-tag", "#trust:3", "#trust:3 #energy:1")]
        [TestCase("node-kind", "kind: line", "kind: arbitrary")]
        [TestCase("unsupported-header", "kind: line", "custom: arbitrary")]
        [TestCase("patient-header", "kind: line", "kind: line\npatient: another-patient")]
        [TestCase("day-header", "kind: line", "kind: line\nday: 2")]
        [TestCase("inline-comment", "这里还要检测吗？", "这里还要检测吗？ // hidden text")]
        [TestCase("duplicate-option", "#id:D1_R1_O2", "#id:D1_R1_O1")]
        [TestCase("measure-effect", "#disposition:treat", "#disposition:treat #trust:1")]
        [TestCase("round-choice", "title: D1_R1_CHOICE\nkind: choice\nround: R1", "title: D1_R1_CHOICE\nkind: choice\nround: R9")]
        public void InvalidAuthoringIsRejectedWithLocatedDiagnostics(string code, string from, string to)
        {
            var token = JObject.Parse(SampleJson());
            var original = (string)token["yarnSource"]!;
            Assert.That(original, Does.Contain(from));
            token["yarnSource"] = original.Replace(from, to);
            var error = Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!;
            Assert.That(error.Diagnostics[0].Code, Is.EqualTo(code));
            Assert.That(error.Diagnostics[0].Line, Is.GreaterThan(0));
        }

        [Test]
        public void DuplicateSourceNodeAndNonconvergingRoundsAreRejected()
        {
            var token = JObject.Parse(SampleJson());
            var source = (string)token["yarnSource"]!;
            token["yarnSource"] = source + "\ntitle: D1_R1_OPEN1\n---\n米洛: 重复\n<<jump END_DAY1>>\n===\n";
            Assert.That(Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!
                .Diagnostics[0].Code, Is.EqualTo("duplicate-node"));
            token["yarnSource"] = source.Replace("-> 嗯，不一样。 #id:D1_R1_O1 #trust:0\n    <<jump D1_R1_CLOSING1>>",
                "-> 嗯，不一样。 #id:D1_R1_O1 #trust:0\n    <<jump D1_R3_OPEN1>>");
            Assert.That(Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!
                .Diagnostics[0].Code, Is.EqualTo("nonconvergent-round"));
        }

        [Test]
        public void JsonSchemaRejectsDuplicatePropertiesCoercionAndUnsupportedDemoDays()
        {
            var json = SampleJson();
            var duplicate = json.Replace("\"formatVersion\": 1", "\"formatVersion\": 1, \"formatVersion\": 1");
            Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(duplicate));
            var token = JObject.Parse(json);
            token["profile"]!["releaseThreshold"] = 3.5;
            Assert.That(Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!
                .Diagnostics[0].Code, Is.EqualTo("json-type"));
            token = JObject.Parse(json); token["profile"]!["day"] = 2;
            Assert.That(Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!
                .Diagnostics[0].Code, Is.EqualTo("profile-day"));
        }

        [Test]
        public void TextEditsChangeContentRevisionWithoutChangingRulesAndMetadataRemainsInRuntimeYarn()
        {
            var original = Sample();
            var token = JObject.Parse(SampleJson());
            token["yarnSource"] = ((string)token["yarnSource"]!).Replace("这里还要检测吗？", "这里仍需检测吗？");
            var edited = DialoguePackageCompiler.Compile(token.ToString());
            Assert.That(edited.Rules.Fingerprint, Is.EqualTo(original.Rules.Fingerprint));
            Assert.That(edited.Content.Revision, Is.Not.EqualTo(original.Content.Revision));
            Assert.That(edited.Package.YarnSource, Is.EqualTo((string)token["yarnSource"]!));
            Assert.That(edited.RuntimeYarn, Does.Contain("#source:D1_R1_OPEN1 #portrait:ML01"));
            Assert.That(edited.RuntimeYarn, Does.Contain("#source:D1_R1_O1"));
            Assert.That(edited.RuntimeYarn, Does.Contain("<<select_response"));
            Assert.That(edited.RuntimeYarn, Does.Not.Contain("<<jump"));
            Assert.That(edited.RuntimeYarn, Does.Not.Contain("<<set $trust"));
        }

        [Test]
        public void OverflowingTrustPathsAreRejectedBeforeRuntimeRatherThanLeavingAnUnselectableAnswer()
        {
            var token = JObject.Parse(SampleJson());
            token["profile"]!["initialTrust"] = int.MaxValue;
            var error = Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(token.ToString()))!;
            Assert.That(error.Diagnostics.Single().Code, Is.EqualTo("trust-overflow"));
        }

        [Test]
        public void EscapedControlPunctuationIsLiteralWhileWholeLineCommentsAndExplicitHeadersArePreserved()
        {
            var token = JObject.Parse(SampleJson());
            token["yarnSource"] = ((string)token["yarnSource"]!).Replace("这里还要检测吗？",
                @"文本含\#标签、\<箭头\>、\{括号\}、\[方括号\]与\\反斜线。")
                .Replace("kind: line", "kind: line\npatient: child\nday: 1");
            var compiled = DialoguePackageCompiler.Compile(token.ToString());
            var first = compiled.SourceNodes.Single(n => n.Id == "D1_R1_OPEN1");
            Assert.That(first.Text, Is.EqualTo(@"文本含#标签、<箭头>、{括号}、[方括号]与\反斜线。"));
            Assert.That(first.Headers["patient"], Is.EqualTo("child"));
            Assert.That(compiled.Package.YarnSource, Is.EqualTo((string)token["yarnSource"]!));
            Assert.That(compiled.RuntimeYarn, Does.Contain(@"文本含\#标签、\<箭头\>、\{括号\}、\[方括号\]与\\反斜线。"));
        }

        private static IEnumerable<List<ResponseDefinition>> EnumeratePaths(IReadOnlyList<RoundDefinition> rounds,
            int index, List<ResponseDefinition> selected)
        {
            if (index == rounds.Count) { yield return new List<ResponseDefinition>(selected); yield break; }
            foreach (var response in rounds[index].Responses)
            {
                selected.Add(response);
                foreach (var path in EnumeratePaths(rounds, index + 1, selected)) yield return path;
                selected.RemoveAt(selected.Count - 1);
            }
        }

        private static List<string> SourceTrace(Dictionary<string, AuthoringNode> nodes, string cursor, string[] responses)
        {
            var trace = new List<string>(); var index = 0;
            while (nodes[cursor].Kind != "measure")
            {
                var node = nodes[cursor];
                if (node.Kind == "line") { trace.Add(node.Id); cursor = node.Next; }
                else
                {
                    var responseId = responses[index++];
                    cursor = node.Options.Single(o => o.Id == responseId).Next;
                }
            }
            Assert.That(index, Is.EqualTo(responses.Length));
            return trace;
        }

        private static GameSession EnterWork(GameRules rules)
        {
            var session = new GameSession(rules);
            Accept(session, new CompleteMorningInspection()); Accept(session, new CompleteBriefing());
            Accept(session, new Interact(InteractionTarget.Computer));
            return session;
        }

        private static void Accept(GameSession session, GameCommand command)
        { var result = session.Execute(command); Assert.That(result.Accepted, Is.True, result.Reason.ToString()); }

        private static void RejectUnchanged(GameSession session, GameCommand command)
        {
            var before = SnapshotCodec.Serialize(session);
            Assert.That(session.Execute(command).Accepted, Is.False);
            Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(before));
        }

        private static string FindWorkspace()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder != null)
            {
                if (File.Exists(Path.Combine(folder.FullName, "src/MiniGame.sln"))) return folder.FullName;
                folder = folder.Parent;
            }
            throw new InvalidOperationException("Cannot locate the MINIGAME source fixtures.");
        }
    }
}
