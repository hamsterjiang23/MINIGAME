using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using Yarn.Unity;

#nullable enable

namespace MiniGame.UnityHost
{
    /// <summary>Runs presentation nodes; GameRuntime remains the sole owner of gameplay state.</summary>
    public sealed class MiniGameYarnBridge : MonoBehaviour
    {
        [SerializeField] private YarnProject? project;
        private DialogueRunner? _runner;
        private MiniGameYarnPresenter? _presenter;
        private YarnTask _nodeTask = YarnTask.CompletedTask;
        private GameRuntime? _runtime;
        private MiniGameScreen? _screen;
        private NodeRun? _run;
        private int _epoch;
        private bool _stopping;
        private bool _pumpRequested;
        private string _completedKey = "";
        private SupervisorDoorwayView? _supervisor;
        private SupervisorDialogueContent? _supervisorContent;
        private YarnProject? _supervisorProject;
        private SupervisorDialogueCheckpoint? _supervisorCursor, _resumeSupervisor;
        public string SupervisorSourceId => _supervisorCursor?.sourceId ?? "";

        public bool IsRunning => _run != null || _stopping || !_nodeTask.IsCompleted();
        public YarnProject? Project => project;
        public string Diagnostics => $"bound={_runtime != null}; project={project != null}; enabled={isActiveAndEnabled}; "
            + $"epoch={_epoch}; pump={_pumpRequested}; stopping={_stopping}; run={_run?.Key ?? "none"}; "
            + $"nodeTaskComplete={_nodeTask.IsCompleted()}; presenterPending={(_presenter != null ? _presenter.PendingPresentationCount : 0)}; "
            + $"runnerActive={(_runner != null && _runner.IsDialogueRunning)}; "
            + $"runnerTaskComplete={(_runner == null || _runner.DialogueTask.IsCompleted())}";

        public void Configure(YarnProject yarnProject)
        {
            project = yarnProject != null ? yarnProject : throw new ArgumentNullException(nameof(yarnProject));
            InvalidateRun();
            PresentCurrent();
        }

        // Call again after Restore, even when its encounter and node identifiers equal the previous view.
        public void Bind(GameRuntime runtime, MiniGameScreen screen, SupervisorDoorwayView? supervisor = null)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            if (_runtime != null) _runtime.Changed -= OnRuntimeChanged;
            InvalidateRun();
            _runtime = runtime;
            _screen = screen;
            _supervisor = supervisor;
            _supervisorContent = SupervisorDialogueContent.Load();
            _supervisorProject = Resources.Load<YarnProject>("MiniGame/Supervisor/DialogueProject");
            var endings = GetComponent<EndingDialoguePresenter>() ?? gameObject.AddComponent<EndingDialoguePresenter>();
            endings.Bind(runtime, screen, supervisor);
            screen.UseYarnPresentation();
            _runtime.Changed += OnRuntimeChanged;
            PresentCurrent();
        }

        public SupervisorDialogueCheckpoint? CaptureSupervisorCheckpoint()
        {
            var cursor = _supervisorCursor ?? _resumeSupervisor;
            return cursor == null ? null : new SupervisorDialogueCheckpoint {
                revision = cursor.revision, sourceId = cursor.sourceId, day = cursor.day, cue = cursor.cue };
        }

        public void RestoreSupervisorCheckpoint(SupervisorDialogueCheckpoint? checkpoint)
        {
            if (checkpoint == null || _runtime == null || _supervisorContent == null
                || checkpoint.revision != _supervisorContent.revision || checkpoint.day != _runtime.GetView().Game.Day
                || checkpoint.cue != _runtime.GetView().Cue?.Kind
                || !_supervisorContent.TryGetCue(_runtime.GetView(), out _, out var lines)
                || !lines.Any(line => SupervisorDialogueContent.SourcePrefix + line.id == checkpoint.sourceId)) return;
            _resumeSupervisor = checkpoint;
        }

