using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(Camera))]
    public sealed class ReferenceCameraProjection : MonoBehaviour
    {
        [SerializeField, Range(20, 120)] private float horizontalFieldOfView = 61.9275f;
        private Camera view;
        private GameRuntime runtime;
        public float HorizontalFieldOfView { get => horizontalFieldOfView; set => horizontalFieldOfView = Mathf.Clamp(value, 20, 120); }
        public void Configure(float angle) { HorizontalFieldOfView = angle; Apply(); }
        public void Bind(GameRuntime game) { runtime = game; if (!game.GetView().TerminalOpen) Apply(); }
        private void Awake() => view = GetComponent<Camera>();
        private void LateUpdate()
        {
            if (runtime != null && runtime.GetView().Input == InputContext.World) Apply();
        }
        private void Apply()
        {
            if (view == null) view = GetComponent<Camera>();
            view.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(horizontalFieldOfView * .5f * Mathf.Deg2Rad) / view.aspect) * Mathf.Rad2Deg;
        }
    }
}
