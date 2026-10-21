using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>An isolated illustrated stage, rendered explicitly into a texture instead of a display.</summary>
    public sealed class DreamAnimationStage : IDisposable
    {
        public const int StageLayer = 29;
        private static int nextStage;
        private readonly GameObject root;
        private readonly Mesh mesh;
        private readonly Material material;
        public Camera StageCamera { get; }
        public RenderTexture Texture { get; }
        public float PlaybackSeconds { get; private set; }
        public int CurrentFrameIndex { get; private set; }
        public int NextFrameIndex { get; private set; }
        public float FrameBlend { get; private set; }

        public DreamAnimationStage(int width = 1920, int height = 1080)
        {
            var shader = Resources.Load<Shader>("DreamProjectionAnimation");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("DreamProjectionAnimation shader is unavailable.");
            root = new GameObject("Dream animation stage") { hideFlags = HideFlags.HideAndDontSave, layer = StageLayer };
            root.transform.position = new Vector3(10000 + ++nextStage * 32, 10000, 10000);
            var cameraObject = new GameObject("Offscreen dream camera") { hideFlags = HideFlags.HideAndDontSave, layer = StageLayer };
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0, -10);
            StageCamera = cameraObject.AddComponent<Camera>();
            StageCamera.enabled = false;
            StageCamera.orthographic = true;
            StageCamera.orthographicSize = 4.5f;
            StageCamera.aspect = 16f / 9f;
            StageCamera.nearClipPlane = .1f;
            StageCamera.farClipPlane = 20;
            StageCamera.clearFlags = CameraClearFlags.SolidColor;
            StageCamera.backgroundColor = Color.black;
            StageCamera.cullingMask = 1 << StageLayer;
            StageCamera.allowHDR = false;
            StageCamera.allowMSAA = false;
            StageCamera.useOcclusionCulling = false;
            Texture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            { name = "Dream wall projection", hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            Texture.Create();
            StageCamera.targetTexture = Texture;
            var plate = new GameObject("Animated illustration layers") { hideFlags = HideFlags.HideAndDontSave, layer = StageLayer };
            plate.transform.SetParent(root.transform, false);
            mesh = new Mesh { name = "Dream stage screen", hideFlags = HideFlags.HideAndDontSave };
            mesh.vertices = new[] { new Vector3(-8, -4.5f, 0), new Vector3(8, -4.5f, 0), new Vector3(8, 4.5f, 0), new Vector3(-8, 4.5f, 0) };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            plate.AddComponent<MeshFilter>().sharedMesh = mesh;
            material = new Material(shader) { name = "Dream procedural motion", hideFlags = HideFlags.HideAndDontSave };
            var renderer = plate.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        public void Render(int day, Texture2D artwork, float seconds, float duration, float motionIntensity = 1)
        {
            CurrentFrameIndex = NextFrameIndex = 0;
            FrameBlend = 0;
            material.SetFloat("_SequenceMode", 0);
            material.SetFloat("_PostIntensity", 0);
            PlaybackSeconds = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.Max(0, seconds);
            material.SetTexture("_Artwork", artwork != null ? artwork : Texture2D.blackTexture);
            material.SetFloat("_Day", Mathf.Clamp(day, 1, 3));
            material.SetFloat("_PlaybackSeconds", PlaybackSeconds);
            material.SetFloat("_Progress", Mathf.Clamp01(PlaybackSeconds / Mathf.Max(.01f, duration)));
            material.SetFloat("_MotionIntensity", Mathf.Clamp(motionIntensity, 0, 2));
            RenderCamera();
        }

        public void Render(DreamDaySettings settings, float seconds)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            int count = settings.Frames?.Length ?? 0;
            if (count == 0)
            {
                Render(settings.Day, settings.Artwork, seconds, settings.Duration, settings.MotionIntensity);
                return;
            }
            float duration = float.IsNaN(settings.Duration) || float.IsInfinity(settings.Duration) ? 20 : Mathf.Max(.01f, settings.Duration);
            PlaybackSeconds = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.Clamp(seconds, 0, duration);
            float step = duration / count;
            CurrentFrameIndex = Mathf.Min(count - 1, Mathf.FloorToInt(PlaybackSeconds / step));
            NextFrameIndex = Mathf.Min(count - 1, CurrentFrameIndex + 1);
            float fade = float.IsNaN(settings.CrossfadeSeconds) || float.IsInfinity(settings.CrossfadeSeconds)
                ? 0 : Mathf.Clamp(settings.CrossfadeSeconds, 0, step);
            FrameBlend = CurrentFrameIndex == NextFrameIndex || fade <= 0 ? 0
                : Mathf.SmoothStep(0, 1, Mathf.Clamp01((PlaybackSeconds - CurrentFrameIndex * step - step + fade) / fade));
            material.SetTexture("_Artwork", Frame(settings, CurrentFrameIndex));
            material.SetTexture("_ArtworkNext", Frame(settings, NextFrameIndex));
            material.SetFloat("_FrameBlend", FrameBlend);
            material.SetFloat("_SequenceMode", 1);
            material.SetFloat("_PostIntensity", float.IsNaN(settings.PostIntensity) || float.IsInfinity(settings.PostIntensity)
                ? 0 : Mathf.Clamp(settings.PostIntensity, 0, 2));
            material.SetFloat("_Day", Mathf.Clamp(settings.Day, 1, 3));
            material.SetFloat("_PlaybackSeconds", PlaybackSeconds);
            material.SetFloat("_Progress", PlaybackSeconds / duration);
            material.SetFloat("_MotionIntensity", float.IsNaN(settings.MotionIntensity) || float.IsInfinity(settings.MotionIntensity)
                ? 0 : Mathf.Clamp(settings.MotionIntensity, 0, 2));
            RenderCamera();
        }

        private static Texture2D Frame(DreamDaySettings settings, int index)
            => settings.Frames[index] != null ? settings.Frames[index]
                : settings.Artwork != null ? settings.Artwork : Texture2D.blackTexture;

        private void RenderCamera()
        {
            var previous = RenderTexture.active;
            try { StageCamera.Render(); }
            finally { RenderTexture.active = previous; }
        }

        public void Dispose()
        {
            if (StageCamera != null) StageCamera.targetTexture = null;
            if (Texture != null) Texture.Release();
            DestroyOwned(root); DestroyOwned(mesh); DestroyOwned(material); DestroyOwned(Texture);
        }

        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
