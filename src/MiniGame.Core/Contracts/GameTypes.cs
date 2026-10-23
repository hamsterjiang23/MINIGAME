#nullable enable

namespace MiniGame.Core
{
    public enum GamePhase
    {
        MorningInspection, Briefing, Apartment, Work, Evening,
        Sleeping, Dream, AfterDream, Ending, SelfReview
    }

    public enum EncounterStage
    {
        NormalDialogue, ResponseBranch, SpecialDialogue, AwaitingDisposition, Reaction, Completed, Introduction
    }

    public enum Disposition { Treat, Purify, Release }
    public enum PatientPresence { Active, Released, Purified, Deceased, TreatmentEnded }
    public enum EndingType { None, TakenAway, LockedInRoom, PriestAttack, DreamSea }
    public enum InteractionTarget { Door, Computer, Bed }
    public enum ScheduledEventKind { PriestAttack, TreatmentTermination }
    public enum CommandKind
    {
        CompleteMorningInspection, CompleteBriefing, Interact,
        CompleteSleep, CompleteDream, SelectResponse, CompleteResponseBranch,
        SubmitDisposition, CompleteReaction, CompleteIntroduction, SelectPatient, BeginNightDream, ConfirmSelfPurification,
        AdvanceSelfReview, EnterHiddenEndingCheat, EnterGmEnding
    }

    public enum RejectionReason
    {
        None, InvalidCommand, GameEnded, WrongPhase, InteractionUnavailable,
        WrongEncounter, WrongStage, WrongNode, UnknownResponse,
        ReleaseUnavailable, AlreadyDisposed, NumericOverflow, DispositionUnavailable, PatientUnavailable
    }

    public enum GameEventKind
    {
        PhaseChanged, EncounterStarted, DialogueNodeStarted, ResponseSelected,
        ResponseBranchStarted, DispositionAvailable, DispositionCommitted,
        ReactionRequested, EncounterCompleted, DayCompleted,
        PatientTerminated, ScheduledEventExecuted, GameEnded, IntroductionStarted, SelfReviewLineAdvanced
    }
}
