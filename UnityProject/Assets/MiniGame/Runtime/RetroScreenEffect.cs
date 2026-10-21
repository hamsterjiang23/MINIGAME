using UnityEngine;

namespace MiniGame.UnityHost
{
    [ExecuteAlways, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    [AddComponentMenu("MiniGame/画面/复古画面效果")]
    public sealed class RetroScreenEffect : MonoBehaviour
    {
        [SerializeField] private RetroScreenProfile profile;
        [SerializeField, HideInInspector] private Shader effectShader;
        private Material material;
        public RetroScreenProfile Profile { get => profile; set => profile = value; }
        // Comparison is transient and never serialized into the scene/profile or included in saves.
        public bool PreviewOriginal { get; set; }
        public bool PresentationBypass { get; set; }

        public void Configure(RetroScreenProfile settings, Shader shader)
        {
            profile = settings;
            if (effectShader != shader) ReleaseMaterial();
            effectShader = shader;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) => Render(source, destination);

        public void Render(RenderTexture source, RenderTexture destination)
        {
            if (!enabled || PreviewOriginal || PresentationBypass || profile == null || !profile.EffectEnabled || profile.Strength <= 0
                || effectShader == null || !effectShader.isSupported)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (material == null)
                material = new Material(effectShader) { hideFlags = HideFlags.HideAndDontSave };
            material.SetFloat("_Strength", profile.Strength);
            material.SetFloat("_PixelSize", profile.PixelSize);
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
            material.SetFloat("_UseVirtualResolution", profile.UseVirtualResolution ? 1 : 0);
            material.SetFloat("_VirtualHeight", profile.VirtualHeight);
            Graphics.Blit(source, destination, material);
        }

        private void OnDisable() { PreviewOriginal = false; ReleaseMaterial(); }
        private void OnDestroy() => ReleaseMaterial();
        private void ReleaseMaterial()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            material = null;
        }
    }
}
