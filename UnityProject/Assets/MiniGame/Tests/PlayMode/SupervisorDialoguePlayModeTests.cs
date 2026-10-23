using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SupervisorDialoguePlayModeTests
    {
        private MiniGameBootstrap host;
        private MiniGameYarnBridge bridge;
        private SupervisorDoorwayView supervisor;
        private GameRules rules;
        private PrototypeContent content;
        private GameRuntime runtime;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.MainMenu.TryDismissForEditorDebug();
            host.StartCampaign();
            bridge = Object.FindObjectOfType<MiniGameYarnBridge>();
            supervisor = host.GetComponent<SupervisorDoorwayView>();
            host.Effects.PrepareForRuntimeChange(); host.Effects.enabled = false;
            host.Dreams?.PrepareForRuntimeChange();
            if (host.Dreams != null) host.Dreams.enabled = false;
            bridge.enabled = false; host.Player.enabled = false;
            host.Screen.DialogueCharactersPerSecond = 0;
            rules = new GameRules("supervisor-content-fixture",
                Enumerable.Range(0, 4).Select(day => new PatientDefinition("supervisor-fixture-" + day, "测试对象" + day,
                    day, 0, null, PatientPresence.TreatmentEnded)),
                Enumerable.Range(0, 4).Select(day => new DayDefinition(day, 1, new[] {
                    new MeetingDefinition("supervisor-fixture-" + day, new[] {
                        new RoundDefinition("supervisor-fixture-question-" + day, new[] {
                            new ResponseDefinition("supervisor-fixture-answer-" + day, "supervisor-fixture-branch-" + day, 0) }) },
                        null, 100, 0, new Dictionary<Disposition,string> { [Disposition.Purify] = "supervisor-fixture-purify-" + day }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            content = new PrototypeContent(rules);
            runtime = new GameRuntime(rules, content, true);
            Accept(runtime.BeginDayZeroOpening());
            CompleteCue();
            AdvanceNight();
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(1));
            Bind();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MiniGameSessionHost.Release();
            yield return null;
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator EveryAuthoredLineAndExpressionPlaysOnceAcrossThreeMorningsAndTakenAway()
        {
            var data = SupervisorDialogueContent.Load();
            var expected = data.conversations.SelectMany(day => day.lines).ToArray();
            var seen = new List<string>();
            var displayedPages = new Dictionary<string, List<string>>();
            var pageCounts = new Dictionary<string, int>();
            var portraits = new HashSet<string>();
            var scanDays = new List<int>();
            host.Audio.Service.SoundPlayed += (channel, sound) =>
            {
                if (sound != MiniGameSound.Scan) return;
                Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_04"));
                scanDays.Add(runtime.GetView().Game.Day);
            };
            float actorHeight = supervisor.Surface.transform.position.y;
            foreach (int day in new[] { 1, 2, 3, 4 })
            {
                if (day < 4) Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
                int target = data.conversations.Where(item => item.day <= day).Sum(item => item.lines.Length);
                int guard = 0;
                while ((seen.Count < target || runtime.GetView().Cue != null || bridge.IsRunning) && guard++ < 400)
                {
                    yield return null;
                    string source = bridge.SupervisorSourceId;
                    if (!string.IsNullOrEmpty(source) && !seen.Contains(source))
                    {
                        var line = expected[seen.Count];
                        Assert.That(source, Is.EqualTo(SupervisorDialogueContent.SourcePrefix + line.id));
                        Assert.That(supervisor.CurrentPortraitId, Is.EqualTo(line.portraitId));
                        Assert.That(host.Screen.CurrentLinePageCount, Is.GreaterThan(0), source);
                        displayedPages.Add(source, new List<string>());
                        pageCounts.Add(source, host.Screen.CurrentLinePageCount);
                        Assert.That(supervisor.Surface.transform.position.y, Is.EqualTo(actorHeight).Within(.001f),
                            "Door avoidance may move the supervisor sideways without changing portrait height.");
                        seen.Add(source); portraits.Add(line.portraitId);
                        Assert.That(scanDays.Count, Is.EqualTo(expected.Take(seen.Count)
                            .Where(item => item.portraitId == "JD_04").Select(item => item.day).Distinct().Count()),
                            "Only the first JD_04 line of each day may play Scan.");
                        if (day == 4 && seen.Count == 40 || day == 1 && (line.portraitId == "JD_04" || line.id.EndsWith("_015")))
                            yield return PrototypePlayModeTests.CaptureIfRequested(host, day == 4 ? "supervisor-red-ending"
                                : line.portraitId == "JD_05" ? "supervisor-green-check" : "supervisor-instrument");
                    }
                    var pageText = host.Screen.GetComponentsInChildren<UnityEngine.UI.Text>(true)
                        .Single(text => text.name == "DialogueBody").text;
                    bool pageWasRevealed = host.Screen.LineIsRevealed;
                    if (host.Screen.AdvanceWorldDialogue() && !string.IsNullOrEmpty(source)
                        && displayedPages.TryGetValue(source, out var pages) && pages.Count < pageCounts[source])
                    {
                        Assert.That(pageWasRevealed, Is.True, "The instant-text fixture must record a complete visible page: " + source);
                        pages.Add(pageText); // Record accepted page advances, including consecutive pages with identical text.
                    }
                }
                Assert.That(seen.Count, Is.EqualTo(target), bridge.Diagnostics);
                Assert.That(guard, Is.LessThan(400), bridge.Diagnostics);
                for (int index = 0; index < target; index++)
                {
                    string source = seen[index];
                    Assert.That(displayedPages[source].Count, Is.EqualTo(pageCounts[source]),
                        "Every visible page must be advanced before completing its source line: " + source);
                    Assert.That(string.Concat(displayedPages[source]), Is.EqualTo(expected[index].text),
                        "The ordered visible pages must reconstruct the complete authored line: " + source);
                }
                if (day < 4) { bridge.enabled = false; AdvanceNight(); bridge.enabled = true; bridge.PresentCurrent(); }
            }
            Assert.That(seen.Count, Is.EqualTo(46));
            Assert.That(scanDays, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(portraits, Is.EquivalentTo(Enumerable.Range(1, 6).Select(index => "JD_" + index.ToString("00"))));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_06"));
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator DoorwayDialogueClicksAndPortraitChangesKeepActorAndCameraPoseStable()
        {
            var lines = SupervisorDialogueContent.Load().conversations.Single(day => day.day == 1).lines;
            Assert.That(lines.Length, Is.GreaterThan(1));
            Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
            yield return Reach(SupervisorDialogueContent.SourcePrefix + lines[0].id, false);
            // Once the first line is visible, opening-door avoidance may no longer move either subject.
            // Do not wait for a return animation here: that would hide the reported dialogue-time drift.
            var camera = host.Player.ViewCamera;
            var actor = supervisor.Surface.transform;
            var cameraPosition = camera.transform.position;
            var cameraRotation = camera.transform.rotation;
            float cameraFov = camera.fieldOfView;
            var actorPosition = actor.position;
            var actorRotation = actor.rotation;
            var seenPortraits = new HashSet<string>();
            int acceptedClicks = 0;
            for (int index = 0; index < lines.Length; index++)
            {
                string expectedSource = SupervisorDialogueContent.SourcePrefix + lines[index].id;
                Assert.That(bridge.SupervisorSourceId, Is.EqualTo(expectedSource), bridge.Diagnostics);
                Assert.That(supervisor.CurrentPortraitId, Is.EqualTo(lines[index].portraitId));
                bool firstPortrait = seenPortraits.Add(supervisor.CurrentPortraitId);
                for (int frame = 0; frame < 2; frame++)
                {
                    yield return null;
                    AssertDialoguePose(camera, actor, cameraPosition, cameraRotation, cameraFov,
                        actorPosition, actorRotation, expectedSource);
                }
                if (firstPortrait)
                {
                    yield return PrototypePlayModeTests.CaptureIfRequested(host, "supervisor-stable-" + supervisor.CurrentPortraitId);
                    AssertDialoguePose(camera, actor, cameraPosition, cameraRotation, cameraFov,
                        actorPosition, actorRotation, expectedSource + " after portrait capture");
                }
                if (index == lines.Length - 1) break; // The normal dialogue exit is allowed to restore the camera.
                int guard = 0;
                while (bridge.SupervisorSourceId == expectedSource && guard++ < 40)
                {
                    if (host.Screen.AdvanceWorldDialogue()) acceptedClicks++;
                    AssertDialoguePose(camera, actor, cameraPosition, cameraRotation, cameraFov,
                        actorPosition, actorRotation, expectedSource + " immediately after clicking");
                    yield return null;
                    AssertDialoguePose(camera, actor, cameraPosition, cameraRotation, cameraFov,
                        actorPosition, actorRotation, expectedSource + " after rendering the next dialogue frame");
                }
                Assert.That(bridge.SupervisorSourceId, Is.Not.EqualTo(expectedSource), bridge.Diagnostics);
            }
            Assert.That(acceptedClicks, Is.GreaterThanOrEqualTo(lines.Length - 1));
            Assert.That(seenPortraits.Count, Is.GreaterThanOrEqualTo(2), "The regression must cover actual portrait changes.");
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator DirectDoorMorningCheckpointStartsAndAdvancesDialogueWithoutDoorView()
        {
            bridge.enabled = false;
            Accept(runtime.TryInteract(InteractionTarget.Door));
            Assert.That(runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(runtime.GetView().DoorViewActive, Is.False);
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            Bind();
            var lines = SupervisorDialogueContent.Load().conversations.Single(day => day.day == 1).lines;
            string firstSource = SupervisorDialogueContent.SourcePrefix + lines[0].id;
            yield return Reach(firstSource, false);
            Assert.That(runtime.GetView().DoorViewActive, Is.False,
                "Legacy direct-door checkpoints must not require the physical door-camera path.");
            bool acceptedClick = false;
            for (int page = 0; page < 40 && bridge.SupervisorSourceId == firstSource; page++)
            {
                acceptedClick |= host.Screen.AdvanceWorldDialogue();
                yield return null;
            }
            Assert.That(acceptedClick, Is.True, "The restored first line must accept dialogue input.");
            Assert.That(bridge.SupervisorSourceId, Is.EqualTo(SupervisorDialogueContent.SourcePrefix + lines[1].id), bridge.Diagnostics);
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator AllSixPortraitFeetAndPixelScaleRemainRegisteredAcrossCheckpointRebinds()
        {
            bridge.enabled = false;
            yield return null;
            var doorway = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            var surface = supervisor.Surface;
            Assert.That(supervisor.SetPortrait("JD_01"), Is.True);
            var position = surface.transform.position;
            var rotation = surface.transform.rotation;
            var scale = surface.transform.localScale;
            float worldUnitsPerPixel = surface.transform.lossyScale.y / surface.sprite.pixelsPerUnit;
            for (int index = 1; index <= 6; index++)
            {
                string portrait = "JD_" + index.ToString("00");
                Assert.That(supervisor.SetPortrait(portrait), Is.True);
                yield return null;
                Assert.That(Vector3.Distance(surface.transform.position, position), Is.LessThan(.001f), portrait);
                Assert.That(Quaternion.Angle(surface.transform.rotation, rotation), Is.LessThan(.01f), portrait);
                Assert.That(Vector3.Distance(surface.transform.localScale, scale), Is.LessThan(.0001f), portrait);
                var sprite = surface.sprite;
                Assert.That(sprite.rect.width, Is.EqualTo(sprite.texture.width), "Keep the original PNG canvas: " + portrait);
                Assert.That(sprite.rect.height, Is.EqualTo(sprite.texture.height), portrait);
                Assert.That(surface.transform.lossyScale.y / sprite.pixelsPerUnit,
                    Is.EqualTo(worldUnitsPerPixel).Within(.000001f), "Expression changes must preserve the JD_01 pixel scale: " + portrait);
                Vector2 measuredFoot = ReadVisibleFootAnchor(sprite.texture);
                Assert.That(Vector2.Distance(sprite.pivot, measuredFoot), Is.LessThan(2),
                    "The pivot must register the visible feet independently of the transparent PNG margins: " + portrait
                    + ", pivot=" + sprite.pivot + ", measured=" + measuredFoot);
                Vector3 localFoot = new Vector3(measuredFoot.x - sprite.pivot.x, measuredFoot.y - sprite.pivot.y, 0)
                    / sprite.pixelsPerUnit;
                var visibleFoot = surface.transform.TransformPoint(localFoot);
                Assert.That(Vector3.Distance(visibleFoot, position), Is.LessThan(worldUnitsPerPixel * 2), portrait);
                var size = surface.bounds.size;

                supervisor.Bind(runtime, host.Player, doorway);
                Assert.That(supervisor.Surface, Is.SameAs(surface), "Rebinding must reuse the world actor: " + portrait);
                Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_01"));
                Assert.That(Vector3.Distance(surface.transform.position, position), Is.LessThan(.001f), portrait);
                Assert.That(Quaternion.Angle(surface.transform.rotation, rotation), Is.LessThan(.01f), portrait);
                Assert.That(Vector3.Distance(surface.transform.localScale, scale), Is.LessThan(.0001f),
                    "Rebinding after a differently sized PNG must restore the reference scale: " + portrait);
                Assert.That(supervisor.SetPortrait(portrait), Is.True);
                yield return null;
                Assert.That(Vector3.Distance(surface.bounds.size, size), Is.LessThan(.001f), portrait);
                Assert.That(Vector3.Distance(surface.transform.TransformPoint(localFoot), visibleFoot), Is.LessThan(.001f), portrait);
            }
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator ScanPortraitPlaysRealSamplesThroughTheActiveAudioOwner()
        {
            Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
            yield return Reach("supervisor__P02_D1_R1_COMMON_014");
            Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_04"));
            var source = host.Audio.Service.GetSource(MiniGameAudioChannel.Scanner);
            Assert.That(source.outputAudioMixerGroup, Is.EqualTo(host.Audio.Profile.SfxGroup));
            var samples = new float[1024];
            float peak = 0, deadline = Time.realtimeSinceStartup + 1.5f;
            while (peak < .0001f && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                source.GetOutputData(samples, 0);
                peak = Mathf.Max(peak, samples.Max(value => Mathf.Abs(value)));
            }
            Assert.That(source.isPlaying, Is.True);
            Assert.That(peak, Is.GreaterThan(.0001f), "The scan must output audio samples, beyond emitting a presentation event.");
            runtime.SetPaused(true);
            yield return null;
            Assert.That(source.isPlaying, Is.False);
            runtime.SetPaused(false);
            yield return null;
            Assert.That(source.isPlaying, Is.True);
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator LoadingTheScanPortraitDoesNotReplayTheSameDaysScan()
        {
            int scans = 0;
            host.Audio.Service.SoundPlayed += (channel, sound) => { if (sound == MiniGameSound.Scan) scans++; };
            Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
            yield return Reach("supervisor__P02_D1_R1_COMMON_014");
            Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_04"));
            Assert.That(scans, Is.EqualTo(1));
            var session = MiniGameSessionHost.Install(runtime, rules, content, host.DreamProfile);
            string path = Path.Combine(Application.temporaryCachePath, "supervisor-scan-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                session.SaveTo(path);
                Assert.That(File.Exists(path), Is.True, session.LastMessage);
                Assert.That(session.SupervisorScanDays, Is.EqualTo(new[] { 1 }));
                session.LoadFrom(path);
                runtime = session.Runtime;
                yield return Reach("supervisor__P02_D1_R1_COMMON_014", false);
                Assert.That(scans, Is.EqualTo(1), "Restoring JD_04 must preserve today's Scan deduplication.");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator PausedCheckpointResumesTheSameAuthoredLineAndPortrait()
        {
            Assert.That(host.Player.ViewCamera.GetComponent<DoorViewCamera>().TryEnter(), Is.True);
            yield return Reach("supervisor__P02_D1_R1_COMMON_004");
            var saved = bridge.CaptureSupervisorCheckpoint();
            Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_03"));
            Accept(runtime.SetPaused(true));
            Assert.That(host.Screen.AdvanceWorldDialogue(), Is.False);
            var session = MiniGameSessionHost.Install(runtime, rules, content, host.DreamProfile);
            string path = Path.Combine(Application.temporaryCachePath, "supervisor-save-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                session.SaveTo(path);
                Assert.That(File.Exists(path), Is.True, session.LastMessage);
                string before = runtime.Save();
                session.LoadFrom(path);
                Assert.That(session.Runtime.Save(), Is.EqualTo(before), session.LastMessage);
                runtime = session.Runtime;
                Accept(runtime.SetPaused(false));
                yield return Reach(saved.sourceId, false);
                Assert.That(bridge.SupervisorSourceId, Is.EqualTo(saved.sourceId));
                Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_03"));
                Assert.That(bridge.CaptureSupervisorCheckpoint().revision, Is.EqualTo(saved.revision));
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [UnityTest, Timeout(25000)]
        public IEnumerator TakenAwayCheckpointKeepsCameraPoseAndZoom()
        {
            bridge.enabled = false;
            for (int day = 1; day <= 3; day++)
            {
                Accept(runtime.TryInteract(InteractionTarget.Door)); CompleteCue(); CompleteCue(); AdvanceNight();
            }
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            Bind();
            yield return Reach("supervisor__P02_D4_R1_COMMON_003");
            var camera = host.Player.ViewCamera;
            var position = camera.transform.position; var rotation = camera.transform.rotation;
            float fov = camera.fieldOfView;
            Assert.That(fov, Is.EqualTo(70).Within(.001f));
            Accept(runtime.SetPaused(true));
            var session = MiniGameSessionHost.Install(runtime, rules, content, host.DreamProfile);
            string path = Path.Combine(Application.temporaryCachePath, "supervisor-ending-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                session.SaveTo(path); session.LoadFrom(path); runtime = session.Runtime;
                Accept(runtime.SetPaused(false));
                yield return Reach("supervisor__P02_D4_R1_COMMON_003", false);
                Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(fov).Within(.001f));
                Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_06"));
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static Vector2 ReadVisibleFootAnchor(Texture2D source)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
            try
            {
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readable.Apply();
                var pixels = readable.GetPixels32();
                int minimumY = source.height, maximumY = -1;
                for (int y = 0; y < source.height; y++)
                    for (int x = 0; x < source.width; x++)
                        if (pixels[y * source.width + x].a > 16)
                        {
                            minimumY = Mathf.Min(minimumY, y);
                            maximumY = Mathf.Max(maximumY, y);
                        }
                Assert.That(maximumY, Is.GreaterThanOrEqualTo(minimumY), source.name + " must contain visible pixels.");
                int upperFootRow = minimumY + Mathf.Max(1, Mathf.CeilToInt((maximumY - minimumY + 1) * .1f));
                long sumX = 0;
                int count = 0;
                for (int y = minimumY; y < upperFootRow; y++)
                    for (int x = 0; x < source.width; x++)
                        if (pixels[y * source.width + x].a > 16) { sumX += x; count++; }
                Assert.That(count, Is.GreaterThan(0));
                return new Vector2((float)sumX / count, minimumY);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(readable);
            }
        }

        private void AssertDialoguePose(Camera camera, Transform actor, Vector3 cameraPosition,
            Quaternion cameraRotation, float cameraFov, Vector3 actorPosition, Quaternion actorRotation, string source)
        {
            var doorway = camera.GetComponent<DoorViewCamera>();
            var door = Object.FindObjectOfType<HingedDoor>();
            string detail = "check=" + source + ", sourceId=" + bridge.SupervisorSourceId
                + ", cameraPhase=" + doorway.Phase + ", door.IsMoving=" + (door != null ? door.IsMoving.ToString() : "missing")
                + ", supervisorPosition=" + actor.position.ToString("F4")
                + ", supervisorRotation=" + actor.rotation.eulerAngles.ToString("F3")
                + ", cameraPosition=" + camera.transform.position.ToString("F4")
                + ", cameraRotation=" + camera.transform.rotation.eulerAngles.ToString("F3")
                + ", cameraFov=" + camera.fieldOfView.ToString("F3");
            Assert.That(Vector3.Distance(camera.transform.position, cameraPosition), Is.LessThan(.001f),
                "Dialogue input must not move the camera: " + detail);
            Assert.That(Quaternion.Angle(camera.transform.rotation, cameraRotation), Is.LessThan(.01f),
                "Dialogue input must not rotate the camera: " + detail);
            Assert.That(camera.fieldOfView, Is.EqualTo(cameraFov).Within(.001f),
                "Dialogue input must not change camera zoom: " + detail);
            Assert.That(Vector3.Distance(actor.position, actorPosition), Is.LessThan(.001f),
                "The supervisor must remain stationary while dialogue is visible: " + detail);
            Assert.That(Quaternion.Angle(actor.rotation, actorRotation), Is.LessThan(.01f),
                "Portrait changes must not rotate the supervisor: " + detail);
        }

        private IEnumerator Reach(string source, bool advance = true)
        {
            int guard = 0;
            while (bridge.SupervisorSourceId != source && guard++ < 200)
            {
                yield return null;
                if (advance && bridge.SupervisorSourceId != source) host.Screen.AdvanceWorldDialogue();
            }
            Assert.That(bridge.SupervisorSourceId, Is.EqualTo(source), bridge.Diagnostics);
        }

        private void Bind()
        {
            host.Audio.Bind(runtime);
            host.Screen.Bind(runtime); host.Screen.UseYarnPresentation(); host.Screen.SetTransitionLocked(false);
            host.Player.Bind(runtime, host.Screen);
            var door = Object.FindObjectOfType<HingedDoor>(); door.Bind(runtime);
            var doorway = host.Player.ViewCamera.GetComponent<DoorViewCamera>(); doorway.Bind(runtime);
            supervisor.Bind(runtime, host.Player, doorway);
            bridge.Bind(runtime, host.Screen, supervisor); bridge.enabled = true;
        }

        private void AdvanceNight()
        {
            if (runtime.GetView().DoorViewActive)
            {
                Accept(runtime.ExitDoorView());
                while (runtime.GetView().Cue != null) CompleteCue();
            }
            if (runtime.GetView().Game.Phase != GamePhase.Evening)
            {
                if (!runtime.GetView().TerminalOpen) { Accept(runtime.TryInteract(InteractionTarget.Computer)); CompleteCue(); }
                if (runtime.GetView().TerminalPage == TerminalPage.Directory)
                    Accept(runtime.EnterSelectedPatient("supervisor-fixture-" + runtime.GetView().Game.Day));
                if (runtime.GetView().Cue?.Kind == PresentationKind.Introduction) CompleteCue();
                var view = runtime.GetView();
                Accept(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Game.AvailableResponseIds[0]));
                CompleteCue();
                Accept(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify));
                while (runtime.GetView().Cue != null) CompleteCue();
                if (runtime.GetView().TerminalOpen) { Accept(runtime.LeaveTerminal()); while (runtime.GetView().Cue != null) CompleteCue(); }
            }
            Accept(runtime.TryInteract(InteractionTarget.Bed)); CompleteCue(); CompleteCue();
            Accept(runtime.TryInteract(InteractionTarget.Bed));
        }

        private void CompleteCue()
        {
            Assert.That(runtime.GetView().Cue, Is.Not.Null);
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
        }

        private static void Accept(RuntimeResult result) => Assert.That(result.Accepted, Is.True, result.Reason);
    }
}
