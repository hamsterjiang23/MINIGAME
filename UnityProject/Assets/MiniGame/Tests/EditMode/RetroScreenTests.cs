using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using MiniGame.UnityHost.Editor;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RetroScreenTests
    {
        private GameObject cameraObject;
        private RetroScreenEffect effect;
        private RetroScreenProfile profile;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("Retro render test", typeof(Camera));
            cameraObject.GetComponent<Camera>().enabled = false;
            effect = cameraObject.AddComponent<RetroScreenEffect>();
            profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(RetroScreenSetup.ShaderPath);
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
            effect.Configure(profile, shader);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            if (profile != null) Object.DestroyImmediate(profile);
        }

        [Test]
        public void BlackAndHdrRemainFiniteAndColorRatiosSurviveQuantization()
        {
            profile.Strength = 1; profile.PixelSize = 1; profile.GammaBefore = 1; profile.GammaAfter = 1;
            var colors = new[] { Color.black, new Color(.37f, .185f, .0925f, .3f),
                new Color(16, 8, 4, .6f), Color.white };
            var result = Render(4, 1, colors);
            Assert.That(result[0].maxColorComponent, Is.Zero.Within(.00001));
            Assert.That(result[1].r, Is.EqualTo(9f / 24).Within(.0001));
            Assert.That(result[1].g / result[1].r, Is.EqualTo(.5f).Within(.0001));
            Assert.That(result[2].r, Is.EqualTo(16).Within(.001));
            Assert.That(result[2].g, Is.EqualTo(8).Within(.001));
            Assert.That(result[1].a, Is.EqualTo(.3f).Within(.0001));
            profile.GammaBefore = 2; profile.GammaAfter = 3;
            foreach (var color in Render(4, 1, colors))
                for (int channel = 0; channel < 4; channel++)
                    Assert.That(float.IsNaN(color[channel]) || float.IsInfinity(color[channel]), Is.False);
        }

        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        [TestCase(1279, 719)]
        public void PixelBlocksKeepOrientationAndPartialEdges(int width, int height)
        {
            profile.Strength = 1; profile.PixelSize = 7;
            profile.GammaBefore = profile.GammaAfter = 1; profile.Levels = 64;
            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    colors[y * width + x] = new Color((float)x / width, (float)y / height, .25f, 1);
            var result = Render(width, height, colors);
            Assert.That(result[0].g, Is.LessThan(.02f), "Bottom must not become top.");
            Assert.That(result[(height - 1) * width].g, Is.GreaterThan(.95f));
            Assert.That(result[width - 1].r, Is.GreaterThan(.95f), "Right edge must not wrap.");
            for (int y = 0; y < height; y += 43)
                for (int x = 0; x < width; x += 37)
                {
                    int startX = x / 7 * 7, startY = y / 7 * 7;
                    var first = result[startY * width + startX];
                    var last = result[Math.Min(startY + 6, height - 1) * width + Math.Min(startX + 6, width - 1)];
                    Assert.That(Vector4.Distance(first, last), Is.LessThan(.0001f));
                }
        }

        [Test]
        public void BypassAndOriginalPreviewPreserveSourceAndSettings()
        {
            var colors = new[] { new Color(.12f, .4f, .8f, .3f), new Color(4, 2, 1, .7f) };
            string before = JsonUtility.ToJson(profile);
            effect.PreviewOriginal = true;
            AssertColors(Render(2, 1, colors), colors);
            Assert.That(JsonUtility.ToJson(profile), Is.EqualTo(before));
            effect.PreviewOriginal = false; profile.Strength = 0;
            AssertColors(Render(2, 1, colors), colors);
            profile.ResetToLightDefaults(); profile.EffectEnabled = false;
            AssertColors(Render(2, 1, colors), colors);
            profile.EffectEnabled = true; effect.Configure(profile, null);
            AssertColors(Render(2, 1, colors), colors);
        }

        [Test]
        public void InvalidValuesAreSanitizedAndProfileSupportsUndoSaveReload()
        {
            profile.Strength = float.NaN; profile.GammaBefore = float.PositiveInfinity;
            profile.GammaAfter = -1; profile.PixelSize = 0; profile.Levels = 999;
            Assert.That(profile.Strength, Is.EqualTo(.65f));
            Assert.That(profile.GammaBefore, Is.EqualTo(.8f));
            Assert.That(profile.GammaAfter, Is.EqualTo(.25f));
            Assert.That(profile.PixelSize, Is.EqualTo(1)); Assert.That(profile.Levels, Is.EqualTo(64));
            string path = "Assets/RetroProfileTest-" + Guid.NewGuid().ToString("N") + ".asset";
            try
            {
                AssetDatabase.CreateAsset(profile, path);
                Undo.RecordObject(profile, "Test profile adjustment");
                profile.PixelSize = 5;
                Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
                Assert.That(profile.PixelSize, Is.EqualTo(1));
                Undo.PerformRedo(); Assert.That(profile.PixelSize, Is.EqualTo(5));
                EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile);
                Resources.UnloadAsset(profile);
                profile = AssetDatabase.LoadAssetAtPath<RetroScreenProfile>(path);
                Assert.That(profile.PixelSize, Is.EqualTo(5));
            }
            finally { AssetDatabase.DeleteAsset(path); profile = null; }
        }

        [Test]
        public void ReplacementCameraAndSavedSceneKeepCustomProfile()
        {
            string root = "Assets/RetroSceneTest-" + Guid.NewGuid().ToString("N");
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = default(UnityEngine.SceneManagement.Scene);
            bool untitledHarness = string.IsNullOrEmpty(previous.path);
            try
            {
                profile.PixelSize = 5;
                AssetDatabase.CreateAsset(profile, root + ".asset");
                // Test Runner supplies an untitled scene; Unity refuses additive loads until it is saved.
                if (untitledHarness) UnityEditor.SceneManagement.EditorSceneManager.SaveScene(previous, root + "-harness.unity");
                scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, root + ".unity");
                Assert.That(RetroScreenSetup.FindSavedProfile(root + ".unity"), Is.SameAs(profile));
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
                var retained = RetroScreenSetup.FindSavedProfile(root + ".unity");
                Assert.That(retained, Is.SameAs(profile));
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Is.EqualTo(previous));
                cameraObject = new GameObject("Replacement camera", typeof(Camera));
                RetroScreenSetup.Attach(cameraObject.GetComponent<Camera>(), retained);
                Assert.That(cameraObject.GetComponent<RetroScreenEffect>().Profile.PixelSize, Is.EqualTo(5));
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                AssetDatabase.DeleteAsset(root + ".unity");
                AssetDatabase.DeleteAsset(root + ".asset");
                profile = null;
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
                if (untitledHarness) AssetDatabase.DeleteAsset(root + "-harness.unity");
            }
        }

        private Color[] Render(int width, int height, Color[] colors)
        {
            var input = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var destination = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var output = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
            var previous = RenderTexture.active;
            try
            {
                input.SetPixels(colors); input.Apply();
                Graphics.Blit(input, source);
                effect.Render(source, destination);
                RenderTexture.active = destination;
                output.ReadPixels(new Rect(0, 0, width, height), 0, 0); output.Apply();
                return output.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(input); Object.DestroyImmediate(output);
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(destination);
            }
        }

        private static void AssertColors(Color[] actual, Color[] expected)
        {
            for (int i = 0; i < expected.Length; i++)
                Assert.That(Vector4.Distance(actual[i], expected[i]), Is.LessThan(.0001f));
        }
    }
}
