using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class WallSeamPlayModeTests
    {
        [Serializable] private sealed class Sample
        {
            public int yaw;
            public float baselineExcess, fixedExcess;
        }

        [Serializable] private sealed class Report
        {
            public Sample[] samples;
            public float baselineMaximum, fixedMaximum;
            public string cameraScope = "Actual player ViewCamera; raw world preview, grading/grain bypassed for measurement";
            public int width = 1280, height = 720;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator CameraYawSweepHasNoBrightSeamAndCoplanarControlReproducesIt()
        {
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            CampaignWorldFixture.Restore(host);
            host.Player.enabled = false;
            string checkpoint = host.Runtime.Save();
            var camera = host.Player.ViewCamera;
            var retro = camera.GetComponent<RetroScreenEffect>();
            var filters = Object.FindObjectsOfType<MeshRenderer>().Where(renderer => renderer.sharedMaterial != null
                && renderer.sharedMaterial.name == "MainRoom_30_B").Select(renderer => renderer.GetComponent<MeshFilter>()).ToArray();
            Assert.That(filters.Length, Is.EqualTo(12));
            var originals = filters.Select(filter => filter.sharedMesh).ToArray();
            var normals = filters.Select((filter, index) => filter.transform.worldToLocalMatrix.transpose
                .MultiplyVector(originals[index].normals[0]).normalized).ToArray();
            var planes = filters.Select((filter, index) => Vector3.Dot(filter.transform.TransformPoint(originals[index].vertices[0]), normals[index])).ToArray();
            var control = new Mesh[filters.Length];
            for (int index = 0; index < filters.Length; index++)
            {
                float plane = planes.Where((value, other) => Vector3.Dot(normals[index], normals[other]) > .999f
                    && Mathf.Abs(value - planes[index]) < .02f).Min();
                var shift = normals[index] * (plane - planes[index]);
                control[index] = Object.Instantiate(originals[index]);
                var filter = filters[index];
                control[index].vertices = originals[index].vertices.Select(vertex => filter.transform.InverseTransformPoint(
                    filter.transform.TransformPoint(vertex) + shift)).ToArray();
                control[index].RecalculateBounds();
            }
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            float fov = camera.fieldOfView, aspect = camera.aspect;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            bool preview = retro.PreviewOriginal;
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            var samples = new List<Sample>();
            string folder = Environment.GetEnvironmentVariable("MINIGAME_CAPTURE_DIR");
            try
            {
                target.Create(); camera.targetTexture = target; camera.aspect = 16f / 9; camera.fieldOfView = 70;
                camera.transform.position = new Vector3(-1.9f, .88712f, -1.85f);
                // Isolate the reported lighting defect from intentional temporal film grain and grading.
                retro.PreviewOriginal = true;
                for (int yaw = -20; yaw <= 20; yaw += 2)
                {
                    camera.transform.rotation = Quaternion.Euler(0, yaw, 0);
                    var sample = new Sample { yaw = yaw };
                    for (int mode = 0; mode < 2; mode++)
                    {
                        for (int index = 0; index < filters.Length; index++) filters[index].sharedMesh = mode == 0 ? control[index] : originals[index];
                        camera.Render(); RenderTexture.active = target;
                        pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
                        float excess = SeamExcess(camera, pixels);
                        if (mode == 0) sample.baselineExcess = excess; else sample.fixedExcess = excess;
                        if (!string.IsNullOrEmpty(folder) && (yaw == 0 || mode == 1 && Mathf.Abs(yaw) == 20))
                        {
                            Directory.CreateDirectory(folder);
                            File.WriteAllBytes(Path.Combine(folder, (mode == 0 ? "seam-control-" : "seam-fixed-") + yaw + ".png"), pixels.EncodeToPNG());
                        }
                    }
                    samples.Add(sample);
                }
                float baselineMaximum = samples.Max(sample => sample.baselineExcess);
                float fixedMaximum = samples.Max(sample => sample.fixedExcess);
                if (!string.IsNullOrEmpty(folder))
                    File.WriteAllText(Path.Combine(Directory.GetParent(folder).FullName, "camera-sweep.json"),
                        JsonUtility.ToJson(new Report { samples = samples.ToArray(), baselineMaximum = baselineMaximum, fixedMaximum = fixedMaximum }, true));
                Assert.That(baselineMaximum, Is.GreaterThan(fixedMaximum + .1f), "The coplanar control must reproduce the original visible lighting seam.");
                Assert.That(fixedMaximum, Is.LessThan(.1f), "The fixed seam must not produce a bright spike across the yaw sweep.");
                Assert.That(host.Runtime.Save(), Is.EqualTo(checkpoint));
            }
            finally
            {
                for (int index = 0; index < filters.Length; index++) { filters[index].sharedMesh = originals[index]; Object.Destroy(control[index]); }
                camera.transform.SetPositionAndRotation(position, rotation); camera.fieldOfView = fov; camera.aspect = aspect;
                camera.targetTexture = previousTarget; retro.PreviewOriginal = preview; RenderTexture.active = previousActive;
                target.Release(); Object.Destroy(target); Object.Destroy(pixels);
            }
            yield return null;
        }

        private static float SeamExcess(Camera camera, Texture2D pixels)
        {
            float maximum = 0;
            for (int row = 0; row <= 24; row++)
            {
                float y = .3f + row * .075f;
                var point = camera.WorldToViewportPoint(new Vector3(-1.82723f, y, .68533f));
                int px = Mathf.RoundToInt(point.x * pixels.width), py = Mathf.RoundToInt(point.y * pixels.height);
                if (px < 10 || px >= pixels.width - 10 || py < 1 || py >= pixels.height - 1) continue;
                float sides = (Luminance(pixels.GetPixel(px - 9, py)) + Luminance(pixels.GetPixel(px + 9, py))) * .5f;
                for (int offset = -3; offset <= 3; offset++)
                    maximum = Mathf.Max(maximum, Luminance(pixels.GetPixel(px + offset, py)) - sides);
            }
            return maximum;
        }

        private static float Luminance(Color color) => color.r * .2126f + color.g * .7152f + color.b * .0722f;
    }
}
