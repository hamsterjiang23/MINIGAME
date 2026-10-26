using System;
using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum TerminalEntryPhase { None, Sitting, Aligning, Pushing, FadingWorld, RevealingInterface, Active, OpeningMonitor, OpeningPullback }

    public enum SleepTransitionPhase { None, FadingOut, BlackHold, Waking, WaitingToWake }

    [Serializable]
    public sealed class SleepTransitionCheckpoint
    {
        public int Version = 2;
        public int Day;
        public GamePhase GamePhase;
        public SleepTransitionPhase Phase;
        public float FadeElapsed;
        public float BlackHoldRemaining;
        public float WakeElapsed;
        public bool HasCameraPose;
        public Quaternion StandingRotation = Quaternion.identity;
        public Quaternion ActorRotation = Quaternion.identity;
        public bool HasPositionPose;
        public Vector3 StandingPosition, StandingLocalPosition, BedPosition;
        public bool HasBedRotation;
        public Quaternion BedRotation = Quaternion.identity;

        public bool Matches(RuntimeView view) => (Version == 1 || Version == 2) && view != null
            && Day == view.Game.Day && GamePhase == view.Game.Phase
            && ((Phase == SleepTransitionPhase.FadingOut || Phase == SleepTransitionPhase.BlackHold)
                && view.Cue?.Kind == PresentationKind.Sleep
                || Version == 2 && Phase == SleepTransitionPhase.Waking && IsWakeView(view)
                || Version == 2 && Phase == SleepTransitionPhase.WaitingToWake && view.Game.Phase == MiniGame.Core.GamePhase.AfterDream)
            && !float.IsNaN(FadeElapsed) && !float.IsInfinity(FadeElapsed) && FadeElapsed >= 0 && FadeElapsed <= .8f
            && !float.IsNaN(BlackHoldRemaining) && !float.IsInfinity(BlackHoldRemaining)
            && BlackHoldRemaining >= 0 && BlackHoldRemaining <= 180
            && !float.IsNaN(WakeElapsed) && !float.IsInfinity(WakeElapsed) && WakeElapsed >= 0 && WakeElapsed <= 180
            && (Phase != SleepTransitionPhase.BlackHold || FadeElapsed == .8f)
            && (!HasCameraPose || ValidRotation(StandingRotation) && ValidRotation(ActorRotation))
            && (!HasPositionPose || HasCameraPose && ValidPosition(StandingPosition) && ValidPosition(StandingLocalPosition) && ValidPosition(BedPosition))
            && (!HasBedRotation || HasCameraPose && ValidRotation(BedRotation));

        public static bool IsWakeView(RuntimeView view) => view != null
            && (view.Game.Phase == MiniGame.Core.GamePhase.MorningInspection || view.Game.Phase == MiniGame.Core.GamePhase.Apartment
                || view.EndingStage == EndingStage.AwaitingDoor);

        public SleepTransitionCheckpoint ForRestoredView(RuntimeView view)
        {
            if (float.IsNaN(WakeElapsed) || float.IsInfinity(WakeElapsed) || WakeElapsed < 0 || WakeElapsed > 180) return null;
            bool legacyWake = Day > 0 && GamePhase == MiniGame.Core.GamePhase.AfterDream
                && (Version == 1 || Version == 2)
                && (Phase == SleepTransitionPhase.WaitingToWake || Phase == SleepTransitionPhase.FadingOut
                    || Phase == SleepTransitionPhase.BlackHold && FadeElapsed == .8f);
            if (legacyWake && view != null && (view.Game.Day == Day && view.Game.Phase == MiniGame.Core.GamePhase.AfterDream
                || view.Game.Day == Day + 1 && IsWakeView(view)))
            {
                var mapped = Copy();
                mapped.Version = 2; mapped.Day = view.Game.Day; mapped.GamePhase = view.Game.Phase;
                mapped.Phase = view.Game.Phase == MiniGame.Core.GamePhase.AfterDream
                    ? SleepTransitionPhase.WaitingToWake : SleepTransitionPhase.Waking;
                mapped.WakeElapsed = 0;
                return mapped.Matches(view) ? mapped : null;
            }
            return Matches(view) ? Copy() : null;
        }

        private static bool ValidPosition(Vector3 value) => !float.IsNaN(value.sqrMagnitude) && !float.IsInfinity(value.sqrMagnitude);

        private static bool ValidRotation(Quaternion value)
        {
            float length = Quaternion.Dot(value, value);
            return !float.IsNaN(length) && !float.IsInfinity(length) && length > .0001f;
        }

        public SleepTransitionCheckpoint Copy() => (SleepTransitionCheckpoint)MemberwiseClone();
    }

    /// <summary>Visual completion is acknowledged with the captured runtime cue token.</summary>
    public sealed class PresentationEffects : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform terminalCameraPose;
        [SerializeField] private Light apartmentLight;
        [SerializeField] private Light bedGuide;
        [SerializeField] private Renderer terminalSurface;
        [SerializeField] private BedtimeGuidance bedtimeGuidance;
        [SerializeField] private ApartmentPlayer seatedPlayer;
        [SerializeField] private Transform seatEyePose;
        [SerializeField] private Transform screenAlignPose;
        [SerializeField] private float daylightIntensity = 1.8f;
        [SerializeField] private Color daylightColor = new Color(.89f, .91f, .74f);
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private Coroutine playing;
        private string activeToken;
        private Vector3 roomPosition;
        private Quaternion roomRotation;
        private float roomFieldOfView;
        private bool hasRoomPose;
        private bool night;
        private MaterialPropertyBlock surfaceProperties;
        private Color surfaceEmission;
        private bool surfaceReady;
        private TerminalSeating seating;
        [SerializeField, Min(.01f)] private float bedLookSeconds = 1.25f;
        [SerializeField, Min(.01f)] private float bedMoveSeconds = .65f;
        [SerializeField, Min(.01f)] private float bedFadeSeconds = .4f;
        [SerializeField, Min(0)] private float sleepBlackSeconds = 1.5f;
        private Quaternion sleepStandingRotation, sleepActorRotation;
        private Quaternion sleepBedRotation;
        private Vector3 sleepStandingPosition, sleepStandingLocalPosition, sleepBedPosition;
        private bool hasSleepView, completingSleep, waking;
        private int lastPresentedDay = -1;
        private SleepTransitionCheckpoint sleepCheckpoint;
        public bool SleepCameraActive => sleepCheckpoint != null || hasSleepView || waking;
        public float TerminalTransitionProgress { get; private set; }
        public bool IsTransitioning => screen != null && screen.TransitionLocked;
        public TerminalEntryPhase EntryPhase { get; private set; }
        public bool IsSeated => seating != null && seating.IsSeated;
        private bool HasSeatedEntry => seatedPlayer != null && seatEyePose != null && screenAlignPose != null && terminalSurface != null;

        public void Configure(Camera camera, Transform terminalPose, Light ceilingLight, Light sleepGuide)
        {
            viewCamera = camera; terminalCameraPose = terminalPose; apartmentLight = ceilingLight; bedGuide = sleepGuide;
            if (ceilingLight != null) { daylightIntensity = ceilingLight.intensity; daylightColor = ceilingLight.color; }
        }
        public void ConfigureTerminalSurface(Renderer surface) { terminalSurface = surface; surfaceReady = false; }
        public void ConfigureBedtime(BedtimeGuidance guidance) => bedtimeGuidance = guidance;
        public void PrewarmPresentation() => bedtimeGuidance?.PrewarmPresentation();
        public void PrepareForRuntimeChange()
        {
            if (runtime != null) runtime.Changed -= Present;
            if (playing != null) StopCoroutine(playing);
            playing = null;
            sleepCheckpoint = null;
            RestoreSleepView();
            screen?.EndDayZeroOpening();
            seating?.Restore(true);
            if (hasRoomPose)
            {
                viewCamera.transform.SetPositionAndRotation(roomPosition, roomRotation);
                viewCamera.fieldOfView = roomFieldOfView;
            }
            hasRoomPose = false;
            activeToken = null;
            EntryPhase = TerminalEntryPhase.None;
        }

        public bool TryGetRoomReturnPose(out Vector3 actorPosition, out Quaternion actorRotation,
            out Vector3 cameraPosition, out Quaternion cameraRotation, out float fieldOfView)
        {
            actorPosition = seatedPlayer != null ? seatedPlayer.transform.position : Vector3.zero;
            actorRotation = seatedPlayer != null ? seatedPlayer.transform.rotation : Quaternion.identity;
            if (hasSleepView)
            {
                actorRotation = sleepActorRotation;
                cameraPosition = StandingPositionForCurrentActor(); cameraRotation = sleepStandingRotation;
                fieldOfView = viewCamera.fieldOfView;
                return true;
            }
            if (seating != null && seating.TryGetStandingPose(out var position, out var rotation))
            { actorPosition = position; actorRotation = rotation; }
            cameraPosition = roomPosition; cameraRotation = roomRotation; fieldOfView = roomFieldOfView;
            return hasRoomPose && seatedPlayer != null;
        }
        public void ConfigureSeatedEntry(ApartmentPlayer player, Transform seatEyes, Transform alignment, Renderer surface)
        {
            seatedPlayer = player; seatEyePose = seatEyes; screenAlignPose = alignment;
            ConfigureTerminalSurface(surface);
        }

        public void Bind(GameRuntime game, MiniGameScreen ui, Action<string> preparationMarker = null)
        {
            if (runtime != null) runtime.Changed -= Present;
            if (playing != null) StopCoroutine(playing);
            playing = null;
            RestoreSleepView();
            runtime = game; screen = ui; activeToken = null;
            lastPresentedDay = game.GetView().Game.Day;
            sleepCheckpoint = null;
            screen.EndDayZeroOpening();
            screen.SetTransitionLocked(false); screen.SetFade(0); screen.SetGlitch(0);
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
            screen.SetTerminalHandoff(0, false, false);
            var view = game.GetView();
            preparationMarker?.Invoke("install.effects_reset");
            if (HasSeatedEntry && seating == null) seating = new TerminalSeating(seatedPlayer, viewCamera);
            EntryPhase = TerminalEntryPhase.None;
            if (hasRoomPose && !view.TerminalOpen)
            {
                seating?.Restore(true);
                viewCamera.transform.SetPositionAndRotation(roomPosition, roomRotation);
                viewCamera.fieldOfView = roomFieldOfView;
                hasRoomPose = false;
            }
            if (view.TerminalOpen)
            {
                RememberRoomPose();
                var kind = view.Cue?.Kind;
                if (HasSeatedEntry)
                {
                    seating.Capture();
                    UpdateScreenDestination();
                    if (kind == PresentationKind.EnterTerminal)
                    {
                        seating.Restore(false);
                        viewCamera.transform.SetPositionAndRotation(roomPosition, roomRotation);
                        viewCamera.fieldOfView = roomFieldOfView;
                    }
                    else { seating.SetPose(seatEyePose, 1); EntryPhase = TerminalEntryPhase.Active; }
                }
                if (kind != PresentationKind.EnterTerminal)
                {
                    viewCamera.transform.SetPositionAndRotation(terminalCameraPose.position, terminalCameraPose.rotation);
                    viewCamera.fieldOfView = 48;
                }
            }
            preparationMarker?.Invoke("install.effects_terminal_pose");
            screen.SetTerminalReveal(view.TerminalOpen && view.Cue?.Kind != PresentationKind.EnterTerminal ? 1 : 0);
            SetSurfaceGlow(view.TerminalPowered ? 1 : 0);
            preparationMarker?.Invoke("install.effects_terminal_glow");
            if (bedtimeGuidance != null) bedtimeGuidance.Bind(game);
            preparationMarker?.Invoke("install.effects_bedtime");
            runtime.Changed += Present;
            Present(view);
            preparationMarker?.Invoke("install.effects_present");
        }

        private void RememberRoomPose()
        {
            if (hasRoomPose) return;
            // Day0 enters the terminal automatically, before the player can walk to
            // the desk. Capture its authored standing approach instead of the spawn.
            var view = runtime.GetView();
            if (HasSeatedEntry && view.Game.Day == 0 && view.TerminalPage == TerminalPage.Review
                && view.TerminalBootRequired && view.Cue?.Kind == PresentationKind.EnterTerminal)
            {
                Transform approach = null;
                foreach (var root in seatedPlayer.gameObject.scene.GetRootGameObjects())
                    foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
                        if (candidate.name == "Computer approach") approach = candidate;
                if (approach != null)
                {
                    var controller = seatedPlayer.GetComponent<CharacterController>();
                    bool wasEnabled = controller.enabled;
                    controller.enabled = false;
                    seatedPlayer.transform.position = approach.position;
                    controller.enabled = wasEnabled;
                }
            }
            roomPosition = viewCamera.transform.position;
            roomRotation = viewCamera.transform.rotation;
            var projection = viewCamera.GetComponent<ReferenceCameraProjection>();
            roomFieldOfView = projection != null && projection.isActiveAndEnabled
                ? projection.VerticalFieldOfView : viewCamera.fieldOfView;
            hasRoomPose = true;
        }

        private void Update()
        {
            if (runtime == null || runtime.GetView().Paused || apartmentLight == null
                || (bedtimeGuidance != null && bedtimeGuidance.isActiveAndEnabled)) return;
            float blend = 1 - Mathf.Exp(-Time.unscaledDeltaTime * 3);
            apartmentLight.intensity = Mathf.Lerp(apartmentLight.intensity, daylightIntensity * (night ? .32f : 1), blend);
            apartmentLight.color = Color.Lerp(apartmentLight.color,
                night ? Color.Lerp(daylightColor, new Color(.43f, .54f, .67f), .65f) : daylightColor, blend);
        }

        private void OnDestroy() { if (runtime != null) runtime.Changed -= Present; }

        private void Present(RuntimeView view)
        {
            night = view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.Sleeping
                || view.Game.Phase == GamePhase.Dream || view.Game.Phase == GamePhase.AfterDream;
            if (bedGuide != null && (bedtimeGuidance == null || !bedtimeGuidance.isActiveAndEnabled))
                bedGuide.enabled = view.Game.Phase == GamePhase.Evening;
            if (completingSleep) return;
            bool newMorning = lastPresentedDay >= 0 && view.Game.Day > lastPresentedDay
                && SleepTransitionCheckpoint.IsWakeView(view);
            lastPresentedDay = view.Game.Day;
            if (newMorning)
            {
                CaptureSleepView();
                activeToken = view.Cue?.Token;
                if (playing != null) StopCoroutine(playing);
                playing = StartCoroutine(WakeTransition());
                return;
            }
            var cue = view.Cue;
            string token = cue?.Token;
            if (activeToken == token)
            {
                if (cue == null) SetSurfaceGlow(view.TerminalPowered ? 1 : 0);
                return;
            }
            activeToken = token;
            sleepCheckpoint = null;
            if (playing != null) { StopCoroutine(playing); playing = null; }
            bool projectionDream = view.UsesDreamFlow && cue?.Kind == PresentationKind.Dream;
            screen.SetTransitionLocked(projectionDream);
            screen.EndDayZeroOpening();
            screen.SetFade(projectionDream ? 1 : 0); screen.SetGlitch(0); screen.SetTerminalHandoff(0, false, false);
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
            if (cue == null)
            {
                SetSurfaceGlow(view.TerminalPowered ? 1 : 0);
                screen.SetTerminalReveal(view.TerminalOpen ? 1 : 0); return;
            }
            switch (cue.Kind)
            {
                case PresentationKind.EnterTerminal:
                    RememberRoomPose();
                    if (view.Game.Day == 0 && view.TerminalPage == TerminalPage.Review && view.TerminalBootRequired)
                    {
                        playing = StartCoroutine(DayZeroOpening(cue.Token));
                        break;
                    }
                    if (view.TerminalBootRequired) screen.BeginTerminalBoot();
                    playing = HasSeatedEntry ? StartCoroutine(SeatedEntry(cue.Token))
                        : StartCoroutine(CameraTransition(cue.Token, terminalCameraPose.position, terminalCameraPose.rotation, false));
                    break;
                case PresentationKind.ExitTerminal:
                    playing = view.UsesTerminalDirectory && !view.LeavingTerminal
                        ? StartCoroutine(TerminalSignalLoss(cue.Token))
                        : StartCoroutine(CameraTransition(cue.Token, hasRoomPose ? roomPosition : viewCamera.transform.position,
                            hasRoomPose ? roomRotation : viewCamera.transform.rotation, true));
                    break;
                case PresentationKind.Sleep: playing = StartCoroutine(SleepTransition(cue.Token)); break;
                case PresentationKind.Reaction: playing = StartCoroutine(ReactionSignal(cue.Token)); break;
            }
        }

        private IEnumerator DayZeroOpening(string token)
        {
            SetSurfaceGlow(1);
            if (HasSeatedEntry)
            {
                seating.Capture(); seating.SetPose(seatEyePose, 1); UpdateScreenDestination();
            }
            if (terminalCameraPose != null)
            {
                viewCamera.transform.SetPositionAndRotation(terminalCameraPose.position, terminalCameraPose.rotation);
                viewCamera.fieldOfView = 48;
            }
            screen.BeginDayZeroOpening(); EntryPhase = TerminalEntryPhase.OpeningMonitor;
            TerminalTransitionProgress = 0;
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.Audio?.PlayTerminalPowerOn();
            // Keep the first view visible until a fresh click; the reveal never advances tutorial dialogue.
            yield return null;
            while (activeToken == token)
            {
                if (!runtime.GetView().Paused && screen.ConsumeOpeningClick()) break;
                yield return null;
            }
            if (activeToken != token) yield break;
            EntryPhase = TerminalEntryPhase.OpeningPullback;
            const float duration = 2.2f;
            float elapsed = 0;
            while (elapsed < duration && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                TerminalTransitionProgress = Mathf.Clamp01(elapsed / duration);
                screen.TerminalDisplay.SetOpeningProgress(Ease(TerminalTransitionProgress));
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.EndDayZeroOpening(); EntryPhase = TerminalEntryPhase.Active;
            screen.SetTransitionLocked(false); playing = null;
            runtime.CompleteCue(token);
        }

        public bool BeginMenuOpeningPullback()
        {
            if (screen == null || !screen.DayZeroOpeningWaiting || runtime?.GetView().Cue?.Kind != PresentationKind.EnterTerminal)
                return false;
            if (playing != null) StopCoroutine(playing);
            playing = null;
            EntryPhase = TerminalEntryPhase.OpeningPullback;
            TerminalTransitionProgress = 0;
            return true;
        }

        public void ApplyMenuOpeningPullback(float progress)
        {
            if (EntryPhase != TerminalEntryPhase.OpeningPullback) return;
            TerminalTransitionProgress = Mathf.Clamp01(progress);
            screen.TerminalDisplay.SetOpeningProgress(Ease(TerminalTransitionProgress));
        }

        public void CompleteMenuOpeningPullback()
        {
            if (EntryPhase != TerminalEntryPhase.OpeningPullback || activeToken == null) return;
            string token = activeToken;
            screen.EndDayZeroOpening(); EntryPhase = TerminalEntryPhase.Active;
            screen.SetTransitionLocked(false);
            runtime.CompleteCue(token);
        }

        private IEnumerator CameraTransition(string token, Vector3 destination, Quaternion rotation, bool exiting)
        {
            bool shuttingDown = exiting && runtime.GetView().TerminalShuttingDown;
            bool booting = !exiting && runtime.GetView().TerminalBootRequired;
            float idleGlow = shuttingDown || booting ? 0 : 1;
            if (shuttingDown)
            {
                yield return PowerOff(token);
                if (activeToken != token) yield break;
                // The room is now dark except for the bed. Returning to the computer's
                // old heading leaves a completely black view and hides the next action.
                if (bedtimeGuidance != null)
                {
                    var towardBed = bedtimeGuidance.SleepFocusPoint - destination;
                    if (towardBed.sqrMagnitude > .001f) rotation = Quaternion.LookRotation(towardBed, Vector3.up);
                }
            }
            var source = viewCamera.transform.position;
            var sourceRotation = viewCamera.transform.rotation;
            float sourceFov = viewCamera.fieldOfView;
            // The viewport may have changed while the terminal was open. End on the
            // same lens the world projection will use when it resumes in LateUpdate.
            var projection = viewCamera.GetComponent<ReferenceCameraProjection>();
            float targetFov = exiting && projection != null && projection.isActiveAndEnabled
                ? projection.VerticalFieldOfView : exiting ? roomFieldOfView : 48;
            float duration = exiting ? TerminalReturnMotion.Seconds : 1.65f;
            var controlA = source + (exiting ? -viewCamera.transform.forward * .12f : viewCamera.transform.forward * .20f);
            var controlB = destination - rotation * Vector3.forward * .15f + Vector3.up * .025f;
            screen.SetTransitionLocked(true);
            screen.SetTerminalReveal(0);
            TerminalTransitionProgress = 0;
            float elapsed = 0;
            while (elapsed < duration && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                TerminalTransitionProgress = p;
                float move = Ease(Mathf.InverseLerp(exiting ? .28f : 0, exiting ? 1 : .68f, p));
                float inverse = 1 - move;
                if (exiting && HasSeatedEntry) seating.SetPose(seatEyePose, 1 - move);
                viewCamera.transform.position = inverse * inverse * inverse * source
                    + 3 * inverse * inverse * move * controlA + 3 * inverse * move * move * controlB + move * move * move * destination;
                viewCamera.transform.rotation = Quaternion.Slerp(sourceRotation, rotation, move);
                viewCamera.fieldOfView = Mathf.Lerp(sourceFov, targetFov, move);
                if (exiting) TerminalReturnMotion.Apply(viewCamera, source, sourceRotation, sourceFov,
                    destination, rotation, targetFov, p);
                screen.SetTerminalReveal(exiting ? 0 : Ease(Mathf.InverseLerp(.82f, 1, p)));
                screen.SetTerminalHandoff(p, exiting, true);
                SetSurfaceGlow(exiting ? idleGlow : Mathf.Lerp(idleGlow, 1, Mathf.InverseLerp(.15f, .7f, p)));
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            viewCamera.transform.SetPositionAndRotation(destination, rotation);
            viewCamera.fieldOfView = targetFov;
            if (booting)
            {
                yield return PowerOn(token);
                if (activeToken != token) yield break;
            }
            if (exiting)
            {
                seating?.Restore(true);
                viewCamera.transform.SetPositionAndRotation(destination, rotation);
                if (shuttingDown && seatedPlayer != null)
                    seatedPlayer.FacePoint(destination + rotation * Vector3.forward);
                hasRoomPose = false;
                EntryPhase = TerminalEntryPhase.None;
            }
            screen.SetTerminalReveal(exiting ? 0 : 1);
            screen.SetTerminalHandoff(1, exiting, false);
            screen.SetTransitionLocked(false); screen.SetGlitch(0);
            playing = null;
            runtime.CompleteCue(token);
        }

        private void UpdateScreenDestination()
        {
            // Project the actual mesh bounds onto the aligned camera axes rather than relying on world AABB size.
            var filter = terminalSurface.GetComponent<MeshFilter>();
            Bounds bounds = filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds : terminalSurface.localBounds;
            var center = terminalSurface.transform.TransformPoint(bounds.center);
            var normal = -screenAlignPose.forward;
            float halfWidth = 0, halfHeight = 0, halfDepth = 0;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        var corner = terminalSurface.transform.TransformPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))) - center;
                        halfWidth = Mathf.Max(halfWidth, Mathf.Abs(Vector3.Dot(corner, screenAlignPose.right)));
                        halfHeight = Mathf.Max(halfHeight, Mathf.Abs(Vector3.Dot(corner, screenAlignPose.up)));
                        halfDepth = Mathf.Max(halfDepth, Mathf.Abs(Vector3.Dot(corner, normal)));
                    }
            float tangent = Mathf.Tan(48 * .5f * Mathf.Deg2Rad);
            float distance = Mathf.Min(halfWidth / (tangent * Mathf.Max(viewCamera.aspect, 16f / 9f)), halfHeight / tangent) * .88f;
            distance = Mathf.Max(viewCamera.nearClipPlane * 1.25f, distance);
            terminalCameraPose.SetPositionAndRotation(center + normal * (halfDepth + distance), screenAlignPose.rotation);
        }

        private IEnumerator SeatedEntry(string token)
        {
            bool booting = runtime.GetView().TerminalBootRequired;
            seating.Capture();
            UpdateScreenDestination();
            var source = viewCamera.transform.position;
            var sourceRotation = viewCamera.transform.rotation;
            float sourceFov = viewCamera.fieldOfView;
            var seatedRotation = Quaternion.Slerp(sourceRotation, seatEyePose.rotation, .3f);
            const float sitEnd = .65f, alignEnd = 1.30f, pushEnd = 2.45f, duration = 3f;
            screen.SetTransitionLocked(true);
            screen.SetTerminalReveal(0);
            screen.SetTerminalHandoff(0, false, false);
            TerminalTransitionProgress = 0;
            float elapsed = 0;
            while (elapsed < duration && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                TerminalTransitionProgress = Mathf.Clamp01(elapsed / duration);
                if (elapsed < sitEnd)
                {
                    EntryPhase = TerminalEntryPhase.Sitting;
                    float p = Ease(elapsed / sitEnd);
                    seating.SetPose(seatEyePose, p);
                    viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(source, seatEyePose.position, p)
                        - Vector3.up * (.025f * Mathf.Sin(p * Mathf.PI)), Quaternion.Slerp(sourceRotation, seatedRotation, p));
                }
                else if (elapsed < alignEnd)
                {
                    EntryPhase = TerminalEntryPhase.Aligning;
                    seating.SetPose(seatEyePose, 1);
                    float p = Ease(Mathf.InverseLerp(sitEnd, alignEnd, elapsed));
                    viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(seatEyePose.position, screenAlignPose.position, p),
                        Quaternion.Slerp(seatedRotation, screenAlignPose.rotation, p));
                    viewCamera.fieldOfView = Mathf.Lerp(sourceFov, 48, p);
                }
                else
                {
                    seating.SetPose(seatEyePose, 1);
                    UpdateScreenDestination();
                    viewCamera.fieldOfView = 48;
                    float p = Ease(Mathf.InverseLerp(alignEnd, pushEnd, elapsed));
                    viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(screenAlignPose.position, terminalCameraPose.position, p), screenAlignPose.rotation);
                    EntryPhase = elapsed < pushEnd ? TerminalEntryPhase.Pushing : TerminalEntryPhase.RevealingInterface;
                    screen.SetTerminalReveal(Ease(Mathf.InverseLerp(pushEnd - .2f, duration, elapsed)));
                    screen.SetFade(0);
                }
                SetSurfaceGlow(booting ? Mathf.Clamp01(elapsed / alignEnd) : 1);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            viewCamera.transform.SetPositionAndRotation(terminalCameraPose.position, terminalCameraPose.rotation);
            screen.SetFade(0); screen.SetTerminalReveal(1);
            if (booting) yield return PowerOn(token);
            if (activeToken != token) yield break;
            EntryPhase = TerminalEntryPhase.Active;
            screen.SetFade(0); screen.SetTerminalReveal(1); screen.SetTransitionLocked(false);
            playing = null;
            runtime.CompleteCue(token);
        }

        private static float Ease(float value) => value * value * value * (value * (value * 6 - 15) + 10);

        private IEnumerator PowerOn(string token)
        {
            screen.SetTransitionLocked(true); screen.SetTerminalReveal(1); screen.SetTerminalPowerFrame(TerminalPowerTimeline.Boot(0));
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.Audio?.PlayTerminalPowerOn();
            float elapsed = 0;
            while (elapsed < TerminalPowerTimeline.BootDuration + TerminalPowerTimeline.LoadingDuration && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                if (elapsed < TerminalPowerTimeline.BootDuration && screen.ConsumeBootSkip()) elapsed = TerminalPowerTimeline.BootDuration;
                screen.SetTerminalPowerFrame(TerminalPowerTimeline.Boot(elapsed));
                elapsed += Time.unscaledDeltaTime; yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken == token) screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
        }

        private IEnumerator PowerOff(string token)
        {
            screen.SetTransitionLocked(true); screen.SetTerminalReveal(1);
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.Audio?.PlayTerminalPowerOff();
            float elapsed = 0;
            while (elapsed < TerminalPowerTimeline.ShutdownDuration && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                screen.SetTerminalPowerFrame(TerminalPowerTimeline.Shutdown(elapsed));
                yield return null;
                while (activeToken == token && runtime.GetView().Paused) yield return null;
                if (activeToken != token) yield break;
                elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, Time.unscaledDeltaTime);
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken == token)
            {
                SetSurfaceGlow(0);
                screen.SetTerminalPowerFrame(TerminalPowerTimeline.Shutdown(TerminalPowerTimeline.ShutdownDuration)); screen.SetTerminalReveal(0);
            }
        }

        public void RefreshTerminalSurfaceGlow()
        {
            if (runtime != null) SetSurfaceGlow(runtime.GetView().TerminalPowered ? 1 : 0);
        }

        private void SetSurfaceGlow(float amount)
        {
            var view = runtime?.GetView();
            if (view != null && !view.TerminalOpen && (view.IsSelfReview
                || view.Game.Phase == GamePhase.Dream && bedtimeGuidance != null && bedtimeGuidance.DreamReturnSelfReviewVisible))
                amount = .55f;
            if (terminalSurface == null || terminalSurface.sharedMaterial == null) return;
            if (!surfaceReady)
            {
                surfaceProperties = new MaterialPropertyBlock();
                surfaceEmission = terminalSurface.sharedMaterial.HasProperty("_EmissionColor")
                    ? terminalSurface.sharedMaterial.GetColor("_EmissionColor") : new Color(.38f, .65f, .46f);
                surfaceReady = true;
            }
            terminalSurface.GetPropertyBlock(surfaceProperties);
            SetLiveDisplay(surfaceProperties);
            surfaceProperties.SetColor("_EmissionColor", surfaceEmission * amount);
            surfaceProperties.SetFloat("_CrtPower", amount);
            terminalSurface.SetPropertyBlock(surfaceProperties);
            // Room lighting uses material-slot blocks, which take precedence over renderer-wide values.
            for (int slot = 0; slot < terminalSurface.sharedMaterials.Length; slot++)
            {
                terminalSurface.GetPropertyBlock(surfaceProperties, slot);
                SetLiveDisplay(surfaceProperties);
                surfaceProperties.SetColor("_EmissionColor", surfaceEmission * amount);
                surfaceProperties.SetFloat("_CrtPower", amount);
                terminalSurface.SetPropertyBlock(surfaceProperties, slot);
            }
        }

        private void SetLiveDisplay(MaterialPropertyBlock properties)
        {
            var view = runtime?.GetView();
            bool live = view != null && (view.TerminalOpen || view.TerminalPowered)
                && screen != null && screen.TerminalDisplay != null;
            properties.SetFloat("_LiveDisplay", live ? 1 : 0);
            if (live) properties.SetTexture("_LiveDisplayTex", screen.TerminalDisplay.Texture);
        }

        public SleepTransitionCheckpoint CaptureSleepCheckpoint()
            => sleepCheckpoint != null && sleepCheckpoint.Matches(runtime?.GetView()) ? sleepCheckpoint.Copy() : null;

        public bool RestoreSleepCheckpoint(SleepTransitionCheckpoint checkpoint)
        {
            checkpoint = checkpoint?.ForRestoredView(runtime?.GetView());
            if (checkpoint == null) return false;
            if (playing != null) StopCoroutine(playing);
            CaptureSleepView();
            sleepCheckpoint = checkpoint.Copy();
            if (checkpoint.HasCameraPose)
            {
                sleepStandingRotation = Quaternion.Normalize(checkpoint.StandingRotation);
                sleepActorRotation = Quaternion.Normalize(checkpoint.ActorRotation);
                hasSleepView = true;
            }
            if (checkpoint.HasPositionPose)
            {
                sleepStandingPosition = checkpoint.StandingPosition;
                sleepStandingLocalPosition = checkpoint.StandingLocalPosition;
                sleepBedPosition = checkpoint.BedPosition;
            }
            sleepBedRotation = checkpoint.HasBedRotation ? Quaternion.Normalize(checkpoint.BedRotation)
                : Quaternion.Euler(-90, sleepStandingRotation.eulerAngles.y, 0);
            activeToken = runtime.GetView().Cue?.Token;
            if (checkpoint.Phase == SleepTransitionPhase.WaitingToWake)
            {
                playing = null;
                screen.SetTransitionLocked(true);
                ApplyWakeFrame(0);
            }
            else playing = StartCoroutine(checkpoint.Phase == SleepTransitionPhase.Waking
                ? WakeTransition() : SleepTransition(activeToken));
            return true;
        }

        public void RestoreLegacyDreamWake()
        {
            var view = runtime?.GetView();
            if (view == null || view.Game.Phase != GamePhase.AfterDream && !SleepTransitionCheckpoint.IsWakeView(view)) return;
            if (playing != null) StopCoroutine(playing);
            CaptureSleepView();
            sleepCheckpoint = CreateSleepCheckpoint(view, view.Game.Phase == GamePhase.AfterDream
                ? SleepTransitionPhase.WaitingToWake : SleepTransitionPhase.Waking);
            activeToken = view.Cue?.Token;
            if (view.Game.Phase == GamePhase.AfterDream)
            {
                playing = null;
                screen.SetTransitionLocked(true);
                ApplyWakeFrame(0);
            }
            else playing = StartCoroutine(WakeTransition());
        }

        private SleepTransitionCheckpoint CreateSleepCheckpoint(RuntimeView view, SleepTransitionPhase phase) => new SleepTransitionCheckpoint
        {
            Day = view.Game.Day, GamePhase = view.Game.Phase, Phase = phase,
            HasCameraPose = true, StandingRotation = sleepStandingRotation, ActorRotation = sleepActorRotation,
            HasPositionPose = true, StandingPosition = sleepStandingPosition,
            StandingLocalPosition = sleepStandingLocalPosition, BedPosition = sleepBedPosition,
            HasBedRotation = true, BedRotation = sleepBedRotation
        };

        private IEnumerator SleepTransition(string token)
        {
            CaptureSleepView();
            screen.SetTransitionLocked(true);
            var view = runtime.GetView();
            if (sleepCheckpoint == null || !sleepCheckpoint.Matches(view))
            {
                float hold = sleepBlackSeconds;
                if (float.IsNaN(hold) || float.IsInfinity(hold)) hold = 1.5f;
                sleepCheckpoint = CreateSleepCheckpoint(view, SleepTransitionPhase.FadingOut);
                sleepCheckpoint.BlackHoldRemaining = Mathf.Clamp(hold, 0, 180);
            }
            var checkpoint = sleepCheckpoint;
            float duration = Mathf.Max(.01f, bedMoveSeconds) + Mathf.Max(.01f, bedLookSeconds) + Mathf.Max(.01f, bedFadeSeconds);
            var ceilingRotation = sleepBedRotation;
            ApplySleepFrame(checkpoint.FadeElapsed, duration, ceilingRotation);
            while (checkpoint.FadeElapsed < .8f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                checkpoint.FadeElapsed = Mathf.Min(.8f, checkpoint.FadeElapsed + Time.unscaledDeltaTime * .8f / duration);
                ApplySleepFrame(checkpoint.FadeElapsed, duration, ceilingRotation);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            checkpoint.Phase = SleepTransitionPhase.BlackHold;
            while (checkpoint.BlackHoldRemaining > 0 && activeToken == token)
            {
                if (!runtime.GetView().Paused)
                    checkpoint.BlackHoldRemaining = Mathf.Max(0, checkpoint.BlackHoldRemaining - Time.unscaledDeltaTime);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            playing = null;
            sleepCheckpoint = null;
            viewCamera.transform.position = sleepStandingPosition;
            viewCamera.transform.rotation = sleepStandingRotation;
            completingSleep = true;
            try { runtime.CompleteCue(token); }
            finally { completingSleep = false; }
            // A dream now owns its own return view; a later bed interaction captures the player's new heading.
            if (runtime.GetView().Game.Day == view.Game.Day) hasSleepView = false;
            Present(runtime.GetView());
        }

        private void ApplySleepFrame(float progress, float duration, Quaternion ceilingRotation)
        {
            float seconds = progress / .8f * duration;
            viewCamera.transform.position = Vector3.Lerp(sleepStandingPosition, sleepBedPosition,
                Ease(Mathf.Clamp01(seconds / Mathf.Max(.01f, bedMoveSeconds))));
            viewCamera.transform.rotation = Quaternion.Slerp(sleepStandingRotation, ceilingRotation,
                Ease(Mathf.Clamp01((seconds - bedMoveSeconds) / Mathf.Max(.01f, bedLookSeconds))));
            screen.SetFade(progress >= .8f ? 1 : Mathf.Min(.9999f,
                Mathf.Clamp01((seconds - bedMoveSeconds - bedLookSeconds) / Mathf.Max(.01f, bedFadeSeconds))));
        }

        private void CaptureSleepView()
        {
            if (hasSleepView || viewCamera == null) return;
            sleepStandingRotation = viewCamera.transform.rotation;
            sleepActorRotation = seatedPlayer != null ? seatedPlayer.transform.rotation : Quaternion.identity;
            sleepStandingPosition = viewCamera.transform.position;
            sleepStandingLocalPosition = viewCamera.transform.localPosition;
            CaptureSleepBedPose();
            hasSleepView = true;
        }

        private void CaptureSleepBedPose()
        {
            sleepBedPosition = bedtimeGuidance != null ? bedtimeGuidance.SleepFocusPoint : viewCamera.transform.position;
            // With no authored bed, rotate the previous ceiling framing clockwise by a quarter turn.
            sleepBedRotation = Quaternion.Euler(-90, sleepStandingRotation.eulerAngles.y, 0)
                * Quaternion.AngleAxis(90, Vector3.forward);
            if (bedtimeGuidance == null || bedtimeGuidance.BedRenderers == null) return;
            Renderer pillow = null, mattress = null;
            foreach (var part in bedtimeGuidance.BedRenderers)
            {
                if (part == null) continue;
                if (part.name.IndexOf("pillow", StringComparison.OrdinalIgnoreCase) >= 0) pillow = part;
                else if (part.name.IndexOf("mattress", StringComparison.OrdinalIgnoreCase) >= 0) mattress = part;
            }
            if (pillow == null) return;
            sleepBedPosition = new Vector3(pillow.bounds.center.x, pillow.bounds.max.y + .12f, pillow.bounds.center.z);
            if (mattress == null) return;
            var headDirection = Vector3.ProjectOnPlane(pillow.bounds.center - mattress.bounds.center, Vector3.up);
            if (headDirection.sqrMagnitude > .0001f)
                sleepBedRotation = Quaternion.LookRotation(Vector3.up, headDirection.normalized);
        }
        private Vector3 StandingPositionForCurrentActor() => viewCamera.transform.parent != null
            ? viewCamera.transform.parent.TransformPoint(sleepStandingLocalPosition) : sleepStandingLocalPosition;

        private void RestoreSleepView()
        {
            if (hasSleepView && viewCamera != null)
            {
                if (seatedPlayer != null) seatedPlayer.transform.rotation = sleepActorRotation;
                viewCamera.transform.localPosition = sleepStandingLocalPosition;
                viewCamera.transform.rotation = sleepStandingRotation;
            }
            hasSleepView = completingSleep = waking = false;
        }

        private IEnumerator WakeTransition()
        {
            waking = true;
            screen.SetTransitionLocked(true);
            var view = runtime.GetView();
            if (sleepCheckpoint == null || sleepCheckpoint.Phase != SleepTransitionPhase.Waking || !sleepCheckpoint.Matches(view))
                sleepCheckpoint = CreateSleepCheckpoint(view, SleepTransitionPhase.Waking);
            var checkpoint = sleepCheckpoint;
            float duration = Mathf.Max(.01f, bedFadeSeconds) + Mathf.Max(.01f, bedLookSeconds) + Mathf.Max(.01f, bedMoveSeconds);
            checkpoint.WakeElapsed = Mathf.Min(checkpoint.WakeElapsed, duration);
            ApplyWakeFrame(checkpoint.WakeElapsed);
            // The dream completion frame remains fully black before the first wake fade frame.
            yield return null;
            while (checkpoint.WakeElapsed < duration)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                checkpoint.WakeElapsed = Mathf.Min(duration, checkpoint.WakeElapsed + Time.unscaledDeltaTime);
                ApplyWakeFrame(checkpoint.WakeElapsed);
                yield return null;
            }
            while (runtime.GetView().Paused) yield return null;
            sleepCheckpoint = null;
            RestoreSleepView();
            screen.SetFade(0);
            screen.SetTransitionLocked(false);
            playing = null;
        }

        private void ApplyWakeFrame(float elapsed)
        {
            screen.SetFade(1 - Mathf.Clamp01(elapsed / Mathf.Max(.01f, bedFadeSeconds)));
            viewCamera.transform.rotation = Quaternion.Slerp(sleepBedRotation, sleepStandingRotation,
                Ease(Mathf.Clamp01((elapsed - bedFadeSeconds) / Mathf.Max(.01f, bedLookSeconds))));
            viewCamera.transform.position = Vector3.Lerp(sleepBedPosition, StandingPositionForCurrentActor(),
                Ease(Mathf.Clamp01((elapsed - bedFadeSeconds - bedLookSeconds) / Mathf.Max(.01f, bedMoveSeconds))));
        }

        private IEnumerator ReactionSignal(string token)
        {
            float elapsed = 0;
            while (activeToken == token)
            {
                if (!runtime.GetView().Paused)
                {
                    elapsed += Time.unscaledDeltaTime;
                    screen.SetGlitch(elapsed < .45f ? .45f * (1 - elapsed / .45f) : .02f);
                }
                yield return null;
            }
        }

        private IEnumerator TerminalSignalLoss(string token)
        {
            screen.SetTransitionLocked(true);
            float elapsed = 0;
            while (elapsed < .68f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                screen.SetGlitch(.55f);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.SetGlitch(0); screen.SetTransitionLocked(false);
            playing = null;
            runtime.CompleteCue(token);
        }
    }
}
