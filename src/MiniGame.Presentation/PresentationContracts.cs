#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniGame.Core;

namespace MiniGame.Presentation
{
    public enum InputContext { World, Dialogue, Presentation, Inspection, Paused, Ending, TerminalDirectory, DoorView }
    public enum TerminalPage { Closed, Directory, Review, Archive, SelfReview }
    public enum PresentationKind { MorningInspection, Briefing, EnterTerminal, ResponseBranch, Reaction, ExitTerminal, Sleep, Dream, Ending, Introduction, DoorMotion, DoorViewReturn }
    public enum InspectionKind { Eyes, Hands, Teeth, Photo, DailySigns, Records }

    public interface IGameContent
    {
        string Revision { get; }
        DialogueContent GetDialogue(PlayerView view);
        string GetPresentation(PresentationKind kind, PlayerView view);
        InspectionContent GetInspection(string patientId, int day, InspectionKind kind);
    }

    public sealed class ResponseContent
    {
        public string Id { get; }
        public string Text { get; }
        public ResponseContent(string id, string text) { Id = id; Text = text; }
    }

    public sealed class DialogueContent
    {
        public string NodeId { get; }
        public string Speaker { get; }
        public string Text { get; }
        public IReadOnlyList<ResponseContent> Responses { get; }
        public DialogueContent(string nodeId, string speaker, string text, IEnumerable<ResponseContent> responses)
        {
            NodeId = nodeId; Speaker = speaker; Text = text;
            Responses = new ReadOnlyCollection<ResponseContent>(new List<ResponseContent>(responses));
        }
    }

    public sealed class InspectionContent
    {
        public InspectionKind Kind { get; }
        public string Title { get; }
        public string Summary { get; }
        public InspectionContent(InspectionKind kind, string title, string summary)
        { Kind = kind; Title = title; Summary = summary; }
    }

    public sealed class PresentationCue
    {
        public string Token { get; }
        public PresentationKind Kind { get; }
        public string Text { get; }
        internal PresentationCue(string token, PresentationKind kind, string text)
        { Token = token; Kind = kind; Text = text; }
    }

    public sealed class PatientView
    {
        public string Id { get; }
        public string Name { get; }
        public PatientPresence Presence { get; }
        public int TreatmentCount { get; }
        public bool IsCurrent { get; }
        public bool HasMetToday { get; }
        public bool CanReceiveToday { get; }
        public Disposition? LastDisposition { get; }
        internal PatientView(string id, string name, PatientPresence presence, int treatmentCount, bool current, bool metToday,
            Disposition? lastDisposition = null, bool canReceiveToday = false)
        {
            Id = id; Name = name; Presence = presence; TreatmentCount = treatmentCount; IsCurrent = current;
            HasMetToday = metToday; LastDisposition = lastDisposition; CanReceiveToday = canReceiveToday;
        }
    }

