using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>uGUI view and input adapter. All displayed rule data comes from RuntimeView.</summary>
    public sealed class MiniGameScreen : MonoBehaviour
    {
        private static readonly Color Ink = TerminalView.Ink;
        private static readonly Color Dark = TerminalView.Desk;
        private static readonly Color Accent = TerminalView.Bright;
        private GameRuntime runtime;
        private RuntimeView current;
        private Font font;
        private RectTransform canvasRoot;
        private GameObject terminal;
        private CanvasGroup terminalGroup;
        private CanvasGroup promptGroup;
        private Image[] promptFrameEdges = Array.Empty<Image>();
        private CanvasGroup conversationGroup;
        private TerminalTransitionView handoff;
        private Text reticle;
        private float promptTarget;
        private bool focusAllowed;
        private float terminalReveal = 1f;
        private TerminalView terminalView;
        private GameObject conversation;
        private GameObject inspection;
        private PauseMenuView pauseMenu;
        private GameObject inspectionDrawing;
        private Image fade;
        private Image glitch;
        private Text header;
        private Text speaker;
        private Text body;
        private Text worldPrompt;
        private Text roomCaption;
        private float roomCaptionRemaining;
        private PropNarrationView propNarration;
        private Text notice;
        private Text inspectionTitle;
        private Text inspectionSummary;
        private Text worldHelp;
        private Button continueButton;
        private Button closeInspection;
        private RectTransform choiceRoot;
        private RectTransform conversationRect;
        private readonly List<Button> choices = new List<Button>();
        private bool yarnBound;
        private bool dialogueInteractable = true;
        private bool transitionLocked;
        private string fallbackKey;
        private float notificationEnd;
        private int pauseClosedFrame = -1;
        private int worldLinePresentedFrame = -1, worldDialogueInputFrame = -1;
        private readonly List<RaycastResult> worldPointerHits = new List<RaycastResult>();
        private int selectedResponse;
        private bool skipBootRequested;
        private bool openingClickRequested;
        public bool DayZeroOpeningWaiting { get; private set; }
        public TerminalPowerPhase PowerPhase { get; private set; } = TerminalPowerPhase.Running;
        public string TerminalConsoleText => handoff?.ConsoleText ?? "";
        public TerminalDisplay TerminalDisplay => terminalView?.Display;
        public TerminalSignalController SignalTuning { get; private set; }
        public bool SignalTuningBlocksInput => SignalTuning != null && SignalTuning.BlocksGameplayInput;
        public RoomLightingTuningPanel LightingTuning { get; private set; }
        public bool PresentationToolsBlockInput => SignalTuningBlocksInput || LightingTuning != null && LightingTuning.BlocksInput;
        internal bool TerminalPointerAllowed => current != null && current.TerminalOpen && !current.Paused
            && !PresentationToolsBlockInput && current.Inspection == null && !transitionLocked && PowerPhase == TerminalPowerPhase.Running && terminalReveal >= .999f;
        private readonly DialogueTextPlayback linePlayback = new DialogueTextPlayback();
        [SerializeField] private float dialogueCharactersPerSecond = 30;
        public float DialogueCharactersPerSecond { get => dialogueCharactersPerSecond; set => dialogueCharactersPerSecond = Mathf.Max(0, value); }
        public bool LineIsRevealed => !linePlayback.Active || linePlayback.PageRevealed;
        public int CurrentLinePageCount => linePlayback.PageCount;
        public event Action SaveRequested;
        public event Action LoadRequested;
        public event Action RestartRequested;
        public Canvas Canvas { get; private set; }
        public float TerminalReveal => terminalReveal;
        public bool TransitionLocked => transitionLocked;
        public bool HistoryOpen => terminalView != null && terminalView.HistoryOpen;
        public bool PauseManualVisible => pauseMenu != null && pauseMenu.ManualVisible;
        public bool PauseSettingsVisible => pauseMenu != null && pauseMenu.SettingsVisible;
        public bool PauseSubmenuVisible => PauseManualVisible || PauseSettingsVisible;
        public bool TryClosePauseSettings() => pauseMenu != null && pauseMenu.TryCloseSettings();
        public bool TryClosePauseManual() => pauseMenu != null && pauseMenu.TryCloseManual();
        public string CurrentRoomCaption => propNarration != null && propNarration.IsOpen ? propNarration.CurrentText
            : roomCaption != null ? roomCaption.text : string.Empty;
        public bool RoomCaptionVisible => propNarration != null && propNarration.Visible
            || roomCaption != null && roomCaption.gameObject.activeSelf && roomCaptionRemaining > 0;
        public PropNarrationView PropNarration => propNarration;
        public bool PropNarrationBlocksWorld => propNarration != null && propNarration.BlocksWorldInput;
        public MiniGameAudioDirector Audio { get; private set; }

        public void BindAudio(MiniGameAudioDirector value) => Audio = value;
        public void BindSelfMonitor(Texture texture) { EnsureBuilt(); terminalView.SetSelfMonitor(texture); }
        public void BindLighting(GameRuntime game, RoomLightingController lighting)
        {
            EnsureBuilt();
            if (LightingTuning == null)
            {
                LightingTuning = gameObject.AddComponent<RoomLightingTuningPanel>();
                LightingTuning.Initialize(canvasRoot, font);
                LightingTuning.VisibilityChanged += RefreshPresentationToolVisibility;
            }
            LightingTuning.Bind(game, lighting, SignalTuning);
        }
        private void RefreshPresentationToolVisibility()
        {
            if (pauseMenu == null) return;
            bool lightingOpen = LightingTuning != null && LightingTuning.IsPanelOpen;
            pauseMenu.SetVisible(current != null && current.Paused && !lightingOpen);
            // Authoring pauses the session, while its room lighting must remain a live view.
            // Skip the ordinary pause fade as well, cancelling a pending frozen-frame capture.
            if (lightingOpen) pauseMenu.gameObject.SetActive(false);
        }

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= Render;
            EnsureBuilt();
            handoff.ResetTyping();
            runtime = game;
            ClearRoomCaption();
            fallbackKey = null;
            worldLinePresentedFrame = worldDialogueInputFrame = -1;
            transitionLocked = false;
            EndDayZeroOpening();
            SetTerminalReveal(1);
            SetTerminalHandoff(0, false, false);
            HideDialogueActions();
            var boundView = runtime.GetView();
            terminalView.ResetNavigation(boundView);
            runtime.Changed += Render;
            Render(boundView);
        }

        public void UseYarnPresentation() => yarnBound = true;
        public void SetWorldPrompt(string text)
        { if (worldPrompt != null) { worldPrompt.text = text; promptTarget = string.IsNullOrEmpty(text) ? 0 : 1; } }
        public void SetWorldFocus(string label, bool allowed, string shortcut = "E")
        {
            focusAllowed = allowed;
            SetWorldPrompt(string.IsNullOrEmpty(label) ? string.Empty : allowed ? "[ " + shortcut + " ]   " + label : label + "  ·  暂不可用");
        }
        public void ShowRoomCaption(string text, float seconds = 4)
        {
            EnsureBuilt();
            roomCaption.text = text;
            roomCaptionRemaining = Mathf.Max(0, seconds);
            roomCaption.gameObject.SetActive(roomCaptionRemaining > 0 && !string.IsNullOrEmpty(text));
        }
        public void ClearRoomCaption()
        {
            propNarration?.CloseImmediately();
            roomCaptionRemaining = 0;
            if (roomCaption != null) { roomCaption.text = string.Empty; roomCaption.gameObject.SetActive(false); }
        }
        public bool ShowPropNarration(ReadableProp prop, Camera camera)
        {
            EnsureBuilt();
            if (current == null || current.Paused || PresentationToolsBlockInput || current.Input != InputContext.World || transitionLocked
                || current.DreamTraversal || PropNarrationBlocksWorld) return false;
            if (propNarration == null)
            {
                var root = new GameObject("Prop narration presentation", typeof(PropNarrationView));
                root.transform.SetParent(transform, false);
                propNarration = root.GetComponent<PropNarrationView>();
            }
            if (!propNarration.Show(prop, camera, font, current.Game.Day)) return false;
            roomCaptionRemaining = 0; roomCaption.gameObject.SetActive(false);
            SetWorldPrompt(string.Empty);
            return true;
        }
        public bool AdvancePropNarration()
        {
            if (current == null || current.Paused || PresentationToolsBlockInput || pauseClosedFrame == Time.frameCount || transitionLocked) return false;
            return propNarration != null && propNarration.Advance();
        }
        public bool TryClosePropNarration()
        {
            if (current == null || current.Paused || PresentationToolsBlockInput || propNarration == null || !propNarration.BlocksWorldInput) return false;
            // Escape belongs to reading until its closing frame has finished, including rejected repeat presses.
            propNarration.Dismiss();
            return true;
        }
        public void SetTerminalReveal(float amount)
        {
            EnsureBuilt();
            terminalReveal = Mathf.Clamp01(amount);
            terminalGroup.alpha = terminalReveal;
            terminalGroup.interactable = terminalReveal >= .999f;
            terminalGroup.blocksRaycasts = terminalReveal >= .999f;
            terminal.transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1f, terminalReveal);
        }
        public void SetTerminalHandoff(float progress, bool exiting, bool active)
        { EnsureBuilt(); handoff.SetFrame(progress, exiting, active); }

        public void SetTerminalPowerFrame(TerminalPowerFrame frame)
        {
            EnsureBuilt(); PowerPhase = frame.Phase;
            int newCharacters = handoff.SetPowerFrame(frame);
            if (isActiveAndEnabled && current != null && current.TerminalOpen && !current.Paused)
                for (int i = 0; i < newCharacters; i++) Audio?.PlayTerminalTyping();
            terminalView.Display.SetCollapse(frame.Phase == TerminalPowerPhase.Collapse ? frame.Collapse : frame.Phase == TerminalPowerPhase.Off ? 1 : -1);
        }
        public void BeginTerminalBoot() { skipBootRequested = false; SetTerminalPowerFrame(TerminalPowerTimeline.Boot(0)); }
        public bool ConsumeBootSkip() { bool value = skipBootRequested; skipBootRequested = false; return value; }
        public void BeginDayZeroOpening()
        {
            openingClickRequested = false; DayZeroOpeningWaiting = true;
            terminalView.SetOpeningLogo(true);
            SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
            SetTerminalReveal(1); SetTransitionLocked(true);
            terminalView.Display.SetOpeningProgress(0);
        }
        public bool RequestDayZeroOpening()
        {
            if (!DayZeroOpeningWaiting || current == null || current.Paused || PresentationToolsBlockInput || pauseClosedFrame == Time.frameCount) return false;
            openingClickRequested = true; DayZeroOpeningWaiting = false; return true;
        }
        public bool ConsumeOpeningClick() { bool clicked = openingClickRequested; openingClickRequested = false; return clicked; }
        public void EndDayZeroOpening()
        {
            DayZeroOpeningWaiting = false; openingClickRequested = false;
            terminalView?.SetOpeningLogo(false);
            terminalView?.Display.SetOpeningProgress(1);
        }
        public void Notify(string text, bool error)
        {
            EnsureBuilt(); notice.text = text;
            notice.color = error ? TerminalView.Bright : Ink;
            notificationEnd = Time.unscaledTime + 5f;
            pauseMenu?.SetStatus(text, error);
        }
        public void ClearNotification()
        {
            if (notice != null) notice.text = string.Empty;
            pauseMenu?.SetStatus(string.Empty);
        }
        public void SetFade(float amount) { EnsureBuilt(); fade.color = new Color(0f, 0f, 0f, Mathf.Clamp01(amount)); }
        public void SetGlitch(float amount)
        {
            EnsureBuilt();
            if (current != null && current.TerminalOpen && current.UsesTerminalDirectory)
            {
                glitch.gameObject.SetActive(false);
                terminalView.SetSignalDistortion(current.Cue?.Kind == PresentationKind.ExitTerminal && !current.LeavingTerminal ? amount : 0);
                return;
            }
            terminalView.SetSignalDistortion(0);
            glitch.gameObject.SetActive(amount > 0f);
            glitch.color = new Color(.35f, .65f, .45f, Mathf.Clamp01(amount) * .32f);
            glitch.rectTransform.anchoredPosition = amount > 0f ? new Vector2(Mathf.Sin(Time.unscaledTime * 73f) * 22f, 0f) : Vector2.zero;
        }
        public void SetTransitionLocked(bool locked)
        { transitionLocked = locked; if (locked) ClearRoomCaption(); ApplyInputGate(); }

        public void SetDialogueText(string displaySpeaker, string text)
        {
            EnsureBuilt(); speaker.text = IsSupervisorCue(current) ? "监督员" : displaySpeaker;
            body.text = WorldCueText(text);
            if (current == null || !current.UsesTerminalDirectory) terminalView.SetLine(displaySpeaker, text, null);
        }

        public void SetDialogueLine(string displaySpeaker, string text, string sourceNodeId, string portraitId)
        {
            SetDialogueText(displaySpeaker, text);
            if (IsSupervisorCue(current)) return;
            terminalView.SetLine(displaySpeaker, text, portraitId, sourceNodeId);
            runtime?.RecordDialogueLine(displaySpeaker, text, sourceNodeId, portraitId);
        }

        public void PresentDialogueLine(string displaySpeaker, string text, string sourceNodeId, string portraitId,
            bool narration, Action complete)
        {
            ShowCueText(text, complete);
            if (narration && current != null && current.TerminalOpen)
            {
                terminalView.SetNarration(text);
                terminalView.SetPortrait(portraitId, sourceNodeId);
                runtime?.RecordDialogueLine(displaySpeaker, text, sourceNodeId, portraitId, true);
            }
            else
            {
                terminalView.ClearNarration();
                SetDialogueLine(displaySpeaker, text, sourceNodeId, portraitId);
            }
            if (runtime == null || !runtime.IsCampaign) return;
            var target = current != null && current.TerminalOpen ? narration ? terminalView.NarrationBody : terminalView.Body : body;
            linePlayback.Begin(target, current != null && !current.TerminalOpen ? WorldCueText(text) : text, dialogueCharactersPerSecond, complete,
                paginate: current == null || !current.TerminalOpen || narration);
            terminalView.SetPageIndicator(linePlayback.PageNumber, linePlayback.PageCount);
            var next = ActiveContinue;
            next.onClick.RemoveAllListeners();
            next.onClick.AddListener(() =>
            {
                if (!TryConsumeDialogueAdvance()) return;
                linePlayback.Advance();
                terminalView.SetPageIndicator(linePlayback.PageNumber, linePlayback.PageCount);
            });
        }

        public void ShowResponseButtons(IReadOnlyList<ResponseContent> responses, Action<string> selected)
        {
            EnsureBuilt();
            HideDialogueActions();
            terminalView.ClearNarration();
            foreach (var response in responses.Take(4))
            {
                var id = response.Id;
                var button = current != null && current.TerminalOpen
                    ? terminalView.AddResponse(id, response.Text, choices.Count, () => { if (CanAdvanceDialogue()) selected(id); })
                    : MakeButton(choiceRoot, "Reply-" + id, response.Text, () => { if (CanAdvanceDialogue()) selected(id); });
                if (current == null || !current.TerminalOpen)
                {
                    var element = button.gameObject.AddComponent<LayoutElement>();
                    element.preferredHeight = 64f;
                    element.minHeight = 54f;
                }
                choices.Add(button);
            }
            selectedResponse = current != null && current.TerminalOpen && choices.Count > 0 ? 0 : -1;
            terminalView.HighlightResponse(selectedResponse);
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void ShowCueText(string text, Action complete)
        {
            EnsureBuilt(); HideDialogueActions();
            if (IsSupervisorCue(current)) worldLinePresentedFrame = Time.frameCount;
            body.text = WorldCueText(text);
            if (current == null || !current.UsesTerminalDirectory) terminalView.Body.text = text;
            var next = ActiveContinue;
            next.gameObject.SetActive(complete != null);
            next.onClick.RemoveAllListeners();
            if (complete != null) next.onClick.AddListener(() => { if (TryConsumeDialogueAdvance()) complete(); });
            next.GetComponentInChildren<Text>().text = current?.Cue?.Kind == PresentationKind.Reaction && !current.UsesTerminalDirectory
                ? "完成处置并返回房间 [空格]" : current != null && current.TerminalOpen ? "下一句  ↵" : "继续 [空格]";
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void HideDialogueActions()
        {
            EnsureBuilt();
            linePlayback.Cancel();
            terminalView.SetPageIndicator(0, 0);
            terminalView.ClearNarration();
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveAllListeners();
            terminalView.Continue.gameObject.SetActive(false);
            terminalView.Continue.onClick.RemoveAllListeners();
            foreach (var choice in choices) if (choice != null) { choice.gameObject.SetActive(false); Destroy(choice.gameObject); }
            choices.Clear();
        }

        public void SetDialogueInteractable(bool enabled)
        { dialogueInteractable = enabled; ApplyInputGate(); }

        public bool TryCloseHistory()
        {
            if (terminalView == null || !terminalView.CloseHistory()) return false;
            pauseClosedFrame = Time.frameCount;
            return true;
        }

        private void Update()
        {
            if (roomCaptionRemaining > 0 && current != null && !current.Paused)
            {
                roomCaptionRemaining -= Time.unscaledDeltaTime;
                if (roomCaptionRemaining <= 0) ClearRoomCaption();
            }
            if (promptGroup != null)
            {
                bool world = current != null && current.Input == InputContext.World && !PropNarrationBlocksWorld;
                bool showPrompt = world || (current != null && current.Input == InputContext.DoorView);
                worldHelp.gameObject.SetActive(false);
                promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, showPrompt ? promptTarget : 0, Time.unscaledDeltaTime * 6);
                worldPrompt.color = focusAllowed ? Ink : TerminalView.Dim;
                foreach (var edge in promptFrameEdges) edge.color = worldPrompt.color;
                reticle.gameObject.SetActive(world);
                if (conversation.activeSelf) conversationGroup.alpha = Mathf.MoveTowards(conversationGroup.alpha, 1, Time.unscaledDeltaTime * 6);
                else conversationGroup.alpha = 0;
            }
            if (notice != null && notice.text.Length > 0 && Time.unscaledTime > notificationEnd) ClearNotification();
            if (PresentationToolsBlockInput) return;
            if (DayZeroOpeningWaiting && Input.GetMouseButtonDown(0)) { RequestDayZeroOpening(); return; }
            if (current != null && !current.Paused && pauseClosedFrame != Time.frameCount && current.TerminalOpen && PowerPhase == TerminalPowerPhase.Boot)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) HandleTerminalKey(KeyCode.Return);
                return;
            }
            if (current == null || current.Paused || pauseClosedFrame == Time.frameCount || current.Inspection != null || transitionLocked) return;
            if (PropNarrationBlocksWorld)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) AdvancePropNarration();
                return;
            }
            if (!HistoryOpen) linePlayback.Tick(Time.unscaledDeltaTime);
            if (IsSupervisorCue(current) && Input.GetMouseButtonDown(0) && !PointerIsOverWorldControl())
            {
                AdvanceWorldDialogue();
                return;
            }
            foreach (var key in DialogueKeys)
                if (Input.GetKeyDown(key) && HandleTerminalKey(key)) return;
        }

        private static readonly KeyCode[] DialogueKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Return,
            KeyCode.KeypadEnter, KeyCode.Space, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

        // The same input path is used by live keyboard input and PlayMode checks.
        // Escape stays in MiniGameBootstrap and closes the read-only record viewer before opening pause.
        public bool HandleTerminalKey(KeyCode key)
        {
            if (key == KeyCode.F2 || key == KeyCode.F3) return false;
            if (PresentationToolsBlockInput) return false;
            if (current != null && !current.Paused && pauseClosedFrame != Time.frameCount && current.TerminalOpen && PowerPhase == TerminalPowerPhase.Boot
                && (key == KeyCode.Return || key == KeyCode.KeypadEnter)) { skipBootRequested = true; return true; }
            if (current == null || current.Paused || pauseClosedFrame == Time.frameCount || current.Inspection != null || transitionLocked
                || PropNarrationBlocksWorld) return false;
            if (current.TerminalOpen)
            {
                if (terminalReveal < .999f || PowerPhase != TerminalPowerPhase.Running) return false;
                if (HistoryOpen)
                {
                    if (key == KeyCode.F3 || key == KeyCode.Escape) return TryCloseHistory();
                    if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
                        terminalView.ScrollSavedHistory(key == KeyCode.UpArrow ? 1 : -1);
                    return key != KeyCode.F2;
                }
                if (current.TerminalPage == TerminalPage.SelfReview) return terminalView.HandleSelfReviewKey(key);
                if (terminalView.HandleDirectoryKey(key) || terminalView.HandleArchiveKey(key)) return true;
                if (terminalView.HandleDispositionKey(key)) return true;
            }
            if (!CanAdvanceDialogue()) return false;
            bool confirm = key == KeyCode.Return || key == KeyCode.KeypadEnter;
            var next = ActiveContinue;
            if ((confirm || key == KeyCode.Space) && next != null && next.gameObject.activeInHierarchy && next.interactable)
                return IsSupervisorCue(current) ? AdvanceWorldDialogue() : MiniGameAudioButton.Activate(next);
            if (choices.Count > 0)
            {
                if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
                {
                    selectedResponse = key == KeyCode.UpArrow
                        ? (selectedResponse < 0 ? choices.Count - 1 : (selectedResponse + choices.Count - 1) % choices.Count)
                        : (selectedResponse + 1) % choices.Count;
                    terminalView.HighlightResponse(selectedResponse);
                    return true;
                }
                if (confirm && selectedResponse >= 0 && selectedResponse < choices.Count && choices[selectedResponse].interactable)
                    return MiniGameAudioButton.Activate(choices[selectedResponse]);
            }
            for (int index = 0; index < choices.Count; index++)
                if (key == KeyCode.Alpha1 + index && choices[index].interactable)
                    return MiniGameAudioButton.Activate(choices[index]);
            return false;
        }

        public bool AdvanceWorldDialogue()
        {
            if (!CanAcceptWorldDialogueInput() || continueButton == null
                || !continueButton.gameObject.activeInHierarchy || !continueButton.interactable) return false;
            return MiniGameAudioButton.Activate(continueButton);
        }

        private bool CanAcceptWorldDialogueInput() => IsSupervisorCue(current) && CanAdvanceDialogue()
            && !PropNarrationBlocksWorld && choices.Count == 0 && pauseClosedFrame != Time.frameCount
            && worldLinePresentedFrame != Time.frameCount && worldDialogueInputFrame != Time.frameCount;

        private bool TryConsumeDialogueAdvance()
        {
            if (!CanAdvanceDialogue()) return false;
            // Computer dialogue keeps its existing input path. Only the world supervisor shares this frame gate.
            if (!IsSupervisorCue(current)) return true;
            if (!CanAcceptWorldDialogueInput()) return false;
            worldDialogueInputFrame = Time.frameCount;
            return true;
        }

        private bool PointerIsOverWorldControl()
        {
            var events = EventSystem.current;
            if (events == null) return false;
            worldPointerHits.Clear();
            events.RaycastAll(new PointerEventData(events) { position = Input.mousePosition }, worldPointerHits);
            return worldPointerHits.Any(hit => hit.gameObject != null && hit.gameObject.GetComponentInParent<Selectable>() != null);
        }

        private void LateUpdate() { terminalView?.Fit(); terminalView?.AnimateSignal(Time.unscaledTime); }
        public void ReflowTerminal() => terminalView?.Fit();
        private Button ActiveContinue => current != null && current.TerminalOpen ? terminalView.Continue : continueButton;
        private bool CanAdvanceDialogue(bool checkSignalPanel = true) => current != null && dialogueInteractable && !transitionLocked && !current.Paused
            && (!checkSignalPanel || !PresentationToolsBlockInput) && current.Inspection == null
            && !HasDedicatedDreamPresentation(current)
            && current.Cue?.Kind != PresentationKind.DoorMotion
            && current.Cue?.Kind != PresentationKind.DoorViewReturn && current.Input != InputContext.DoorView
            && (!current.TerminalOpen || terminalReveal >= .999f)
            && (!current.TerminalOpen || (PowerPhase == TerminalPowerPhase.Running && terminalView.IsReview && !terminalView.HistoryOpen));

        private void OnDestroy()
        {
            propNarration?.CloseImmediately();
            if (LightingTuning != null) LightingTuning.VisibilityChanged -= RefreshPresentationToolVisibility;
            UnityEngine.Canvas.willRenderCanvases -= ReflowTerminal;
            Font.textureRebuilt -= RefreshFontTexture;
            if (runtime != null) runtime.Changed -= Render;
            if (font != null) Destroy(font);
        }
        private void OnDisable() => ClearRoomCaption();

        private void RefreshFontTexture(Font rebuilt)
        {
            if (rebuilt == font && font != null && font.material != null && font.material.mainTexture != null)
                font.material.mainTexture.filterMode = FilterMode.Point;
        }

        private void Render(RuntimeView view)
        {
            EnsureBuilt();
            // Submit can close the pause menu before this frame's dialogue input is polled.
            if (current != null && current.Paused && !view.Paused) pauseClosedFrame = Time.frameCount;
            if (current != null && current.Game.Phase != view.Game.Phase) ClearNotification();
            current = view;
            if (!view.Paused && view.Input != InputContext.World) ClearRoomCaption();
            propNarration?.SetPaused(view.Paused);
            header.text = "和平部 / 检定室 07     第 " + view.Game.Day + " 天     " + PhaseName(view.Game.Phase)
                + "     净化要求：" + view.Game.PurificationsToday + "/" + view.Game.PurificationRequirement;
            bool bedtime = BedtimeGuidance.IsSleepOnly(view);
            bool dedicatedDream = HasDedicatedDreamPresentation(view);
            header.gameObject.SetActive(!view.Paused && !view.TerminalOpen && !bedtime && !dedicatedDream);
            // Leaving the desk hides the fullscreen output, but a powered computer keeps rendering its UI.
            terminal.SetActive(view.TerminalOpen || view.TerminalPowered);
            terminalView.Display.SetOutputVisible(view.TerminalOpen);
            terminalView.Render(view, transitionLocked || view.Paused || view.Inspection != null);
            conversation.SetActive(!dedicatedDream && !view.TerminalOpen && view.Cue?.Kind != PresentationKind.DoorMotion
                && view.Cue?.Kind != PresentationKind.DoorViewReturn
                && (view.Cue != null || view.Dialogue != null || view.Game.Phase == GamePhase.Ending));
            bool supervisor = IsSupervisorCue(view) && conversation.activeSelf;
            SetRect(conversationRect, supervisor ? new Vector2(.20f, .08f) : new Vector2(.17f, .19f),
                supervisor ? new Vector2(.80f, .38f) : new Vector2(.83f, .74f));
            SetRect(body.rectTransform, supervisor ? new Vector2(.05f, .23f) : new Vector2(.03f, .57f),
                supervisor ? new Vector2(.95f, .83f) : new Vector2(.97f, .86f));
            worldHelp.gameObject.SetActive(false); worldHelp.text = string.Empty;
            inspection.SetActive(view.Inspection != null);
            if (view.Inspection != null)
            {
                inspectionTitle.text = view.Inspection.Title;
                inspectionSummary.text = view.Inspection.Summary + "\n\n这份观察仅是线索。处置由你决定。";
                BuildInspectionDrawing(view.Inspection.Kind);
            }
            pauseMenu.SetContext("第 " + view.Game.Day + " 天  /  " + PhaseName(view.Game.Phase));
            RefreshPresentationToolVisibility();
            if (dedicatedDream) HideDialogueActions();
            else if (!yarnBound || (view.Cue != null && (view.Cue.Kind == PresentationKind.EnterTerminal || view.Cue.Kind == PresentationKind.ExitTerminal || view.Cue.Kind == PresentationKind.Sleep || view.Cue.Kind == PresentationKind.DoorMotion || view.Cue.Kind == PresentationKind.DoorViewReturn)))
                RenderFallback(view);
            if (view.Game.Phase == GamePhase.Ending && view.Cue == null && !dedicatedDream)
            {
                SetDialogueText("检查期结束", EndingName(view.Game.Ending) + "\n\n按 ESC 可保存、恢复或开始新的检查期。");
                HideDialogueActions();
            }
            ApplyInputGate();
        }

        private static bool HasDedicatedDreamPresentation(RuntimeView view)
            => view.UsesDreamFlow && (view.Game.Phase == GamePhase.Dream || view.Game.Phase == GamePhase.SelfReview
                || view.Game.Ending == EndingType.DreamSea);

        private static bool IsSupervisorCue(RuntimeView view) => view != null && !view.TerminalOpen
            && (view.Cue?.Kind == PresentationKind.MorningInspection || view.Cue?.Kind == PresentationKind.Briefing
                || view.Cue?.Kind == PresentationKind.Ending && view.Game.Ending == EndingType.TakenAway);
        private string WorldCueText(string text) => IsSupervisorCue(current)
            ? (text ?? string.Empty).Replace("【开发占位】", string.Empty).Trim() : text;

        private void RenderFallback(RuntimeView view)
        {
            string key = view.Cue?.Token ?? view.Dialogue?.NodeId ?? "idle";
            if (fallbackKey == key) return;
            fallbackKey = key;
            if (view.Cue != null)
            {
                var token = view.Cue.Token;
                bool automatic = view.Cue.Kind == PresentationKind.EnterTerminal || view.Cue.Kind == PresentationKind.ExitTerminal || view.Cue.Kind == PresentationKind.Sleep || view.Cue.Kind == PresentationKind.DoorMotion || view.Cue.Kind == PresentationKind.DoorViewReturn;
                SetDialogueText("和平仪", view.Cue.Text);
                ShowCueText(view.Cue.Text, automatic ? null : (Action)(() => Check(runtime.CompleteCue(token))));
            }
            else if (view.Dialogue != null)
            {
                string encounter = view.Game.CurrentEncounterId;
                string node = view.Game.CurrentNodeId;
                SetDialogueText(view.Dialogue.Speaker, view.Dialogue.Text);
                if (view.UsesTerminalDirectory) terminalView.SetLine(view.Dialogue.Speaker, view.Dialogue.Text, null, view.Dialogue.NodeId);
                ShowResponseButtons(view.Dialogue.Responses, id => Check(runtime.SelectResponse(encounter, node, id)));
            }
            else { SetDialogueText("和平仪", "待机。请完成观察与会谈，再提交处置。"); HideDialogueActions(); }
        }

        private void ApplyInputGate()
        {
            if (current == null) return;
            // The transient tuning overlay gates events, not the cached dialogue availability.
            bool enabled = CanAdvanceDialogue(false);
            if (continueButton != null) continueButton.interactable = enabled;
            if (terminalView != null) terminalView.Continue.interactable = enabled;
            foreach (var button in choices) if (button != null) button.interactable = enabled;
            terminalView?.Render(current, transitionLocked || current.Paused || current.Inspection != null);
            if (closeInspection != null) closeInspection.interactable = !current.Paused;
        }

        private void Check(RuntimeResult result)
        { if (!result.Accepted) Notify("请先完成当前的对话或演出。", true); else ClearNotification(); }

        private void EnsureBuilt()
        {
            if (canvasRoot != null) return;
            font = GameUiTypography.CreateFont();
            Font.textureRebuilt += RefreshFontTexture;
            RefreshFontTexture(font);
            var go = new GameObject("Prototype UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            Canvas = go.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            canvasRoot = go.GetComponent<RectTransform>();
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SignalTuning = gameObject.AddComponent<TerminalSignalController>();
            SignalTuning.Initialize(canvasRoot, font, () => current != null && current.Paused);

            header = MakeText(canvasRoot, "ShiftHeader", "", 22, TextAnchor.MiddleLeft, new Vector2(.03f, .925f), new Vector2(.97f, .98f));
            worldHelp = MakeText(canvasRoot, "WorldInstructions", "", 19, TextAnchor.LowerLeft, new Vector2(.03f, .04f), new Vector2(.57f, .16f));
            var promptFrame = MakePanel(canvasRoot, "InteractionFrame", new Color(.025f, .018f, .008f, .55f), new Vector2(.25f, .25f), new Vector2(.75f, .33f));
            promptFrameEdges = AddNarrativeFrame(promptFrame);
            worldPrompt = MakeText(promptFrame, "InteractionPrompt", "", 26, TextAnchor.MiddleCenter, new Vector2(.04f, .12f), new Vector2(.96f, .88f));
            roomCaption = MakeText(canvasRoot, "RoomPropCaption", "", 26, TextAnchor.MiddleCenter, new Vector2(.12f, .12f), new Vector2(.88f, .21f));
            roomCaption.raycastTarget = false;
            roomCaption.gameObject.SetActive(false);
            promptGroup = promptFrame.gameObject.AddComponent<CanvasGroup>(); promptGroup.blocksRaycasts = false; promptGroup.alpha = 0;
            reticle = MakeText(canvasRoot, "Reticle", "·", 28, TextAnchor.MiddleCenter, new Vector2(.49f, .485f), new Vector2(.51f, .515f));
            terminal = MakePanel(canvasRoot, "Terminal", Color.black, Vector2.zero, Vector2.one).gameObject;
            terminal.GetComponent<Image>().raycastTarget = false;
            terminalGroup = terminal.AddComponent<CanvasGroup>();
            terminalView = new TerminalView(terminal.GetComponent<RectTransform>(), font,
                id => Check(runtime.EnterSelectedPatient(id)), () => Check(runtime.ReturnToDirectory()),
                () => Check(runtime.LeaveTerminal()), disposition =>
                {
                    var view = runtime.GetView();
                    var result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition);
                    Check(result);
                }, kind => Check(runtime.Inspect(kind)), () => Check(runtime.ConfirmSelfPurification()), SignalTuning);
            terminalView.ConfirmRequested = () => HandleTerminalKey(KeyCode.Return);
            UnityEngine.Canvas.willRenderCanvases += ReflowTerminal;
            conversationRect = MakePanel(canvasRoot, "Conversation", new Color(.025f, .018f, .008f, .55f), new Vector2(.36f, .115f), new Vector2(.955f, .65f));
            AddNarrativeFrame(conversationRect);
            conversation = conversationRect.gameObject;
            conversationGroup = conversation.AddComponent<CanvasGroup>();
            speaker = MakeText(conversationRect, "Speaker", "", 26, TextAnchor.MiddleLeft, new Vector2(.03f, .875f), new Vector2(.97f, .98f));
            body = MakeText(conversationRect, "DialogueBody", "", 27, TextAnchor.UpperLeft, new Vector2(.03f, .57f), new Vector2(.97f, .86f));
            choiceRoot = MakePanel(conversationRect, "Choices", Color.clear, new Vector2(.03f, .035f), new Vector2(.97f, .53f));
            choiceRoot.GetComponent<Image>().raycastTarget = false;
            var layout = choiceRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 9f;
            layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            continueButton = MakeButton(conversationRect, "Continue", "继续 [空格]", () => { });
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(.69f, .07f), new Vector2(.97f, .22f));

            inspection = MakePanel(canvasRoot, "InspectionModal", new Color(.043f, .031f, .012f, .995f), new Vector2(.14f, .12f), new Vector2(.86f, .86f)).gameObject;
            var ir = inspection.GetComponent<RectTransform>();
            inspectionTitle = MakeText(ir, "InspectionTitle", "", 32, TextAnchor.MiddleLeft, new Vector2(.04f, .84f), new Vector2(.95f, .96f));
            inspectionDrawing = MakePanel(ir, "ObservationIllustration", new Color(.12f, .085f, .028f), new Vector2(.05f, .19f), new Vector2(.43f, .82f)).gameObject;
            inspectionSummary = MakeText(ir, "InspectionDescription", "", 29, TextAnchor.UpperLeft, new Vector2(.48f, .20f), new Vector2(.95f, .81f));
            closeInspection = MakeButton(ir, "CloseInspection", "记录并返回 [ESC]", () => runtime.CloseInspection());
            SetRect(closeInspection.GetComponent<RectTransform>(), new Vector2(.58f, .04f), new Vector2(.95f, .15f));

            var labels = new[] { "继续检查", "保存进度", "读取存档", "开始新检查期", "游戏设置", "操作手册", "退出游戏" };
            Action[] actions = { () => runtime.SetPaused(false), () => SaveRequested?.Invoke(), () => LoadRequested?.Invoke(),
                () => RestartRequested?.Invoke(), () => pauseMenu.ShowSettings(), () => pauseMenu.ShowManual(), () => Application.Quit() };
            pauseMenu = PauseMenuView.Create(canvasRoot, font, "PauseModal", labels, new[] { "ESC", "F5", "F9", "", "", "", "" }, actions);
            pauseMenu.SetHint("保存当前进度，下次可从这里继续检查。");
            fade = MakePanel(canvasRoot, "PresentationFade", Color.clear, Vector2.zero, Vector2.one).GetComponent<Image>();
            fade.raycastTarget = false;
            // Several narrow stripes provide a generated signal-loss effect, without a recovered shader.
            glitch = MakePanel(canvasRoot, "GeneratedSignalLoss", Color.clear, Vector2.zero, Vector2.one).GetComponent<Image>();
            glitch.raycastTarget = false;
            for (int index = 0; index < 27; index++)
            {
                var stripe = MakePanel(glitch.rectTransform, "Scanline" + index, new Color(.12f, .07f, .015f, .4f), new Vector2(0, index / 27f), new Vector2(1, index / 27f + .005f));
                stripe.GetComponent<Image>().raycastTarget = false;
            }
            glitch.gameObject.SetActive(false);
            handoff = new TerminalTransitionView(terminalView.Logical, terminalView.Typeface);
            notice = MakeText(canvasRoot, "Notification", "", 24, TextAnchor.MiddleCenter, new Vector2(.08f, .865f), new Vector2(.92f, .925f));

        }

        private void BuildInspectionDrawing(InspectionKind kind)
        {
            var root = inspectionDrawing.GetComponent<RectTransform>();
            for (int index = root.childCount - 1; index >= 0; index--) { root.GetChild(index).gameObject.SetActive(false); Destroy(root.GetChild(index).gameObject); }
            var pale = Ink;
            if (kind == InspectionKind.Eyes)
            {
                for (int index = 0; index < 2; index++)
                {
                    float offset = .10f + index * .45f;
                    MakePanel(root, "Eye", pale, new Vector2(offset, .41f), new Vector2(offset + .35f, .58f));
                    MakePanel(root, "Pupil", Dark, new Vector2(offset + .13f, .41f), new Vector2(offset + .21f, .58f));
                }
            }
            else if (kind == InspectionKind.Hands)
            {
                for (int side = 0; side < 2; side++)
                {
                    float offset = .12f + side * .43f;
                    MakePanel(root, "Palm", pale, new Vector2(offset, .22f), new Vector2(offset + .26f, .5f));
                    for (int finger = 0; finger < 4; finger++) MakePanel(root, "Finger", pale,
                        new Vector2(offset + finger * .066f, .5f), new Vector2(offset + finger * .066f + .05f, .71f + (finger % 2) * .08f));
                }
            }
            else if (kind == InspectionKind.Teeth)
            {
                MakePanel(root, "Mouth", new Color(.2f, .1f, .08f), new Vector2(.12f, .35f), new Vector2(.88f, .65f));
                for (int row = 0; row < 2; row++)
                for (int tooth = 0; tooth < 8; tooth++) MakePanel(root, "Tooth", pale, new Vector2(.14f + tooth * .09f, .38f + row * .14f), new Vector2(.21f + tooth * .09f, .50f + row * .14f));
            }
            else if (kind == InspectionKind.Photo)
            {
                var photo = MakePanel(root, "ArchivePhoto", pale, new Vector2(.16f, .13f), new Vector2(.84f, .88f));
                MakePanel(photo, "PhotoHead", Dark, new Vector2(.33f, .41f), new Vector2(.66f, .75f));
                MakePanel(photo, "PhotoShoulders", Dark, new Vector2(.17f, .1f), new Vector2(.83f, .43f));
                MakeText(photo, "DateStamp", "档案副本", 20, TextAnchor.MiddleCenter, new Vector2(.04f, .01f), new Vector2(.96f, .1f));
            }
            else
            {
                MakeText(root, "Paper", kind == InspectionKind.DailySigns ? "当日通知\n\n现象记录\n————\n制度措辞\n————\n观察待核" : "会谈档案\n\n姓名与编号\n————\n既往处置\n————\n记录摘录", 27, TextAnchor.MiddleCenter, new Vector2(.06f, .08f), new Vector2(.94f, .9f));
            }
            MakeText(root, "VisualDisclaimer", "原创示意 / 非诊断结果", 17, TextAnchor.MiddleCenter, new Vector2(.02f, .01f), new Vector2(.98f, .1f));
        }

        private RectTransform MakePanel(Transform parent, string name, Color color, Vector2 minimum, Vector2 maximum)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); SetRect(rect, minimum, maximum);
            go.GetComponent<Image>().color = color;
            return rect;
        }
        private static Image[] AddNarrativeFrame(RectTransform parent)
        {
            var edges = new Image[4];
            for (int index = 0; index < 4; index++)
            {
                var edge = new GameObject("NarrativeFrame" + index, typeof(RectTransform), typeof(Image));
                edge.transform.SetParent(parent, false);
                var rect = (RectTransform)edge.transform;
                bool horizontal = index < 2;
                rect.anchorMin = horizontal ? new Vector2(0, index) : new Vector2(index - 2, 0);
                rect.anchorMax = horizontal ? new Vector2(1, index) : new Vector2(index - 2, 1);
                rect.sizeDelta = horizontal ? new Vector2(0, 2) : new Vector2(2, 0);
                rect.anchoredPosition = Vector2.zero;
                var image = edge.GetComponent<Image>(); image.color = Ink; image.raycastTarget = false;
                edges[index] = image;
            }
            return edges;
        }
        private Text MakeText(Transform parent, string name, string value, int size, TextAnchor alignment, Vector2 minimum, Vector2 maximum)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), minimum, maximum);
            var text = go.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size; text.color = Ink;
            text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow; text.raycastTarget = false;
            return text;
        }
        private Button MakeButton(Transform parent, string name, string label, Action action)
        {
            var rect = MakePanel(parent, name, new Color(.12f, .085f, .028f), Vector2.zero, Vector2.one);
            var button = rect.gameObject.AddComponent<MiniGameAudioButton>();
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = TerminalView.Bright;
            colors.pressedColor = Accent; colors.disabledColor = TerminalView.Edge; button.colors = colors;
            var text = MakeText(rect, "Label", label, 24, TextAnchor.MiddleLeft, new Vector2(.04f, .02f), new Vector2(.97f, .98f));
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 18; text.resizeTextMaxSize = 24;
            button.onClick.AddListener(() => action());
            return button;
        }
        private static void SetRect(RectTransform rect, Vector2 minimum, Vector2 maximum)
        { rect.anchorMin = minimum; rect.anchorMax = maximum; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        private static string EndingName(EndingType ending)
        {
            switch (ending) { case EndingType.TakenAway: return "结局：被带走"; case EndingType.LockedInRoom: return "结局：锁在房间";
                case EndingType.PriestAttack: return "结局：神父袭击"; case EndingType.DreamSea: return "结局：海上之梦";
                default: return "检查期结束"; }
        }
        private static string PhaseName(GamePhase phase)
        {
            switch (phase) { case GamePhase.MorningInspection: return "晨检"; case GamePhase.Briefing: return "简报"; case GamePhase.Apartment: return "公寓";
                case GamePhase.Work: return "工作"; case GamePhase.Evening: return "夜间"; case GamePhase.Sleeping: return "睡眠";
                case GamePhase.Dream: return "梦境"; case GamePhase.AfterDream: return "梦后"; default: return "检查期结束"; }
        }
        private static string WorldInstruction(GamePhase phase)
        {
            switch (phase) { case GamePhase.MorningInspection: return "门外有人。到门前完成晨检。"; case GamePhase.Apartment: return "简报已送达。到电脑开始工作。";
                case GamePhase.Work: return "下一位病人在等候。到电脑继续。"; case GamePhase.Evening: return "工作结束。到床前休息。";
                case GamePhase.AfterDream: return "从梦境醒来。只有床可以继续推进日期。"; default: return "按屏幕提示继续。"; }
        }
    }
}
