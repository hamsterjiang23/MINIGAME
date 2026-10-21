#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;

namespace MiniGame.Presentation
{
    // Adapts the reference's room/dialogue/callback lifecycle. Rules stay in GameSession.
    public sealed class GameRuntime
    {
        private readonly GameSession _session;
        private readonly GameRules _rules;
        private readonly IGameContent _content;
        private readonly List<ObservationRecord> _observations = new List<ObservationRecord>();
        private bool _terminalOpen;
        private int _terminalPowerDay;
        private TerminalPowerState _terminalPower;
        private bool _paused;
        private PresentationCue? _cue;
        private InspectionContent? _inspection;
        private RuntimeView? _cachedView;
        private readonly bool _useTerminalDirectory;
        private TerminalPage _terminalPage;
        private bool _leavingDirectory;
        private bool _archiveDirectoryVisible;
        private bool _roomDoorOpen;
        private bool _doorViewActive;
        private bool _dreamFromBed;
        private string? _historyEncounterId;
        private readonly List<TranscriptEntry> _transcript = new List<TranscriptEntry>();

        public event Action<RuntimeView>? Changed;
        public bool IsCampaign => _rules.StartDay == 0;
        public bool UsesDreamFlow => _rules.DreamFlowEnabled;
        public bool LockDoorAfterSupervisor { get; private set; }
        public bool IsDoorLockedAfterSupervisor
        {
            get
            {
                var game = _session.GetPlayerView();
                return LockDoorAfterSupervisor && game.Day > 0
                    && (game.Phase == GamePhase.Apartment || game.Phase == GamePhase.Work);
            }
        }

        /// <summary>Applies the host's global door policy without changing campaign or saved progress.</summary>
        public void ConfigureDoorLockAfterSupervisor(bool enabled)
        {
            if (LockDoorAfterSupervisor == enabled) return;
            LockDoorAfterSupervisor = enabled;
            Accept();
        }

        public GameRuntime(GameRules rules, IGameContent content, bool useTerminalDirectory = false)
            : this(new GameSession(rules), rules, content, useTerminalDirectory) { }

        private GameRuntime(GameSession session, GameRules rules, IGameContent content, bool useTerminalDirectory = false)
        {
            _session = session;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _useTerminalDirectory = useTerminalDirectory;
            _terminalPowerDay = session.GetPlayerView().Day;
            if (string.IsNullOrWhiteSpace(content.Revision))
                throw new ArgumentException("Content requires a stable revision.", nameof(content));
        }

        public RuntimeView GetView()
        {
            if (_cachedView != null) return _cachedView;
            var game = _session.GetPlayerView();
            var snapshot = _session.CreateSnapshot().State;
            var input = _paused ? InputContext.Paused : game.Phase == GamePhase.Ending ? InputContext.Ending
                : _inspection != null ? InputContext.Inspection : _cue != null ? InputContext.Presentation
                : _doorViewActive ? InputContext.DoorView
                : _terminalOpen && (_terminalPage == TerminalPage.Directory || _terminalPage == TerminalPage.Archive || _terminalPage == TerminalPage.SelfReview)
                    ? InputContext.TerminalDirectory : _terminalOpen ? InputContext.Dialogue : InputContext.World;
            var day = _rules.Days.FirstOrDefault(item => item.Day == game.Day);
            var patients = _rules.Patients.Where(profile => profile.FirstDay <= game.Day
                && (!IsCampaign || day == null || day.Meetings.Any(meeting => meeting.PatientId == profile.Id))).Select(profile =>
            {
                var state = snapshot.Patients.Single(patient => patient.Id == profile.Id);
                return new PatientView(state.Id, profile.DisplayName, state.Presence, state.TreatmentCount,
                    game.CurrentPatientId == state.Id,
                    snapshot.Encounters.Any(encounter => encounter.Day == game.Day && encounter.PatientId == state.Id
                        && encounter.Stage == EncounterStage.Completed),
                    snapshot.Encounters.LastOrDefault(encounter => encounter.PatientId == state.Id && encounter.Disposition.HasValue)?.Disposition,
                    _session.CanReceiveToday(state.Id));
            });
            var dialogue = _terminalOpen && _terminalPage == TerminalPage.Review && _cue == null && _inspection == null
                && (game.EncounterStage == EncounterStage.NormalDialogue || game.EncounterStage == EncounterStage.SpecialDialogue)
                ? _content.GetDialogue(game) : null;
            if (dialogue != null)
                dialogue = new DialogueContent(dialogue.NodeId, dialogue.Speaker, ResolveSegmentText(dialogue.NodeId, dialogue.Text),
                    dialogue.Responses.Select(response => new ResponseContent(response.Id, ResolveDialogueText(response.Id, response.Text))));
            var presented = snapshot.Encounters.SingleOrDefault(encounter => encounter.Id == _historyEncounterId);
            var presentedProfile = _rules.Patients.SingleOrDefault(patient => patient.Id == presented?.PatientId);
            var presentedMeeting = presented == null ? null : _rules.Days.Single(item => item.Day == presented.Day)
                .Meetings.Single(meeting => meeting.PatientId == presented.PatientId);
            _cachedView = new RuntimeView(game, input, _terminalOpen, _paused, _cue, dialogue, _inspection,
                patients, input == InputContext.World ? WorldInteractions(game) : Array.Empty<InteractionTarget>(),
                _observations.Select(observation => $"{observation.Day}:{observation.PatientId}:{observation.Kind}"),
                _terminalPage, _useTerminalDirectory, _leavingDirectory,
                _transcript.Where(entry => entry.EncounterId == _historyEncounterId), _archiveDirectoryVisible,
                presented?.PatientId, presentedProfile?.DisplayName, presented?.Id, presented?.Disposition, presentedMeeting?.ReactionNodes.Keys,
                _roomDoorOpen, _doorViewActive, UsesDreamFlow, _terminalPower);
            return _cachedView;
        }

