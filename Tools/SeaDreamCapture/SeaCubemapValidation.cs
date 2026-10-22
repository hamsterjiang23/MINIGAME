using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Copied into an isolated temporary Unity project. Never installed into the live game.
public static class SeaCubemapValidation
{
    [Serializable] private sealed class Report
    {
        public string unity, source, scope;
        public int faces, faceSize;
        public float maximumStoredChannelError, maximumCenterRenderError;
        public float maximumFaceMeanRenderError;
        public bool passed;
    }

    private static readonly string[] Names = { "px", "nx", "py", "ny", "pz", "nz" };
    private static readonly Vector3[] Directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
    private static readonly Vector3[] Ups = { Vector3.up, Vector3.up, Vector3.back, Vector3.forward, Vector3.up, Vector3.up };

    public static void Run()
    {
        string root = Environment.GetEnvironmentVariable("SEA_CAPTURE_ROOT");
        string evidence = Environment.GetEnvironmentVariable("SEA_CAPTURE_EVIDENCE")
            ?? Path.Combine(root, "Artifacts/Unity/SeaMidpoint20261022");
        Directory.CreateDirectory(evidence);
        try
        {
            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/SeaMidpointPanorama.asset");
            if (cube == null || cube.width != 1024 || !cube.isReadable)
                throw new InvalidOperationException("Native Cubemap did not import as a readable 1024-pixel texture.");
            var report = new Report
            {
                unity = Application.unityVersion,
                source = "SeaMidpointPanorama.asset",
                scope = "Isolated Unity import, six-face byte comparison and GPU skybox rendering; not a full Campaign Play Mode test.",
                faces = 6,
                faceSize = cube.width
            };
            var go = new GameObject("Cubemap validation camera", typeof(Camera), typeof(Skybox));
            var camera = go.GetComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.cullingMask = 0;
            camera.fieldOfView = 90;
            camera.aspect = 1;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            var material = new Material(Shader.Find("Skybox/Cubemap"));
            material.SetTexture("_Tex", cube);
            material.SetColor("_Tint", new Color(.5f, .5f, .5f, 1));
            material.SetFloat("_Exposure", 1);
            go.GetComponent<Skybox>().material = material;
            var target = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            var readback = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            for (int face = 0; face < 6; face++)
            {
                var original = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!original.LoadImage(File.ReadAllBytes(Path.Combine(root, "Art/DreamEnding/SeaMidpoint/Faces/" + Names[face] + ".png"))))
                    throw new InvalidOperationException("Could not load rendered face " + Names[face]);
                var expected = original.GetPixels32();
                var actual = cube.GetPixels((CubemapFace)face);
                if (expected.Length != actual.Length) throw new InvalidOperationException("Face pixel count differs.");
                for (int pixel = 0; pixel < expected.Length; pixel++)
                {
                    int x = pixel % cube.width, y = pixel / cube.width;
                    Color32 stored = actual[(cube.width - 1 - y) * cube.width + x];
                    report.maximumStoredChannelError = Mathf.Max(report.maximumStoredChannelError,
                        Math.Abs(expected[pixel].r - stored.r), Math.Abs(expected[pixel].g - stored.g),
                        Math.Abs(expected[pixel].b - stored.b));
                }
                camera.transform.rotation = Quaternion.LookRotation(Directions[face], Ups[face]);
                camera.Render();
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                readback.Apply();
                File.WriteAllBytes(Path.Combine(evidence, Names[face] + ".png"), readback.EncodeToPNG());
                Color a = original.GetPixelBilinear(128.5f / 256, 128.5f / 256), b = readback.GetPixel(128, 128);
                report.maximumCenterRenderError = Mathf.Max(report.maximumCenterRenderError,
                    Math.Abs(a.r - b.r), Math.Abs(a.g - b.g), Math.Abs(a.b - b.b));
                // A single center pixel can fall on the high-contrast horizon. Check the full
                // face interior instead, with pixel-center UVs at the actual target resolution.
                float error = 0; int samples = 0;
                for (int y = 4; y < 252; y += 8)
                    for (int x = 4; x < 252; x += 8)
                    {
                        Color expectedColor = original.GetPixelBilinear((x + .5f) / 256, (y + .5f) / 256);
                        Color actualColor = readback.GetPixel(x, y);
                        error += Math.Abs(expectedColor.r - actualColor.r) + Math.Abs(expectedColor.g - actualColor.g)
                            + Math.Abs(expectedColor.b - actualColor.b);
                        samples += 3;
                    }
                report.maximumFaceMeanRenderError = Mathf.Max(report.maximumFaceMeanRenderError, error / samples);
                UnityEngine.Object.DestroyImmediate(original);
            }
            RenderTexture.active = null;
            report.passed = report.maximumStoredChannelError == 0 && report.maximumFaceMeanRenderError < .03f;
            File.WriteAllText(Path.Combine(evidence, "verification.json"), JsonUtility.ToJson(report, true));
            if (!report.passed) throw new InvalidOperationException("Cubemap pixel or render verification failed.");
            Debug.Log("SEA_CUBEMAP_VERIFIED " + JsonUtility.ToJson(report));
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            File.WriteAllText(Path.Combine(evidence, "failure.txt"), exception.ToString());
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
}
