using System;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RetroScreenGradingTests
    {
        private GameObject cameraObject;
        private RetroScreenProfile profile;
        private RetroScreenEffect effect;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("Retro grading test", typeof(Camera));
            cameraObject.GetComponent<Camera>().enabled = false;
            effect = cameraObject.AddComponent<RetroScreenEffect>();
            profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            profile.ResetToNaturalDefaults();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(RetroScreenSetup.ShaderPath);
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported && !ShaderUtil.ShaderHasError(shader), Is.True);
            effect.Configure(profile, shader);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void NaturalDefaultsAreExactBypassIncludingNegativeAndHdrChannels()
        {
            var source = new[] { new Color(-.1f, .23f, 4, .2f), new Color(.1234f, .4567f, .8912f, .8f), Color.black };
            AssertColors(Render(3, 1, source), source);
            profile.FullscreenQuantization = true; profile.QuantizationStrength = 0;
            profile.GammaBefore = .25f; profile.GammaAfter = 3;
            AssertColors(Render(3, 1, source), source);
        }

        [Test]
        public void ToneTintSlotsAffectTheirOwnLuminanceZones()
        {
            var source = new[] { new Color(.03f, .03f, .03f), new Color(.5f, .5f, .5f), new Color(.98f, .98f, .98f) };
            profile.ColorGradingEnabled = true;
            profile.ShadowTint = new Color(1, .5f, .5f);
            profile.MidtoneTint = new Color(.5f, 1, .5f);
            profile.HighlightTint = new Color(.5f, .5f, 1);
            var actual = Render(3, 1, source);
            Assert.That(actual[0].r, Is.GreaterThan(actual[0].g * 1.5f));
            Assert.That(actual[1].g, Is.GreaterThan(actual[1].r * 1.5f));
            Assert.That(actual[2].b, Is.GreaterThan(actual[2].r * 1.5f));
        }

        [Test]
        public void ExposureAndToneExposuresUseEvAndNeutralCurveStaysFinite()
        {
            profile.ColorGradingEnabled = true; profile.PostExposure = 1;
            var source = new[] { new Color(.1f, .2f, .3f, .4f) };
            var actual = Render(1, 1, source)[0];
            AssertLinearExposure(actual, source[0], 2);
            profile.PostExposure = 0;
            profile.ShadowExposure = profile.MidtoneExposure = profile.HighlightExposure = 1;
            AssertLinearExposure(Render(1, 1, source)[0], source[0], 2);
            profile.NeutralTonemapping = true;
            var hdr = Render(2, 1, new[] { new Color(16, 8, 4), new Color(10000, 1000, 100) });
            foreach (var color in hdr)
                for (int channel = 0; channel < 4; channel++)
                    Assert.That(float.IsNaN(color[channel]) || float.IsInfinity(color[channel]), Is.False);
            Assert.That(hdr[0].r, Is.LessThan(2));
        }

        [Test]
        public void DisablingColorGradingAlsoBypassesItsNeutralTonemapping()
        {
            profile.ColorGradingEnabled = false; profile.NeutralTonemapping = true;
            var source = new[] { new Color(.12f, .24f, .48f, .3f), new Color(16, 8, 4, .7f) };
            AssertColors(Render(2, 1, source), source);
        }

        [Test]
        public void PerceptualQuantizationUsesLuminanceAndExactlyNLevels()
        {
            profile.FullscreenQuantization = true; profile.PerceptualQuantization = true;
            profile.Levels = 4;
            var source = new[] { new Color(.3f, .3f, .3f), new Color(.6f, .3f, .15f), Color.black };
            var actual = Render(3, 1, source);
            Assert.That(actual[0].r, Is.EqualTo(1f / 3).Within(.0001f));
            Assert.That(Luma(actual[1]), Is.EqualTo(1f / 3).Within(.0001f));
            Assert.That(actual[1].g / actual[1].r, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(actual[2].maxColorComponent, Is.Zero.Within(.0001f));
        }

        [Test]
        public void BayerDitherIsRepeatableAndUsesTheSamePixelBlocks()
        {
            profile.FullscreenQuantization = true; profile.PerceptualQuantization = true;
            profile.Levels = 4; profile.DitherStrength = 1; profile.PixelSize = 2;
            var source = new Color[16 * 16];
            for (int i = 0; i < source.Length; i++) source[i] = new Color(.5f, .5f, .5f);
            var first = Render(16, 16, source); var second = Render(16, 16, source);
            AssertColors(first, second);
            bool lower = false, higher = false;
            for (int y = 0; y < 16; y += 2)
                for (int x = 0; x < 16; x += 2)
                {
                    float value = first[y * 16 + x].r;
                    lower |= value < .5f; higher |= value > .5f;
                    Assert.That(first[(y + 1) * 16 + x + 1].r, Is.EqualTo(value).Within(.0001f));
                    Assert.That(first[(y % 8) * 16 + x % 8].r, Is.EqualTo(value).Within(.0001f));
                }
            Assert.That(lower && higher, Is.True, "Dither must move a threshold value into both neighbouring levels.");
        }

        [TestCase(641, 361, 180)]
        [TestCase(1279, 719, 540)]
        [TestCase(127, 71, 1080)]
        public void VirtualGridPreservesAspectOrientationBordersAndActualTexelCentres(int width, int height, int virtualHeight)
        {
            profile.UseVirtualResolution = true; profile.VirtualHeight = virtualHeight;
            var source = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    source[y * width + x] = new Color((x + .5f) / width, (y + .5f) / height, .2f, 1);
            var actual = Render(width, height, source);
            int gridHeight = Math.Min(height, virtualHeight);
            int gridWidth = Math.Min(width, Mathf.RoundToInt((float)gridHeight * width / height));
            float blockWidth = (float)width / gridWidth, blockHeight = (float)height / gridHeight;
            Assert.That(Mathf.Abs(blockWidth - blockHeight), Is.LessThan(.02f));
            foreach (int y in new[] { 0, 1, height / 2, height - 2, height - 1 })
                foreach (int x in new[] { 0, 1, width / 2, width - 2, width - 1 })
                {
                    int sampleX = SampleTexel(x, blockWidth, width);
                    int sampleY = SampleTexel(y, blockHeight, height);
                    Assert.That(Vector4.Distance(actual[y * width + x], source[sampleY * width + sampleX]),
                        Is.LessThan(.0001f), "Odd edge cells must not wrap, flip, stretch or blend adjacent texels.");
                }
        }

        [Test]
        public void PresetsResetPreviouslyEditedControlsAndRangesCannotOverlap()
        {
            profile.ResetToReferenceStyle();
            Assert.That(profile.ColorGradingEnabled, Is.True);
            Assert.That(profile.FullscreenQuantization || profile.UseVirtualResolution, Is.False);
            profile.ShadowEnd = .6f; profile.HighlightStart = .4f;
            Assert.That(profile.HighlightStart, Is.GreaterThanOrEqualTo(profile.ShadowEnd + .05f));
            profile.Contrast = float.NaN; profile.PostExposure = float.PositiveInfinity;
            Assert.That(profile.Contrast, Is.Zero); Assert.That(profile.PostExposure, Is.Zero);
            profile.ResetToNaturalDefaults();
            Assert.That(profile.ColorGradingEnabled || profile.FullscreenQuantization || profile.NeutralTonemapping, Is.False);
            Assert.That(profile.ShadowTint, Is.EqualTo(Color.white)); Assert.That(profile.PostExposure, Is.Zero);
            profile.ResetToReferenceStyle(); profile.ResetToLightDefaults();
            Assert.That(profile.ColorGradingEnabled || profile.PerceptualQuantization || profile.UseVirtualResolution, Is.False);
            Assert.That(profile.FullscreenQuantization, Is.True);
            Assert.That(profile.QuantizationStrength, Is.EqualTo(1)); Assert.That(profile.DitherStrength, Is.Zero);
        }

        private static int SampleTexel(int pixel, float block, int size)
        {
            float cell = Mathf.Floor(pixel / block);
            int start = Mathf.CeilToInt(cell * block), end = Math.Min(size, Mathf.CeilToInt((cell + 1) * block));
            return (start + end - 1) / 2;
        }

        private static float Luma(Color color) => .2126f * color.r + .7152f * color.g + .0722f * color.b;

        private static void AssertLinearExposure(Color actual, Color source, float multiplier)
        {
            for (int channel = 0; channel < 3; channel++)
                Assert.That(Mathf.GammaToLinearSpace(actual[channel]),
                    Is.EqualTo(Mathf.GammaToLinearSpace(source[channel]) * multiplier).Within(.001f),
                    "One EV must double linear light energy, rather than double the display-encoded value.");
            Assert.That(actual.a, Is.EqualTo(source.a).Within(.0001f));
        }

        private Color[] Render(int width, int height, Color[] colors)
        {
            var input = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var output = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var destination = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                input.SetPixels(colors); input.Apply(); Graphics.Blit(input, source);
                effect.Render(source, destination); RenderTexture.active = destination;
                output.ReadPixels(new Rect(0, 0, width, height), 0, 0); output.Apply();
                return output.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(destination);
                Object.DestroyImmediate(input); Object.DestroyImmediate(output);
            }
        }

        private static void AssertColors(Color[] actual, Color[] expected)
        {
            for (int i = 0; i < expected.Length; i++)
                Assert.That(Vector4.Distance(actual[i], expected[i]), Is.LessThan(.0001f));
        }
    }
}