        public RuntimeResult TryInteract(InteractionTarget target)
        {
            var view = GetView();
            if (!view.AllowedInteractions.Contains(target)) return Reject("InteractionUnavailable");
            if (target == InteractionTarget.Door)
            {
                if (view.Game.Phase != GamePhase.MorningInspection) return Reject("InteractionUnavailable");
                SetCue(PresentationKind.MorningInspection);
                return Accept();
            }
            if (target == InteractionTarget.Computer)
            {
                if (view.Game.Phase == GamePhase.SelfReview && UsesDreamFlow)
                {
                    // The post-dream self review is a live safety display, not another daily work boot.
                    _terminalPower = TerminalPowerState.Running;
                    _terminalOpen = true;
                    _terminalPage = TerminalPage.SelfReview;
                    _archiveDirectoryVisible = false;
                    _historyEncounterId = null;
                    SetCue(PresentationKind.EnterTerminal);
                    return Accept();
                }
                CommandResult? result = null;
                if (view.Game.Phase == GamePhase.Apartment)
                {
                    result = _session.Execute(new Interact(target));
                    if (!result.Accepted) return Reject(result.Reason.ToString());
                }
                if (_session.GetPlayerView().Phase == GamePhase.Work)
                {
                    if (_terminalPower == TerminalPowerState.Off) _terminalPower = TerminalPowerState.Booting;
                    _terminalOpen = true;
                    _terminalPage = _useTerminalDirectory ? TerminalPage.Directory : TerminalPage.Review;
                    _archiveDirectoryVisible = false;
                    _historyEncounterId = _session.GetPlayerView().CurrentEncounterId;
                    SetCue(PresentationKind.EnterTerminal);
                }
                return Accept(result?.Events);
            }
            if (UsesDreamFlow && target == InteractionTarget.Bed && view.Game.Phase == GamePhase.Evening)
            {
                _dreamFromBed = true;
                SetCue(PresentationKind.Sleep);
                return Accept();
            }
            if (UsesDreamFlow && target == InteractionTarget.Bed && view.Game.Day == 0 && view.Game.Phase == GamePhase.AfterDream)
            {
                SetCue(PresentationKind.Sleep);
                return Accept();
            }
            var bedResult = _session.Execute(new Interact(target));
            if (!bedResult.Accepted) return Reject(bedResult.Reason.ToString());
            _terminalOpen = false;
            _terminalPage = TerminalPage.Closed;
            var phase = _session.GetPlayerView().Phase;
            if (phase == GamePhase.Sleeping) SetCue(PresentationKind.Sleep);
            else if (phase == GamePhase.Ending) SetCue(PresentationKind.Ending);
            return Accept(bedResult.Events);
        }

        /// <summary>Fresh campaign opening using the normal authoritative interaction and patient selection.</summary>
        public RuntimeResult BeginDayZeroOpening()
        {
            var view = GetView();
            if (!IsCampaign || !_useTerminalDirectory || view.Game.Day != 0 || view.Game.Phase != GamePhase.Apartment
                || view.Input != InputContext.World) return Reject("OpeningUnavailable");
            var entered = _session.Execute(new Interact(InteractionTarget.Computer));
            if (!entered.Accepted) return Reject(entered.Reason.ToString());
            var game = _session.GetPlayerView();
            var selected = _session.Execute(new SelectPatient(game.CurrentPatientId!));
            if (!selected.Accepted) throw new InvalidOperationException("The Day0 tutorial could not be selected.");
            _terminalOpen = true;
            _terminalPower = TerminalPowerState.Booting;
            _terminalPage = TerminalPage.Review;
            _archiveDirectoryVisible = false;
            _historyEncounterId = game.CurrentEncounterId;
            SetCue(PresentationKind.EnterTerminal);
            return Accept(entered.Events.Concat(selected.Events));
        }

        public RuntimeResult BeginNightDream()
        {
            var view = GetView();
            if (!UsesDreamFlow || view.Input != InputContext.World || view.TerminalOpen || view.Game.Phase != GamePhase.Evening)
                return Reject("NightDreamUnavailable");
            var result = _session.Execute(new BeginNightDream());
            if (!result.Accepted) return Reject(result.Reason.ToString());
            if (_session.GetPlayerView().Phase == GamePhase.Dream) SetCue(PresentationKind.Dream);
            return Accept(result.Events);
        }

        public RuntimeResult ConfirmSelfPurification()
        {
            var view = GetView();
            if (!UsesDreamFlow || view.Input != InputContext.TerminalDirectory || !view.TerminalOpen
                || view.TerminalPage != TerminalPage.SelfReview || view.Game.Phase != GamePhase.SelfReview)
                return Reject("SelfPurificationUnavailable");
            var result = _session.Execute(new ConfirmSelfPurification());
            if (!result.Accepted) return Reject(result.Reason.ToString());
            _terminalOpen = false;
            _terminalPower = TerminalPowerState.Off;
            _terminalPage = TerminalPage.Closed;
            _leavingDirectory = false;
            _archiveDirectoryVisible = false;
            SetCue(PresentationKind.Ending);
            return Accept(result.Events);
        }

        public RuntimeResult ToggleRoomDoor()
        {
            var view = GetView();
            if (view.Input != InputContext.World) return Reject("InputLocked");
            if (view.Game.Phase != GamePhase.MorningInspection && view.Game.Phase != GamePhase.Apartment
                && view.Game.Phase != GamePhase.Work) return Reject("InteractionUnavailable");
            if (!_roomDoorOpen && IsDoorLockedAfterSupervisor) return Reject("DoorLockedAfterSupervisor");
            _roomDoorOpen = !_roomDoorOpen;
            SetCue(PresentationKind.DoorMotion);
            return Accept();
        }

