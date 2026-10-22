using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Asset-backed authoring shared by the window, Scene handles and Editor tests.</summary>
    [InitializeOnLoad]
    public static class RoomLightingAuthoring
    {
        private static readonly HashSet<RoomLightingProfile> Pending = new HashSet<RoomLightingProfile>();
        private sealed class LightBaseline
        {
            public RoomLightingController Controller;
            public RoomLightingProfile Profile;
            public SceneLightSettings Value;
            public RoomLightingPeriod Period;
        }
        private static readonly Dictionary<(Light Light, RoomLightingPeriod Period), LightBaseline> FirstOverrides =
            new Dictionary<(Light, RoomLightingPeriod), LightBaseline>();
        private static readonly Dictionary<RoomLightingController, RoomLightingPeriod> EditingPeriods =
            new Dictionary<RoomLightingController, RoomLightingPeriod>();
        private static readonly Dictionary<int, SceneSavePreview> SavingPreviews = new Dictionary<int, SceneSavePreview>();
        private sealed class SceneSavePreview
        {
            public Scene Scene;
            public Scene OriginalActiveScene;
            public EnvironmentPreview OriginalActiveEnvironment;
            public RoomLightingController[] Controllers;
            public RoomLightingPreviewMode[] Modes;
            public LightPreview[] Lights;
            public CameraPreview[] Cameras;
            public EnvironmentPreview Environment;
        }
        private sealed class LightPreview
        {
            public Light Light;
            public SceneLightSettings Settings;
            public Quaternion Rotation;
            public LightRenderMode RenderMode;
            public int ShadowResolution;
            public Texture Cookie;
        }
        private sealed class CameraPreview
        {
            public Camera Camera;
            public CameraClearFlags ClearFlags;
            public Color Background;
            public DepthTextureMode DepthMode;
        }
        private sealed class EnvironmentPreview
        {
            public AmbientMode Mode;
            public Color Ambient, Sky, Equator, Ground, FogColor;
            public float Intensity, Reflection;
            public SphericalHarmonicsL2 Probe;
            public bool Fog;
            public Material Skybox;
        }
        private static bool synchronizingNative;
        public static bool IsApplying { get; private set; }
        public static bool IsSavingScenePreview => SavingPreviews.Count > 0;

        static RoomLightingAuthoring()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            Undo.postprocessModifications += OnNativeModifications;
            RoomLightingTuningPanel.ProfileEditedInEditor += OnGamePanelProfileEdited;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChangedInEditMode;
            AssemblyReloadEvents.beforeAssemblyReload += PrepareForReloadOrQuit;
            EditorApplication.quitting += PrepareForReloadOrQuit;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !scene.IsValid() || !scene.isLoaded || SavingPreviews.ContainsKey(scene.handle)) return;
            var rooms = GetLoadedControllers().Where(room => room.gameObject.scene == scene && room.Profile != null).ToArray();
            if (!rooms.Any(room => room.PreviewMode == RoomLightingPreviewMode.Night)) return;
            var active = SceneManager.GetActiveScene();
            var preview = new SceneSavePreview { Scene = scene, OriginalActiveScene = active,
                OriginalActiveEnvironment = active != scene ? CaptureEnvironment() : null,
                Controllers = rooms, Modes = rooms.Select(room => room.PreviewMode).ToArray(),
                Lights = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true)).Select(CapturePreview).ToArray(),
                Cameras = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Select(camera => new CameraPreview {
                    Camera = camera, ClearFlags = camera.clearFlags, Background = camera.backgroundColor, DepthMode = camera.depthTextureMode }).ToArray() };
            SavingPreviews.Add(scene.handle, preview);
            bool previous = IsApplying; IsApplying = true;
            try
            {
                // RenderSettings belongs to the active scene. Its night fog/skybox must also be cleared before saving an inactive room.
                if (active != scene && !SceneManager.SetActiveScene(scene))
                    throw new InvalidOperationException("保存夜间预览前无法激活目标房间场景。");
                preview.Environment = CaptureEnvironment();
                // Scene files store the day baseline. Night stays exclusively in the profile, including lights without overrides.
                foreach (var room in rooms) room.SetPreviewMode(RoomLightingPreviewMode.Day);
            }
            catch
            { RestoreSavingPreview(scene.handle); throw; }
            finally { IsApplying = previous; }
            // sceneSaved is not raised after a failed save; restore on the next Editor update as a fallback.
            EditorApplication.delayCall -= RestoreUnfinishedSceneSaves;
            EditorApplication.delayCall += RestoreUnfinishedSceneSaves;
        }

        private static void OnSceneSaved(Scene scene) => RestoreSavingPreview(scene.handle);

        private static void RestoreUnfinishedSceneSaves()
        {
            foreach (int handle in SavingPreviews.Keys.ToArray()) RestoreSavingPreview(handle);
        }

        private static void RestoreSavingPreview(int handle)
        {
            if (!SavingPreviews.TryGetValue(handle, out var preview)) return;
            if (!preview.Scene.IsValid() || !preview.Scene.isLoaded) { SavingPreviews.Remove(handle); return; }
            bool previous = IsApplying; IsApplying = true;
            try
            {
                for (int index = 0; index < preview.Controllers.Length; index++)
                    if (preview.Controllers[index] != null) preview.Controllers[index].SetPreviewMode(preview.Modes[index]);
                // Multiple controllers can touch the same scene. Restore its exact visible state after all mode callbacks.
                foreach (var item in preview.Lights) RestoreLightPreview(item);
                foreach (var item in preview.Cameras)
                    if (item.Camera != null) { item.Camera.clearFlags = item.ClearFlags; item.Camera.backgroundColor = item.Background; item.Camera.depthTextureMode = item.DepthMode; }
                if (preview.Environment != null && preview.Scene == SceneManager.GetActiveScene()) RestoreEnvironment(preview.Environment);
                if (preview.OriginalActiveScene.IsValid() && preview.OriginalActiveScene.isLoaded
                    && preview.OriginalActiveScene != SceneManager.GetActiveScene()) SceneManager.SetActiveScene(preview.OriginalActiveScene);
                if (preview.OriginalActiveEnvironment != null && preview.OriginalActiveScene == SceneManager.GetActiveScene())
                    RestoreEnvironment(preview.OriginalActiveEnvironment);
            }
            finally { SavingPreviews.Remove(handle); IsApplying = previous; }
            SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
        }

        private static LightPreview CapturePreview(Light light) => new LightPreview { Light = light,
            Settings = new SceneLightSettings { dayEnabled = light.enabled, intensity = light.intensity, color = light.color,
                localPosition = light.transform.localPosition, range = light.range, type = light.type,
                spotAngle = light.spotAngle, innerSpotAngle = light.innerSpotAngle, shadows = light.shadows,
                shadowStrength = light.shadowStrength, shadowBias = light.shadowBias, shadowNormalBias = light.shadowNormalBias },
            Rotation = light.transform.localRotation, RenderMode = light.renderMode, ShadowResolution = light.shadowCustomResolution, Cookie = light.cookie };

        private static void RestoreLightPreview(LightPreview item)
        {
            var light = item.Light; if (light == null) return;
            var value = item.Settings;
            light.transform.localPosition = value.localPosition; light.transform.localRotation = item.Rotation;
            light.type = value.type; light.range = value.range; light.spotAngle = value.spotAngle; light.innerSpotAngle = value.innerSpotAngle;
            light.shadows = value.shadows; light.shadowStrength = value.shadowStrength;
            light.shadowBias = value.shadowBias; light.shadowNormalBias = value.shadowNormalBias;
            light.color = value.color; light.intensity = value.intensity; light.enabled = value.dayEnabled;
            light.renderMode = item.RenderMode; light.shadowCustomResolution = item.ShadowResolution; light.cookie = item.Cookie;
        }

        private static EnvironmentPreview CaptureEnvironment() => new EnvironmentPreview { Mode = RenderSettings.ambientMode,
            Ambient = RenderSettings.ambientLight, Sky = RenderSettings.ambientSkyColor, Equator = RenderSettings.ambientEquatorColor,
            Ground = RenderSettings.ambientGroundColor, Intensity = RenderSettings.ambientIntensity, Reflection = RenderSettings.reflectionIntensity,
            Probe = RenderSettings.ambientProbe, Fog = RenderSettings.fog, FogColor = RenderSettings.fogColor, Skybox = RenderSettings.skybox };

        private static void RestoreEnvironment(EnvironmentPreview value)
        {
            RenderSettings.ambientMode = value.Mode; RenderSettings.ambientLight = value.Ambient;
            RenderSettings.ambientSkyColor = value.Sky; RenderSettings.ambientEquatorColor = value.Equator; RenderSettings.ambientGroundColor = value.Ground;
            RenderSettings.ambientIntensity = value.Intensity; RenderSettings.reflectionIntensity = value.Reflection; RenderSettings.ambientProbe = value.Probe;
            RenderSettings.fog = value.Fog; RenderSettings.fogColor = value.FogColor; RenderSettings.skybox = value.Skybox;
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            RestoreSavingPreview(scene.handle);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            ResetScenePreviews(scene);
        }

        private static void OnActiveSceneChangedInEditMode(Scene previousScene, Scene nextScene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !nextScene.IsValid() || !nextScene.isLoaded) return;
            bool previous = IsApplying; IsApplying = true;
            try
            {
                // The runtime Update handle check can miss an A -> B -> A switch within one Editor update.
                foreach (var room in GetLoadedControllers().Where(room => room.gameObject.scene == nextScene && room.Profile != null))
                    room.ApplyFromProfile();
            }
            finally { IsApplying = previous; }
            SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void PrepareForReloadOrQuit()
        {
            SavePendingChanges();
            EditorApplication.delayCall -= RestoreUnfinishedSceneSaves;
            SavingPreviews.Clear();
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var scene in GetLoadedControllers().Select(room => room.gameObject.scene).Distinct()) ResetScenePreviews(scene);
        }

        private static void ResetScenePreviews(Scene scene)
        {
            bool previous = IsApplying; IsApplying = true;
            try
            {
                foreach (var room in GetLoadedControllers().Where(room => room.gameObject.scene == scene))
                { EditingPeriods.Remove(room); room.SetPreviewMode(RoomLightingPreviewMode.Automatic); }
            }
            finally { IsApplying = previous; }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode) SavePendingChanges();
            if (state != PlayModeStateChange.EnteredEditMode && state != PlayModeStateChange.EnteredPlayMode) return;
            // Scene objects are available at EnteredEditMode. A delayCall can arrive after several Editor updates, so apply immediately too.
            ReapplyLoadedRoomsAfterModeChange();
            EditorApplication.delayCall -= ReapplyLoadedRoomsAfterModeChange;
            EditorApplication.delayCall += ReapplyLoadedRoomsAfterModeChange;
        }

        private static void ReapplyLoadedRoomsAfterModeChange()
        {
            var profiles = Resources.FindObjectsOfTypeAll<RoomLightingController>().Where(room => room != null
                && room.gameObject.scene.IsValid() && room.gameObject.scene.isLoaded && room.Profile != null)
                .SelectMany(room => new[] { room.Profile, room.NightProfile }).Where(profile => profile != null).Distinct().ToArray();
            foreach (var profile in profiles) ApplyProfile(profile);
        }

        public static RoomLightingController[] GetLoadedControllers() => Resources.FindObjectsOfTypeAll<RoomLightingController>()
            .Where(room => room != null && room.gameObject.scene.IsValid() && room.gameObject.scene.isLoaded)
            .OrderBy(room => room.gameObject.scene.path, StringComparer.Ordinal).ThenBy(room => room.name, StringComparer.Ordinal).ToArray();

        public static RoomLightingController FindController()
        {
            var selected = FindControllerForObject(Selection.activeObject);
            if (selected != null) return selected;
            var rooms = GetLoadedControllers();
            var active = rooms.Where(room => room.gameObject.scene == SceneManager.GetActiveScene()).ToArray();
            return active.Length == 1 ? active[0] : rooms.Length == 1 ? rooms[0] : null;
        }

        public static RoomLightingController FindControllerForObject(UnityEngine.Object target)
        {
            var selected = target as GameObject ?? (target as Component)?.gameObject;
            if (selected == null || !selected.scene.IsValid() || !selected.scene.isLoaded) return null;
            var parent = selected.GetComponentInParent<RoomLightingController>();
            if (parent != null) return parent;
            var rooms = GetLoadedControllers().Where(room => room.gameObject.scene == selected.scene).ToArray();
            return rooms.Length == 1 ? rooms[0] : null;
        }

        public static void SetEditingPeriod(RoomLightingController controller, RoomLightingPeriod period)
        { if (controller != null) EditingPeriods[controller] = period; }
        public static RoomLightingPeriod GetEditingPeriod(RoomLightingController controller, RoomLightingPeriod fallback = RoomLightingPeriod.Day)
            => controller != null && EditingPeriods.TryGetValue(controller, out var period) ? period : fallback;
        public static void ClearEditingPeriod(RoomLightingController controller)
        { if (controller != null) EditingPeriods.Remove(controller); }

        public static Light[] GetEditableLights(RoomLightingController controller) => GetEditableLights(controller, RoomLightingPeriod.Day);
        public static Light[] GetEditableLights(RoomLightingController controller, RoomLightingPeriod period) => controller != null
            ? controller.GetEditableSceneLights(period) : Array.Empty<Light>();
        public static Light ResolveLight(RoomLightingController controller, string hierarchyPath)
            => ResolveLight(controller, hierarchyPath, RoomLightingPeriod.Day);
        public static Light ResolveLight(RoomLightingController controller, string hierarchyPath, RoomLightingPeriod period)
        {
            if (controller == null || string.IsNullOrEmpty(hierarchyPath)) return null;
            var light = controller.ResolveSceneLight(hierarchyPath, period);
            return light != null && controller.IsEditableSceneLight(light, period) ? light : null;
        }
        public static SceneLightSettings ReadLight(RoomLightingController controller, Light light)
            => ReadLight(controller, light, RoomLightingPeriod.Day);
        public static SceneLightSettings ReadLight(RoomLightingController controller, Light light, RoomLightingPeriod period)
        {
            ValidateLight(controller, light, period);
            return controller.CaptureSceneLight(light, period);
        }
        public static void SelectLight(RoomLightingController controller, Light light)
            => SelectLight(controller, light, RoomLightingPeriod.Day);
        public static void SelectLight(RoomLightingController controller, Light light, RoomLightingPeriod period)
        {
            ValidateLight(controller, light, period);
            Selection.activeGameObject = light.gameObject;
            EditorGUIUtility.PingObject(light);
        }

        public static void WriteLight(RoomLightingController controller, Light light, SceneLightSettings settings)
            => WriteLight(controller, light, settings, RoomLightingPeriod.Day);
        public static void WriteLight(RoomLightingController controller, Light light, SceneLightSettings settings, RoomLightingPeriod period)
            => WriteLightCore(controller, light, settings, period, true);

        private static void WriteLightCore(RoomLightingController controller, Light light, SceneLightSettings settings,
            RoomLightingPeriod period, bool recordSceneObjects)
        {
            ValidateLight(controller, light, period);
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var profile = controller.GetProfile(period);
            RequirePersistent(profile);
            var value = settings.Copy(); value.HierarchyPath = controller.GetHierarchyPath(light);
            var entries = controller.GetSceneLightSettingsList(period);
            if (!entries.Any(item => item != null && item.HierarchyPath == value.HierarchyPath))
                FirstOverrides[(light, period)] = new LightBaseline { Controller = controller, Profile = profile,
                    Period = period, Value = ReadLight(controller, light, period) };
            // Native Inspector/Gizmo edits already own their scene-object Undo records; recording them again can feed nighttime zeros back.
            if (recordSceneObjects) Undo.RecordObjects(new UnityEngine.Object[] { profile, light, light.transform }, "调整场景灯光");
            else Undo.RecordObject(profile, "调整场景灯光");
            int index = entries.FindIndex(item => item != null && item.HierarchyPath == value.HierarchyPath);
            if (index >= 0) entries[index] = value; else entries.Add(value);
            ProfileEdited(profile);
        }

        public static void ProfileEdited(RoomLightingProfile profile)
        {
            RequirePersistent(profile);
            profile.NotifyChanged(); EditorUtility.SetDirty(profile); Pending.Add(profile);
            ApplyProfile(profile);
        }
        private static void OnGamePanelProfileEdited(RoomLightingProfile profile)
        {
            // The Game panel has already validated and incremented the profile revision.
            if (profile == null || !EditorUtility.IsPersistent(profile)) return;
            EditorUtility.SetDirty(profile); Pending.Add(profile); ApplyProfile(profile);
        }
        public static void SaveProfile(RoomLightingProfile profile)
        {
            RequirePersistent(profile);
            AssetDatabase.SaveAssetIfDirty(profile); Pending.Remove(profile);
            if (profile.ScreenStyle != null && EditorUtility.IsPersistent(profile.ScreenStyle))
                AssetDatabase.SaveAssetIfDirty(profile.ScreenStyle);
        }
        public static void SavePendingChanges()
        {
            foreach (var profile in Pending.ToArray())
            {
                if (profile != null && EditorUtility.IsPersistent(profile)) SaveProfile(profile);
                else Pending.Remove(profile);
            }
        }
        private static void ApplyProfile(RoomLightingProfile profile)
        {
            bool previous = IsApplying; IsApplying = true;
            try
            {
                foreach (var room in Resources.FindObjectsOfTypeAll<RoomLightingController>().Where(room => room != null
                    && room.gameObject.scene.IsValid() && room.gameObject.scene.isLoaded
                    && (room.Profile == profile || room.NightProfile == profile))) room.ApplyFromProfile();
            }
            finally { IsApplying = previous; }
            SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
        }
        private static void OnUndoRedo()
        {
            var profiles = Resources.FindObjectsOfTypeAll<RoomLightingController>().Where(room => room != null
                && room.gameObject.scene.IsValid() && room.gameObject.scene.isLoaded && room.Profile != null)
                .SelectMany(room => new[] { room.Profile, room.NightProfile }).Where(profile => profile != null).Distinct().ToArray();
            foreach (var profile in profiles)
            {
                if (EditorUtility.IsPersistent(profile) && EditorUtility.IsDirty(profile)) Pending.Add(profile);
                ApplyProfile(profile);
                RestoreRemovedOverrideBaselines(profile);
            }
        }

        private static void RestoreRemovedOverrideBaselines(RoomLightingProfile profile)
        {
            foreach (var key in FirstOverrides.Keys.Where(key => key.Light == null).ToArray()) FirstOverrides.Remove(key);
            foreach (var pair in FirstOverrides.Where(pair => pair.Value.Profile == profile))
            {
                var baseline = pair.Value;
                if (baseline.Controller != null && baseline.Controller.HasIndependentProfiles) continue;
                if (baseline.Controller == null || baseline.Controller.GetSceneLightSettingsList(baseline.Period)
                    .Any(item => item != null && item.HierarchyPath == baseline.Value.HierarchyPath)) continue;
                if (baseline.Period != RoomLightingPeriod.Day) continue;
                // Undo restores the Light/Transform. Its retained daytime values also need restoring when an empty list resumes scene defaults.
                var scene = baseline.Controller.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded) continue;
                var bedtime = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<BedtimeGuidance>(true)).FirstOrDefault();
                if (bedtime == null) continue;
                bedtime.UpdateDaylightLight(pair.Key.Light, baseline.Value.dayEnabled, baseline.Value.intensity, baseline.Value.color);
                bedtime.ReapplyLighting();
            }
        }

        /// <summary>Consumes explicit native Inspector/Gizmo Undo properties only; never samples a faded intensity.</summary>
        public static bool SyncNativeModifications(RoomLightingController controller, Light light, UndoPropertyModification[] modifications)
            => SyncNativeModifications(controller, light, modifications, RoomLightingPeriod.Day);
        public static bool SyncNativeModifications(RoomLightingController controller, Light light, UndoPropertyModification[] modifications,
            RoomLightingPeriod period)
        {
            ValidateLight(controller, light, period);
            if (IsApplying || modifications == null) return false;
            var value = ReadLight(controller, light, period);
            bool changed = false, rotationChanged = false, eulerChanged = false;
            var rotation = light.transform.localRotation;
            foreach (var modification in modifications)
            {
                var property = modification.currentValue;
                if (property == null || property.target != light && property.target != light.transform) continue;
                if (!float.TryParse(property.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number)
                    || float.IsNaN(number) || float.IsInfinity(number)) continue;
                string path = property.propertyPath;
                bool recognized = false;
                if (property.target == light.transform)
                {
                    if (path.StartsWith("m_LocalPosition.", StringComparison.Ordinal)) recognized = SetVectorPart(ref value.localPosition, path, number);
                    else if (path.StartsWith("m_LocalEulerAnglesHint.", StringComparison.Ordinal))
                    { recognized = SetVectorPart(ref value.localEuler, path, number); eulerChanged |= recognized; }
                    else if (path.StartsWith("m_LocalRotation.", StringComparison.Ordinal))
                    { recognized = SetRotationPart(ref rotation, path, number); rotationChanged |= recognized; }
                }
                else
                {
                    switch (path)
                    {
                        case "m_Enabled": value.dayEnabled = number != 0; recognized = true; break;
                        case "m_Intensity":
                            float multiplier = controller.HasIndependentProfiles ? controller.GetProfile(period).lightMultiplier : 1;
                            value.intensity = multiplier > .0001f ? number / multiplier : number;
                            recognized = true; break;
                        case "m_Range": value.range = number; recognized = true; break;
                        case "m_Type": value.type = (LightType)(int)number; recognized = true; break;
                        case "m_SpotAngle": value.spotAngle = number; recognized = true; break;
                        case "m_InnerSpotAngle": value.innerSpotAngle = number; recognized = true; break;
                        case "m_Shadows.m_Type": value.shadows = (LightShadows)(int)number; recognized = true; break;
                        case "m_Shadows.m_Bias": value.shadowBias = number; recognized = true; break;
                        case "m_Shadows.m_NormalBias": value.shadowNormalBias = number; recognized = true; break;
                        case "m_Shadows.m_Strength": value.shadowStrength = number; recognized = true; break;
                        case "m_Color.r": value.color.r = number; recognized = true; break;
                        case "m_Color.g": value.color.g = number; recognized = true; break;
                        case "m_Color.b": value.color.b = number; recognized = true; break;
                        case "m_Color.a": value.color.a = number; recognized = true; break;
                    }
                }
                changed |= recognized;
            }
            if (!changed) return false;
            if (rotationChanged && !eulerChanged) value.localEuler = rotation.eulerAngles;
            WriteLightCore(controller, light, value, period, false);
            return true;
        }

        private static UndoPropertyModification[] OnNativeModifications(UndoPropertyModification[] modifications)
        {
            if (synchronizingNative || IsApplying || modifications == null) return modifications;
            // Use the modified object, not Selection: a locked Inspector can edit an unselected light.
            var lights = modifications.Where(item => item.currentValue != null).Select(item => item.currentValue.target)
                .Select(target => target as Light ?? (target as Transform)?.GetComponent<Light>()).Where(light => light != null).Distinct().ToArray();
            synchronizingNative = true;
            try
            {
                foreach (var light in lights)
                {
                    var room = FindControllerForObject(light);
                    if (room == null) continue;
                    var period = EditingPeriods.TryGetValue(room, out var editing) ? editing : room.EffectivePeriod;
                    var profile = room.GetProfile(period);
                    if (profile == null || !EditorUtility.IsPersistent(profile)) continue;
                    // Our own profile changes also record the Light for first-entry Undo. Do not feed them back.
                    if (modifications.Any(item => item.currentValue != null
                        && (item.currentValue.target == room.Profile || item.currentValue.target == room.NightProfile))) continue;
                    if (!room.IsEditableSceneLight(light, period)) continue;
                    var relevant = modifications.Where(item => item.currentValue != null
                        && (item.currentValue.target == light || item.currentValue.target == light.transform)).ToArray();
                    SyncNativeModifications(room, light, relevant, period);
                }
            }
            catch (Exception exception) { Debug.LogWarning("灯光调整尚未写入配置：" + exception.Message); }
            finally { synchronizingNative = false; }
            return modifications;
        }

        private static bool SetVectorPart(ref Vector3 vector, string path, float value)
        {
            if (path.EndsWith(".x", StringComparison.Ordinal)) vector.x = value;
            else if (path.EndsWith(".y", StringComparison.Ordinal)) vector.y = value;
            else if (path.EndsWith(".z", StringComparison.Ordinal)) vector.z = value;
            else return false;
            return true;
        }
        private static bool SetRotationPart(ref Quaternion rotation, string path, float value)
        {
            if (path.EndsWith(".x", StringComparison.Ordinal)) rotation.x = value;
            else if (path.EndsWith(".y", StringComparison.Ordinal)) rotation.y = value;
            else if (path.EndsWith(".z", StringComparison.Ordinal)) rotation.z = value;
            else if (path.EndsWith(".w", StringComparison.Ordinal)) rotation.w = value;
            else return false;
            return true;
        }
        private static void ValidateLight(RoomLightingController controller, Light light, RoomLightingPeriod period)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (light == null) throw new ArgumentNullException(nameof(light));
            if (!controller.IsEditableSceneLight(light, period)) throw new ArgumentException("该灯不属于当前房间或当前时段可编辑的光源。", nameof(light));
        }
        private static void RequirePersistent(RoomLightingProfile profile)
        {
            if (profile == null || !EditorUtility.IsPersistent(profile))
                throw new InvalidOperationException("请绑定 Project 中的房间灯光配置资产；临时运行配置不能保存策划调整。");
        }
    }
}
