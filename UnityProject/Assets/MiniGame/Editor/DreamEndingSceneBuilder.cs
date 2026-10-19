using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Builds the separate ending, reading original bed assets without rebuilding Combined.</summary>
    public static class DreamEndingSceneBuilder
    {
        public const string ScenePath = "Assets/MiniGame/Scenes/DreamEndingBeach.unity";
        public const string ArtPath = "Assets/MiniGame/Art/DreamEnding";
        private sealed class BedPart
        {
            public string Name;
            public Mesh Mesh;
            public Material[] Materials;
            public Vector3 Position, Scale;
            public Quaternion Rotation;
            public Bounds Bounds;
        }

        public static void BuildScene(DreamSequenceProfile profile)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先退出播放模式再生成海滩场景。");
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("请先保存已打开的场景，再生成海滩。");
            if (SceneManager.GetSceneByPath(ScenePath).isLoaded)
                throw new InvalidOperationException("请关闭已打开的海滩场景后重新生成。");
            Directory.CreateDirectory(ArtPath);
            AssetDatabase.Refresh();
            var terrainAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ArtPath + "/BeachTerrain.fbx");
            if (terrainAsset == null) throw new InvalidOperationException("先运行 Art/DreamEnding/build_beach.py 生成 Blender 海滩地形。");
            var parts = ReadOriginalBed();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var stage = new GameObject("Dream Ending Beach");
                var sand = Asset(ArtPath + "/Sand.mat", () => new Material(RequiredShader("DreamSeaSand")));
                var water = Asset(ArtPath + "/Sea.mat", () => new Material(RequiredShader("DreamSea")));
                var sky = Asset(ArtPath + "/CloudSky.mat", () => new Material(RequiredShader("DreamCloud")));
                var terrain = (GameObject)PrefabUtility.InstantiatePrefab(terrainAsset, scene);
                terrain.name = "Editable Blender beach";
                terrain.transform.SetParent(stage.transform, false);
                OrientShoreTowardOcean(terrain);
                foreach (var renderer in terrain.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = sand;
                var terrainBounds = terrain.GetComponentsInChildren<Renderer>().Select(r => r.bounds).ToArray();
                if (terrainBounds.Length == 0) throw new InvalidOperationException("海滩 FBX 未包含可渲染网格。");

                var ocean = new GameObject("Animated Sea", typeof(MeshFilter), typeof(MeshRenderer));
                ocean.transform.SetParent(stage.transform, false);
                ocean.GetComponent<MeshFilter>().sharedMesh = RefreshSeaGridAsset();
                var seaRenderer = ocean.GetComponent<MeshRenderer>();
                seaRenderer.sharedMaterial = water;
                seaRenderer.shadowCastingMode = ShadowCastingMode.Off;
                seaRenderer.receiveShadows = false;

                var bed = CreateOriginalBed(stage.transform, parts, out var pillow);
                var cameraGo = new GameObject("Ending Camera", typeof(Camera), typeof(AudioListener));
                cameraGo.tag = "MainCamera";
                cameraGo.transform.SetParent(stage.transform, false);
                cameraGo.transform.position = new Vector3(0, 1.85f, -11);
                cameraGo.transform.rotation = Quaternion.Euler(3, 0, 0);
                var camera = cameraGo.GetComponent<Camera>();
                camera.fieldOfView = 62;
                camera.nearClipPlane = .03f;
                camera.farClipPlane = 1200;
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.allowHDR = true;
                var start = new GameObject("Walk Start").transform;
                start.SetParent(stage.transform, false);
                start.SetPositionAndRotation(cameraGo.transform.position, cameraGo.transform.rotation);
                var sun = new GameObject("Afternoon Sun", typeof(Light));
                sun.transform.SetParent(stage.transform, false);
                sun.transform.rotation = Quaternion.Euler(52, -32, 0);
                var light = sun.GetComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1, .97f, .9f);
                light.intensity = .9f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = .5f;
                RenderSettings.skybox = sky;
                RenderSettings.sun = light;
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(.48f, .61f, .72f);
                RenderSettings.ambientEquatorColor = new Color(.44f, .50f, .52f);
                RenderSettings.ambientGroundColor = new Color(.34f, .31f, .25f);
                RenderSettings.ambientIntensity = .75f;
                RenderSettings.fog = false;
                var controller = stage.AddComponent<DreamEndingController>();
                controller.Configure(profile, camera, bed, pillow, start, seaRenderer);
                PrefabUtility.SaveAsPrefabAssetAndConnect(stage, ArtPath + "/DreamEndingStage.prefab", InteractionMode.AutomatedAction);
                EditorSceneManager.SaveScene(scene, ScenePath);
                var builds = EditorBuildSettings.scenes.ToList();
                if (!builds.Any(item => item.path == ScenePath)) builds.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = builds.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("海滩终局已生成：" + ScenePath + "；原床四个网格与材质完整复用。");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void OrientShoreTowardOcean(GameObject terrain)
        {
            // Blender/FBX can reverse the forward axis. Measure the imported vertices,
            // then ensure that +Z descends below sea level and -Z remains the dry shore.
            double sumZ = 0, sumY = 0, sumZZ = 0, sumZY = 0;
            int count = 0;
            foreach (var filter in terrain.GetComponentsInChildren<MeshFilter>())
                foreach (var vertex in filter.sharedMesh.vertices)
                {
                    var p = filter.transform.TransformPoint(vertex);
                    sumZ += p.z; sumY += p.y; sumZZ += p.z * p.z; sumZY += p.z * p.y; count++;
                }
            double variance = sumZZ - sumZ * sumZ / Math.Max(count, 1);
            if (count == 0 || variance < .001) throw new InvalidOperationException("海滩地形无法确定海岸方向。");
            double slope = (sumZY - sumZ * sumY / count) / variance;
            if (Math.Abs(slope) < .005) throw new InvalidOperationException("海滩地形未沿 Z 轴形成海岸坡度。");
            if (slope > 0) terrain.transform.rotation = Quaternion.Euler(0, 180, 0) * terrain.transform.rotation;
            Debug.Log("Beach imported height/Z slope=" + slope.ToString("F5") + "; ocean orientation corrected=" + (slope > 0));
        }

        private static List<BedPart> ReadOriginalBed()
        {
            var preview = SceneManager.GetSceneByPath(MainRoomGameplayBuilder.ScenePath);
            bool opened = !preview.isLoaded;
            if (opened) preview = EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath, OpenSceneMode.Additive);
            try
            {
                var names = new[] { "B_SM_bed_1", "B_SM_mattress_1", "B_SM_pillow_1", "B_SM_rug_2" };
                var renderers = preview.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
                var result = new List<BedPart>();
                foreach (var name in names)
                {
                    var source = renderers.SingleOrDefault(r => r.name == name);
                    if (source == null || source.GetComponent<MeshFilter>()?.sharedMesh == null)
                        throw new InvalidOperationException("Combined 原床网格缺失或重名：" + name);
                    result.Add(new BedPart { Name = name, Mesh = source.GetComponent<MeshFilter>().sharedMesh,
                        Materials = source.sharedMaterials, Position = source.transform.position,
                        Rotation = source.transform.rotation, Scale = source.transform.lossyScale, Bounds = source.bounds });
                }
                return result;
            }
            finally { if (opened) EditorSceneManager.CloseScene(preview, true); }
        }

        private static Transform CreateOriginalBed(Transform parent, List<BedPart> parts, out Transform pillow)
        {
            var bounds = parts[0].Bounds;
            foreach (var part in parts) bounds.Encapsulate(part.Bounds);
            var origin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var root = new GameObject("Original apartment bed").transform;
            root.SetParent(parent, false);
            Transform pillowMesh = null;
            foreach (var part in parts)
            {
                var obj = new GameObject(part.Name, typeof(MeshFilter), typeof(MeshRenderer));
                obj.transform.SetParent(root, false);
                obj.transform.localPosition = part.Position - origin;
                obj.transform.localRotation = part.Rotation;
                obj.transform.localScale = part.Scale;
                obj.GetComponent<MeshFilter>().sharedMesh = part.Mesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials = part.Materials;
                if (part.Name == "B_SM_pillow_1") pillowMesh = obj.transform;
            }
            var pillowBounds = pillowMesh.GetComponent<Renderer>().bounds;
            var headDirection = Vector3.ProjectOnPlane(pillowBounds.center, Vector3.up);
            root.rotation = Quaternion.FromToRotation(headDirection.normalized, Vector3.forward);
            root.position = new Vector3(0, .02f, 1.6f);
            pillow = new GameObject("Pillow Eye Position").transform;
            pillow.SetParent(root, false);
            var pillowNow = pillowMesh.GetComponent<Renderer>().bounds;
            pillow.position = new Vector3(pillowNow.center.x, pillowNow.max.y + .12f, pillowNow.center.z);
            pillow.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            return root;
        }

        private static Shader RequiredShader(string name)
        {
            var shader = Resources.Load<Shader>(name);
            if (shader == null) throw new InvalidOperationException("Missing beach shader: " + name);
            return shader;
        }

        private static T Asset<T>(string path, Func<T> create) where T : UnityEngine.Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = create();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Mesh RefreshSeaGridAsset()
        {
            const string path = ArtPath + "/SeaGrid.asset";
            var generated = SeaGrid();
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }
            EditorUtility.CopySerialized(generated, existing);
            UnityEngine.Object.DestroyImmediate(generated);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Mesh SeaGrid()
        {
            const int width = 240, depth = 240;
            var vertices = new Vector3[(width + 1) * (depth + 1)];
            var triangles = new int[width * depth * 6];
            int index = 0;
            for (int z = 0; z <= depth; z++)
                for (int x = 0; x <= width; x++)
                {
                    float xn = (float)x / width * 2 - 1;
                    float zn = (float)z / depth;
                    // Resolve the near-shore rolling crests; stretch only the outer grid toward the horizon.
                    float ax = Mathf.Abs(xn);
                    float worldX = ax <= .75f ? ax / .75f * 50
                        : 50 + Mathf.Pow((ax - .75f) / .25f, 2) * 1950;
                    vertices[z * (width + 1) + x] = new Vector3(Mathf.Sign(xn) * worldX, -.03f, -8 + zn * zn * 905);
                    if (x == width || z == depth) continue;
                    int a = z * (width + 1) + x;
                    triangles[index++] = a; triangles[index++] = a + width + 1; triangles[index++] = a + 1;
                    triangles[index++] = a + 1; triangles[index++] = a + width + 1; triangles[index++] = a + width + 2;
                }
            var mesh = new Mesh { name = "Beach sea surface", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var bounds = mesh.bounds;
            bounds.Expand(new Vector3(4, 4, 4)); // Include the shader's displaced wave envelope.
            mesh.bounds = bounds;
            return mesh;
        }
    }
}