        public RuntimeResult EnterDoorView()
        {
            var view = GetView();
            if (view.Input != InputContext.World) return Reject("InputLocked");
            if (view.Game.Phase != GamePhase.MorningInspection && view.Game.Phase != GamePhase.Apartment
                && view.Game.Phase != GamePhase.Work) return Reject("InteractionUnavailable");
            if (!_roomDoorOpen && IsDoorLockedAfterSupervisor) return Reject("DoorLockedAfterSupervisor");
            _doorViewActive = true;
            _roomDoorOpen = true;
            SetCue(PresentationKind.DoorMotion);
            return Accept();
        }

        public RuntimeResult ExitDoorView()
        {
            if (GetView().Input != InputContext.DoorView) return Reject("InputLocked");
            // The host returns the camera before acknowledging this cue; only then may the door close.
            SetCue(PresentationKind.DoorViewReturn);
            return Accept();
        }

        public RuntimeResult SelectResponse(string encounterId, string nodeId, string responseId)
        {
            if (GetView().Input != InputContext.Dialogue) return Reject("InputLocked");
            var selected = GetView().Dialogue?.Responses.SingleOrDefault(response => response.Id == responseId);
            var result = _session.Execute(new SelectResponse(encounterId, nodeId, responseId));
            if (!result.Accepted) return Reject(result.Reason.ToString());
            AddTranscript(encounterId, nodeId, "值班员", selected?.Text ?? responseId, responseId, "", true);
            SetCue(PresentationKind.ResponseBranch);
            return Accept(result.Events);
        }

        public RuntimeResult SubmitDisposition(string encounterId, Disposition disposition)
        {
            if (GetView().Input != InputContext.Dialogue) return Reject("InputLocked");
            var result = _session.Execute(new SubmitDisposition(encounterId, disposition));
            if (!result.Accepted) return Reject(result.Reason.ToString());
            AddTranscript(encounterId, GetView().Game.CurrentNodeId ?? "disposition", "值班员",
                "【处置】" + (disposition == Disposition.Treat ? "治疗" : disposition == Disposition.Purify ? "净化" : "释放"),
                "disposition:" + disposition, "", true);
            SetCue(PresentationKind.Reaction);
            return Accept(result.Events);
        }

        public RuntimeResult CompleteCue(string token)
        {
            if (_paused || _inspection != null) return Reject("InputLocked");
            if (_cue == null || _cue.Token != token) return Reject("StaleCue");
            var kind = _cue.Kind;
            var game = _session.GetPlayerView();
            GameCommand? command = kind switch
            {
                PresentationKind.MorningInspection => new CompleteMorningInspection(),
                PresentationKind.Briefing => new CompleteBriefing(),
                PresentationKind.ResponseBranch => new CompleteResponseBranch(game.CurrentEncounterId!, game.CurrentNodeId!),
                PresentationKind.Reaction => new CompleteReaction(game.CurrentEncounterId!),
                PresentationKind.Introduction => new CompleteIntroduction(game.CurrentEncounterId!, game.CurrentNodeId!),
                PresentationKind.Sleep => _dreamFromBed && game.Phase == GamePhase.Evening
                    ? (GameCommand)new BeginNightDream() : game.Day == 0 && game.Phase == GamePhase.AfterDream
                    ? (GameCommand)new Interact(InteractionTarget.Bed) : new CompleteSleep(),
                PresentationKind.Dream => new CompleteDream(),
                _ => null
            };
            CommandResult? result = command == null ? null : _session.Execute(command);
            if (result != null && !result.Accepted) return Reject(result.Reason.ToString());
            IEnumerable<GameEvent>? events = result?.Events;
            _cue = null;
            if (_dreamFromBed && (kind == PresentationKind.Sleep || kind == PresentationKind.Dream)
                && _session.GetPlayerView().Phase == GamePhase.AfterDream)
            {
                var wake = _session.Execute(new Interact(InteractionTarget.Bed));
                if (!wake.Accepted) throw new InvalidOperationException("Completed bed dream could not advance the night.");
                events = (events ?? Array.Empty<GameEvent>()).Concat(wake.Events);
                _dreamFromBed = false;
                if (_session.GetPlayerView().Phase == GamePhase.Ending) SetCue(PresentationKind.Ending);
            }
            else if (_dreamFromBed && kind == PresentationKind.Dream) _dreamFromBed = false;
            switch (kind)
            {
                case PresentationKind.DoorMotion:
                    if (game.Phase == GamePhase.MorningInspection && _roomDoorOpen)
                        SetCue(PresentationKind.MorningInspection);
                    break;
                case PresentationKind.DoorViewReturn:
                    _doorViewActive = false;
                    _roomDoorOpen = false;
                    SetCue(PresentationKind.DoorMotion);
                    break;
                case PresentationKind.EnterTerminal:
                    _terminalPower = TerminalPowerState.Running;
                    if ((!_useTerminalDirectory || _terminalPage == TerminalPage.Review) && game.EncounterStage == EncounterStage.Introduction)
                        SetCue(PresentationKind.Introduction);
                    break;
                case PresentationKind.MorningInspection: SetCue(PresentationKind.Briefing); break;
                case PresentationKind.Reaction:
                    if (!_useTerminalDirectory && _session.GetPlayerView().Phase == GamePhase.Evening)
                        _terminalPower = TerminalPowerState.ShuttingDown;
                    SetCue(PresentationKind.ExitTerminal);
                    break;
                case PresentationKind.ExitTerminal:
                    if (_useTerminalDirectory && !_leavingDirectory)
                    {
                        _terminalPage = TerminalPage.Archive;
                        if (game.Phase == GamePhase.Evening && game.CurrentEncounterId == null)
                        {
                            _terminalPower = TerminalPowerState.ShuttingDown;
                            _leavingDirectory = true;
                            _archiveDirectoryVisible = false;
                            SetCue(PresentationKind.ExitTerminal);
                            break;
                        }
                    }
                    else
                    {
                        if (_terminalPower == TerminalPowerState.ShuttingDown) _terminalPower = TerminalPowerState.Off;
                        _terminalOpen = false; _terminalPage = TerminalPage.Closed;
                    }
                    _leavingDirectory = false;
                    _archiveDirectoryVisible = false;
                    break;
                case PresentationKind.Sleep:
                    if (_session.GetPlayerView().Phase == GamePhase.Dream) SetCue(PresentationKind.Dream);
                    break;
            }
            return Accept(events);
        }

