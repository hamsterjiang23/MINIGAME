#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniGame.Presentation.Authoring
{
    /// <summary>V2 compiles independently bounded meetings into one authoritative campaign ruleset.</summary>
    internal static class CampaignPackageCompiler
    {
        internal static bool IsCampaign(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (json.Length > 16 * 1024 * 1024) RestrictedYarnParser.Fail("package-size", "The campaign exceeds 16 MiB.");
            try
            {
                using var buffer = new StringReader(json);
                using var reader = new JsonTextReader(buffer) { MaxDepth = 32, DateParseHandling = DateParseHandling.None };
                var root = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read()) RestrictedYarnParser.Fail("json", "Additional content after the package is unsupported.");
                return root["formatVersion"]?.Type == JTokenType.Integer && (int)root["formatVersion"]! == 2;
            }
            catch (JsonException error) { throw new DialoguePackageException(new DialogueDiagnostic("json", error.Message)); }
        }

        internal static CompiledDialoguePackage Compile(string json)
        {
            DialoguePackage package;
            try
            {
                package = JsonConvert.DeserializeObject<DialoguePackage>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error, TypeNameHandling = TypeNameHandling.None,
                    MaxDepth = 32, DateParseHandling = DateParseHandling.None
                }) ?? throw new JsonException("Empty package.");
            }
            catch (JsonException error) { throw new DialoguePackageException(new DialogueDiagnostic("json", error.Message)); }
            var campaign = package.Campaign;
            if (campaign == null) RestrictedYarnParser.Fail("campaign", "V2 requires a campaign definition.");
            if (!RestrictedYarnParser.IsIdentifier(package.PackageId) || string.IsNullOrWhiteSpace(package.ContentVersion) || package.ContentVersion.Length > 128)
                RestrictedYarnParser.Fail("package-id", "A valid package ID and content version are required.");
            if (campaign!.Patients.Count == 0 || campaign.Patients.Count > 64 || campaign.Days.Count == 0 || campaign.Days.Count > 64)
                RestrictedYarnParser.Fail("campaign-size", "Campaigns require 1–64 patients and days.");
            var portraits = new HashSet<string>(StringComparer.Ordinal);
            foreach (var portrait in package.Portraits)
                if (!RestrictedYarnParser.IsIdentifier(portrait.Id) || !portraits.Add(portrait.Id) || string.IsNullOrWhiteSpace(portrait.Label))
                    RestrictedYarnParser.Fail("portrait-id", "Portrait IDs must be unique and have labels.");
            var patients = new List<PatientDefinition>();
            foreach (var patient in campaign.Patients)
            {
                if (!RestrictedYarnParser.IsIdentifier(patient.Id) ||
                    !Enum.TryParse<PatientPresence>(patient.TreatmentOutcome, out var outcome))
                    RestrictedYarnParser.Fail("patient", "Patient ID or treatment outcome is invalid.");
                Enum.TryParse<PatientPresence>(patient.TreatmentOutcome, out outcome);
                patients.Add(new PatientDefinition(patient.Id, patient.Name, patient.FirstDay, patient.InitialTrust,
                    patient.TreatmentLimit, outcome, patient.TreatmentDelayDays, patient.AttacksAfterRelease));
            }
            var segments = new Dictionary<string, DialogueSegment>(StringComparer.Ordinal);
            var sourceNodes = new List<AuthoringNode>();
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            var days = new List<DayDefinition>();
            foreach (var day in campaign.Days)
            {
                var meetings = new List<MeetingDefinition>();
                foreach (var meeting in day.Meetings)
                {
                    if (meeting.YarnSource.Length > 2 * 1024 * 1024)
                        RestrictedYarnParser.Fail("source-size", "A meeting exceeds 2 MiB.");
                    var nodes = RestrictedYarnParser.Parse(meeting.YarnSource, campaign: true);
                    sourceNodes.AddRange(nodes);
                    if (sourceNodes.Count > 8192) RestrictedYarnParser.Fail("campaign-node-limit", "A campaign exceeds 8192 source nodes.");
                    foreach (var node in nodes)
                    {
                        if (!identifiers.Add(node.Id)) RestrictedYarnParser.Fail("duplicate-node", "Campaign node IDs must be globally unique.", node.SourceLine, node.Id);
                        foreach (var option in node.Options)
                            if (!identifiers.Add(option.Id)) RestrictedYarnParser.Fail("duplicate-option", "Campaign options must be globally unique.", option.SourceLine, option.Id);
                        if (node.PortraitId.Length != 0 && !portraits.Contains(node.PortraitId))
                            RestrictedYarnParser.Fail("unknown-portrait", "The portrait ID is undeclared.", node.SourceLine, node.Id);
                        if (!node.Headers.TryGetValue("patient", out var patientId) || patientId != meeting.PatientId ||
                            !node.Headers.TryGetValue("day", out var dayText) || dayText != day.Day.ToString(System.Globalization.CultureInfo.InvariantCulture))
                            RestrictedYarnParser.Fail("meeting-header", "Every node must identify its configured patient and day.", node.SourceLine, node.Id);
                    }
                    meetings.Add(CompileMeeting(meeting, nodes, campaign.ReleaseThreshold, segments));
                }
                days.Add(new DayDefinition(day.Day, day.Quota, meetings));
            }
            var canonicalSources = string.Join("\n", campaign.Days.SelectMany(d => d.Meetings).Select(m => m.YarnSource));
            if (package.YarnSource != canonicalSources)
                RestrictedYarnParser.Fail("campaign-source", "Top-level Yarn must exactly concatenate the ordered meeting sources.");
            var variantIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var variant in campaign.Variants)
            {
                if (!identifiers.Contains(variant.SourceId) || !variantIds.Add(variant.SourceId) || string.IsNullOrWhiteSpace(variant.FallbackText))
                    RestrictedYarnParser.Fail("text-variant", "Variants require a unique existing source ID and nonempty fallback.");
                if (variant.Predicate != "other_patient_exited_yesterday" && variant.Predicate != "asked_diplomat_for_evidence_yesterday")
                    RestrictedYarnParser.Fail("variant-predicate", "Unknown historical predicate: " + variant.Predicate);
            }
            ValidateTrust(patients, days);
            var rules = new GameRules("campaign:" + package.PackageId + ":" + package.ContentVersion, patients, days,
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            // Include rules and wording variants in the revision; equal Yarn with changed rules must invalidate old saves.
            var sourceHash = DialoguePackageCompiler.Hash(JsonConvert.SerializeObject(package));
            var content = new PackageGameContent(rules, package.PackageId + ":" + package.ContentVersion + ":" + sourceHash + ":" + rules.Fingerprint,
                segments, campaign.Variants);
            return new CompiledDialoguePackage(package, rules, content, DialoguePackageCompiler.BuildRuntimeYarn(content), sourceHash,
                sourceNodes, new List<DialogueDiagnostic>());
        }

        private static MeetingDefinition CompileMeeting(CampaignMeeting meeting, List<AuthoringNode> nodes, int releaseThreshold,
            Dictionary<string, DialogueSegment> segments)
        {
            var map = nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
            if (!map.ContainsKey(meeting.StartNode) || map.ContainsKey(meeting.EndNode) || !RestrictedYarnParser.IsIdentifier(meeting.EndNode))
                RestrictedYarnParser.Fail("meeting-boundary", "Each meeting needs a valid start and a distinct external end.");
            if ((meeting.SpecialRoundStartNode != null) != meeting.SpecialTrustThreshold.HasValue)
                RestrictedYarnParser.Fail("special-metadata", "A special round requires both an entry node and a trust threshold.");
            if (meeting.SpecialRoundStartNode != null &&
                (!map.TryGetValue(meeting.SpecialRoundStartNode, out var specialEntry) || specialEntry.Kind == "measure"))
                RestrictedYarnParser.Fail("special-entry", "The special round must start at an existing line or choice node.");
            var active = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            Visit(meeting.StartNode, meeting.EndNode, map, active, visited);
            if (visited.Count != nodes.Count) RestrictedYarnParser.Fail("unreachable", "Every V2 source node must be reachable within its meeting.");
            if (nodes.Count(n => n.Kind == "measure") != 1) RestrictedYarnParser.Fail("measure-count", "Each meeting requires exactly one measure node.");
            var rounds = new List<RoundDefinition>();
            RoundDefinition? specialRound = null;
            var roundNames = new HashSet<string>(StringComparer.Ordinal);
            var cursor = meeting.StartNode;
            string? introNodeId = null;
            while (map[cursor].Kind != "measure")
            {
                var entry = map[cursor];
                var isSpecial = entry.Id == meeting.SpecialRoundStartNode;
                if (isSpecial && rounds.Count == 0)
                    RestrictedYarnParser.Fail("special-position", "The special round must follow ordinary dialogue.", entry.SourceLine, entry.Id);
                var intro = new List<DialogueLine>();
                var choice = entry;
                while (IsLine(choice))
                {
                    if (choice.Id == meeting.SpecialRoundStartNode && !isSpecial)
                        RestrictedYarnParser.Fail("special-entry", "The special entry must begin its own choice group.", choice.SourceLine, choice.Id);
                    intro.Add(new DialogueLine(choice));
                    if (!map.TryGetValue(choice.Next, out var next)) RestrictedYarnParser.Fail("intro-end", "An introduction must reach a choice or measure gate.", choice.SourceLine, choice.Id);
                    choice = next!;
                }
                if (choice.Kind == "measure")
                {
                    if (rounds.Count != 0 || intro.Count == 0) RestrictedYarnParser.Fail("intro-route", "A standalone introduction is supported only before a meeting with no ordinary choices.");
                    introNodeId = entry.Id;
                    Add(segments, new DialogueSegment(entry.Id, intro, new List<ResponseContent>()));
                    cursor = choice.Id;
                    break;
                }
                if (choice.Kind != "choice" || entry.Round.Length == 0 ||
                    (!isSpecial && !roundNames.Add(entry.Round)) || choice.Round != entry.Round)
                    RestrictedYarnParser.Fail("round-choice", "Each distinct round must lead to one choice in the same round.", entry.SourceLine, entry.Id);
                var responses = new List<ResponseDefinition>();
                var labels = new List<ResponseContent>();
                string? boundary = null;
                foreach (var option in choice.Options)
                {
                    var lines = new List<DialogueLine>();
                    var next = FollowBranch(option.Next, entry.Round, meeting.EndNode, meeting.SpecialRoundStartNode, map, lines);
                    if (next == meeting.EndNode || (boundary != null && boundary != next))
                        RestrictedYarnParser.Fail("nonconvergent-round", "All options must converge at the same subsequent choice group or measure.", option.SourceLine, choice.Id);
                    boundary = next;
                    var id = option.Id + ".branch";
                    Add(segments, new DialogueSegment(id, lines, new List<ResponseContent>()));
                    responses.Add(new ResponseDefinition(option.Id, id, option.TrustDelta));
                    labels.Add(new ResponseContent(option.Id, option.Text));
                }
                var round = new RoundDefinition(entry.Id, responses);
                if (isSpecial)
                {
                    if (map[boundary!].Kind != "measure")
                        RestrictedYarnParser.Fail("special-position", "The special round must follow ordinary dialogue and converge directly at the measure gate.", entry.SourceLine, entry.Id);
                    specialRound = round;
                }
                else rounds.Add(round);
                Add(segments, new DialogueSegment(entry.Id, intro, labels));
                cursor = boundary!;
                if (rounds.Count + (specialRound == null ? 0 : 1) > 128)
                    RestrictedYarnParser.Fail("round-limit", "A meeting exceeds 128 choice groups.");
            }
            if (meeting.SpecialRoundStartNode != null && specialRound == null)
                RestrictedYarnParser.Fail("special-entry", "The configured special entry was not compiled as a choice group.");
            var reactions = new Dictionary<Disposition, string>();
            var deltas = new Dictionary<Disposition, int>();
            foreach (var option in map[cursor].Options)
            {
                var lines = new List<DialogueLine>();
                var next = option.Next;
                while (next != meeting.EndNode)
                {
                    var node = map[next];
                    if (!IsLine(node)) RestrictedYarnParser.Fail("reaction-route", "Reactions must be linear and terminate at this meeting boundary.", node.SourceLine, node.Id);
                    lines.Add(new DialogueLine(node)); next = node.Next;
                }
                if (lines.Count == 0) RestrictedYarnParser.Fail("reaction-empty", "Every disposition needs a reaction.");
                var id = option.Id + ".reaction";
                reactions.Add(option.Disposition!.Value, id);
                deltas.Add(option.Disposition.Value, option.TrustDelta);
                Add(segments, new DialogueSegment(id, lines, new List<ResponseContent>()));
            }
            return new MeetingDefinition(meeting.PatientId, rounds, specialRound, meeting.SpecialTrustThreshold ?? int.MaxValue,
                meeting.ReleaseThreshold ?? releaseThreshold,
                reactions, introNodeId: introNodeId, dispositionTrustDeltas: deltas);
        }

        private static bool IsLine(AuthoringNode node) => node.Kind == "line" || node.Kind == "narration";

        private static string FollowBranch(string cursor, string round, string end, string? specialEntry,
            Dictionary<string, AuthoringNode> map, List<DialogueLine> lines)
        {
            while (cursor != end)
            {
                var node = map[cursor];
                if (cursor == specialEntry) return cursor;
                if (node.Kind == "measure") return cursor;
                if (node.Round != round)
                {
                    // A decision preamble is text, not a fabricated extra player choice.
                    var probe = node;
                    while (IsLine(probe) && probe.Next != end) probe = map[probe.Next];
                    if (probe.Kind != "measure") return cursor;
                }
                if (!IsLine(node)) RestrictedYarnParser.Fail("nested-choice", "Response branches cannot contain another choice in the same round.", node.SourceLine, node.Id);
                lines.Add(new DialogueLine(node)); cursor = node.Next;
            }
            return cursor;
        }

        private static void Visit(string id, string end, Dictionary<string, AuthoringNode> map, HashSet<string> active, HashSet<string> visited)
        {
            if (id == end || visited.Contains(id)) return;
            if (!map.TryGetValue(id, out var node)) RestrictedYarnParser.Fail("missing-target", "Unknown meeting target: " + id);
            if (!active.Add(id)) RestrictedYarnParser.Fail("cycle", "Cycles are unsupported.", node!.SourceLine, id);
            foreach (var target in IsLine(node!) ? new[] { node!.Next } : node!.Options.Select(o => o.Next)) Visit(target, end, map, active, visited);
            active.Remove(id); visited.Add(id);
        }

        private static void Add(Dictionary<string, DialogueSegment> segments, DialogueSegment segment)
        {
            if (segments.ContainsKey(segment.CoreNodeId)) RestrictedYarnParser.Fail("derived-id-collision", "Derived segment IDs collide: " + segment.CoreNodeId);
            segments.Add(segment.CoreNodeId, segment);
        }

        private static void ValidateTrust(List<PatientDefinition> patients, List<DayDefinition> days)
        {
            foreach (var patient in patients)
            {
                long minimum = patient.InitialTrust, maximum = minimum;
                foreach (var meeting in days.SelectMany(d => d.Meetings).Where(m => m.PatientId == patient.Id))
                {
                    foreach (var round in meeting.NormalRounds)
                    {
                        minimum += round.Responses.Min(r => r.TrustDelta);
                        maximum += round.Responses.Max(r => r.TrustDelta);
                        if (minimum < int.MinValue || maximum > int.MaxValue)
                            RestrictedYarnParser.Fail("trust-overflow", "A campaign response would overflow Int32 before a later response can offset it.", nodeId: round.NodeId);
                    }
                    if (meeting.SpecialRound != null && maximum >= meeting.SpecialTrustThreshold)
                    {
                        var round = meeting.SpecialRound;
                        var specialMinimum = Math.Max(minimum, meeting.SpecialTrustThreshold) + round.Responses.Min(r => r.TrustDelta);
                        var specialMaximum = maximum + round.Responses.Max(r => r.TrustDelta);
                        if (specialMinimum < int.MinValue || specialMaximum > int.MaxValue)
                            RestrictedYarnParser.Fail("trust-overflow", "A campaign special response would overflow Int32.", nodeId: round.NodeId);
                        if (minimum < meeting.SpecialTrustThreshold)
                        {
                            // Low-trust paths skip the special round and keep their previous value.
                            maximum = Math.Max((long)meeting.SpecialTrustThreshold - 1, specialMaximum);
                            minimum = Math.Min(minimum, specialMinimum);
                        }
                        else
                        {
                            minimum = specialMinimum;
                            maximum = specialMaximum;
                        }
                    }
                    // Disposition deltas only affect the current instrument reading
                    // (computed with Int64); they never alter later dialogue bounds.
                }
            }
        }
    }
}
