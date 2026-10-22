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
        private MiniGameBootstrap host;
        private GameRuntime runtime;
        private SelfReviewMonitor monitor;

        [UnitySetUp]
        public IEnumerator LoadRoomWithoutRegisteringTheFixtureRuntimeAsASceneTransition()
        {
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
        public IEnumerator SelfReviewShowsTheLiveSeatedPlayerAndOnlyPurification()
        {
            Assert.That(monitor.Output, Is.Not.Null, "Configure allocates the output before self-review entry.");
            Assert.That(monitor.IsActive, Is.False);
            yield return EnterTerminal();
            var buttons = host.Screen.GetComponentsInChildren<Button>(false).Where(button => button.IsActive()).ToArray();
            Assert.That(buttons.Select(button => button.name), Is.EqualTo(new[] { "SelfPurify" }));
            Assert.That(buttons[0].GetComponentInChildren<Text>().text, Is.EqualTo("净化"));
            var feed = host.Screen.GetComponentsInChildren<RawImage>(false).Single(image => image.name == "SelfMonitorFeed");
            Assert.That(feed.texture, Is.SameAs(monitor.Output));
            Assert.That(monitor.Output.IsCreated(), Is.True);
            Assert.That(monitor.IsActive, Is.True);
            Assert.That(monitor.ViewCamera.GetComponent<AudioListener>(), Is.Null, "The monitor must not add another listener.");
            Assert.That(host.Player.ViewCamera.cullingMask & (1 << SelfReviewMonitor.ProxyLayer), Is.Zero);
            Assert.That(monitor.ViewCamera.cullingMask & (1 << SelfReviewMonitor.ProxyLayer), Is.Not.Zero);
            Assert.That(monitor.ProxyRoot.GetComponentsInChildren<Renderer>(), Has.Length.GreaterThan(10));
            Assert.That(monitor.ProxyRoot.GetComponentsInChildren<Renderer>().All(item => item.gameObject.layer == SelfReviewMonitor.ProxyLayer), Is.True);
            Assert.That(monitor.ProxyRoot.GetComponentsInChildren<Collider>(), Is.Empty);
            Assert.That(Vector3.Distance(monitor.ProxyRoot.position, host.Player.transform.position), Is.LessThan(.001f));
            foreach (string part in new[] { "Head", "Torso" })
            {
                var point = monitor.ViewCamera.WorldToViewportPoint(monitor.ProxyRoot.Find(part).position);
                Assert.That(point.x, Is.InRange(0f, 1f), part + " must be in the monitor frame.");
                Assert.That(point.y, Is.InRange(0f, 1f));
                Assert.That(point.z, Is.GreaterThan(monitor.ViewCamera.nearClipPlane));
            }
            int frames = monitor.RenderedFrames;
            yield return null;
            Assert.That(monitor.RenderedFrames, Is.GreaterThan(frames), "The feed must update instead of showing a static portrait.");
            AssertVisibleFeed();
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Space), Is.False, "Only the explicit purification action may commit self review.");
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.SelfReview));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator EnterPurifiesOnceAndHidesNormalEndingUiBeforeTheSceneHandoff()
        {
            yield return EnterTerminal();
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
            int frames = monitor.RenderedFrames;
            yield return null;
            Assert.That(monitor.RenderedFrames, Is.EqualTo(frames));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator PointerPurificationRespectsTransitionAndPauseAndCommitsOnce()
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False, "The camera approach must finish first.");
            yield return Until(() => runtime.GetView().Cue == null && !host.Effects.IsTransitioning && host.Screen.TerminalReveal >= .999f,
                "self-review camera entry");
            var button = FindPurify();
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            Click(button);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.SelfReview));
            Assert.That(host.Screen.HandleTerminalKey(KeyCode.Return), Is.False);
            Assert.That(runtime.SetPaused(false).Accepted, Is.True);
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
        }

        private IEnumerator EnterTerminal()
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Effects.IsTransitioning && host.Screen.TerminalReveal >= .999f && monitor.RenderedFrames > 0,
                "self-review terminal and live monitor");
        }

        private MiniGameAudioButton FindPurify() => host.Screen.GetComponentsInChildren<MiniGameAudioButton>(true)
            .Single(button => button.name == "SelfPurify");

        private static void Click(MiniGameAudioButton button) => button.OnPointerClick(new PointerEventData(EventSystem.current)
        { button = PointerEventData.InputButton.Left, clickCount = 1 });

        private void AssertVisibleFeed()
        {
            var previous = RenderTexture.active;
            var pixels = new Texture2D(monitor.Output.width, monitor.Output.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = monitor.Output;
                pixels.ReadPixels(new Rect(0, 0, pixels.width, pixels.height), 0, 0);
                pixels.Apply();
                var colors = pixels.GetPixels32();
                Assert.That(colors.Select(color => color.r * 65536 + color.g * 256 + color.b).Distinct().Count(), Is.GreaterThan(32),
                    "A uniformly dark/blank monitor is not a live room feed.");
            }
            finally { RenderTexture.active = previous; Object.Destroy(pixels); }
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
            var patient = new PatientDefinition("unused", "未安排会谈", 0, 0, null, PatientPresence.TreatmentEnded);
            var rules = new GameRules("self-monitor-test", new[] { patient }, Enumerable.Range(0, 4)
                .Select(day => new DayDefinition(day, 0, Array.Empty<MeetingDefinition>())),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(100, 100, 100, new System.Collections.Generic.Dictionary<string, int>());
            var game = new GameRuntime(rules, new EmptyContent(), true);
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
