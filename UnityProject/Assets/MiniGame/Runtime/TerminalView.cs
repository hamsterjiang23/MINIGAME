using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Fixed 640 x 360 terminal layout. All patient and rule state is supplied by RuntimeView.</summary>
    internal sealed class TerminalView
    {
        private static readonly Color Desk = ColorOf("bfc2bc");
        private static readonly Color Pane = ColorOf("d1d3cd");
        private static readonly Color Field = ColorOf("dedfd8");
        private static readonly Color Ink = ColorOf("26312c");
        private static readonly Color Dim = ColorOf("5e695f");
        private static readonly Color Title = ColorOf("384640");
        private static readonly Color Pale = ColorOf("e0e3da");
        private static readonly Color Selected = ColorOf("455b4e");
        private readonly Font font;
        private readonly RectTransform viewport;
        private readonly RectTransform logical;
        private readonly GameObject directory;
        private readonly GameObject review;
        private readonly GameObject answers;
        private readonly GameObject history;
        private readonly Text breadcrumb;
        private readonly Text rosterPreview;
        private readonly Text rosterCount;
        private readonly Text record;
        private readonly Text quota;
        private readonly Text signal;
        private readonly Text historyText;
        private readonly Image lamp;
        private readonly GameObject silhouette;
        private readonly Button enter;
        private readonly Button returnDirectory;
        private readonly Button leave;
        private readonly Button answersTab;
        private readonly Button historyTab;
        private readonly List<Button> rosterRows = new List<Button>();
        private readonly Dictionary<Disposition, Button> dispositions = new Dictionary<Disposition, Button>();
        private readonly List<Button> inspections = new List<Button>();
        private readonly Action<string> enterPatient;
        private readonly Action returnToDirectory;
        private readonly Action leaveTerminal;
        private RuntimeView current;
        private int rosterIndex;
        private int historyOffset;
        private bool historyOpen;
        private bool locked;
        private string portraitId = "ML01";

        public RectTransform ChoiceRoot { get; }
        public Text Speaker { get; }
        public Text Body { get; }
        public Button Continue { get; }
        public bool HistoryOpen => historyOpen;
        public bool IsReview => current != null && current.TerminalPage == TerminalPage.Review;
        public Action HistoryChanged;

        public TerminalView(RectTransform parent, Font terminalFont, Action<string> enterSelected,
            Action returnDirectoryAction, Action leaveAction, Action<Disposition> dispositionAction,
            Action<InspectionKind> inspectionAction)
        {
            font = terminalFont;
            viewport = parent;
            enterPatient = enterSelected;
            returnToDirectory = returnDirectoryAction;
            leaveTerminal = leaveAction;
            logical = Panel(parent, "TerminalLogical640x360", Desk, 0, 0, 640, 360);
            logical.anchorMin = logical.anchorMax = logical.pivot = new Vector2(.5f, .5f);
            logical.anchoredPosition = Vector2.zero;
            Frame(logical, "TitleBar", "人员监控与复核终端       PMS / 3.14", 3, 3, 634, 24, true);
            Label(logical, "TerminalNumber", "T-06  047  1994-11-08", 458, 8, 173, 14, 10, Pale);
            Bevel(logical, "BreadcrumbPanel", 3, 30, 634, 17, Pane);
            breadcrumb = Label(logical, "TerminalBreadcrumb", "", 12, 32, 610, 14, 11, Ink);
            Bevel(logical, "StatusBar", 3, 346, 634, 11, ColorOf("aeb7ae"));
            Label(logical, "TerminalStatus", "PMS / LOCAL                       SEC.047  /  PERSONNEL REGISTER", 10, 346, 615, 11, 9, Dim);
            directory = Panel(logical, "ObjectDirectory", Color.clear, 0, 0, 640, 360).gameObject;
            review = Panel(logical, "PersonnelReview", Color.clear, 0, 0, 640, 360).gameObject;
            directory.GetComponent<Image>().raycastTarget = false;
            review.GetComponent<Image>().raycastTarget = false;

            var dr = directory.GetComponent<RectTransform>();
            Frame(dr, "RosterFrame", "观察对象 / 登记名册", 3, 50, 350, 292);
            Frame(dr, "PreviewFrame", "登记档案 / 对象预览", 357, 50, 280, 292);
            Panel(dr, "RosterColumns", Field, 7, 70, 343, 15);
            Label(dr, "RosterColumnLabels", "姓名 / 登记编号                         单元    状态", 13, 71, 332, 14, 11, Ink);
            for (int index = 0; index < 5; index++)
            {
                int slot = index;
                var row = Button(dr, "Roster-" + index, "", 7, 86 + index * 45, 342, 43, () => SelectRoster(slot));
                row.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
                var pointer = row.gameObject.AddComponent<TerminalRosterPointer>();
                pointer.DoubleClicked = () => { SelectRoster(slot); EnterSelected(); };
                rosterRows.Add(row);
            }
            rosterCount = Label(dr, "RosterCount", "", 13, 320, 332, 16, 10, Dim);
            Bevel(dr, "RegistrationPortrait", 366, 73, 107, 127, Field, true);
            var mug = Panel(dr, "RegistrationSilhouette", ColorOf("969f98"), 384, 92, 70, 90);
            Panel(mug, "Head", ColorOf("bbc0b7"), 21, 15, 28, 31);
            Panel(mug, "Shoulders", ColorOf("b7beb5"), 10, 47, 51, 40);
            rosterPreview = Label(dr, "RosterPreview", "", 486, 80, 143, 188, 11, Ink);
            enter = Button(dr, "EnterReview", "进入审查", 395, 293, 205, 30, EnterSelected);
            leave = Button(dr, "LeaveTerminal", "返回房间", 476, 315, 145, 22, () => leaveTerminal());
            // The entry action and room exit occupy distinct rows even on small monitors.
            Position(enter.GetComponent<RectTransform>(), 395, 279, 205, 28);

            var rr = review.GetComponent<RectTransform>();
            Frame(rr, "IndividualRecordFrame", "个体记录", 3, 50, 146, 180);
            Frame(rr, "MonitorFrame", "实时监控 / B-12", 153, 50, 361, 180);
            Frame(rr, "VerificationFrame", "核验结果", 518, 50, 119, 83);
            Frame(rr, "DispositionFrame", "处置", 518, 137, 119, 93);
            Frame(rr, "SpeechFrame", "当前发言", 3, 233, 146, 109);
            Frame(rr, "AnswerFrame", "应答", 153, 233, 484, 109);
            Bevel(rr, "RegistrationNumberField", 9, 72, 134, 22, Field, true);
            Label(rr, "RegistrationNumber", "R-0317-042", 15, 76, 124, 15, 11, Ink);
            record = Label(rr, "IndividualRecord", "", 12, 99, 128, 76, 11, Ink);
            Label(rr, "RecordExcerpt", "记录摘录\n\n本次会谈实时记录", 12, 181, 130, 43, 10, Dim);
            var monitor = Bevel(rr, "LiveMonitor", 158, 70, 350, 144, ColorOf("1b2420"), true);
            silhouette = Panel(monitor, "PatientSilhouette", Color.clear, 0, 0, 350, 144).gameObject;
            silhouette.GetComponent<Image>().raycastTarget = false;
            var sr = silhouette.GetComponent<RectTransform>();
            Panel(sr, "Hair", ColorOf("3d4842"), 161, 29, 56, 24);
            Panel(sr, "Face", ColorOf("6e7771"), 163, 53, 52, 29);
            Panel(sr, "Shoulders", ColorOf("4c5650"), 136, 85, 106, 52);
            for (int index = 0; index < 32; index++)
                Panel(monitor, "MonitorScanline" + index, new Color(0, 0, 0, .10f), 3, 4 + index * 4, 344, 1);
            Label(rr, "CameraLabel", "CAM B12-01 / FIXED", 169, 78, 150, 13, 9, ColorOf("b4c2b5"));
            signal = Label(rr, "VideoSignal", "CH 01 / 视频：正常                         拾音：接通", 160, 215, 350, 14, 10, Ink);
            Label(rr, "DepartureVerification", "离所核验", 525, 76, 75, 14, 11, Ink);
            Bevel(rr, "LampHousing", 603, 71, 17, 17, ColorOf("829085"), true);
            lamp = Panel(rr, "DepartureLamp", ColorOf("6d786f"), 608, 76, 7, 7).GetComponent<Image>();
            Label(rr, "PurificationQuotaLabel", "净化剩余", 525, 108, 72, 15, 11, Ink);
            quota = Label(rr, "PurificationQuota", "", 603, 108, 28, 15, 11, Ink);
            var types = new[] { Disposition.Release, Disposition.Treat, Disposition.Purify };
            for (int index = 0; index < types.Length; index++)
            {
                var type = types[index];
                dispositions.Add(type, Button(rr, "Disposition-" + type, DispositionName(type), 530, 157 + 23 * index, 95, 20, () => dispositionAction(type)));
            }
            Bevel(rr, "SpeechField", 9, 255, 134, 82, Field, true);
            Speaker = Label(rr, "TerminalSpeaker", "对象", 15, 261, 122, 15, 11, Ink);
            Body = Label(rr, "TerminalDialogueBody", "", 15, 279, 122, 53, 12, Ink);
            Body.gameObject.AddComponent<RectMask2D>();
            answersTab = Button(rr, "AnswersTab", "应答", 548, 236, 39, 15, () => SetHistory(false));
            historyTab = Button(rr, "HistoryTab", "记录", 591, 236, 39, 15, () => SetHistory(true));
            Bevel(rr, "AnswerField", 159, 254, 472, 82, Field, true);
            answers = Panel(rr, "AnswerContents", Color.clear, 162, 257, 466, 76).gameObject;
            answers.GetComponent<Image>().raycastTarget = false;
            ChoiceRoot = answers.GetComponent<RectTransform>();
            Continue = Button(ChoiceRoot, "Continue", "下一句  ↵", 4, 4, 105, 25, () => { });
            returnDirectory = Button(ChoiceRoot, "ReturnDirectory", "返回对象清单", 4, 4, 150, 25, () =>
            {
                SetHistory(false);
                returnToDirectory();
            });
            history = Panel(rr, "TranscriptPanel", Color.clear, 163, 258, 463, 74).gameObject;
            history.GetComponent<Image>().raycastTarget = false;
            historyText = Label(history.transform, "TranscriptText", "", 4, 0, 438, 74, 10, Ink);
            historyText.gameObject.AddComponent<RectMask2D>();
            Button(history.transform, "HistoryEarlier", "↑", 443, 0, 16, 21, () => ScrollHistory(1));
            Button(history.transform, "HistoryLater", "↓", 443, 51, 16, 21, () => ScrollHistory(-1));
            // Optional physical observations remain accessible without covering dialogue controls.
            var kinds = (InspectionKind[])Enum.GetValues(typeof(InspectionKind));
            for (int index = 0; index < kinds.Length; index++)
            {
                var kind = kinds[index];
                inspections.Add(Button(rr, "Inspect-" + kind, InspectionName(kind), 161 + index * 57, 216, 55, 12, () => inspectionAction(kind), 9));
            }
            Continue.gameObject.SetActive(false);
            returnDirectory.gameObject.SetActive(false);
            SetHistory(false);
            Fit();
        }

        public void Fit()
        {
            float scale = Mathf.Min(viewport.rect.width / 640f, viewport.rect.height / 360f);
            float next = Mathf.Max(.01f, scale);
            if (!Mathf.Approximately(logical.localScale.x, next)) logical.localScale = Vector3.one * next;
        }

        public void ResetNavigation()
        {
            rosterIndex = 0;
            historyOffset = 0;
            portraitId = "ML01";
            SetHistory(false);
        }

        public void Render(RuntimeView view, bool inputLocked)
        {
            current = view;
            locked = inputLocked;
            bool isDirectory = view.TerminalPage == TerminalPage.Directory || (view.TerminalPage == TerminalPage.Archive && view.ArchiveDirectoryVisible);
            directory.SetActive(isDirectory);
            review.SetActive(!isDirectory);
            breadcrumb.text = "第三安置所 / B区 / " + (isDirectory ? "对象目录" : "人员复核") + "          DAY " + view.Game.Day.ToString("00");
            record.text = "对象    " + (view.Game.CurrentPatientName ?? "小孩") + "\n单元    B-12\n状态    " + (view.Cue?.Kind == PresentationKind.Reaction ? "已提交处置" : "观察中");
            quota.text = view.Game.RemainingPurificationQuota.ToString("00");
            bool atDisposition = view.Game.EncounterStage == EncounterStage.AwaitingDisposition;
            bool release = view.Game.AvailableDispositions.Contains(Disposition.Release);
            lamp.color = !atDisposition ? ColorOf("6d786f") : release ? ColorOf("8faf83") : ColorOf("b07871");
            foreach (var pair in dispositions)
            {
                pair.Value.gameObject.SetActive(view.UsesTerminalDirectory || (atDisposition && view.Game.AvailableDispositions.Contains(pair.Key)));
                pair.Value.interactable = !locked && !historyOpen && view.Input == InputContext.Dialogue && atDisposition && view.Game.AvailableDispositions.Contains(pair.Key);
            }
            foreach (var button in inspections)
                button.interactable = !locked && !historyOpen && view.Input == InputContext.Dialogue && view.Game.CurrentPatientId != null;
            bool archived = view.TerminalPage == TerminalPage.Archive;
            returnDirectory.gameObject.SetActive(archived && !isDirectory);
            returnDirectory.interactable = !locked && !historyOpen;
            signal.text = archived ? "CH 01 / SIGNAL LOST                         拾音：断开" : "CH 01 / 视频：正常                         拾音：接通";
            silhouette.SetActive(!archived);
            RenderRoster();
            RenderHistory();
            Fit();
        }

        public void SetLine(string displaySpeaker, string text, string portrait)
        {
            Speaker.text = displaySpeaker;
            Body.text = text;
            if (!string.IsNullOrEmpty(portrait)) portraitId = portrait;
            // Portrait identifiers select presentation only. Formal sprites may replace the silhouette.
            var face = silhouette.transform.Find("Face");
            if (face != null) face.GetComponent<Image>().color = portraitId == "ML03" ? ColorOf("797a6c") : ColorOf("6e7771");
        }

        public Button AddResponse(string id, string text, int index, Action action) =>
            Button(ChoiceRoot, "Reply-" + id, (index + 1).ToString("00") + "   " + text, 0, index * 19, 466, 18, action, 12);

        public void SetHistory(bool open)
        {
            historyOpen = open;
            answers.SetActive(!open);
            history.SetActive(open);
            answersTab.GetComponent<Image>().color = open ? Pane : ColorOf("b0bdb1");
            historyTab.GetComponent<Image>().color = open ? ColorOf("b0bdb1") : Pane;
            if (current != null) Render(current, locked);
            HistoryChanged?.Invoke();
        }

        public void HighlightResponse(int index)
        {
            for (int child = 0, response = 0; child < ChoiceRoot.childCount; child++)
            {
                var item = ChoiceRoot.GetChild(child);
                if (!item.name.StartsWith("Reply-", StringComparison.Ordinal) || !item.gameObject.activeSelf) continue;
                item.GetComponent<Image>().color = response++ == index ? ColorOf("c0cac0") : Pane;
            }
        }

        public bool HandleDirectoryKey(KeyCode key)
        {
            if (current == null || !directory.activeSelf || locked) return false;
            if (key == KeyCode.UpArrow) SelectRoster(Mathf.Max(0, rosterIndex - 1));
            else if (key == KeyCode.DownArrow) SelectRoster(Mathf.Min(4, rosterIndex + 1));
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter) EnterSelected();
            else return false;
            return true;
        }

        public void ScrollHistory(int amount)
        {
            if (current == null) return;
            historyOffset = Mathf.Clamp(historyOffset + amount, 0, Mathf.Max(0, current.Transcript.Count - 5));
            RenderHistory();
        }

        private void SelectRoster(int index) { rosterIndex = index; RenderRoster(); }
        private void EnterSelected()
        {
            if (current == null || locked || !enter.interactable || rosterIndex >= current.Patients.Count) return;
            SetHistory(false);
            enterPatient(current.Patients[rosterIndex].Id);
        }

        private void RenderRoster()
        {
            if (current == null) return;
            for (int index = 0; index < rosterRows.Count; index++)
            {
                var row = rosterRows[index];
                var patient = index < current.Patients.Count ? current.Patients[index] : null;
                var label = row.GetComponentInChildren<Text>();
                label.text = patient == null ? "未配置对象 " + (index + 1).ToString("00") + "\n-- -- --                                     未接入" :
                    patient.Name + "                         B-12     " + PatientStatus(patient) + "\nR-0317-042";
                row.GetComponent<Image>().color = index == rosterIndex ? Selected : Pane;
                label.color = index == rosterIndex ? Pale : Ink;
                row.interactable = !locked;
            }
            var selected = rosterIndex < current.Patients.Count ? current.Patients[rosterIndex] : null;
            bool available = selected != null && selected.IsCurrent && !selected.HasMetToday && current.TerminalPage == TerminalPage.Directory;
            rosterPreview.text = selected == null ? "未配置\n\n-- -- --\n\n状态  未接入" :
                selected.Name + "\n\nR-0317-042\n\n单元  B-12\n\n状态  " + PatientStatus(selected) + "\n\n\n当前批次 DAY " + current.Game.Day.ToString("00") + "\n\n登记状态 " + (selected.HasMetToday ? "已归档" : "待复核");
            enter.interactable = available && !locked;
            enter.GetComponentInChildren<Text>().text = available ? "进入审查" : selected == null ? "未接入" : "已归档 / 禁止进入";
            leave.interactable = !locked;
            rosterCount.text = "当日登记 " + current.Patients.Count.ToString("00") + "    待审 " + current.Patients.Count(p => !p.HasMetToday).ToString("00") + "    已处置 " + current.Patients.Count(p => p.HasMetToday).ToString("00");
        }

        private void RenderHistory()
        {
            if (current == null) return;
            int count = current.Transcript.Count;
            historyOffset = Mathf.Clamp(historyOffset, 0, Mathf.Max(0, count - 5));
            int start = Mathf.Max(0, count - 5 - historyOffset);
            historyText.text = string.Join("\n", current.Transcript.Skip(start).Take(5).Select(entry =>
                (entry.IsPlayer ? "值班员" : entry.Speaker) + "  " + entry.Text.Replace("\n", " ")));
        }

        private RectTransform Bevel(Transform parent, string name, float x, float y, float width, float height, Color fill, bool sunken = false)
        {
            var rect = Panel(parent, name, fill, x, y, width, height);
            Color top = sunken ? ColorOf("707b72") : ColorOf("f0f0e9");
            Color bottom = sunken ? ColorOf("f0f0e9") : ColorOf("69756b");
            Panel(rect, "TopEdge", top, 0, 0, width, 1);
            Panel(rect, "LeftEdge", top, 0, 0, 1, height);
            Panel(rect, "BottomEdge", bottom, 0, height - 1, width, 1);
            Panel(rect, "RightEdge", bottom, width - 1, 0, 1, height);
            return rect;
        }

        private void Frame(Transform parent, string name, string title, float x, float y, float width, float height, bool dark = false)
        {
            var rect = Bevel(parent, name, x, y, width, height, dark ? Title : Pane);
            if (!dark) Panel(rect, "FrameHeading", ColorOf("aeb7ae"), 2, 2, width - 4, 16);
            Label(rect, "Heading", title, dark ? 12 : 7, dark ? 4 : 3, width - 15, dark ? 17 : 15, dark ? 13 : 11, dark ? Pale : Ink);
        }

        private RectTransform Panel(Transform parent, string name, Color color, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Position(rect, x, y, width, height);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private Text Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Position(go.GetComponent<RectTransform>(), x, y, width, height);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.text = value;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private Button Button(Transform parent, string name, string label, float x, float y, float width, float height, Action action, int size = 11)
        {
            var rect = Bevel(parent, name, x, y, width, height, Pane);
            rect.GetComponent<Image>().raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = ColorOf("b0bdb1");
            colors.pressedColor = ColorOf("899e8c");
            colors.disabledColor = ColorOf("bfc2bc");
            button.colors = colors;
            var text = Label(rect, "Label", label, 5, 0, width - 10, height, size, Ink);
            text.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => { if (button.interactable) action(); });
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            return button;
        }

        private static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
        private static Color ColorOf(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
        private static string DispositionName(Disposition type) => type == Disposition.Treat ? "治疗" : type == Disposition.Purify ? "净化" : "释放";
        private static string PatientStatus(PatientView patient) => patient.HasMetToday ? "已归档" : patient.IsCurrent ? "待审" : "未接入";
        private static string InspectionName(InspectionKind kind) => kind == InspectionKind.Eyes ? "眼睛" : kind == InspectionKind.Hands ? "手部" : kind == InspectionKind.Teeth ? "牙齿" : kind == InspectionKind.Photo ? "照片" : kind == InspectionKind.DailySigns ? "通知" : "档案";
    }

}
