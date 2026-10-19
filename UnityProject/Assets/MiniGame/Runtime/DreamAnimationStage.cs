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
            PlaybackSeconds = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.Max(0, seconds);
            material.SetTexture("_Artwork", artwork != null ? artwork : Texture2D.blackTexture);
            material.SetFloat("_Day", Mathf.Clamp(day, 1, 3));
            material.SetFloat("_PlaybackSeconds", PlaybackSeconds);
            material.SetFloat("_Progress", Mathf.Clamp01(PlaybackSeconds / Mathf.Max(.01f, duration)));
            material.SetFloat("_MotionIntensity", Mathf.Clamp(motionIntensity, 0, 2));
            StageCamera.Render();
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
