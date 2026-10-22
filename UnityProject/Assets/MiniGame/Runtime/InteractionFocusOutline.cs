using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Draws the exterior of a focused mesh group, leaving enclosed openings unoutlined.</summary>
    internal sealed class InteractionFocusOutline : IDisposable
    {
        private const CameraEvent RenderEvent = CameraEvent.BeforeForwardAlpha;
        private static readonly int OffsetId = Shader.PropertyToID("_FocusOutlineOffset");
        private static readonly int WidthId = Shader.PropertyToID("_FocusOutlineWidth");
        private static readonly int ExteriorId = Shader.PropertyToID("_FocusOutlineExterior");
        private static readonly int HasExteriorId = Shader.PropertyToID("_FocusOutlineHasExterior");
        private static readonly int MaskScaleId = Shader.PropertyToID("_FocusOutlineMaskScale");
        private readonly Renderer[] surfaces;
        private readonly Material material;
        private readonly ComputeShader exterior;
        private readonly Dictionary<Camera, CameraOutline> buffers = new Dictionary<Camera, CameraOutline>();

        private sealed class CameraOutline : IDisposable
        {
            public readonly CommandBuffer Buffer = new CommandBuffer { name = "Muted exterior interaction contour" };
            public RenderTexture Mask, Exterior;
            public ComputeBuffer Parents;

            public void Resize(int width, int height)
            {
                if (Mask != null && Mask.width == width && Mask.height == height) return;
                ReleaseTextures();
                Mask = new RenderTexture(width, height, 0, RenderTextureFormat.R8)
                { name = "Interaction silhouette mask", filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
                Exterior = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat)
                { name = "Interaction exterior background", filterMode = FilterMode.Point, enableRandomWrite = true,
                    hideFlags = HideFlags.HideAndDontSave };
                Mask.Create(); Exterior.Create();
                Parents = new ComputeBuffer(width * height, sizeof(uint));
            }

            private void ReleaseTextures()
            {
                Parents?.Release(); Parents = null;
                if (Mask != null) { Mask.Release(); DestroyResource(Mask); Mask = null; }
                if (Exterior != null) { Exterior.Release(); DestroyResource(Exterior); Exterior = null; }
            }

            public void Dispose() { Buffer.Release(); ReleaseTextures(); }
        }

        private InteractionFocusOutline(Renderer[] surfaces, Shader shader)
        {
            this.surfaces = surfaces;
            material = new Material(shader) { name = "Muted interaction contour", hideFlags = HideFlags.HideAndDontSave };
            if (SystemInfo.supportsComputeShaders)
            {
                var resource = Resources.Load<ComputeShader>("InteractionOutlineExterior");
                if (resource != null) exterior = UnityEngine.Object.Instantiate(resource);
            }
            Camera.onPreCull += PrepareCamera;
        }

        public static InteractionFocusOutline Create(Renderer surface, Renderer[] group = null)
        {
            if (surface == null) return null;
            var filter = surface.GetComponent<MeshFilter>();
            var mesh = surface is SkinnedMeshRenderer skinned ? skinned.sharedMesh : filter != null ? filter.sharedMesh : null;
            var shader = Resources.Load<Shader>("InteractionFocusOutline");
            return mesh != null && shader != null && shader.isSupported
                ? new InteractionFocusOutline(group != null && group.Length > 0 ? group : new[] { surface }, shader) : null;
        }

        internal static int ActivePixelSize(Camera camera)
        {
            var effect = camera.GetComponent<RetroScreenEffect>();
            var profile = effect != null ? effect.RenderProfile : null;
            return effect != null && effect.isActiveAndEnabled && !effect.PreviewOriginal
                && !effect.PresentationBypass
                && profile != null && profile.EffectEnabled && profile.Strength > 0 ? effect.EffectivePixelSize : 1;
        }

        private void PrepareCamera(Camera camera)
        {
            bool visible = false;
            foreach (var surface in surfaces)
                visible |= surface != null && surface.enabled && surface.gameObject.activeInHierarchy
                    && (camera.cullingMask & (1 << surface.gameObject.layer)) != 0;
            if (!visible)
            {
                if (buffers.TryGetValue(camera, out var previous)) previous.Buffer.Clear();
                return;
            }
            if (!buffers.TryGetValue(camera, out var state))
            {
                state = new CameraOutline();
                buffers.Add(camera, state);
                camera.AddCommandBuffer(RenderEvent, state.Buffer);
            }
            var buffer = state.Buffer;
            buffer.Clear();
            int pixelSize = ActivePixelSize(camera);
            material.SetFloat(WidthId, pixelSize > 1 ? Mathf.Max(3, pixelSize * 2) : 3);
            buffer.SetGlobalFloat(HasExteriorId, 0);
            if (exterior != null)
            {
                int width = Mathf.Max(1, camera.pixelWidth), height = Mathf.Max(1, camera.pixelHeight);
                state.Resize(width, height);
                // Render beyond the visible viewport as needed. A cropped headboard hole
                // must not accidentally connect to the exterior at the screen edge.
                buffer.SetGlobalFloat(MaskScaleId, MaskScale(camera));
                buffer.SetRenderTarget(state.Mask);
                buffer.ClearRenderTarget(false, true, Color.black);
                DrawSurfaces(buffer, camera, 3);
                int initialize = exterior.FindKernel("Initialize"), connect = exterior.FindKernel("Connect"),
                    resolve = exterior.FindKernel("Exterior");
                buffer.SetComputeIntParam(exterior, "_Width", width);
                buffer.SetComputeIntParam(exterior, "_Height", height);
                foreach (int kernel in new[] { initialize, connect, resolve })
                    buffer.SetComputeBufferParam(exterior, kernel, "_Parents", state.Parents);
                buffer.SetComputeTextureParam(exterior, initialize, "_Silhouette", state.Mask);
                buffer.SetComputeTextureParam(exterior, resolve, "_Exterior", state.Exterior);
                buffer.DispatchCompute(exterior, initialize, (width + 7) / 8, (height + 7) / 8, 1);
                buffer.DispatchCompute(exterior, connect, (width + 7) / 8, (height + 7) / 8, 1);
                buffer.DispatchCompute(exterior, resolve, (width + 7) / 8, (height + 7) / 8, 1);
                buffer.SetGlobalTexture(ExteriorId, state.Exterior);
                buffer.SetGlobalFloat(HasExteriorId, 1);
                buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            }
            // Mask the union first. Screen-space translations dilate concave silhouettes
            // uniformly, including separate bed frame, bedding and pillow meshes.
            DrawSurfaces(buffer, camera, 0);
            for (int sample = 0; sample < 16; sample++)
            {
                float angle = sample * Mathf.PI * 2 / 16;
                buffer.SetGlobalVector(OffsetId, new Vector4(Mathf.Cos(angle), Mathf.Sin(angle), 0, 0));
                DrawSurfaces(buffer, camera, 1);
            }
            DrawSurfaces(buffer, camera, 2);
        }

        private float MaskScale(Camera camera)
        {
            float extent = 1;
            foreach (var surface in surfaces)
            {
                if (surface == null || !surface.enabled || !surface.gameObject.activeInHierarchy) continue;
                var bounds = surface.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var point = camera.WorldToViewportPoint(bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));
                    if (point.z <= camera.nearClipPlane) continue;
                    extent = Mathf.Max(extent, Mathf.Max(Mathf.Abs(point.x - .5f) * 2 + .02f,
                        Mathf.Abs(point.y - .5f) * 2 + .02f));
                }
            }
            return 1 / Mathf.Clamp(Mathf.Ceil(extent * 4) / 4, 1, 8);
        }

        private void DrawSurfaces(CommandBuffer buffer, Camera camera, int pass)
        {
            foreach (var surface in surfaces)
            {
                if (surface == null || !surface.enabled || !surface.gameObject.activeInHierarchy
                    || (camera.cullingMask & (1 << surface.gameObject.layer)) == 0) continue;
                var filter = surface.GetComponent<MeshFilter>();
                var mesh = surface is SkinnedMeshRenderer skinned ? skinned.sharedMesh : filter != null ? filter.sharedMesh : null;
                if (mesh == null) continue;
                for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                    buffer.DrawRenderer(surface, material, submesh, pass);
            }
        }

        public void Dispose()
        {
            Camera.onPreCull -= PrepareCamera;
            foreach (var pair in buffers)
            {
                if (pair.Key != null) pair.Key.RemoveCommandBuffer(RenderEvent, pair.Value.Buffer);
                pair.Value.Dispose();
            }
            buffers.Clear();
            DestroyResource(material);
            if (exterior != null) DestroyResource(exterior);
        }

        private static void DestroyResource(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
