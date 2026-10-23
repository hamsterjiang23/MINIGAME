using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("InteractionWakeGm")]
    public sealed class MainMenuPersistencePlayModeTests
    {
        private readonly List<Object> objects = new List<Object>();
        private readonly List<string> files = new List<string>();
        private string directory;
        private PresentationEffects effects;
        private MiniGameScreen screen;

        [SetUp]
        public void SetUp()
        {
            MiniGameSessionHost.Release();
            directory = Path.Combine(Path.GetTempPath(), "minigame-menu-persistence-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (effects != null) effects.PrepareForRuntimeChange();
            MiniGameSessionHost.Release();
            foreach (var value in objects.AsEnumerable().Reverse()) if (value != null) Object.DestroyImmediate(value);
            objects.Clear();
            effects = null; screen = null;
            foreach (var path in files) if (File.Exists(path)) File.Delete(path);
            files.Clear();
            Assert.That(Path.GetFileName(directory), Does.StartWith("minigame-menu-persistence-"));
            if (Directory.Exists(directory)) Directory.Delete(directory, false);
        }

        [Test]
        public void ResumePriorityKeepsCorruptAutomaticRecordAheadOfManualFallback()
        {
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.Null);
            var manual = FilePath("campaign-save.json");
            File.WriteAllText(manual, "plain manual");
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.EqualTo(manual));
            var dreamManual = FilePath("campaign-dream-save.json");
            File.WriteAllText(dreamManual, "dream manual");
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.EqualTo(dreamManual));
            var automatic = FilePath("campaign-continue.json");
            File.WriteAllText(automatic, "invalid automatic record");
            Assert.That(MainMenuContinueStore.ResolveResumePath(directory), Is.EqualTo(automatic),
                "A bad automatic record must surface an error, never silently fall back to older progress.");
        }

        [Test]
        public void AtomicContinueSaveDoesNotOverwriteManualSavesAndReportsWriteFailure()
        {
            var rules = Rules();
            var runtime = new GameRuntime(rules, new FixtureContent(), true);
            var session = MiniGameSessionHost.Install(runtime, rules, new FixtureContent(), null);
            var automatic = FilePath("campaign-continue.json");
            var manual = FilePath("campaign-dream-save.json");
            File.WriteAllText(manual, "keep manual record");
            Assert.That(session.TrySaveTo(automatic, out var error), Is.True, error);
            string first = File.ReadAllText(automatic);
            runtime.SetPaused(true);
            Assert.That(session.TrySaveTo(automatic, out error), Is.True, error);
            Assert.That(File.ReadAllText(automatic), Is.Not.EqualTo(first));
            Assert.That(File.ReadAllText(manual), Is.EqualTo("keep manual record"));
            Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
            string beforeFailure = File.ReadAllText(automatic);
            Assert.That(session.TrySaveTo(directory, out error), Is.False, "A directory cannot be replaced with a save file.");
            Assert.That(error, Does.StartWith("保存失败"));
            Assert.That(File.ReadAllText(automatic), Is.EqualTo(beforeFailure));
            Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
        }

        [TestCase("{not json")]
        [TestCase("{\"FormatVersion\":99,\"Runtime\":\"old\"}")]
        [TestCase("{\"FormatVersion\":1,\"Runtime\":\"{}\"}")]
        public void InvalidLoadPreservesLiveRuntimeAndOriginalFile(string json)
        {
            var rules = Rules();
            var runtime = new GameRuntime(rules, new FixtureContent(), true);
            var session = MiniGameSessionHost.Install(runtime, rules, new FixtureContent(), null);
            var path = FilePath("campaign-continue.json");
            File.WriteAllText(path, json);
            string state = runtime.Save();
            Assert.That(session.TryLoadFrom(path, out var error), Is.False);
            Assert.That(error, Does.StartWith("读取失败"));
            Assert.That(session.Runtime, Is.SameAs(runtime));
            Assert.That(runtime.Save(), Is.EqualTo(state));
            Assert.That(File.ReadAllText(path), Is.EqualTo(json));
        }

        [Test]
        public void MismatchedSleepCheckpointIsRejectedBeforeReplacingLiveRuntime()
        {
            var rules = Rules();
            var content = new FixtureContent();
            var runtime = SleepingRuntime(rules, content, false);
            runtime.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            var session = MiniGameSessionHost.Install(runtime, rules, content, null);
            var path = FilePath("campaign-continue.json");
            var envelope = new Envelope
            {
                FormatVersion = 1, Runtime = runtime.Save(), HasSleepCheckpoint = true,
                Sleep = new SleepTransitionCheckpoint
                {
                    Day = 2, GamePhase = GamePhase.AfterDream, Phase = SleepTransitionPhase.BlackHold,
                    FadeElapsed = .8f, BlackHoldRemaining = .5f
                }
            };
            File.WriteAllText(path, JsonUtility.ToJson(envelope));
            Assert.That(session.TryLoadFrom(path, out var error), Is.False);
            Assert.That(error, Does.Contain("睡眠检查点"));
            Assert.That(session.Runtime, Is.SameAs(runtime));
        }

        [UnityTest]
        public IEnumerator PausedFadeRestoresOpacityAndOnlyItsRemainingTime()
        {
            var rules = Rules();
            var content = new FixtureContent();
            var runtime = SleepingRuntime(rules, content, false);
            CreateEffects(runtime);
            yield return Until(() => effects.CaptureSleepCheckpoint()?.FadeElapsed >= .35f, 2);
            runtime.SetPaused(true);
            var checkpoint = effects.CaptureSleepCheckpoint();
            Assert.That(checkpoint.Phase, Is.EqualTo(SleepTransitionPhase.FadingOut));
            Assert.That(checkpoint.FadeElapsed, Is.LessThan(.8f));
            float opacity = Fade().color.a;
            string oldToken = runtime.GetView().Cue.Token;
            var saved = PersistSleep(runtime, rules, content);
            Assert.That(saved.Sleep.FadeElapsed, Is.EqualTo(checkpoint.FadeElapsed));
            var restored = GameRuntime.Restore(saved.Runtime, rules, content);
            screen.Bind(restored);
            effects.Bind(restored, screen);
            Assert.That(effects.RestoreSleepCheckpoint(checkpoint), Is.True);
            Assert.That(Fade().color.a, Is.EqualTo(opacity).Within(.00001f));
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(effects.CaptureSleepCheckpoint().FadeElapsed, Is.EqualTo(checkpoint.FadeElapsed));
            float resumedAt = Time.realtimeSinceStartup;
            restored.SetPaused(false);
            yield return Until(() => restored.GetView().Cue?.Kind == PresentationKind.Dream, 4);
            Assert.That(Time.realtimeSinceStartup - resumedAt, Is.EqualTo((.8f - checkpoint.FadeElapsed) / .8f * 2.3f + checkpoint.BlackHoldRemaining).Within(.16f));
            Assert.That(restored.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(restored.CompleteCue(oldToken).Accepted, Is.False);
            Assert.That(effects.CaptureSleepCheckpoint(), Is.Null);
        }

        [UnityTest]
        public IEnumerator PausedAutomaticWakeRestoresRemainingTimeWithoutAdvancingTheDayAgain()
        {
            var rules = Rules();
            var content = new FixtureContent();
            var runtime = SleepingRuntime(rules, content, true);
            CreateEffects(runtime);
            string oldToken = runtime.GetView().Cue.Token;
            Assert.That(runtime.CompleteCue(oldToken).Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(Fade().color.a, Is.EqualTo(1), "Dream completion starts the wake sequence under black.");
            yield return Until(() => effects.CaptureSleepCheckpoint()?.Phase == SleepTransitionPhase.Waking
                && effects.CaptureSleepCheckpoint().WakeElapsed >= .2f, 2);
            runtime.SetPaused(true);
            var checkpoint = effects.CaptureSleepCheckpoint();
            Assert.That(checkpoint.WakeElapsed, Is.InRange(.2f, .5f));
            float opacity = Fade().color.a;
            var saved = PersistSleep(runtime, rules, content);
            Assert.That(saved.Sleep.WakeElapsed, Is.EqualTo(checkpoint.WakeElapsed));
            var restored = GameRuntime.Restore(saved.Runtime, rules, content);
            screen.Bind(restored);
            effects.Bind(restored, screen);
            Assert.That(effects.RestoreSleepCheckpoint(checkpoint), Is.True);
            Assert.That(Fade().color.a, Is.EqualTo(opacity).Within(.00001f));
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(effects.CaptureSleepCheckpoint().WakeElapsed, Is.EqualTo(checkpoint.WakeElapsed));
            Assert.That(restored.GetView().Game.Day, Is.EqualTo(2));
            float resumedAt = Time.realtimeSinceStartup;
            restored.SetPaused(false);
            yield return Until(() => effects.CaptureSleepCheckpoint() == null, 3);
            Assert.That(Time.realtimeSinceStartup - resumedAt, Is.EqualTo(2.3f - checkpoint.WakeElapsed).Within(.16f));
            Assert.That(restored.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(restored.CompleteCue(oldToken).Accepted, Is.False);
            yield return null;
            Assert.That(restored.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(Fade().color.a, Is.Zero);
        }

        private void CreateEffects(GameRuntime runtime)
        {
            var cameraObject = Track(new GameObject("Menu sleep checkpoint camera", typeof(Camera)));
            cameraObject.GetComponent<Camera>().enabled = false;
            screen = Track(new GameObject("Menu sleep checkpoint UI")).AddComponent<MiniGameScreen>();
            screen.Bind(runtime);
            effects = Track(new GameObject("Menu sleep checkpoint effects")).AddComponent<PresentationEffects>();
            effects.Configure(cameraObject.GetComponent<Camera>(), cameraObject.transform, null, null);
            effects.Bind(runtime, screen);
        }

        private Image Fade() => screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
        private Envelope PersistSleep(GameRuntime runtime, GameRules rules, IGameContent content)
        {
            var session = MiniGameSessionHost.Install(runtime, rules, content, null);
            var path = FilePath("campaign-continue.json");
            Assert.That(session.TrySaveTo(path, out var error), Is.True, error);
            var saved = JsonUtility.FromJson<Envelope>(File.ReadAllText(path));
            Assert.That(saved.HasSleepCheckpoint, Is.True);
            Assert.That(saved.Sleep.Matches(runtime.GetView()), Is.True);
            return saved;
        }
        private T Track<T>(T value) where T : Object { objects.Add(value); return value; }
        private string FilePath(string name) { var path = Path.Combine(directory, name); files.Add(path); return path; }

        private static IEnumerator Until(Func<bool> condition, float timeout)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, "Checkpoint transition timed out.");
        }

        private static GameRuntime SleepingRuntime(GameRules rules, IGameContent content, bool waking)
        {
            var runtime = new GameRuntime(rules, content, true);
            for (int step = 0; step < 120; step++)
            {
                var view = runtime.GetView();
                if (waking && view.Game.Day == 1 && view.Cue?.Kind == PresentationKind.Dream) return runtime;
                if (!waking && view.Game.Day == 1 && view.Cue == null && view.Game.Phase == GamePhase.Evening
                    && !view.TerminalOpen)
                {
                    Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
                    return runtime;
                }
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory) result = runtime.EnterSelectedPatient(view.Game.CurrentPatientId);
                else if (view.Input == InputContext.Dialogue) result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                    ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Treat)
                    : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "pick");
                else if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                    result = runtime.TryInteract(InteractionTarget.Bed);
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else result = runtime.TryInteract(InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, "Fixture could not advance from " + view.Game.Phase);
            }
            throw new InvalidOperationException("Sleep fixture exceeded its command bound.");
        }

        private static GameRules Rules()
        {
            var patient = new PatientDefinition("patient", "测试病人", 0, 0, null, PatientPresence.TreatmentEnded);
            return new GameRules("main-menu-persistence-fixture", new[] { patient }, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, 0, new[] { new MeetingDefinition(patient.Id,
                    new[] { new RoundDefinition("day" + day, new[] { new ResponseDefinition("pick", "branch", 1) }) },
                    null, 100, 0, new Dictionary<Disposition, string> { [Disposition.Treat] = "treated" }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new Dictionary<string, int>());
        }

        private sealed class FixtureContent : IGameContent
        {
            public string Revision => "main-menu-persistence-content";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "测试病人", "测试对白",
                view.AvailableResponseIds.Select(id => new ResponseContent(id, "回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind, "观察", "记录");
        }

        [Serializable]
        private sealed class Envelope
        {
            public int FormatVersion;
            public string Runtime;
            public bool HasSleepCheckpoint;
            public SleepTransitionCheckpoint Sleep;
        }
    }
}
