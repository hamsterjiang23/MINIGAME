#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MiniGame.Core
{
    public sealed class GameEvent
    {
        public GameEventKind Kind { get; }
        public int Day { get; }
        public string? PatientId { get; }
        public string? EncounterId { get; }
        public string? ReferenceId { get; }
        public GameEvent(GameEventKind kind, int day, string? patientId = null,
            string? encounterId = null, string? referenceId = null)
        { Kind = kind; Day = day; PatientId = patientId; EncounterId = encounterId; ReferenceId = referenceId; }
    }

    public sealed class CommandResult
    {
        public bool Accepted { get; }
        public RejectionReason Reason { get; }
        public IReadOnlyList<GameEvent> Events { get; }
        internal CommandResult(RejectionReason reason, IEnumerable<GameEvent> events)
        {
            Accepted = reason == RejectionReason.None;
            Reason = reason;
            Events = new ReadOnlyCollection<GameEvent>(new List<GameEvent>(events));
        }
    }

    /// <summary>A read-only instrument reading relative to this meeting's release threshold.</summary>
    public sealed class SocialAdaptationReading
    {
        // No raw score or patient history is exposed to the terminal.
        public double RelativeReleaseMargin { get; }
        public bool MeetsReleaseThreshold => RelativeReleaseMargin >= 0;

        internal SocialAdaptationReading(int trust, int releaseThreshold)
        {
            RelativeReleaseMargin = ((double)trust - releaseThreshold)
                / Math.Max(1d, Math.Abs((double)releaseThreshold));
        }
    }

    // Raw trust and assessment history remain private; only the current instrument reading is public.
    public sealed class PlayerView
    {
        public int Day { get; }
        public GamePhase Phase { get; }
        /// <summary>The phase CompleteDream will enter, or null outside a dream. Reading it has no side effects.</summary>
        public GamePhase? DreamReturnPhase { get; }
        public EndingType Ending { get; }
        public int RemainingPurificationQuota { get; }
        public int PurificationsToday { get; }
        public int PurificationRequirement { get; }
        public int Rebellion { get; }
        public int SelfReviewLineIndex { get; }
        public int SelfReviewLineCount { get; }
        public bool SelfReviewCompleted => Phase == GamePhase.SelfReview && SelfReviewLineCount > 0
            && SelfReviewLineIndex == SelfReviewLineCount;
        public string? CurrentPatientId { get; }
        public string? CurrentPatientName { get; }
        public string? CurrentEncounterId { get; }
        public string? CurrentNodeId { get; }
        public EncounterStage? EncounterStage { get; }
        public IReadOnlyList<string> AvailableResponseIds { get; }
        public IReadOnlyList<Disposition> AvailableDispositions { get; }
        public IReadOnlyList<InteractionTarget> AllowedInteractions { get; }
        public SocialAdaptationReading? SocialAdaptation { get; }

        internal PlayerView(int day, GamePhase phase, EndingType ending, int remainingQuota,
            string? patientId, string? patientName, string? encounterId, string? nodeId,
            EncounterStage? stage, IEnumerable<string> responses,
            IEnumerable<Disposition> dispositions, IEnumerable<InteractionTarget> interactions, int rebellion = 0,
            int purificationsToday = 0, int purificationRequirement = 0,
            int selfReviewLineIndex = 0, int selfReviewLineCount = 0, GamePhase? dreamReturnPhase = null,
            SocialAdaptationReading? socialAdaptation = null)
        {
            Day = day; Phase = phase; Ending = ending;
            DreamReturnPhase = dreamReturnPhase;
            RemainingPurificationQuota = remainingQuota;
            PurificationsToday = purificationsToday;
            PurificationRequirement = purificationRequirement;
            Rebellion = rebellion;
            SelfReviewLineIndex = selfReviewLineIndex;
            SelfReviewLineCount = selfReviewLineCount;
            CurrentPatientId = patientId; CurrentPatientName = patientName;
            CurrentEncounterId = encounterId; CurrentNodeId = nodeId; EncounterStage = stage;
            AvailableResponseIds = new ReadOnlyCollection<string>(new List<string>(responses));
            AvailableDispositions = new ReadOnlyCollection<Disposition>(new List<Disposition>(dispositions));
            AllowedInteractions = new ReadOnlyCollection<InteractionTarget>(new List<InteractionTarget>(interactions));
            SocialAdaptation = socialAdaptation;
        }
    }
}
