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
        private static string NormalizeNewlines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');

        [Test]
        public void ActiveCampaignBindsTheValidatedRebalancedPackageAndRuntimeYarn()
        {
            var compiled = Compile();
            var project = Path.Combine(Workspace(), "UnityProject");
            var active = File.ReadAllText(Path.Combine(project, "Assets/Resources/MiniGame/ActiveCampaignPackage.asset"));
            Assert.That(active, Does.Contain("sourceHash: " + compiled.SourceHash));
            var match = System.Text.RegularExpressions.Regex.Match(active, @"packageSource: \{fileID: 4900000, guid: ([a-f0-9]+)");
            Assert.That(match.Success, Is.True);
            var meta = Directory.EnumerateFiles(Path.Combine(project, "Assets/MiniGame/ImportedDialogue/inspection-campaign"),
                "Package.minigame.json.meta", SearchOption.AllDirectories)
                .Single(path => File.ReadAllText(path).Contains("guid: " + match.Groups[1].Value));
            Assert.That(File.ReadAllText(meta.Substring(0, meta.Length - 5)), Is.EqualTo(Source("campaign.minigame.json")));
            Assert.That(NormalizeNewlines(File.ReadAllText(Path.Combine(Path.GetDirectoryName(meta)!, "Runtime.yarn"))),
                Is.EqualTo(NormalizeNewlines(compiled.RuntimeYarn)));
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void RebalancedCampaignRetainsLegalQuotaRoutesAndBothHiddenOutcomes(bool soothing, bool insultDoctor)
        {
            var compiled = Compile();
            var rules = compiled.Rules.WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
            var session = new GameSession(rules);
            var doctorReleaseChecked = false;
            for (var step = 0; step < 1000; step++)
            {
                var view = session.GetPlayerView();
                if (view.Phase == GamePhase.SelfReview || (view.Day == 3 && view.Phase == GamePhase.AfterDream)) break;
                GameCommand command;
                if (view.Phase == GamePhase.MorningInspection) command = new CompleteMorningInspection();
                else if (view.Phase == GamePhase.Briefing) command = new CompleteBriefing();
                else if (view.Phase == GamePhase.Apartment) command = new Interact(InteractionTarget.Computer);
                else if (view.Phase == GamePhase.Evening) command = new BeginNightDream();
                else if (view.Phase == GamePhase.Dream) command = new CompleteDream();
                else if (view.Phase == GamePhase.AfterDream) command = new Interact(InteractionTarget.Bed);
                else if (view.EncounterStage == EncounterStage.Introduction)
                    command = new CompleteIntroduction(view.CurrentEncounterId!, view.CurrentNodeId!);
                else if (view.EncounterStage == EncounterStage.ResponseBranch)
                    command = new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!);
                else if (view.EncounterStage == EncounterStage.Reaction)
                    command = new CompleteReaction(view.CurrentEncounterId!);
                else if (view.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    if (view.Day == 3 && view.CurrentPatientId == "battlefield-doctor" && soothing && !insultDoctor)
                    {
                        Assert.That(view.AvailableDispositions, Does.Contain(Disposition.Release));
                        doctorReleaseChecked = true;
                    }
                    var purify = view.Day == 0 || (view.Day == 1 && view.CurrentPatientId == "former-ruler")
                        || (view.Day == 2 && view.CurrentPatientId == "priest")
                        || (view.Day == 3 && view.CurrentPatientId == "battlefield-doctor");
                    var release = soothing && view.Day == 3 && (view.CurrentPatientId == "child" || view.CurrentPatientId == "diplomat");
                    command = new SubmitDisposition(view.CurrentEncounterId!, purify ? Disposition.Purify : release ? Disposition.Release : Disposition.Treat);
                }
                else
                {
                    var meeting = rules.Days.Single(d => d.Day == view.Day).Meetings.Single(m => m.PatientId == view.CurrentPatientId);
                    var round = view.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound!
                        : meeting.NormalRounds.Single(r => r.NodeId == view.CurrentNodeId);
                    var response = (soothing ? round.Responses.OrderByDescending(r => r.TrustDelta)
                        : round.Responses.OrderBy(r => r.TrustDelta)).First();
                    if (insultDoctor)
                        response = round.Responses.FirstOrDefault(r => r.Id == "battlefield-doctor__P04_D2_R4_OPTION_001") ?? response;
                    command = new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, response.Id);
                }
                var result = session.Execute(command);
                Assert.That(result.Accepted, Is.True, command.GetType().Name + ": " + result.Reason);
            }
            Assert.That(session.GetPlayerView().Phase, Is.EqualTo(soothing ? GamePhase.SelfReview : GamePhase.AfterDream));
            var state = session.CreateSnapshot().State;
            var total = state.Patients.Where(p => p.Id != "priest" && p.Id != "tutorial").Sum(p => p.Trust);
            Assert.That(total >= 10, Is.EqualTo(soothing));
            Assert.That(state.Days.Where(d => d.Day > 0).All(d => d.QuotaMet == true), Is.True);
            if (soothing && !insultDoctor) Assert.That(doctorReleaseChecked, Is.True);
            if (soothing)
            {
                for (var line = 0; line < 16; line++)
                    Assert.That(session.Execute(new AdvanceSelfReviewCommand(line)).Accepted, Is.True);
                Assert.That(session.Execute(new ConfirmSelfPurification()).Accepted, Is.True);
                Assert.That(session.GetPlayerView().Ending, Is.EqualTo(EndingType.DreamSea));
            }
        }

        private static IEnumerable<TestCaseData> ScheduledDispositionCases()
        {
            foreach (var day in Compile().Rules.Days)
                foreach (var meeting in day.Meetings)
                    foreach (var disposition in meeting.ReactionNodes.Keys)
                        yield return new TestCaseData(day.Day, meeting.PatientId, disposition);
        }

        [TestCaseSource(nameof(ScheduledDispositionCases))]
        public void OperationReactionOnlyChangesTheInstrumentAndRestoresWithoutChangingTrust(int day, string patientId, Disposition disposition)
        {
            var compiled = Compile();
            var meeting = compiled.Rules.Days.Single(d => d.Day == day).Meetings.Single(m => m.PatientId == patientId);
            var original = compiled.Rules.Patients.Single(p => p.Id == patientId);
            var patient = new PatientDefinition(patientId, original.DisplayName, 1, patientId == "priest" ? 0 : 30,
                original.TreatmentLimit, original.TreatmentOutcome);
            var rules = new GameRules("disposition-reaction-contract", new[] { patient },
                new[] { new DayDefinition(1, 0, new[] { meeting }) }, compiled.Rules.EndingPriority);
            var session = new GameSession(rules);
            Action<GameCommand> accept = command => Assert.That(session.Execute(command).Accepted, Is.True, command.GetType().Name);
            accept(new CompleteMorningInspection()); accept(new CompleteBriefing()); accept(new Interact(InteractionTarget.Computer));
            for (var step = 0; step < 100; step++)
            {
                var view = session.GetPlayerView();
                if (view.EncounterStage == EncounterStage.AwaitingDisposition) break;
                if (view.EncounterStage == EncounterStage.Introduction)
                    accept(new CompleteIntroduction(view.CurrentEncounterId!, view.CurrentNodeId!));
                else if (view.EncounterStage == EncounterStage.ResponseBranch)
                    accept(new CompleteResponseBranch(view.CurrentEncounterId!, view.CurrentNodeId!));
                else
                {
                    var round = view.EncounterStage == EncounterStage.SpecialDialogue ? meeting.SpecialRound!
                        : meeting.NormalRounds.Single(r => r.NodeId == view.CurrentNodeId);
                    accept(new SelectResponse(view.CurrentEncounterId!, view.CurrentNodeId!, round.Responses.OrderByDescending(r => r.TrustDelta).First().Id));
                }
            }
            var before = session.GetPlayerView();
            Assert.That(before.AvailableDispositions, Does.Contain(disposition));
            int trust = session.CreateSnapshot().State.Patients.Single().Trust;
            int delta = meeting.DispositionTrustDeltas[disposition];
            if (patientId == "priest" && disposition == Disposition.Release) Assert.That(delta, Is.EqualTo(-200));
            var command = new SubmitDisposition(before.CurrentEncounterId!, disposition);
            accept(command);
            Assert.That(session.GetPlayerView().EncounterStage, Is.EqualTo(EncounterStage.Reaction));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(trust));
            var reading = session.GetPlayerView().SocialAdaptation;
            if (reading != null)
                Assert.That(reading.RelativeReleaseMargin,
                    Is.EqualTo(((double)trust + delta - meeting.ReleaseTrustThreshold) / Math.Max(1d, Math.Abs((double)meeting.ReleaseTrustThreshold))));
            string saved = SnapshotCodec.Serialize(session);
            Assert.That(session.Execute(command).Accepted, Is.False);
            Assert.That(SnapshotCodec.Serialize(session), Is.EqualTo(saved));
            session = SnapshotCodec.Deserialize(saved, rules);
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(trust));
            Assert.That(session.GetPlayerView().SocialAdaptation?.RelativeReleaseMargin, Is.EqualTo(reading?.RelativeReleaseMargin));
            accept(new CompleteReaction(before.CurrentEncounterId!));
            Assert.That(session.CreateSnapshot().State.Patients.Single().Trust, Is.EqualTo(trust));
        }

        [Test]
        public void TutorialStartsAtMaximumAgitationAndRetainsItsPurifyOnlyFlowAcrossRestore()
        {
            var compiled = Compile();
            Assert.That(compiled.Rules.Patients.Single(p => p.Id == "tutorial").InitialTrust, Is.EqualTo(-2));
            Assert.That(compiled.Rules.Patients.Where(p => p.Id != "tutorial").All(p => p.InitialTrust == 0), Is.True);
            var session = new GameSession(compiled.Rules);
            Assert.That(session.Execute(new Interact(InteractionTarget.Computer)).Accepted, Is.True);
            var view = session.GetPlayerView();
            Assert.That(view.EncounterStage, Is.EqualTo(EncounterStage.Introduction));
            Assert.That(view.SocialAdaptation, Is.Not.Null, "Day0 must have a reading even though release is not authored.");
            Assert.That(SocialAdaptationSignal.CoordinateFor(view.SocialAdaptation!), Is.Zero);
            Assert.That(SocialAdaptationSignal.Parameters(0).Agitation, Is.EqualTo(1));
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), compiled.Rules);
            view = session.GetPlayerView();
            Assert.That(SocialAdaptationSignal.CoordinateFor(view.SocialAdaptation!), Is.Zero);
            Assert.That(session.Execute(new CompleteIntroduction(view.CurrentEncounterId!, view.CurrentNodeId!)).Accepted, Is.True);
            view = session.GetPlayerView();
            Assert.That(view.AvailableDispositions, Is.EqualTo(new[] { Disposition.Purify }));
            Assert.That(session.Execute(new SubmitDisposition(view.CurrentEncounterId!, Disposition.Release)).Accepted, Is.False);
            Assert.That(session.Execute(new SubmitDisposition(view.CurrentEncounterId!, Disposition.Purify)).Accepted, Is.True);
            Assert.That(session.CreateSnapshot().State.Patients.Single(p => p.Id == "tutorial").Trust, Is.EqualTo(-2));
            Assert.That(session.GetPlayerView().RemainingPurificationQuota, Is.Zero);
            session = SnapshotCodec.Deserialize(SnapshotCodec.Serialize(session), compiled.Rules);
            Assert.That(SocialAdaptationSignal.CoordinateFor(session.GetPlayerView().SocialAdaptation!), Is.Zero);
        }

        [Test]
        public void FullCampaignHasThirteenMeetingsWithStaggeredArrivalAndOriginalDeclaredPortraits()
        {
            var package = Compile();
            Assert.That(package.Package.FormatVersion, Is.EqualTo(2));
            Assert.That(package.Rules.Days.Select(d => d.Day), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            Assert.That(package.Rules.Days.Sum(d => d.Meetings.Count), Is.EqualTo(13));
            var roster = new[] { "former-ruler", "battlefield-doctor", "child", "priest", "diplomat" };
            foreach (var day in package.Rules.Days.Skip(1))
                Assert.That(day.Meetings.Select(m => m.PatientId), Is.EqualTo(roster.Take(day.Day + 2)));
            var tutorial = package.Rules.Days[0].Meetings.Single();
            Assert.That(tutorial.NormalRounds, Is.Empty);
            Assert.That(package.Content.GetSegment(tutorial.IntroNodeId!).Lines.Count, Is.EqualTo(9));
            Assert.That(tutorial.ReactionNodes.Keys, Is.EqualTo(new[] { Disposition.Purify }));
            Assert.That(tutorial.DispositionTrustDeltas[Disposition.Purify], Is.Zero);
            Assert.That(package.Content.GetSegment(tutorial.ReactionNodes[Disposition.Purify]).Lines.Count, Is.EqualTo(2));
            Assert.That(package.Package.Portraits.Count, Is.EqualTo(21));
            Assert.That(package.SourceNodes.Select(n => n.PortraitId).Where(p => p.Length > 0).Distinct(), Is.EquivalentTo(new[]
                { "AD01", "AD03", "AD04", "ED02", "ED03", "ED04", "JN02", "JN03", "JN04", "ML01", "ML02", "ML03", "VV01", "VV02", "VV03", "VV04" }));
            var specialMeetings = package.Rules.Days.SelectMany(day => day.Meetings).Where(meeting => meeting.SpecialRound != null).ToArray();
            Assert.That(specialMeetings.Length, Is.EqualTo(7));
            Assert.That(specialMeetings.Select(meeting => meeting.PatientId).Distinct(),
                Is.EquivalentTo(new[] { "child", "former-ruler", "diplomat" }));
            Assert.That(specialMeetings.All(meeting => meeting.SpecialTrustThreshold == 3 && meeting.ReleaseTrustThreshold == 5), Is.True);
            Assert.That(package.Diagnostics, Is.Empty);
            Assert.That(NormalizeNewlines(package.Package.YarnSource), Is.EqualTo(NormalizeNewlines(Source("campaign.yarn"))));
        }

        [Test]
        public void EveryActiveSourceRowMapsToOriginalOrApprovedTextAndExcludedRowsRemainAudited()
        {
            var compiled = Compile();
            var audit = JObject.Parse(Source("provenance.json"));
            var textEdits = JObject.Parse(File.ReadAllText(Path.Combine(Workspace(),
                "Tools/DialogueCampaign/approved-text-edits.json")))["books"]!;
            var mappings = (JArray)audit["mappings"]!;
            var nodes = compiled.SourceNodes.ToDictionary(n => n.Id);
            var options = compiled.SourceNodes.SelectMany(n => n.Options).ToDictionary(o => o.Id);
            Assert.That((int)audit["sourceRowCount"]!, Is.EqualTo(1360));
            Assert.That((int)audit["activeSourceRowCount"]!, Is.EqualTo(1294));
            Assert.That((int)audit["excludedSourceRowCount"]!, Is.EqualTo(66));
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
                        if (patient == "battlefield-doctor" && (string?)row["I"] == "P04_D1_R6_COMMON_002" && field == "open")
                        {
                            Assert.That(originalText, Is.EqualTo("我不记得了"));
                            originalText += "。"; // User-approved punctuation; retain the source workbook text in the audit.
                        }
                        var edit = textEdits[patient]?["edits"]?.SingleOrDefault(item =>
                            (string?)item["id"] == (string?)row[(string)columns["id"]!]
                            && (string?)item["field"] == field);
                        if (edit != null)
                        {
                            Assert.That(originalText, Is.EqualTo((string)edit["expectedText"]!), key + ":" + column);
                            originalText = (string)edit["newText"]!;
                        }
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
            int emittedMappings = 0;
            foreach (var mapping in mappings)
            {
                var id = (string)mapping["sourceId"]!;
                var text = (string)mapping["text"]!;
                if (!(bool)mapping["scheduled"]!)
                {
                    Assert.That(nodes.ContainsKey(id) || options.ContainsKey(id), Is.False,
                        "Later workbook visits remain audited but cannot extend the three-day campaign: " + id);
                    continue;
                }
                emittedMappings++;
                if ((string)mapping["kind"]! == "option")
                {
                    Assert.That(options[id].Text, Is.EqualTo(text), id);
                    Assert.That(options[id].TrustDelta, Is.EqualTo((int)mapping["trustDelta"]!), id);
                }
                else Assert.That(nodes[id].Text, Is.EqualTo(text), id);
            }
            Assert.That(nodes.Values.Count(node => node.Kind == "line" || node.Kind == "narration") + options.Count,
                Is.EqualTo(emittedMappings), "Every scheduled sentence and player option must retain its active source row.");
        }

        [Test]
        public void DiplomatArrivesOnDayThreeWithTheFirstWorkbookVisitAndKeepsUnmodifiedWording()
        {
            var compiled = Compile();
            Assert.That(compiled.Rules.Patients.Single(patient => patient.Id == "diplomat").FirstDay, Is.EqualTo(3));
            Assert.That(compiled.Rules.Days.Where(day => day.Day < 3).SelectMany(day => day.Meetings)
                .Any(meeting => meeting.PatientId == "diplomat"), Is.False);
            var meeting = compiled.Rules.Days.Single(day => day.Day == 3).Meetings.Single(item => item.PatientId == "diplomat");
            Assert.That(meeting.NormalRounds, Is.Not.Empty);
            Assert.That(compiled.Content.TextVariants, Is.Empty);
            foreach (var line in compiled.Content.Segments.Values.SelectMany(segment => segment.Lines))
            {
                Assert.That(compiled.Content.ResolveText(line.SourceNodeId, line.Text, _ => false), Is.EqualTo(line.Text));
                Assert.That(compiled.Content.ResolveText(line.SourceNodeId, line.Text, _ => true), Is.EqualTo(line.Text));
            }
            Assert.That(compiled.Content.IsNarration("diplomat__P05_D1_R2_COMMON_001"), Is.False);
            var introduction = compiled.SourceNodes.Single(node => node.Id == "diplomat__P05_D1_R2_COMMON_001");
            Assert.That(introduction.Kind, Is.EqualTo("line"));
            Assert.That(introduction.Text, Is.EqualTo("阿德里安。贸易代表，来这里商谈工业制品的采购和交付。"));
            Assert.That(compiled.SourceNodes.Where(node => node.Kind == "narration"), Is.Empty);
            Assert.That(compiled.RuntimeYarn, Does.Not.Contain("#kind:narration"));
            Assert.That(compiled.RuntimeYarn, Does.Contain("#source:diplomat__P05_D1_R2_COMMON_001"));
            Assert.That(compiled.SourceNodes.Any(node => node.Id.StartsWith("diplomat__P05_D3_", StringComparison.Ordinal)), Is.False);
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
            Assert.That(response.TrustDelta, Is.EqualTo(-1));
            var optionMapping = audit["mappings"]!.Single(item => (string)item["sourceId"]! == response.Id);
            Assert.That((int)optionMapping["unadjustedTrustDelta"]!, Is.Zero);
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
