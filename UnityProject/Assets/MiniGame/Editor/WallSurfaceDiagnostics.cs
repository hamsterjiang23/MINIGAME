using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Batch-mode, unsaved scene diagnostics. It never enters Play Mode or changes an asset.</summary>
    public static class WallSurfaceDiagnostics
    {
        private const int Width = 960, Height = 540;
        private const string DefaultScene = "Assets/MiniGame/Scenes/MainRoomCombined.unity";

        [Serializable] private sealed class Report
        {
            public string result, unity, project, sourceScene, outputFolder, graphics, colorSpace;
            public string sourceSceneSha256Before, sourceSceneSha256After;
            public string captureScope = "EditMode raw world Camera.Render; no Bootstrap, UI, gameplay or camera post-effects. Unlit/Texture uses its own depth/cull/queue pass. Surface-ID is opaque ZWrite On, ZTest LEqual, Cull Off with the scene geometry/depth unchanged. No scene or asset is saved.";
            public string surfaceIdLegend = "red: exact MainRoom_30/MainRoom_30_B material slots; blue: non-wallpaper Wall renderer slots; dark gray: other geometry";
            public string floorPath;
            public Bounds floorBounds;
            public Vector3 eye;
            public bool temporaryStatesRestored, materialsRestored, sourceSceneFileUnchanged, repairPreview;
            public RendererRecord[] renderers;
            public LightRecord[] lights;
            public string[] wallpaperTargets, wallShellTargets, warnings, failures;
            public CaptureRecord[] captures;
        }
        [Serializable] private sealed class RendererRecord
        {
            public string name, path, type, meshName, meshAsset, shadowCasting, sortingLayer, normalStatus;
            public Bounds bounds;
            public Vector3 localScale, lossyScale, averageWorldNormal, averageWorldNormalDirection;
            public bool enabled, activeInHierarchy, forceRenderingOff, receiveShadows, wallpaperTarget, wallShellTarget, meshReadable, normalsFinite;
            public int layer, sortingOrder, vertices, subMeshes, normalCount, nonFiniteNormals;
            public float transformDeterminant;
            public long[] subMeshIndexCounts;
            public string[] subMeshTopology;
            public MaterialRecord[] materials;
        }
        [Serializable] private sealed class MaterialRecord
        {
            public string name, asset, shader, renderType, mainTextureName, mainTextureAsset;
            public int slot, queue;
            public bool supported, hasMode, hasCull, hasZWrite, hasColor, slotPropertyBlockPresent;
            public float mode, cull, zWrite;
            public Color color;
            public Vector2 mainTextureScale, mainTextureOffset;
        }
        [Serializable] private sealed class LightRecord
        {
            public string path, type, shadows, cookie;
            public bool enabled, activeInHierarchy;
            public Vector3 position, rotation;
            public Color color;
            public float intensity, range, spotAngle, shadowStrength, shadowBias, shadowNormalBias;
            public int cullingMask;
        }
        [Serializable] private sealed class CaptureRecord
        {
            public string mode, view, file, result, error;
            public Vector3 position, forward, euler;
            public Quaternion rotation;
            public float verticalFieldOfView, aspect, nearClip, farClip;
            public int width, height;
        }
        private sealed class ViewPose
        {
            public string Name;
            public Vector3 Forward;
        }

        public static void CaptureRepairComparison()
        {
            string previousLabel = Environment.GetEnvironmentVariable("MINIGAME_WALL_CAPTURE_LABEL");
            string previousPreview = Environment.GetEnvironmentVariable("MINIGAME_WALL_REPAIR_PREVIEW");
            try
            {
                Environment.SetEnvironmentVariable("MINIGAME_WALL_CAPTURE_LABEL", "geometry-before");
                Environment.SetEnvironmentVariable("MINIGAME_WALL_REPAIR_PREVIEW", "0");
                Capture();
                Environment.SetEnvironmentVariable("MINIGAME_WALL_CAPTURE_LABEL", "geometry-preview");
                Environment.SetEnvironmentVariable("MINIGAME_WALL_REPAIR_PREVIEW", "1");
                Capture();
            }
            finally
            {
                Environment.SetEnvironmentVariable("MINIGAME_WALL_CAPTURE_LABEL", previousLabel);
                Environment.SetEnvironmentVariable("MINIGAME_WALL_REPAIR_PREVIEW", previousPreview);
            }
        }

        public static void Capture()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("WallSurfaceDiagnostics requires EditMode batch execution.");
            string root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (string.IsNullOrWhiteSpace(root)) throw new InvalidOperationException("Set MINIGAME_REPOSITORY_ROOT.");
            string label = Environment.GetEnvironmentVariable("MINIGAME_WALL_CAPTURE_LABEL");
            if (string.IsNullOrWhiteSpace(label)) label = "local";
            if (label == "." || label == ".." || label.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || label.Contains("/") || label.Contains("\\"))
                throw new InvalidOperationException("MINIGAME_WALL_CAPTURE_LABEL must be one directory name.");
            string folder = Path.Combine(Path.GetFullPath(root), "Artifacts", "WallLighting", label);
            Directory.CreateDirectory(folder);
            var report = new Report { result = "failed", unity = Application.unityVersion,
                project = Path.GetDirectoryName(Application.dataPath), outputFolder = folder,
                graphics = SystemInfo.graphicsDeviceType.ToString(), colorSpace = QualitySettings.activeColorSpace.ToString() };
            var captures = new List<CaptureRecord>();
            var failures = new List<string>();
            var warnings = new List<string>();
            Camera camera = null;
            Light[] lights = Array.Empty<Light>();
            LightShadows[] originalShadows = Array.Empty<LightShadows>();
            Renderer[] renderers = Array.Empty<Renderer>();
            Material[][] originalMaterials = Array.Empty<Material[]>();
            bool[] originalEnabled = Array.Empty<bool>();
            ShadowCastingMode[] originalCasting = Array.Empty<ShadowCastingMode>();
            try
            {
                string source = Environment.GetEnvironmentVariable("MINIGAME_WALL_SCENE");
                report.sourceScene = string.IsNullOrWhiteSpace(source) ? DefaultScene : source.Replace('\\', '/');
                report.sourceSceneSha256Before = FileHash(SceneFilePath(report.sourceScene));
                var scene = EditorSceneManager.OpenScene(report.sourceScene, OpenSceneMode.Single);
                report.repairPreview = Environment.GetEnvironmentVariable("MINIGAME_WALL_REPAIR_PREVIEW") == "1";
                if (report.repairPreview)
                {
                    var repair = WallSurfaceRepairSetup.Apply(scene, false);
                    File.WriteAllText(Path.Combine(folder, "repair-preview.json"), JsonUtility.ToJson(repair, true));
                }
                renderers = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Renderer>(true)).ToArray();
                originalMaterials = renderers.Select(item => item.sharedMaterials).ToArray();
                originalEnabled = renderers.Select(item => item.enabled).ToArray();
                originalCasting = renderers.Select(item => item.shadowCastingMode).ToArray();
                lights = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Light>(true)).ToArray();
                originalShadows = lights.Select(item => item.shadows).ToArray();
                report.lights = lights.Select(DescribeLight).ToArray();
                var floor = renderers.FirstOrDefault(item => item.name == "B_Floor")
                    ?? renderers.Where(item => item.name.IndexOf("floor", StringComparison.OrdinalIgnoreCase) >= 0
                        && item.name.IndexOf("rug", StringComparison.OrdinalIgnoreCase) < 0)
                        .OrderByDescending(item => item.bounds.size.x * item.bounds.size.z).FirstOrDefault();
                if (floor == null) throw new InvalidOperationException("No B_Floor or named floor renderer exists in the source scene.");
                report.floorPath = HierarchyPath(floor.transform); report.floorBounds = floor.bounds;
                report.eye = new Vector3(floor.bounds.center.x, floor.bounds.max.y + 1.65f, floor.bounds.center.z);
                var paper = renderers.Where(IsWallpaper).ToArray();
                var shells = renderers.Where(IsWallShell).ToArray();
                report.wallpaperTargets = paper.Select(item => HierarchyPath(item.transform)).ToArray();
                report.wallShellTargets = shells.Select(item => HierarchyPath(item.transform)).ToArray();
                if (paper.Length == 0) warnings.Add("No MainRoom_30 or MainRoom_30_B material target: wallpaper-no-cast has no affected renderer.");
                if (shells.Length == 0) warnings.Add("No non-wallpaper Wall renderer target: hide-wall-shell has no affected renderer.");
                report.renderers = renderers.Where(item => item == floor || IsSuspicious(item)).Select(DescribeRenderer).ToArray();

                var template = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Camera>(true))
                    .OrderByDescending(item => item.CompareTag("MainCamera")).FirstOrDefault();
                var temporary = new GameObject("Wall diagnostics camera") { hideFlags = HideFlags.HideAndDontSave };
                camera = temporary.AddComponent<Camera>();
                if (template != null) camera.CopyFrom(template);
                camera.enabled = false; camera.targetTexture = null;
                camera.usePhysicalProperties = false;
                camera.fieldOfView = 80; camera.aspect = (float)Width / Height;
                camera.nearClipPlane = .02f; camera.farClipPlane = Mathf.Max(40, camera.farClipPlane);
                camera.ResetProjectionMatrix(); camera.ResetWorldToCameraMatrix(); camera.ResetCullingMatrix();
                camera.transform.position = report.eye;
                var axes = new[] { new ViewPose { Name = "positive-x", Forward = Vector3.right },
                    new ViewPose { Name = "negative-x", Forward = Vector3.left },
                    new ViewPose { Name = "positive-z", Forward = Vector3.forward },
                    new ViewPose { Name = "negative-z", Forward = Vector3.back } };
                var views = new List<ViewPose>(axes);
                var bed = renderers.FirstOrDefault(item => item.name == "B_SM_mattress_1")
                    ?? renderers.FirstOrDefault(item => item.name.IndexOf("bed", StringComparison.OrdinalIgnoreCase) >= 0);
                var desk = renderers.FirstOrDefault(item => item.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal))
                    ?? renderers.FirstOrDefault(item => item.name.IndexOf("table", StringComparison.OrdinalIgnoreCase) >= 0);
                if (bed != null && desk != null)
                {
                    var direction = Vector3.ProjectOnPlane((bed.bounds.center + desk.bounds.center) * .5f - report.eye, Vector3.up);
                    if (direction.sqrMagnitude > .01f) views.Add(new ViewPose { Name = "desk-bed-rear", Forward = direction.normalized });
                    else warnings.Add("Desk/bed average is at the room centre; rear oblique omitted.");
                }
                else warnings.Add("Desk or bed renderer unavailable; rear oblique omitted.");

                CapturePass(camera, views, "lit", null, folder, captures, failures);
                foreach (var light in lights) light.shadows = LightShadows.None;
                try { CapturePass(camera, views, "shadows-none", null, folder, captures, failures); }
                finally { RestoreLights(lights, originalShadows); }
                var unlit = Shader.Find("Unlit/Texture");
                if (unlit == null || !unlit.isSupported) failures.Add("Unlit/Texture replacement shader is unavailable or unsupported.");
                else CapturePass(camera, views, "unlit-texture", unlit, folder, captures, failures);

                foreach (var shell in shells) shell.enabled = false;
                try { CapturePass(camera, axes, "hide-wall-shell", null, folder, captures, failures); }
                finally { RestoreRenderers(renderers, originalEnabled, originalCasting); }
                foreach (var wallpaper in paper) wallpaper.shadowCastingMode = ShadowCastingMode.Off;
                try { CapturePass(camera, axes, "wallpaper-no-cast", null, folder, captures, failures); }
                finally { RestoreRenderers(renderers, originalEnabled, originalCasting); }
                CaptureSurfaceIds(camera, views, renderers, originalMaterials, folder, captures, failures);
            }
            catch (Exception exception) { failures.Add(exception.ToString()); }
            finally
            {
                RestoreLights(lights, originalShadows);
                RestoreRenderers(renderers, originalEnabled, originalCasting);
                RestoreMaterials(renderers, originalMaterials);
                if (camera != null) { camera.ResetReplacementShader(); Object.DestroyImmediate(camera.gameObject); }
                report.materialsRestored = renderers.Select((item, index) => item == null || item.sharedMaterials.SequenceEqual(originalMaterials[index])).All(value => value);
                report.temporaryStatesRestored = lights.Select((item, index) => item == null || item.shadows == originalShadows[index]).All(value => value)
                    && renderers.Select((item, index) => item == null || item.enabled == originalEnabled[index]
                        && item.shadowCastingMode == originalCasting[index]).All(value => value) && report.materialsRestored;
                try
                {
                    report.sourceSceneSha256After = FileHash(SceneFilePath(report.sourceScene));
                    report.sourceSceneFileUnchanged = report.sourceSceneSha256Before == report.sourceSceneSha256After;
                    if (!report.sourceSceneFileUnchanged) failures.Add("Source scene bytes changed during diagnostics.");
                }
                catch (Exception exception) { failures.Add("Final source hash: " + exception.Message); }
                report.captures = captures.ToArray(); report.failures = failures.ToArray(); report.warnings = warnings.ToArray();
                report.result = failures.Count == 0 && report.temporaryStatesRestored ? "passed" : "failed";
                File.WriteAllText(Path.Combine(folder, "diagnostics.json"), JsonUtility.ToJson(report, true));
            }
            if (report.result != "passed") throw new InvalidOperationException("Wall diagnostics failed; see " + folder + "/diagnostics.json");
            Debug.Log("MINIGAME_WALL_DIAGNOSTICS_PASSED captures=" + captures.Count + " folder=" + folder);
        }

        private static void CaptureSurfaceIds(Camera camera, IEnumerable<ViewPose> poses, Renderer[] renderers, Material[][] originals,
            string folder, List<CaptureRecord> captures, List<string> failures)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/MiniGame/Editor/WallSurfaceDiagnostic.shader");
            if (shader == null || !shader.isSupported)
            { failures.Add("Editor WallSurfaceDiagnostic shader is unavailable or unsupported; surface-id skipped."); return; }
            var palette = new List<Material>();
            try
            {
                foreach (var color in new[] { Color.red, Color.blue, new Color(.06f, .06f, .06f, 1) })
                {
                    var material = new Material(shader) { name = "Temporary wall surface ID " + palette.Count,
                        hideFlags = HideFlags.HideAndDontSave };
                    // A dedicated property avoids the scene's unchanged _Color MaterialPropertyBlocks overriding the IDs.
                    material.SetColor("_DiagnosticColor", color); palette.Add(material);
                }
                for (int index = 0; index < renderers.Length; index++)
                {
                    var renderer = renderers[index];
                    if (renderer == null) continue;
                    bool shell = IsWallShell(renderer);
                    renderer.sharedMaterials = originals[index].Select(material =>
                        IsWallpaperMaterial(material) ? palette[0] : shell ? palette[1] : palette[2]).ToArray();
                }
                CapturePass(camera, poses, "surface-id", null, folder, captures, failures);
            }
            finally
            {
                RestoreMaterials(renderers, originals);
                foreach (var material in palette) if (material != null) Object.DestroyImmediate(material);
            }
        }

        private static void CapturePass(Camera camera, IEnumerable<ViewPose> poses, string mode, Shader replacement,
            string folder, List<CaptureRecord> captures, List<string> failures)
        {
            foreach (var pose in poses)
            {
                camera.transform.rotation = Quaternion.LookRotation(pose.Forward, Vector3.up);
                var record = new CaptureRecord { mode = mode, view = pose.Name, file = mode + "-" + pose.Name + ".png",
                    position = camera.transform.position, rotation = camera.transform.rotation, euler = camera.transform.eulerAngles,
                    forward = camera.transform.forward, verticalFieldOfView = camera.fieldOfView, aspect = camera.aspect,
                    nearClip = camera.nearClipPlane, farClip = camera.farClipPlane, width = Width, height = Height, result = "failed" };
                var active = RenderTexture.active;
                var previous = camera.targetTexture;
                RenderTexture target = null;
                Texture2D pixels = null;
                try
                {
                    target = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.ARGB32);
                    pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                    camera.targetTexture = target;
                    if (replacement == null) camera.Render(); else camera.RenderWithShader(replacement, "");
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false); pixels.Apply(false, false);
                    File.WriteAllBytes(Path.Combine(folder, record.file), pixels.EncodeToPNG()); record.result = "passed";
                }
                catch (Exception exception) { record.error = exception.ToString(); failures.Add(mode + "/" + pose.Name + ": " + exception.Message); }
                finally
                {
                    camera.targetTexture = previous; RenderTexture.active = active; camera.ResetReplacementShader();
                    if (target != null) RenderTexture.ReleaseTemporary(target);
                    if (pixels != null) Object.DestroyImmediate(pixels);
                }
                captures.Add(record);
            }
        }

        private static RendererRecord DescribeRenderer(Renderer renderer)
        {
            var mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            var data = new RendererRecord { name = renderer.name, path = HierarchyPath(renderer.transform), type = renderer.GetType().Name,
                bounds = renderer.bounds, enabled = renderer.enabled, activeInHierarchy = renderer.gameObject.activeInHierarchy,
                forceRenderingOff = renderer.forceRenderingOff, layer = renderer.gameObject.layer, sortingLayer = renderer.sortingLayerName,
                sortingOrder = renderer.sortingOrder, localScale = renderer.transform.localScale, lossyScale = renderer.transform.lossyScale,
                transformDeterminant = renderer.localToWorldMatrix.determinant, shadowCasting = renderer.shadowCastingMode.ToString(),
                receiveShadows = renderer.receiveShadows, wallpaperTarget = IsWallpaper(renderer), wallShellTarget = IsWallShell(renderer),
                materials = renderer.sharedMaterials.Select((material, index) => DescribeMaterial(renderer, material, index)).ToArray(),
                normalStatus = "no mesh" };
            if (mesh == null) return data;
            data.meshName = mesh.name; data.meshAsset = AssetDatabase.GetAssetPath(mesh);
            data.meshReadable = mesh.isReadable; data.vertices = mesh.vertexCount; data.subMeshes = mesh.subMeshCount;
            data.subMeshIndexCounts = Enumerable.Range(0, mesh.subMeshCount).Select(index => (long)mesh.GetIndexCount(index)).ToArray();
            data.subMeshTopology = Enumerable.Range(0, mesh.subMeshCount).Select(index => mesh.GetTopology(index).ToString()).ToArray();
            data.normalStatus = mesh.isReadable ? "readable" : "not CPU readable; importer unchanged";
            if (!mesh.isReadable) return data;
            var normals = mesh.normals; data.normalCount = normals.Length;
            var normalMatrix = renderer.localToWorldMatrix.inverse.transpose;
            Vector3 sum = Vector3.zero;
            foreach (var normal in normals)
            {
                var world = normalMatrix.MultiplyVector(normal);
                if (!Finite(normal) || !Finite(world)) { data.nonFiniteNormals++; continue; }
                sum += world.normalized;
            }
            data.normalsFinite = data.nonFiniteNormals == 0;
            int valid = normals.Length - data.nonFiniteNormals;
            data.averageWorldNormal = valid > 0 ? sum / valid : Vector3.zero;
            data.averageWorldNormalDirection = data.averageWorldNormal.normalized;
            if (normals.Length == 0) data.normalStatus = "readable but has no normals";
            return data;
        }

        private static MaterialRecord DescribeMaterial(Renderer renderer, Material material, int slot)
        {
            var data = new MaterialRecord { slot = slot, name = material == null ? "<null>" : material.name };
            if (material == null) return data;
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, slot);
            data.slotPropertyBlockPresent = !block.isEmpty;
            data.asset = AssetDatabase.GetAssetPath(material); data.shader = material.shader == null ? "<null>" : material.shader.name;
            data.supported = material.shader != null && material.shader.isSupported; data.queue = material.renderQueue;
            data.renderType = material.GetTag("RenderType", false, "");
            data.hasMode = material.HasProperty("_Mode"); if (data.hasMode) data.mode = material.GetFloat("_Mode");
            data.hasCull = material.HasProperty("_Cull"); if (data.hasCull) data.cull = material.GetFloat("_Cull");
            data.hasZWrite = material.HasProperty("_ZWrite"); if (data.hasZWrite) data.zWrite = material.GetFloat("_ZWrite");
            data.hasColor = material.HasProperty("_Color"); if (data.hasColor) data.color = material.GetColor("_Color");
            if (material.HasProperty("_MainTex"))
            {
                var texture = material.GetTexture("_MainTex"); data.mainTextureName = texture == null ? "" : texture.name;
                data.mainTextureAsset = texture == null ? "" : AssetDatabase.GetAssetPath(texture);
                data.mainTextureScale = material.GetTextureScale("_MainTex"); data.mainTextureOffset = material.GetTextureOffset("_MainTex");
            }
            return data;
        }

        private static LightRecord DescribeLight(Light light) => new LightRecord { path = HierarchyPath(light.transform),
            type = light.type.ToString(), shadows = light.shadows.ToString(), enabled = light.enabled, activeInHierarchy = light.gameObject.activeInHierarchy,
            position = light.transform.position, rotation = light.transform.eulerAngles, color = light.color, intensity = light.intensity,
            range = light.range, spotAngle = light.spotAngle, shadowStrength = light.shadowStrength, shadowBias = light.shadowBias,
            shadowNormalBias = light.shadowNormalBias, cullingMask = light.cullingMask, cookie = light.cookie == null ? "" : light.cookie.name };

        private static bool IsWallpaper(Renderer renderer) => renderer.sharedMaterials.Any(IsWallpaperMaterial);
        private static bool IsWallpaperMaterial(Material material) => material != null
            && (IsWallpaperMaterialName(material.name) || IsWallpaperMaterialName(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(material))));
        private static bool IsWallpaperMaterialName(string name) => string.Equals(name, "MainRoom_30", StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "MainRoom_30_B", StringComparison.OrdinalIgnoreCase);
        private static bool IsWallShell(Renderer renderer) => !IsWallpaper(renderer)
            && renderer.name.IndexOf("wall", StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool IsSuspicious(Renderer renderer)
        {
            string name = renderer.name.ToLowerInvariant();
            return IsWallpaper(renderer) || name.Contains("wall") || name.Contains("plane") || name.Contains("平面")
                || name.Contains("墙") || name.Contains("ceiling") || name == "b_awal"
                || renderer.sharedMaterials.Any(material => material != null && (material.name.IndexOf("wall", StringComparison.OrdinalIgnoreCase) >= 0
                    || material.name.IndexOf("MainRoom_30", StringComparison.OrdinalIgnoreCase) >= 0));
        }
        private static void RestoreLights(Light[] lights, LightShadows[] values)
        { for (int i = 0; i < Mathf.Min(lights.Length, values.Length); i++) if (lights[i] != null) lights[i].shadows = values[i]; }
        private static void RestoreRenderers(Renderer[] renderers, bool[] enabled, ShadowCastingMode[] casting)
        { for (int i = 0; i < Mathf.Min(renderers.Length, enabled.Length, casting.Length); i++) if (renderers[i] != null)
            { renderers[i].enabled = enabled[i]; renderers[i].shadowCastingMode = casting[i]; } }
        private static void RestoreMaterials(Renderer[] renderers, Material[][] materials)
        { for (int i = 0; i < Mathf.Min(renderers.Length, materials.Length); i++) if (renderers[i] != null) renderers[i].sharedMaterials = materials[i]; }
        private static bool Finite(Vector3 value) => !(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
            || float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z));
        private static string HierarchyPath(Transform transform)
        { string path = transform.name; for (var parent = transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path; return path; }
        private static string FileHash(string path)
        { using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
        private static string SceneFilePath(string path) => Path.IsPathRooted(path) ? path : Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
    }
}
