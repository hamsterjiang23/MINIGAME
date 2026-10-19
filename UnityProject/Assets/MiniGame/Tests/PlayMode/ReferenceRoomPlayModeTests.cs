using System;
using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class ReferenceRoomPlayModeTests
    {
        [UnityTest, Timeout(90000)]
        public IEnumerator PostersUseSecondaryInputDistanceOcclusionAndPausedRefreshableCaptions()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Host();
            var posters = Object.FindObjectsOfType<ReadableProp>();
            // This fixture drives pose/clicks explicitly; desktop mouse input must not move its ray during timed waits.
            host.Player.enabled = false;
            Assert.That(posters.Select(poster => poster.Id), Is.EquivalentTo(new[] { "poster-beach", "poster-instrument" }));
            var beach = posters.Single(poster => poster.Id == "poster-beach");
            var instrument = posters.Single(poster => poster.Id == "poster-instrument");
            Assert.That(beach.Caption, Is.EqualTo("这里是我的故乡…但我从没去过海边"));
            Assert.That(instrument.Caption, Is.EqualTo("新一代手持仪器引领我们走向未来！"));
            foreach (var poster in posters)
            {
                Assert.That(poster.Texture, Is.Not.Null, poster.Id + " needs its actual generated poster texture.");
                Assert.That(poster.InteractionDistance, Is.EqualTo(2).Within(.001f));
                Assert.That(poster.CaptionSeconds, Is.EqualTo(4).Within(.001f));
            }
            yield return StageAtPoster(host, beach);
            Assert.That(beach.IsHighlighted, Is.True);
            var material = beach.Surface.sharedMaterial;
            var materialColor = material.HasProperty("_Color") ? material.color : Color.white;
            string checkpoint = host.Runtime.Save();
            Assert.That(host.Player.InteractFocused(), Is.False, "E must not activate the left-click poster interaction.");
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
            Assert.That(host.Screen.RoomCaptionVisible, Is.False);

            PositionCameraFromSurface(host, beach, 2.05f);
            Assert.That(host.Player.FocusedReadable, Is.Null, "A poster beyond two metres must not stay focused.");
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            PositionCameraFromSurface(host, beach, 1.85f);
            Assert.That(host.Player.FocusedReadable, Is.EqualTo(beach));
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Reference room poster occlusion probe";
            blocker.transform.SetPositionAndRotation(Vector3.Lerp(host.Player.ViewCamera.transform.position,
                beach.Surface.bounds.center, .5f), host.Player.ViewCamera.transform.rotation);
            blocker.transform.localScale = new Vector3(.4f, .4f, .1f);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedReadable, Is.Null, "The focus ray must stop at a real intervening collider.");
            Assert.That(beach.IsHighlighted, Is.False);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            Object.Destroy(blocker);
            yield return null;
            yield return StageAtPoster(host, beach);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo(beach.Caption));
            Assert.That(host.Screen.RoomCaptionVisible, Is.True);
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Reading a poster must not change authoritative gameplay or save data.");
            if (material.HasProperty("_Color")) Assert.That(material.color, Is.EqualTo(materialColor), "The poster itself must not be tinted for focus.");
            yield return Capture(host, "reference-poster-beach");
            var captionCameraPosition = host.Player.ViewCamera.transform.position;
            var captionCameraRotation = host.Player.ViewCamera.transform.rotation;
            yield return new WaitForSecondsRealtime(1.9f);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            yield return new WaitForSecondsRealtime(2.3f);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            yield return null;
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo(beach.Caption));
            Assert.That(host.Screen.RoomCaptionVisible, Is.True, "The paused interval must not consume the caption's four seconds.");
            yield return new WaitForSecondsRealtime(1.6f);
            Assert.That(host.Screen.RoomCaptionVisible, Is.True);
            bool repeatedClick = host.Player.InteractSecondaryFocused();
            Physics.Raycast(host.Player.ViewCamera.transform.position, host.Player.ViewCamera.transform.forward, out var captionHit, 3.2f);
            Assert.That(repeatedClick, Is.True, "A repeated click should restart the subtitle lifetime. Camera drift: position="
                + Vector3.Distance(captionCameraPosition, host.Player.ViewCamera.transform.position)
                + " rotation=" + Quaternion.Angle(captionCameraRotation, host.Player.ViewCamera.transform.rotation)
                + " hit=" + (captionHit.collider != null ? captionHit.collider.name : "none") + " distance=" + captionHit.distance);
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(host.Screen.RoomCaptionVisible, Is.True);
            yield return new WaitForSecondsRealtime(3.5f);
            Assert.That(host.Screen.RoomCaptionVisible, Is.False, "A refreshed caption should eventually expire after four active seconds.");

            yield return StageAtPoster(host, instrument);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo(instrument.Caption));
            yield return Capture(host, "reference-poster-instrument");
            yield return StageAt(host, InteractionTarget.Computer);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            yield return null;
            Assert.That(host.Screen.RoomCaptionVisible, Is.False, "The room subtitle must clear when computer entry begins.");
            Assert.That(host.Screen.CurrentRoomCaption, Is.Empty);
            Assert.That(posters.All(poster => !poster.IsHighlighted), Is.True);
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Screen.TransitionLocked, "the physical computer entry still completes");
            Debug.Log("MINIGAME_REFERENCE_POSTERS_PASSED actual textures; two metres; collider occlusion; left-click only; pause and refresh; terminal clears subtitle");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator DoorMovesItsHandleAndColliderRestoresTargetsAndRunsMorningOnlyAfterOpening()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Host();
            var door = Object.FindObjectOfType<HingedDoor>();
            Assert.That(door, Is.Not.Null);
            Assert.That(door.EnableToggle, Is.True);
            Assert.That(door.OpenAngle, Is.EqualTo(60).Within(.001f));
            Assert.That(door.AnimationSeconds, Is.EqualTo(.6f).Within(.001f));
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            var leaf = door.GetComponentsInChildren<Collider>().FirstOrDefault(collider =>
                collider.GetComponentInParent<WorldInteraction>()?.Target == InteractionTarget.Door);
            var handle = door.GetComponentsInChildren<Renderer>().FirstOrDefault(renderer =>
                renderer.name.IndexOf("handle", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(leaf, Is.Not.Null, "A physical door collider must be parented below its hinge.");
            Assert.That(handle, Is.Not.Null, "The handle must rotate with its door, rather than remain on the frame.");
            var closedLeafPosition = leaf.transform.position;
            var closedLeafRotation = leaf.transform.rotation;
            var closedHandlePosition = handle.transform.position;
            string core = CoreJson(host.Runtime);
            yield return StageAt(host, InteractionTarget.Door);
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.DoorMotion));
            string motionToken = host.Runtime.GetView().Cue.Token;
            Assert.That(FindContinue(host), Is.Null, "A generic Continue button must not skip physical door movement.");
            Assert.That(host.Player.InteractFocused(), Is.False, "Repeated E input must not reverse an active opening.");
            yield return Until(host, () => door.CurrentAngle > 8 && door.CurrentAngle < 55, "the opening visibly interpolates");
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            float angle = door.CurrentAngle;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(door.CurrentAngle, Is.EqualTo(angle).Within(.0001f));
            host.RestoreCheckpoint(host.Runtime.Save());
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(door.CurrentAngle, Is.EqualTo(60).Within(.001f), "Loading motion snaps directly to the persisted target.");
            Assert.That(door.IsMoving, Is.False);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(motionToken).Accepted, Is.False);
            Assert.That(host.Runtime.GetView().Cue, Is.Null, "Day0 opening must not manufacture a formal morning inspection.");
            Assert.That(CoreJson(host.Runtime), Is.EqualTo(core));
            Assert.That(Vector3.Distance(leaf.transform.position, closedLeafPosition), Is.GreaterThan(.1f));
            Assert.That(Quaternion.Angle(leaf.transform.rotation, closedLeafRotation), Is.EqualTo(60).Within(.1f));
            Assert.That(Vector3.Distance(handle.transform.position, closedHandlePosition), Is.GreaterThan(.1f));
            var room = GameObject.Find("MainRoomCombined");
            var frameRight = (Marker(room, "ReferenceRight") - Marker(room, "ReferenceOrigin")).normalized;
            var frameDepth = (Marker(room, "ReferenceDepth") - Marker(room, "ReferenceOrigin")).normalized;
            MovePlayer(host, GameObject.Find("Door approach").transform.position - frameRight * 1.65f + frameDepth * .4f);
            yield return null;
            host.Player.FacePoint(Marker(room, "DoorCentre") + frameRight * 1.2f + frameDepth * .4f + Vector3.up * .25f);
            yield return Capture(host, "reference-door-60-corridor");

            var outside = Vector3.ProjectOnPlane(GameObject.Find("Unity_DoorCentre").transform.position
                - GameObject.Find("Door approach").transform.position, Vector3.up).normalized;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.Move(outside * 4);
            Physics.SyncTransforms();
            Assert.That(Vector3.Dot(host.Player.transform.position - GameObject.Find("Unity_DoorCentre").transform.position, outside),
                Is.LessThan(0), "The open doorway must retain its view-only threshold boundary.");
            yield return StageAt(host, InteractionTarget.Door);
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(host, () => !door.IsMoving && host.Runtime.GetView().Cue == null, "Day0 door closes");
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            door.OpenAngle = 90;
            Assert.That(host.Player.InteractFocused(), Is.True);
            yield return Until(host, () => !door.IsMoving && host.Runtime.GetView().Cue == null, "configured ninety degree opening");
            Assert.That(Quaternion.Angle(leaf.transform.rotation, closedLeafRotation), Is.EqualTo(90).Within(.1f));
            Assert.That(CoreJson(host.Runtime), Is.EqualTo(core));
            door.EnableToggle = false;
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            Assert.That(door.TryInteract().Accepted, Is.False, "A disabled switch must stay closed during the ordinary apartment stage.");

            string morning = DayOneMorning();
            host.RestoreCheckpoint(morning);
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            yield return StageAt(host, InteractionTarget.Door);
            Assert.That(host.Player.InteractFocused(), Is.True, "Disabled physical movement must retain the morning inspection entrance.");
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            Complete(host.Runtime, PresentationKind.MorningInspection);
            Complete(host.Runtime, PresentationKind.Briefing);
            door.EnableToggle = true;
            door.OpenAngle = 60;
            host.RestoreCheckpoint(morning);
            yield return StageAt(host, InteractionTarget.Door);
            Assert.That(host.Player.InteractFocused(), Is.True);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            Assert.That(host.Runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.DoorMotion));
            yield return Until(host, () => host.Runtime.GetView().Cue?.Kind == PresentationKind.MorningInspection,
                "formal morning waits for the fully open physical door");
            Assert.That(door.CurrentAngle, Is.EqualTo(60).Within(.001f));
            Assert.That(door.IsMoving, Is.False);
            Complete(host.Runtime, PresentationKind.MorningInspection);
            Complete(host.Runtime, PresentationKind.Briefing);
            core = CoreJson(host.Runtime);
            for (int toggle = 0; toggle < 2; toggle++)
            {
                yield return StageAt(host, InteractionTarget.Door);
                Assert.That(host.Player.InteractFocused(), Is.True);
                yield return Until(host, () => !door.IsMoving && host.Runtime.GetView().Cue == null,
                    "ordinary Day1 door motion completes without another inspection");
                Assert.That(CoreJson(host.Runtime), Is.EqualTo(core));
                Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            }
            Debug.Log("MINIGAME_REFERENCE_DOOR_PASSED default60; physical hinge/handle/collider; threshold; pause; save target and stale token; configurable90; disabled fallback; morning once");
        }

        [UnityTest, Timeout(90000)]
        public IEnumerator ReferenceEnvelopeAndCameraMatchTheSourceAndNightHidesTheCorridor()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Host();
            var room = GameObject.Find("MainRoomCombined");
            Assert.That(room, Is.Not.Null);
            Vector3 origin = Marker(room, "ReferenceOrigin");
            Vector3 right = (Marker(room, "ReferenceRight") - origin).normalized;
            Vector3 depth = (Marker(room, "ReferenceDepth") - origin).normalized;
            Vector3 up = (Marker(room, "ReferenceUp") - origin).normalized;
            Assert.That(Vector3.Dot(up, Vector3.up), Is.GreaterThan(.999f));
            Vector3 probe = origin + right * (9.70078f / 2) + depth * (4.41176f / 2) + up * 2.8f;
            Assert.That(Span(probe, right), Is.EqualTo(9.70078f).Within(.08f), "The actual inner walls must match the reference room length.");
            Assert.That(Span(probe, depth), Is.EqualTo(4.41176f).Within(.08f), "The actual inner walls must match the reference room depth.");
            Assert.That(VerticalSpan(probe), Is.EqualTo(3.12066f).Within(.08f), "The physical floor and ceiling must match the reference net height.");
            var camera = host.Player.ViewCamera;
            float horizontalFov = 2 * Mathf.Atan(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2) * camera.aspect) * Mathf.Rad2Deg;
            Assert.That(horizontalFov, Is.EqualTo(61.9275f).Within(.05f), "Horizontal framing must stay correct for the current Game view aspect.");
            var exterior = Object.FindObjectsOfType<MeshRenderer>().Where(renderer => renderer.name.StartsWith("B_ExteriorConcrete_", StringComparison.Ordinal)).ToArray();
            Assert.That(exterior, Has.Length.EqualTo(2));
            Assert.That(exterior.All(renderer => renderer.enabled), Is.True);
            foreach (string anchor in new[] { "Player spawn", "Door approach", "Computer approach", "Bed approach", "Poster beach approach", "Poster device approach" })
            {
                var marker = GameObject.Find(anchor);
                Assert.That(marker, Is.Not.Null, anchor);
                Assert.That(Physics.Raycast(marker.transform.position + up * .1f, -up, out var floor, .3f, ~0, QueryTriggerInteraction.Ignore),
                    Is.True, anchor + " needs a physical floor below the feet.");
                Assert.That(floor.collider.name.IndexOf("Floor", StringComparison.OrdinalIgnoreCase), Is.GreaterThanOrEqualTo(0), anchor);
                Assert.That(Vector3.Dot(marker.transform.position - origin, up), Is.EqualTo(.055f).Within(.03f), anchor);
            }
            host.Player.FacePoint(Marker(room, "BedCentre") + Vector3.up * .5f);
            yield return Capture(host, "reference-room-wide");
            var door = Object.FindObjectOfType<HingedDoor>();
            var reaction = TutorialReaction(true);
            host.RestoreCheckpoint(reaction.Save());
            Assert.That(door.CurrentAngle, Is.EqualTo(60).Within(.001f));
            Complete(host.Runtime, PresentationKind.Reaction);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Evening));
            Assert.That(host.Runtime.GetView().RoomDoorOpen, Is.False);
            yield return Until(host, () => !door.IsMoving && host.Runtime.GetView().TerminalPage == TerminalPage.Archive,
                "night closes the door while the terminal finishes its existing signal loss");
            Assert.That(door.CurrentAngle, Is.Zero.Within(.001f));
            Assert.That(host.Runtime.ReturnToDirectory().Accepted, Is.True);
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            var guidance = Object.FindObjectOfType<BedtimeGuidance>();
            yield return Until(host, () => host.Runtime.GetView().Input == InputContext.World
                && guidance.Isolation > .999f && guidance.BeamStrength > .999f, "the reference room isolates around the bed");
            Assert.That(Object.FindObjectsOfType<Light>().Where(light => light != guidance.Spotlight)
                .All(light => !light.enabled || light.intensity < .001f), Is.True,
                "All corridor and room lights must go dark outside the bed spotlight.");
            Assert.That(guidance.Spotlight.enabled, Is.True);
            Assert.That(RenderSettings.ambientIntensity, Is.Zero);
            var poster = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-instrument");
            yield return StageAtPoster(host, poster, false);
            Assert.That(host.Player.FocusedReadable, Is.Null);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            Assert.That(poster.IsHighlighted, Is.False);
            Assert.That(host.Screen.RoomCaptionVisible, Is.False);
            Assert.That(door.TryInteract().Accepted, Is.False);
            MovePlayer(host, GameObject.Find("Player spawn").transform.position);
            yield return null;
            host.Player.FacePoint(Marker(room, "BedCentre") + Vector3.up * .6f);
            yield return Capture(host, "reference-room-night-only-bed");
            yield return StageAt(host, InteractionTarget.Bed);
            host.Player.RefreshInteractionFocus();
            // The visible bed remains the only permitted route to the next day.
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.EqualTo(new[] { InteractionTarget.Bed }));
            Debug.Log("MINIGAME_REFERENCE_LAYOUT_PASSED net9.70078x4.41176x3.12066; horizontalFOV61.9275; grounded anchors; exterior concrete; door auto-close; corridor dark; posters sleep-disabled");
        }

        private static MiniGameBootstrap Host()
        {
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null);
            Assert.That(host.CampaignActive, Is.True);
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            // These tests isolate physical hinge/poster rules; camera travel has its own regression fixture.
            var doorCamera = host.Player.ViewCamera.GetComponent<DoorViewCamera>();
            if (doorCamera != null) doorCamera.enabled = false;
            return host;
        }

        private static IEnumerator StageAt(MiniGameBootstrap host, InteractionTarget target)
        {
            var anchor = GameObject.Find(target + " approach");
            Assert.That(anchor, Is.Not.Null);
            MovePlayer(host, anchor.transform.position);
            yield return null;
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == target);
            host.Player.FacePoint(interaction.GetComponent<Collider>().bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.EqualTo(interaction), "The validated anchor must see the actual " + target + ".");
        }

        private static IEnumerator StageAtPoster(MiniGameBootstrap host, ReadableProp poster, bool requireFocus = true)
        {
            var anchor = GameObject.Find(poster.Id == "poster-beach" ? "Poster beach approach" : "Poster device approach");
            Assert.That(anchor, Is.Not.Null);
            MovePlayer(host, anchor.transform.position);
            yield return null;
            host.Player.FacePoint(poster.Surface.bounds.center);
            host.Player.RefreshInteractionFocus();
            if (requireFocus) Assert.That(host.Player.FocusedReadable, Is.EqualTo(poster), "The poster needs a clear, usable approach anchor.");
        }

        private static void PositionCameraFromSurface(MiniGameBootstrap host, ReadableProp poster, float distance)
        {
            var camera = host.Player.ViewCamera;
            var direction = (camera.transform.position - poster.Surface.bounds.center).normalized;
            var desired = poster.Surface.bounds.center + direction * distance;
            MovePlayer(host, host.Player.transform.position + desired - camera.transform.position);
            host.Player.FacePoint(poster.Surface.bounds.center);
            host.Player.RefreshInteractionFocus();
        }

        private static void MovePlayer(MiniGameBootstrap host, Vector3 position)
        {
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static Vector3 Marker(GameObject room, string name) => room.GetComponentsInChildren<Transform>(true)
            .Single(item => item.name == "Unity_" + name).position;

        private static float Span(Vector3 center, Vector3 direction)
        {
            Assert.That(Physics.Raycast(center, direction, out var positive, 15, ~0, QueryTriggerInteraction.Ignore), Is.True);
            Assert.That(Physics.Raycast(center, -direction, out var negative, 15, ~0, QueryTriggerInteraction.Ignore), Is.True);
            return positive.distance + negative.distance;
        }

        private static float VerticalSpan(Vector3 center)
        {
            var lower = Physics.RaycastAll(center, Vector3.down, 5, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.name.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(hit => hit.distance).FirstOrDefault();
            Assert.That(lower.collider, Is.Not.Null, "The reference floor must have a collider.");
            Assert.That(Physics.Raycast(center, Vector3.up, out var ceiling, 5, ~0, QueryTriggerInteraction.Ignore), Is.True,
                "The reference ceiling must close the room above the player.");
            return lower.distance + ceiling.distance;
        }

        private static Button FindContinue(MiniGameBootstrap host) => host.Screen.GetComponentsInChildren<Button>(false)
            .FirstOrDefault(button => button.name == "Continue" && button.IsActive() && button.IsInteractable());

        private static IEnumerator Until(MiniGameBootstrap host, Func<bool> condition, string expected)
        {
            float deadline = Time.realtimeSinceStartup + 12;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expected + ": phase=" + host.Runtime.GetView().Game.Phase
                + " input=" + host.Runtime.GetView().Input + " cue=" + host.Runtime.GetView().Cue?.Kind);
        }

        private static GameRuntime TutorialReaction(bool openDoor)
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            var package = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var runtime = new GameRuntime(package.Rules, package.Content, true);
            if (openDoor)
            {
                Assert.That(runtime.ToggleRoomDoor().Accepted, Is.True);
                Complete(runtime, PresentationKind.DoorMotion);
            }
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Complete(runtime, PresentationKind.EnterTerminal);
            Assert.That(runtime.EnterSelectedPatient("tutorial").Accepted, Is.True);
            Complete(runtime, PresentationKind.Introduction);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            return runtime;
        }

        private static string DayOneMorning()
        {
            var runtime = TutorialReaction(false);
            Complete(runtime, PresentationKind.Reaction);
            Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.ReturnToDirectory().Accepted, Is.True);
            Assert.That(runtime.LeaveTerminal().Accepted, Is.True);
            Complete(runtime, PresentationKind.ExitTerminal);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Complete(runtime, PresentationKind.Sleep);
            Complete(runtime, PresentationKind.Dream);
            Assert.That(runtime.TryInteract(InteractionTarget.Bed).Accepted, Is.True);
            Assert.That(runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.MorningInspection));
            return runtime.Save();
        }

        private static void Complete(GameRuntime runtime, PresentationKind expected)
        {
            var cue = runtime.GetView().Cue;
            Assert.That(cue, Is.Not.Null);
            Assert.That(cue.Kind, Is.EqualTo(expected));
            Assert.That(runtime.CompleteCue(cue.Token).Accepted, Is.True);
        }

        [Serializable] private sealed class CheckpointEnvelope { public string CoreJson; }
        private static string CoreJson(GameRuntime runtime) => JsonUtility.FromJson<CheckpointEnvelope>(runtime.Save()).CoreJson;
        private static IEnumerator Capture(MiniGameBootstrap host, string name) => PrototypePlayModeTests.CaptureIfRequested(host, name);
    }
}