        private void OnRuntimeChanged(RuntimeView view) => PresentCurrent();

        public void PresentCurrent()
        {
            _pumpRequested = true;
            if (_runtime != null && _screen != null)
            {
                var view = _runtime.GetView();
                _screen.SetDialogueInteractable(!view.Paused && view.Inspection == null);
            }
        }

        private void Update()
        {
            if (!_pumpRequested || _runtime == null || _screen == null || project == null || _stopping) return;
            var view = _runtime.GetView();
            if (_runtime.UsesEndingFlow && (view.IsSelfReview || view.Game.Phase == GamePhase.Ending))
            {
                if (_run != null) InvalidateRun();
                _pumpRequested = false;
                return;
            }
            if (view.UsesDreamFlow && (view.Game.Phase == GamePhase.Dream || view.Game.Phase == GamePhase.SelfReview
                || view.Game.Ending == EndingType.DreamSea))
            {
                if (_run != null) InvalidateRun();
                _pumpRequested = false;
                _screen.HideDialogueActions();
                return; // Projection and self review own their presentation and never map to patient Yarn nodes.
            }
            if (_run != null)
            {
                // A successful response changes core to its branch while the current runner finishes.
                if (!_run.ResponseSubmitted && !Matches(_run, view)) InvalidateRun();
                return;
            }
            if (view.Paused || view.Inspection != null) return;
            _pumpRequested = false;
            var cue = view.Cue;
            string nodeName;
            string key;
            DialogueContent? dialogue = null;
            if (cue != null)
            {
                if (cue.Kind == PresentationKind.EnterTerminal || cue.Kind == PresentationKind.ExitTerminal || cue.Kind == PresentationKind.Sleep || cue.Kind == PresentationKind.DoorMotion || cue.Kind == PresentationKind.DoorViewReturn)
                {
                    _screen.SetDialogueText("终端", cue.Text);
                    _screen.HideDialogueActions();
                    return; // These are camera/room transitions completed by the host effects.
                }
                if (cue.Kind == PresentationKind.ResponseBranch || cue.Kind == PresentationKind.Reaction || cue.Kind == PresentationKind.Introduction)
                {
                    if (view.Game.CurrentNodeId == null) throw new InvalidOperationException("A branch/reaction cue requires a core node.");
                    nodeName = PrototypeYarnSource.NodeName(view.Game.CurrentNodeId);
                }
                else nodeName = PrototypeYarnSource.CueNodeName(cue.Kind);
                key = "cue:" + cue.Token;
            }
            else if (view.Dialogue != null && view.Input == InputContext.Dialogue)
            {
                dialogue = view.Dialogue;
                nodeName = PrototypeYarnSource.NodeName(dialogue.NodeId);
                key = "dialogue:" + view.Game.CurrentEncounterId + ":" + dialogue.NodeId;
            }
            else
            {
                _screen.HideDialogueActions();
                if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    _screen.SetDialogueText("处置终端", "会谈结束。请结合已观察的细节与实际陈述，提交一次处置。");
                return;
            }
            if (key == _completedKey) return;
            var nodeProject = project;
            if (_runtime.IsCampaign && _supervisor != null && _supervisorContent != null
                && _supervisorContent.TryGetCue(view, out var supervisorNode, out _))
            {
                nodeName = supervisorNode;
                nodeProject = _supervisorProject;
            }
            if (nodeProject == null) throw new InvalidOperationException("The supervisor Yarn project has not been imported.");
            EnsureRunner();
            if (_runner == null) return;
            if (!nodeProject.NodeNames.Contains(nodeName))
            {
                Debug.LogError("The configured Yarn Project is missing the mapped node: " + nodeName, this);
                _screen.SetDialogueText("内容错误", "对话暂时无法继续，请返回主菜单后重新读取进度。");
                _screen.HideDialogueActions();
                return;
            }
            if (_runner.YarnProject != nodeProject) _runner.SetProject(nodeProject);
            if (nodeProject == project) _runner.VariableStorage.SetValue(PrototypeYarnSource.CueTextVariable, cue?.Text ?? "");
            var run = new NodeRun(_epoch, _runtime, key, view.Game.CurrentEncounterId,
                view.Game.CurrentNodeId, cue?.Token, dialogue);
            _run = run;
            _nodeTask = RunNodeAsync(run, nodeName);
            _nodeTask.Forget();
        }

