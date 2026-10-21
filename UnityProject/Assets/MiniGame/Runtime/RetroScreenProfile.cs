using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/画面/复古效果配置", fileName = "RetroScreenProfile")]
    public sealed class RetroScreenProfile : ScriptableObject
    {
        [SerializeField] private bool effectEnabled = true;
        [SerializeField, Range(0, 1)] private float strength = .65f;
        [SerializeField, Range(1, 8)] private int pixelSize = 2;
        [SerializeField] private bool fullscreenQuantization = true;
        [SerializeField, Range(4, 64)] private int levels = 24;
        [SerializeField, Range(.25f, 2)] private float gammaBefore = .8f;
        [SerializeField, Range(.25f, 3)] private float gammaAfter = 1.25f;
        [SerializeField] private bool colorGradingEnabled;
        [SerializeField, Range(-2, 2)] private float postExposure;
        [SerializeField, Range(-50, 50)] private float contrast;
        [SerializeField, Range(-100, 100)] private float saturation;
        [SerializeField] private Color shadowTint = Color.white;
        [SerializeField] private Color midtoneTint = Color.white;
        [SerializeField] private Color highlightTint = Color.white;
        [SerializeField, Range(-2, 2)] private float shadowExposure;
        [SerializeField, Range(-2, 2)] private float midtoneExposure;
        [SerializeField, Range(-2, 2)] private float highlightExposure;
        [SerializeField, Range(.05f, .6f)] private float shadowEnd = .35f;
        [SerializeField, Range(.4f, .95f)] private float highlightStart = .65f;
        [SerializeField] private bool neutralTonemapping;
        [SerializeField] private bool perceptualQuantization;
        [SerializeField, Range(0, 1)] private float quantizationStrength = 1;
        [SerializeField, Range(0, 1)] private float ditherStrength;
        [SerializeField, Range(0, .15f)] private float filmGrainStrength;
        [SerializeField] private bool useVirtualResolution;
        [SerializeField, Range(180, 1080)] private int virtualHeight = 540;

        public bool EffectEnabled { get => effectEnabled; set => effectEnabled = value; }
        public float Strength { get => Safe(strength, 0, 1, .65f); set => strength = Safe(value, 0, 1, .65f); }
        public int PixelSize { get => Mathf.Clamp(pixelSize, 1, 8); set => pixelSize = Mathf.Clamp(value, 1, 8); }
        public bool FullscreenQuantization { get => fullscreenQuantization; set => fullscreenQuantization = value; }
        public int Levels { get => Mathf.Clamp(levels, 4, 64); set => levels = Mathf.Clamp(value, 4, 64); }
        public float GammaBefore { get => Safe(gammaBefore, .25f, 2, .8f); set => gammaBefore = Safe(value, .25f, 2, .8f); }
        public float GammaAfter { get => Safe(gammaAfter, .25f, 3, 1.25f); set => gammaAfter = Safe(value, .25f, 3, 1.25f); }
        public bool ColorGradingEnabled { get => colorGradingEnabled; set => colorGradingEnabled = value; }
        public float PostExposure { get => Safe(postExposure, -2, 2, 0); set => postExposure = Safe(value, -2, 2, 0); }
        public float Contrast { get => Safe(contrast, -50, 50, 0); set => contrast = Safe(value, -50, 50, 0); }
        public float Saturation { get => Safe(saturation, -100, 100, 0); set => saturation = Safe(value, -100, 100, 0); }
        public Color ShadowTint { get => SafeTint(shadowTint); set => shadowTint = SafeTint(value); }
        public Color MidtoneTint { get => SafeTint(midtoneTint); set => midtoneTint = SafeTint(value); }
        public Color HighlightTint { get => SafeTint(highlightTint); set => highlightTint = SafeTint(value); }
        public float ShadowExposure { get => Safe(shadowExposure, -2, 2, 0); set => shadowExposure = Safe(value, -2, 2, 0); }
        public float MidtoneExposure { get => Safe(midtoneExposure, -2, 2, 0); set => midtoneExposure = Safe(value, -2, 2, 0); }
        public float HighlightExposure { get => Safe(highlightExposure, -2, 2, 0); set => highlightExposure = Safe(value, -2, 2, 0); }
        public float ShadowEnd { get => Safe(shadowEnd, .05f, .6f, .35f); set => shadowEnd = Safe(value, .05f, .6f, .35f); }
        public float HighlightStart
        {
            get => Mathf.Max(ShadowEnd + .05f, Safe(highlightStart, .4f, .95f, .65f));
            set => highlightStart = Mathf.Max(ShadowEnd + .05f, Safe(value, .4f, .95f, .65f));
        }
        public bool NeutralTonemapping { get => neutralTonemapping; set => neutralTonemapping = value; }
        public bool PerceptualQuantization { get => perceptualQuantization; set => perceptualQuantization = value; }
        public float QuantizationStrength { get => Safe(quantizationStrength, 0, 1, 1); set => quantizationStrength = Safe(value, 0, 1, 1); }
        public float DitherStrength { get => Safe(ditherStrength, 0, 1, 0); set => ditherStrength = Safe(value, 0, 1, 0); }
        public float FilmGrainStrength { get => Safe(filmGrainStrength, 0, .15f, 0); set => filmGrainStrength = Safe(value, 0, .15f, 0); }
        public bool UseVirtualResolution { get => useVirtualResolution; set => useVirtualResolution = value; }
        public int VirtualHeight { get => Mathf.Clamp(virtualHeight, 180, 1080); set => virtualHeight = Mathf.Clamp(value, 180, 1080); }

        public void ResetToLightDefaults()
        {
            ResetNewControls();
            effectEnabled = true; strength = .65f; pixelSize = 2; fullscreenQuantization = true;
            levels = 24; gammaBefore = .8f; gammaAfter = 1.25f;
        }

        public void ResetToNaturalDefaults()
        {
            ResetNewControls();
            effectEnabled = true; strength = 1; pixelSize = 1; fullscreenQuantization = false;
            levels = 10; gammaBefore = gammaAfter = 1;
        }

        public void ResetToReferenceStyle()
        {
            ResetToNaturalDefaults();
            colorGradingEnabled = true; postExposure = .5f; contrast = -5; saturation = -5;
            shadowTint = new Color(.93f, 1, .90f, 1);
            midtoneTint = new Color(1, .99f, .91f, 1);
            highlightTint = new Color(1, .99f, .95f, 1);
            // Prepare restrained optional posterization; the preset does not enable it.
            perceptualQuantization = true; quantizationStrength = .35f; ditherStrength = .15f;
            useVirtualResolution = false; virtualHeight = 540;
        }

        private void ResetNewControls()
        {
            colorGradingEnabled = false; postExposure = contrast = saturation = 0;
            shadowTint = midtoneTint = highlightTint = Color.white;
            shadowExposure = midtoneExposure = highlightExposure = 0;
            shadowEnd = .35f; highlightStart = .65f; neutralTonemapping = false;
            perceptualQuantization = false; quantizationStrength = 1; ditherStrength = 0;
            filmGrainStrength = 0;
            useVirtualResolution = false; virtualHeight = 540;
        }

        private void OnValidate()
        {
            Strength = strength; PixelSize = pixelSize; Levels = levels;
            GammaBefore = gammaBefore; GammaAfter = gammaAfter;
            PostExposure = postExposure; Contrast = contrast; Saturation = saturation;
            ShadowTint = shadowTint; MidtoneTint = midtoneTint; HighlightTint = highlightTint;
            ShadowExposure = shadowExposure; MidtoneExposure = midtoneExposure; HighlightExposure = highlightExposure;
            ShadowEnd = shadowEnd; HighlightStart = highlightStart;
            QuantizationStrength = quantizationStrength; DitherStrength = ditherStrength; VirtualHeight = virtualHeight;
            FilmGrainStrength = filmGrainStrength;
        }

        private static Color SafeTint(Color value) => new Color(
            Safe(value.r, 0, 4, 1), Safe(value.g, 0, 4, 1), Safe(value.b, 0, 4, 1), 1);

        private static float Safe(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
    }
}
