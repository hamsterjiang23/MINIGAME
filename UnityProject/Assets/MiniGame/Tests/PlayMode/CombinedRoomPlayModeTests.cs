using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class CombinedRoomPlayModeTests
    {
        [UnityTest, Timeout(60000)]
        public IEnumerator PhysicalComputerEntryPausesRestoresAndReturnsToTheSameRoomPose()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo(); // Keep this regression fixture on its original one-patient content.
            host.Player.enabled = false; // Drive pose and clicks explicitly, without desktop mouse drift after returning control.
            Assert.That(host, Is.Not.Null);
            Assert.That(Object.FindObjectsOfType<MiniGameBootstrap>(), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsOfType<Camera>(), Has.Length.EqualTo(1));
            Assert.That(host.TerminalDemoActive, Is.True);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(Object.FindObjectsOfType<WorldInteraction>().Select(item => item.Target),
                Is.EquivalentTo(new[] { InteractionTarget.Door, InteractionTarget.Computer, InteractionTarget.Bed }));
            var exterior = Object.FindObjectsOfType<MeshRenderer>()
                .Where(renderer => renderer.name.StartsWith("B_ExteriorConcrete_", StringComparison.Ordinal)).ToArray();
            Assert.That(exterior, Has.Length.EqualTo(2), "The window must retain its actual exterior wall and ledge.");
            Assert.That(exterior.All(renderer => renderer.enabled), Is.True);
            var restoredDoor = Object.FindObjectOfType<HingedDoor>();
            Assert.That(restoredDoor, Is.Not.Null);
            Assert.That(restoredDoor.GetComponentsInChildren<Renderer>().All(renderer => renderer.enabled), Is.True);

            yield return StageAt(host, InteractionTarget.Computer);
            Assert.That(host.Player.InteractFocused(), Is.False, "The real computer cannot bypass morning inspection.");
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            yield return FinishMorningThroughDoor(host);
            yield return StageAt(host, InteractionTarget.Computer);
            var camera = host.Player.ViewCamera;
            var roomPosition = camera.transform.position;
            var roomRotation = camera.transform.rotation;
            float roomFov = camera.fieldOfView;
            int quota = host.Runtime.GetView().Game.RemainingPurificationQuota;
            string roomCheckpoint = host.Runtime.Save();
            yield return Capture(host, "combined-approach");
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.EnterTerminal));
            Assert.That(host.Screen.TransitionLocked, Is.True);
            Assert.That(host.Screen.TerminalReveal, Is.LessThan(.01f), "The desktop must wait for the physical camera approach.");
            yield return Until(host, () => host.Effects.TerminalTransitionProgress >= .70f, "CRT boot readout becomes visible");
            yield return Capture(host, "combined-boot");
            Assert.That(host.Effects.IsTransitioning, Is.True);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            float pausedProgress = host.Effects.TerminalTransitionProgress;
            var pausedPosition = camera.transform.position;
            float pausedReveal = host.Screen.TerminalReveal;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(host.Effects.TerminalTransitionProgress, Is.EqualTo(pausedProgress).Within(.00001f));
            Assert.That(Vector3.Distance(camera.transform.position, pausedPosition), Is.LessThan(.00001f));
            Assert.That(host.Screen.TerminalReveal, Is.EqualTo(pausedReveal).Within(.00001f));
            yield return Capture(host, "combined-boot-paused");

            string entering = host.Runtime.Save();
            string cueToken = host.Runtime.GetView().Cue.Token;
            host.RestoreCheckpoint(entering);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            string restoredToken = host.Runtime.GetView().Cue.Token;
            Assert.That(restoredToken, Is.Not.EqualTo(cueToken), "Restoring must invalidate callbacks from the previous runtime.");
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(cueToken).Accepted, Is.False, "The pre-restore cue token must be rejected after unpausing.");
            Assert.That(host.Runtime.GetView().Cue.Token, Is.EqualTo(restoredToken));
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Screen.TransitionLocked && host.Screen.TerminalReveal >= .999f, "restored CRT entry completes");
            Assert.That(host.Runtime.CompleteCue(restoredToken).Accepted, Is.False, "The fresh entry token must be acknowledged exactly once.");
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(quota));
            Assert.That(host.Runtime.GetView().Transcript, Is.Empty);
            Assert.That(Vector3.Distance(camera.transform.position, GameObject.Find("Terminal camera destination").transform.position), Is.LessThan(.01f));
            yield return Capture(host, "combined-directory");
            yield return ClickNamed(host, "LeaveTerminal");
            string exitCheckpoint = host.Runtime.Save();
            host.RestoreCheckpoint(roomCheckpoint);
            host.RestoreCheckpoint(exitCheckpoint);
            Assert.That(Vector3.Distance(camera.transform.position, GameObject.Find("Terminal camera destination").transform.position), Is.LessThan(.01f),
                "Loading an exit from the room must replay from the terminal while preserving its room destination.");
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.World && !host.Effects.IsTransitioning,
                "directory exit restores world control");
            yield return null;
            Assert.That(Vector3.Distance(camera.transform.position, roomPosition), Is.LessThan(.04f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, roomRotation), Is.LessThan(.2f));
            Assert.That(camera.fieldOfView, Is.EqualTo(roomFov).Within(.01f));
            Assert.That(host.Screen.TerminalReveal, Is.LessThan(.01f));
            Assert.That(host.Screen.TransitionLocked, Is.False);
            // Batch-mode windows cannot capture an OS cursor; the world ray is checked below in both modes.
            if (host.Player.enabled && !Application.isBatchMode && Application.isFocused)
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction?.Target, Is.EqualTo(InteractionTarget.Computer));
            yield return Capture(host, "combined-return-room");
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator PhysicalRoomAndNativeYarnCompleteSixRoundsOneDispositionAndBed()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo();
            host.Player.enabled = false;
            yield return FinishMorningThroughDoor(host);
            yield return StageAt(host, InteractionTarget.Computer);
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Screen.TransitionLocked, "physical terminal entry");
            yield return ClickNamed(host, "EnterReview");

            int replies = 0;
            bool submitted = false;
            bool captured = false;
            float deadline = Time.realtimeSinceStartup + 65f;
            while (host.Runtime.GetView().TerminalPage != TerminalPage.Archive && Time.realtimeSinceStartup < deadline)
            {
                var view = host.Runtime.GetView();
                if (view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    Assert.That(replies, Is.EqualTo(6));
                    var decision = Find(host, "Disposition-Purify");
                    Assert.That(decision, Is.Not.Null);
                    var staleCallback = decision.onClick;
                    yield return Click(decision);
                    staleCallback.Invoke();
                    Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(4), "Repeated input cannot consume a second quota.");
                    Assert.That(host.Runtime.GetView().Transcript.Count(entry => entry.SourceNodeId == "disposition:Purify"), Is.EqualTo(1));
                    submitted = true;
                }
                else
                {
                    var next = Find(host, "Continue");
                    var reply = host.Screen.GetComponentsInChildren<Button>(false)
                        .FirstOrDefault(button => button.name.StartsWith("Reply-", StringComparison.Ordinal) && button.IsInteractable());
                    if (next != null) yield return Click(next);
                    else if (reply != null)
                    {
                        if (!captured) { yield return Capture(host, "combined-dialogue"); captured = true; }
                        yield return Click(reply);
                        replies++;
                    }
                }
                yield return null;
            }
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Archive), Describe(host));
            Assert.That(submitted, Is.True);
            Assert.That(replies, Is.EqualTo(6));
            Assert.That(host.Runtime.GetView().Transcript.Count(entry => entry.IsPlayer && !entry.SourceNodeId.StartsWith("disposition:", StringComparison.Ordinal)), Is.EqualTo(6));
            Assert.That(host.Runtime.GetView().Transcript.Count(entry => entry.SourceNodeId == "disposition:Purify"), Is.EqualTo(1));
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(4));
            yield return Capture(host, "combined-archive");
            yield return ClickNamed(host, "ReturnDirectory");
            Assert.That(host.Runtime.GetView().ArchiveDirectoryVisible, Is.True);
            Assert.That(Find(host, "EnterReview"), Is.Null, "Completed patients cannot be submitted a second time.");
            yield return ClickNamed(host, "LeaveTerminal");
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.World && !host.Effects.IsTransitioning,
                "archive exit restores the room");
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            yield return StageAt(host, InteractionTarget.Bed);
            yield return Capture(host, "combined-bed");
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Sleep));
            yield return Until(host, () => host.Runtime.GetView().Cue?.Kind == PresentationKind.Dream, "bed fade completes into dream");
            Assert.That(host.Runtime.GetView().Game.RemainingPurificationQuota, Is.EqualTo(4));
            Debug.Log("MINIGAME_COMBINED_ROOM_FLOW_PASSED physical door/computer/bed; native Yarn six rounds; one disposition; archive and room return");
        }

        private static IEnumerator FinishMorningThroughDoor(MiniGameBootstrap host)
        {
            yield return StageAt(host, InteractionTarget.Door);
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(host, () => host.Runtime.GetView().Cue?.Kind == PresentationKind.MorningInspection,
                "door opens fully before morning inspection");
            yield return ClickNamed(host, "Continue");
            yield return Until(host, () => host.Runtime.GetView().Game.Phase == GamePhase.Briefing, "morning inspection UI completion");
            yield return ClickNamed(host, "Continue");
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.DoorView
                || host.Runtime.GetView().Input == InputContext.World, "briefing UI completion");
            if (host.Runtime.GetView().Input == InputContext.DoorView)
                Assert.That(host.Player.InteractFocused(), Is.True, "E returns from the door observation before using the computer.");
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.World
                && host.Runtime.GetView().Game.Phase == GamePhase.Apartment, "briefing UI completion");
        }

        private static IEnumerator StageAt(MiniGameBootstrap host, InteractionTarget target)
        {
            var marker = GameObject.Find(target + " approach");
            Assert.That(marker, Is.Not.Null, "Scene setup must provide the validated " + target + " standing position.");
            var player = host.Player;
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = marker.transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            Assert.That(Physics.Raycast(player.transform.position + Vector3.up * .1f, Vector3.down, out var floor, .3f), Is.True,
                "A real floor collider must support the standing position.");
            Assert.That(floor.collider.name.IndexOf("floor", StringComparison.OrdinalIgnoreCase), Is.GreaterThanOrEqualTo(0));
            Assert.That(Physics.OverlapCapsule(player.transform.position + Vector3.up * .27f,
                player.transform.position + Vector3.up * 1.48f, .23f, ~0, QueryTriggerInteraction.Ignore)
                .Where(collider => collider != controller), Is.Empty, "The standing capsule must not start inside furniture or walls.");
            yield return null;
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == target);
            player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.EqualTo(interaction), "The camera ray must hit the visible object without an intervening collider.");
            Assert.That(player.transform.position.y, Is.GreaterThan(marker.transform.position.y - .1f));
        }

        private static IEnumerator Until(MiniGameBootstrap host, Func<bool> condition, string expectation)
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expectation + ": " + Describe(host));
        }

        private static Button Find(MiniGameBootstrap host, string name) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name == name && button.IsActive() && button.IsInteractable());

        private static IEnumerator ClickNamed(MiniGameBootstrap host, string name)
        {
            yield return Until(host, () => Find(host, name) != null, "button " + name);
            yield return Click(Find(host, name));
        }

        private static IEnumerator Click(Button button)
        {
            yield return null;
            Assert.That(button != null && button.IsActive() && button.IsInteractable(), Is.True);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center)),
                button = PointerEventData.InputButton.Left, clickCount = 1
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits, Is.Not.Empty, button.name + " must receive a pointer ray.");
            Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button),
                "A hidden terminal layer or transition overlay must not swallow " + button.name);
            var target = hits[0].gameObject;
            ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(target, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static IEnumerator Capture(MiniGameBootstrap host, string name) => PrototypePlayModeTests.CaptureIfRequested(host, name);
        private static string Describe(MiniGameBootstrap host)
        {
            var view = host.Runtime.GetView();
            return "phase=" + view.Game.Phase + " input=" + view.Input + " cue=" + view.Cue?.Kind
                + " page=" + view.TerminalPage + " reveal=" + host.Screen.TerminalReveal
                + " yarn=" + host.GetComponent<MiniGameYarnBridge>().Diagnostics;
        }
    }
}
