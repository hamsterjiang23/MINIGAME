using System;
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
    public sealed class PatientMonitorCompositionPlayModeTests
    {
        [UnityTest, Timeout(30000)]
        public IEnumerator AnotherFeedCannotContributePixelsToAnExistingMonitor()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var template = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(template, Is.Not.Null);
            var portrait = catalog.Find("VV04");
            var alternate = catalog.Find("ML01");
            Assert.That(portrait, Is.Not.Null);
            Assert.That(alternate, Is.Not.Null);
            var firstProfile = FrozenProfile(template);
            var secondProfile = FrozenProfile(template);
            secondProfile.Find("former-ruler").ActorHeight = 2.3f;
            secondProfile.PortraitPlacements = new[]
            {
                new PatientMonitorProfile.PortraitPlacement
                {
                    PortraitId = alternate.name, HeightMultiplier = 1,
                    BottomCenter = new Vector2(.23f, .05f)
                }
            };
            GameObject firstOwner = null, secondOwner = null;
            try
            {
                var first = CreateFeed("First independent monitor", firstProfile, out firstOwner);
                first.Show("former-ruler", portrait, true);
                Assert.That(first.Output, Is.Not.Null);
                Assert.That(first.Output.IsCreated(), Is.True);
                var baseline = ReadPixels(first.Output);
                Capture(first.Output, "monitor-isolation-before");

                // A second active monitor deliberately puts a different person in a different place.
                // Both clocks stay at zero; any new pixels in the first output come from composition.
                var second = CreateFeed("Second independent monitor", secondProfile, out secondOwner);
                second.Show("former-ruler", alternate, true);
                Assert.That(second.Output, Is.Not.Null);
                Rerender(first, alternate, portrait);
                var simultaneous = ReadPixels(first.Output);
                Capture(first.Output, "monitor-isolation-with-second");
                Capture(second.Output, "monitor-isolation-second-output");
                int changed = baseline.Zip(simultaneous, (before, after) => before.Equals(after) ? 0 : 1).Sum();
                Assert.That(first.PresentationTime, Is.Zero);
                Assert.That(second.PresentationTime, Is.Zero);
                Assert.That(changed, Is.Zero,
                    "An unrelated active surveillance stage changed " + changed + " pixels in this monitor.");

                secondOwner.SetActive(false);
                Rerender(first, alternate, portrait);
                Assert.That(ReadPixels(first.Output), Is.EqualTo(baseline),
                    "Disabling the second monitor must not alter the first monitor either.");
            }
            finally
            {
                Object.Destroy(firstOwner); Object.Destroy(secondOwner);
                Object.Destroy(firstProfile); Object.Destroy(secondProfile);
            }
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator RealTerminalUsesOnePortraitSourceAndRestoresFallbackWhenMonitoringIsDisabled()
        {
            var rules = PrototypeRules.Create();
            var runtime = new GameRuntime(rules, new PrototypeContent(rules), true);
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var template = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(template, Is.Not.Null);
            var configuration = FrozenProfile(template);
            var owner = new GameObject("Real terminal portrait composition");
            var screen = owner.AddComponent<MiniGameScreen>();
            try
            {
                screen.Bind(runtime); screen.UseYarnPresentation();
                var feed = screen.GetComponentsInChildren<PatientSurveillanceFeed>(true).Single();
                var video = feed.GetComponent<RawImage>();
                feed.UseExternalClock = true;
                feed.Initialize(video, configuration);
                OpenDirectory(runtime);
                Assert.That(runtime.EnterSelectedPatient("former-ruler").Accepted, Is.True);
                if (runtime.GetView().Cue != null)
                    Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
                screen.SetDialogueLine("对象", "如果只能看到一张纸，再问那张容易找到的。", "monitor-composition", "VV04");
                yield return null;
                screen.TerminalDisplay.RenderFrame();
                var current = PortraitImage(screen, "CurrentPortrait");
                var previous = PortraitImage(screen, "PreviousPortrait");
                Capture(feed.Output, "monitor-vv04-native-feed");
                Capture(screen.TerminalDisplay.Texture, "monitor-vv04-terminal-source");
                CaptureComposite(screen.TerminalDisplay, "monitor-vv04-terminal");
                Assert.That(feed.CurrentSprite, Is.SameAs(catalog.Find("VV04")));
                Assert.That(video.isActiveAndEnabled, Is.True);
                Assert.That(current.enabled, Is.False,
                    "The original current portrait must not be composited underneath the live room feed.");
                Assert.That(previous.enabled, Is.False,
                    "A previous crossfade portrait must not survive underneath the live room feed.");

                configuration.Enabled = false;
                configuration.MarkChanged();
                feed.Advance(0);
                yield return new WaitForSecondsRealtime(.2f);
                screen.TerminalDisplay.RenderFrame();
                Capture(screen.TerminalDisplay.Texture, "monitor-vv04-fallback-source");
                Assert.That(video.enabled, Is.False);
                Assert.That(current.isActiveAndEnabled, Is.True);
                Assert.That(current.sprite, Is.SameAs(catalog.Find("VV04")));
                Assert.That(current.color.a, Is.GreaterThan(.99f));
                Assert.That(previous.enabled, Is.False,
                    "Fallback must settle on exactly one original portrait.");

                configuration.Enabled = true;
                configuration.MarkChanged();
                feed.Advance(0);
                yield return null;
                screen.TerminalDisplay.RenderFrame();
                Capture(feed.Output, "monitor-vv04-reconnected-feed");
                Capture(screen.TerminalDisplay.Texture, "monitor-vv04-reconnected-source");
                Assert.That(video.isActiveAndEnabled, Is.True);
                Assert.That(current.enabled, Is.False);
                Assert.That(previous.enabled, Is.False);
            }
            finally { Object.Destroy(owner); Object.Destroy(configuration); }
            yield return null; yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator EveryPatientAndGuideRendersItsActualCatalogPortraitInASingleFeed()
        {
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            var template = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(template, Is.Not.Null);
            var configuration = FrozenProfile(template);
            GameObject owner = null;
            var groups = new[]
            {
                new { PatientId = "child", PortraitIds = new[] { "ML01", "ML02", "ML03" }, SourceIds = new[] { "ML01", "ML02", "ML03" } },
                new { PatientId = "former-ruler", PortraitIds = new[] { "VV01", "VV02", "VV03", "VV04" }, SourceIds = new[] { "VV01", "VV02", "VV03", "VV04" } },
                new { PatientId = "battlefield-doctor", PortraitIds = new[] { "ED01", "ED02", "ED03", "ED04" }, SourceIds = new[] { "ED04", "ED01", "ED03", "ED02" } },
                new { PatientId = "diplomat", PortraitIds = new[] { "AD01", "AD02", "AD03", "AD04" }, SourceIds = new[] { "AD01", "AD02", "AD03", "AD04" } },
                new { PatientId = "priest", PortraitIds = new[] { "JN01", "JN02", "JN03", "JN04" }, SourceIds = new[] { "JN04", "JN01", "JN02", "JN03" } },
                new { PatientId = "tutorial", PortraitIds = new[] { "D0_01", "D0_02", "D0_03" }, SourceIds = new[] { "D0_02", "D0_03", "D0_01" } }
            };
            var targets = groups.SelectMany(group => group.PortraitIds.Select((id, index) =>
                new { group.PatientId, PortraitId = id, SourceId = group.SourceIds[index] })).ToArray();
            Assert.That(targets, Has.Length.EqualTo(22));
            try
            {
                var feed = CreateFeed("Five patients and the tutorial guide", configuration, out owner);
                Color32[] previousOutput = null;
                foreach (var target in targets)
                {
                    var portrait = catalog.Find(target.PortraitId);
                    Assert.That(portrait, Is.Not.Null, target.PortraitId);
                    Assert.That(portrait.texture.name, Is.EqualTo(target.SourceId),
                        "The monitor must use the artwork pose assigned to the workbook portrait ID.");
                    Assert.That(portrait.name, Is.EqualTo(target.SourceId));
                    int before = feed.RenderedFrames;
                    feed.Show(target.PatientId, portrait, true);
                    Assert.That(feed.PatientId, Is.EqualTo(target.PatientId));
                    Assert.That(feed.CurrentSprite, Is.SameAs(portrait));
                    Assert.That(feed.RenderedFrames, Is.GreaterThan(before));
                    Assert.That(feed.GetComponent<RawImage>().isActiveAndEnabled, Is.True);
                    var currentOutput = ReadPixels(feed.Output);
                    Capture(feed.Output, "monitor-patient-" + target.PatientId + "-" + target.PortraitId);
                    Assert.That(currentOutput.Count(pixel => pixel.r > 20 || pixel.g > 20 || pixel.b > 20),
                        Is.GreaterThan(1000), target.PortraitId + " must render a nonempty GPU frame.");
                    long red = currentOutput.Sum(pixel => (long)pixel.r);
                    long green = currentOutput.Sum(pixel => (long)pixel.g);
                    long blue = currentOutput.Sum(pixel => (long)pixel.b);
                    Assert.That(red, Is.GreaterThan(green * 1.02), target.PortraitId + " must retain the original warm monitor filter.");
                    Assert.That(green, Is.GreaterThan(blue * 1.1), target.PortraitId + " must be amber, not a gray/color bypass.");
                    if (previousOutput != null)
                        Assert.That(previousOutput.Zip(currentOutput, (a, b) => a.Equals(b) ? 0 : 1).Sum(),
                            Is.GreaterThan(100), "Changing portrait must replace the actual output pixels.");

                    var actors = feed.ActiveRoom.transform.parent.GetComponentsInChildren<SpriteRenderer>(true)
                        .Where(renderer => renderer.sprite == portrait).ToArray();
                    Assert.That(actors, Has.Length.EqualTo(1), "The requested catalog portrait must have one renderer.");
                    var actor = actors[0];
                    var camera = feed.ViewCamera;
                    camera.Render();
                    var withActor = ReadPixels(camera.targetTexture);
                    try
                    {
                        actor.enabled = false;
                        camera.Render();
                        var withoutActor = ReadPixels(camera.targetTexture);
                        Assert.That(withActor.Zip(withoutActor, (a, b) => a.Equals(b) ? 0 : 1).Sum(),
                            Is.GreaterThan(100), target.PortraitId + " must contribute visible pixels to its camera.");
                    }
                    finally { actor.enabled = true; camera.Render(); }
                    previousOutput = currentOutput;
                    yield return null;
                }
            }
            finally { Object.Destroy(owner); Object.Destroy(configuration); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SuppliedSeatedPortraitsStayAtTheLeftBedEdgeAtMediumRoomScale()
        {
            var template = Resources.Load<PatientMonitorProfile>("MiniGame/PatientMonitorDefault");
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            Assert.That(template, Is.Not.Null);
            Assert.That(catalog, Is.Not.Null);
            var configuration = FrozenProfile(template);
            GameObject owner = null;
            try
            {
                var feed = CreateFeed("Supplied seated placement references", configuration, out owner);
                foreach (var target in new[] { new { Id = "VV04", Patient = "former-ruler" }, new { Id = "ED04", Patient = "battlefield-doctor" } })
                {
                    var sprite = catalog.Find(target.Id);
                    Assert.That(sprite, Is.Not.Null);
                    Assert.That(sprite.name, Is.EqualTo(target.Id), "Placement must never remap the workbook ID to another PNG.");
                    feed.Show(target.Patient, sprite, true);
                    var actor = feed.ActiveRoom.transform.parent.Find("Seated patient cutout").GetComponent<SpriteRenderer>();
                    var bounds = actor.bounds;
                    var bottom = feed.ViewCamera.WorldToViewportPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
                    var top = feed.ViewCamera.WorldToViewportPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
                    Assert.That(bottom.x, Is.InRange(.49f, .55f), "Feet belong to the floor left of the bed, not its far-right footboard.");
                    Assert.That(bottom.y, Is.InRange(.27f, .33f));
                    Assert.That(top.y - bottom.y, Is.InRange(.28f, .4f), "The supplied sheets use a medium room-wide seated figure, not a foreground enlargement.");
                    AssertVisibleSeatedPixels(feed, actor, target.Id);
                    Capture(feed.Output, "reference-seated-" + target.Id);
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR")))
                    {
                        try
                        {
                            foreach (int width in new[] { 1920, 2560 })
                            {
                                int height = width * 9 / 16;
                                ResizeFeedForCapture(feed, configuration, width, height);
                                Assert.That(feed.Output.width, Is.EqualTo(width));
                                Assert.That(feed.Output.height, Is.EqualTo(height));
                                AssertVisibleSeatedPixels(feed, actor, target.Id);
                                Capture(feed.Output, "reference-seated-" + target.Id + "-" + width + "x" + height);
                            }
                        }
                        finally { ResizeFeedForCapture(feed, configuration, 640, 360); }
                    }
                    yield return null;
                }
            }
            finally
            {
                configuration.Width = 640; configuration.Height = 360;
                Object.Destroy(owner); Object.Destroy(configuration);
            }
        }

        private static void AssertVisibleSeatedPixels(PatientSurveillanceFeed feed, SpriteRenderer actor, string portraitId)
        {
            var camera = feed.ViewCamera;
            camera.Render();
            var withActor = ReadPixels(camera.targetTexture);
            try
            {
                actor.enabled = false;
                camera.Render();
                var withoutActor = ReadPixels(camera.targetTexture);
                Assert.That(withActor.Zip(withoutActor, (a, b) => a.Equals(b) ? 0 : 1).Sum(),
                    Is.GreaterThan(camera.targetTexture.width * camera.targetTexture.height / 500),
                    portraitId + " must contribute visible GPU pixels, independently of its configured placement.");
            }
            finally { actor.enabled = true; camera.Render(); }
        }

        private static void ResizeFeedForCapture(PatientSurveillanceFeed feed, PatientMonitorProfile configuration, int width, int height)
        {
            configuration.Width = width; configuration.Height = height;
            // Native test captures bypass the normal 1600x800 performance cap, without upscaling a small image.
            foreach (var texture in new[] { feed.ViewCamera.targetTexture, feed.Output })
            {
                texture.Release();
                texture.width = width; texture.height = height;
                Assert.That(texture.Create(), Is.True, "The requested native capture texture must be created.");
            }
            // Use the feed's public rendering path while retaining its frozen clock and final room-wide framing.
            feed.SetCloseUp(true);
            feed.SetCloseUp(false);
        }

        private static PatientMonitorProfile FrozenProfile(PatientMonitorProfile template)
        {
            var value = Object.Instantiate(template);
            value.Enabled = true;
            value.Noise = 0;
            value.IdleMotion = 0;
            value.Width = 640; value.Height = 360;
            return value;
        }

        private static PatientSurveillanceFeed CreateFeed(string name, PatientMonitorProfile profile, out GameObject owner)
        {
            owner = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            ((RectTransform)owner.transform).sizeDelta = new Vector2(464, 261);
            var feed = owner.AddComponent<PatientSurveillanceFeed>();
            feed.UseExternalClock = true;
            feed.Initialize(owner.GetComponent<RawImage>(), profile);
            return feed;
        }

        private static void Rerender(PatientSurveillanceFeed feed, Sprite alternate, Sprite wanted)
        {
            feed.Show("former-ruler", alternate, true);
            feed.Show("former-ruler", wanted, true);
        }

        private static Image PortraitImage(MiniGameScreen screen, string name) =>
            screen.GetComponentsInChildren<Image>(true).Single(image => image.name == name);

        private static void OpenDirectory(GameRuntime runtime)
        {
            Assert.That(runtime.TryInteract(InteractionTarget.Door).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(runtime.TryInteract(InteractionTarget.Computer).Accepted, Is.True);
            Assert.That(runtime.CompleteCue(runtime.GetView().Cue.Token).Accepted, Is.True);
        }

        private static Color32[] ReadPixels(RenderTexture source)
        {
            var image = Read(source);
            try { return image.GetPixels32(); }
            finally { Object.Destroy(image); }
        }

        private static Texture2D Read(RenderTexture source)
        {
            Assert.That(source, Is.Not.Null);
            var before = RenderTexture.active;
            var image = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                image.Apply(); return image;
            }
            finally { RenderTexture.active = before; }
        }

        private static void CaptureComposite(TerminalDisplay display, string name)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR"))) return;
            var result = new RenderTexture(1600, 1200, 0, RenderTextureFormat.ARGB32);
            result.Create();
            var previous = RenderTexture.active;
            try { Graphics.Blit(display.Texture, result, display.ScreenMaterial); Capture(result, name); }
            finally { RenderTexture.active = previous; result.Release(); Object.Destroy(result); }
        }

        private static void Capture(RenderTexture source, string name)
        {
            string folder = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder);
            var image = Read(source);
            try { File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG()); }
            finally { Object.Destroy(image); }
        }
    }
}
