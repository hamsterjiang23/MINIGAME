using System.Linq;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ApartmentPlayer : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float walkSpeed = 2.4f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float reach = 3.2f;
        private CharacterController controller;
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private WorldInteraction focused;
        private float pitch;
        private float downwardSpeed;
        public Camera ViewCamera => viewCamera;
        public void Configure(Camera camera) => viewCamera = camera;
        private void Awake() => controller = GetComponent<CharacterController>();
        public void Bind(GameRuntime game, MiniGameScreen ui)
        {
            runtime = game; screen = ui;
            ClearFocus();
        }

        private void Update()
        {
            if (runtime == null || viewCamera == null) return;
            bool world = runtime.GetView().Input == InputContext.World;
            Cursor.visible = !world;
            Cursor.lockState = world ? CursorLockMode.Locked : CursorLockMode.None;
            if (!world) { ClearFocus(); return; }

            // Mouse axes are pinned in ProjectSettings/InputManager.asset.
            float yaw = Input.GetAxisRaw("Mouse X") * lookSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * lookSensitivity, -75f, 75f);
            transform.Rotate(0f, yaw, 0f);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            var motion = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) motion += transform.forward;
            if (Input.GetKey(KeyCode.S)) motion -= transform.forward;
            if (Input.GetKey(KeyCode.A)) motion -= transform.right;
            if (Input.GetKey(KeyCode.D)) motion += transform.right;
            downwardSpeed = controller.isGrounded ? -1f : downwardSpeed - 9.81f * Time.deltaTime;
            controller.Move((motion.normalized * walkSpeed + Vector3.up * downwardSpeed) * Time.deltaTime);
            RefreshFocus();
            if (Input.GetKeyDown(KeyCode.E) && focused != null)
            {
                var result = runtime.TryInteract(focused.Target);
                if (!result.Accepted) screen.Notify("此时无法操作：" + result.Reason, true);
            }
        }

        private void RefreshFocus()
        {
            WorldInteraction next = null;
            if (Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out var hit, reach))
                next = hit.collider.GetComponentInParent<WorldInteraction>();
            if (next != focused)
            {
                if (focused != null) focused.Highlight(false);
                focused = next;
            }
            var view = runtime.GetView();
            bool allowed = focused != null && view.AllowedInteractions.Contains(focused.Target);
            if (focused != null) focused.Highlight(allowed);
            screen.SetWorldPrompt(focused == null ? string.Empty : (allowed ? "[E] " : "暂不可用 · ") + focused.Label);
        }

        private void ClearFocus()
        {
            if (focused != null) focused.Highlight(false);
            focused = null;
            if (screen != null) screen.SetWorldPrompt(string.Empty);
        }
        private void OnDisable() { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; ClearFocus(); }
    }
}
