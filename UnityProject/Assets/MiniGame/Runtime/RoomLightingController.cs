using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    [ExecuteAlways, DefaultExecutionOrder(-250), DisallowMultipleComponent]
    [AddComponentMenu("MiniGame/画面/房间灯光")]
    public sealed class RoomLightingController : MonoBehaviour
    {
        [SerializeField] private RoomLightingProfile profile;
        [SerializeField] private RoomLightingProfile nightProfile;
        [SerializeField] private Light ceiling, window, fill, exterior, monitor;
        [SerializeField] private Renderer floor, lamp, rug;
        [SerializeField] private int bulbSlot;
        [SerializeField] private Renderer[] roomSurfaces;
        [SerializeField] private BedtimeGuidance bedtime;
        private MaterialPropertyBlock properties;
        private Texture2D coneCookie;
        private float cookieInner = -1, cookieOuter = -1;
        private RoomLightingProfile appliedProfile;
        private RoomLightingProfile appliedNightProfile;
        private int appliedRevision = -1;
        private int appliedNightRevision = -1;
        private int lastActiveSceneHandle = int.MinValue;
        private bool lastPreview;
        private readonly Dictionary<Light, SceneLightSettings> originalLights = new Dictionary<Light, SceneLightSettings>();
        private readonly Dictionary<Light, SceneLightSettings> dayLights = new Dictionary<Light, SceneLightSettings>();
        public RoomLightingProfile Profile => profile;
        public RoomLightingProfile NightProfile => nightProfile;
        public float NightBlend
        {
            get
            {
                var guidance = GetBedtimeGuidance();
                return guidance != null && guidance.isActiveAndEnabled ? Mathf.SmoothStep(0, 1, guidance.Isolation)
                    : PreviewMode == RoomLightingPreviewMode.Night ? 1 : 0;
            }
        }
        public bool HasIndependentProfiles => profile != null && nightProfile != null && profile != nightProfile;
        public RoomLightingProfile GetProfile(RoomLightingPeriod period)
        {
            if (period != RoomLightingPeriod.Night || nightProfile == null) return profile;
            // Serialized references can bypass ConfigureProfiles; never expose the day asset as an editable night asset.
            return nightProfile == profile ? null : nightProfile;
        }
        public void ConfigureProfiles(RoomLightingProfile day, RoomLightingProfile night)
        {
            if (day != null && ReferenceEquals(day, night))
                throw new ArgumentException("Day and night lighting must use separate profile assets.", nameof(night));
            profile = day; nightProfile = night;
            ApplyNow();
        }
        public Light MainLight => ceiling;
        public Light WindowLight => window;
        public Light MonitorLight => monitor;
        public Renderer Rug => rug;
        public bool PreviewUnbanded { get; set; }
        public RoomLightingPreviewMode PreviewMode { get; private set; }
        public RoomLightingPeriod EffectivePeriod => PreviewMode == RoomLightingPreviewMode.Night
            || PreviewMode == RoomLightingPreviewMode.Automatic && GetBedtimeGuidance() != null
            && GetBedtimeGuidance().OwnsLighting ? RoomLightingPeriod.Night : RoomLightingPeriod.Day;

        public void SetPreviewMode(RoomLightingPreviewMode mode)
        {
            PreviewMode = Enum.IsDefined(typeof(RoomLightingPreviewMode), mode) ? mode : RoomLightingPreviewMode.Automatic;
            ApplyNow();
        }

        public List<SceneLightSettings> GetSceneLightSettingsList(RoomLightingPeriod period)
        {
            if (profile == null) return null;
            if (HasIndependentProfiles)
            {
                var selected = GetProfile(period);
                return selected.SceneLights ??= new List<SceneLightSettings>();
            }
            if (period == RoomLightingPeriod.Day) return profile.SceneLights ??= new List<SceneLightSettings>();
            profile.night ??= new RoomNightLightingSettings();
            return profile.night.SceneLights ??= new List<SceneLightSettings>();
        }
        public List<SceneLightSettings> GetSceneLightSettingsList() => GetSceneLightSettingsList(RoomLightingPeriod.Day);

        public Light[] GetEditableSceneLights() => GetEditableSceneLights(RoomLightingPeriod.Day);
        public Light[] GetEditableSceneLights(RoomLightingPeriod period)
        {
            var scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded) return Array.Empty<Light>();
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
                .Where(light => IsEditableSceneLight(light, period)).OrderBy(GetHierarchyPath, StringComparer.Ordinal).ToArray();
        }

        public bool IsEditableSceneLight(Light light) => IsEditableSceneLight(light, RoomLightingPeriod.Day);
        public bool IsEditableSceneLight(Light light, RoomLightingPeriod period)
        {
            if (light == null || light.gameObject.scene != gameObject.scene || !gameObject.scene.IsValid()
                || !gameObject.scene.isLoaded || light.GetComponents<Light>().Length != 1
                || (light.hideFlags & HideFlags.DontSave) != 0 || (light.gameObject.hideFlags & HideFlags.DontSave) != 0) return false;
            if (!HasIndependentProfiles && period == RoomLightingPeriod.Day)
            {
                var guidance = GetBedtimeGuidance();
                if (light == ceiling || light == window || light == fill || light == exterior || light == monitor
                    || light == (guidance != null ? guidance.Spotlight : null)) return false;
            }
            return light.type == LightType.Point || light.type == LightType.Spot || light.type == LightType.Directional;
        }

        public string GetHierarchyPath(Light light)
        {
            if (light == null || light.gameObject.scene != gameObject.scene) return "";
            var parts = new List<string>();
            for (var target = light.transform; target != null; target = target.parent)
            {
                int ordinal = 0;
                foreach (var sibling in Siblings(target))
                {
                    if (sibling == target) break;
                    if (sibling.name == target.name) ordinal++;
                }
                parts.Add(Uri.EscapeDataString(target.name) + "#" + ordinal.ToString(CultureInfo.InvariantCulture));
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        public Light ResolveSceneLight(string path) => ResolveSceneLight(path, RoomLightingPeriod.Day);
        public Light ResolveSceneLight(string path, RoomLightingPeriod period)
        {
            var scene = gameObject.scene;
            if (string.IsNullOrEmpty(path) || !scene.IsValid() || !scene.isLoaded) return null;
            Transform parent = null;
            foreach (string segment in path.Split('/'))
            {
                int separator = segment.LastIndexOf('#');
                if (separator < 0 || !int.TryParse(segment.Substring(separator + 1), NumberStyles.None,
                    CultureInfo.InvariantCulture, out int ordinal) || ordinal < 0) return null;
                string name;
                try { name = Uri.UnescapeDataString(segment.Substring(0, separator)); }
                catch (UriFormatException) { return null; }
                var candidates = parent == null ? scene.GetRootGameObjects().Select(root => root.transform) : parent.Cast<Transform>();
                parent = candidates.Where(item => item.name == name).Skip(ordinal).FirstOrDefault();
                if (parent == null) return null;
            }
            var light = parent != null ? parent.GetComponent<Light>() : null;
            return IsEditableSceneLight(light, period) ? light : null;
        }

        public SceneLightSettings CaptureSceneLight(Light light) => CaptureSceneLight(light, RoomLightingPeriod.Day);
        public SceneLightSettings CaptureSceneLight(Light light, RoomLightingPeriod period)
        {
            if (!IsEditableSceneLight(light, period)) return null;
            if (HasIndependentProfiles)
            {
                // Editors display authored values; the overall multiplier is applied only when presenting the light.
                var complete = BuildPeriodLights(GetProfile(period), period, false);
                return complete.TryGetValue(light, out var value) ? value.Copy() : null;
            }
            string path = GetHierarchyPath(light);
            var stored = GetSceneLightSettingsList(period)?.FirstOrDefault(item => item != null && item.HierarchyPath == path);
            if (stored != null) { var copy = stored.Copy(); copy.ValidateValues(); return copy; }
            var day = GetDaySettings(light);
            if (period == RoomLightingPeriod.Day) return day;
            var guidance = GetBedtimeGuidance();
            if (guidance != null && light == guidance.Spotlight)
            { day.dayEnabled = true; day.intensity = guidance.DefaultSpotIntensity; return day; }
            float multiplier = profile != null && profile.night != null ? profile.night.lightMultiplier : 0;
            day.intensity *= multiplier; day.dayEnabled &= day.intensity > 0;
            return day;
        }

        private SceneLightSettings GetDaySettings(Light light)
        {
            if (dayLights.TryGetValue(light, out var retained)) return retained.Copy();
            if (!originalLights.TryGetValue(light, out var original))
            {
                original = ReadLight(light);
                var guidance = GetBedtimeGuidance();
                if (guidance != null && guidance.TryGetDaylightSettings(light, out var previous)) original = previous;
                original.HierarchyPath = GetHierarchyPath(light);
                originalLights.Add(light, original.Copy());
            }
            var result = original.Copy();
            var stored = profile?.SceneLights?.FirstOrDefault(item => item != null && item.HierarchyPath == result.HierarchyPath);
            if (stored != null && IsEditableSceneLight(light)) result = stored.Copy();
            result.ValidateValues(); return result;
        }

        private SceneLightSettings GetOriginalSettings(Light light)
        {
            if (!originalLights.TryGetValue(light, out var original))
            {
                original = ReadLight(light);
                var guidance = GetBedtimeGuidance();
                if (guidance != null && guidance.TryGetDaylightSettings(light, out var previous)) original = previous;
                original.HierarchyPath = GetHierarchyPath(light);
                originalLights.Add(light, original.Copy());
            }
            return original.Copy();
        }

        private Dictionary<Light, SceneLightSettings> BuildPeriodLights(RoomLightingProfile p, RoomLightingPeriod period, bool applyMultiplier = true)
        {
            var result = new Dictionary<Light, SceneLightSettings>();
            foreach (var light in GetEditableSceneLights(period))
            {
                string path = GetHierarchyPath(light);
                var saved = p.SceneLightDefaults?.FirstOrDefault(item => item != null && item.HierarchyPath == path);
                var settings = saved != null ? saved.Copy() : GetOriginalSettings(light);
                settings.ValidateValues(); result.Add(light, settings);
            }
            if (ceiling != null && lamp != null && result.TryGetValue(ceiling, out var main))
            {
                var position = new Vector3(lamp.bounds.center.x, lamp.bounds.min.y, lamp.bounds.center.z) + p.lampOffset;
                main.localPosition = ceiling.transform.parent != null ? ceiling.transform.parent.InverseTransformPoint(position) : position;
                var rotation = Quaternion.Euler(RoomLightingProfile.SafeEuler(p.lampEulerAngles, new Vector3(90, 0, 0)));
                main.localEuler = (ceiling.transform.parent != null ? Quaternion.Inverse(ceiling.transform.parent.rotation) * rotation : rotation).eulerAngles;
                main.type = LightType.Spot; main.range = p.lampRange; main.spotAngle = p.outerAngle; main.innerSpotAngle = p.innerAngle;
                main.shadows = LightShadows.Soft; main.shadowStrength = p.shadowStrength;
                main.shadowBias = p.shadowBias; main.shadowNormalBias = p.shadowNormalBias;
                main.dayEnabled = true; main.intensity = p.lampIntensity; main.color = p.lampColor;
                ceiling.renderMode = LightRenderMode.ForcePixel; ceiling.shadowCustomResolution = 2048;
            }
            if (window != null && result.TryGetValue(window, out var windowSettings))
            {
                windowSettings.range = p.windowRange; windowSettings.shadows = LightShadows.Soft;
                windowSettings.intensity = p.windowIntensity; windowSettings.dayEnabled = p.windowIntensity > 0; windowSettings.color = p.windowColor;
                window.renderMode = LightRenderMode.ForcePixel;
            }
            foreach (var light in new[] { fill, exterior })
                if (light != null && result.TryGetValue(light, out var settings)) { settings.dayEnabled = false; settings.intensity = 0; }
            if (monitor != null && result.TryGetValue(monitor, out var monitorSettings))
            { monitorSettings.dayEnabled = p.monitorIntensity > 0; monitorSettings.intensity = p.monitorIntensity; }
            var guidance = GetBedtimeGuidance();
            if (guidance != null && guidance.Spotlight != null && result.TryGetValue(guidance.Spotlight, out var bed)
                && !(p.SceneLightDefaults?.Any(item => item != null && item.HierarchyPath == bed.HierarchyPath) ?? false))
            {
                bed.dayEnabled = period == RoomLightingPeriod.Night;
                bed.intensity = period == RoomLightingPeriod.Night ? guidance.DefaultSpotIntensity : 0;
            }
            foreach (var entry in p.SceneLights ?? Enumerable.Empty<SceneLightSettings>())
            {
                if (entry == null) continue;
                var light = ResolveSceneLight(entry.HierarchyPath, period);
                if (light == null) continue;
                var copy = entry.Copy(); copy.ValidateValues(); result[light] = copy;
            }
            if (applyMultiplier)
                foreach (var entry in result.Values)
                { entry.intensity *= p.lightMultiplier; entry.dayEnabled &= entry.intensity > 0; }
            return result;
        }

        private SceneLightSettings ReadLight(Light light) => new SceneLightSettings
        {
            HierarchyPath = GetHierarchyPath(light), dayEnabled = light.enabled, localPosition = light.transform.localPosition,
            localEuler = light.transform.localEulerAngles, color = light.color, intensity = light.intensity,
            range = light.range, type = light.type, spotAngle = light.spotAngle, innerSpotAngle = light.innerSpotAngle,
            shadows = light.shadows, shadowStrength = light.shadowStrength, shadowBias = light.shadowBias, shadowNormalBias = light.shadowNormalBias
        };

        internal static void ApplyLight(Light light, SceneLightSettings settings)
        {
            if (light == null || settings == null) return;
            light.transform.localPosition = settings.localPosition; light.transform.localRotation = Quaternion.Euler(settings.localEuler);
            light.type = settings.type; light.range = settings.range;
            light.spotAngle = settings.spotAngle; light.innerSpotAngle = settings.innerSpotAngle;
            light.shadows = settings.shadows; light.shadowStrength = settings.shadowStrength;
            light.shadowBias = settings.shadowBias; light.shadowNormalBias = settings.shadowNormalBias;
            light.color = settings.color; light.intensity = settings.intensity; light.enabled = settings.dayEnabled;
        }

        internal void ApplyPresentedLight(Light light, SceneLightSettings settings)
        {
            ApplyLight(light, settings);
            if (light != ceiling) return;
            if (settings.type == LightType.Spot)
            { UpdateCookie(settings.innerSpotAngle, settings.spotAngle); light.cookie = coneCookie; }
            else light.cookie = null;
        }

        private static IEnumerable<Transform> Siblings(Transform target) => target.parent != null
            ? target.parent.Cast<Transform>() : target.gameObject.scene.GetRootGameObjects().Select(root => root.transform);

        private BedtimeGuidance GetBedtimeGuidance()
        {
            if (bedtime != null && bedtime.gameObject.scene == gameObject.scene) return bedtime;
            var scene = gameObject.scene;
            if (scene.IsValid() && scene.isLoaded) bedtime = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<BedtimeGuidance>(true)).FirstOrDefault();
            return bedtime;
        }

        public void Configure(RoomLightingProfile settings, Light main, Light windowLight, Light fillLight,
            Light exteriorLight, Light monitorLight, Renderer floorSurface, Renderer lampSurface, int emissionSlot,
            Renderer[] surfaces, Renderer rugSurface, BedtimeGuidance guidance)
        {
            RestorePreview(); originalLights.Clear(); dayLights.Clear();
            profile = settings; ceiling = main; window = windowLight; fill = fillLight; exterior = exteriorLight; monitor = monitorLight;
            floor = floorSurface; lamp = lampSurface; bulbSlot = emissionSlot; roomSurfaces = surfaces; rug = rugSurface; bedtime = guidance;
            ApplyNow();
        }

        public void ApplyFromProfile() => ApplyNow();
        private void OnEnable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            lastActiveSceneHandle = SceneManager.GetActiveScene().handle;
            ApplyNow();
        }
        private void OnActiveSceneChanged(Scene previous, Scene current)
        {
            lastActiveSceneHandle = current.handle;
            if (current == gameObject.scene) ApplyNow();
        }
        private void OnValidate() => appliedRevision = -1;
        private void Update()
        {
            var activeScene = SceneManager.GetActiveScene();
            bool becameActive = activeScene.handle != lastActiveSceneHandle && activeScene == gameObject.scene;
            lastActiveSceneHandle = activeScene.handle;
            // SceneManager.activeSceneChanged is Play-only; the handle check also covers edit-mode Scene changes.
            if (becameActive || profile != appliedProfile || profile != null && profile.Revision != appliedRevision
                || nightProfile != appliedNightProfile || nightProfile != null && nightProfile.Revision != appliedNightRevision || PreviewUnbanded != lastPreview)
                ApplyNow();
        }

        public void ApplyNow()
        {
            if (profile == null) return;
            properties ??= new MaterialPropertyBlock();
            appliedProfile = profile; appliedRevision = profile.Revision; lastPreview = PreviewUnbanded;
            appliedNightProfile = nightProfile; appliedNightRevision = nightProfile != null ? nightProfile.Revision : -1;
            var guidance = GetBedtimeGuidance();
            guidance?.CaptureActiveSceneEnvironment();
            if (HasIndependentProfiles)
            {
                var day = BuildPeriodLights(profile, RoomLightingPeriod.Day);
                var night = BuildPeriodLights(nightProfile, RoomLightingPeriod.Night);
                dayLights.Clear();
                foreach (var item in day) dayLights.Add(item.Key, item.Value);
                if (guidance != null && guidance.isActiveAndEnabled)
                    guidance.ConfigureFullLighting(this, profile, nightProfile, PreviewMode, day, night);
                else
                {
                    bool showNight = PreviewMode == RoomLightingPreviewMode.Night;
                    foreach (var item in showNight ? night : day) ApplyPresentedLight(item.Key, item.Value);
                    ApplyFullSurfaceBlend(showNight ? 1 : 0);
                    ApplyAmbient(showNight ? nightProfile : profile);
                }
                return;
            }
            // Rebuild only after edits, using the retained scene baseline rather than night-faded Light values.
            dayLights.Clear();
            foreach (var light in GetEditableSceneLights(RoomLightingPeriod.Night)) dayLights.Add(light, GetDaySettings(light));
            var p = profile;
            if (ceiling != null && lamp != null && dayLights.TryGetValue(ceiling, out var main))
            {
                var position = new Vector3(lamp.bounds.center.x, lamp.bounds.min.y, lamp.bounds.center.z) + p.lampOffset;
                main.localPosition = ceiling.transform.parent != null ? ceiling.transform.parent.InverseTransformPoint(position) : position;
                var rotation = Quaternion.Euler(RoomLightingProfile.SafeEuler(p.lampEulerAngles, new Vector3(90, 0, 0)));
                main.localEuler = (ceiling.transform.parent != null ? Quaternion.Inverse(ceiling.transform.parent.rotation) * rotation : rotation).eulerAngles;
                main.type = LightType.Spot; main.range = p.lampRange; main.spotAngle = p.outerAngle; main.innerSpotAngle = p.innerAngle;
                main.shadows = LightShadows.Soft; main.shadowStrength = p.shadowStrength;
                main.shadowBias = p.shadowBias; main.shadowNormalBias = p.shadowNormalBias;
                main.dayEnabled = true; main.intensity = p.lampIntensity; main.color = p.lampColor;
                ceiling.renderMode = LightRenderMode.ForcePixel; ceiling.shadowCustomResolution = 2048;
                UpdateCookie(); ceiling.cookie = coneCookie;
            }
            if (window != null && dayLights.TryGetValue(window, out var windowSettings))
            {
                windowSettings.range = p.windowRange; windowSettings.shadows = LightShadows.Soft;
                windowSettings.intensity = p.windowIntensity; windowSettings.dayEnabled = p.windowIntensity > 0; windowSettings.color = p.windowColor;
                window.renderMode = LightRenderMode.ForcePixel;
            }
            foreach (var light in new[] { fill, exterior })
                if (light != null && dayLights.TryGetValue(light, out var settings)) { settings.dayEnabled = false; settings.intensity = 0; }
            if (monitor != null && dayLights.TryGetValue(monitor, out var monitorSettings))
            { monitorSettings.dayEnabled = p.monitorIntensity > 0; monitorSettings.intensity = p.monitorIntensity; }
            foreach (var item in dayLights)
            {
                if (guidance != null && item.Key == guidance.Spotlight) continue;
                guidance?.UpdateDaylightSettings(item.Key, item.Value);
                if (guidance == null || !guidance.OwnsLighting) ApplyPresentedLight(item.Key, item.Value);
            }
            var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(p.AmbientLightColor);
            if (gameObject.scene == SceneManager.GetActiveScene() && (guidance == null || !guidance.OwnsLighting))
            {
                RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = p.AmbientLightColor;
                RenderSettings.ambientSkyColor = RenderSettings.ambientEquatorColor = RenderSettings.ambientGroundColor = p.AmbientLightColor;
                RenderSettings.ambientIntensity = 1; RenderSettings.ambientProbe = probe; RenderSettings.reflectionIntensity = p.reflectionIntensity;
            }
            guidance?.UpdateDaylightAmbient(p.AmbientLightColor, probe, p.reflectionIntensity);
            if (roomSurfaces != null) foreach (var surface in roomSurfaces) ApplySurface(surface);
            if (rug != null && floor != null)
            {
                rug.gameObject.SetActive(p.rugVisible);
                rug.transform.position = new Vector3(floor.bounds.center.x, floor.bounds.max.y, floor.bounds.center.z) + p.rugOffset;
                rug.transform.rotation = Quaternion.Euler(0, p.rugYaw, 0);
                rug.transform.localScale = new Vector3(p.rugSize.x, 1, p.rugSize.y); ApplySurface(rug);
            }
            if (lamp != null && bulbSlot >= 0 && bulbSlot < lamp.sharedMaterials.Length)
            {
                var color = p.lampColor * p.bulbEmission;
                if (guidance == null || !guidance.OwnsLighting)
                { lamp.GetPropertyBlock(properties, bulbSlot); properties.SetColor("_EmissionColor", color); lamp.SetPropertyBlock(properties, bulbSlot); }
                guidance?.UpdateDaylightEmission(lamp, bulbSlot, color);
            }
            guidance?.ConfigureLighting(this, p.night ?? new RoomNightLightingSettings(), PreviewMode, dayLights);
            // ConfigureLighting may capture the room for the first time. Write the authored
            // day environment after that capture, including for an additively loaded room.
            guidance?.UpdateDaylightAmbient(p.AmbientLightColor, probe, p.reflectionIntensity);
            guidance?.ReapplyLighting();
        }

        private void RestorePreview()
        {
            PreviewMode = RoomLightingPreviewMode.Automatic;
            if (bedtime != null) bedtime.ClearLightingPreview(this);
        }

        private void ApplyAmbient(RoomLightingProfile selected)
        {
            if (gameObject.scene != SceneManager.GetActiveScene()) return;
            var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(selected.AmbientLightColor);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = selected.AmbientLightColor;
            RenderSettings.ambientSkyColor = RenderSettings.ambientEquatorColor = RenderSettings.ambientGroundColor = selected.AmbientLightColor;
            RenderSettings.ambientIntensity = 1; RenderSettings.ambientProbe = probe;
            RenderSettings.reflectionIntensity = selected.reflectionIntensity;
        }

        // BedtimeGuidance owns the transition clock. Both endpoints here are complete, independent assets.
        internal void ApplyFullSurfaceBlend(float amount, float visibility = 1)
        {
            if (!HasIndependentProfiles) return;
            properties ??= new MaterialPropertyBlock();
            amount = Mathf.Clamp01(amount);
            var a = profile; var b = nightProfile;
            if (roomSurfaces != null) foreach (var surface in roomSurfaces) ApplySurfaceBlend(surface, a, b, amount);
            if (rug != null && floor != null)
            {
                rug.gameObject.SetActive(amount < .5f ? a.rugVisible : b.rugVisible);
                rug.transform.position = new Vector3(floor.bounds.center.x, floor.bounds.max.y, floor.bounds.center.z)
                    + Vector3.Lerp(a.rugOffset, b.rugOffset, amount);
                rug.transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(a.rugYaw, b.rugYaw, amount), 0);
                var size = Vector2.Lerp(a.rugSize, b.rugSize, amount);
                rug.transform.localScale = new Vector3(size.x, 1, size.y);
                ApplySurfaceBlend(rug, a, b, amount);
            }
            if (lamp != null && bulbSlot >= 0 && bulbSlot < lamp.sharedMaterials.Length)
            {
                lamp.GetPropertyBlock(properties, bulbSlot);
                properties.SetColor("_EmissionColor", Color.Lerp(a.lampColor * a.bulbEmission * a.emissionMultiplier,
                    b.lampColor * b.bulbEmission * b.emissionMultiplier, amount) * visibility);
                lamp.SetPropertyBlock(properties, bulbSlot);
            }
        }

        private void ApplySurfaceBlend(Renderer surface, RoomLightingProfile a, RoomLightingProfile b, float amount)
        {
            if (surface == null) return;
            var materials = surface.sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
            {
                var material = materials[slot];
                if (material == null || !material.HasProperty("_LightingBandsEnabled")) continue;
                surface.GetPropertyBlock(properties, slot);
                var selected = amount < .5f ? a : b;
                properties.SetFloat("_LightingBandsEnabled", selected.lightingBandsEnabled && !PreviewUnbanded ? 1 : 0);
                properties.SetFloat("_LightingBands", Mathf.Lerp(a.lightingBands, b.lightingBands, amount));
                properties.SetFloat("_LightingDither", Mathf.Lerp(a.lightingDither, b.lightingDither, amount));
                properties.SetFloat("_LightingBandsStrength", Mathf.Lerp(a.lightingBandsStrength, b.lightingBandsStrength, amount));
                if (surface == floor)
                {
                    properties.SetFloat("_AlbedoGain", Mathf.Lerp(a.floorGain, b.floorGain, amount));
                    properties.SetFloat("_AlbedoContrast", Mathf.Lerp(a.floorContrast, b.floorContrast, amount));
                    float smoothness = Mathf.Lerp(a.floorSmoothness, b.floorSmoothness, amount);
                    properties.SetFloat("_Glossiness", smoothness); properties.SetFloat("_GlossMapScale", smoothness);
                }
                if (surface == rug) properties.SetColor("_Color", Color.Lerp(a.rugTint, b.rugTint, amount));
                surface.SetPropertyBlock(properties, slot);
            }
        }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            RestorePreview();
        }

        private void ApplySurface(Renderer surface)
        {
            if (surface == null) return;
            var materials = surface.sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
            {
                var material = materials[slot];
                if (material == null || !material.HasProperty("_LightingBandsEnabled")) continue;
                surface.GetPropertyBlock(properties, slot);
                properties.SetFloat("_LightingBandsEnabled", profile.lightingBandsEnabled && !PreviewUnbanded ? 1 : 0);
                properties.SetFloat("_LightingBands", profile.lightingBands); properties.SetFloat("_LightingDither", profile.lightingDither);
                properties.SetFloat("_LightingBandsStrength", profile.lightingBandsStrength);
                if (surface == floor)
                {
                    properties.SetFloat("_AlbedoGain", profile.floorGain); properties.SetFloat("_AlbedoContrast", profile.floorContrast);
                    properties.SetFloat("_Glossiness", profile.floorSmoothness); properties.SetFloat("_GlossMapScale", profile.floorSmoothness);
                }
                if (surface == rug) properties.SetColor("_Color", profile.rugTint);
                surface.SetPropertyBlock(properties, slot);
            }
        }

        private void UpdateCookie() => UpdateCookie(profile.innerAngle, profile.outerAngle);
        private void UpdateCookie(float inner, float outer)
        {
            if (coneCookie != null && cookieInner == inner && cookieOuter == outer) return;
            if (coneCookie == null) coneCookie = new Texture2D(128, 128, TextureFormat.RGBA32, false, true)
                { name = "Room spotlight inner cone", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            cookieInner = inner; cookieOuter = outer;
            float innerRadius = Mathf.Tan(cookieInner * .5f * Mathf.Deg2Rad) / Mathf.Tan(cookieOuter * .5f * Mathf.Deg2Rad);
            var pixels = new Color32[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float radius = new Vector2((x + .5f) / 64 - 1, (y + .5f) / 64 - 1).magnitude;
                float a = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerRadius, .99f, radius));
                pixels[y * 128 + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255));
            }
            coneCookie.SetPixels32(pixels); coneCookie.Apply(false, false);
        }

        private void OnDestroy()
        {
            RestorePreview();
            if (ceiling != null && ceiling.cookie == coneCookie) ceiling.cookie = null;
            if (coneCookie != null) { if (Application.isPlaying) Destroy(coneCookie); else DestroyImmediate(coneCookie); }
        }
    }
}
