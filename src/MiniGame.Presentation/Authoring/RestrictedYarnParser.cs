#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MiniGame.Core;

namespace MiniGame.Presentation.Authoring
{
    /// <summary>A deliberately bounded authoring dialect, not a replacement for the Yarn compiler.</summary>
    internal static class RestrictedYarnParser
    {
        private static readonly Regex Identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_.-]{0,127}$", RegexOptions.CultureInvariant);
        private static readonly Regex Jump = new Regex(@"^<<jump ([A-Za-z_][A-Za-z0-9_.-]{0,127})>>$", RegexOptions.CultureInvariant);
        private static readonly Regex Tag = new Regex(@"(?<!\S)#([A-Za-z_][A-Za-z0-9_]*):([^\s]+)", RegexOptions.CultureInvariant);
        private static readonly HashSet<string> HeaderNames = new HashSet<string>(new[]
            { "title", "kind", "portrait", "round", "patient", "day" }, StringComparer.Ordinal);

        internal static bool IsIdentifier(string value) => value != null && Identifier.IsMatch(value);

        internal static List<AuthoringNode> Parse(string source)
        {
            var sourceLines = source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var result = new List<AuthoringNode>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var optionIds = new HashSet<string>(StringComparer.Ordinal);
            var index = 0;
            while (index < sourceLines.Length)
            {
                while (index < sourceLines.Length && Ignored(sourceLines[index])) index++;
                if (index == sourceLines.Length) break;
                var startLine = index + 1;
                var headers = new Dictionary<string, string>(StringComparer.Ordinal);
                while (index < sourceLines.Length && sourceLines[index].Trim() != "---")
                {
                    var text = sourceLines[index].Trim();
                    if (Ignored(text)) { index++; continue; }
                    var colon = text.IndexOf(':');
                    if (colon < 1) Fail("header", "Expected a supported key: value header or ---.", index + 1, "");
                    var key = text.Substring(0, colon).Trim();
                    var value = text.Substring(colon + 1).Trim();
                    if (!HeaderNames.Contains(key)) Fail("unsupported-header", "Unsupported header: " + key, index + 1, "");
                    if (headers.ContainsKey(key)) Fail("duplicate-header", "Duplicate header: " + key, index + 1, "");
                    headers.Add(key, value); index++;
                }
                var id = Value(headers, "title");
                if (!IsIdentifier(id)) Fail("node-id", "A title with a stable ASCII identifier is required.", startLine, id);
                if (!ids.Add(id)) Fail("duplicate-node", "Duplicate node title.", startLine, id);
                if (index == sourceLines.Length) Fail("node-delimiter", "Missing --- delimiter.", startLine, id);
                index++;
                var body = new List<BodyLine>();
                while (index < sourceLines.Length && sourceLines[index].Trim() != "===")
                {
                    if (!Ignored(sourceLines[index])) body.Add(new BodyLine(sourceLines[index].Trim(), index + 1));
                    index++;
                }
                if (index == sourceLines.Length) Fail("node-delimiter", "Missing === delimiter.", startLine, id);
                index++;
                var round = Value(headers, "round");
                var declaredKind = Value(headers, "kind");
                var portrait = Value(headers, "portrait");
                if (body.Count == 0) Fail("empty-node", "An authoring node cannot be empty.", startLine, id);
                if (body[0].Text.StartsWith("-> ", StringComparison.Ordinal))
                {
                    if (body.Count % 2 != 0) Fail("option-jump", "Each option must have exactly one jump.", startLine, id);
                    var options = new List<AuthoringOption>();
                    for (var item = 0; item < body.Count; item += 2)
                    {
                        var line = body[item];
                        if (!line.Text.StartsWith("-> ", StringComparison.Ordinal))
                            Fail("option", "Expected -> option text followed by its jump.", line.Number, id);
                        var (text, tags) = ParseTags(line.Text.Substring(3), line.Number, id,
                            new[] { "id", "trust", "disposition", "line" });
                        var optionId = Value(tags, "id");
                        if (!IsIdentifier(optionId)) Fail("option-id", "Every option needs #id:STABLE_ID.", line.Number, id);
                        if (!optionIds.Add(optionId)) Fail("duplicate-option", "Option identifiers must be unique across the package.", line.Number, id);
                        Disposition? disposition = null;
                        var delta = 0;
                        if (tags.TryGetValue("disposition", out var dispositionText))
                        {
                            disposition = dispositionText switch
                            {
                                "treat" => MiniGame.Core.Disposition.Treat,
                                "purify" => MiniGame.Core.Disposition.Purify,
                                "release" => MiniGame.Core.Disposition.Release,
                                _ => (MiniGame.Core.Disposition?)null
                            };
                            if (!disposition.HasValue) Fail("disposition", "Use treat, purify or release.", line.Number, id);
                            if (tags.ContainsKey("trust")) Fail("measure-effect", "Disposition options cannot also mutate trust.", line.Number, id);
                        }
                        else if (!tags.TryGetValue("trust", out var deltaText) ||
                            !int.TryParse(deltaText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out delta))
                            Fail("trust-delta", "Normal options require an integer #trust: value.", line.Number, id);
                        options.Add(new AuthoringOption(optionId, text, ParseJump(body[item + 1], id), delta, disposition, line.Number));
                    }
                    var measure = options.Any(o => o.Disposition.HasValue);
                    var kind = measure ? "measure" : "choice";
                    CheckKind(declaredKind, kind, startLine, id);
                    if (options.Count > 4) Fail("option-count", "A choice supports one to four options.", startLine, id);
                    if (options.Any(o => o.Disposition.HasValue != measure))
                        Fail("mixed-options", "A node cannot mix normal and disposition options.", startLine, id);
                    if (measure && (options.Count != 3 || options.Select(o => o.Disposition).Distinct().Count() != 3))
                        Fail("measure-options", "A measure node must offer treat, purify and release exactly once.", startLine, id);
                    result.Add(new AuthoringNode(id, kind, round, portrait, "", "", "", startLine, options, headers));
                }
                else
                {
                    CheckKind(declaredKind, "line", startLine, id);
                    if (body.Count != 2) Fail("line-body", "A line node contains one speaker: text line and one jump.", startLine, id);
                    var colon = body[0].Text.IndexOf(':');
                    if (colon <= 0) Fail("speaker", "A line must begin with speaker: text.", body[0].Number, id);
                    var speaker = body[0].Text.Substring(0, colon).Trim();
                    var (text, _) = ParseTags(body[0].Text.Substring(colon + 1).Trim(), body[0].Number, id, new[] { "line" });
                    if (speaker.IndexOfAny(new[] { '<', '>', '#', '{', '}', '[', ']', '\\' }) >= 0)
                        Fail("speaker", "Speaker names cannot contain Yarn control characters.", body[0].Number, id);
                    result.Add(new AuthoringNode(id, "line", round, portrait, speaker, text,
                        ParseJump(body[1], id), startLine, new List<AuthoringOption>(), headers));
                }
                if (result.Count > 1024) Fail("node-limit", "At most 1024 source nodes are supported.", startLine, id);
            }
            if (result.Count == 0) Fail("empty-source", "No dialogue nodes were found.", 1, "");
            return result;
        }