    public sealed class RuntimeView
    {
        public PlayerView Game { get; }
        public InputContext Input { get; }
        public bool TerminalOpen { get; }
        public bool Paused { get; }
        public PresentationCue? Cue { get; }
        public DialogueContent? Dialogue { get; }
        public InspectionContent? Inspection { get; }
        public IReadOnlyList<PatientView> Patients { get; }
        public IReadOnlyList<InteractionTarget> AllowedInteractions { get; }
        public IReadOnlyList<string> ObservedSigns { get; }
        public TerminalPage TerminalPage { get; }
        public bool UsesTerminalDirectory { get; }
        public bool LeavingTerminal { get; }
        public bool ArchiveDirectoryVisible { get; }
        public bool RoomDoorOpen { get; }
        public bool DoorViewActive { get; }
        public bool UsesDreamFlow { get; }
        public bool DreamTraversal { get; }
        public IReadOnlyList<TranscriptEntry> Transcript { get; }
        public string? PresentedPatientId { get; }
        public string? PresentedPatientName { get; }
        public string? PresentedEncounterId { get; }
        public Disposition? PresentedDisposition { get; }
        public IReadOnlyList<Disposition> ConfiguredDispositions { get; }
        internal RuntimeView(PlayerView game, InputContext input, bool terminalOpen, bool paused,
            PresentationCue? cue, DialogueContent? dialogue, InspectionContent? inspection,
            IEnumerable<PatientView> patients, IEnumerable<InteractionTarget> interactions, IEnumerable<string> observedSigns,
            TerminalPage terminalPage = TerminalPage.Closed, bool usesTerminalDirectory = false,
            bool leavingTerminal = false, IEnumerable<TranscriptEntry>? transcript = null, bool archiveDirectoryVisible = false,
            string? presentedPatientId = null, string? presentedPatientName = null, string? presentedEncounterId = null,
            Disposition? presentedDisposition = null, IEnumerable<Disposition>? configuredDispositions = null,
            bool roomDoorOpen = false, bool doorViewActive = false, bool usesDreamFlow = false)
        {
            Game = game; Input = input; TerminalOpen = terminalOpen; Paused = paused;
            Cue = cue; Dialogue = dialogue; Inspection = inspection;
            Patients = new ReadOnlyCollection<PatientView>(new List<PatientView>(patients));
            AllowedInteractions = new ReadOnlyCollection<InteractionTarget>(new List<InteractionTarget>(interactions));
            ObservedSigns = new ReadOnlyCollection<string>(new List<string>(observedSigns));
            TerminalPage = terminalPage; UsesTerminalDirectory = usesTerminalDirectory; LeavingTerminal = leavingTerminal;
            ArchiveDirectoryVisible = archiveDirectoryVisible;
            RoomDoorOpen = roomDoorOpen;
            DoorViewActive = doorViewActive;
            UsesDreamFlow = usesDreamFlow;
            DreamTraversal = usesDreamFlow && !terminalOpen && (game.Phase == GamePhase.AfterDream || game.Phase == GamePhase.SelfReview);
            Transcript = new ReadOnlyCollection<TranscriptEntry>(new List<TranscriptEntry>(transcript ?? Array.Empty<TranscriptEntry>()));
            PresentedPatientId = presentedPatientId; PresentedPatientName = presentedPatientName;
            PresentedEncounterId = presentedEncounterId; PresentedDisposition = presentedDisposition;
            ConfiguredDispositions = new ReadOnlyCollection<Disposition>(new List<Disposition>(configuredDispositions ??
                new[] { Disposition.Release, Disposition.Treat, Disposition.Purify }));
        }
    }

    public sealed class TranscriptEntry
    {
        public string Key { get; }
        public string EncounterId { get; }
        public string CoreNodeId { get; }
        public string Speaker { get; }
        public string Text { get; }
        public string SourceNodeId { get; }
        public string PortraitId { get; }
        public bool IsPlayer { get; }
        public bool IsNarration { get; }
        public bool Completed { get; internal set; }
        internal TranscriptEntry(string key, string encounterId, string coreNodeId, string speaker,
            string text, string sourceNodeId, string portraitId, bool isPlayer, bool isNarration = false, bool completed = false)
        {
            Key = key; EncounterId = encounterId; CoreNodeId = coreNodeId; Speaker = speaker;
            Text = text; SourceNodeId = sourceNodeId; PortraitId = portraitId; IsPlayer = isPlayer;
            IsNarration = isNarration; Completed = completed;
        }
    }

    public sealed class RuntimeResult
    {
        public bool Accepted { get; }
        public string Reason { get; }
        public IReadOnlyList<GameEvent> Events { get; }
        internal RuntimeResult(string reason, IEnumerable<GameEvent>? events = null)
        {
            Reason = reason; Accepted = reason.Length == 0;
            Events = new ReadOnlyCollection<GameEvent>(new List<GameEvent>(events ?? Array.Empty<GameEvent>()));
        }
    }
}
