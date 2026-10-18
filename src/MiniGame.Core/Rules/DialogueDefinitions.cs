#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MiniGame.Core
{
    public sealed class ResponseDefinition
    {
        public string Id { get; }
        public string BranchNodeId { get; }
        public int TrustDelta { get; }
        public ResponseDefinition(string id, string branchNodeId, int trustDelta)
        {
            Id = RuleGuard.Identifier(id, nameof(id));
            BranchNodeId = RuleGuard.Identifier(branchNodeId, nameof(branchNodeId));
            TrustDelta = trustDelta;
        }
    }

    public sealed class RoundDefinition
    {
        public string NodeId { get; }
        public IReadOnlyList<ResponseDefinition> Responses { get; }
        public RoundDefinition(string nodeId, IEnumerable<ResponseDefinition> responses)
        {
            NodeId = RuleGuard.Identifier(nodeId, nameof(nodeId));
            var list = RuleGuard.Copy(responses, nameof(responses));
            if (list.Count < 1 || list.Count > 4 || list.Select(r => r.Id).Distinct().Count() != list.Count)
                throw new ArgumentException("A round must have 1-4 uniquely identified responses.", nameof(responses));
            Responses = list.AsReadOnly();
        }
    }

    public sealed class MeetingDefinition
    {
        public string PatientId { get; }
        public IReadOnlyList<RoundDefinition> NormalRounds { get; }
        public RoundDefinition? SpecialRound { get; }
        public int SpecialTrustThreshold { get; }
        public int ReleaseTrustThreshold { get; }
        public IReadOnlyDictionary<Disposition, string> ReactionNodes { get; }
        public string? IntroNodeId { get; }
        public IReadOnlyDictionary<Disposition, int> DispositionTrustDeltas { get; }

        public MeetingDefinition(string patientId, IEnumerable<RoundDefinition> normalRounds,
            RoundDefinition? specialRound, int specialTrustThreshold, int releaseTrustThreshold,
            IDictionary<Disposition, string> reactionNodes, string? introNodeId = null,
            IDictionary<Disposition, int>? dispositionTrustDeltas = null)
        {
            PatientId = RuleGuard.Identifier(patientId, nameof(patientId));
            var rounds = RuleGuard.Copy(normalRounds, nameof(normalRounds));
            IntroNodeId = introNodeId == null ? null : RuleGuard.Identifier(introNodeId, nameof(introNodeId));
            if (rounds.Count == 0 && IntroNodeId == null)
                throw new ArgumentException("A meeting requires normal dialogue or an introduction.", nameof(normalRounds));
            var nodeIds = rounds.Select(r => r.NodeId).ToList();
            if (IntroNodeId != null) nodeIds.Add(IntroNodeId);
            if (specialRound != null) nodeIds.Add(specialRound.NodeId);
            if (nodeIds.Distinct().Count() != nodeIds.Count)
                throw new ArgumentException("Dialogue node identifiers must be unique within a meeting.");
            NormalRounds = rounds.AsReadOnly(); SpecialRound = specialRound;
            SpecialTrustThreshold = specialTrustThreshold; ReleaseTrustThreshold = releaseTrustThreshold;
            if (reactionNodes == null) throw new ArgumentNullException(nameof(reactionNodes));
            if (reactionNodes.Count == 0 || reactionNodes.Keys.Any(d => !Enum.IsDefined(typeof(Disposition), d)))
                throw new ArgumentException("At least one supported disposition reaction is required.", nameof(reactionNodes));
            ReactionNodes = new ReadOnlyDictionary<Disposition, string>(reactionNodes.ToDictionary(
                pair => pair.Key, pair => RuleGuard.Identifier(pair.Value, nameof(reactionNodes))));
            var deltas = dispositionTrustDeltas ?? new Dictionary<Disposition, int>();
            if (deltas.Keys.Any(d => !ReactionNodes.ContainsKey(d)))
                throw new ArgumentException("Trust changes require an allowed disposition.", nameof(dispositionTrustDeltas));
            DispositionTrustDeltas = new ReadOnlyDictionary<Disposition, int>(deltas.ToDictionary(p => p.Key, p => p.Value));
        }
    }

    internal static class RuleGuard
    {
        internal static string Identifier(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("An identifier is required.", parameter);
            return value;
        }

        internal static List<T> Copy<T>(IEnumerable<T> source, string parameter) where T : class
        {
            if (source == null) throw new ArgumentNullException(parameter);
            var list = source.ToList();
            if (list.Any(x => x == null)) throw new ArgumentException("Null entries are not permitted.", parameter);
            return list;
        }
    }
}
