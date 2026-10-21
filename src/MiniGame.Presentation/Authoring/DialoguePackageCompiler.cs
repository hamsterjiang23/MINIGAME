#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MiniGame.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniGame.Presentation.Authoring
{
    /// <summary>Validates the terminal dialect and derives rules, presentation segments and native runtime Yarn.</summary>
    public static class DialoguePackageCompiler
    {
        public static CompiledDialoguePackage Compile(string json)
        {
            if (CampaignPackageCompiler.IsCampaign(json)) return CampaignPackageCompiler.Compile(json);
            var package = ReadPackage(json);
            var nodes = RestrictedYarnParser.Parse(package.YarnSource);
            var diagnostics = new List<DialogueDiagnostic>();
            var map = nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
            ValidateGraph(package, nodes, map, diagnostics);
            var segments = new Dictionary<string, DialogueSegment>(StringComparer.Ordinal);
            var rounds = new List<RoundDefinition>();
            var cursor = package.Profile.StartNode;
            var roundNames = new HashSet<string>(StringComparer.Ordinal);
            while (cursor != package.Profile.EndNode)
            {
                var entry = map[cursor];
                if (entry.Kind == "measure") break;
                if (entry.Round.Length == 0 || !roundNames.Add(entry.Round))
                    RestrictedYarnParser.Fail("round", "Each sequential group must have a distinct round header.", entry.SourceLine, entry.Id);
                var intro = new List<DialogueLine>();
                var choice = FollowIntro(map, entry, intro);
                var responses = new List<ResponseDefinition>();
                var labels = new List<ResponseContent>();
                string? nextRound = null;
                foreach (var option in choice.Options)
                {
                    var branchLines = new List<DialogueLine>();
                    var boundary = FollowBranch(map, option.Next, entry.Round, package.Profile.EndNode, branchLines);
                    if (boundary == package.Profile.EndNode)
                        RestrictedYarnParser.Fail("early-end", "Normal options must reach another group or the measure node.", option.SourceLine, choice.Id);
                    if (nextRound != null && nextRound != boundary)
                        RestrictedYarnParser.Fail("nonconvergent-round", "All options in a group must reach the same next group boundary.", option.SourceLine, choice.Id);
                    nextRound = boundary;
                    var branchId = option.Id + ".branch";
                    AddSegment(segments, new DialogueSegment(branchId, branchLines, new List<ResponseContent>()), choice);
                    responses.Add(new ResponseDefinition(option.Id, branchId, option.TrustDelta));
                    labels.Add(new ResponseContent(option.Id, option.Text));
                }
                rounds.Add(new RoundDefinition(entry.Id, responses));
                AddSegment(segments, new DialogueSegment(entry.Id, intro, labels), entry);
                cursor = nextRound ?? throw new InvalidOperationException("A validated choice has no options.");
                if (rounds.Count > 128) RestrictedYarnParser.Fail("round-limit", "At most 128 sequential groups are supported.", entry.SourceLine, entry.Id);
            }
            if (rounds.Count == 0 || cursor == package.Profile.EndNode || map[cursor].Kind != "measure")
                RestrictedYarnParser.Fail("measure-route", "The dialogue must contain a normal group and end at one measure node.");
            var measure = map[cursor];
            var reactions = new Dictionary<Disposition, string>();
            foreach (var option in measure.Options)
            {
                var lines = FollowReaction(map, option.Next, package.Profile.EndNode);
                var id = option.Id + ".reaction";
                reactions.Add(option.Disposition!.Value, id);
                AddSegment(segments, new DialogueSegment(id, lines, new List<ResponseContent>()), measure);
            }
            if (nodes.Count(n => n.Kind == "measure") != 1)
                RestrictedYarnParser.Fail("measure-count", "The package must contain exactly one measure node.");
            var profile = package.Profile;
            long minTrust = profile.InitialTrust;
            long maxTrust = profile.InitialTrust;
            foreach (var round in rounds)
            {
                minTrust += round.Responses.Min(r => r.TrustDelta);
                maxTrust += round.Responses.Max(r => r.TrustDelta);
                if (minTrust < int.MinValue || maxTrust > int.MaxValue)
                    RestrictedYarnParser.Fail("trust-overflow", "At least one response route would overflow the core trust integer.", map[round.NodeId].SourceLine, round.NodeId);
            }
            var meeting = new MeetingDefinition(profile.PatientId, rounds, null, int.MaxValue,
                profile.ReleaseThreshold, reactions);
            var patient = new PatientDefinition(profile.PatientId, profile.PatientName, 1, profile.InitialTrust,
                null, PatientPresence.TreatmentEnded);
            var rules = new GameRules("terminal-demo:" + package.PackageId + ":" + package.ContentVersion,
                new[] { patient }, new[] { new DayDefinition(1, profile.InitialQuota, new[] { meeting }) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var sourceHash = Hash(package.YarnSource);
            var revision = package.PackageId + ":" + package.ContentVersion + ":" + sourceHash + ":" + rules.Fingerprint;
            var content = new PackageGameContent(rules, revision, segments);
            return new CompiledDialoguePackage(package, rules, content, BuildRuntimeYarn(content), sourceHash, nodes, diagnostics);
        }

        private static DialoguePackage ReadPackage(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (json.Length > 4 * 1024 * 1024) RestrictedYarnParser.Fail("package-size", "The JSON package exceeds the 4 MiB character limit.");
            DialoguePackage? package;
            try
            {
                using var buffer = new StringReader(json);
                using var reader = new JsonTextReader(buffer) { MaxDepth = 32, DateParseHandling = DateParseHandling.None };
                var token = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read()) RestrictedYarnParser.Fail("json", "Additional content after the JSON package is unsupported.");
                RequireTypes(token, new[] { "formatVersion" }, JTokenType.Integer);
                RequireTypes(token, new[] { "packageId", "contentVersion", "yarnSource" }, JTokenType.String);
                if (!(token["profile"] is JObject profile)) RestrictedYarnParser.Fail("profile", "A profile object is required.");
                else
                {
                    RequireTypes(profile, new[] { "day", "initialTrust", "releaseThreshold", "initialQuota" }, JTokenType.Integer);
                    RequireTypes(profile, new[] { "patientId", "patientName", "startNode", "endNode" }, JTokenType.String);
                }
                if (!(token["portraits"] is JArray portraits)) RestrictedYarnParser.Fail("portraits", "A portraits array is required.");
                else foreach (var item in portraits)
                {
                    if (!(item is JObject portrait)) RestrictedYarnParser.Fail("portrait", "A portrait must be an object.");
                    else RequireTypes(portrait, new[] { "id", "label" }, JTokenType.String);
                }
                package = token.ToObject<DialoguePackage>(JsonSerializer.Create(new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error, TypeNameHandling = TypeNameHandling.None,
                    MaxDepth = 32, DateParseHandling = DateParseHandling.None
                }));
            }
            catch (JsonException error)
            { throw new DialoguePackageException(new DialogueDiagnostic("json", error.Message)); }
            if (package == null) throw new DialoguePackageException(new DialogueDiagnostic("json", "The package is empty."));
            if (package.FormatVersion != 1) RestrictedYarnParser.Fail("format-version", "Only formatVersion 1 is supported.");
            if (!RestrictedYarnParser.IsIdentifier(package.PackageId) || string.IsNullOrWhiteSpace(package.ContentVersion) || package.ContentVersion.Length > 128)
                RestrictedYarnParser.Fail("package-id", "A stable packageId and nonempty contentVersion of at most 128 characters are required.");
            if (package.YarnSource.Length > 2 * 1024 * 1024) RestrictedYarnParser.Fail("source-size", "The Yarn source exceeds the 2 MiB character limit.");
            var settings = package.Profile;
            if (settings.Day != 1) RestrictedYarnParser.Fail("profile-day", "The independent formatVersion 1 demo supports day 1 only.");
            if (!RestrictedYarnParser.IsIdentifier(settings.PatientId) || !RestrictedYarnParser.IsIdentifier(settings.StartNode) ||
                !RestrictedYarnParser.IsIdentifier(settings.EndNode) || string.IsNullOrWhiteSpace(settings.PatientName))
                RestrictedYarnParser.Fail("profile-id", "Profile identifiers and patientName are required.");
            if (settings.InitialQuota < 0) RestrictedYarnParser.Fail("profile-quota", "initialQuota cannot be negative.");
            var portraitIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var portrait in package.Portraits)
                if (!RestrictedYarnParser.IsIdentifier(portrait.Id) || !portraitIds.Add(portrait.Id) || string.IsNullOrWhiteSpace(portrait.Label))
                    RestrictedYarnParser.Fail("portrait-id", "Portrait IDs must be valid, unique, and have nonempty labels.");
            return package;
        }

        private static void RequireTypes(JObject obj, IEnumerable<string> names, JTokenType type)
        {
            foreach (var name in names)
                if (obj[name]?.Type != type) RestrictedYarnParser.Fail("json-type", name + " must be a " + type + ".");
        }

        private static void ValidateGraph(DialoguePackage package, List<AuthoringNode> nodes,
            Dictionary<string, AuthoringNode> map, List<DialogueDiagnostic> diagnostics)
        {
            var profile = package.Profile;
            if (!map.ContainsKey(profile.StartNode)) RestrictedYarnParser.Fail("start-node", "profile.startNode is missing.");
            if (map.ContainsKey(profile.EndNode) || profile.StartNode == profile.EndNode)
                RestrictedYarnParser.Fail("end-node", "profile.endNode must be a distinct external terminal identifier.");
            var portraits = new HashSet<string>(package.Portraits.Select(p => p.Id), StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                if ((node.Kind == "line" || node.PortraitId.Length != 0) && !portraits.Contains(node.PortraitId))
                    RestrictedYarnParser.Fail("unknown-portrait", "The node needs a declared portrait ID.", node.SourceLine, node.Id);
                if (node.Kind != "measure" && node.Round.Length == 0)
                    RestrictedYarnParser.Fail("round", "Normal line and choice nodes require a round header.", node.SourceLine, node.Id);
                if (node.Headers.TryGetValue("patient", out var patient) && patient != profile.PatientId)
                    RestrictedYarnParser.Fail("patient-header", "The patient header must match profile.patientId.", node.SourceLine, node.Id);
                if (node.Headers.TryGetValue("day", out var day) &&
                    (!int.TryParse(day, NumberStyles.None, CultureInfo.InvariantCulture, out var dayNumber) || dayNumber != profile.Day))
                    RestrictedYarnParser.Fail("day-header", "The day header must match profile.day.", node.SourceLine, node.Id);
                foreach (var target in Targets(node))
                    if (target != profile.EndNode && !map.ContainsKey(target))
                        RestrictedYarnParser.Fail("missing-target", "Jump target does not exist: " + target, node.SourceLine, node.Id);
                if (node.Text.Length > 200) diagnostics.Add(new DialogueDiagnostic("long-text", "Long text may overflow the terminal viewport.", node.SourceLine, node.Id, true));
            }
            var colors = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var node in nodes) Visit(node.Id, map, profile.EndNode, colors);
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            MarkReachable(profile.StartNode, map, profile.EndNode, reachable);
            foreach (var node in nodes.Where(n => !reachable.Contains(n.Id)))
                diagnostics.Add(new DialogueDiagnostic("unreachable", "The node cannot be reached from profile.startNode.", node.SourceLine, node.Id, true));
        }

        private static void Visit(string id, Dictionary<string, AuthoringNode> map, string end, Dictionary<string, int> colors)
        {
            if (id == end) return;
            if (colors.TryGetValue(id, out var color))
            {
                if (color == 1) RestrictedYarnParser.Fail("cycle", "Cycles are unsupported in the terminal dialogue dialect.", map[id].SourceLine, id);
                return;
            }
            colors[id] = 1;
            foreach (var target in Targets(map[id])) Visit(target, map, end, colors);
            colors[id] = 2;
        }

        private static void MarkReachable(string id, Dictionary<string, AuthoringNode> map, string end, HashSet<string> reachable)
        {
            if (id == end || !reachable.Add(id)) return;
            foreach (var target in Targets(map[id])) MarkReachable(target, map, end, reachable);
        }

        private static IEnumerable<string> Targets(AuthoringNode node) => node.Kind == "line" ? new[] { node.Next } : node.Options.Select(o => o.Next);

        private static AuthoringNode FollowIntro(Dictionary<string, AuthoringNode> map, AuthoringNode entry, List<DialogueLine> lines)
        {
            var node = entry;
            while (node.Kind == "line")
            {
                if (node.Round != entry.Round) RestrictedYarnParser.Fail("intro-round", "A round intro must reach its own choice before another round.", node.SourceLine, node.Id);
                lines.Add(new DialogueLine(node));
                if (!map.TryGetValue(node.Next, out var successor)) RestrictedYarnParser.Fail("intro-end", "A round intro cannot jump directly to the terminal.", node.SourceLine, node.Id);
                node = successor!;
            }
            if (node.Kind != "choice" || node.Round != entry.Round)
                RestrictedYarnParser.Fail("round-choice", "Each normal group must lead to one choice in the same round.", node.SourceLine, node.Id);
            return node;
        }

        private static string FollowBranch(Dictionary<string, AuthoringNode> map, string cursor, string round,
            string end, List<DialogueLine> lines)
        {
            while (cursor != end)
            {
                var node = map[cursor];
                if (node.Kind == "measure" || node.Round != round) return cursor;
                if (node.Kind != "line") RestrictedYarnParser.Fail("nested-choice", "A group response must contain only lines until the next round.", node.SourceLine, node.Id);
                lines.Add(new DialogueLine(node)); cursor = node.Next;
            }
            return cursor;
        }

        private static List<DialogueLine> FollowReaction(Dictionary<string, AuthoringNode> map, string cursor, string end)
        {
            var lines = new List<DialogueLine>();
            while (cursor != end)
            {
                var node = map[cursor];
                if (node.Kind != "line") RestrictedYarnParser.Fail("reaction-route", "Disposition reactions must contain only lines and reach profile.endNode.", node.SourceLine, node.Id);
                lines.Add(new DialogueLine(node)); cursor = node.Next;
            }
            if (lines.Count == 0) RestrictedYarnParser.Fail("reaction-empty", "Each disposition requires at least one reaction line.");
            return lines;
        }

        private static void AddSegment(Dictionary<string, DialogueSegment> segments, DialogueSegment segment, AuthoringNode source)
        {
            if (segments.ContainsKey(segment.CoreNodeId))
                RestrictedYarnParser.Fail("derived-id-collision", "A source or option identifier collides with a generated core segment ID: " + segment.CoreNodeId,
                    source.SourceLine, source.Id);
            segments.Add(segment.CoreNodeId, segment);
        }

        internal static string BuildRuntimeYarn(PackageGameContent content)
        {
            var source = new StringBuilder();
            source.AppendLine("// Derived from the canonical package Yarn; edit the authoring source, not this file.");
            source.AppendLine("title: RuntimeDeclarations\n---");
            source.AppendLine("<<declare " + PrototypeYarnSource.CueTextVariable + " = \"\">>\n===\n");
            foreach (var node in content.Nodes)
            {
                source.AppendLine("title: " + PrototypeYarnSource.NodeName(node.NodeId)); source.AppendLine("---");
                if (content.TryGetSegment(node.NodeId, out var segment) && segment != null)
                {
                    for (var index = 0; index < segment.Lines.Count; index++)
                    {
                        var line = segment.Lines[index];
                        source.AppendLine(Escape(line.Speaker) + ": " + Escape(line.Text)
                            + " #line:mg_" + PrototypeYarnSource.EncodeIdentifier(node.NodeId) + "_" + index.ToString(CultureInfo.InvariantCulture)
                            + " #source:" + line.SourceNodeId + (line.PortraitId.Length == 0 ? "" : " #portrait:" + line.PortraitId)
                            + (line.IsNarration ? " #kind:narration" : ""));
                    }
                    // Yarn prunes empty nodes; keep silent response branches executable without inventing dialogue.
                    if (segment.Lines.Count == 0 && node.Responses.Count == 0)
                        source.AppendLine("<<stop>>");
                }
                else source.AppendLine(Escape(node.Speaker) + ": " + Escape(node.Text));
                foreach (var option in node.Responses)
                {
                    var code = PrototypeYarnSource.EncodeIdentifier(option.Id);
                    source.AppendLine("-> " + Escape(option.Text) + " #" + PrototypeYarnSource.ResponseMetadataPrefix + code + " #source:" + option.Id);
                    source.AppendLine("    <<select_response \"" + PrototypeYarnSource.EncodeIdentifier(node.NodeId) + "\" \"" + code + "\">>");
                }
                source.AppendLine("===\n");
            }
            foreach (var kind in new[] { PresentationKind.MorningInspection, PresentationKind.Briefing, PresentationKind.Dream, PresentationKind.Ending })
            {
                source.AppendLine("title: " + PrototypeYarnSource.CueNodeName(kind)); source.AppendLine("---");
                source.AppendLine("终端: {" + PrototypeYarnSource.CueTextVariable + "}\n===\n");
            }
            return source.ToString();
        }

        private static string Escape(string value)
        {
            var result = new StringBuilder();
            foreach (var ch in value)
            {
                if ("\\#<>[]{}".IndexOf(ch) >= 0) result.Append('\\');
                result.Append(ch == '\r' || ch == '\n' ? ' ' : ch);
            }
            return result.ToString();
        }

        internal static string Hash(string source)
        {
            using var hash = SHA256.Create();
            return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(source)).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
