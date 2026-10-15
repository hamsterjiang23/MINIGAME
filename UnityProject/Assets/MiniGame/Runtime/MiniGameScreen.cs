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
        private static readonly Color Ink = new Color(.73f, .85f, .77f);
        private static readonly Color Dark = new Color(.035f, .065f, .06f, .98f);
        private static readonly Color Accent = new Color(.29f, .58f, .42f);
        private GameRuntime runtime;
        private RuntimeView current;
        private Font font;
        private RectTransform canvasRoot;
        private GameObject terminal;
        private TerminalView terminalView;
        private GameObject conversation;
        private GameObject inspection;
        private GameObject pause;
        private GameObject inspectionDrawing;
        private Image fade;
        private Image glitch;
        private Text header;
        private Text speaker;
        private Text body;
        private Text worldPrompt;
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
        private int selectedResponse;
        public event Action SaveRequested;
        public event Action LoadRequested;
        public event Action RestartRequested;
        public Canvas Canvas { get; private set; }

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= Render;
            EnsureBuilt();
            runtime = game;
            fallbackKey = null;
            transitionLocked = false;
            HideDialogueActions();
            runtime.Changed += Render;
            Render(runtime.GetView());
            terminalView.ResetNavigation();
        }

        public void UseYarnPresentation() => yarnBound = true;
        public void SetWorldPrompt(string text) { if (worldPrompt != null) worldPrompt.text = text; }
        public void Notify(string text, bool error)
        {
            EnsureBuilt(); notice.text = text;
            notice.color = error ? new Color(1f, .58f, .42f) : Ink;
            notificationEnd = Time.unscaledTime + 5f;
        }
        public void SetFade(float amount) { EnsureBuilt(); fade.color = new Color(0f, 0f, 0f, Mathf.Clamp01(amount)); }
        public void SetGlitch(float amount)
        {
            EnsureBuilt();
            glitch.gameObject.SetActive(amount > 0f);
            glitch.color = new Color(.35f, .65f, .45f, Mathf.Clamp01(amount) * .32f);
            glitch.rectTransform.anchoredPosition = amount > 0f ? new Vector2(Mathf.Sin(Time.unscaledTime * 73f) * 22f, 0f) : Vector2.zero;
        }
        public void SetTransitionLocked(bool locked) { transitionLocked = locked; ApplyInputGate(); }

        public void SetDialogueText(string displaySpeaker, string text)
        { EnsureBuilt(); speaker.text = displaySpeaker; body.text = text; terminalView.SetLine(displaySpeaker, text, null); }

        public void SetDialogueLine(string displaySpeaker, string text, string sourceNodeId, string portraitId)
        {
            SetDialogueText(displaySpeaker, text);
            terminalView.SetLine(displaySpeaker, text, portraitId);
            runtime?.RecordDialogueLine(displaySpeaker, text, sourceNodeId, portraitId);
        }

        public void ShowResponseButtons(IReadOnlyList<ResponseContent> responses, Action<string> selected)
        {
            EnsureBuilt();
            HideDialogueActions();
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
            selectedResponse = 0;
            terminalView.HighlightResponse(selectedResponse);
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void ShowCueText(string text, Action complete)
        {
            EnsureBuilt(); HideDialogueActions();
            body.text = text;
            terminalView.Body.text = text;
            var next = ActiveContinue;
            next.gameObject.SetActive(complete != null);
            next.onClick.RemoveAllListeners();
            if (complete != null) next.onClick.AddListener(() => { if (CanAdvanceDialogue()) complete(); });
            next.GetComponentInChildren<Text>().text = current?.Cue?.Kind == PresentationKind.Reaction && !current.UsesTerminalDirectory
                ? "完成处置并返回房间 [空格]" : current != null && current.TerminalOpen ? "下一句  ↵" : "继续 [空格]";
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void HideDialogueActions()
        {
            EnsureBuilt();
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
            if (current == null || current.Paused || !current.TerminalOpen || !terminalView.HistoryOpen) return false;
            terminalView.SetHistory(false);
            ApplyInputGate();
            return true;
        }

        private void Update()
        {
            if (notice != null && Time.unscaledTime > notificationEnd) notice.text = string.Empty;
            if (current == null || current.Paused || current.Inspection != null || transitionLocked) return;
            if (current.TerminalOpen)
            {
                foreach (var key in new[] { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Return, KeyCode.KeypadEnter })
                    if (Input.GetKeyDown(key) && terminalView.HandleDirectoryKey(key)) return;
                if (terminalView.HistoryOpen)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow)) terminalView.ScrollHistory(1);
                    if (Input.GetKeyDown(KeyCode.DownArrow)) terminalView.ScrollHistory(-1);
                    if (Input.mouseScrollDelta.y != 0) terminalView.ScrollHistory((int)Mathf.Sign(Input.mouseScrollDelta.y));
                    return;
                }
            }
            if (!CanAdvanceDialogue()) return;
            bool confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);
            var next = ActiveContinue;
            if (confirm && next != null && next.gameObject.activeInHierarchy && next.interactable)
            { next.onClick.Invoke(); return; }
            if (choices.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) selectedResponse = Mathf.Max(0, selectedResponse - 1);
                if (Input.GetKeyDown(KeyCode.DownArrow)) selectedResponse = Mathf.Min(choices.Count - 1, selectedResponse + 1);
                terminalView.HighlightResponse(selectedResponse);
                if (confirm && choices[selectedResponse].interactable) { choices[selectedResponse].onClick.Invoke(); return; }
            }
            for (int index = 0; index < choices.Count; index++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + index) && choices[index].interactable) choices[index].onClick.Invoke();
        }

        private void LateUpdate() => terminalView?.Fit();
        public void ReflowTerminal() => terminalView?.Fit();
        private Button ActiveContinue => current != null && current.TerminalOpen ? terminalView.Continue : continueButton;
        private bool CanAdvanceDialogue() => current != null && dialogueInteractable && !transitionLocked && !current.Paused && current.Inspection == null
            && (!current.TerminalOpen || (terminalView.IsReview && !terminalView.HistoryOpen));

        private void OnDestroy()
        {
            UnityEngine.Canvas.willRenderCanvases -= ReflowTerminal;
            Font.textureRebuilt -= RefreshFontTexture;
            if (runtime != null) runtime.Changed -= Render;
            if (font != null) Destroy(font);
        }

        private void RefreshFontTexture(Font rebuilt)
        {
            if (rebuilt == font && font != null && font.material != null && font.material.mainTexture != null)
                font.material.mainTexture.filterMode = FilterMode.Point;
        }

        private void Render(RuntimeView view)
        {
            EnsureBuilt(); current = view;
            header.text = "和平部 / 检定室 07     第 " + view.Game.Day + " 天     " + PhaseName(view.Game.Phase);
            header.gameObject.SetActive(!view.TerminalOpen);
            terminal.SetActive(view.TerminalOpen);
            terminalView.Render(view, transitionLocked || view.Paused || view.Inspection != null);
            conversation.SetActive(!view.TerminalOpen && (view.Cue != null || view.Dialogue != null || view.Game.Phase == GamePhase.Ending));
            SetRect(conversationRect, new Vector2(.17f, .19f), new Vector2(.83f, .74f));
            worldHelp.gameObject.SetActive(view.Input == InputContext.World);
            worldHelp.text = "WASD 移动 / 鼠标观察 / E 操作\n" + WorldInstruction(view.Game.Phase) + "\nESC 暂停 · F5 保存 · F9 恢复";
            inspection.SetActive(view.Inspection != null);
            if (view.Inspection != null)
            {
                inspectionTitle.text = view.Inspection.Title;
                inspectionSummary.text = view.Inspection.Summary + "\n\n这份观察仅是线索。处置由你决定。";
                BuildInspectionDrawing(view.Inspection.Kind);
            }
            pause.SetActive(view.Paused);
            if (view.Paused) pause.transform.SetAsLastSibling();
            if (!yarnBound || (view.Cue != null && (view.Cue.Kind == PresentationKind.EnterTerminal || view.Cue.Kind == PresentationKind.ExitTerminal || view.Cue.Kind == PresentationKind.Sleep)))
                RenderFallback(view);
            if (view.Game.Phase == GamePhase.Ending && view.Cue == null)
            {
                SetDialogueText("检查期结束", EndingName(view.Game.Ending) + "\n\n按 ESC 可保存、恢复或开始新的检查期。");
                HideDialogueActions();
            }
            ApplyInputGate();
        }

        private void RenderFallback(RuntimeView view)
        {
            string key = view.Cue?.Token ?? view.Dialogue?.NodeId ?? "idle";
            if (fallbackKey == key) return;
            fallbackKey = key;
            if (view.Cue != null)
            {
                var token = view.Cue.Token;
                bool automatic = view.Cue.Kind == PresentationKind.EnterTerminal || view.Cue.Kind == PresentationKind.ExitTerminal || view.Cue.Kind == PresentationKind.Sleep;
                SetDialogueText("和平仪", view.Cue.Text);
                ShowCueText(view.Cue.Text, automatic ? null : (Action)(() => Check(runtime.CompleteCue(token))));
            }
            else if (view.Dialogue != null)
            {
                string encounter = view.Game.CurrentEncounterId;
                string node = view.Game.CurrentNodeId;
                SetDialogueText(view.Dialogue.Speaker, view.Dialogue.Text);
                ShowResponseButtons(view.Dialogue.Responses, id => Check(runtime.SelectResponse(encounter, node, id)));
            }
            else { SetDialogueText("和平仪", "待机。请完成观察与会谈，再提交处置。"); HideDialogueActions(); }
        }

        private void ApplyInputGate()
        {
            if (current == null) return;
            bool enabled = CanAdvanceDialogue();
            if (continueButton != null) continueButton.interactable = enabled;
            if (terminalView != null) terminalView.Continue.interactable = enabled;
            foreach (var button in choices) if (button != null) button.interactable = enabled;
            terminalView?.Render(current, transitionLocked || current.Paused || current.Inspection != null);
            if (closeInspection != null) closeInspection.interactable = !current.Paused;
        }

        private void Check(RuntimeResult result) { if (!result.Accepted) Notify("操作未被接受：" + result.Reason, true); }

        private void EnsureBuilt()
        {
            if (canvasRoot != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "SimSun", "Noto Serif CJK SC", "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 24);
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

            header = MakeText(canvasRoot, "ShiftHeader", "", 25, TextAnchor.MiddleLeft, new Vector2(.025f, .925f), new Vector2(.97f, .98f));
            worldHelp = MakeText(canvasRoot, "WorldInstructions", "", 24, TextAnchor.LowerLeft, new Vector2(.03f, .04f), new Vector2(.57f, .19f));
            worldPrompt = MakeText(canvasRoot, "InteractionPrompt", "", 32, TextAnchor.MiddleCenter, new Vector2(.25f, .4f), new Vector2(.75f, .47f));
            MakeText(canvasRoot, "Reticle", "·", 32, TextAnchor.MiddleCenter, new Vector2(.49f, .485f), new Vector2(.51f, .515f));
            terminal = MakePanel(canvasRoot, "Terminal", Color.black, Vector2.zero, Vector2.one).gameObject;
            terminalView = new TerminalView(terminal.GetComponent<RectTransform>(), font,
                id => Check(runtime.EnterSelectedPatient(id)), () => Check(runtime.ReturnToDirectory()),
                () => Check(runtime.LeaveTerminal()), disposition =>
                {
                    var view = runtime.GetView();
                    Check(runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition));
                }, kind => Check(runtime.Inspect(kind)));
            terminalView.HistoryChanged = ApplyInputGate;
            UnityEngine.Canvas.willRenderCanvases += ReflowTerminal;
            conversationRect = MakePanel(canvasRoot, "Conversation", new Color(.045f, .07f, .06f, .98f), new Vector2(.36f, .115f), new Vector2(.955f, .65f));
            conversation = conversationRect.gameObject;
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

            inspection = MakePanel(canvasRoot, "InspectionModal", new Color(.03f, .05f, .045f, .995f), new Vector2(.14f, .12f), new Vector2(.86f, .86f)).gameObject;
            var ir = inspection.GetComponent<RectTransform>();
            inspectionTitle = MakeText(ir, "InspectionTitle", "", 32, TextAnchor.MiddleLeft, new Vector2(.04f, .84f), new Vector2(.95f, .96f));
            inspectionDrawing = MakePanel(ir, "ObservationIllustration", new Color(.12f, .16f, .13f), new Vector2(.05f, .19f), new Vector2(.43f, .82f)).gameObject;
            inspectionSummary = MakeText(ir, "InspectionDescription", "", 29, TextAnchor.UpperLeft, new Vector2(.48f, .20f), new Vector2(.95f, .81f));
            closeInspection = MakeButton(ir, "CloseInspection", "记录并返回 [ESC]", () => runtime.CloseInspection());
            SetRect(closeInspection.GetComponent<RectTransform>(), new Vector2(.58f, .04f), new Vector2(.95f, .15f));

            pause = MakePanel(canvasRoot, "PauseModal", new Color(.015f, .025f, .025f, .995f), new Vector2(.27f, .12f), new Vector2(.73f, .88f)).gameObject;
            var pr = pause.GetComponent<RectTransform>();
            MakeText(pr, "PauseTitle", "暂停 / 检查点", 38, TextAnchor.MiddleCenter, new Vector2(.04f, .81f), new Vector2(.96f, .96f));
            var labels = new[] { "返回 [ESC]", "保存 [F5]", "恢复 [F9]", "开始新检查期" };
            Action[] actions = { () => runtime.SetPaused(false), () => SaveRequested?.Invoke(), () => LoadRequested?.Invoke(), () => RestartRequested?.Invoke() };
            for (int index = 0; index < labels.Length; index++)
            {
                var button = MakeButton(pr, "PauseAction" + index, labels[index], actions[index]);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(.12f, .62f - index * .145f), new Vector2(.88f, .735f - index * .145f));
            }
            MakeText(pr, "PauseHint", "存档保留当前会谈与演出检查点。", 23, TextAnchor.MiddleCenter, new Vector2(.04f, .035f), new Vector2(.96f, .16f));
            fade = MakePanel(canvasRoot, "PresentationFade", Color.clear, Vector2.zero, Vector2.one).GetComponent<Image>();
            fade.raycastTarget = false;
            // Several narrow stripes provide a generated signal-loss effect, without a recovered shader.
            glitch = MakePanel(canvasRoot, "GeneratedSignalLoss", Color.clear, Vector2.zero, Vector2.one).GetComponent<Image>();
            glitch.raycastTarget = false;
            for (int index = 0; index < 27; index++)
            {
                var stripe = MakePanel(glitch.rectTransform, "Scanline" + index, new Color(.0f, .07f, .025f, .4f), new Vector2(0, index / 27f), new Vector2(1, index / 27f + .005f));
                stripe.GetComponent<Image>().raycastTarget = false;
            }
            glitch.gameObject.SetActive(false);
            notice = MakeText(canvasRoot, "Notification", "", 24, TextAnchor.MiddleCenter, new Vector2(.08f, .865f), new Vector2(.92f, .925f));

        }

        private void BuildInspectionDrawing(InspectionKind kind)
        {
            var root = inspectionDrawing.GetComponent<RectTransform>();
            for (int index = root.childCount - 1; index >= 0; index--) { root.GetChild(index).gameObject.SetActive(false); Destroy(root.GetChild(index).gameObject); }
            var pale = new Color(.58f, .64f, .55f);
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
            var rect = MakePanel(parent, name, new Color(.11f, .20f, .15f), Vector2.zero, Vector2.one);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(.72f, .95f, .8f);
            colors.pressedColor = Accent; colors.disabledColor = new Color(.32f, .36f, .33f); button.colors = colors;
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
                case EndingType.PriestAttack: return "结局：神父袭击"; default: return "检查期结束"; }
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
