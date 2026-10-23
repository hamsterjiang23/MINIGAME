using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamImageSequencePlayModeTests
    {
        private const int Width = 128;
        private const int Height = 72;

        [UnityTest]
        public IEnumerator TenFramesAdvanceEveryHalfSecondWithoutBlendingAndHoldTheLastImage()
        {
            var colors = new Color32[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta,
                Color.yellow, Color.white, Color.black, new Color32(32, 64, 128, 255), new Color32(192, 96, 48, 255) };
            var frames = colors.Select(color => Solid(color)).ToArray();
            try
            {
                var settings = new DreamDaySettings { Day = 2, Frames = frames, Duration = 5,
                    CrossfadeSeconds = 2, MotionIntensity = 2, PostIntensity = 2, VignetteIntensity = 0, BlackEdgeStrength = Vector4.zero };
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    for (int index = 0; index < frames.Length; index++)
                    {
                        foreach (float time in new[] { index * .5f, index * .5f + .499f })
                        {
                            stage.Render(settings, time);
                            Assert.That(stage.CurrentFrameIndex, Is.EqualTo(index), "Frame at " + time);
                            Assert.That(stage.NextFrameIndex, Is.EqualTo(index));
                            Assert.That(stage.FrameBlend, Is.Zero);
                            Assert.That(Read(stage.Texture), Is.All.EqualTo(colors[index]), "Source pixels at " + time);
                        }
                    }
                    foreach (float time in new[] { 5f, 50f })
                    {
                        stage.Render(settings, time);
                        Assert.That(stage.CurrentFrameIndex, Is.EqualTo(9));
                        Assert.That(stage.PlaybackSeconds, Is.EqualTo(5));
                        Assert.That(Read(stage.Texture), Is.All.EqualTo(colors[9]));
                    }
                    foreach (float time in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                    {
                        stage.Render(settings, time);
                        Assert.That(stage.CurrentFrameIndex, Is.Zero);
                        Assert.That(stage.PlaybackSeconds, Is.Zero);
                        Assert.That(Read(stage.Texture), Is.All.EqualTo(colors[0]));
                    }
                }
            }
            finally { foreach (var frame in frames) Object.Destroy(frame); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ZeroVignettePreservesEverySourcePixelDespiteLegacySettingsAndPriorProceduralRendering()
        {
            var source = Pattern();
            var next = Solid(Color.blue);
            try
            {
                var expected = source.GetPixels32();
                var settings = new DreamDaySettings { Frames = new[] { source, next }, Duration = 5,
                    CrossfadeSeconds = 2, MotionIntensity = 2, PostIntensity = 2, VignetteIntensity = 0, BlackEdgeStrength = Vector4.zero };
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    for (int day = 1; day <= 3; day++)
                    {
                        settings.Day = day;
                        stage.Render(day, source, 17, 20, 2);
                        foreach (float time in new[] { 0f, .25f, 1.75f, 2.499f, .25f })
                        {
                            stage.Render(settings, time);
                            Assert.That(stage.CurrentFrameIndex, Is.Zero);
                            Assert.That(Read(stage.Texture), Is.EqualTo(expected),
                                "Day " + day + " must preserve all source pixels throughout the frame at " + time);
                        }
                    }
                }
            }
            finally { Object.Destroy(source); Object.Destroy(next); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SubtleVignetteDarkensCornersWithoutChangingTheirColorBalanceOrTheCenter()
        {
            var original = new Color32(200, 120, 60, 255);
            var source = Solid(original);
            try
            {
                var settings = new DreamDaySettings { Day = 3, Frames = new[] { source }, Duration = 5,
                    MotionIntensity = 2, PostIntensity = 2, VignetteIntensity = .12f, BlackEdgeStrength = Vector4.zero };
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    stage.Render(settings, 0);
                    var pixels = Read(stage.Texture);
                    Assert.That(pixels[Height / 2 * Width + Width / 2], Is.EqualTo(original));
                    foreach (int index in new[] { 0, Width - 1, (Height - 1) * Width, pixels.Length - 1 })
                    {
                        var corner = pixels[index];
                        float scale = corner.r / (float)original.r;
                        Assert.That(scale, Is.InRange(.875f, .89f), "A subtle vignette darkens the corner by about twelve percent.");
                        Assert.That(corner.g / (float)original.g, Is.EqualTo(scale).Within(.01f));
                        Assert.That(corner.b / (float)original.b, Is.EqualTo(scale).Within(.01f));
                        Assert.That(corner.a, Is.EqualTo(original.a));
                    }
                    stage.Render(settings, 4.999f);
                    Assert.That(Read(stage.Texture), Is.EqualTo(pixels), "The vignette must not animate or flicker.");
                    settings.VignetteIntensity = 0;
                    stage.Render(settings, 4.999f);
                    Assert.That(Read(stage.Texture), Is.All.EqualTo(original), "Disabling the vignette must remove all darkening.");
                }
            }
            finally { Object.Destroy(source); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingFramesUseTheArtworkOrBlackWithoutEnablingProceduralEffects()
        {
            var source = Pattern();
            try
            {
                var settings = new DreamDaySettings { Day = 3, Artwork = source, Duration = 5,
                    CrossfadeSeconds = 2, MotionIntensity = 2, PostIntensity = 2, VignetteIntensity = 0, BlackEdgeStrength = Vector4.zero };
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    foreach (var frames in new[] { null, System.Array.Empty<Texture2D>(), new Texture2D[] { null } })
                    {
                        settings.Frames = frames;
                        stage.Render(settings, 3);
                        Assert.That(Read(stage.Texture), Is.EqualTo(source.GetPixels32()));
                    }
                    settings.Artwork = null;
                    stage.Render(settings, 3);
                    Assert.That(Read(stage.Texture).All(pixel => pixel.r == 0 && pixel.g == 0 && pixel.b == 0), Is.True,
                        "Missing artwork must render black RGB; the projection surface does not sample source alpha.");
                }
            }
            finally { Object.Destroy(source); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaticBlackEdgesHaveDifferentSidesAndAnUnchangedCenterWithoutFlicker()
        {
            var original = new Color32(200, 120, 60, 255);
            var source = Solid(original);
            try
            {
                var settings = new DreamDaySettings { Frames = new[] { source }, Duration = 15, VignetteIntensity = 0,
                    BlackEdgeWidths = new Vector4(.05f, .18f, .12f, .08f),
                    BlackEdgeStrength = new Vector4(.6f, .95f, 1, .8f),
                    BlackEdgeSoftness = .025f, BlackEdgeIrregularity = .02f };
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    stage.Render(settings, 0);
                    var pixels = Read(stage.Texture);
                    Assert.That(pixels[Height / 2 * Width + Width / 2], Is.EqualTo(original));
                    var sides = new[] { pixels[Height / 2 * Width + 1], pixels[Height / 2 * Width + Width - 2],
                        pixels[Width / 2], pixels[(Height - 1) * Width + Width / 2] };
                    Assert.That(sides.Select(pixel => pixel.r).Distinct().Count(), Is.EqualTo(4), "Each side has its own darkening strength.");
                    Assert.That(sides.All(pixel => pixel.r < original.r / 2), Is.True);
                    Assert.That(pixels[Height / 2 * Width + (int)(Width * .9f)].r,
                        Is.LessThan(pixels[Height / 2 * Width + (int)(Width * .1f)].r), "The right border must be wider than the left.");
                    var contour = Enumerable.Range(Height / 4, Height / 2).Select(y => pixels[y * Width + (int)(Width * .82f)].r);
                    Assert.That(contour.Distinct().Count(), Is.GreaterThan(2), "The soft border must have a non-straight contour.");
                    stage.Render(settings, 7.3f);
                    Assert.That(Read(stage.Texture), Is.EqualTo(pixels), "Neither the border nor its irregular contour may flicker.");
                }
            }
            finally { Object.Destroy(source); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllThreeProductionDreamsUseTenOrderedFramesOverFifteenSeconds()
        {
            var profile = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            Assert.That(profile, Is.Not.Null);
            for (int day = 1; day <= 3; day++)
            {
                var settings = profile.GetDay(day);
                Assert.That(settings.Frames.Length, Is.EqualTo(10));
                Assert.That(settings.Duration, Is.EqualTo(15));
                Assert.That(settings.CrossfadeSeconds, Is.Zero);
                Assert.That(settings.MotionIntensity, Is.Zero);
                Assert.That(settings.PostIntensity, Is.Zero);
                Assert.That(settings.VignetteIntensity, Is.InRange(0f, .2f));
                Assert.That(settings.BlackEdgeWidths.x, Is.Not.EqualTo(settings.BlackEdgeWidths.y));
                Assert.That(settings.BlackEdgeWidths.z, Is.Not.EqualTo(settings.BlackEdgeWidths.w));
                Assert.That(settings.BlackEdgeStrength.x, Is.GreaterThan(.8f));
                Assert.That(settings.Frames.Select(frame => frame.name),
                    Is.EqualTo(Enumerable.Range(1, 10).Select(index => "Frame" + index.ToString("00"))));
                Assert.That(settings.Frames.All(frame => frame.width >= 1600 && frame.height >= 900), Is.True);
                using (var stage = new DreamAnimationStage(Width, Height))
                {
                    for (int index = 0; index < 10; index++)
                    {
                        stage.Render(settings, index * 1.5f);
                        Assert.That(stage.CurrentFrameIndex, Is.EqualTo(index));
                        var pixels = Read(stage.Texture);
                        stage.Render(settings, index * 1.5f + 1.499f);
                        Assert.That(stage.CurrentFrameIndex, Is.EqualTo(index));
                        Assert.That(stage.FrameBlend, Is.Zero);
                        Assert.That(Read(stage.Texture), Is.EqualTo(pixels), "Production frames and their border must remain unchanged until the next 1.5-second boundary.");
                    }
                }
                yield return null;
            }
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixels(Enumerable.Repeat(color, 16).ToArray()); texture.Apply(); return texture;
        }

        private static Texture2D Pattern()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[Width * Height];
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                pixels[y * Width + x] = new Color32((byte)(x * 2), (byte)(y * 3), (byte)((x + y) % 2 * 255), 255);
            texture.SetPixels32(pixels); texture.Apply(); return texture;
        }

        private static Color32[] Read(RenderTexture target)
        {
            var previous = RenderTexture.active;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                return texture.GetPixels32();
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(texture); }
        }
    }
}
