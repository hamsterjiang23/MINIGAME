#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniGame.Core;
using Newtonsoft.Json;

namespace MiniGame.Presentation.Authoring
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DialoguePackage
    {
        [JsonProperty("formatVersion", Required = Required.Always)] public int FormatVersion { get; set; }
        [JsonProperty("packageId", Required = Required.Always)] public string PackageId { get; set; } = "";
        [JsonProperty("contentVersion", Required = Required.Always)] public string ContentVersion { get; set; } = "";
        [JsonProperty("yarnSource", Required = Required.Always)] public string YarnSource { get; set; } = "";
        [JsonProperty("portraits", Required = Required.Always)] public List<PortraitDefinition> Portraits { get; set; } = new List<PortraitDefinition>();
        [JsonProperty("profile", Required = Required.Always)] public DialogueProfile Profile { get; set; } = new DialogueProfile();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class PortraitDefinition
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; } = "";
        [JsonProperty("label", Required = Required.Always)] public string Label { get; set; } = "";
    }

    /// <summary>Explicit development parameters for the independent one-day terminal demo.</summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DialogueProfile
    {
        [JsonProperty("patientId", Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty("patientName", Required = Required.Always)] public string PatientName { get; set; } = "";
        [JsonProperty("day", Required = Required.Always)] public int Day { get; set; }
        [JsonProperty("startNode", Required = Required.Always)] public string StartNode { get; set; } = "";
        [JsonProperty("endNode", Required = Required.Always)] public string EndNode { get; set; } = "";
        [JsonProperty("initialTrust", Required = Required.Always)] public int InitialTrust { get; set; }
        [JsonProperty("releaseThreshold", Required = Required.Always)] public int ReleaseThreshold { get; set; }
        [JsonProperty("initialQuota", Required = Required.Always)] public int InitialQuota { get; set; }
    }

    public sealed class DialogueDiagnostic
    {
        public string Code { get; }
        public string Message { get; }
        public int Line { get; }
        public string NodeId { get; }
        public bool IsWarning { get; }
        public DialogueDiagnostic(string code, string message, int line = 0, string nodeId = "", bool isWarning = false)
        { Code = code; Message = message; Line = line; NodeId = nodeId; IsWarning = isWarning; }
        public override string ToString() => $"{Code}: {NodeId} line {Line}: {Message}";
    }

    public sealed class DialoguePackageException : Exception
    {
        public IReadOnlyList<DialogueDiagnostic> Diagnostics { get; }
        public DialoguePackageException(DialogueDiagnostic diagnostic) : base(diagnostic.ToString())
        { Diagnostics = Array.AsReadOnly(new[] { diagnostic }); }
    }

    public sealed class AuthoringOption
    {
        public string Id { get; }
        public string Text { get; }
        public string Next { get; }
        public int TrustDelta { get; }
        public Disposition? Disposition { get; }
        public int SourceLine { get; }
        internal AuthoringOption(string id, string text, string next, int trustDelta, Disposition? disposition, int sourceLine)
        { Id = id; Text = text; Next = next; TrustDelta = trustDelta; Disposition = disposition; SourceLine = sourceLine; }
    }

    public sealed class AuthoringNode
    {
        public string Id { get; }
        public string Kind { get; }
        public string Round { get; }
        public string PortraitId { get; }
        public string Speaker { get; }
        public string Text { get; }
        public string Next { get; }
        public int SourceLine { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public IReadOnlyList<AuthoringOption> Options { get; }
        internal AuthoringNode(string id, string kind, string round, string portraitId, string speaker,
            string text, string next, int sourceLine, List<AuthoringOption> options, Dictionary<string, string> headers)
        {
            Id = id; Kind = kind; Round = round; PortraitId = portraitId; Speaker = speaker;
            Text = text; Next = next; SourceLine = sourceLine; Options = options.AsReadOnly();
            Headers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(headers, StringComparer.Ordinal));
        }
    }

    public sealed class DialogueLine
    {
        public string SourceNodeId { get; }
        public string Speaker { get; }
        public string Text { get; }
        public string PortraitId { get; }
        internal DialogueLine(AuthoringNode node)
        { SourceNodeId = node.Id; Speaker = node.Speaker; Text = node.Text; PortraitId = node.PortraitId; }
    }

    public sealed class DialogueSegment
    {
        public string CoreNodeId { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public IReadOnlyList<ResponseContent> Responses { get; }
        internal DialogueSegment(string coreNodeId, List<DialogueLine> lines, List<ResponseContent> responses)
        { CoreNodeId = coreNodeId; Lines = lines.AsReadOnly(); Responses = responses.AsReadOnly(); }
    }

    public sealed class CompiledDialoguePackage
    {
        public DialoguePackage Package { get; }
        public GameRules Rules { get; }
        public PackageGameContent Content { get; }
        public string RuntimeYarn { get; }
        public string SourceHash { get; }
        public IReadOnlyList<AuthoringNode> SourceNodes { get; }
        public IReadOnlyList<DialogueDiagnostic> Diagnostics { get; }
        internal CompiledDialoguePackage(DialoguePackage package, GameRules rules, PackageGameContent content,
            string runtimeYarn, string sourceHash, List<AuthoringNode> nodes, List<DialogueDiagnostic> diagnostics)
        {
            Package = package; Rules = rules; Content = content; RuntimeYarn = runtimeYarn; SourceHash = sourceHash;
            SourceNodes = nodes.AsReadOnly(); Diagnostics = diagnostics.AsReadOnly();
        }
    }
}
