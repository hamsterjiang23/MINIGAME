using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Imports the published room art into an independent Built-in preview.</summary>
    public static class MainRoomPreviewBuilder
    {
        public const string ScenePath = "Assets/MiniGame/Scenes/MainRoomPreview.unity";
        private const string ArtPath = "Assets/MiniGame/Art/MainRoom/";
        public const string CombinedScenePath = "Assets/MiniGame/Scenes/MainRoomCombined.unity";

        [Serializable] private sealed class SourceManifest
        {
            public string sourceSha256 = "";
            public int meshCount = 0;
            public bool hasExteriorConcrete = false;
            public MaterialDefinition[] materials = Array.Empty<MaterialDefinition>();
        }

        [Serializable] private sealed class MaterialDefinition
        {
            public string name = "", label = "", baseTexture = "", normalTexture = "", metallicRoughnessTexture = "";
            public float[] color = Array.Empty<float>(), emission = Array.Empty<float>();
            public float metallic = 0, roughness = 1, normalScale = 1;
            public bool doubleSided = false;
            public bool windowGlass = false;
        }

        [Serializable] private sealed class ImportReport
        {
            public string unity, scene, sourceSha256, screenshot, scope;
            public int meshRenderers, materials, missingMaterials, unsupportedShaders;
            public long triangles;
            public Vector3 dimensions;
        }

        [MenuItem("MiniGame/Room Art/Import and open preview")]
        public static void ImportAndOpen() => Build(ArtPath, ScenePath, "MainRoom");

        [MenuItem("MiniGame/Room Art/Import and open combined room")]
        public static void ImportCombinedAndOpen() => Build(ArtPath + "Combined/", CombinedScenePath, "MainRoomCombined", true);

        private static void Build(string artPath, string scenePath, string evidenceName, bool installGameplay = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before importing the room preview.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var evidencePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/Unity", evidenceName));
            var modelPath = artPath + "MainRoom.fbx";
            var source = JsonUtility.FromJson<SourceManifest>(File.ReadAllText(artPath + "materials.json"));
            Directory.CreateDirectory(artPath + "Materials");
            Directory.CreateDirectory(evidencePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var materials = source.materials.ToDictionary(definition => definition.name, definition => MakeMaterial(definition, artPath));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) throw new InvalidOperationException("FBX did not produce a model asset.");
            var room = (GameObject)PrefabUtility.InstantiatePrefab(model);
            room.name = evidenceName;
            var renderers = room.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported =>
                {
                    if (imported == null || !materials.TryGetValue(imported.name, out var replacement))
                        throw new InvalidOperationException("Unmapped FBX material on " + renderer.name + ": " + (imported == null ? "null" : imported.name));
                    return replacement;
                }).ToArray();
                // The source reference camera is outside this removable front closure.
                if (renderer.name.StartsWith("Front ", StringComparison.Ordinal) || renderer.name.StartsWith("Entrance ", StringComparison.Ordinal))
                    renderer.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            if (Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) > 12f || bounds.size.y < 2f)
                throw new InvalidOperationException("Unexpected room scale: " + bounds.size);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.18f, .20f, .17f);
            RenderSettings.fog = false;
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = Marker(room, "PreviewPosition");
            camera.transform.LookAt(Marker(room, "PreviewTarget"));
            camera.fieldOfView = 58.2f;
            camera.nearClipPlane = .03f;
            camera.farClipPlane = 40;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.045f, .05f, .045f);
            camera.allowHDR = true;
            AddLight(room, "CeilingLight", new Color(.70f, .82f, .69f), 1.3f, 8, true);
            AddLight(room, "WindowLight", new Color(.47f, .58f, .72f), 1.1f, 7, true);
            AddLight(room, "MonitorLight", new Color(.48f, .76f, .61f), .35f, 2, false);
            AddLight(room, "FillLight", new Color(.66f, .69f, .60f), .45f, 7, false);
            if (source.hasExteriorConcrete)
                AddLight(room, "ExteriorLight", new Color(.62f, .68f, .72f), .85f, 10, true);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, scenePath);
            var report = new ImportReport
            {
                unity = Application.unityVersion, scene = scenePath, sourceSha256 = source.sourceSha256,
                screenshot = Path.Combine(evidencePath, "main-room-unity.png"),
                scope = "Imported art preview only; no gameplay integration, collider or performance acceptance",
                meshRenderers = renderers.Length, materials = materials.Count, dimensions = bounds.size,
                triangles = room.GetComponentsInChildren<MeshFilter>().Sum(filter =>
                    Enumerable.Range(0, filter.sharedMesh.subMeshCount).Sum(index => (long)filter.sharedMesh.GetIndexCount(index) / 3)),
                missingMaterials = renderers.Sum(renderer => renderer.sharedMaterials.Count(material => material == null)),
                unsupportedShaders = materials.Values.Count(material => !material.shader.isSupported)
            };
            if (report.meshRenderers != source.meshCount || report.missingMaterials != 0 || report.unsupportedShaders != 0)
                throw new InvalidOperationException("Room import validation failed: " + JsonUtility.ToJson(report));
            Selection.activeGameObject = room;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(camera.transform.position + camera.transform.forward * 3, camera.transform.rotation, 3, false, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            DynamicGI.UpdateEnvironment();
            var readyAt = EditorApplication.timeSinceStartup + 1;
            EditorApplication.CallbackFunction finish = null;
            finish = () =>
            {
                if (EditorApplication.timeSinceStartup < readyAt || EditorApplication.isCompiling) return;
                EditorApplication.update -= finish;
                if (camera == null) return;
                Capture(camera, report.screenshot);
                if (source.hasExteriorConcrete)
                {
                    var position = camera.transform.position;
                    var rotation = camera.transform.rotation;
                    var fieldOfView = camera.fieldOfView;
                    try
                    {
                        camera.transform.position = Marker(room, "WindowDetailPosition");
                        camera.transform.LookAt(Marker(room, "WindowDetailTarget"));
                        camera.fieldOfView = 50;
                        Capture(camera, Path.Combine(evidencePath, "window-detail.png"));
                    }
                    finally { camera.transform.SetPositionAndRotation(position, rotation); camera.fieldOfView = fieldOfView; }
                    EditorSceneManager.SaveScene(scene, scenePath);
                }
                File.WriteAllText(Path.Combine(evidencePath, "import-report.json"), JsonUtility.ToJson(report, true));
                Debug.Log("MINIGAME_ROOM_PREVIEW_READY " + JsonUtility.ToJson(report));
                if (installGameplay) MainRoomGameplayBuilder.ConfigureOpenScene();
            };
            EditorApplication.update += finish;
        }

        private static Vector3 Marker(GameObject room, string name) => room.GetComponentsInChildren<Transform>(true)
            .Single(transform => transform.name == "Unity_" + name).position;

        private static void AddLight(GameObject room, string marker, Color color, float intensity, float range, bool shadows)
        {
            var light = new GameObject(marker, typeof(Light)).GetComponent<Light>();
            light.transform.position = Marker(room, marker);
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowBias = .02f;
        }

        private static Material MakeMaterial(MaterialDefinition definition, string artPath)
        {
            var label = definition.label.Split('|')[0].Trim();
            var path = artPath + "Materials/" + definition.name + "_" + Regex.Replace(label, "[^A-Za-z0-9_-]", "_") + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find(definition.windowGlass ? "MiniGame/Room Window Glass" : "MiniGame/Room Art Standard");
            if (shader == null) throw new InvalidOperationException("Room art shader did not import.");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            material.shaderKeywords = Array.Empty<string>();
            material.name = Path.GetFileNameWithoutExtension(path);
            material.SetInt("_Cull", (int)(definition.doubleSided ? CullMode.Off : CullMode.Back));
            material.color = SourceColor(definition.color);
            if (definition.windowGlass) { EditorUtility.SetDirty(material); return material; }
            material.mainTexture = Texture(artPath, definition.baseTexture, false);
            material.SetFloat("_Metallic", definition.metallic);
            material.SetFloat("_Glossiness", 1 - definition.roughness);
            if (!string.IsNullOrEmpty(definition.normalTexture))
            {
                material.SetTexture("_BumpMap", Texture(artPath, definition.normalTexture, true));
                material.SetFloat("_BumpScale", definition.normalScale);
                material.EnableKeyword("_NORMALMAP");
            }
            if (!string.IsNullOrEmpty(definition.metallicRoughnessTexture))
            {
                var source = Texture(artPath, definition.metallicRoughnessTexture, false, true);
                var pixels = source.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32((byte)(pixels[i].b * definition.metallic), 0, 0,
                        (byte)(255 - pixels[i].g * definition.roughness));
                var converted = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
                converted.SetPixels32(pixels); converted.Apply();
                var packedPath = artPath + "Textures/" + definition.name + "_metallic-smoothness.png";
                File.WriteAllBytes(packedPath, converted.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(converted);
                AssetDatabase.ImportAsset(packedPath, ImportAssetOptions.ForceSynchronousImport);
                material.SetTexture("_MetallicGlossMap", Texture(artPath, "Textures/" + Path.GetFileName(packedPath), false, true));
                material.SetFloat("_GlossMapScale", 1);
                material.EnableKeyword("_METALLICGLOSSMAP");
            }
            material.SetColor("_EmissionColor", SourceColor(definition.emission));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Color SourceColor(float[] components)
        {
            var linear = new Color(components[0], components[1], components[2], components.Length > 3 ? components[3] : 1);
            // glTF factors are linear; this project's Gamma workflow does no shader conversion.
            return QualitySettings.activeColorSpace == ColorSpace.Gamma ? linear.gamma : linear;
        }

        private static Texture2D Texture(string artPath, string relative, bool normal, bool data = false)
        {
            if (string.IsNullOrEmpty(relative)) return null;
            var path = artPath + relative;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new FileNotFoundException("Texture did not import", path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal && !data;
            importer.isReadable = data;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Capture(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
