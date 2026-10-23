using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("TerminalV0824")]
    public sealed class AmberTerminalPlayModeTests
    {
        [UnityTest, Timeout(30000)]
        public IEnumerator NativeCompositeRendersAndWarpedPickingMatchesEveryDisplayMode()
        {
            var rules = PrototypeRules.Create(); var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("Amber composite test"); var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation(); OpenDirectory(runtime);
                yield return null;
                var display = screen.TerminalDisplay; display.RenderFrame();
                Assert.That(display.Texture.IsCreated(), Is.True);
                Assert.That(display.OutputRect.rect.width / display.OutputRect.rect.height, Is.EqualTo(4f / 3).Within(.0001));
                AssertAmberPixels(display.Texture);
                CaptureTexture(display.Texture, "amber-directory-source"); CaptureComposite(display,"amber-directory");
                foreach (int mode in new[] { 0, 1, 2 })
                {
                    display.SetMode(mode);
                    var targets = Enumerable.Range(0, runtime.GetView().Patients.Count)
                        .Select(index => new Vector2(400, 121 + index * 30)).ToArray();
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var hits = Pick(display, targets[i]);
                        Assert.That(hits, Is.Not.Empty, "Configured roster row " + i + " in CRT mode " + mode);
                        Assert.That(hits[0].gameObject.GetComponentInParent<Button>()?.name, Is.EqualTo("Roster-" + i), "Mode " + mode + " top hit: " + string.Join(",", hits.Select(h => h.gameObject.name)));
                    }
                }
                display.SetMode(1);
                var selected = Pick(display, new Vector2(400, 121)).Select(h => h.gameObject.GetComponentInParent<Button>()).First(b => b != null && b.name == "Roster-0");
                var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, clickCount = 1 };
                ExecuteEvents.Execute(selected.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
                if (runtime.GetView().Cue != null) runtime.CompleteCue(runtime.GetView().Cue.Token);
                screen.SetDialogueLine("对象", "这是监控画面中的人物。", "D1_R1_OPEN1", "ML01");
                int replies = 0;
                screen.ShowResponseButtons(new[] { new ResponseContent("one", "第一项"), new ResponseContent("two", "第二项") }, _ => replies++);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(replies, Is.EqualTo(1));
                display.RenderFrame();
                var replyHits = Pick(display, new Vector2(650, 441));
                Assert.That(replyHits[0].gameObject.GetComponentInParent<Button>()?.name, Is.EqualTo("Reply-two"));
                var reply = replyHits.Select(h => h.gameObject.GetComponentInParent<Button>()).FirstOrDefault(b => b != null && b.name == "Reply-two");
                Assert.That(reply, Is.Not.Null, string.Join(",", replyHits.Select(h => h.gameObject.name)));
                ExecuteEvents.Execute(reply.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                Assert.That(replies, Is.EqualTo(2));
                Assert.That(screen.HandleTerminalKey(KeyCode.F2),Is.False);
                Assert.That(screen.HandleTerminalKey(KeyCode.F3),Is.False);
                Assert.That(screen.HistoryOpen,Is.False);
                Assert.That(screen.TryCloseHistory(),Is.False);
                screen.HandleTerminalKey(KeyCode.Return);Assert.That(replies,Is.EqualTo(3));
                runtime.SetPaused(true);
                Assert.That(Pick(display, new Vector2(650, 441)).Any(h => h.module.GetType().Name == "TerminalDisplayRaycaster"), Is.False,
                    "The terminal raycaster must yield to the real pause overlay.");
                runtime.SetPaused(false);
                yield return null;
                // Equal-sized reply replacements must still appear in the source camera after hierarchy caching.
                screen.ShowResponseButtons(new[] { new ResponseContent("three", "第三项"), new ResponseContent("four", "第四项") }, _ => replies++);
                yield return null; yield return null;
                display.RenderFrame();
                var replaced = Read(display.Texture);
                try
                {
                    Assert.That(LogicalPixels(replaced, new Rect(530, 392, 248, 28)).Count(pixel => pixel.r > .3f && pixel.g > .12f),
                        Is.GreaterThan(1000), "The replacement's selected reply must render, not disappear behind the layer mask.");
                }
                finally { Object.Destroy(replaced); }
                Assert.That(Pick(display, new Vector2(650, 441))[0].gameObject.GetComponentInParent<Button>()?.name, Is.EqualTo("Reply-four"));
                display.RenderFrame(); CaptureTexture(display.Texture, "amber-review-source"); CaptureComposite(display,"amber-review");
            }
            finally { Object.Destroy(owner); }
            yield return null; yield return null;
            Assert.That(Object.FindObjectsOfType<TerminalDisplay>().Any(d => d.name == "Terminal"), Is.False);
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator HtmlBitmapGlyphsAndHighlightStatesRenderOnActualNativeSource()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var owner = new GameObject("HTML font and highlight GPU test");
            var screen = owner.AddComponent<MiniGameScreen>();
            var highlight = new Color32(238, 180, 48, 255);
            var desk = new Color32(11, 8, 3, 255);
            Texture2D selectedImage = null, hoverImage = null, pressedImage = null;
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation(); OpenDirectory(runtime);
                yield return null;
                var display = screen.TerminalDisplay;
                Assert.That(display.Texture.width, Is.EqualTo(800));
                Assert.That(display.Texture.height, Is.EqualTo(600));
                Assert.That(display.Texture.filterMode, Is.EqualTo(FilterMode.Point));
                ClearTerminalFontReferences(screen);
                var first = NativeButton(screen, "Roster-0");
                var second = NativeButton(screen, "Roster-1");
                display.RenderFrame(); selectedImage = Read(display.Texture);
                AssertButtonFill(screen, selectedImage, first, highlight, "Initial keyboard selection");
                AssertButtonFill(screen, selectedImage, second, desk, "Other directory row");
                CaptureTexture(display.Texture, "html-font-directory-selected-source");
                CaptureComposite(display, "html-font-directory-selected");

                // Compare a real label against the same live frame with that component disabled, so a
                // pre-rendered package background cannot make an empty font renderer pass this test.
                var heading = screen.GetComponentsInChildren<TerminalBitmapText>(true).Single(t => t.name == "RosterHeading");
                heading.enabled = false; display.RenderFrame();
                var withoutHeading = Read(display.Texture);
                try
                {
                    var withText = LogicalPixels(selectedImage, new Rect(23, 51, 490, 20));
                    var withoutText = LogicalPixels(withoutHeading, new Rect(23, 51, 490, 20));
                    Assert.That(withText.Zip(withoutText, (a, b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b))
                        .Count(delta => delta > .05f), Is.GreaterThan(100), "The null-Font bitmap label must contribute real GPU pixels.");
                }
                finally { Object.Destroy(withoutHeading); heading.enabled = true; }

                var pointer = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = display.ScreenPointFor((RectTransform)second.transform)
                };
                ExecuteEvents.Execute(second.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                display.RenderFrame(); hoverImage = Read(display.Texture);
                AssertButtonFill(screen, hoverImage, first, desk, "Hover suppresses the keyboard selection");
                AssertButtonFill(screen, hoverImage, second, highlight, "Only the hovered row is reversed");
                CaptureTexture(display.Texture, "html-font-directory-hover-source");
                CaptureComposite(display, "html-font-directory-hover");

                var name = second.GetComponentsInChildren<TerminalBitmapText>().Single(t => t.name == "Label");
                Vector2 resting = name.rectTransform.anchoredPosition;
                ExecuteEvents.Execute(second.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                Assert.That(name.rectTransform.anchoredPosition, Is.EqualTo(resting + new Vector2(1, -1)));
                display.RenderFrame(); pressedImage = Read(display.Texture);
                AssertButtonFill(screen, pressedImage, second, highlight, "Pointer down retains the reversed face");
                AssertDarkGlyphShift(hoverImage, pressedImage, new Rect(64, 142, 158, 20));
                CaptureTexture(display.Texture, "html-font-directory-pressed-source");
                CaptureComposite(display, "html-font-directory-pressed");
                ExecuteEvents.Execute(second.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                Assert.That(name.rectTransform.anchoredPosition, Is.EqualTo(resting));
                ExecuteEvents.Execute(second.gameObject, pointer, ExecuteEvents.pointerExitHandler);

                Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
                if (runtime.GetView().Cue != null) runtime.CompleteCue(runtime.GetView().Cue.Token);
                screen.SetDialogueLine("对象", "请继续审查。", "font-gpu-line", "ML01");
                ClearTerminalFontReferences(screen);
                var lockedDisposition = NativeButton(screen, "Disposition-Release");
                Assert.That(lockedDisposition.gameObject.activeInHierarchy, Is.True);
                Assert.That(lockedDisposition.IsInteractable(), Is.False);
                ExecuteEvents.Execute(lockedDisposition.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.Execute(lockedDisposition.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                display.RenderFrame();
                var disabled = Read(display.Texture);
                try { AssertButtonFill(screen, disabled, lockedDisposition, desk, "Locked disposition cannot acquire hover or press fill"); }
                finally { Object.Destroy(disabled); }
                CaptureTexture(display.Texture, "html-font-disposition-disabled-source");
                CaptureComposite(display, "html-font-disposition-disabled");

                screen.ShowResponseButtons(new[]
                {
                    new ResponseContent("gpu-one", new string('选', 30)),
                    new ResponseContent("gpu-two", "继续审查")
                }, _ => { });
                ClearTerminalFontReferences(screen);
                var firstReply = NativeButton(screen, "Reply-gpu-one");
                var secondReply = NativeButton(screen, "Reply-gpu-two");
                display.RenderFrame();
                var choices = Read(display.Texture);
                try
                {
                    AssertButtonFill(screen, choices, firstReply, highlight, "First response starts selected");
                    AssertButtonFill(screen, choices, secondReply, desk, "Other response remains dark");
                    Assert.That(firstReply.GetComponentsInChildren<TerminalBitmapText>().Single(t => t.name == "Label")
                        .GetComponentInParent<RectMask2D>(), Is.Not.Null);
                    foreach (var pixel in LogicalPixels(choices, new Rect(773, 397, 4, 18)))
                        AssertColor(pixel, highlight, "Long response text must stop at its real RectMask2D viewport.");
                }
                finally { Object.Destroy(choices); }
                CaptureTexture(display.Texture, "html-font-choices-selected-source");
                CaptureComposite(display, "html-font-choices-selected");
                ExecuteEvents.Execute(secondReply.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                display.RenderFrame();
                var hoveredChoice = Read(display.Texture);
                try
                {
                    AssertButtonFill(screen, hoveredChoice, firstReply, desk, "Choice hover suppresses the selected response");
                    AssertButtonFill(screen, hoveredChoice, secondReply, highlight, "Hovered response is the only reversed response");
                }
                finally { Object.Destroy(hoveredChoice); }
                CaptureTexture(display.Texture, "html-font-choices-hover-source");
                CaptureComposite(display, "html-font-choices-hover");
                ExecuteEvents.Execute(secondReply.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                screen.HideDialogueActions();

                // Reach the real core's disposition stage, rather than merely enabling a synthetic button.
                for (int step = 0; step < 32 && runtime.GetView().Game.EncounterStage != EncounterStage.AwaitingDisposition; step++)
                {
                    var view = runtime.GetView();
                    if (view.Cue != null) Assert.That(runtime.CompleteCue(view.Cue.Token).Accepted, Is.True);
                    else
                    {
                        Assert.That(view.Dialogue, Is.Not.Null);
                        Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId,
                            view.Dialogue.Responses[0].Id).Accepted, Is.True);
                    }
                }
                Assert.That(runtime.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.AwaitingDisposition));
                ClearTerminalFontReferences(screen);
                var availableDisposition = screen.GetComponentsInChildren<Button>(false)
                    .First(b => b.name.StartsWith("Disposition-", StringComparison.Ordinal) && b.IsInteractable());
                display.RenderFrame();
                var ready = Read(display.Texture);
                try { AssertButtonFill(screen, ready, availableDisposition, highlight, "The first available real disposition is selected"); }
                finally { Object.Destroy(ready); }
                CaptureTexture(display.Texture, "html-font-disposition-ready-source");
                CaptureComposite(display, "html-font-disposition-ready");
            }
            finally
            {
                Object.Destroy(selectedImage); Object.Destroy(hoverImage); Object.Destroy(pressedImage);
                Object.Destroy(owner);
            }
            yield return null;
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator BootCanBeSkippedPausedRestoredAndLeavingKeepsTheDailySessionPowered()
        {
            var rules = PrototypeRules.Create(); var content = new PrototypeContent(rules); var runtime = new GameRuntime(rules, content, true);
            var owner = new GameObject("Amber power test"); var screen = owner.AddComponent<MiniGameScreen>();
            var cameraObject = new GameObject("Amber fixture camera", typeof(Camera)); cameraObject.transform.SetParent(owner.transform);
            var pose = new GameObject("Amber terminal pose").transform; pose.SetParent(owner.transform); pose.position = Vector3.forward;
            var effects = owner.AddComponent<PresentationEffects>(); effects.Configure(cameraObject.GetComponent<Camera>(), pose, null, null);
            try
            {
                screen.Bind(runtime); effects.Bind(runtime, screen);
                runtime.TryInteract(InteractionTarget.Door); runtime.CompleteCue(runtime.GetView().Cue.Token); runtime.CompleteCue(runtime.GetView().Cue.Token);
                runtime.TryInteract(InteractionTarget.Computer);
                yield return Until(() => screen.PowerPhase == TerminalPowerPhase.Boot, 8, "boot console");
                Assert.That(screen.TransitionLocked, Is.True);
                CaptureTexture(screen.TerminalDisplay.Texture, "amber-boot-source"); CaptureComposite(screen.TerminalDisplay,"amber-boot");
                string save = runtime.Save(); string oldToken = runtime.GetView().Cue.Token;
                runtime = GameRuntime.Restore(save, rules, content); screen.Bind(runtime); effects.Bind(runtime, screen);
                Assert.That(runtime.CompleteCue(oldToken).Accepted, Is.False);
                yield return Until(() => screen.PowerPhase == TerminalPowerPhase.Boot, 8, "restored boot console");
                runtime.SetPaused(true); yield return null;
                runtime.SetPaused(false);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False, "The Enter that resumes pause must not also skip boot.");
                yield return null;
                screen.HandleTerminalKey(KeyCode.Return);
                yield return Until(() => screen.PowerPhase == TerminalPowerPhase.Loading, 3, "partition loading");
                runtime.SetPaused(true); var phase = screen.PowerPhase;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(screen.PowerPhase, Is.EqualTo(phase));
                Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.EnterTerminal));
                runtime.SetPaused(false);
                yield return new WaitForSecondsRealtime(.5f); CaptureTexture(screen.TerminalDisplay.Texture, "amber-loading-source"); CaptureComposite(screen.TerminalDisplay,"amber-loading");
                yield return Until(() => runtime.GetView().Cue == null, 3, "running terminal");
                Assert.That(screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
                int quota = runtime.GetView().Game.RemainingPurificationQuota;
                Assert.That(runtime.LeaveTerminal().Accepted, Is.True);
                Assert.That(runtime.GetView().TerminalShuttingDown, Is.False);
                Assert.That(screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.False);
                yield return Until(() => !runtime.GetView().TerminalOpen, 3, "room return with daily power retained");
                Assert.That(screen.TerminalReveal, Is.Zero);
                Assert.That(runtime.GetView().TerminalPowered, Is.True);
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
                Assert.That(runtime.GetView().Transcript, Is.Empty);
                runtime = GameRuntime.Restore(runtime.Save(), rules, content); screen.Bind(runtime); effects.Bind(runtime, screen);
                Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                Assert.That(runtime.GetView().TerminalBootRequired, Is.False);
                yield return Until(() => runtime.GetView().Cue == null, 3, "reopened terminal without boot console");
                Assert.That(screen.PowerPhase, Is.EqualTo(TerminalPowerPhase.Running));
                Assert.That(runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
            }
            finally { Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator LongNarrationPagesMatchActualWrappedLabelsAndNewestSpeechRemainsVisible()
        {
            var asset=Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var package=DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var runtime=new GameRuntime(package.Rules,package.Content,true);
            var owner=new GameObject("Wrapped campaign narration test");var screen=owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime);screen.UseYarnPresentation();screen.DialogueCharactersPerSecond=0;
                runtime.TryInteract(InteractionTarget.Computer);runtime.CompleteCue(runtime.GetView().Cue.Token);runtime.EnterSelectedPatient("tutorial");
                string paragraph=string.Concat(Enumerable.Repeat("第三次治疗后，阿德里安从病人队列中消失。审查员阅读完整的处置说明，之后才能继续当天的会谈。",8));
                int completed=0;screen.PresentDialogueLine("系统",paragraph,"wrap-regression","D0_01",true,()=>completed++);
                var label=screen.GetComponentsInChildren<Text>(true).Single(t=>t.name=="ConclusionBody");
                Assert.That(label.horizontalOverflow,Is.EqualTo(HorizontalWrapMode.Wrap));
                Assert.That(label.alignment,Is.EqualTo(TextAnchor.UpperLeft));
                Assert.That(screen.CurrentLinePageCount,Is.GreaterThan(1));
                string reconstructed="";
                for(int i=0;i<100&&completed==0;i++)
                {
                    reconstructed+=label.text;
                    Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(label.rectTransform.rect.height+1),"Actual rendering must match pagination measurement.");
                    screen.HandleTerminalKey(KeyCode.Return);
                }
                Assert.That(completed,Is.EqualTo(1));Assert.That(reconstructed,Is.EqualTo(paragraph));
                var simpleRules=PrototypeRules.Create();runtime=new GameRuntime(simpleRules,new PrototypeContent(simpleRules),true);screen.Bind(runtime);OpenDirectory(runtime);runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId);
                if(runtime.GetView().Cue!=null)runtime.CompleteCue(runtime.GetView().Cue.Token);
                for(int i=0;i<8;i++) screen.SetDialogueLine("对象",new string('长',70)+"当前"+i,"wrap-speech-"+i,"D0_01");
                yield return null;
                screen.TerminalDisplay.RenderFrame();
                var rows=screen.GetComponentsInChildren<Text>(false).Where(t=>t.name.StartsWith("TranscriptText")).ToArray();
                Assert.That(rows,Has.Length.EqualTo(8),"Scrolling must retain every speech row instead of discarding older records.");
                Assert.That(rows.First().text,Does.EndWith("当前0"));
                var last=rows.LastOrDefault();
                Assert.That(last,Is.Not.Null);
                {
                    Assert.That(last.horizontalOverflow,Is.EqualTo(HorizontalWrapMode.Wrap));
                    Assert.That(last.text,Does.EndWith("当前7"));
                    var scroll=last.GetComponentInParent<ScrollRect>();
                    Assert.That(scroll,Is.Not.Null);
                    Assert.That(scroll.viewport.GetComponent<RectMask2D>(),Is.Not.Null);
                    Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
                    Assert.That(scroll.verticalNormalizedPosition,Is.LessThanOrEqualTo(.01f),"New speech must follow the bottom when the reader has not scrolled away.");
                    var row=(RectTransform)last.transform.parent;
                    var corners=new Vector3[4];row.GetWorldCorners(corners);
                    float bottom=scroll.viewport.InverseTransformPoint(corners[0]).y;
                    float top=scroll.viewport.InverseTransformPoint(corners[1]).y;
                    Assert.That(bottom,Is.InRange(scroll.viewport.rect.yMin-.5f,scroll.viewport.rect.yMin+5),
                        "The newest row must finish inside the clipped viewport with the four-pixel bottom padding.");
                    Assert.That(top,Is.GreaterThan(scroll.viewport.rect.yMin+1),"The newest wrapped speech must overlap the visible viewport.");
                    Assert.That(last.canvasRenderer.cull,Is.False,"RectMask2D must leave the newest speech visible.");
                }
            }
            finally {Object.Destroy(owner);}
            yield return null;
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator EveryCampaignPortraitRemainsVisibleInItsOwnDynamicRoom()
        {
            var asset=Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            var package=DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var catalog=Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var configuration=Object.Instantiate(Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault"));
            var owner=new GameObject("Actual campaign monitor visibility",typeof(RectTransform),typeof(RawImage));
            ((RectTransform)owner.transform).sizeDelta=new Vector2(464,261);
            var feed=owner.AddComponent<PatientSurveillanceFeed>();feed.UseExternalClock=true;feed.Initialize(owner.GetComponent<RawImage>(),configuration);
            var diagnosticShader=Shader.Find("Hidden/MiniGame/Patient Visibility Diagnostic");
            Assert.That(diagnosticShader,Is.Not.Null);
            var diagnostic=new Material(diagnosticShader);
            int checkedPortraits=0;
            var checkedIds=new HashSet<string>();
            try
            {
                foreach(var room in configuration.Rooms)
                {
                    string baseId=PortraitCatalog.BasePortrait(room.PatientId),prefix=baseId.Substring(0,baseId.Length-2);
                    foreach(var item in package.Package.Portraits.Where(p=>p.Id.StartsWith(prefix,StringComparison.Ordinal)))
                    {
                        var sprite=catalog.Find(item.Id);Assert.That(sprite,Is.Not.Null,item.Id);
                        feed.Show(room.PatientId,sprite,false);Assert.That(feed.Output,Is.Not.Null);
                        CaptureTexture(feed.Output,"monitor-"+item.Id);
                        var actor=feed.ActiveRoom.transform.parent.GetComponentInChildren<SpriteRenderer>(true);
                        var original=actor.sharedMaterial;
                        var roomRenderers=feed.ActiveRoom.GetComponentsInChildren<Renderer>(true);
                        var enabled=roomRenderers.Select(r=>r.enabled).ToArray();
                        var camera=feed.ViewCamera;Color background=camera.backgroundColor;
                        Color32[] expected,actual;
                        try
                        {
                            actor.sharedMaterial=diagnostic;camera.backgroundColor=Color.black;
                            foreach(var renderer in roomRenderers)renderer.enabled=false;
                            camera.Render();var image=Read(camera.targetTexture);expected=image.GetPixels32();Object.Destroy(image);
                            for(int i=0;i<roomRenderers.Length;i++)roomRenderers[i].enabled=enabled[i];
                            camera.Render();image=Read(camera.targetTexture);actual=image.GetPixels32();Object.Destroy(image);
                        }
                        finally
                        {
                            actor.sharedMaterial=original;camera.backgroundColor=background;
                            for(int i=0;i<roomRenderers.Length;i++)roomRenderers[i].enabled=enabled[i];
                        }
                        int silhouette=0,visible=0;
                        for(int i=0;i<expected.Length;i++)
                            if(expected[i].r>250&&expected[i].g>250&&expected[i].b>250)
                            {silhouette++;if(actual[i].r>250&&actual[i].g>250&&actual[i].b>250)visible++;}
                        Assert.That(silhouette,Is.GreaterThan(100),item.Id+" must be inside the real camera view");
                        float fraction=(float)visible/silhouette;
                        Assert.That(fraction,Is.GreaterThanOrEqualTo(.97f),item.Id+" furniture occlusion");
                        Debug.Log("MINIGAME_AMBER_MONITOR_VISIBILITY "+item.Id+" visible="+fraction+" silhouette="+silhouette);
                        checkedPortraits++;checkedIds.Add(item.Id);yield return null;
                    }
                }
                Assert.That(package.Package.Portraits,Is.Not.Empty);
                Assert.That(checkedPortraits,Is.EqualTo(package.Package.Portraits.Count));
                Assert.That(checkedIds,Is.EquivalentTo(package.Package.Portraits.Select(item=>item.Id)),
                    "Every portrait declared by the active campaign must have been rendered.");
            }
            finally {Object.Destroy(owner);Object.Destroy(configuration);Object.Destroy(diagnostic);}
            yield return null;
        }

        private static IEnumerator Until(Func<bool> predicate, float seconds, string label)
        { float deadline = Time.realtimeSinceStartup + seconds; while (!predicate() && Time.realtimeSinceStartup < deadline) yield return null; Assert.That(predicate(), Is.True, label); }
        private static void OpenDirectory(GameRuntime runtime)
        { runtime.TryInteract(InteractionTarget.Door); runtime.CompleteCue(runtime.GetView().Cue.Token); runtime.CompleteCue(runtime.GetView().Cue.Token); runtime.TryInteract(InteractionTarget.Computer); runtime.CompleteCue(runtime.GetView().Cue.Token); }
        private static List<RaycastResult> Pick(TerminalDisplay display, Vector2 source)
        {
            Vector2 wanted = new Vector2(source.x / 800, 1 - source.y / 600), uv = wanted;
            for (int i = 0; i < 12; i++) uv += wanted - TerminalDisplay.DisplayToSource(uv, display.Mode);
            var r = display.OutputRect; var local = new Vector3(r.rect.xMin + uv.x * r.rect.width, r.rect.yMin + uv.y * r.rect.height, 0);
            var data = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, r.TransformPoint(local)) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits); return hits;
        }
        private static void AssertAmberPixels(RenderTexture source)
        {
            Texture2D image = Read(source);
            try { var pixels = image.GetPixels32(); int lit = pixels.Count(p => p.r > 80 && p.r > p.g && p.g > p.b); Assert.That(lit, Is.GreaterThan(1000), "Native UI must render amber text and rows, not an empty clear color."); }
            finally { Object.Destroy(image); }
        }
        private static Button NativeButton(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Button>(true).Single(button => button.name == name);
        private static void ClearTerminalFontReferences(MiniGameScreen screen)
        {
            foreach (var label in screen.GetComponentsInChildren<TerminalBitmapText>(true)) label.font = null;
        }
        private static Color[] LogicalPixels(Texture2D image, Rect topOriginRect)
        {
            int x = Mathf.RoundToInt(topOriginRect.x * image.width / 800f);
            int width = Mathf.RoundToInt(topOriginRect.width * image.width / 800f);
            int height = Mathf.RoundToInt(topOriginRect.height * image.height / 600f);
            int y = image.height - Mathf.RoundToInt(topOriginRect.yMax * image.height / 600f);
            return image.GetPixels(x, y, width, height);
        }
        private static Color TopPixel(Texture2D image, int x, int y) => image.GetPixel(
            Mathf.FloorToInt(x * image.width / 800f), image.height - 1 - Mathf.FloorToInt(y * image.height / 600f));
        private static void AssertButtonFill(MiniGameScreen screen, Texture2D image, Button button, Color32 expected, string reason)
        {
            var logical = screen.GetComponentsInChildren<RectTransform>(true).Single(rect => rect.name == "TerminalLogical800x600");
            var rect = (RectTransform)button.transform;
            Vector3 local = logical.InverseTransformPoint(rect.TransformPoint(new Vector3(rect.rect.xMin + 4, rect.rect.center.y)));
            AssertColor(TopPixel(image, Mathf.RoundToInt(local.x - logical.rect.xMin),
                Mathf.RoundToInt(logical.rect.yMax - local.y)), expected, reason);
        }
        private static void AssertColor(Color actual, Color32 expected, string reason)
        {
            Color32 bytes = actual;
            Assert.That(Mathf.Abs(bytes.r - expected.r), Is.LessThanOrEqualTo(2), reason + " (red)");
            Assert.That(Mathf.Abs(bytes.g - expected.g), Is.LessThanOrEqualTo(2), reason + " (green)");
            Assert.That(Mathf.Abs(bytes.b - expected.b), Is.LessThanOrEqualTo(2), reason + " (blue)");
        }
        private static void AssertDarkGlyphShift(Texture2D before, Texture2D after, Rect textArea)
        {
            int dark = 0, shifted = 0;
            for (int y = (int)textArea.yMin; y < textArea.yMax; y++)
                for (int x = (int)textArea.xMin; x < textArea.xMax; x++)
                {
                    Color pixel = TopPixel(before, x, y);
                    if (pixel.r > .3f || pixel.g > .3f) continue;
                    dark++;
                    Color next = TopPixel(after, x + 1, y + 1);
                    if (next.r <= .3f && next.g <= .3f) shifted++;
                }
            Assert.That(dark, Is.GreaterThan(30), "The actual hovered roster name must have visible inverse glyph pixels.");
            Assert.That((float)shifted / dark, Is.GreaterThan(.97f), "Pointer down must move the rendered glyph pixels right and down by one native pixel.");
        }
        private static Texture2D Read(RenderTexture source)
        { var before = RenderTexture.active; var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false); try { RenderTexture.active = source; image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0); image.Apply(); return image; } finally { RenderTexture.active = before; } }
        private static void CaptureComposite(TerminalDisplay display, string name)
        {
            var texture=new RenderTexture(1600,1200,0,RenderTextureFormat.ARGB32); texture.Create();
            var previous=RenderTexture.active;
            try { Graphics.Blit(display.Texture,texture,display.ScreenMaterial); CaptureTexture(texture,name); }
            finally { RenderTexture.active=previous; texture.Release();Object.Destroy(texture); }
        }
        private static void CaptureTexture(RenderTexture source, string name)
        {
            string folder = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR"); if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder); var image = Read(source);
            try { File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG()); } finally { Object.Destroy(image); }
        }
    }
}
