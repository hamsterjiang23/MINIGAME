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
                    var targets = new[] { new Vector2(400, 121), new Vector2(400, 151), new Vector2(400, 181), new Vector2(400, 211), new Vector2(400, 241) };
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var hits = Pick(display, targets[i]);
                        Assert.That(hits, Is.Not.Empty);
                        Assert.That(hits[0].gameObject.GetComponentInParent<Button>()?.name, Is.EqualTo("Roster-" + i), "Mode " + mode + " top hit: " + string.Join(",", hits.Select(h => h.gameObject.name)));
                    }
                }
                display.SetMode(1);
                var selected = Pick(display, new Vector2(400, 121)).Select(h => h.gameObject.GetComponentInParent<Button>()).First(b => b != null && b.name == "Roster-0");
                var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, clickCount = 2 };
                selected.GetComponent<TerminalRosterPointer>().OnPointerClick(pointer);
                Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
                if (runtime.GetView().Cue != null) runtime.CompleteCue(runtime.GetView().Cue.Token);
                screen.SetDialogueLine("对象", "这是监控画面中的人物。", "D1_R1_OPEN1", "ML01");
                int replies = 0;
                screen.ShowResponseButtons(new[] { new ResponseContent("one", "第一项"), new ResponseContent("two", "第二项") }, _ => replies++);
                Assert.That(screen.HandleTerminalKey(KeyCode.Return), Is.True);
                Assert.That(replies, Is.EqualTo(1));
                display.RenderFrame();
                var replyHits = Pick(display, new Vector2(650, 423));
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
                Assert.That(Pick(display, new Vector2(650, 423)).Any(h => h.module.GetType().Name == "TerminalDisplayRaycaster"), Is.False,
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
                    Assert.That(replaced.GetPixels(1060, 396, 496, 56).Count(pixel => pixel.r > .3f && pixel.g > .12f),
                        Is.GreaterThan(1000), "The replacement's selected reply must render, not disappear behind the layer mask.");
                }
                finally { Object.Destroy(replaced); }
                Assert.That(Pick(display, new Vector2(650, 423))[0].gameObject.GetComponentInParent<Button>()?.name, Is.EqualTo("Reply-four"));
                display.RenderFrame(); CaptureTexture(display.Texture, "amber-review-source"); CaptureComposite(display,"amber-review");
            }
            finally { Object.Destroy(owner); }
            yield return null; yield return null;
            Assert.That(Object.FindObjectsOfType<TerminalDisplay>().Any(d => d.name == "Terminal"), Is.False);
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
                var last=screen.GetComponentsInChildren<Text>(false).Where(t=>t.name.StartsWith("TranscriptText")).LastOrDefault();
                Assert.That(last,Is.Not.Null);
                {
                    Assert.That(last.horizontalOverflow,Is.EqualTo(HorizontalWrapMode.Wrap));
                    var clip=(RectTransform)last.transform.parent;Assert.That(-clip.anchoredPosition.y+clip.rect.height,Is.LessThanOrEqualTo(135));
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
                        checkedPortraits++;yield return null;
                    }
                }
                Assert.That(checkedPortraits,Is.EqualTo(27));
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