        public void PrewarmPresentation() => EnsureRunner();

        private void EnsureRunner()
        {
            if (_runner != null || project == null) return;
            var runnerObject = new GameObject("Original Yarn Presentation Runner");
            runnerObject.transform.SetParent(transform, false);
            _runner = runnerObject.AddComponent<DialogueRunner>();
            _runner.autoStart = false;
            _runner.runSelectedOptionAsLine = false;
            _runner.SetProject(project);
            if (_runner.LineProvider is BuiltinLocalisedLineProvider provider)
            {
                provider.LocaleCode = "zh-Hans";
                provider.AssetLocaleCode = "zh-Hans";
            }
            _presenter = runnerObject.AddComponent<MiniGameYarnPresenter>();
            _presenter.Initialize(this);
            _runner.DialoguePresenters = new[] { _presenter };
            _runner.AddCommandHandler("select_response", (Action<string, string>)SelectResponse);
        }

        private async YarnTask RunNodeAsync(NodeRun run, string nodeName)
        {
            var runner = _runner;
            if (runner == null) return;
            try
            {
                await runner.StartDialogue(nodeName);
                await runner.DialogueTask;
                // DialogueTask completes before the runner has disposed its cancellation sources.
                // A later frame prevents a successor node from racing that cleanup.
                await YarnTask.Yield();
                while (IsCurrent(run) && Matches(run, run.Runtime.GetView()) && !CanInteract(run))
                    await YarnTask.Yield();
                if (!IsCurrent(run)) return;
                bool supervisor = _supervisorCursor != null;
                _supervisorCursor = null;
                _run = null;
                _completedKey = run.CueToken != null || run.ResponseSubmitted ? run.Key : "";
                if (run.CueToken != null && Matches(run, run.Runtime.GetView()))
                    run.Runtime.CompleteCue(run.CueToken);
                if (supervisor && run.Runtime.GetView().Game.Ending != EndingType.TakenAway
                    && run.Runtime.GetView().Cue?.Kind != PresentationKind.Briefing) _supervisor?.SetPortrait("JD_01");
                _pumpRequested = true;
            }
            catch (Exception exception)
            {
                if (!IsCurrent(run)) return;
                _run = null;
                _completedKey = run.Key;
                if (_screen != null)
                {
                    _screen.HideDialogueActions();
                    _screen.SetDialogueText("对话错误", "Yarn演出未完成，当前会话保留。请查看运行日志。");
                }
                Debug.LogException(exception, this);
            }
        }

        private void SelectResponse(string encodedNode, string encodedResponse)
        {
            var run = _run;
            if (run == null || run.Dialogue == null || run.EncounterId == null || run.NodeId == null ||
                run.ResponseSubmitted || !CanInteract(run) ||
                PrototypeYarnSource.EncodeIdentifier(run.NodeId) != encodedNode) return;
            var response = run.Dialogue.Responses.SingleOrDefault(r => PrototypeYarnSource.EncodeIdentifier(r.Id) == encodedResponse);
            if (response == null) return;
            var result = run.Runtime.SelectResponse(run.EncounterId, run.NodeId, response.Id);
            run.ResponseSubmitted = result.Accepted;
            if (!result.Accepted) Debug.LogWarning("Core rejected Yarn response: " + result.Reason, this);
        }

