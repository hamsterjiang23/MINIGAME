using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>A fixed wall print of the head of a bed, backed by a real exterior.</summary>
    public sealed class DreamWallPortal : IDisposable
    {
        public const int ExteriorLayer = 25;
        public const int HeadCaptureLayer = 29;
        private readonly List<Material> materials = new List<Material>();
        private readonly GameObject props, exterior;
        private readonly Camera viewer;
        private readonly Skybox skybox;
        private readonly bool createdSkybox, originalSkyEnabled;
        private readonly Material originalSky;
        private readonly DreamSequenceProfile profile;
        private readonly List<Material> exteriorMaterials = new List<Material>();
        private bool disposed;
        public Plane Wall { get; }
        public Vector3 WallNormal => Wall.normal;
        public Vector3 PrintedScreenCenter { get; }
        public float PrintedScreenHeight { get; }
        public Vector3 PrintedScreenNormal { get; }
        public bool IsOutside => Wall.GetDistanceToPoint(viewer.transform.position) > 0;
        public GameObject Exterior => exterior;
        public GameObject PrintedProps => props;
        public RenderTexture HeadTexture { get; private set; }
        public Vector3 HeadProjectionOrigin { get; }

        public DreamWallPortal(DreamSequenceProfile settings, Camera camera, Transform parent,
            Bounds roomBounds, Transform bed, Renderer[] bedParts, Vector3 origin)
        {
            profile = settings;
            viewer = camera;
            try
            {
                if (settings.CubemapSourcePrefab == null)
                    throw new InvalidOperationException("海滩全景缺少捕获源预制体，请在梦境与结局配置中指定。");
                var direction = Vector3.ProjectOnPlane(bed.forward, Vector3.up).normalized;
                var normal = Mathf.Abs(direction.x) > Mathf.Abs(direction.z)
                    ? Vector3.right * Mathf.Sign(direction.x) : Vector3.forward * Mathf.Sign(direction.z);
                var wallPoint = roomBounds.center + Vector3.Scale(normal, roomBounds.extents);
                Wall = new Plane(normal, wallPoint);
                // The bed pivot is its lengthwise midpoint: half of the real bed crosses the wall.
                bed.position -= normal * Wall.GetDistanceToPoint(bed.position);
                HeadProjectionOrigin = new Vector3(bed.position.x, origin.y, bed.position.z) - normal * 1.8f;

                props = new GameObject("Wall print - pillow half only");
                props.transform.SetParent(parent, false);
                foreach (var part in bedParts) CopyHead(part);
                // This monitor already exists in the authored panorama. Only locate its pixels on the wall.
                var monitorDirection = Quaternion.Euler(0, settings.CubemapYaw, 0) * settings.WallMonitorDirection.normalized;
                if (monitorDirection.sqrMagnitude < .5f) throw new InvalidOperationException("墙面原显示器方向无效。");
                PrintedScreenCenter = ProjectToWall(roomBounds, origin, origin + monitorDirection);
                var nearestFace = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back }
                    .OrderBy(n => Mathf.Abs(Vector3.Dot(n, PrintedScreenCenter - roomBounds.center)
                        - Vector3.Dot(Vector3.Scale(n, n), roomBounds.extents))).First();
                PrintedScreenNormal = nearestFace;
                var right = Vector3.Cross(Vector3.up, monitorDirection).normalized;
                float halfHeight = Mathf.Clamp(settings.WallMonitorHeightDegrees, .5f, 15) * .5f;
                var top = ProjectToWall(roomBounds, origin, origin + Quaternion.AngleAxis(-halfHeight, right) * monitorDirection);
                var bottom = ProjectToWall(roomBounds, origin, origin + Quaternion.AngleAxis(halfHeight, right) * monitorDirection);
                PrintedScreenHeight = Vector3.Distance(top, bottom);

                exterior = UnityEngine.Object.Instantiate(settings.CubemapSourcePrefab, parent, false);
                exterior.name = "Real beach beyond the printed wall";
                var source = exterior.GetComponent<DreamCubemapSource>();
                if (source == null || source.StandingPose == null) throw new InvalidOperationException("海滩缺少参考眼位。");
                exterior.transform.rotation = Quaternion.Euler(0, settings.CubemapYaw, 0);
                exterior.transform.position += origin - source.StandingPose.position;
                foreach (var t in exterior.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ExteriorLayer;
                foreach (var c in exterior.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                foreach (var light in exterior.GetComponentsInChildren<Light>(true)) light.cullingMask = 1 << ExteriorLayer;
                foreach (var r in exterior.GetComponentsInChildren<Renderer>(true))
                {
                    var copies = r.sharedMaterials.Select(m => m == null ? null : new Material(m) { hideFlags = HideFlags.DontSave }).ToArray();
                    exteriorMaterials.AddRange(copies.Where(m => m != null));
                    r.sharedMaterials = copies;
                }
                skybox = viewer.GetComponent<Skybox>();
                createdSkybox = skybox == null;
                if (createdSkybox) skybox = viewer.gameObject.AddComponent<Skybox>();
                originalSky = skybox.material; originalSkyEnabled = skybox.enabled;
                skybox.material = settings.RoomPanoramaSkybox != null ? settings.RoomPanoramaSkybox : source.SkyMaterial;
                skybox.enabled = true;
                viewer.clearFlags = CameraClearFlags.Skybox;
            }
            catch { Dispose(); throw; }
        }

        private void CopyHead(Renderer original)
        {
            var mesh = original.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null) return;
            var copy = new GameObject(original.name + " - printed head", typeof(MeshFilter), typeof(MeshRenderer));
            copy.layer = HeadCaptureLayer;
            copy.transform.SetParent(props.transform, false);
            copy.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
            copy.transform.localScale = original.transform.lossyScale;
            copy.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = copy.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.sharedMaterials = original.sharedMaterials.Select(m =>
            {
                if (m == null) return null;
                var material = new Material(m) { hideFlags = HideFlags.DontSave };
                materials.Add(material);
                if (!material.HasProperty("_DreamClipEnabled"))
                    throw new InvalidOperationException("床材质不支持墙面半床裁剪。");
                material.SetFloat("_DreamClipEnabled", 1);
                material.SetVector("_DreamClipPlane", new Vector4(Wall.normal.x, Wall.normal.y, Wall.normal.z, Wall.distance));
                return material;
            }).ToArray();
        }

        public static Vector3 ProjectToWall(Bounds bounds, Vector3 origin, Vector3 point)
        {
            var direction = point - origin;
            float distance = float.PositiveInfinity;
            for (int axis = 0; axis < 3; axis++)
            {
                if (Mathf.Abs(direction[axis]) < .00001f) continue;
                float hit = ((direction[axis] > 0 ? bounds.max[axis] : bounds.min[axis]) - origin[axis]) / direction[axis];
                if (hit > 0) distance = Mathf.Min(distance, hit);
            }
            if (float.IsInfinity(distance)) throw new InvalidOperationException("显示器的墙面投影方向无效。");
            return origin + direction * distance;
        }

        public void CaptureHeadWallpaper()
        {
            if (HeadTexture != null) return;
            HeadTexture = new RenderTexture(Mathf.Clamp(profile.CubemapFaceSize, 256, 1024),
                Mathf.Clamp(profile.CubemapFaceSize, 256, 1024), 24, RenderTextureFormat.ARGB32)
            {
                dimension = TextureDimension.Cube, name = "Pillow half wall print", hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            if (!HeadTexture.Create()) throw new InvalidOperationException("无法创建半床墙面贴图。");
            var captureObject = new GameObject("Pillow half capture", typeof(Camera));
            captureObject.transform.SetParent(props.transform, false);
            captureObject.transform.position = HeadProjectionOrigin;
            var camera = captureObject.GetComponent<Camera>();
            camera.enabled = false; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear;
            camera.cullingMask = 1 << HeadCaptureLayer; camera.nearClipPlane = .02f; camera.farClipPlane = 30;
            camera.allowHDR = false; camera.allowMSAA = false;
            var previous = RenderTexture.active;
            try
            {
                if (!camera.RenderToCubemap(HeadTexture, 63)) throw new InvalidOperationException("半床墙面捕获失败。");
            }
            finally { RenderTexture.active = previous; Destroy(captureObject); }
        }

        public void UpdateView(float time, float exteriorReveal = 0)
        {
            // Render the exterior behind the dissolving wall before crossing, avoiding a one-frame scene switch.
            viewer.cullingMask = (1 << 26) | (IsOutside || exteriorReveal > 0 ? 1 << ExteriorLayer : 0);
            viewer.clearFlags = CameraClearFlags.Skybox;
            foreach (var material in exteriorMaterials)
            {
                Set(material, "_UseDreamTime", 1); Set(material, "_DreamTime", time);
                Set(material, "_WaveHeight", profile.SeaWaveHeight); Set(material, "_WavePeriod", profile.SeaWavePeriod);
                Set(material, "_WaveLength", profile.SeaWaveLength); Set(material, "_FoamAmount", profile.SeaFoamAmount);
            }
        }
        private static void Set(Material material, string name, float value) { if (material.HasProperty(name)) material.SetFloat(name, value); }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (skybox != null)
            {
                skybox.material = originalSky; skybox.enabled = originalSkyEnabled;
                if (createdSkybox) Destroy(skybox);
            }
            if (props != null) props.SetActive(false);
            if (exterior != null) exterior.SetActive(false);
            Destroy(props); Destroy(exterior);
            if (HeadTexture != null) { HeadTexture.Release(); Destroy(HeadTexture); }
            foreach (var material in materials.Concat(exteriorMaterials)) Destroy(material);
        }
        private static void Destroy(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
