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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SeatedTerminalEntryPlayModeTests
    {
        [UnityTest, Timeout(60000)]
        public IEnumerator ActorSitsBeforeAlignedDollyAndPhysicalCoveragePrecedesInterface()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo();
            CompleteMorning(host);
            yield return StageAtComputer(host);
            var camera = host.Player.ViewCamera;
            var controller = host.Player.GetComponent<CharacterController>();
            var rootPosition = host.Player.transform.position;
            var rootRotation = host.Player.transform.rotation;
            var eyePosition = camera.transform.position;
            float capsuleHeight = controller.height;
            float roomFov = camera.fieldOfView;
            var observations = new List<EntryObservation>();
            var surface = Object.FindObjectsOfType<MeshRenderer>().Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            Assert.That(surface, Is.Not.Null);
            string world = host.Runtime.Save();
            Assert.That(host.Player.InteractFocused(), Is.True);
            var observer = host.StartCoroutine(ObserveEntry(host, surface, observations));
            var captureDirectory = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            var timeline = new MotionTimeline();
            float nextCapture = 0;
            try
            {
                float deadline = Time.realtimeSinceStartup + 20;
                while (host.Effects.EntryPhase != TerminalEntryPhase.Active && Time.realtimeSinceStartup < deadline)
                {
                    if (!string.IsNullOrEmpty(captureDirectory) && Time.realtimeSinceStartup >= nextCapture)
                    {
                        nextCapture = Time.realtimeSinceStartup + .1f;
                        string name = "terminal-motion-" + timeline.frames.Count.ToString("000");
                        var frame = new MotionFrame
                        {
                            file = name + ".png", requestedPhase = host.Effects.EntryPhase.ToString(),
                            requestedProgress = host.Effects.TerminalTransitionProgress
                        };
                        yield return PrototypePlayModeTests.CaptureIfRequested(host, name);
                        frame.completedPhase = host.Effects.EntryPhase.ToString();
                        frame.completedProgress = host.Effects.TerminalTransitionProgress;
                        timeline.frames.Add(frame);
                    }
                    else yield return null;
                }
            }
            finally { host.StopCoroutine(observer); }
            if (!string.IsNullOrEmpty(captureDirectory))
            {
                string name = "terminal-motion-" + timeline.frames.Count.ToString("000");
                yield return PrototypePlayModeTests.CaptureIfRequested(host, name);
                timeline.frames.Add(new MotionFrame { file = name + ".png", requestedPhase = "Active", completedPhase = "Active",
                    requestedProgress = 1, completedProgress = 1 });
                File.WriteAllText(Path.Combine(captureDirectory, "terminal-motion-timeline.json"), JsonUtility.ToJson(timeline, true));
            }
            foreach (var observation in observations) AssertEntryObservation(observation, camera.nearClipPlane);
            var phases = observations.Select(item => item.phase).Distinct().ToArray();
            Assert.That(phases, Does.Contain(TerminalEntryPhase.Sitting));
            Assert.That(phases, Does.Contain(TerminalEntryPhase.Aligning));
            Assert.That(phases, Does.Contain(TerminalEntryPhase.Pushing));
            Assert.That(phases, Does.Contain(TerminalEntryPhase.FadingWorld));
            Assert.That(phases, Does.Contain(TerminalEntryPhase.RevealingInterface));
            Assert.That(host.Effects.EntryPhase, Is.EqualTo(TerminalEntryPhase.Active));
            Assert.That(host.Effects.IsSeated, Is.True);
            Assert.That(controller.height, Is.EqualTo(1.2f).Within(.001f));
            Assert.That(Vector3.Distance(host.Player.transform.position, rootPosition), Is.GreaterThan(.1f));
            Assert.That(host.Screen.TerminalReveal, Is.EqualTo(1));
            AssertScreenCoversViewport(camera, surface);
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            string exiting = host.Runtime.Save();
            string staleExitToken = host.Runtime.GetView().Cue.Token;
            host.RestoreCheckpoint(world);
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True);
            host.RestoreCheckpoint(exiting);
            Assert.That(host.Runtime.CompleteCue(staleExitToken).Accepted, Is.False);
            Assert.That(host.Effects.IsSeated, Is.True);
            Assert.That(host.Screen.TransitionLocked, Is.True);
            AssertScreenCoversViewport(camera, surface);
            yield return Until(() => !host.Effects.IsTransitioning && host.Runtime.GetView().Input == InputContext.World);
            Assert.That(host.Effects.IsSeated, Is.False);
            Assert.That(controller.enabled, Is.True);
            Assert.That(controller.height, Is.EqualTo(capsuleHeight).Within(.001f));
            Assert.That(Vector3.Distance(host.Player.transform.position, rootPosition), Is.LessThan(.04f));
            Assert.That(Quaternion.Angle(host.Player.transform.rotation, rootRotation), Is.LessThan(.2f));
            Assert.That(Vector3.Distance(camera.transform.position, eyePosition), Is.LessThan(.04f));
            Assert.That(camera.fieldOfView, Is.EqualTo(roomFov).Within(.001f));
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator SeatedEntryPauseRestoreAndWideViewportKeepOriginalStandingPose()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo();
            CompleteMorning(host);
            yield return StageAtComputer(host);
            var camera = host.Player.ViewCamera;
            camera.aspect = 16f / 9f;
            var rootPosition = host.Player.transform.position;
            var roomPosition = camera.transform.position;
            string world = host.Runtime.Save();
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(() => host.Effects.EntryPhase == TerminalEntryPhase.Pushing);
            camera.aspect = 2.4f;
            yield return null;
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var position = camera.transform.position;
            var seatedPosition = host.Player.transform.position;
            float progress = host.Effects.TerminalTransitionProgress;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(camera.transform.position, Is.EqualTo(position));
            Assert.That(host.Player.transform.position, Is.EqualTo(seatedPosition));
            Assert.That(host.Effects.TerminalTransitionProgress, Is.EqualTo(progress));
            string entering = host.Runtime.Save();
            string staleToken = host.Runtime.GetView().Cue.Token;
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return Until(() => host.Effects.EntryPhase == TerminalEntryPhase.FadingWorld);
            var surface = Object.FindObjectsOfType<MeshRenderer>().Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            AssertScreenCoversViewport(camera, surface); // Same entry, before restore can recompute the destination.
            host.RestoreCheckpoint(entering);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(staleToken).Accepted, Is.False);
            yield return Until(() => host.Effects.EntryPhase == TerminalEntryPhase.Active && !host.Effects.IsTransitioning);
            AssertScreenCoversViewport(camera, surface);
            string directory = host.Runtime.Save();
            host.RestoreCheckpoint(world);
            Assert.That(Vector3.Distance(camera.transform.position, roomPosition), Is.LessThan(.001f));
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True);
            host.RestoreCheckpoint(directory);
            Assert.That(host.Effects.IsSeated, Is.True);
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.False);
            AssertScreenCoversViewport(camera, surface);
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            yield return Until(() => !host.Effects.IsTransitioning && host.Runtime.GetView().Input == InputContext.World);
            Assert.That(Vector3.Distance(host.Player.transform.position, rootPosition), Is.LessThan(.04f));
            Assert.That(Vector3.Distance(camera.transform.position, roomPosition), Is.LessThan(.04f));
            camera.ResetAspect();
        }

        private static void CompleteMorning(MiniGameBootstrap host)
        {
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
        }

        private static IEnumerator StageAtComputer(MiniGameBootstrap host)
        {
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Computer approach").transform.position;
            controller.enabled = true;
            yield return null;
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Computer);
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.EqualTo(interaction));
        }

        private static void AssertScreenCoversViewport(Camera camera, Renderer surface)
        {
            var bounds = ProjectScreen(camera, surface, out var nearestDepth);
            Assert.That(nearestDepth, Is.GreaterThan(camera.nearClipPlane));
            Assert.That(bounds.x, Is.LessThanOrEqualTo(0)); Assert.That(bounds.z, Is.GreaterThanOrEqualTo(1));
            Assert.That(bounds.y, Is.LessThanOrEqualTo(0)); Assert.That(bounds.w, Is.GreaterThanOrEqualTo(1));
        }

        private static Vector4 ProjectScreen(Camera camera, Renderer surface, out float nearestDepth)
        {
            var mesh = surface.GetComponent<MeshFilter>().sharedMesh.bounds;
            float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            nearestDepth = float.PositiveInfinity;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        var point = surface.transform.TransformPoint(mesh.center + Vector3.Scale(mesh.extents, new Vector3(x, y, z)));
                        var viewport = camera.WorldToViewportPoint(point);
                        nearestDepth = Mathf.Min(nearestDepth, viewport.z);
                        minX = Mathf.Min(minX, viewport.x); maxX = Mathf.Max(maxX, viewport.x);
                        minY = Mathf.Min(minY, viewport.y); maxY = Mathf.Max(maxY, viewport.y);
                    }
            return new Vector4(minX, minY, maxX, maxY);
        }

        // A separate coroutine still samples every frame while the capture helper yields for canvas updates.
        private static IEnumerator ObserveEntry(MiniGameBootstrap host, Renderer surface, List<EntryObservation> observations)
        {
            var camera = host.Player.ViewCamera;
            var controller = host.Player.GetComponent<CharacterController>();
            var alignment = GameObject.Find("Terminal screen alignment").transform;
            var handoff = host.Screen.GetComponentsInChildren<Transform>(true).Single(item => item.name == "TerminalHandoff").GetComponentInParent<Canvas>();
            while (host.Effects.EntryPhase != TerminalEntryPhase.Active)
            {
                var phase = host.Effects.EntryPhase;
                if (phase != TerminalEntryPhase.None)
                {
                    var bounds = ProjectScreen(camera, surface, out float depth);
                    observations.Add(new EntryObservation
                    {
                        phase = phase, controllerEnabled = controller.enabled, locked = host.Screen.TransitionLocked,
                        reveal = host.Screen.TerminalReveal, handoff = handoff.renderMode == RenderMode.ScreenSpaceOverlay && handoff.gameObject.activeInHierarchy ? 1 : 0, seated = host.Effects.IsSeated,
                        alignment = Quaternion.Angle(camera.transform.rotation, alignment.rotation), fieldOfView = camera.fieldOfView,
                        screenBounds = bounds, nearestDepth = depth
                    });
                }
                yield return null;
            }
        }

        private static void AssertEntryObservation(EntryObservation observation, float nearClipPlane)
        {
            Assert.That(observation.controllerEnabled, Is.False, "The actor capsule must not fight the seated motion.");
            Assert.That(observation.locked, Is.True);
            Assert.That(observation.handoff, Is.Zero, "The original boot card must not hide the physical screen approach.");
            if (observation.phase <= TerminalEntryPhase.FadingWorld)
                Assert.That(observation.reveal, Is.Zero, "UI cannot cover the room before the physical display fills the view.");
            if (observation.phase == TerminalEntryPhase.Pushing || observation.phase == TerminalEntryPhase.FadingWorld)
            {
                Assert.That(observation.seated, Is.True);
                Assert.That(observation.alignment, Is.LessThan(.01f));
                Assert.That(observation.fieldOfView, Is.EqualTo(48).Within(.001f));
            }
            if (observation.phase != TerminalEntryPhase.FadingWorld) return;
            Assert.That(observation.nearestDepth, Is.GreaterThan(nearClipPlane));
            Assert.That(observation.screenBounds.x, Is.LessThanOrEqualTo(0));
            Assert.That(observation.screenBounds.y, Is.LessThanOrEqualTo(0));
            Assert.That(observation.screenBounds.z, Is.GreaterThanOrEqualTo(1));
            Assert.That(observation.screenBounds.w, Is.GreaterThanOrEqualTo(1));
        }

        private sealed class EntryObservation
        {
            public TerminalEntryPhase phase;
            public bool controllerEnabled, locked, seated;
            public float reveal, handoff, alignment, fieldOfView, nearestDepth;
            public Vector4 screenBounds;
        }

        [Serializable] private sealed class MotionTimeline
        {
            public List<MotionFrame> frames = new List<MotionFrame>();
        }

        [Serializable] private sealed class MotionFrame
        {
            public string file, requestedPhase, completedPhase;
            public float requestedProgress, completedProgress;
        }

        private static IEnumerator Until(Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, "Terminal transition did not reach its expected phase.");
        }
    }
}
