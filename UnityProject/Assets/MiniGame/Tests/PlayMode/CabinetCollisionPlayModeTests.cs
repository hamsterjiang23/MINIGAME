using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class CabinetCollisionPlayModeTests
    {
        private ApartmentPlayer player;
        private CharacterController controller;
        private Bounds cabinetBounds;
        private float floorY;

        [UnitySetUp]
        public IEnumerator LoadProductionRoom()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectsOfType<MiniGameBootstrap>()
                .Single(item => item.gameObject.scene.name == "MainRoomCombined");
            host.enabled = false;
            host.Dreams?.PrepareForRuntimeChange();
            host.Effects?.PrepareForRuntimeChange();
            player = host.Player;
            player.enabled = false;
            controller = player.GetComponent<CharacterController>();
            cabinetBounds = SceneComponents<MeshRenderer>().Single(item => item.name == "B_立方体.023").bounds;
            floorY = SceneComponents<Collider>().Single(item => item.name == "Reference floor collider").bounds.max.y;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MiniGameSessionHost.Release();
            var scene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Cabinet collision cleanup");
            SceneManager.SetActiveScene(empty);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator FrontAndCornersAllowRetreatAfterWalkingIntoCabinet()
        {
            for (int sample = 0; sample < 13; sample++)
            {
                var start = new Vector3(Mathf.Lerp(cabinetBounds.min.x - .3f, cabinetBounds.max.x + .3f, sample / 12f),
                    floorY + .055f, cabinetBounds.min.z - 1);
                AssertApproachAndRetreat(start, Vector3.forward, "front " + sample);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DoorSideAllowsRetreatAfterWalkingIntoCabinet()
        {
            for (int sample = 0; sample < 13; sample++)
            {
                var start = new Vector3(cabinetBounds.min.x - 1, floorY + .055f,
                    Mathf.Lerp(cabinetBounds.min.z - .3f, cabinetBounds.max.z - .1f, sample / 12f));
                AssertApproachAndRetreat(start, Vector3.right, "door side " + sample);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PreviouslyTrappedPositionsCanWalkBackOut()
        {
            var positions = new[] { new Vector3(-6.1845f, -.5229f, -.0336f),
                new Vector3(-6.1611f, -.7029f, -.0624f), new Vector3(-6.2247f, -.7029f, .1892f) };
            for (int index = 0; index < positions.Length; index++)
            {
                Place(positions[index]);
                float gravity = 0;
                var direction = index == 2 ? Vector3.left : Vector3.back;
                Walk(direction, 60, ref gravity);
                Assert.That(Vector3.Dot(player.transform.position - positions[index], direction), Is.GreaterThan(1),
                    "A restored position at the old snag must be able to leave: " + index);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SolidCabinetStillAllowsInteractionRayToFamilyLetter()
        {
            var hull = SceneComponents<BoxCollider>().Single(item => item.name == "Cabinet solid collision");
            Assert.That(hull.enabled && !hull.isTrigger, Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(controller.gameObject.layer, hull.gameObject.layer), Is.False,
                "The ray-transparent hull must still stop the player.");
            var letter = SceneComponents<Collider>().Single(item => item.name == "Opened family letter");
            var origin = new Vector3(letter.bounds.center.x, .832f, -.7f);
            Physics.SyncTransforms();
            Assert.That(Physics.Raycast(origin, (letter.bounds.center - origin).normalized, out var hit, 3.5f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.True);
            Assert.That(hit.collider, Is.EqualTo(letter), "The new walking hull must not block reading the family letter.");
            yield return null;
        }

        private T[] SceneComponents<T>() where T : Component => player.gameObject.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        private void AssertApproachAndRetreat(Vector3 start, Vector3 direction, string sample)
        {
            Place(start);
            float gravity = 0;
            Walk(direction, 120, ref gravity);
            var contact = player.transform.position;
            Walk(-direction, 60, ref gravity);
            Assert.That(Vector3.Dot(player.transform.position - contact, -direction), Is.GreaterThan(1),
                sample + ": walking back must move away from cabinet contact at " + contact);
            Assert.That(player.transform.position.y, Is.GreaterThan(floorY - .05f), "The floor must remain solid.");
        }

        private void Place(Vector3 position)
        {
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private void Walk(Vector3 direction, int frames, ref float gravity)
        {
            // Same 2.4 m/s motion and grounded gravity as ApartmentPlayer, at a fixed 60 Hz.
            for (int frame = 0; frame < frames; frame++)
            {
                gravity = controller.isGrounded ? -1 : gravity - 9.81f / 60;
                controller.Move((direction * 2.4f + Vector3.up * gravity) / 60);
            }
        }
    }
}
