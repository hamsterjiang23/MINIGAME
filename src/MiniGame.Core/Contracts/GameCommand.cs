#nullable enable

namespace MiniGame.Core
{
    public abstract class GameCommand { }
    public sealed class CompleteMorningInspection : GameCommand { }
    public sealed class CompleteBriefing : GameCommand { }
    public sealed class CompleteSleep : GameCommand { }
    public sealed class CompleteDream : GameCommand { }
    public sealed class BeginNightDream : GameCommand { }
    public sealed class ConfirmSelfPurification : GameCommand { }

    public sealed class AdvanceSelfReviewCommand : GameCommand
    {
        public int ExpectedLineIndex { get; }
        public AdvanceSelfReviewCommand(int expectedLineIndex) { ExpectedLineIndex = expectedLineIndex; }
    }

    public sealed class Interact : GameCommand
    {
        public InteractionTarget Target { get; }
        public Interact(InteractionTarget target) { Target = target; }
    }

    public sealed class SelectPatient : GameCommand
    {
        public string PatientId { get; }
        public SelectPatient(string patientId) { PatientId = patientId; }
    }

    public sealed class SelectResponse : GameCommand
    {
        public string EncounterId { get; }
        public string NodeId { get; }
        public string ResponseId { get; }
        public SelectResponse(string encounterId, string nodeId, string responseId)
        { EncounterId = encounterId; NodeId = nodeId; ResponseId = responseId; }
    }

    public sealed class CompleteResponseBranch : GameCommand
    {
        public string EncounterId { get; }
        public string BranchNodeId { get; }
        public CompleteResponseBranch(string encounterId, string branchNodeId)
        { EncounterId = encounterId; BranchNodeId = branchNodeId; }
    }

    public sealed class SubmitDisposition : GameCommand
    {
        public string EncounterId { get; }
        public Disposition Disposition { get; }
        public SubmitDisposition(string encounterId, Disposition disposition)
        { EncounterId = encounterId; Disposition = disposition; }
    }

    public sealed class CompleteReaction : GameCommand
    {
        public string EncounterId { get; }
        public CompleteReaction(string encounterId) { EncounterId = encounterId; }
    }

    public sealed class CompleteIntroduction : GameCommand
    {
        public string EncounterId { get; }
        public string NodeId { get; }
        public CompleteIntroduction(string encounterId, string nodeId)
        { EncounterId = encounterId; NodeId = nodeId; }
    }
}
