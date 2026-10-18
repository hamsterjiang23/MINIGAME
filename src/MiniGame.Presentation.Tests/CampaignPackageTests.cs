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
            Assert.That(tutorial.DispositionTrustDeltas[Disposition.Purify], Is.EqualTo(-1));
            Assert.That(package.Content.GetSegment(tutorial.ReactionNodes[Disposition.Purify]).Lines.Count, Is.EqualTo(2));
            Assert.That(package.Package.Portraits.Count, Is.EqualTo(27));
            Assert.That(package.SourceNodes.Select(n => n.PortraitId).Where(p => p.Length > 0).Distinct().Count(), Is.EqualTo(24));
            Assert.That(package.Diagnostics, Is.Empty);
            Assert.That(package.Package.YarnSource, Is.EqualTo(Source("campaign.yarn")));
        }

        [Test]
        public void EverySourceRowAndTextCellMapsToUnmodifiedCompiledTextAndEveryOptionKeepsItsDelta()
        {
            var compiled = Compile();
            var audit = JObject.Parse(Source("provenance.json"));
            var mappings = (JArray)audit["mappings"]!;
            var nodes = compiled.SourceNodes.ToDictionary(n => n.Id);
            var options = compiled.SourceNodes.SelectMany(n => n.Options).ToDictionary(o => o.Id);
            Assert.That((int)audit["sourceRowCount"]!, Is.EqualTo(994));
            foreach (var book in audit["books"]!)
            foreach (var row in book["rows"]!)
            {
                var sourceMappings = mappings.Where(m => (string)m["patientId"]! == (string)book["patientId"]! && (string)m["originalId"]! == (string)row["J"]!).ToArray();
                Assert.That(sourceMappings, Is.Not.Empty, (string)row["J"]!);
                foreach (var column in new[] { "E", "F", "G", "H", "I" }.Where(c => row[c] != null))
                    Assert.That(sourceMappings.Any(m => (string)m["column"]! == column && (string)m["text"]! == (string)row[column]!), Is.True, row["J"] + ":" + column);
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
        }

        [Test]
        public void DiplomatResponsesRemainThreeSequentialColumnsAndFactsOnlySelectWording()
        {
            var compiled = Compile();
            var responseId = "diplomat__AD_D1_R1_O1";
            Assert.That(compiled.Content.GetSegment(responseId + ".branch").Lines.Select(l => l.SourceNodeId),
                Is.EqualTo(new[] { responseId + "__G", responseId + "__H", responseId + "__I" }));
            foreach (var variant in compiled.Content.TextVariants.Values)
            {
                Assert.That(compiled.Content.ResolveText(variant.SourceId, "original", _ => false), Is.EqualTo(variant.FallbackText));
                Assert.That(compiled.Content.ResolveText(variant.SourceId, "original", _ => true), Is.EqualTo("original"));
            }
            Assert.That(compiled.Content.TextVariants.Count, Is.EqualTo(26));
            Assert.That(compiled.Content.IsNarration("diplomat__AD_END_TREATMENT__visit_day3"), Is.True);
            Assert.That(compiled.Content.IsNarration("diplomat__AD_D1_R1_START"), Is.False);
            Assert.That(compiled.RuntimeYarn, Does.Contain("#kind:narration"));
            Assert.That(compiled.RuntimeYarn, Does.Contain("#source:diplomat__AD_D1_R1_O1__H"));
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
            var original = DialoguePackageCompiler.Compile(json.ToString());
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
