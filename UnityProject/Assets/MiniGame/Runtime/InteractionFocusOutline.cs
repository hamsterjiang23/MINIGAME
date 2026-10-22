using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Draws one focused mesh without replacing its materials or property blocks.</summary>
    internal sealed class InteractionFocusOutline : IDisposable
    {
        private const CameraEvent RenderEvent = CameraEvent.BeforeForwardAlpha;
        private static readonly int CenterId = Shader.PropertyToID("_FocusOutlineCenter");
        private static readonly int WidthId = Shader.PropertyToID("_FocusOutlineWidth");
        private readonly Renderer surface;
        private readonly Mesh mesh;
        private readonly Material material;
        private readonly Dictionary<Camera, CommandBuffer> buffers = new Dictionary<Camera, CommandBuffer>();

        private InteractionFocusOutline(Renderer surface, Mesh mesh, Shader shader)
        {
            this.surface = surface;
            this.mesh = mesh;
            material = new Material(shader) { name = "White interaction silhouette", hideFlags = HideFlags.HideAndDontSave };
            material.SetVector(CenterId, mesh.bounds.center);
            Camera.onPreCull += PrepareCamera;
        }

        public static InteractionFocusOutline Create(Renderer surface)
        {
            if (surface == null) return null;
            var filter = surface.GetComponent<MeshFilter>();
            var mesh = surface is SkinnedMeshRenderer skinned ? skinned.sharedMesh : filter != null ? filter.sharedMesh : null;
            var shader = Resources.Load<Shader>("InteractionFocusOutline");
            return mesh != null && shader != null && shader.isSupported
                ? new InteractionFocusOutline(surface, mesh, shader) : null;
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
            if (!buffers.TryGetValue(camera, out var buffer))
            {
                buffer = new CommandBuffer { name = "White interaction silhouette" };
                buffers.Add(camera, buffer);
                camera.AddCommandBuffer(RenderEvent, buffer);
            }
            buffer.Clear();
            if (surface == null || mesh == null || !surface.enabled || !surface.gameObject.activeInHierarchy
                || (camera.cullingMask & (1 << surface.gameObject.layer)) == 0) return;

            int pixelSize = ActivePixelSize(camera);
            material.SetFloat(WidthId, pixelSize > 1 ? Mathf.Max(3, pixelSize * 2) : 3);

            // Mask every submesh before drawing the outline, so material seams stay invisible.
            // The final pass releases the temporary stencil bit before transparent/UI rendering.
            for (int pass = 0; pass < 3; pass++)
                for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                    buffer.DrawRenderer(surface, material, submesh, pass);
        }

        public void Dispose()
        {
            Camera.onPreCull -= PrepareCamera;
            foreach (var pair in buffers)
            {
                if (pair.Key != null) pair.Key.RemoveCommandBuffer(RenderEvent, pair.Value);
                pair.Value.Release();
            }
            buffers.Clear();
            if (Application.isPlaying) UnityEngine.Object.Destroy(material);
            else UnityEngine.Object.DestroyImmediate(material);
        }
    }
}
