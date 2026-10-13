#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MiniGame.Presentation
{
    /// <summary>Produces original Yarn source; the mapping never changes a core identifier.</summary>
    public static class PrototypeYarnSource
    {
        public const string CueTextVariable = "$minigame_cue_text";
        public const string ResponseMetadataPrefix = "response_";

        // Hex UTF-8 is injective and uses only characters accepted by Yarn node names and commands.
        public static string EncodeIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("An identifier is required.", nameof(value));
            return string.Concat(Encoding.UTF8.GetBytes(value).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }

        public static string NodeName(string coreNodeId) => "Core_" + EncodeIdentifier(coreNodeId);
        public static string CueNodeName(PresentationKind kind) => "Cue_" + kind;

        public static string Build(PrototypeContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var source = new StringBuilder();
            source.AppendLine("// Original Chinese development placeholders. No original-game Yarn content is reconstructed.");
            source.AppendLine("// Core owns response effects, availability, branches and dispositions; Yarn presents one node at a time.");
            source.AppendLine("title: RuntimeDeclarations\n---");
            source.AppendLine("<<declare " + CueTextVariable + " = \"\">>");
            source.AppendLine("===\n");
            foreach (var node in content.Nodes)
            {
                source.AppendLine("title: " + NodeName(node.NodeId));
                source.AppendLine("---");
                source.AppendLine(EscapeLine(node.Speaker) + ": " + EscapeLine(node.Text));
                foreach (var response in node.Responses)
                {
                    var responseCode = EncodeIdentifier(response.Id);
                    source.AppendLine("-> " + EscapeLine(response.Text) + " #" + ResponseMetadataPrefix + responseCode);
                    source.AppendLine("    <<select_response \"" + EncodeIdentifier(node.NodeId) + "\" \"" + responseCode + "\">>");
                }
                source.AppendLine("===\n");
            }
            foreach (var kind in new[] { PresentationKind.MorningInspection, PresentationKind.Briefing,
                PresentationKind.Dream, PresentationKind.Ending })
            {
                source.AppendLine("title: " + CueNodeName(kind));
                source.AppendLine("---");
                source.AppendLine("终端: {" + CueTextVariable + "}");
                source.AppendLine("===\n");
            }
            return source.ToString();
        }

        private static string EscapeLine(string value) => value.Replace("\\", "\\\\")
            .Replace("[", "\\[").Replace("]", "\\]").Replace("{", "\\{").Replace("}", "\\}")
            .Replace("#", "\\#").Replace("\r", " ").Replace("\n", " ");
    }
}
