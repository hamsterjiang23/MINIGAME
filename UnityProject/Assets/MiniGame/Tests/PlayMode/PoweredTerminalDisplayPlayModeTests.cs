using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PoweredTerminalDisplayPlayModeTests
    {
        [UnityTest, Timeout(90000)]
        public IEnumerator WorldExitPauseAndRestoreKeepLiveUiUntilTheDailyShutdown()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo();
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            CompleteCue(host);
            CompleteCue(host);
            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && host.Runtime.GetView().Cue == null, 20, "initial terminal boot");
            var surface = Object.FindObjectsOfType<MeshRenderer>()
                .Single(item => item.name.StartsWith("CRT screen emission", StringComparison.Ordinal));
            var display = host.Screen.TerminalDisplay;
            Assert.That(host.Runtime.LeaveTerminal().Accepted, Is.True);
            yield return Until(() => !host.Runtime.GetView().TerminalOpen
                && host.Runtime.GetView().Input == InputContext.World, 8, "return to room");

            int firstFrame = display.RenderedFrameCount;
            yield return new WaitForSecondsRealtime(2);
            AssertPersistentUi(host, surface, firstFrame);
            AssertHiddenUiCannotBePicked(host);

            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            AssertLiveTexture(host, surface);
            Assert.That(host.Runtime.SetPaused(false).Accepted, Is.True);
            firstFrame = display.RenderedFrameCount;
            yield return new WaitForSecondsRealtime(2);
            AssertPersistentUi(host, surface, firstFrame);

            string checkpoint = host.Runtime.Save();
            host.RestoreCheckpoint(checkpoint);
            firstFrame = display.RenderedFrameCount;
            yield return new WaitForSecondsRealtime(2);
            AssertPersistentUi(host, surface, firstFrame);
            AssertHiddenUiCannotBePicked(host);

            Assert.That(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(host.Runtime.GetView().TerminalBootRequired, Is.False);
            yield return Until(() => host.Runtime.GetView().Input == InputContext.TerminalDirectory
                && host.Runtime.GetView().Cue == null, 10, "resume powered terminal");
            yield return CompleteChildEncounter(host);
            yield return Until(() => !host.Runtime.GetView().TerminalOpen
                && host.Runtime.GetView().TerminalPower == TerminalPowerState.Off, 10, "final daily shutdown");
            yield return null;
            firstFrame = display.RenderedFrameCount;
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(SourceCamera(host).gameObject.activeInHierarchy, Is.False,
                "A shut-down CRT must stop the offscreen source.");
            Assert.That(display.RenderedFrameCount, Is.EqualTo(firstFrame));
            var properties = new MaterialPropertyBlock();
            surface.GetPropertyBlock(properties, 0);
            Assert.That(properties.GetFloat("_CrtPower"), Is.LessThan(.001f));
            Assert.That(properties.GetColor("_EmissionColor").maxColorComponent, Is.LessThan(.001f));
        }

        private static IEnumerator CompleteChildEncounter(MiniGameBootstrap host)
        {
            float deadline = Time.realtimeSinceStartup + 40;
            float nextClick = 0;
            bool dispositionSubmitted = false;
            while (Time.realtimeSinceStartup < deadline)
            {
                var view = host.Runtime.GetView();
                if (dispositionSubmitted && view.Game.Phase == GamePhase.Evening) yield break;
                if (Time.realtimeSinceStartup >= nextClick)
                {
                    nextClick = Time.realtimeSinceStartup + .045f;
                    var buttons = host.Screen.GetComponentsInChildren<Button>(true)
                        .Where(button => button.IsActive() && button.IsInteractable()).ToArray();
                    Button button;
                    if (view.Input == InputContext.TerminalDirectory && view.Cue == null)
                        button = buttons.FirstOrDefault(item => item.name == "EnterReview");
                    else if (view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        button = buttons.FirstOrDefault(item => item.name == "Disposition-Treat");
                        dispositionSubmitted = button != null;
                    }
                    else
                        button = buttons.FirstOrDefault(item => item.name == "Continue")
                            ?? buttons.FirstOrDefault(item => item.name.StartsWith("Reply-", StringComparison.Ordinal));
                    if (button != null) button.onClick.Invoke();
                }
                yield return null;
            }
            Assert.Fail("The actual child encounter did not reach its automatic daily shutdown.");
        }

        private static void AssertPersistentUi(MiniGameBootstrap host, Renderer surface, int previousFrame)
        {
            Assert.That(host.Runtime.GetView().TerminalOpen, Is.False);
            Assert.That(host.Runtime.GetView().TerminalPowered, Is.True);
            Assert.That(host.Screen.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "ObjectDirectory").gameObject.activeInHierarchy, Is.True,
                "A powered computer in world mode must show its directory, not an unstarted interview.");
            Assert.That(host.Screen.TerminalDisplay.RenderedFrameCount, Is.GreaterThan(previousFrame + 2),
                "The physical screen must keep receiving frames beyond the exit transition.");
            AssertLiveTexture(host, surface);
            var texture = host.Screen.TerminalDisplay.Texture;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                pixels.Apply();
                var values = pixels.GetPixels32();
                Assert.That(values.Count(pixel => pixel.r > 80 && pixel.r > pixel.b + 20), Is.GreaterThan(1000),
                    "The real terminal texture must contain its amber interface.");
                Assert.That(values.Count(pixel => pixel.r < 40 && pixel.g < 40 && pixel.b < 40), Is.GreaterThan(1000),
                    "A flat illuminated color cannot substitute for interface content.");
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(pixels);
            }
        }

        private static void AssertLiveTexture(MiniGameBootstrap host, Renderer surface)
        {
            var display = host.Screen.TerminalDisplay;
            Assert.That(SourceCamera(host).gameObject.activeInHierarchy, Is.True);
            Assert.That(SourceCamera(host).targetTexture, Is.SameAs(display.Texture));
            var properties = new MaterialPropertyBlock();
            surface.GetPropertyBlock(properties, 0);
            Assert.That(properties.GetFloat("_LiveDisplay"), Is.GreaterThan(.99f));
            Assert.That(properties.GetTexture("_LiveDisplayTex"), Is.SameAs(display.Texture));
            Assert.That(display.OutputRect.gameObject.activeInHierarchy, Is.False,
                "The full-screen overlay must remain hidden while the room CRT is powered.");
        }

        private static void AssertHiddenUiCannotBePicked(MiniGameBootstrap host)
        {
            var button = host.Screen.GetComponentsInChildren<Button>(true).Single(item => item.name == "EnterReview");
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = host.Screen.TerminalDisplay.ScreenPointFor((RectTransform)button.transform)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Any(hit => hit.gameObject.transform.IsChildOf(SourceCamera(host).transform.parent)),
                Is.False, "World pointer input must not operate the offscreen interface.");
        }

        private static Camera SourceCamera(MiniGameBootstrap host) =>
            host.Screen.GetComponentsInChildren<Camera>(true).Single(item => item.name == "Terminal UI camera");

        private static void CompleteCue(MiniGameBootstrap host) =>
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);

        private static IEnumerator Until(Func<bool> condition, float seconds, string description)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, description);
        }
    }
}
