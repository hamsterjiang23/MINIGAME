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
    public sealed class BedtimeGuidance : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Light spotlight;
        [SerializeField] private Renderer[] bedRenderers;
        [SerializeField] private Vector3 bedSurfaceCenter;
        [SerializeField] private float beamRadius = 1.12f;
        [SerializeField] private float fadeSeconds = 1.1f;
        [SerializeField] private float spotIntensity = 8f;
        private GameRuntime runtime;
        private BedtimeAtmosphere atmosphere;
        private readonly List<LightState> lights = new List<LightState>();
        private readonly List<SurfaceState> surfaces = new List<SurfaceState>();
        private bool captured, restored = true;
        private float isolationTarget, beamTarget;
        private AmbientMode ambientMode;
        private Color ambient, sky, equator, ground, background, fogColor;
        private float ambientIntensity, reflectionIntensity;
        private bool fog;
        private Material skybox;
        private SphericalHarmonicsL2 ambientProbe;
        private CameraClearFlags clearFlags;
        private DepthTextureMode depthMode;

        public float Isolation { get; private set; }
        public float BeamStrength { get; private set; }
        public bool OwnsLighting => isActiveAndEnabled && (isolationTarget > 0 || Isolation > 0);
        public Light Spotlight => spotlight;
        public BedtimeAtmosphere Atmosphere => atmosphere;
        public Renderer[] BedRenderers => bedRenderers;
        public static bool IsSleepOnly(RuntimeView view) => view != null &&
            (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream);

        public void Configure(Camera camera, Light light, Renderer[] targets, Vector3 surfaceCenter, float radius)
        {
            viewCamera = camera; spotlight = light; bedRenderers = targets;
            bedSurfaceCenter = surfaceCenter; beamRadius = radius;
        }

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= SetTargets;
            CaptureRoom();
            if (atmosphere == null)
            {
                atmosphere = gameObject.AddComponent<BedtimeAtmosphere>();
                atmosphere.Initialize(spotlight.transform, bedSurfaceCenter, beamRadius);
            }
            runtime = game;
            runtime.Changed += SetTargets;
            SetTargets(game.GetView());
            // Checkpoint restoration derives presentation from the saved phase, without a flash of daylight.
            Isolation = isolationTarget;
            BeamStrength = beamTarget;
            ApplyRoom();
            atmosphere.SetVisualState(BeamStrength, 0, game.GetView().Paused);
        }

        private void CaptureRoom()
        {
            if (captured) return;
            captured = true;
            ambientMode = RenderSettings.ambientMode; ambient = RenderSettings.ambientLight;
            sky = RenderSettings.ambientSkyColor; equator = RenderSettings.ambientEquatorColor; ground = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity; reflectionIntensity = RenderSettings.reflectionIntensity;
            ambientProbe = RenderSettings.ambientProbe;
            skybox = RenderSettings.skybox; fog = RenderSettings.fog; fogColor = RenderSettings.fogColor;
            background = viewCamera.backgroundColor; clearFlags = viewCamera.clearFlags; depthMode = viewCamera.depthTextureMode;
            foreach (var light in FindObjectsOfType<Light>(true).Where(item => item != spotlight && item.gameObject.scene == gameObject.scene))
                lights.Add(new LightState { Light = light, Enabled = light.enabled, Intensity = light.intensity, Color = light.color });
            foreach (var renderer in FindObjectsOfType<Renderer>(true).Where(item => item.gameObject.scene == gameObject.scene))
            {
                var materials = renderer.sharedMaterials;
                var global = new MaterialPropertyBlock(); renderer.GetPropertyBlock(global);
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    var material = materials[slot];
                    if (material == null) continue;
                    bool glass = material.shader != null && material.shader.name == "MiniGame/Room Window Glass";
                    if (!glass && (!material.HasProperty("_EmissionColor") || material.GetColor("_EmissionColor").maxColorComponent <= 0)) continue;
                    int property = Shader.PropertyToID(glass ? "_Color" : "_EmissionColor");
                    var original = new MaterialPropertyBlock(); renderer.GetPropertyBlock(original, slot);
                    Color value = original.HasProperty(property) ? original.GetColor(property)
                        : global.HasProperty(property) ? global.GetColor(property) : material.GetColor(property);
                    surfaces.Add(new SurfaceState { Renderer = renderer, Slot = slot, Property = property, Color = value,
                        Original = original, Working = new MaterialPropertyBlock() });
                }
            }
        }

        private void SetTargets(RuntimeView view)
        {
            bool inDream = view.Game.Phase == GamePhase.Sleeping || view.Game.Phase == GamePhase.Dream;
            bool guide = IsSleepOnly(view) && (!view.TerminalOpen || view.LeavingTerminal);
            isolationTarget = guide || inDream ? 1 : 0;
            beamTarget = guide ? 1 : 0;
        }

        private void LateUpdate()
        {
            if (runtime == null) return;
            bool paused = runtime.GetView().Paused;
            float delta = paused ? 0 : Time.unscaledDeltaTime;
            Isolation = Mathf.MoveTowards(Isolation, isolationTarget, delta / Mathf.Max(.01f, fadeSeconds));
            BeamStrength = Mathf.MoveTowards(BeamStrength, beamTarget, delta / (beamTarget > BeamStrength ? .85f : .45f));
            ApplyRoom();
            atmosphere.SetVisualState(BeamStrength, delta, paused);
        }

        private void ApplyRoom()
        {
            if (!captured) return;
            if (Isolation <= 0 && isolationTarget <= 0)
            {
                if (!restored) RestoreRoom();
                spotlight.enabled = false;
                return;
            }
            restored = false;
            float amount = Mathf.SmoothStep(0, 1, Isolation);
            foreach (var state in lights)
            {
                if (state.Light == null) continue;
                state.Light.enabled = state.Enabled && amount < .9999f;
                state.Light.intensity = state.Intensity * (1 - amount);
            }
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(ambient, Color.black, amount);
            RenderSettings.ambientSkyColor = Color.Lerp(sky, Color.black, amount);
            RenderSettings.ambientEquatorColor = Color.Lerp(equator, Color.black, amount);
            RenderSettings.ambientGroundColor = Color.Lerp(ground, Color.black, amount);
            RenderSettings.ambientIntensity = ambientIntensity * (1 - amount);
            RenderSettings.reflectionIntensity = reflectionIntensity * (1 - amount);
            var probe = ambientProbe;
            for (int channel = 0; channel < 3; channel++)
                for (int coefficient = 0; coefficient < 9; coefficient++) probe[channel, coefficient] *= 1 - amount;
            RenderSettings.ambientProbe = probe;
            RenderSettings.fogColor = Color.Lerp(fogColor, Color.black, amount);
            if (amount >= .9999f) { RenderSettings.skybox = null; RenderSettings.fog = false; }
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = Color.Lerp(background, Color.black, amount);
            viewCamera.depthTextureMode = depthMode | DepthTextureMode.Depth;
            foreach (var state in surfaces)
            {
                if (state.Renderer == null) continue;
                state.Renderer.GetPropertyBlock(state.Working, state.Slot);
                state.Working.SetColor(state.Property, new Color(state.Color.r * (1 - amount), state.Color.g * (1 - amount), state.Color.b * (1 - amount), state.Color.a));
                state.Renderer.SetPropertyBlock(state.Working, state.Slot);
            }
            spotlight.enabled = BeamStrength > .001f;
            spotlight.intensity = spotIntensity * Mathf.SmoothStep(0, 1, BeamStrength);
        }

        private void RestoreRoom()
        {
            if (!captured) return;
            foreach (var state in lights)
                if (state.Light != null) { state.Light.enabled = state.Enabled; state.Light.intensity = state.Intensity; state.Light.color = state.Color; }
            foreach (var state in surfaces)
                if (state.Renderer != null) state.Renderer.SetPropertyBlock(state.Original.isEmpty ? null : state.Original, state.Slot);
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
            if (spotlight != null) spotlight.enabled = false;
            restored = true;
        }

        private void OnDestroy()
        {
            if (runtime != null) runtime.Changed -= SetTargets;
            if (!restored) RestoreRoom();
        }

        private void OnDisable()
        {
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
            atmosphere.SetVisualState(BeamStrength, 0, runtime.GetView().Paused);
        }

        private sealed class LightState { public Light Light; public bool Enabled; public float Intensity; public Color Color; }
        private sealed class SurfaceState
        {
            public Renderer Renderer; public int Slot, Property; public Color Color;
            public MaterialPropertyBlock Original, Working;
        }
    }
}