        public RuntimeResult EnterSelectedPatient(string patientId)
        {
            var view = GetView();
            if (!_useTerminalDirectory || view.Input != InputContext.TerminalDirectory
                || _terminalPage != TerminalPage.Directory)
                return Reject("PatientUnavailable");
            var result = _session.Execute(new SelectPatient(patientId));
            if (!result.Accepted) return Reject(result.Reason.ToString());
            var game = _session.GetPlayerView();
            _terminalPage = TerminalPage.Review;
            _historyEncounterId = game.CurrentEncounterId;
            if (game.EncounterStage == EncounterStage.Introduction) SetCue(PresentationKind.Introduction);
            return Accept(result.Events);
        }

        public RuntimeResult ReturnToDirectory()
        {
            var view = GetView();
            if (!_useTerminalDirectory || view.Input != InputContext.TerminalDirectory || _terminalPage == TerminalPage.SelfReview)
                return Reject("EncounterInProgress");
            if (_terminalPage == TerminalPage.Archive)
            {
                if (view.Game.CurrentEncounterId != null)
                {
                    _terminalPage = TerminalPage.Directory;
                    _archiveDirectoryVisible = false;
                }
                else _archiveDirectoryVisible = true;
            }
            return Accept();
        }

        public RuntimeResult LeaveTerminal()
        {
            var view = GetView();
            if (!_useTerminalDirectory || view.Input != InputContext.TerminalDirectory || _terminalPage == TerminalPage.SelfReview)
                return Reject("EncounterInProgress");
            _leavingDirectory = true;
            SetCue(PresentationKind.ExitTerminal);
            return Accept();
        }

        public string ResolveDialogueText(string sourceId, string original) => _content is PackageGameContent package
            ? package.ResolveText(sourceId, original, EvaluateDialoguePredicate) : original;

        public bool IsNarration(string sourceId) => _content is PackageGameContent package && package.IsNarration(sourceId);

        private string ResolveSegmentText(string nodeId, string fallback)
            => _content is PackageGameContent package && package.TryGetSegment(nodeId, out var segment)
                ? string.Join("\n", segment!.Lines.Select(line => ResolveDialogueText(line.SourceNodeId, line.Text))) : fallback;

        private bool EvaluateDialoguePredicate(string predicate)
        {
            int yesterday = _session.GetPlayerView().Day - 1;
            return predicate switch
            {
                "other_patient_exited_yesterday" => _session.HasDispositionOtherThan("diplomat", yesterday, Disposition.Purify)
                    || _session.HasDispositionOtherThan("diplomat", yesterday, Disposition.Release),
                "asked_diplomat_for_evidence_yesterday" => _session.HasResponse("diplomat", yesterday, "diplomat__AD_D1_R2_O2"),
                _ => throw new InvalidOperationException("Unknown dialogue predicate: " + predicate)
            };
        }

        public bool IsDialogueLineComplete(string sourceId)
        {
            var game = _session.GetPlayerView();
            return _transcript.Any(entry => entry.EncounterId == game.CurrentEncounterId && entry.CoreNodeId == game.CurrentNodeId
                && entry.SourceNodeId == sourceId && !entry.IsPlayer && entry.Completed);
        }

        public void CompleteDialogueLine(string sourceId)
        {
            var view = GetView();
            if (!CanRecordDialogue(view)) return;
            var entry = _transcript.SingleOrDefault(item => item.EncounterId == view.Game.CurrentEncounterId
                && item.CoreNodeId == view.Game.CurrentNodeId && item.SourceNodeId == sourceId && !item.IsPlayer);
            if (entry != null && !entry.Completed) { entry.Completed = true; Accept(); }
        }

        public void RecordDialogueLine(string speaker, string text, string sourceNodeId, string portraitId, bool narration = false)
        {
            var view = GetView();
            if (!CanRecordDialogue(view)
                || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(sourceNodeId)) return;
            if (AddTranscript(view.Game.CurrentEncounterId!, view.Game.CurrentNodeId!, speaker, text, sourceNodeId, portraitId, false, narration))
                Accept();
        }

        private bool CanRecordDialogue(RuntimeView view)
            => _terminalOpen && _terminalPage == TerminalPage.Review && !view.Paused && view.Inspection == null
                && view.Game.CurrentEncounterId != null && view.Game.CurrentNodeId != null
                && view.Game.CurrentEncounterId == _historyEncounterId
                && (view.Input == InputContext.Dialogue || view.Cue?.Kind == PresentationKind.Introduction
                    || view.Cue?.Kind == PresentationKind.ResponseBranch || view.Cue?.Kind == PresentationKind.Reaction);

        private bool AddTranscript(string encounterId, string coreNodeId, string speaker, string text,
            string sourceNodeId, string portraitId, bool isPlayer, bool narration = false)
        {
            var key = encounterId + ":" + (isPlayer ? "player:" + coreNodeId + ":" : "actor:") + sourceNodeId;
            if (_transcript.Any(entry => entry.Key == key)) return false;
            _transcript.Add(new TranscriptEntry(key, encounterId, coreNodeId, speaker, text, sourceNodeId, portraitId, isPlayer, narration));
            _historyEncounterId = encounterId;
            return true;
        }

