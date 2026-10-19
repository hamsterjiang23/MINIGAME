using MiniGame.Core;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public sealed class WorldInteraction : MonoBehaviour
    {
        [SerializeField] private InteractionTarget target;
        [SerializeField] private string label;
        [SerializeField, InspectorName("描边物体")] private Renderer indicator;
        [SerializeField, Min(.1f), InspectorName("交互距离（米）")]
        [Tooltip("从玩家镜头到鼠标命中的碰撞表面的最大距离；门和床不描边，但同样受此距离限制。")]
        private float interactionDistance = 3.2f;
        private InteractionFocusOutline outline;

        public InteractionTarget Target => target;
        public string Label => label;
        public Renderer Surface => indicator;
        public float InteractionDistance
        {
            get => ValidDistance(interactionDistance);
            set => interactionDistance = ValidDistance(value);
        }
        public bool IsHighlighted => outline != null;

        public void Configure(InteractionTarget interaction, string displayName, Renderer surface, float distance = 3.2f)
        {
            ClearOutline();
            target = interaction;
            label = displayName;
            indicator = surface;
            InteractionDistance = distance;
        }

        public void Highlight(bool enabled)
        {
            bool show = enabled && isActiveAndEnabled && target != InteractionTarget.Door && target != InteractionTarget.Bed;
            if (!show)
            {
                ClearOutline();
                return;
            }
            if (outline == null) outline = InteractionFocusOutline.Create(indicator);
        }

        private static float ValidDistance(float value) => float.IsNaN(value) || float.IsInfinity(value)
            ? 3.2f : Mathf.Max(.1f, value);

        private void OnValidate() => interactionDistance = ValidDistance(interactionDistance);
        private void OnDisable() => ClearOutline();
        private void OnDestroy() => ClearOutline();

        private void ClearOutline()
        {
            outline?.Dispose();
            outline = null;
        }
    }
}