        private static (string Text, Dictionary<string, string> Tags) ParseTags(string value, int line, string nodeId, IEnumerable<string> allowed)
        {
            var tags = new Dictionary<string, string>(StringComparer.Ordinal);
            var matches = Tag.Matches(value);
            var textEnd = value.Length;
            foreach (Match match in matches)
            {
                if (textEnd == value.Length) textEnd = match.Index;
                var name = match.Groups[1].Value;
                if (!allowed.Contains(name)) Fail("unsupported-tag", "Unsupported metadata: " + name, line, nodeId);
                if (tags.ContainsKey(name)) Fail("duplicate-tag", "Duplicate metadata: " + name, line, nodeId);
                tags.Add(name, match.Groups[2].Value);
            }
            var suffix = value.Substring(textEnd);
            if (Tag.Replace(suffix, "").Trim().Length != 0)
                Fail("metadata", "Metadata must appear after all dialogue text.", line, nodeId);
            var raw = value.Substring(0, textEnd).TrimEnd();
            if (raw.Contains("//"))
                Fail("inline-comment", "Inline // comments are unsupported; put comments on their own line.", line, nodeId);
            var decoded = new StringBuilder();
            for (var index = 0; index < raw.Length; index++)
            {
                var ch = raw[index];
                if (ch == '\\')
                {
                    if (++index >= raw.Length || "\\#<>[]{}".IndexOf(raw[index]) < 0)
                        Fail("escape", "Supported escapes are backslash and Yarn control punctuation.", line, nodeId);
                    decoded.Append(raw[index]);
                }
                else
                {
                    if ("#<>[]{}".IndexOf(ch) >= 0) Fail("unsupported-syntax", "Escape literal Yarn punctuation; expressions and markup are unsupported.", line, nodeId);
                    decoded.Append(ch);
                }
            }
            if (decoded.Length == 0) Fail("empty-text", "Dialogue and option text cannot be empty.", line, nodeId);
            return (decoded.ToString(), tags);
        }

        private static string ParseJump(BodyLine line, string nodeId)
        {
            var match = Jump.Match(line.Text);
            if (!match.Success) Fail("unsupported-command", "Only <<jump STABLE_NODE_ID>> is supported in source nodes.", line.Number, nodeId);
            return match.Groups[1].Value;
        }

        private static bool Ignored(string value) => string.IsNullOrWhiteSpace(value) || value.TrimStart().StartsWith("//", StringComparison.Ordinal);
        private static string Value(Dictionary<string, string> values, string name) => values.TryGetValue(name, out var value) ? value : "";
        private static void CheckKind(string declared, string actual, int line, string id)
        { if (declared.Length != 0 && declared != actual) Fail("node-kind", "The kind header disagrees with the node body.", line, id); }
        internal static void Fail(string code, string message, int line = 0, string nodeId = "")
            => throw new DialoguePackageException(new DialogueDiagnostic(code, message, line, nodeId));

        private readonly struct BodyLine
        {
            internal string Text { get; }
            internal int Number { get; }
            internal BodyLine(string text, int number) { Text = text; Number = number; }
        }
    }
}
