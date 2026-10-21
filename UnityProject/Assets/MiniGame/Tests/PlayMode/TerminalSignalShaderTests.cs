using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalSignalShaderTests
    {
        [UnityTest]
        public IEnumerator ZeroIntensityMatchesDefaultUiAndFixedClockIsRepeatable()
        {
            using (var fixture = new SignalFixture(false))
            {
                yield return null;
                fixture.Image.color = new Color(.75f, .9f, .65f, .8f);
                fixture.Image.material = null;
                var baseline = fixture.Capture();
                Assert.That(VisiblePixels(baseline), Is.GreaterThan(SignalFixture.Size * SignalFixture.Size / 2),
                    "The camera must render the fixture UI before its pixels can validate the shader.");

                fixture.Image.material = fixture.Material;
                fixture.Material.SetFloat("_Intensity", 0);
                Assert.That(MaxChannelDifference(baseline, fixture.Capture()), Is.LessThanOrEqualTo(1),
                    "Zero intensity must reproduce UI/Default including tint and alpha, within one RGBA8 unit.");

                fixture.Material.SetFloat("_Intensity", 1);
                fixture.Material.SetFloat("_Clock", .47f);
                fixture.Material.SetFloat("_Tear", .08f);
                fixture.Material.SetFloat("_RgbSplit", .025f);
                fixture.Material.SetFloat("_Noise", .2f);
                var first = fixture.Capture();
                yield return null;
                var second = fixture.Capture();
                Assert.That(MaxChannelDifference(first, second), Is.LessThanOrEqualTo(1),
                    "An unchanged _Clock must freeze all shader animation across different frames.");
                Assert.That(MeanRgbDifference(baseline, first), Is.GreaterThan(.005f),
                    "Repeatability must be tested while the GPU produces an effect, not an unchanged image.");
            }
        }

        [UnityTest]
        public IEnumerator MonitorScanlineStrengthChangesRenderedRows()
        {
            using (var fixture = new SignalFixture(false))
            {
                yield return null;
                fixture.Material.SetFloat("_ScanlineDensity", 32);
                fixture.Material.SetFloat("_ScanlineSpeed", 0);
                fixture.Material.SetFloat("_ScanlineStrength", 0);
                var withoutLines = fixture.Capture();
                fixture.Material.SetFloat("_ScanlineStrength", .7f);
                var withLines = fixture.Capture();
                Assert.That(MeanRgbDifference(withoutLines, withLines), Is.GreaterThan(.03f),
                    "Changing scanline strength must alter the pixels rendered by the UI camera.");

                float darkestRowLoss = 0;
                float lightestRowLoss = 1;
                for (int y = 8; y < SignalFixture.Size - 8; y++)
                {
                    float rowLoss = 0;
                    for (int x = 8; x < SignalFixture.Size - 8; x++)
                    {
                        int index = y * SignalFixture.Size + x;
                        rowLoss += (withoutLines[index].g - withLines[index].g) / 255f;
                    }
                    rowLoss /= SignalFixture.Size - 16;
                    darkestRowLoss = Mathf.Max(darkestRowLoss, rowLoss);
                    lightestRowLoss = Mathf.Min(lightestRowLoss, rowLoss);
                }
                Assert.That(darkestRowLoss - lightestRowLoss, Is.GreaterThan(.05f),
                    "The scanline control must create alternating horizontal rows, not only uniform darkening.");
                Assert.That(MaxAlphaDifference(withoutLines, withLines), Is.LessThanOrEqualTo(1),
                    "Scanlines must change monitor color without cutting holes in its alpha.");
            }
        }

        [UnityTest]
        public IEnumerator LogoBurstPreservesTransparencyAndRendersColoredFringes()
        {
            using (var fixture = new SignalFixture(true))
            {
                yield return null;
                fixture.Image.material = null;
                var ordinaryLogo = fixture.Capture();
                fixture.Image.material = fixture.Material;
                fixture.Material.SetFloat("_RgbSplit", .03f);
                fixture.Material.SetFloat("_Clock", 0);
                var restingLogo = fixture.Capture();
                Assert.That(MaxChannelDifference(ordinaryLogo, restingLogo), Is.LessThanOrEqualTo(1),
                    "Between bursts, the logo must remain the original readable image.");

                // Frequency=1, duration=.2: this is halfway through the first burst.
                fixture.Material.SetFloat("_Clock", .37f + .2f * .5f);
                var burst = fixture.Capture();
                Assert.That(MeanRgbDifference(restingLogo, burst), Is.GreaterThan(.001f),
                    "The explicit burst time must visibly change the rendered logo.");

                int coloredFringes = 0;
                int extendedCoverage = 0;
                for (int y = 0; y < SignalFixture.Size; y++)
                {
                    for (int x = 0; x < SignalFixture.Size; x++)
                    {
                        int index = y * SignalFixture.Size + x;
                        var pixel = burst[index];
                        if (x < 16 || x >= SignalFixture.Size - 16 || y < 16 || y >= SignalFixture.Size - 16)
                        {
                            Assert.That(pixel.a, Is.LessThanOrEqualTo(1), "The logo's empty border must remain transparent.");
                            Assert.That(Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b)), Is.LessThanOrEqualTo(1),
                                "Transparent background pixels must not become a solid or noisy rectangle.");
                        }
                        int chroma = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b))
                            - Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
                        if (pixel.a > 8 && chroma > 20) coloredFringes++;
                        if (restingLogo[index].a <= 1 && pixel.a > 8) extendedCoverage++;
                    }
                }
                Assert.That(coloredFringes, Is.GreaterThan(8),
                    "RGB-separated edges need visible channel color, not black alpha silhouettes.");
                Assert.That(extendedCoverage, Is.GreaterThan(8),
                    "The shifted channels must carry their own alpha beyond the original glyph coverage.");
            }
        }

        private static int VisiblePixels(Color32[] pixels)
        {
            int count = 0;
            foreach (var pixel in pixels) if (pixel.a > 8) count++;
            return count;
        }

        private static int MaxChannelDifference(Color32[] first, Color32[] second)
        {
            int maximum = 0;
            for (int i = 0; i < first.Length; i++)
            {
                maximum = Mathf.Max(maximum, Mathf.Abs(first[i].r - second[i].r));
                maximum = Mathf.Max(maximum, Mathf.Abs(first[i].g - second[i].g));
                maximum = Mathf.Max(maximum, Mathf.Abs(first[i].b - second[i].b));
                maximum = Mathf.Max(maximum, Mathf.Abs(first[i].a - second[i].a));
            }
            return maximum;
        }

        private static int MaxAlphaDifference(Color32[] first, Color32[] second)
        {
            int maximum = 0;
            for (int i = 0; i < first.Length; i++)
                maximum = Mathf.Max(maximum, Mathf.Abs(first[i].a - second[i].a));
            return maximum;
        }

        private static float MeanRgbDifference(Color32[] first, Color32[] second)
        {
            double sum = 0;
            for (int i = 0; i < first.Length; i++)
                sum += Mathf.Abs(first[i].r - second[i].r) + Mathf.Abs(first[i].g - second[i].g)
                    + Mathf.Abs(first[i].b - second[i].b);
            return (float)(sum / (first.Length * 3 * 255));
        }

        private sealed class SignalFixture : IDisposable
        {
            public const int Size = 128;
            public RawImage Image { get; }
            public Material Material { get; }
            private readonly GameObject root;
            private readonly Camera camera;
            private readonly RenderTexture target;
            private readonly Texture2D source;
            private readonly Texture2D readback;

            public SignalFixture(bool logo)
            {
                var shader = Resources.Load<Shader>("TerminalSignalDistortion");
                Assert.That(shader, Is.Not.Null, "The shader must be included as a runtime resource.");
                Assert.That(shader.isSupported, Is.True, "This fixture requires an active GPU that supports the UI shader.");
                root = new GameObject("Terminal signal shader GPU fixture");
                root.transform.position = new Vector3(10000, 10000, 0);
                target = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    antiAliasing = 1
                };
                target.Create();
                source = CreateSource(logo);
                readback = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);
                Material = new Material(shader);
                Material.SetFloat("_Intensity", 1);
                Material.SetFloat("_LogoMode", logo ? 1 : 0);
                Material.SetFloat("_Clock", 0);
                Material.SetFloat("_Tear", 0);
                Material.SetFloat("_RgbSplit", 0);
                Material.SetFloat("_Noise", 0);
                Material.SetFloat("_ScanlineStrength", 0);
                Material.SetFloat("_RollStrength", 0);
                Material.SetFloat("_BurstFrequency", 1);
                Material.SetFloat("_BurstDuration", .2f);

                var cameraObject = new GameObject("Signal fixture camera", typeof(Camera));
                cameraObject.transform.SetParent(root.transform, false);
                cameraObject.transform.localPosition = new Vector3(0, 0, -10);
                camera = cameraObject.GetComponent<Camera>();
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.orthographic = true;
                camera.orthographicSize = Size * .5f;
                camera.aspect = 1;
                camera.nearClipPlane = .1f;
                camera.farClipPlane = 20;
                camera.cullingMask = 1 << 31;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;

                var canvasObject = new GameObject("Signal fixture canvas", typeof(RectTransform), typeof(Canvas));
                canvasObject.layer = 31;
                canvasObject.transform.SetParent(root.transform, false);
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = camera;
                canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(Size, Size);
                var imageObject = new GameObject("Signal fixture image", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                imageObject.layer = 31;
                imageObject.transform.SetParent(canvasObject.transform, false);
                Image = imageObject.GetComponent<RawImage>();
                Image.rectTransform.sizeDelta = new Vector2(Size, Size);
                Image.texture = source;
                Image.material = Material;
                Image.raycastTarget = false;
            }

            public Color32[] Capture()
            {
                var previousActive = RenderTexture.active;
                try
                {
                    Image.SetMaterialDirty();
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                    readback.Apply();
                    return readback.GetPixels32();
                }
                finally
                {
                    RenderTexture.active = previousActive;
                }
            }

            public void Dispose()
            {
                camera.targetTexture = null;
                Object.Destroy(root);
                Object.Destroy(Material);
                Object.Destroy(source);
                Object.Destroy(readback);
                target.Release();
                Object.Destroy(target);
            }

            private static Texture2D CreateSource(bool logo)
            {
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                var pixels = new Color[Size * Size];
                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        if (logo)
                        {
                            // A white N-shaped glyph with a one-pixel translucent edge.
                            float left = Mathf.Abs(x - Size * .32f);
                            float right = Mathf.Abs(x - Size * .68f);
                            float diagonal = Mathf.Abs(x - (Size * .32f + (y - Size * .25f) * .72f));
                            float stroke = Mathf.Clamp01(4.5f - Mathf.Min(left, Mathf.Min(right, diagonal)));
                            float vertical = Mathf.Clamp01(Mathf.Min(y - Size * .25f, Size * .75f - y));
                            pixels[y * Size + x] = new Color(1, 1, 1, stroke * vertical);
                        }
                        else
                        {
                            bool checker = ((x / 8 + y / 8) & 1) == 0;
                            pixels[y * Size + x] = checker ? new Color(.8f, .9f, .7f, 1) : new Color(.25f, .6f, .8f, 1);
                        }
                    }
                }
                texture.SetPixels(pixels);
                texture.Apply();
                return texture;
            }
        }
    }
}
