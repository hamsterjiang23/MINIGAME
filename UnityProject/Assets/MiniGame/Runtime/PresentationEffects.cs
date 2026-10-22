using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum TerminalEntryPhase { None, Sitting, Aligning, Pushing, FadingWorld, RevealingInterface, Active, OpeningMonitor, OpeningPullback }

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
        public void PrepareForRuntimeChange()
        {
            if (runtime != null) runtime.Changed -= Present;
            if (playing != null) StopCoroutine(playing);
            playing = null;
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

        public void Bind(GameRuntime game, MiniGameScreen ui)
        {
            if (runtime != null) runtime.Changed -= Present;
            if (playing != null) StopCoroutine(playing);
            playing = null;
            runtime = game; screen = ui; activeToken = null;
            screen.EndDayZeroOpening();
            screen.SetTransitionLocked(false); screen.SetFade(0); screen.SetGlitch(0);
            screen.SetTerminalPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
            screen.SetTerminalHandoff(0, false, false);
            var view = game.GetView();
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
            screen.SetTerminalReveal(view.TerminalOpen && view.Cue?.Kind != PresentationKind.EnterTerminal ? 1 : 0);
            SetSurfaceGlow(view.TerminalPowered ? 1 : 0);
            if (bedtimeGuidance != null) bedtimeGuidance.Bind(game);
            runtime.Changed += Present;
            Present(view);
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
            roomFieldOfView = viewCamera.fieldOfView;
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
                bedGuide.enabled = view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream;
            var cue = view.Cue;
            string token = cue?.Token;
            if (activeToken == token)
            {
                if (cue == null) SetSurfaceGlow(view.TerminalPowered ? 1 : 0);
                return;
            }
            activeToken = token;
            if (playing != null) { StopCoroutine(playing); playing = null; }
            screen.SetTransitionLocked(cue?.Kind == PresentationKind.Dream);
            screen.EndDayZeroOpening();
            screen.SetFade(cue?.Kind == PresentationKind.Dream ? 1 : 0); screen.SetGlitch(0); screen.SetTerminalHandoff(0, false, false);
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
            float targetFov = exiting ? roomFieldOfView : 48;
            float duration = exiting ? 1.2f : 1.65f;
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

        private void SetSurfaceGlow(float amount)
        {
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

        private IEnumerator SleepTransition(string token)
        {
            screen.SetTransitionLocked(true);
            bool waking = runtime.GetView().Game.Phase == GamePhase.AfterDream;
            float elapsed = 0;
            while (elapsed < .8f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                // Keep opacity 1 reserved for the final yielded frame, including float rounding near the endpoint.
                screen.SetFade(elapsed >= .8f ? 1 : Mathf.Min(.9999f, Ease(Mathf.Clamp01(elapsed / .8f))));
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            if (waking)
            {
                float hold = GetComponent<MiniGameBootstrap>()?.DreamProfile?.WakeBlackSeconds ?? 1;
                if (float.IsNaN(hold) || float.IsInfinity(hold)) hold = 1;
                float held = 0;
                while (held < Mathf.Max(.1f, hold) && activeToken == token)
                {
                    if (!runtime.GetView().Paused) held += Time.unscaledDeltaTime;
                    yield return null;
                }
                while (activeToken == token && runtime.GetView().Paused) yield return null;
                if (activeToken != token) yield break;
            }
            playing = null;
            runtime.CompleteCue(token);
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
