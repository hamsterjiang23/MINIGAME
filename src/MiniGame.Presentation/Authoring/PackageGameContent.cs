#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MiniGame.Core;

namespace MiniGame.Presentation.Authoring
{
    public sealed class PackageGameContent : IGameContent
    {
        private readonly PrototypeContent _fallback;
        private readonly Dictionary<string, DialogueSegment> _segments;
        private readonly Dictionary<string, DialogueContent> _nodes;
        public string Revision { get; }
        public IReadOnlyList<DialogueContent> Nodes { get; }
        public IReadOnlyDictionary<string, DialogueSegment> Segments { get; }

        internal PackageGameContent(GameRules rules, string revision, Dictionary<string, DialogueSegment> segments)
        {
            Revision = revision;
            _fallback = new PrototypeContent(rules);
            _segments = new Dictionary<string, DialogueSegment>(segments, StringComparer.Ordinal);
            Segments = new ReadOnlyDictionary<string, DialogueSegment>(_segments);
            _nodes = _fallback.Nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal);
            foreach (var segment in _segments.Values)
            {
                var first = segment.Lines.FirstOrDefault();
                _nodes[segment.CoreNodeId] = new DialogueContent(segment.CoreNodeId, first?.Speaker ?? "终端",
                    string.Join("\n", segment.Lines.Select(l => l.Text)), segment.Responses);
            }
            Nodes = Array.AsReadOnly(_nodes.Values.OrderBy(n => n.NodeId, StringComparer.Ordinal).ToArray());
        }

        public DialogueSegment GetSegment(string coreNodeId) => _segments.TryGetValue(coreNodeId, out var segment)
            ? segment : throw new InvalidOperationException("The core node has no imported dialogue segment: " + coreNodeId);

        public bool TryGetSegment(string coreNodeId, out DialogueSegment? segment)
        {
            var found = _segments.TryGetValue(coreNodeId, out var value);
            segment = value;
            return found;
        }

        public DialogueContent GetDialogue(PlayerView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (view.CurrentNodeId == null || !_nodes.TryGetValue(view.CurrentNodeId, out var node))
                throw new InvalidOperationException("No content is mapped to the current core node.");
            return new DialogueContent(node.NodeId, node.Speaker, node.Text,
                node.Responses.Where(r => view.AvailableResponseIds.Contains(r.Id)));
        }

        public string GetPresentation(PresentationKind kind, PlayerView view)
        {
            if (kind == PresentationKind.ResponseBranch || kind == PresentationKind.Reaction) return GetDialogue(view).Text;
            return _fallback.GetPresentation(kind, view);
        }

        public InspectionContent GetInspection(string patientId, int day, InspectionKind kind)
            => _fallback.GetInspection(patientId, day, kind);
    }
}
