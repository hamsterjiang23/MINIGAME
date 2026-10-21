using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class CampaignPackageTests
    {
        private static string Workspace()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "UnityProject"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Workspace not found.");
        }
        private static string Source(string name) => File.ReadAllText(Path.Combine(Workspace(), "UnityProject/Assets/MiniGame/Content/Campaign", name));
        private static CompiledDialoguePackage Compile() => DialoguePackageCompiler.Compile(Source("campaign.minigame.json"));

        [Test]
        public void FullCampaignHasTutorialAndFifteenOrderedMeetingsWithOriginalDeclaredPortraits()
        {
            var package = Compile();
            Assert.That(package.Package.FormatVersion, Is.EqualTo(2));
            Assert.That(package.Rules.Days.Select(d => d.Day), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(package.Rules.Days.Sum(d => d.Meetings.Count), Is.EqualTo(16));
            foreach (var day in package.Rules.Days.Skip(1))
                Assert.That(day.Meetings.Select(m => m.PatientId), Is.EqualTo(new[] { "child", "former-ruler", "battlefield-doctor", "diplomat", "priest" }));
            var tutorial = package.Rules.Days[0].Meetings.Single();
            Assert.That(tutorial.NormalRounds, Is.Empty);
            Assert.That(package.Content.GetSegment(tutorial.IntroNodeId!).Lines.Count, Is.EqualTo(5));
            Assert.That(tutorial.ReactionNodes.Keys, Is.EqualTo(new[] { Disposition.Purify }));
            Assert.That(tutorial.DispositionTrustDeltas[Disposition.Purify], Is.Zero);
            Assert.That(package.Content.GetSegment(tutorial.ReactionNodes[Disposition.Purify]).Lines.Count, Is.EqualTo(2));
            Assert.That(package.Package.Portraits.Count, Is.EqualTo(22));
            Assert.That(package.SourceNodes.Select(n => n.PortraitId).Where(p => p.Length > 0).Distinct().Count(), Is.EqualTo(16));
            var specialMeetings = package.Rules.Days.SelectMany(day => day.Meetings).Where(meeting => meeting.SpecialRound != null).ToArray();
            Assert.That(specialMeetings.Length, Is.EqualTo(9));
            Assert.That(specialMeetings.Select(meeting => meeting.PatientId).Distinct(),
                Is.EquivalentTo(new[] { "child", "former-ruler", "diplomat" }));
            Assert.That(specialMeetings.All(meeting => meeting.SpecialTrustThreshold == 3 && meeting.ReleaseTrustThreshold == 5), Is.True);
            Assert.That(package.Diagnostics, Is.Empty);
            Assert.That(package.Package.YarnSource, Is.EqualTo(Source("campaign.yarn")));
        }

        [Test]
        public void EveryActiveSourceRowMapsToUnmodifiedTextAndExcludedRowsRemainAudited()
        {
            var compiled = Compile();
            var audit = JObject.Parse(Source("provenance.json"));
            var mappings = (JArray)audit["mappings"]!;
            var nodes = compiled.SourceNodes.ToDictionary(n => n.Id);
            var options = compiled.SourceNodes.SelectMany(n => n.Options).ToDictionary(o => o.Id);
            Assert.That((int)audit["sourceRowCount"]!, Is.EqualTo(1437));
            Assert.That((int)audit["activeSourceRowCount"]!, Is.EqualTo(1295));
            Assert.That((int)audit["excludedSourceRowCount"]!, Is.EqualTo(142));
            Assert.That(mappings.Count, Is.EqualTo((int)audit["activeSourceRowCount"]!));
            var excluded = (JArray)audit["excludedRows"]!;
            Assert.That(excluded.Count, Is.EqualTo((int)audit["excludedSourceRowCount"]!));
            var mappedRows = mappings.ToLookup(mapping => ((string)mapping["patientId"]!, (int)mapping["row"]!));
            var excludedRows = excluded.ToLookup(row => ((string)row["patientId"]!, (int)row["row"]!));
            foreach (var book in audit["books"]!)
            {
                var patient = (string)book["patientId"]!;
                var active = book["activeRows"]!.ToDictionary(row => int.Parse((string)row["row"]!, CultureInfo.InvariantCulture));
                var columns = book["schemaColumns"]!;
                foreach (var row in book["rows"]!)
                {
                    var number = int.Parse((string)row["row"]!, CultureInfo.InvariantCulture);
                    var key = (patient, number);
                    var sourceMappings = mappedRows[key].ToArray();
                    var sourceExcluded = excludedRows[key].ToArray();
                    Assert.That(active.ContainsKey(number) ^ (sourceExcluded.Length == 1), Is.True, key.ToString());
                    if (sourceExcluded.Length != 0)
                    {
                        Assert.That(sourceMappings, Is.Empty, key.ToString());
                        Assert.That((string)sourceExcluded[0]["reason"]!, Is.Not.Empty);
                        Assert.That((string)sourceExcluded[0]["originalId"]!, Is.EqualTo((string)row[(string)columns["id"]!]!));
                        continue;
                    }
                    Assert.That(sourceMappings, Is.Not.Empty, key.ToString());
                    foreach (var field in new[] { "open", "option", "response", "action", "actionResponse" })
                    {
                        var column = (string)columns[field]!;
                        var originalText = (string?)row[column] ?? "";
                        Assert.That((string)active[number][field]!, Is.EqualTo(originalText), key + ":" + column);
                        if (originalText.Length == 0) continue;
                        Assert.That(sourceMappings.Count(mapping => (string)mapping["column"]! == column
                            && (string)mapping["text"]! == originalText), Is.EqualTo(1), key + ":" + column);
                    }
                    foreach (var mapping in sourceMappings)
                    {
                        Assert.That((string)mapping["originalId"]!, Is.EqualTo((string)row[(string)columns["id"]!]!));
                        Assert.That((string)mapping["authoredDelta"]!, Is.EqualTo((string?)row[(string)columns["delta"]!] ?? ""));
                    }
                }
            }
            foreach (var mapping in mappings)
            {
                var id = (string)mapping["sourceId"]!;
                var text = (string)mapping["text"]!;
                if ((string)mapping["kind"]! == "option")
                {
                    Assert.That(options[id].Text, Is.EqualTo(text), id);
                    Assert.That(options[id].TrustDelta, Is.EqualTo((int)mapping["trustDelta"]!), id);
                }
                else Assert.That(nodes[id].Text, Is.EqualTo(text), id);
            }
            Assert.That(nodes.Values.Count(node => node.Kind == "line" || node.Kind == "narration") + options.Count,
                Is.EqualTo(mappings.Count), "Every compiled sentence and player option must have an active source row.");
        }

        [Test]
        public void RevisedDiplomatKeepsIndividualSentencesWithoutLegacyHistoricalWording()
        {
            var compiled = Compile();
            var meeting = compiled.Rules.Days.Single(day => day.Day == 1).Meetings.Single(item => item.PatientId == "diplomat");
            Assert.That(compiled.Content.GetSegment(meeting.NormalRounds[0].NodeId).Lines, Is.Empty);
            Assert.That(compiled.Content.GetSegment(meeting.NormalRounds[0].NodeId).Responses.Single().Text,
                Is.EqualTo("先核对身份。姓名、职业，再说明你是怎么被送进来的。"));
            Assert.That(compiled.Content.GetSegment(meeting.NormalRounds[1].NodeId).Lines.Select(line => line.SourceNodeId),
                Is.EqualTo(Enumerable.Range(1, 4).Select(index => "diplomat__P05_D1_R2_COMMON_00" + index)));
            Assert.That(compiled.Content.TextVariants, Is.Empty);
            foreach (var line in compiled.Content.Segments.Values.SelectMany(segment => segment.Lines))
            {
                Assert.That(compiled.Content.ResolveText(line.SourceNodeId, line.Text, _ => false), Is.EqualTo(line.Text));
                Assert.That(compiled.Content.ResolveText(line.SourceNodeId, line.Text, _ => true), Is.EqualTo(line.Text));
            }
            Assert.That(compiled.Content.IsNarration("diplomat__P05_D1_R2_COMMON_001"), Is.False);
            foreach (var narrator in compiled.SourceNodes.Where(node => node.Kind == "narration"))
                Assert.That(compiled.Content.IsNarration(narrator.Id), Is.True, narrator.Id);
            Assert.That(compiled.RuntimeYarn, Does.Contain("#kind:narration"));
            Assert.That(compiled.RuntimeYarn, Does.Contain("#source:diplomat__P05_D1_R2_COMMON_001"));
        }

        [Test]
        public void DoctorLineTrustIsAuditedSeparatelyAndAddedExactlyOnceToItsUniqueAnswer()
        {
            var compiled = Compile();
            var audit = JObject.Parse(Source("provenance.json"));
            var transfer = audit["adaptations"]!.Single(item => (string)item["kind"]! == "line-trust-on-unique-answer");
            Assert.That((string)transfer["patientId"]!, Is.EqualTo("battlefield-doctor"));
            Assert.That((string)transfer["sourceCell"]!, Is.EqualTo("K65"));
            Assert.That((int)transfer["authoredDelta"]!, Is.EqualTo(5));
            Assert.That((int)transfer["authoredOptionDelta"]!, Is.EqualTo(-5));
            Assert.That((int)transfer["effectiveOptionDelta"]!, Is.Zero);
            var response = compiled.SourceNodes.SelectMany(node => node.Options)
                .Single(option => option.Id == (string)transfer["appliedTo"]!);
            Assert.That(response.Id, Is.EqualTo("battlefield-doctor__P04_D1_R6_OPTION_002"));
            Assert.That(response.TrustDelta, Is.Zero);
            Assert.That(compiled.Content.GetSegment(response.Id + ".branch").Lines.Select(line => line.SourceNodeId),
                Does.Contain("battlefield-doctor__P04_D1_R6_CLOSE_004"));
            var lineMapping = audit["mappings"]!.Single(item => (string)item["sourceId"]! == "battlefield-doctor__P04_D1_R6_CLOSE_004");
            Assert.That((string)lineMapping["authoredDelta"]!, Is.EqualTo("5"));
            Assert.That((string)lineMapping["trustAppliedTo"]!, Is.EqualTo(response.Id));
        }

        [Test]
        public void SilentResponseBranchesHaveExplicitStopSoYarnDoesNotPruneTheirNodes()
        {
            var compiled = Compile();
            var silent = compiled.Content.Segments.Values
                .Where(segment => segment.Lines.Count == 0 && segment.Responses.Count == 0).ToArray();
            Assert.That(silent, Is.Not.Empty, "The revised campaign includes player choices with no immediate response text.");
            Assert.That(silent.Select(segment => segment.CoreNodeId), Does.Contain("child__P02_D1_R1_OPTION_001.branch"));
            var yarn = compiled.RuntimeYarn.Replace("\r\n", "\n");
            foreach (var segment in silent)
            {
                var header = "title: " + PrototypeYarnSource.NodeName(segment.CoreNodeId) + "\n---\n";
                var start = yarn.IndexOf(header, StringComparison.Ordinal);
                Assert.That(start, Is.GreaterThanOrEqualTo(0), segment.CoreNodeId);
                start += header.Length;
                var end = yarn.IndexOf("\n===", start, StringComparison.Ordinal);
                Assert.That(end, Is.GreaterThanOrEqualTo(start), segment.CoreNodeId);
                Assert.That(yarn.Substring(start, end - start).Trim(), Is.EqualTo("<<stop>>"), segment.CoreNodeId);
            }
        }

        [Test]
        public void EachRoundAndDispositionBranchContainsOnlyItsOwnMeetingDayAndEndsAtCoreBoundary()
        {
            var compiled = Compile();
            var allLines = compiled.Content.Segments.Values.SelectMany(s => s.Lines).Select(l => l.SourceNodeId).ToHashSet();
            foreach (var node in compiled.SourceNodes.Where(n => n.Kind == "line" || n.Kind == "narration"))
                Assert.That(allLines.Contains(node.Id), Is.True, node.Id);
            foreach (var day in compiled.Rules.Days)
            foreach (var meeting in day.Meetings)
            {
                var originals = compiled.Package.Campaign!.Days.Single(d => d.Day == day.Day).Meetings.Single(m => m.PatientId == meeting.PatientId);
                foreach (var reaction in meeting.ReactionNodes.Values)
                    Assert.That(compiled.Content.GetSegment(reaction).Lines, Is.Not.Empty);
                Assert.That(originals.YarnSource, Does.Not.Contain("<<jump " + meeting.PatientId + "__D" + (day.Day + 1) + "_"));
            }
        }

        [Test]
        public void InvalidVariantAndSourceMismatchAreRejectedAndVariantChangeInvalidatesContentRevision()
        {
            var json = JObject.Parse(Source("campaign.minigame.json"));
            var sourceId = DialoguePackageCompiler.Compile(json.ToString()).SourceNodes.First(node => node.Kind == "line").Id;
            json["campaign"]!["variants"] = new JArray(new JObject
            {
                ["sourceId"] = sourceId, ["predicate"] = "other_patient_exited_yesterday", ["fallbackText"] = "测试历史缺失时的文字。"
            });
            var original = DialoguePackageCompiler.Compile(json.ToString());
            Assert.That(original.Content.ResolveText(sourceId, "原文", _ => false), Is.EqualTo("测试历史缺失时的文字。"));
            Assert.That(original.Content.ResolveText(sourceId, "原文", _ => true), Is.EqualTo("原文"));
            json["campaign"]!["variants"]![0]!["fallbackText"] = "另一段开发适配文本。";
            Assert.That(DialoguePackageCompiler.Compile(json.ToString()).Content.Revision, Is.Not.EqualTo(original.Content.Revision));
            json["campaign"]!["variants"]![0]!["predicate"] = "invented_history";
            Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(json.ToString()));
            json = JObject.Parse(Source("campaign.minigame.json"));
            json["yarnSource"] = "stale concatenated source";
            Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(json.ToString()));
        }

        [TestCase(int.MaxValue, 1, -1)]
        [TestCase(int.MinValue, -1, 1)]
        public void IntermediateTrustOverflowIsRejectedEvenWhenTheFollowingRoundWouldCancelIt(int initialTrust, int first, int second)
        {
            var json = TwoRoundFixture(initialTrust, first, second);
            var error = Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(json.ToString()));
            Assert.That(error!.Diagnostics.Single().Code, Is.EqualTo("trust-overflow"));
            Assert.That(error.Diagnostics.Single().NodeId, Is.EqualTo("FIRST"));
        }

        [TestCase("day: 1", "day: 2", "meeting-header")]
        [TestCase("<<jump SECOND>>", "<<jump MISSING_CHOICE>>", "missing-target")]
        public void MeetingDayAndChoiceTargetsCannotEscapeTheirConfiguredSource(string before, string after, string expectedCode)
        {
            var json = TwoRoundFixture(0, 0, 0);
            var source = ((string)json["yarnSource"]!).Replace(before, after);
            json["yarnSource"] = source;
            json["campaign"]!["days"]![0]!["meetings"]![0]!["yarnSource"] = source;
            var error = Assert.Throws<DialoguePackageException>(() => DialoguePackageCompiler.Compile(json.ToString()));
            Assert.That(error!.Diagnostics.Single().Code, Is.EqualTo(expectedCode));
        }

        private static JObject TwoRoundFixture(int initialTrust, int first, int second)
        {
            var json = JObject.Parse(Source("campaign.minigame.json"));
            var child = (JObject)json["campaign"]!["patients"]!.Single(patient => (string)patient["id"]! == "child").DeepClone();
            child["initialTrust"] = initialTrust;
            var source = string.Join("\n", new[]
            {
                "title: FIRST", "kind: choice", "round: R1", "patient: child", "day: 1", "---",
                "-> 第一轮 #id:FIRST_OPTION #trust:" + first, "    <<jump FIRST_LINE>>", "===",
                "title: FIRST_LINE", "kind: line", "round: R1", "patient: child", "day: 1", "portrait: ML01", "---",
                "米洛: 第一轮回应。", "<<jump SECOND>>", "===",
                "title: SECOND", "kind: choice", "round: R2", "patient: child", "day: 1", "---",
                "-> 第二轮 #id:SECOND_OPTION #trust:" + second, "    <<jump SECOND_LINE>>", "===",
                "title: SECOND_LINE", "kind: line", "round: R2", "patient: child", "day: 1", "portrait: ML01", "---",
                "米洛: 第二轮回应。", "<<jump MEASURE>>", "===",
                "title: MEASURE", "kind: measure", "round: M", "patient: child", "day: 1", "---",
                "-> 净化 #id:PURIFY #trust:0 #disposition:purify", "    <<jump REACTION>>", "===",
                "title: REACTION", "kind: line", "round: M", "patient: child", "day: 1", "portrait: ML01", "---",
                "米洛: 再见。", "<<jump END>>", "==="
            });
            json["yarnSource"] = source;
            json["campaign"]!["patients"] = new JArray(child);
            json["campaign"]!["variants"] = new JArray();
            json["campaign"]!["days"] = new JArray(new JObject
            {
                ["day"] = 1, ["quota"] = 0,
                ["meetings"] = new JArray(new JObject
                { ["patientId"] = "child", ["startNode"] = "FIRST", ["endNode"] = "END", ["yarnSource"] = source })
            });
            return json;
        }
    }
}
