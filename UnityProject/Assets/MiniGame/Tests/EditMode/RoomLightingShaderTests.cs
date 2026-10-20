using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RoomLightingShaderTests
    {
        [Test]
        public void RoomShaderCompilesAndDefaultsToUnmodifiedStandardLighting()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/MiniGame/Art/MainRoom/RoomArtStandard.shader");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
            var material = new Material(shader);
            try
            {
                Assert.That(material.GetFloat("_LightingBandsEnabled"), Is.Zero);
                Assert.That(material.GetFloat("_LightingBands"), Is.EqualTo(8));
                Assert.That(material.GetFloat("_LightingDither"), Is.Zero);
                Assert.That(material.GetFloat("_AlbedoGain"), Is.EqualTo(1));
                Assert.That(material.GetFloat("_AlbedoContrast"), Is.EqualTo(1));
                Assert.That(material.HasProperty("_BumpMap"), Is.True);
                Assert.That(material.HasProperty("_MetallicGlossMap"), Is.True);
                Assert.That(material.HasProperty("_EmissionColor"), Is.True);
            }
            finally { Object.DestroyImmediate(material); }
        }

        [Test]
        public void LegacyRetroProfileKeepsQuantizationUntilExplicitlyDisabled()
        {
            var profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            try
            {
                // Existing serialized assets omit the new field and must retain legacy behaviour.
                JsonUtility.FromJsonOverwrite("{\"pixelSize\":5}", profile);
                Assert.That(profile.FullscreenQuantization, Is.True);
                profile.FullscreenQuantization = false;
                profile.ResetToLightDefaults();
                Assert.That(profile.FullscreenQuantization, Is.True);
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void TwoLightingBandsStillPreserveContinuousAlbedoDetail()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Lighting bands camera", typeof(Camera));
            var lightObject = new GameObject("Lighting bands light", typeof(Light));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/MiniGame/Art/MainRoom/RoomArtStandard.shader");
            var material = new Material(shader);
            var gradient = new Texture2D(128, 1, TextureFormat.RGBAFloat, false, true);
            var output = new Texture2D(128, 8, TextureFormat.RGBAFloat, false, true);
            var target = RenderTexture.GetTemporary(128, 8, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                SceneManager.MoveGameObjectToScene(lightObject, scene);
                SceneManager.MoveGameObjectToScene(quad, scene);
                const int layer = 28;
                quad.layer = layer;
                quad.transform.position = new Vector3(10000, 0, 2);
                quad.transform.localScale = new Vector3(32, 2, 1);
                var camera = cameraObject.GetComponent<Camera>();
                camera.enabled = false; camera.scene = scene; camera.cullingMask = 1 << layer;
                camera.transform.position = new Vector3(10000, 0, 0);
                camera.orthographic = true; camera.orthographicSize = 1; camera.aspect = 16;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                camera.allowHDR = true; camera.renderingPath = RenderingPath.Forward; camera.targetTexture = target;
                var light = lightObject.GetComponent<Light>();
                light.type = LightType.Directional; light.intensity = .6f;
                light.renderMode = LightRenderMode.ForcePixel; light.cullingMask = 1 << layer;
                var colors = new Color[128];
                for (int i = 0; i < colors.Length; i++) colors[i] = new Color(.1f + i / 160f, .2f, .1f, 1);
                gradient.SetPixels(colors); gradient.Apply(); gradient.wrapMode = TextureWrapMode.Clamp;
                material.SetTexture("_MainTex", gradient);
                material.SetFloat("_LightingBandsEnabled", 1); material.SetFloat("_LightingBands", 2);
                quad.GetComponent<MeshRenderer>().sharedMaterial = material;
                camera.Render(); RenderTexture.active = target;
                output.ReadPixels(new Rect(0, 0, 128, 8), 0, 0); output.Apply();
                var pixels = output.GetPixels();
                int detailTransitions = 0;
                for (int x = 2; x < 126; x++)
                    if (Mathf.Abs(pixels[4 * 128 + x].r - pixels[4 * 128 + x - 1].r) > .0001f) detailTransitions++;
                Assert.That(detailTransitions, Is.GreaterThan(100),
                    "A constant lighting band must multiply the original gradient, not quantize texture colours.");
            }
            finally
            {
                RenderTexture.active = previous;
                cameraObject.GetComponent<Camera>().targetTexture = null;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(material); Object.DestroyImmediate(gradient); Object.DestroyImmediate(output);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        public void DisablingFullscreenQuantizationPreservesFineColoursAndHdr()
        {
            var colors = new[] { new Color(.123f, .401f, .804f, .3f), new Color(4, 2, 1, .7f),
                new Color(-.1f, .2f, .7f, 1), Color.black };
            var cameraObject = new GameObject("Retro quantization test", typeof(Camera));
            cameraObject.GetComponent<Camera>().enabled = false;
            var effect = cameraObject.AddComponent<RetroScreenEffect>();
            var profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            var input = new Texture2D(4, 1, TextureFormat.RGBAFloat, false, true);
            var output = new Texture2D(4, 1, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(4, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var destination = RenderTexture.GetTemporary(4, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            try
            {
                effect.Configure(profile, AssetDatabase.LoadAssetAtPath<Shader>(RetroScreenSetup.ShaderPath));
                profile.Strength = 1; profile.PixelSize = 1; profile.FullscreenQuantization = false;
                profile.GammaBefore = .25f; profile.GammaAfter = 3; profile.Levels = 4;
                input.SetPixels(colors); input.Apply();
                Graphics.Blit(input, source);
                effect.Render(source, destination);
                RenderTexture.active = destination;
                output.ReadPixels(new Rect(0, 0, 4, 1), 0, 0); output.Apply();
                var actual = output.GetPixels();
                for (int i = 0; i < colors.Length; i++)
                    Assert.That(Vector4.Distance(actual[i], colors[i]), Is.LessThan(.0001f),
                        "Disabling screen quantization must also bypass both gamma adjustments.");
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(destination);
                Object.DestroyImmediate(input); Object.DestroyImmediate(output);
                Object.DestroyImmediate(cameraObject); Object.DestroyImmediate(profile);
            }
        }
    }
}