        internal async YarnTask PresentLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            var run = _run;
            var screen = _screen;
            if (run == null || screen == null) return;
            while (IsCurrent(run) && !CanInteract(run)) await YarnTask.Yield();
            if (!IsCurrent(run)) return;
            var source = line.Metadata.FirstOrDefault(metadata => metadata.StartsWith("source:", StringComparison.Ordinal));
            var portrait = line.Metadata.FirstOrDefault(metadata => metadata.StartsWith("portrait:", StringComparison.Ordinal));
            string sourceId = source != null ? source.Substring("source:".Length) : (run.NodeId ?? run.Key);
            bool supervisor = sourceId.StartsWith(SupervisorDialogueContent.SourcePrefix, StringComparison.Ordinal);
            if (supervisor)
            {
                if (_resumeSupervisor != null && _resumeSupervisor.sourceId != sourceId) return;
                _resumeSupervisor = null;
                if (run.Runtime.GetView().Cue?.Kind == PresentationKind.Ending) _supervisor?.ShowEndingAtDoorway();
                // Opening the door can finish before the actor's clearance slide. Show the first
                // line only after both the doorway camera and the supervisor have come to rest.
                while (IsCurrent(run) && (!CanInteract(run)
                    || (_supervisor != null && run.Runtime.GetView().DoorViewActive
                        && !_supervisor.IsReadyForDialogue))) await YarnTask.Yield();
                if (!IsCurrent(run)) return;
                string portraitId = portrait != null ? portrait.Substring("portrait:".Length) : "JD_01";
                screen.Audio?.StopSupervisorScan();
                if (_supervisor == null || !_supervisor.SetPortrait(portraitId))
                    throw new InvalidOperationException("Missing supervisor portrait: " + portraitId);
                screen.Audio?.PresentSupervisorPortrait(run.Runtime, portraitId);
                _supervisorCursor = new SupervisorDialogueCheckpoint {
                    revision = _supervisorContent!.revision, sourceId = sourceId,
                    day = run.Runtime.GetView().Game.Day, cue = run.Runtime.GetView().Cue!.Kind };
            }
            else if (run.Runtime.IsDialogueLineComplete(sourceId)) return;
            var advanced = false;
            string text = run.Runtime.ResolveDialogueText(sourceId, line.TextWithoutCharacterName.Text);
            screen.PresentDialogueLine(line.CharacterName ?? "终端", text, sourceId,
                portrait != null ? portrait.Substring("portrait:".Length) : "", run.Runtime.IsNarration(sourceId), () =>
                {
                    if (!CanInteract(run)) return;
                    if (supervisor) screen.Audio?.StopSupervisorScan();
                    if (!supervisor) run.Runtime.CompleteDialogueLine(sourceId);
                    advanced = true;
                });
            screen.SetDialogueInteractable(CanInteract(run));
            while (!advanced && !token.IsNextContentRequested && IsCurrent(run)) await YarnTask.Yield();
            if (supervisor && IsCurrent(run)) screen.Audio?.StopSupervisorScan();
            if (IsCurrent(run)) screen.HideDialogueActions();
        }

        internal async YarnTask<DialogueOption?> PresentOptionsAsync(DialogueOption[] options, LineCancellationToken token)
        {
            var run = _run;
            var screen = _screen;
            if (run?.Dialogue == null || screen == null) return null;
            while (IsCurrent(run) && !CanInteract(run)) await YarnTask.Yield();
            if (!IsCurrent(run)) return null;
            var choices = new Dictionary<string, DialogueOption>(StringComparer.Ordinal);
            var labels = new List<ResponseContent>();
            foreach (var option in options.Where(o => o.IsAvailable))
            {
                var metadata = option.Line.Metadata.FirstOrDefault(m => m.StartsWith(PrototypeYarnSource.ResponseMetadataPrefix, StringComparison.Ordinal));
                if (metadata == null) continue;
                var code = metadata.Substring(PrototypeYarnSource.ResponseMetadataPrefix.Length);
                var response = run.Dialogue.Responses.SingleOrDefault(r => PrototypeYarnSource.EncodeIdentifier(r.Id) == code);
                if (response == null) continue;
                choices.Add(response.Id, option);
                labels.Add(new ResponseContent(response.Id, run.Runtime.ResolveDialogueText(response.Id, option.Line.TextWithoutCharacterName.Text)));
            }
            if (labels.Count == 0)
            {
                Debug.LogError("Yarn supplied no options matching the current core response identifiers.", this);
                return null;
            }
            DialogueOption? selected = null;
            screen.ShowResponseButtons(labels, responseId =>
            {
                if (selected == null && CanInteract(run) && run.Runtime.GetView().Game.AvailableResponseIds.Contains(responseId)
                    && choices.TryGetValue(responseId, out var option)) selected = option;
            });
            screen.SetDialogueInteractable(CanInteract(run));
            while (selected == null && !token.IsNextContentRequested && IsCurrent(run)) await YarnTask.Yield();
            // A choice already clicked waits through a pause/history overlay before its Yarn command runs.
            while (selected != null && IsCurrent(run) && !CanInteract(run)) await YarnTask.Yield();
            if (IsCurrent(run)) screen.HideDialogueActions();
            return IsCurrent(run) && !token.IsNextContentRequested ? selected : null;
        }

        private bool IsCurrent(NodeRun run) => _run == run && run.Epoch == _epoch && _runtime == run.Runtime;

        private bool CanInteract(NodeRun run)
        {
            if (!IsCurrent(run)) return false;
            var view = run.Runtime.GetView();
            return !view.Paused && view.Inspection == null && _screen != null && !_screen.HistoryOpen
                && !_screen.TransitionLocked && Matches(run, view);
        }

        private static bool Matches(NodeRun run, RuntimeView view) => run.CueToken != null
            ? view.Cue?.Token == run.CueToken
            : view.Cue == null && view.TerminalOpen &&
                view.Game.CurrentEncounterId == run.EncounterId && view.Game.CurrentNodeId == run.NodeId;

        private void InvalidateRun()
        {
            _epoch++;
            _run = null;
            _completedKey = "";
            _supervisorCursor = null;
            _resumeSupervisor = null;
            if (_screen != null) _screen.HideDialogueActions();
            if (_runner != null && !_stopping && (_runner.IsDialogueRunning || !_nodeTask.IsCompleted()
                || (_presenter != null && _presenter.HasPendingPresentations))) StopRunnerAsync().Forget();
        }

        private async YarnTask StopRunnerAsync()
        {
            var runner = _runner;
            if (runner == null) return;
            var nodeTask = _nodeTask;
            _stopping = true;
            try
            {
                // Stop waits OnDialogueCompleteAsync, whose barrier now drains the presenters while
                // the old runner cancellation source remains cancelled. Stop must not be called idle.
                if (runner.IsDialogueRunning) await runner.Stop();
                await nodeTask;
                if (_presenter != null) await _presenter.DrainPresentationsAsync();
                await runner.DialogueTask;
                await YarnTask.Yield();
            }
            finally { _stopping = false; _pumpRequested = true; }
        }

        private void OnDestroy()
        {
            if (_runtime != null) _runtime.Changed -= OnRuntimeChanged;
            // The child DialogueRunner cancels itself on destruction. Never rebuild UI during scene teardown.
            _epoch++;
            _run = null;
            _runtime = null;
            _screen = null;
            _pumpRequested = false;
        }

        private sealed class NodeRun
        {
            public int Epoch { get; }
            public GameRuntime Runtime { get; }
            public string Key { get; }
            public string? EncounterId { get; }
            public string? NodeId { get; }
            public string? CueToken { get; }
            public DialogueContent? Dialogue { get; }
            public bool ResponseSubmitted { get; set; }
            public NodeRun(int epoch, GameRuntime runtime, string key, string? encounterId, string? nodeId,
                string? cueToken, DialogueContent? dialogue)
            { Epoch = epoch; Runtime = runtime; Key = key; EncounterId = encounterId; NodeId = nodeId; CueToken = cueToken; Dialogue = dialogue; }
        }
    }
}
