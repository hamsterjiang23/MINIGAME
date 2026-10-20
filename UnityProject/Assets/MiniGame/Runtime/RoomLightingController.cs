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
        [SerializeField] private Light ceiling, window, fill, exterior, monitor;
        [SerializeField] private Renderer floor, lamp, rug;
        [SerializeField] private int bulbSlot;
        [SerializeField] private Renderer[] roomSurfaces;
        [SerializeField] private BedtimeGuidance bedtime;
        private MaterialPropertyBlock properties;
        private Texture2D coneCookie;
        private float cookieInner = -1, cookieOuter = -1;
        private RoomLightingProfile appliedProfile;
        private int appliedRevision = -1;
        private bool lastPreview;
        public RoomLightingProfile Profile => profile;
        public Light MainLight => ceiling;
        public Renderer Rug => rug;
        public bool PreviewUnbanded { get; set; }

        public void Configure(RoomLightingProfile settings, Light main, Light windowLight, Light fillLight,
            Light exteriorLight, Light monitorLight, Renderer floorSurface, Renderer lampSurface, int emissionSlot,
            Renderer[] surfaces, Renderer rugSurface, BedtimeGuidance guidance)
        {
            profile = settings; ceiling = main; window = windowLight; fill = fillLight; exterior = exteriorLight; monitor = monitorLight;
            floor = floorSurface; lamp = lampSurface; bulbSlot = emissionSlot; roomSurfaces = surfaces; rug = rugSurface; bedtime = guidance;
            ApplyNow();
        }

        private void OnEnable() => ApplyNow();
        private void OnValidate() => appliedRevision = -1;
        private void Update()
        {
            if (profile != appliedProfile || profile != null && profile.Revision != appliedRevision || PreviewUnbanded != lastPreview)
                ApplyNow();
        }

        public void ApplyNow()
        {
            if (profile == null || ceiling == null || lamp == null || floor == null) return;
            properties ??= new MaterialPropertyBlock();
            appliedProfile = profile; appliedRevision = profile.Revision; lastPreview = PreviewUnbanded;
            var p = profile;
            ceiling.transform.position = new Vector3(lamp.bounds.center.x, lamp.bounds.min.y, lamp.bounds.center.z) + p.lampOffset;
            ceiling.transform.rotation = Quaternion.Euler(90, 0, 0);
            ceiling.type = LightType.Spot;
            ceiling.range = p.lampRange; ceiling.spotAngle = p.outerAngle; ceiling.innerSpotAngle = p.innerAngle;
            ceiling.renderMode = LightRenderMode.ForcePixel; ceiling.shadows = LightShadows.Soft;
            ceiling.shadowCustomResolution = 2048; ceiling.shadowStrength = p.shadowStrength;
            ceiling.shadowBias = p.shadowBias; ceiling.shadowNormalBias = p.shadowNormalBias;
            UpdateCookie(); ceiling.cookie = coneCookie;
            SetDaylight(ceiling, true, p.lampIntensity, p.lampColor);
            if (window != null)
            {
                window.range = p.windowRange; window.renderMode = LightRenderMode.ForcePixel;
                window.shadows = LightShadows.Soft;
                SetDaylight(window, p.windowIntensity > 0, p.windowIntensity, p.windowColor);
            }
            if (fill != null) SetDaylight(fill, false, 0, fill.color);
            if (exterior != null) SetDaylight(exterior, false, 0, exterior.color);
            if (monitor != null) SetDaylight(monitor, p.monitorIntensity > 0, p.monitorIntensity, monitor.color);
            var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(p.ambientColor);
            if (gameObject.scene == SceneManager.GetActiveScene() && (bedtime == null || !bedtime.OwnsLighting))
            {
                RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = p.ambientColor;
                RenderSettings.ambientSkyColor = RenderSettings.ambientEquatorColor = RenderSettings.ambientGroundColor = p.ambientColor;
                RenderSettings.ambientIntensity = 1; RenderSettings.ambientProbe = probe;
                RenderSettings.reflectionIntensity = p.reflectionIntensity;
            }
            bedtime?.UpdateDaylightAmbient(p.ambientColor, probe, p.reflectionIntensity);
            if (roomSurfaces != null) foreach (var surface in roomSurfaces) ApplySurface(surface);
            if (rug != null)
            {
                rug.gameObject.SetActive(p.rugVisible);
                rug.transform.position = new Vector3(floor.bounds.center.x, floor.bounds.max.y, floor.bounds.center.z) + p.rugOffset;
                rug.transform.rotation = Quaternion.Euler(0, p.rugYaw, 0);
                rug.transform.localScale = new Vector3(p.rugSize.x, 1, p.rugSize.y);
                ApplySurface(rug);
            }
            if (bulbSlot >= 0 && bulbSlot < lamp.sharedMaterials.Length)
            {
                var color = p.lampColor * p.bulbEmission;
                lamp.GetPropertyBlock(properties, bulbSlot); properties.SetColor("_EmissionColor", color);
                lamp.SetPropertyBlock(properties, bulbSlot);
                bedtime?.UpdateDaylightEmission(lamp, bulbSlot, color);
            }
            bedtime?.ReapplyLighting();
        }

        private void SetDaylight(Light light, bool dayEnabled, float intensity, Color color)
        {
            if (bedtime == null || !bedtime.OwnsLighting) { light.enabled = dayEnabled; light.intensity = intensity; }
            light.color = color;
            bedtime?.UpdateDaylightLight(light, dayEnabled, intensity, color);
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

        private void UpdateCookie()
        {
            if (coneCookie != null && cookieInner == profile.innerAngle && cookieOuter == profile.outerAngle) return;
            if (coneCookie == null) coneCookie = new Texture2D(128, 128, TextureFormat.RGBA32, false, true)
                { name = "Room spotlight inner cone", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            cookieInner = profile.innerAngle; cookieOuter = profile.outerAngle;
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
            if (ceiling != null && ceiling.cookie == coneCookie) ceiling.cookie = null;
            if (coneCookie != null) { if (Application.isPlaying) Destroy(coneCookie); else DestroyImmediate(coneCookie); }
        }
    }
}
