using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation of the existing bed-only stages. It never advances gameplay.</summary>
    [ExecuteAlways]
    public sealed class BedtimeGuidance : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Light spotlight;
        [SerializeField] private Renderer[] bedRenderers;
        [SerializeField] private Vector3 bedSurfaceCenter;
        [SerializeField] private float beamRadius = 1.12f;
        [SerializeField] private float fadeSeconds = 1.1f;
        [SerializeField] private float spotIntensity = 8f;
        [SerializeField] private Renderer selfReviewSurface;
        private GameRuntime runtime;
        private BedtimeAtmosphere atmosphere;
        private bool awaitingDayZeroWake;
        private readonly List<LightState> lights = new List<LightState>();
        private readonly List<SurfaceState> surfaces = new List<SurfaceState>();
        private bool captured, restored = true;
        private bool environmentCapturedForScene;
        private float isolationTarget, beamTarget;
        private AmbientMode ambientMode;
        private Color ambient, sky, equator, ground, background, fogColor;
        private float ambientIntensity, reflectionIntensity;
        private bool fog;
        private Material skybox;
        private SphericalHarmonicsL2 ambientProbe;
        private CameraClearFlags clearFlags;
        private DepthTextureMode depthMode;
        private Light selfReviewGlow;
        private bool selfReviewTarget;
        private Color selfReviewEmission = new Color(.38f, .65f, .46f);
        private RoomLightingController lightingController;
        private RoomNightLightingSettings nightSettings = new RoomNightLightingSettings();
        private RoomLightingPreviewMode previewMode;
        private readonly Dictionary<Light, SceneLightSettings> nightLights = new Dictionary<Light, SceneLightSettings>();
        private SceneLightSettings spotlightDay;
        private bool specialIsolation;
        private bool dreamBedReveal, bedOnlyTarget;
        private RuntimeView lastView;
        private RoomLightingProfile fullDayProfile, fullNightProfile;
        private readonly Dictionary<Light, SceneLightSettings> fullDayLights = new Dictionary<Light, SceneLightSettings>();
        private readonly Dictionary<Light, SceneLightSettings> fullNightLights = new Dictionary<Light, SceneLightSettings>();
        private bool UsesFullProfiles => fullDayProfile != null && fullNightProfile != null;

        public float Isolation { get; private set; }
        public float BeamStrength { get; private set; }
        public bool OwnsLighting => isActiveAndEnabled && (isolationTarget > 0 || Isolation > 0);
        public Light Spotlight => spotlight;
        public float DefaultSpotIntensity => spotIntensity;
        public BedtimeAtmosphere Atmosphere => atmosphere;
        public Renderer[] BedRenderers => bedRenderers;
        public Vector3 SleepFocusPoint => bedSurfaceCenter + Vector3.up * .25f;
        public static bool IsSleepOnly(RuntimeView view) => view != null && (view.Game.Phase == GamePhase.AfterDream
            || view.Game.Phase == GamePhase.Evening);

        public void ConfigureSelfReview(Renderer terminalScreen)
        {
            selfReviewSurface = terminalScreen;
            if (terminalScreen != null && terminalScreen.sharedMaterial != null && terminalScreen.sharedMaterial.HasProperty("_EmissionColor"))
            {
                var color = terminalScreen.sharedMaterial.GetColor("_EmissionColor");
                if (color.maxColorComponent > .01f) selfReviewEmission = color;
            }
        }

        public void Configure(Camera camera, Light light, Renderer[] targets, Vector3 surfaceCenter, float radius)
        {
            if (captured && (viewCamera != camera || spotlight != light))
            {
                RestoreRoom(); lights.Clear(); surfaces.Clear(); nightLights.Clear(); captured = false;
                previewMode = RoomLightingPreviewMode.Automatic; Isolation = BeamStrength = isolationTarget = beamTarget = 0;
            }
            viewCamera = camera; spotlight = light; bedRenderers = targets;
            bedSurfaceCenter = surfaceCenter; beamRadius = radius;
        }

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= SetTargets;
            lightingController?.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            CaptureRoom();
            EnsureSelfReviewGlow();
            if (atmosphere == null)
            {
                atmosphere = gameObject.AddComponent<BedtimeAtmosphere>();
                atmosphere.Initialize(spotlight.transform, bedSurfaceCenter, beamRadius);
            }
            runtime = game;
            awaitingDayZeroWake = false;
            dreamBedReveal = false;
            runtime.Changed += SetTargets;
            SetTargets(game.GetView());
            // Checkpoint restoration derives presentation from the saved phase, without a flash of daylight.
            Isolation = isolationTarget;
            BeamStrength = beamTarget;
            ApplyRoom();
            atmosphere.SetVisualState(VisibleBeamStrength(), 0, game.GetView().Paused);
        }

        private void CaptureRoom()
        {
            if (captured || viewCamera == null || spotlight == null) return;
            captured = true;
            environmentCapturedForScene = gameObject.scene == SceneManager.GetActiveScene();
            ambientMode = RenderSettings.ambientMode; ambient = RenderSettings.ambientLight;
            sky = RenderSettings.ambientSkyColor; equator = RenderSettings.ambientEquatorColor; ground = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity; reflectionIntensity = RenderSettings.reflectionIntensity;
            ambientProbe = RenderSettings.ambientProbe;
            skybox = RenderSettings.skybox; fog = RenderSettings.fog; fogColor = RenderSettings.fogColor;
            background = viewCamera.backgroundColor; clearFlags = viewCamera.clearFlags; depthMode = viewCamera.depthTextureMode;
            foreach (var light in FindObjectsOfType<Light>(true).Where(item => item != spotlight && item.gameObject.scene == gameObject.scene))
                lights.Add(new LightState { Light = light, Settings = ReadLight(light) });
            spotlightDay = ReadLight(spotlight);
            foreach (var renderer in FindObjectsOfType<Renderer>(true).Where(item => item.gameObject.scene == gameObject.scene))
            {
                var materials = renderer.sharedMaterials;
                var global = new MaterialPropertyBlock(); renderer.GetPropertyBlock(global);
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    var material = materials[slot];
                    if (material == null) continue;
                    bool glass = material.shader != null && material.shader.name == "MiniGame/Room Window Glass";
                    if (!glass && !material.HasProperty("_EmissionColor")) continue;
                    int property = Shader.PropertyToID(glass ? "_Color" : "_EmissionColor");
                    var original = new MaterialPropertyBlock(); renderer.GetPropertyBlock(original, slot);
                    Color value = original.HasProperty(property) ? original.GetColor(property)
                        : global.HasProperty(property) ? global.GetColor(property) : material.GetColor(property);
                    if (!glass && value.maxColorComponent <= 0) continue;
                    surfaces.Add(new SurfaceState { Renderer = renderer, Slot = slot, Property = property, Color = value,
                        Working = new MaterialPropertyBlock() });
                }
            }
        }

        internal void CaptureActiveSceneEnvironment()
        {
            if (!captured || environmentCapturedForScene || gameObject.scene != SceneManager.GetActiveScene()) return;
            // An additively loaded room must retain its own global fog/skybox when it first becomes active.
            ambientMode = RenderSettings.ambientMode; ambient = RenderSettings.ambientLight;
            sky = RenderSettings.ambientSkyColor; equator = RenderSettings.ambientEquatorColor; ground = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity; reflectionIntensity = RenderSettings.reflectionIntensity;
            ambientProbe = RenderSettings.ambientProbe; skybox = RenderSettings.skybox;
            fog = RenderSettings.fog; fogColor = RenderSettings.fogColor;
            environmentCapturedForScene = true;
        }

        private void SetTargets(RuntimeView view)
        {
            if (awaitingDayZeroWake && view.Game.Day > 0)
                foreach (var root in gameObject.scene.GetRootGameObjects())
                    foreach (var player in root.GetComponentsInChildren<ApartmentPlayer>(true)) player.RestoreDayZeroWakePose();
            awaitingDayZeroWake = view.Game.Day == 0 && IsSleepOnly(view);
            lastView = view;
            if (view.Game.Phase != GamePhase.Dream && view.Game.Phase != GamePhase.AfterDream)
                dreamBedReveal = false;
            bool inDream = view.Game.Phase == GamePhase.Sleeping || view.Game.Phase == GamePhase.Dream;
            bool guide = IsSleepOnly(view) && (!view.TerminalOpen || view.LeavingTerminal);
            bedOnlyTarget = view.Game.Day == 0 && (guide || inDream)
                || view.Game.Day > 0 && (dreamBedReveal || view.Game.Phase == GamePhase.AfterDream);
            bool beforeDream = view.UsesDreamFlow && view.Game.Phase == GamePhase.Evening && (!view.TerminalOpen || view.LeavingTerminal);
            selfReviewTarget = view.UsesDreamFlow && view.Game.Phase == GamePhase.SelfReview;
            isolationTarget = guide || inDream || beforeDream || selfReviewTarget || bedOnlyTarget ? 1 : 0;
            beamTarget = guide || bedOnlyTarget ? 1 : 0;
            specialIsolation = inDream || selfReviewTarget || bedOnlyTarget;
            ApplyPreviewTargets();
        }

        /// <summary>Reveal the bed while a nightly dream returns its camera; the runtime still owns waking.</summary>
        public void SetDreamBedReveal(bool reveal)
        {
            dreamBedReveal = reveal;
            if (runtime == null) return;
            var view = runtime.GetView();
            SetTargets(view);
            // Returning checkpoints can be restored while paused, before the transition clock can advance.
            if (previewMode == RoomLightingPreviewMode.Automatic && view.Game.Day > 0
                && (view.Game.Phase == GamePhase.Dream || view.Game.Phase == GamePhase.AfterDream))
            { Isolation = isolationTarget; BeamStrength = beamTarget; }
            ApplyRoom();
            if (atmosphere != null) atmosphere.SetVisualState(VisibleBeamStrength(), 0, view.Paused);
        }

        private void ApplyPreviewTargets()
        {
            if (previewMode == RoomLightingPreviewMode.Automatic) return;
            isolationTarget = previewMode == RoomLightingPreviewMode.Night ? 1 : 0;
            beamTarget = isolationTarget;
            specialIsolation = selfReviewTarget = bedOnlyTarget = false;
        }

        public void ConfigureLighting(RoomLightingController controller, RoomNightLightingSettings settings,
            RoomLightingPreviewMode preview, IReadOnlyDictionary<Light, SceneLightSettings> daylight)
        {
            if (!isActiveAndEnabled) return;
            CaptureRoom();
            fullDayProfile = fullNightProfile = null; fullDayLights.Clear(); fullNightLights.Clear();
            lightingController = controller; nightSettings = settings ?? new RoomNightLightingSettings();
            nightSettings.ValidateValues();
            foreach (var item in daylight) UpdateDaylightSettings(item.Key, item.Value);
            nightLights.Clear();
            if (controller != null)
                foreach (var entry in nightSettings.SceneLights)
                {
                    if (entry == null) continue;
                    var source = controller.ResolveSceneLight(entry.HierarchyPath, RoomLightingPeriod.Night);
                    if (source == null || nightLights.ContainsKey(source)) continue;
                    var copy = entry.Copy(); copy.ValidateValues(); nightLights.Add(source, copy);
                }
            bool changed = previewMode != preview;
            previewMode = preview;
            if (lastView != null) SetTargets(lastView);
            else { isolationTarget = beamTarget = 0; specialIsolation = selfReviewTarget = false; ApplyPreviewTargets(); }
            // Authoring previews are immediate, including while gameplay is paused.
            if (changed || !Application.isPlaying) { Isolation = isolationTarget; BeamStrength = beamTarget; }
            ApplyRoom();
        }

        public void ConfigureFullLighting(RoomLightingController controller, RoomLightingProfile day, RoomLightingProfile night,
            RoomLightingPreviewMode preview, IReadOnlyDictionary<Light, SceneLightSettings> daylight,
            IReadOnlyDictionary<Light, SceneLightSettings> nightlight)
        {
            if (!isActiveAndEnabled) return;
            CaptureRoom();
            lightingController = controller; fullDayProfile = day; fullNightProfile = night;
            fullDayLights.Clear(); fullNightLights.Clear();
            foreach (var item in daylight)
            {
                fullDayLights.Add(item.Key, item.Value.Copy());
                if (item.Key == spotlight) spotlightDay = item.Value.Copy();
                else UpdateDaylightSettings(item.Key, item.Value);
            }
            foreach (var item in nightlight) fullNightLights.Add(item.Key, item.Value.Copy());
            bool changed = previewMode != preview;
            previewMode = preview;
            if (lastView != null) SetTargets(lastView);
            else { isolationTarget = beamTarget = 0; specialIsolation = selfReviewTarget = false; ApplyPreviewTargets(); }
            if (changed || !Application.isPlaying) { Isolation = isolationTarget; BeamStrength = beamTarget; }
            ApplyRoom();
        }

        public void ClearLightingPreview(RoomLightingController controller)
        {
            if (lightingController != controller) return;
            fullDayProfile = fullNightProfile = null; fullDayLights.Clear(); fullNightLights.Clear();
            nightSettings = new RoomNightLightingSettings(); nightLights.Clear();
            previewMode = RoomLightingPreviewMode.Automatic;
            if (lastView != null) SetTargets(lastView);
            else { isolationTarget = beamTarget = 0; specialIsolation = selfReviewTarget = false; }
            Isolation = isolationTarget; BeamStrength = beamTarget;
            ApplyRoom();
            lightingController = null;
        }

        // The room profile can change during Play Mode, including while daylight is faded out.
        // Update the retained day state without recapturing the currently darkened scene.
        public void UpdateDaylightLight(Light source, bool dayEnabled, float intensity, Color color)
        {
            if (!captured || source == null || source == spotlight) return;
            var state = lights.Find(item => item.Light == source);
            if (state == null) { state = new LightState { Light = source, Settings = ReadLight(source) }; lights.Add(state); }
            state.Settings.dayEnabled = dayEnabled; state.Settings.intensity = intensity; state.Settings.color = color;
        }

        public void UpdateDaylightSettings(Light source, SceneLightSettings settings)
        {
            if (!captured || source == null || source == spotlight || settings == null) return;
            var state = lights.Find(item => item.Light == source);
            if (state == null) { state = new LightState { Light = source }; lights.Add(state); }
            state.Settings = settings.Copy();
        }

        public bool TryGetDaylightSettings(Light source, out SceneLightSettings settings)
        {
            var state = lights.Find(item => item.Light == source);
            settings = state?.Settings?.Copy();
            return captured && settings != null;
        }

        public bool TryGetDaylightLight(Light source, out bool dayEnabled, out float intensity, out Color color)
        {
            dayEnabled = false; intensity = 0; color = Color.white;
            if (!captured || source == null || source == spotlight || source.gameObject.scene != gameObject.scene) return false;
            var state = lights.Find(item => item.Light == source);
            if (state == null) return false;
            dayEnabled = state.Settings.dayEnabled; intensity = state.Settings.intensity; color = state.Settings.color;
            return true;
        }

        public void UpdateDaylightAmbient(Color color, SphericalHarmonicsL2 probe, float reflections)
        {
            if (!captured) return;
            ambientMode = AmbientMode.Flat; ambient = sky = equator = ground = color;
            ambientIntensity = 1; ambientProbe = probe; reflectionIntensity = reflections;
        }

        public void UpdateDaylightEmission(Renderer renderer, int slot, Color color)
        {
            if (!captured || renderer == null) return;
            int property = Shader.PropertyToID("_EmissionColor");
            var state = surfaces.Find(item => item.Renderer == renderer && item.Slot == slot && item.Property == property);
            if (state == null)
            {
                state = new SurfaceState { Renderer = renderer, Slot = slot, Property = property,
                    Working = new MaterialPropertyBlock() };
                surfaces.Add(state);
            }
            state.Color = color;
        }

        public void ReapplyLighting() => ApplyRoom();

        private void LateUpdate()
        {
            if (runtime == null) return;
            bool paused = runtime.GetView().Paused;
            float delta = paused ? 0 : Time.unscaledDeltaTime;
            float duration = UsesFullProfiles ? (isolationTarget > Isolation ? fullNightProfile.fadeSeconds : fullDayProfile.fadeSeconds)
                : lightingController != null ? nightSettings.fadeSeconds : fadeSeconds;
            Isolation = Mathf.MoveTowards(Isolation, isolationTarget, delta / Mathf.Max(.01f, duration));
            BeamStrength = Mathf.MoveTowards(BeamStrength, beamTarget, delta / (beamTarget > BeamStrength ? .85f : .45f));
            ApplyRoom();
            if (atmosphere != null) atmosphere.SetVisualState(VisibleBeamStrength(), delta, paused);
        }

        private void ApplyRoom()
        {
            if (!captured) return;
            if (UsesFullProfiles) { ApplyFullRoom(); return; }
            if (Isolation <= 0 && isolationTarget <= 0)
            {
                if (!restored) RestoreRoom();
                spotlight.enabled = false;
                if (selfReviewGlow != null) selfReviewGlow.enabled = false;
                return;
            }
            restored = false;
            float amount = Mathf.SmoothStep(0, 1, Isolation);
            foreach (var state in lights)
            {
                if (state.Light == null) continue;
                var day = state.Settings;
                SceneLightSettings night = null;
                bool overridden = !specialIsolation && nightLights.TryGetValue(state.Light, out night);
                var target = overridden ? night : day;
                // Geometry belongs to each period; day edits made during night never become the night baseline.
                if (lightingController != null) lightingController.ApplyPresentedLight(state.Light, amount > 0 && overridden ? target : day);
                else RoomLightingController.ApplyLight(state.Light, amount > 0 && overridden ? target : day);
                float targetIntensity = specialIsolation ? 0 : overridden ? target.intensity : day.intensity * nightSettings.lightMultiplier;
                bool targetEnabled = !specialIsolation && (overridden ? target.dayEnabled : day.dayEnabled && targetIntensity > 0);
                state.Light.enabled = amount < .9999f && day.dayEnabled || amount > .0001f && targetEnabled;
                state.Light.intensity = Mathf.Lerp(day.dayEnabled ? day.intensity : 0, targetEnabled ? targetIntensity : 0, amount);
                state.Light.color = Color.Lerp(day.color, target.color, amount);
            }
            var nightAmbient = specialIsolation ? Color.black : nightSettings.ambientColor;
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.Lerp(ambient, nightAmbient, amount);
                RenderSettings.ambientSkyColor = Color.Lerp(sky, nightAmbient, amount);
                RenderSettings.ambientEquatorColor = Color.Lerp(equator, nightAmbient, amount);
                RenderSettings.ambientGroundColor = Color.Lerp(ground, nightAmbient, amount);
                RenderSettings.ambientIntensity = Mathf.Lerp(ambientIntensity, nightAmbient.maxColorComponent > 0 ? 1 : 0, amount);
                RenderSettings.reflectionIntensity = Mathf.Lerp(reflectionIntensity, specialIsolation ? 0 : nightSettings.reflectionIntensity, amount);
                var probe = ambientProbe; var nightProbe = new SphericalHarmonicsL2(); nightProbe.AddAmbientLight(nightAmbient);
                for (int channel = 0; channel < 3; channel++)
                    for (int coefficient = 0; coefficient < 9; coefficient++)
                        probe[channel, coefficient] = Mathf.Lerp(probe[channel, coefficient], nightProbe[channel, coefficient], amount);
                RenderSettings.ambientProbe = probe;
                RenderSettings.fogColor = Color.Lerp(fogColor, Color.black, amount);
                if (amount >= .9999f) { RenderSettings.skybox = null; RenderSettings.fog = false; }
            }
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = Color.Lerp(background, Color.black, amount);
            viewCamera.depthTextureMode = depthMode | DepthTextureMode.Depth;
            foreach (var state in surfaces)
            {
                if (state.Renderer == null) continue;
                state.Renderer.GetPropertyBlock(state.Working, state.Slot);
                float emission = Mathf.Lerp(1, specialIsolation ? 0 : nightSettings.emissionMultiplier, amount);
                var color = new Color(state.Color.r * emission, state.Color.g * emission, state.Color.b * emission, state.Color.a);
                if (selfReviewTarget && state.Renderer == selfReviewSurface && state.Property == Shader.PropertyToID("_EmissionColor"))
                    color = Color.Lerp(state.Color, selfReviewEmission, amount);
                state.Working.SetColor(state.Property, color);
                state.Renderer.SetPropertyBlock(state.Working, state.Slot);
            }
            SceneLightSettings nightSpot = null;
            bool spotOverride = !specialIsolation && nightLights.TryGetValue(spotlight, out nightSpot);
            if (spotOverride) RoomLightingController.ApplyLight(spotlight, nightSpot);
            else if (spotlightDay != null) RoomLightingController.ApplyLight(spotlight, spotlightDay);
            spotlight.enabled = BeamStrength > .001f && (!spotOverride || nightSpot.dayEnabled);
            spotlight.intensity = (spotOverride ? nightSpot.intensity : spotIntensity) * Mathf.SmoothStep(0, 1, BeamStrength);
            if (selfReviewGlow != null)
            {
                selfReviewGlow.enabled = selfReviewTarget && amount > .001f && selfReviewSurface != null && selfReviewSurface.enabled;
                selfReviewGlow.intensity = .55f * amount;
                if (selfReviewSurface != null)
                {
                    var center = selfReviewSurface.bounds.center;
                    var towardRoom = viewCamera != null ? (viewCamera.transform.position - center).normalized : Vector3.forward;
                    selfReviewGlow.transform.position = center + towardRoom * .08f;
                }
            }
        }

        private float VisibleBeamStrength()
        {
            if (bedOnlyTarget)
                return spotlight != null && spotlight.enabled && spotlight.intensity > .001f ? BeamStrength : 0;
            if (UsesFullProfiles)
                return spotlight != null && spotlight.enabled && spotlight.intensity > .001f && !specialIsolation ? 1 : 0;
            if (spotlight != null && (!spotlight.enabled || spotlight.intensity <= .001f)) return 0;
            return BeamStrength;
        }

        private void ApplyFullRoom()
        {
            float amount = Mathf.SmoothStep(0, 1, Isolation);
            restored = false;
            foreach (var item in fullDayLights)
            {
                if (item.Key == null) continue;
                var day = item.Value;
                var night = fullNightLights.TryGetValue(item.Key, out var configured) ? configured : day;
                var target = amount < .5f ? day : night;
                var blended = target.Copy();
                blended.localPosition = Vector3.Lerp(day.localPosition, night.localPosition, amount);
                blended.localEuler = Quaternion.Slerp(Quaternion.Euler(day.localEuler), Quaternion.Euler(night.localEuler), amount).eulerAngles;
                blended.range = Mathf.Lerp(day.range, night.range, amount);
                blended.spotAngle = Mathf.Lerp(day.spotAngle, night.spotAngle, amount);
                blended.innerSpotAngle = Mathf.Lerp(day.innerSpotAngle, night.innerSpotAngle, amount);
                blended.shadowStrength = Mathf.Lerp(day.shadowStrength, night.shadowStrength, amount);
                blended.shadowBias = Mathf.Lerp(day.shadowBias, night.shadowBias, amount);
                blended.shadowNormalBias = Mathf.Lerp(day.shadowNormalBias, night.shadowNormalBias, amount);
                blended.color = Color.Lerp(day.color, night.color, amount);
                float nightIntensity = specialIsolation ? 0 : night.dayEnabled ? night.intensity : 0;
                blended.intensity = Mathf.Lerp(day.dayEnabled ? day.intensity : 0, nightIntensity, amount);
                blended.dayEnabled = blended.intensity > 0 && (amount < .9999f && day.dayEnabled || amount > .0001f && !specialIsolation && night.dayEnabled);
                lightingController.ApplyPresentedLight(item.Key, blended);
            }
            if (bedOnlyTarget)
            {
                // A disabled or dim ordinary-night bed light cannot suppress the dream's wake-up cue.
                if (spotlightDay != null) RoomLightingController.ApplyLight(spotlight, spotlightDay);
                spotlight.enabled = BeamStrength > .001f;
                spotlight.intensity = spotIntensity * Mathf.SmoothStep(0, 1, BeamStrength);
            }
            var dayAmbient = fullDayProfile.AmbientLightColor;
            var nightAmbient = specialIsolation ? Color.black : fullNightProfile.AmbientLightColor;
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                var color = Color.Lerp(dayAmbient, nightAmbient, amount);
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = RenderSettings.ambientSkyColor = RenderSettings.ambientEquatorColor = RenderSettings.ambientGroundColor = color;
                RenderSettings.ambientIntensity = specialIsolation ? 1 - amount : 1;
                RenderSettings.reflectionIntensity = Mathf.Lerp(fullDayProfile.reflectionIntensity, specialIsolation ? 0 : fullNightProfile.reflectionIntensity, amount);
                var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(color); RenderSettings.ambientProbe = probe;
                RenderSettings.skybox = specialIsolation && amount >= .9999f ? null : skybox;
                RenderSettings.fog = !specialIsolation && fog;
                RenderSettings.fogColor = specialIsolation ? Color.Lerp(fogColor, Color.black, amount) : fogColor;
            }
            if (viewCamera != null)
            {
                viewCamera.clearFlags = specialIsolation ? CameraClearFlags.SolidColor : clearFlags;
                viewCamera.backgroundColor = specialIsolation ? Color.Lerp(background, Color.black, amount) : background;
                viewCamera.depthTextureMode = depthMode | (VisibleBeamStrength() > 0 ? DepthTextureMode.Depth : DepthTextureMode.None);
            }
            foreach (var state in surfaces)
            {
                if (state.Renderer == null) continue;
                state.Renderer.GetPropertyBlock(state.Working, state.Slot);
                float emission = Mathf.Lerp(fullDayProfile.emissionMultiplier,
                    specialIsolation ? 0 : fullNightProfile.emissionMultiplier, amount);
                var color = new Color(state.Color.r * emission, state.Color.g * emission, state.Color.b * emission, state.Color.a);
                if (selfReviewTarget && state.Renderer == selfReviewSurface && state.Property == Shader.PropertyToID("_EmissionColor"))
                    color = Color.Lerp(state.Color * fullDayProfile.emissionMultiplier, selfReviewEmission, amount);
                state.Working.SetColor(state.Property, color); state.Renderer.SetPropertyBlock(state.Working, state.Slot);
            }
            lightingController.ApplyFullSurfaceBlend(amount, specialIsolation ? 1 - amount : 1);
            if (selfReviewGlow != null)
            {
                selfReviewGlow.enabled = selfReviewTarget && amount > .001f && selfReviewSurface != null && selfReviewSurface.enabled;
                selfReviewGlow.intensity = .55f * amount;
                if (selfReviewSurface != null)
                {
                    var center = selfReviewSurface.bounds.center;
                    var towardRoom = viewCamera != null ? (viewCamera.transform.position - center).normalized : Vector3.forward;
                    selfReviewGlow.transform.position = center + towardRoom * .08f;
                }
            }
            // Explicitly disabled bed lighting also suppresses its beam and particles in edit-mode previews.
            if (atmosphere != null) atmosphere.SetVisualState(VisibleBeamStrength(), 0, true);
        }

        private void EnsureSelfReviewGlow()
        {
            if (selfReviewSurface == null || selfReviewGlow != null) return;
            var root = new GameObject("Self-review terminal glow") { hideFlags = HideFlags.DontSave };
            root.transform.SetParent(transform, false);
            selfReviewGlow = root.AddComponent<Light>();
            selfReviewGlow.type = LightType.Point;
            selfReviewGlow.range = 1.2f;
            selfReviewGlow.color = new Color(.42f, .72f, .54f);
            selfReviewGlow.shadows = LightShadows.None;
            selfReviewGlow.enabled = false;
        }

        private void RestoreRoom()
        {
            if (!captured) return;
            foreach (var state in lights)
                if (state.Light != null)
                {
                    if (lightingController != null) lightingController.ApplyPresentedLight(state.Light, state.Settings);
                    else RoomLightingController.ApplyLight(state.Light, state.Settings);
                }
            foreach (var state in surfaces)
                if (state.Renderer != null)
                {
                    // Own only this colour; retain live band/floor settings written by other presenters.
                    state.Renderer.GetPropertyBlock(state.Working, state.Slot);
                    state.Working.SetColor(state.Property, state.Color);
                    state.Renderer.SetPropertyBlock(state.Working, state.Slot);
                }
            // Unloading an old room must not replace a newly activated scene's global environment.
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                RenderSettings.ambientMode = ambientMode; RenderSettings.ambientLight = ambient;
                RenderSettings.ambientSkyColor = sky; RenderSettings.ambientEquatorColor = equator; RenderSettings.ambientGroundColor = ground;
                RenderSettings.ambientIntensity = ambientIntensity; RenderSettings.reflectionIntensity = reflectionIntensity;
                RenderSettings.ambientProbe = ambientProbe; RenderSettings.skybox = skybox;
                RenderSettings.fog = fog; RenderSettings.fogColor = fogColor;
            }
            if (viewCamera != null) { viewCamera.backgroundColor = background; viewCamera.clearFlags = clearFlags; viewCamera.depthTextureMode = depthMode; }
            if (spotlight != null) { if (spotlightDay != null) RoomLightingController.ApplyLight(spotlight, spotlightDay); spotlight.enabled = false; }
            if (selfReviewGlow != null) selfReviewGlow.enabled = false;
            restored = true;
        }

        private void OnDestroy()
        {
            if (runtime != null) runtime.Changed -= SetTargets;
            if (!restored) RestoreRoom();
            if (selfReviewGlow != null) Destroy(selfReviewGlow.gameObject);
        }

        private void OnDisable()
        {
            previewMode = RoomLightingPreviewMode.Automatic;
            lightingController?.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            if (!restored) RestoreRoom();
            Isolation = BeamStrength = 0;
            if (atmosphere != null) atmosphere.SetVisualState(0, 0, true);
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            SetTargets(runtime.GetView());
            Isolation = isolationTarget; BeamStrength = beamTarget;
            ApplyRoom();
            if (atmosphere != null) atmosphere.SetVisualState(VisibleBeamStrength(), 0, runtime.GetView().Paused);
        }

        private static SceneLightSettings ReadLight(Light source) => new SceneLightSettings
        {
            dayEnabled = source.enabled, intensity = source.intensity, color = source.color,
            localPosition = source.transform.localPosition, localEuler = source.transform.localEulerAngles,
            range = source.range, type = source.type, spotAngle = source.spotAngle, innerSpotAngle = source.innerSpotAngle,
            shadows = source.shadows, shadowStrength = source.shadowStrength, shadowBias = source.shadowBias, shadowNormalBias = source.shadowNormalBias
        };
        private sealed class LightState { public Light Light; public SceneLightSettings Settings; }
        private sealed class SurfaceState
        {
            public Renderer Renderer; public int Slot, Property; public Color Color;
            public MaterialPropertyBlock Working;
        }
    }
}
