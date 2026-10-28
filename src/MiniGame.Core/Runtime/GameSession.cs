#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MiniGame.Core
{
    /// <summary>
    /// A deterministic, single-threaded session. Only accepted commands mutate state.
    /// The host owns text, animation, input and persistence; this class owns game rules.
    /// </summary>
    public sealed class GameSession
    {
        private readonly GameRules _rules;
        private readonly GameStateSnapshot _state;
        private readonly List<CommandRecord> _history = new List<CommandRecord>();

        public GameSession(GameRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _state = new GameStateSnapshot
            {
                Day = rules.StartDay, Phase = rules.StartDay == 0 ? GamePhase.Apartment : GamePhase.MorningInspection,
                Ending = EndingType.None,
                Rebellion = rules.DreamFlowEnabled ? rules.InitialRebellion : 0,
                Patients = rules.Patients.Select(p => new PatientSnapshot
                    { Id = p.Id, Trust = p.InitialTrust, Presence = PatientPresence.Active }).ToList()
            };
            PrepareDay();
        }

        public CommandResult Execute(GameCommand? command)
        {
            var events = new List<GameEvent>();
            if (command == null) return new CommandResult(RejectionReason.InvalidCommand, events);
            if (_state.Phase == GamePhase.Ending && !(command is EnterGmEnding))
                return new CommandResult(RejectionReason.GameEnded, events);
            RejectionReason reason;
            switch (command)
            {
                case CompleteMorningInspection _: reason = AdvancePhase(GamePhase.MorningInspection, GamePhase.Briefing, events); break;
                case CompleteBriefing _: reason = AdvancePhase(GamePhase.Briefing, GamePhase.Apartment, events); break;
                case CompleteSleep _: reason = AdvancePhase(GamePhase.Sleeping, GamePhase.Dream, events); break;
                case CompleteDream _: reason = FinishDream(events); break;
                case BeginNightDream _: reason = StartNightDream(events); break;
                case EnterHiddenEndingCheat _: reason = EnterHiddenEnding(events); break;
                case EnterGmEnding c: reason = EnterGmRoute(c, events); break;
                case ConfirmSelfPurification _: reason = SelfPurify(events); break;
                case AdvanceSelfReviewCommand c: reason = AdvanceSelfReview(c, events); break;
                case Interact c: reason = InteractWith(c, events); break;
                case SelectPatient c: reason = SelectPatientForToday(c, events); break;
                case SelectResponse c: reason = Select(c, events); break;
                case CompleteResponseBranch c: reason = FinishBranch(c, events); break;
                case SubmitDisposition c: reason = Dispose(c, events); break;
                case CompleteReaction c: reason = FinishReaction(c, events); break;
                case CompleteIntroduction c: reason = FinishIntroduction(c, events); break;
                default: reason = RejectionReason.InvalidCommand; break;
            }
            if (reason == RejectionReason.None) _history.Add(CommandRecord.From(command));
            return new CommandResult(reason, events);
        }

        public PlayerView GetPlayerView()
        {
            var encounter = CurrentEncounter();
            var record = _state.Days.SingleOrDefault(d => d.Day == _state.Day);
            var remaining = record == null ? 0 : Math.Max(0, record.PurificationQuota - record.Purifications);
            var responses = new List<string>();
            var dispositions = new List<Disposition>();
            if (encounter != null)
            {
                if (encounter.Stage == EncounterStage.NormalDialogue || encounter.Stage == EncounterStage.SpecialDialogue)
                    responses.AddRange(CurrentRound(encounter).Responses.Select(r => r.Id));
                if (encounter.Stage == EncounterStage.AwaitingDisposition)
                {
                    dispositions.AddRange(Meeting(encounter).ReactionNodes.Keys.OrderBy(d => d)
                        .Where(d => d != Disposition.Release || CanRelease(encounter)));
                }
            }
            return new PlayerView(_state.Day, _state.Phase, _state.Ending, remaining,
                encounter?.PatientId, encounter == null ? null : _rules.Patient(encounter.PatientId).DisplayName,
                encounter?.Id, encounter == null ? null : CurrentNode(encounter), encounter?.Stage,
                responses, dispositions, AllowedInteractions(), _state.Rebellion,
                record?.Purifications ?? 0, record?.PurificationQuota ?? 0,
                _state.SelfReviewLineIndex, _rules.DreamFlowEnabled ? _rules.SelfReviewLineCount : 0,
                _state.Phase == GamePhase.Dream ? ResolveDreamReturnPhase() : (GamePhase?)null,
                encounter != null && (_state.Day == 0 || Meeting(encounter).ReactionNodes.ContainsKey(Disposition.Release))
                    ? new SocialAdaptationReading(SocialAdaptationTrust(encounter), Meeting(encounter).ReleaseTrustThreshold)
                    : null);
        }

        // Disposition reactions express a temporary emotional response. Derive the
        // reading from the accepted encounter so replay restores it without adding
        // a second authoritative trust value or carrying the effect into tomorrow.
        private long SocialAdaptationTrust(EncounterSnapshot encounter)
        {
            long trust = Patient(encounter.PatientId).Trust;
            if (encounter.Stage == EncounterStage.Reaction && encounter.Disposition.HasValue
                && Meeting(encounter).DispositionTrustDeltas.TryGetValue(encounter.Disposition.Value, out var delta))
                trust += delta;
            return trust;
        }

        public GameSnapshot CreateSnapshot() => SnapshotCodec.Copy(new GameSnapshot
        {
            SchemaVersion = SnapshotCodec.CurrentSchemaVersion,
            RulesId = _rules.RulesId, RulesFingerprint = _rules.Fingerprint,
            State = _state, CommandHistory = _history
        });

        // Read-only facts from accepted commands, for content that refers to previous choices.
        public bool HasResponse(string patientId, int day, string responseId)
            => _state.Encounters.Any(e => e.PatientId == patientId && e.Day == day &&
                e.Responses.Any(r => r.ResponseId == responseId));

        public bool HasDispositionOtherThan(string patientId, int day, Disposition disposition)
            => _state.Encounters.Any(e => e.PatientId != patientId && e.Day == day && e.Disposition == disposition);

        public bool CanReceiveToday(string patientId)
            => CanSelectPendingEncounter() && _state.Queue.Skip(_state.QueueIndex).Contains(patientId)
                && Patient(patientId).Presence == PatientPresence.Active
                && !_state.Encounters.Any(e => e.Day == _state.Day && e.PatientId == patientId
                    && e.Stage == EncounterStage.Completed);

        private bool CanSelectPendingEncounter()
        {
            var current = CurrentEncounter();
            if (_state.Phase != GamePhase.Work || current == null || current.ExplicitlySelected
                || current.NormalRoundIndex != 0 || current.Responses.Count != 0 || current.Disposition.HasValue)
                return false;
            var initialStage = Meeting(current).IntroNodeId == null
                ? EncounterStage.NormalDialogue : EncounterStage.Introduction;
            return current.Stage == initialStage;
        }

        private RejectionReason SelectPatientForToday(SelectPatient command, List<GameEvent> events)
        {
            if (_state.Phase != GamePhase.Work) return RejectionReason.WrongPhase;
            if (!CanReceiveToday(command.PatientId)) return RejectionReason.PatientUnavailable;
            var current = CurrentEncounter()!;
            if (current.PatientId != command.PatientId)
            {
                // Only an unpresented, untouched reservation can be replaced. Completed records stay in place.
                _state.Encounters.Remove(current);
                var selectedIndex = _state.Queue.IndexOf(command.PatientId, _state.QueueIndex);
                _state.Queue.RemoveAt(selectedIndex);
                _state.Queue.Insert(_state.QueueIndex, command.PatientId);
                StartNextEncounterOrEndWork(events);
                current = CurrentEncounter()!;
            }
            current.ExplicitlySelected = true;
            return RejectionReason.None;
        }

        public static GameSession Restore(GameSnapshot snapshot, GameRules rules)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (snapshot.SchemaVersion != SnapshotCodec.CurrentSchemaVersion)
                throw new SnapshotValidationException("Unsupported snapshot schema version.");
            if (snapshot.RulesId != rules.RulesId || snapshot.RulesFingerprint != rules.Fingerprint)
                throw new SnapshotValidationException("Snapshot rules do not match the supplied rules.");
            if (snapshot.State == null || snapshot.CommandHistory == null)
                throw new SnapshotValidationException("Snapshot state and command history are required.");
            try
            {
                var owned = SnapshotCodec.Copy(snapshot);
                var session = new GameSession(rules);
                foreach (var record in owned.CommandHistory)
                {
                    if (record == null) throw new SnapshotValidationException("Null command history entry.");
                    // Replay happens only inside this new session, without publishing host events.
                    if (!session.Execute(record.ToCommand()).Accepted)
                        throw new SnapshotValidationException("Snapshot contains an illegal command sequence.");
                }
                if (SnapshotCodec.Canonical(session._state) != SnapshotCodec.Canonical(owned.State))
                    throw new SnapshotValidationException("Saved state is inconsistent with its command history.");
                return session;
            }
            catch (JsonException exception)
            {
                throw new SnapshotValidationException("Snapshot contains invalid fields.", exception);
            }
        }

        private RejectionReason AdvancePhase(GamePhase expected, GamePhase next, List<GameEvent> events)
        {
            if (_state.Phase != expected) return RejectionReason.WrongPhase;
            SetPhase(next, events);
            return RejectionReason.None;
        }

        private RejectionReason StartNightDream(List<GameEvent> events)
        {
            if (!_rules.DreamFlowEnabled || _state.Phase != GamePhase.Evening) return RejectionReason.WrongPhase;
            SetPhase(_state.Day == 0 ? GamePhase.AfterDream : GamePhase.Dream, events);
            return RejectionReason.None;
        }

        private RejectionReason FinishDream(List<GameEvent> events)
        {
            if (_state.Phase != GamePhase.Dream) return RejectionReason.WrongPhase;
            var destination = ResolveDreamReturnPhase();
            if (destination == GamePhase.SelfReview) _state.SelfReviewLineIndex = 0;
            SetPhase(destination, events);
            return RejectionReason.None;
        }

        private GamePhase ResolveDreamReturnPhase() => _rules.DreamFlowEnabled && _state.Day == 3
            && HasHiddenEndingTrust() ? GamePhase.SelfReview : GamePhase.AfterDream;

        private bool HasHiddenEndingTrust()
        {
            // Trust retains signed dialogue changes after a patient leaves.
            // Disposition deltas are reaction-only instrument effects.
            // Only these four histories contribute; the priest, tutorial and player never do.
            return _state.Patients.Where(patient => patient.Id == PrototypeRules.FormerRuler
                || patient.Id == PrototypeRules.Doctor || patient.Id == PrototypeRules.Child
                || patient.Id == PrototypeRules.Diplomat)
                .Sum(patient => (long)patient.Trust) >= _rules.ActivePatientTrustThreshold;
        }

        private RejectionReason AdvanceSelfReview(AdvanceSelfReviewCommand command, List<GameEvent> events)
        {
            if (!_rules.DreamFlowEnabled || _state.Phase != GamePhase.SelfReview) return RejectionReason.WrongPhase;
            if (_state.SelfReviewLineIndex >= _rules.SelfReviewLineCount) return RejectionReason.WrongStage;
            if (command.ExpectedLineIndex != _state.SelfReviewLineIndex) return RejectionReason.WrongNode;
            _state.SelfReviewLineIndex++;
            events.Add(new GameEvent(GameEventKind.SelfReviewLineAdvanced, _state.Day,
                referenceId: _state.SelfReviewLineIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            return RejectionReason.None;
        }

        private RejectionReason EnterHiddenEnding(List<GameEvent> events)
        {
            if (!_rules.DreamFlowEnabled) return RejectionReason.WrongPhase;
            // Record the explicit shortcut so saves replay it without forging patient decisions or trust.
            EndGame(EndingType.DreamSea, events);
            return RejectionReason.None;
        }

        private RejectionReason EnterGmRoute(EnterGmEnding command, List<GameEvent> events)
        {
            if (!_rules.DreamFlowEnabled) return RejectionReason.WrongPhase;
            if (command.Ending == EndingType.None || !Enum.IsDefined(typeof(EndingType), command.Ending))
                return RejectionReason.InvalidCommand;
            // Keep real campaign records intact; only abandon the active presentation and choose its route.
            _state.CurrentEncounterId = null;
            _state.SelfReviewLineIndex = 0;
            if (command.Ending == EndingType.DreamSea)
            {
                _state.Ending = EndingType.None;
                SetPhase(GamePhase.SelfReview, events);
            }
            else EndGame(command.Ending, events);
            return RejectionReason.None;
        }

        private RejectionReason SelfPurify(List<GameEvent> events)
        {
            if (!_rules.DreamFlowEnabled || _state.Phase != GamePhase.SelfReview) return RejectionReason.WrongPhase;
            if (_state.SelfReviewLineIndex != _rules.SelfReviewLineCount) return RejectionReason.WrongStage;
            // This is the player's ending decision, not a patient disposition or a new work day.
            _state.Day++;
            EndGame(EndingType.DreamSea, events);
            return RejectionReason.None;
        }

        private EndingType ResolveEnding(int day, bool priestAttack)
        {
            bool finished = day > _rules.EndDay;
            var eligible = new HashSet<EndingType>();
            if (priestAttack) eligible.Add(EndingType.PriestAttack);
            if ((finished || _rules.DreamFlowEnabled) && _state.Days.Any(item => item.Day > 0 && item.Completed && item.QuotaMet == false))
                eligible.Add(EndingType.LockedInRoom);
            if (finished) eligible.Add(EndingType.TakenAway);
            return _rules.EndingPriority.FirstOrDefault(eligible.Contains);
        }

        private void EndGame(EndingType ending, List<GameEvent> events)
        {
            _state.Ending = ending;
            SetPhase(GamePhase.Ending, events);
            events.Add(new GameEvent(GameEventKind.GameEnded, _state.Day, referenceId: ending.ToString()));
        }

        private RejectionReason InteractWith(Interact command, List<GameEvent> events)
        {
            if (!Enum.IsDefined(typeof(InteractionTarget), command.Target)) return RejectionReason.InvalidCommand;
            if (!AllowedInteractions().Contains(command.Target)) return RejectionReason.InteractionUnavailable;
            if (command.Target == InteractionTarget.Computer)
            {
                SetPhase(GamePhase.Work, events);
                StartNextEncounterOrEndWork(events);
            }
            else if (_state.Phase == GamePhase.Evening) SetPhase(GamePhase.Sleeping, events);
            else AdvanceDay(events);
            return RejectionReason.None;
        }

        private IEnumerable<InteractionTarget> AllowedInteractions()
        {
            if (_state.Phase == GamePhase.Apartment) return new[] { InteractionTarget.Computer };
            if ((_state.Phase == GamePhase.Evening && !_rules.DreamFlowEnabled) || _state.Phase == GamePhase.AfterDream)
                return new[] { InteractionTarget.Bed };
            return Array.Empty<InteractionTarget>();
        }

        private RejectionReason MatchEncounter(string encounterId, out EncounterSnapshot? encounter)
        {
            encounter = CurrentEncounter();
            if (_state.Phase != GamePhase.Work) return RejectionReason.WrongPhase;
            if (encounter == null || encounter.Id != encounterId) return RejectionReason.WrongEncounter;
            return RejectionReason.None;
        }

        private RejectionReason Select(SelectResponse command, List<GameEvent> events)
        {
            var reason = MatchEncounter(command.EncounterId, out var current);
            if (reason != RejectionReason.None) return reason;
            var encounter = current!;
            if (encounter.Stage != EncounterStage.NormalDialogue && encounter.Stage != EncounterStage.SpecialDialogue)
                return RejectionReason.WrongStage;
            var round = CurrentRound(encounter);
            if (command.NodeId != round.NodeId) return RejectionReason.WrongNode;
            var response = round.Responses.SingleOrDefault(r => r.Id == command.ResponseId);
            if (response == null) return RejectionReason.UnknownResponse;
            var patient = Patient(encounter.PatientId);
            int trust;
            try { trust = checked(patient.Trust + response.TrustDelta); }
            catch (OverflowException) { return RejectionReason.NumericOverflow; }

            patient.Trust = trust;
            if (_rules.DreamFlowEnabled)
            {
                _rules.RebellionOptionDeltas.TryGetValue(response.Id, out var rebellionDelta);
                _state.Rebellion = (int)Math.Max(0L, Math.Min(_rules.MaximumRebellion, (long)_state.Rebellion + rebellionDelta));
            }
            encounter.BranchIsSpecial = encounter.Stage == EncounterStage.SpecialDialogue;
            encounter.PendingBranchNodeId = response.BranchNodeId;
            encounter.Stage = EncounterStage.ResponseBranch;
            encounter.Responses.Add(new ResponseRecord
                { NodeId = round.NodeId, ResponseId = response.Id, TrustDelta = response.TrustDelta });
            Emit(events, GameEventKind.ResponseSelected, encounter, response.Id);
            Emit(events, GameEventKind.ResponseBranchStarted, encounter, response.BranchNodeId);
            return RejectionReason.None;
        }

        private RejectionReason FinishBranch(CompleteResponseBranch command, List<GameEvent> events)
        {
            var reason = MatchEncounter(command.EncounterId, out var current);
            if (reason != RejectionReason.None) return reason;
            var encounter = current!;
            if (encounter.Stage != EncounterStage.ResponseBranch) return RejectionReason.WrongStage;
            if (command.BranchNodeId != encounter.PendingBranchNodeId) return RejectionReason.WrongNode;
            encounter.PendingBranchNodeId = null;
            if (encounter.BranchIsSpecial)
            {
                encounter.BranchIsSpecial = false;
                encounter.SpecialCompleted = true;
                AwaitDisposition(encounter, events);
            }
            else
            {
                encounter.NormalRoundIndex++;
                StartDialogueOrDisposition(encounter, events);
            }
            return RejectionReason.None;
        }

        private RejectionReason FinishIntroduction(CompleteIntroduction command, List<GameEvent> events)
        {
            var reason = MatchEncounter(command.EncounterId, out var current);
            if (reason != RejectionReason.None) return reason;
            var encounter = current!;
            if (encounter.Stage != EncounterStage.Introduction) return RejectionReason.WrongStage;
            if (command.NodeId != Meeting(encounter).IntroNodeId) return RejectionReason.WrongNode;
            StartDialogueOrDisposition(encounter, events);
            return RejectionReason.None;
        }

        private void StartDialogueOrDisposition(EncounterSnapshot encounter, List<GameEvent> events)
        {
            var meeting = Meeting(encounter);
            if (encounter.NormalRoundIndex < meeting.NormalRounds.Count)
            {
                encounter.Stage = EncounterStage.NormalDialogue;
                Emit(events, GameEventKind.DialogueNodeStarted, encounter, CurrentNode(encounter));
            }
            else if (meeting.SpecialRound != null && Patient(encounter.PatientId).Trust >= meeting.SpecialTrustThreshold)
            {
                encounter.SpecialStarted = true;
                encounter.Stage = EncounterStage.SpecialDialogue;
                Emit(events, GameEventKind.DialogueNodeStarted, encounter, meeting.SpecialRound.NodeId);
            }
            else AwaitDisposition(encounter, events);
        }

        private void AwaitDisposition(EncounterSnapshot encounter, List<GameEvent> events)
        {
            encounter.Stage = EncounterStage.AwaitingDisposition;
            Emit(events, GameEventKind.DispositionAvailable, encounter);
        }

        private RejectionReason Dispose(SubmitDisposition command, List<GameEvent> events)
        {
            var reason = MatchEncounter(command.EncounterId, out var current);
            if (reason != RejectionReason.None) return reason;
            var encounter = current!;
            if (!Enum.IsDefined(typeof(Disposition), command.Disposition)) return RejectionReason.InvalidCommand;
            if (encounter.Disposition.HasValue) return RejectionReason.AlreadyDisposed;
            if (encounter.Stage != EncounterStage.AwaitingDisposition) return RejectionReason.WrongStage;
            var meeting = Meeting(encounter);
            if (!meeting.ReactionNodes.ContainsKey(command.Disposition)) return RejectionReason.DispositionUnavailable;
            if (command.Disposition == Disposition.Release && !CanRelease(encounter)) return RejectionReason.ReleaseUnavailable;

            var patient = Patient(encounter.PatientId);
            var profile = _rules.Patient(patient.Id);
            if (command.Disposition == Disposition.Treat && patient.TreatmentCount == int.MaxValue)
                return RejectionReason.NumericOverflow;
            if (command.Disposition == Disposition.Purify && CurrentDay().Purifications == int.MaxValue)
                return RejectionReason.NumericOverflow;
            encounter.Disposition = command.Disposition;
            encounter.Stage = EncounterStage.Reaction;
            if (command.Disposition == Disposition.Purify)
            {
                patient.Presence = PatientPresence.Purified;
                CurrentDay().Purifications++;
            }
            else if (command.Disposition == Disposition.Release)
            {
                patient.Presence = PatientPresence.Released;
                if (profile.AttacksAfterRelease)
                    Schedule(encounter, ScheduledEventKind.PriestAttack, _state.Day + profile.AttackDelayDays);
            }
            else
            {
                patient.TreatmentCount++;
                if (profile.TreatmentLimit.HasValue && patient.TreatmentCount >= profile.TreatmentLimit.Value)
                {
                    if (profile.TreatmentDelayDays == 0)
                    {
                        patient.Presence = profile.TreatmentOutcome;
                        Emit(events, GameEventKind.PatientTerminated, encounter, profile.TreatmentOutcome.ToString());
                    }
                    else Schedule(encounter, ScheduledEventKind.TreatmentTermination, _state.Day + profile.TreatmentDelayDays);
                }
            }
            Emit(events, GameEventKind.DispositionCommitted, encounter, command.Disposition.ToString());
            Emit(events, GameEventKind.ReactionRequested, encounter, CurrentNode(encounter));
            return RejectionReason.None;
        }

        private RejectionReason FinishReaction(CompleteReaction command, List<GameEvent> events)
        {
            var reason = MatchEncounter(command.EncounterId, out var current);
            if (reason != RejectionReason.None) return reason;
            var encounter = current!;
            if (encounter.Stage != EncounterStage.Reaction) return RejectionReason.WrongStage;
            encounter.Stage = EncounterStage.Completed;
            Emit(events, GameEventKind.EncounterCompleted, encounter);
            _state.CurrentEncounterId = null;
            _state.QueueIndex++;
            StartNextEncounterOrEndWork(events);
            return RejectionReason.None;
        }

        private void PrepareDay()
        {
            var day = _rules.Day(_state.Day);
            _state.Queue = day.Meetings.Where(m => Patient(m.PatientId).Presence == PatientPresence.Active)
                .Select(m => m.PatientId).ToList();
            _state.QueueIndex = 0;
            _state.CurrentEncounterId = null;
            _state.Days.Add(new DaySnapshot { Day = _state.Day, PurificationQuota = day.PurificationQuota });
        }

        private void StartNextEncounterOrEndWork(List<GameEvent> events)
        {
            if (_state.QueueIndex == _state.Queue.Count)
            {
                var day = CurrentDay();
                day.Completed = true;
                day.QuotaMet = day.Purifications >= day.PurificationQuota;
                events.Add(new GameEvent(GameEventKind.DayCompleted, _state.Day));
                SetPhase(GamePhase.Evening, events);
                return;
            }
            var patientId = _state.Queue[_state.QueueIndex];
            var encounter = new EncounterSnapshot
            {
                Id = $"day{_state.Day}:{patientId}", Day = _state.Day,
                PatientId = patientId,
                Stage = _rules.Meeting(_state.Day, patientId).IntroNodeId == null
                    ? EncounterStage.NormalDialogue : EncounterStage.Introduction
            };
            _state.Encounters.Add(encounter);
            _state.CurrentEncounterId = encounter.Id;
            Emit(events, GameEventKind.EncounterStarted, encounter, CurrentNode(encounter));
            if (encounter.Stage == EncounterStage.Introduction)
                Emit(events, GameEventKind.IntroductionStarted, encounter, CurrentNode(encounter));
        }

        private void AdvanceDay(List<GameEvent> events)
        {
            _state.Day++;
            var priestAttack = false;
            var due = _state.PendingEvents.Where(e => e.DueDay <= _state.Day)
                .OrderBy(e => e.DueDay).ThenBy(e => e.Id, StringComparer.Ordinal).ToList();
            foreach (var scheduled in due)
            {
                _state.PendingEvents.Remove(scheduled);
                if (_state.ExecutedEventIds.Contains(scheduled.Id)) continue;
                _state.ExecutedEventIds.Add(scheduled.Id);
                if (scheduled.Kind == ScheduledEventKind.PriestAttack) priestAttack = true;
                else
                {
                    var patient = Patient(scheduled.PatientId);
                    if (patient.Presence == PatientPresence.Active)
                    {
                        patient.Presence = _rules.Patient(patient.Id).TreatmentOutcome;
                        events.Add(new GameEvent(GameEventKind.PatientTerminated, _state.Day, patient.Id,
                            referenceId: patient.Presence.ToString()));
                    }
                }
                events.Add(new GameEvent(GameEventKind.ScheduledEventExecuted, _state.Day,
                    scheduled.PatientId, referenceId: scheduled.Id));
            }
            var ending = ResolveEnding(_state.Day, priestAttack);
            if (ending != EndingType.None)
            {
                // Evaluate scheduled events at the natural next dawn before the KPI jump.
                // The jump is an ending date, not three fabricated work-day records.
                if (_rules.DreamFlowEnabled && ending == EndingType.LockedInRoom) _state.Day = 4;
                EndGame(ending, events);
                return;
            }
            PrepareDay();
            SetPhase(GamePhase.MorningInspection, events);
        }

        private void Schedule(EncounterSnapshot encounter, ScheduledEventKind kind, int dueDay)
        {
            _state.PendingEvents.Add(new ScheduledEventSnapshot
            {
                Id = $"{encounter.Id}:{kind}", PatientId = encounter.PatientId, Kind = kind, DueDay = dueDay
            });
        }

        private void SetPhase(GamePhase phase, List<GameEvent> events)
        {
            _state.Phase = phase;
            events.Add(new GameEvent(GameEventKind.PhaseChanged, _state.Day, referenceId: phase.ToString()));
        }

        private void Emit(List<GameEvent> events, GameEventKind kind, EncounterSnapshot encounter, string? referenceId = null)
            => events.Add(new GameEvent(kind, _state.Day, encounter.PatientId, encounter.Id, referenceId));

        private PatientSnapshot Patient(string id) => _state.Patients.Single(p => p.Id == id);
        private DaySnapshot CurrentDay() => _state.Days.Single(d => d.Day == _state.Day);
        private EncounterSnapshot? CurrentEncounter() => _state.CurrentEncounterId == null ? null
            : _state.Encounters.Single(e => e.Id == _state.CurrentEncounterId);
        private MeetingDefinition Meeting(EncounterSnapshot encounter) => _rules.Meeting(encounter.Day, encounter.PatientId);
        private bool CanRelease(EncounterSnapshot encounter) => Patient(encounter.PatientId).Trust >= Meeting(encounter).ReleaseTrustThreshold;
        private RoundDefinition CurrentRound(EncounterSnapshot encounter) => encounter.Stage == EncounterStage.SpecialDialogue
            ? Meeting(encounter).SpecialRound! : Meeting(encounter).NormalRounds[encounter.NormalRoundIndex];

        private string? CurrentNode(EncounterSnapshot encounter)
        {
            switch (encounter.Stage)
            {
                case EncounterStage.Introduction: return Meeting(encounter).IntroNodeId;
                case EncounterStage.NormalDialogue: return Meeting(encounter).NormalRounds[encounter.NormalRoundIndex].NodeId;
                case EncounterStage.SpecialDialogue: return Meeting(encounter).SpecialRound!.NodeId;
                case EncounterStage.ResponseBranch: return encounter.PendingBranchNodeId;
                case EncounterStage.Reaction: return Meeting(encounter).ReactionNodes[encounter.Disposition!.Value];
                default: return null;
            }
        }
    }
}
