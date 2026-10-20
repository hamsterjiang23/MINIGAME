using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PropNarrationPlayModeTests
    {
        [UnityTest]
        public IEnumerator ReadingOwnsInputPagesAndPauseWithoutChangingGameState()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            prop.Configure(prop.Id, "这里是我的故乡。\n但我从没去过海边。", prop.Surface);
            string checkpoint = host.Runtime.Save();
            int originalMask = host.Player.ViewCamera.cullingMask;
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False, "Opening click must not consume the first page.");
            Assert.That(host.Player.InteractFocused(), Is.False);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            yield return new WaitForSecondsRealtime(.3f);
            var narration = host.Screen.PropNarration;
            Assert.That(narration.GetComponentInChildren<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            var labels = narration.GetComponentsInChildren<Text>(true);
            Assert.That(labels.Length, Is.EqualTo(1), "The poster contains only its narrative text, without a controls footer.");
            Assert.That(labels[0].text, Is.EqualTo("这里是我的故乡。"));
            var edges = narration.GetComponentsInChildren<Image>(true).Where(image => image.name.StartsWith("Frame ")).ToArray();
            Assert.That(edges.Length, Is.EqualTo(4), "The narrative text is enclosed by a complete rectangular frame.");
            foreach (var edge in edges)
            {
                Assert.That(edge.color, Is.EqualTo(Color.white), edge.name);
                Assert.That(edge.rectTransform.rect.width, Is.GreaterThan(0), edge.name);
                Assert.That(edge.rectTransform.rect.height, Is.GreaterThan(0), edge.name);
                Assert.That(edge.raycastTarget, Is.False, "Frame geometry must not consume reading input.");
            }
            Assert.That(narration.GetComponentsInChildren<Transform>(true).Any(item => item.name == "Fading rule" || item.name == "Reading controls"), Is.False);
            var textPoint = host.Player.ViewCamera.WorldToViewportPoint(narration.WorldPosition);
            Assert.That(textPoint.z, Is.GreaterThan(.1f));
            Assert.That(textPoint.x, Is.InRange(.1f, .9f));
            Assert.That(textPoint.y, Is.InRange(.15f, .85f));
            Assert.That(narration.Opacity, Is.InRange(.1f, .99f), "Lettering must remain translucent.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "poster-white-frame");
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False);
            Assert.That(narration.IsOpen, Is.True);
            Assert.That(narration.Visible, Is.False);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            Assert.That(host.Screen.AdvancePropNarration(), Is.False);
            yield return null;
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo("但我从没去过海边。"));
            Assert.That(host.Screen.AdvancePropNarration(), Is.False, "One input cannot skip two pages.");
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.False);
            yield return new WaitForSecondsRealtime(.35f);
            yield return null;
            Assert.That(narration.IsOpen, Is.False);
            Assert.That(host.Player.ViewCamera.cullingMask, Is.EqualTo(originalMask));
            Assert.That(host.Runtime.GetView().Input, Is.EqualTo(InputContext.World));
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint), "Reading must not consume rules, rewards or inventory.");
        }

        [UnityTest]
        public IEnumerator RestoreDisableAndTargetRemovalReleaseNarrationAndCameraOwnership()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            string checkpoint = host.Runtime.Save();
            var camera = host.Player.ViewCamera;
            int originalMask = camera.cullingMask;
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            yield return null;
            host.RestoreCheckpoint(checkpoint);
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            Assert.That(camera.cullingMask, Is.EqualTo(originalMask));
            yield return null;
            host.Player.enabled = false;
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            yield return null;
            host.Screen.enabled = false;
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            host.Screen.enabled = true;
            yield return null;
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            camera.cullingMask &= ~(1 << 4);
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            prop.gameObject.SetActive(false);
            yield return null;
            yield return null;
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
            Assert.That(camera.cullingMask, Is.EqualTo(originalMask & ~(1 << 4)), "Cleanup preserves other systems' mask changes.");
            Assert.That(camera.GetComponent<PropNarrationComposite>().enabled, Is.False);
        }

        private static MiniGameBootstrap Stage()
        {
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            host.Player.enabled = false;
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Poster beach approach").transform.position;
            controller.enabled = true;
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            host.Player.FacePoint(prop.Surface.bounds.center);
            Physics.SyncTransforms(); host.Player.RefreshInteractionFocus();
            return host;
        }

        [UnityTest]
        public IEnumerator LongParagraphRemainsCompleteAcrossReadablePages()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Stage();
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(item => item.Id == "poster-beach");
            string text = string.Concat(Enumerable.Repeat("海风吹过门外，记忆仍在远方。", 20));
            prop.Configure(prop.Id, text, prop.Surface);
            Assert.That(host.Player.InteractSecondaryFocused(), Is.True);
            Assert.That(host.Screen.PropNarration.PageCount, Is.GreaterThan(1));
            var read = new List<string>();
            for (int index = 0; index < host.Screen.PropNarration.PageCount; index++)
            {
                yield return new WaitForSecondsRealtime(.22f);
                read.Add(host.Screen.CurrentRoomCaption);
                Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            }
            Assert.That(string.Concat(read), Is.EqualTo(text), "Pagination must never silently discard a sentence.");
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(host.Screen.PropNarration.IsOpen, Is.False);
        }
    }
}
