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
    public sealed class DayZeroCollisionPlayModeTests
    {
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            if (host != null)
            {
                host.enabled = false;
                host.Dreams?.PrepareForRuntimeChange();
                host.Effects?.PrepareForRuntimeChange();
            }
            MiniGameSessionHost.Release();
            var scene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Day0 collision cleanup");
            SceneManager.SetActiveScene(empty);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator MainRoomDayZeroKeepsFurnitureSolidAndBedTouchableThroughOneSleep()
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
            for (int index = 0; index < furniture.Length; index++)
                Assert.That(furniture[index].enabled, Is.EqualTo(enabledBefore[index]), furniture[index].name + " must retain its authored collision after Day0.");
            Assert.That(floor.enabled && !floor.isTrigger, Is.True, "The floor must still support the character.");
            Assert.That(door.enabled && !door.isTrigger, Is.True, "The door remains a solid room boundary.");
            Assert.That(host.Player.GetComponent<CharacterController>().enabled, Is.True, "Day0 retains player collision.");
            Assert.That(host.Player.DreamTraversalActive, Is.False);
            Assert.That(bedCollider.enabled, Is.True);
            Assert.That(bedCollider.isTrigger, Is.EqualTo(bedWasTrigger), "The bed retains its authored solid collision and ray target.");

            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Bed approach").transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            host.Player.FacePoint(bedCollider.bounds.center);
            host.Player.RefreshInteractionFocus();
            Assert.That(host.Player.FocusedInteraction, Is.EqualTo(bed), "The production player ray must detect the solid bed.");
            Quaternion headingBeforeSleep = host.Player.transform.rotation;
            Quaternion lookBeforeSleep = host.Player.ViewCamera.transform.rotation;
            float fovBeforeSleep = host.Player.ViewCamera.fieldOfView;
            Assert.That(host.Player.InteractFocused(), Is.True);
            // Some presentation profiles advance the day immediately; others retain a Sleep cue first.
            for (int step = 0; host.Runtime.GetView().Game.Day == 0 && step < 4; step++)
            {
                var cue = host.Runtime.GetView().Cue;
                Assert.That(cue, Is.Not.Null, "A single bed interaction must complete Day0.");
                Assert.That(host.Runtime.CompleteCue(cue.Token).Accepted, Is.True);
            }
            Assert.That(host.Runtime.GetView().Game.Day, Is.EqualTo(1));
            float wakeDeadline = Time.realtimeSinceStartup + 5;
            while (host.Screen.TransitionLocked && Time.realtimeSinceStartup < wakeDeadline) yield return null;
            Assert.That(host.Screen.TransitionLocked, Is.False);
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

        [UnityTest, Timeout(30000)]
        public IEnumerator DayZeroBedtimePauseAndCheckpointReloadPreserveAuthoredColliderStates()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            var disabled = new GameObject("Authored disabled collision").AddComponent<BoxCollider>();
            disabled.enabled = false;
            var trigger = new GameObject("Authored trigger collision").AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            var states = host.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Where(collider => !(collider is CharacterController))
                .ToDictionary(collider => collider, collider => (collider.enabled, collider.isTrigger));
            Assert.That(states.Count, Is.GreaterThan(10));
            var night = TutorialBedtime(host.Runtime.UsesDreamFlow);
            host.RestoreCheckpoint(night.Save());
            AssertColliderStates(states);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            yield return null;
            AssertColliderStates(states);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            host.RestoreCheckpoint(host.Runtime.Save());
            yield return null;
            AssertColliderStates(states);
            Object.Destroy(disabled.gameObject);
            Object.Destroy(trigger.gameObject);
        }

        private static void AssertColliderStates(System.Collections.Generic.Dictionary<Collider, (bool enabled, bool isTrigger)> states)
        {
            foreach (var pair in states)
            {
                Assert.That(pair.Key, Is.Not.Null);
                Assert.That(pair.Key.enabled, Is.EqualTo(pair.Value.enabled), pair.Key.name);
                Assert.That(pair.Key.isTrigger, Is.EqualTo(pair.Value.isTrigger), pair.Key.name);
            }
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

    }
}
