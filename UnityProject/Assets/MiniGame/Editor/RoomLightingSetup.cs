using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public static class RoomLightingSetup
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/RoomLightingDefault.asset";
        public const string DayProfilePath = "Assets/MiniGame/Resources/RoomLightingDay.asset";
        public const string NightProfilePath = "Assets/MiniGame/Resources/RoomLightingNight.asset";
        public const string RetroPath = "Assets/MiniGame/Resources/RoomLightingReferenceRetro.asset";
        public const string BackupRetroPath = "Assets/MiniGame/Resources/RetroScreenBeforeRoomLighting.asset";

        public static RoomLightingProfile EnsureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(ProfilePath);
            if (profile != null) return profile;
            profile = ScriptableObject.CreateInstance<RoomLightingProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            return profile;
        }

        public static (RoomLightingProfile Day, RoomLightingProfile Night) EnsureIndependentProfiles(RoomLightingProfile source = null)
        {
            source = source != null ? source : EnsureProfile();
            return (EnsurePeriodProfile(DayProfilePath, source), EnsurePeriodProfile(NightProfilePath, source));
        }

        private static RoomLightingProfile EnsurePeriodProfile(string path, RoomLightingProfile source)
        {
            var result = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(path);
            if (result != null) return result;
            result = UnityEngine.Object.Instantiate(source);
            result.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(result, path);
            return result;
        }

        public static RoomLightingController FindController(Scene scene) => scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<RoomLightingController>(true)).FirstOrDefault() : null;

        public static RoomLightingController EnsureInScene(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请退出播放后首次安装房间灯光。");
            if (scene.path != MainRoomGameplayBuilder.ScenePath) throw new InvalidOperationException("请先打开 MainRoomCombined 场景。");
            var room = scene.GetRootGameObjects().Single(item => item.name == "MainRoomCombined");
            var surfaces = room.GetComponentsInChildren<MeshRenderer>(true);
            if (surfaces.Any(surface => surface.sharedMaterials.Any(material => material != null && material.name == "MainRoom_30_B")))
                WallSurfaceRepairSetup.Apply(scene, true);
            var floor = surfaces.Single(item => item.name == "B_Floor");
            var lamp = surfaces.Single(item => item.name.Contains("Attic_Light"));
            var lights = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Light>(true)).ToArray();
            var controller = FindController(scene);
            bool firstInstall = controller == null;
            if (controller == null)
            {
                var go = new GameObject("Room lighting"); SceneManager.MoveGameObjectToScene(go, scene);
                Undo.RegisterCreatedObjectUndo(go, "添加房间灯光配置");
                controller = Undo.AddComponent<RoomLightingController>(go);
            }
            var profile = controller.Profile != null ? controller.Profile : EnsureProfile();
            var rug = RoomRugSetup.Ensure(scene, controller.transform, floor.sharedMaterial);
            Undo.RecordObject(controller, "接入房间灯光配置");
            Undo.RecordObjects(lights, "更新房间灯光");
            var guidance = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<BedtimeGuidance>(true)).FirstOrDefault();
            var main = lights.Single(item => item.name == "CeilingLight");
            main.lightmapBakeType = LightmapBakeType.Realtime;
            controller.Configure(profile, main,
                lights.FirstOrDefault(item => item.name == "WindowLight"), lights.FirstOrDefault(item => item.name == "FillLight"),
                lights.FirstOrDefault(item => item.name == "ExteriorLight"), lights.FirstOrDefault(item => item.name == "MonitorLight"),
                floor, lamp, 0, surfaces, rug, guidance);
            if (!controller.HasIndependentProfiles)
            {
                var previousMode = controller.PreviewMode;
                controller.SetPreviewMode(RoomLightingPreviewMode.Day);
                // Both periods begin from the visible daytime baseline. Existing period assets are never overwritten during installation.
                bool dayExists = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(DayProfilePath) != null;
                bool nightExists = AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(NightProfilePath) != null;
                var profiles = EnsureIndependentProfiles(profile);
                var baseline = lights.Select(light => new SceneLightSettings { HierarchyPath = controller.GetHierarchyPath(light),
                    dayEnabled = light.enabled, localPosition = light.transform.localPosition, localEuler = light.transform.localEulerAngles,
                    color = light.color, intensity = light.intensity, range = light.range, type = light.type,
                    spotAngle = light.spotAngle, innerSpotAngle = light.innerSpotAngle, shadows = light.shadows,
                    shadowStrength = light.shadowStrength, shadowBias = light.shadowBias, shadowNormalBias = light.shadowNormalBias }).ToArray();
                if (!dayExists) { profiles.Day.SceneLightDefaults = baseline.Select(value => value.Copy()).ToList(); EditorUtility.SetDirty(profiles.Day); }
                if (!nightExists) { profiles.Night.SceneLightDefaults = baseline.Select(value => value.Copy()).ToList(); EditorUtility.SetDirty(profiles.Night); }
                controller.ConfigureProfiles(profiles.Day, profiles.Night);
                controller.SetPreviewMode(previousMode);
            }
            var camera = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<ApartmentPlayer>(true))
                .Select(item => item.ViewCamera).FirstOrDefault();
            if (camera != null)
            {
                var previous = camera.GetComponent<RetroScreenEffect>()?.Profile;
                if (firstInstall && previous != null && !File.Exists(BackupRetroPath)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(previous), BackupRetroPath);
                var reference = AssetDatabase.LoadAssetAtPath<RetroScreenProfile>(RetroPath);
                if (reference == null)
                {
                    reference = ScriptableObject.CreateInstance<RetroScreenProfile>();
                    reference.EffectEnabled = true; reference.Strength = 1; reference.PixelSize = 1;
                    reference.FullscreenQuantization = false; reference.GammaBefore = reference.GammaAfter = 1;
                    AssetDatabase.CreateAsset(reference, RetroPath);
                }
                if (firstInstall || previous == null || AssetDatabase.GetAssetPath(previous) == RetroScreenSetup.ProfilePath)
                    RetroScreenSetup.Attach(camera, reference);
            }
            foreach (var texture in new[] { floor.sharedMaterial.mainTexture, floor.sharedMaterial.GetTexture("_BumpMap") })
            {
                if (texture == null) continue;
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (importer == null || importer.mipmapEnabled && importer.anisoLevel >= 8 && importer.filterMode == FilterMode.Trilinear) continue;
                importer.mipmapEnabled = true; importer.anisoLevel = 8; importer.filterMode = FilterMode.Trilinear;
                importer.SaveAndReimport();
            }
            // Imported textures alone do not activate the surface shader's local normal-map variant.
            if (floor.sharedMaterial.GetTexture("_BumpMap") != null && !floor.sharedMaterial.IsKeywordEnabled("_NORMALMAP"))
            {
                floor.sharedMaterial.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(floor.sharedMaterial);
            }
            EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(scene);
            return controller;
        }

        [MenuItem("MiniGame/画面/为当前主场景安装房间灯光")]
        public static void InstallInOpenScene()
        {
            EnsureInScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
    }
}
