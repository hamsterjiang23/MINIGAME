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
        private readonly Text date, day, count, previewName, previewId, previewStage, previewStatus, previewUnit, quota;
        private readonly Text previewIntroduction, previewNoImage;
        private readonly Text previewAge, previewOccupation;
        private readonly TerminalDossierSummary dossierSummary;
        private readonly TerminalQuotaField dossierQuota;
        private readonly Image previewPortrait;
        private readonly Text recordName, monitorHeading, cameraLabel, roundLabel, footerText, footerHelp, pageLabel;
        private readonly Text directoryRequirement;
        private readonly TerminalQuotaField dailyWorkRemaining;
        private readonly TerminalAdaptationWaveform adaptation;
        private readonly Text dispositionInstructions;
        private readonly TerminalTranscriptView transcriptView;
        private readonly TerminalTranscriptView historyTranscript;
        private readonly GameObject historyModal;
        private readonly Button enter, leave, returnDirectory, selfPurify, footer;
        private readonly RawImage selfMonitor;
        private readonly RawImage supervisor;
        private readonly TerminalSignalEffect supervisorSignal;
        private readonly Texture2D openingLogo;
        private bool showingOpeningLogo;
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
        public bool HistoryOpen => historyModal != null && historyModal.activeSelf;
                public bool IsReview => current != null && current.TerminalPage == TerminalPage.Review;
        public Action ConfirmRequested;
        public TerminalDisplay Display => display;
        public RectTransform Logical => logical;
        public Font Typeface => font;

        public TerminalView(RectTransform parent, Font terminalFont, Action<string> enterSelected,
            Action returnDirectoryAction, Action leaveAction, Action<Disposition> dispositionAction,
            Action<InspectionKind> inspectionAction, Action selfPurifyAction = null, TerminalSignalController signals = null)
        {
            font = terminalFont; assets = Resources.Load<TerminalAssetSet>("MiniGame/TerminalAssets"); enterPatient = enterSelected; returnToDirectory = returnDirectoryAction;
            if (assets != null && assets.Typeface != null) font = assets.Typeface;
            portraits = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            logical = Panel(parent, "TerminalLogical800x600", Desk, 0, 0, 800, 600);
            logical.gameObject.AddComponent<TerminalHighlightGroup>();
            logical.anchorMin = logical.anchorMax = logical.pivot = new Vector2(.5f, .5f);
            logical.anchoredPosition = Vector2.zero;
            var packageBackdrop = new GameObject("PMS package frame", typeof(RectTransform), typeof(RawImage));
            packageBackdrop.transform.SetParent(logical, false); Position((RectTransform)packageBackdrop.transform, 0, 0, 800, 600);
            packageFrame = packageBackdrop.GetComponent<RawImage>();
            packageFrame.raycastTarget = false;
            Border(logical, "TerminalBorder", 8, 8, 784, 584, ColorOf("d9a339"));
            Rule(logical, 8, 36, 784);
            date = Label(logical, "TerminalDate", "", 22, 16, 160, 17, 13, Bright);
            Label(logical, "TerminalVersion", "PMS / 3.14", 270, 16, 260, 17, 13, Ink).alignment = TextAnchor.UpperCenter;
            directory = Panel(logical, "ObjectDirectory", Color.clear, 0, 0, 800, 548).gameObject;
            review = Panel(logical, "PersonnelReview", Color.clear, 0, 0, 800, 548).gameObject;
            selfReview = Panel(logical, "SelfReview", Color.clear, 0, 0, 800, 548).gameObject;
            var dr = (RectTransform)directory.transform;
            Label(dr, "RosterHeading", "第三安置所 / B区 / 观察对象", 23, 51, 490, 20, 14, Bright);
            day = Label(dr, "DirectoryDay", "", 597, 51, 180, 20, 13, Ink); day.alignment = TextAnchor.UpperRight;
            Label(dr, "RosterColumnLabels", "对象 / 登记编号", 29, 82, 480, 18, 13, Dim);
            Label(dr, "RosterStageColumn", "阶段", 490, 82, 104, 18, 13, Dim);
            Label(dr, "RosterUnitColumn", "单元", 610, 82, 88, 18, 13, Dim).alignment = TextAnchor.UpperRight;
            Label(dr, "RosterStatusColumn", "状态", 714, 82, 56, 18, 13, Dim).alignment = TextAnchor.UpperRight;
            for (int i = 0; i < 5; i++)
            {
                int slot = i;
                var row = Button(dr, "Roster-" + i, "", 22, 108 + 30 * i, 756, 27, () => { SelectRoster(slot); EnterSelected(); });
                var pointer = row.gameObject.AddComponent<TerminalRosterPointer>();
                pointer.Hovered = () => SelectRoster(slot);
                Label(row.transform, "RowNumber", (i + 1).ToString("00"), 8, 5, 28, 18, 12, Dim);
                var name = row.transform.Find("Label").GetComponent<Text>(); Position(name.rectTransform, 42, 4, 158, 20);
                var registration = Label(row.transform, "Registration", "", 200, 4, 240, 20, 14, Ink);
                var stage = Label(row.transform, "Stage", "", 468, 5, 104, 18, 12, Ink);
                var unit = Label(row.transform, "Unit", "", 588, 5, 88, 18, 12, Ink); unit.alignment = TextAnchor.UpperRight;
                var status = Label(row.transform, "Status", "", 692, 5, 56, 18, 12, Ink); status.alignment = TextAnchor.UpperRight;
                foreach (var label in new[] { registration, stage, unit, status, row.transform.Find("RowNumber").GetComponent<Text>() }) TerminalIndustrialSkin.RegisterLabel(row, label);
                rosterRows.Add(row); rosterLabels.Add(new[] { name, registration, unit, status, stage });
            }
            count = Label(dr, "RosterCount", "", 29, 283, 470, 20, 12, Dim);
            directoryRequirement = Label(dr, "DirectoryPurificationRequirement", "", 554, 283, 220, 20, 13, Bright);
            directoryRequirement.alignment = TextAnchor.UpperRight;
            var dossierPortrait = Panel(dr, "DirectoryPortraitPanel", Color.clear, 30, 322, 98, 130);
            previewPortrait = Portrait(dossierPortrait, "DirectoryPortrait", 1, 1, 96, 128);
            previewNoImage = Label(dossierPortrait, "DirectoryPortraitNoImage", "影像待载入", 1, 58, 96, 20, 12, Dim);
            previewNoImage.alignment = TextAnchor.UpperCenter;
            previewId = Label(dr, "PreviewId", "", 140, 322, 638, 24, 18, Bright);
            Label(dr, "DossierFacilityLabel", "终端", 140, 356, 70, 20, 14, Ink);
            Label(dr, "DossierFacility", "第三安置所 / B区", 212, 356, 190, 20, 14, Ink);
            Label(dr, "DossierSubjectLabel", "对象", 140, 380, 70, 20, 14, Ink);
            previewName = Label(dr, "Preview对象", "", 212, 380, 190, 20, 14, Ink);
            Label(dr, "DossierUnitLabel", "单元", 140, 404, 70, 20, 14, Ink);
            previewUnit = Label(dr, "PreviewUnit", "", 212, 404, 190, 20, 14, Ink);
            Label(dr, "状态Label", "档案状态", 140, 428, 70, 20, 14, Ink);
            previewStatus = Label(dr, "Preview状态", "", 212, 428, 32, 20, 14, Ink);
            Label(dr, "DossierStatusSeparator", "/", 249, 428, 14, 20, 14, Ink);
            previewStage = Label(dr, "PreviewStage", "", 270, 428, 132, 20, 14, Ink);
            Label(dr, "净化配额Label", "净化配额", 140, 452, 70, 20, 14, Ink);
            var quotaRoot = Panel(dr, "DossierQuota", ColorOf("eeb430"), 212, 448, 56, 24);
            quota = Label(quotaRoot, "PurificationQuota", "", 0, 0, 56, 24, 14, ColorOf("170e02"));
            quota.alignment = TextAnchor.MiddleCenter;
            dossierQuota = quotaRoot.gameObject.AddComponent<TerminalQuotaField>();
            // Draw live digits with the terminal pixel font instead of the pre-rendered smooth number.
            dossierQuota.Initialize(null, null, quota, new Vector2Int(56, 24));
            Label(dr, "DossierAgeLabel", "年龄", 418, 354, 70, 20, 14, Ink);
            previewAge = Label(dr, "DossierAge", "", 499, 354, 270, 20, 14, ColorOf("170e02"));
            Label(dr, "DossierOccupationLabel", "职业", 418, 388, 70, 20, 14, Ink);
            previewOccupation = Label(dr, "DossierOccupation", "", 499, 388, 270, 20, 14, ColorOf("170e02"));
            Label(dr, "PreviewIntroductionLabel", "登记摘要", 418, 422, 70, 20, 14, Ink);
            var summaryRoot = Panel(dr, "DossierSummary", Color.clear, 490, 416, 288, 56);
            summaryRoot.GetComponent<Image>().raycastTarget = true;
            previewIntroduction = Label(summaryRoot, "PreviewIntroduction", "", 9, 6, 270, 44, 14, ColorOf("170e02"));
            dossierSummary = summaryRoot.gameObject.AddComponent<TerminalDossierSummary>();
            dossierSummary.Initialize(previewIntroduction, font);
            Label(dr, "DossierCommandLabel", "当前命令", 24, 492, 90, 20, 14, Ink);
            enter = Button(dr, "EnterReview", "进入人员复核", 118, 486, 660, 26, EnterSelected, 14);
            ((TerminalButtonVisual)enter).SetCommandField();
            Position(enter.transform.Find("Label").GetComponent<RectTransform>(), 10, 6, 640, 20);
            leave = Button(dr, "LeaveTerminal", "退出终端返回房间", 590, 518, 188, 24, leaveAction, 13);

            var rr = (RectTransform)review.transform;
            recordName = Label(rr, "RecordRegistration", "", 26, 51, 470, 20, 13, Ink);
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
            patientMonitor.VisibilityChanged += showing => portraitPresenter.SetVisible(!showing);
            signals?.Attach(feed.GetComponent<RawImage>());
            cameraLabel = Label(video, "CameraLabel", "", 7, 8, 280, 16, 10, Dim);
            var supervisorObject = new GameObject("Day0SupervisorFeed", typeof(RectTransform), typeof(RawImage));
            supervisorObject.transform.SetParent(video, false); Position((RectTransform)supervisorObject.transform, 0, 0, 464, 261);
            supervisor = supervisorObject.GetComponent<RawImage>(); supervisor.raycastTarget = false;
            supervisorSignal = signals?.Attach(supervisor);
            openingLogo = Resources.Load<Texture2D>("MiniGame/NormLogo");
            // Keep the legacy object name for signal controls and opening captures.
            // Only the opening logo uses this overlay; the tutorial uses the patient feed.
            supervisor.texture = openingLogo;
            supervisorObject.SetActive(false);
            signalLoss = Panel(video, "MonitorSignalLoss", ColorOf("0e0b04"), 0, 0, 464, 261).gameObject;
            for (int i = 0; i < 22; i++) stripes.Add(Panel(signalLoss.transform, "Noise" + i, i % 3 == 0 ? Ink : Edge, 0, 7 + i * 11, 464, i % 2 == 0 ? 2 : 1));
            signalLostMessage = Label(video, "SignalLostMessage", "SIGNAL LOST", 176, 123, 200, 25, 13, Dim).gameObject;
            var instrument = Panel(rr, "SocialAdaptationInstrument", Desk, 26, 362, 472, 64);
            Label(instrument, "SocialAdaptationHeading", "社会适应度", 0, 0, 145, 17, 13, Ink);
            var plot = new GameObject("SocialAdaptationWaveform", typeof(RectTransform), typeof(TerminalAdaptationWaveform));
            plot.transform.SetParent(instrument, false);
            Position((RectTransform)plot.transform, 0, 18, 472, 44);
            adaptation = plot.GetComponent<TerminalAdaptationWaveform>(); adaptation.Initialize();
            historyHeading = Label(rr, "DialogueHeading", "对话", 26, 431, 470, 20, 13, Dim);
            Rule(rr, 22, 451, 480);
            transcriptView = new TerminalTranscriptView(rr, font, signals, y: 456, height: 76);
            history = transcriptView.GameObject;
            roundLabel = Label(rr, "RecordNode", "人员复核 / R1", 534, 83, 124, 19, 13, Dim);
            roundLabel.gameObject.SetActive(false);
            Label(rr, "DailyWorkIndicatorLabel", "当日指标 / 净化剩余", 534, 64, 168, 20, 12, Bright);
            var quotaField = Panel(rr, "DailyWorkQuotaField", Bright, 712, 52, 66, 32);
            var quotaFallback = Label(quotaField, "DailyWorkIndicatorRemaining", "", 0, 0, 66, 32, 24, ColorOf("170e02"));
            quotaFallback.alignment = TextAnchor.MiddleCenter;
            dailyWorkRemaining = quotaField.gameObject.AddComponent<TerminalQuotaField>();
            dailyWorkRemaining.Initialize(assets, quotaFallback);
            Label(rr, "DispositionHeading", "处置命令", 534, 128, 240, 19, 13, Dim); Rule(rr, 530, 150, 248);
            var types = new[] { Disposition.Release, Disposition.Treat, Disposition.Purify };
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var button = Button(rr, "Disposition-" + type, DispositionName(type), 530, 164 + i * 64, 248, 44, () => dispositionAction(type));
                var visual = (TerminalButtonVisual)button;
                visual.SetCommandArtwork(type == Disposition.Release ? assets.ReleaseCommand
                    : type == Disposition.Treat ? assets.TreatmentCommand : assets.PurificationCommand);
                dispositions.Add(type, button);
                visual.PointerSelected = () => SelectDisposition(button);
            }
            Rule(rr, 530, 362, 248); Label(rr, "AnswerHeading", "应答命令", 534, 374, 240, 19, 13, Dim);
            answers = Panel(rr, "AnswerContents", Color.clear, 530, 402, 248, 134).gameObject;
            ChoiceRoot = (RectTransform)answers.transform;
            // Keep response selection visible when the pointer rests on another panel's commands.
            ChoiceRoot.gameObject.AddComponent<TerminalHighlightGroup>();
            dispositionInstructions = Label(ChoiceRoot, "DispositionInstructions", "会谈结束。请用鼠标点击上方处置命令。", 6, 8, 236, 76, 14, Dim);
            dispositionInstructions.horizontalOverflow = HorizontalWrapMode.Wrap;
            dispositionInstructions.gameObject.SetActive(false);
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
            signals?.Attach(selfMonitor);
            Label(sr, "SelfCameraLabel", "CAM 07 / LIVE", 37, 100, 300, 20, 10, Dim);
            Label(sr, "SelfStatus", "本人复核 / 处置待确认", 534, 112, 240, 70, 14, Ink);
            selfPurify = Button(sr, "SelfPurify", "01   净化", 530, 204, 248, 28, selfPurifyAction);

            footer = Button(logical, "TerminalCommandStrip", "", 9, 548, 782, 43, () => ConfirmRequested?.Invoke());
            ((TerminalButtonVisual)footer).SetAlwaysHighlighted();
            footerText = footer.transform.Find("Label").GetComponent<Text>(); Position(footerText.rectTransform, 5, 6, 772, 20); footerText.alignment = TextAnchor.UpperCenter;
            footerHelp = Label(footer.transform, "TerminalHelp", "", 5, 26, 772, 16, 12, Desk); footerHelp.alignment = TextAnchor.UpperCenter;
            TerminalIndustrialSkin.RegisterLabel(footer, footerHelp); TerminalIndustrialSkin.SetSelected(footer, true);
            historyModal = Panel(logical, "DialogueHistoryOverlay", new Color(0, 0, 0, .8f), 0, 0, 800, 600).gameObject;
            historyModal.GetComponent<Image>().raycastTarget = true;
            var historyWindow = Panel(historyModal.transform, "DialogueHistoryWindow", Desk, 40, 55, 720, 480);
            Border(historyWindow, "HistoryBorder", 0, 0, 720, 480, Edge);
            Label(historyWindow, "HistoryTitle", "对话记录 / 当前检查期", 17, 15, 660, 24, 14, Bright);
            historyTranscript = new TerminalTranscriptView(historyWindow, font, signals, "SavedDialogueRecords", 17, 48, 686, 372);
            Button(historyWindow, "CloseHistory", "关闭记录  ESC / F3", 480, 435, 220, 28, () => CloseHistory(), 13);
            historyModal.SetActive(false);
            Continue.gameObject.SetActive(false); returnDirectory.gameObject.SetActive(false); NarrationBody.gameObject.SetActive(false);
            display = parent.gameObject.AddComponent<TerminalDisplay>(); display.Initialize(logical);
            Fit();
        }

        public void Fit()
        {
            display.Fit();
            if (showingOpeningLogo) FitOpeningLogo();
        }
        public void SetOpeningLogo(bool visible)
        {
            showingOpeningLogo = visible;
            supervisor.texture = openingLogo;
            supervisorSignal?.SetLogoMode(visible);
            supervisor.gameObject.SetActive(visible);
            if (visible)
            {
                livePortrait.SetActive(false);
                silhouette.SetActive(false);
                cameraLabel.gameObject.SetActive(false);
                FitOpeningLogo();
            }
            else
            {
                Position(supervisor.rectTransform, 0, 0, 464, 261);
                if (current != null) Render(current, locked);
            }
        }
        private void FitOpeningLogo()
        {
            if (openingLogo == null) return;
            var viewport = ((RectTransform)display.transform).rect;
            float aspect = viewport.width / Mathf.Max(1, viewport.height);
            float visibleWidth = Mathf.Min(464, 261 * aspect);
            float width = .82f * Mathf.Min(visibleWidth, visibleWidth / Mathf.Max(.01f, aspect) * openingLogo.width / openingLogo.height);
            float height = width * openingLogo.height / openingLogo.width;
            if (Mathf.Approximately(supervisor.rectTransform.rect.width, width)
                && Mathf.Approximately(supervisor.rectTransform.rect.height, height)) return;
            Position(supervisor.rectTransform, (464 - width) * .5f, (261 - height) * .5f, width, height);
        }
        public void ResetNavigation(RuntimeView view)
        {
            CloseHistory();
            current = view; rosterIndex = 0;
            transcriptView.Reset();
            dossierSummary.ResetScroll();
            sourceNodeId = ""; Body.text = ""; portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId ?? view.Game.CurrentPatientId);
            ClearNarration(); RestorePresentation(view); SetSignalDistortion(0); Render(view, false);
        }
        public void Render(RuntimeView view, bool inputLocked)
        {
            bool nextEncounter = current == null || current.PresentedEncounterId != view.PresentedEncounterId;
            bool enteringDirectory = view.TerminalPage == TerminalPage.Directory && current?.TerminalPage != TerminalPage.Directory;
            bool reopeningTerminal = view.TerminalOpen && current?.TerminalOpen != true;
            current = view; locked = inputLocked;
            date.text = view.CalendarDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            if (nextEncounter) { sourceNodeId = ""; Body.text = ""; portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId); transcriptView.Reset(); RestorePresentation(view); }
            if (enteringDirectory) { int next = view.Patients.ToList().FindIndex(p => p.IsCurrent); if (next >= 0) rosterIndex = next; }
            bool self = false; // Self judgment uses the ordinary directory and personnel review.
            bool dir = view.TerminalPage == TerminalPage.Directory || view.ArchiveDirectoryVisible
                || view.TerminalPage == TerminalPage.Closed && view.TerminalPowered && view.UsesTerminalDirectory;
            packageFrame.texture = assets != null ? (dir ? assets.DirectoryFrame : assets.ReviewFrame) : null;
            directory.SetActive(dir && !self); review.SetActive(!dir && !self); selfReview.SetActive(self);
            bool archived = view.TerminalPage == TerminalPage.Archive;
            if (!view.TerminalOpen || dir || self || view.LeavingTerminal) CloseHistory();
            selfPurify.interactable = self && !locked && view.Cue == null && !view.Paused;
            string id = view.PresentedPatientId ?? view.Game.CurrentPatientId;
            recordName.text = PortraitCatalog.Registration(id);
            monitorHeading.text = "监控 / " + PortraitCatalog.Unit(id);
            cameraLabel.text = "CAM " + PortraitCatalog.Unit(id).Replace("-", "") + "-01";
            string round = RoundName(sourceNodeId);
            roundLabel.text = "人员复核 / " + round;
            directoryRequirement.text = "净化要求：" + view.Game.PurificationsToday + "/" + view.Game.PurificationRequirement;
            dailyWorkRemaining.SetValue(view.IsSelfReview ? 1 : view.Game.RemainingPurificationQuota);
            bool measure = (view.Game.EncounterStage == EncounterStage.AwaitingDisposition || view.SelfReviewReady) && !archived;
            RenderDispositionCommands();
            returnDirectory.gameObject.SetActive(archived && !dir && !view.LeavingTerminal);
            returnDirectory.interactable = !locked;
            answers.SetActive(true);
            bool live = !dir && !self && !archived && !view.LeavingTerminal;
            bool measuredPatient = live && !view.IsSelfReview && !showingOpeningLogo
                && view.PresentedEncounterId == view.Game.CurrentEncounterId
                && id == view.Game.CurrentPatientId;
            adaptation.SetReading(measuredPatient ? view.Game.SocialAdaptation : null,
                view.PresentedEncounterId, view.Paused || locked || !view.TerminalOpen);
            bool showPatient = live && !showingOpeningLogo;
            livePortrait.SetActive(showPatient); silhouette.SetActive(showPatient && (portraits == null || portraits.Find(portraitId) == null));
            cameraLabel.gameObject.SetActive(showPatient);
            signalLostMessage.SetActive(archived);
            if (archived || view.LeavingTerminal || view.Cue?.Kind != PresentationKind.ExitTerminal) SetSignalDistortion(0);
            portraitPresenter.Paused = view.Paused;
            if (showPatient) UpdatePortrait();
            bool tutorial = view.Game.Day == 0;
            supervisor.gameObject.SetActive(showingOpeningLogo && live);
            historyHeading.text = "对话";
            if (enteringDirectory || reopeningTerminal) dossierSummary.ResetScroll();
            RenderRoster(); RenderHistory();
            footer.interactable = !locked && !tutorial;
            footer.enabled = !tutorial;
            footer.GetComponent<Image>().raycastTarget = !tutorial;
            Footer(self ? "本人复核 / 确认净化" : dir ? enter.interactable ? "选择对象并确认，进入人员复核" : "当前对象不可再次接待" : archived ? "审查已结束 / 档案已关闭" : measure ? "会谈结束 / 单击处置命令执行" : "按 Enter 或单击反白命令条继续",
                dir ? "↑↓ 选择对象   单击 / Enter 打开" : measure ? "仅鼠标左键执行处置" : "↑↓ 选择   Enter / 单击 确认   1–4 应答");
            if (!dir && !self && !measure && ChoiceRoot.GetComponentsInChildren<Button>(false).Any(b => b.name.StartsWith("Reply-", StringComparison.Ordinal))) footerText.text = "选择应答并确认";
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
            silhouette.SetActive(livePortrait.activeSelf && sprite == null);
            portraitPresenter.Show(sprite);
            patientMonitor.SetCloseUp(false);
            patientMonitor.Show(current?.PresentedPatientId ?? current?.Game.CurrentPatientId, sprite, current != null && (current.Paused));
            portraitPresenter.SetVisible(!patientMonitor.IsShowing);
        }
        private void RestorePresentation(RuntimeView view)
        {
            var last = view.Transcript.LastOrDefault(e => !e.IsPlayer && !e.IsNarration);
            if (last != null)
            {
                Body.text = last.Text; Speaker.text = last.Speaker; sourceNodeId = last.SourceNodeId;
                if (!string.IsNullOrEmpty(last.PortraitId)) portraitId = last.PortraitId;
            }
        }
        public void SetNarration(string text) { NarrationBody.text = text; NarrationBody.gameObject.SetActive(true); Position(Continue.GetComponent<RectTransform>(), 0, 105, 248, 28); }
        public void ClearNarration() { NarrationBody.gameObject.SetActive(false); Position(Continue.GetComponent<RectTransform>(), 0, 0, 248, 28); }
        public void SetPageIndicator(int page, int pages) => pageLabel.text = pages > 1 ? page + "/" + pages : "";
        public Button AddResponse(string id, string text, int index, Action action, Action hovered = null)
        {
            var button = Button(ChoiceRoot, "Reply-" + id, text, 0, index * 35, 248, 28, action, 14);
            ((TerminalButtonVisual)button).PointerSelected = hovered;
            var label = button.transform.Find("Label").GetComponent<Text>(); Position(label.rectTransform, 42, 4, 200, 21);
            // Numbering has its own label; the complete response is an ordinary single text run.
            if (label is TerminalBitmapText bitmap) bitmap.CommandRow = false;
            var clip = Panel(button.transform, "ChoiceTextViewport", Color.clear, 42, 4, 200, 21); clip.gameObject.AddComponent<RectMask2D>();
            var content = Panel(clip, "ChoiceTextContent", Color.clear, 0, 0, Mathf.Max(200, label.preferredWidth), 21);
            label.transform.SetParent(content, false); Position(label.rectTransform, 0, 0, content.rect.width, 21);
            button.gameObject.AddComponent<TerminalChoiceMarquee>().Initialize(button, clip, content, label);
            var number = Label(button.transform, "ChoiceNumber", (index + 1).ToString("00"), 8, 5, 28, 18, 12, Dim);
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
        public void HighlightResponse(int index, bool keyboard = true)
        {
            if (keyboard) TerminalHighlightGroup.ClearPointerHighlight(logical);
            footerText.text = "选择应答并确认";
            int number = 0;
            foreach (Transform child in ChoiceRoot)
                if (child.name.StartsWith("Reply-", StringComparison.Ordinal) && child.gameObject.activeSelf) TerminalIndustrialSkin.SetSelected(child.GetComponent<Button>(), number++ == index);
        }
        public bool HandleDispositionKey(KeyCode key)
        {
            if (locked || current == null || current.Game.EncounterStage != EncounterStage.AwaitingDisposition && !current.SelfReviewReady) return false;
            if (key == KeyCode.Return || key == KeyCode.KeypadEnter || key == KeyCode.Space) return true;
            var active = dispositions.Values.Where(b => b.gameObject.activeInHierarchy && b.interactable).ToList();
            if (active.Count == 0) return false;
            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
            {
                TerminalHighlightGroup.ClearPointerHighlight(logical);
                selectedDisposition = (selectedDisposition + (key == KeyCode.UpArrow ? active.Count - 1 : 1)) % active.Count;
                for (int i = 0; i < active.Count; i++) TerminalIndustrialSkin.SetSelected(active[i], i == selectedDisposition);
                return true;
            }
            return false;
        }
        public bool HandleDirectoryKey(KeyCode key)
        {
            if (!directory.activeSelf || locked) return false;
            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow) TerminalHighlightGroup.ClearPointerHighlight(logical);
            if (key == KeyCode.UpArrow) SelectRoster(Mathf.Max(0, rosterIndex - 1));
            else if (key == KeyCode.DownArrow) SelectRoster(rosterIndex + 1);
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter) return MiniGameAudioButton.Activate(enter);
            else return false;
            return true;
        }
        public bool HandleArchiveKey(KeyCode key) => !locked && returnDirectory.gameObject.activeInHierarchy && (key == KeyCode.Return || key == KeyCode.KeypadEnter) && MiniGameAudioButton.Activate(returnDirectory);
        public void ScrollHistory(int amount) { if (current != null) transcriptView.Scroll(amount); }
        public bool OpenHistory()
        {
            if (locked || current == null || !current.TerminalOpen
                || current.TerminalPage != TerminalPage.Review && current.TerminalPage != TerminalPage.Archive) return false;
            historyTranscript.Reset();
            historyModal.SetActive(true); RenderHistory(); RenderDispositionCommands();
            return true;
        }
        public bool CloseHistory()
        {
            if (!HistoryOpen) return false;
            historyModal.SetActive(false); RenderDispositionCommands(); return true;
        }
        public void ScrollSavedHistory(int amount) => historyTranscript.Scroll(amount);
        private void RenderDispositionCommands()
        {
            if (current == null) return;
            bool measure = (current.Game.EncounterStage == EncounterStage.AwaitingDisposition || current.SelfReviewReady)
                && current.TerminalPage != TerminalPage.Archive;
            dispositionInstructions.gameObject.SetActive(measure && !HistoryOpen);
            foreach (var pair in dispositions)
            {
                bool configured = !current.UsesTerminalDirectory || current.ConfiguredDispositions.Contains(pair.Key);
                pair.Value.gameObject.SetActive(configured);
                pair.Value.interactable = configured && measure && !locked && !HistoryOpen && !current.Paused
                    && current.Input == InputContext.Dialogue
                    && (current.IsSelfReview ? pair.Key == Disposition.Purify && current.SelfReviewReady
                        : current.Game.AvailableDispositions.Contains(pair.Key));
            }
            var active = dispositions.Values.Where(b => b.gameObject.activeSelf && b.interactable).ToList();
            selectedDisposition = Mathf.Clamp(selectedDisposition, 0, Mathf.Max(0, active.Count - 1));
            foreach (var button in dispositions.Values)
                TerminalIndustrialSkin.SetSelected(button, active.Count > 0 && button == active[selectedDisposition]);
        }
        private void SelectDisposition(Button button)
        {
            var active = dispositions.Values.Where(value => value.gameObject.activeSelf && value.interactable).ToList();
            int index = active.IndexOf(button);
            if (index < 0) return;
            selectedDisposition = index;
            foreach (var value in dispositions.Values) TerminalIndustrialSkin.SetSelected(value, value == button);
        }
        private void SelectRoster(int index)
        {
            if (locked || current == null || current.Patients.Count == 0) return;
            index = Mathf.Clamp(index, 0, current.Patients.Count - 1);
            if (rosterIndex == index) return;
            rosterIndex = index; RenderRoster();
        }
        private void EnterSelected() { if (current == null || locked || !enter.interactable || rosterIndex >= current.Patients.Count) return; enterPatient(current.Patients[rosterIndex].Id); }
        private void RenderRoster()
        {
            if (current == null) return;
            rosterIndex = Mathf.Clamp(rosterIndex, 0, Mathf.Max(0, current.Patients.Count - 1));
            for (int i = 0; i < rosterRows.Count; i++)
            {
                var p = i < current.Patients.Count ? current.Patients[i] : null; var labels = rosterLabels[i];
                rosterRows[i].gameObject.SetActive(p != null);
                if (p == null) continue;
                labels[0].text = p.Name;
                labels[1].text = PortraitCatalog.Registration(p.Id);
                labels[2].text = PortraitCatalog.Unit(p.Id);
                labels[3].text = PatientStatus(p);
                labels[4].text = PatientStage(p);
                rosterRows[i].interactable = !locked; TerminalIndustrialSkin.SetSelected(rosterRows[i], i == rosterIndex);
            }
            var selected = rosterIndex < current.Patients.Count ? current.Patients[rosterIndex] : null;
            bool available = selected != null && selected.CanReceiveToday && current.TerminalPage == TerminalPage.Directory;
            enter.interactable = available && !locked;
            var dossier = assets != null && assets.Dossiers != null ? assets.Dossiers.Find(selected?.Id) : null;
            previewName.text = selected == null ? "未配置" : selected.Name + " / " + PortraitCatalog.Registration(selected.Id);
            previewId.text = "对象档案 / " + (selected == null ? "未配置" : PortraitCatalog.Registration(selected.Id));
            bool reviewingSelf = selected?.Id == GameRuntime.SelfPatientId;
            previewAge.text = selected == null ? "--" : reviewingSelf ? "25岁" : dossier?.age ?? "未登记";
            previewOccupation.text = selected == null ? "--" : reviewingSelf ? "审查员" : dossier?.occupation ?? "未登记";
            dossierSummary.SetContent(selected?.Id, dossier?.summary ?? PublicIntroduction(selected?.Id));
            dossierSummary.SetInputEnabled(!locked && !current.Paused && current.TerminalOpen);
            var dossierPortrait = dossier?.portrait;
            previewPortrait.sprite = selected == null ? null : dossierPortrait != null ? dossierPortrait
                : portraits == null ? null : portraits.Find(PortraitCatalog.BasePortrait(selected.Id));
            previewPortrait.enabled = previewPortrait.sprite != null;
            previewNoImage.gameObject.SetActive(previewPortrait.sprite == null);
            previewStatus.text = selected == null ? "--" : PatientStatus(selected);
            previewStage.text = selected == null ? "--" : PatientStage(selected);
            previewUnit.text = selected == null ? "--" : PortraitCatalog.Unit(selected.Id);
            dossierQuota.SetValue(reviewingSelf ? 1 : current.Game.RemainingPurificationQuota);
            enter.GetComponentInChildren<Text>().text = available ? "进入人员复核" : selected == null ? "尚未接入" : "档案已关闭";
            leave.interactable = !locked; day.text = "人员复核 / DAY " + current.Game.Day.ToString("00");
            count.text = "当日登记 " + current.Patients.Count.ToString("00") + " / 待审 " + current.Patients.Count(p => !p.HasMetToday).ToString("00") + " / 已审 " + current.Patients.Count(p => p.HasMetToday).ToString("00");
        }
        private void RenderHistory()
        {
            if (current == null) return;
            transcriptView.Render(current.Transcript, sourceNodeId, Body.text);
            if (HistoryOpen) historyTranscript.Render(current.Transcript, sourceNodeId, Body.text);
        }
        private void Footer(string text, string help) { footerText.text = text; footerHelp.text = help; }
        private static string PublicIntroduction(string patientId)
        {
            switch (patientId)
            {
                case "former-ruler": return "薇拉·沃斯，前执政委员会成员。要求核验身份，并对收容记录中的判断提出异议。";
                case "battlefield-doctor": return "埃德温，战场医生。曾在前线救治伤员，关注治疗的具体方法与风险。";
                case "child": return "米洛，年幼的收容者。面对审查显得紧张，反复确认自己的回答是否正确。";
                case "priest": return "乔纳，神职人员。谈话常围绕信仰与赎罪，明确表达了接受净化的愿望。";
                case "diplomat": return "阿德里安，以贸易代表身份登记。来访事项涉及工业制品采购与交付，因例行检测异常被送入安置所。";
                case "tutorial": return "身份尚未登记的收容者。等待本次审查。";
                case "self-review": return "第三安置所值班审查员。本人复核记录。";
                default: return "暂无公开档案。";
            }
        }
        private static string PatientStatus(PatientView p) => p.HasMetToday ? "已审" : "待审";
        private static string PatientStage(PatientView p) => p.Presence == PatientPresence.Purified ? "已净化"
            : p.Presence == PatientPresence.Released ? "已释放"
            : p.TreatmentCount > 0 ? "治疗" + p.TreatmentCount + "次" : "未处理";
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
