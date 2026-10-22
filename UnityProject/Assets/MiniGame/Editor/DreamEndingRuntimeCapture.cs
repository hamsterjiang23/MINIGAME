using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Small real-GPU capture for the authored Unity dream settings; also callable from batch mode.</summary>
    public static class DreamEndingRuntimeCapture
    {
        [Serializable] private sealed class Report
        {
            public string unity, sceneScope;
            public int changedWavePixels, changedRetroPixels, captureFrames;
            public bool fixedCaptureEye, liveParameters, passed;
        }

        [MenuItem("MiniGame/梦境/验证动态海浪与复古效果")]
        public static void Capture()
        {
            string output = Environment.GetEnvironmentVariable("MINIGAME_DREAM_RUNTIME_EVIDENCE")
                ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/Unity/DreamRuntime20261022"));
            Directory.CreateDirectory(output);
            var authored = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            if (authored == null || authored.PanoramaWaterMask == null || authored.EndingRetro == null
                || authored.RoomPanoramaSkybox == null || authored.RoomPanoramaSkybox.shader == null)
                throw new InvalidOperationException("梦境缺少海面遮罩、天空盒或独立复古配置。");
            var profile = UnityEngine.Object.Instantiate(authored);
            profile.hideFlags = HideFlags.HideAndDontSave;
            profile.CubemapFaceSize = 256;
            var cameraObject = new GameObject("Dream render validation", typeof(Camera), typeof(Skybox), typeof(RetroScreenEffect))
                { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false; camera.cullingMask = 0; camera.fieldOfView = 80; camera.aspect = 16f / 9;
            camera.transform.position = new Vector3(500, 100, 500);
            camera.transform.rotation = Quaternion.Euler(8, 0, 0);
            var retro = cameraObject.GetComponent<RetroScreenEffect>();
            retro.Configure(profile.EndingRetro, Resources.Load<Shader>("RetroScreen"));
            retro.PresentationBypass = true;
            var sky = new Material(profile.RoomPanoramaSkybox) { hideFlags = HideFlags.HideAndDontSave };
            cameraObject.GetComponent<Skybox>().material = sky;
            var target = new RenderTexture(640, 360, 24, RenderTextureFormat.ARGB32);
            target.Create(); camera.targetTexture = target;
            DreamCubemapRenderer capture = null;
            Texture2D first = null, second = null, tuned = null, styled = null;
            var previous = RenderTexture.active;
            try
            {
                var origin = camera.transform.position;
                capture = new DreamCubemapRenderer(profile, camera, profile.RoomPanoramaSkybox, origin);
                capture.Render(0, BeachEndingPhase.Walking, 0, 0, true);
                sky.SetTexture("_Tex", capture.Texture);
                camera.clearFlags = CameraClearFlags.Skybox;
                first = Read(camera, target, Path.Combine(output, "waves-t0.png"));
                capture.Render(2.1f, BeachEndingPhase.Walking, 0, 0, true);
                sky.SetTexture("_Tex", capture.Texture);
                second = Read(camera, target, Path.Combine(output, "waves-t2.png"));
                profile.SeaWaveHeight = .48f;
                capture.Render(2.1f, BeachEndingPhase.Walking, 0, 0);
                bool live = Mathf.Abs(capture.PanoramaWaterMaterial.GetFloat("_WaveHeight") - .48f) < .0001f;
                sky.SetTexture("_Tex", capture.Texture);
                tuned = Read(camera, target, Path.Combine(output, "waves-tuned.png"));
                retro.PresentationBypass = false;
                styled = Read(camera, target, Path.Combine(output, "retro.png"));
                camera.transform.position += new Vector3(1, .2f, -.5f);
                capture.Render(3, BeachEndingPhase.Walking, 0, 0);
                var report = new Report
                {
                    unity = Application.unityVersion,
                    sceneScope = "Authored production assets and shaders rendered on GPU; Campaign Play Mode tested separately.",
                    changedWavePixels = Changed(first, second), changedRetroPixels = Changed(tuned, styled),
                    captureFrames = capture.CapturedFrames,
                    fixedCaptureEye = (capture.CaptureCamera.transform.position - origin).sqrMagnitude < .000001f,
                    liveParameters = live
                };
                report.passed = report.changedWavePixels > 100 && report.changedRetroPixels > 100 && report.fixedCaptureEye && live;
                File.WriteAllText(Path.Combine(output, "capture-verification.json"), JsonUtility.ToJson(report, true));
                if (!report.passed) throw new InvalidOperationException("动态梦境 GPU 验证失败，见 capture-verification.json。");
                Debug.Log("DREAM_RUNTIME_CAPTURE_PASSED " + JsonUtility.ToJson(report));
            }
            finally
            {
                capture?.Dispose();
                RenderTexture.active = previous;
                camera.targetTexture = null; target.Release();
                foreach (var value in new UnityEngine.Object[] { first, second, tuned, styled, target, sky, cameraObject, profile })
                    if (value != null) UnityEngine.Object.DestroyImmediate(value);
            }
        }

        private static Texture2D Read(Camera camera, RenderTexture target, string path)
        {
            camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            return image;
        }

        private static int Changed(Texture2D first, Texture2D second)
        {
            var a = first.GetPixels32(); var b = second.GetPixels32(); int count = 0;
            for (int i = 0; i < a.Length; i++)
                if (Math.Abs(a[i].r - b[i].r) + Math.Abs(a[i].g - b[i].g) + Math.Abs(a[i].b - b[i].b) > 12) count++;
            return count;
        }
    }
}
