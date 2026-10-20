using UnityEngine;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class PropNarrationComposite : MonoBehaviour
    {
        public PropNarrationView Owner { get; set; }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Owner != null) Owner.Composite(source, destination);
            else Graphics.Blit(source, destination);
        }
        private void OnDestroy() { if (Owner != null) Owner.CloseImmediately(); }
    }
}
