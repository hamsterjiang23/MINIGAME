using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class WindowObservationPlayModeTests
    {
        private const string Caption = "无论搬到哪里，窗外永远是这幅景色。";
        private bool previousSkipStartup;

        [SetUp]
        public void SkipStartup()
        {
            previousSkipStartup = MainMenuController.SkipStartup;
            MainMenuController.SkipStartup = true;
            if (MainMenuController.Current != null) Object.DestroyImmediate(MainMenuController.Current.gameObject);
        }

        [TearDown]
        public void RestoreStartup()
        {
            MainMenuController.SkipStartup = previousSkipStartup;
            MiniGameSessionHost.Release();
        }

        [UnityTest]
        public IEnumerator WindowUsesSharedReadableFocusInputAndNarrationWithoutChangingTheSession()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            yield return null;
            var prop = Object.FindObjectsOfType<ReadableProp>().Single(value => value.Id == "window-observation");
            Assert.That(prop.Document, Is.Not.Null);
            foreach (int day in new[] { 0, 1, 2, 3 }) Assert.That(prop.CaptionForDay(day), Is.EqualTo(Caption));
            Assert.That(prop.InteractionLabel, Is.EqualTo("查看窗户"));
            Assert.That(prop.InteractionDistance, Is.EqualTo(3.5f));
            var player = host.Player;
            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            var camera = player.ViewCamera;
            var window = prop.Surface.bounds.center;

            void Stand(float distance)
            {
                player.transform.position += window - camera.transform.position - Vector3.right * distance;
                player.FacePoint(window);
                Physics.SyncTransforms(); player.RefreshInteractionFocus();
            }

            Stand(3.51f);
            Assert.That(player.FocusedReadable, Is.Null);
            Assert.That(player.InteractPrimaryFocused(), Is.False);
            Stand(1.8f);
            Assert.That(player.FocusedReadable, Is.SameAs(prop));
            Assert.That(prop.IsHighlighted, Is.True);
            Assert.That(host.Screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.Read));
            string checkpoint = host.Runtime.Save();
            Assert.That(player.HandleInteractionInput(true, false), Is.True);
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo(Caption));
            Assert.That(host.Screen.PropNarrationBlocksWorld, Is.True);
            Assert.That(player.InteractPrimaryFocused(), Is.False);
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(host.Screen.PropNarration.Visible, Is.True);
            Assert.That(host.Screen.PropNarration.PageCount, Is.EqualTo(1));
            Assert.That(host.Screen.AdvancePropNarration(), Is.True);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(host.Screen.PropNarrationBlocksWorld, Is.False);
            yield return null;
            Stand(1.8f);
            Assert.That(player.HandleInteractionInput(false, true), Is.True, "Left click shares the existing readable action.");
            Assert.That(host.Screen.CurrentRoomCaption, Is.EqualTo(Caption));
            Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
            host.Screen.PropNarration.CloseImmediately();
        }
    }
}
