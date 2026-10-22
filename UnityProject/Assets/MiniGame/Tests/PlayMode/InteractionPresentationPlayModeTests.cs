using System.Collections;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class InteractionPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator SupervisorCardShowsOnlyLargerBodyAndStillAdvances()
        {
            var root = new GameObject("Presentation check");
            try
            {
                var screen = root.AddComponent<MiniGameScreen>();
                var rules = PrototypeRules.Create();
                var runtime = new GameRuntime(rules, new PrototypeContent(rules));
                screen.Bind(runtime);
                Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
                yield return null;
                Canvas.ForceUpdateCanvases();
                var body = screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "DialogueBody");
                var speaker = screen.GetComponentsInChildren<Text>(true).Single(text => text.name == "Speaker");
                var panel = body.transform.parent.GetComponent<RectTransform>();
                var next = panel.GetComponentsInChildren<Button>(true).Single(button => button.name == "Continue");
                Assert.That(speaker.gameObject.activeSelf, Is.False);
                Assert.That(next.GetComponentInChildren<Text>(true).color.a, Is.Zero);
                Assert.That(body.fontSize, Is.GreaterThanOrEqualTo(42));
                Assert.That(panel.anchorMax.y - panel.anchorMin.y, Is.LessThan(.2f));
                Assert.That(body.text, Is.Not.Empty);
                var token = runtime.GetView().Cue.Token;
                next.onClick.Invoke();
                Assert.That(runtime.GetView().Cue?.Token, Is.Not.EqualTo(token));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllFourCursorsRenderOnlyTheirYellowGlyphAndHideWhilePaused()
        {
            var root = new GameObject("Cursor presentation check");
            var cameraRoot = new GameObject("Cursor capture", typeof(Camera));
            var target = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            try
            {
                var screen = root.AddComponent<MiniGameScreen>();
                var rules = PrototypeRules.Create();
                var runtime = new GameRuntime(rules, new PrototypeContent(rules));
                screen.Bind(runtime);
                var camera = cameraRoot.GetComponent<Camera>();
                camera.enabled = false; camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear;
                camera.allowHDR = camera.allowMSAA = false;
                camera.transform.position = new Vector3(30000, 30000, 30000);
                camera.nearClipPlane = .1f; camera.farClipPlane = 10;
                target.Create(); camera.targetTexture = target;
                screen.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
                screen.Canvas.worldCamera = camera; screen.Canvas.planeDistance = 1;
                screen.Canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                foreach (var kind in new[] { InteractionCursorKind.Read, InteractionCursorKind.Door,
                    InteractionCursorKind.Sleep, InteractionCursorKind.Work })
                {
                    screen.SetInteractionCursor(kind, true);
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    Assert.That(screen.InteractionCursorVisible, Is.True);
                    camera.Render(); RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0,0,256,256),0,0); pixels.Apply();
                    var colors = pixels.GetPixels32();
                    Assert.That(colors.Count(color => color.a > 200), Is.GreaterThan(40), kind + " must render its actual artwork.");
                    foreach (var color in colors.Where(color => color.a == 255))
                    {
                        Assert.That(color.r, Is.EqualTo(239).Within(2), kind + " must use reticle yellow.");
                        Assert.That(color.g, Is.EqualTo(184).Within(2));
                        Assert.That(color.b, Is.EqualTo(63).Within(2));
                    }
                    Assert.That(pixels.GetPixel(100,100).a, Is.LessThan(.01f), "No card/background may surround the cursor.");
                    string capture = System.Environment.GetEnvironmentVariable("MINIGAME_INTERACTION_CAPTURE_DIR");
                    if (!string.IsNullOrEmpty(capture))
                    {
                        Directory.CreateDirectory(capture);
                        File.WriteAllBytes(Path.Combine(capture,"cursor-"+kind.ToString().ToLowerInvariant()+".png"),pixels.EncodeToPNG());
                    }
                }
                Assert.That(screen.Canvas.transform.Find("InteractionFrame"), Is.Null, "The E/action strip must be removed.");
                runtime.SetPaused(true); yield return null;
                Assert.That(screen.InteractionCursorVisible, Is.False);
                runtime.SetPaused(false);
                screen.SetInteractionCursor(InteractionCursorKind.Sleep, false); yield return null;
                Assert.That(screen.InteractionCursor, Is.EqualTo(InteractionCursorKind.None));
                Assert.That(screen.InteractionCursorVisible, Is.False);
            }
            finally
            {
                cameraRoot.GetComponent<Camera>().targetTexture = null;
                RenderTexture.active = previous; target.Release();
                Object.Destroy(pixels); Object.Destroy(target); Object.Destroy(cameraRoot); Object.Destroy(root);
            }
            yield return null;
        }
    }
}
