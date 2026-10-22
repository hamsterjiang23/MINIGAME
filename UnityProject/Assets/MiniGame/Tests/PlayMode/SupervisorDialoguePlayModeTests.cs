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
                        Assert.That(host.Screen.GetComponentsInChildren<UnityEngine.UI.Text>(true).Single(text => text.name == "DialogueBody").text, Is.EqualTo(line.text));
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
                    host.Screen.AdvanceWorldDialogue();
                }
                Assert.That(seen.Count, Is.EqualTo(target), bridge.Diagnostics);
                Assert.That(guard, Is.LessThan(400), bridge.Diagnostics);
                if (day < 4) { bridge.enabled = false; AdvanceNight(); bridge.enabled = true; bridge.PresentCurrent(); }
            }
            Assert.That(seen.Count, Is.EqualTo(46));
            Assert.That(scanDays, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(portraits, Is.EquivalentTo(Enumerable.Range(1, 6).Select(index => "JD_" + index.ToString("00"))));
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            Assert.That(supervisor.CurrentPortraitId, Is.EqualTo("JD_06"));
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
