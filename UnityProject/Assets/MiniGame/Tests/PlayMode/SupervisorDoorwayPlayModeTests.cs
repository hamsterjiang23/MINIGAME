using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class SupervisorDoorwayPlayModeTests
    {
        private GameObject root;
        private GameRuntime runtime;
        private GameRules rules;
        private PrototypeContent content;
        private ApartmentPlayer player;
        private DoorViewCamera doorway;
        private HingedDoor door;
        private SupervisorDoorwayView supervisor;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Supervisor doorway regression");
            var actor = new GameObject("Player", typeof(CharacterController), typeof(ApartmentPlayer));
            actor.transform.SetParent(root.transform);
            var controller = actor.GetComponent<CharacterController>();
            controller.height = 1.8f; controller.center = Vector3.up * .9f;
            player = actor.GetComponent<ApartmentPlayer>();
            player.enabled = false;
            var cameraObject = new GameObject("Door camera", typeof(Camera), typeof(DoorViewCamera));
            cameraObject.transform.SetParent(actor.transform);
            cameraObject.transform.localPosition = new Vector3(0, 1.6f, -.5f);
            cameraObject.GetComponent<Camera>().enabled = false;
            player.Configure(cameraObject.GetComponent<Camera>());
            var inside = new GameObject("Door alignment").transform;
            inside.SetParent(root.transform); inside.position = new Vector3(0, 1.6f, 0);
            var outside = new GameObject("Door observation").transform;
            outside.SetParent(root.transform); outside.SetPositionAndRotation(inside.position, inside.rotation);
            var hinge = new GameObject("Door", typeof(HingedDoor));
            hinge.transform.SetParent(root.transform); hinge.transform.position = new Vector3(0, 0, .85f);
            door = hinge.GetComponent<HingedDoor>();
            door.Configure(hinge.transform, 60, .1f);
            doorway = cameraObject.GetComponent<DoorViewCamera>();
            doorway.Configure(door, player, inside, outside);
            rules = PrototypeRules.Create(); content = new PrototypeContent(rules);
            runtime = new GameRuntime(rules, content);
            door.Bind(runtime); doorway.Bind(runtime);
            supervisor = root.AddComponent<SupervisorDoorwayView>();
            supervisor.Bind(runtime, player, doorway);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator SupervisorAlreadyStandsOutsideClosedDoorAndRemainsThroughMorningAndWork()
        {
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(supervisor.IsVisible, Is.True, "The world actor must exist before any door interaction.");
            var initialPosition = supervisor.Surface.transform.position;
            Assert.That(doorway.TryEnter(), Is.True);
            yield return UntilMorning();
            Assert.That(supervisor.IsVisible, Is.True);
            var surface = supervisor.Surface;
            Assert.That(surface.transform.parent, Is.Null, "The supervisor must be a world object, never a camera or Canvas overlay.");
            Assert.That(surface.GetComponentInParent<Canvas>(), Is.Null);
            Assert.That(surface.gameObject.layer, Is.Zero);
            Assert.That(surface.GetComponent<Collider>(), Is.Null, "A portrait must not introduce an indoor physics obstacle.");
            var outward = doorway.InsidePose.forward;
            Assert.That(Vector3.Dot(surface.transform.position - door.transform.position, outward), Is.EqualTo(.9f).Within(.001f),
                "The supervisor must stand beyond the actual door plane, outside the room.");
            Assert.That(surface.bounds.min.y, Is.EqualTo(player.GetComponent<CharacterController>().bounds.min.y + .1f).Within(.001f),
                "The supervisor is raised ten centimetres from his former floor-aligned position.");
            Assert.That(surface.bounds.size.y, Is.EqualTo(1.8f).Within(.001f));
            Assert.That(surface.bounds.size.x / surface.bounds.size.y, Is.EqualTo(1024f / 1536).Within(.001f));
            Assert.That(Vector3.Dot(surface.transform.right, doorway.InsidePose.right), Is.GreaterThan(.999f),
                "The front portrait must preserve its original left/right orientation.");

            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            Assert.That(runtime.GetView().Cue.Kind, Is.EqualTo(PresentationKind.Briefing));
            Assert.That(supervisor.IsVisible, Is.True);
            Accept(runtime.CompleteCue(runtime.GetView().Cue.Token));
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(doorway.TryReturn(), Is.True);
            float deadline = Time.realtimeSinceStartup + 5;
            while (runtime.GetView().Input != InputContext.World && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(runtime.GetView().RoomDoorOpen, Is.False);
            Assert.That(supervisor.IsVisible, Is.True, "Closing the physical door must not despawn the world actor.");
            Accept(runtime.TryInteract(InteractionTarget.Computer));
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(Vector3.Distance(surface.transform.position, initialPosition), Is.LessThan(.0001f),
                "Door and work transitions must never move the permanently placed supervisor.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator PlayerMovementAndDoorEntryDoNotMoveThePermanentSupervisor()
        {
            var controller = player.GetComponent<CharacterController>();
            var fixedPosition = supervisor.Surface.transform.position;
            controller.enabled = false;
            player.transform.position -= Vector3.up * .45f;
            controller.height = 1.2f; controller.center = Vector3.up * .6f;
            Accept(runtime.SetPaused(true));
            yield return null;
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(Vector3.Distance(supervisor.Surface.transform.position, fixedPosition), Is.LessThan(.0001f));
            Accept(runtime.SetPaused(false));
            Assert.That(doorway.TryEnter(), Is.True);
            yield return UntilMorning();
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(Vector3.Distance(supervisor.Surface.transform.position, fixedPosition), Is.LessThan(.0001f),
                "Entering the doorway must retain the actor's original pose instead of spawning at the player's new height.");
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator PauseAndCheckpointRebindKeepTheOutsideWorldPose()
        {
            Assert.That(doorway.TryEnter(), Is.True);
            yield return UntilMorning();
            var position = supervisor.Surface.transform.position;
            var rotation = supervisor.Surface.transform.rotation;
            var originalSurface = supervisor.Surface;
            Accept(runtime.SetPaused(true));
            yield return null;
            Assert.That(supervisor.IsVisible, Is.True);
            Assert.That(Vector3.Distance(position, supervisor.Surface.transform.position), Is.LessThan(.0001f));
            runtime = GameRuntime.Restore(runtime.Save(), rules, content);
            door.Bind(runtime); doorway.Bind(runtime); supervisor.Bind(runtime, player, doorway);
            Assert.That(supervisor.IsVisible, Is.True, "A paused morning checkpoint restores the same derived world visibility.");
            Assert.That(Vector3.Distance(position, supervisor.Surface.transform.position), Is.LessThan(.0001f));
            Assert.That(Quaternion.Angle(rotation, supervisor.Surface.transform.rotation), Is.LessThan(.001f));
            Assert.That(supervisor.Surface, Is.SameAs(originalSurface), "Rebinding must reuse the existing world actor.");
            Accept(runtime.SetPaused(false));
            yield return null;
            Assert.That(supervisor.IsVisible, Is.True);
        }

        private IEnumerator UntilMorning()
        {
            float deadline = Time.realtimeSinceStartup + 5;
            while (runtime.GetView().Cue?.Kind != PresentationKind.MorningInspection && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(runtime.GetView().Cue?.Kind, Is.EqualTo(PresentationKind.MorningInspection));
            Assert.That(runtime.GetView().RoomDoorOpen, Is.True);
        }

        private static void Accept(RuntimeResult result) => Assert.That(result.Accepted, Is.True, result.Reason);
    }
}
