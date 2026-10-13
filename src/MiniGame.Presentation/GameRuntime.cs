#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MiniGame.Core;

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
        private bool _paused;
        private PresentationCue? _cue;
        private InspectionContent? _inspection;
        private RuntimeView? _cachedView;

        public event Action<RuntimeView>? Changed;

        public GameRuntime(GameRules rules, IGameContent content)
            : this(new GameSession(rules), rules, content) { }

        private GameRuntime(GameSession session, GameRules rules, IGameContent content)
        {
            _session = session;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _content = content ?? throw new ArgumentNullException(nameof(content));
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
                : _terminalOpen ? InputContext.Dialogue : InputContext.World;
            var patients = _rules.Patients.Where(profile => profile.FirstDay <= game.Day).Select(profile =>
            {
                var state = snapshot.Patients.Single(patient => patient.Id == profile.Id);
                return new PatientView(state.Id, profile.DisplayName, state.Presence, state.TreatmentCount,
                    game.CurrentPatientId == state.Id,
                    snapshot.Encounters.Any(encounter => encounter.Day == game.Day && encounter.PatientId == state.Id
                        && encounter.Stage == EncounterStage.Completed));
            });
            var dialogue = _terminalOpen && _cue == null && _inspection == null
                && (game.EncounterStage == EncounterStage.NormalDialogue || game.EncounterStage == EncounterStage.SpecialDialogue)
                ? _content.GetDialogue(game) : null;
            _cachedView = new RuntimeView(game, input, _terminalOpen, _paused, _cue, dialogue, _inspection,
                patients, input == InputContext.World ? WorldInteractions(game) : Array.Empty<InteractionTarget>(),
                _observations.Select(observation => $"{observation.Day}:{observation.PatientId}:{observation.Kind}"));
            return _cachedView;
        }

        public RuntimeResult TryInteract(InteractionTarget target)
        {
            var view = GetView();
            if (!view.AllowedInteractions.Contains(target)) return Reject("InteractionUnavailable");
            if (target == InteractionTarget.Door)
            {
                SetCue(PresentationKind.MorningInspection);
                return Accept();
            }
            if (target == InteractionTarget.Computer)
            {
                CommandResult? result = null;
                if (view.Game.Phase == GamePhase.Apartment)
                {
                    result = _session.Execute(new Interact(target));
                    if (!result.Accepted) return Reject(result.Reason.ToString());
                }
                if (_session.GetPlayerView().Phase == GamePhase.Work)
                {
                    _terminalOpen = true;
                    SetCue(PresentationKind.EnterTerminal);
                }
                return Accept(result?.Events);
            }
            var bedResult = _session.Execute(new Interact(target));
            if (!bedResult.Accepted) return Reject(bedResult.Reason.ToString());
            _terminalOpen = false;
            var phase = _session.GetPlayerView().Phase;
            if (phase == GamePhase.Sleeping) SetCue(PresentationKind.Sleep);
            else if (phase == GamePhase.Ending) SetCue(PresentationKind.Ending);
            return Accept(bedResult.Events);
        }

        public RuntimeResult SelectResponse(string encounterId, string nodeId, string responseId)
        {
            if (GetView().Input != InputContext.Dialogue) return Reject("InputLocked");
            var result = _session.Execute(new SelectResponse(encounterId, nodeId, responseId));
            if (!result.Accepted) return Reject(result.Reason.ToString());
            SetCue(PresentationKind.ResponseBranch);
            return Accept(result.Events);
        }

        public RuntimeResult SubmitDisposition(string encounterId, Disposition disposition)
        {
            if (GetView().Input != InputContext.Dialogue) return Reject("InputLocked");
            var result = _session.Execute(new SubmitDisposition(encounterId, disposition));
            if (!result.Accepted) return Reject(result.Reason.ToString());
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
                PresentationKind.Sleep => new CompleteSleep(),
                PresentationKind.Dream => new CompleteDream(),
                _ => null
            };
            CommandResult? result = command == null ? null : _session.Execute(command);
            if (result != null && !result.Accepted) return Reject(result.Reason.ToString());
            _cue = null;
            switch (kind)
            {
                case PresentationKind.MorningInspection: SetCue(PresentationKind.Briefing); break;
                case PresentationKind.Reaction: SetCue(PresentationKind.ExitTerminal); break;
                case PresentationKind.ExitTerminal: _terminalOpen = false; break;
                case PresentationKind.Sleep: SetCue(PresentationKind.Dream); break;
            }
            return Accept(result?.Events);
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
                Version = 1, ContentRevision = _content.Revision, CoreJson = SnapshotCodec.Serialize(_session),
                TerminalOpen = _terminalOpen, Paused = _paused, CueKind = _cue?.Kind,
                OpenInspectionKind = _inspection?.Kind,
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
                if (checkpoint.Version != 1 || checkpoint.ContentRevision != content.Revision)
                    throw new SnapshotValidationException("Runtime checkpoint version or content revision does not match.");
                var session = SnapshotCodec.Deserialize(checkpoint.CoreJson, rules);
                ValidateCheckpoint(checkpoint, session, rules);
                var runtime = new GameRuntime(session, rules, content)
                {
                    _terminalOpen = checkpoint.TerminalOpen, _paused = checkpoint.Paused
                };
                runtime._observations.AddRange(checkpoint.Observations);
                if (checkpoint.CueKind.HasValue) runtime.SetCue(checkpoint.CueKind.Value);
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
            var cue = checkpoint.CueKind;
            var open = checkpoint.TerminalOpen;
            var snapshot = session.CreateSnapshot();
            var current = snapshot.State.Encounters.SingleOrDefault(item => item.Id == game.CurrentEncounterId);
            var firstRound = current != null && current.NormalRoundIndex == 0 && current.Responses.Count == 0;
            var afterReaction = snapshot.CommandHistory.LastOrDefault()?.Kind == CommandKind.CompleteReaction;
            var valid = game.Phase switch
            {
                GamePhase.MorningInspection => !open && (cue == null || cue == PresentationKind.MorningInspection),
                GamePhase.Briefing => !open && cue == PresentationKind.Briefing,
                GamePhase.Apartment => !open && cue == null,
                GamePhase.Work => game.EncounterStage switch
                {
                    EncounterStage.NormalDialogue => (open && cue == null)
                        || (firstRound && open && cue == PresentationKind.EnterTerminal)
                        || (firstRound && afterReaction && ((!open && cue == null) || (open && cue == PresentationKind.ExitTerminal))),
                    EncounterStage.SpecialDialogue => open && cue == null,
                    EncounterStage.ResponseBranch => open && cue == PresentationKind.ResponseBranch,
                    EncounterStage.AwaitingDisposition => open && cue == null,
                    EncounterStage.Reaction => open && cue == PresentationKind.Reaction,
                    _ => false
                },
                GamePhase.Evening => (!open && cue == null) || (afterReaction && open && cue == PresentationKind.ExitTerminal),
                GamePhase.Sleeping => !open && cue == PresentationKind.Sleep,
                GamePhase.Dream => !open && cue == PresentationKind.Dream,
                GamePhase.AfterDream => !open && cue == null,
                GamePhase.Ending => !open && (cue == null || cue == PresentationKind.Ending),
                _ => false
            };
            if (!valid) throw new SnapshotValidationException("Presentation checkpoint does not match the core phase.");
            if (checkpoint.Observations == null || checkpoint.Observations.Any(item => item == null))
                throw new SnapshotValidationException("Observation records are required.");
            var state = snapshot.State;
            var unique = new HashSet<(int Day, string PatientId, InspectionKind Kind)>();
            foreach (var item in checkpoint.Observations)
            {
                if (item.Day < 1 || item.Day > game.Day || !Enum.IsDefined(typeof(InspectionKind), item.Kind)
                    || !rules.Patients.Any(profile => profile.Id == item.PatientId && profile.FirstDay <= item.Day)
                    || !state.Encounters.Any(encounter => encounter.Day == item.Day && encounter.PatientId == item.PatientId)
                    || !unique.Add((item.Day, item.PatientId, item.Kind)))
                    throw new SnapshotValidationException("Invalid or duplicate observation record.");
            }
            if (checkpoint.OpenInspectionKind.HasValue && (!open || cue != null || game.Phase != GamePhase.Work
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
            if (view.Phase == GamePhase.Apartment || (view.Phase == GamePhase.Work && !_terminalOpen))
                return new[] { InteractionTarget.Computer };
            if (view.Phase == GamePhase.Evening || view.Phase == GamePhase.AfterDream)
                return new[] { InteractionTarget.Bed };
            return Array.Empty<InteractionTarget>();
        }

        private void SetCue(PresentationKind kind)
        {
            // Fresh tokens invalidate callbacks from prior loads or abandoned host instances.
            _cue = new PresentationCue(Guid.NewGuid().ToString("N"), kind,
                _content.GetPresentation(kind, _session.GetPlayerView()));
        }

        private RuntimeResult Accept(IEnumerable<GameEvent>? events = null)
        {
            _cachedView = null;
            Changed?.Invoke(GetView());
            return new RuntimeResult("", events);
        }

        private static RuntimeResult Reject(string reason) => new RuntimeResult(reason);
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
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class ObservationRecord
    {
        [JsonProperty(Required = Required.Always)] public int Day { get; set; }
        [JsonProperty(Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public InspectionKind Kind { get; set; }
    }
}
