using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>800 x 600 amber terminal. RuntimeView owns all patient and rule state.</summary>
    internal sealed class TerminalView
    {
        internal static readonly Color Desk = ColorOf("0b0803"), Ink = ColorOf("efb83f"), Bright = ColorOf("ffcf68"), Dim = ColorOf("a77d2d"), Edge = ColorOf("674b19");
        private readonly Font font;
        private readonly RectTransform logical;
        private readonly GameObject directory, review, selfReview, history, answers;
        private readonly Text date, day, count, previewName, previewId, previewStatus, previewUnit, quota;
        private readonly Text recordName, monitorHeading, cameraLabel, roundLabel, commandStatus, footerText, footerHelp, pageLabel;
        private readonly TerminalTranscriptView transcriptView;
        private readonly Button enter, leave, returnDirectory, selfPurify, footer;
        private readonly RawImage selfMonitor;
        private readonly RawImage supervisor;
        private readonly PortraitCatalog portraits;
        private readonly PatientSurveillanceFeed patientMonitor;
        private readonly TerminalPortraitPresenter portraitPresenter;
        private readonly GameObject livePortrait, silhouette, signalLoss, signalLostMessage;
        private readonly List<RectTransform> stripes = new List<RectTransform>();
        private readonly List<Button> rosterRows = new List<Button>();
        private readonly List<Text[]> rosterLabels = new List<Text[]>();
        private readonly Dictionary<Disposition, Button> dispositions = new Dictionary<Disposition, Button>();
        private readonly Action<string> enterPatient;
        private readonly Action returnToDirectory;
        private readonly TerminalDisplay display;
        private RuntimeView current;
        private bool locked;
        private readonly Text historyHeading;
        private readonly RawImage packageFrame;
        private readonly TerminalAssetSet assets;
        private int rosterIndex, selectedDisposition;
        private string portraitId = "ML01", sourceNodeId = "";
        public RectTransform ChoiceRoot { get; }
        public Text Speaker { get; }
        public Text Body { get; }
        public Text NarrationBody { get; }
        public Button Continue { get; }
        public bool HistoryOpen => false;
                public bool IsReview => current != null && current.TerminalPage == TerminalPage.Review;
        public Action ConfirmRequested;
        public TerminalDisplay Display => display;
        public RectTransform Logical => logical;
        public Font Typeface => font;

        public TerminalView(RectTransform parent, Font terminalFont, Action<string> enterSelected,
            Action returnDirectoryAction, Action leaveAction, Action<Disposition> dispositionAction,
            Action<InspectionKind> inspectionAction, Action selfPurifyAction = null)
        {
            font = terminalFont; assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets"); enterPatient = enterSelected; returnToDirectory = returnDirectoryAction;
            if (assets != null && assets.Typeface != null) font = assets.Typeface;
            portraits = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            logical = Panel(parent, "TerminalLogical800x600", Desk, 0, 0, 800, 600);
            logical.anchorMin = logical.anchorMax = logical.pivot = new Vector2(.5f, .5f);
            logical.anchoredPosition = Vector2.zero;
            var packageBackdrop = new GameObject("PMS package frame", typeof(RectTransform), typeof(RawImage));
            packageBackdrop.transform.SetParent(logical, false); Position((RectTransform)packageBackdrop.transform, 0, 0, 800, 600);
            packageFrame = packageBackdrop.GetComponent<RawImage>();
            packageFrame.raycastTarget = false;
            Border(logical, "TerminalBorder", 8, 8, 784, 584, ColorOf("d9a339"));
            Rule(logical, 8, 36, 784);
            date = Label(logical, "TerminalDate", "1994-11-08", 22, 16, 160, 17, 13, Bright);
            Label(logical, "TerminalVersion", "PMS / 3.14", 334, 16, 160, 17, 13, Ink);
            Label(logical, "TerminalNumber", "T-06 / 047", 674, 16, 110, 17, 13, Bright);
            directory = Panel(logical, "ObjectDirectory", Color.clear, 0, 0, 800, 548).gameObject;
            review = Panel(logical, "PersonnelReview", Color.clear, 0, 0, 800, 548).gameObject;
            selfReview = Panel(logical, "SelfReview", Color.clear, 0, 0, 800, 548).gameObject;
            var dr = (RectTransform)directory.transform;
            Label(dr, "RosterHeading", "第三安置所 / B区 / 观察对象", 23, 51, 490, 20, 14, Bright);
            day = Label(dr, "DirectoryDay", "", 600, 51, 180, 20, 13, Ink);
            Label(dr, "RosterColumnLabels", "对象 / 登记编号", 29, 82, 480, 18, 13, Dim);
            Label(dr, "RosterStatusColumn", "单元       状态", 624, 82, 150, 18, 13, Dim);
            for (int i = 0; i < 5; i++)
            {
                int slot = i;
                var row = Button(dr, "Roster-" + i, "", 22, 108 + 30 * i, 756, 27, () => SelectRoster(slot));
                var pointer = row.gameObject.AddComponent<TerminalRosterPointer>();
                pointer.DoubleClicked = () => { SelectRoster(slot); EnterSelected(); };
                Label(row.transform, "RowNumber", (i + 1).ToString("00"), 7, 5, 28, 18, 13, Dim);
                var name = row.transform.Find("Label").GetComponent<Text>(); Position(name.rectTransform, 40, 4, 160, 20);
                var registration = Label(row.transform, "Registration", "", 200, 5, 270, 18, 13, Ink);
                var unit = Label(row.transform, "Unit", "", 600, 5, 65, 18, 13, Ink);
                var status = Label(row.transform, "Status", "", 672, 5, 80, 18, 13, Ink);
                foreach (var label in new[] { registration, unit, status, row.transform.Find("RowNumber").GetComponent<Text>() }) TerminalIndustrialSkin.RegisterLabel(row, label);
                rosterRows.Add(row); rosterLabels.Add(new[] { name, registration, unit, status });
            }
            count = Label(dr, "RosterCount", "", 29, 351, 730, 20, 13, Dim);
            Rule(dr, 8, 393, 784);
            Field(dr, "终端", "第三安置所 / B区", 410);
            previewName = Field(dr, "对象", "", 436);
            enter = Button(dr, "EnterReview", "进入人员复核", 131, 458, 465, 26, EnterSelected);
            Label(dr, "CurrentCommandLabel", "当前命令", 24, 462, 100, 19, 14, Ink);
            previewStatus = Field(dr, "档案状态", "", 488);
            quota = Field(dr, "净化配额", "", 514);
            quota.name = "PurificationQuota";
            previewUnit = Label(dr, "PreviewUnit", "", 596, 514, 180, 20, 14, Ink);
            previewId = Label(dr, "PreviewId", "", 390, 436, 260, 20, 14, Ink);
            leave = Button(dr, "LeaveTerminal", "退出终端返回房间", 602, 458, 176, 26, leaveAction, 13);

            var rr = (RectTransform)review.transform;
            recordName = Label(rr, "RecordRegistration", "", 26, 51, 470, 20, 13, Ink);
            Label(rr, "ReviewFacility", "第三安置所 / B区", 532, 51, 245, 20, 13, Bright);
            Panel(rr, "ReviewDivider", Edge, 516, 48, 1, 486);
            monitorHeading = Label(rr, "MonitorHeading", "", 26, 69, 330, 19, 13, Bright);
            Label(rr, "MonitorChannel", "CH 01", 451, 69, 60, 18, 12, Dim);
            Border(rr, "MonitorFrame", 26, 88, 472, 269, Edge);
            var video = Panel(rr, "MonitorVideo", ColorOf("0e0b04"), 30, 92, 464, 261);
            silhouette = Label(video, "MonitorNoImage", "NO IMAGE", 172, 123, 160, 22, 13, Dim).gameObject;
            livePortrait = Panel(video, "LivePortrait", Color.clear, 0, 0, 464, 261).gameObject;
            var previous = Portrait(livePortrait.transform, "PreviousPortrait", 145, 20, 174, 241);
            var next = Portrait(livePortrait.transform, "CurrentPortrait", 145, 20, 174, 241);
            portraitPresenter = livePortrait.AddComponent<TerminalPortraitPresenter>(); portraitPresenter.Initialize(previous, next);
            var feed = new GameObject("PatientRoomFeed", typeof(RectTransform), typeof(RawImage), typeof(PatientSurveillanceFeed));
            feed.transform.SetParent(livePortrait.transform, false); Position((RectTransform)feed.transform, 0, 0, 464, 261);
            patientMonitor = feed.GetComponent<PatientSurveillanceFeed>();
            patientMonitor.Initialize(feed.GetComponent<RawImage>(), Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault"));
            cameraLabel = Label(video, "CameraLabel", "", 7, 8, 280, 16, 10, Dim);
            var supervisorObject = new GameObject("Day0SupervisorFeed", typeof(RectTransform), typeof(RawImage));
            supervisorObject.transform.SetParent(video, false); Position((RectTransform)supervisorObject.transform, 0, 0, 464, 261);
            supervisor = supervisorObject.GetComponent<RawImage>(); supervisor.raycastTarget = false;
            supervisor.texture = Resources.Load<Texture2D>("MiniGame/Day0Supervisor");
            supervisorObject.SetActive(false);
            signalLoss = Panel(video, "MonitorSignalLoss", ColorOf("0e0b04"), 0, 0, 464, 261).gameObject;
            for (int i = 0; i < 22; i++) stripes.Add(Panel(signalLoss.transform, "Noise" + i, i % 3 == 0 ? Ink : Edge, 0, 7 + i * 11, 464, i % 2 == 0 ? 2 : 1));
            signalLostMessage = Label(video, "SignalLostMessage", "SIGNAL LOST", 176, 123, 200, 25, 13, Dim).gameObject;
            historyHeading = Label(rr, "DialogueHeading", "对话 / R1", 26, 368, 470, 20, 13, Dim);
            Rule(rr, 22, 388, 480);
            transcriptView = new TerminalTranscriptView(rr, font);
            history = transcriptView.GameObject;
            roundLabel = Label(rr, "RecordNode", "人员复核 / R1", 534, 83, 242, 19, 13, Dim);
            commandStatus = Label(rr, "CommandStatus", "", 534, 112, 240, 46, 12, Dim);
            Label(rr, "DispositionHeading", "处置命令", 534, 173, 240, 19, 13, Dim); Rule(rr, 530, 194, 248);
            var types = new[] { Disposition.Release, Disposition.Treat, Disposition.Purify };
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var button = Button(rr, "Disposition-" + type, (i + 1).ToString("00") + "   " + DispositionName(type), 530, 204 + i * 37, 248, 28, () => dispositionAction(type));
                var availability = Label(button.transform, "Availability", "锁定", 184, 6, 60, 18, 13, Dim);
                TerminalIndustrialSkin.RegisterLabel(button, availability); dispositions.Add(type, button);
            }
            Rule(rr, 530, 334, 248); Label(rr, "AnswerHeading", "应答命令", 534, 346, 240, 19, 13, Dim);
            answers = Panel(rr, "AnswerContents", Color.clear, 530, 374, 248, 158).gameObject;
            ChoiceRoot = (RectTransform)answers.transform;
            Continue = Button(ChoiceRoot, "Continue", "下一句      Enter", 0, 0, 248, 28, null);
            returnDirectory = Button(ChoiceRoot, "ReturnDirectory", "返回对象目录   Enter", 0, 0, 248, 28, () => returnToDirectory());
            NarrationBody = Label(ChoiceRoot, "ConclusionBody", "", 6, 2, 236, 100, 14, Ink);
            NarrationBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            NarrationBody.verticalOverflow = VerticalWrapMode.Truncate;
            NarrationBody.alignment = TextAnchor.UpperLeft;
            // Text playback has a stable measuring target; the transcript displays the current page below.
            var playback = Panel(rr, "PlaybackMeasurement", Color.clear, -2000, 0, 415, 32);
            playback.gameObject.AddComponent<CanvasGroup>().alpha = 0;
            Body = Label(playback, "TerminalDialogueBody", "", 0, 0, 415, 32, 14, Ink);
            Body.horizontalOverflow = HorizontalWrapMode.Wrap;
            Body.verticalOverflow = VerticalWrapMode.Truncate;
            Body.alignment = TextAnchor.UpperLeft;
            Speaker = Label(playback, "TerminalSpeaker", "对象", 0, 0, 60, 18, 11, Dim);
            pageLabel = Label(rr, "SpeechPage", "", 443, 532, 58, 13, 10, Dim);

            var sr = (RectTransform)selfReview.transform;
            Label(sr, "SelfSubject", "值班审查员 / 本人复核", 26, 51, 730, 22, 14, Bright);
            Border(sr, "SelfMonitorFrame", 26, 88, 472, 269, Edge);
            var selfFeed = new GameObject("SelfMonitorFeed", typeof(RectTransform), typeof(RawImage)); selfFeed.transform.SetParent(sr, false);
            Position((RectTransform)selfFeed.transform, 30, 92, 464, 261); selfMonitor = selfFeed.GetComponent<RawImage>(); selfMonitor.raycastTarget = false;
            Label(sr, "SelfCameraLabel", "CAM 07 / LIVE", 37, 100, 300, 20, 10, Dim);
            Label(sr, "SelfStatus", "本人复核 / 处置待确认", 534, 112, 240, 70, 14, Ink);
            selfPurify = Button(sr, "SelfPurify", "01   净化", 530, 204, 248, 28, selfPurifyAction);

            footer = Button(logical, "TerminalCommandStrip", "", 9, 548, 782, 43, () => ConfirmRequested?.Invoke());
            footerText = footer.transform.Find("Label").GetComponent<Text>(); Position(footerText.rectTransform, 5, 5, 772, 20); footerText.alignment = TextAnchor.UpperCenter;
            footerHelp = Label(footer.transform, "TerminalHelp", "", 5, 26, 772, 16, 12, Desk); footerHelp.alignment = TextAnchor.UpperCenter;
            TerminalIndustrialSkin.RegisterLabel(footer, footerHelp); TerminalIndustrialSkin.SetSelected(footer, true);
            Continue.gameObject.SetActive(false); returnDirectory.gameObject.SetActive(false); NarrationBody.gameObject.SetActive(false);
            display = parent.gameObject.AddComponent<TerminalDisplay>(); display.Initialize(logical);
            Fit();
        }

        public void Fit() => display.Fit();
        public void ResetNavigation(RuntimeView view)
        {
            current = view; rosterIndex = 0;
            transcriptView.Reset();
            sourceNodeId = ""; Body.text = ""; portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId ?? view.Game.CurrentPatientId);
            ClearNarration(); RestorePresentation(view); SetSignalDistortion(0); Render(view, false);
        }
        public void Render(RuntimeView view, bool inputLocked)
        {
            bool nextEncounter = current == null || current.PresentedEncounterId != view.PresentedEncounterId;
            bool enteringDirectory = view.TerminalPage == TerminalPage.Directory && current?.TerminalPage != TerminalPage.Directory;
            current = view; locked = inputLocked;
            if (nextEncounter) { sourceNodeId = ""; Body.text = ""; portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId); transcriptView.Reset(); RestorePresentation(view); }
            if (enteringDirectory) { int next = view.Patients.ToList().FindIndex(p => p.IsCurrent); if (next >= 0) rosterIndex = next; }
            bool self = view.TerminalPage == TerminalPage.SelfReview;
            bool dir = view.TerminalPage == TerminalPage.Directory || view.ArchiveDirectoryVisible;
            packageFrame.texture = assets != null ? (dir ? assets.DirectoryFrame : assets.ReviewFrame) : null;
            directory.SetActive(dir && !self); review.SetActive(!dir && !self); selfReview.SetActive(self);
            bool archived = view.TerminalPage == TerminalPage.Archive;
            selfPurify.interactable = self && !locked && view.Cue == null && !view.Paused;
            string id = view.PresentedPatientId ?? view.Game.CurrentPatientId;
            recordName.text = PortraitCatalog.Registration(id);
            monitorHeading.text = "监控 / " + PortraitCatalog.Unit(id);
            cameraLabel.text = "CAM " + PortraitCatalog.Unit(id).Replace("-", "") + "-01";
            string round = RoundName(sourceNodeId);
            roundLabel.text = "人员复核 / " + round;
            bool measure = view.Game.EncounterStage == EncounterStage.AwaitingDisposition && !archived;
            commandStatus.text = measure ? "会谈结束，可执行处置。" : view.PresentedDisposition.HasValue ? "处置已登记。" : "会谈进行中，处置暂时锁定。";
            int available = 0;
            foreach (var pair in dispositions)
            {
                bool configured = !view.UsesTerminalDirectory || view.ConfiguredDispositions.Contains(pair.Key);
                pair.Value.gameObject.SetActive(configured);
                bool enabled = configured && measure && !locked && view.Input == InputContext.Dialogue && view.Game.AvailableDispositions.Contains(pair.Key);
                pair.Value.interactable = enabled;
                pair.Value.transform.Find("Availability").GetComponent<Text>().text = enabled ? "可执行" : "锁定";
                if (enabled) available++;
            }
            var activeDispositions = dispositions.Values.Where(b => b.gameObject.activeSelf && b.interactable).ToList();
            selectedDisposition = Mathf.Clamp(selectedDisposition, 0, Mathf.Max(0, available - 1));
            foreach (var pair in dispositions) TerminalIndustrialSkin.SetSelected(pair.Value, activeDispositions.Count > 0 && pair.Value == activeDispositions[selectedDisposition]);
            returnDirectory.gameObject.SetActive(archived && !dir && !view.LeavingTerminal);
            returnDirectory.interactable = !locked;
            answers.SetActive(true);
            bool live = !dir && !self && !archived && !view.LeavingTerminal;
            livePortrait.SetActive(live); silhouette.SetActive(live && (portraits == null || portraits.Find(portraitId) == null));
            cameraLabel.gameObject.SetActive(live);
            signalLostMessage.SetActive(archived);
            if (archived || view.LeavingTerminal || view.Cue?.Kind != PresentationKind.ExitTerminal) SetSignalDistortion(0);
            portraitPresenter.Paused = view.Paused;
            if (live) UpdatePortrait();
            bool tutorial = view.Game.Day == 0;
            supervisor.gameObject.SetActive(tutorial && live);
            if (tutorial) { livePortrait.SetActive(false); silhouette.SetActive(false); cameraLabel.gameObject.SetActive(false); }
            historyHeading.text = "对话 / " + round;
            RenderRoster(); RenderHistory();
            footer.interactable = !locked && !tutorial;
            footer.enabled = !tutorial;
            footer.GetComponent<Image>().raycastTarget = !tutorial;
            Footer(self ? "本人复核 / 确认净化" : dir ? enter.interactable ? "选择对象并确认，进入人员复核" : "当前对象不可再次接待" : archived ? "审查已结束 / 档案已关闭" : measure ? "会谈结束 / 选择处置命令并确认" : "按 Enter 或单击反白命令条继续",
                dir ? "↑↓ 选择对象   双击 / Enter 打开" : "↑↓ 选择   Enter / 单击 确认   1–4 应答");
            if (!dir && !self && ChoiceRoot.GetComponentsInChildren<Button>(false).Any(b => b.name.StartsWith("Reply-", StringComparison.Ordinal))) footerText.text = "选择应答并确认";
            Fit();
        }
        public void SetSelfMonitor(Texture texture) => selfMonitor.texture = texture;
        public bool HandleSelfReviewKey(KeyCode key) => selfReview.activeInHierarchy && !locked && (key == KeyCode.Return || key == KeyCode.KeypadEnter) && MiniGameAudioButton.Activate(selfPurify);
        public void SetLine(string speaker, string text, string portrait, string nodeId = null) { Speaker.text = speaker; Body.text = text; SetPortrait(portrait, nodeId); }
        public void SetPortrait(string portrait, string nodeId = null)
        {
            if (!string.IsNullOrEmpty(portrait)) portraitId = portrait;
            if (!string.IsNullOrEmpty(nodeId)) sourceNodeId = nodeId;
            UpdatePortrait();
        }
        private void UpdatePortrait()
        {
            Sprite sprite = portraits != null ? portraits.Find(portraitId) : null;
            portraitPresenter.Show(sprite);
            patientMonitor.SetCloseUp(false);
            patientMonitor.Show(current?.PresentedPatientId ?? current?.Game.CurrentPatientId, sprite, current != null && (current.Paused));
        }
        private void RestorePresentation(RuntimeView view)
        {
            var last = view.Transcript.LastOrDefault(e => !e.IsPlayer && !e.IsNarration);
            if (last != null) { Body.text = last.Text; Speaker.text = last.Speaker; sourceNodeId = last.SourceNodeId; portraitId = last.PortraitId; }
        }
        public void SetNarration(string text) { NarrationBody.text = text; NarrationBody.gameObject.SetActive(true); Position(Continue.GetComponent<RectTransform>(), 0, 119, 248, 28); }
        public void ClearNarration() { NarrationBody.gameObject.SetActive(false); Position(Continue.GetComponent<RectTransform>(), 0, 0, 248, 28); }
        public void SetPageIndicator(int page, int pages) => pageLabel.text = pages > 1 ? page + "/" + pages : "";
        public Button AddResponse(string id, string text, int index, Action action)
        {
            var button = Button(ChoiceRoot, "Reply-" + id, text, 0, index * 35, 248, 28, action, 14);
            var label = button.transform.Find("Label").GetComponent<Text>(); Position(label.rectTransform, 34, 5, 208, 21);
            var clip = Panel(button.transform, "ChoiceTextViewport", Color.clear, 34, 5, 208, 21); clip.gameObject.AddComponent<RectMask2D>();
            label.transform.SetParent(clip, false); Position(label.rectTransform, 0, 0, 208, 21);
            var number = Label(button.transform, "ChoiceNumber", (index + 1).ToString("00"), 7, 6, 25, 18, 13, Dim);
            TerminalIndustrialSkin.RegisterLabel(button, number);
            display.InvalidateSourceLayers(); return button;
        }
        public void SetSignalDistortion(float amount) => signalLoss.SetActive(amount > 0);
        public void AnimateSignal(float time)
        {
            if (current != null && !current.Paused && signalLoss.activeInHierarchy)
                for (int i = 0; i < stripes.Count; i++) stripes[i].anchoredPosition = new Vector2(0, -((7 + i * 11 + Mathf.FloorToInt(time * 25)) % 261));
            RenderHistory();
        }
        public void HighlightResponse(int index)
        {
            footerText.text = "选择应答并确认";
            int number = 0;
            foreach (Transform child in ChoiceRoot)
                if (child.name.StartsWith("Reply-", StringComparison.Ordinal) && child.gameObject.activeSelf) TerminalIndustrialSkin.SetSelected(child.GetComponent<Button>(), number++ == index);
        }
        public bool HandleDispositionKey(KeyCode key)
        {
            if (locked || current?.Game.EncounterStage != EncounterStage.AwaitingDisposition) return false;
            var active = dispositions.Values.Where(b => b.gameObject.activeInHierarchy && b.interactable).ToList();
            if (active.Count == 0) return false;
            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
            {
                selectedDisposition = (selectedDisposition + (key == KeyCode.UpArrow ? active.Count - 1 : 1)) % active.Count;
                for (int i = 0; i < active.Count; i++) TerminalIndustrialSkin.SetSelected(active[i], i == selectedDisposition);
                return true;
            }
            return (key == KeyCode.Return || key == KeyCode.KeypadEnter) && MiniGameAudioButton.Activate(active[Mathf.Clamp(selectedDisposition, 0, active.Count - 1)]);
        }
        public bool HandleDirectoryKey(KeyCode key)
        {
            if (!directory.activeSelf || locked) return false;
            if (key == KeyCode.UpArrow) SelectRoster(Mathf.Max(0, rosterIndex - 1));
            else if (key == KeyCode.DownArrow) SelectRoster(Mathf.Min(4, rosterIndex + 1));
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter) return MiniGameAudioButton.Activate(enter);
            else return false;
            return true;
        }
        public bool HandleArchiveKey(KeyCode key) => !locked && returnDirectory.gameObject.activeInHierarchy && (key == KeyCode.Return || key == KeyCode.KeypadEnter) && MiniGameAudioButton.Activate(returnDirectory);
        public void ScrollHistory(int amount) { if (current != null) transcriptView.Scroll(amount); }
        private void SelectRoster(int index) { if (locked) return; rosterIndex = index; RenderRoster(); }
        private void EnterSelected() { if (current == null || locked || !enter.interactable || rosterIndex >= current.Patients.Count) return; enterPatient(current.Patients[rosterIndex].Id); }
        private void RenderRoster()
        {
            if (current == null) return;
            for (int i = 0; i < rosterRows.Count; i++)
            {
                var p = i < current.Patients.Count ? current.Patients[i] : null; var labels = rosterLabels[i];
                labels[0].text = p?.Name ?? "未配置对象 " + (i + 1).ToString("00");
                labels[1].text = p == null ? "" : PortraitCatalog.Registration(p.Id);
                labels[2].text = p == null ? "--" : PortraitCatalog.Unit(p.Id);
                labels[3].text = p == null ? "未接入" : PatientStatus(p);
                rosterRows[i].interactable = !locked; TerminalIndustrialSkin.SetSelected(rosterRows[i], i == rosterIndex);
            }
            var selected = rosterIndex < current.Patients.Count ? current.Patients[rosterIndex] : null;
            bool available = selected != null && selected.CanReceiveToday && current.TerminalPage == TerminalPage.Directory;
            enter.interactable = available && !locked;
            previewName.text = selected?.Name ?? "未配置";
            previewId.text = selected == null ? "" : PortraitCatalog.Registration(selected.Id);
            previewStatus.text = selected == null ? "无登记数据" : PatientStatus(selected);
            previewUnit.text = "单元    " + (selected == null ? "--" : PortraitCatalog.Unit(selected.Id));
            quota.text = current.Game.RemainingPurificationQuota.ToString("00");
            enter.GetComponentInChildren<Text>().text = available ? "进入人员复核" : selected == null ? "尚未接入" : "档案已关闭";
            leave.interactable = !locked; day.text = "人员复核 / DAY " + current.Game.Day.ToString("00");
            count.text = "当日登记 " + current.Patients.Count.ToString("00") + " / 待审 " + current.Patients.Count(p => p.CanReceiveToday).ToString("00") + " / 已处置 " + current.Patients.Count(p => p.HasMetToday).ToString("00");
        }
        private void RenderHistory()
        {
            if (current == null) return;
            transcriptView.Render(current.Transcript, sourceNodeId, Body.text);
        }
        private void Footer(string text, string help) { footerText.text = text; footerHelp.text = help; }
        private Text Field(Transform parent, string label, string value, float y) { Label(parent, label + "Label", label, 24, y, 110, 20, 14, Ink); return Label(parent, "Preview" + label, value, 138, y, 440, 20, 14, Ink); }
        private static string PatientStatus(PatientView p) => p.HasMetToday || p.Presence != PatientPresence.Active ? p.LastDisposition.HasValue ? "已" + DispositionName(p.LastDisposition.Value) : "已归档" : p.CanReceiveToday ? "待审" : "待接待";
        private static string RoundName(string id) { if (string.IsNullOrEmpty(id)) return "R1"; if (id.IndexOf("_M_", StringComparison.Ordinal) >= 0) return "措施"; int start = id.IndexOf("_R", StringComparison.Ordinal); if (start < 0) return "R1"; int end = id.IndexOf('_', start + 1); return id.Substring(start + 1, (end < 0 ? id.Length : end) - start - 1); }
        private static string DispositionName(Disposition type) => type == Disposition.Release ? "释放" : type == Disposition.Treat ? "治疗" : "净化";
        private static Color ColorOf(string value) => TerminalIndustrialSkin.ColorOf(value);
        private static void Position(RectTransform r, float x, float y, float w, float h) => TerminalIndustrialSkin.Position(r, x, y, w, h);
        private static RectTransform Panel(Transform p, string name, Color color, float x, float y, float w, float h) => TerminalIndustrialSkin.Panel(p, name, color, x, y, w, h);
        private Text Label(Transform p, string name, string value, float x, float y, float w, float h, int size, Color color) => TerminalIndustrialSkin.Label(p, font, name, value, x, y, w, h, size, color);
        private Button Button(Transform p, string name, string value, float x, float y, float w, float h, Action action, int size = 14) => TerminalIndustrialSkin.Button(p, font, name, value, x, y, w, h, action, size, true);
        private static void Rule(Transform p, float x, float y, float width) => Panel(p, "Rule", Edge, x, y, width, 1);
        internal static void Border(Transform p, string name, float x, float y, float w, float h, Color color)
        { var r = Panel(p, name, Color.clear, x, y, w, h); Panel(r, "Top", color, 0, 0, w, 1); Panel(r, "Bottom", color, 0, h - 1, w, 1); Panel(r, "Left", color, 0, 0, 1, h); Panel(r, "Right", color, w - 1, 0, 1, h); }
        private Image Portrait(Transform p, string name, float x, float y, float w, float h) { var image = Panel(p, name, Color.white, x, y, w, h).GetComponent<Image>(); image.preserveAspect = true; return image; }
    }
}
