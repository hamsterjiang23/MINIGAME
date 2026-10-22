using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
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
    public sealed class PrototypePlayModeTests
    {
        [UnityTest]
        public IEnumerator SceneBootsWithOneRuntimeAndOriginalInteractionTargets()
        {
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartLegacyPrototype();
            Assert.That(host, Is.Not.Null);
            Assert.That(Object.FindObjectsOfType<MiniGameBootstrap>(), Has.Length.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EquivalentTo(new[] { InteractionTarget.Door }));
            Assert.That(Object.FindObjectsOfType<WorldInteraction>().Select(item => item.Target),
                Is.EquivalentTo(new[] { InteractionTarget.Door, InteractionTarget.Computer, InteractionTarget.Bed }));
            Assert.That(host.Screen.Canvas.transform.Find("ShiftHeader"), Is.Null);
            Assert.That(host.Screen.Canvas.transform.Find("InteractionCursor"), Is.Not.Null);
            var text = host.Screen.GetComponentsInChildren<Text>(true).First(item => item.name == "DialogueBody");
            Assert.That(text.font, Is.Not.Null);
            Assert.That(text.font.HasCharacter('检'), Is.True, "A Chinese OS font must be installed for the target player.");
            yield return CaptureIfRequested(host, "world");
            host.Runtime.SetPaused(true);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.False);
            host.Runtime.SetPaused(false);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.Presentation));
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator ActualYarnButtonsCompleteThreeDaysAndReturnToApartmentBetweenPatients()
        {
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartLegacyPrototype();
            var runtime = host.Runtime;
            int frames = 0;
            float deadline = Time.realtimeSinceStartup + 90f;
            int roomReturns = 0;
            bool sawEyes = false;
            bool capturedTerminal = false;
            string inspectedEncounter = null;
            string previousState = null;
            while (runtime.GetView().Game.Phase != GamePhase.Ending && Time.realtimeSinceStartup < deadline)
            {
                frames++;
                var view = runtime.GetView();
                string state = view.Game.Day + ":" + view.Game.Phase + ":" + view.Game.CurrentNodeId + ":" + view.Cue?.Kind;
                if (state != previousState) { previousState = state; DumpState(host, "flow transition"); }
                if (view.Input == InputContext.World)
                {
                    if (view.Game.Phase == GamePhase.Work) roomReturns++;
                    Assert.That(runtime.TryInteract(view.AllowedInteractions.First()).Accepted, Is.True);
                }
                else if (view.Input == InputContext.Dialogue && view.Game.CurrentEncounterId != inspectedEncounter)
                {
                    inspectedEncounter = view.Game.CurrentEncounterId;
                    if (!capturedTerminal && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR")))
                    {
                        yield return WaitForButton(host, "Continue");
                        yield return CaptureIfRequested(host, "terminal");
                        capturedTerminal = true;
                    }
                    foreach (var kind in new[] { InspectionKind.Eyes, InspectionKind.Hands, InspectionKind.Teeth, InspectionKind.Photo, InspectionKind.DailySigns, InspectionKind.Records })
                    {
                        var check = host.Screen.GetComponentsInChildren<Button>(false).Single(button => button.name == "Inspect-" + kind);
                        Assert.That(check.interactable, Is.True);
                        check.onClick.Invoke();
                        Assert.That(runtime.GetView().Input, Is.EqualTo(InputContext.Inspection));
                        Assert.That(runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "empathize").Accepted, Is.False);
                        Assert.That(host.Screen.GetComponentsInChildren<Text>(true).Single(item => item.name == "InspectionDescription").text, Is.Not.Empty);
                        if (kind == InspectionKind.Eyes && !sawEyes) yield return CaptureIfRequested(host, "eyes");
                        host.Screen.GetComponentsInChildren<Button>(false).Single(button => button.name == "CloseInspection").onClick.Invoke();
                        Assert.That(runtime.GetView().Inspection, Is.Null);
                    }
                    sawEyes = true;
                }
                else
                {
                    var buttons = host.Screen.GetComponentsInChildren<Button>(false);
                    if (view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        string name = "Disposition-" + (view.Game.RemainingPurificationQuota > 0 ? Disposition.Purify : Disposition.Treat);
                        var decision = buttons.Single(button => button.name == name && button.interactable);
                        yield return ClickThroughPointer(decision);
                    }
                    else
                    {
                        var reply = buttons.FirstOrDefault(button => button.name.StartsWith("Reply-") && button.interactable);
                        var next = buttons.FirstOrDefault(button => button.name == "Continue" && button.interactable);
                        if (reply != null) yield return ClickThroughPointer(reply);
                        else if (next != null)
                        {
                            if (view.Cue?.Kind == PresentationKind.Reaction)
                            {
                                Assert.That(next.GetComponentInChildren<Text>().text, Does.Contain("返回房间"));
                                Assert.That(buttons.Any(button => button.name.StartsWith("Disposition-")), Is.False);
                            }
                            yield return ClickThroughPointer(next);
                        }
                    }
                }
                yield return null;
            }
            DumpState(host, "flow final frames=" + frames);
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "The runtime or Yarn UI stopped offering a usable action.");
            Assert.That(sawEyes, Is.True);
            Assert.That(roomReturns, Is.GreaterThan(0), "Every reaction must return through the room before the next terminal session.");
            Assert.That(runtime.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            var rules = PrototypeRules.Create();
            var restored = GameRuntime.Restore(runtime.Save(), rules, new PrototypeContent(rules));
            Assert.That(restored.GetView().Game.Ending, Is.EqualTo(EndingType.TakenAway));
            Debug.Log("MINIGAME_PLAYMODE_FLOW_PASSED three days; real Yarn line/options; six-check UI; room returns; ending restore");
        }

        private static void AssertPointerCanReach(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rect = button.GetComponent<RectTransform>();
            var pointer = new PointerEventData(EventSystem.current)
            { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Debug.Log("MINIGAME_RAYCAST button=" + button.name + " pointer=" + pointer.position + " rect=" + rect.rect
                + " hits=" + string.Join(";", hits.Select(hit => hit.gameObject.name + " depth=" + hit.depth))
                + " rendererDepth=" + button.GetComponent<CanvasRenderer>().absoluteDepth + " culled=" + button.GetComponent<CanvasRenderer>().cull);
            Assert.That(hits, Is.Not.Empty, "The visible button must receive a uGUI raycast.");
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button),
                "A later modal or conversation panel must not intercept the button click.");
        }

        private static IEnumerator ClickThroughPointer(Button button)
        {
            // Let the newly enabled graphic reach the Canvas render pass before a user click.
            yield return null;
            Assert.That(button != null && button.IsActive() && button.IsInteractable(), Is.True);
            AssertPointerCanReach(button);
            var rect = button.GetComponent<RectTransform>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                button = PointerEventData.InputButton.Left,
                clickCount = 1
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var target = hits[0].gameObject;
            var pressed = ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerDownHandler);
            var clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            yield return null;
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);
            hits.Clear();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty);
            Assert.That(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject), Is.EqualTo(clickTarget));
            int clicks = 0;
            UnityEngine.Events.UnityAction countClick = () => clicks++;
            button.onClick.AddListener(countClick);
            try { ExecuteEvents.Execute(clickTarget, pointer, ExecuteEvents.pointerClickHandler); }
            finally { button.onClick.RemoveListener(countClick); }
            Assert.That(clicks, Is.EqualTo(1), "One pointer press/release must invoke the button exactly once.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator RestoreInvalidatesOldContinueAndReplyCallbacksWhileFreshYarnRemainsUsable()
        {
            foreach (bool replies in new[] { false, true })
            {
                yield return SceneManager.LoadSceneAsync("InspectionApartment");
                yield return null;
                var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartLegacyPrototype();
                Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                yield return WaitForButton(host, "Continue");
                if (replies)
                {
                    FindActiveButton(host, "Continue").onClick.Invoke();
                    yield return WaitForPhase(host, GamePhase.Briefing);
                    yield return WaitForButton(host, "Continue");
                    FindActiveButton(host, "Continue").onClick.Invoke();
                    yield return WaitForPhase(host, GamePhase.Apartment);
                    Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
                    yield return WaitForButton(host, "Continue");
                    FindActiveButton(host, "Continue").onClick.Invoke();
                    yield return WaitForButton(host, "Reply-", true);
                }
                var staleButton = FindActiveButton(host, replies ? "Reply-" : "Continue", replies);
                var staleCallback = staleButton.onClick;
                var checkpoint = host.Runtime.Save();
                host.RestoreCheckpoint(checkpoint);
                var fresh = host.Runtime;
                string before = fresh.Save();
                staleCallback.Invoke();
                for (int index = 0; index < 8; index++) yield return null;
                Assert.That(fresh.Save(), Is.EqualTo(before), "An old UI callback must not advance a restored runtime.");
                if (replies)
                {
                    // Completed actor lines resume at the outstanding choices instead of replaying the introduction.
                    yield return WaitForButton(host, "Reply-", true);
                    FindActiveButton(host, "Reply-", true).onClick.Invoke();
                    for (int index = 0; index < 4; index++) yield return null;
                    Assert.That(fresh.GetView().Game.EncounterStage, Is.EqualTo(EncounterStage.ResponseBranch));
                    Assert.That(fresh.GetView().Cue.Kind, Is.EqualTo(PresentationKind.ResponseBranch));
                }
                else
                {
                    yield return WaitForButton(host, "Continue");
                    FindActiveButton(host, "Continue").onClick.Invoke();
                    yield return WaitForPhase(host, GamePhase.Briefing);
                }
            }
        }

        private static Button FindActiveButton(MiniGameBootstrap host, string name, bool prefix = false) =>
            host.Screen.GetComponentsInChildren<Button>(false).FirstOrDefault(button => button.interactable &&
                (prefix ? button.name.StartsWith(name) : button.name == name));

        private static IEnumerator WaitForButton(MiniGameBootstrap host, string name, bool prefix = false)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && FindActiveButton(host, name, prefix) == null) yield return null;
            DumpState(host, "wait button " + name);
            Assert.That(FindActiveButton(host, name, prefix), Is.Not.Null, "A fresh Yarn presentation failed to offer " + name);
        }

        private static IEnumerator WaitForPhase(MiniGameBootstrap host, GamePhase phase)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && host.Runtime.GetView().Game.Phase != phase) yield return null;
            DumpState(host, "wait phase " + phase);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(phase));
        }

        private static void DumpState(MiniGameBootstrap host, string context)
        {
            var view = host.Runtime.GetView();
            string buttons = string.Join(";", host.Screen.GetComponentsInChildren<Button>(true)
                .Where(button => button.name == "Continue" || button.name.StartsWith("Reply-"))
                .Select(button => button.name + " active=" + button.gameObject.activeInHierarchy + " enabled=" + button.interactable));
            var bridge = host.GetComponent<MiniGameYarnBridge>();
            Debug.Log("MINIGAME_UI_STATE " + context + " time=" + Time.realtimeSinceStartup + " day=" + view.Game.Day
                + " phase=" + view.Game.Phase + " input=" + view.Input + " cue=" + view.Cue?.Kind + " terminal=" + view.TerminalOpen
                + " encounter=" + view.Game.CurrentEncounterId + " node=" + view.Game.CurrentNodeId + " yarn=" + bridge.Diagnostics + " buttons=" + buttons);
        }

        internal static IEnumerator CaptureIfRequested(MiniGameBootstrap host, string name)
        {
            var directory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            if (string.IsNullOrEmpty(directory)) yield break;
            Directory.CreateDirectory(directory);
            var camera = host.Player.ViewCamera;
            var canvas = host.Screen.Canvas;
            var mode = canvas.renderMode;
            var uiCamera = canvas.worldCamera;
            var distance = canvas.planeDistance;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                LogTerminalBounds(host, "overlay");
                target.Create(); camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .1f;
                yield return null; yield return null; yield return null;
                Canvas.ForceUpdateCanvases();
                host.Screen.ReflowTerminal();
                Canvas.ForceUpdateCanvases();
                LogTerminalBounds(host, "capture");
                camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); pixels.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), pixels.EncodeToPNG());
                Debug.Log("MINIGAME_CAPTURE " + Path.Combine(directory, name + ".png"));
            }
            finally
            {
                canvas.renderMode = mode; canvas.worldCamera = uiCamera; canvas.planeDistance = distance;
                camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
                target.Release(); Object.Destroy(target); Object.Destroy(pixels);
            }
            yield return null;
        }

        private static void LogTerminalBounds(MiniGameBootstrap host, string context)
        {
            var logical = host.Screen.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(item => item.name == "TerminalLogical640x360");
            if (logical == null) return;
            var corners = new Vector3[4]; logical.GetWorldCorners(corners);
            var canvas = host.Screen.Canvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Debug.Log("MINIGAME_TERMINAL_BOUNDS " + context + " screen=" + Screen.width + "x" + Screen.height
                + " canvas=" + ((RectTransform)canvas.transform).rect + " viewport=" + ((RectTransform)logical.parent).rect
                + " logical=" + logical.rect + " scale=" + logical.localScale + " lossy=" + logical.lossyScale
                + " bottomLeft=" + RectTransformUtility.WorldToScreenPoint(camera, corners[0])
                + " topRight=" + RectTransformUtility.WorldToScreenPoint(camera, corners[2]));
        }

        [UnityTest]
        public IEnumerator RestoredEffectsClearOverlaysSnapTerminalAndPauseAtFinalSleepFrame()
        {
            yield return SceneManager.LoadSceneAsync("InspectionApartment");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartLegacyPrototype();
            var initial = host.Runtime.Save();
            host.Screen.SetFade(.8f); host.Screen.SetGlitch(.7f);
            host.RestoreCheckpoint(initial);
            var fade = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PresentationFade");
            var glitch = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "GeneratedSignalLoss");
            Assert.That(fade.color.a, Is.Zero);
            Assert.That(glitch.gameObject.activeSelf, Is.False);

            var runtime = host.Runtime;
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            string terminal = runtime.Save();
            host.Player.ViewCamera.transform.position += new Vector3(.37f, 0f, 0f);
            host.RestoreCheckpoint(terminal);
            Assert.That(Vector3.Distance(host.Player.ViewCamera.transform.position,
                GameObject.Find("Terminal camera destination").transform.position), Is.LessThan(.0001f));

            runtime = host.Runtime;
            int operations = 0;
            while ((runtime.GetView().Game.Phase != GamePhase.Evening || runtime.GetView().Cue != null) && operations++ < 300)
            {
                var view = runtime.GetView();
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.Input == InputContext.World) result = runtime.TryInteract(view.AllowedInteractions.First());
                else if (view.Dialogue != null) result = runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, view.Dialogue.Responses[0].Id);
                else result = runtime.SubmitDisposition(view.Game.CurrentEncounterId, view.Game.RemainingPurificationQuota > 0 ? Disposition.Purify : Disposition.Treat);
                Assert.That(result.Accepted, Is.True, result.Reason);
            }
            Assert.That(operations, Is.LessThan(300));
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            string sleepToken = runtime.GetView().Cue.Token;
            host.Effects.StopAllCoroutines();
            yield return null;
            var method = typeof(PresentationEffects).GetMethod("SleepTransition", BindingFlags.NonPublic | BindingFlags.Instance);
            var transition = (IEnumerator)method.Invoke(host.Effects, new object[] { sleepToken });
            int steps = 0;
            // Alpha reaches one only at the final yielded frame; resume after pausing at that exact boundary.
            while (fade.color.a < 1f && steps++ < 100000) Assert.That(transition.MoveNext(), Is.True);
            Assert.That(fade.color.a, Is.EqualTo(1f));
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            yield return null;
            var pauseModal = host.Screen.GetComponentsInChildren<Image>(true).Single(image => image.name == "PauseModal");
            Assert.That(pauseModal.transform.GetSiblingIndex(), Is.GreaterThan(fade.transform.GetSiblingIndex()),
                "Pause controls must remain visible above a fully opaque sleep fade.");
            Assert.That(pauseModal.transform.GetSiblingIndex(), Is.GreaterThan(glitch.transform.GetSiblingIndex()));
            AssertPointerCanReach(host.Screen.GetComponentsInChildren<Button>(false).Single(button => button.name == "PauseAction0"));
            yield return CaptureIfRequested(host, "pause-sleep");
            Assert.That(transition.MoveNext(), Is.True, "A paused final frame must keep waiting rather than lose its completion.");
            Assert.That(runtime.GetView().Cue.Token, Is.EqualTo(sleepToken));
            runtime.SetPaused(false);
            Assert.That(transition.MoveNext(), Is.False);
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Dream));
            Assert.That(runtime.CompleteCue(sleepToken).Accepted, Is.False);
            host.RestoreCheckpoint(initial);
            Assert.That(fade.color.a, Is.Zero);
            Assert.That(glitch.gameObject.activeSelf, Is.False);
        }
    }
}
