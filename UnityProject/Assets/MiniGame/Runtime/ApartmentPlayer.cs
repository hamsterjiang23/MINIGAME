using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(-100)]
    public sealed class ApartmentPlayer : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float walkSpeed = 2.4f;
        [SerializeField, Range(.1f, 4f)] private float lookSensitivity = PlayerRuntimeSettings.DefaultMouseSensitivity;
        [Header("梦后行走"), SerializeField] private bool dreamTraversalConfigured;
        [SerializeField, InspectorName("行走区域中心")] private Vector3 dreamTraversalCenter;
        [SerializeField, InspectorName("行走区域尺寸")] private Vector3 dreamTraversalSize = new Vector3(9, 3, 4);
        [SerializeField] private Transform dreamWakePose;
        [SerializeField] private DreamSequenceDirector dreamDirector;
        private CharacterController controller;
        private Vector3 standingEyeLocalPosition;
        private bool standingEyeCaptured;
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private WorldInteraction focused;
        private ReadableProp focusedReadable;
        private float pitch;
        private float downwardSpeed;
        private bool dreamTraversalActive, controllerBeforeDream;
        private int dreamTraversalDay;
        private float dreamFloorY;
        private WorldInteraction[] dreamTargets = new WorldInteraction[0];
        public Camera ViewCamera => viewCamera;
        public Vector3 StandingEyeLocalPosition => standingEyeLocalPosition;
        public WorldInteraction FocusedInteraction => focused;
        public ReadableProp FocusedReadable => focusedReadable;
        public bool DreamTraversalActive => dreamTraversalActive;
        public void Configure(Camera camera)
        {
            viewCamera = camera;
            RememberStandingEye();
        }
        public void ConfigureDreamTraversal(Bounds bounds, Transform wakePose, DreamSequenceDirector director)
        {
            RestoreDreamTraversal(false);
            dreamTraversalCenter = bounds.center; dreamTraversalSize = bounds.size;
            dreamWakePose = wakePose; dreamDirector = director; dreamTraversalConfigured = true;
            if (runtime != null && isActiveAndEnabled) UpdateDreamTraversal(runtime.GetView());
        }
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            RememberStandingEye();
        }
        private void RememberStandingEye()
        {
            if (standingEyeCaptured || viewCamera == null) return;
            standingEyeLocalPosition = viewCamera.transform.localPosition;
            standingEyeCaptured = true;
        }
        public void Bind(GameRuntime game, MiniGameScreen ui)
        {
            RememberStandingEye();
            if (runtime != null) runtime.Changed -= OnRuntimeViewChanged;
            RestoreDreamTraversal(false);
            runtime = game; screen = ui;
            ClearFocus();
            if (runtime != null && isActiveAndEnabled)
            {
                runtime.Changed += OnRuntimeViewChanged;
                UpdateDreamTraversal(runtime.GetView());
            }
        }

        private void Update()
        {
            if (runtime == null || viewCamera == null) return;
            if (screen != null && screen.PresentationToolsBlockInput)
            {
                ClearFocus();
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
                return;
            }
            if (MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning) { ClearFocus(); return; }
            var current = runtime.GetView();
            UpdateDreamTraversal(current);
            if (dreamDirector != null && dreamDirector.OwnsCamera)
            {
                ClearFocus();
                Cursor.visible = current.Paused;
                Cursor.lockState = current.Paused ? CursorLockMode.None : CursorLockMode.Locked;
                return;
            }
            var doorCamera = viewCamera.GetComponent<DoorViewCamera>();
            if (doorCamera != null) doorCamera.RecoverDisabledView();
            if (screen != null && screen.PropNarrationBlocksWorld)
            {
                ClearFocus();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }
            if (runtime.GetView().Input == InputContext.DoorView)
            {
                ClearFocus();
                Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;
                if (screen != null) screen.SetWorldFocus("返回房间", true, "E");
                if (Input.GetKeyDown(KeyCode.E)) InteractFocused();
                return;
            }
            bool world = runtime.GetView().Input == InputContext.World;
            Cursor.visible = !world;
            Cursor.lockState = world ? CursorLockMode.Locked : CursorLockMode.None;
            if (!world) { ClearFocus(); return; }

            UpdateLook();
            var motion = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) motion += transform.forward;
            if (Input.GetKey(KeyCode.S)) motion -= transform.forward;
            if (Input.GetKey(KeyCode.A)) motion -= transform.right;
            if (Input.GetKey(KeyCode.D)) motion += transform.right;
            if (dreamTraversalActive) TryMoveDreamTraversal(motion.normalized * walkSpeed * Time.deltaTime);
            else if (controller.enabled)
            {
                downwardSpeed = controller.isGrounded ? -1f : downwardSpeed - 9.81f * Time.deltaTime;
                controller.Move((motion.normalized * walkSpeed + Vector3.up * downwardSpeed) * Time.deltaTime);
            }
            RefreshFocus();
            if (Input.GetKeyDown(KeyCode.E)) InteractFocused();
            else if (Input.GetMouseButtonDown(0)) InteractSecondaryFocused();
        }

        private void UpdateLook()
        {
            // Mouse axes are pinned in ProjectSettings/InputManager.asset.
            float yaw = Input.GetAxisRaw("Mouse X") * lookSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * lookSensitivity, -75f, 75f);
            transform.Rotate(0f, yaw, 0f);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        public void FacePoint(Vector3 worldPoint)
        {
            if (viewCamera == null || (dreamDirector != null && dreamDirector.OwnsCamera)) return;
            var direction = worldPoint - viewCamera.transform.position;
            if (direction.sqrMagnitude < .0001f) return;
            var rotation = Quaternion.LookRotation(direction).eulerAngles;
            transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            pitch = Mathf.Clamp(Mathf.DeltaAngle(0, rotation.x), -75, 75);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        public void RefreshInteractionFocus()
        {
            if (runtime == null || runtime.GetView().Input != InputContext.World
                || (screen != null && screen.PresentationToolsBlockInput)
                || (screen != null && screen.PropNarrationBlocksWorld)
                || (dreamDirector != null && dreamDirector.OwnsCamera)) { ClearFocus(); return; }
            UpdateDreamTraversal(runtime.GetView());
            RefreshFocus();
        }

        public bool InteractFocused()
        {
            if (screen != null && (screen.PresentationToolsBlockInput || screen.PropNarrationBlocksWorld)) return false;
            var doorView = viewCamera.GetComponent<DoorViewCamera>();
            if (runtime.GetView().Input == InputContext.DoorView)
                return CompleteWorldInteraction(doorView != null && doorView.TryReturn());
            RefreshInteractionFocus();
            if (focused == null) return false;
            var view = runtime.GetView();
            if (BedtimeGuidance.IsSleepOnly(view) && !view.AllowedInteractions.Contains(focused.Target))
            {
                if (screen != null) screen.Notify(focused.Target == InteractionTarget.Computer
                    ? "今日审判已结束，电脑暂不可使用，请先休息。" : "夜晚暂不可出门，请先休息。", true);
                return false;
            }
            var door = focused.GetComponentInParent<HingedDoor>();
            if (door != null && door.EnableToggle && doorView != null && doorView.Handles(door))
                return CompleteWorldInteraction(doorView.TryEnter());
            var result = door != null ? door.TryInteract() : runtime.TryInteract(focused.Target);
            if (!result.Accepted && screen != null)
                screen.Notify(runtime.GetView().Game.Phase == GamePhase.MorningInspection
                    ? "请先到门口完成晨检。" : focused.Target == InteractionTarget.Bed
                        ? "今天的工作尚未结束。" : "请先完成当前的对话或演出。", true);
            else if (screen != null) screen.ClearNotification();
            return CompleteWorldInteraction(result.Accepted);
        }

        public bool InteractSecondaryFocused()
        {
            if (screen != null && screen.PresentationToolsBlockInput) return false;
            if (screen != null && screen.PropNarrationBlocksWorld) return false;
            RefreshInteractionFocus();
            if (focusedReadable != null && CanRead(runtime.GetView()) && focusedReadable.IsReadableOnDay(runtime.GetView().Game.Day))
            {
                bool shown = screen != null && screen.ShowPropNarration(focusedReadable, viewCamera);
                if (shown)
                {
                    ClearFocus();
                    Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
                }
                return CompleteWorldInteraction(shown);
            }
            return focused != null && focused.Target == InteractionTarget.Computer && InteractFocused();
        }

        private static bool CanRead(RuntimeView view) => view.Input == InputContext.World && !view.DreamTraversal;

        private bool CompleteWorldInteraction(bool accepted)
        {
            if (accepted && screen != null) screen.Audio?.PlayWorldInteraction();
            return accepted;
        }

        private void RefreshFocus()
        {
            WorldInteraction next = null;
            ReadableProp readable = null;
            var view = runtime.GetView();
            if (dreamTraversalActive) next = FindDreamTarget(view);
            else if (Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out var hit, Mathf.Infinity,
                Physics.DefaultRaycastLayers, view.Game.Day == 0 && BedtimeGuidance.IsSleepOnly(view)
                    ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
            {
                readable = hit.collider.GetComponentInParent<ReadableProp>();
                if (readable != null && (hit.distance > readable.InteractionDistance || !CanRead(view) || !readable.IsReadableOnDay(view.Game.Day))) readable = null;
                if (readable == null) next = hit.collider.GetComponentInParent<WorldInteraction>();
                if (next != null && hit.distance > next.InteractionDistance) next = null;
            }
            if (focusedReadable != readable)
            {
                if (focusedReadable != null) focusedReadable.Highlight(false);
                focusedReadable = readable;
            }
            if (next != focused)
            {
                if (focused != null) focused.Highlight(false);
                focused = next;
            }
            if (focusedReadable != null)
            {
                bool canRead = CanRead(view) && focusedReadable.IsReadableOnDay(view.Game.Day);
                focusedReadable.Highlight(canRead);
                if (screen != null) screen.SetWorldFocus(canRead ? focusedReadable.InteractionLabel : string.Empty, canRead, "鼠标左键");
                return;
            }
            bool allowed = focused != null && view.AllowedInteractions.Contains(focused.Target);
            var hingedDoor = focused != null ? focused.GetComponentInParent<HingedDoor>() : null;
            if (focused != null && focused.Target == InteractionTarget.Door)
                allowed = allowed && !BedtimeGuidance.IsSleepOnly(view)
                    && (view.Game.Phase == GamePhase.MorningInspection || hingedDoor != null && hingedDoor.EnableToggle);
            if (focused != null) focused.Highlight(allowed);
            bool bedtime = BedtimeGuidance.IsSleepOnly(view);
            bool doorLocked = focused != null && focused.Target == InteractionTarget.Door
                && runtime.IsDoorLockedAfterSupervisor && !view.RoomDoorOpen;
            if (screen != null) screen.SetWorldFocus(focused == null ? string.Empty
                : bedtime && focused.Target == InteractionTarget.Bed ? "躺下休息" : doorLocked ? "门已锁" : hingedDoor != null ? hingedDoor.IsOpen ? "关门" : "开门" : focused.Label, allowed,
                focused != null && focused.Target == InteractionTarget.Computer ? "E / 鼠标左键" : "E");
        }

        private void ClearFocus()
        {
            if (focused != null) focused.Highlight(false);
            if (focusedReadable != null) focusedReadable.Highlight(false);
            focused = null;
            focusedReadable = null;
            if (screen != null) screen.SetWorldPrompt(string.Empty);
        }

        private void OnRuntimeViewChanged(RuntimeView view)
        {
            UpdateDreamTraversal(view);
            if (view.Input != InputContext.World) ClearFocus();
        }

        private void UpdateDreamTraversal(RuntimeView view)
        {
            bool requested = dreamTraversalConfigured && view.Game.Day > 0 && view.DreamTraversal;
            if (requested == dreamTraversalActive) return;
            if (!requested)
            {
                bool waking = view.Game.Day > dreamTraversalDay
                    && (view.Game.Phase == GamePhase.MorningInspection || view.Game.Phase == GamePhase.Apartment);
                RestoreDreamTraversal(waking);
                return;
            }
            if (controller == null) controller = GetComponent<CharacterController>();
            dreamTraversalActive = true;
            dreamTraversalDay = view.Game.Day;
            dreamFloorY = transform.position.y;
            controllerBeforeDream = controller.enabled;
            controller.enabled = false;
            downwardSpeed = 0;
            dreamTargets = gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true)).ToArray();
            ClearFocus();
        }

        public bool TryMoveDreamTraversal(Vector3 displacement)
        {
            if (!dreamTraversalActive || runtime == null || runtime.GetView().Input != InputContext.World
                || (dreamDirector != null && dreamDirector.OwnsCamera)) return false;
            var bounds = new Bounds(dreamTraversalCenter, dreamTraversalSize);
            Vector3 next = transform.position + displacement;
            transform.position = new Vector3(Mathf.Clamp(next.x, bounds.min.x, bounds.max.x), dreamFloorY,
                Mathf.Clamp(next.z, bounds.min.z, bounds.max.z));
            return true;
        }

        private WorldInteraction FindDreamTarget(RuntimeView view)
        {
            var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            float nearest = float.PositiveInfinity;
            WorldInteraction result = null;
            foreach (var candidate in dreamTargets)
            {
                if (candidate == null || !candidate.isActiveAndEnabled
                    || (!BedtimeGuidance.IsSleepOnly(view) && !view.AllowedInteractions.Contains(candidate.Target))) continue;
                foreach (var collider in candidate.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || collider.isTrigger || collider.GetComponentInParent<WorldInteraction>() != candidate) continue;
                    if (!collider.Raycast(ray, out var hit, candidate.InteractionDistance) || hit.distance >= nearest) continue;
                    nearest = hit.distance;
                    result = candidate;
                }
            }
            return result;
        }

        private void RestoreDreamTraversal(bool wake)
        {
            if (!dreamTraversalActive) return;
            dreamTraversalActive = false;
            if (controller != null) controller.enabled = false;
            if (wake) PlaceAtWakePose();
            if (controller != null) controller.enabled = controllerBeforeDream;
            downwardSpeed = 0;
            dreamTargets = new WorldInteraction[0];
            ClearFocus();
        }

        // Restore the Day0 wake position even while presentation keeps player input disabled.
        public void RestoreDayZeroWakePose()
        {
            if (dreamWakePose == null) return;
            if (controller == null) controller = GetComponent<CharacterController>();
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            PlaceAtWakePose();
            controller.enabled = wasEnabled;
            downwardSpeed = 0;
            ClearFocus();
        }

        private void PlaceAtWakePose()
        {
            if (dreamWakePose == null) return;
            // The authored anchor is a safe position before furniture becomes solid again.
            // Waking does not replace the player's chosen heading or camera look angles.
            transform.position = dreamWakePose.position;
        }

        private void OnDisable()
        {
            if (runtime != null) runtime.Changed -= OnRuntimeViewChanged;
            RestoreDreamTraversal(false);
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None; ClearFocus();
        }
        private void OnEnable()
        {
            if (runtime == null) return;
            runtime.Changed -= OnRuntimeViewChanged;
            runtime.Changed += OnRuntimeViewChanged;
            UpdateDreamTraversal(runtime.GetView());
        }
        private void OnDestroy()
        {
            if (runtime != null) runtime.Changed -= OnRuntimeViewChanged;
            RestoreDreamTraversal(false);
        }
    }
}
