using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Captures all six beach faces before publishing a new panorama to one viewer.</summary>
    public sealed class DreamCubemapRenderer : IDisposable
    {
        public const int StageLayer = 28;
        private const int StageMask = 1 << StageLayer;
        private static int nextStage;
        private readonly DreamSequenceProfile profile;
        private readonly Camera viewer;
        private readonly CameraClearFlags previousClearFlags;
        private readonly int previousCullingMask;
        private readonly List<Material> materials = new List<Material>();
        private readonly HashSet<Light> sourceLights = new HashSet<Light>();
        private readonly List<Light> maskedLights = new List<Light>();
        private readonly List<int> previousLightMasks = new List<int>();
        private readonly Stopwatch timer = new Stopwatch();
        private GameObject root;
        private DreamCubemapSource source;
        private Camera captureCamera;
        private Material sourceSky, viewerMaterial, previousSkyMaterial;
        private Skybox viewerSkybox;
        private bool createdSkybox, previousSkyEnabled, disposed;
        private RenderTexture front, back;
        private float nextCaptureTime, lastWorldTime;

        public bool IsReady { get; private set; }
        public RenderTexture Texture => front;
        public int CapturedFrames { get; private set; }
        /// <summary>CPU duration of the synchronous six-face render call; not a GPU timing query.</summary>
        public float LastCaptureMilliseconds { get; private set; }
        public Camera CaptureCamera => captureCamera;
        public GameObject SourceRoot => root;

        /// <summary>Photographs a selected skybox at the doorway without including room geometry.</summary>
        public DreamCubemapRenderer(DreamSequenceProfile settings, Camera viewCamera, Material skybox, Vector3 captureOrigin)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (viewCamera == null) throw new ArgumentNullException(nameof(viewCamera));
            if (skybox == null || skybox.shader == null || !skybox.shader.isSupported)
                throw new InvalidOperationException("门口固定天空盒材质不可用。");
            profile = settings;
            viewer = viewCamera;
            previousClearFlags = viewer.clearFlags;
            previousCullingMask = viewer.cullingMask;
            try
            {
                root = new GameObject("Doorway fixed skybox capture") { hideFlags = HideFlags.HideAndDontSave };
                root.transform.position = captureOrigin;
                sourceSky = new Material(skybox) { name = "Doorway capture sky", hideFlags = HideFlags.HideAndDontSave };
                captureCamera = root.AddComponent<Camera>();
                captureCamera.enabled = false;
                captureCamera.cullingMask = 0;
                captureCamera.clearFlags = CameraClearFlags.Skybox;
                captureCamera.nearClipPlane = .03f;
                captureCamera.farClipPlane = 2500;
                captureCamera.allowHDR = false;
                captureCamera.allowMSAA = false;
                captureCamera.useOcclusionCulling = false;
                root.AddComponent<Skybox>().material = sourceSky;
                int size = Mathf.ClosestPowerOfTwo(Mathf.Clamp(profile.CubemapFaceSize, 256, 2048));
                front = CreateTexture(size, "Doorway fixed panorama front");
                back = CreateTexture(size, "Doorway fixed panorama back");
            }
            catch { Dispose(); throw; }
        }

        public DreamCubemapRenderer(DreamSequenceProfile settings, Camera viewCamera, bool useSkybox = true)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (viewCamera == null) throw new ArgumentNullException(nameof(viewCamera));
            profile = settings;
            viewer = viewCamera;
            previousClearFlags = viewer.clearFlags;
            previousCullingMask = viewer.cullingMask;
            ValidateSource(profile.CubemapSourcePrefab);
            var shader = Resources.Load<Shader>("DreamDynamicCubemap");
            if (shader == null || !shader.isSupported)
                throw new InvalidOperationException("DreamDynamicCubemap shader is unavailable on this graphics device.");
            try
            {
                root = new GameObject("Isolated beach cubemap source")
                { hideFlags = HideFlags.HideAndDontSave, layer = StageLayer };
                root.SetActive(false);
                root.transform.position = new Vector3(10000 + ++nextStage * 4000, 10000, 10000);
                var instance = UnityEngine.Object.Instantiate(profile.CubemapSourcePrefab, root.transform, false);
                source = instance.GetComponent<DreamCubemapSource>();
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = StageLayer;
                    child.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                foreach (var light in instance.GetComponentsInChildren<Light>(true))
                {
                    light.cullingMask = StageMask;
                    sourceLights.Add(light);
                }
                CloneMaterials(instance);
                sourceSky = new Material(source.SkyMaterial)
                { name = "Beach capture sky", hideFlags = HideFlags.HideAndDontSave };
                var cameraObject = new GameObject("Beach cubemap capture camera")
                { hideFlags = HideFlags.HideAndDontSave, layer = StageLayer };
                cameraObject.transform.SetParent(root.transform, false);
                captureCamera = cameraObject.AddComponent<Camera>();
                captureCamera.enabled = false;
                captureCamera.cullingMask = StageMask;
                captureCamera.clearFlags = CameraClearFlags.Skybox;
                captureCamera.nearClipPlane = .03f;
                captureCamera.farClipPlane = 2500;
                captureCamera.allowHDR = false;
                captureCamera.allowMSAA = false;
                captureCamera.useOcclusionCulling = false;
                cameraObject.AddComponent<Skybox>().material = sourceSky;
                int size = Mathf.ClosestPowerOfTwo(Mathf.Clamp(profile.CubemapFaceSize, 256, 2048));
                front = CreateTexture(size, "Beach panorama front");
                back = CreateTexture(size, "Beach panorama back");
                if (useSkybox)
                {
                    viewerMaterial = new Material(shader)
                    { name = "Room beach panorama", hideFlags = HideFlags.HideAndDontSave };
                    viewerSkybox = viewer.GetComponent<Skybox>();
                    createdSkybox = viewerSkybox == null;
                    if (createdSkybox)
                    {
                        viewerSkybox = viewer.gameObject.AddComponent<Skybox>();
                        viewerSkybox.hideFlags = HideFlags.HideAndDontSave;
                    }
                    previousSkyMaterial = viewerSkybox.material;
                    previousSkyEnabled = viewerSkybox.enabled;
                }
                viewer.cullingMask &= ~StageMask;
                instance.SetActive(true);
                root.SetActive(true);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private static void ValidateSource(GameObject prefab)
        {
            if (prefab == null) throw new InvalidOperationException("海滩全景缺少捕获源预制体，请在梦境与结局配置中指定。");
            var value = prefab.GetComponent<DreamCubemapSource>();
            if (value == null || value.StandingPose == null || value.PillowPose == null || value.SkyMaterial == null)
                throw new InvalidOperationException("海滩捕获源必须包含 DreamCubemapSource、站立定位、枕头定位和天空材质。");
            if (!value.StandingPose.IsChildOf(prefab.transform) || !value.PillowPose.IsChildOf(prefab.transform))
                throw new InvalidOperationException("海滩捕获定位点必须属于捕获源预制体。");
            if (value.SkyMaterial.shader == null || !value.SkyMaterial.shader.isSupported)
                throw new InvalidOperationException("海滩捕获源天空 Shader 不可用。");
            if (prefab.GetComponentsInChildren<Renderer>(true).Length == 0)
                throw new InvalidOperationException("海滩捕获源没有海岸或海面渲染器。");
            foreach (var behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour == null || !(behaviour is DreamCubemapSource))
                    throw new InvalidOperationException("海滩捕获源只能包含 DreamCubemapSource 脚本，不能包含游戏会话或演出控制器。");
            if (prefab.GetComponentInChildren<Camera>(true) != null
                || prefab.GetComponentInChildren<AudioSource>(true) != null
                || prefab.GetComponentInChildren<AudioListener>(true) != null
                || prefab.GetComponentInChildren<Canvas>(true) != null)
                throw new InvalidOperationException("海滩捕获源不能包含相机、音频或 UI。");
            if (value.WaterSurfaces != null) foreach (var surface in value.WaterSurfaces)
                if (surface != null && !surface.transform.IsChildOf(prefab.transform))
                    throw new InvalidOperationException("海滩捕获源海面引用不能指向预制体之外。");
            if (value.Lights != null) foreach (var light in value.Lights)
                if (light != null && !light.transform.IsChildOf(prefab.transform))
                    throw new InvalidOperationException("海滩捕获源灯光引用不能指向预制体之外。");
        }

        private void CloneMaterials(GameObject instance)
        {
            var clones = new Dictionary<Material, Material>();
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var values = renderer.sharedMaterials;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == null) continue;
                    if (!clones.TryGetValue(values[i], out var clone))
                    {
                        clone = new Material(values[i])
                        { name = values[i].name + " (Beach capture)", hideFlags = HideFlags.HideAndDontSave };
                        clones.Add(values[i], clone);
                        materials.Add(clone);
                    }
                    values[i] = clone;
                }
                renderer.sharedMaterials = values;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        private static RenderTexture CreateTexture(int size, string name)
        {
            var texture = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32)
            {
                name = name, hideFlags = HideFlags.HideAndDontSave, dimension = TextureDimension.Cube,
                filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp,
                useMipMap = false, autoGenerateMips = false, antiAliasing = 1
            };
            if (!texture.Create())
            {
                DestroyOwned(texture);
                throw new InvalidOperationException("无法创建海滩 Cubemap RenderTexture，请降低面分辨率。");
            }
            return texture;
        }

        public void Render(float worldTime, BeachEndingPhase phase, float phaseProgress, float driftElapsed,
            bool force = false, Vector3 explorationOffset = default)
        {
            if (disposed) throw new ObjectDisposedException(nameof(DreamCubemapRenderer));
            float time = Safe(worldTime, 0, 0);
            bool clockRewound = IsReady && time < lastWorldTime;
            var position = CapturePosition(phase, phaseProgress, driftElapsed, explorationOffset);
            bool positionChanged = position != captureCamera.transform.position;
            // A translated viewer needs a fresh view of the nearby ground in the same
            // frame as the physical bed. Only stationary animation updates are throttled.
            if (!force && IsReady && !clockRewound && !positionChanged && time + .0001f < nextCaptureTime) return;
            ApplyParameters(time);
            captureCamera.transform.SetPositionAndRotation(position, Quaternion.identity);
            var previousActive = RenderTexture.active;
            var lighting = new CaptureLighting();
            bool rendered;
            timer.Restart();
            try
            {
                if (source != null)
                {
                    ExcludeExternalLights();
                    lighting.Apply(source, sourceSky, sourceLights);
                }
                // One synchronous call renders every face with the same explicit animation time.
                rendered = captureCamera.RenderToCubemap(back, 63);
            }
            finally
            {
                RenderTexture.active = previousActive;
                lighting.Restore();
                RestoreExternalLights();
                timer.Stop();
                LastCaptureMilliseconds = (float)timer.Elapsed.TotalMilliseconds;
            }
            if (!rendered) throw new InvalidOperationException("海滩 Cubemap 六面捕获失败，保留上一幅完整全景。");
            var previous = front;
            front = back;
            back = previous;
            if (viewerMaterial != null)
            {
                viewerMaterial.SetTexture("_Tex", front);
                viewerMaterial.SetFloat("_Yaw", Finite(profile.CubemapYaw) ? profile.CubemapYaw : 0);
            }
            if (viewer != null && viewerSkybox != null)
            {
                viewerSkybox.material = viewerMaterial;
                viewerSkybox.enabled = true;
                viewer.clearFlags = CameraClearFlags.Skybox;
            }
            CapturedFrames++;
            IsReady = true;
            lastWorldTime = time;
            nextCaptureTime = time + 1 / Mathf.Clamp(Safe(profile.CubemapRefreshRate, 10, 1), 1, 30);
        }

        private void ApplyParameters(float time)
        {
            foreach (var material in materials)
            {
                SetFloat(material, "_UseDreamTime", 1);
                SetFloat(material, "_DreamTime", time);
                SetFloat(material, "_WaveHeight", Mathf.Clamp(Safe(profile.SeaWaveHeight, .24f, 0), 0, .6f));
                SetFloat(material, "_WavePeriod", Mathf.Clamp(Safe(profile.SeaWavePeriod, 6, 2), 2, 15));
                SetFloat(material, "_WaveLength", Mathf.Clamp(Safe(profile.SeaWaveLength, 7, 3), 3, 20));
                SetFloat(material, "_ShoreWash", Mathf.Clamp(Safe(profile.ShoreWashDistance, 1.8f, 0), 0, 4));
                SetFloat(material, "_FoamAmount", Mathf.Clamp(Safe(profile.SeaFoamAmount, .85f, 0), 0, 1.5f));
                if (material.HasProperty("_DreamOrigin")) material.SetVector("_DreamOrigin", root.transform.position);
            }
            SetFloat(sourceSky, "_DreamTime", time);
            SetFloat(sourceSky, "_CloudSpeed", Safe(profile.CloudSpeed, .006f, 0));
        }

        private Vector3 CapturePosition(BeachEndingPhase phase, float phaseProgress, float driftElapsed, Vector3 explorationOffset)
        {
            if (source == null) return captureCamera.transform.position;
            if (!Finite(explorationOffset.x) || !Finite(explorationOffset.y) || !Finite(explorationOffset.z))
                explorationOffset = Vector3.zero;
            // Match the yaw used by the panorama shader for both rays and displacement.
            float yaw = Finite(profile.CubemapYaw) ? profile.CubemapYaw : 0;
            var sourceOffset = Quaternion.Euler(0, -yaw, 0) * explorationOffset;
            Vector3 position = source.StandingPose.position + sourceOffset;
            if (phase == BeachEndingPhase.Lying)
                position = Vector3.Lerp(position, source.PillowPose.position,
                    Mathf.SmoothStep(0, 1, Mathf.Clamp01(Safe(phaseProgress, 0, 0))));
            else if (phase == BeachEndingPhase.Drifting)
            {
                float elapsed = Safe(driftElapsed, 0, 0);
                float wave = elapsed * Mathf.PI * 2 / Safe(profile.FloatPeriod, 8, .1f);
                float tilt = Safe(profile.FloatTiltDegrees, 1.5f, 0);
                var pivot = source.PillowPose.parent;
                var relativePillow = source.PillowPose.position - pivot.position;
                var worldTilt = pivot.rotation
                    * Quaternion.Euler(Mathf.Sin(wave * .7f) * tilt, 0, Mathf.Sin(wave) * tilt * .7f)
                    * Quaternion.Inverse(pivot.rotation);
                position = pivot.position + worldTilt * relativePillow
                    + Vector3.up * (Mathf.Sin(wave) * Safe(profile.FloatAmplitude, .08f, 0))
                    + Vector3.forward * (elapsed * Safe(profile.DriftSpeed, .04f, 0));
            }
            // Cubemap faces remain world-aligned. The viewer supplies look direction and bed tilt.
            return position;
        }

        private void ExcludeExternalLights()
        {
            foreach (var light in UnityEngine.Object.FindObjectsOfType<Light>())
            {
                if (sourceLights.Contains(light) || (light.cullingMask & StageMask) == 0) continue;
                maskedLights.Add(light);
                previousLightMasks.Add(light.cullingMask);
                light.cullingMask &= ~StageMask;
            }
        }

        private void RestoreExternalLights()
        {
            for (int i = 0; i < maskedLights.Count; i++)
                if (maskedLights[i] != null) maskedLights[i].cullingMask = previousLightMasks[i];
            maskedLights.Clear();
            previousLightMasks.Clear();
        }

        // Scope the authored beach lighting to capture, keeping the room's global settings intact.
        private sealed class CaptureLighting
        {
            private readonly bool fog = RenderSettings.fog;
            private readonly AmbientMode mode = RenderSettings.ambientMode;
            private readonly Color sky = RenderSettings.ambientSkyColor;
            private readonly Color equator = RenderSettings.ambientEquatorColor;
            private readonly Color ground = RenderSettings.ambientGroundColor;
            private readonly float intensity = RenderSettings.ambientIntensity;
            private readonly Material skybox = RenderSettings.skybox;
            private readonly Light sun = RenderSettings.sun;
            private readonly SphericalHarmonicsL2 probe = RenderSettings.ambientProbe;

            public void Apply(DreamCubemapSource value, Material material, HashSet<Light> lights)
            {
                RenderSettings.fog = false;
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = value.AmbientSkyColor;
                RenderSettings.ambientEquatorColor = value.AmbientEquatorColor;
                RenderSettings.ambientGroundColor = value.AmbientGroundColor;
                RenderSettings.ambientIntensity = Safe(value.AmbientIntensity, .75f, 0);
                RenderSettings.skybox = material;
                RenderSettings.sun = null;
                foreach (var light in lights)
                    if (light != null && light.enabled && light.type == LightType.Directional)
                    { RenderSettings.sun = light; break; }
            }

            public void Restore()
            {
                RenderSettings.fog = fog;
                RenderSettings.ambientMode = mode;
                RenderSettings.ambientSkyColor = sky;
                RenderSettings.ambientEquatorColor = equator;
                RenderSettings.ambientGroundColor = ground;
                RenderSettings.ambientIntensity = intensity;
                RenderSettings.skybox = skybox;
                RenderSettings.sun = sun;
                RenderSettings.ambientProbe = probe;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            IsReady = false;
            if (viewer != null)
            {
                viewer.clearFlags = previousClearFlags;
                viewer.cullingMask = previousCullingMask;
            }
            if (viewerSkybox != null)
            {
                viewerSkybox.material = previousSkyMaterial;
                viewerSkybox.enabled = previousSkyEnabled;
                if (createdSkybox) DestroyOwned(viewerSkybox);
            }
            if (root != null) root.SetActive(false);
            if (captureCamera != null) captureCamera.targetTexture = null;
            if (front != null) front.Release();
            if (back != null) back.Release();
            DestroyOwned(root);
            DestroyOwned(front);
            DestroyOwned(back);
            DestroyOwned(sourceSky);
            DestroyOwned(viewerMaterial);
            foreach (var material in materials) DestroyOwned(material);
            materials.Clear();
            front = back = null;
            root = null;
            source = null;
            captureCamera = null;
        }

        private static void SetFloat(Material material, string name, float value)
        { if (material.HasProperty(name)) material.SetFloat(name, value); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static float Safe(float value, float fallback, float minimum)
            => Finite(value) ? Mathf.Max(minimum, value) : fallback;
        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
