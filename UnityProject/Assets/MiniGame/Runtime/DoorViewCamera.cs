using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum DoorCameraPhase { None, Aligning, WaitingForDoor, Pushing, Viewing, Returning }

    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class DoorViewCamera : MonoBehaviour
    {
        [SerializeField] private HingedDoor door;
        [SerializeField] private ApartmentPlayer player;
        [SerializeField, InspectorName("门内对齐点")] private Transform insidePose;
        [SerializeField, InspectorName("门口观察点")] private Transform outsidePose;
        [SerializeField, Min(.01f), InspectorName("定位时长（秒）")] private float alignSeconds = .65f;
        [SerializeField, Min(.01f), InspectorName("推进时长（秒）")] private float pushSeconds = .9f;
        [SerializeField, Min(.01f), InspectorName("返回时长（秒）")] private float returnSeconds = 1.1f;
        private Camera viewCamera;
        private GameRuntime runtime;
        private string activeToken;
        private bool hasRoomPose;
        private Vector3 roomPosition, playerPosition, sourcePosition;
        private Quaternion roomRotation, playerRotation, sourceRotation;
        private float roomFieldOfView, elapsed;

        public DoorCameraPhase Phase { get; private set; }
        public bool HasRoomPose => hasRoomPose;
        public Transform InsidePose => insidePose;
        public Transform OutsidePose => outsidePose;
        public Vector3 ReturnPosition => roomPosition;
        public Quaternion ReturnRotation => roomRotation;
        public float ReturnFieldOfView => roomFieldOfView;
        public bool Handles(HingedDoor target) => isActiveAndEnabled && door == target && insidePose != null && outsidePose != null;

        public void Configure(HingedDoor target, ApartmentPlayer owner, Transform alignment, Transform observation)
        { door = target; player = owner; insidePose = alignment; outsidePose = observation; viewCamera = GetComponent<Camera>(); }

        public void PrepareForRuntimeChange(GameRuntime nextRuntime)
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            if (door != null) door.MotionCompletionBlocked = false;
            RestoreRoomPose(!nextRuntime.GetView().DoorViewActive);
            Phase = DoorCameraPhase.None; activeToken = null;
        }

        // An explicit visual save supplies a new base pose; a runtime-only rebind still retains its current capture.
        internal void DiscardRoomPoseCapture() => hasRoomPose = false;

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            if (door != null) door.MotionCompletionBlocked = false;
            runtime = game;
            viewCamera = GetComponent<Camera>();
            activeToken = null;
            Phase = DoorCameraPhase.None;
            if (game.GetView().DoorViewActive && Handles(door))
            {
                RememberRoomPose();
                var cue = game.GetView().Cue;
                if (cue?.Kind == PresentationKind.DoorViewReturn) Begin(cue.Token, DoorCameraPhase.Returning);
                else if (cue?.Kind == PresentationKind.DoorMotion)
                {
                    Begin(cue.Token, DoorCameraPhase.Aligning);
                    door.MotionCompletionBlocked = true;
                }
                else
                {
                    ApplyPose(outsidePose.position, outsidePose.rotation);
                    Phase = DoorCameraPhase.Viewing;
                }
            }
            else RestoreRoomPose();
            runtime.Changed += OnViewChanged;
        }

        public bool TryEnter()
        {
            if (!Handles(door) || runtime == null || !door.EnableToggle || runtime.GetView().Input != InputContext.World) return false;
            bool previousBlock = door.MotionCompletionBlocked;
            door.MotionCompletionBlocked = true;
            var result = runtime.EnterDoorView();
            if (!result.Accepted) door.MotionCompletionBlocked = previousBlock;
            return result.Accepted;
        }

        public bool TryReturn() => runtime != null && runtime.ExitDoorView().Accepted;

        public void RecoverDisabledView()
        {
            if (isActiveAndEnabled || runtime == null || runtime.GetView().Paused || !runtime.GetView().DoorViewActive) return;
            if (runtime.GetView().Input == InputContext.DoorView) runtime.ExitDoorView();
            var cue = runtime.GetView().Cue;
            if (cue?.Kind == PresentationKind.DoorViewReturn) runtime.CompleteCue(cue.Token);
        }

        private void OnViewChanged(RuntimeView view)
        {
            if (!isActiveAndEnabled || insidePose == null || outsidePose == null || door == null) return;
            if (!view.DoorViewActive)
            {
                RestoreRoomPose();
                door.MotionCompletionBlocked = false;
                activeToken = null;
                Phase = DoorCameraPhase.None;
                return;
            }
            var cue = view.Cue;
            if (cue == null || cue.Token == activeToken) return;
            if (cue.Kind == PresentationKind.DoorMotion && view.RoomDoorOpen)
            {
                RememberRoomPose();
                Begin(cue.Token, DoorCameraPhase.Aligning);
                door.MotionCompletionBlocked = true;
            }
            else if (cue.Kind == PresentationKind.DoorViewReturn)
                Begin(cue.Token, DoorCameraPhase.Returning);
        }

        private void Begin(string token, DoorCameraPhase phase)
        {
            activeToken = token;
            Phase = phase;
            elapsed = 0;
            sourcePosition = viewCamera.transform.position;
            sourceRotation = viewCamera.transform.rotation;
        }

        private void LateUpdate()
        {
            if (runtime == null || runtime.GetView().Paused || !hasRoomPose) return;
            if (Phase == DoorCameraPhase.Viewing) { ApplyPose(outsidePose.position, outsidePose.rotation); return; }
            if (Phase == DoorCameraPhase.None) return;
            var cue = runtime.GetView().Cue;
            if (cue == null || cue.Token != activeToken) return;
            elapsed += Time.unscaledDeltaTime;
            if (Phase == DoorCameraPhase.Aligning)
            {
                MoveTo(insidePose.position, insidePose.rotation, alignSeconds);
                if (elapsed >= Duration(alignSeconds)) { Phase = DoorCameraPhase.WaitingForDoor; elapsed = 0; }
            }
            else if (Phase == DoorCameraPhase.WaitingForDoor)
            {
                ApplyPose(insidePose.position, insidePose.rotation);
                if (!door.IsMoving)
                {
                    if (Vector3.SqrMagnitude(outsidePose.position - insidePose.position) < .000001f
                        && Quaternion.Angle(outsidePose.rotation, insidePose.rotation) < .01f)
                        FinishEntry();
                    else
                    {
                        Phase = DoorCameraPhase.Pushing; elapsed = 0;
                        sourcePosition = insidePose.position; sourceRotation = insidePose.rotation;
                    }
                }
            }
            else if (Phase == DoorCameraPhase.Pushing)
            {
                MoveTo(outsidePose.position, outsidePose.rotation, pushSeconds);
                if (elapsed >= Duration(pushSeconds))
                {
                    FinishEntry();
                }
            }
            else if (Phase == DoorCameraPhase.Returning)
            {
                // A threshold hold has no outward leg to retrace before returning to the room.
                float progress = Mathf.Clamp01(elapsed / Duration(returnSeconds));
                if (Vector3.SqrMagnitude(sourcePosition - insidePose.position) < .000001f)
                {
                    float p = Smooth(progress);
                    ApplyPose(Vector3.Lerp(sourcePosition, roomPosition, p), Quaternion.Slerp(sourceRotation, roomRotation, p));
                }
                else if (progress < .5f)
                {
                    float p = Smooth(progress * 2);
                    ApplyPose(Vector3.Lerp(sourcePosition, insidePose.position, p), Quaternion.Slerp(sourceRotation, insidePose.rotation, p));
                }
                else
                {
                    float p = Smooth((progress - .5f) * 2);
                    ApplyPose(Vector3.Lerp(insidePose.position, roomPosition, p), Quaternion.Slerp(insidePose.rotation, roomRotation, p));
                }
                if (progress >= 1)
                {
                    string token = activeToken;
                    RestoreRoomPose();
                    Phase = DoorCameraPhase.None; activeToken = null;
                    runtime.CompleteCue(token);
                }
            }
        }

        private void FinishEntry()
        {
            ApplyPose(outsidePose.position, outsidePose.rotation);
            Phase = DoorCameraPhase.Viewing;
            door.MotionCompletionBlocked = false;
        }

        private void MoveTo(Vector3 destination, Quaternion rotation, float seconds)
        {
            float progress = Smooth(Mathf.Clamp01(elapsed / Duration(seconds)));
            ApplyPose(Vector3.Lerp(sourcePosition, destination, progress), Quaternion.Slerp(sourceRotation, rotation, progress));
        }
        private void ApplyPose(Vector3 position, Quaternion rotation)
        { viewCamera.transform.SetPositionAndRotation(position, rotation); viewCamera.fieldOfView = roomFieldOfView; }
        private void RememberRoomPose()
        {
            if (hasRoomPose) return;
            hasRoomPose = true;
            roomPosition = viewCamera.transform.position; roomRotation = viewCamera.transform.rotation;
            roomFieldOfView = viewCamera.fieldOfView;
            playerPosition = player.transform.position; playerRotation = player.transform.rotation;
        }
        private void RestoreRoomPose(bool clearCapture = true)
        {
            if (!hasRoomPose || viewCamera == null || player == null) return;
            var controller = player.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled) controller.enabled = false;
            player.transform.SetPositionAndRotation(playerPosition, playerRotation);
            ApplyPose(roomPosition, roomRotation);
            player.FacePoint(roomPosition + roomRotation * Vector3.forward);
            if (wasEnabled) controller.enabled = true;
            if (clearCapture) hasRoomPose = false;
        }
        private static float Smooth(float p) => p * p * (3 - 2 * p);
        private static float Duration(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Max(.01f, value);
        private void OnDisable()
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            if (door != null) door.MotionCompletionBlocked = false;
            RestoreRoomPose(); Phase = DoorCameraPhase.None; activeToken = null;
        }
        private void OnEnable() { if (runtime != null) Bind(runtime); }
    }
}
