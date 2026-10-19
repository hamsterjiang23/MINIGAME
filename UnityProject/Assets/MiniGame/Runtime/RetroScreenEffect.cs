using UnityEngine;

namespace MiniGame.UnityHost
{
    [ExecuteAlways, RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    [AddComponentMenu("MiniGame/画面/复古画面效果")]
    public sealed class RetroScreenEffect : MonoBehaviour
    {
        [SerializeField] private RetroScreenProfile profile;
        [SerializeField, HideInInspector] private Shader effectShader;
        private Material material;
        public RetroScreenProfile Profile { get => profile; set => profile = value; }
        // Comparison is transient and never serialized into the scene/profile or included in saves.
        public bool PreviewOriginal { get; set; }

        public void Configure(RetroScreenProfile settings, Shader shader)
        {
            profile = settings;
            if (effectShader != shader) ReleaseMaterial();
            effectShader = shader;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) => Render(source, destination);

        public void Render(RenderTexture source, RenderTexture destination)
        {
            if (!enabled || PreviewOriginal || profile == null || !profile.EffectEnabled || profile.Strength <= 0
                || effectShader == null || !effectShader.isSupported)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (material == null)
                material = new Material(effectShader) { hideFlags = HideFlags.HideAndDontSave };
            material.SetFloat("_Strength", profile.Strength);
            material.SetFloat("_PixelSize", profile.PixelSize);
            material.SetFloat("_Levels", profile.Levels);
            material.SetFloat("_GammaBefore", profile.GammaBefore);
            material.SetFloat("_GammaAfter", profile.GammaAfter);
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
