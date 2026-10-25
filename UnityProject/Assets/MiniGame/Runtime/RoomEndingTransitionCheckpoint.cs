using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum RoomEndingTransitionPhase { FadingOut, BlackHold, Entering, Exploring, Failed }

    [Serializable]
    public sealed class RoomEndingTransitionCheckpoint
    {
        public RoomEndingTransitionPhase Phase;
        public float Elapsed;
        public bool HasEntryPose;
        public bool MonitorReturn;
        public Vector3 EntryStartPosition;
        public Quaternion EntryStartRotation = Quaternion.identity;
        public Vector3 EntryTargetPosition;
        public Quaternion EntryTargetRotation = Quaternion.identity;

        public RoomEndingTransitionCheckpoint Copy() => (RoomEndingTransitionCheckpoint)MemberwiseClone();

        public bool IsValid()
        {
            if (!Enum.IsDefined(typeof(RoomEndingTransitionPhase), Phase) || !Finite(Elapsed) || Elapsed < 0) return false;
            float maximum = Phase == RoomEndingTransitionPhase.FadingOut ? RoomEndingTransitionClock.FadeOutSeconds
                : Phase == RoomEndingTransitionPhase.BlackHold ? RoomEndingTransitionClock.BlackHoldSeconds
                : Phase == RoomEndingTransitionPhase.Entering ? RoomEndingTransitionClock.EntrySeconds : 0;
            if (Elapsed > maximum || Phase == RoomEndingTransitionPhase.Entering && !HasEntryPose) return false;
            return !HasEntryPose || Finite(EntryStartPosition) && Finite(EntryTargetPosition)
                && ValidRotation(EntryStartRotation) && ValidRotation(EntryTargetRotation);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool ValidRotation(Quaternion value) => Finite(value.x) && Finite(value.y) && Finite(value.z)
            && Finite(value.w) && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > .0001f;
    }

    /// <summary>Counts only visible phase time, so scene preparation never consumes the required black hold.</summary>
    public sealed class RoomEndingTransitionClock
    {
        public const float FadeOutSeconds = .35f;
        public const float BlackHoldSeconds = 3f;
        public const float EntrySeconds = TerminalReturnMotion.Seconds;
        public RoomEndingTransitionCheckpoint State { get; private set; }
        public float Opacity => State.Phase == RoomEndingTransitionPhase.FadingOut
            ? Mathf.Clamp01(State.Elapsed / FadeOutSeconds)
            : State.Phase == RoomEndingTransitionPhase.Entering ? State.MonitorReturn ? 0 : 1 - Mathf.Clamp01(State.Elapsed / EntrySeconds)
            : State.Phase == RoomEndingTransitionPhase.Exploring ? 0 : 1;

        public RoomEndingTransitionClock(RoomEndingTransitionCheckpoint saved = null)
        {
            State = saved?.Copy() ?? new RoomEndingTransitionCheckpoint();
            if (!Enum.IsDefined(typeof(RoomEndingTransitionPhase), State.Phase)) State.Phase = RoomEndingTransitionPhase.FadingOut;
            if (float.IsNaN(State.Elapsed) || float.IsInfinity(State.Elapsed) || State.Elapsed < 0) State.Elapsed = 0;
            float maximum = State.Phase == RoomEndingTransitionPhase.FadingOut ? FadeOutSeconds
                : State.Phase == RoomEndingTransitionPhase.BlackHold ? BlackHoldSeconds : EntrySeconds;
            State.Elapsed = Mathf.Min(State.Elapsed, maximum);
        }

        public void Advance(float dt, bool sceneReady, bool paused)
        {
            if (paused || float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0
                || State.Phase == RoomEndingTransitionPhase.Exploring || State.Phase == RoomEndingTransitionPhase.Failed) return;
            State.Elapsed += dt;
            if (State.Phase == RoomEndingTransitionPhase.FadingOut && State.Elapsed >= FadeOutSeconds)
                Next(RoomEndingTransitionPhase.BlackHold);
            else if (State.Phase == RoomEndingTransitionPhase.BlackHold && State.Elapsed >= BlackHoldSeconds)
            {
                State.Elapsed = BlackHoldSeconds;
                if (sceneReady) Next(RoomEndingTransitionPhase.Entering);
            }
            else if (State.Phase == RoomEndingTransitionPhase.Entering && State.Elapsed >= EntrySeconds)
                Next(RoomEndingTransitionPhase.Exploring);
        }

        public void Fail() => Next(RoomEndingTransitionPhase.Failed);
        public void Retry() => Next(RoomEndingTransitionPhase.BlackHold);
        private void Next(RoomEndingTransitionPhase phase) { State.Phase = phase; State.Elapsed = 0; }
    }
}