        public RuntimeResult Inspect(InspectionKind kind)
        {
            var view = GetView();
            if (!Enum.IsDefined(typeof(InspectionKind), kind)) return Reject("UnknownInspection");
            if (view.Input != InputContext.Dialogue || view.Game.CurrentPatientId == null) return Reject("InputLocked");
            var patientId = view.Game.CurrentPatientId;
            _inspection = _content.GetInspection(patientId, view.Game.Day, kind);
            if (!_observations.Any(item => item.Day == view.Game.Day && item.PatientId == patientId && item.Kind == kind))
                _observations.Add(new ObservationRecord { Day = view.Game.Day, PatientId = patientId, Kind = kind });
            return Accept();
        }

        public RuntimeResult CloseInspection()
        {
            if (_paused) return Reject("InputLocked");
            if (_inspection == null) return Reject("InspectionNotOpen");
            _inspection = null;
            return Accept();
        }

        public RuntimeResult SetPaused(bool paused)
        {
            _paused = paused;
            return Accept();
        }

        public string Save()
        {
            var checkpoint = new RuntimeCheckpoint
            {
                Version = IsCampaign ? 2 : 1, ContentRevision = _content.Revision, CoreJson = SnapshotCodec.Serialize(_session),
                TerminalOpen = _terminalOpen, Paused = _paused, CueKind = _cue?.Kind,
                OpenInspectionKind = _inspection?.Kind, UseTerminalDirectory = _useTerminalDirectory,
                Page = _terminalPage, LeavingDirectory = _leavingDirectory, HistoryEncounterId = _historyEncounterId,
                TerminalPowerDay = _terminalPowerDay, TerminalPower = _terminalPower,
                ArchiveDirectoryVisible = _archiveDirectoryVisible,
                RoomDoorOpen = _roomDoorOpen,
                DoorViewActive = _doorViewActive,
                DreamFromBed = _dreamFromBed,
                Transcript = _transcript.Select(entry => new TranscriptRecord
                {
                    Key = entry.Key, EncounterId = entry.EncounterId, CoreNodeId = entry.CoreNodeId,
                    Speaker = entry.Speaker, Text = entry.Text, SourceNodeId = entry.SourceNodeId,
                    PortraitId = entry.PortraitId, IsPlayer = entry.IsPlayer, IsNarration = entry.IsNarration, Completed = entry.Completed
                }).ToList(),
                Observations = _observations.Select(item => new ObservationRecord
                    { Day = item.Day, PatientId = item.PatientId, Kind = item.Kind }).ToList()
            };
            using var buffer = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonTextWriter(buffer) { Formatting = Formatting.Indented };
            JsonSerializer.Create(JsonSettings()).Serialize(writer, checkpoint);
            writer.Flush();
            return buffer.ToString();
        }

        public static GameRuntime Restore(string json, GameRules rules, IGameContent content)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (content == null) throw new ArgumentNullException(nameof(content));
            try
            {
                using var buffer = new StringReader(json ?? throw new ArgumentNullException(nameof(json)));
                using var reader = new JsonTextReader(buffer) { MaxDepth = 64, DateParseHandling = DateParseHandling.None };
                var checkpoint = JsonSerializer.Create(JsonSettings()).Deserialize<RuntimeCheckpoint>(reader)
                    ?? throw new SnapshotValidationException("Runtime checkpoint must be an object.");
                if (reader.Read()) throw new SnapshotValidationException("Runtime checkpoint contains additional JSON.");
                if (checkpoint.Version != (rules.StartDay == 0 ? 2 : 1) || checkpoint.ContentRevision != content.Revision)
                    throw new SnapshotValidationException("Runtime checkpoint version or content revision does not match.");
                var session = SnapshotCodec.Deserialize(checkpoint.CoreJson, rules);
                ValidateCheckpoint(checkpoint, session, rules);
                var runtime = new GameRuntime(session, rules, content, checkpoint.UseTerminalDirectory)
                {
                    _terminalOpen = checkpoint.TerminalOpen, _paused = checkpoint.Paused,
                    _terminalPage = checkpoint.Page ?? (checkpoint.TerminalOpen ? TerminalPage.Review : TerminalPage.Closed),
                    _leavingDirectory = checkpoint.LeavingDirectory, _historyEncounterId = checkpoint.HistoryEncounterId,
                    _terminalPowerDay = session.GetPlayerView().Day,
                    _terminalPower = rules.DreamFlowEnabled && session.GetPlayerView().Phase == GamePhase.SelfReview
                        ? TerminalPowerState.Running : checkpoint.TerminalPower ?? InferLegacyTerminalPower(checkpoint, session),
                    _archiveDirectoryVisible = checkpoint.ArchiveDirectoryVisible,
                    _roomDoorOpen = checkpoint.RoomDoorOpen,
                    _doorViewActive = checkpoint.DoorViewActive,
                    _dreamFromBed = checkpoint.DreamFromBed
                };
                runtime._transcript.AddRange(checkpoint.Transcript.Select(entry => new TranscriptEntry(entry.Key,
                    entry.EncounterId, entry.CoreNodeId, entry.Speaker, entry.Text, entry.SourceNodeId, entry.PortraitId, entry.IsPlayer,
                    entry.IsNarration, entry.Completed)));
                runtime._observations.AddRange(checkpoint.Observations);
                if (checkpoint.CueKind == PresentationKind.DoorMotion)
                {
                    // The host snaps the physical door to its persisted target. Continue only the required story cue.
                    if (session.GetPlayerView().Phase == GamePhase.MorningInspection)
                        runtime.SetCue(PresentationKind.MorningInspection);
                }
                else if (checkpoint.CueKind == PresentationKind.DoorViewReturn)
                {
                    // There is no persisted camera pose. Restore directly to the room instead of replaying a return.
                    runtime._doorViewActive = false;
                    runtime._roomDoorOpen = false;
                }
                else if (checkpoint.CueKind.HasValue) runtime.SetCue(checkpoint.CueKind.Value);
                if (checkpoint.OpenInspectionKind.HasValue)
                {
                    var view = session.GetPlayerView();
                    runtime._inspection = content.GetInspection(view.CurrentPatientId!, view.Day, checkpoint.OpenInspectionKind.Value);
                }
                return runtime;
            }
            catch (JsonException error)
            {
                throw new SnapshotValidationException("Runtime checkpoint JSON is invalid.", error);
            }
        }

