using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Captures authored daytime rendering at one unchanged pose and simulation frame.</summary>
    public static class PixelStrengthPreviewCapture
    {
        private const int Width = 1280, Height = 720;

        [Serializable] private sealed class CaptureReport
        {
            public string scene, unity, camera, sourceProfile, sourceProfileGuid, sourceProfileJson, shader;
            public string captureMode = "Camera.Render, actual Combined daytime GPU output; UI canvases temporarily disabled.";
            public int width = Width, height = Height, frame;
            public Vector3 position, eulerAngles;
            public Quaternion rotation;
            public float fieldOfView, nightBlend;
            public bool restored;
            public List<FrameReport> captures = new List<FrameReport>();
        }

        [Serializable] private sealed class FrameReport
        {
            public string name, path, sha256;
            public int pixelSize, gridColumns, frame;
            public float shaderPixelSize, shaderVirtualResolution;
        }

        public static string Capture(string outputDirectory = "Assets/MiniGame/Resources/MiniGame/PixelPreviews")
        {
            Require(Application.isPlaying, "Preview capture requires Play Mode.");
            var host = UnityEngine.Object.FindObjectsOfType<MiniGameBootstrap>()
                .SingleOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
            Require(host != null && host.Player != null && host.Player.ViewCamera != null,
                "Expected the formal Combined player camera.");
            var camera = host.Player.ViewCamera;
            var effect = camera.GetComponent<RetroScreenEffect>();
            Require(effect != null && effect.isActiveAndEnabled, "The actual player retro effect must be active.");
            var lighting = effect.LightingSource;
            Require(lighting != null && lighting.EffectivePeriod == RoomLightingPeriod.Day && lighting.NightBlend <= .0001f,
                "Prepare a stable daytime room before capturing previews.");
            var profile = effect.ActiveProfile;
            Require(profile != null && profile.EffectEnabled && profile.Strength > 0,
                "The active daytime style must enable the pixel shader.");
            Require(effect.PresentationProfile == null, "Finish the current presentation before capturing previews.");
            var shader = new SerializedObject(effect).FindProperty("effectShader").objectReferenceValue as Shader;
            Require(shader != null && shader.isSupported, "The actual camera pixel shader must be supported.");
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.GetFullPath(Path.IsPathRooted(outputDirectory)
                ? outputDirectory : Path.Combine(projectRoot, outputDirectory));
            Directory.CreateDirectory(output);
            var report = new CaptureReport
            {
                scene = host.gameObject.scene.path, unity = Application.unityVersion, camera = camera.name,
                sourceProfile = AssetDatabase.GetAssetPath(profile), sourceProfileJson = JsonUtility.ToJson(profile),
                shader = shader.name, frame = Time.frameCount, position = camera.transform.position,
                rotation = camera.transform.rotation, eulerAngles = camera.transform.eulerAngles,
                fieldOfView = camera.fieldOfView, nightBlend = lighting.NightBlend
            };
            report.sourceProfileGuid = AssetDatabase.AssetPathToGUID(report.sourceProfile);
            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;
            var originalRect = camera.rect;
            float originalAspect = camera.aspect;
            var originalProfile = effect.PresentationProfile;
            bool originalPreview = effect.PreviewOriginal, originalBypass = effect.PresentationBypass;
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            bool[] canvasStates = canvases.Select(item => item.enabled).ToArray();
            var temporary = UnityEngine.Object.Instantiate(profile);
            temporary.hideFlags = HideFlags.HideAndDontSave;
            var target = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                foreach (var canvas in canvases) canvas.enabled = false;
                effect.PreviewOriginal = false;
                effect.PresentationBypass = false;
                effect.PresentationProfile = temporary;
                temporary.UseVirtualResolution = false;
                camera.targetTexture = target;
                camera.rect = new Rect(0, 0, 1, 1);
                camera.aspect = Width / (float)Height;
                string[] names = { "High", "Medium", "Low" };
                int[] sizes = { 8, 4, 2 };
                for (int i = 0; i < names.Length; i++)
                {
                    temporary.PixelSize = sizes[i];
                    camera.Render();
                    Require(Time.frameCount == report.frame, "Preview frames must share one simulation frame.");
                    Require(camera.transform.position == report.position && camera.transform.rotation == report.rotation,
                        "The camera pose changed during capture.");
                    var material = (Material)typeof(RetroScreenEffect)
                        .GetField("material", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effect);
                    Require(material != null && material.GetFloat("_PixelSize") == sizes[i]
                        && material.GetFloat("_UseVirtualResolution") == 0, "Actual shader uniforms differ from the requested preset.");
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                    pixels.Apply(false, false);
                    byte[] png = pixels.EncodeToPNG();
                    string path = Path.Combine(output, names[i] + ".png");
                    File.WriteAllBytes(path, png);
                    report.captures.Add(new FrameReport
                    {
                        name = names[i], path = path, pixelSize = sizes[i], gridColumns = 16 / sizes[i],
                        frame = Time.frameCount, shaderPixelSize = material.GetFloat("_PixelSize"),
                        shaderVirtualResolution = material.GetFloat("_UseVirtualResolution"), sha256 = Hash(png)
                    });
                }
            }
            finally
            {
                camera.targetTexture = originalTarget;
                camera.rect = originalRect;
                camera.aspect = originalAspect;
                RenderTexture.active = originalActive;
                effect.PresentationProfile = originalProfile;
                effect.PreviewOriginal = originalPreview;
                effect.PresentationBypass = originalBypass;
                for (int i = 0; i < canvases.Length; i++) if (canvases[i] != null) canvases[i].enabled = canvasStates[i];
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(pixels);
                UnityEngine.Object.DestroyImmediate(temporary);
            }
            Require(JsonUtility.ToJson(profile) == report.sourceProfileJson, "The authored daytime profile was modified.");
            report.restored = camera.targetTexture == originalTarget && RenderTexture.active == originalActive
                && effect.PresentationProfile == originalProfile && effect.PreviewOriginal == originalPreview
                && effect.PresentationBypass == originalBypass
                && canvases.Select((item, index) => item == null || item.enabled == canvasStates[index]).All(value => value);
            Require(report.restored, "Temporary capture state was not restored.");
            foreach (var capture in report.captures) ImportTexture(capture.path, projectRoot);
            string json = JsonUtility.ToJson(report, true);
            File.WriteAllText(Path.Combine(output, "capture-metadata.json"), json);
            AssetDatabase.Refresh();
            return json;
        }

        private static void ImportTexture(string path, string projectRoot)
        {
            string prefix = Path.Combine(projectRoot, "Assets") + Path.DirectorySeparatorChar;
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
            string assetPath = path.Substring(projectRoot.Length + 1).Replace('\\', '/');
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
