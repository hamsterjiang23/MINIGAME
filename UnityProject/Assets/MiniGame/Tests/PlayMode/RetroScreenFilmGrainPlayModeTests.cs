using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RetroScreenFilmGrainPlayModeTests
    {
        private GameObject cameraObject;
        private RetroScreenProfile profile;
        private RetroScreenEffect effect;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("Film grain regression", typeof(Camera));
            cameraObject.GetComponent<Camera>().enabled = false;
            effect = cameraObject.AddComponent<RetroScreenEffect>();
            profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            profile.ResetToNaturalDefaults();
            var shader = Resources.Load<Shader>("RetroScreen");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            effect.Configure(profile, shader);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void GrainIsOptionalBoundedAndResetsWithoutEnablingQuantization()
        {
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = 5;
            Assert.That(profile.FilmGrainStrength, Is.EqualTo(.15f));
            profile.FilmGrainStrength = -1;
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = float.NaN;
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = float.PositiveInfinity;
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = .05f; profile.ResetToLightDefaults();
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = .05f; profile.ResetToReferenceStyle();
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            profile.FilmGrainStrength = .05f; profile.ResetToNaturalDefaults();
            Assert.That(profile.FilmGrainStrength, Is.Zero);
            Assert.That(profile.FullscreenQuantization, Is.False);
            Assert.That(Resources.Load<RetroScreenProfile>("RoomLightingReferenceRetro").FilmGrainStrength,
                Is.EqualTo(.05f).Within(.0001f));
        }

        [Test]
        public void GrainAddsMonochromeDetailWithoutChangingAlphaOrMeanBrightness()
        {
            var original = RenderFlat();
            profile.FilmGrainStrength = .05f;
            var grain = RenderFlat();
            double mean = 0;
            float maximum = 0;
            for (int i = 0; i < grain.Length; i++)
            {
                float delta = grain[i].r - original[i].r;
                mean += delta;
                maximum = Mathf.Max(maximum, Mathf.Abs(delta));
                Assert.That(grain[i].r, Is.EqualTo(grain[i].g).Within(.00001f));
                Assert.That(grain[i].g, Is.EqualTo(grain[i].b).Within(.00001f));
                Assert.That(grain[i].a, Is.EqualTo(original[i].a).Within(.00001f));
            }
            Assert.That(Difference(original, grain), Is.GreaterThan(.002f), "Grain must be visible with quantization disabled.");
            Assert.That(maximum, Is.LessThanOrEqualTo(.05f), "Grain must stay restrained around the source image.");
            Assert.That(Mathf.Abs((float)(mean / grain.Length)), Is.LessThan(.003f), "Signed noise must not systematically brighten or darken the image.");
            profile.Strength = .5f;
            var half = RenderFlat();
            Assert.That(Difference(original, half), Is.EqualTo(Difference(original, grain) * .5f).Within(.00001f));
        }

        [UnityTest]
        public IEnumerator GrainChangesBetweenFramesAndBypassesPreserveTheSource()
        {
            var original = RenderFlat();
            profile.FilmGrainStrength = .05f;
            var first = RenderFlat();
            Assert.That(Difference(first, RenderFlat()), Is.LessThan(.00001f), "Multiple camera renders in one frame share a stable grain field.");
            yield return null;
            Assert.That(Difference(first, RenderFlat()), Is.GreaterThan(.002f), "Film grain must vary between frames rather than repeat the static Bayer pattern.");
            effect.PreviewOriginal = true;
            Assert.That(Difference(original, RenderFlat()), Is.LessThan(.00001f));
            effect.PreviewOriginal = false; effect.PresentationBypass = true;
            Assert.That(Difference(original, RenderFlat()), Is.LessThan(.00001f));
            effect.PresentationBypass = false; profile.Strength = 0;
            Assert.That(Difference(original, RenderFlat()), Is.LessThan(.00001f));
        }

        private Color[] RenderFlat()
        {
            const int size = 64;
            var input = new Texture2D(size, size, TextureFormat.RGBAFloat, false, true);
            var output = new Texture2D(size, size, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var destination = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                var colors = new Color[size * size];
                for (int i = 0; i < colors.Length; i++) colors[i] = new Color(.25f, .25f, .25f, .37f);
                input.SetPixels(colors); input.Apply(); Graphics.Blit(input, source);
                effect.Render(source, destination); RenderTexture.active = destination;
                output.ReadPixels(new Rect(0, 0, size, size), 0, 0); output.Apply();
                return output.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(destination);
                Object.DestroyImmediate(input); Object.DestroyImmediate(output);
            }
        }

        private static float Difference(Color[] first, Color[] second)
        {
            double sum = 0;
            for (int i = 0; i < first.Length; i++)
                sum += Mathf.Abs(first[i].r - second[i].r) + Mathf.Abs(first[i].g - second[i].g) + Mathf.Abs(first[i].b - second[i].b);
            return (float)(sum / (first.Length * 3));
        }
    }
}
