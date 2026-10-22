using System.Collections;
using MiniGame.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class InteractionDistancePlayModeTests
    {
        [UnityTest]
        public IEnumerator PerObjectDistancesControlFocusAndUseBeyondTheFormerGlobalCap()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = new Vector3(0, 50, 0);
            host.Player.transform.rotation = Quaternion.identity;
            var camera = host.Player.ViewCamera;
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                target.transform.position = camera.transform.position + Vector3.forward * 4.5f;
                host.Player.FacePoint(target.transform.position);
                var interaction = target.AddComponent<WorldInteraction>();
                interaction.Configure(InteractionTarget.Computer, "distance fixture", target.GetComponent<Renderer>(), 3.2f);
                blocker.SetActive(false);
                Physics.SyncTransforms();
                host.Player.RefreshInteractionFocus();
                Assert.That(host.Player.FocusedInteraction, Is.Null);
                Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
                interaction.InteractionDistance = 5;
                host.Player.RefreshInteractionFocus();
                Assert.That(host.Player.FocusedInteraction, Is.SameAs(interaction));
                Assert.That(interaction.IsHighlighted, Is.True);
                blocker.transform.position = camera.transform.position + Vector3.forward * 2;
                blocker.SetActive(true); Physics.SyncTransforms();
                host.Player.RefreshInteractionFocus();
                Assert.That(host.Player.FocusedInteraction, Is.Null, "A nearer solid collider blocks interaction even with a longer range.");
                Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
                blocker.SetActive(false); Physics.SyncTransforms();
                Assert.That(host.Player.InteractPrimaryFocused(), Is.True, "Configured range must govern the action as well as the focus.");
            }
            finally { Object.Destroy(target); Object.Destroy(blocker); controller.enabled = true; }
        }

        [UnityTest]
        public IEnumerator PosterDistanceAlsoControlsFocusAndCaptionWithoutASeparateGlobalLimit()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false; host.Player.transform.position = new Vector3(0, 50, 0);
            var camera = host.Player.ViewCamera;
            var target = GameObject.CreatePrimitive(PrimitiveType.Quad);
            try
            {
                target.transform.position = camera.transform.position + Vector3.forward * 4;
                host.Player.FacePoint(target.transform.position);
                var poster = target.AddComponent<ReadableProp>();
                poster.Configure("range-fixture", "distance caption", target.GetComponent<Renderer>(), 2);
                Physics.SyncTransforms();
                Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
                poster.InteractionDistance = 4.1f;
                host.Player.RefreshInteractionFocus();
                Assert.That(host.Player.FocusedReadable, Is.SameAs(poster));
                Assert.That(host.Player.InteractPrimaryFocused(), Is.True);
                Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo("distance caption"));
                poster.InteractionDistance = 3.9f;
                host.Player.RefreshInteractionFocus();
                Assert.That(poster.IsHighlighted, Is.False);
                Assert.That(host.Player.InteractPrimaryFocused(), Is.False);
            }
            finally { Object.Destroy(target); controller.enabled = true; }
        }
    }
}
