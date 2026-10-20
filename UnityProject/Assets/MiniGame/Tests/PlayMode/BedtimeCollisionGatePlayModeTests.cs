using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class BedtimeCollisionGatePlayModeTests
    {
        [UnityTest, Timeout(30000)]
        public IEnumerator MainRoomDayZeroLeavesTheBedTouchableAndRestoresFurnitureAfterOneSleep()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var scene = host.gameObject.scene;
            var furniture = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Where(collider => collider.transform.parent != null && collider.transform.parent.name == "Room collision"
                    && (collider.name.StartsWith("B_HT01") || collider.name.StartsWith("B_obj_01")
                        || collider.name.StartsWith("B_SM_"))).ToArray();
            Assert.That(furniture.Length, Is.GreaterThanOrEqualTo(5), "Both authored room variants have furniture obstruction colliders.");
            var enabledBefore = furniture.Select(collider => collider.enabled).ToArray();
            var bed = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(item => item.Target == InteractionTarget.Bed);
            var bedCollider = bed.GetComponent<Collider>();
            bool bedWasTrigger = bedCollider.isTrigger;
            var floor = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Single(collider => collider.name == "Reference floor collider" || collider.name == "Floor substrate collider");
            var door = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(item => item.Target == InteractionTarget.Door).GetComponent<Collider>();

            var night = TutorialBedtime(host.Runtime.UsesDreamFlow);
            host.RestoreCheckpoint(night.Save());
            yield return null;
            var gate = host.GetComponent<BedtimeCollisionGate>();
            Assert.That(gate, Is.Not.Null);
            Assert.That(gate.Suppressing, Is.True);
            Assert.That(furniture.All(collider => !collider.enabled), Is.True);
            Assert.That(floor.enabled && !floor.isTrigger, Is.True, "The floor must still support the character.");
            Assert.That(door.enabled && !door.isTrigger, Is.True, "The door remains a solid room boundary.");
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True, "Day0 retains player collision.");
            Assert.That(host.Player.DreamTraversalActive, Is.False);
            Assert.That(bedCollider.enabled && bedCollider.isTrigger, Is.True, "The bed stops obstructing movement but retains its ray target.");
            Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ReadableProp>(true))
                .SelectMany(prop => prop.GetComponentsInChildren<Collider>(true)).All(collider => !collider.enabled), Is.True);

            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Bed approach").transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            host.Player.FacePoint(bedCollider.bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.EqualTo(bed), "The production player ray must detect the retained sleep trigger.");
            Quaternion headingBeforeSleep = host.Player.transform.rotation;
            Quaternion lookBeforeSleep = host.Player.ViewCamera.transform.rotation;
            float fovBeforeSleep = host.Player.ViewCamera.fieldOfView;
            Assert.That(host.Player.InteractFocused(), Is.True);
            // Sleep can be requested while the player stands inside furniture that is temporarily non-solid.
            controller.enabled = false;
            host.Player.transform.position = bedCollider.bounds.center - Vector3.up * .2f;
            controller.enabled = true;
            // Some presentation profiles advance the day immediately; others retain a Sleep cue first.
            for (int step = 0; host.Runtime.GetView().Game.Day == 0 && step < 4; step++)
            {
                var cue = host.Runtime.GetView().Cue;
                Assert.That(cue, Is.Not.Null, "A single bed interaction must complete Day0.");
                Assert.That(host.Runtime.CompleteCue(cue.Token).Accepted, Is.True);
            }
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            Assert.That(gate.Suppressing, Is.False);
            Assert.That(Quaternion.Angle(host.Player.transform.rotation, headingBeforeSleep), Is.LessThan(.01f), "Day0 waking must not turn the player toward the wake anchor.");
            Assert.That(Quaternion.Angle(host.Player.ViewCamera.transform.rotation, lookBeforeSleep), Is.LessThan(.01f), "Day0 waking must preserve the camera look angle.");
            Assert.That(host.Player.ViewCamera.fieldOfView, Is.EqualTo(fovBeforeSleep));
            Assert.That(bedCollider.bounds.Contains(host.Player.transform.position + Vector3.up * .5f), Is.False,
                "Waking relocates the player before solid furniture returns.");
            for (int index = 0; index < furniture.Length; index++)
                Assert.That(furniture[index].enabled, Is.EqualTo(enabledBefore[index]), furniture[index].name);
            Assert.That(bedCollider.isTrigger, Is.EqualTo(bedWasTrigger));
            Assert.That(floor.enabled && door.enabled && controller.enabled, Is.True);
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator DisableRebindAndReloadRestoreOriginalEnabledAndTriggerStates()
        {
            var scene = SceneManager.CreateScene("Bedtime collision isolation fixture");
            var owner = new GameObject("Collision fixture");
            SceneManager.MoveGameObjectToScene(owner, scene);
            try
            {
                var solids = Child(owner.transform, "Room collision");
                var furniture = Collider(solids, "B_HT01.001 collider");
                var originallyDisabled = Collider(solids, "B_SM_SingleBox collider");
                originallyDisabled.enabled = false;
                var unknown = Collider(solids, "Unclassified structural mesh");
                var floor = Collider(solids, "Reference floor collider");
                var bed = Collider(owner.transform, "Bed interaction collider");
                bed.gameObject.AddComponent<WorldInteraction>().Configure(InteractionTarget.Bed, "Bed", null);
                var disabledBed = Collider(bed.transform, "Disabled extra bed hitbox");
                disabledBed.enabled = false;
                var computer = Collider(owner.transform, "Computer interaction collider");
                computer.gameObject.AddComponent<WorldInteraction>().Configure(InteractionTarget.Computer, "Computer", null);
                var door = Collider(owner.transform, "Entrance door");
                door.gameObject.AddComponent<WorldInteraction>().Configure(InteractionTarget.Door, "Door", null);
                var doorTrigger = Collider(door.transform, "Door interaction trigger");
                doorTrigger.isTrigger = true;
                var player = Child(owner.transform, "Player").gameObject.AddComponent<CharacterController>();
                var playerBody = Collider(player.transform, "Player physical child");
                var gate = owner.AddComponent<BedtimeCollisionGate>();
                var night = TutorialBedtime();
                gate.Bind(night);
                Assert.That(furniture.enabled || computer.enabled || doorTrigger.enabled, Is.False);
                Assert.That(unknown.enabled && floor.enabled && door.enabled && player.enabled && playerBody.enabled, Is.True);
                Assert.That(bed.isTrigger && disabledBed.isTrigger && !disabledBed.enabled, Is.True);
                night.SetPaused(true);
                Assert.That(gate.Suppressing, Is.True, "Pause does not restore furniture into the player's path.");
                gate.enabled = false;
                Assert.That(furniture.enabled && computer.enabled && doorTrigger.enabled, Is.True);
                Assert.That(originallyDisabled.enabled || disabledBed.enabled || bed.isTrigger || disabledBed.isTrigger, Is.False);
                gate.enabled = true;
                Assert.That(gate.Suppressing && !furniture.enabled && bed.isTrigger, Is.True);
                var package = Package();
                var restored = GameRuntime.Restore(night.Save(), package.Rules, package.Content);
                gate.Bind(restored);
                Assert.That(!furniture.enabled && bed.isTrigger, Is.True, "A saved bedtime re-derives collision presentation.");
                gate.Bind(new GameRuntime(package.Rules, package.Content, true));
                Assert.That(furniture.enabled && computer.enabled && !bed.isTrigger, Is.True);
                night.SetPaused(false);
                Assert.That(furniture.enabled, Is.True, "The previous runtime must no longer drive the gate.");
                Assert.That(originallyDisabled.enabled, Is.False);
                yield return null;
            }
            finally { Object.Destroy(owner); }
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        private static CompiledDialoguePackage Package()
        {
            var asset = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(asset, Is.Not.Null);
            return DialoguePackageCompiler.Compile(asset.PackageSource.text);
        }

        private static GameRuntime TutorialBedtime(bool dreamFlow = false)
        {
            var package = Package();
            var rules = package.Rules;
            if (dreamFlow)
            {
                var rebellion = Resources.Load<PlayerRebellionProfile>("MiniGame/PlayerRebellion");
                Assert.That(rebellion, Is.Not.Null);
                rules = rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, rebellion.CreateDeltas());
            }
            var runtime = new GameRuntime(rules, package.Content, true);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            if (runtime.GetView().TerminalPage == TerminalPage.Directory)
                Assert.That(runtime.EnterSelectedPatient(runtime.GetView().Game.CurrentPatientId).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.SubmitDisposition(runtime.GetView().Game.CurrentEncounterId, Disposition.Purify).Accepted, Is.True);
            for (int step = 0; runtime.GetView().Cue != null && step < 6; step++)
                Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            if (runtime.GetView().Game.Phase == GamePhase.Evening && runtime.UsesDreamFlow)
                Assert.That(runtime.BeginNightDream().Accepted, Is.True);
            Assert.That(BedtimeGuidance.IsSleepOnly(runtime.GetView()), Is.True);
            Assert.That(runtime.GetView().TerminalOpen, Is.False);
            return runtime;
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static BoxCollider Collider(Transform parent, string name) => Child(parent, name).gameObject.AddComponent<BoxCollider>();
    }
}
