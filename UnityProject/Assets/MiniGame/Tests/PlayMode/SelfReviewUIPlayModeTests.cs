using System;
using System.Collections;
using System.Linq;
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
    public sealed class SelfReviewUIPlayModeTests
    {
        private bool previousSkipStartup;
        private void EnterDirectWorldMode()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            if (MainMenuController.Current != null) Object.DestroyImmediate(MainMenuController.Current.gameObject);
        }
        [TearDown]
        public void RestoreStartupMode() => MainMenuController.SkipStartup = previousSkipStartup;
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
        public IEnumerator EnterPurifiesOnceAndHidesNormalEndingUiBeforeTheSceneHandoff()
        {
            yield return EnterTerminal();
            yield return CompleteSelfReview();
            var button = FindPurify();
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.True);
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

        private IEnumerator CompleteSelfReview()
        {
            Assert.That(runtime.EnterSelectedPatient(GameRuntime.SelfPatientId).Accepted, Is.True);
            for (int index = 0; index < 16; index++)
            {
                yield return null;
                Assert.That(runtime.CurrentEndingLine, Is.Not.Null);
                Assert.That(runtime.CurrentEndingLine.PortraitId, Is.EqualTo("SJ01").Or.EqualTo("SJ02"));
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
        private IEnumerator Until(Func<bool> condition, string expectation)
        {
            float deadline = Time.realtimeSinceStartup + 12;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expectation + "; phase=" + runtime.GetView().Game.Phase
                + "; cue=" + runtime.GetView().Cue?.Kind + "; reveal=" + host.Screen.TerminalReveal);
        }

        private static GameRuntime CreateRuntime(bool stopAtDream)
        {
            var patient = new PatientDefinition("unused", "未安排会谈", 1, 10, null, PatientPresence.TreatmentEnded);
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
                Assert.That(result.Accepted, Is.True, result.Reason);
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
