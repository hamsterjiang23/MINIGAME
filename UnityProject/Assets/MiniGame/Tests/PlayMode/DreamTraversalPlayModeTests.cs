using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamTraversalPlayModeTests
    {
        private readonly List<Object> cleanup = new List<Object>();
        private readonly Vector3 origin = new Vector3(400, 2, 400);
        private ApartmentPlayer player;
        private CharacterController controller;
        private Camera camera;
        private Transform wake;
        private Bounds bounds;
        private WorldInteraction bed, computer, door;

        [TearDown]
        public void TearDown()
        {
            if (player != null) player.enabled = false;
            for (int index = cleanup.Count - 1; index >= 0; index--)
                if (cleanup[index] != null) Object.DestroyImmediate(cleanup[index]);
            cleanup.Clear();
        }

        [UnityTest]
        public IEnumerator AfterDreamPassesFurnitureClampsTheFloorAndWakesSafelyAfterBed()
        {
            CreateRig();
            var runtime = Runtime();
            player.Bind(runtime, null);
            Physics.SyncTransforms();
            controller.Move(Vector3.forward * 2);
            Assert.That(player.transform.position.z, Is.LessThan(origin.z + 1), "Normal movement must retain physical furniture collision.");
            DriveTo(runtime, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.AfterDream);
            Assert.That(player.DreamTraversalActive, Is.True);
            Assert.That(controller.enabled, Is.False);
            Assert.That(player.TryMoveDreamTraversal(origin - player.transform.position), Is.True);
            Assert.That(player.TryMoveDreamTraversal(Vector3.forward * 2), Is.True);
            Assert.That(player.transform.position.z, Is.EqualTo(origin.z + 2).Within(.001f), "Dream traversal must cross the furniture blocker.");
            Assert.That(player.transform.position.y, Is.EqualTo(origin.y));
            player.TryMoveDreamTraversal(origin - player.transform.position);
            player.FacePoint(bed.GetComponent<Collider>().bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.SameAs(bed), "The invisible furniture blocker must not occlude the allowed bed interaction.");
            Assert.That(player.FocusedReadable, Is.Null);
            var pausedPosition = player.transform.position;
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(player.TryMoveDreamTraversal(Vector3.one), Is.False);
            yield return null;
            Assert.That(player.transform.position, Is.EqualTo(pausedPosition));
            Assert.That(runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(player.TryMoveDreamTraversal(new Vector3(100, 100, 100)), Is.True);
            Assert.That(player.transform.position.x, Is.EqualTo(bounds.max.x));
            Assert.That(player.transform.position.z, Is.EqualTo(bounds.max.z));
            Assert.That(player.transform.position.y, Is.EqualTo(origin.y));
            player.TryMoveDreamTraversal(origin - player.transform.position);
            player.FacePoint(bed.GetComponent<Collider>().bounds.center);
            Assert.That(player.InteractFocused(), Is.True);
            Assert.That(runtime.GetView().Game.Day, Is.EqualTo(2));
            Assert.That(player.DreamTraversalActive, Is.False);
            Assert.That(controller.enabled, Is.True);
            Assert.That(player.transform.position, Is.EqualTo(wake.position), "Returning to daytime must use the safe authored wake position.");
        }

        [UnityTest]
        public IEnumerator SelfReviewOffersOnlyComputerAndDoesNotTeleportOnTerminalEntry()
        {
            CreateRig();
            var runtime = Runtime(100);
            DriveTo(runtime, view => view.Game.Phase == GamePhase.SelfReview);
            player.Bind(runtime, null);
            Physics.SyncTransforms();
            Assert.That(player.DreamTraversalActive, Is.True);
            player.FacePoint(bed.GetComponent<Collider>().bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.Null);
            player.FacePoint(door.GetComponent<Collider>().bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.Null);
            player.FacePoint(computer.GetComponent<Collider>().bounds.center);
            player.RefreshInteractionFocus();
            Assert.That(player.FocusedInteraction, Is.SameAs(computer));
            var previousPosition = player.transform.position;
            Assert.That(player.InteractSecondaryFocused(), Is.True);
            Assert.That(runtime.GetView().TerminalOpen, Is.True);
            Assert.That(runtime.GetView().TerminalPage, Is.EqualTo(TerminalPage.SelfReview));
            Assert.That(player.DreamTraversalActive, Is.False);
            Assert.That(controller.enabled, Is.True);
            Assert.That(player.transform.position, Is.EqualTo(previousPosition), "The seating transition must start from the current position, not the bed's wake anchor.");
            yield return null;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DisableAndRebindRestoreThePreviousControllerState(bool initiallyEnabled)
        {
            CreateRig();
            controller.enabled = initiallyEnabled;
            var night = Runtime();
            DriveTo(night, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.AfterDream);
            player.Bind(night, null);
            Assert.That(controller.enabled, Is.False);
            player.enabled = false;
            Assert.That(controller.enabled, Is.EqualTo(initiallyEnabled));
            player.enabled = true;
            Assert.That(controller.enabled, Is.False);
            player.Bind(Runtime(), null);
            Assert.That(controller.enabled, Is.EqualTo(initiallyEnabled));
            Assert.That(player.DreamTraversalActive, Is.False);
        }

        [UnityTest]
        public IEnumerator BedLightingStartsAfterDreamAndSelfReviewKeepsOnlyTheTerminalGlow()
        {
            CreateRig();
            var emissiveShader = Shader.Find("Standard");
            var screenMaterial = Track(new Material(emissiveShader));
            screenMaterial.EnableKeyword("_EMISSION");
            screenMaterial.SetColor("_EmissionColor", Color.green * .8f);
            computer.Surface.sharedMaterial = screenMaterial;
            var roomMaterial = Track(new Material(emissiveShader));
            roomMaterial.EnableKeyword("_EMISSION");
            roomMaterial.SetColor("_EmissionColor", Color.red);
            door.Surface.sharedMaterial = roomMaterial;
            var spotlight = Track(new GameObject("Dream bed spotlight")).AddComponent<Light>();
            spotlight.type = LightType.Spot;
            spotlight.transform.position = bed.transform.position + Vector3.up * 2;
            spotlight.transform.rotation = Quaternion.Euler(90, 0, 0);
            var guidance = Track(new GameObject("Dream bedtime guidance")).AddComponent<BedtimeGuidance>();
            guidance.Configure(camera, spotlight, new[] { bed.Surface }, bed.Surface.bounds.center, 1);
            guidance.ConfigureSelfReview(computer.Surface);
            var runtime = Runtime();
            DriveTo(runtime, view => view.Game.Day == 1 && view.Game.Phase == GamePhase.Evening && !view.TerminalOpen && view.Cue == null);
            Assert.That(BedtimeGuidance.IsSleepOnly(runtime.GetView()), Is.False);
            guidance.Bind(runtime);
            Assert.That(spotlight.enabled, Is.False);
            Assert.That(guidance.Isolation, Is.EqualTo(1));
            Assert.That(runtime.BeginNightDream().Accepted, Is.True);
            guidance.Bind(runtime);
            Assert.That(spotlight.enabled, Is.False);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            guidance.Bind(runtime);
            Assert.That(BedtimeGuidance.IsSleepOnly(runtime.GetView()), Is.True);
            Assert.That(spotlight.enabled, Is.True);
            var review = Runtime(100);
            DriveTo(review, view => view.Game.Phase == GamePhase.SelfReview);
            guidance.Bind(review);
            Assert.That(spotlight.enabled, Is.False);
            Assert.That(Emission(computer.Surface).g, Is.GreaterThan(.1f));
            Assert.That(Emission(door.Surface).r, Is.Zero.Within(.0001f));
            Assert.That(guidance.GetComponentsInChildren<Light>().Single(light => light.name == "Self-review terminal glow").enabled, Is.True);
            guidance.enabled = false;
            Assert.That(Emission(door.Surface).r, Is.EqualTo(1));
            yield return null;
        }

        private void CreateRig()
        {
            var body = Track(new GameObject("Dream traversal player", typeof(CharacterController)));
            body.transform.position = origin;
            controller = body.GetComponent<CharacterController>();
            controller.height = 1.8f; controller.center = Vector3.up * .9f; controller.radius = .3f;
            camera = Track(new GameObject("Dream traversal camera", typeof(Camera))).GetComponent<Camera>();
            camera.enabled = false;
            camera.transform.SetParent(body.transform, false);
            camera.transform.localPosition = Vector3.up * 1.6f;
            player = body.AddComponent<ApartmentPlayer>();
            player.Configure(camera);
            wake = Track(new GameObject("Dream safe wake pose")).transform;
            wake.position = origin + new Vector3(-1, 0, -1);
            bounds = new Bounds(origin, new Vector3(6, 4, 6));
            player.ConfigureDreamTraversal(bounds, wake, null);
            var blocker = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            blocker.name = "Furniture occluder";
            blocker.transform.position = origin + new Vector3(0, .9f, 1);
            blocker.transform.localScale = new Vector3(5, 2, .25f);
            bed = Target(InteractionTarget.Bed, 0);
            computer = Target(InteractionTarget.Computer, 2);
            door = Target(InteractionTarget.Door, -2);
            Physics.SyncTransforms();
        }

        private WorldInteraction Target(InteractionTarget kind, float x)
        {
            var root = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            root.name = kind.ToString();
            root.transform.position = origin + new Vector3(x, 1.6f, 2.5f);
            root.transform.localScale = Vector3.one * .6f;
            var interaction = root.AddComponent<WorldInteraction>();
            interaction.Configure(kind, kind.ToString(), root.GetComponent<Renderer>(), 5);
            return interaction;
        }

        private static Color Emission(Renderer renderer)
        {
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, 0);
            int id = Shader.PropertyToID("_EmissionColor");
            return block.HasProperty(id) ? block.GetColor(id) : renderer.sharedMaterial.GetColor(id);
        }

        private static GameRuntime Runtime(int initial = 0)
        {
            var patient = new PatientDefinition("patient", "测试病人", 0, 0, null, PatientPresence.TreatmentEnded);
            var rules = new GameRules("dream-traversal-fixture", new[] { patient }, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, 0, new[] { new MeetingDefinition(patient.Id,
                    new[] { new RoundDefinition("day" + day, new[] { new ResponseDefinition("pick", "branch", 1) }) },
                    null, 100, 0, new Dictionary<Disposition,string> { [Disposition.Treat] = "treated" }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(initial, 100, 100, new Dictionary<string,int>());
            return new GameRuntime(rules, new FixtureContent(), true);
        }

        private static void DriveTo(GameRuntime runtime, Func<RuntimeView,bool> stop)
        {
            for (int step = 0; step < 180; step++)
            {
                var view = runtime.GetView();
                if (stop(view)) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory) result = runtime.EnterSelectedPatient(view.Game.CurrentPatientId);
                else if (view.Input == InputContext.Dialogue) result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                    ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Treat)
                    : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "pick");
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else result = runtime.TryInteract(InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, "Fixture could not advance from " + view.Game.Phase);
            }
            Assert.Fail("The bounded traversal fixture did not reach its target phase.");
        }

        private T Track<T>(T value) where T : Object { cleanup.Add(value); return value; }
        private sealed class FixtureContent : IGameContent
        {
            public string Revision => "dream-traversal-content";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "测试病人", "测试对白",
                view.AvailableResponseIds.Select(id => new ResponseContent(id,"回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind,"观察","记录");
        }
    }
}
