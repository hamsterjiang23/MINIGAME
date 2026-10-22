using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Real legacy sea geometry, captured at a fixed eye and clipped to the authored panorama's water.</summary>
    public sealed class DreamPanoramaWater : IDisposable
    {
        private readonly DreamSequenceProfile profile;
        private readonly Vector3 captureOrigin;
        private readonly GameObject surface;
        private int previousSettings;
        private bool haveSettings;
        public Material Material { get; }
        public bool Enabled => profile.AnimateRoomWater && profile.PanoramaWaterMask != null;

        private static Renderer LegacySea(DreamSequenceProfile settings)
        {
            var source = settings.CubemapSourcePrefab != null
                ? settings.CubemapSourcePrefab.GetComponent<DreamCubemapSource>() : null;
            if (source?.WaterSurfaces == null) return null;
            foreach (var candidate in source.WaterSurfaces)
                if (candidate != null && candidate.sharedMaterial != null && candidate.sharedMaterial.shader != null
                    && candidate.sharedMaterial.shader.name == "MiniGame/Dream Sea"
                    && candidate.GetComponent<MeshFilter>()?.sharedMesh != null) return candidate;
            return null;
        }

        public static bool CanCreate(DreamSequenceProfile settings) => LegacySea(settings) != null;

        public DreamPanoramaWater(DreamSequenceProfile settings, Transform parent, Vector3 origin)
        {
            profile = settings;
            captureOrigin = origin;
            var sea = LegacySea(settings);
            var mesh = sea != null ? sea.GetComponent<MeshFilter>()?.sharedMesh : null;
            if (mesh == null) throw new InvalidOperationException("动态全景海浪缺少旧 Dream Sea 网格和材质，请更新海滩捕获源。");
            Material = new Material(sea.sharedMaterial) { name = "Panorama legacy sea runtime", hideFlags = HideFlags.HideAndDontSave };
            surface = new GameObject("Masked legacy Dream Sea", typeof(MeshFilter), typeof(MeshRenderer))
                { layer = DreamCubemapRenderer.StageLayer, hideFlags = HideFlags.HideAndDontSave };
            surface.transform.SetParent(parent, false);
            surface.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = surface.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Apply(0);
        }

        public bool Apply(float time)
        {
            int hash;
            unchecked
            {
                hash = Enabled.GetHashCode();
                foreach (float value in new[] { profile.SeaWaveHeight, profile.SeaWavePeriod, profile.SeaWaveLength,
                    profile.ShoreWashDistance, profile.SeaFoamAmount, profile.PanoramaSeaLevel, profile.PanoramaShoreDistance })
                    hash = hash * 31 + value.GetHashCode();
                hash = hash * 31 + (profile.PanoramaWaterMask != null ? profile.PanoramaWaterMask.GetInstanceID() : 0);
            }
            bool changed = !haveSettings || hash != previousSettings;
            previousSettings = hash; haveSettings = true;
            surface.SetActive(Enabled);
            var origin = captureOrigin + new Vector3(0, Safe(profile.PanoramaSeaLevel, -6.5f, -30, -.1f) + .03f,
                Safe(profile.PanoramaShoreDistance, 12, 0, 60));
            surface.transform.position = origin;
            Material.SetVector("_DreamOrigin", origin);
            Material.SetVector("_CaptureOrigin", captureOrigin);
            Material.SetTexture("_PanoramaWaterMask", profile.PanoramaWaterMask);
            Material.SetFloat("_UsePanoramaMask", 1);
            Material.SetFloat("_UseDreamTime", 1);
            Material.SetFloat("_DreamTime", time);
            Material.SetFloat("_WaveHeight", Safe(profile.SeaWaveHeight, .24f, 0, .6f));
            Material.SetFloat("_WavePeriod", Safe(profile.SeaWavePeriod, 6, 2, 15));
            Material.SetFloat("_WaveLength", Safe(profile.SeaWaveLength, 7, 3, 20));
            Material.SetFloat("_ShoreWash", Safe(profile.ShoreWashDistance, 1.8f, 0, 4));
            Material.SetFloat("_FoamAmount", Safe(profile.SeaFoamAmount, .85f, 0, 1.5f));
            return changed;
        }

        private static float Safe(float value, float fallback, float min, float max)
            => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);

        public void Dispose()
        {
            if (surface != null) surface.SetActive(false);
            if (Application.isPlaying) { UnityEngine.Object.Destroy(surface); UnityEngine.Object.Destroy(Material); }
            else { UnityEngine.Object.DestroyImmediate(surface); UnityEngine.Object.DestroyImmediate(Material); }
        }
    }
}
