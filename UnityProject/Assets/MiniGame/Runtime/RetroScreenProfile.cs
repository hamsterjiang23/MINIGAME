using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/画面/复古效果配置", fileName = "RetroScreenProfile")]
    public sealed class RetroScreenProfile : ScriptableObject
    {
        [SerializeField] private bool effectEnabled = true;
        [SerializeField, Range(0, 1)] private float strength = .65f;
        [SerializeField, Range(1, 8)] private int pixelSize = 2;
        [SerializeField, Range(4, 64)] private int levels = 24;
        [SerializeField, Range(.25f, 2)] private float gammaBefore = .8f;
        [SerializeField, Range(.25f, 3)] private float gammaAfter = 1.25f;

        public bool EffectEnabled { get => effectEnabled; set => effectEnabled = value; }
        public float Strength { get => Safe(strength, 0, 1, .65f); set => strength = Safe(value, 0, 1, .65f); }
        public int PixelSize { get => Mathf.Clamp(pixelSize, 1, 8); set => pixelSize = Mathf.Clamp(value, 1, 8); }
        public int Levels { get => Mathf.Clamp(levels, 4, 64); set => levels = Mathf.Clamp(value, 4, 64); }
        public float GammaBefore { get => Safe(gammaBefore, .25f, 2, .8f); set => gammaBefore = Safe(value, .25f, 2, .8f); }
        public float GammaAfter { get => Safe(gammaAfter, .25f, 3, 1.25f); set => gammaAfter = Safe(value, .25f, 3, 1.25f); }

        public void ResetToLightDefaults()
        {
            effectEnabled = true; strength = .65f; pixelSize = 2;
            levels = 24; gammaBefore = .8f; gammaAfter = 1.25f;
        }

        private void OnValidate()
        {
            Strength = strength; PixelSize = pixelSize; Levels = levels;
            GammaBefore = gammaBefore; GammaAfter = gammaAfter;
        }

        private static float Safe(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
    }
}
