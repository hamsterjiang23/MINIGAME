#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniGame.Core;

namespace MiniGame.Presentation
{
    public enum InputContext { World, Dialogue, Presentation, Inspection, Paused, Ending }
    public enum PresentationKind { MorningInspection, Briefing, EnterTerminal, ResponseBranch, Reaction, ExitTerminal, Sleep, Dream, Ending }
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
        internal PatientView(string id, string name, PatientPresence presence, int treatmentCount, bool current, bool metToday)
        { Id = id; Name = name; Presence = presence; TreatmentCount = treatmentCount; IsCurrent = current; HasMetToday = metToday; }
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
        internal RuntimeView(PlayerView game, InputContext input, bool terminalOpen, bool paused,
            PresentationCue? cue, DialogueContent? dialogue, InspectionContent? inspection,
            IEnumerable<PatientView> patients, IEnumerable<InteractionTarget> interactions, IEnumerable<string> observedSigns)
        {
            Game = game; Input = input; TerminalOpen = terminalOpen; Paused = paused;
            Cue = cue; Dialogue = dialogue; Inspection = inspection;
            Patients = new ReadOnlyCollection<PatientView>(new List<PatientView>(patients));
            AllowedInteractions = new ReadOnlyCollection<InteractionTarget>(new List<InteractionTarget>(interactions));
            ObservedSigns = new ReadOnlyCollection<string>(new List<string>(observedSigns));
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
