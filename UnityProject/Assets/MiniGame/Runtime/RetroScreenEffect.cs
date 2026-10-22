using UnityEngine;

namespace MiniGame.UnityHost
{
    [ExecuteAlways, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    [AddComponentMenu("MiniGame/画面/复古画面效果")]
    public sealed class RetroScreenEffect : MonoBehaviour
    {
        [SerializeField, InspectorName("单配置／兼容配置")] private RetroScreenProfile profile;
        [SerializeField, InspectorName("跟随房间昼夜调色")] private bool followRoomLighting = true;
        [SerializeField, HideInInspector] private Shader effectShader;
        private Material material;
        private RoomLightingController lightingSource;
        private RetroScreenProfile blendedProfile;
        public bool UsePlayerPixelSettings { get; set; }
        public bool PlayerPixelSettingsActive => UsePlayerPixelSettings && Application.isPlaying && PresentationProfile == null;
        public int EffectivePixelSize => PlayerPixelSettingsActive ? PlayerGraphicsSettings.PixelSize : RenderProfile != null ? RenderProfile.PixelSize : 1;
        // Explicit assignments retain the legacy single-profile override used by captures and presentations.
        public RetroScreenProfile Profile { get => profile; set { profile = value; followRoomLighting = false; } }
        public bool FollowRoomLighting { get => followRoomLighting; set => followRoomLighting = value; }
        public RoomLightingController LightingSource
        {
            get
            {
                if (lightingSource != null && lightingSource.isActiveAndEnabled) return lightingSource;
                lightingSource = null;
                var scene = gameObject.scene;
                if (scene.IsValid() && scene.isLoaded)
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var rooms = root.GetComponentsInChildren<RoomLightingController>();
                        foreach (var room in rooms)
                            if (room.isActiveAndEnabled)
                            {
                                // Ambiguous rooms need an explicit binding rather than borrowing another room's style.
                                if (lightingSource != null) { lightingSource = null; return null; }
                                lightingSource = room;
                            }
                    }
                return lightingSource;
            }
        }
        public void BindLighting(RoomLightingController room) => lightingSource = room;
        public RetroScreenProfile GetProfile(RoomLightingPeriod period)
            => followRoomLighting && LightingSource != null
                ? LightingSource.GetProfile(period)?.ScreenStyle ?? profile : profile;
        public RetroScreenProfile ActiveProfile => PresentationProfile != null ? PresentationProfile
            : GetProfile(LightingSource != null ? LightingSource.EffectivePeriod : RoomLightingPeriod.Day);
        internal RetroScreenProfile RenderProfile => ProfileForRendering();
        // Comparison is transient and never serialized into the scene/profile or included in saves.
        public bool PreviewOriginal { get; set; }
        public bool PresentationBypass { get; set; }
        public RetroScreenProfile PresentationProfile { get; set; }

        public void Configure(RetroScreenProfile settings, Shader shader)
        {
            profile = settings;
            followRoomLighting = false;
            if (effectShader != shader) ReleaseMaterial();
            effectShader = shader;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) => Render(source, destination);

        public void Render(RenderTexture source, RenderTexture destination)
        {
            var profile = ProfileForRendering();
            if (!enabled || PreviewOriginal || PresentationBypass || profile == null || !profile.EffectEnabled || profile.Strength <= 0
                || effectShader == null || !effectShader.isSupported)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (material == null)
                material = new Material(effectShader) { hideFlags = HideFlags.HideAndDontSave };
            material.SetFloat("_Strength", profile.Strength);
            material.SetFloat("_PixelSize", PlayerPixelSettingsActive ? PlayerGraphicsSettings.PixelSize : profile.PixelSize);
            material.SetFloat("_FullscreenQuantization", profile.FullscreenQuantization ? 1 : 0);
            material.SetFloat("_Levels", profile.Levels);
            material.SetFloat("_GammaBefore", profile.GammaBefore);
            material.SetFloat("_GammaAfter", profile.GammaAfter);
            material.SetFloat("_ColorGradingEnabled", profile.ColorGradingEnabled ? 1 : 0);
            material.SetVector("_ColorAdjustments", new Vector4(profile.PostExposure, profile.Contrast, profile.Saturation, 0));
            material.SetColor("_ShadowTint", profile.ShadowTint);
            material.SetColor("_MidtoneTint", profile.MidtoneTint);
            material.SetColor("_HighlightTint", profile.HighlightTint);
            material.SetVector("_ToneExposures", new Vector4(profile.ShadowExposure, profile.MidtoneExposure, profile.HighlightExposure, 0));
            material.SetVector("_ToneRanges", new Vector4(profile.ShadowEnd, profile.HighlightStart, 0, 0));
            material.SetFloat("_NeutralTonemapping", profile.ColorGradingEnabled && profile.NeutralTonemapping ? 1 : 0);
            material.SetFloat("_PerceptualQuantization", profile.PerceptualQuantization ? 1 : 0);
            material.SetFloat("_QuantizationStrength", profile.QuantizationStrength);
            material.SetFloat("_DitherStrength", profile.DitherStrength);
            material.SetFloat("_FilmGrainStrength", profile.FilmGrainStrength);
            material.SetFloat("_FilmGrainFrame", Time.frameCount % 4096);
            material.SetFloat("_UseVirtualResolution", !PlayerPixelSettingsActive && profile.UseVirtualResolution ? 1 : 0);
            material.SetFloat("_VirtualHeight", profile.VirtualHeight);
            Graphics.Blit(source, destination, material);
        }

        private RetroScreenProfile ProfileForRendering()
        {
            if (PresentationProfile != null) return PresentationProfile;
            var room = followRoomLighting ? LightingSource : null;
            if (room == null) return profile;
            var day = GetProfile(RoomLightingPeriod.Day);
            var night = GetProfile(RoomLightingPeriod.Night);
            float amount = room.NightBlend;
            if (amount <= 0 || day == night || night == null) return day;
            if (amount >= 1 || day == null) return night;
            if (blendedProfile == null) blendedProfile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            blendedProfile.hideFlags = HideFlags.HideAndDontSave;
            blendedProfile.BlendFrom(day, night, amount);
            return blendedProfile;
        }

        private void OnDisable() { PreviewOriginal = false; ReleaseMaterial(); ReleaseBlend(); }
        private void OnDestroy() { ReleaseMaterial(); ReleaseBlend(); }
        private void ReleaseBlend()
        {
            if (blendedProfile == null) return;
            if (Application.isPlaying) Destroy(blendedProfile); else DestroyImmediate(blendedProfile);
            blendedProfile = null;
        }
        private void ReleaseMaterial()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            material = null;
        }
    }
}
