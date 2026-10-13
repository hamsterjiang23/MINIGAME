using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private GameObject conversation;
        private GameObject inspection;
        private GameObject pause;
        private GameObject inspectionDrawing;
        private Image fade;
        private Image glitch;
        private Text header;
        private Text patientList;
        private Text patientName;
        private Text speaker;
        private Text body;
        private Text worldPrompt;
        private Text notice;
        private Text inspectionTitle;
        private Text inspectionSummary;
        private Text worldHelp;
        private Text observationLog;
        private Button continueButton;
        private Button closeInspection;
        private RectTransform choiceRoot;
        private RectTransform conversationRect;
        private readonly List<Button> choices = new List<Button>();
        private readonly List<Button> checkButtons = new List<Button>();
        private readonly Dictionary<Disposition, Button> dispositionButtons = new Dictionary<Disposition, Button>();
        private bool yarnBound;
        private bool dialogueInteractable = true;
        private bool transitionLocked;
        private string fallbackKey;
        private float notificationEnd;
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
            runtime.Changed += Render;
            Render(runtime.GetView());
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
        { EnsureBuilt(); speaker.text = displaySpeaker; body.text = text; }

        public void ShowResponseButtons(IReadOnlyList<ResponseContent> responses, Action<string> selected)
        {
            EnsureBuilt();
            HideDialogueActions();
            foreach (var response in responses.Take(4))
            {
                var id = response.Id;
                var button = MakeButton(choiceRoot, "Reply-" + id, response.Text, () => selected(id));
                var element = button.gameObject.AddComponent<LayoutElement>();
                element.preferredHeight = 64f;
                element.minHeight = 54f;
                choices.Add(button);
            }
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void ShowCueText(string text, Action complete)
        {
            EnsureBuilt(); HideDialogueActions();
            body.text = text;
            continueButton.gameObject.SetActive(complete != null);
            continueButton.onClick.RemoveAllListeners();
            if (complete != null) continueButton.onClick.AddListener(() => complete());
            dialogueInteractable = true;
            ApplyInputGate();
        }

        public void HideDialogueActions()
        {
            EnsureBuilt();
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveAllListeners();
            foreach (var choice in choices) if (choice != null) { choice.gameObject.SetActive(false); Destroy(choice.gameObject); }
            choices.Clear();
        }

        public void SetDialogueInteractable(bool enabled)
        { dialogueInteractable = enabled; ApplyInputGate(); }

        private void Update()
        {
            if (notice != null && Time.unscaledTime > notificationEnd) notice.text = string.Empty;
            if (Input.GetKeyDown(KeyCode.Space) && continueButton != null && continueButton.gameObject.activeInHierarchy && continueButton.interactable)
                continueButton.onClick.Invoke();
            if (current == null || current.Input != InputContext.Dialogue) return;
            for (int index = 0; index < choices.Count; index++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + index) && choices[index].interactable) choices[index].onClick.Invoke();
        }

        private void OnDestroy() { if (runtime != null) runtime.Changed -= Render; if (font != null) Destroy(font); }

        private void Render(RuntimeView view)
        {
            EnsureBuilt(); current = view;
            header.text = "和平部 / 检定室 07     第 " + view.Game.Day + " 天     净化配额剩余 " + view.Game.RemainingPurificationQuota + "     " + PhaseName(view.Game.Phase);
            terminal.SetActive(view.TerminalOpen);
            conversation.SetActive(view.TerminalOpen || view.Cue != null || view.Dialogue != null || view.Game.Phase == GamePhase.Ending);
            SetRect(conversationRect, view.TerminalOpen ? new Vector2(.36f, .115f) : new Vector2(.17f, .19f),
                view.TerminalOpen ? new Vector2(.955f, .65f) : new Vector2(.83f, .74f));
            patientName.text = view.Game.CurrentPatientName ?? "线路待机";
            var roster = new StringBuilder("病人记录\n\n");
            foreach (var patient in view.Patients)
            {
                roster.Append(patient.IsCurrent ? "▶ " : "  ").Append(patient.Name).Append('\n');
                roster.Append("    ").Append(PresenceName(patient.Presence)).Append(" · 治疗 ").Append(patient.TreatmentCount);
                if (patient.HasMetToday) roster.Append(" · 今日已会谈");
                roster.Append("\n\n");
            }
            patientList.text = roster.ToString();
            string prefix = view.Game.Day + ":" + view.Game.CurrentPatientId + ":";
            var observations = view.ObservedSigns.Where(sign => sign.StartsWith(prefix, StringComparison.Ordinal))
                .Select(sign => Enum.TryParse(sign.Substring(prefix.Length), out InspectionKind kind) ? InspectionName(kind) : sign).ToArray();
            observationLog.text = "本会谈观察记录 " + observations.Length + " / 6\n" + string.Join(" · ", observations);
            worldHelp.gameObject.SetActive(view.Input == InputContext.World);
            worldHelp.text = "WASD 移动 / 鼠标观察 / E 操作\n" + WorldInstruction(view.Game.Phase) + "\nESC 暂停 · F5 保存 · F9 恢复";
            foreach (var pair in dispositionButtons)
            {
                bool available = view.Game.AvailableDispositions.Contains(pair.Key);
                pair.Value.gameObject.SetActive(pair.Key != Disposition.Release || available);
                pair.Value.interactable = available && view.Input == InputContext.Dialogue;
            }
            foreach (var button in checkButtons)
                button.interactable = view.Input == InputContext.Dialogue && view.Game.CurrentPatientId != null;
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
                SetDialogueText("检查期结束", EndingName(view.Game.Ending) + "\n\n按 ESC 可保存、恢复或开始新的检查期。\n开发占位结局，条件来自核心规则。");
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
                speaker.text = "系统演出";
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
            bool enabled = dialogueInteractable && !transitionLocked && !current.Paused && current.Inspection == null;
            if (continueButton != null) continueButton.interactable = enabled;
            foreach (var button in choices) if (button != null) button.interactable = enabled && current.Input == InputContext.Dialogue;
            if (closeInspection != null) closeInspection.interactable = !current.Paused;
        }

        private void Check(RuntimeResult result) { if (!result.Accepted) Notify("操作未被接受：" + result.Reason, true); }

        private void EnsureBuilt()
        {
            if (canvasRoot != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Noto Sans CJK SC", "Arial" }, 24);
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
            terminal = MakePanel(canvasRoot, "Terminal", Dark, new Vector2(.015f, .025f), new Vector2(.985f, .91f)).gameObject;
            var tr = terminal.GetComponent<RectTransform>();
            MakeText(tr, "TerminalTitle", "和平仪 · 远程会谈线路", 29, TextAnchor.MiddleLeft, new Vector2(.03f, .92f), new Vector2(.7f, .98f));
            var portrait = MakePanel(tr, "OriginalPlaceholderPortrait", new Color(.08f, .11f, .10f), new Vector2(.03f, .37f), new Vector2(.32f, .9f));
            MakePanel(portrait, "Shoulders", new Color(.2f, .27f, .23f), new Vector2(.15f, .04f), new Vector2(.85f, .43f));
            MakePanel(portrait, "Head", new Color(.37f, .43f, .38f), new Vector2(.34f, .38f), new Vector2(.66f, .79f));
            MakePanel(portrait, "EyeLeft", new Color(.04f, .05f, .045f), new Vector2(.37f, .62f), new Vector2(.46f, .65f));
            MakePanel(portrait, "EyeRight", new Color(.04f, .05f, .045f), new Vector2(.54f, .62f), new Vector2(.63f, .65f));
            patientName = MakeText(portrait, "PatientName", "", 30, TextAnchor.MiddleCenter, new Vector2(.02f, .81f), new Vector2(.98f, .99f));
            MakeText(portrait, "PortraitDisclaimer", "原创灰盒肖像 / 开发占位", 17, TextAnchor.MiddleCenter, new Vector2(.03f, .01f), new Vector2(.97f, .08f));
            patientList = MakeText(tr, "PatientRoster", "", 20, TextAnchor.UpperLeft, new Vector2(.03f, .015f), new Vector2(.32f, .35f));
            observationLog = MakeText(tr, "ObservationHistory", "", 20, TextAnchor.MiddleLeft, new Vector2(.36f, .73f), new Vector2(.97f, .82f));
            var kinds = (InspectionKind[])Enum.GetValues(typeof(InspectionKind));
            for (int index = 0; index < kinds.Length; index++)
            {
                var kind = kinds[index];
                var button = MakeButton(tr, "Inspect-" + kind, InspectionName(kind), () => Check(runtime.Inspect(kind)));
                SetRect(button.GetComponent<RectTransform>(), new Vector2(.36f + index * .102f, .83f), new Vector2(.455f + index * .102f, .905f));
                checkButtons.Add(button);
            }
            var dispositions = new[] { Disposition.Treat, Disposition.Purify, Disposition.Release };
            for (int index = 0; index < dispositions.Length; index++)
            {
                var disposition = dispositions[index];
                var button = MakeButton(tr, "Disposition-" + disposition, DispositionName(disposition), () =>
                {
                    var view = runtime.GetView();
                    Check(runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition));
                });
                SetRect(button.GetComponent<RectTransform>(), new Vector2(.36f + index * .205f, .01f), new Vector2(.55f + index * .205f, .09f));
                button.GetComponent<Image>().color = disposition == Disposition.Purify ? new Color(.42f, .2f, .18f) : disposition == Disposition.Release ? new Color(.14f, .34f, .23f) : new Color(.29f, .29f, .15f);
                dispositionButtons.Add(disposition, button);
            }
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
            MakeText(pr, "PauseHint", "存档包含当前会谈和演出检查点。\n开发配置：三天 / 五病人 / 三结局。", 23, TextAnchor.MiddleCenter, new Vector2(.04f, .035f), new Vector2(.96f, .16f));
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
            MakeText(canvasRoot, "PrototypeDisclaimer", "开发占位：对白、肖像、检查线索和梦境均非正式策划；不提供真假 / 善恶标准答案。", 17, TextAnchor.MiddleCenter, new Vector2(.08f, 0f), new Vector2(.92f, .025f));
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
        private static string DispositionName(Disposition disposition) => disposition == Disposition.Treat ? "治疗" : disposition == Disposition.Purify ? "净化" : "释放";
        private static string InspectionName(InspectionKind kind)
        {
            switch (kind) { case InspectionKind.Eyes: return "眼睛"; case InspectionKind.Hands: return "手部"; case InspectionKind.Teeth: return "牙齿";
                case InspectionKind.Photo: return "照片"; case InspectionKind.DailySigns: return "当日通知"; default: return "档案"; }
        }
        private static string PresenceName(PatientPresence presence)
        {
            switch (presence) { case PatientPresence.Released: return "已释放"; case PatientPresence.Purified: return "已净化";
                case PatientPresence.Deceased: return "已死亡"; case PatientPresence.TreatmentEnded: return "治疗终止"; default: return "在册"; }
        }
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
