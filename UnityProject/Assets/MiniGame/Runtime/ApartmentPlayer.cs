using System.Linq;
using MiniGame.Core;
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
        public WorldInteraction FocusedInteraction => focused;
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
            bool computerClick = Input.GetMouseButtonDown(0) && focused != null
                && focused.Target == InteractionTarget.Computer;
            if (Input.GetKeyDown(KeyCode.E) || computerClick) InteractFocused();
        }

        public void FacePoint(Vector3 worldPoint)
        {
            var direction = worldPoint - viewCamera.transform.position;
            if (direction.sqrMagnitude < .0001f) return;
            var rotation = Quaternion.LookRotation(direction).eulerAngles;
            transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            pitch = Mathf.Clamp(Mathf.DeltaAngle(0, rotation.x), -75, 75);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        public void RefreshInteractionFocus()
        {
            if (runtime == null || runtime.GetView().Input != InputContext.World) { ClearFocus(); return; }
            RefreshFocus();
        }

        public bool InteractFocused()
        {
            RefreshInteractionFocus();
            if (focused == null) return false;
            var view = runtime.GetView();
            if (BedtimeGuidance.IsSleepOnly(view) && !view.AllowedInteractions.Contains(focused.Target))
            {
                screen.ClearNotification();
                return false;
            }
            var result = runtime.TryInteract(focused.Target);
            if (!result.Accepted)
                screen.Notify(runtime.GetView().Game.Phase == GamePhase.MorningInspection
                    ? "请先到门口完成晨检。" : focused.Target == InteractionTarget.Bed
                        ? "今天的工作尚未结束。" : "请先完成当前的对话或演出。", true);
            else screen.ClearNotification();
            return result.Accepted;
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
            bool bedtime = BedtimeGuidance.IsSleepOnly(view);
            screen.SetWorldFocus(focused == null || (bedtime && !allowed) ? string.Empty
                : bedtime ? "躺下休息" : focused.Label, allowed,
                focused != null && focused.Target == InteractionTarget.Computer ? "E / 鼠标左键" : "E");
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
