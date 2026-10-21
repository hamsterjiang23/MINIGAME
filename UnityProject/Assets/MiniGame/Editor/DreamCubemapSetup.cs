using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Copies the authored beach environment without its player, bed or presentation controller.</summary>
    public static class DreamCubemapSetup
    {
        public const string SourcePath = "Assets/MiniGame/Art/DreamEnding/DreamCubemapSource.prefab";

        [MenuItem("MiniGame/梦境/生成海滩全景捕获源")]
        public static void BuildSource()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先退出播放模式再更新全景捕获源。");
            var profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(DreamSequenceSetup.ProfilePath);
            if (profile == null) throw new InvalidOperationException("缺少默认梦境配置：" + DreamSequenceSetup.ProfilePath);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DreamEndingSceneBuilder.ScenePath) == null)
                throw new InvalidOperationException("缺少海滩终局场景：" + DreamEndingSceneBuilder.ScenePath);

            var previous = SceneManager.GetActiveScene();
            var beach = SceneManager.GetSceneByPath(DreamEndingSceneBuilder.ScenePath);
            bool alreadyInSetup = EditorSceneManager.GetSceneManagerSetup().Any(item => item.path == DreamEndingSceneBuilder.ScenePath);
            bool opened = !beach.isLoaded;
            if (!opened && beach.isDirty)
                throw new InvalidOperationException("请先保存海滩场景的编辑，再更新全景捕获源；其他已打开场景不会被保存或重载。");
            Scene staging = default;
            try
            {
                if (opened) beach = EditorSceneManager.OpenScene(DreamEndingSceneBuilder.ScenePath, OpenSceneMode.Additive);
                SceneManager.SetActiveScene(beach);
                var sky = RenderSettings.skybox ?? AssetDatabase.LoadAssetAtPath<Material>(DreamEndingSceneBuilder.ArtPath + "/CloudSky.mat");
                if (sky == null) throw new InvalidOperationException("海滩场景缺少天空材质。");
                var ambientSky = RenderSettings.ambientSkyColor;
                var ambientEquator = RenderSettings.ambientEquatorColor;
                var ambientGround = RenderSettings.ambientGroundColor;
                float ambientIntensity = RenderSettings.ambientIntensity;
                var transforms = beach.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
                var terrain = RequiredTransform(transforms, "Editable Blender beach");
                var sea = RequiredTransform(transforms, "Animated Sea");
                var sun = RequiredTransform(transforms, "Afternoon Sun");
                var controller = beach.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DreamEndingController>(true)).SingleOrDefault();
                if (controller == null) throw new InvalidOperationException("海滩场景缺少唯一的终局控制器，无法读取原机位。");
                Transform standing, bed, pillow;
                using (var bindings = new SerializedObject(controller))
                {
                    standing = bindings.FindProperty("approach").objectReferenceValue as Transform;
                    if (standing == null)
                    {
                        var camera = bindings.FindProperty("viewCamera").objectReferenceValue as Camera;
                        if (camera != null) standing = camera.transform;
                    }
                    bed = bindings.FindProperty("bed").objectReferenceValue as Transform;
                    pillow = bindings.FindProperty("pillow").objectReferenceValue as Transform;
                }
                if (standing == null || bed == null || pillow == null)
                    throw new InvalidOperationException("海滩场景缺少站立、床或枕头机位绑定。");

                // Create only in our own scene, leaving user scenes and their dirty state intact.
                staging = EditorSceneManager.NewPreviewScene();
                var root = new GameObject("Dream Cubemap Source");
                SceneManager.MoveGameObjectToScene(root, staging);
                CloneEnvironment(terrain, root.transform, true);
                CloneEnvironment(sea, root.transform, true);
                CloneEnvironment(sun, root.transform, true);
                var source = root.AddComponent<DreamCubemapSource>();
                source.SkyMaterial = sky;
                source.AmbientSkyColor = ambientSky;
                source.AmbientEquatorColor = ambientEquator;
                source.AmbientGroundColor = ambientGround;
                source.AmbientIntensity = ambientIntensity;
                source.StandingPose = CopyPose(standing, "Standing Capture Pose", root.transform);
                var bedPivot = CopyPose(bed, "Virtual Bed Pivot", root.transform);
                bedPivot.localScale = bed.lossyScale;
                source.PillowPose = CopyPose(pillow, "Pillow Capture Pose", bedPivot);
                source.WaterSurfaces = root.GetComponentsInChildren<Renderer>(true);
                source.Lights = root.GetComponentsInChildren<Light>(true);
                if (source.WaterSurfaces.Length == 0 || source.Lights.Length == 0)
                    throw new InvalidOperationException("海滩全景捕获源缺少网格或灯光。");

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, SourcePath);
                if (prefab == null) throw new InvalidOperationException("无法保存全景捕获源：" + SourcePath);
                Undo.RecordObject(profile, "绑定海滩全景捕获源");
                profile.CubemapSourcePrefab = prefab;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssetIfDirty(profile);
                Debug.Log("海滩全景捕获源已保存并绑定：" + SourcePath);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (staging.IsValid()) EditorSceneManager.ClosePreviewScene(staging);
                if (opened && beach.IsValid() && beach.isLoaded) EditorSceneManager.CloseScene(beach, !alreadyInSetup);
            }
        }

        private static Transform RequiredTransform(Transform[] transforms, string name)
        {
            var matches = transforms.Where(item => item.name == name).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("海滩场景对象缺失或重名：" + name);
            return matches[0];
        }

        private static Transform CopyPose(Transform original, string name, Transform parent)
        {
            var copy = new GameObject(name).transform;
            copy.SetParent(parent, false);
            copy.SetPositionAndRotation(original.position, original.rotation);
            return copy;
        }

        private static void CloneEnvironment(Transform original, Transform parent, bool worldTransform)
        {
            var copy = new GameObject(original.name);
            copy.transform.SetParent(parent, false);
            if (worldTransform)
            {
                copy.transform.SetPositionAndRotation(original.position, original.rotation);
                copy.transform.localScale = original.lossyScale;
            }
            else
            {
                copy.transform.localPosition = original.localPosition;
                copy.transform.localRotation = original.localRotation;
                copy.transform.localScale = original.localScale;
            }
            // Whitelisting components also excludes future gameplay scripts, cameras, colliders and UI.
            var filter = original.GetComponent<MeshFilter>();
            if (filter != null) copy.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var renderer = original.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var target = copy.AddComponent<MeshRenderer>();
                EditorUtility.CopySerialized(renderer, target);
                target.probeAnchor = null;
                target.lightProbeProxyVolumeOverride = null;
            }
            var light = original.GetComponent<Light>();
            if (light != null) EditorUtility.CopySerialized(light, copy.AddComponent<Light>());
            foreach (Transform child in original) CloneEnvironment(child, copy.transform, false);
            copy.SetActive(original.gameObject.activeSelf);
        }
    }
}
