using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(RawImage))]
    public sealed class TerminalSignalEffect : MonoBehaviour
    {
        private RawImage image;
        private Material original, owned;
        private TerminalSignalController controller;
        public bool LogoMode { get; private set; }
        public Material EffectMaterial => owned;

        public void Initialize(TerminalSignalController owner, bool logo)
        {
            controller = owner;
            LogoMode = logo;
            image = GetComponent<RawImage>();
            original = image.material;
            var shader = Resources.Load<Shader>("TerminalSignalDistortion");
            if (shader == null || !shader.isSupported)
            {
                Debug.LogError("标志与监控扰动 Shader 不可用。", this);
                return;
            }
            owned = new Material(shader) { name = name + " signal", hideFlags = HideFlags.DontSave };
            image.material = owned;
            Apply();
        }

        public void SetLogoMode(bool logo) { LogoMode = logo; Apply(); }

        public void Apply()
        {
            if (owned == null || controller == null || controller.Profile == null) return;
            var settings = LogoMode ? controller.Profile.Logo : controller.Profile.Monitor;
            settings.Apply(owned, controller.Clock, LogoMode);
            // Stencil masking can create a separate material; its animated uniforms need updating too.
            if (image != null && image.isActiveAndEnabled)
            {
                var rendered = image.materialForRendering;
                if (rendered != null && rendered != owned) settings.Apply(rendered, controller.Clock, LogoMode);
            }
        }

        private void OnEnable() => Apply();
        private void OnDestroy()
        {
            if (image != null && image.material == owned) image.material = original;
            if (owned != null) Destroy(owned);
        }
    }
}
