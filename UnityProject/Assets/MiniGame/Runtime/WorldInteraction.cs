using MiniGame.Core;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public sealed class WorldInteraction : MonoBehaviour
    {
        [SerializeField] private InteractionTarget target;
        [SerializeField] private string label;
        [SerializeField] private Renderer indicator;
        private Material material;
        private Color baseColor;
        public InteractionTarget Target => target;
        public string Label => label;
        public void Configure(InteractionTarget interaction, string displayName, Renderer surface)
        { target = interaction; label = displayName; indicator = surface; }
        private void Awake()
        {
            if (indicator == null) return;
            material = indicator.material;
            baseColor = material.color;
        }
        public void Highlight(bool enabled)
        {
            if (material != null) material.color = enabled ? Color.Lerp(baseColor, new Color(.56f, .88f, .7f), .5f) : baseColor;
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
