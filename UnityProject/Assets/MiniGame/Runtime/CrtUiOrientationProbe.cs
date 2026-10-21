#if DEVELOPMENT_BUILD || UNITY_EDITOR || MINIGAME_FEEDBACK_PROBE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Tests the complete native UI camera to physical CRT shader path using asymmetric UI markers.</summary>
    internal static class CrtUiOrientationProbe
    {
        private static readonly string[] Labels = { "TL-red", "TR-green", "BL-blue", "BR-yellow" };
        private static readonly Vector2[] Positions =
        {
            new Vector2(.25f, .75f), new Vector2(.75f, .75f),
            new Vector2(.25f, .25f), new Vector2(.75f, .25f)
        };
        private static readonly Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow };

        [Serializable]
        private sealed class Sample
        {
            public string expected;
            public Vector2 pixel;
            public Color centerColor;
            public int matchingPixels, sampledPixels;
            public bool passed;
        }

        [Serializable]
        private sealed class Report
        {
            public bool passed;
            public string capture, graphicsDevice, description;
            public int sourceFramesBefore, sourceFramesAfter;
            public Vector4 screenViewport;
            public Sample[] samples;
        }

        public static bool CaptureAndVerify(MiniGameBootstrap host, Renderer surface, string path)
        {
            var display = host.Screen.TerminalDisplay;
            var sourceCamera = host.Screen.GetComponentsInChildren<Camera>(true)
                .Single(item => item.name == "Terminal UI camera");
            var alignment = host.Player.transform.root.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Terminal screen alignment");
            var source = (RectTransform)sourceCamera.transform.parent;
            var markers = new List<GameObject>();
            var previous = RenderTexture.active;
            GameObject cameraObject = null;
            RenderTexture target = null;
            Texture2D image = null;
            var report = new Report
            {
                capture = path,
                graphicsDevice = SystemInfo.graphicsDeviceType.ToString(),
                description = "Four Image markers in the real 800x600 native source canvas, sampled on the front-facing physical CRT after the existing material and UV mapping.",
                sourceFramesBefore = display.RenderedFrameCount
            };
            try
            {
                for (int index = 0; index < Positions.Length; index++)
                {
                    var marker = new GameObject("CRT orientation " + Labels[index], typeof(RectTransform), typeof(Image));
                    markers.Add(marker);
                    marker.transform.SetParent(source, false);
                    var rect = (RectTransform)marker.transform;
                    rect.anchorMin = rect.anchorMax = Positions[index];
                    rect.pivot = new Vector2(.5f, .5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(80, 80);
                    var graphic = marker.GetComponent<Image>();
                    graphic.color = Colors[index];
                    graphic.raycastTarget = false;
                }
                display.RenderFrame();
                report.sourceFramesAfter = display.RenderedFrameCount;

                cameraObject = new GameObject("CRT orientation capture camera", typeof(Camera));
                var camera = cameraObject.GetComponent<Camera>();
                camera.CopyFrom(host.Player.ViewCamera);
                camera.enabled = false;
                camera.orthographic = true;
                camera.aspect = 4f / 3;
                camera.nearClipPlane = .01f;
                camera.farClipPlane = 10;
                // Stay in front of the glass and inside the chair so furniture cannot hide a marker.
                camera.transform.SetPositionAndRotation(surface.bounds.center - alignment.forward * .35f, alignment.rotation);
                var meshBounds = surface.GetComponent<MeshFilter>().sharedMesh.bounds;
                var vertices = new List<Vector3>();
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                            vertices.Add(surface.transform.TransformPoint(meshBounds.center
                                + Vector3.Scale(meshBounds.extents, new Vector3(x, y, z))));
                var local = vertices.Select(point => camera.transform.InverseTransformPoint(point)).ToArray();
                float minX = local.Min(point => point.x), maxX = local.Max(point => point.x);
                float minY = local.Min(point => point.y), maxY = local.Max(point => point.y);
                camera.transform.position += alignment.right * ((minX + maxX) * .5f)
                    + alignment.up * ((minY + maxY) * .5f);
                camera.orthographicSize = Mathf.Max((maxY - minY) * .5f, (maxX - minX) / (2 * camera.aspect)) * 1.2f;
                target = RenderTexture.GetTemporary(1280, 960, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();

                var projected = vertices.Select(point => camera.WorldToViewportPoint(point)).ToArray();
                report.screenViewport = new Vector4(projected.Min(point => point.x), projected.Min(point => point.y),
                    projected.Max(point => point.x), projected.Max(point => point.y));
                var pixels = image.GetPixels32();
                report.samples = new Sample[Positions.Length];
                for (int index = 0; index < Positions.Length; index++)
                {
                    float x = Mathf.Lerp(report.screenViewport.x, report.screenViewport.z, Positions[index].x);
                    float y = Mathf.Lerp(report.screenViewport.y, report.screenViewport.w, Positions[index].y);
                    int px = Mathf.RoundToInt(x * (image.width - 1)), py = Mathf.RoundToInt(y * (image.height - 1));
                    var sample = new Sample { expected = Labels[index], pixel = new Vector2(px, py) };
                    if (px >= 5 && py >= 5 && px < image.width - 5 && py < image.height - 5)
                    {
                        sample.centerColor = pixels[py * image.width + px];
                        for (int dy = -5; dy <= 5; dy++)
                            for (int dx = -5; dx <= 5; dx++)
                            {
                                sample.sampledPixels++;
                                if (Matches(pixels[(py + dy) * image.width + px + dx], index)) sample.matchingPixels++;
                            }
                    }
                    sample.passed = sample.sampledPixels == 121 && sample.matchingPixels >= 85;
                    report.samples[index] = sample;
                }
                report.passed = report.sourceFramesAfter > report.sourceFramesBefore
                    && projected.All(point => point.z > camera.nearClipPlane)
                    && report.samples.All(sample => sample.passed);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
                File.WriteAllText(Path.ChangeExtension(path, ".json"), JsonUtility.ToJson(report, true));
                return report.passed;
            }
            finally
            {
                RenderTexture.active = previous;
                foreach (var marker in markers)
                {
                    marker.SetActive(false);
                    UnityEngine.Object.Destroy(marker);
                }
                if (cameraObject != null)
                {
                    cameraObject.GetComponent<Camera>().targetTexture = null;
                    UnityEngine.Object.Destroy(cameraObject);
                }
                if (target != null) RenderTexture.ReleaseTemporary(target);
                if (image != null) UnityEngine.Object.Destroy(image);
                RenderTexture.active = previous;
                display.RenderFrame();
                RenderTexture.active = previous;
            }
        }

        private static bool Matches(Color32 pixel, int index)
        {
            switch (index)
            {
                case 0: return pixel.r > 70 && pixel.r > pixel.g * 1.6f && pixel.r > pixel.b * 1.6f;
                case 1: return pixel.g > 70 && pixel.g > pixel.r * 1.6f && pixel.g > pixel.b * 1.6f;
                case 2: return pixel.b > 70 && pixel.b > pixel.r * 1.6f && pixel.b > pixel.g * 1.6f;
                default: return pixel.r > 70 && pixel.g > 70 && Mathf.Min(pixel.r, pixel.g) > pixel.b * 1.6f;
            }
        }
    }
}
#endif
