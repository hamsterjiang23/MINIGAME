using System;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Animates the host-owned door target, then acknowledges its presentation token.</summary>
    public sealed class HingedDoor : MonoBehaviour
    {
        [SerializeField] private bool enableToggle = true;
        [SerializeField] private float openAngle = 60f;
        [SerializeField, Min(0)] private float animationSeconds = .6f;
        [SerializeField] private Transform pivot;
        private Transform capturedPivot;
        private Quaternion closedRotation;
        private GameRuntime runtime;
        private bool subscribed;
        private float startAngle, targetAngle, elapsed;
        private string pendingToken;
        private string lastSoundToken;
        public event Action<HingedDoor, bool> MotionStarted;
        internal bool IsBoundTo(GameRuntime game) => runtime == game;

        public bool EnableToggle
        {
            get => enableToggle;
            set { enableToggle = value; RefreshConfiguration(); }
        }
        public float OpenAngle
        {
            get => openAngle;
            set { openAngle = IsFinite(value) ? value : 60f; RefreshConfiguration(); }
        }
        public float AnimationSeconds
        {
            get => animationSeconds;
            set => animationSeconds = IsFinite(value) ? Mathf.Max(0, value) : .6f;
        }
        public bool IsMoving { get; private set; }
        public bool MotionCompletionBlocked { get; set; }
        public float CurrentAngle { get; private set; }
        public bool IsOpen => enableToggle && (runtime != null ? runtime.GetView().RoomDoorOpen : Mathf.Abs(CurrentAngle) > .01f);

        public void Configure(Transform hinge, float angle = 60f, float duration = .6f)
        {
            pivot = hinge != null ? hinge : transform;
            CaptureClosedPose();
            openAngle = IsFinite(angle) ? angle : 60f;
            animationSeconds = IsFinite(duration) ? Mathf.Max(0, duration) : .6f;
            RefreshConfiguration();
        }

        public void Bind(GameRuntime game)
        {
            Unsubscribe();
            runtime = game ?? throw new ArgumentNullException(nameof(game));
            CaptureClosedPose();
            var view = runtime.GetView();
            Snap(TargetAngle(view));
            pendingToken = view.Cue?.Kind == PresentationKind.DoorMotion ? view.Cue.Token : null;
            lastSoundToken = pendingToken; // Restored/rebound poses are silent.
            Subscribe();
        }

        public RuntimeResult TryInteract()
        {
            if (runtime == null) throw new InvalidOperationException("Bind the room door before interacting with it.");
            return enableToggle ? runtime.ToggleRoomDoor() : runtime.TryInteract(InteractionTarget.Door);
        }

        private void Awake() => CaptureClosedPose();

        private void OnEnable()
        {
            if (runtime == null) return;
            Bind(runtime);
        }

        private void OnDisable()
        {
            Unsubscribe();
            pendingToken = null;
            IsMoving = false;
        }

        private void OnDestroy() => Unsubscribe();

        private void OnValidate()
        {
            if (!IsFinite(openAngle)) openAngle = 60f;
            if (!IsFinite(animationSeconds)) animationSeconds = .6f;
            animationSeconds = Mathf.Max(0, animationSeconds);
            if (Application.isPlaying) RefreshConfiguration();
        }

        private void CaptureClosedPose()
        {
            if (pivot == null) pivot = transform;
            if (capturedPivot == pivot) return;
            capturedPivot = pivot;
            closedRotation = pivot.localRotation;
            CurrentAngle = targetAngle = 0;
        }

        private void Subscribe()
        {
            if (runtime == null || subscribed || !isActiveAndEnabled) return;
            runtime.Changed += OnViewChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (runtime != null && subscribed) runtime.Changed -= OnViewChanged;
            subscribed = false;
        }

        private void RefreshConfiguration()
        {
            if (runtime == null) return;
            OnViewChanged(runtime.GetView());
        }

        private float TargetAngle(RuntimeView view) => enableToggle && view.RoomDoorOpen ? openAngle : 0;

        private void OnViewChanged(RuntimeView view)
        {
            pendingToken = view.Cue?.Kind == PresentationKind.DoorMotion ? view.Cue.Token : null;
            float desired = TargetAngle(view);
            if (!enableToggle)
            {
                Snap(0);
                return;
            }
            if (Mathf.Approximately(targetAngle, desired)) return;
            startAngle = CurrentAngle;
            targetAngle = desired;
            elapsed = 0;
            IsMoving = !Mathf.Approximately(startAngle, targetAngle);
        }

        private void Update()
        {
            if (runtime == null || runtime.GetView().Paused) return;
            var view = runtime.GetView();
            // Inspector edits and rebinding do not require a new story command.
            if (!Mathf.Approximately(targetAngle, TargetAngle(view))) OnViewChanged(view);
            if (IsMoving)
            {
                if (pendingToken != null && pendingToken != lastSoundToken)
                {
                    lastSoundToken = pendingToken;
                    MotionStarted?.Invoke(this, Mathf.Abs(targetAngle) > .01f);
                }
                elapsed += Time.unscaledDeltaTime;
                float fraction = animationSeconds <= 0 ? 1 : Mathf.Clamp01(elapsed / animationSeconds);
                SetAngle(Mathf.LerpUnclamped(startAngle, targetAngle, Mathf.SmoothStep(0, 1, fraction)));
                if (fraction >= 1)
                {
                    SetAngle(targetAngle);
                    IsMoving = false;
                }
            }
            if (IsMoving || pendingToken == null || MotionCompletionBlocked) return;
            var token = pendingToken;
            pendingToken = null;
            if (view.Cue?.Kind == PresentationKind.DoorMotion && view.Cue.Token == token)
                runtime.CompleteCue(token);
        }

        private void Snap(float angle)
        {
            targetAngle = startAngle = angle;
            elapsed = 0;
            IsMoving = false;
            SetAngle(angle);
        }

        private void SetAngle(float angle)
        {
            CaptureClosedPose();
            CurrentAngle = angle;
            pivot.localRotation = closedRotation * Quaternion.AngleAxis(angle, Vector3.up);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