        private static void ValidateCheckpoint(RuntimeCheckpoint checkpoint, GameSession session, GameRules rules)
        {
            var game = session.GetPlayerView();
            var doorMotion = checkpoint.CueKind == PresentationKind.DoorMotion;
            var doorReturn = checkpoint.CueKind == PresentationKind.DoorViewReturn;
            var cue = doorMotion || doorReturn ? null : checkpoint.CueKind;
            var open = checkpoint.TerminalOpen;
            if (checkpoint.DreamFromBed && (!rules.DreamFlowEnabled || open
                || !((game.Phase == GamePhase.Evening && cue == PresentationKind.Sleep)
                    || (game.Phase == GamePhase.Dream && cue == PresentationKind.Dream))))
                throw new SnapshotValidationException("Bed-started dream does not match its presentation checkpoint.");
            if (checkpoint.TerminalPower.HasValue != checkpoint.TerminalPowerDay.HasValue)
                throw new SnapshotValidationException("Terminal power and day must be saved together.");
            if (checkpoint.TerminalPower.HasValue)
            {
                var power = checkpoint.TerminalPower.Value;
                if (!Enum.IsDefined(typeof(TerminalPowerState), power) || checkpoint.TerminalPowerDay != game.Day
                    || (power == TerminalPowerState.Booting && (!open || cue != PresentationKind.EnterTerminal))
                    || (power == TerminalPowerState.ShuttingDown && (!open || cue != PresentationKind.ExitTerminal
                        || game.Phase != GamePhase.Evening || checkpoint.UseTerminalDirectory && !checkpoint.LeavingDirectory))
                    || (power == TerminalPowerState.Off && (open || game.Phase == GamePhase.Work))
                    || (power == TerminalPowerState.Running && (game.Phase == GamePhase.MorningInspection
                        || game.Phase == GamePhase.Briefing || game.Phase == GamePhase.Apartment)))
                    throw new SnapshotValidationException("Terminal power does not match the daily presentation checkpoint.");
            }
            if (checkpoint.RoomDoorOpen && !DoorCanRemainOpen(game.Phase))
                throw new SnapshotValidationException("The room door must be closed outside the working day.");
            if (checkpoint.DoorViewActive && (!checkpoint.RoomDoorOpen || open || checkpoint.OpenInspectionKind.HasValue
                || !DoorCanRemainOpen(game.Phase)
                || (checkpoint.CueKind.HasValue && !doorMotion && !doorReturn
                    && checkpoint.CueKind != PresentationKind.MorningInspection && checkpoint.CueKind != PresentationKind.Briefing)
                || (game.Phase == GamePhase.MorningInspection && !doorMotion && checkpoint.CueKind != PresentationKind.MorningInspection)))
                throw new SnapshotValidationException("Door view does not match an open room door and its presentation checkpoint.");
            if (doorReturn && (!checkpoint.DoorViewActive
                || (game.Phase != GamePhase.Apartment && game.Phase != GamePhase.Work)))
                throw new SnapshotValidationException("Door view return requires a completed outside-door presentation.");
            if (doorMotion && (open || checkpoint.OpenInspectionKind.HasValue || !DoorCanRemainOpen(game.Phase)
                || game.Phase == GamePhase.Briefing
                || (game.Phase == GamePhase.MorningInspection && !checkpoint.RoomDoorOpen)))
                throw new SnapshotValidationException("Door motion does not match an available room interaction.");
            var snapshot = session.CreateSnapshot();
            var current = snapshot.State.Encounters.SingleOrDefault(item => item.Id == game.CurrentEncounterId);
            var firstRound = current != null && current.NormalRoundIndex == 0 && current.Responses.Count == 0
                && (current.Stage == EncounterStage.Introduction || current.Stage == EncounterStage.NormalDialogue);
            var afterReaction = snapshot.CommandHistory.LastOrDefault()?.Kind == CommandKind.CompleteReaction;
            var page = checkpoint.Page ?? (open ? TerminalPage.Review : TerminalPage.Closed);
            if (page == TerminalPage.SelfReview && (!rules.DreamFlowEnabled || game.Phase != GamePhase.SelfReview))
                throw new SnapshotValidationException("Self review requires its dream-flow checkpoint.");
            if (!Enum.IsDefined(typeof(TerminalPage), page) || (page != TerminalPage.Closed) != open
                || (!checkpoint.UseTerminalDirectory && page != TerminalPage.SelfReview && page != (open ? TerminalPage.Review : TerminalPage.Closed))
                || (checkpoint.LeavingDirectory && (!checkpoint.UseTerminalDirectory || cue != PresentationKind.ExitTerminal
                    || (page != TerminalPage.Directory && page != TerminalPage.Archive))))
                throw new SnapshotValidationException("Terminal page does not match the presentation checkpoint.");
            if (checkpoint.ArchiveDirectoryVisible && (!checkpoint.UseTerminalDirectory || page != TerminalPage.Archive))
                throw new SnapshotValidationException("Archived directory does not match the terminal page.");
            var valid = game.Phase switch
            {
                GamePhase.MorningInspection => !open && (cue == null || cue == PresentationKind.MorningInspection),
                GamePhase.Briefing => !open && cue == PresentationKind.Briefing,
                GamePhase.Apartment => !open && cue == null,
                GamePhase.Work => game.EncounterStage switch
                {
                    EncounterStage.Introduction => (open && cue == PresentationKind.Introduction)
                        || (firstRound && open && cue == PresentationKind.EnterTerminal)
                        || (firstRound && afterReaction && ((!open && cue == null) || (open && cue == PresentationKind.ExitTerminal))),
                    EncounterStage.NormalDialogue => (open && cue == null)
                        || (firstRound && open && cue == PresentationKind.EnterTerminal)
                        || (firstRound && afterReaction && ((!open && cue == null) || (open && cue == PresentationKind.ExitTerminal))),
                    EncounterStage.SpecialDialogue => open && cue == null,
                    EncounterStage.ResponseBranch => open && cue == PresentationKind.ResponseBranch,
                    EncounterStage.AwaitingDisposition => open && cue == null,
                    EncounterStage.Reaction => open && cue == PresentationKind.Reaction,
                    _ => false
                },
                GamePhase.Evening => (!open && (cue == null || checkpoint.DreamFromBed && cue == PresentationKind.Sleep))
                    || (afterReaction && open && cue == PresentationKind.ExitTerminal),
                GamePhase.Sleeping => !open && cue == PresentationKind.Sleep,
                GamePhase.Dream => !open && cue == PresentationKind.Dream,
                GamePhase.AfterDream => !open && (cue == null || rules.DreamFlowEnabled && game.Day == 0 && cue == PresentationKind.Sleep),
                GamePhase.SelfReview => rules.DreamFlowEnabled && ((!open && cue == null)
                    || (open && page == TerminalPage.SelfReview && (cue == null || cue == PresentationKind.EnterTerminal))),
                GamePhase.Ending => !open && (cue == null || cue == PresentationKind.Ending),
                _ => false
            };
            if (checkpoint.UseTerminalDirectory)
            {
                var unselected = firstRound && !current!.ExplicitlySelected;
                var directoryPage = page == TerminalPage.Directory && unselected && game.Phase == GamePhase.Work;
                var archivePage = page == TerminalPage.Archive && afterReaction
                    && (game.Phase == GamePhase.Evening || (firstRound && game.Phase == GamePhase.Work));
                if (open && (directoryPage || archivePage)
                    && (cue == null || cue == PresentationKind.EnterTerminal || cue == PresentationKind.ExitTerminal)) valid = true;
                if (!open && page == TerminalPage.Closed && unselected && game.Phase == GamePhase.Work && cue == null) valid = true;
                if (page == TerminalPage.Directory && (!directoryPage
                    || (cue != null && cue != PresentationKind.EnterTerminal && !(cue == PresentationKind.ExitTerminal && checkpoint.LeavingDirectory)))) valid = false;
                if (page == TerminalPage.Archive && (!archivePage
                    || (cue != null && !(cue == PresentationKind.ExitTerminal && checkpoint.LeavingDirectory)))) valid = false;
            }
            if (!valid) throw new SnapshotValidationException("Presentation checkpoint does not match the core phase.");
            if (checkpoint.Transcript == null || checkpoint.Transcript.Any(entry => entry == null))
                throw new SnapshotValidationException("Transcript records are required.");
            var transcriptKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in checkpoint.Transcript)
            {
                var encounter = snapshot.State.Encounters.SingleOrDefault(item => item.Id == entry.EncounterId);
                if (encounter == null || string.IsNullOrWhiteSpace(entry.CoreNodeId) || string.IsNullOrWhiteSpace(entry.SourceNodeId)
                    || string.IsNullOrWhiteSpace(entry.Text) || entry.Speaker == null || entry.PortraitId == null
                    || entry.Key != entry.EncounterId + ":" + (entry.IsPlayer ? "player:" + entry.CoreNodeId + ":" : "actor:") + entry.SourceNodeId
                    || !transcriptKeys.Add(entry.Key))
                    throw new SnapshotValidationException("Invalid or duplicate transcript record.");
                if (entry.IsPlayer && !entry.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal)
                    && !encounter.Responses.Any(response => response.NodeId == entry.CoreNodeId && response.ResponseId == entry.SourceNodeId))
                    throw new SnapshotValidationException("Transcript reply was not accepted by the core.");
                if (entry.IsPlayer && entry.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal)
                    && (!encounter.Disposition.HasValue || entry.SourceNodeId != "disposition:" + encounter.Disposition.Value))
                    throw new SnapshotValidationException("Transcript disposition was not accepted by the core.");
            }
            if (checkpoint.HistoryEncounterId != null && !snapshot.State.Encounters.Any(item => item.Id == checkpoint.HistoryEncounterId))
                throw new SnapshotValidationException("Transcript encounter does not exist.");
            if (checkpoint.UseTerminalDirectory && (page == TerminalPage.Archive
                || (page == TerminalPage.Review && cue == PresentationKind.ExitTerminal && !checkpoint.LeavingDirectory))
                && checkpoint.HistoryEncounterId != snapshot.CommandHistory.LastOrDefault()?.EncounterId)
                throw new SnapshotValidationException("Archived transcript does not match the completed encounter.");
            if (checkpoint.UseTerminalDirectory && page == TerminalPage.Review && cue != PresentationKind.ExitTerminal
                && checkpoint.HistoryEncounterId != game.CurrentEncounterId)
                throw new SnapshotValidationException("Review transcript does not match the current encounter.");
            if (checkpoint.Observations == null || checkpoint.Observations.Any(item => item == null))
                throw new SnapshotValidationException("Observation records are required.");
            var state = snapshot.State;
            var unique = new HashSet<(int Day, string PatientId, InspectionKind Kind)>();
            foreach (var item in checkpoint.Observations)
            {
                if (item.Day < rules.StartDay || item.Day > game.Day || !Enum.IsDefined(typeof(InspectionKind), item.Kind)
                    || !rules.Patients.Any(profile => profile.Id == item.PatientId && profile.FirstDay <= item.Day)
                    || !state.Encounters.Any(encounter => encounter.Day == item.Day && encounter.PatientId == item.PatientId)
                    || !unique.Add((item.Day, item.PatientId, item.Kind)))
                    throw new SnapshotValidationException("Invalid or duplicate observation record.");
            }
            if (checkpoint.OpenInspectionKind.HasValue && (!open || page != TerminalPage.Review || cue != null || game.Phase != GamePhase.Work
                || !Enum.IsDefined(typeof(InspectionKind), checkpoint.OpenInspectionKind.Value)
                || !checkpoint.Observations.Any(item => item.Day == game.Day && item.PatientId == game.CurrentPatientId
                    && item.Kind == checkpoint.OpenInspectionKind.Value)))
                throw new SnapshotValidationException("Open inspection does not match a recorded observation.");
        }

        private static JsonSerializerSettings JsonSettings() => new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture, TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Error,
            MaxDepth = 64, Converters = { new StringEnumConverter { AllowIntegerValues = false } }
        };

        private IEnumerable<InteractionTarget> WorldInteractions(PlayerView view)
        {
            if (view.Phase == GamePhase.MorningInspection) return new[] { InteractionTarget.Door };
            if (view.Phase == GamePhase.SelfReview && UsesDreamFlow) return new[] { InteractionTarget.Computer };
            if (view.Phase == GamePhase.Apartment || (view.Phase == GamePhase.Work && !_terminalOpen))
                return IsDoorLockedAfterSupervisor && !_roomDoorOpen
                    ? new[] { InteractionTarget.Computer }
                    : new[] { InteractionTarget.Computer, InteractionTarget.Door };
            if (view.Phase == GamePhase.Evening || view.Phase == GamePhase.AfterDream)
                return new[] { InteractionTarget.Bed };
            return Array.Empty<InteractionTarget>();
        }

        private void SetCue(PresentationKind kind)
        {
            // Fresh tokens invalidate callbacks from prior loads or abandoned host instances.
            var view = _session.GetPlayerView();
            var text = kind == PresentationKind.DoorMotion || kind == PresentationKind.DoorViewReturn
                || (kind == PresentationKind.Ending && view.Ending == EndingType.DreamSea) ? ""
                : kind == PresentationKind.Introduction ? _content.GetDialogue(view).Text : _content.GetPresentation(kind, view);
            if (view.CurrentNodeId != null && (kind == PresentationKind.Introduction
                || kind == PresentationKind.ResponseBranch || kind == PresentationKind.Reaction))
                text = ResolveSegmentText(view.CurrentNodeId, text);
            _cue = new PresentationCue(Guid.NewGuid().ToString("N"), kind, text);
        }

        private RuntimeResult Accept(IEnumerable<GameEvent>? events = null)
        {
            var game = _session.GetPlayerView();
            if (_terminalPowerDay != game.Day)
            {
                _terminalPowerDay = game.Day;
                _terminalPower = TerminalPowerState.Off;
            }
            if (UsesDreamFlow && game.Phase == GamePhase.SelfReview)
                _terminalPower = TerminalPowerState.Running;
            if (!DoorCanRemainOpen(_session.GetPlayerView().Phase))
            {
                _roomDoorOpen = false;
                _doorViewActive = false;
            }
            _cachedView = null;
            Changed?.Invoke(GetView());
            return new RuntimeResult("", events);
        }

        private static RuntimeResult Reject(string reason) => new RuntimeResult(reason);

        private static TerminalPowerState InferLegacyTerminalPower(RuntimeCheckpoint checkpoint, GameSession session)
        {
            var game = session.GetPlayerView();
            if (checkpoint.TerminalOpen && checkpoint.CueKind == PresentationKind.EnterTerminal) return TerminalPowerState.Booting;
            if (checkpoint.TerminalOpen && checkpoint.CueKind == PresentationKind.ExitTerminal
                && game.Phase == GamePhase.Evening && (!checkpoint.UseTerminalDirectory || checkpoint.LeavingDirectory))
                return TerminalPowerState.ShuttingDown;
            if (checkpoint.TerminalOpen || game.Phase == GamePhase.Work || game.Phase == GamePhase.SelfReview)
                return TerminalPowerState.Running;
            return TerminalPowerState.Off;
        }

        private static bool DoorCanRemainOpen(GamePhase phase) => phase == GamePhase.MorningInspection
            || phase == GamePhase.Briefing || phase == GamePhase.Apartment || phase == GamePhase.Work;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class RuntimeCheckpoint
    {
        [JsonProperty(Required = Required.Always)] public int Version { get; set; }
        [JsonProperty(Required = Required.Always)] public string ContentRevision { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string CoreJson { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public bool TerminalOpen { get; set; }
        [JsonProperty(Required = Required.Always)] public bool Paused { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public PresentationKind? CueKind { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public InspectionKind? OpenInspectionKind { get; set; }
        [JsonProperty(Required = Required.Always)] public List<ObservationRecord> Observations { get; set; } = new List<ObservationRecord>();
        [JsonProperty] public bool UseTerminalDirectory { get; set; }
        [JsonProperty] public TerminalPage? Page { get; set; }
        [JsonProperty] public int? TerminalPowerDay { get; set; }
        [JsonProperty] public TerminalPowerState? TerminalPower { get; set; }
        [JsonProperty] public bool LeavingDirectory { get; set; }
        [JsonProperty] public bool ArchiveDirectoryVisible { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool RoomDoorOpen { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool DoorViewActive { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool DreamFromBed { get; set; }
        [JsonProperty] public string? HistoryEncounterId { get; set; }
        [JsonProperty] public List<TranscriptRecord> Transcript { get; set; } = new List<TranscriptRecord>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class TranscriptRecord
    {
        [JsonProperty(Required = Required.Always)] public string Key { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string EncounterId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string CoreNodeId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string Speaker { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string Text { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string SourceNodeId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string PortraitId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public bool IsPlayer { get; set; }
        [JsonProperty] public bool IsNarration { get; set; }
        [JsonProperty] public bool Completed { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class ObservationRecord
    {
        [JsonProperty(Required = Required.Always)] public int Day { get; set; }
        [JsonProperty(Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public InspectionKind Kind { get; set; }
    }
}
