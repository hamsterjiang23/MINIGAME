using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("TerminalV0824")]
    public sealed class TerminalHtmlParityTests
    {
        [UnityTest]
        public IEnumerator EndingCharactersRenderThroughTheLiveDialogueCanvas()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("Ending glyph regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                var lines = new[] {
                    "一定是上帝……您还期盼我给更多人带来净化。",
                    "呵呵...蛀虫已把这个社会蚕食殆尽，哪还会有新鲜的血肉？",
                    "那里的天好蓝，水好清澈，我想回去看看了。" };
                for (int index = 0; index < lines.Length; index++)
                {
                    screen.SetDialogueLine("对象", lines[index], "glyph-audit-" + index, "ML01");
                    yield return null;
                    screen.TerminalDisplay.RenderFrame();
                    var label = screen.GetComponentsInChildren<TerminalBitmapText>()
                        .Single(item => item.name.StartsWith("TranscriptText") && item.text == lines[index]);
                    foreach (char character in label.text.Where(character => !char.IsWhiteSpace(character)))
                    {
                        Assert.That(label.ArkTypeface.TryGet(character, label.fontSize, out var glyph), Is.True, character.ToString());
                        Assert.That(glyph.w * glyph.h, Is.GreaterThan(0), character.ToString());
                    }
                    Assert.That(label.canvasRenderer.GetMesh().vertexCount,
                        Is.EqualTo(label.text.Count(character => !char.IsWhiteSpace(character)) * 4));
                    CaptureTerminalSource(screen, "ending-glyphs-" + index);
                }
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DirectoryDossierFollowsPointerAndKeyboardWithoutStartingAnEncounter()
        {
            var ids = new[] { "former-ruler", "battlefield-doctor", "child", "priest", "diplomat" };
            var names = new[] { "薇拉", "埃德温", "米洛", "乔纳", "阿德里安" };
            var patients = ids.Select((id, index) => new PatientDefinition(id, names[index], 1, 0, null, PatientPresence.TreatmentEnded));
            var rules = new GameRules("directory-dossier-fixture", patients,
                new[] { new DayDefinition(1, 1, ids.Select(id => PrototypeRules.CreateMeeting(id, 1))) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("Directory dossier regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenDirectory(screen, runtime);
                string checkpoint = runtime.Save();
                yield return null;
                var catalog = Resources.Load<TerminalDossierCatalog>("MiniGame/TerminalDossiers");
                var summary = screen.GetComponentInChildren<TerminalDossierSummary>(true);
                var portrait = screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "DirectoryPortrait");
                var introduction = TextNamed(screen, "PreviewIntroduction");
                for (int index = 0; index < ids.Length; index++)
                {
                    var row = ButtonNamed(screen, "Roster-" + index);
                    var pointer = new PointerEventData(EventSystem.current)
                        { position = screen.TerminalDisplay.ScreenPointFor((RectTransform)row.transform) };
                    ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                    Assert.That(TextNamed(screen, "Preview对象").text, Does.StartWith(names[index] + " / "));
                    Assert.That(portrait.sprite, Is.SameAs(catalog.Find(ids[index]).portrait));
                    Assert.That(portrait.sprite, Is.Not.Null);
                    Assert.That(portrait.enabled && portrait.preserveAspect, Is.True);
                    Assert.That(ObjectNamed(screen, "DirectoryPortraitNoImage").activeSelf, Is.False);
                    Assert.That(introduction.text, Is.Not.Empty);
                    Assert.That(summary.FullText, Is.EqualTo(catalog.Find(ids[index]).summary));
                    Assert.That(introduction.preferredHeight, Is.LessThanOrEqualTo(44));
                    Assert.That(TextNamed(screen, "DossierAge").text, Is.EqualTo(catalog.Find(ids[index]).age));
                    Assert.That(TextNamed(screen, "DossierOccupation").text, Is.EqualTo(catalog.Find(ids[index]).occupation));
                    Assert.That(TextNamed(screen, "PreviewStage").text, Is.EqualTo("未处理"));
                    Assert.That(TextNamed(screen, "Preview状态").text, Is.EqualTo("待审"));
                    Assert.That(TextNamed(screen, "PreviewUnit").text, Does.Contain(PortraitCatalog.Unit(ids[index])));
                    Assert.That(runtime.Save(), Is.EqualTo(checkpoint), "Browsing dossiers cannot select or mutate the authoritative encounter.");
                    yield return EndingVisualCapture.Capture("dossier-" + ids[index] + "-" + Screen.width, runtime);
                }
                Assert.That(summary.FullText, Is.EqualTo("高度危险的政治倾向，********，建议直接净化。"));
                Assert.That(introduction.text, Does.Not.Contain("观察审查人员"));
                Assert.That(introduction.text, Does.Not.Contain("秘密"));
                Assert.That(screen.HandleTerminalKey(KeyCode.UpArrow), Is.True);
                Assert.That(portrait.sprite, Is.SameAs(catalog.Find("priest").portrait));
                Assert.That(TextNamed(screen, "Preview对象").text, Does.StartWith("乔纳 / "));
                Assert.That(runtime.Save(), Is.EqualTo(checkpoint));
                Assert.That(ButtonNamed(screen, "EnterReview").interactable, Is.True);
                Assert.That(ButtonNamed(screen, "LeaveTerminal").interactable, Is.True);
                var panel = ObjectNamed(screen, "DirectoryPortraitPanel").GetComponent<RectTransform>();
                Assert.That(panel.rect.size, Is.EqualTo(new Vector2(98, 130)));
                Assert.That(panel.anchoredPosition, Is.EqualTo(new Vector2(30, -322)));
                Assert.That(ButtonNamed(screen, "EnterReview").GetComponent<RectTransform>().anchoredPosition.y,
                    Is.EqualTo(-486));
                yield return EndingVisualCapture.Flush();
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PatientStageAndDailyReviewStatusStaySeparateInRosterAndSelection()
        {
            foreach (var disposition in new[] { Disposition.Treat, Disposition.Purify, Disposition.Release })
            {
                var runtime = CreateRuntime();
                var owner = new GameObject("Patient stage and status regression");
                var screen = owner.AddComponent<MiniGameScreen>();
                try
                {
                    OpenReview(screen, runtime);
                    string patientId = runtime.GetView().Game.CurrentPatientId;
                    int patientIndex = runtime.GetView().Patients.ToList().FindIndex(patient => patient.Id == patientId);
                    var row = ButtonNamed(screen, "Roster-" + patientIndex);
                    Assert.That(row.transform.Find("Stage").GetComponent<Text>().text, Is.EqualTo("未处理"));
                    Assert.That(row.transform.Find("Status").GetComponent<Text>().text, Is.EqualTo("待审"));
                    for (int step = 0; step < 100 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; step++)
                    {
                        var view = runtime.GetView();
                        var result = view.Cue != null ? runtime.CompleteCue(view.Cue.Token)
                            : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                        Assert.That(result.Accepted, Is.True, result.Reason);
                    }
                    Assert.That(runtime.GetView().Game.AvailableDispositions, Does.Contain(disposition));
                    Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, disposition).Accepted, Is.True);
                    for (int step = 0; step < 5 && runtime.GetView().Cue != null; step++)
                        Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                    Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
                    Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
                    yield return null;

                    string stage = disposition == Disposition.Treat ? "治疗1次"
                        : disposition == Disposition.Purify ? "已净化" : "已释放";
                    Assert.That(row.gameObject.activeInHierarchy, Is.True, "Same-day disposed patients retain their directory records.");
                    Assert.That(row.transform.Find("Stage").GetComponent<Text>().text, Is.EqualTo(stage));
                    Assert.That(row.transform.Find("Status").GetComponent<Text>().text, Is.EqualTo("已审"));
                    Assert.That(row.transform.Find("Unit").GetComponent<Text>().text, Is.EqualTo(PortraitCatalog.Unit(patientId)));
                    Assert.That(TextNamed(screen, "RosterStageColumn").text, Is.EqualTo("阶段"));
                    Assert.That(TextNamed(screen, "RosterUnitColumn").text, Is.EqualTo("单元"));
                    Assert.That(TextNamed(screen, "RosterStatusColumn").text, Is.EqualTo("状态"));
                    var stageRect = row.transform.Find("Stage").GetComponent<RectTransform>();
                    var unitRect = row.transform.Find("Unit").GetComponent<RectTransform>();
                    var statusRect = row.transform.Find("Status").GetComponent<RectTransform>();
                    Assert.That(stageRect.anchoredPosition.x + stageRect.rect.width, Is.LessThan(unitRect.anchoredPosition.x));
                    Assert.That(unitRect.anchoredPosition.x + unitRect.rect.width, Is.LessThan(statusRect.anchoredPosition.x));

                    var pointer = new PointerEventData(EventSystem.current)
                        { position = screen.TerminalDisplay.ScreenPointFor((RectTransform)row.transform) };
                    ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                    Assert.That(TextNamed(screen, "PreviewStage").text, Is.EqualTo(stage));
                    Assert.That(TextNamed(screen, "Preview状态").text, Is.EqualTo("已审"));
                    Assert.That(TextNamed(screen, "PreviewUnit").text, Does.Contain(PortraitCatalog.Unit(patientId)));
                    Assert.That(runtime.GetView().Patients.Where(patient => !patient.HasMetToday).Count(), Is.GreaterThan(0));
                    Assert.That(TextNamed(screen, "RosterCount").text, Does.Contain("已审 01"));
                    CaptureTerminalSource(screen, "patient-stage-" + disposition);
                }
                finally { Object.Destroy(owner); }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator UnknownPortraitIdShowsNoImageAndValidIdClearsTheWarning()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("Portrait fallback regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
                string patient = runtime.GetView().PresentedPatientId;
                string portraitId = PortraitCatalog.BasePortrait(patient);
                var expected = catalog.Find(portraitId);
                Assert.That(expected, Is.Not.Null);
                // Reproduce a stale warning left by a previous failed lookup.
                ObjectNamed(screen, "MonitorNoImage").SetActive(true);
                screen.SetDialogueLine("病人", "缺失表情", "portrait-regression", "missing-expression");
                Assert.That(ObjectNamed(screen, "MonitorNoImage").activeSelf, Is.True);
                Assert.That(screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "CurrentPortrait").sprite,
                    Is.Null, "An unknown table ID must not substitute another portrait.");
                screen.SetDialogueLine("病人", "正常表情", "portrait-regression", portraitId);
                Assert.That(ObjectNamed(screen, "MonitorNoImage").activeSelf, Is.False);
                Assert.That(screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "CurrentPortrait").sprite, Is.SameAs(expected));
                yield return null;
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChoiceHoverPickingAndKeyboardSelectionStayTogetherAcrossCrtModes()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("Choice hover regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                var selected = new List<string>();
                screen.ShowResponseButtons(Responses(), selected.Add);
                yield return null;
                var rows = Enumerable.Range(1, 4).Select(i => ButtonNamed(screen, "Reply-choice-" + i)).ToArray();
                var initialHighlight = rows[0].GetComponent<Image>().color;
                Assert.That(rows.Skip(1).All(row => row.GetComponent<Image>().color != initialHighlight), Is.True,
                    "The first response must visibly start selected before pointer input.");
                // Another panel's pointer highlight must not suppress the default response.
                var outside = ButtonNamed(screen, "Disposition-Release");
                outside.interactable = true;
                var outsidePointer = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(outside.gameObject, outsidePointer, ExecuteEvents.pointerEnterHandler);
                Assert.That(rows[0].GetComponent<Image>().color, Is.EqualTo(initialHighlight));
                ExecuteEvents.Execute(outside.gameObject, outsidePointer, ExecuteEvents.pointerExitHandler);
                outside.interactable = false;
                CaptureTerminalSource(screen, "choice-default-selected");
                var hits = new List<RaycastResult>();
                for (int mode = 0; mode < 3; mode++)
                {
                    screen.TerminalDisplay.SetMode(mode);
                    foreach (var row in rows)
                    {
                        var point = screen.TerminalDisplay.ScreenPointFor((RectTransform)row.transform);
                        var pointer = new PointerEventData(EventSystem.current) { position = point };
                        for (int sample = -1; sample <= 1; sample++)
                        {
                            pointer.position = point + Vector2.up * sample * 2;
                            hits.Clear();
                            EventSystem.current.RaycastAll(pointer, hits);
                            Assert.That(hits.FirstOrDefault().gameObject, Is.EqualTo(row.gameObject), "Visible row must match picking in CRT mode " + mode);
                        }
                        ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                        var highlighted = row.GetComponent<Image>().color;
                        ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                        Assert.That(row.GetComponent<Image>().color, Is.EqualTo(highlighted), "Crossing the gap must retain the last hovered choice.");
                        Assert.That(rows.Where(other => other != row).All(other => other.GetComponent<Image>().color != highlighted), Is.True);
                        screen.HandleTerminalKey(KeyCode.Return);
                        Assert.That(selected.Last(), Is.EqualTo(row.name.Substring("Reply-".Length)), "Enter must confirm the highlighted choice.");
                    }
                }
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-1"), "Keyboard navigation wraps from the last hovered choice.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChoiceKeysDefaultToFirstWrapAndSupportAllFourNumberShortcuts()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("HTML Choice Input Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                var selected = new List<string>();
                var responses = Responses();
                screen.ShowResponseButtons(responses, selected.Add);
                yield return null;
                Assert.That(screen.WorldInputBlockedThisFrame, Is.False, "Keyboard checks need a fresh frame after the fixture's world transition.");
                screen.HandleTerminalKey(KeyCode.Return);
                screen.HandleTerminalKey(KeyCode.KeypadEnter);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(selected, Is.EqualTo(new[] { "choice-1", "choice-1" }), "Enter confirms the initial first choice; Space never chooses.");

                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.Space);
                Assert.That(selected, Has.Count.EqualTo(2), "Space cannot confirm a response.");
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-4"), "Up from no selection starts at the last choice.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected, Has.Count.EqualTo(4), "Replacing choices selects the first item.");
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.KeypadEnter);
                Assert.That(selected.Last(), Is.EqualTo("choice-2"), "Down from the default first item chooses the second.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.DownArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-1"), "Down wraps from the last choice to the first.");

                screen.ShowResponseButtons(responses, selected.Add);
                screen.HandleTerminalKey(KeyCode.UpArrow);
                screen.HandleTerminalKey(KeyCode.Return);
                Assert.That(selected.Last(), Is.EqualTo("choice-4"), "Up wraps from the first choice to the last.");

                for (int index = 0; index < responses.Length; index++)
                {
                    screen.ShowResponseButtons(responses, selected.Add);
                    int before = selected.Count;
                    screen.HandleTerminalKey((KeyCode)((int)KeyCode.Alpha1 + index));
                    Assert.That(selected.Count, Is.EqualTo(before + 1));
                    Assert.That(selected.Last(), Is.EqualTo(responses[index].Id));
                }
                Assert.That(selected, Has.Count.EqualTo(11), "Each confirmation invokes exactly one response callback.");
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReadOnlyRecordsPreserveTranscriptAndBlockUnderlyingChoicesWhileF3StaysDisabled()
        {
            var runtime = CreateRuntime(); var owner = new GameObject("Passive transcript test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                for (int i=0;i<8;i++) screen.SetDialogueLine("对象", "记录"+i+"：审查对话", "passive-"+i,"ML01");
                int advanced=0, quota=runtime.GetView().Game.RemainingPurificationQuota;
                screen.ShowResponseButtons(Responses(),_=>advanced++); yield return null;
                var recordKeys=runtime.GetView().Transcript.Select(entry=>entry.Key).ToArray();
                Assert.That(screen.HandleTerminalKey(KeyCode.F2),Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.F3),Is.False);
                var terminalView = typeof(MiniGameScreen).GetField("terminalView",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(screen);
                Assert.That(terminalView.GetType().GetMethod("OpenHistory").Invoke(terminalView, null), Is.EqualTo(true));
                Assert.That(screen.HistoryOpen,Is.True);
                CaptureTerminalSource(screen,"ark-history");
                AssertGlyphEffectIsScoped(screen);
                CaptureDefaultTextMotion(screen);
                screen.HandleTerminalKey(KeyCode.Return);
                screen.HandleTerminalKey(KeyCode.Alpha1);
                Assert.That(advanced,Is.Zero,"The record viewer cannot activate controls behind it.");
                Assert.That(screen.TryCloseHistory(),Is.True);
                Assert.That(screen.HistoryOpen,Is.False);
                yield return null;
                CaptureTerminalSource(screen,"ark-live-transcript");
                Assert.That(screen.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="TerminalSettings"||t.name=="HistoryTab"),Is.False);
                var rows=screen.GetComponentsInChildren<Text>(false).Where(t=>t.name.StartsWith("TranscriptText")).ToArray();
                Assert.That(rows,Has.Length.EqualTo(8),"The passive scrollable transcript must retain all saved records.");
                Assert.That(rows.Select(row=>row.text),Is.EqualTo(Enumerable.Range(0,8).Select(i=>"记录"+i+"：审查对话")));
                var bitmapLabels=screen.GetComponentsInChildren<TerminalBitmapText>(true).ToArray();
                Assert.That(bitmapLabels.All(label=>label.ArkTypeface==TerminalPixelFont.Default),Is.True);
                foreach(var label in bitmapLabels)
                {
                    bool dialogue=label.name.StartsWith("TranscriptText")||label.name.StartsWith("TranscriptSpeaker")
                        ||label.name.StartsWith("HistoryTranscriptText")||label.name.StartsWith("HistoryTranscriptSpeaker");
                    Assert.That(label.GetComponent<TerminalDialogueTextGlitch>()!=null,Is.EqualTo(dialogue),label.name);
                }
                Assert.That(bitmapLabels.Count(label=>label.name.StartsWith("HistoryTranscriptText")),Is.EqualTo(8));
                Assert.That(runtime.GetView().Transcript.Count(entry=>entry.SourceNodeId.StartsWith("passive-",StringComparison.Ordinal)),Is.EqualTo(8));
                Assert.That(runtime.GetView().Transcript.Select(entry=>entry.Key),Is.EqualTo(recordKeys),"Removed function keys must not alter saved transcript entries.");
                var scroll=rows.Last().GetComponentInParent<ScrollRect>();
                Assert.That(scroll,Is.Not.Null);
                Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
                screen.HandleTerminalKey(KeyCode.Return); Assert.That(advanced,Is.EqualTo(1));
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota,Is.EqualTo(quota));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DispositionKeepsLastSpeechAndLocalSignalLossThenReturnsToDirectoryBeforeRoomExit()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("HTML Archive Flow Test");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                string patientId = runtime.GetView().Game.CurrentPatientId;
                string lastSpeech = string.Empty;
                for (int step = 0; step < 32 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; step++)
                {
                    var view = runtime.GetView();
                    if (view.Cue != null)
                    {
                        Assert.That(view.Cue.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
                        lastSpeech = "对象回应 " + step;
                        screen.SetDialogueLine("对象", lastSpeech, "html-reaction-" + step, "ML02");
                        Assert.That(runtime.CompleteCue(view.Cue.Token).Accepted, Is.True);
                    }
                    else
                    {
                        Assert.That(view.Dialogue, Is.Not.Null);
                        lastSpeech = view.Dialogue.Text;
                        screen.SetDialogueLine(view.Dialogue.Speaker, lastSpeech, "html-line-" + step, "ML01");
                        Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId,
                            view.Dialogue.Responses[0].Id).Accepted, Is.True);
                    }
                }
                var dispositionView = runtime.GetView();
                Assert.That(dispositionView.Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition),
                    "The real core conversation must reach disposition within the bounded loop.");
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                screen.SetDialogueText("系统", "请选择处置");
                screen.ShowCueText("处置记录准备中", null);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                Assert.That(runtime.SubmitDisposition(dispositionView.Game.CurrentEncounterId,
                    dispositionView.Game.AvailableDispositions.First()).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Reaction));
                lastSpeech = "我知道了。这是我的最后一句话。";
                screen.SetDialogueLine("对象", lastSpeech, "D1_M_TREAT_T1", "ML03");
                screen.SetDialogueInteractable(true);
                Assert.That(TextNamed(screen, "RecordNode").text, Is.EqualTo("人员复核 / 措施"));
                screen.SetGlitch(.55f);
                Assert.That(ObjectNamed(screen, "GeneratedSignalLoss").activeSelf, Is.False);
                Assert.That(ObjectNamed(screen, "MonitorSignalLoss").activeSelf, Is.False,
                    "Signal loss starts after the actor finishes the reaction.");
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
                Assert.That(runtime.GetView().LeavingTerminal, Is.False);
                screen.SetDialogueText("系统", "对象已归档");
                screen.SetGlitch(.55f);
                Assert.That(ObjectNamed(screen, "GeneratedSignalLoss").activeSelf, Is.False);
                Assert.That(ObjectNamed(screen, "MonitorSignalLoss").activeInHierarchy, Is.True);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));

                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                screen.SetGlitch(0);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
                Assert.That(ObjectNamed(screen, "SignalLostMessage").activeInHierarchy, Is.True);
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech));
                var checkpoint = runtime.Save();
                var rules = PrototypeRules.Create();
                runtime = GameRuntime.Restore(checkpoint, rules, new PrototypeContent(rules));
                Assert.That(runtime.GetView().Transcript.Last(entry => !entry.IsPlayer).Text, Is.EqualTo(lastSpeech));
                screen.Bind(runtime);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive));
                Assert.That(TextNamed(screen, "TerminalDialogueBody").text, Is.EqualTo(lastSpeech),
                    "Rebinding an archive checkpoint restores the last actor speech from its transcript.");
                Assert.That(ObjectNamed(screen, "SignalLostMessage").activeInHierarchy, Is.True);
                yield return null;
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True, "A fresh confirmation returns the restored archive to its directory.");
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
                Assert.That(runtime.GetView().TerminalOpen, Is.True, "Archive completion returns to the terminal directory, not the room.");
                Assert.That(ObjectNamed(screen, "ObjectDirectory").activeInHierarchy, Is.True);
                Assert.That(ButtonNamed(screen, "EnterReview").interactable, Is.True, "The next queued patient is now available.");
                Assert.That(runtime.GetView().Game.CurrentPatientId, Is.Not.EqualTo(patientId));
                Assert.That(runtime.EnterSelectedPatient(patientId).Accepted, Is.False);
                ButtonNamed(screen, "LeaveTerminal").onClick.Invoke();
                Assert.That(runtime.GetView().LeavingTerminal, Is.True, "The explicit directory exit still requests the room transition.");
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ExitTerminal));
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                Assert.That(runtime.GetView().TerminalOpen, Is.False);
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DispositionHoverKeepsItsSelectionAcrossPointerGapsAndCrtModes()
        {
            var runtime = CreateRuntime();
            var owner = new GameObject("Disposition hover regression");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                OpenReview(screen, runtime);
                for (int step = 0; step < 100 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; step++)
                {
                    var view = runtime.GetView();
                    var result = view.Cue != null ? runtime.CompleteCue(view.Cue.Token)
                        : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds.First());
                    Assert.That(result.Accepted, Is.True, result.Reason);
                }
                Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
                yield return null;
                var rows = screen.GetComponentsInChildren<Button>(true).Where(button => button.name.StartsWith("Disposition-")
                    && button.gameObject.activeInHierarchy && button.interactable).ToArray();
                Assert.That(rows.Length, Is.GreaterThanOrEqualTo(2));
                for (int mode = 0; mode < 3; mode++)
                {
                    screen.TerminalDisplay.SetMode(mode);
                    foreach (var row in rows)
                    {
                        var pointer = new PointerEventData(EventSystem.current) { position = screen.TerminalDisplay.ScreenPointFor((RectTransform)row.transform) };
                        var hits = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(pointer, hits);
                        Assert.That(hits.First().gameObject, Is.EqualTo(row.gameObject));
                        ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                        var artwork = row.transform.Find("CommandArtwork").GetComponent<RawImage>();
                        var selectedTexture = artwork.texture;
                        ExecuteEvents.Execute(row.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                        for (int frame = 0; frame < 5; frame++)
                        {
                            yield return null;
                            Assert.That(artwork.texture, Is.SameAs(selectedTexture), "A pointer gap must retain the last hovered disposition.");
                        }
                    }
                }
                var expected = (Disposition)Enum.Parse(typeof(Disposition), rows.Last().name.Substring("Disposition-".Length));
                string before = runtime.Save();
                foreach (var key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space })
                {
                    Assert.That(screen.HandleTerminalKey(key), Is.True, "Disposition confirmation keys are consumed without executing.");
                    Assert.That(runtime.Save(), Is.EqualTo(before));
                }
                ExecuteEvents.Execute(rows.Last().gameObject,
                    new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Assert.That(runtime.GetView().PresentedDisposition, Is.EqualTo(expected));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        private static GameRuntime CreateRuntime()
        {
            var rules = PrototypeRules.Create();
            return new GameRuntime(rules, new PrototypeContent(rules), useTerminalDirectory: true);
        }

        private static void OpenReview(MiniGameScreen screen, GameRuntime runtime)
        {
            OpenDirectory(screen, runtime);
            Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
        }

        private static void OpenDirectory(MiniGameScreen screen, GameRuntime runtime)
        {
            screen.Bind(runtime);
            screen.UseYarnPresentation();
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            if (runtime.GetView().Cue != null)
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
        }

        private static ResponseContent[] Responses() => Enumerable.Range(1, 4)
            .Select(index => new ResponseContent("choice-" + index, "选项 " + index)).ToArray();

        private static Button ButtonNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Button>(true).Single(button => button.name == name);

        private static Text TextNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Text>(true).Single(text => text.name == name);

        private static GameObject ObjectNamed(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Transform>(true).Single(item => item.name == name).gameObject;

        private static void CaptureTerminalSource(MiniGameScreen screen, string name)
        {
            if (!Application.isEditor) return;
            var display = screen.TerminalDisplay; display.RenderFrame();
            var target = display.Texture; var active = RenderTexture.active;
            var pixels = new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            try
            {
                RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0); pixels.Apply();
                string folder=System.Environment.GetEnvironmentVariable("MINIGAME_FONT_CAPTURE_DIR")
                    ?? System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Artifacts/TerminalArk20261021/Visuals"));
                System.IO.Directory.CreateDirectory(folder);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
            }
            finally { RenderTexture.active=active; Object.Destroy(pixels); }
        }

        private static void AssertGlyphEffectIsScoped(MiniGameScreen screen)
        {
            var effects=screen.GetComponentsInChildren<TerminalDialogueTextGlitch>(true);
            var originals=effects.Select(effect=>effect.Settings).ToArray();
            var seeds=effects.Select(effect=>effect.Seed).ToArray();
            var preview=ScriptableObject.CreateInstance<TerminalDialogueTextSettings>();
            preview.Intensity=preview.BurstChance=preview.GlyphFraction=1; preview.HorizontalPixels=0; preview.VerticalPixels=2;
            var heading=TextNamed(screen,"TerminalVersion");
            var body=TextNamed(screen,"HistoryTranscriptText"); string text=body.text;
            try
            {
                foreach(var effect in effects)
                {
                    effect.Configure(preview); effect.PreviewClock=0; effect.Seed=123;
                    effect.GetComponent<TerminalBitmapText>().SetVerticesDirty();
                }
                screen.TerminalDisplay.RenderFrame();
                var active=body.canvasRenderer.GetMesh().vertices;
                var stable=heading.canvasRenderer.GetMesh().vertices;
                foreach(var effect in effects)
                { effect.PreviewClock=.1f; effect.GetComponent<TerminalBitmapText>().SetVerticesDirty(); }
                screen.TerminalDisplay.RenderFrame();
                var idle=body.canvasRenderer.GetMesh().vertices;
                Assert.That(active.Length,Is.GreaterThan(0));
                Assert.That(active.SequenceEqual(idle),Is.False,"The real Canvas glyph mesh must react to the preview clock.");
                Assert.That(active.Zip(idle,(a,b)=>a.x==b.x).All(value=>value),Is.True,"The default haunting displacement is vertical.");
                Assert.That(active.Zip(idle,(a,b)=>a.y>b.y).Any(value=>value),Is.True);
                Assert.That(active.Zip(idle,(a,b)=>a.y<b.y).Any(value=>value),Is.True);
                Assert.That(heading.canvasRenderer.GetMesh().vertices,Is.EqualTo(stable),"Non-dialogue headings remain fixed.");
                Assert.That(body.text,Is.EqualTo(text));
            }
            finally
            {
                for(int index=0;index<effects.Length;index++)
                { effects[index].Configure(originals[index]); effects[index].PreviewClock=null; effects[index].Seed=seeds[index]; }
                Object.Destroy(preview);
            }
        }

        private static void CaptureDefaultTextMotion(MiniGameScreen screen)
        {
            if(!Application.isEditor) return;
            var effects=screen.GetComponentsInChildren<TerminalDialogueTextGlitch>(true);
            try
            {
                for(int frame=0;frame<16;frame++)
                {
                    float clock=(frame/2)*.125f+(frame%2==0?0:.075f);
                    foreach(var effect in effects)
                    { effect.PreviewClock=clock; effect.GetComponent<TerminalBitmapText>().SetVerticesDirty(); }
                    CaptureTerminalSource(screen,"ark-vertical-"+frame.ToString("00"));
                }
            }
            finally
            {
                foreach(var effect in effects)
                { effect.PreviewClock=null; effect.GetComponent<TerminalBitmapText>().SetVerticesDirty(); }
            }
        }
    }
}
