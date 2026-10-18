using MiniGame.Core;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public sealed class WorldInteraction : MonoBehaviour
    {
        [SerializeField] private InteractionTarget target;
        [SerializeField] private string label;
        [SerializeField] private Renderer indicator;
        private MaterialPropertyBlock properties;
        private Color baseColor;
        private float highlight;
        private bool requested;
        public InteractionTarget Target => target;
        public string Label => label;
        public void Configure(InteractionTarget interaction, string displayName, Renderer surface)
        { target = interaction; label = displayName; indicator = surface; }
        private void Awake()
        {
            if (indicator == null) return;
            if (indicator.sharedMaterial == null || !indicator.sharedMaterial.HasProperty("_Color")) return;
            properties = new MaterialPropertyBlock();
            baseColor = indicator.sharedMaterial.color;
        }
        public void Highlight(bool enabled)
        {
            requested = enabled;
        }
        private void LateUpdate()
        {
            if (properties == null || indicator == null) return;
            highlight = Mathf.MoveTowards(highlight, requested ? 1 : 0, Time.unscaledDeltaTime * 5);
            indicator.GetPropertyBlock(properties);
            properties.SetColor("_Color", Color.Lerp(baseColor, new Color(.56f, .88f, .7f), highlight * .12f));
            indicator.SetPropertyBlock(properties);
        }
        private void OnDisable()
        {
            requested = false; highlight = 0;
            if (properties == null || indicator == null) return;
            indicator.GetPropertyBlock(properties); properties.SetColor("_Color", baseColor); indicator.SetPropertyBlock(properties);
        }
    }
}
