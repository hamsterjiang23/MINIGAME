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
        [Header("梦后行走"), SerializeField] private bool dreamTraversalConfigured;
        [SerializeField, InspectorName("行走区域中心")] private Vector3 dreamTraversalCenter;
        [SerializeField, InspectorName("行走区域尺寸")] private Vector3 dreamTraversalSize = new Vector3(9, 3, 4);
        [SerializeField] private Transform dreamWakePose;
        [SerializeField] private DreamSequenceDirector dreamDirector;
        private CharacterController controller;
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
        public WorldInteraction FocusedInteraction => focused;
        public ReadableProp FocusedReadable => focusedReadable;
        public bool DreamTraversalActive => dreamTraversalActive;
        public void Configure(Camera camera) => viewCamera = camera;
        public void ConfigureDreamTraversal(Bounds bounds, Transform wakePose, DreamSequenceDirector director)
        {
            RestoreDreamTraversal(false);
            dreamTraversalCenter = bounds.center; dreamTraversalSize = bounds.size;
            dreamWakePose = wakePose; dreamDirector = director; dreamTraversalConfigured = true;
            if (runtime != null && isActiveAndEnabled) UpdateDreamTraversal(runtime.GetView());
        }
        private void Awake() => controller = GetComponent<CharacterController>();
        public void Bind(GameRuntime game, MiniGameScreen ui)
        {
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
                || (dreamDirector != null && dreamDirector.OwnsCamera)) { ClearFocus(); return; }
            UpdateDreamTraversal(runtime.GetView());
            RefreshFocus();
        }

        public bool InteractFocused()
        {
            var doorView = viewCamera.GetComponent<DoorViewCamera>();
            if (runtime.GetView().Input == InputContext.DoorView)
                return CompleteWorldInteraction(doorView != null && doorView.TryReturn());
            RefreshInteractionFocus();
            if (focused == null) return false;
            var view = runtime.GetView();
            if (BedtimeGuidance.IsSleepOnly(view) && !view.AllowedInteractions.Contains(focused.Target))
            {
                if (screen != null) screen.ClearNotification();
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
            RefreshInteractionFocus();
            if (focusedReadable != null && CanRead(runtime.GetView()))
            {
                if (screen != null) screen.ShowRoomCaption(focusedReadable.Caption, focusedReadable.CaptionSeconds);
                return CompleteWorldInteraction(true);
            }
            return focused != null && focused.Target == InteractionTarget.Computer && InteractFocused();
        }

        private static bool CanRead(RuntimeView view) => view.Input == InputContext.World && !BedtimeGuidance.IsSleepOnly(view) && !view.DreamTraversal;

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
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                readable = hit.collider.GetComponentInParent<ReadableProp>();
                if (readable != null && (hit.distance > readable.InteractionDistance || !CanRead(runtime.GetView()))) readable = null;
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
                bool canRead = CanRead(view);
                focusedReadable.Highlight(canRead);
                if (screen != null) screen.SetWorldFocus(canRead ? "查看海报" : string.Empty, canRead, "鼠标左键");
                return;
            }
            bool allowed = focused != null && view.AllowedInteractions.Contains(focused.Target);
            var hingedDoor = focused != null ? focused.GetComponentInParent<HingedDoor>() : null;
            if (focused != null && focused.Target == InteractionTarget.Door)
                allowed = !BedtimeGuidance.IsSleepOnly(view) && (view.Game.Phase == GamePhase.MorningInspection || hingedDoor != null && hingedDoor.EnableToggle);
            if (focused != null) focused.Highlight(allowed);
            bool bedtime = BedtimeGuidance.IsSleepOnly(view);
            if (screen != null) screen.SetWorldFocus(focused == null || (bedtime && !allowed) ? string.Empty
                : bedtime ? "躺下休息" : hingedDoor != null ? hingedDoor.IsOpen ? "关门" : "开门" : focused.Label, allowed,
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
            bool requested = dreamTraversalConfigured && view.DreamTraversal;
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
                if (candidate == null || !candidate.isActiveAndEnabled || !view.AllowedInteractions.Contains(candidate.Target)
                    || (candidate.Target != InteractionTarget.Bed && candidate.Target != InteractionTarget.Computer)) continue;
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
            if (wake && dreamWakePose != null)
            {
                transform.SetPositionAndRotation(dreamWakePose.position, Quaternion.Euler(0, dreamWakePose.eulerAngles.y, 0));
                pitch = 0;
                if (viewCamera != null) viewCamera.transform.localRotation = Quaternion.identity;
            }
            if (controller != null) controller.enabled = controllerBeforeDream;
            downwardSpeed = 0;
            dreamTargets = new WorldInteraction[0];
            ClearFocus();
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
