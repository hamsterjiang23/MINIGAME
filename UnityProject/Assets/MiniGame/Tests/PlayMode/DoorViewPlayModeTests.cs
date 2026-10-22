using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class DoorViewPlayModeTests
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
        private ApartmentPlayer player;
        private Camera camera;
        private DoorViewCamera doorCamera;
        private HingedDoor door;

        [UnitySetUp]
        public IEnumerator LoadCombinedCampaignAndFacePhysicalDoor()
        {
            EnterDirectWorldMode();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null, "MainRoomCombined must contain its configured runtime host.");
            Assert.That(host.CampaignActive, Is.True, "Door camera regression uses the actual Day0 campaign.");
            CampaignWorldFixture.Restore(host);
            Assert.That(host.Runtime.GetView().Game.Day, Is.Zero);
            Assert.That(host.Runtime.GetView().Game.Phase, Is.EqualTo(GamePhase.Apartment));
            player = host.Player;
            player.enabled = false; // Keep physical mouse/keyboard input out of deterministic camera assertions.
            camera = player.ViewCamera;
            doorCamera = camera.GetComponent<DoorViewCamera>();
            Assert.That(doorCamera, Is.Not.Null, "The combined main camera must have DoorViewCamera configured.");
            var interaction = Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Door);
            door = interaction.GetComponentInParent<HingedDoor>();
            Assert.That(door, Is.Not.Null);
            Assert.That(doorCamera.Handles(door), Is.True, "DoorViewCamera needs the door, player and both authored pose references.");
            Assert.That(doorCamera.InsidePose, Is.Not.Null);
            Assert.That(doorCamera.OutsidePose, Is.Not.Null);
            Assert.That(Vector3.Distance(doorCamera.InsidePose.position, doorCamera.OutsidePose.position), Is.LessThan(.001f),
                "The main room observes from its aligned threshold without continuing into the hall.");

            var marker = GameObject.Find("Door approach");
            Assert.That(marker, Is.Not.Null, "The authored Door approach marker is required.");
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = marker.transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            var collider = interaction.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null, "The door interaction must be backed by its real collider.");
            player.FacePoint(collider.bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.EqualTo(interaction), "The staged camera ray must actually hit the door. " + Describe());
            yield return null; // Let the camera projection settle before recording the room FOV.
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World), Describe());
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator EntryStopsAtThresholdAndEPathReturnsBeforeClosingWithoutMovingTheBody()
        {
            var room = CapturePose();
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "door-view-room");
            Assert.That(player.InteractFocused(), Is.True, Describe());
            Assert.That(doorCamera.Phase, Is.EqualTo(DoorCameraPhase.Aligning));
            string openingToken = host.Runtime.GetView().Cue.Token;
            var phases = new List<DoorCameraPhase>();
            float deadline = Time.realtimeSinceStartup + 10f;
            while (host.Runtime.GetView().Input != InputContext.DoorView && Time.realtimeSinceStartup < deadline)
            {
                if (phases.Count == 0 || phases[phases.Count - 1] != doorCamera.Phase) phases.Add(doorCamera.Phase);
                AssertBody(room);
                Assert.That(player.InteractFocused(), Is.False, "Repeated E must not interrupt entry. " + Describe());
                Assert.That(doorCamera.TryEnter(), Is.False, "A direct repeated entry must not release the pending motion acknowledgment.");
                Assert.That(doorCamera.TryReturn(), Is.False, "Return is unavailable until the outside hold begins.");
                if (doorCamera.Phase != DoorCameraPhase.Viewing)
                {
                    Assert.That(host.Runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.DoorMotion), Describe());
                    Assert.That(host.Runtime.GetView().Cue?.Token, Is.EqualTo(openingToken));
                    Assert.That(door.MotionCompletionBlocked, Is.True, "Physical opening must not acknowledge before alignment finishes.");
                }
                if (doorCamera.Phase == DoorCameraPhase.WaitingForDoor)
                    AssertCameraAt(doorCamera.InsidePose.position, doorCamera.InsidePose.rotation, room.FieldOfView);
                yield return null;
            }
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView), "Entry timed out: " + Describe());
            Assert.That(phases.IndexOf(DoorCameraPhase.Aligning), Is.GreaterThanOrEqualTo(0));
            Assert.That(phases, Has.No.Member(DoorCameraPhase.Pushing),
                "Door opening must stop at its first aligned threshold. Observed phases: " + string.Join(", ", phases));
            AssertOutside(room);
            Assert.That(host.Runtime.CompleteCue(openingToken).Accepted, Is.False, "Opening acknowledgment is single-use.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "door-view-outside");

            Assert.That(player.InteractFocused(), Is.True, "InteractFocused is the same path used by the outside E prompt.");
            Assert.That(doorCamera.Phase, Is.EqualTo(DoorCameraPhase.Returning));
            string returnToken = host.Runtime.GetView().Cue.Token;
            bool observedClosing = false;
            deadline = Time.realtimeSinceStartup + 10f;
            while (host.Runtime.GetView().Input != InputContext.World && Time.realtimeSinceStartup < deadline)
            {
                AssertBody(room);
                Assert.That(player.InteractFocused(), Is.False, "Repeated E must be rejected during return and closing.");
                Assert.That(doorCamera.TryReturn(), Is.False);
                var view = host.Runtime.GetView();
                if (view.DoorViewActive)
                {
                    Assert.That(view.RoomDoorOpen, Is.True);
                    Assert.That(view.Cue?.Kind, Is.EqualTo(PresentationKind.DoorViewReturn), Describe());
                    Assert.That(view.Cue?.Token, Is.EqualTo(returnToken));
                    Assert.That(door.CurrentAngle, Is.EqualTo(door.OpenAngle).Within(.001f), "Do not close through the returning camera.");
                }
                else
                {
                    observedClosing = true;
                    Assert.That(view.Cue?.Kind, Is.EqualTo(PresentationKind.DoorMotion), Describe());
                    AssertCameraAt(room.CameraPosition, room.CameraRotation, room.FieldOfView);
                }
                yield return null;
            }
            Assert.That(observedClosing, Is.True, "The room camera must be restored before physical closing.");
            AssertReturned(room);
            Assert.That(host.Runtime.CompleteCue(returnToken).Accepted, Is.False);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "door-view-returned");
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator PauseFreezesAlignmentAndReturnAndResumesTheSameCue()
        {
            var room = CapturePose();
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => doorCamera.Phase == DoorCameraPhase.Aligning, "camera reaches its alignment phase");
            yield return PauseAndAssertFrozen(room);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "alignment resumes and reaches the threshold hold");
            AssertOutside(room);
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => doorCamera.Phase == DoorCameraPhase.Returning
                && Vector3.Distance(camera.transform.position, doorCamera.OutsidePose.position) > .02f, "return has physically advanced");
            yield return PauseAndAssertFrozen(room);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "return and closing resume after pause");
            AssertReturned(room);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator RestoringOutsideKeepsTheOriginalRoomPoseAcrossRepeatedRebinds()
        {
            var room = CapturePose();
            Assert.That(player.InteractFocused(), Is.True);
            string staleOpeningToken = host.Runtime.GetView().Cue.Token;
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "initial outside hold");
            string checkpoint = host.Runtime.Save();
            for (int load = 0; load < 3; load++)
            {
                host.RestoreCheckpoint(checkpoint);
                Assert.That(doorCamera.HasRoomPose, Is.True);
                Assert.That(Vector3.Distance(doorCamera.ReturnPosition, room.CameraPosition), Is.LessThan(.001f),
                    "An outside reload must not overwrite the stored room destination. Load=" + load);
                Assert.That(Quaternion.Angle(doorCamera.ReturnRotation, room.CameraRotation), Is.LessThan(.05f));
                AssertOutside(room);
                Assert.That(host.Runtime.CompleteCue(staleOpeningToken).Accepted, Is.False);
                yield return null;
            }
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "return after outside checkpoint restore");
            AssertReturned(room);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator RestoringMidReturnCancelsToTheRoomAndRejectsItsOldToken()
        {
            var room = CapturePose();
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "outside hold before saving return");
            Assert.That(player.InteractFocused(), Is.True);
            string staleReturnToken = host.Runtime.GetView().Cue.Token;
            yield return Until(() => doorCamera.Phase == DoorCameraPhase.Returning
                && Vector3.Distance(camera.transform.position, doorCamera.OutsidePose.position) > .02f, "return advances before checkpointing");
            string returning = host.Runtime.Save();
            host.RestoreCheckpoint(returning);
            AssertReturned(room);
            Assert.That(host.Runtime.GetView().Cue, Is.Null, "A restored return snaps to the room without replaying motion.");
            Assert.That(host.Runtime.CompleteCue(staleReturnToken).Accepted, Is.False);
            float deadline = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                AssertReturned(room);
                Assert.That(host.Runtime.GetView().Cue, Is.Null, "The abandoned camera motion must never publish a late callback.");
                yield return null;
            }
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction?.Target, Is.EqualTo(InteractionTarget.Door), Describe());
            Assert.That(player.InteractFocused(), Is.True, "A fresh door entry must still work after cancelling a saved return.");
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator OutsideHoldShowsTheReturnPromptThroughItsCanvasGroup()
        {
            var room = CapturePose();
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "outside hold before showing the return prompt");
            // The fixture disables physical input, so provide the same cursor as ApartmentPlayer.Update.
            host.Screen.SetInteractionCursor(InteractionCursorKind.Door, true);
            yield return Until(() => host.Screen.InteractionCursorVisible, "outside return cursor becomes visible");
            Assert.That(host.Screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Door));
            for (int frame = 0; frame < 3; frame++)
            {
                yield return null;
                Assert.That(host.Screen.InteractionCursorVisible, Is.True, "DoorView must keep the return cursor visible.");
                AssertOutside(room);
            }
            Assert.That(player.InteractPrimaryFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "return through the left-click cursor");
            AssertReturned(room);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator ImmediateDisableAndReenableDuringReturnDoesNotTrapPresentationInput()
        {
            var room = CapturePose();
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "outside hold");
            Assert.That(player.InteractFocused(), Is.True);
            doorCamera.enabled = false;
            doorCamera.enabled = true;
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "re-enabled return and door closure");
            AssertReturned(room);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator CameraPathAndOutsideSightlineClearTheOpenDoorLeaf()
        {
            var leaf = door.GetComponentsInChildren<Collider>().Single(item => item.name == "Entrance door");
            Assert.That(player.InteractFocused(), Is.True);
            float deadline = Time.realtimeSinceStartup + 12;
            while (host.Runtime.GetView().Input != InputContext.DoorView && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                Assert.That(Vector3.Distance(leaf.ClosestPoint(camera.transform.position), camera.transform.position),
                    Is.GreaterThan(camera.nearClipPlane), "The camera path must not intersect the moving door leaf. " + Describe());
            }
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView));
            Assert.That(Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, 8), Is.True);
            Assert.That(hit.collider, Is.Not.SameAs(leaf), "The outside view must show the corridor rather than the open door leaf.");
            Assert.That(player.InteractFocused(), Is.True);
            while (host.Runtime.GetView().Input != InputContext.World && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                Assert.That(Vector3.Distance(leaf.ClosestPoint(camera.transform.position), camera.transform.position),
                    Is.GreaterThan(camera.nearClipPlane), "The returning camera must clear the door before it closes.");
            }
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator FixedDoorAlignmentFacesOutwardAndHoldsInsideTheThresholdWithoutForwardDrift()
        {
            var room = CapturePose();
            var transforms = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var origin = transforms.Single(item => item.name == "Unity_ReferenceOrigin");
            var right = transforms.Single(item => item.name == "Unity_ReferenceRight");
            var outward = (right.position - origin.position).normalized;
            Assert.That(outward.sqrMagnitude, Is.GreaterThan(.999f), "The room reference markers must define the outward direction.");
            Assert.That(Mathf.Abs(outward.y), Is.LessThan(.0001f), "The closed doorway normal must remain horizontal.");
            var doorway = transforms.Single(item => item.name == "Unity_DoorCentre");
            var insidePosition = doorCamera.InsidePose.position;
            Assert.That(Vector3.Dot(insidePosition - doorway.position, outward), Is.EqualTo(-.85f).Within(.001f),
                "The observation point must remain 0.85 metres inside the actual doorway plane.");
            Assert.That(player.InteractFocused(), Is.True);
            float deadline = Time.realtimeSinceStartup + 10f;
            while (host.Runtime.GetView().Input != InputContext.DoorView && Time.realtimeSinceStartup < deadline)
            {
                Assert.That(doorCamera.Phase, Is.Not.EqualTo(DoorCameraPhase.Pushing), "Opening does not include a further corridor dolly.");
                if (doorCamera.Phase == DoorCameraPhase.WaitingForDoor)
                    AssertCameraAt(insidePosition, doorCamera.InsidePose.rotation, room.FieldOfView);
                AssertBody(room);
                yield return null;
            }
            AssertOutside(room);
            for (int frame = 0; frame < 10; frame++)
            {
                AssertCameraAt(insidePosition, doorCamera.InsidePose.rotation, room.FieldOfView);
                Assert.That(Vector3.Dot(camera.transform.forward, outward), Is.GreaterThan(.9999f),
                    "The threshold hold must keep looking through the door normal.");
                yield return null;
            }
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "door-view-threshold");
            Debug.Log("MINIGAME_DOOR_THRESHOLD_HOLD position=" + camera.transform.position.ToString("F5")
                + " alignedDistance=" + Vector3.Distance(camera.transform.position, insidePosition).ToString("F6")
                + " signedDoorwayDistance=" + Vector3.Dot(camera.transform.position - doorway.position, outward).ToString("F6"));
            Assert.That(player.InteractFocused(), Is.True, "E must still return from the threshold hold.");
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World, "E return and door closure after the threshold hold");
            AssertReturned(room);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator RestoringTerminalFromOutsideKeepsTerminalPoseAndReturnsToTheOriginalRoom()
        {
            var room = CapturePose();
            string roomCheckpoint = host.Runtime.Save();
            // Construct a real terminal checkpoint through accepted commands and the normal camera presentation.
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Effects.IsTransitioning && !host.Screen.TransitionLocked, "legal terminal entry used to prepare a checkpoint", 20);
            string terminalCheckpoint = host.Runtime.Save();
            host.RestoreCheckpoint(roomCheckpoint);
            AssertReturned(room);
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "outside hold before loading the terminal checkpoint");
            AssertOutside(room);

            host.RestoreCheckpoint(terminalCheckpoint);
            var destination = GameObject.Find("Terminal camera destination");
            Assert.That(destination, Is.Not.Null, "The terminal presentation must retain its authored camera destination.");
            for (int frame = 0; frame < 3; frame++)
            {
                Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.TerminalDirectory), Describe());
                Assert.That(host.Runtime.GetView().TerminalOpen, Is.True);
                Assert.That(host.Runtime.GetView().DoorViewActive, Is.False);
                Assert.That(doorCamera.Phase, Is.EqualTo(DoorCameraPhase.None));
                Assert.That(doorCamera.HasRoomPose, Is.False, "Door camera state must release its old pose before terminal binding.");
                AssertCameraAt(destination.transform.position, destination.transform.rotation, 48f);
                yield return null;
            }
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.World && !host.Effects.IsTransitioning,
                "terminal exit after replacing the outside view");
            AssertReturned(room);
            Assert.That(Vector3.Distance(camera.transform.position, doorCamera.OutsidePose.position), Is.GreaterThan(.1f),
                "Terminal exit must return to the original room position, never the previously visible outside position.");
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator RestoringOrdinaryDayZeroPatientReviewPreservesTheExplicitRoomReturnPose()
        {
            var room = CapturePose();
            string roomCheckpoint = host.Runtime.Save();
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && !host.Effects.IsTransitioning, "ordinary Day0 directory", 20);
            Assert.That(host.Runtime.EnterSelectedPatient(host.Runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(host.Runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.Review));
            Assert.That(host.Runtime.GetView().TerminalBootRequired, Is.False);
            string reviewCheckpoint = host.Runtime.Save();
            host.RestoreCheckpoint(roomCheckpoint);
            AssertReturned(room);
            Assert.That(player.InteractFocused(), Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.DoorView, "outside view before restoring patient review");
            host.RestoreCheckpoint(reviewCheckpoint);
            Assert.That(host.Effects.TryGetRoomReturnPose(out var actor, out var actorRotation,
                out var eyes, out var eyeRotation, out var fov), Is.True);
            Assert.That(Vector3.Distance(actor, room.PlayerPosition), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(actorRotation, room.PlayerRotation), Is.LessThan(.01f));
            Assert.That(Vector3.Distance(eyes, room.CameraPosition), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(eyeRotation, room.CameraRotation), Is.LessThan(.01f));
            Assert.That(fov, Is.EqualTo(room.FieldOfView).Within(.001f));
        }

        private IEnumerator PauseAndAssertFrozen(RoomPose room)
        {
            string token = host.Runtime.GetView().Cue.Token;
            var phase = doorCamera.Phase;
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            float fov = camera.fieldOfView;
            float angle = door.CurrentAngle;
            float deadline = Time.realtimeSinceStartup + .3f;
            int frames = 0;
            while (Time.realtimeSinceStartup < deadline || frames < 3)
            {
                Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.Paused));
                Assert.That(doorCamera.Phase, Is.EqualTo(phase), "Paused motion may not advance to a new phase.");
                AssertCameraAt(position, rotation, fov);
                AssertBody(room);
                Assert.That(door.CurrentAngle, Is.EqualTo(angle).Within(.00001f));
                Assert.That(host.Runtime.GetView().Cue?.Token, Is.EqualTo(token));
                Assert.That(player.InteractFocused(), Is.False);
                Assert.That(host.Runtime.CompleteCue(token).Accepted, Is.False);
                frames++;
                yield return null;
            }
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Runtime.GetView().Cue?.Token, Is.EqualTo(token), "Resuming preserves the outstanding acknowledgment token.");
            Assert.That(doorCamera.Phase, Is.EqualTo(phase));
        }

        private RoomPose CapturePose() => new RoomPose
        {
            CameraPosition = camera.transform.position,
            CameraRotation = camera.transform.rotation,
            PlayerPosition = player.transform.position,
            PlayerRotation = player.transform.rotation,
            FieldOfView = camera.fieldOfView
        };

        private void AssertBody(RoomPose room)
        {
            Assert.That(Vector3.Distance(player.transform.position, room.PlayerPosition), Is.LessThan(.001f), "Door observation only moves the camera. " + Describe());
            Assert.That(Quaternion.Angle(player.transform.rotation, room.PlayerRotation), Is.LessThan(.05f));
        }

        private void AssertCameraAt(Vector3 position, Quaternion rotation, float fov)
        {
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.001f), Describe());
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.05f), Describe());
            Assert.That(camera.fieldOfView, Is.EqualTo(fov).Within(.001f));
        }

        private void AssertOutside(RoomPose room)
        {
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.DoorView), Describe());
            Assert.That(host.Runtime.GetView().DoorViewActive, Is.True);
            Assert.That(host.Runtime.GetView().RoomDoorOpen, Is.True);
            Assert.That(host.Runtime.GetView().Cue, Is.Null);
            Assert.That(host.Runtime.GetView().AllowedInteractions, Is.Empty);
            Assert.That(doorCamera.Phase, Is.EqualTo(DoorCameraPhase.Viewing));
            Assert.That(door.IsMoving, Is.False);
            Assert.That(door.CurrentAngle, Is.EqualTo(door.OpenAngle).Within(.001f));
            AssertBody(room);
            AssertCameraAt(doorCamera.OutsidePose.position, doorCamera.OutsidePose.rotation, room.FieldOfView);
        }

        private void AssertReturned(RoomPose room)
        {
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World), Describe());
            Assert.That(host.Runtime.GetView().DoorViewActive, Is.False);
            Assert.That(host.Runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(doorCamera.Phase, Is.EqualTo(DoorCameraPhase.None));
            Assert.That(doorCamera.HasRoomPose, Is.False);
            Assert.That(door.IsMoving, Is.False);
            Assert.That(door.CurrentAngle, Is.EqualTo(0).Within(.001f));
            AssertBody(room);
            AssertCameraAt(room.CameraPosition, room.CameraRotation, room.FieldOfView);
        }

        private IEnumerator Until(Func<bool> condition, string expectation, float seconds = 10)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, expectation + ": " + Describe());
        }

        private string Describe()
        {
            var view = host.Runtime.GetView();
            return "input=" + view.Input + " cue=" + view.Cue?.Kind + " cameraPhase=" + doorCamera.Phase
                + " active=" + view.DoorViewActive + " open=" + view.RoomDoorOpen + " doorAngle=" + door.CurrentAngle
                + " blocked=" + door.MotionCompletionBlocked + " camera=" + camera.transform.position.ToString("F4")
                + " body=" + player.transform.position.ToString("F4");
        }

        private struct RoomPose
        {
            public Vector3 CameraPosition, PlayerPosition;
            public Quaternion CameraRotation, PlayerRotation;
            public float FieldOfView;
        }
    }
}
