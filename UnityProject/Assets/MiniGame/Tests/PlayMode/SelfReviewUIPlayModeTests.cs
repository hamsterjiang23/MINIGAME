using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    [Category("TerminalV0824")]
    public sealed class SelfReviewUIPlayModeTests
    {
        private bool previousSkipStartup;
        private void EnterDirectWorldMode()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            if (MainMenuController.Current != null) Object.DestroyImmediate(MainMenuController.Current.gameObject);
        }
        [UnityTearDown]
        public IEnumerator RestoreStartupModeAndUnloadFixtureRoom()
        {
            MainMenuController.SkipStartup = previousSkipStartup;
            MiniGameSessionHost.Release();
            if (MainMenuController.Current != null) Object.Destroy(MainMenuController.Current.gameObject);
            if (host != null)
            {
                host.enabled = false;
                if (host.Player != null) host.Player.enabled = false;
                if (host.Dreams != null) host.Dreams.PrepareForRuntimeChange();
                if (host.Effects != null) host.Effects.PrepareForRuntimeChange();
            }
            var room = SceneManager.GetSceneByName(MiniGameSessionHost.CombinedScene);
            if (room.IsValid() && room.isLoaded)
            {
                var empty = SceneManager.CreateScene("Self-review fixture cleanup " + Guid.NewGuid().ToString("N"));
                SceneManager.SetActiveScene(empty);
                yield return SceneManager.UnloadSceneAsync(room);
            }
            yield return null;
            Assert.That(Object.FindObjectsOfType<MiniGameScreen>(), Is.Empty,
                "The Combined fixture must release its terminal raycaster before another isolated UI test starts.");
        }
        private MiniGameBootstrap host;
        private GameRuntime runtime;
        private SelfReviewMonitor monitor;

        [UnitySetUp]
        public IEnumerator LoadRoomWithoutRegisteringTheFixtureRuntimeAsASceneTransition()
        {
            EnterDirectWorldMode();
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            host.enabled = false;
            host.Player.enabled = false;
            if (host.Dreams != null) host.Dreams.enabled = false;
            monitor = host.GetComponent<SelfReviewMonitor>();
            if (monitor == null) monitor = host.gameObject.AddComponent<SelfReviewMonitor>();
            Bind(CreateRuntime(false));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator SelfDirectoryUsesAllSixteenAuthoredLinesBeforeOfferingOnlyPurification()
        {
            yield return EnterTerminal();
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Directory));
            Assert.That(runtime.GetView().Patients.Select(patient => patient.Id), Is.EqualTo(new[] { GameRuntime.SelfPatientId }));
            Assert.That(monitor.IsActive, Is.False, "Self review uses the ordinary patient presentation, not the obsolete proxy feed.");
            Assert.That(runtime.ConfirmSelfPurification().Accepted, Is.False);
            yield return CompleteSelfReview();
            var buttons = host.Screen.GetComponentsInChildren<Button>(false)
                .Where(button => button.IsActive() && button.name.StartsWith("Disposition-", StringComparison.Ordinal)).ToArray();
            Assert.That(buttons.Select(button => button.name), Is.EqualTo(new[] { "Disposition-Purify" }));
            Assert.That(runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(16));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator BothSelfReviewPortraitsUseTheWardBackdropAndAmberMonitorInCombinedScene()
        {
            const string captureKey = "MINIGAME_ENDING_CAPTURE_DIR";
            string previousCaptureDirectory = Environment.GetEnvironmentVariable(captureKey);
            if (string.IsNullOrEmpty(previousCaptureDirectory))
                Environment.SetEnvironmentVariable(captureKey, CaptureDirectory());
            try
            {
                yield return EnterTerminal();
                var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
                var profile = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
                Assert.That(catalog, Is.Not.Null);
                Assert.That(profile, Is.Not.Null);
                Assert.That(profile.Backdrop, Is.Not.Null);
                Assert.That(profile.Find(GameRuntime.SelfPatientId), Is.Not.Null);
                var feed = host.Screen.GetComponentsInChildren<PatientSurveillanceFeed>(true).Single();
                var captured = new HashSet<string>();
                yield return CompleteSelfReview(portraitId =>
                {
                    if (!captured.Add(portraitId)) return;
                    Assert.That(feed.PatientId, Is.EqualTo(GameRuntime.SelfPatientId));
                    Assert.That(feed.CurrentSprite, Is.SameAs(catalog.Find(portraitId)));
                    Assert.That(feed.IsShowing, Is.True, "Self review must use the normal ward surveillance feed.");
                    Assert.That(monitor.IsActive, Is.False);
                    var fallbackPortraits = host.Screen.GetComponentsInChildren<Image>(true)
                        .Where(image => image.name == "CurrentPortrait" || image.name == "PreviousPortrait");
                    Assert.That(fallbackPortraits.All(image => !image.enabled), Is.True);

                    var pixels = ReadMonitorPixels(feed.Output);
                    Assert.That(pixels.Count(pixel => pixel.r > 20 || pixel.g > 20 || pixel.b > 20),
                        Is.GreaterThan(pixels.Length / 4), "The ward feed must not be a black background.");
                    long red = pixels.Sum(pixel => (long)pixel.r);
                    long green = pixels.Sum(pixel => (long)pixel.g);
                    long blue = pixels.Sum(pixel => (long)pixel.b);
                    Assert.That(red, Is.GreaterThan(green * 1.02), "Self review must retain the warm monitor filter.");
                    Assert.That(green, Is.GreaterThan(blue * 1.1), "Self review must retain the amber monitor filter.");

                    var backdrop = feed.ActiveRoom.transform.parent.GetComponentsInChildren<SpriteRenderer>(true)
                        .Single(renderer => renderer.sprite != null && renderer.sprite.texture == profile.Backdrop);
                    Assert.That(backdrop.enabled && backdrop.gameObject.activeInHierarchy, Is.True);
                    var camera = feed.ViewCamera;
                    camera.Render();
                    var withBackdrop = ReadMonitorPixels(camera.targetTexture);
                    try
                    {
                        backdrop.enabled = false;
                        camera.Render();
                        var withoutBackdrop = ReadMonitorPixels(camera.targetTexture);
                        Assert.That(withBackdrop.Zip(withoutBackdrop, (a, b) => a.Equals(b) ? 0 : 1).Sum(),
                            Is.GreaterThan(withBackdrop.Length / 4), "The authored ward must contribute visible background pixels.");
                    }
                    finally { backdrop.enabled = true; camera.Render(); }
                    host.Screen.TerminalDisplay.RenderFrame();
                    CaptureMonitor(feed.Output, "self-review-" + portraitId + "-feed");
                    CaptureMonitor(host.Screen.TerminalDisplay.Texture, "self-review-" + portraitId + "-terminal");
                    EndingVisualCapture.Request("self-review-" + portraitId + "-combined", runtime);
                });
                Assert.That(captured, Is.EquivalentTo(new[] { "SJ01", "SJ02" }));
                // Flush reports the two completed captures in reverse request order.
                LogAssert.Expect(LogType.Log, new Regex(@"^MINIGAME_ENDING_CAPTURE self-review-SJ02-combined .+[\\/]self-review-SJ02-combined-frame\d+\.png$"));
                LogAssert.Expect(LogType.Log, new Regex(@"^MINIGAME_ENDING_CAPTURE self-review-SJ01-combined .+[\\/]self-review-SJ01-combined-frame\d+\.png$"));
                yield return EndingVisualCapture.Flush();
                LogAssert.NoUnexpectedReceived();
            }
            finally { Environment.SetEnvironmentVariable(captureKey, previousCaptureDirectory); }
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator ConfirmationKeysDoNotPurifyAndThePointerHidesNormalEndingUiBeforeTheSceneHandoff()
        {
            yield return EnterTerminal();
            yield return CompleteSelfReview();
            var button = FindPurify();
            string awaitingClick = runtime.Save();
            foreach (var key in new[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space })
            {
                Assert.That(host.Screen.HandleTerminalKey(key), Is.True);
                Assert.That(runtime.Save(), Is.EqualTo(awaitingClick), "Self purification requires an explicit left click.");
            }
            button.OnSubmit(new BaseEventData(EventSystem.current));
            Assert.That(runtime.Save(), Is.EqualTo(awaitingClick));
            Click(button);
            string committed = runtime.Save();
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Assert.That(runtime.GetView().Game.CurrentEncounterId, Is.Null);
            button.OnSubmit(new BaseEventData(EventSystem.current));
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            Assert.That(runtime.Save(), Is.EqualTo(committed));
            Assert.That(host.Screen.Canvas.transform.Find("Conversation").gameObject.activeInHierarchy, Is.False);
            Assert.That(host.Screen.Canvas.transform.Find("ShiftHeader"), Is.Null);
            Assert.That(monitor.IsActive, Is.False);
            yield return null;
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator PointerPurificationRespectsTransitionAndPauseAndCommitsOnce()
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False, "The camera approach must finish first.");
            yield return Until(() => runtime.GetView().Cue == null && !host.Effects.IsTransitioning && host.Screen.TerminalReveal >= .999f,
                "self-review camera entry");
            yield return CompleteSelfReview();
            var button = FindPurify();
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            Click(button);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.SelfReview));
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            Assert.That(runtime.SetPaused(false).Accepted, Is.True);
            yield return null;
            Click(button);
            string committed = runtime.Save();
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.DreamSea));
            Click(button);
            Assert.That(runtime.Save(), Is.EqualTo(committed));
            yield return null;
        }
        [UnityTest, Timeout(45000)]
        public IEnumerator DreamProjectionHasNoConversationContinueHeaderOrKeyboardSkip()
        {
            Bind(CreateRuntime(true));
            string token = runtime.GetView().Cue.Token;
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
                Assert.That(host.Screen.Canvas.transform.Find("Conversation").gameObject.activeInHierarchy, Is.False);
                Assert.That(host.Screen.Canvas.transform.Find("ShiftHeader"), Is.Null);
                Assert.That(host.Screen.GetComponentsInChildren<Button>(false).Any(button => button.name == "Continue"), Is.False);
                Assert.That(host.Screen.HandleTerminalKey(KeyCode.Space), Is.False);
                Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
                Assert.That(host.Player.InteractFocused(), Is.False);
                Assert.That(runtime.GetView().Cue.Token, Is.EqualTo(token));
                Assert.That(monitor.IsActive, Is.False);
            }
            LogAssert.NoUnexpectedReceived();
        }

        private void Bind(GameRuntime game)
        {
            runtime = game;
            host.Screen.Bind(runtime);
            host.Player.Bind(runtime, host.Screen);
            host.Effects.Bind(runtime, host.Screen);
            monitor.Configure(host.Player);
            monitor.Bind(runtime);
            host.Screen.BindSelfMonitor(monitor.Output);
            host.GetComponent<MiniGameYarnBridge>().Bind(runtime, host.Screen);
            host.GetComponent<EndingDialoguePresenter>().Bind(runtime, host.Screen, null);
            host.Screen.DialogueCharactersPerSecond = 0;
        }

        private IEnumerator EnterTerminal()
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Effects.IsTransitioning && host.Screen.TerminalReveal >= .999f,
                "self-review terminal directory");
        }

        private MiniGameAudioButton FindPurify() => host.Screen.GetComponentsInChildren<MiniGameAudioButton>(true)
            .Single(button => button.name == "Disposition-Purify");

        private static void Click(MiniGameAudioButton button) => button.OnPointerClick(new PointerEventData(EventSystem.current)
        { button = PointerEventData.InputButton.Left, clickCount = 1 });

        private IEnumerator CompleteSelfReview(Action<string> inspectPortrait = null)
        {
            Assert.That(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId).Accepted, Is.True);
            for (int index = 0; index < 16; index++)
            {
                yield return null;
                Assert.That(runtime.CurrentEndingLine, Is.Not.Null);
                Assert.That(runtime.CurrentEndingLine.PortraitId, Is.EqualTo("SJ01").Or.EqualTo("SJ02"));
                if (inspectPortrait != null)
                {
                    inspectPortrait(runtime.CurrentEndingLine.PortraitId);
                    yield return null;
                }
                Assert.That(runtime.ConfirmSelfPurification().Accepted, Is.False);
                int pages = 0;
                while (runtime.GetView().Game.SelfReviewLineIndex == index && pages++ < 24)
                {
                    Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
                    yield return null;
                }
                Assert.That(runtime.GetView().Game.SelfReviewLineIndex, Is.EqualTo(index + 1));
            }
            Assert.That(runtime.GetView().SelfReviewReady, Is.True);
        }

        private static Color32[] ReadMonitorPixels(RenderTexture source)
        {
            var image = ReadMonitor(source);
            try { return image.GetPixels32(); }
            finally { Object.Destroy(image); }
        }

        private static Texture2D ReadMonitor(RenderTexture source)
        {
            Assert.That(source, Is.Not.Null);
            var previous = RenderTexture.active;
            var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                image.Apply();
                return image;
            }
            finally { RenderTexture.active = previous; }
        }

        private static void CaptureMonitor(RenderTexture source, string name)
        {
            string folder = CaptureDirectory();
            Directory.CreateDirectory(folder);
            var image = ReadMonitor(source);
            try { File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG()); }
            finally { Object.Destroy(image); }
        }

        private static string CaptureDirectory() => Path.GetFullPath(Path.Combine(Application.dataPath,
            "..", "..", "Artifacts", "HiddenEndingRevision20261023"));

        private IEnumerator Until(Func<bool> condition, string expectation)
        {
            float deadline = Time.realtimeSinceStartup + 12;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expectation + "; phase=" + runtime.GetView().Game.Phase
                + "; cue=" + runtime.GetView().Cue?.Kind + "; reveal=" + host.Screen.TerminalReveal);
        }

        private static GameRuntime CreateRuntime(bool stopAtDream)
        {
            // Hidden-ending eligibility counts the four formal patient histories, not arbitrary fixture IDs.
            var patient = new PatientDefinition(PrototypeRules.Child, "未安排会谈", 1, 10, null, PatientPresence.TreatmentEnded);
            var rules = new GameRules("self-monitor-test", new[] { patient }, Enumerable.Range(0, 4)
                .Select(day => new DayDefinition(day, 0, Array.Empty<MeetingDefinition>())),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new System.Collections.Generic.Dictionary<string, int>());
            var game = new GameRuntime(rules, new EmptyContent(), true);
            game.ConfigureEndings(EndingDialoguePresenter.LoadCatalog());
            for (int guard = 0; guard < 80; guard++)
            {
                var view = game.GetView();
                if (view.Game.Phase == GamePhase.SelfReview || (stopAtDream && view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream)) return game;
                RuntimeResult result;
                if (view.Cue != null) result = game.CompleteCue(view.Cue.Token);
                else if (view.Game.Phase == GamePhase.Evening) result = game.BeginNightDream();
                else result = game.TryInteract(view.Game.Phase == GamePhase.AfterDream ? InteractionTarget.Bed
                    : view.Game.Phase == GamePhase.MorningInspection ? InteractionTarget.Door : InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, result.Reason + "; day=" + view.Game.Day
                    + "; phase=" + view.Game.Phase + "; input=" + view.Input + "; page=" + view.TerminalPage);
            }
            throw new InvalidOperationException("The legal self-review fixture did not reach its checkpoint.");
        }

        private sealed class EmptyContent : IGameContent
        {
            public string Revision => "self-monitor-test";
            public DialogueContent GetDialogue(PlayerView view) => throw new InvalidOperationException("Self review must never request a fake patient's dialogue.");
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind)
                => throw new InvalidOperationException("Self review must never request a patient inspection.");
        }
    }
}
