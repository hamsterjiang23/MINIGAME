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
        private static readonly Color Pale = ColorOf("e0e3da");
        private readonly Font font;
        private readonly RectTransform viewport;
        private readonly RectTransform logical;
        private readonly GameObject directory;
        private readonly GameObject review;
        private readonly GameObject selfReview;
        private readonly RawImage selfMonitor;
        private readonly Button selfPurify;
        private readonly GameObject answers;
        private readonly GameObject history;
        private readonly Text breadcrumb;
        private readonly Text previewName, previewId, previewUnit, previewStatus, previewBatch, previewRegistration;
        private readonly GameObject previewPortrait, previewNoData;
        private readonly Text rosterCount;
        private readonly Text recordName, recordStatus, recordPortrait, recordNode, monitorPortrait;
        private readonly Text recordRegistration, recordUnit, monitorHeading, answerHeading, cameraLabel, pageLabel;
        private readonly PortraitCatalog portraits;
        private readonly TerminalPortraitPresenter portraitPresenter;
        private readonly GameObject livePortrait;
        private readonly Image previewSprite;
        private readonly List<Image> rosterSprites = new List<Image>();
        private readonly List<GameObject> rosterPlaceholders = new List<GameObject>();
        private readonly Text quota;
        private readonly Text signal, audioSignal;
        private readonly Text[] historySpeakers = new Text[5];
        private readonly Text[] historyRows = new Text[5];
        private readonly RectTransform historyTrack, historyThumb;
        private readonly Image lamp;
        private readonly GameObject lampGlint;
        private readonly GameObject silhouette;
        private readonly GameObject signalLoss, signalLostMessage;
        private readonly List<RectTransform> signalStripes = new List<RectTransform>();
        private readonly Button enter;
        private readonly Button returnDirectory;
        private readonly Button leave;
        private readonly Button answersTab;
        private readonly Button historyTab;
        private readonly List<Button> rosterRows = new List<Button>();
        private readonly List<Text[]> rosterLabels = new List<Text[]>();
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
        private string sourceNodeId = string.Empty;

        public RectTransform ChoiceRoot { get; }
        public Text Speaker { get; }
        public Text Body { get; }
        public Text NarrationBody { get; }
        public Button Continue { get; }
        public bool HistoryOpen => historyOpen;
        public bool IsReview => current != null && current.TerminalPage == TerminalPage.Review;
        public Action HistoryChanged;

        public TerminalView(RectTransform parent, Font terminalFont, Action<string> enterSelected,
            Action returnDirectoryAction, Action leaveAction, Action<Disposition> dispositionAction,
            Action<InspectionKind> inspectionAction, Action selfPurifyAction = null)
        {
            font = terminalFont;
            portraits = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            viewport = parent;
            enterPatient = enterSelected;
            returnToDirectory = returnDirectoryAction;
            leaveTerminal = leaveAction;
            logical = Panel(parent, "TerminalLogical640x360", Desk, 0, 0, 640, 360);
            logical.anchorMin = logical.anchorMax = logical.pivot = new Vector2(.5f, .5f);
            logical.anchoredPosition = Vector2.zero;
            Frame(logical, "TitleBar", "人员监控与复核终端", 3, 3, 634, 24, true);
            Label(logical, "TerminalVersion", "PMS / 3.14", 194, 8, 150, 14, 11, Pale);
            Label(logical, "TerminalNumber", "T-06  047  1994-11-08", 460, 9, 173, 14, 10, Pale);
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
            Label(dr, "RosterColumnLabels", "姓名 / 登记编号", 13, 72, 220, 14, 11, Ink);
            Label(dr, "RosterUnitColumn", "单元", 251, 72, 40, 14, 11, Ink);
            Label(dr, "RosterStatusColumn", "状态", 298, 72, 40, 14, 11, Ink);
            for (int index = 0; index < 5; index++)
            {
                int slot = index;
                var row = Button(dr, "Roster-" + index, "", 7, 86 + index * 45, 342, 43, () => SelectRoster(slot), 12, true);
                Panel(row.transform, "RegistrationEdge", ColorOf("819184"), 1, 2, 2, 39);
                Panel(row.transform, "RegistrationInset", ColorOf("e6e8df"), 4, 2, 1, 39);
                var photo = Panel(row.transform, "RosterPortrait", ColorOf("939e96"), 6, 5, 30, 32);
                Panel(photo, "Head", ColorOf("c4c9c0"), 11, 7, 9, 11);
                Panel(photo, "Shoulders", ColorOf("c5c8bf"), 7, 19, 17, 11);
                rosterPlaceholders.Add(photo.gameObject);
                rosterSprites.Add(PortraitImage(row.transform, "RosterPortraitSprite", 6, 5, 30, 32));
                var name = row.transform.Find("Label").GetComponent<Text>();
                Position(name.rectTransform, 46, 6, 195, 15);
                name.alignment = TextAnchor.UpperLeft;
                var labels = new[] { name,
                    Label(row.transform, "Registration", "", 46, 24, 195, 14, 10, Dim),
                    Label(row.transform, "Unit", "", 247, 18, 38, 14, 11, Ink),
                    Label(row.transform, "Status", "", 286, 18, 55, 14, 11, Ink) };
                foreach (var label in labels.Skip(1)) TerminalIndustrialSkin.RegisterLabel(row, label);
                rosterLabels.Add(labels);
                var pointer = row.gameObject.AddComponent<TerminalRosterPointer>();
                pointer.DoubleClicked = () => { SelectRoster(slot); EnterSelected(); };
                rosterRows.Add(row);
            }
            Panel(dr, "RosterCountBar", ColorOf("aeb7ae"), 7, 320, 342, 16);
            rosterCount = Label(dr, "RosterCount", "", 13, 322, 332, 14, 10, Dim);
            Bevel(dr, "RegistrationPortrait", 366, 73, 107, 127, Field, true);
            var mug = Panel(dr, "RegistrationSilhouette", ColorOf("969f98"), 384, 92, 70, 90);
            previewPortrait = mug.gameObject;
            previewSprite = PortraitImage(dr, "RegistrationPortraitSprite", 371, 79, 97, 115);
            Panel(mug, "Head", ColorOf("bbc0b7"), 21, 15, 28, 31);
            Panel(mug, "Shoulders", ColorOf("b7beb5"), 10, 47, 51, 40);
            previewNoData = Label(dr, "NoRegistrationData", "NO DATA", 382, 120, 88, 16, 11, Dim).gameObject;
            previewName = Label(dr, "PreviewName", "", 487, 81, 140, 18, 13, Ink);
            previewId = Label(dr, "PreviewRegistrationId", "", 486, 106, 140, 14, 10, Dim);
            previewUnit = Label(dr, "PreviewUnit", "", 486, 126, 140, 16, 11, Ink);
            previewStatus = Label(dr, "PreviewStatus", "", 486, 145, 140, 16, 11, Ink);
            Panel(dr, "PreviewRule", ColorOf("69756b"), 366, 214, 261, 1);
            previewBatch = Label(dr, "PreviewBatch", "", 368, 222, 255, 17, 11, Ink);
            previewRegistration = Label(dr, "PreviewRegistration", "", 368, 242, 255, 17, 11, Ink);
            enter = Button(dr, "EnterReview", "进入审查", 395, 293, 205, 30, EnterSelected);
            // The HTML is standalone; the game retains an explicit exit for the accepted room transition.
            leave = Button(dr, "LeaveTerminal", "返回房间", 395, 325, 205, 14, () => leaveTerminal(), 9);

            var rr = review.GetComponent<RectTransform>();
            Frame(rr, "IndividualRecordFrame", "个体记录", 3, 50, 146, 180);
            Frame(rr, "MonitorFrame", "实时监控 / B-12", 153, 50, 361, 180);
            monitorHeading = rr.Find("MonitorFrame/Heading").GetComponent<Text>();
            Frame(rr, "VerificationFrame", "核验结果", 518, 50, 119, 83);
            Frame(rr, "DispositionFrame", "处置", 518, 137, 119, 93);
            Frame(rr, "SpeechFrame", "当前发言", 3, 233, 146, 109);
            Frame(rr, "AnswerFrame", "应答 / R-0317-042", 153, 233, 484, 109);
            answerHeading = rr.Find("AnswerFrame/Heading").GetComponent<Text>();
            Bevel(rr, "RegistrationNumberField", 9, 72, 134, 22, Field, true);
            recordRegistration = Label(rr, "RegistrationNumber", "R-0317-042", 15, 76, 124, 15, 11, Ink);
            Label(rr, "RecordNameCaption", "对象", 12, 100, 45, 15, 11, Dim);
            recordName = Label(rr, "IndividualRecord", "", 61, 100, 82, 15, 11, Ink);
            Label(rr, "RecordUnitCaption", "单元", 12, 119, 45, 15, 11, Dim);
            recordUnit = Label(rr, "RecordUnit", "B-12", 61, 119, 82, 15, 11, Ink);
            Label(rr, "RecordStatusCaption", "状态", 12, 138, 45, 15, 11, Dim);
            recordStatus = Label(rr, "RecordStatus", "", 61, 138, 82, 15, 11, Ink);
            Label(rr, "RecordPortraitCaption", "立绘", 12, 157, 45, 15, 11, Dim);
            recordPortrait = Label(rr, "RecordPortrait", "ML01", 61, 157, 82, 15, 11, Ink);
            Panel(rr, "RecordRule", ColorOf("69756b"), 11, 176, 129, 1);
            Label(rr, "RecordExcerptCaption", "记录摘录", 12, 181, 130, 14, 10, Dim);
            Label(rr, "RecordExcerpt", "暂无正式配置", 12, 196, 130, 15, 11, Ink);
            recordNode = Label(rr, "RecordNode", "", 11, 216, 130, 12, 9, Dim);
            var monitor = Bevel(rr, "LiveMonitor", 158, 70, 350, 144, ColorOf("1b2420"), true);
            var video = Panel(monitor, "VideoViewport", ColorOf("1b2420"), 2, 2, 346, 140);
            video.gameObject.AddComponent<RectMask2D>();
            silhouette = Panel(video, "PatientSilhouette", ColorOf("1b2420"), 0, 0, 346, 140).gameObject;
            silhouette.GetComponent<Image>().raycastTarget = false;
            var sr = silhouette.GetComponent<RectTransform>();
            Panel(sr, "Floor", ColorOf("242e29"), 1, 112, 344, 28);
            Panel(sr, "Crown", ColorOf("303b35"), 164, 25, 46, 8);
            Panel(sr, "Hair", ColorOf("3d4842"), 159, 33, 56, 18);
            Panel(sr, "Face", ColorOf("6e7771"), 161, 51, 52, 26);
            Panel(sr, "Neck", ColorOf("626c64"), 167, 77, 41, 8);
            Panel(sr, "Shoulders", ColorOf("4c5650"), 134, 85, 106, 50);
            Panel(sr, "Collar", ColorOf("5d665d"), 147, 82, 80, 12);
            for (int index = 0; index < 110; index++)
                Panel(sr, "MonitorSpeckle" + index, ColorOf(index % 2 == 0 ? "202c24" : "27332c"), (index * 37 + 17) % 346, (index * 19 + 31) % 140, 1, 1);
            Panel(sr, "CameraLabelBackground", ColorOf("25312b"), 5, 4, 137, 13);
            livePortrait = Panel(video, "LivePatientPortrait", Color.clear, 0, 0, 346, 140).gameObject;
            var before = PortraitImage(livePortrait.transform, "PreviousPortrait", 93, 0, 160, 140);
            var after = PortraitImage(livePortrait.transform, "CurrentPortrait", 93, 0, 160, 140);
            portraitPresenter = livePortrait.AddComponent<TerminalPortraitPresenter>();
            portraitPresenter.Initialize(before, after);
            cameraLabel = Label(video, "CameraLabel", "CAM B12-01 / FIXED", 8, 6, 150, 13, 9, ColorOf("b4c2b5"));
            monitorPortrait = Label(video, "MonitorPortrait", "ML01", 296, 6, 48, 13, 9, ColorOf("b4c2b5"));
            signalLoss = Panel(video, "MonitorSignalLoss", ColorOf("1b2420"), 0, 0, 346, 140).gameObject;
            var noiseColors = new[] { "879c8b", "384b40", "ccd0c3", "0d1b14" };
            for (int index = 0; index < 45; index++)
                signalStripes.Add(Panel(signalLoss.transform, "Noise" + index, ColorOf(noiseColors[index % 4]), (index * 43) % 15, (index * 17) % 140, 346 - (index * 7) % 15, index % 3 == 0 ? 2 : 1));
            signalLoss.SetActive(false);
            signalLostMessage = Label(video, "SignalLostMessage", "CH 01  /  SIGNAL LOST", 77, 55, 220, 16, 10, ColorOf("b4c2b5")).gameObject;
            signalLostMessage.SetActive(false);
            signal = Label(rr, "VideoSignal", "CH 01 / 视频：正常", 160, 216, 195, 13, 10, Ink);
            audioSignal = Label(rr, "AudioSignal", "拾音：接通 / 12 FPS", 363, 216, 145, 13, 10, Ink);
            Label(rr, "DepartureVerification", "离所核验", 525, 76, 75, 14, 11, Ink);
            Panel(rr, "LampHousing", ColorOf("829085"), 603, 71, 16, 16);
            Panel(rr, "LampRim", ColorOf("ecede4"), 604, 72, 14, 14);
            Panel(rr, "LampInset", ColorOf("3c4a40"), 606, 74, 10, 10);
            lamp = Panel(rr, "DepartureLamp", ColorOf("6d786f"), 608, 76, 6, 6).GetComponent<Image>();
            lampGlint = Panel(lamp.transform, "LampGlint", ColorOf("e4e9d8"), 1, 0, 2, 1).gameObject;
            Panel(rr, "VerificationRule", ColorOf("69756b"), 523, 101, 108, 1);
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
            pageLabel = Label(rr, "SpeechPage", "", 111, 262, 25, 12, 8, Dim);
            pageLabel.alignment = TextAnchor.UpperRight;
            var speechClip = Panel(rr, "SpeechViewport", Color.clear, 15, 280, 122, 52);
            speechClip.gameObject.AddComponent<RectMask2D>();
            Body = Label(speechClip, "TerminalDialogueBody", "", 0, 0, 122, 52, 12, Ink);
            answersTab = Button(rr, "AnswersTab", "应答", 548, 236, 39, 15, () => SetHistory(false));
            historyTab = Button(rr, "HistoryTab", "记录", 591, 236, 39, 15, () => SetHistory(true));
            Bevel(rr, "AnswerField", 159, 254, 472, 82, Field, true);
            answers = Panel(rr, "AnswerContents", Color.clear, 162, 257, 466, 76).gameObject;
            answers.GetComponent<Image>().raycastTarget = false;
            ChoiceRoot = answers.GetComponent<RectTransform>();
            NarrationBody = Label(ChoiceRoot, "ConclusionBody", "", 5, 3, 455, 44, 11, Ink);
            NarrationBody.gameObject.SetActive(false);
            Continue = Button(ChoiceRoot, "Continue", "下一句  ↵", 4, 4, 105, 25, () => { });
            returnDirectory = Button(ChoiceRoot, "ReturnDirectory", "返回对象清单", 4, 4, 150, 25, () =>
            {
                SetHistory(false);
                returnToDirectory();
            });
            history = Panel(rr, "TranscriptPanel", Color.clear, 163, 258, 463, 74).gameObject;
            history.GetComponent<Image>().raycastTarget = false;
            for (int index = 0; index < 5; index++)
            {
                historySpeakers[index] = Label(history.transform, "TranscriptSpeaker" + index, "", 5, 1 + index * 14, 47, 13, 10, Dim);
                var clip = Panel(history.transform, "TranscriptRow" + index, Color.clear, 56, 1 + index * 14, 381, 13);
                clip.gameObject.AddComponent<RectMask2D>();
                historyRows[index] = Label(clip, index == 0 ? "TranscriptText" : "TranscriptText" + index, "", 0, 0, 381, 13, 10, Ink);
                historyRows[index].horizontalOverflow = HorizontalWrapMode.Overflow;
                Panel(history.transform, "TranscriptRule" + index, ColorOf("b8beb5"), 2, 13 + index * 14, 444, 1);
            }
            historyTrack = Panel(history.transform, "HistoryTrack", ColorOf("b2bbb0"), 453, 0, 10, 70);
            historyThumb = Panel(historyTrack, "HistoryThumb", ColorOf("69756b"), 0, 0, 10, 70);
            // Optional physical observations remain accessible without covering dialogue controls.
            var kinds = (InspectionKind[])Enum.GetValues(typeof(InspectionKind));
            for (int index = 0; index < kinds.Length; index++)
            {
                var kind = kinds[index];
                inspections.Add(Button(rr, "Inspect-" + kind, InspectionName(kind), 161 + index * 57, 216, 55, 12, () => inspectionAction(kind), 9));
            }
            Continue.gameObject.SetActive(false);
            returnDirectory.gameObject.SetActive(false);
            selfReview = Panel(logical, "SelfReview", Color.clear, 0, 0, 640, 360).gameObject;
            selfReview.GetComponent<Image>().raycastTarget = false;
            var selfReviewRect = selfReview.GetComponent<RectTransform>();
            Frame(selfReviewRect, "SelfMonitorFrame", "实时监控 / 检定室 07", 3, 50, 457, 292);
            var feed = new GameObject("SelfMonitorFeed", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            feed.transform.SetParent(selfReviewRect, false);
            Position(feed.GetComponent<RectTransform>(), 11, 76, 441, 248);
            selfMonitor = feed.GetComponent<RawImage>();
            selfMonitor.color = Color.white;
            selfMonitor.raycastTarget = false;
            Label(selfReviewRect, "SelfCameraLabel", "CAM 07 / LIVE", 12, 326, 420, 11, 9, Dim);
            Frame(selfReviewRect, "SelfDispositionFrame", "本人复核", 464, 50, 173, 292);
            Label(selfReviewRect, "SelfSubject", "值班审查员", 479, 88, 142, 22, 16, Ink);
            Label(selfReviewRect, "SelfStatus", "处置待确认", 479, 119, 142, 18, 11, Dim);
            selfPurify = Button(selfReviewRect, "SelfPurify", "净化", 480, 270, 140, 36, () => selfPurifyAction?.Invoke(), 16);
            selfReview.SetActive(false);
            TerminalIndustrialSkin.AddCrtOverlay(logical);
            SetHistory(false);
            Fit();
        }

        public void Fit()
        {
            float scale = Mathf.Min(viewport.rect.width / 640f, viewport.rect.height / 360f);
            float next = Mathf.Max(.01f, scale);
            if (!Mathf.Approximately(logical.localScale.x, next)) logical.localScale = Vector3.one * next;
        }

        public void ResetNavigation(RuntimeView view)
        {
            current = view;
            rosterIndex = 0;
            historyOffset = 0;
            portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId ?? view.Game.CurrentPatientId);
            sourceNodeId = string.Empty;
            Body.text = string.Empty;
            ClearNarration();
            RestorePresentation(view);
            SetSignalDistortion(0);
            SetHistory(false);
        }

        public void Render(RuntimeView view, bool inputLocked)
        {
            bool newEncounter = current == null || view.PresentedEncounterId != current.PresentedEncounterId;
            bool enteringDirectory = view.TerminalPage == TerminalPage.Directory && current?.TerminalPage != TerminalPage.Directory;
            current = view;
            bool isSelfReview = view.UsesDreamFlow && view.Game.Phase == GamePhase.SelfReview && view.TerminalPage == TerminalPage.SelfReview;
            selfReview.SetActive(isSelfReview);
            selfPurify.interactable = isSelfReview && !inputLocked && view.Input == InputContext.TerminalDirectory && view.Cue == null;
            if (isSelfReview)
            {
                locked = inputLocked;
                historyOpen = false;
                directory.SetActive(false);
                review.SetActive(false);
                Continue.gameObject.SetActive(false);
                returnDirectory.gameObject.SetActive(false);
                breadcrumb.text = "第三安置所 / B区 / 本人复核";
                Fit();
                return;
            }
            if (newEncounter)
            {
                Body.text = string.Empty;
                sourceNodeId = string.Empty;
                portraitId = PortraitCatalog.BasePortrait(view.PresentedPatientId);
                ClearNarration();
                RestorePresentation(view);
            }
            if (enteringDirectory)
            {
                int next = view.Patients.ToList().FindIndex(patient => patient.IsCurrent);
                if (next >= 0) rosterIndex = next;
            }
            locked = inputLocked;
            bool isDirectory = view.TerminalPage == TerminalPage.Directory || (view.TerminalPage == TerminalPage.Archive && view.ArchiveDirectoryVisible);
            directory.SetActive(isDirectory);
            review.SetActive(!isDirectory);
            breadcrumb.text = "第三安置所 / B区 / " + (isDirectory ? "对象目录" : "人员复核");
            string patientId = view.PresentedPatientId ?? view.Game.CurrentPatientId;
            recordName.text = view.PresentedPatientName ?? view.Game.CurrentPatientName ?? "";
            recordStatus.text = view.PresentedDisposition.HasValue ? "已" + DispositionName(view.PresentedDisposition.Value) : "观察中";
            recordRegistration.text = PortraitCatalog.Registration(patientId);
            recordUnit.text = PortraitCatalog.Unit(patientId);
            monitorHeading.text = "实时监控 / " + recordUnit.text;
            answerHeading.text = "应答 / " + recordRegistration.text;
            cameraLabel.text = "CAM " + recordUnit.text.Replace("-", "") + "-01 / FIXED";
            recordPortrait.text = portraitId;
            recordNode.text = string.IsNullOrEmpty(sourceNodeId) ? "R1" : sourceNodeId.Substring(0, Math.Min(20, sourceNodeId.Length));
            quota.text = view.Game.RemainingPurificationQuota.ToString("00");
            bool atDisposition = view.Game.EncounterStage == EncounterStage.AwaitingDisposition;
            bool release = view.Game.AvailableDispositions.Contains(Disposition.Release);
            lamp.color = !atDisposition ? ColorOf("6d786f") : release ? ColorOf("8faf83") : ColorOf("b07871");
            lampGlint.SetActive(atDisposition);
            foreach (var pair in dispositions)
            {
                pair.Value.gameObject.SetActive(view.UsesTerminalDirectory ? view.ConfiguredDispositions.Contains(pair.Key)
                    : atDisposition && view.Game.AvailableDispositions.Contains(pair.Key));
                pair.Value.interactable = !locked && view.Input == InputContext.Dialogue && atDisposition && view.Game.AvailableDispositions.Contains(pair.Key);
            }
            foreach (var button in inspections)
            {
                button.gameObject.SetActive(!view.UsesTerminalDirectory);
                button.interactable = !locked && !historyOpen && view.Input == InputContext.Dialogue && view.Game.CurrentPatientId != null;
            }
            bool archived = view.TerminalPage == TerminalPage.Archive;
            returnDirectory.gameObject.SetActive(archived && !isDirectory);
            returnDirectory.interactable = !locked && !historyOpen;
            signal.text = archived ? "视讯：断开" : "CH 01 / 视频：正常";
            audioSignal.text = archived ? "拾音：断开" : "拾音：接通 / 12 FPS";
            UpdatePortrait();
            livePortrait.SetActive(!archived && portraits != null && portraits.Find(portraitId) != null);
            silhouette.SetActive(!archived && !livePortrait.activeSelf);
            cameraLabel.gameObject.SetActive(!archived);
            monitorPortrait.gameObject.SetActive(!archived);
            portraitPresenter.Paused = view.Paused || historyOpen;
            signalLostMessage.SetActive(archived);
            if (archived || view.Cue?.Kind != PresentationKind.ExitTerminal || view.LeavingTerminal) SetSignalDistortion(0);
            RenderRoster();
            RenderHistory();
            Fit();
        }

        public void SetSelfMonitor(Texture texture) => selfMonitor.texture = texture;

        public bool HandleSelfReviewKey(KeyCode key)
        {
            if (!selfReview.activeInHierarchy || locked || current?.TerminalPage != TerminalPage.SelfReview) return false;
            if (key != KeyCode.Return && key != KeyCode.KeypadEnter) return false;
            return MiniGameAudioButton.Activate(selfPurify);
        }

        public void SetLine(string displaySpeaker, string text, string portrait, string nodeId = null)
        {
            Speaker.text = current != null && current.UsesTerminalDirectory ? "对象" : displaySpeaker;
            Body.text = text;
            SetPortrait(portrait, nodeId);
        }

        public void SetPortrait(string portrait, string nodeId = null)
        {
            if (!string.IsNullOrEmpty(portrait)) portraitId = portrait;
            if (!string.IsNullOrEmpty(nodeId)) sourceNodeId = nodeId;
            recordPortrait.text = portraitId;
            monitorPortrait.text = portraitId;
            recordNode.text = sourceNodeId.Substring(0, Math.Min(20, sourceNodeId.Length));
            // Portrait identifiers select presentation only. Formal sprites may replace the silhouette.
            var face = silhouette.transform.Find("Face");
            if (face != null) face.GetComponent<Image>().color = portraitId == "ML03" ? ColorOf("797a6c") : ColorOf("6e7771");
            UpdatePortrait();
        }

        private void RestorePresentation(RuntimeView view)
        {
            var lastSpeech = view.Transcript.LastOrDefault(entry => !entry.IsPlayer && !entry.IsNarration);
            if (lastSpeech != null) SetLine(lastSpeech.Speaker, lastSpeech.Text, lastSpeech.PortraitId, lastSpeech.SourceNodeId);
            var lastPortrait = view.Transcript.LastOrDefault(entry => !entry.IsPlayer && !string.IsNullOrEmpty(entry.PortraitId));
            if (lastPortrait != null) SetPortrait(lastPortrait.PortraitId);
        }

        private void UpdatePortrait() => portraitPresenter.Show(portraits != null ? portraits.Find(portraitId) : null);

        public void SetNarration(string text)
        {
            NarrationBody.text = text;
            NarrationBody.gameObject.SetActive(true);
            Position(Continue.GetComponent<RectTransform>(), 350, 50, 110, 24);
        }

        public void ClearNarration()
        {
            NarrationBody.gameObject.SetActive(false);
            Position(Continue.GetComponent<RectTransform>(), 4, 4, 105, 25);
        }

        public void SetPageIndicator(int page, int count) => pageLabel.text = count > 1 ? page + "/" + count : "";

        public Button AddResponse(string id, string text, int index, Action action)
        {
            var button = Button(ChoiceRoot, "Reply-" + id, text, 0, index * 19, 466, 18, action, 12, true);
            TerminalIndustrialSkin.SetChoiceRow(button);
            var label = button.transform.Find("Label").GetComponent<Text>();
            Position(label.rectTransform, 33, 2, 426, 15);
            label.alignment = TextAnchor.UpperLeft;
            var clip = Panel(button.transform, "ChoiceTextViewport", Color.clear, 33, 2, 426, 15);
            clip.gameObject.AddComponent<RectMask2D>();
            label.transform.SetParent(clip, false);
            Position(label.rectTransform, 0, 0, 426, 15);
            var number = Label(button.transform, "ChoiceNumber", (index + 1).ToString("00"), 8, 3, 25, 13, 10, Dim);
            TerminalIndustrialSkin.RegisterLabel(button, number);
            return button;
        }

        public void SetSignalDistortion(float amount) => signalLoss.SetActive(amount > 0f);

        public void AnimateSignal(float time)
        {
            if (!signalLoss.activeInHierarchy) return;
            int tick = Mathf.FloorToInt(time * 25);
            for (int index = 0; index < signalStripes.Count; index++)
                signalStripes[index].anchoredPosition = new Vector2((index * 43) % 15, -((index * 17 + tick) % 140));
        }

        public void SetHistory(bool open)
        {
            historyOpen = open;
            answers.SetActive(!open);
            history.SetActive(open);
            TerminalIndustrialSkin.SetSelected(answersTab, !open);
            TerminalIndustrialSkin.SetSelected(historyTab, open);
            if (current != null) Render(current, locked);
            HistoryChanged?.Invoke();
        }

        public void HighlightResponse(int index)
        {
            for (int child = 0, response = 0; child < ChoiceRoot.childCount; child++)
            {
                var item = ChoiceRoot.GetChild(child);
                if (!item.name.StartsWith("Reply-", StringComparison.Ordinal) || !item.gameObject.activeSelf) continue;
                TerminalIndustrialSkin.SetSelected(item.GetComponent<Button>(), response++ == index);
            }
        }

        public bool HandleDirectoryKey(KeyCode key)
        {
            if (current == null || !directory.activeSelf || locked) return false;
            if (key == KeyCode.UpArrow) SelectRoster(Mathf.Max(0, rosterIndex - 1));
            else if (key == KeyCode.DownArrow) SelectRoster(Mathf.Min(4, rosterIndex + 1));
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter) return MiniGameAudioButton.Activate(enter);
            else return false;
            return true;
        }

        public bool HandleArchiveKey(KeyCode key)
        {
            if (locked || historyOpen || !returnDirectory.gameObject.activeInHierarchy || !returnDirectory.interactable) return false;
            if (key != KeyCode.Return && key != KeyCode.KeypadEnter) return false;
            return MiniGameAudioButton.Activate(returnDirectory);
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
                var labels = rosterLabels[index];
                labels[0].text = patient?.Name ?? "未配置对象 " + (index + 1).ToString("00");
                labels[1].text = patient == null ? "-- -- --" : PortraitCatalog.Registration(patient.Id);
                labels[2].text = patient == null ? "—" : PortraitCatalog.Unit(patient.Id);
                labels[3].text = patient == null ? "未配置" : PatientStatus(patient);
                var sprite = patient != null && portraits != null ? portraits.Find(PortraitCatalog.BasePortrait(patient.Id)) : null;
                rosterSprites[index].sprite = sprite;
                rosterSprites[index].enabled = sprite != null;
                rosterSprites[index].gameObject.SetActive(sprite != null);
                rosterPlaceholders[index].SetActive(sprite == null);
                TerminalIndustrialSkin.SetSelected(row, index == rosterIndex);
                row.interactable = !locked;
            }
            var selected = rosterIndex < current.Patients.Count ? current.Patients[rosterIndex] : null;
            bool available = selected != null && selected.CanReceiveToday && current.TerminalPage == TerminalPage.Directory;
            previewSprite.sprite = selected != null && portraits != null ? portraits.Find(PortraitCatalog.BasePortrait(selected.Id)) : null;
            previewSprite.enabled = previewSprite.sprite != null;
            previewSprite.gameObject.SetActive(previewSprite.sprite != null);
            previewPortrait.SetActive(selected != null && previewSprite.sprite == null);
            previewNoData.SetActive(selected == null);
            previewName.text = selected?.Name ?? "未配置";
            previewId.text = selected == null ? "-- -- --" : PortraitCatalog.Registration(selected.Id);
            previewUnit.text = "单元  " + (selected == null ? "—" : PortraitCatalog.Unit(selected.Id));
            previewStatus.text = "状态  " + (selected == null ? "未接入" : PatientStatus(selected));
            previewBatch.text = "当前批次   DAY " + current.Game.Day.ToString("00");
            previewRegistration.text = "登记状态    " + (selected == null ? "未接入" : selected.HasMetToday ? "已归档" : "待复核");
            enter.interactable = available && !locked;
            enter.GetComponentInChildren<Text>().text = available ? "进入审查" : selected == null ? "未接入" : "已归档 / 禁止进入";
            leave.interactable = !locked;
            rosterCount.text = "当日登记 " + current.Patients.Count.ToString("00") + "    待审 " + current.Patients.Count(p => p.CanReceiveToday).ToString("00") + "    已处置 " + current.Patients.Count(p => p.HasMetToday).ToString("00");
        }

        private void RenderHistory()
        {
            if (current == null) return;
            int count = current.Transcript.Count;
            historyOffset = Mathf.Clamp(historyOffset, 0, Mathf.Max(0, count - 5));
            int start = Mathf.Max(0, count - 5 - historyOffset);
            for (int index = 0; index < historyRows.Length; index++)
            {
                var entry = start + index < count ? current.Transcript[start + index] : null;
                historySpeakers[index].text = entry == null ? "" : entry.IsPlayer ? "值班员" : entry.IsNarration ? "终端" : "对象";
                historyRows[index].text = entry == null ? "" : entry.Text.Replace("\r", " ").Replace("\n", " ");
            }
            historyTrack.gameObject.SetActive(count > 5);
            float thumbHeight = Mathf.Max(11, Mathf.Round(70f * 5 / Mathf.Max(5, count)));
            float thumbY = count <= 5 ? 0 : Mathf.Round((count - 5f - historyOffset) / (count - 5) * (70 - thumbHeight));
            Position(historyThumb, 0, thumbY, 10, thumbHeight);
        }

        private string SubmittedDisposition()
        {
            if (current == null) return null;
            var entry = current.Transcript.LastOrDefault(item => item.SourceNodeId != null && item.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal));
            return entry != null && Enum.TryParse(entry.SourceNodeId.Substring("disposition:".Length), out Disposition type)
                ? "已" + DispositionName(type) : null;
        }

        private RectTransform Bevel(Transform parent, string name, float x, float y, float width, float height, Color fill, bool sunken = false)
            => TerminalIndustrialSkin.Bevel(parent, name, x, y, width, height, fill, sunken);

        private void Frame(Transform parent, string name, string title, float x, float y, float width, float height, bool dark = false)
            => TerminalIndustrialSkin.Frame(parent, font, name, title, x, y, width, height, dark);

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

        private Button Button(Transform parent, string name, string label, float x, float y, float width, float height, Action action, int size = 11, bool rowStyle = false)
            => TerminalIndustrialSkin.Button(parent, font, name, label, x, y, width, height, action, size, rowStyle);

        private static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
        private static Color ColorOf(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
        private static string DispositionName(Disposition type) => type == Disposition.Treat ? "治疗" : type == Disposition.Purify ? "净化" : "释放";
        private string PatientStatus(PatientView patient) => patient.HasMetToday || patient.Presence != PatientPresence.Active
            ? patient.LastDisposition.HasValue ? "已" + DispositionName(patient.LastDisposition.Value) : "已归档"
            : patient.CanReceiveToday ? "待审" : "待接待";

        private Image PortraitImage(Transform parent, string name, float x, float y, float width, float height)
        {
            var image = Panel(parent, name, Color.white, x, y, width, height).GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }
        private static string InspectionName(InspectionKind kind) => kind == InspectionKind.Eyes ? "眼睛" : kind == InspectionKind.Hands ? "手部" : kind == InspectionKind.Teeth ? "牙齿" : kind == InspectionKind.Photo ? "照片" : kind == InspectionKind.DailySigns ? "通知" : "档案";
    }

}
