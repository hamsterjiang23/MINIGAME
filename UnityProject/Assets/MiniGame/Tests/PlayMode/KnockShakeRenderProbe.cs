using UnityEngine;

namespace MiniGame.UnityHost.Tests
{
    public sealed class KnockShakeRenderProbe : MonoBehaviour
    {
        public Quaternion RenderRotation { get; private set; }
        public Vector3 RenderPosition { get; private set; }
        private void OnPreRender()
        {
            RenderRotation = transform.rotation;
            RenderPosition = transform.position;
        }
    }
}
